using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_DriverManagement;

namespace TruckingSystem.Desktop.SERVICE_DriverManagement
{
    public class DriverApiService
    {
        private readonly ApiConnection _conn;

        public DriverApiService(ApiConnection conn)
        {
            _conn = conn;
        }

        public Task<List<DriverListItem>> GetDriversAsync()
            => _conn.GetAsync<List<DriverListItem>>("/api/drivers")!;
    }
}