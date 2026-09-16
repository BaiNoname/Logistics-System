namespace TruckingSystem.Desktop.DTO_LocationManagement;

public class LocationItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public double? Lat { get; set; }
    public double? Lng { get; set; }

    // Hiện tên trong ComboBox mà không cần set DisplayMember riêng
    public override string ToString() => Name;
}
