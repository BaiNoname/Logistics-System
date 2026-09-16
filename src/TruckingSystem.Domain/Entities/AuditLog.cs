using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string? UserId { get; set; } // FK -> AspNetUsers.Id, nullable (hệ thống tự động cũng có thể ghi log)
    public string Action { get; set; } = string.Empty; // "Approve", "Delete", "AutoComplete"...
    public string EntityName { get; set; } = string.Empty; // "TransportOrder"
    public int? EntityId { get; set; }
    public string? Detail { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
