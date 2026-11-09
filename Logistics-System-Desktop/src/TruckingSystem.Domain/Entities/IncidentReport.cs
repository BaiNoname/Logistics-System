using TruckingSystem.Domain.Common;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Domain.Entities;

public class IncidentReport : BaseEntity
{
    public int TripId { get; set; }
    public Trip Trip { get; set; } = null!;

    public IncidentType Type { get; set; }
    public string? Description { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public decimal? Cost { get; set; }
}
