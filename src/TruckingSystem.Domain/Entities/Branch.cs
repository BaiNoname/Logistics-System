using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

public class Branch : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public ICollection<TransportOrder> TransportOrders { get; set; } = new List<TransportOrder>();
}
