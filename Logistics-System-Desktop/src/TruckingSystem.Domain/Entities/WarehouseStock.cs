using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

/// <summary>
/// Tồn kho hiện tại theo Warehouse + CargoLine.
/// QtyOnHand = Σ GoodsReceipt.QtyReceived - Σ GoodsIssue.QtyIssued (cập nhật ở service, không phải computed column).
/// </summary>
public class WarehouseStock : BaseEntity
{
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public int CargoLineId { get; set; }
    public CargoLine CargoLine { get; set; } = null!;

    public decimal QtyOnHand { get; set; }
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}
