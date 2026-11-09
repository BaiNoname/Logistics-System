using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

public class Notification : BaseEntity
{
    // Nullable — null nghĩa là thông báo CHUNG, không gắn riêng 1 người dùng
    // nào, AI đăng nhập mở màn Thông báo cũng thấy được (xem
    // NotificationService.CreateBroadcastAsync). Trước đây bắt buộc có giá
    // trị (string.Empty mặc định), chỉ dùng được cho thông báo RIÊNG từng
    // người (giấy tờ xe hết hạn, thiếu lịch làm việc...).
    public string? UserId { get; set; } // FK -> AspNetUsers.Id, NULL = thông báo chung
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
}