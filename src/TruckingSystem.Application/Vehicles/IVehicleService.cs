namespace TruckingSystem.Application.Vehicles;

public interface IVehicleService
{
    Task<List<VehicleListItemDto>> GetAllAsync();
    Task<VehicleListItemDto> CreateAsync(CreateVehicleRequest request);
}
