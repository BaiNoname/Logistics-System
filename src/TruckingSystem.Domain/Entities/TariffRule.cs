using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

public class TariffRule : BaseEntity
{
    public int PriceListId { get; set; }
    public PriceList PriceList { get; set; } = null!;

    public string VehicleType { get; set; } = string.Empty;
    public decimal PricePerKm { get; set; }
    public decimal PricePerTon { get; set; }
    public decimal SurchargeFragile { get; set; }
    public decimal SurchargeHazmat { get; set; }
}
