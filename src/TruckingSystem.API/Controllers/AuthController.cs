using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Auth;

namespace TruckingSystem.API.Controllers;

/// <summary>
/// Controller giờ rất "mỏng" — chỉ nhận request, gọi Application Service, trả response.
/// Không còn UserManager/SignInManager/TokenService nằm ở đây nữa (đã dời sang
/// TruckingSystem.Infrastructure.Auth.AuthService, gọi qua interface IAuthService).
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResult>> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return result.Success ? Ok(result) : Unauthorized(result);
    }
}
