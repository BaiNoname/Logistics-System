using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LogisticsSystem.Employee.Config;

namespace LogisticsSystem.Employee.Services;

public sealed class ApiException : Exception
{
    public HttpStatusCode? StatusCode { get; }

    public ApiException(string message, HttpStatusCode? statusCode = null) : base(message)
    {
        StatusCode = statusCode;
    }
}

/// <summary>Đường dẫn khớp với AuthController và AttendanceController của TruckingSystem.API.</summary>
public static class ApiRoutes
{
    public const string Login = "api/auth/login";
    public const string Me = "api/attendance/me";
    public const string Scan = "api/attendance/scan";
    public const string MyHistory = "api/attendance/me/history";
    public const string MyAdjustments = "api/attendance/me/adjustments";
    public const string AdjustmentReasons = "api/attendance/adjustment-reasons";
}

/// <summary>Gọi TruckingSystem.API bằng JWT. Mobile không bao giờ kết nối trực tiếp PostgreSQL.</summary>
public sealed class ApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly SessionService _session;
    private HttpClient? _http;
    private string _baseUrl = string.Empty;

    public ApiClient(SessionService session)
    {
        _session = session;
    }

    private HttpClient Http
    {
        get
        {
            var current = AppSettings.ApiBaseUrl;
            if (_http is null || !string.Equals(_baseUrl, current, StringComparison.Ordinal))
            {
                _http?.Dispose();
                _http = new HttpClient { BaseAddress = new Uri(current), Timeout = TimeSpan.FromSeconds(20) };
                _baseUrl = current;
            }
            return _http;
        }
    }

    public Task<T?> GetAsync<T>(string path, CancellationToken ct = default) =>
        SendAsync<T>(HttpMethod.Get, path, null, ct);

    public Task<T?> PostAsync<T>(string path, object? body, CancellationToken ct = default) =>
        SendAsync<T>(HttpMethod.Post, path, body, ct);

    private async Task<T?> SendAsync<T>(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, path);

        if (path != ApiRoutes.Login)
        {
            var token = await _session.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        if (body is not null)
            request.Content = JsonContent.Create(body, options: JsonOptions);

        HttpResponseMessage response;
        try
        {
            response = await Http.SendAsync(request, ct);
        }
        catch (HttpRequestException)
        {
            throw new ApiException("Không kết nối được máy chủ. Kiểm tra mạng hoặc địa chỉ máy chủ.");
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new ApiException("Máy chủ phản hồi quá chậm. Vui lòng thử lại.");
        }

        using (response)
        {
            // Đăng nhập sai trả 401 kèm thông báo; các request khác 401 nghĩa là phiên đã hết hạn.
            if (response.StatusCode == HttpStatusCode.Unauthorized && path != ApiRoutes.Login)
            {
                _session.Clear();
                MainThread.BeginInvokeOnMainThread(AppRoot.ShowLogin);
                throw new ApiException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", response.StatusCode);
            }

            var text = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(
                    ExtractMessage(text) ?? $"Máy chủ trả về lỗi ({(int)response.StatusCode}).",
                    response.StatusCode);
            }

            if (string.IsNullOrWhiteSpace(text))
                return default;

            return JsonSerializer.Deserialize<T>(text, JsonOptions);
        }
    }

    private static string? ExtractMessage(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        try
        {
            using var doc = JsonDocument.Parse(text);
            if (doc.RootElement.ValueKind != JsonValueKind.Object) return null;

            foreach (var name in new[] { "message", "title", "detail" })
            {
                if (doc.RootElement.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.String)
                {
                    var value = prop.GetString();
                    if (!string.IsNullOrWhiteSpace(value)) return value;
                }
            }
        }
        catch (JsonException)
        {
            // Phản hồi không phải JSON: dùng thông báo mặc định.
        }
        return null;
    }
}
