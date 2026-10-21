namespace TruckingSystem.Application.Payroll;

public record PayrollResult(bool Success, string? Message = null, int? Id = null);
public record PayrollEmployeeDto(int Id, string Code, string FullName, string DepartmentName,
    string PositionName, DateTime HireDate, DateTime? TerminationDate, bool IsDriver = false);
public record SalaryDto(int Id, DateTime EffectiveFrom, DateTime? EffectiveTo, decimal MonthlySalary, string Reason);
public record SalaryRequest(DateTime EffectiveFrom, decimal MonthlySalary, string Reason);
public record PeriodRequest(int Year, int Month);
public record RevisionRequest(int Revision);
public record DriverAttendanceRequest(int Revision, decimal StandardDays);
public record DriverPayrollDaysDto(DateTime FromDate, DateTime ToDate, List<DateTime> Dates);
public record PayrollPeriodDto(int Id, int Year, int Month, int Revision, string CreatedBy,
    DateTime? ApprovedAt, string? ApprovedBy);
public record AttendanceRequest(int Revision, decimal StandardDays, decimal WorkedDays, decimal PaidLeaveDays,
    decimal HolidayDays, decimal UnpaidLeaveDays, decimal WeekdayOvertimeHours,
    decimal RestDayOvertimeHours, decimal HolidayOvertimeHours, string Note);
public record AttendanceDto(int EmployeeId, string EmployeeCode, string EmployeeName,
    decimal StandardDays, decimal WorkedDays, decimal PaidLeaveDays, decimal HolidayDays, decimal UnpaidLeaveDays,
    decimal WeekdayOvertimeHours, decimal RestDayOvertimeHours, decimal HolidayOvertimeHours,
    string Note, DateTime? ConfirmedAt, string? ConfirmedBy, string? DriverWorkDates = null,
    DateTime? WorkFrom = null, DateTime? WorkTo = null);
public record PayrollItemRequest(int Revision, string Kind, decimal Amount, bool ProrateByAttendance, string Reason);
public record PaymentRequest(int Revision, decimal Amount, DateTime PaidOn, string Method, string Reference);
public record PayrollItemDto(int Id, string Kind, decimal Amount, bool ProrateByAttendance,
    decimal CalculatedAmount, string Reason, bool IsAdjustment, string CreatedBy, DateTime CreatedAt);
public record SalaryPaymentDto(int Id, decimal Amount, DateTime PaidOn, string Method, string Reference, string RecordedBy);
public record PayrollSlipDto(int Id, int EmployeeId, int PayrollPeriodId, string EmployeeCode, string EmployeeName,
    string DepartmentName, string PositionName, decimal MonthlySalary, DateTime SalaryEffectiveFrom,
    decimal StandardDays, decimal WorkedDays, decimal PaidLeaveDays, decimal HolidayDays, decimal UnpaidLeaveDays,
    decimal WeekdayOvertimeHours, decimal RestDayOvertimeHours, decimal HolidayOvertimeHours,
    string AttendanceNote, string AttendanceConfirmedBy, DateTime AttendanceConfirmedAt,
    decimal EarnedSalary, decimal GrossIncome, decimal Deductions, decimal NetPay, decimal? ApprovedNetPay,
    decimal PaidAmount, decimal RemainingAmount, string PaymentStatus,
    List<PayrollItemDto> Items, List<SalaryPaymentDto> Payments,
    bool IsDriver = false, string? DriverWorkDates = null, DateTime? WorkFrom = null, DateTime? WorkTo = null);
