using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

public class Position : BaseEntity
{
    public string Name { get; set; } = string.Empty; // Tài xế, Điều phối viên, Kế toán...
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
