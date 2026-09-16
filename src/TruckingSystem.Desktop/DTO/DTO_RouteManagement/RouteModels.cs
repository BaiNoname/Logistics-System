namespace TruckingSystem.Desktop.DTO_RouteManagement;

public class RoutePointItem
{
    public double Lat { get; set; }
    public double Lng { get; set; }
}

public class RouteResultItem
{
    public bool Success { get; set; }
    public double DistanceKm { get; set; }
    public int DurationMinutes { get; set; }
    public List<RoutePointItem> Points { get; set; } = new();
    public string? TileUrlTemplate { get; set; }
    public string? Message { get; set; }
}
