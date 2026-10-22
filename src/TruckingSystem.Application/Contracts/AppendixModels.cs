namespace TruckingSystem.Application.Contracts;

public record ContractAppendixDto(int Id, string Code, int ContractId, string ContractCode,
    DateTime IssueDate, DateTime EffectiveDate, AppendixTariffOptionDto TariffRule);
public record SaveContractAppendixRequest(string Code, int ContractId, DateTime IssueDate,
    DateTime EffectiveDate, int TariffRuleId);
public record AppendixTariffOptionDto(int Id, string PriceListName, string VehicleType,
    decimal PricePerKm, decimal PricePerTon, decimal SurchargeFragile, decimal SurchargeHazmat, int PriceListId = 0);
public record ContractAppendixLookupsDto(List<ContractOptionDto> Contracts, List<AppendixTariffOptionDto> TariffRules)
{
    public List<ContractOptionDto> PriceLists { get; init; } = new();
}
