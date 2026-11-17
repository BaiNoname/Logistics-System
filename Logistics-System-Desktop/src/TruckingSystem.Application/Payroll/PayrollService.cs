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
        var today = VnClock.Today;
        return rates.Select((s, i) =>
        {
            DateTime? effectiveTo = i + 1 < rates.Count ? rates[i + 1].EffectiveFrom.AddDays(-1) : null;
            return new SalaryDto(s.Id, s.EffectiveFrom, effectiveTo, s.MonthlySalary, s.Reason,
                s.EmployeeId, s.UpdatedAt ?? s.CreatedAt, PayrollRules.SalaryStatus(s.EffectiveFrom, effectiveTo, today));
        }).ToList();
    }
    //thêm lương mới
    public Task<PayrollResult> AddSalaryAsync(int employeeId, SalaryRequest request, string actor) => WriteAsync(async () =>
    {
        var employee = await EmployeeAsync(employeeId);
        if (employee is null) return Fail("Không tìm thấy nhân viên/tài xế.");
        var error = await SalaryWriter.StageAsync(db, employee, request, actor);
        if (error is not null) return Fail(error);
        return new(true);
    });
    //lấy danh sách kỳ lương
    public Task<List<PayrollPeriodDto>> GetPeriodsAsync() => db.PayrollPeriods.AsNoTracking()
        .OrderByDescending(p => p.Year).ThenByDescending(p => p.Month)
        .Select(p => new PayrollPeriodDto(p.Id, p.Year, p.Month, p.Revision, p.CreatedBy, p.ApprovedAt, p.ApprovedBy,
            p.LastAutomaticCalculationAt, p.AutomaticCalculationError)).ToListAsync();
    //lấy danh sách phiếu lương
    public async Task<List<PayrollSlipDto>> GetSlipsAsync(int periodId)
    {
        var slips = await db.PayrollSlips.AsNoTracking().Include(s => s.Items).Include(s => s.Payments)
            .Where(s => s.PayrollPeriodId == periodId).OrderBy(s => s.EmployeeName).ToListAsync();
        return slips.Select(ToDto).ToList();
    }
    //tạo phiếu lương nháp
    public Task<PayrollResult> GenerateSlipAsync(int periodId, int employeeId, RevisionRequest request, string actor)
        => Task.FromResult(Fail("Hệ thống tự tính toàn bộ bảng lương tháng hằng ngày."));
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
        if (slip!.CalculationVersion == 2) return Fail("Dùng danh sách cộng/trừ theo tháng; bảng đã chốt thì ghi điều chỉnh ở tháng chưa chốt.");
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
        if (slip!.CalculationVersion == 2) return Fail("Xóa khoản tại danh sách cộng/trừ tháng, sau đó tính lại bảng nháp.");
        var item = slip!.Items.FirstOrDefault(i => i.Id == itemId);
        if (item is null) return Fail("Không tìm thấy khoản cộng/trừ.");
        if (item.IsDriverKpi) return Fail("KPI tự tính theo chuyến; xóa và lập lại phiếu nháp để tính lại.");
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
            return Fail("Chưa đến ngày chốt lương (ngày cuối tháng). Chỉ được lập phiếu nháp trước ngày chốt.");
        var slips = await db.PayrollSlips.Include(s => s.Items).Where(s => s.PayrollPeriodId == periodId).ToListAsync();
        if (slips.Count == 0 || !employees.Select(e => e.Id).Order().SequenceEqual(slips.Select(s => s.EmployeeId).Order()))
            return Fail("Phải có đúng một phiếu cho mỗi nhân viên thuộc kỳ (kể cả công bằng 0). Kiểm tra danh sách và phiếu còn thiếu/thừa.");
        if (slips.Any(s => s.CalculationVersion == 2))
        {
            var current = await CalculateMonthAsync(period, actor);
            if (current.Errors.Count > 0) return Fail(string.Join(Environment.NewLine, current.Errors));
            if (slips.Any(s => s.CalculationVersion != 2 || current.Slips.SingleOrDefault(c => c.EmployeeId == s.EmployeeId)?.SourceSnapshot != s.SourceSnapshot))
                return Fail("Công, chuyến, mức lương, cấu hình hoặc khoản cộng/trừ đã thay đổi. Tải lại bảng lương sau lần tính tự động tiếp theo rồi đối soát và chốt.");
            period.ApprovedAt = DateTime.UtcNow;
            period.ApprovedBy = actor;
            foreach (var s in slips) s.ApprovedNetPay = s.NetPay;
            Touch(period);
            Audit(actor, "Approve", "PayrollPeriod", periodId, new { Count = slips.Count, NetPay = slips.Sum(s => s.NetPay) });
            return new(true);
        }
        return Fail("Bảng nháp còn theo cơ chế cũ. Đối chiếu, chuyển khoản tay vào danh sách cộng/trừ tháng rồi xóa nháp cũ để hệ thống tự tính lại.");
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
        var employedTo = employee.TerminationDate?.Date < range.To ? employee.TerminationDate.Value.Date : range.To;
        var salary = await db.EmployeeSalaries.Where(s => s.EmployeeId == employee.Id && s.EffectiveFrom <= employedTo)
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
    private static void Recalculate(PayrollSlip slip)
    {
        slip.GrossIncome = slip.EarnedSalary + slip.Items.Where(i => i.Kind != PayrollItemKind.Deduction).Sum(i => i.CalculatedAmount);
        slip.Deductions = slip.Items.Where(i => i.Kind == PayrollItemKind.Deduction).Sum(i => i.CalculatedAmount);
        slip.NetPay = slip.GrossIncome - slip.Deductions;
        slip.UpdatedAt = DateTime.UtcNow;
    }
    private void Audit(string? actor, string action, string entity, int? id, object detail) => db.AuditLogs.Add(new AuditLog
    { UserId = actor, Action = action, EntityName = entity, EntityId = id, Detail = JsonSerializer.Serialize(detail) });
    private async Task<PayrollResult> WriteAsync(Func<Task<PayrollResult>> action, CancellationToken cancellationToken = default)
    {
        // Serialize read/check/write so concurrent approval, attendance, rates and payments cannot diverge.
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
            : await db.Database.BeginTransactionAsync(cancellationToken);
        var result = await action();
        if (!result.Success) return result;
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return result;
    }
    private static PayrollSlipDto ToDto(PayrollSlip s) => new(s.Id, s.EmployeeId, s.PayrollPeriodId,
        s.EmployeeCode, s.EmployeeName, s.DepartmentName, s.PositionName, s.MonthlySalary, s.SalaryEffectiveFrom,
        s.StandardDays, s.WorkedDays, s.PaidLeaveDays, s.HolidayDays, s.UnpaidLeaveDays,
        s.WeekdayOvertimeHours, s.RestDayOvertimeHours, s.HolidayOvertimeHours, s.AttendanceNote,
        s.AttendanceConfirmedBy, s.AttendanceConfirmedAt, s.EarnedSalary, s.GrossIncome, s.Deductions, s.NetPay,
        s.ApprovedNetPay, s.PaidAmount, s.NetPay - s.PaidAmount, PayrollRules.PaymentStatus(s.NetPay, s.PaidAmount),
        s.Items.OrderBy(i => i.Id).Select(i => new PayrollItemDto(i.Id, i.Kind.ToString(), i.Amount,
            i.ProrateByAttendance, i.CalculatedAmount, i.Reason, i.IsAdjustment, i.CreatedBy, i.CreatedAt, i.IsDriverKpi)).ToList(),
        s.Payments.OrderBy(p => p.PaidOn).ThenBy(p => p.Id).Select(p => new SalaryPaymentDto(p.Id, p.Amount,
            p.PaidOn, p.Method, p.Reference, p.RecordedBy)).ToList(),
        s.DriverWorkDates is not null, s.DriverWorkDates, s.WorkFrom, s.WorkTo, s.CalculationVersion, s.KpiPoints, s.KpiTarget);
}
