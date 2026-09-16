using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

public class CustomerContact : BaseEntity
{
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Position { get; set; }
}
