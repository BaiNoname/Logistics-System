using TruckingSystem.Domain.Common;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Domain.Entities;

public class DriverSchedule : BaseEntity
{
    public int DriverEmployeeId { get; set; }
    public Employee DriverEmployee { get; set; } = null!;

    public DateTime ShiftDate { get; set; }
    public ShiftType ShiftType { get; set; }
}
