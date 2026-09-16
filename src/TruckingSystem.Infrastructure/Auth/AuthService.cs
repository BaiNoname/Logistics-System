using Microsoft.AspNetCore.Identity;
using TruckingSystem.Application.Auth;
using TruckingSystem.Domain.Identity;

namespace TruckingSystem.Infrastructure.Auth;

/// <summary>
/// Cài đặt thật của IAuthService — dùng UserManager/SignInManager (ASP.NET Core
/// Identity, gắn chặt EF Core) nên thuộc về Infrastructure, không phải Application.
/// </summary>
public class AuthService : IAuthService
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenGenerator _tokenGenerator;

    public AuthService(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ITokenGenerator tokenGenerator)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user is null || !user.IsActive)
            return new LoginResult(false, null, "Sai tài khoản hoặc tài khoản đã bị khóa.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
            return new LoginResult(false, null, "Sai tài khoản hoặc mật khẩu.");

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenGenerator.GenerateToken(user.Id, user.UserName ?? string.Empty, roles);

        return new LoginResult(true, token, null);
    }
}
