using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Auth;
using TruckingSystem.Domain.Identity;
using TruckingSystem.Infrastructure.Persistence;

namespace TruckingSystem.Infrastructure.Auth;

public class AccountManagementService : IAccountManagementService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly RoleManager<ApplicationRole> _roles;
    private readonly AccountEmployeeService _employees;

    public AccountManagementService(AppDbContext db, UserManager<ApplicationUser> users, RoleManager<ApplicationRole> roles, AccountEmployeeService employees)
    {
        _db = db;
        _users = users;
        _roles = roles;
        _employees = employees;
    }

    public async Task<List<AccountDto>> GetAllAsync()
    {
        var users = await _db.Users.AsNoTracking().OrderBy(u => u.UserName).ToListAsync();
        var assignments = await (from link in _db.UserRoles
                                 join role in _db.Roles on link.RoleId equals role.Id
                                 select new { link.UserId, role.Name }).ToListAsync();
        var rolesByUser = assignments.ToLookup(a => a.UserId, a => a.Name!);
        return await _employees.IncludeEmployeesAsync(users.Select(u => new AccountDto(u.Id, u.UserName!, u.Email, u.IsActive,
            rolesByUser[u.Id].OrderBy(r => r).ToArray())).ToList());
    }

    public Task<List<string>> GetRolesAsync() =>
        _roles.Roles.OrderBy(r => r.Name).Select(r => r.Name!).ToListAsync();

    public async Task<AccountActionResult> CreateAsync(CreateAccountRequest request)
    {
        var error = await ValidateRoles(request.Roles);
        if (error is not null) return new(false, Message: error);
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return new(false, Message: "Cần nhập tên đăng nhập và mật khẩu.");

        await using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var user = new ApplicationUser
        {
            UserName = request.Username.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            IsActive = request.IsActive,
        };
        var created = await _users.CreateAsync(user, request.Password);
        if (!created.Succeeded) return new(false, Message: Errors(created));

        var assigned = await _users.AddToRolesAsync(user, request.Roles.Distinct(StringComparer.OrdinalIgnoreCase));
        if (!assigned.Succeeded) return new(false, Message: Errors(assigned));
        error = await _employees.AssignAsync(user.Id, request.EmployeeId);
        if (error is not null) return new(false, Message: error);
        await _db.SaveChangesAsync();
        var accounts = await _employees.IncludeEmployeesAsync(new() { new(user.Id, user.UserName!, user.Email, user.IsActive, request.Roles) });
        await transaction.CommitAsync();
        return new(true, accounts[0]);
    }

    public async Task<AccountActionResult> UpdateAsync(string id, string? actorId, UpdateAccountRequest request)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var user = await _users.FindByIdAsync(id);
        if (user is null) return new(false, NotFound: true);
        var error = await ValidateRoles(request.Roles);
        if (error is not null) return new(false, Message: error);

        var currentRoles = await _users.GetRolesAsync(user);
        if (id == actorId && (!request.IsActive || !request.Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase)))
            return new(false, Message: "Không thể tự khóa tài khoản hoặc gỡ quyền Admin của mình.");
        if (currentRoles.Contains("Admin") &&
            (!request.IsActive || !request.Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase)) &&
            !await HasAnotherActiveAdmin(id))
            return new(false, Message: "Phải còn ít nhất một tài khoản Admin đang hoạt động.");

        if (request.EmployeeAssignmentSpecified)
        {
            error = await _employees.AssignAsync(id, request.EmployeeId);
            if (error is not null) return new(false, Message: error);
        }
        user.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        user.IsActive = request.IsActive;
        var updated = await _users.UpdateAsync(user);
        if (!updated.Succeeded) return new(false, Message: Errors(updated));

        var removed = await _users.RemoveFromRolesAsync(user, currentRoles.Except(request.Roles, StringComparer.OrdinalIgnoreCase));
        if (!removed.Succeeded) return new(false, Message: Errors(removed));
        var added = await _users.AddToRolesAsync(user, request.Roles.Except(currentRoles, StringComparer.OrdinalIgnoreCase));
        if (!added.Succeeded) return new(false, Message: Errors(added));
        await transaction.CommitAsync();
        return new(true);
    }

    public async Task<AccountActionResult> ResetPasswordAsync(string id, ResetPasswordRequest request)
    {
        var user = await _users.FindByIdAsync(id);
        if (user is null) return new(false, NotFound: true);
        if (string.IsNullOrWhiteSpace(request.Password)) return new(false, Message: "Cần nhập mật khẩu mới.");
        var token = await _users.GeneratePasswordResetTokenAsync(user);
        var result = await _users.ResetPasswordAsync(user, token, request.Password);
        return result.Succeeded ? new(true) : new(false, Message: Errors(result));
    }

    private async Task<string?> ValidateRoles(IReadOnlyCollection<string>? roles)
    {
        if (roles is null || roles.Count == 0) return "Chọn ít nhất một vai trò.";
        var valid = await _roles.Roles.Select(r => r.Name!).ToListAsync();
        return roles.All(r => valid.Contains(r, StringComparer.OrdinalIgnoreCase)) ? null : "Vai trò không hợp lệ.";
    }

    private Task<bool> HasAnotherActiveAdmin(string id) =>
        (from user in _db.Users
         join link in _db.UserRoles on user.Id equals link.UserId
         join role in _db.Roles on link.RoleId equals role.Id
         where user.Id != id && user.IsActive && role.Name == "Admin"
         select user.Id).AnyAsync();

    private static string Errors(IdentityResult result) => string.Join("; ", result.Errors.Select(e => e.Description));
}
