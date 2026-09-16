using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.Vehicles;

public class VehicleService : IVehicleService
{
    private readonly IApplicationDbContext _db;

    public VehicleService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<VehicleListItemDto>> GetAllAsync()
    {
        return await _db.Vehicles
            .OrderBy(v => v.PlateNumber)
            .Select(v => new VehicleListItemDto(v.Id, v.PlateNumber, v.VehicleType, v.CapacityQty, v.Status.ToString()))
            .ToListAsync();
    }

    public async Task<VehicleListItemDto> CreateAsync(CreateVehicleRequest request)
    {
        var vehicle = new Vehicle
        {
            PlateNumber = request.PlateNumber,
            VehicleType = request.VehicleType,
            CapacityQty = request.CapacityQty,
            CapacityKg = request.CapacityKg,
            CapacityM3 = request.CapacityM3,
            Status = VehicleStatus.Available,
        };

        _db.Vehicles.Add(vehicle);
        await _db.SaveChangesAsync();

        return new VehicleListItemDto(vehicle.Id, vehicle.PlateNumber, vehicle.VehicleType, vehicle.CapacityQty, vehicle.Status.ToString());
    }
}
