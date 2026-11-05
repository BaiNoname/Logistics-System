namespace TruckingSystem.Desktop.DTO_ContractManagement;

public record ContractAppendixItem(int Id, string Code, int ContractId, string ContractCode,
    DateTime IssueDate, DateTime EffectiveDate, AppendixTariffOption? TariffRule,
    decimal? DiscountPercent = null, int? PriceListId = null, string? PriceListName = null,
    string Status = "Active", string ContractStatus = "Active")
{
    public string TariffName => PriceListName ?? TariffRule?.PriceListName ?? "—";
    public string StatusText => Status == "Active" ? "Đang áp dụng" : "Ngừng áp dụng";
    public string ContractStatusText => new ContractListItem { Status = ContractStatus }.StatusText;
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
    public List<AppendixContractOption> ContractDiscounts { get; init; } = new();
    public int MaxDiscountPercent { get; init; }
}

public record AppendixContractOption(int Id, string? Type, decimal? DiscountPercent);
