using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;

namespace TruckingSystem.Application.AuditLogs;

public class AuditLogService : IAuditLogService
{
    private readonly IApplicationDbContext _db;

    public AuditLogService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<AuditLogDto>> GetAsync(AuditLogFilterRequest filter)
    {
        var query = _db.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Action))
            query = query.Where(a => a.Action == filter.Action);
        if (filter.FromDate.HasValue)
            query = query.Where(a => a.Timestamp >= filter.FromDate.Value);
        if (filter.ToDate.HasValue)
            query = query.Where(a => a.Timestamp <= filter.ToDate.Value);

        return await query
            .OrderByDescending(a => a.Timestamp)
            .Take(200) // giới hạn 200 dòng gần nhất, tránh tải cả bảng khổng lồ về 1 lúc
            .Select(a => new AuditLogDto(a.Id, a.Action, a.EntityName, a.EntityId, a.Detail, a.Timestamp))
            .ToListAsync();
    }

    public async Task<List<string>> GetDistinctActionsAsync()
    {
        return await _db.AuditLogs.Select(a => a.Action).Distinct().OrderBy(a => a).ToListAsync();
    }
}
