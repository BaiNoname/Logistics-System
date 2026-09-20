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
}

public class TripActionResultItem
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}