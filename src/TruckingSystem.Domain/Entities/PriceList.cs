using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

public class PriceList : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }

    public ICollection<TariffRule> TariffRules { get; set; } = new List<TariffRule>();
}
