namespace TruckingSystem.Desktop.DTO_InvoiceManagement
{
    public class InvoiceOrderOption
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public string PickupPoint { get; set; } = "";
        public string DropoffPoint { get; set; } = "";
        public string DisplayName => $"{Code} — {CustomerName}";
    }

    public class InvoiceListItem
    {
        public int Id { get; set; }
        public string OrderCode { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "";
        public DateTime IssueDate { get; set; }
    }
}
