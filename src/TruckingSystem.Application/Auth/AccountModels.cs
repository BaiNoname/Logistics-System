namespace TruckingSystem.Application.Auth;

public record AccountDto(string Id, string Username, string? Email, bool IsActive, IReadOnlyCollection<string> Roles, int? EmployeeId = null, string? EmployeeName = null);
public record AccountEmployeeOptionDto(int Id, string Code, string FullName);
public record CreateAccountRequest(string Username, string? Email, string Password, bool IsActive, IReadOnlyCollection<string> Roles, int? EmployeeId = null);
public record UpdateAccountRequest(string? Email, bool IsActive, IReadOnlyCollection<string> Roles)
{
    private int? _employeeId;
    public int? EmployeeId
    {
        get => _employeeId;
        init { _employeeId = value; EmployeeAssignmentSpecified = true; }
    }

    // Client cũ không gửi EmployeeId phải giữ nguyên liên kết; null là bỏ gán.
    [System.Text.Json.Serialization.JsonIgnore]
    public bool EmployeeAssignmentSpecified { get; private init; }
}
public record ResetPasswordRequest(string Password);
public record AccountActionResult(bool Success, AccountDto? Account = null, string? Message = null, bool NotFound = false);
