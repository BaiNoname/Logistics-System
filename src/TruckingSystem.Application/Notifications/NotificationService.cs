using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Common.Interfaces;
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

    public const string CategoryTrip = "Chuyến chờ duyệt";
    public const string CategoryVehicleDoc = "Giấy tờ xe";
    public const string CategorySchedule = "Lịch tài xế";
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
        return CategoryOther;
    }

    private static bool IsSystemAlert(string title)
        => title.StartsWith(PrefixDocExpired) || title.StartsWith(PrefixDocSoon) || title.StartsWith(PrefixSchedule);

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
                ? $"{docTypeText} của xe {plate} đã hết hạn từ {doc.ExpiryDate:dd/MM/yyyy} — xe này sẽ KHÔNG được Chia xe cho tới khi gia hạn."
                : $"{docTypeText} của xe {plate} sẽ hết hạn vào {doc.ExpiryDate:dd/MM/yyyy} — nên gia hạn sớm.";
            result.Add((title, message));
        }
        return result;
    }

    /// <summary>Tài xế (đang Active, bằng lái còn hạn) CHƯA đăng ký ca làm việc
    /// cho NGÀY MAI — cảnh báo sớm 1 ngày để Dispatcher nhắc tài xế.</summary>
    private async Task<List<(string Title, string Message)>> BuildDriverScheduleAlertsAsync()
    {
        // "Ngày mai" theo GIỜ VIỆT NAM (UTC+7) — cùng quy ước với Chia xe.
        var tomorrow = VnClock.Today.AddDays(1);

        var activeDrivers = await _db.DriverProfiles
            .Include(dp => dp.Employee)
            .Where(dp => dp.Employee.Status == EmployeeStatus.Active && dp.LicenseExpiryDate >= tomorrow)
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
