namespace TruckingSystem.Desktop.DTO_ContractManagement;

public class ContractListItem
{
    public string Status { get; set; } = "";
    public string StatusText => Status switch
    {
        "Upcoming" => "Chưa hiệu lực",
        "Active" => "Đang hiệu lực",
        "Expired" => "Hết hạn",
        _ => "—",
    };

    public int Id { get; set; }
    public string? Type { get; set; }
    public decimal? DiscountPercent { get; set; }
    public string TypeText => Type switch
    {
        "OneTime" => "Hợp đồng vận chuyển một lần",
        "LongTerm" => "Hợp đồng vận chuyển dài hạn",
        _ => "Chưa phân loại",
    };
    public int CustomerId { get; set; }
    public string Code { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Terms { get; set; }
    public string? PriceListName { get; set; }
    public int? PriceListId { get; set; }
    public TruckingSystem.Desktop.DTO_PricingManagement.TariffRuleItem? TariffRule { get; set; }
    public List<string> AppendixCodes { get; set; } = new();
    public string AppendixText => AppendixCodes.FirstOrDefault() ?? "Chưa có";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ContractLookups
{
    public int MaxDiscountPercent { get; set; } = 20;
    public List<ContractOption> Customers { get; set; } = new();
    public List<ContractOption> PriceLists { get; set; } = new();
    public List<TruckingSystem.Desktop.DTO_PricingManagement.TariffRuleItem> TariffRules { get; set; } = new();
}

public class ContractOption
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}
