namespace TruckingSystem.Application.Pricing;

public record PriceListDto(int Id, string Name, DateTime EffectiveDate, int RuleCount, int ContractCount);
public record SavePriceListRequest(string Name, DateTime EffectiveDate);
public record TariffRuleDto(int Id, int PriceListId, string VehicleType, decimal PricePerKm,
    decimal PricePerTon, decimal SurchargeFragile, decimal SurchargeHazmat);
public record SaveTariffRuleRequest(string VehicleType, decimal PricePerKm,
    decimal PricePerTon, decimal SurchargeFragile, decimal SurchargeHazmat);
