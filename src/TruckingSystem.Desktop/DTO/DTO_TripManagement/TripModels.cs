namespace TruckingSystem.Desktop.DTO_TripManagement;

public class TripListItem
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = "";
    public string VehiclePlate { get; set; } = "";
    public string DriverName { get; set; } = "";
    public string Status { get; set; } = "";
    public string LegType { get; set; } = "";
    public decimal TotalQtyPlanned { get; set; }
    public double? FromLat { get; set; }
    public double? FromLng { get; set; }
    public double? ToLat { get; set; }
    public double? ToLng { get; set; }
}

public class TripActionResultItem
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}

/// <summary>1 điểm GPS thật đã ghi nhận — dùng để vẽ đường đi thực tế.</summary>
public class WaypointItem
{
    public double Lat { get; set; }
    public double Lng { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal? SpeedKmh { get; set; }
}

/// <summary>So sánh chi phí xăng ƯỚC TÍNH (từ GPS thật) với chi phí THỰC TẾ đã ghi.</summary>
public class FuelEstimateItem
{
    public decimal ActualDistanceKm { get; set; }
    public decimal? EstimatedLiters { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal ActualFuelCost { get; set; }
    public decimal? VarianceCost { get; set; }
    public string? Message { get; set; }
}

/// <summary>Sự cố giữa đường ghi nhận cho 1 chuyến (tai nạn/hỏng xe/vi phạm/khác).</summary>
public class IncidentReportItem
{
    public int Id { get; set; }
    public string Type { get; set; } = "";
    public string? Description { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal? Cost { get; set; }
}