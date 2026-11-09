using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Payroll;

namespace TruckingSystem.Application.Payroll;

internal static class SalaryWriter
{
    // Stage only: the caller saves the employee, salary and audit in one transaction.
    public static async Task<string?> StageAsync(IApplicationDbContext db, Employee employee,
        SalaryRequest request, string actor)
    {
        if (!PayrollRules.ValidMoney(request.MonthlySalary))
            return "Lương tháng phải là số nguyên dương, tối đa 1.000 tỷ đồng.";
        var latest = employee.Id == 0 ? null : await db.EmployeeSalaries
            .Where(s => s.EmployeeId == employee.Id).OrderByDescending(s => s.EffectiveFrom).FirstOrDefaultAsync();
        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Trim().Length > 500)
            return "Nhập căn cứ thay đổi lương (1–500 ký tự).";
        var from = request.EffectiveFrom.Date;
        if (from.Year < 2000 || from.Year > 2100 || from < employee.HireDate.Date
            || (employee.TerminationDate.HasValue && from > employee.TerminationDate.Value.Date))
            return "Ngày hiệu lực phải thuộc thời gian làm việc, từ năm 2000 đến 2100.";
        if (from.Day != 1 && from != employee.HireDate.Date)
            return "Mức lương áp dụng từ ngày 1 đầu kỳ; nhân viên/tài xế mới có thể áp dụng từ ngày vào làm.";
        if (latest is not null && from <= latest.EffectiveFrom)
            return "Ngày hiệu lực phải sau mức lương gần nhất; lịch sử lương không được ghi đè.";
        db.EmployeeSalaries.Add(new EmployeeSalary
        {
            Employee = employee, EffectiveFrom = from, MonthlySalary = request.MonthlySalary,
            Reason = request.Reason.Trim(), CreatedBy = actor
        });
        db.AuditLogs.Add(new AuditLog
        {
            UserId = actor, Action = "SetSalary", EntityName = "EmployeeSalary",
            EntityId = employee.Id == 0 ? null : employee.Id,
            Detail = JsonSerializer.Serialize(new { employee.Code, request.EffectiveFrom, request.MonthlySalary, request.Reason })
        });
        return null;
    }
}
