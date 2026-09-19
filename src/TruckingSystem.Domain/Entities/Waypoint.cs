using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

/// <summary>Lịch sử ping GPS, ghi mỗi 30s từ app tài xế.</summary>
public class Waypoint : BaseEntity
{
    public int TripId { get; set; }
    public Trip Trip { get; set; } = null!;

    public double Lat { get; set; }
    public double Lng { get; set; }
    public decimal? SpeedKmh { get; set; }
    public decimal? Accuracy { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Note { get; set; } // "gap alert", "kẹt xe", "cấm tải"...

    // Chỉ điền khi phát hiện nghi ngờ chậm trễ tại điểm này (không gọi API
    // thời tiết cho MỌI điểm ping — tốn API call vô ích vì đa số sẽ NULL).
    public string? WeatherCondition { get; set; }  // "Mưa to", "Giông bão"...
    public bool? IsSevereWeather { get; set; }      // true nếu đủ xấu để coi là lý do chậm trễ
}