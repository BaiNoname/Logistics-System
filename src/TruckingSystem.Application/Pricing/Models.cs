namespace TruckingSystem.Application.Pricing;

public record PriceListDto(int Id, string Name, DateTime EffectiveDate, int RuleCount, int ContractCount, string Status = "Active");
public record SavePriceListRequest(string Name, DateTime EffectiveDate, string? Status = null);
public record CopyPriceListRequest(string Name);
public record TariffRuleDto(int Id, int PriceListId, string VehicleType, decimal PricePerKm,
    decimal PricePerTon, decimal SurchargeFragile, decimal SurchargeHazmat);
public record SaveTariffRuleRequest(string VehicleType, decimal PricePerKm,
    decimal PricePerTon, decimal SurchargeFragile, decimal SurchargeHazmat);
