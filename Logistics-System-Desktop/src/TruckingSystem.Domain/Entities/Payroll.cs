using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

// A rate lasts until the next EffectiveFrom; there are no overlapping editable end dates.
public class EmployeeSalary : BaseEntity
{
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public DateTime EffectiveFrom { get; set; }
    public decimal MonthlySalary { get; set; }
    public string Reason { get; set; } = "";
    public string CreatedBy { get; set; } = "";
}

public class PayrollPeriod : BaseEntity
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Revision { get; set; }
    public string CreatedBy { get; set; } = "";
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

public class MonthlyAttendance : BaseEntity
{
    public DateTime? WorkFrom { get; set; }
    public DateTime? WorkTo { get; set; }
    public string? DriverWorkDates { get; set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public int PayrollPeriodId { get; set; }
    public PayrollPeriod PayrollPeriod { get; set; } = null!;
    public decimal StandardDays { get; set; }
    public decimal WorkedDays { get; set; }
    public decimal PaidLeaveDays { get; set; }
    public decimal HolidayDays { get; set; }
    public decimal UnpaidLeaveDays { get; set; }
    public decimal WeekdayOvertimeHours { get; set; }
    public decimal RestDayOvertimeHours { get; set; }
    public decimal HolidayOvertimeHours { get; set; }
    public string Note { get; set; } = "";
    public string? ConfirmedBy { get; set; }
    public DateTime? ConfirmedAt { get; set; }
}

public class PayrollSlip : BaseEntity
{
    public int CalculationVersion { get; set; }
    public string SourceSnapshot { get; set; } = "";
    public decimal KpiPoints { get; set; }
    public decimal KpiTarget { get; set; }
    public string? DriverWorkDates { get; set; }
    public DateTime? WorkFrom { get; set; }
    public DateTime? WorkTo { get; set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public int PayrollPeriodId { get; set; }
    public PayrollPeriod PayrollPeriod { get; set; } = null!;
    public string EmployeeCode { get; set; } = "";
    public string EmployeeName { get; set; } = "";
    public string DepartmentName { get; set; } = "";
    public string PositionName { get; set; } = "";
    public decimal MonthlySalary { get; set; }
    public DateTime SalaryEffectiveFrom { get; set; }
    public decimal StandardDays { get; set; }
    public decimal WorkedDays { get; set; }
    public decimal PaidLeaveDays { get; set; }
    public decimal HolidayDays { get; set; }
    public decimal UnpaidLeaveDays { get; set; }
    public decimal WeekdayOvertimeHours { get; set; }
    public decimal RestDayOvertimeHours { get; set; }
    public decimal HolidayOvertimeHours { get; set; }
    public string AttendanceNote { get; set; } = "";
    public string AttendanceConfirmedBy { get; set; } = "";
    public DateTime AttendanceConfirmedAt { get; set; }
    public decimal EarnedSalary { get; set; }
    public decimal GrossIncome { get; set; }
    public decimal Deductions { get; set; }
    public decimal NetPay { get; set; }
    public decimal? ApprovedNetPay { get; set; }
    public decimal PaidAmount { get; set; }
    public List<PayrollItem> Items { get; set; } = new();
    public List<SalaryPayment> Payments { get; set; } = new();
}

public enum PayrollItemKind { Allowance, Bonus, Overtime, Deduction }

public class PayrollItem : BaseEntity
{
    public bool IsDriverKpi { get; set; }
    public int PayrollSlipId { get; set; }
    public PayrollSlip PayrollSlip { get; set; } = null!;
    public PayrollItemKind Kind { get; set; }
    public decimal Amount { get; set; }
    public bool ProrateByAttendance { get; set; }
    public decimal CalculatedAmount { get; set; }
    public string Reason { get; set; } = "";
    public bool IsAdjustment { get; set; }
    public string CreatedBy { get; set; } = "";
}

public class SalaryPayment : BaseEntity
{
    public int PayrollSlipId { get; set; }
    public PayrollSlip PayrollSlip { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime PaidOn { get; set; }
    public string Method { get; set; } = "";
    public string Reference { get; set; } = "";
    public string RecordedBy { get; set; } = "";
}

public class DriverKpiRate : BaseEntity
{
    public string VehicleType { get; set; } = "";
    public decimal VehicleFactor { get; set; }
    public decimal AmountPerPoint { get; set; }
    public decimal KmPerPoint { get; set; } = 200;
}

public class PayrollKpiPolicy : BaseEntity
{
    public DateTime EffectiveFrom { get; set; }
    public decimal KmPerPoint { get; set; }
    public decimal TargetPoints { get; set; }
    public string FactorsJson { get; set; } = "[]";
}

public class PayrollMonthlyAdjustment : BaseEntity
{
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public int Year { get; set; }
    public int Month { get; set; }
    public PayrollItemKind Kind { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = "";
    public string CreatedBy { get; set; } = "";
}
