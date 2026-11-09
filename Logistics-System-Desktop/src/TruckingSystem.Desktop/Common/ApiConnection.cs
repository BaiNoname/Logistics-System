using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace TruckingSystem.Desktop.Common
{
    /// <summary>
    /// Lớp kết nối HTTP nền tảng — thay thế vai trò "DAL" bên Desktop (Desktop
    /// không đụng database trực tiếp, chỉ gọi API). Toàn bộ SERVICE_* đều dùng
    /// chung 1 instance của lớp này để giữ Token đăng nhập tập trung, tránh mỗi
    /// service tự tạo HttpClient riêng (rò rỉ socket).
    /// </summary>
    public class ApiConnection
    {
        public HttpClient Http { get; }
        public string BaseUrl { get; }
        public string? Token { get; private set; }
        public IReadOnlySet<string> Roles => _roles;

        private readonly HashSet<string> _roles = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Máy chủ trả 401 (phiên đăng nhập hết hạn / token không còn hợp lệ) — màn chính lắng nghe để báo 1 lần
        /// rồi đưa về màn đăng nhập (trước đây màn nào cũng chỉ báo lỗi lạ, người dùng không biết phải làm gì).</summary>
        public event EventHandler? SessionExpired;

        public ApiConnection(string baseUrl)
        {
            BaseUrl = baseUrl.TrimEnd('/');
            Http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        }

        /// <summary>Gửi yêu cầu, đổi lỗi MẠNG thành câu dễ hiểu (trước đây hiện nguyên văn tiếng Anh như
        /// "No such host is known", "A task was canceled").</summary>
        private static async Task<HttpResponseMessage> SendAsync(Func<Task<HttpResponseMessage>> send)
        {
            try { return await send(); }
            catch (TaskCanceledException ex)
            {
                throw new HttpRequestException("Máy chủ phản hồi quá lâu — kiểm tra mạng rồi thử lại sau ít phút.", ex);
            }
            catch (HttpRequestException ex) when (ex.StatusCode is null)
            {
                throw new HttpRequestException("Không kết nối được máy chủ — kiểm tra mạng (wifi / dây mạng) rồi bấm Làm mới.", ex);
            }
        }

        public void SetToken(string token, IEnumerable<string>? roles = null)
        {
            Token = token;
            Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _roles.Clear();
            if (roles is not null)
                foreach (var role in roles) _roles.Add(role);
        }

        public bool IsInRole(params string[] roles) => roles.Any(_roles.Contains);

        public void ClearToken()
        {
            Token = null;
            Http.DefaultRequestHeaders.Authorization = null;
            _roles.Clear();
        }

        public async Task<T?> GetAsync<T>(string path)
            => await ReadResultAsync<T>(await SendAsync(() => Http.GetAsync(path)));

        public async Task<T?> PostAsync<T>(string path, object payload)
        {
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            return await ReadResultAsync<T>(await SendAsync(() => Http.PostAsync(path, content)));
        }

        public async Task<T?> PutAsync<T>(string path, object payload)
        {
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            return await ReadResultAsync<T>(await SendAsync(() => Http.PutAsync(path, content)));
        }

        public async Task<T?> DeleteAsync<T>(string path)
            => await ReadResultAsync<T>(await SendAsync(() => Http.DeleteAsync(path)));

        /// <summary>Đọc phản hồi API — xử lý lỗi TẬP TRUNG cho mọi màn hình:
        /// - 400 mà body là object kết quả {success:false, message:"..."} -> trả về object đó
        ///   luôn (màn hình tự hiện result.Message sạch sẽ), không ném lỗi.
        /// - Các lỗi khác -> ném HttpRequestException với thông báo TIẾNG VIỆT dễ hiểu,
        ///   thay vì "Lỗi API (400): {json thô}" như trước.</summary>
        private async Task<T?> ReadResultAsync<T>(HttpResponseMessage resp)
        {
            var body = await resp.Content.ReadAsStringAsync();
            if ((int)resp.StatusCode == 401 && Token is not null) SessionExpired?.Invoke(this, EventArgs.Empty);
            if (resp.IsSuccessStatusCode)
                return string.IsNullOrWhiteSpace(body) ? default : JsonConvert.DeserializeObject<T>(body);

            if ((int)resp.StatusCode == 400 && LooksLikeActionResult(body))
            {
                try { return JsonConvert.DeserializeObject<T>(body); } catch { /* không khớp kiểu -> báo lỗi bên dưới */ }
            }
            throw new HttpRequestException(FriendlyError((int)resp.StatusCode, body), null, resp.StatusCode);
        }

        private static bool LooksLikeActionResult(string body)
        {
            try
            {
                return Newtonsoft.Json.Linq.JToken.Parse(body) is Newtonsoft.Json.Linq.JObject obj
                       && (obj["success"] != null || obj["Success"] != null);
            }
            catch { return false; }
        }

        /// <summary>Đổi lỗi HTTP thành câu tiếng Việt cho người dùng.</summary>
        public static string FriendlyError(int status, string? body)
        {
            switch (status)
            {
                case 401: return "Phiên đăng nhập đã hết hạn — vui lòng đăng nhập lại.";
                case 403: return "Tài khoản của bạn không có quyền thực hiện chức năng này.";
                case 404 when string.IsNullOrWhiteSpace(body): return "Không tìm thấy dữ liệu (404).";
            }

            var message = ExtractMessage(body);
            if (status >= 500)
                return "Máy chủ gặp lỗi khi xử lý" + (string.IsNullOrWhiteSpace(message) ? "" : ": " + Shorten(message)) + " — thử lại sau.";
            return string.IsNullOrWhiteSpace(message) ? $"Yêu cầu không hợp lệ ({status})." : message!;
        }

        /// <summary>Lấy câu thông báo trong body lỗi: chuỗi thuần, {message}, hoặc
        /// ProblemDetails {title, errors:{field:[...]}} của ASP.NET.</summary>
        private static string? ExtractMessage(string? body)
        {
            if (string.IsNullOrWhiteSpace(body)) return null;
            try
            {
                var token = Newtonsoft.Json.Linq.JToken.Parse(body);
                if (token.Type == Newtonsoft.Json.Linq.JTokenType.String) return token.ToString();
                if (token is Newtonsoft.Json.Linq.JObject obj)
                {
                    var msg = (string?)(obj["message"] ?? obj["Message"]);
                    if (!string.IsNullOrWhiteSpace(msg)) return msg;
                    if (obj["errors"] is Newtonsoft.Json.Linq.JObject errors)
                    {
                        var all = errors.Properties().SelectMany(p => p.Value.Values<string>()).Where(s => !string.IsNullOrWhiteSpace(s));
                        var joined = string.Join("; ", all);
                        if (joined.Length > 0) return joined;
                    }
                    var title = (string?)(obj["title"] ?? obj["Title"]);
                    if (!string.IsNullOrWhiteSpace(title)) return title;
                }
            }
            catch
            {
                // không phải JSON -> chuỗi thuần
            }
            return Shorten(body.Trim().Trim('"'));
        }

        private static string Shorten(string s) => s.Length > 300 ? s[..300] + "…" : s;

        /// <summary>Post không cần parse response thành công cụ thể (dùng riêng cho Login vì cần đọc cả trường hợp lỗi 401).</summary>
        public async Task<(bool IsSuccess, string Body)> PostRawAsync(string path, object payload)
        {
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var resp = await SendAsync(() => Http.PostAsync(path, content));
            var body = await resp.Content.ReadAsStringAsync();
            return (resp.IsSuccessStatusCode, body);
        }
    }
}
