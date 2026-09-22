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

        public Task CreateCustomerAsync(string name, string? phone, string? taxCode, string customerType)
            => _conn.PostAsync<object>("/api/customers", new { name, phone, taxCode, customerType });
    }
}
