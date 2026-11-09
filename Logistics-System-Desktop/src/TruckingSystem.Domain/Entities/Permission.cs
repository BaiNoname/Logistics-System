using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

/// <summary>
/// Quyền hạt nhân, ví dụ: "transport_order.approve", "vehicle.manage".
/// Dùng cùng RolePermission để phân quyền chi tiết hơn AspNetRoles đơn thuần.
/// </summary>
public class Permission : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Module { get; set; } = string.Empty; // Transport, Vehicle, Finance...

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
