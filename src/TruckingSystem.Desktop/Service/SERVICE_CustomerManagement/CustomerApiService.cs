using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_CustomerManagement;

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

        public Task CreateCustomerAsync(string name, string? phone, string? taxCode, string customerType, string? address, decimal? creditLimit, string? googleCalendarId)
            => _conn.PostAsync<object>("/api/customers", new { name, phone, taxCode, customerType, address, creditLimit, googleCalendarId });

        /// <summary>Sửa thông tin khách hàng đã có.</summary>
        public Task<CustomerActionResultItem> UpdateCustomerAsync(int id, string name, string? phone, string? taxCode, string customerType, string? address, decimal? creditLimit, string? googleCalendarId)
            => _conn.PutAsync<CustomerActionResultItem>($"/api/customers/{id}", new { name, phone, taxCode, customerType, address, creditLimit, googleCalendarId })!;
    }
}
