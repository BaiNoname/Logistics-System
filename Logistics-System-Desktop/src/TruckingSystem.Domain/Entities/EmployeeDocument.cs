using TruckingSystem.Domain.Common;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Domain.Entities;

public class EmployeeDocument : BaseEntity
{
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public EmployeeDocumentType DocType { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
}
