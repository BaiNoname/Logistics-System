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
    public int CustomerId { get; set; }
    public string Code { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Terms { get; set; }
    public string? PriceListName { get; set; }
    public int? PriceListId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ContractLookups
{
    public List<ContractOption> Customers { get; set; } = new();
    public List<ContractOption> PriceLists { get; set; } = new();
}

public class ContractOption
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}
