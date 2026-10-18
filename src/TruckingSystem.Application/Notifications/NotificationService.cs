using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Application.Drivers;
using TruckingSystem.Application.Trips;
using TruckingSystem.Application.Vehicles;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.Notifications;

public class NotificationService : INotificationService
{
    // Tiền tố tiêu đề — dùng để PHÂN LOẠI thông báo (lọc trên Desktop) và để
    // nhận ra thông báo nào do hệ thống TỰ SINH (giấy tờ xe, lịch tài xế).
    // Không cần thêm cột mới trong DB -> không cần migration.
    private const string PrefixDocExpired = "🔴 ";
    private const string PrefixDocSoon = "🟡 ";
    private const string PrefixSchedule = "📅 ";
    private const string PrefixTripPending = "🟢 ";
    private const string PrefixDriverDoc = "🪪 ";
    private const string PrefixDeadline = "⏰ ";
    private const string PrefixTripBlocked = "🚫 ";

    public const string CategoryTrip = "Chuyến chờ duyệt";
    public const string CategoryVehicleDoc = "Giấy tờ xe";
    public const string CategorySchedule = "Lịch tài xế";
    public const string CategoryDriverDoc = "Giấy tờ tài xế";
    public const string CategoryDeadline = "Hạn giao";
    public const string CategoryTripBlocked = "Chuyến bị chặn";
    public const string CategoryOther = "Khác";

    private readonly IApplicationDbContext _db;

