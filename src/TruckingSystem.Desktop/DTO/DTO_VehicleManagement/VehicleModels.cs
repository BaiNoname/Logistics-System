namespace TruckingSystem.Desktop.DTO_VehicleManagement;

public class VehicleListItem
{
    public int Id { get; set; }
    public string PlateNumber { get; set; } = "";
    public string VehicleType { get; set; } = "";
    public decimal CapacityQty { get; set; }
    public string Status { get; set; } = "";
}
