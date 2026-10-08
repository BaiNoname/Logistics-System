using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.Drivers;

public class DriverService : IDriverService
{
    private readonly IApplicationDbContext _db;

    public DriverService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<DriverListItemDto>> GetAllAsync()
    {
        // Tài xế "Đang làm" — ĐÚNG Y HỆT điều kiện VehicleAllocationService
        // dùng để loại trừ tài xế bận khi chạy thuật toán Greedy.
        var busyDriverIds = await _db.Trips
            .Where(t => t.Status == TripStatus.Planned || t.Status == TripStatus.InProgress || t.Status == TripStatus.PendingConfirmation)
            .Select(t => t.DriverEmployeeId)
            .Distinct()
            .ToListAsync();

        // Tài xế đã đăng ký ÍT NHẤT 1 ca cho HÔM NAY (không phân biệt loại ca).
        // "Hôm nay" theo GIỜ VIỆT NAM (UTC+7) — UtcNow.Date bị lệch 1 ngày
        // trong khoảng 0h–7h sáng, cùng quy ước với VehicleAllocationService.
        var today = DateTime.UtcNow.AddHours(7).Date;
        var scheduledTodayIds = await _db.DriverSchedules
            .Where(s => s.ShiftDate.Date == today)
            .Select(s => s.DriverEmployeeId)
            .Distinct()
            .ToListAsync();

        // Lấy tên Kho riêng để ghép tay (tránh phụ thuộc navigation chưa chắc
        // có sẵn trên DriverProfile) — nhẹ, chỉ 1 query phụ.
        var warehouseNames = await _db.Warehouses
            .Select(w => new { w.Id, w.Name })
            .ToDictionaryAsync(w => w.Id, w => w.Name);

        // Bắt đầu từ Employee (không phải DriverProfile) vì CHẮC CHẮN có
        // navigation Employee.DriverProfile (đã dùng ở nhiều chỗ khác trong
        // hệ thống, ví dụ TripService khi đồng bộ Calendar).
        var employees = await _db.Employees
            .Where(e => e.DriverProfile != null)
            .Include(e => e.DriverProfile)
            .OrderBy(e => e.FullName)
            .ToListAsync();

        return employees.Select(e =>
        {
            var dp = e.DriverProfile!;
            var availability = busyDriverIds.Contains(e.Id) ? "Đang làm"
                : scheduledTodayIds.Contains(e.Id) ? "Sẵn sàng"
                : "Không sẵn sàng";

            return new DriverListItemDto(
                e.Id, e.Code, e.FullName, e.Phone,
                dp.LicenseNumber, dp.LicenseClass, dp.LicenseExpiryDate,
                dp.YearsOfExperience, dp.WarehouseId,
                dp.WarehouseId.HasValue && warehouseNames.TryGetValue(dp.WarehouseId.Value, out var name) ? name : null,
                availability);
        }).ToList();
    }
}