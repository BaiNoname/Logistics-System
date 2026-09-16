using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Application.Locations;

public class LocationService : ILocationService
{
    private readonly IApplicationDbContext _db;

    public LocationService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<LocationDto>> GetAllAsync()
    {
        return await _db.Locations
            .OrderBy(l => l.Name)
            .Select(l => new LocationDto(l.Id, l.Name, l.Address, l.Lat, l.Lng, l.CustomerId))
            .ToListAsync();
    }

    public async Task<LocationDto> CreateAsync(CreateLocationRequest request)
    {
        var location = new Location
        {
            Name = request.Name,
            Address = request.Address,
            Lat = request.Lat,
            Lng = request.Lng,
            CustomerId = request.CustomerId,
        };
        _db.Locations.Add(location);
        await _db.SaveChangesAsync();
        return new LocationDto(location.Id, location.Name, location.Address, location.Lat, location.Lng, location.CustomerId);
    }
}
