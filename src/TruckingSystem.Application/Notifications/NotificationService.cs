using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.Notifications;

public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _db;

    public NotificationService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<NotificationDto>> CheckAndGetNotificationsAsync(string userId)
    {
        await GenerateVehicleDocumentAlertsAsync(userId);
        await GenerateDriverScheduleAlertsAsync(userId);

        return await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50) // giới hạn 50 thông báo gần nhất, tránh danh sách phình to vô hạn
            .Select(n => new NotificationDto(n.Id, n.Title, n.Message, n.IsRead, n.CreatedAt))
            .ToListAsync();
    }

    /// <summary>Quét TOÀN BỘ giấy tờ xe (Đăng kiểm/Bảo hiểm/Giấy phép vận tải)
    /// SẮP hết hạn (trong 30 ngày) hoặc ĐÃ hết hạn — tạo 1 thông báo RIÊNG cho
    /// mỗi giấy tờ, chỉ tạo nếu CHƯA có thông báo CÙNG TIÊU ĐỀ đang chưa đọc
    /// (tránh spam trùng lặp mỗi lần người dùng mở app).</summary>
    private async Task GenerateVehicleDocumentAlertsAsync(string userId)
    {
        var today = DateTime.UtcNow;
        var warningThreshold = today.AddDays(30);

        var docsNeedingAlert = await _db.VehicleDocuments
            .Include(d => d.Vehicle)
            .Where(d => d.ExpiryDate <= warningThreshold)
            .ToListAsync();

        foreach (var doc in docsNeedingAlert)
        {
            var isExpired = doc.ExpiryDate < today;
            var docTypeText = doc.DocType switch
            {
                VehicleDocumentType.Registration => "Đăng kiểm",
                VehicleDocumentType.Insurance => "Bảo hiểm",
                VehicleDocumentType.TransportPermit => "Giấy phép vận tải",
                _ => "Giấy tờ",
            };
            var title = isExpired
                ? $"🔴 {docTypeText} xe {doc.Vehicle.PlateNumber} ĐÃ HẾT HẠN"
                : $"🟡 {docTypeText} xe {doc.Vehicle.PlateNumber} sắp hết hạn";

            var alreadyExists = await _db.Notifications.AnyAsync(n => n.UserId == userId && n.Title == title && !n.IsRead);
            if (alreadyExists) continue;

            var message = isExpired
                ? $"{docTypeText} của xe {doc.Vehicle.PlateNumber} đã hết hạn từ {doc.ExpiryDate:dd/MM/yyyy} — xe này sẽ KHÔNG được Chia xe cho tới khi gia hạn."
                : $"{docTypeText} của xe {doc.Vehicle.PlateNumber} sẽ hết hạn vào {doc.ExpiryDate:dd/MM/yyyy} (còn {(doc.ExpiryDate - today).Days} ngày) — nên gia hạn sớm.";

            _db.Notifications.Add(new Notification { UserId = userId, Title = title, Message = message, IsRead = false });
        }
        await _db.SaveChangesAsync();
    }

    /// <summary>Quét tài xế (đang Active, đã có hồ sơ tài xế) mà CHƯA đăng ký
    /// ca làm việc cho NGÀY MAI — cảnh báo sớm 1 ngày để Dispatcher biết cần
    /// nhắc tài xế đăng ký lịch, tránh phát hiện quá muộn lúc Chia xe.</summary>
    private async Task GenerateDriverScheduleAlertsAsync(string userId)
    {
        var tomorrow = DateTime.UtcNow.Date.AddDays(1);

        var activeDrivers = await _db.DriverProfiles
            .Include(dp => dp.Employee)
            .Where(dp => dp.Employee.Status == EmployeeStatus.Active && dp.LicenseExpiryDate > DateTime.UtcNow)
            .ToListAsync();

        var driverIdsScheduledTomorrow = await _db.DriverSchedules
            .Where(s => s.ShiftDate.Date == tomorrow)
            .Select(s => s.DriverEmployeeId)
            .Distinct()
            .ToListAsync();

        foreach (var driver in activeDrivers.Where(d => !driverIdsScheduledTomorrow.Contains(d.EmployeeId)))
        {
            var title = $"📅 Tài xế {driver.Employee.FullName} chưa đăng ký ca làm NGÀY MAI";

            var alreadyExists = await _db.Notifications.AnyAsync(n => n.UserId == userId && n.Title == title && !n.IsRead);
            if (alreadyExists) continue;

            var message = $"Tài xế {driver.Employee.FullName} chưa đăng ký ca làm việc cho ngày {tomorrow:dd/MM/yyyy} — nếu không đăng ký, sẽ KHÔNG được Chia xe cho các đơn cần chạy vào ngày đó.";
            _db.Notifications.Add(new Notification { UserId = userId, Title = title, Message = message, IsRead = false });
        }
        await _db.SaveChangesAsync();
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        var notification = await _db.Notifications.FindAsync(notificationId);
        if (notification is null) return;

        notification.IsRead = true;
        await _db.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        var unread = await _db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
        foreach (var n in unread) n.IsRead = true;
        await _db.SaveChangesAsync();
    }
}
