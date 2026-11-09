namespace TruckingSystem.Application.Pricing;

public record PriceListDto(int Id, string Name, DateTime EffectiveDate, int RuleCount, int ContractCount, string Status = "Active");
public record SavePriceListRequest(string Name, DateTime EffectiveDate, string? Status = null, List<SaveTariffRuleRequest>? TariffRules = null);
public record CopyPriceListRequest(string Name, List<SaveTariffRuleRequest>? TariffRules = null);
public record TariffRuleDto(int Id, int PriceListId, string VehicleType, decimal PricePerKm,
    decimal PricePerTon, decimal SurchargeFragile, decimal SurchargeHazmat);
public record SaveTariffRuleRequest(string VehicleType, decimal PricePerKm,
    decimal PricePerTon, decimal? SurchargeFragile = null, decimal? SurchargeHazmat = null);

public record SurchargeRates(DateOnly EffectiveDate, decimal FragilePercent, decimal HazmatPercent);

public record SaveTariffRuleRow(int? Id, string VehicleType, decimal PricePerKm, decimal PricePerTon);
public record SaveTariffRulesRequest(List<SaveTariffRuleRow> Rows);