    public NotificationService(IApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>Phân loại thông báo theo tiền tố tiêu đề.</summary>
    public static string GetCategory(string title)
    {
        if (title.StartsWith(PrefixTripPending)) return CategoryTrip;
        if (title.StartsWith(PrefixDocExpired) || title.StartsWith(PrefixDocSoon)) return CategoryVehicleDoc;
        if (title.StartsWith(PrefixSchedule)) return CategorySchedule;
        if (title.StartsWith(PrefixDriverDoc)) return CategoryDriverDoc;
        if (title.StartsWith(PrefixDeadline)) return CategoryDeadline;
        if (title.StartsWith(PrefixTripBlocked)) return CategoryTripBlocked;
        return CategoryOther;
    }

    private static bool IsSystemAlert(string title)
        => title.StartsWith(PrefixDocExpired) || title.StartsWith(PrefixDocSoon) || title.StartsWith(PrefixSchedule)
           || title.StartsWith(PrefixDriverDoc) || title.StartsWith(PrefixDeadline) || title.StartsWith(PrefixTripBlocked);

    public async Task<List<NotificationDto>> CheckAndGetNotificationsAsync(string userId)
    {
        await SyncSystemAlertsAsync(userId);
        await AutoResolveTripPendingAsync();

        // Lấy CẢ thông báo riêng của user này LẪN thông báo CHUNG (UserId =
        // null, xem CreateBroadcastAsync) — ví dụ tài xế báo xong chuyến, cần
        // Vận hành (bất kỳ ai đang trực) thấy và duyệt.
        var rows = await _db.Notifications
            .Where(n => n.UserId == userId || n.UserId == null)
            .OrderByDescending(n => n.CreatedAt)
            .Take(200) // giới hạn, tránh danh sách phình to vô hạn (Desktop có bộ lọc)
            .Select(n => new { n.Id, n.Title, n.Message, n.IsRead, n.CreatedAt })
            .ToListAsync();

        // CreatedAt lưu giờ UTC (cột timestamp without time zone) -> gắn Kind=Utc
        // để Desktop đổi đúng sang giờ Việt Nam (trước đây bị lệch 7 tiếng).
        return rows
            .Select(n => new NotificationDto(n.Id, n.Title, n.Message, n.IsRead,
                DateTime.SpecifyKind(n.CreatedAt, DateTimeKind.Utc), GetCategory(n.Title)))
            .ToList();
    }

    /// <summary>Tạo thông báo CHUNG (UserId = null) — không kiểm tra trùng lặp
    /// vì mỗi lần gọi hàm này đều ứng với 1 SỰ KIỆN THẬT riêng biệt đã xảy ra
    /// (VD tài xế báo xong chuyến #15 rồi lại báo xong chuyến #16).</summary>
    public async Task CreateBroadcastAsync(string title, string message)
    {
        _db.Notifications.Add(new Notification { UserId = null, Title = title, Message = message, IsRead = false });
        await _db.SaveChangesAsync();
    }

    /// <summary>Chuyến đã được Duyệt / Từ chối / Hoàn tất -> thông báo
    /// "Tài xế báo xong chuyến #id — chờ duyệt" không còn việc gì để làm nữa,
    /// tự đánh dấu đã đọc để danh sách "chưa đọc" chỉ còn việc THẬT SỰ cần xử lý.</summary>
    public async Task ResolveTripPendingAsync(int tripId)
    {
        var titlePrefix = $"{PrefixTripPending}Tài xế báo xong chuyến #{tripId} ";
        var rows = await _db.Notifications
            .Where(n => n.UserId == null && !n.IsRead && n.Title.StartsWith(titlePrefix))
            .ToListAsync();
        if (rows.Count == 0) return;
        foreach (var n in rows) n.IsRead = true;
        await _db.SaveChangesAsync();
    }

    /// <summary>Thông báo "chờ duyệt" CHƯA đọc nhưng chuyến đó đã không còn ở
    /// trạng thái Chờ xác nhận (đã duyệt / từ chối / hủy, kể cả xử lý trước khi
    /// có bản sửa này) -> tự đánh dấu đã đọc.</summary>
    private async Task AutoResolveTripPendingAsync()
    {
        var prefix = $"{PrefixTripPending}Tài xế báo xong chuyến #";
        var pending = await _db.Notifications
            .Where(n => n.UserId == null && !n.IsRead && n.Title.StartsWith(prefix))
            .ToListAsync();
        if (pending.Count == 0) return;

        int? ParseTripId(string title)
        {
            var rest = title.Substring(prefix.Length);
            var digits = new string(rest.TakeWhile(char.IsDigit).ToArray());
            return int.TryParse(digits, out var id) ? id : null;
        }

        var tripIds = pending.Select(n => ParseTripId(n.Title)).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
        var stillPendingIds = await _db.Trips
            .Where(t => tripIds.Contains(t.Id) && t.Status == TripStatus.PendingConfirmation)
            .Select(t => t.Id)
            .ToListAsync();

        var changed = false;
        foreach (var n in pending)
        {
            var id = ParseTripId(n.Title);
            if (id.HasValue && stillPendingIds.Contains(id.Value)) continue;
            n.IsRead = true;
            changed = true;
        }
        if (changed) await _db.SaveChangesAsync();
    }

    /// <summary>ĐỒNG BỘ cảnh báo tự sinh (giấy tờ xe + lịch tài xế) của user
    /// với tình trạng HIỆN TẠI:
    /// 1) Tính ra danh sách cảnh báo ĐÚNG lúc này (tiêu đề + nội dung cố định,
    ///    không chứa số "còn N ngày" thay đổi mỗi ngày).
    /// 2) Cảnh báo nào ĐÃ CÓ (dù đã đọc hay chưa) -> giữ nguyên, KHÔNG tạo thêm.
    ///    (Lỗi cũ: chỉ kiểm tra bản CHƯA đọc, nên bấm vào 1 dòng = đánh dấu đã
    ///    đọc -> lần tải lại kế tiếp tạo lại y hệt dòng đó -> nhân bản mãi.)
    /// 3) Cảnh báo cũ KHÔNG còn đúng (đã gia hạn giấy tờ, tài xế đã đăng ký ca,
    ///    ngày đã qua) hoặc bị TRÙNG -> xóa đi.</summary>
    private async Task SyncSystemAlertsAsync(string userId)
    {
        var expected = new List<(string Title, string Message)>();
        expected.AddRange(await BuildVehicleDocumentAlertsAsync());
        expected.AddRange(await BuildDriverScheduleAlertsAsync());
        expected.AddRange(await BuildDriverDocumentAlertsAsync());
        expected.AddRange(await BuildDeadlineAlertsAsync());
        expected.AddRange(await BuildBlockedTripAlertsAsync());

        var existing = (await _db.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync())
            .Where(n => IsSystemAlert(n.Title))
            .ToList();

        var expectedKeys = expected.Select(e => e.Title + "\n" + e.Message).ToHashSet();
        var keptKeys = new HashSet<string>();
        var changed = false;

        // Giữ lại ĐÚNG 1 bản cho mỗi cảnh báo còn hiệu lực (ưu tiên bản đã đọc,
        // để không "sống lại" thành chưa đọc), xóa bản trùng + bản hết hiệu lực.
        foreach (var n in existing.OrderByDescending(n => n.IsRead).ThenBy(n => n.CreatedAt))
        {
            var key = n.Title + "\n" + n.Message;
            if (expectedKeys.Contains(key) && keptKeys.Add(key)) continue;
            _db.Notifications.Remove(n);
            changed = true;
        }

        foreach (var (title, message) in expected)
        {
            if (!keptKeys.Add(title + "\n" + message)) continue;
            _db.Notifications.Add(new Notification { UserId = userId, Title = title, Message = message, IsRead = false });
            changed = true;
        }

        if (changed) await _db.SaveChangesAsync();
    }

    /// <summary>Giấy tờ xe (Đăng kiểm/Bảo hiểm/Giấy phép vận tải) SẮP hết hạn
    /// (trong 30 ngày) hoặc ĐÃ hết hạn — 1 cảnh báo cho mỗi LOẠI giấy tờ của mỗi xe,
    /// chỉ xét bản MỚI NHẤT (gia hạn = thêm dòng mới; trước đây dòng cũ đã hết hạn
    /// vẫn báo "ĐÃ HẾT HẠN" mãi). So theo NGÀY Việt Nam, cùng quy tắc với Chia xe.</summary>
    private async Task<List<(string Title, string Message)>> BuildVehicleDocumentAlertsAsync()
    {
        var today = VnClock.Today;
        var warningThreshold = today.AddDays(30);

        var latestDocs = await VehicleDocumentRules.GetLatestAsync(_db);
        var docsNeedingAlert = latestDocs.Where(d => d.ExpiryDate.Date <= warningThreshold).ToList();
        if (docsNeedingAlert.Count == 0) return new List<(string Title, string Message)>();

        var vehicleIds = docsNeedingAlert.Select(d => d.VehicleId).Distinct().ToList();
        var plates = await _db.Vehicles
            .Where(v => vehicleIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, v => v.PlateNumber);
        var result = new List<(string Title, string Message)>();
        foreach (var doc in docsNeedingAlert)
        {
            if (!plates.TryGetValue(doc.VehicleId, out var plate)) continue;
            var isExpired = !VehicleDocumentRules.IsValidOn(doc.ExpiryDate, today);
            var docTypeText = VehicleDocumentRules.DocTypeText(doc.DocType);
            var title = isExpired
                ? $"{PrefixDocExpired}{docTypeText} xe {plate} ĐÃ HẾT HẠN"
                : $"{PrefixDocSoon}{docTypeText} xe {plate} sắp hết hạn";
            var message = isExpired
                ? $"{docTypeText} của xe {plate} đã hết hạn từ {doc.ExpiryDate:dd/MM/yyyy} — xe này sẽ KHÔNG được Chia xe cho tới khi gia hạn." +
                  // Câu CỐ ĐỊNH (không liệt kê mã chuyến): cảnh báo được nhận diện theo tiêu đề + nội dung,
                  // nội dung đổi theo danh sách chuyến sẽ làm cảnh báo đã đọc hiện lại thành chưa đọc.
                  " Các chuyến ĐÃ CHIA cho xe này mà chưa chạy (VD chặng Giao đang chờ nhập kho) cũng sẽ KHÔNG xuất phát được — gia hạn hoặc Đổi xe / tài xế."
                : $"{docTypeText} của xe {plate} sẽ hết hạn vào {doc.ExpiryDate:dd/MM/yyyy} — nên gia hạn sớm.";
            result.Add((title, message));
        }
        return result;
    }

    /// <summary>HẠN GIAO (DeliveryDeadlineRules) — nhắc TRƯỚC khi quá muộn, vì chuyến "Đã lên kế
    /// hoạch" để đó bao lâu hệ thống cũng không tự chạy:
    ///   • Đơn đã xác nhận, CHƯA chia xe, hạn giao còn ≤ 2 ngày.
    ///   • Chuyến chưa chạy tới MỐC XUẤT PHÁT MUỘN NHẤT (hôm nay / ngày mai).
    ///   • Chuyến chưa chạy đã QUA mốc đó -> không còn kịp hạn (cần xử lý / báo khách).
    ///   • Đơn chưa xong mà đã QUÁ hạn giao (thay cho các nhắc theo chuyến của đơn đó).
    /// Nội dung CỐ ĐỊNH theo từng tình trạng (không chứa "còn N ngày") — chuyển tình trạng thì
    /// cảnh báo cũ tự biến mất, cảnh báo mới hiện ra (cơ chế đồng bộ ở SyncSystemAlertsAsync).</summary>
    /// <summary>CHUYẾN ĐÃ CHIA NHƯNG CHƯA CHẠY mà giờ KHÔNG THỂ xuất phát vì giấy tờ / tài xế đã
    /// thay đổi sau lúc Chia xe — VD chuyến để đó nhiều ngày: giấy tờ xe, bằng lái, giấy khám hết
    /// hạn trước khi chuyến xong; ngày làm đăng ký lúc chia đã qua mà chưa đăng ký ngày mới; tài xế
    /// nghỉ / đổi chức danh; xe chuyển Bảo trì. Bắt đầu chuyến vốn ĐÃ chặn những trường hợp này,
    /// nhưng trước đây chỉ biết lúc bấm Bắt đầu (hoặc tài xế mở app) — giờ báo TRƯỚC để kịp gia
    /// hạn / đăng ký ngày làm / Đổi xe-tài xế. Xét CÙNG quy tắc với Bắt đầu chuyến, trên khoảng
    /// [ngày xuất phát dự kiến (đã qua thì hôm nay) .. ngày cuối dự kiến của chặng].</summary>
    private async Task<List<(string Title, string Message)>> BuildBlockedTripAlertsAsync()
    {
        const int maxDrivingMinutesPerDay = 480;
        var today = VnClock.Today;
        var trips = await _db.Trips
            .Where(t => t.Status == TripStatus.Planned)
            .Select(t => new
            {
                t.Id, t.LegType, t.VehicleId, t.DriverEmployeeId, t.PlannedStartDate, t.EstimatedDurationMinutes,
                OrderCode = t.TransportOrder.Code,
                Plate = t.Vehicle.PlateNumber, VehicleStatus = t.Vehicle.Status, t.Vehicle.CapacityKg, t.Vehicle.VehicleType,
            })
            .ToListAsync();
        if (trips.Count == 0) return new List<(string Title, string Message)>();

        var docsByVehicle = (await VehicleDocumentRules.GetLatestAsync(_db)).ToLookup(d => d.VehicleId);
        var driverIds = trips.Select(t => t.DriverEmployeeId).Distinct().ToList();
        var drivers = await _db.Employees
            .Where(e => driverIds.Contains(e.Id))
            .Select(e => new
            {
                e.Id, e.FullName, e.Status,
                PositionName = e.Position != null ? e.Position.Name : null,
                HasProfile = e.DriverProfile != null,
                LicenseClass = e.DriverProfile != null ? e.DriverProfile.LicenseClass : null,
                LicenseExpiry = e.DriverProfile != null ? (DateTime?)e.DriverProfile.LicenseExpiryDate : null,
                HealthExpiry = e.DriverProfile != null ? e.DriverProfile.HealthCertExpiryDate : null,
            })
            .ToDictionaryAsync(e => e.Id);
        var workDays = (await _db.DriverSchedules
                .Where(s => driverIds.Contains(s.DriverEmployeeId) && s.ShiftDate >= today)
                .Select(s => new { s.DriverEmployeeId, s.ShiftDate })
                .ToListAsync())
            .GroupBy(s => s.DriverEmployeeId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ShiftDate.Date).ToHashSet());

        var result = new List<(string Title, string Message)>();
        foreach (var t in trips)
        {
            var from = t.PlannedStartDate is DateTime p && p.Date > today ? p.Date : today;
            var days = Math.Max(1, (int)Math.Ceiling((t.EstimatedDurationMinutes ?? 0) / (double)maxDrivingMinutesPerDay));
            var last = from.AddDays(days - 1);

            string? reason = t.VehicleStatus switch
            {
                VehicleStatus.UnderMaintenance => $"Xe {t.Plate} đang Bảo trì",
                VehicleStatus.OutOfService => $"Xe {t.Plate} đang Ngưng hoạt động",
                _ => VehicleDocumentRules.BlockReason(docsByVehicle[t.VehicleId], t.Plate, last),
            };
            if (reason is null)
            {
                if (!drivers.TryGetValue(t.DriverEmployeeId, out var d) || !d.HasProfile)
                    reason = "Tài xế của chuyến chưa có hồ sơ tài xế";
                else if (d.Status != EmployeeStatus.Active)
                    reason = $"Tài xế {d.FullName} không còn ở trạng thái Đang làm việc";
                else if (!DriverPositionRules.IsDriverPosition(d.PositionName))
                    reason = $"Tài xế {d.FullName} có chức danh \"{d.PositionName ?? "(trống)"}\" — không phải Tài xế";
                else if (DriverLicenseRules.BlockReason(d.LicenseClass, t.CapacityKg, t.VehicleType) is { } lic)
                    reason = $"Tài xế {d.FullName} không lái được xe {t.Plate}: {lic}";
                else if (d.LicenseExpiry is DateTime le && le.Date < last)
                    reason = $"Bằng lái của tài xế {d.FullName} hết hạn {le:dd/MM/yyyy}, trước khi chuyến xong ({last:dd/MM/yyyy})";
                else if (d.HealthExpiry is DateTime he && he.Date < last)
                    reason = $"Giấy khám sức khỏe của tài xế {d.FullName} hết hạn {he:dd/MM/yyyy}, trước khi chuyến xong ({last:dd/MM/yyyy})";
                else
                {
                    var registered = workDays.TryGetValue(t.DriverEmployeeId, out var set) ? set : new HashSet<DateTime>();
                    var missing = Enumerable.Range(0, days).Select(i => from.AddDays(i)).Where(x => !registered.Contains(x)).ToList();
                    if (missing.Count > 0)
                        reason = $"Tài xế {d.FullName} chưa đăng ký ngày làm {string.Join(", ", missing.Select(x => x.ToString("dd/MM")))}";
                }
            }
            if (reason is null) continue;

            var leg = t.LegType switch { TripLegType.Pickup => "Lấy hàng", TripLegType.Return => "Trả hàng", _ => "Giao hàng" };
            result.Add(($"{PrefixTripBlocked}Chuyến #{t.Id} ({leg}, đơn {t.OrderCode}) chưa thể xuất phát",
                $"{reason}. Chuyến dự kiến chạy {from:dd/MM}" + (days > 1 ? $" – {last:dd/MM}" : "") +
                " — gia hạn giấy tờ / đăng ký ngày làm cho tài xế, hoặc Đổi xe / tài xế ở màn Chuyến trước khi xuất phát."));
        }
        return result;
    }

