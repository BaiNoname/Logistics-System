namespace TruckingSystem.Desktop.DTO_AuditLogManagement
{
    public class AuditLogItem
    {
        public int Id { get; set; }
        public string Action { get; set; } = "";
        public string EntityName { get; set; } = "";
        public int? EntityId { get; set; }
        public string? Detail { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
