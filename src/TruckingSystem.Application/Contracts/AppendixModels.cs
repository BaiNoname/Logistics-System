namespace TruckingSystem.Application.Contracts;

public record ContractAppendixDto(int Id, string Code, int ContractId, string ContractCode,
    DateTime IssueDate, DateTime EffectiveDate, AppendixTariffOptionDto? TariffRule,
    decimal? DiscountPercent = null, int? PriceListId = null, string? PriceListName = null,
    string Status = "Active", string ContractStatus = "Active");
public record SaveContractAppendixRequest(string Code, int ContractId, DateTime IssueDate,
    DateTime EffectiveDate, int? TariffRuleId = null, decimal? DiscountPercent = null, int? PriceListId = null);
public record AppendixTariffOptionDto(int Id, string PriceListName, string VehicleType,
    decimal PricePerKm, decimal PricePerTon, decimal SurchargeFragile, decimal SurchargeHazmat, int PriceListId = 0);
public record ContractAppendixLookupsDto(List<ContractOptionDto> Contracts, List<AppendixTariffOptionDto> TariffRules)
{
    public List<ContractOptionDto> PriceLists { get; init; } = new();
    public List<AppendixContractOptionDto> ContractDiscounts { get; init; } = new();
    public int MaxDiscountPercent { get; init; }
}

public record AppendixContractOptionDto(int Id, string? Type, decimal? DiscountPercent);
