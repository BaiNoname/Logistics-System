namespace TruckingSystem.Application.Auth;

public record AccountDto(string Id, string Username, string? Email, bool IsActive, IReadOnlyCollection<string> Roles);
public record CreateAccountRequest(string Username, string? Email, string Password, bool IsActive, IReadOnlyCollection<string> Roles);
public record UpdateAccountRequest(string? Email, bool IsActive, IReadOnlyCollection<string> Roles);
public record ResetPasswordRequest(string Password);
public record AccountActionResult(bool Success, AccountDto? Account = null, string? Message = null, bool NotFound = false);
