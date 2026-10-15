using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Application.Drivers;
using TruckingSystem.Application.Vehicles;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.Resources;

/// <summary>Tổng hợp NGUỒN LỰC theo kho cho màn "🧭 Nguồn lực theo kho" — chỉ đọc, dùng lại
/// đúng các quy tắc đã có để kết quả KHỚP với Chia xe / Bắt đầu chuyến:
///   Xe chạy được hôm nay  = trạng thái Rảnh + đủ 3 giấy tờ còn hạn (VehicleDocumentRules).
///   Tài xế chạy được hôm nay = đang làm + chức danh Tài xế + có hồ sơ + bằng lái / khám sức khỏe
///   còn hạn + đã đăng ký ngày làm hôm nay + không đang bận chuyến khác.
/// "Sắp hết hạn" = trong 30 ngày tới.</summary>
public class ResourceOverviewService : IResourceOverviewService
{
    private const int ExpiringDays = 30;
    private readonly IApplicationDbContext _db;

    public ResourceOverviewService(IApplicationDbContext db)
    {
        _db = db;
    }

    private sealed record ActiveTrip(int Id, int VehicleId, int DriverEmployeeId, TripStatus Status, TripLegType LegType,
        int? WarehouseId, DateTime? PlannedStartDate, string OrderCode, string Plate);

    private static readonly TripStatus[] Active = { TripStatus.Planned, TripStatus.InProgress, TripStatus.PendingConfirmation };

    private static string LegText(TripLegType leg) => leg switch
    {
        TripLegType.Pickup => "Lấy hàng",
        TripLegType.Delivery => "Giao hàng",
        TripLegType.Return => "Trả hàng",
        _ => leg.ToString(),
    };

    private static string StatusText(TripStatus s) => s switch
    {
        TripStatus.Planned => "chưa chạy",
        TripStatus.InProgress => "đang chạy",
        TripStatus.PendingConfirmation => "chờ duyệt",
        _ => s.ToString(),
    };

