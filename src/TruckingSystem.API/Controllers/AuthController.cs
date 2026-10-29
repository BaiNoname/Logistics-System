using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Auth;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.API.Controllers;

/// <summary>
/// Controller giờ rất "mỏng" — chỉ nhận request, gọi Application Service, trả response.
/// Không còn UserManager/SignInManager/TokenService nằm ở đây nữa (đã dời sang
/// TruckingSystem.Infrastructure.Auth.AuthService, gọi qua interface IAuthService).
/// Mỗi lần đăng nhập (thành công / thất bại) ghi 1 dòng NHẬT KÝ HỆ THỐNG.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IApplicationDbContext _db;

    public AuthController(IAuthService authService, IApplicationDbContext db)
    {
        _authService = authService;
        _db = db;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResult>> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        try
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            _db.AuditLogs.Add(new AuditLog
            {
                Action = result.Success ? "Login" : "LoginFailed",
                EntityName = "Account",
                UserName = string.IsNullOrWhiteSpace(request.Username) ? null : request.Username.Trim(),
                UserRole = result.Success && result.Roles is { Count: > 0 } ? string.Join(",", result.Roles) : null,
                Detail = (result.Success ? "Đăng nhập thành công" : "Đăng nhập thất bại: " + result.Message) +
                         (string.IsNullOrEmpty(ip) ? "" : $" — máy {ip}"),
            });
            await _db.SaveChangesAsync();
        }
        catch { /* ghi nhật ký lỗi không được chặn việc đăng nhập */ }
        return result.Success ? Ok(result) : Unauthorized(result);
    }
}
