using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.SERVICE_Auth;

public class AccountApiService
{
    private readonly ApiConnection _conn;

    public AccountApiService(ApiConnection conn) => _conn = conn;

    public async Task<List<AccountItem>> GetAllAsync() =>
        await _conn.GetAsync<List<AccountItem>>("/api/accounts") ?? new();

    public async Task<List<string>> GetRolesAsync() =>
        await _conn.GetAsync<List<string>>("/api/accounts/roles") ?? new();

    public async Task<List<AccountEmployeeOption>> GetEmployeeOptionsAsync(string? accountId) =>
        await _conn.GetAsync<List<AccountEmployeeOption>>("/api/accounts/employee-options" +
            (accountId is null ? "" : "?accountId=" + Uri.EscapeDataString(accountId))) ?? new();

    public Task<AccountItem?> CreateAsync(string username, string? email, string password, bool isActive, string[] roles, int? employeeId = null) =>
        _conn.PostAsync<AccountItem>("/api/accounts", new { username, email, password, isActive, roles, employeeId });

    public Task<object?> UpdateAsync(string id, string? email, bool isActive, string[] roles, int? employeeId) =>
        _conn.PutAsync<object>($"/api/accounts/{Uri.EscapeDataString(id)}", new { email, isActive, roles, employeeId });

    public Task<object?> ResetPasswordAsync(string id, string password) =>
        _conn.PutAsync<object>($"/api/accounts/{Uri.EscapeDataString(id)}/password", new { password });
}

public class AccountItem
{
    public string Id { get; set; } = "";
    public string Username { get; set; } = "";
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public List<string> Roles { get; set; } = new();
}

public record AccountEmployeeOption(int Id, string Code, string FullName)
{
    public string DisplayName => Id == 0 ? "Không gán nhân viên" : $"{Code} — {FullName}";
}
