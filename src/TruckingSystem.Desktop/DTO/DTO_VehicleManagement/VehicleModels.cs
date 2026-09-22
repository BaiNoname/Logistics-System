namespace TruckingSystem.Desktop.DTO_VehicleManagement
{
    public class VehicleListItem
    {
        public int Id { get; set; }
        public string PlateNumber { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public decimal CapacityQty { get; set; }
        public decimal CapacityKg { get; set; }
        public decimal CapacityM3 { get; set; }
        public string Status { get; set; } = "";
    }

    public class VehicleActionResultItem
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    public class VehicleDocumentItem
    {
        public int Id { get; set; }
        public string DocType { get; set; } = "";
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string? FileUrl { get; set; }
    }
}