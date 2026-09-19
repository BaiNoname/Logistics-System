using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

/// <summary>Từng vị trí lưu trữ cụ thể (ô/kệ/pallet) trong 1 khu vực — đơn vị nhỏ nhất để xếp hàng.</summary>
public class StorageBin : BaseEntity
{
    public int WarehouseZoneId { get; set; }
    public WarehouseZone WarehouseZone { get; set; } = null!;

    public string Code { get; set; } = string.Empty; // vd: "A-01-01"
    public decimal CapacityM3 { get; set; }
    public decimal MaxWeightKg { get; set; }

    /// <summary>Thể tích đang chiếm dụng — cập nhật mỗi khi có StorageAssignment mới/gỡ bỏ.</summary>
    public decimal CurrentUsedM3 { get; set; }

    // Tọa độ hàng/cột trong lưới — dùng để VẼ vị trí này đúng chỗ thật trên sơ
    // đồ trực quan của kho (không ảnh hưởng logic Best-Fit, chỉ phục vụ hiển thị).
    public int Row { get; set; }
    public int Column { get; set; }

    public ICollection<StorageAssignment> Assignments { get; set; } = new List<StorageAssignment>();
}