    private async Task<List<(string Title, string Message)>> BuildDeadlineAlertsAsync()
    {
        var today = VnClock.Today;
        var orders = await _db.TransportOrders
            .Where(o => o.CommittedDeliveryDate != null
                        && (o.Status == TransportOrderStatus.Confirmed || o.Status == TransportOrderStatus.InProgress))
            .Select(o => new { o.Id, o.Code, Due = o.CommittedDeliveryDate!.Value })
            .ToListAsync();
        if (orders.Count == 0) return new List<(string Title, string Message)>();

        var orderIds = orders.Select(o => o.Id).ToList();
        var tripsByOrder = (await _db.Trips.Where(t => orderIds.Contains(t.TransportOrderId)).ToListAsync())
            .GroupBy(t => t.TransportOrderId).ToDictionary(g => g.Key, g => g.ToList());

        var result = new List<(string Title, string Message)>();
        foreach (var o in orders)
        {
            var due = o.Due.Date;
            var trips = tripsByOrder.TryGetValue(o.Id, out var list) ? list : new List<Trip>();
            var active = trips.Where(t => t.Status != TripStatus.Cancelled).ToList();

            if (today > due)
            {
                result.Add(($"{PrefixDeadline}Đơn {o.Code} ĐÃ QUÁ HẠN giao {due:dd/MM}",
                    $"Đơn {o.Code} có hạn giao {due:dd/MM/yyyy} nhưng chưa giao xong — kiểm tra các chuyến của đơn và báo khách thời gian giao mới."));
                continue;
            }
            if (active.Count == 0)
            {
                if ((due - today).Days <= 2)
                    result.Add(($"{PrefixDeadline}Đơn {o.Code} chưa chia xe — hạn giao {due:dd/MM}",
                        $"Đơn {o.Code} đã xác nhận nhưng CHƯA chia xe, trong khi hạn giao là {due:dd/MM/yyyy} — Chia xe ngay để kịp hạn."));
                continue;
            }

            foreach (var t in active.Where(t => t.Status == TripStatus.Planned && t.LegType != TripLegType.Return))
            {
                var info = DeliveryDeadlineRules.Evaluate(t, trips, o.Due, today);
                if (info.LatestStart is not { } latest) continue;
                var leg = t.LegType == TripLegType.Pickup ? "Lấy hàng" : "Giao hàng";
                if (info.Level == DeliveryDeadlineRules.Level.Warning)
                    result.Add(($"{PrefixDeadline}Chuyến #{t.Id} ({leg}, đơn {o.Code}) phải xuất phát trước hết {latest:dd/MM}",
                        $"Chuyến #{t.Id} ({leg}) của đơn {o.Code} chưa chạy. Hạn giao {due:dd/MM/yyyy} — để kịp hạn phải xuất phát chậm nhất hết ngày {latest:dd/MM/yyyy}."));
                else if (info.Level == DeliveryDeadlineRules.Level.Late)
                    result.Add(($"{PrefixDeadline}Chuyến #{t.Id} ({leg}, đơn {o.Code}) KHÔNG CÒN KỊP hạn giao {due:dd/MM}",
                        $"Chuyến #{t.Id} ({leg}) của đơn {o.Code} chưa chạy và đã qua mốc xuất phát muộn nhất ({latest:dd/MM/yyyy}) — dự kiến giao trễ so với hạn {due:dd/MM/yyyy}. Cho xe xuất phát ngay / đổi xe-tài xế và báo trước cho khách."));
            }
        }
        return result;
    }

