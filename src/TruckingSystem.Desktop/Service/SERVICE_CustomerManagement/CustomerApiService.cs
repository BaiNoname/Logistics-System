using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_CustomerManagement;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TruckingSystem.Desktop.SERVICE_CustomerManagement
{
    public class CustomerApiService
    {
        private readonly ApiConnection _conn;

        public CustomerApiService(ApiConnection conn)
        {
            _conn = conn;
        }

        public Task<List<CustomerListItem>> GetCustomersAsync()
            => _conn.GetAsync<List<CustomerListItem>>("/api/customers")!;

        public Task<CustomerActionResultItem> CreateCustomerAsync(string name, string? phone, string? taxCode, string customerType, string? address, decimal? creditLimit, string? googleCalendarId)
            => _conn.PostAsync<CustomerActionResultItem>("/api/customers", new { name, phone, taxCode, customerType, address, creditLimit, googleCalendarId })!;

        /// <summary>Sửa thông tin khách hàng đã có.</summary>
        public Task<CustomerActionResultItem> UpdateCustomerAsync(int id, string name, string? phone, string? taxCode, string customerType, string? address, decimal? creditLimit, string? googleCalendarId)
            => _conn.PutAsync<CustomerActionResultItem>($"/api/customers/{id}", new { name, phone, taxCode, customerType, address, creditLimit, googleCalendarId })!;

        public async Task<CustomerBusinessCardInfoItem> ReadBusinessCardAsync(string filePath, CancellationToken cancellationToken = default)
        {
            var contentType = Path.GetExtension(filePath).ToLowerInvariant() switch
            {
                ".png" => "image/png", ".jpg" or ".jpeg" => "image/jpeg",
                _ => throw new ArgumentException("Chỉ hỗ trợ ảnh JPG hoặc PNG.")
            };
            using var form = new MultipartFormDataContent();
            using var file = File.OpenRead(filePath);
            if (file.Length == 0 || file.Length > 10 * 1024 * 1024)
                throw new ArgumentException("Vui lòng chọn ảnh danh thiếp không rỗng, tối đa 10 MB.");
            using var image = new StreamContent(file);
            image.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            form.Add(image, "image", Path.GetFileName(filePath));
            using var response = await _conn.Http.PostAsync("/api/customers/read-business-card", form, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var message = (int)response.StatusCode switch
                {
                    401 => "Phiên đăng nhập hết hiệu lực. Vui lòng đăng nhập lại.",
                    403 => "Tài khoản không có quyền quét danh thiếp.",
                    404 => "Không tìm thấy API quét danh thiếp. Vui lòng khởi động lại API đã cập nhật.",
                    413 => "Ảnh danh thiếp quá lớn. Vui lòng chọn ảnh tối đa 10 MB.",
                    504 => "Dịch vụ AI phản hồi quá lâu. Vui lòng thử lại sau.",
                    _ => "Không thể quét danh thiếp. Vui lòng thử lại sau."
                };
                try
                {
                    var error = JToken.Parse(body);
                    var detail = error.Type == JTokenType.String ? error.Value<string>() :
                        error.Type == JTokenType.Object ? (string?)(error["message"] ?? error["detail"] ?? error["title"]) : null;
                    if (!string.IsNullOrWhiteSpace(detail)) message = detail;
                }
                catch (JsonException) { }
                throw new HttpRequestException($"Lỗi API ({(int)response.StatusCode}): {message}", null, response.StatusCode);
            }
            return JsonConvert.DeserializeObject<CustomerBusinessCardInfoItem>(body)
                ?? throw new InvalidOperationException("Không đọc được kết quả từ AI.");
        }
    }
}
