using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_VehicleManagement;

namespace TruckingSystem.Desktop.SERVICE_VehicleManagement;

public class VehicleApiService
{
    private readonly ApiConnection _conn;

    public VehicleApiService(ApiConnection conn)
    {
        _conn = conn;
    }

    public Task<List<VehicleListItem>> GetVehiclesAsync()
        => _conn.GetAsync<List<VehicleListItem>>("/api/vehicles")!;

    public Task CreateVehicleAsync(string plateNumber, string vehicleType, decimal capacityQty, decimal capacityKg, decimal capacityM3)
        => _conn.PostAsync<object>("/api/vehicles", new { plateNumber, vehicleType, capacityQty, capacityKg, capacityM3 });
}
