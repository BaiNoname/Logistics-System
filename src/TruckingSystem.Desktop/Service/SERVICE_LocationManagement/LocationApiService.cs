using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_LocationManagement;

namespace TruckingSystem.Desktop.SERVICE_LocationManagement;

public class LocationApiService
{
    private readonly ApiConnection _conn;

    public LocationApiService(ApiConnection conn)
    {
        _conn = conn;
    }

    public Task<List<LocationItem>> GetLocationsAsync()
        => _conn.GetAsync<List<LocationItem>>("/api/locations")!;

    public Task CreateLocationAsync(string name, string address, double? lat, double? lng)
        => _conn.PostAsync<object>("/api/locations", new { name, address, lat, lng, customerId = (int?)null });
}
