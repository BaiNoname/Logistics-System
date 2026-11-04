using Microsoft.EntityFrameworkCore;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Infrastructure.Persistence;

internal static class PayrollConfiguration
{
    public static void Configure(ModelBuilder builder)
    {
        builder.Entity<PayrollKpiPolicy>(e =>
        {
            e.Property(p => p.EffectiveFrom).HasColumnType("date");
            e.HasIndex(p => p.EffectiveFrom).IsUnique();
            e.ToTable(t => t.HasCheckConstraint("CK_PayrollKpiPolicy_Positive", "\"KmPerPoint\" > 0 AND \"TargetPoints\" > 0"));
        });
        builder.Entity<PayrollMonthlyAdjustment>(e =>
        {
            e.HasOne(a => a.Employee).WithMany().HasForeignKey(a => a.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(a => new { a.Year, a.Month, a.EmployeeId });
            e.Property(a => a.Reason).HasMaxLength(500);
            e.ToTable(t => t.HasCheckConstraint("CK_PayrollMonthlyAdjustment_Valid", "\"Amount\" > 0 AND \"Month\" BETWEEN 1 AND 12 AND \"Year\" BETWEEN 2000 AND 2100"));
        });
        builder.Entity<DriverKpiRate>(e =>
        {
            e.Property(r => r.VehicleType).HasMaxLength(100);
            e.HasIndex(r => r.VehicleType).IsUnique();
            e.ToTable(t => t.HasCheckConstraint("CK_DriverKpiRate_Positive", "\"VehicleFactor\" > 0 AND \"AmountPerPoint\" > 0 AND \"KmPerPoint\" > 0"));
        });
        builder.Entity<Employee>().ToTable(t => t.HasCheckConstraint("CK_Employee_EmploymentDates",
            "\"TerminationDate\" IS NULL OR \"TerminationDate\" >= \"HireDate\"::date"));
        builder.Entity<EmployeeSalary>(e =>
        {
            e.HasOne(s => s.Employee).WithMany().HasForeignKey(s => s.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(s => new { s.EmployeeId, s.EffectiveFrom }).IsUnique();
            e.Property(s => s.EffectiveFrom).HasColumnType("date");
            e.Property(s => s.Reason).HasMaxLength(500);
            e.ToTable(t => t.HasCheckConstraint("CK_EmployeeSalary_Positive", "\"MonthlySalary\" > 0"));
        });
        builder.Entity<PayrollPeriod>(e =>
        {
            e.HasIndex(p => new { p.Year, p.Month }).IsUnique();
            e.Property(p => p.Revision).IsConcurrencyToken();
            e.ToTable(t => t.HasCheckConstraint("CK_PayrollPeriod_Month", "\"Month\" BETWEEN 1 AND 12 AND \"Year\" BETWEEN 2000 AND 2100"));
        });
        builder.Entity<MonthlyAttendance>(e =>
        {
            e.HasOne(a => a.Employee).WithMany().HasForeignKey(a => a.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.PayrollPeriod).WithMany().HasForeignKey(a => a.PayrollPeriodId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(a => new { a.PayrollPeriodId, a.EmployeeId }).IsUnique();
            e.Property(a => a.Note).HasMaxLength(500);
            e.Property(a => a.DriverWorkDates).HasMaxLength(500);
            e.Property(a => a.WorkFrom).HasColumnType("date");
            e.Property(a => a.WorkTo).HasColumnType("date");
            e.ToTable(t => t.HasCheckConstraint("CK_MonthlyAttendance_Days",
                "\"StandardDays\" > 0 AND \"StandardDays\" <= 31 AND \"WorkedDays\" >= 0 AND \"PaidLeaveDays\" >= 0 AND \"HolidayDays\" >= 0 AND \"UnpaidLeaveDays\" >= 0 AND \"WorkedDays\" + \"PaidLeaveDays\" + \"HolidayDays\" + \"UnpaidLeaveDays\" <= CASE WHEN \"DriverWorkDates\" IS NOT NULL THEN 31 ELSE \"StandardDays\" END AND \"WeekdayOvertimeHours\" >= 0 AND \"RestDayOvertimeHours\" >= 0 AND \"HolidayOvertimeHours\" >= 0"));
        });
        builder.Entity<PayrollSlip>(e =>
        {
            e.HasOne(s => s.Employee).WithMany().HasForeignKey(s => s.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.PayrollPeriod).WithMany().HasForeignKey(s => s.PayrollPeriodId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(s => new { s.PayrollPeriodId, s.EmployeeId }).IsUnique();
            e.Property(s => s.SalaryEffectiveFrom).HasColumnType("date");
            e.Property(s => s.WorkFrom).HasColumnType("date");
            e.Property(s => s.WorkTo).HasColumnType("date");
            e.Property(s => s.DriverWorkDates).HasMaxLength(500);
            e.ToTable(t => t.HasCheckConstraint("CK_PayrollSlip_Balance", "\"NetPay\" >= 0 AND \"PaidAmount\" >= 0 AND \"PaidAmount\" <= \"NetPay\""));
        });
        builder.Entity<PayrollItem>(e =>
        {
            e.HasOne(i => i.PayrollSlip).WithMany(s => s.Items).HasForeignKey(i => i.PayrollSlipId).OnDelete(DeleteBehavior.Restrict);
            e.Property(i => i.Reason).HasMaxLength(500);
            e.ToTable(t => t.HasCheckConstraint("CK_PayrollItem_Amount", "\"Amount\" > 0 AND \"CalculatedAmount\" >= 0"));
        });
        builder.Entity<SalaryPayment>(e =>
        {
            e.HasOne(p => p.PayrollSlip).WithMany(s => s.Payments).HasForeignKey(p => p.PayrollSlipId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(p => p.Reference).IsUnique();
            e.Property(p => p.Reference).HasMaxLength(100);
            e.Property(p => p.PaidOn).HasColumnType("date");
            e.ToTable(t => t.HasCheckConstraint("CK_SalaryPayment_Positive", "\"Amount\" > 0"));
        });
    }
}
