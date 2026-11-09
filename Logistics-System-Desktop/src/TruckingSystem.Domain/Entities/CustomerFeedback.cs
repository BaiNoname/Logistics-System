using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

public class CustomerFeedback : BaseEntity
{
    public int TransportOrderId { get; set; }
    public TransportOrder TransportOrder { get; set; } = null!;

    public int Rating { get; set; } // 1-5
    public string? Comment { get; set; }
}
