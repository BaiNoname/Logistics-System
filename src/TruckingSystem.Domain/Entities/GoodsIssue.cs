using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

/// <summary>Biên nhận hàng XUẤT kho — hàng được đưa lên xe để đi giao (gắn với Trip chặng Delivery).</summary>
public class GoodsIssue : BaseEntity
{
    public int TransportOrderId { get; set; }
    public TransportOrder TransportOrder { get; set; } = null!;

    public int CargoLineId { get; set; }
    public CargoLine CargoLine { get; set; } = null!;

    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public int TripId { get; set; }
    public Trip Trip { get; set; } = null!;

    public int IssuedByEmployeeId { get; set; }
    public Employee IssuedByEmployee { get; set; } = null!;

    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public decimal QtyIssued { get; set; }
}
