namespace TruckingSystem.Application.Payroll;

public record PayrollResult(bool Success, string? Message = null, int? Id = null);
public record PayrollEmployeeDto(int Id, string Code, string FullName, string DepartmentName,
    string PositionName, DateTime HireDate, DateTime? TerminationDate, bool IsDriver = false);
public record SalaryDto(int Id, DateTime EffectiveFrom, DateTime? EffectiveTo, decimal MonthlySalary, string Reason,
    int EmployeeId = 0, DateTime? UpdatedAt = null, string Status = "");
public record SalaryRequest(DateTime EffectiveFrom, decimal MonthlySalary, string Reason);
public record RevisionRequest(int Revision);
public record PayrollPeriodDto(int Id, int Year, int Month, int Revision, string CreatedBy,
    DateTime? ApprovedAt, string? ApprovedBy, DateTime? LastAutomaticCalculationAt = null,
    string? AutomaticCalculationError = null);
public record PayrollItemRequest(int Revision, string Kind, decimal Amount, bool ProrateByAttendance, string Reason);
public record PaymentRequest(int Revision, decimal Amount, DateTime PaidOn, string Method, string Reference);
public record PayrollItemDto(int Id, string Kind, decimal Amount, bool ProrateByAttendance,
    decimal CalculatedAmount, string Reason, bool IsAdjustment, string CreatedBy, DateTime CreatedAt, bool IsDriverKpi = false);
public record SalaryPaymentDto(int Id, decimal Amount, DateTime PaidOn, string Method, string Reference, string RecordedBy);
public record PayrollSlipDto(int Id, int EmployeeId, int PayrollPeriodId, string EmployeeCode, string EmployeeName,
    string DepartmentName, string PositionName, decimal MonthlySalary, DateTime SalaryEffectiveFrom,
    decimal StandardDays, decimal WorkedDays, decimal PaidLeaveDays, decimal HolidayDays, decimal UnpaidLeaveDays,
    decimal WeekdayOvertimeHours, decimal RestDayOvertimeHours, decimal HolidayOvertimeHours,
    string AttendanceNote, string AttendanceConfirmedBy, DateTime AttendanceConfirmedAt,
    decimal EarnedSalary, decimal GrossIncome, decimal Deductions, decimal NetPay, decimal? ApprovedNetPay,
    decimal PaidAmount, decimal RemainingAmount, string PaymentStatus,
    List<PayrollItemDto> Items, List<SalaryPaymentDto> Payments,
    bool IsDriver = false, string? DriverWorkDates = null, DateTime? WorkFrom = null, DateTime? WorkTo = null, int CalculationVersion = 0, decimal KpiPoints = 0, decimal KpiTarget = 0);

public class DriverKpiRateDto
{
    public string VehicleType { get; set; } = "";
    public decimal VehicleFactor { get; set; }
    public decimal AmountPerPoint { get; set; }
    public decimal KmPerPoint { get; set; } = 200;
}

public record GenerateMonthRequest(int Year, int Month, int Revision);
public class KpiFactorDto
{
    public string VehicleType { get; set; } = "";
    public decimal Factor { get; set; }
}
public record KpiPolicyDto(int Id, DateTime EffectiveFrom, decimal KmPerPoint, decimal TargetPoints, List<KpiFactorDto> Factors);
public record SaveKpiPolicyRequest(DateTime EffectiveFrom, decimal KmPerPoint, decimal TargetPoints, List<KpiFactorDto> Factors);
public record MonthlyAdjustmentDto(int Id, int EmployeeId, string EmployeeName, int Year, int Month, string Kind, decimal Amount, string Reason);
public record MonthlyAdjustmentRequest(int EmployeeId, int Year, int Month, string Kind, decimal Amount, string Reason);
public record BulkMonthlyAdjustmentRequest(List<int> EmployeeIds, int Year, int Month, string Kind, decimal Amount, string Reason);
