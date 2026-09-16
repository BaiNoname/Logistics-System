using TruckingSystem.Domain.Common;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Domain.Entities;

public class Vehicle : BaseEntity
{
    public string PlateNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public decimal CapacityQty { get; set; }
    public decimal CapacityKg { get; set; }
    public decimal CapacityM3 { get; set; }
    public VehicleStatus Status { get; set; } = VehicleStatus.Available;

    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public ICollection<VehicleDocument> Documents { get; set; } = new List<VehicleDocument>();
    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
}
