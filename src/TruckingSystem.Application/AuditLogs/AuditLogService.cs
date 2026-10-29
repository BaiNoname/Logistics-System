using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;

namespace TruckingSystem.Application.AuditLogs;

/// <summary>Đọc NHẬT KÝ HỆ THỐNG. Dòng nhật ký do bộ ghi tự động (AuditSaveChangesInterceptor — mọi thêm / sửa / xóa)
/// và các thao tác nghiệp vụ ghi tay (Chia xe, Đổi hạn giao, Đăng nhập...) tạo ra.</summary>
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
        if (!string.IsNullOrWhiteSpace(filter.EntityName))
            query = query.Where(a => a.EntityName == filter.EntityName);
        if (!string.IsNullOrWhiteSpace(filter.UserName))
            query = filter.UserName == SystemUser
                ? query.Where(a => a.UserName == null)
                : query.Where(a => a.UserName == filter.UserName);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = "%" + filter.Search.Trim() + "%";
            query = query.Where(a => (a.Detail != null && EF.Functions.ILike(a.Detail, pattern))
                                     || EF.Functions.ILike(a.EntityName, pattern)
                                     || (a.UserName != null && EF.Functions.ILike(a.UserName, pattern)));
        }
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

        var take = Math.Clamp(filter.Take, 1, 1000);
        return await query
            .OrderByDescending(a => a.Timestamp).ThenByDescending(a => a.Id)
            .Skip(Math.Max(0, filter.Skip))
            .Take(take)
            .Select(a => new AuditLogDto(a.Id, a.Action, a.EntityName, a.EntityId, a.Detail, a.Timestamp, a.UserName, a.UserRole))
            .ToListAsync();
    }

    /// <summary>Giá trị lọc "người thao tác" cho các dòng do HỆ THỐNG tự ghi (không có người đăng nhập).</summary>
    public const string SystemUser = "(Hệ thống)";

    public async Task<List<string>> GetDistinctActionsAsync()
    {
        return await _db.AuditLogs.Select(a => a.Action).Distinct().OrderBy(a => a).ToListAsync();
    }

    public async Task<AuditLogFilterOptionsDto> GetFilterOptionsAsync()
    {
        var actions = await GetDistinctActionsAsync();
        var users = await _db.AuditLogs.Where(a => a.UserName != null).Select(a => a.UserName!).Distinct().OrderBy(u => u).ToListAsync();
        if (await _db.AuditLogs.AnyAsync(a => a.UserName == null)) users.Add(SystemUser);
        var entities = await _db.AuditLogs.Select(a => a.EntityName).Distinct().OrderBy(e => e).ToListAsync();
        return new AuditLogFilterOptionsDto(actions, users, entities);
    }
}
