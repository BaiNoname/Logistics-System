namespace TruckingSystem.Desktop.DTO_PricingManagement;

public record PriceListItem(int Id, string Name, DateTime EffectiveDate, int RuleCount, int ContractCount);
public record TariffRuleItem(int Id, int PriceListId, string VehicleType, decimal PricePerKm,
    decimal PricePerTon, decimal SurchargeFragile, decimal SurchargeHazmat);
