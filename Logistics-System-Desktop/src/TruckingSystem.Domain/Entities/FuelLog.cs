using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

/// <summary>Đổ xăng thực tế mỗi chuyến — khác FuelPriceHistory (chỉ là bảng giá tham chiếu).</summary>
public class FuelLog : BaseEntity
{
    public int TripId { get; set; }
    public Trip Trip { get; set; } = null!;

    public decimal Liters { get; set; }
    public decimal Cost { get; set; }
    public string? StationName { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
