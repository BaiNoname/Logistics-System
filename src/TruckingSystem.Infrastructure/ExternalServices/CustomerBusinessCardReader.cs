using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TruckingSystem.Application.Customers;

namespace TruckingSystem.Infrastructure.ExternalServices;

public sealed class CustomerBusinessCardReader : ICustomerBusinessCardReader
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _model;

    public CustomerBusinessCardReader(HttpClient http, IConfiguration configuration)
    {
        _http = http;
        _apiKey = configuration["Nvidia:ApiKey"] ?? throw new InvalidOperationException("Thiếu cấu hình Nvidia:ApiKey.");
        _model = configuration["Nvidia:Model"] ?? throw new InvalidOperationException("Thiếu cấu hình Nvidia:Model.");
        var baseUrl = configuration["Nvidia:BaseUrl"] ?? "https://integrate.api.nvidia.com/v1/";
        _http.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        _http.Timeout = TimeSpan.FromSeconds(90);
    }

    public async Task<CustomerBusinessCardInfo> ReadAsync(byte[] image, string contentType, CancellationToken cancellationToken = default)
    {
        var dataUrl = $"data:{contentType};base64,{Convert.ToBase64String(image)}";
        var payload = new
        {
            model = _model,
            messages = new[] { new { role = "user", content = new object[] {
                new { type = "text", text = "Read the business card. Return only a JSON object with Name, Phone, TaxCode, CustomerType (Company or Individual), and Address. Use null for fields that are not visible. Do not guess." },
                new { type = "image_url", image_url = new { url = dataUrl } }
            } } },
            max_tokens = 1024,
            temperature = 0.2,
            top_k = 1,
            chat_template_kwargs = new { enable_thinking = false }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        using var response = await _http.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"NVIDIA API trả về {(int)response.StatusCode}: {body}");

        using var document = JsonDocument.Parse(body);
        var content = document.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        if (string.IsNullOrWhiteSpace(content)) throw new InvalidOperationException("AI không đọc được thông tin trên danh thiếp.");

        var jsonStart = content.IndexOf('{');
        var jsonEnd = content.LastIndexOf('}');
        if (jsonStart < 0 || jsonEnd < jsonStart) throw new InvalidOperationException("AI trả về dữ liệu không đúng định dạng.");
        return JsonSerializer.Deserialize<CustomerBusinessCardInfo>(content[jsonStart..(jsonEnd + 1)], new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("Không đọc được thông tin trên danh thiếp.");
    }
}
