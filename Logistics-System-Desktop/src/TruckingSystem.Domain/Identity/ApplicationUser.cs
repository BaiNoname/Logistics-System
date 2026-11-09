using Microsoft.AspNetCore.Identity;

namespace TruckingSystem.Domain.Identity;

/// <summary>
/// Mở rộng AspNetUsers mặc định của Identity. Không lưu thông tin hồ sơ
/// (họ tên, CMND...) ở đây — thông tin đó nằm ở Employee, nối qua Employee.UserId.
/// AspNetUser chỉ lo việc đăng nhập.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public bool IsActive { get; set; } = true;
}
