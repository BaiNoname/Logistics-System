using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Payroll;

namespace TruckingSystem.Application.Payroll;

public partial class PayrollService
{
    public async Task<DriverPayrollDaysDto?> GetDriverWorkDaysAsync(int periodId, int employeeId)
    {
        var period = await db.PayrollPeriods.FindAsync(periodId);
        var employee = await EmployeeAsync(employeeId);
        if (period is null || employee is null || !IsDriver(employee)) return null;
        var range = PayrollRules.PeriodRange(period.Year, period.Month);
        return new(range.From, range.To, await DriverDatesAsync(employee, period));
    }

    public Task<PayrollResult> ImportDriverAttendanceAsync(int periodId, int employeeId, DriverAttendanceRequest request, string actor)
        => WriteAsync(async () =>
        {
            var period = await db.PayrollPeriods.FindAsync(periodId);
            var error = PeriodError(period, request.Revision);
            if (error is not null) return Fail(error);
            var employee = await EmployeeAsync(employeeId);
            if (employee is null || !IsDriver(employee)) return Fail("Nhân viên được chọn không phải tài xế.");
            if (await db.PayrollSlips.AnyAsync(s => s.PayrollPeriodId == periodId && s.EmployeeId == employeeId))
                return Fail("Xóa phiếu lương nháp trước khi lấy lại công tài xế, sau đó xác nhận và lập lại phiếu.");
            var dates = await DriverDatesAsync(employee, period!);
            var range = PayrollRules.PeriodRange(period!.Year, period.Month);
            var attendance = new MonthlyAttendance
            {
                EmployeeId = employeeId, PayrollPeriodId = periodId, StandardDays = request.StandardDays,
                WorkedDays = dates.Count, DriverWorkDates = FormatDates(dates),
                WorkFrom = range.From, WorkTo = range.To,
                Note = $"Lịch đăng ký {range.From:dd/MM/yyyy}–{range.To:dd/MM/yyyy}; mỗi ngày riêng biệt tính 1 công; chốt ngày 25."
            };
            error = AttendanceError(employee, period!, attendance);
            if (error is not null) return Fail(error);
            var existing = await db.MonthlyAttendances.FirstOrDefaultAsync(a => a.PayrollPeriodId == periodId && a.EmployeeId == employeeId);
            if (existing is null) db.MonthlyAttendances.Add(attendance);
            else
            {
                existing.StandardDays = attendance.StandardDays;
                existing.WorkFrom = attendance.WorkFrom;
                existing.WorkTo = attendance.WorkTo;
                existing.WorkedDays = attendance.WorkedDays;
                existing.DriverWorkDates = attendance.DriverWorkDates;
                existing.Note = attendance.Note;
                existing.PaidLeaveDays = existing.HolidayDays = existing.UnpaidLeaveDays = 0;
                existing.WeekdayOvertimeHours = existing.RestDayOvertimeHours = existing.HolidayOvertimeHours = 0;
                existing.ConfirmedBy = null;
                existing.ConfirmedAt = null;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            Touch(period!);
            Audit(actor, "ImportDriverAttendance", "MonthlyAttendance", employeeId,
                new { periodId, request.StandardDays, attendance.WorkedDays, attendance.DriverWorkDates, range.From, range.To });
            return new(true);
        });

    private async Task<List<DateTime>> DriverDatesAsync(Employee employee, PayrollPeriod period)
    {
        var (from, to) = PayrollRules.PeriodRange(period.Year, period.Month);
        if (employee.HireDate.Date > from) from = employee.HireDate.Date;
        if (employee.TerminationDate.HasValue && employee.TerminationDate.Value.Date < to) to = employee.TerminationDate.Value.Date;
        var endExclusive = to.AddDays(1);
        return await db.DriverSchedules.AsNoTracking()
            .Where(s => s.DriverEmployeeId == employee.Id && s.ShiftDate >= from && s.ShiftDate < endExclusive)
            .Select(s => s.ShiftDate.Date).Distinct().OrderBy(d => d).ToListAsync();
    }

    private async Task<string?> DriverScheduleErrorAsync(Employee employee, PayrollPeriod period, MonthlyAttendance attendance)
    {
        if (!IsDriver(employee)) return null;
        var dates = await DriverDatesAsync(employee, period);
        if (attendance.DriverWorkDates != FormatDates(dates) || attendance.WorkedDays != dates.Count)
            return "Lịch tài xế đã thay đổi; xóa phiếu nháp nếu có, lấy lại công và xác nhận trước khi tính/duyệt lương.";
        if (attendance.PaidLeaveDays != 0 || attendance.HolidayDays != 0 || attendance.UnpaidLeaveDays != 0
            || attendance.WeekdayOvertimeHours != 0 || attendance.RestDayOvertimeHours != 0 || attendance.HolidayOvertimeHours != 0)
            return "Công tài xế chỉ được lấy từ các ngày đăng ký; vui lòng lấy lại công từ lịch làm việc.";
        return null;
    }

    private static string FormatDates(List<DateTime> dates)
        => string.Join(", ", dates.Select(d => d.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)));
}