    /// <summary>Giấy tờ TÀI XẾ (bằng lái, giấy khám sức khỏe) SẮP hết hạn (trong 30 ngày) hoặc
    /// ĐÃ hết hạn — tương tự giấy tờ xe. Trước đây chỉ có cảnh báo giấy tờ xe: bằng lái hết hạn
    /// thì tài xế tự nhiên bị loại khỏi Chia xe / không xuất phát được mà không ai được báo trước.
    /// Chỉ xét người ĐANG LÀM có chức danh Tài xế (cùng quy tắc màn Tài xế).</summary>
    private async Task<List<(string Title, string Message)>> BuildDriverDocumentAlertsAsync()
    {
        var today = VnClock.Today;
        var warningThreshold = today.AddDays(30);
        var driverPositionIds = await DriverPositionRules.DriverPositionIdsAsync(_db);

        var drivers = await _db.DriverProfiles
            .Where(dp => dp.Employee.Status == EmployeeStatus.Active
                         && dp.Employee.PositionId != null && driverPositionIds.Contains(dp.Employee.PositionId.Value)
                         && (dp.LicenseExpiryDate <= warningThreshold
                             || (dp.HealthCertExpiryDate != null && dp.HealthCertExpiryDate <= warningThreshold)))
            .Select(dp => new { dp.Employee.FullName, dp.Employee.Code, dp.LicenseExpiryDate, dp.HealthCertExpiryDate })
            .ToListAsync();

        var result = new List<(string Title, string Message)>();
        foreach (var d in drivers)
        {
            var who = $"{d.FullName} ({d.Code})";
            if (d.LicenseExpiryDate.Date <= warningThreshold)
            {
                var expired = d.LicenseExpiryDate.Date < today; // ngày hết hạn vẫn tính còn hạn (giống Chia xe)
                result.Add(expired
                    ? ($"{PrefixDriverDoc}Bằng lái tài xế {who} ĐÃ HẾT HẠN",
                       $"Bằng lái của tài xế {who} đã hết hạn từ {d.LicenseExpiryDate:dd/MM/yyyy} — tài xế này sẽ KHÔNG được Chia xe và các chuyến đã chia chưa chạy cũng KHÔNG xuất phát được. Cập nhật hồ sơ (màn Tài xế) hoặc Đổi xe / tài xế.")
                    : ($"{PrefixDriverDoc}Bằng lái tài xế {who} sắp hết hạn",
                       $"Bằng lái của tài xế {who} sẽ hết hạn vào {d.LicenseExpiryDate:dd/MM/yyyy} — nhắc tài xế gia hạn sớm, sau ngày đó sẽ không được Chia xe."));
            }
            if (d.HealthCertExpiryDate is { } health && health.Date <= warningThreshold)
            {
                var expired = health.Date < today;
                result.Add(expired
                    ? ($"{PrefixDriverDoc}Giấy khám sức khỏe tài xế {who} ĐÃ HẾT HẠN",
                       $"Giấy khám sức khỏe của tài xế {who} đã hết hạn từ {health:dd/MM/yyyy} — nhắc tài xế khám lại và cập nhật hồ sơ (màn Tài xế).")
                    : ($"{PrefixDriverDoc}Giấy khám sức khỏe tài xế {who} sắp hết hạn",
                       $"Giấy khám sức khỏe của tài xế {who} sẽ hết hạn vào {health:dd/MM/yyyy} — nhắc tài xế khám lại sớm."));
            }
        }
        return result;
    }

