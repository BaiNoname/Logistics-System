namespace TruckingSystem.Desktop.DTO_CustomerManagement;

public class CustomerListItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Phone { get; set; }
    public string CustomerType { get; set; } = "";
}
