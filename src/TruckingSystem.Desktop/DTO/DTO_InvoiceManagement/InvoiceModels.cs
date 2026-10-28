namespace TruckingSystem.Desktop.DTO_InvoiceManagement
{
    public record InvoiceQuoteLine(string Description, decimal Quantity, decimal UnitPrice, decimal Amount);
    public record InvoiceDetail(int Id, string OrderCode, string CustomerName, decimal TotalAmount,
        List<InvoiceQuoteLine> Lines);
    public record InvoiceQuote(int TransportOrderId, string ContractCode, string? AppendixCode,
        DateTime PricingDate, string VehicleType, decimal? DistanceKm, decimal WeightTons,
        decimal? EstimatedFuelLiters, decimal OtherCosts, decimal TotalAmount,
        List<InvoiceQuoteLine> Lines, string? Note);

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