    /// <summary>Tài xế (đang Active, bằng lái còn hạn) CHƯA đăng ký ca làm việc
    /// cho NGÀY MAI — cảnh báo sớm 1 ngày để Dispatcher nhắc tài xế.</summary>
    private async Task<List<(string Title, string Message)>> BuildDriverScheduleAlertsAsync()
    {
        // "Ngày mai" theo GIỜ VIỆT NAM (UTC+7) — cùng quy ước với Chia xe.
        var tomorrow = VnClock.Today.AddDays(1);

        // Chỉ người có chức danh Tài xế (cùng quy tắc màn Tài xế / Chia xe).
        var driverPositionIds = await DriverPositionRules.DriverPositionIdsAsync(_db);
        var activeDrivers = await _db.DriverProfiles
            .Include(dp => dp.Employee)
            .Where(dp => dp.Employee.Status == EmployeeStatus.Active && dp.LicenseExpiryDate >= tomorrow
                         && dp.Employee.PositionId != null && driverPositionIds.Contains(dp.Employee.PositionId.Value))
            .ToListAsync();

        var driverIdsScheduledTomorrow = await _db.DriverSchedules
            .Where(s => s.ShiftDate.Date == tomorrow)
            .Select(s => s.DriverEmployeeId)
            .Distinct()
            .ToListAsync();

        return activeDrivers
            .Where(d => !driverIdsScheduledTomorrow.Contains(d.EmployeeId))
            .Select(d => (
                Title: $"{PrefixSchedule}Tài xế {d.Employee.FullName} ({d.Employee.Code}) chưa đăng ký ngày làm {tomorrow:dd/MM}",
                Message: $"Tài xế {d.Employee.FullName} chưa đăng ký ngày làm {tomorrow:dd/MM/yyyy} (màn Tài xế → Ngày làm việc) — nếu không đăng ký, sẽ KHÔNG được Chia xe cho các đơn cần chạy vào ngày đó."))
            .ToList();
    }

    public async Task MarkAsReadAsync(int notificationId, string userId)
    {
        // Chỉ đánh dấu thông báo CỦA MÌNH hoặc thông báo chung — không đụng được
        // thông báo riêng của người khác.
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && (n.UserId == userId || n.UserId == null));
        if (notification is null) return;

        notification.IsRead = true;
        await _db.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        // Gồm cả thông báo CHUNG (UserId = null) — nếu không, bấm "Đánh dấu tất cả đã
        // đọc" sẽ bỏ sót đúng loại thông báo "tài xế báo xong chuyến, chờ duyệt".
        // Lưu ý: thông báo chung dùng CHUNG 1 cờ IsRead cho mọi người — 1 người đã
        // đọc thì người khác cũng thấy là đã đọc (chấp nhận được với quy mô đồ án).
        var unread = await _db.Notifications.Where(n => (n.UserId == userId || n.UserId == null) && !n.IsRead).ToListAsync();
        foreach (var n in unread) n.IsRead = true;
        await _db.SaveChangesAsync();
    }
}
