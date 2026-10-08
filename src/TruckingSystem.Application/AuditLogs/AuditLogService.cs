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
        // Lọc theo NGÀY Việt Nam (UTC+7), Timestamp lưu UTC. "Đến ngày" tính TRỌN
        // ngày đó — trước đây so <= 00:00 nên mất toàn bộ log của ngày cuối.
        if (filter.FromDate.HasValue)
        {
            var fromUtc = filter.FromDate.Value.Date.AddHours(-7);
            query = query.Where(a => a.Timestamp >= fromUtc);
        }
        if (filter.ToDate.HasValue)
        {
            var toUtcExclusive = filter.ToDate.Value.Date.AddDays(1).AddHours(-7);
            query = query.Where(a => a.Timestamp < toUtcExclusive);
        }

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
