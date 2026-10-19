using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Application.Drivers;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;
using TruckingSystem.Domain.Payroll;

namespace TruckingSystem.Application.Payroll;

public partial class PayrollService(IApplicationDbContext db) : IPayrollService
{
    //lấy danh sách
    public async Task<List<PayrollEmployeeDto>> GetEmployeesAsync(int year, int month)
    {
        if (!ValidPeriod(year, month)) return new();
        var (start, cutoff) = PayrollRules.PeriodRange(year, month);
        var end = cutoff.AddDays(1);
        var driverPositions = await DriverPositionRules.DriverPositionIdsAsync(db);
        var period = await db.PayrollPeriods.AsNoTracking().FirstOrDefaultAsync(p => p.Year == year && p.Month == month);
        if (period?.ApprovedAt is not null)
            return await db.PayrollSlips.AsNoTracking().Where(s => s.PayrollPeriodId == period.Id)
                .OrderBy(s => s.EmployeeName).Select(s => new PayrollEmployeeDto(s.EmployeeId, s.EmployeeCode,
                    s.EmployeeName, s.DepartmentName, s.PositionName, s.Employee.HireDate, s.Employee.TerminationDate,
                    s.DriverWorkDates != null)).ToListAsync();
        return await db.Employees.AsNoTracking()
            .Where(e => e.HireDate < end && (e.TerminationDate == null || e.TerminationDate >= start))
            .OrderBy(e => e.FullName)
            .Select(e => new PayrollEmployeeDto(e.Id, e.Code, e.FullName,
                e.Department == null ? "" : e.Department.Name, e.Position == null ? "" : e.Position.Name,
                e.HireDate, e.TerminationDate,
                e.DriverProfile != null || (e.PositionId.HasValue && driverPositions.Contains(e.PositionId.Value)))).ToListAsync();
    }
    //lấy lịch sử lương
    public async Task<List<SalaryDto>> GetSalariesAsync(int employeeId)
    {
        var rates = await db.EmployeeSalaries.AsNoTracking().Where(s => s.EmployeeId == employeeId)
            .OrderBy(s => s.EffectiveFrom).ToListAsync();
        return rates.Select((s, i) => new SalaryDto(s.Id, s.EffectiveFrom,
            i + 1 < rates.Count ? rates[i + 1].EffectiveFrom.AddDays(-1) : null, s.MonthlySalary, s.Reason)).ToList();
    }
    //thêm lương mới
    public Task<PayrollResult> AddSalaryAsync(int employeeId, SalaryRequest request, string actor) => WriteAsync(async () =>
    {
        var employee = await EmployeeAsync(employeeId);
        if (employee is null) return Fail("Không tìm thấy nhân viên/tài xế.");
        if (!PayrollRules.ValidMoney(request.MonthlySalary)) return Fail("Lương tháng phải là số nguyên dương, tối đa 1.000 tỷ đồng.");
        if (!ValidReason(request.Reason)) return Fail("Nhập căn cứ thay đổi lương (1–500 ký tự).");
        var from = request.EffectiveFrom.Date;
        if (from.Year < 2000 || from.Year > 2100 || from < employee.HireDate.Date
            || (employee.TerminationDate.HasValue && from > employee.TerminationDate.Value.Date))
            return Fail("Ngày hiệu lực phải thuộc thời gian làm việc, từ năm 2000 đến 2100.");
        if (from.Day != 26 && from != employee.HireDate.Date)
            return Fail("Mức lương áp dụng từ ngày 26 đầu kỳ; nhân viên/tài xế mới có thể áp dụng từ ngày vào làm.");
        var latest = await db.EmployeeSalaries.Where(s => s.EmployeeId == employeeId)
            .OrderByDescending(s => s.EffectiveFrom).FirstOrDefaultAsync();
        if (latest is not null && from <= latest.EffectiveFrom)
            return Fail("Ngày hiệu lực phải sau mức lương gần nhất; lịch sử lương không được ghi đè.");
        var salary = new EmployeeSalary
        {
            EmployeeId = employeeId,
            EffectiveFrom = from,
            MonthlySalary = request.MonthlySalary,
            Reason = request.Reason.Trim(),
            CreatedBy = actor
        };
        db.EmployeeSalaries.Add(salary);
        Audit(actor, "SetSalary", "EmployeeSalary", employeeId, request);
        return new(true);
    });
    //lấy danh sách kỳ lương
    public Task<List<PayrollPeriodDto>> GetPeriodsAsync() => db.PayrollPeriods.AsNoTracking()
        .OrderByDescending(p => p.Year).ThenByDescending(p => p.Month)
        .Select(p => new PayrollPeriodDto(p.Id, p.Year, p.Month, p.Revision, p.CreatedBy, p.ApprovedAt, p.ApprovedBy)).ToListAsync();
    //  tạo kỳ lương mới
    public Task<PayrollResult> CreatePeriodAsync(PeriodRequest request, string actor) => WriteAsync(async () =>
    {
        if (!ValidPeriod(request.Year, request.Month)) return Fail("Kỳ lương phải từ tháng 1–12, năm 2000–2100.");
        var existing = await db.PayrollPeriods.FirstOrDefaultAsync(p => p.Year == request.Year && p.Month == request.Month);
        if (existing is not null) return new(true, Id: existing.Id);
        db.PayrollPeriods.Add(new PayrollPeriod { Year = request.Year, Month = request.Month, CreatedBy = actor });
        Audit(actor, "Create", "PayrollPeriod", null, request);
        return new(true);
    });
    //lấy danh sách công
    public Task<List<AttendanceDto>> GetAttendanceAsync(int periodId) => db.MonthlyAttendances.AsNoTracking()
        .Where(a => a.PayrollPeriodId == periodId).OrderBy(a => a.Employee.FullName)
        .Select(a => new AttendanceDto(a.EmployeeId, a.Employee.Code, a.Employee.FullName, a.StandardDays,
            a.WorkedDays, a.PaidLeaveDays, a.HolidayDays, a.UnpaidLeaveDays, a.WeekdayOvertimeHours,
            a.RestDayOvertimeHours, a.HolidayOvertimeHours, a.Note, a.ConfirmedAt, a.ConfirmedBy,
            a.DriverWorkDates, a.WorkFrom, a.WorkTo)).ToListAsync();
    //lưu công
    public Task<PayrollResult> SaveAttendanceAsync(int periodId, int employeeId, AttendanceRequest request, string actor) => WriteAsync(async () =>
    {
        var period = await db.PayrollPeriods.FindAsync(periodId);
        var error = PeriodError(period, request.Revision);
        if (error is not null) return Fail(error);
        if (await db.PayrollSlips.AnyAsync(s => s.PayrollPeriodId == periodId && s.EmployeeId == employeeId))
            return Fail("Xóa phiếu lương nháp trước khi sửa công, sau đó xác nhận công và lập lại phiếu.");
        var employee = await EmployeeAsync(employeeId);
        if (employee is not null && IsDriver(employee))
            return Fail("Công tài xế phải lấy từ lịch làm việc; chọn Lấy công tài xế.");
        var range = PayrollRules.PeriodRange(period!.Year, period.Month);
        var attendance = new MonthlyAttendance
        {
            WorkFrom = range.From, WorkTo = range.To,
            EmployeeId = employeeId,
            PayrollPeriodId = periodId,
            StandardDays = request.StandardDays,
            WorkedDays = request.WorkedDays,
            PaidLeaveDays = request.PaidLeaveDays,
            HolidayDays = request.HolidayDays,
            UnpaidLeaveDays = request.UnpaidLeaveDays,
            WeekdayOvertimeHours = request.WeekdayOvertimeHours,
            RestDayOvertimeHours = request.RestDayOvertimeHours,
            HolidayOvertimeHours = request.HolidayOvertimeHours,
            Note = request.Note?.Trim() ?? ""
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
            existing.PaidLeaveDays = attendance.PaidLeaveDays;
            existing.HolidayDays = attendance.HolidayDays;
            existing.UnpaidLeaveDays = attendance.UnpaidLeaveDays;
            existing.WeekdayOvertimeHours = attendance.WeekdayOvertimeHours;
            existing.RestDayOvertimeHours = attendance.RestDayOvertimeHours;
            existing.HolidayOvertimeHours = attendance.HolidayOvertimeHours;
            existing.Note = attendance.Note;
            existing.DriverWorkDates = null;
            existing.ConfirmedAt = null;
            existing.ConfirmedBy = null;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        Touch(period!);
        Audit(actor, "SaveAttendance", "MonthlyAttendance", employeeId, new { periodId, request });
        return new(true);
    });
    //xác nhận công
    public Task<PayrollResult> ConfirmAttendanceAsync(int periodId, int employeeId, RevisionRequest request, string actor) => WriteAsync(async () =>
    {
        var period = await db.PayrollPeriods.FindAsync(periodId);
        var error = PeriodError(period, request.Revision);
        if (error is not null) return Fail(error);
        var attendance = await db.MonthlyAttendances.FirstOrDefaultAsync(a => a.PayrollPeriodId == periodId && a.EmployeeId == employeeId);
        if (attendance is null) return Fail("Chưa nhập công cho nhân viên.");
        var employee = await EmployeeAsync(employeeId);
        error = AttendanceError(employee, period!, attendance);
        if (error is not null) return Fail(error);
        error = await DriverScheduleErrorAsync(employee!, period!, attendance);
        if (error is not null) return Fail(error);
        if (attendance.ConfirmedAt.HasValue) return new(true);
        attendance.ConfirmedAt = DateTime.UtcNow;
        attendance.ConfirmedBy = actor;
        Touch(period!);
        Audit(actor, "ConfirmAttendance", "MonthlyAttendance", attendance.Id, new { periodId, employeeId });
        return new(true);
    });
    //lấy danh sách phiếu lương
    public async Task<List<PayrollSlipDto>> GetSlipsAsync(int periodId)
    {
        var slips = await db.PayrollSlips.AsNoTracking().Include(s => s.Items).Include(s => s.Payments)
            .Where(s => s.PayrollPeriodId == periodId).OrderBy(s => s.EmployeeName).ToListAsync();
        return slips.Select(ToDto).ToList();
    }
    //tạo phiếu lương nháp
    public Task<PayrollResult> GenerateSlipAsync(int periodId, int employeeId, RevisionRequest request, string actor) => WriteAsync(async () =>
    {
        var period = await db.PayrollPeriods.FindAsync(periodId);
        var error = PeriodError(period, request.Revision);
        if (error is not null) return Fail(error);
        var existing = await db.PayrollSlips.FirstOrDefaultAsync(s => s.PayrollPeriodId == periodId && s.EmployeeId == employeeId);
        if (existing is not null) return new(true, "Phiếu đã tồn tại. Xóa phiếu nháp nếu cần tính lại.", existing.Id);
        var employee = await EmployeeAsync(employeeId);
        var attendance = await db.MonthlyAttendances.FirstOrDefaultAsync(a => a.PayrollPeriodId == periodId && a.EmployeeId == employeeId);
        if (attendance?.ConfirmedAt is null) return Fail("Cần nhập và xác nhận công trước khi lập phiếu lương.");
        error = AttendanceError(employee, period!, attendance);
        if (error is not null) return Fail(error);
        error = await DriverScheduleErrorAsync(employee!, period!, attendance);
        if (error is not null) return Fail(error);
        var salary = await SalaryForAsync(employee!, period!);
        if (salary is null) return Fail("Chưa có mức lương áp dụng trọn kỳ 26–25. Kiểm tra ngày hiệu lực trong lịch sử mức lương.");
        var earned = PayrollRules.Prorate(salary.MonthlySalary, attendance.StandardDays,
            attendance.WorkedDays + attendance.PaidLeaveDays + attendance.HolidayDays);
        db.PayrollSlips.Add(new PayrollSlip
        {
            EmployeeId = employeeId,
            PayrollPeriodId = periodId,
            EmployeeCode = employee!.Code,
            EmployeeName = employee.FullName,
            DepartmentName = employee.Department?.Name ?? "",
            PositionName = employee.Position?.Name ?? "",
            MonthlySalary = salary.MonthlySalary,
            SalaryEffectiveFrom = salary.EffectiveFrom,
            StandardDays = attendance.StandardDays,
            WorkedDays = attendance.WorkedDays,
            PaidLeaveDays = attendance.PaidLeaveDays,
            HolidayDays = attendance.HolidayDays,
            UnpaidLeaveDays = attendance.UnpaidLeaveDays,
            WeekdayOvertimeHours = attendance.WeekdayOvertimeHours,
            RestDayOvertimeHours = attendance.RestDayOvertimeHours,
            HolidayOvertimeHours = attendance.HolidayOvertimeHours,
            AttendanceNote = attendance.Note,
            AttendanceConfirmedBy = attendance.ConfirmedBy!,
            AttendanceConfirmedAt = attendance.ConfirmedAt.Value,
            EarnedSalary = earned,
            GrossIncome = earned,
            NetPay = earned,
            DriverWorkDates = attendance.DriverWorkDates,
            WorkFrom = attendance.WorkFrom,
            WorkTo = attendance.WorkTo
        });
        Touch(period!);
        Audit(actor, "Generate", "PayrollSlip", null, new { periodId, employeeId });
        return new(true);
    });
    //xóa phiếu lương nháp
    public Task<PayrollResult> DeleteDraftSlipAsync(int slipId, RevisionRequest request, string actor) => WriteAsync(async () =>
    {
        var slip = await SlipAsync(slipId);
        var error = PeriodError(slip?.PayrollPeriod, request.Revision);
        if (error is not null) return Fail(error);
        db.PayrollItems.RemoveRange(slip!.Items);
        db.PayrollSlips.Remove(slip);
        Touch(slip.PayrollPeriod);
        Audit(actor, "DeleteDraft", "PayrollSlip", slipId, ToDto(slip));
        return new(true);
    });
    //thêm khoản cộng/trừ hoặc điều chỉnh
    public Task<PayrollResult> AddItemAsync(int slipId, PayrollItemRequest request, bool adjustment, string actor) => WriteAsync(async () =>
    {
        var slip = await SlipAsync(slipId);
        var error = PeriodError(slip?.PayrollPeriod, request.Revision, adjustment);
        if (error is not null) return Fail(error);
        if (adjustment && !slip!.PayrollPeriod.ApprovedAt.HasValue) return Fail("Chỉ điều chỉnh sau duyệt; phiếu nháp dùng khoản cộng/trừ.");
        if (!Enum.TryParse<PayrollItemKind>(request.Kind, out var kind) || !Enum.IsDefined(kind)) return Fail("Loại khoản không hợp lệ.");
        if (!PayrollRules.ValidMoney(request.Amount) || !ValidReason(request.Reason))
            return Fail("Số tiền phải là số nguyên dương, tối đa 1.000 tỷ; căn cứ/lý do từ 1–500 ký tự.");
        if (request.ProrateByAttendance && (kind != PayrollItemKind.Allowance || adjustment))
            return Fail("Chỉ phụ cấp trước duyệt được tính theo công; điều chỉnh là số tiền thực tế.");
        if (slip!.Items.Count >= 100) return Fail("Mỗi phiếu tối đa 100 khoản cộng/trừ và điều chỉnh.");
        var item = new PayrollItem
        {
            Kind = kind,
            Amount = request.Amount,
            ProrateByAttendance = request.ProrateByAttendance,
            Reason = request.Reason.Trim(),
            IsAdjustment = adjustment,
            CreatedBy = actor
        };
        item.CalculatedAmount = request.ProrateByAttendance
            ? PayrollRules.Prorate(request.Amount, slip.StandardDays, slip.WorkedDays + slip.PaidLeaveDays + slip.HolidayDays)
            : request.Amount;
        var net = slip.NetPay + (kind == PayrollItemKind.Deduction ? -item.CalculatedAmount : item.CalculatedAmount);
        if (net < 0 || net < slip.PaidAmount) return Fail("Thực lĩnh không được âm hoặc thấp hơn số đã chi trả.");
        slip.Items.Add(item);
        Recalculate(slip);
        Touch(slip.PayrollPeriod);
        Audit(actor, adjustment ? "AdjustApproved" : "AddItem", "PayrollSlip", slipId, request);
        return new(true);
    });
    //xóa khoản cộng/trừ
    public Task<PayrollResult> DeleteItemAsync(int slipId, int itemId, RevisionRequest request, string actor) => WriteAsync(async () =>
    {
        var slip = await SlipAsync(slipId);
        var error = PeriodError(slip?.PayrollPeriod, request.Revision);
        if (error is not null) return Fail(error);
        var item = slip!.Items.FirstOrDefault(i => i.Id == itemId);
        if (item is null) return Fail("Không tìm thấy khoản cộng/trừ.");
        if (item.Kind != PayrollItemKind.Deduction && slip.NetPay < item.CalculatedAmount)
            return Fail("Xóa khoản này sẽ làm thực lĩnh âm; kiểm tra khoản khấu trừ trước.");
        slip.Items.Remove(item);
        db.PayrollItems.Remove(item);
        Recalculate(slip);
        Touch(slip.PayrollPeriod);
        Audit(actor, "DeleteItem", "PayrollSlip", slipId, new { item.Id, item.Kind, item.Amount, item.Reason });
        return new(true);
    });
    //duyệt kỳ lương
    public Task<PayrollResult> ApproveAsync(int periodId, RevisionRequest request, string actor) => WriteAsync(async () =>
    {
        var period = await db.PayrollPeriods.FindAsync(periodId);
        var error = PeriodError(period, request.Revision);
        if (error is not null) return Fail(error);
        var employees = await GetEmployeesAsync(period!.Year, period.Month);
        var range = PayrollRules.PeriodRange(period.Year, period.Month);
        if (VnClock.Today < range.To)
            return Fail("Chưa đến ngày chốt lương (ngày 25 của tháng). Chỉ được lập phiếu nháp trước ngày chốt.");
        var slips = await db.PayrollSlips.Include(s => s.Items).Where(s => s.PayrollPeriodId == periodId).ToListAsync();
        if (slips.Count == 0 || !employees.Select(e => e.Id).Order().SequenceEqual(slips.Select(s => s.EmployeeId).Order()))
            return Fail("Phải có đúng một phiếu cho mỗi nhân viên thuộc kỳ (kể cả công bằng 0). Kiểm tra danh sách và phiếu còn thiếu/thừa.");
        foreach (var slip in slips)
        {
            if (slip.WorkFrom != range.From || slip.WorkTo != range.To)
                return Fail($"{slip.EmployeeCode}: phiếu nháp chưa theo kỳ 26–25; xóa phiếu nháp, kiểm tra/nhập lại công và lập lại phiếu.");
            var employee = await EmployeeAsync(slip.EmployeeId);
            var attendance = await db.MonthlyAttendances.FirstOrDefaultAsync(a => a.PayrollPeriodId == periodId && a.EmployeeId == slip.EmployeeId);
            if (attendance?.ConfirmedAt is null) return Fail($"{slip.EmployeeCode}: công chưa xác nhận.");
            error = AttendanceError(employee, period, attendance);
            if (error is not null) return Fail($"{slip.EmployeeCode}: {error}");
            error = await DriverScheduleErrorAsync(employee!, period, attendance);
            if (error is not null) return Fail($"{slip.EmployeeCode}: {error}");
            if (slip.DriverWorkDates != attendance.DriverWorkDates)
                return Fail($"{slip.EmployeeCode}: nguồn công đã thay đổi; xóa phiếu nháp và lập lại.");
            var salary = await SalaryForAsync(employee!, period);
            if (salary is null || salary.MonthlySalary != slip.MonthlySalary || salary.EffectiveFrom != slip.SalaryEffectiveFrom)
                return Fail($"{slip.EmployeeCode}: mức lương đã thay đổi; xóa phiếu nháp và lập lại trước khi duyệt.");
            var hours = slip.WeekdayOvertimeHours + slip.RestDayOvertimeHours + slip.HolidayOvertimeHours;
            if (hours > 0 && !slip.Items.Any(i => i.Kind == PayrollItemKind.Overtime))
                return Fail($"{slip.EmployeeCode}: có giờ tăng ca nhưng chưa nhập tiền tăng ca và căn cứ tính.");
        }
        period.ApprovedAt = DateTime.UtcNow;
        period.ApprovedBy = actor;
        foreach (var slip in slips) slip.ApprovedNetPay = slip.NetPay;
        Touch(period);
        Audit(actor, "Approve", "PayrollPeriod", periodId, new { Count = slips.Count, NetPay = slips.Sum(s => s.NetPay) });
        return new(true);
    });
    //chi trả phiếu lương
    public Task<PayrollResult> PayAsync(int slipId, PaymentRequest request, string actor) => WriteAsync(async () =>
    {
        var slip = await SlipAsync(slipId);
        var error = PeriodError(slip?.PayrollPeriod, request.Revision, true);
        if (error is not null) return Fail(error);
        if (!slip!.PayrollPeriod.ApprovedAt.HasValue) return Fail("Chỉ chi trả phiếu lương đã duyệt.");
        if (!PayrollRules.ValidMoney(request.Amount) || request.Amount > slip.NetPay - slip.PaidAmount)
            return Fail("Số tiền chi phải là số nguyên dương và không vượt số còn phải trả.");
        if (request.PaidOn.Date > VnClock.Today || request.PaidOn.Year < 2000)
            return Fail("Ngày trả phải từ năm 2000 đến hôm nay.");
        if (request.Method is not ("Cash" or "BankTransfer")) return Fail("Chọn tiền mặt hoặc chuyển khoản.");
        var reference = request.Reference?.Trim().ToUpperInvariant() ?? "";
        if (reference.Length is < 1 or > 100) return Fail("Nhập mã chứng từ/giao dịch duy nhất (1–100 ký tự).");
        if (await db.SalaryPayments.AnyAsync(p => p.Reference == reference)) return Fail("Mã chứng từ/giao dịch này đã được ghi nhận, không chi trùng.");
        slip.Payments.Add(new SalaryPayment
        {
            Amount = request.Amount,
            PaidOn = request.PaidOn.Date,
            Method = request.Method,
            Reference = reference,
            RecordedBy = actor
        });
        slip.PaidAmount += request.Amount;
        Touch(slip.PayrollPeriod);
        Audit(actor, "Pay", "PayrollSlip", slipId, request);
        return new(true);
    });
    //lấy thông tin nhân viên
    private Task<Employee?> EmployeeAsync(int id) => db.Employees.Include(e => e.Position).Include(e => e.Department)
        .Include(e => e.DriverProfile).FirstOrDefaultAsync(e => e.Id == id);
    //lấy thông tin phiếu lương
    private Task<PayrollSlip?> SlipAsync(int id) => db.PayrollSlips.Include(s => s.PayrollPeriod)
        .Include(s => s.Items).Include(s => s.Payments).FirstOrDefaultAsync(s => s.Id == id);
    //lấy thông tin mức lương áp dụng cho nhân viên trong kỳ
    private async Task<EmployeeSalary?> SalaryForAsync(Employee employee, PayrollPeriod period)
    {
        var range = PayrollRules.PeriodRange(period.Year, period.Month);
        var effective = employee.HireDate.Date > range.From ? employee.HireDate.Date : range.From;
        var salary = await db.EmployeeSalaries.Where(s => s.EmployeeId == employee.Id && s.EffectiveFrom <= range.To)
            .OrderByDescending(s => s.EffectiveFrom).FirstOrDefaultAsync();
        // shortcut: aggregate attendance cannot split mid-cycle rates; use daily attendance when supporting mid-cycle changes.
        return salary is not null && salary.EffectiveFrom <= effective ? salary : null;
    }
    //kiểm tra nhân viên có phải tài xế hay không
    private static bool IsDriver(Employee e) => e.DriverProfile is not null || DriverPositionRules.IsDriverPosition(e.Position?.Name);
    private static bool ValidPeriod(int year, int month) => year is >= 2000 and <= 2100 && month is >= 1 and <= 12;
    private static bool ValidReason(string? value) => !string.IsNullOrWhiteSpace(value) && value.Trim().Length <= 500;
    private static PayrollResult Fail(string message) => new(false, message);
    private static string? PeriodError(PayrollPeriod? period, int revision, bool allowApproved = false)
    {
        if (period is null) return "Không tìm thấy kỳ/phiếu lương.";
        if (period.Revision != revision) return "Kỳ lương đã thay đổi; tải lại dữ liệu trước khi thao tác.";
        return !allowApproved && period.ApprovedAt.HasValue ? "Kỳ lương đã duyệt và khóa." : null;
    }
    private static void Touch(PayrollPeriod period) { period.Revision++; period.UpdatedAt = DateTime.UtcNow; }
    private static string? AttendanceError(Employee? employee, PayrollPeriod period, MonthlyAttendance a)
    {
        if (employee is null) return "Nhân viên/tài xế không tồn tại.";
        if (IsDriver(employee) != (a.DriverWorkDates is not null))
            return "Nhóm nhân viên/tài xế đã thay đổi hoặc chưa lấy công từ lịch; nhập/lấy lại công trước khi tính lương.";
        if (employee.Status == EmployeeStatus.Terminated && !employee.TerminationDate.HasValue)
            return "Bổ sung ngày nghỉ việc thực tế trước khi tính công/lương.";
        var (start, end) = PayrollRules.PeriodRange(period.Year, period.Month);
        if (a.WorkFrom != start || a.WorkTo != end)
            return "Công chưa theo kỳ 26–25; xóa phiếu nháp nếu có, kiểm tra/nhập lại công trước khi xác nhận.";
        if (employee.HireDate.Date > end || employee.TerminationDate?.Date < start) return "Nhân viên không làm việc trong kỳ này.";
        if (employee.HireDate.Year < 2000 || employee.TerminationDate?.Date < employee.HireDate.Date)
            return "Kiểm tra ngày vào/nghỉ việc thực tế của nhân viên.";
        var employedStart = employee.HireDate.Date > start ? employee.HireDate.Date : start;
        var employedEnd = employee.TerminationDate?.Date < end ? employee.TerminationDate.Value.Date : end;
        var quantities = new[] { a.StandardDays, a.WorkedDays, a.PaidLeaveDays, a.HolidayDays, a.UnpaidLeaveDays,
            a.WeekdayOvertimeHours, a.RestDayOvertimeHours, a.HolidayOvertimeHours };
        if (quantities.Any(q => !PayrollRules.ValidQuantity(q) || q > 744)) return "Công/giờ phải từ 0–744 và tối đa 2 chữ số thập phân.";
        var days = a.WorkedDays + a.PaidLeaveDays + a.HolidayDays + a.UnpaidLeaveDays;
        if (a.StandardDays <= 0 || a.StandardDays > (end - start).Days + 1)
            return "Công chuẩn phải lớn hơn 0 và không vượt số ngày trong kỳ công.";
        if (days > a.StandardDays || days > (employedEnd - employedStart).Days + 1)
            return "Tổng công làm/nghỉ/lễ vượt công chuẩn hoặc thời gian làm việc trong kỳ.";
        if (a.WeekdayOvertimeHours + a.RestDayOvertimeHours + a.HolidayOvertimeHours > ((employedEnd - employedStart).Days + 1) * 24)
            return "Giờ tăng ca vượt thời gian làm việc trong kỳ.";
        return a.Note.Length > 500 ? "Ghi chú công tối đa 500 ký tự." : null;
    }
    private static void Recalculate(PayrollSlip slip)
    {
        slip.GrossIncome = slip.EarnedSalary + slip.Items.Where(i => i.Kind != PayrollItemKind.Deduction).Sum(i => i.CalculatedAmount);
        slip.Deductions = slip.Items.Where(i => i.Kind == PayrollItemKind.Deduction).Sum(i => i.CalculatedAmount);
        slip.NetPay = slip.GrossIncome - slip.Deductions;
        slip.UpdatedAt = DateTime.UtcNow;
    }
    private void Audit(string actor, string action, string entity, int? id, object detail) => db.AuditLogs.Add(new AuditLog
    { UserId = actor, Action = action, EntityName = entity, EntityId = id, Detail = JsonSerializer.Serialize(detail) });
    private async Task<PayrollResult> WriteAsync(Func<Task<PayrollResult>> action)
    {
        // Serialize read/check/write so concurrent approval, attendance, rates and payments cannot diverge.
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable)
            : await db.Database.BeginTransactionAsync();
        var result = await action();
        if (!result.Success) return result;
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return result;
    }
    private static PayrollSlipDto ToDto(PayrollSlip s) => new(s.Id, s.EmployeeId, s.PayrollPeriodId,
        s.EmployeeCode, s.EmployeeName, s.DepartmentName, s.PositionName, s.MonthlySalary, s.SalaryEffectiveFrom,
        s.StandardDays, s.WorkedDays, s.PaidLeaveDays, s.HolidayDays, s.UnpaidLeaveDays,
        s.WeekdayOvertimeHours, s.RestDayOvertimeHours, s.HolidayOvertimeHours, s.AttendanceNote,
        s.AttendanceConfirmedBy, s.AttendanceConfirmedAt, s.EarnedSalary, s.GrossIncome, s.Deductions, s.NetPay,
        s.ApprovedNetPay, s.PaidAmount, s.NetPay - s.PaidAmount, PayrollRules.PaymentStatus(s.NetPay, s.PaidAmount),
        s.Items.OrderBy(i => i.Id).Select(i => new PayrollItemDto(i.Id, i.Kind.ToString(), i.Amount,
            i.ProrateByAttendance, i.CalculatedAmount, i.Reason, i.IsAdjustment, i.CreatedBy, i.CreatedAt)).ToList(),
        s.Payments.OrderBy(p => p.PaidOn).ThenBy(p => p.Id).Select(p => new SalaryPaymentDto(p.Id, p.Amount,
            p.PaidOn, p.Method, p.Reference, p.RecordedBy)).ToList(),
        s.DriverWorkDates is not null, s.DriverWorkDates, s.WorkFrom, s.WorkTo);
}
