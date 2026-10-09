using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;
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
            // "Sẵn sàng" = đang làm việc + bằng lái còn hạn (theo ngày VN, ngày hết hạn vẫn
            // tính còn hạn) + có ngày làm HÔM NAY. Lưu ý: khi Chia xe còn xét thêm kho đóng
            // quân của tài xế và số ngày chuyến cần (chuyến dài phải đăng ký đủ các ngày).
            var eligible = e.Status == EmployeeStatus.Active && dp.LicenseExpiryDate.Date >= today;
            var availability = busyDriverIds.Contains(e.Id) ? "Đang làm"
                : !eligible ? "Không sẵn sàng"
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

    // =====================================================================
    // NGÀY LÀM VIỆC (bảng DriverSchedules) — đăng ký theo NGÀY, không chia ca.
    // Chia xe / thông báo / danh sách tài xế đều chỉ xét "có lịch ngày đó hay
    // không", nên mỗi ngày chỉ cần 1 dòng (lưu ShiftType = FullDay).
    // =====================================================================

    private const int MaxRegisterDays = 62;

    private static DateTime TodayVn() => VnClock.Today;

    /// <summary>Id chuyến chưa xong của tài xế mà khoảng thời gian chạy (ước tính) bao
    /// trùm ngày <paramref name="date"/>; null nếu không có.
    /// Tính theo TỪNG ĐƠN (giống Chia xe: chuyến Lấy hàng + Giao hàng của 1 đơn cần đủ
    /// ngày liên tiếp): bắt đầu = ngày chặng sớm nhất đã chạy (giờ VN), chưa chạy thì từ
    /// hôm nay; số ngày = tổng thời lượng ước tính các chặng chưa hủy / 8 giờ lái mỗi
    /// ngày, tối thiểu 1 ngày (chưa có ước tính thì 1 ngày cho mỗi chặng còn lại).</summary>
    private async Task<int?> FindTripCoveringDateAsync(int driverEmployeeId, DateTime date)
    {
        const int maxDrivingMinutesPerDay = 480;
        var today = TodayVn();
        var activeOrderIds = await _db.Trips
            .Where(t => t.DriverEmployeeId == driverEmployeeId &&
                        (t.Status == TripStatus.Planned || t.Status == TripStatus.InProgress || t.Status == TripStatus.PendingConfirmation))
            .Select(t => t.TransportOrderId)
            .Distinct()
            .ToListAsync();
        if (activeOrderIds.Count == 0) return null;

        var legs = await _db.Trips
            .Where(t => t.DriverEmployeeId == driverEmployeeId && activeOrderIds.Contains(t.TransportOrderId) && t.Status != TripStatus.Cancelled)
            .Select(t => new { t.Id, t.TransportOrderId, t.Status, t.StartTime, t.EstimatedDurationMinutes })
            .ToListAsync();

        foreach (var order in legs.GroupBy(l => l.TransportOrderId))
        {
            var started = order.Where(l => l.StartTime.HasValue).Select(l => l.StartTime!.Value.AddHours(7).Date).ToList();
            var start = started.Count > 0 ? started.Min() : today;

            var knownMinutes = order.Sum(l => l.EstimatedDurationMinutes ?? 0);
            var unknownOpenLegs = order.Count(l => l.EstimatedDurationMinutes is null && l.Status != TripStatus.Completed);
            var days = Math.Max(1, (int)Math.Ceiling(knownMinutes / (double)maxDrivingMinutesPerDay) + unknownOpenLegs);
            var end = start.AddDays(days - 1);
            if (end < today) end = today; // chạy quá dự kiến: vẫn giữ ít nhất hôm nay

            // Chặng CHƯA chạy (VD chặng Giao đang chờ chặng Lấy hàng của xe khác làm lại)
            // có thể bắt đầu bất kỳ ngày nào từ hôm nay -> giữ thêm đủ số ngày các chặng
            // đó cần TÍNH TỪ HÔM NAY (+1 ngày dự phòng), tránh xóa đúng ngày chuyến sẽ chạy.
            var plannedLegs = order.Where(l => l.Status == TripStatus.Planned).ToList();
            if (plannedLegs.Count > 0)
            {
                var plannedDays = Math.Max(1,
                    (int)Math.Ceiling(plannedLegs.Sum(l => l.EstimatedDurationMinutes ?? 0) / (double)maxDrivingMinutesPerDay)
                    + plannedLegs.Count(l => l.EstimatedDurationMinutes is null));
                var plannedEnd = today.AddDays(plannedDays);
                if (plannedEnd > end) end = plannedEnd;
            }

            if (date >= start && date <= end)
                return order.Where(l => l.Status != TripStatus.Completed).Select(l => l.Id).DefaultIfEmpty(order.First().Id).First();
        }
        return null;
    }

    public async Task<List<DriverWorkDayDto>> GetWorkDaysAsync(int employeeId)
    {
        var rows = await _db.DriverSchedules
            .Where(s => s.DriverEmployeeId == employeeId)
            .OrderBy(s => s.ShiftDate)
            .Select(s => new { s.Id, s.ShiftDate, s.ShiftType })
            .ToListAsync();

        // Dữ liệu cũ có thể còn nhiều ca trong 1 ngày -> chỉ hiện 1 dòng / ngày.
        return rows
            .GroupBy(r => r.ShiftDate.Date)
            .Select(g => new DriverWorkDayDto(
                g.First().Id,
                DateOnly.FromDateTime(g.Key),
                g.Any(r => r.ShiftType != ShiftType.FullDay)))
            .ToList();
    }

    public async Task<DriverWorkDayResult> RegisterWorkDaysAsync(int employeeId, RegisterWorkDaysRequest request)
    {
        var employee = await _db.Employees
            .Where(e => e.Id == employeeId)
            .Select(e => new { e.Status, IsDriver = e.DriverProfile != null })
            .FirstOrDefaultAsync();
        if (employee is null)
            return new DriverWorkDayResult(false, "Không tìm thấy tài xế.");
        if (!employee.IsDriver)
            return new DriverWorkDayResult(false, "Nhân viên này chưa có hồ sơ tài xế — không đăng ký ngày làm được.");
        if (employee.Status != EmployeeStatus.Active)
            return new DriverWorkDayResult(false, "Tài xế không ở trạng thái Đang làm việc (nghỉ phép / đã nghỉ) — không đăng ký ngày làm được.");

        var from = request.FromDate.ToDateTime(TimeOnly.MinValue);
        var to = request.ToDate.ToDateTime(TimeOnly.MinValue);
        var today = TodayVn();

        if (from > to)
            return new DriverWorkDayResult(false, "\"Từ ngày\" phải trước hoặc bằng \"Đến ngày\".");
        if (from < today)
            return new DriverWorkDayResult(false, $"Không đăng ký được ngày đã qua (hôm nay là {today:dd/MM/yyyy}).");
        if ((to - from).TotalDays + 1 > MaxRegisterDays)
            return new DriverWorkDayResult(false, $"Mỗi lần chỉ đăng ký tối đa {MaxRegisterDays} ngày.");

        // Ngày đã có lịch (bất kể ca cũ nào) thì bỏ qua — không tạo trùng.
        var existing = (await _db.DriverSchedules
                .Where(s => s.DriverEmployeeId == employeeId && s.ShiftDate >= from && s.ShiftDate < to.AddDays(1))
                .Select(s => s.ShiftDate)
                .ToListAsync())
            .Select(d => d.Date)
            .ToHashSet();

        int added = 0, skipped = 0;
        for (var day = from; day <= to; day = day.AddDays(1))
        {
            if (existing.Contains(day))
            {
                skipped++;
                continue;
            }
            _db.DriverSchedules.Add(new DriverSchedule
            {
                DriverEmployeeId = employeeId,
                ShiftDate = day,
                ShiftType = ShiftType.FullDay,
            });
            added++;
        }

        if (added == 0)
            return new DriverWorkDayResult(false, "Tất cả các ngày đã chọn đều đã được đăng ký trước đó.", 0, skipped);

        await _db.SaveChangesAsync();
        var msg = $"Đã đăng ký {added} ngày làm việc." + (skipped > 0 ? $" Bỏ qua {skipped} ngày đã có lịch." : "");
        return new DriverWorkDayResult(true, msg, added, skipped);
    }

    public async Task<DriverWorkDayResult> DeleteWorkDayAsync(int workDayId)
    {
        var target = await _db.DriverSchedules.FindAsync(workDayId);
        if (target is null)
            return new DriverWorkDayResult(false, "Không tìm thấy ngày làm việc này.");

        var date = target.ShiftDate.Date;
        if (date < TodayVn())
            return new DriverWorkDayResult(false, "Không xóa được ngày đã qua — đây là lịch sử làm việc.");

        // Chỉ chặn những ngày NẰM TRONG thời gian các chuyến chưa xong của tài xế (xóa
        // lúc này sẽ làm tài xế "nghỉ" giữa chuyến) — ngày không liên quan vẫn xóa được.
        var conflict = await FindTripCoveringDateAsync(target.DriverEmployeeId, date);
        if (conflict is not null)
            return new DriverWorkDayResult(false,
                $"Ngày {date:dd/MM/yyyy} nằm trong thời gian chuyến #{conflict} chưa hoàn thành của tài xế — không xóa được. Hoàn thành hoặc hủy chuyến trước.");

        // Xóa mọi dòng của ngày đó (dữ liệu cũ có thể có nhiều ca trong 1 ngày).
        var sameDay = await _db.DriverSchedules
            .Where(s => s.DriverEmployeeId == target.DriverEmployeeId && s.ShiftDate >= date && s.ShiftDate < date.AddDays(1))
            .ToListAsync();
        _db.DriverSchedules.RemoveRange(sameDay);
        await _db.SaveChangesAsync();
        return new DriverWorkDayResult(true, $"Đã xóa ngày làm {date:dd/MM/yyyy}.", 0, 0);
    }
}
