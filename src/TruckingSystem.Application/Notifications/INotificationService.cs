namespace TruckingSystem.Application.Notifications;

public interface INotificationService
{
    /// <summary>Quét các điều kiện cần cảnh báo (giấy tờ xe sắp/đã hết hạn,
    /// tài xế thiếu lịch làm việc sắp tới) và TỰ ĐỘNG tạo thông báo mới cho
    /// người dùng này nếu CHƯA có thông báo CÙNG NỘI DUNG đang ở trạng thái
    /// chưa đọc — tránh tạo trùng lặp mỗi lần gọi. Sau đó trả về TOÀN BỘ
    /// thông báo (đã đọc + chưa đọc) mới nhất của người dùng.</summary>
    Task<List<NotificationDto>> CheckAndGetNotificationsAsync(string userId);

    Task MarkAsReadAsync(int notificationId);
    Task MarkAllAsReadAsync(string userId);
}
