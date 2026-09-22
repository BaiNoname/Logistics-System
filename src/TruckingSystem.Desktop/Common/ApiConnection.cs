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

        public ApiConnection(string baseUrl)
        {
            BaseUrl = baseUrl.TrimEnd('/');
            Http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        }

        public void SetToken(string token)
        {
            Token = token;
            Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public void ClearToken()
        {
            Token = null;
            Http.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<T?> GetAsync<T>(string path)
        {
            var json = await Http.GetStringAsync(path);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public async Task<T?> PostAsync<T>(string path, object payload)
        {
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var resp = await Http.PostAsync(path, content);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"Lỗi API ({(int)resp.StatusCode}): {body}");
            return JsonConvert.DeserializeObject<T>(body);
        }

        public async Task<T?> PutAsync<T>(string path, object payload)
        {
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var resp = await Http.PutAsync(path, content);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"Lỗi API ({(int)resp.StatusCode}): {body}");
            return JsonConvert.DeserializeObject<T>(body);
        }

        public async Task<T?> DeleteAsync<T>(string path)
        {
            var resp = await Http.DeleteAsync(path);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"Lỗi API ({(int)resp.StatusCode}): {body}");
            return JsonConvert.DeserializeObject<T>(body);
        }

        /// <summary>Post không cần parse response thành công cụ thể (dùng riêng cho Login vì cần đọc cả trường hợp lỗi 401).</summary>
        public async Task<(bool IsSuccess, string Body)> PostRawAsync(string path, object payload)
        {
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            var resp = await Http.PostAsync(path, content);
            var body = await resp.Content.ReadAsStringAsync();
            return (resp.IsSuccessStatusCode, body);
        }
    }
}