    public async Task<ResourceOverviewDto> GetAsync(int? warehouseId)
    {
        var today = VnClock.Today;
        var soon = today.AddDays(ExpiringDays);

        var warehouses = await _db.Warehouses.OrderBy(w => w.Name)
            .Select(w => new ResourceWarehouseDto(w.Id, w.Name)).ToListAsync();
        var whName = warehouses.ToDictionary(w => w.Id, w => w.Name);

        // Chuyến đang hoạt động (để biết xe / tài xế đang bận chuyến nào).
        var activeTrips = await _db.Trips
            .Where(t => Active.Contains(t.Status))
            .Select(t => new ActiveTrip(t.Id, t.VehicleId, t.DriverEmployeeId, t.Status, t.LegType, t.WarehouseId, t.PlannedStartDate,
                                        t.TransportOrder.Code, t.Vehicle.PlateNumber))
            .ToListAsync();
        static string TripText(ActiveTrip t) => $"#{t.Id} {LegText(t.LegType)} — đơn {t.OrderCode} ({StatusText(t.Status)})";
        // Ưu tiên chuyến đang chạy, rồi chờ duyệt, rồi chưa chạy.
        static int Rank(TripStatus s) => s == TripStatus.InProgress ? 0 : s == TripStatus.PendingConfirmation ? 1 : 2;

        // ---------- XE ----------
        var vehicles = await _db.Vehicles
            .Where(v => warehouseId == null || v.WarehouseId == null || v.WarehouseId == warehouseId)
            .OrderBy(v => v.PlateNumber)
            .Select(v => new { v.Id, v.PlateNumber, v.VehicleType, v.CapacityKg, v.WarehouseId, v.Status })
            .ToListAsync();
        var docs = (await VehicleDocumentRules.GetLatestAsync(_db)).ToLookup(d => d.VehicleId);
        var fuel = await VehicleFuelLevelRules.ComputeAsync(_db);

        var vehicleRows = vehicles.Select(v =>
        {
            var vd = docs[v.Id].ToList();
            DateTime? Exp(VehicleDocumentType t) => vd.FirstOrDefault(d => d.DocType == t)?.ExpiryDate;
            var trip = activeTrips.Where(t => t.VehicleId == v.Id).OrderBy(t => Rank(t.Status)).FirstOrDefault();
            string? reason = v.Status switch
            {
                VehicleStatus.UnderMaintenance => "Đang bảo trì",
                VehicleStatus.OutOfService => "Ngưng hoạt động",
                VehicleStatus.OnTrip => trip is null ? "Đang chạy" : $"Đang bận chuyến #{trip.Id}",
                _ => null,
            };
            reason ??= VehicleDocumentRules.BlockReason(vd, v.PlateNumber, today);
            var expiring = vd.Count(d => VehicleDocumentRules.Required.Contains(d.DocType) && d.ExpiryDate.Date >= today && d.ExpiryDate.Date <= soon);
            var f = fuel.GetValueOrDefault(v.Id);
            return new VehicleResourceDto(v.Id, v.PlateNumber, v.VehicleType, v.CapacityKg,
                DriverLicenseRules.RequiredText(v.CapacityKg, v.VehicleType),
                v.WarehouseId, v.WarehouseId is int w && whName.TryGetValue(w, out var n) ? n : null, v.Status.ToString(),
                Exp(VehicleDocumentType.Registration), Exp(VehicleDocumentType.Insurance), Exp(VehicleDocumentType.TransportPermit),
                f?.CurrentLiters, f?.TankCapacityLiters,
                trip is null ? null : TripText(trip), reason is null, reason, expiring);
        }).ToList();

        // ---------- TÀI XẾ ----------
        var driverPositionIds = await DriverPositionRules.DriverPositionIdsAsync(_db);
        var employees = await _db.Employees
            .Where(e => e.Status == EmployeeStatus.Active
                        && ((e.PositionId != null && driverPositionIds.Contains(e.PositionId.Value)) || e.DriverProfile != null))
            .Select(e => new
            {
                e.Id, e.Code, e.FullName, e.Phone, e.PositionId,
                HasProfile = e.DriverProfile != null,
                WarehouseId = e.DriverProfile != null ? e.DriverProfile.WarehouseId : null,
                LicenseClass = e.DriverProfile != null ? e.DriverProfile.LicenseClass : null,
                LicenseExpiry = e.DriverProfile != null ? (DateTime?)e.DriverProfile.LicenseExpiryDate : null,
                HealthExpiry = e.DriverProfile != null ? e.DriverProfile.HealthCertExpiryDate : null,
            })
            .OrderBy(e => e.FullName)
            .ToListAsync();
        employees = employees.Where(e => warehouseId == null || e.WarehouseId == null || e.WarehouseId == warehouseId).ToList();

        var empIds = employees.Select(e => e.Id).ToList();
        var weekEnd = today.AddDays(7);
        var schedules = await _db.DriverSchedules
            .Where(s => empIds.Contains(s.DriverEmployeeId) && s.ShiftDate >= today && s.ShiftDate < weekEnd)
            .Select(s => new { s.DriverEmployeeId, s.ShiftDate })
            .ToListAsync();

        var driverRows = employees.Select(e =>
        {
            var days = schedules.Where(s => s.DriverEmployeeId == e.Id).Select(s => s.ShiftDate.Date).Distinct().ToList();
            var worksToday = days.Contains(today);
            var trip = activeTrips.Where(t => t.DriverEmployeeId == e.Id).OrderBy(t => Rank(t.Status)).FirstOrDefault();
            var isDriverPosition = e.PositionId is int p && driverPositionIds.Contains(p);
            string? reason =
                !e.HasProfile ? "Chưa có hồ sơ tài xế"
                : !isDriverPosition ? "Chức danh không phải Tài xế"
                : e.LicenseExpiry is DateTime le && le.Date < today ? $"Bằng lái hết hạn {le:dd/MM/yyyy}"
                : e.HealthExpiry is DateTime he && he.Date < today ? $"Giấy khám sức khỏe hết hạn {he:dd/MM/yyyy}"
                : trip is not null ? $"Đang bận chuyến #{trip.Id}"
                : !worksToday ? "Chưa đăng ký ngày làm hôm nay"
                : null;
            var expiring = (e.LicenseExpiry is DateTime l && l.Date >= today && l.Date <= soon ? 1 : 0)
                         + (e.HealthExpiry is DateTime h && h.Date >= today && h.Date <= soon ? 1 : 0);
            var drivable = e.LicenseClass is null ? new List<int>()
                : vehicleRows.Where(v => DriverLicenseRules.BlockReason(e.LicenseClass, v.CapacityKg, v.VehicleType) is null).Select(v => v.Id).ToList();
            return new DriverResourceDto(e.Id, e.Code, e.FullName, e.Phone,
                e.WarehouseId, e.WarehouseId is int w && whName.TryGetValue(w, out var n) ? n : null, e.HasProfile,
                e.LicenseClass, e.LicenseExpiry, e.HealthExpiry, worksToday, days.Count,
                trip is null ? null : TripText(trip), trip?.Plate,
                reason is null, reason, expiring, drivable);
        }).ToList();

        // ---------- TỔNG HỢP ----------
        bool InWarehouse(int? w) => warehouseId == null || w == warehouseId;
        var expiredOrMissing =
            vehicleRows.Count(v => v.RegistrationExpiry is null || v.RegistrationExpiry.Value.Date < today) +
            vehicleRows.Count(v => v.InsuranceExpiry is null || v.InsuranceExpiry.Value.Date < today) +
            vehicleRows.Count(v => v.PermitExpiry is null || v.PermitExpiry.Value.Date < today) +
            driverRows.Count(d => d.HasProfile && d.LicenseExpiry is DateTime l && l.Date < today) +
            driverRows.Count(d => d.HasProfile && d.HealthCertExpiry is DateTime h && h.Date < today);
        var summary = new ResourceSummaryDto(
            vehicleRows.Count(v => v.CanRunToday), vehicleRows.Count,
            driverRows.Count(d => d.CanRunToday), driverRows.Count,
            vehicleRows.Sum(v => v.ExpiringSoonCount) + driverRows.Sum(d => d.ExpiringSoonCount),
            expiredOrMissing,
            activeTrips.Count(t => t.Status == TripStatus.Planned && t.PlannedStartDate != null && t.PlannedStartDate.Value.Date == today && InWarehouse(t.WarehouseId)),
            activeTrips.Count(t => t.Status == TripStatus.InProgress && InWarehouse(t.WarehouseId)));

        return new ResourceOverviewDto(warehouseId, warehouses, summary, vehicleRows, driverRows);
    }
}
