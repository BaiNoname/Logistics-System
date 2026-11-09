namespace TruckingSystem.Application.Notifications;

public interface INotificationService
{
    /// <summary>Quét các điều kiện cần cảnh báo (giấy tờ xe sắp/đã hết hạn,
    /// tài xế thiếu lịch làm việc sắp tới) và TỰ ĐỘNG tạo thông báo mới cho
    /// người dùng này nếu CHƯA có thông báo CÙNG NỘI DUNG đang ở trạng thái
    /// chưa đọc — tránh tạo trùng lặp mỗi lần gọi. Trả về cả thông báo RIÊNG
    /// của người dùng này LẪN thông báo CHUNG (broadcast, xem CreateBroadcastAsync).</summary>
    /// <param name="roles">Vai trò người xem — chỉ trả thông báo LIÊN QUAN (NotificationService.CanSee).</param>
    /// <param name="refresh">true = quét lại ngay (mở màn Thông báo); false = tối đa 1 lần / phút (chấm đỏ).</param>
    Task<List<NotificationDto>> CheckAndGetNotificationsAsync(string userId, IEnumerable<string> roles, bool refresh = false);

    /// <summary>Tạo 1 thông báo CHUNG — KHÔNG gắn riêng 1 user cụ thể nào,
    /// AI đăng nhập mở màn Thông báo cũng thấy được (đơn giản, không cần biết
    /// chính xác ai đang giữ vai trò Dispatcher). Dùng cho các sự kiện cần cả
    /// nhóm vận hành biết, ví dụ: tài xế báo xong chuyến, cần duyệt.</summary>
    Task CreateBroadcastAsync(string title, string message);

    /// <summary>Chuyến đã được xử lý (Duyệt / Từ chối / Hoàn tất) -> tự đánh dấu
    /// đã đọc thông báo "chờ duyệt" của chuyến đó.</summary>
    Task ResolveTripPendingAsync(int tripId);

    /// <summary>Đánh dấu "Đã xử lý" bằng tay (chỉ thông báo loại "Khác"). Trả về null nếu
    /// thành công, hoặc thông điệp lỗi.</summary>
    Task<string?> MarkAsReadAsync(int notificationId, string userId);
    Task MarkAllAsReadAsync(string userId);
}