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
        // Chỉ ghi nhật ký đăng nhập THÀNH CÔNG (ai vào hệ thống, lúc nào, từ máy nào). Đăng nhập sai (gõ nhầm mật khẩu...)
        // không ghi — không có giá trị đối chiếu, chỉ làm loãng nhật ký.
        if (result.Success)
        {
            try
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                _db.AuditLogs.Add(new AuditLog
                {
                    Action = "Login",
                    EntityName = "Account",
                    UserName = string.IsNullOrWhiteSpace(request.Username) ? null : request.Username.Trim(),
                    UserRole = result.Roles is { Count: > 0 } ? string.Join(",", result.Roles) : null,
                    Detail = "Đăng nhập thành công" +
                             (string.IsNullOrEmpty(ip) ? "" : $" — máy {ip}"),
                });
                await _db.SaveChangesAsync();
            }
            catch { /* ghi nhật ký lỗi không được chặn việc đăng nhập */ }
        }
        return result.Success ? Ok(result) : Unauthorized(result);
    }
}
