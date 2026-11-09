using TruckingSystem.Domain.Common;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Domain.Entities;

/// <summary>Chia 1 kho thành các khu vực riêng biệt theo tính chất hàng hóa (thường/dễ vỡ/nguy hiểm).</summary>
public class WarehouseZone : BaseEntity
{
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public string Name { get; set; } = string.Empty; // vd: "Khu A", "Khu hàng dễ vỡ"
    public ZoneType ZoneType { get; set; } = ZoneType.Normal;

    // Vị trí/kích thước THẬT (mét) của khu vực này bên trong sơ đồ tổng thể
    // của kho (Warehouse.LengthMeters x WidthMeters) — dùng để vẽ đúng tỉ lệ,
    // đặt đúng chỗ, không phải chỉ hiện thẻ rời rạc như trước.
    public decimal PosX { get; set; }
    public decimal PosY { get; set; }
    public decimal WidthMeters { get; set; }
    public decimal HeightMeters { get; set; }

    public ICollection<StorageBin> StorageBins { get; set; } = new List<StorageBin>();
}