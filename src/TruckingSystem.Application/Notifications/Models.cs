namespace TruckingSystem.Application.Notifications;

/// <summary>Category: "Chuyến chờ duyệt" / "Giấy tờ xe" / "Giấy tờ tài xế" / "Hạn giao" / "Lịch tài xế" / "Khác" — để Desktop lọc.</summary>
public record NotificationDto(int Id, string Title, string Message, bool IsRead, DateTime CreatedAt, string Category = "Khác");
