using Microsoft.AspNetCore.Identity;

namespace TruckingSystem.Domain.Identity;

/// <summary>
/// Mở rộng AspNetRoles mặc định. Giữ đơn giản, không cần thêm field ngay.
/// </summary>
public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }
}
