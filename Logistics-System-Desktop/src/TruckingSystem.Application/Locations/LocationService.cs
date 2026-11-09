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
        // (0, 0) là điểm giữa biển (vịnh Guinea) — gần như luôn do ô nhập để mặc định / chưa
        // chọn địa chỉ từ bản đồ; lưu vào sẽ làm lộ trình, quãng đường, số ngày chạy sai hẳn.
        if (lat == 0 && lng == 0)
            throw new InvalidOperationException("Tọa độ (0, 0) không hợp lệ — hãy chọn địa chỉ từ ô tìm kiếm bản đồ để lấy đúng tọa độ.");
        // Tên địa điểm không trùng (không phân biệt hoa/thường) — tên hiện trong ô chọn Điểm lấy /
        // Điểm giao của đơn; 2 địa điểm cùng tên thì dễ chọn nhầm chỗ, chuyến chạy sai địa chỉ.
        var lowerName = name.ToLower();
        if (await _db.Locations.AnyAsync(l => l.Name.Trim().ToLower() == lowerName))
            throw new InvalidOperationException($"Đã có địa điểm tên \"{name}\" — đặt tên khác (VD thêm tên quận / tên khách).");
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
