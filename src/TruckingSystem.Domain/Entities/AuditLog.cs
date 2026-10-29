using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string? UserId { get; set; } // FK -> AspNetUsers.Id, nullable (hệ thống tự động cũng có thể ghi log)

    /// <summary>Tên đăng nhập của người thao tác lúc ghi log (giữ lại cả khi tài khoản sau này bị đổi tên / xóa);
    /// null = hệ thống tự chạy (khởi tạo dữ liệu, tác vụ nền).</summary>
    public string? UserName { get; set; }

    /// <summary>Vai trò của người thao tác LÚC ghi log (mã role, nhiều role nối dấu phẩy: "Admin", "Dispatcher,Accountant")
    /// — để lọc nhật ký theo vai trò thay vì theo từng người; giữ nguyên dù sau này tài khoản bị đổi vai trò.</summary>
    public string? UserRole { get; set; }
    public string Action { get; set; } = string.Empty; // "Approve", "Delete", "AutoComplete"...
    public string EntityName { get; set; } = string.Empty; // "TransportOrder"
    public int? EntityId { get; set; }
    public string? Detail { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
