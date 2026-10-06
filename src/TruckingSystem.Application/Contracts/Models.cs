namespace TruckingSystem.Application.Contracts;

public class ContractListItemDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Terms { get; set; }
    public string? PriceListName { get; set; }
    public int? PriceListId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ContractLookupDto
{
    public List<ContractOptionDto> Customers { get; set; } = new();
    public List<ContractOptionDto> PriceLists { get; set; } = new();
}

public class ContractOptionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class SaveContractRequest
{
    public int CustomerId { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Terms { get; set; }
    public int? PriceListId { get; set; }
}
