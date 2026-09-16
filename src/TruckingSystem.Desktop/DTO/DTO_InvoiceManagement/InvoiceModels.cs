namespace TruckingSystem.Desktop.DTO_InvoiceManagement;

public class InvoiceListItem
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "";
    public DateTime IssueDate { get; set; }
}
