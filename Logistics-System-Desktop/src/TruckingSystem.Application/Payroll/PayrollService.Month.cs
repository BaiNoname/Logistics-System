using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TruckingSystem.Application.Common;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;
using TruckingSystem.Domain.Leave;
using TruckingSystem.Domain.Payroll;

namespace TruckingSystem.Application.Payroll;

public partial class PayrollService
{
    public Task<PayrollResult> GenerateMonthAsync(GenerateMonthRequest request, string actor) => WriteAsync(async () =>
    {
        if (!ValidPeriod(request.Year, request.Month)) return Fail("Tháng không hợp lệ.");
        var period = await db.PayrollPeriods.SingleOrDefaultAsync(p => p.Year == request.Year && p.Month == request.Month);
        if (period?.ApprovedAt is not null) return Fail("Bảng lương đã chốt, không tính lại.");
        if ((period?.Revision ?? 0) != request.Revision) return Fail("Bảng lương đã thay đổi; chọn lại tháng để tải dữ liệu mới.");
        period ??= new PayrollPeriod { Year = request.Year, Month = request.Month, CreatedBy = actor };
        var result = await SaveMonthDraftAsync(period, actor, automatic: false, CancellationToken.None);
        if (result.Success) period.AutomaticCalculationError = null;
        return result;
    });

    public async Task<List<(int Year, int Month)>> GetOpenMonthsBeforeAsync(int year, int month, CancellationToken cancellationToken)
    {
        var months = await db.PayrollPeriods.AsNoTracking()
            .Where(p => p.ApprovedAt == null && (p.Year < year || p.Year == year && p.Month < month))
            .OrderByDescending(p => p.Year).ThenByDescending(p => p.Month)
            .Select(p => new { p.Year, p.Month }).ToListAsync(cancellationToken);
        return months.Select(p => (p.Year, p.Month)).ToList();
    }

    public Task RunAutomaticCalculationAsync(int year, int month, DateTime scheduledAtUtc, CancellationToken cancellationToken)
        => WriteAsync(async () =>
    {
        var period = await db.PayrollPeriods.SingleOrDefaultAsync(p => p.Year == year && p.Month == month, cancellationToken);
        if (period?.ApprovedAt is not null || period?.LastAutomaticCalculationAt >= scheduledAtUtc)
            return new(true);

        const string actor = "Hệ thống";
        var startedAt = DateTime.UtcNow;
        period ??= new PayrollPeriod { Year = year, Month = month, CreatedBy = actor };
        var result = await SaveMonthDraftAsync(period, actor, automatic: true, cancellationToken);
        if (period.Id == 0) db.PayrollPeriods.Add(period);
        period.LastAutomaticCalculationAt = startedAt;
        period.AutomaticCalculationError = result.Success ? null : result.Message;
        return new(true);
    }, cancellationToken);

    private async Task<PayrollResult> SaveMonthDraftAsync(PayrollPeriod period, string actor, bool automatic,
        CancellationToken cancellationToken)
    {
        var old = period.Id == 0 ? new List<PayrollSlip>() : await db.PayrollSlips.Include(s => s.Items).Include(s => s.Payments)
            .Where(s => s.PayrollPeriodId == period.Id).ToListAsync(cancellationToken);
        if (old.Any(s => s.Payments.Count != 0)) return Fail("Phiếu đã có chi trả, không thay thế.");
        if (old.Any(s => s.CalculationVersion != 2)) return Fail("Tháng còn phiếu nháp theo cơ chế cũ. Xuất đối chiếu, chuyển khoản cộng/trừ sang danh sách tháng rồi xóa nháp cũ để hệ thống tự tính lại.");
        var calculated = await CalculateMonthAsync(period, actor, cancellationToken);
        if (calculated.Errors.Count > 0) return Fail(string.Join(Environment.NewLine, calculated.Errors));
        if (automatic && old.Count == calculated.Slips.Count)
        {
            var existing = old.ToDictionary(s => s.EmployeeId);
            if (calculated.Slips.All(s => existing.TryGetValue(s.EmployeeId, out var draft)
                && draft.SourceSnapshot == s.SourceSnapshot && draft.EarnedSalary == s.EarnedSalary && draft.NetPay == s.NetPay))
                return new(true);
        }

        if (period.Id == 0) db.PayrollPeriods.Add(period);
        foreach (var slip in old) { db.PayrollItems.RemoveRange(slip.Items); db.PayrollSlips.Remove(slip); }
        // Delete old unique (period, employee) rows before inserting replacements; one transaction keeps the month atomic.
        await db.SaveChangesAsync(cancellationToken);
        foreach (var slip in calculated.Slips) { slip.PayrollPeriod = period; db.PayrollSlips.Add(slip); }
        Touch(period);
        Audit(automatic ? null : actor, "GenerateMonth", "PayrollPeriod", period.Id,
            new { period.Year, period.Month, Count = calculated.Slips.Count });
        return new(true, $"Đã tạo/tính lại {calculated.Slips.Count} phiếu lương. Chưa chốt.");
    }

