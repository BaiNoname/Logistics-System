namespace TruckingSystem.Domain.Entities;

/// <summary>
/// Bảng N-N nối AspNetRoles (RoleId kiểu string) với Permission.
/// Không kế thừa BaseEntity vì đây là bảng nối thuần, dùng khóa chính ghép.
/// </summary>
public class RolePermission
{
    public string RoleId { get; set; } = string.Empty; // FK -> AspNetRoles.Id
    public int PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}
