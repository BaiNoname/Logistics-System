namespace TruckingSystem.Desktop.DTO_PricingManagement;

public record PriceListItem(int Id, string Name, DateTime EffectiveDate, int RuleCount, int ContractCount, string Status = "Active")
{
    public string StatusText => Status switch
    {
        "Active" => "Đang áp dụng",
        "Pending" => "Chưa áp dụng",
        "Cancelled" => "Đã hủy",
        "Inactive" or "Expired" => "Ngưng áp dụng",
        _ => Status,
    };
}
public record TariffRuleItem(int Id, int PriceListId, string VehicleType, decimal PricePerKm,
    decimal PricePerTon)
{
    public string DisplayName => $"{VehicleType} · {PricePerKm:N2} đ/km · {PricePerTon:N2} đ/tấn";
    public string Details => $"Giá/km: {PricePerKm:N2} đ · Giá/tấn: {PricePerTon:N2} đ";
}

public record SaveTariffRuleRow(int? Id, string VehicleType, decimal PricePerKm, decimal PricePerTon);
