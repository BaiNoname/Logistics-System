using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

/// <summary>
/// Ghi nhận 1 Trip đã được đồng bộ thành sự kiện (Event) trong 1 Google
/// Calendar cụ thể. Thiết kế TRUNG LẬP — không quan tâm CalendarId đó là của
/// Tài xế hay Khách hàng (cả 2 đều nhập tay, ghi qua 1 Service Account chung)
/// — cả 2 trường hợp đều dùng chung 1 bảng này để tra cứu lại khi cần CẬP
/// NHẬT/XÓA sự kiện (tránh tạo trùng lặp mỗi lần đồng bộ lại).
///
/// QUAN TRỌNG khi code GoogleCalendarSyncService (chưa code, chờ có Service
/// Account credentials): phần MÔ TẢ (description) của sự kiện tạo trên Google
/// Calendar PHẢI nhúng kèm link tới trang tracking công khai đã có sẵn:
///   https://{domain}/track.html?token={TransportOrder.PublicTrackingToken}
/// — dùng ĐÚNG 1 link này cho CẢ sự kiện của Khách hàng (để họ theo dõi tiến
/// trình) LẪN sự kiện của Tài xế (để họ tự kiểm tra lại vị trí GPS/bản đồ đã
/// ghi nhận đúng chưa) — không cần làm 2 trang riêng, tái dùng trang đã có.
/// </summary>
public class TripCalendarEvent : BaseEntity
{
    public int TripId { get; set; }
    public Trip Trip { get; set; } = null!;

    public string CalendarId { get; set; } = string.Empty; // Calendar ID Google thật đã ghi vào
    public string GoogleEventId { get; set; } = string.Empty;
    public DateTime SyncedAt { get; set; } = DateTime.UtcNow;
}
