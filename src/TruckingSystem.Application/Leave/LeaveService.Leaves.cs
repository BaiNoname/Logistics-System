using Microsoft.EntityFrameworkCore;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;
using TruckingSystem.Domain.Leave;

namespace TruckingSystem.Application.Leave;

public partial class LeaveService
{
    private static DateTime DateOnly(DateTime d) => DateTime.SpecifyKind(d.Date, DateTimeKind.Unspecified);

    private static List<LeaveType> QuotaTypes() =>
        Enum.GetValues<LeaveType>().Where(LeaveRules.DeductsAnnualQuota).ToList();

    public Task<List<LeaveTypeDto>> GetLeaveTypesAsync() => Task.FromResult(
        Enum.GetValues<LeaveType>().Select(t => new LeaveTypeDto(t.ToString(), LeaveRules.DisplayName(t),
            LeaveRules.Group(t).ToString(), LeaveRules.IsPaid(t), LeaveRules.DeductsAnnualQuota(t),
            LeaveRules.LimitText(t))).ToList());

    public async Task<List<LeaveDto>> GetLeavesAsync(int employeeId, int year, int month)
    {
        if (year < 2000 || year > 2100 || month < 1 || month > 12) return new();
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1);
        var rows = await db.LeaveRecords.AsNoTracking()
            .Where(l => l.EmployeeId == employeeId && l.LeaveDate >= start && l.LeaveDate < end)
            .OrderBy(l => l.LeaveDate).ToListAsync();
        var locked = (await ApprovedMonthsAsync(new[] { start })).Count > 0;
        return rows.Select(l => new LeaveDto(l.Id, l.EmployeeId, l.LeaveDate.Date, l.Type.ToString(),
            LeaveRules.DisplayName(l.Type), LeaveRules.IsPaid(l.Type), l.BatchId, l.Note, l.CreatedBy, locked)).ToList();
    }

    public async Task<LeaveQuotaDto> GetQuotaAsync(int employeeId, int year)
    {
        var used = await UsedQuotaAsync(employeeId, new[] { year });
        var u = used.GetValueOrDefault(year);
        return new(year, u, LeaveRules.AnnualQuotaDays, Math.Max(0, LeaveRules.AnnualQuotaDays - u));
    }

    private async Task<Dictionary<int, int>> UsedQuotaAsync(int employeeId, IEnumerable<int> years)
    {
        var ys = years.Distinct().ToList();
        var minYear = ys.Min();
        var maxYear = ys.Max();
        var types = QuotaTypes();
        var dates = await db.LeaveRecords.AsNoTracking()
            .Where(l => l.EmployeeId == employeeId && types.Contains(l.Type)
                && l.LeaveDate.Year >= minYear && l.LeaveDate.Year <= maxYear)
            .Select(l => l.LeaveDate).ToListAsync();
        return dates.GroupBy(d => d.Year).Where(g => ys.Contains(g.Key)).ToDictionary(g => g.Key, g => g.Count());
    }

    public Task<LeaveResult> AddLeaveAsync(AddLeaveRequest request, string actor) => WriteAsync(async () =>
    {
        if (!Enum.TryParse<LeaveType>(request.Type, out var type) || !Enum.IsDefined(type))
            return Fail("Loại nghỉ không hợp lệ.");
        var employee = await db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == request.EmployeeId);
        if (employee is null) return Fail("Không tìm thấy nhân viên.");
        var from = DateOnly(request.From);
        var to = DateOnly(request.To);
        var note = (request.Note ?? "").Trim();
        if (note.Length > 500) return Fail("Ghi chú tối đa 500 ký tự.");

        // Khoảng sai hoặc quá dài: để ValidateRequest trả lý do trước khi duyệt từng ngày.
        if (to < from || (to - from).TotalDays + 1 > LeaveRules.MaxRangeDays)
            return Fail(LeaveRules.ValidateRequest(type, from, to, 0, employee.Gender)!);

        var holidayList = await db.PublicHolidays.AsNoTracking()
            .Where(h => h.Date >= from && h.Date <= to).Select(h => h.Date).ToListAsync();
        var holidays = holidayList.Select(d => d.Date).ToHashSet();
        var (expanded, skipped) = LeaveRules.ExpandWorkingDays(from, to, holidays);
        var days = expanded.Select(DateOnly).ToList();

        var error = LeaveRules.ValidateRequest(type, from, to, days.Count, employee.Gender);
        if (error is not null) return Fail(error);

        var hire = employee.HireDate.Date;
        if (days.Any(d => d < hire)) return Fail($"Nhân viên vào làm ngày {hire:dd/MM/yyyy}.");
        if (employee.TerminationDate is { } term && days.Any(d => d > term.Date))
            return Fail($"Nhân viên đã nghỉ việc từ {term.Date:dd/MM/yyyy}.");

        var existing = await db.LeaveRecords.AsNoTracking()
            .Where(l => l.EmployeeId == employee.Id && l.LeaveDate >= from && l.LeaveDate <= to)
            .Select(l => l.LeaveDate).ToListAsync();
        var existingDates = existing.Select(e => e.Date).ToHashSet();
        var dup = days.Where(existingDates.Contains).ToList();
        if (dup.Count > 0)
            return Fail($"Đã có ngày nghỉ vào: {string.Join(", ", dup.Select(d => d.ToString("dd/MM/yyyy")))}.");

        if (LeaveRules.DeductsAnnualQuota(type))
        {
            var used = await UsedQuotaAsync(employee.Id, days.Select(d => d.Year));
            var quotaErrors = LeaveRules.QuotaErrors(used, days);
            if (quotaErrors.Count > 0) return Fail(string.Join(" ", quotaErrors));
        }

        var locked = await ApprovedMonthsAsync(days);
        if (locked.Count > 0) return Fail(LockedMessage(locked));

        var warnings = skipped.Select(d => holidays.Contains(d)
            ? $"Ngày {d:dd/MM} là ngày lễ nên được bỏ qua."
            : $"Ngày {d:dd/MM} là Chủ nhật nên được bỏ qua.").ToList();
        var first = days.Min();
        var last = days.Max();
        var attended = await db.AttendanceRecords.AsNoTracking()
            .Where(a => a.EmployeeId == employee.Id && a.WorkDate >= first && a.WorkDate <= last)
            .Select(a => a.WorkDate).ToListAsync();
        var attendedDates = attended.Select(a => a.Date).ToHashSet();
        warnings.AddRange(days.Where(attendedDates.Contains).Select(d =>
            $"Ngày {d:dd/MM} đã có chấm công — bảng lương sẽ tính theo chấm công."));

        var batchId = Guid.NewGuid();
        foreach (var d in days)
            db.LeaveRecords.Add(new LeaveRecord
            { EmployeeId = employee.Id, LeaveDate = d, Type = type, BatchId = batchId, Note = note, CreatedBy = actor });
        Audit(actor, "AddLeave", "LeaveRecord", null,
            new { EmployeeId = employee.Id, Type = type.ToString(), Days = days, BatchId = batchId });
        return new(true, $"Đã thêm {days.Count} ngày nghỉ.", warnings.Count > 0 ? warnings : null);
    });

    public Task<LeaveResult> DeleteLeaveAsync(int id, string actor) => WriteAsync(async () =>
    {
        var record = await db.LeaveRecords.FindAsync(id);
        if (record is null) return Fail("Không tìm thấy ngày nghỉ.");
        return await RemoveAsync(new List<LeaveRecord> { record }, actor,
            new { record.Id, record.EmployeeId, record.LeaveDate, record.Type });
    });

    public Task<LeaveResult> DeleteLeaveBatchAsync(Guid batchId, string actor) => WriteAsync(async () =>
    {
        var records = await db.LeaveRecords.Where(l => l.BatchId == batchId).ToListAsync();
        if (records.Count == 0) return Fail("Không tìm thấy đợt nghỉ.");
        return await RemoveAsync(records, actor,
            new { BatchId = batchId, Days = records.Select(r => r.LeaveDate).ToList() });
    });

    private async Task<LeaveResult> RemoveAsync(List<LeaveRecord> records, string actor, object detail)
    {
        var locked = await ApprovedMonthsAsync(records.Select(r => r.LeaveDate.Date));
        if (locked.Count > 0) return Fail(LockedMessage(locked));
        db.LeaveRecords.RemoveRange(records);
        Audit(actor, "DeleteLeave", "LeaveRecord", records.Count == 1 ? records[0].Id : null, detail);
        return new(true, $"Đã xóa {records.Count} ngày nghỉ.");
    }
}
