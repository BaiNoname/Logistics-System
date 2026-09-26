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

/// <summary>Kết quả đồng bộ Google Calendar — báo riêng từng bên (tài xế/khách hàng).</summary>
public class CalendarSyncResultItem
{
    public bool DriverSynced { get; set; }
    public string? DriverMessage { get; set; }
    public bool CustomerSynced { get; set; }
    public string? CustomerMessage { get; set; }
}

/// <summary>Chi phí phát sinh KHÁC ngoài xăng dầu (phí cầu đường, bến bãi,
/// bốc xếp, phạt...) cho 1 chuyến.</summary>
public class TripExpenseItem
{
    public int Id { get; set; }
    public string Type { get; set; } = "";
    public decimal Amount { get; set; }
    public string? Note { get; set; }
}

/// <summary>1 điểm dừng trong lộ trình — kèm nhãn giải thích rõ đây là điểm gì.</summary>
public class RouteStopItem
{
    public double? Lat { get; set; }
    public double? Lng { get; set; }
    public string Label { get; set; } = "";
}

/// <summary>Lộ trình đầy đủ của 1 chặng — chặng Lấy hàng có 3 điểm dừng
/// (Kho→Đối tác→Kho, round-trip thật vì xe phải quay về), chặng Giao hàng
/// chỉ 2 điểm (1 chiều, xong việc cho đơn này).</summary>
public class TripRouteItem
{
    public string OrderCode { get; set; } = "";
    public string LegType { get; set; } = "";
    public List<RouteStopItem> Stops { get; set; } = new();
}

/// <summary>Kết quả tính + LƯU LẠI ước tính khoảng cách/thời gian từ VietMap
/// cho 1 chuyến — dùng cho thống kê so sánh Ước tính vs Thực tế sau này.</summary>
public class TripRouteEstimateItem
{
    public decimal DistanceKm { get; set; }
    public int DurationMinutes { get; set; }
    public string? Message { get; set; }
}