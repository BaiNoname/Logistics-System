using TruckingSystem.Domain.Common;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Domain.Entities;

/// <summary>
/// Cầu nối cho phép 1 đơn hàng lớn chia thành nhiều chuyến xe nhỏ.
/// Ghi rõ trong Trip cụ thể nào, chở bao nhiêu trên tổng CargoLine.
/// </summary>
public class TripLine : BaseEntity
{
    public int TripId { get; set; }
    public Trip Trip { get; set; } = null!;

    public int CargoLineId { get; set; }
    public CargoLine CargoLine { get; set; } = null!;

    public decimal QtyPlanned { get; set; }
    public decimal QtyPicked { get; set; }
    public decimal QtyDelivered { get; set; }
    public TripLineStatus Status { get; set; } = TripLineStatus.Planned;
}
