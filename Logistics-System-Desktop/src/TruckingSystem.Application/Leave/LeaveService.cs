using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Application.Leave;

public partial class LeaveService(IApplicationDbContext db) : ILeaveService
{
    private static LeaveResult Fail(string message) => new(false, message);

    private void Audit(string actor, string action, string entity, int? id, object detail) => db.AuditLogs.Add(new AuditLog
    { UserId = actor, Action = action, EntityName = entity, EntityId = id, Detail = JsonSerializer.Serialize(detail) });

    /// <summary>Ghi trong transaction Serializable (khi là CSDL quan hệ); chỉ lưu và commit khi thành công.</summary>
    private async Task<LeaveResult> WriteAsync(Func<Task<LeaveResult>> action)
    {
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable)
            : await db.Database.BeginTransactionAsync();
        var result = await action();
        if (!result.Success) return result;
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return result;
    }

    /// <summary>Các tháng đã chốt lương trong số các ngày (đã chuẩn hóa .Date) được truyền vào.</summary>
    private async Task<List<(int Year, int Month)>> ApprovedMonthsAsync(IEnumerable<DateTime> days)
    {
        var months = days.Select(d => (d.Year, d.Month)).Distinct().ToList();
        if (months.Count == 0) return new();
        var years = months.Select(m => m.Year).Distinct().ToList();
        var approved = await db.PayrollPeriods.AsNoTracking()
            .Where(p => p.ApprovedAt != null && years.Contains(p.Year))
            .Select(p => new { p.Year, p.Month }).ToListAsync();
        return months.Where(m => approved.Any(a => a.Year == m.Year && a.Month == m.Month))
            .OrderBy(m => m.Year).ThenBy(m => m.Month).ToList();
    }

    private static string LockedMessage(IEnumerable<(int Year, int Month)> months)
        => string.Join(" ", months.Select(m => $"Tháng {m.Month:00}/{m.Year} đã chốt lương."));
}
