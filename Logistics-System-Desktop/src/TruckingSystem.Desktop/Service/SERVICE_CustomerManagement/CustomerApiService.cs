using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_CustomerManagement;
using System.Net;
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

        public Task<CustomerActionResultItem> CreateCustomerAsync(string name, string? phone, string? taxCode, string customerType, string? address, decimal? creditLimit, string? googleCalendarId, List<CustomerContactInput>? contacts = null)
            => _conn.PostAsync<CustomerActionResultItem>("/api/customers", new { name, phone, taxCode, customerType, address, creditLimit, googleCalendarId, contacts })!;

        /// <summary>Sửa thông tin khách hàng đã có.</summary>
        public Task<CustomerActionResultItem> UpdateCustomerAsync(int id, string name, string? phone, string? taxCode, string customerType, string? address, decimal? creditLimit, string? googleCalendarId)
            => _conn.PutAsync<CustomerActionResultItem>($"/api/customers/{id}", new { name, phone, taxCode, customerType, address, creditLimit, googleCalendarId })!;

        public Task<List<CustomerContactListItem>> GetContactsAsync()
            => _conn.GetAsync<List<CustomerContactListItem>>("/api/customers/contacts")!;

        public Task<CustomerActionResultItem> CreateContactAsync(int customerId, string name, string? phone, string? position)
            => _conn.PostAsync<CustomerActionResultItem>("/api/customers/contacts", new { customerId, name, phone, position })!;

        public Task<CustomerActionResultItem> UpdateContactAsync(int id, int customerId, string name, string? phone, string? position)
            => _conn.PutAsync<CustomerActionResultItem>($"/api/customers/contacts/{id}", new { customerId, name, phone, position })!;

        public Task<CustomerActionResultItem> DeleteContactAsync(int id)
            => _conn.DeleteAsync<CustomerActionResultItem>($"/api/customers/contacts/{id}")!;

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
                    422 => "Ảnh không phải danh thiếp. Vui lòng chọn ảnh danh thiếp.",
                    502 => $"Máy chủ {_conn.BaseUrl} trả về 502 (Bad Gateway) nhưng không cung cấp thông báo lỗi đọc được. Cần kiểm tra dịch vụ API hoặc cổng kết nối của máy chủ.",
                    504 => "Dịch vụ AI phản hồi quá lâu. Vui lòng thử lại sau.",
                    _ => "Không thể quét danh thiếp. Vui lòng thử lại sau."
                };
                try
                {
                    var error = JToken.Parse(body);
                    if ((int)response.StatusCode == 502 && error.Type == JTokenType.Object
                        && error["rawResponse"]?.Type == JTokenType.String
                        && error["errorMessage"]?.Type == JTokenType.String)
                        return error.ToObject<CustomerBusinessCardInfoItem>()
                            ?? throw new InvalidOperationException("Không đọc được kết quả từ AI.");
                    var detail = error.Type == JTokenType.String ? error.Value<string>() :
                        error.Type == JTokenType.Object ? (string?)(error["message"] ?? error["detail"] ?? error["title"]) : null;
                    if (!string.IsNullOrWhiteSpace(detail)) message = detail;
                }
                catch (JsonException) { }
                if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
                    throw new HttpRequestException(message, null, response.StatusCode);
                throw new HttpRequestException($"Lỗi API ({(int)response.StatusCode}): {message}", null, response.StatusCode);
            }
            return JsonConvert.DeserializeObject<CustomerBusinessCardInfoItem>(body)
                ?? throw new InvalidOperationException("Không đọc được kết quả từ AI.");
        }
    }
}
