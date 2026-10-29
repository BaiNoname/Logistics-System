using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TruckingSystem.Domain.Common;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Infrastructure.Persistence;

/// <summary>NHẬT KÝ HỆ THỐNG TỰ ĐỘNG: mỗi lần lưu database, ghi lại mọi bản ghi nghiệp vụ được THÊM / SỬA / XÓA —
/// ai làm (người đăng nhập của request), lúc nào, đối tượng nào, đổi field gì từ giá trị cũ → mới. Không phải chèn
/// code ghi log vào từng chức năng; chức năng mới thêm sau này cũng tự có log.
///  • Bỏ qua bảng sinh tự động / số lượng lớn: GPS, video, lịch Google, thông báo, tồn kho & xếp kệ (đã có phiếu
///    nhập / xuất ghi lại), chính bảng nhật ký.
///  • Sửa chỉ ghi khi có field THẬT SỰ đổi giá trị (bỏ qua UpdatedAt / CreatedAt / Version).
///  • Chỉ đổi Trạng thái -> hành động "StatusChange" (VD chuyến Đã lên kế hoạch → Đang chạy).
///  • Trong cùng lần lưu đã có dòng nhật ký nghiệp vụ ghi tay cho đúng đối tượng (VD "Đổi hạn giao") thì không ghi
///    thêm dòng "Sửa" trùng; dòng ghi tay chưa có người thao tác thì tự điền.
/// Dòng nhật ký của bản ghi MỚI ghi sau khi lưu (lúc đó mới có Id) bằng 1 lần lưu phụ trong cùng transaction.</summary>
public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private static readonly HashSet<string> SkippedEntities = new()
    {
        nameof(AuditLog), nameof(Notification), nameof(Waypoint), nameof(TripVideoSegment), nameof(TripCalendarEvent),
        nameof(WarehouseStock), nameof(StorageAssignment),
        // Bản ghi phụ sinh TỰ ĐỘNG cùng 1 thao tác đã có dòng nhật ký riêng -> ghi nữa chỉ làm loãng:
        //  • TripLine (dòng hàng của chuyến): tạo khi Chia xe (đã có dòng "Chia xe"), đổi trạng thái khi chuyến chạy / xong.
        //  • TripCargoPhoto: mỗi ảnh tài xế chụp 1 dòng — đã có dòng "Đổi trạng thái chuyến" lúc tài xế báo xong.
        //  • DeliveryDeadlineChange: đã có dòng ghi tay "Đổi hạn giao" / "Ghi nhận phí lưu kho" trên đơn.
        nameof(TripLine), nameof(TripCargoPhoto), nameof(DeliveryDeadlineChange),
    };

    /// <summary>Field hệ thống TỰ tính lại — đổi giá trị không phải do người thao tác, không ghi nhật ký:
    /// m³ đã dùng của ô kệ (đổi theo mỗi phiếu nhập / xuất — phiếu đã được ghi), km / thời gian ước tính và tiền xăng
    /// thực tế của chuyến (tự tính), mốc Từ chối (đã có dòng đổi trạng thái).</summary>
    private static readonly HashSet<string> SkippedProperties = new()
    {
        "Id", "CreatedAt", "UpdatedAt", "Version", "PasswordHash",
        "CurrentUsedM3", "EstimatedDistanceKm", "EstimatedDurationMinutes", "ActualFuelCost", "ConfirmRejectedAt",
    };

    private readonly IHttpContextAccessor _http;
    private List<(EntityEntry Entry, AuditLog Log)>? _pendingAdded;
    private bool _writing;

    public AuditSaveChangesInterceptor(IHttpContextAccessor http) => _http = http;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Capture(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Capture(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (TakeAdded(eventData.Context) is { } ctx)
        {
            try { _writing = true; ctx.SaveChanges(); }
            finally { _writing = false; }
        }
        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = default)
    {
        if (TakeAdded(eventData.Context) is { } ctx)
        {
            try { _writing = true; await ctx.SaveChangesAsync(cancellationToken); }
            finally { _writing = false; }
        }
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        _pendingAdded = null;
        base.SaveChangesFailed(eventData);
    }

    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        _pendingAdded = null;
        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    // ------------------------------------------------------------------ ghi nhận thay đổi

    private void Capture(DbContext? context)
    {
        if (_writing || context is null) return;
        var (userId, userName, userRole) = CurrentUser();
        var now = DateTime.UtcNow;

        // Dòng nhật ký ghi tay trong lần lưu này: điền người thao tác + nhớ đối tượng để không ghi "Sửa" trùng.
        var manual = new HashSet<(string, int?)>();
        foreach (var e in context.ChangeTracker.Entries<AuditLog>().Where(e => e.State == EntityState.Added))
        {
            e.Entity.UserId ??= userId;
            e.Entity.UserName ??= userName;
            if (e.Entity.UserName == userName) e.Entity.UserRole ??= userRole;
            manual.Add((e.Entity.EntityName, e.Entity.EntityId));
        }

        var logs = new List<AuditLog>();
        _pendingAdded = new();
        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>().ToList())
        {
            var type = entry.Metadata.ClrType.Name;
            if (SkippedEntities.Contains(type)) continue;
            switch (entry.State)
            {
                case EntityState.Added:
                    _pendingAdded.Add((entry, NewLog("Create", type, null, $"Thêm {Label(type)} {Title(entry, false)}".TrimEnd() +
                        Join(" · ", entry.Properties.Where(p => Show(p) && p.CurrentValue is not null)
                            .Take(10).Select(p => $"{PropLabel(p)}: {Fmt(p.CurrentValue)}")), userId, userName, now)));
                    break;
                case EntityState.Deleted:
                    logs.Add(NewLog("Delete", type, entry.Entity.Id, $"Xóa {Label(type)} {Title(entry, true)}".TrimEnd() +
                        Join(" · ", entry.Properties.Where(p => Show(p) && p.OriginalValue is not null)
                            .Take(10).Select(p => $"{PropLabel(p)}: {Fmt(p.OriginalValue)}")), userId, userName, now));
                    break;
                case EntityState.Modified:
                    if (manual.Contains((type, entry.Entity.Id))) break;
                    var changed = entry.Properties.Where(p => p.IsModified && Show(p) && !Equals(p.OriginalValue, p.CurrentValue)).ToList();
                    if (changed.Count == 0) break;
                    if (IsAutoVehicleStatus(type, changed)) break;
                    var action = changed.All(p => p.Metadata.Name == "Status") ? "StatusChange" : "Update";
                    logs.Add(NewLog(action, type, entry.Entity.Id, $"Sửa {Label(type)} {Title(entry, false)}".TrimEnd() +
                        Join(" · ", changed.Select(p => $"{PropLabel(p)}: {Fmt(p.OriginalValue)} → {Fmt(p.CurrentValue)}")), userId, userName, now));
                    break;
            }
        }
        if (logs.Count > 0) context.Set<AuditLog>().AddRange(logs);
        if (_pendingAdded.Count == 0) _pendingAdded = null;
    }

    /// <summary>Xe tự chuyển "Đang chạy" khi chuyến bắt đầu / về "Rảnh" khi chuyến xong — hệ thống tự làm theo chuyến
    /// (chuyến đã có dòng đổi trạng thái riêng) -> không ghi. Đổi tay sang Bảo trì / Ngưng hoạt động vẫn ghi bình thường.</summary>
    private static bool IsAutoVehicleStatus(string type, List<PropertyEntry> changed)
        => type == nameof(Vehicle)
           && changed.Count == 1 && changed[0].Metadata.Name == "Status"
           && (Equals(changed[0].OriginalValue?.ToString(), "OnTrip") || Equals(changed[0].CurrentValue?.ToString(), "OnTrip"));

    /// <summary>Bản ghi MỚI đã có Id sau khi lưu -> thêm dòng nhật ký; trả về context nếu cần lưu phụ.</summary>
    private DbContext? TakeAdded(DbContext? context)
    {
        if (_writing || context is null || _pendingAdded is null) return null;
        var pending = _pendingAdded;
        _pendingAdded = null;
        foreach (var (entry, log) in pending)
        {
            log.EntityId = (entry.Entity as BaseEntity)?.Id;
            context.Set<AuditLog>().Add(log);
        }
        return context;
    }

    private AuditLog NewLog(string action, string entity, int? id, string detail, string? userId, string? userName, DateTime now)
        => new()
        {
            Action = action,
            EntityName = entity,
            EntityId = id,
            Detail = detail.Length > 2000 ? detail[..2000] + "…" : detail,
            UserId = userId,
            UserName = userName,
            UserRole = CurrentUser().Role,
            Timestamp = now,
        };

    private (string? Id, string? Name, string? Role) CurrentUser()
    {
        var user = _http.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true) return (null, null, null);
        var roles = string.Join(",", user.FindAll(ClaimTypes.Role).Select(c => c.Value).Distinct());
        return (user.FindFirst(ClaimTypes.NameIdentifier)?.Value, user.FindFirst(ClaimTypes.Name)?.Value ?? user.Identity.Name,
                roles.Length == 0 ? null : roles);
    }

    // ------------------------------------------------------------------ hiển thị tiếng Việt

    private static string Join(string sep, IEnumerable<string> parts)
    {
        var list = parts.ToList();
        return list.Count == 0 ? "" : " — " + string.Join(sep, list);
    }

    private static bool Show(PropertyEntry p)
    {
        var name = p.Metadata.Name;
        if (SkippedProperties.Contains(name) || p.Metadata.IsShadowProperty()) return false;
        var t = Nullable.GetUnderlyingType(p.Metadata.ClrType) ?? p.Metadata.ClrType;
        return t != typeof(byte[]) && !name.Contains("Token", StringComparison.OrdinalIgnoreCase)
               && !name.Contains("Password", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>"đơn TO-2026...", "xe 51C-123.45", "kho Kho HCM" — lấy field mang tên đầu tiên có giá trị.</summary>
    private static string Title(EntityEntry entry, bool original)
    {
        foreach (var name in new[] { "Code", "PlateNumber", "Name", "FullName", "LicenseNumber", "Title" })
        {
            var prop = entry.Metadata.FindProperty(name);
            if (prop is null) continue;
            var v = original ? entry.Property(name).OriginalValue : entry.Property(name).CurrentValue;
            if (v is string s && s.Length > 0) return s;
        }
        return "";
    }

    private static string Fmt(object? v) => v switch
    {
        null => "(trống)",
        bool b => b ? "Có" : "Không",
        DateTime d => d.TimeOfDay == TimeSpan.Zero ? d.ToString("dd/MM/yyyy") : d.AddHours(d.Kind == DateTimeKind.Local ? 0 : 7).ToString("dd/MM/yyyy HH:mm"),
        decimal m => (m / 1.000000000000000000000000000000000m).ToString("#,##0.####", CultureInfo.GetCultureInfo("vi-VN")),
        double x => x.ToString("0.######", CultureInfo.InvariantCulture),
        Enum e => EnumLabels.TryGetValue(e.ToString(), out var l) ? l : e.ToString(),
        string s => s.Length > 120 ? s[..120] + "…" : s,
        _ => v.ToString() ?? "",
    };

    private static string PropLabel(PropertyEntry p) => PropLabels.TryGetValue(p.Metadata.Name, out var l) ? l : p.Metadata.Name;

    private static string Label(string entity) => EntityLabels.TryGetValue(entity, out var l) ? l : entity;

    private static readonly Dictionary<string, string> EntityLabels = new()
    {
        ["TransportOrder"] = "đơn vận chuyển",
        ["CargoLine"] = "dòng hàng",
        ["Trip"] = "chuyến",
        ["TripLine"] = "dòng hàng của chuyến",
        ["Vehicle"] = "xe",
        ["VehicleDocument"] = "giấy tờ xe",
        ["Warehouse"] = "kho",
        ["WarehouseZone"] = "khu vực kho",
        ["StorageBin"] = "vị trí lưu trữ",
        ["GoodsReceipt"] = "phiếu nhập kho",
        ["GoodsIssue"] = "phiếu xuất kho",
        ["Location"] = "địa điểm",
        ["DriverProfile"] = "hồ sơ tài xế",
        ["DriverSchedule"] = "lịch làm tài xế",
        ["FuelLog"] = "lần đổ xăng",
        ["FuelProvider"] = "hãng nhiên liệu",
        ["FuelPriceHistory"] = "giá nhiên liệu",
        ["TripExpense"] = "chi phí chuyến",
        ["IncidentReport"] = "sự cố",
        ["TripCargoPhoto"] = "ảnh hàng",
        ["DeliveryDeadlineChange"] = "lịch sử đổi hạn giao",
        ["Customer"] = "khách hàng",
        ["CustomerContact"] = "liên hệ khách hàng",
        ["Contract"] = "hợp đồng",
        ["PriceList"] = "bảng giá",
        ["TariffRule"] = "quy tắc giá",
        ["Invoice"] = "hóa đơn",
        ["InvoiceLine"] = "dòng hóa đơn",
        ["Payment"] = "thanh toán",
        ["Employee"] = "nhân viên",
        ["EmployeeDocument"] = "giấy tờ nhân viên",
        ["Department"] = "phòng ban",
        ["Position"] = "chức danh",
        ["CustomerFeedback"] = "đánh giá của khách",
    };

    private static readonly Dictionary<string, string> PropLabels = new()
    {
        ["Status"] = "Trạng thái",
        ["Code"] = "Mã",
        ["Name"] = "Tên",
        ["FullName"] = "Họ tên",
        ["Address"] = "Địa chỉ",
        ["CommittedDeliveryDate"] = "Hạn giao",
        ["PickupPoint"] = "Điểm lấy",
        ["DropoffPoint"] = "Điểm giao",
        ["PickupMethod"] = "Cách lấy hàng",
        ["CustomerId"] = "Khách hàng #",
        ["ContractId"] = "Hợp đồng #",
        ["WarehouseId"] = "Kho #",
        ["TransportOrderId"] = "Đơn #",
        ["VehicleId"] = "Xe #",
        ["DriverEmployeeId"] = "Tài xế #",
        ["TripId"] = "Chuyến #",
        ["CargoLineId"] = "Dòng hàng #",
        ["LegType"] = "Chặng",
        ["StartTime"] = "Giờ bắt đầu",
        ["EndTime"] = "Giờ kết thúc",
        ["PlannedStartDate"] = "Ngày xuất phát",
        ["CancelReason"] = "Lý do hủy",
        ["PlateNumber"] = "Biển số",
        ["VehicleType"] = "Loại xe",
        ["CapacityKg"] = "Tải trọng (kg)",
        ["CapacityQty"] = "Sức chứa",
        ["FuelType"] = "Nhiên liệu",
        ["FuelTankCapacityLiters"] = "Dung tích bình (L)",
        ["FuelLevelLiters"] = "Mức nhiên liệu (L)",
        ["StorageFeePerM3PerDay"] = "Phí lưu kho (đ/m³/ngày)",
        ["QtyReceived"] = "SL nhập",
        ["QtyIssued"] = "SL xuất",
        ["QtyTotal"] = "Số lượng",
        ["QtyPlanned"] = "SL kế hoạch",
        ["QtyDelivered"] = "SL đã giao",
        ["Liters"] = "Số lít",
        ["Cost"] = "Chi phí",
        ["Amount"] = "Số tiền",
        ["Note"] = "Ghi chú",
        ["Description"] = "Mô tả",
        ["Type"] = "Loại",
        ["Reason"] = "Lý do",
        ["ExpiryDate"] = "Hết hạn",
        ["IssueDate"] = "Ngày cấp",
        ["DocType"] = "Loại giấy tờ",
        ["LicenseNumber"] = "Số bằng lái",
        ["LicenseClass"] = "Hạng bằng",
        ["LicenseExpiryDate"] = "Bằng lái hết hạn",
        ["HealthCertExpiryDate"] = "Giấy khám SK hết hạn",
        ["CargoType"] = "Loại hàng",
        ["WeightKg"] = "Khối lượng (kg)",
        ["VolumeM3"] = "Thể tích (m³)",
        ["PackageCount"] = "Số kiện",
        ["ProgressPercent"] = "Tiến độ (%)",
        ["ReversedAt"] = "Hoàn tác lúc",
        ["ReversedByUserName"] = "Người hoàn tác",
        ["PerformedByUserName"] = "Người thực hiện",
        ["LengthMeters"] = "Dài (m)",
        ["WidthMeters"] = "Rộng (m)",
        ["HeightMeters"] = "Cao (m)",
        ["PosX"] = "Vị trí X",
        ["PosY"] = "Vị trí Y",
        ["CapacityM3"] = "Sức chứa (m³)",
        ["MaxWeightKg"] = "Tải tối đa (kg)",
        ["ZoneType"] = "Loại khu",
        ["Lat"] = "Vĩ độ",
        ["Lng"] = "Kinh độ",
        ["Phone"] = "Điện thoại",
        ["Email"] = "Email",
    };

    private static readonly Dictionary<string, string> EnumLabels = new()
    {
        ["Draft"] = "Nháp",
        ["Confirmed"] = "Đã xác nhận",
        ["InProgress"] = "Đang chạy",
        ["Done"] = "Hoàn tất",
        ["Cancelled"] = "Đã hủy",
        ["Planned"] = "Đã lên kế hoạch",
        ["Completed"] = "Hoàn tất",
        ["PendingConfirmation"] = "Chờ xác nhận",
        ["Available"] = "Rảnh",
        ["OnTrip"] = "Đang chạy chuyến",
        ["UnderMaintenance"] = "Bảo trì",
        ["OutOfService"] = "Ngưng hoạt động",
        ["Pickup"] = "Lấy hàng",
        ["Delivery"] = "Giao hàng",
        ["Return"] = "Trả hàng",
        ["CompanyPickup"] = "Công ty đi lấy",
        ["SelfDelivery"] = "Khách tự mang tới",
        ["Customer"] = "Khách hàng",
        ["Company"] = "Công ty",
    };
}
