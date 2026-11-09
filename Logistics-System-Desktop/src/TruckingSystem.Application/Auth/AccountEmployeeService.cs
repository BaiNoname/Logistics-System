using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;

namespace TruckingSystem.Application.Auth;

public class AccountEmployeeService(IApplicationDbContext db)
{
    public Task<List<AccountEmployeeOptionDto>> GetOptionsAsync(string? accountId) =>
        db.Employees.AsNoTracking()
            .Where(e => e.UserId == null || e.UserId == accountId)
            .OrderBy(e => e.FullName).ThenBy(e => e.Code)
            .Select(e => new AccountEmployeeOptionDto(e.Id, e.Code, e.FullName)).ToListAsync();

    public async Task<List<AccountDto>> IncludeEmployeesAsync(List<AccountDto> accounts)
    {
        var accountIds = accounts.Select(a => a.Id).ToArray();
        var links = await db.Employees.AsNoTracking().Where(e => e.UserId != null && accountIds.Contains(e.UserId))
            .Select(e => new { e.UserId, e.Id, e.FullName }).ToListAsync();
        var byAccount = links.ToLookup(e => e.UserId!);
        return accounts.Select(a =>
        {
            var employee = byAccount[a.Id].FirstOrDefault();
            return a with { EmployeeId = employee?.Id, EmployeeName = employee?.FullName };
        }).ToList();
    }

    // Caller lưu cùng transaction với tài khoản và phân quyền.
    public async Task<string?> AssignAsync(string accountId, int? employeeId)
    {
        var employee = employeeId.HasValue ? await db.Employees.FindAsync(employeeId.Value) : null;
        if (employeeId.HasValue && employee is null) return "Nhân viên đã chọn không tồn tại.";
        if (employee?.UserId is not null && employee.UserId != accountId)
            return "Nhân viên đã được gán cho tài khoản khác. Vui lòng tải lại danh sách.";

        var current = await db.Employees.Where(e => e.UserId == accountId).ToListAsync();
        foreach (var previous in current) previous.UserId = null;
        if (employee is not null) employee.UserId = accountId;
        return null;
    }
}
