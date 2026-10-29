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

    public async Task<List<AuditLogDto>?> GetHistoryAsync(string entityName, int entityId)
    {
        var related = new List<(string Name, List<int> Ids)>();
        switch (entityName)
        {
            case "TransportOrder":
                related.Add(("TransportOrder", new() { entityId }));
                related.Add(("Trip", await _db.Trips.Where(t => t.TransportOrderId == entityId).Select(t => t.Id).ToListAsync()));
                related.Add(("CargoLine", await _db.CargoLines.Where(x => x.TransportOrderId == entityId).Select(x => x.Id).ToListAsync()));
                related.Add(("DeliveryDeadlineChange", await _db.DeliveryDeadlineChanges.Where(x => x.TransportOrderId == entityId).Select(x => x.Id).ToListAsync()));
                related.Add(("GoodsReceipt", await _db.GoodsReceipts.Where(x => x.TransportOrderId == entityId).Select(x => x.Id).ToListAsync()));
                related.Add(("GoodsIssue", await _db.GoodsIssues.Where(x => x.TransportOrderId == entityId).Select(x => x.Id).ToListAsync()));
                break;
            case "Trip":
                related.Add(("Trip", new() { entityId }));
                related.Add(("FuelLog", await _db.FuelLogs.Where(x => x.TripId == entityId).Select(x => x.Id).ToListAsync()));
                related.Add(("TripExpense", await _db.TripExpenses.Where(x => x.TripId == entityId).Select(x => x.Id).ToListAsync()));
                related.Add(("IncidentReport", await _db.IncidentReports.Where(x => x.TripId == entityId).Select(x => x.Id).ToListAsync()));
                related.Add(("TripLine", await _db.TripLines.Where(x => x.TripId == entityId).Select(x => x.Id).ToListAsync()));
                related.Add(("GoodsReceipt", await _db.GoodsReceipts.Where(x => x.TripId == entityId).Select(x => x.Id).ToListAsync()));
                related.Add(("GoodsIssue", await _db.GoodsIssues.Where(x => x.TripId == entityId).Select(x => x.Id).ToListAsync()));
                break;
            case "Vehicle":
                related.Add(("Vehicle", new() { entityId }));
                related.Add(("VehicleDocument", await _db.VehicleDocuments.Where(x => x.VehicleId == entityId).Select(x => x.Id).ToListAsync()));
                break;
            default:
                return null;
        }

        var result = new List<AuditLogDto>();
        foreach (var (name, ids) in related.Where(r => r.Ids.Count > 0))
        {
            var idsN = ids.Select(i => (int?)i).ToList();
            result.AddRange(await _db.AuditLogs
                .Where(a => a.EntityName == name && idsN.Contains(a.EntityId))
                .OrderByDescending(a => a.Timestamp).Take(500)
                .Select(a => new AuditLogDto(a.Id, a.Action, a.EntityName, a.EntityId, a.Detail, a.Timestamp, a.UserName, a.UserRole))
                .ToListAsync());
        }
        return result.OrderByDescending(a => a.Timestamp).ThenByDescending(a => a.Id).Take(1000).ToList();
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
