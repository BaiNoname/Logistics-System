namespace TruckingSystem.Application.Auth;

public interface IAccountManagementService
{
    Task<List<AccountDto>> GetAllAsync();
    Task<List<string>> GetRolesAsync();
    Task<AccountActionResult> CreateAsync(CreateAccountRequest request);
    Task<AccountActionResult> UpdateAsync(string id, string? actorId, UpdateAccountRequest request);
    Task<AccountActionResult> ResetPasswordAsync(string id, ResetPasswordRequest request);
}
