namespace TruckingSystem.Desktop.DTO_ContractManagement;

public record ContractAppendixItem(int Id, string Code, int ContractId, string ContractCode,
    DateTime IssueDate, DateTime EffectiveDate, AppendixTariffOption TariffRule)
{
    public string TariffName => TariffRule.Name;
}

public record AppendixTariffOption(int Id, string PriceListName, string VehicleType,
    decimal PricePerKm, decimal PricePerTon, int PriceListId = 0)
{
    public string Name => $"{PriceListName} · {VehicleType} · {PricePerKm:N2} đ/km · {PricePerTon:N2} đ/tấn";
    public string Details => $"Giá/km: {PricePerKm:N2} đ · Giá/tấn: {PricePerTon:N2} đ";
}

public record ContractAppendixLookups(List<ContractOption> Contracts, List<AppendixTariffOption> TariffRules)
{
    public List<ContractOption> PriceLists { get; init; } = new();
}
