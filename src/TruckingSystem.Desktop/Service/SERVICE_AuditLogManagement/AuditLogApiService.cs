using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_AuditLogManagement;

namespace TruckingSystem.Desktop.SERVICE_AuditLogManagement
{
    public class AuditLogApiService
    {
        private readonly ApiConnection _conn;

        public AuditLogApiService(ApiConnection conn)
        {
            _conn = conn;
        }

        public Task<List<AuditLogItem>> GetAsync(string? action, DateTime? fromDate, DateTime? toDate)
        {
            var query = new List<string>();
            if (!string.IsNullOrEmpty(action)) query.Add($"action={Uri.EscapeDataString(action)}");
            if (fromDate.HasValue) query.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
            if (toDate.HasValue) query.Add($"toDate={toDate.Value:yyyy-MM-dd}");
            var qs = query.Count > 0 ? "?" + string.Join("&", query) : "";

            return _conn.GetAsync<List<AuditLogItem>>($"/api/audit-logs{qs}")!;
        }

        public Task<List<string>> GetDistinctActionsAsync()
            => _conn.GetAsync<List<string>>("/api/audit-logs/actions")!;
    }
}
