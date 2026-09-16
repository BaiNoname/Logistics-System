using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

/// <summary>Dòng hàng khách khai báo. QtyRemaining tính ngược = QtyTotal - Σ TripLine.QtyPlanned.</summary>
public class CargoLine : BaseEntity
{
    public int TransportOrderId { get; set; }
    public TransportOrder TransportOrder { get; set; } = null!;

    public string CargoType { get; set; } = string.Empty;
    public int PackageCount { get; set; }
    public decimal WeightKg { get; set; }
    public decimal VolumeM3 { get; set; }
    public bool IsFragile { get; set; }
    public bool IsHazmat { get; set; }
    public decimal QtyTotal { get; set; }

    public ICollection<TripLine> TripLines { get; set; } = new List<TripLine>();
}
