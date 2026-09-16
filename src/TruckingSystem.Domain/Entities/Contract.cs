using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

public class Contract : BaseEntity
{
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public string Code { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Terms { get; set; }

    public int? PriceListId { get; set; }
    public PriceList? PriceList { get; set; }
}
