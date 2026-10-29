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
            => GetAsync(action, fromDate, toDate, null, null, null, 0, 300);

        /// <summary>Nhật ký mới nhất trước, lọc phía server; skip/take để tải thêm trang cũ hơn (take tối đa 1000).</summary>
        public Task<List<AuditLogItem>> GetAsync(string? action, DateTime? fromDate, DateTime? toDate,
            string? userName, string? entityName, string? search, int skip, int take)
        {
            var query = new List<string> { $"skip={skip}", $"take={take}" };
            if (!string.IsNullOrEmpty(action)) query.Add($"action={Uri.EscapeDataString(action)}");
            if (fromDate.HasValue) query.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
            if (toDate.HasValue) query.Add($"toDate={toDate.Value:yyyy-MM-dd}");
            if (!string.IsNullOrEmpty(userName)) query.Add($"userName={Uri.EscapeDataString(userName)}");
            if (!string.IsNullOrEmpty(entityName)) query.Add($"entityName={Uri.EscapeDataString(entityName)}");
            if (!string.IsNullOrEmpty(search)) query.Add($"search={Uri.EscapeDataString(search)}");
            return _conn.GetAsync<List<AuditLogItem>>("/api/audit-logs?" + string.Join("&", query))!;
        }

        public Task<List<string>> GetDistinctActionsAsync()
            => _conn.GetAsync<List<string>>("/api/audit-logs/actions")!;

        public Task<AuditLogFilterOptionsItem> GetFilterOptionsAsync()
            => _conn.GetAsync<AuditLogFilterOptionsItem>("/api/audit-logs/filter-options")!;
    }
}