    private async Task<(List<PayrollSlip> Slips, List<string> Errors)> CalculateMonthAsync(PayrollPeriod period, string actor,
        CancellationToken cancellationToken = default)
    {
        var slips = new List<PayrollSlip>();
        var errors = new List<string>();
        var employees = await GetEmployeesAsync(period.Year, period.Month);
        if (employees.Count == 0) errors.Add("Không có nhân viên thuộc tháng này.");
        var (from, to) = PayrollRules.PeriodRange(period.Year, period.Month);
        var policy = await db.PayrollKpiPolicies.AsNoTracking().Where(p => p.EffectiveFrom <= from)
            .OrderByDescending(p => p.EffectiveFrom).FirstOrDefaultAsync(cancellationToken);
        var adjustments = await db.PayrollMonthlyAdjustments.AsNoTracking().Where(a => a.Year == period.Year && a.Month == period.Month)
            .OrderBy(a => a.Id).ToListAsync(cancellationToken);
        foreach (var adjustment in adjustments.Where(a => !employees.Any(e => e.Id == a.EmployeeId)))
            errors.Add($"Khoản #{adjustment.Id}: nhân viên #{adjustment.EmployeeId} không thuộc tháng.");
        foreach (var row in employees)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var employee = (await EmployeeAsync(row.Id))!;
            if (employee.Status == EmployeeStatus.Terminated && !employee.TerminationDate.HasValue)
            { errors.Add($"{row.Code}: thiếu ngày nghỉ việc."); continue; }
            var salary = await SalaryForAsync(employee, period);
            if (salary is null) { errors.Add($"{row.Code}: chưa có LCB hợp lệ cho tháng (không hỗ trợ đổi LCB giữa tháng)."); continue; }
            var slip = new PayrollSlip { CalculationVersion = 2, EmployeeId = row.Id, EmployeeCode = row.Code, EmployeeName = row.FullName,
                DepartmentName = row.DepartmentName, PositionName = row.PositionName, MonthlySalary = salary.MonthlySalary,
                SalaryEffectiveFrom = salary.EffectiveFrom, WorkFrom = from, WorkTo = to,
                StandardDays = PayrollRules.StandardDays(period.Year, period.Month), AttendanceConfirmedBy = actor, AttendanceConfirmedAt = DateTime.UtcNow };
            object source;
            if (row.IsDriver)
            {
                if (policy is null) { errors.Add($"{row.Code}: chưa có cấu hình K, điểm chuẩn và H có hiệu lực."); continue; }
                var factors = JsonSerializer.Deserialize<List<KpiFactorDto>>(policy.FactorsJson)!;
                var startUtc = VnClock.VnToUtc(from); var endUtc = VnClock.VnToUtc(to.AddDays(1));
                var trips = await db.Trips.AsNoTracking().Include(t => t.Vehicle)
                    .Where(t => t.DriverEmployeeId == row.Id && t.Status == TripStatus.Completed && t.StartTime != null
                        && t.EndTime >= startUtc && t.EndTime < endUtc).OrderBy(t => t.Id).ToListAsync();
                var invalid = false;
                foreach (var trip in trips)
                {
                    var factor = factors.SingleOrDefault(f => f.VehicleType == trip.Vehicle.VehicleType);
                    if (factor is null || trip.EstimatedDistanceKm is not (> 0 and <= 1_000_000))
                    { errors.Add($"{row.Code}, chuyến #{trip.Id}: thiếu H loại xe hoặc km tuyến hợp lệ."); invalid = true; continue; }
                    slip.KpiPoints += PayrollRules.TripPoints(trip.EstimatedDistanceKm.Value, factor.Factor, policy.KmPerPoint);
                }
                if (invalid) continue;
                slip.KpiPoints = decimal.Round(slip.KpiPoints, 8, MidpointRounding.AwayFromZero);
                if (slip.KpiPoints / policy.TargetPoints > PayrollRules.MaxAmount / salary.MonthlySalary)
                { errors.Add($"{row.Code}: lương theo KPI vượt giới hạn."); continue; }
                slip.KpiTarget = policy.TargetPoints;
                slip.EarnedSalary = PayrollRules.DriverSalary(salary.MonthlySalary, slip.KpiPoints, policy.TargetPoints);
                slip.DriverWorkDates = ""; // Existing DTO uses non-null to identify a driver; new slips never read registered work dates.
                slip.AttendanceNote = FormattableString.Invariant($"LCB × (P / điểm chuẩn); N={trips.Count}; D={trips.Sum(t => t.EstimatedDistanceKm ?? 0)} km; K={policy.KmPerPoint}; P={slip.KpiPoints:0.######}; điểm chuẩn={policy.TargetPoints}; KPI={slip.KpiPoints / policy.TargetPoints:P2}. Không chia theo ngày làm.");
                slip.AttendanceNote += Environment.NewLine + string.Join(Environment.NewLine, trips.Select(t =>
                    FormattableString.Invariant($"Chuyến #{t.Id}: {t.Vehicle.VehicleType}, {t.EstimatedDistanceKm} km, H={factors.Single(f => f.VehicleType == t.Vehicle.VehicleType).Factor}")));
                source = new { Formula = "(km / K) * H", policy.EffectiveFrom, policy.KmPerPoint, policy.TargetPoints, Factors = factors,
                    Trips = trips.Select(t => new { t.Id, t.Vehicle.VehicleType, t.EstimatedDistanceKm, t.StartTime, t.EndTime }).ToList() };
            }
            else
            {
                var attendance = await EmployeePayrollAttendanceAsync(employee, from, to);
                if (attendance.Errors.Count > 0) { errors.AddRange(attendance.Errors.Select(e => $"{row.Code}: {e}")); continue; }
                var d = attendance.Days;
                slip.WorkedDays = d.WorkedDays; slip.HolidayDays = d.HolidayDays;
                slip.PaidLeaveDays = d.PaidLeaveDays; slip.UnpaidLeaveDays = d.UnpaidLeaveDays;
                slip.EarnedSalary = PayrollRules.Prorate(salary.MonthlySalary, slip.StandardDays, d.WorkedDays + d.PaidLeaveDays + d.HolidayDays);
                slip.AttendanceNote = "Lương theo công hưởng lương từ module chấm công; sử dụng điều chỉnh đã duyệt.";
                if (d.HolidayDays + d.PaidLeaveDays + d.UnpaidLeaveDays > 0)
                {
                    var parts = new List<string> { $"Lễ {d.HolidayDays}" };
                    parts.AddRange(d.LeaveCounts.Select(k => $"{LeaveRules.DisplayName(k.Key)} {k.Value}"));
                    slip.AttendanceNote += Environment.NewLine + string.Join("; ", parts);
                }
                source = attendance.Snapshot;
            }
            var entries = adjustments.Where(a => a.EmployeeId == row.Id).ToList();
            slip.Items = entries.Select(a => new PayrollItem { Kind = a.Kind, Amount = a.Amount, CalculatedAmount = a.Amount,
                Reason = a.Reason, CreatedBy = a.CreatedBy }).ToList();
            Recalculate(slip);
            if (slip.NetPay < 0 || slip.GrossIncome > PayrollRules.MaxAmount) { errors.Add($"{row.Code}: thực lĩnh âm hoặc thu nhập vượt giới hạn."); continue; }
            slip.SourceSnapshot = JsonSerializer.Serialize(new { row.Id, row.Code, row.FullName, row.IsDriver, row.DepartmentName, row.PositionName,
                employee.HireDate, employee.TerminationDate, salary.EffectiveFrom, salary.MonthlySalary, Source = source,
                Adjustments = entries.Select(a => new { a.Id, a.Kind, a.Amount, a.Reason }).ToList() });
            slips.Add(slip);
        }
        return (slips, errors);
    }
    private async Task<(PayrollDayBreakdown Days, object Snapshot, List<string> Errors)> EmployeePayrollAttendanceAsync(Employee employee, DateTime from, DateTime to)
    {
        if (employee.HireDate.Date > from) from = employee.HireDate.Date;
        if (employee.TerminationDate?.Date < to) to = employee.TerminationDate.Value.Date;
        var rows = await db.AttendanceRecords.AsNoTracking().Where(a => a.EmployeeId == employee.Id && a.WorkDate >= from && a.WorkDate <= to)
            .OrderBy(a => a.WorkDate).ToListAsync();
        var errors = new List<string>();
        if (await db.AttendanceAdjustmentRequests.AnyAsync(a => a.EmployeeId == employee.Id && a.WorkDate >= from && a.WorkDate <= to
            && a.Status == AttendanceAdjustmentStatus.Pending)) errors.Add("Còn yêu cầu điều chỉnh công chưa xử lý.");
        var attendanceDays = new Dictionary<DateTime, decimal>();
        foreach (var row in rows)
        {
            var checkIn = row.EffectiveCheckInAt; var checkOut = row.EffectiveCheckOutAt;
            if (!checkIn.HasValue || !checkOut.HasValue || checkOut <= checkIn)
            { errors.Add($"Ngày {row.WorkDate:dd/MM}: thiếu hoặc sai giờ vào/ra."); continue; }
            var paid = TruckingSystem.Domain.Attendance.AttendanceRules.PaidDays(checkIn.Value, checkOut.Value);
            var key = row.WorkDate.Date;
            attendanceDays[key] = attendanceDays.TryGetValue(key, out var existing) ? existing + paid : paid;
        }
        var holidayList = await db.PublicHolidays.AsNoTracking().Where(h => h.Date >= from && h.Date <= to).Select(h => h.Date).ToListAsync();
        var holidays = holidayList.Select(h => h.Date).ToHashSet();
        var leaveRows = await db.LeaveRecords.AsNoTracking().Where(l => l.EmployeeId == employee.Id && l.LeaveDate >= from && l.LeaveDate <= to)
            .Select(l => new { l.LeaveDate, l.Type, l.BatchId }).ToListAsync();
        var leaves = new Dictionary<DateTime, LeaveType>();
        var batchByDate = new Dictionary<DateTime, Guid?>();
        foreach (var l in leaveRows) { leaves[l.LeaveDate.Date] = l.Type; batchByDate[l.LeaveDate.Date] = l.BatchId; }
        var days = LeaveRules.ClassifyPayrollDays(from, to, attendanceDays, holidays, leaves);
        if (rows.Count == 0 && days.PaidLeaveDays + days.UnpaidLeaveDays == 0)
            errors.Add("Chưa có dữ liệu chấm công của tháng.");
        var snapshot = new
        {
            Attendance = rows.Select(r => new { r.WorkDate, r.EffectiveCheckInAt, r.EffectiveCheckOutAt, r.AdjustmentRequestId }).ToList(),
            Holidays = days.HolidayDates,
            Leaves = days.LeaveDates.Select(x => new { x.Date, Type = x.Type.ToString(), BatchId = batchByDate.GetValueOrDefault(x.Date.Date) }).ToList()
        };
        return (days, snapshot, errors);
    }

}
