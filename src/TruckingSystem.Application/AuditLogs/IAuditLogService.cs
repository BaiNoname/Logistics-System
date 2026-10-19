namespace TruckingSystem.Application.AuditLogs;

public interface IAuditLogService
{
    Task<List<AuditLogDto>> GetAsync(AuditLogFilterRequest filter, bool includePayroll = false);

    /// <summary>Danh sách các giá trị Action ĐÃ TỪNG xuất hiện trong log —
    /// đổ vào dropdown lọc, tránh gõ tay sai chính tả.</summary>
    Task<List<string>> GetDistinctActionsAsync(bool includePayroll = false);
}
