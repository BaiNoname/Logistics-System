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
        // Kiểm tra ở server (trước đây chỉ Desktop kiểm tra): thiếu tên/địa chỉ/tọa độ
        // thì địa điểm vô dụng cho Chia xe, ước tính quãng đường, bản đồ.
        var name = (request.Name ?? "").Trim();
        var address = (request.Address ?? "").Trim();
        if (name.Length == 0) throw new InvalidOperationException("Tên địa điểm không được để trống.");
        if (address.Length == 0) throw new InvalidOperationException("Địa chỉ không được để trống.");
        if (request.Lat is not { } lat || request.Lng is not { } lng)
            throw new InvalidOperationException("Địa điểm phải có tọa độ — chọn địa chỉ từ ô tìm kiếm bản đồ.");
        if (lat is < -90 or > 90 || lng is < -180 or > 180)
            throw new InvalidOperationException("Tọa độ không hợp lệ.");
        if (request.CustomerId.HasValue && !await _db.Customers.AnyAsync(c => c.Id == request.CustomerId.Value))
            throw new InvalidOperationException("Khách hàng của địa điểm không tồn tại.");

        var location = new Location
        {
            Name = name,
            Address = address,
            Lat = request.Lat,
            Lng = request.Lng,
            CustomerId = request.CustomerId,
        };
        _db.Locations.Add(location);
        await _db.SaveChangesAsync();
        return new LocationDto(location.Id, location.Name, location.Address, location.Lat, location.Lng, location.CustomerId);
    }
}
