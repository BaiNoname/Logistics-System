namespace TruckingSystem.Application.Locations;

public interface ILocationService
{
    Task<List<LocationDto>> GetAllAsync();
    Task<LocationDto> CreateAsync(CreateLocationRequest request);
}
