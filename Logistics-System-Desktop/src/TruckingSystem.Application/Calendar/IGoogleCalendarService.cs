namespace TruckingSystem.Application.Calendar;

/// <summary>1 sự kiện lịch cần tạo/cập nhật trên Google Calendar.</summary>
public record CalendarEventRequest(
    string CalendarId,
    string? ExistingEventId, // có giá trị = CẬP NHẬT sự kiện cũ; null = TẠO MỚI
    string Title,
    string Description,
    DateTime StartUtc,     // AllDay = true: NGÀY bắt đầu (lấy phần ngày)
    DateTime EndUtc,       // AllDay = true: NGÀY kết thúc (KHÔNG tính — ngày sau ngày cuối)
    string? Location,
    bool AllDay = false,
    string? ColorId = null // màu sự kiện chuẩn Google: 8 xám, 10 xanh lá, 5 vàng, 11 đỏ
);

public record CalendarEventResult(bool Success, string? EventId, string? Message);

/// <summary>Gọi Google Calendar API thật (qua Service Account, không qua OAuth
/// đăng nhập trình duyệt) để tạo/cập nhật sự kiện trên Calendar của tài xế
/// hoặc khách hàng — Calendar phải ĐÃ ĐƯỢC CHIA SẺ quyền chỉnh sửa cho đúng
/// email Service Account từ trước (làm thủ công 1 lần trên Google Calendar).</summary>
public interface IGoogleCalendarService
{
    Task<CalendarEventResult> CreateOrUpdateEventAsync(CalendarEventRequest request);

    /// <summary>Xóa 1 sự kiện đã tạo (VD: khi Trip bị hủy) — không chặn nghiệp
    /// vụ nếu xóa lỗi (Calendar không phải nguồn dữ liệu chính).</summary>
    Task<bool> DeleteEventAsync(string calendarId, string eventId);
}
