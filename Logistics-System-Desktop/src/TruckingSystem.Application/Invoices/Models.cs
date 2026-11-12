namespace TruckingSystem.Application.Invoices;

public record InvoiceListItemDto(int Id, string OrderCode, string CustomerName, decimal TotalAmount, string Status, DateTime IssueDate);

public record InvoiceDetailDto(int Id, string OrderCode, string CustomerName, decimal TotalAmount,
    List<InvoiceQuoteLineDto> Lines)
{
    public string Status { get; init; } = "Unpaid";
    public DateTime IssueDate { get; init; }
    public DateTime? DueDate { get; init; }
    public int CustomerId { get; init; }
    public string? CustomerTaxCode { get; init; }
    public string? CustomerPhone { get; init; }
    public string? CustomerAddress { get; init; }
    public int TransportOrderId { get; init; }
    public string? ContractCode { get; init; }
    public string? PickupPoint { get; init; }
    public string? DropoffPoint { get; init; }
    public List<InvoicePaymentDto> Payments { get; init; } = new();
}

public record InvoicePaymentDto(int Id, DateTime PaymentDate, decimal Amount, string Method);

public record InvoiceOrderOptionDto(int Id, string Code, string CustomerName, string PickupPoint, string DropoffPoint);

public record CreateInvoiceRequest(int TransportOrderId, decimal Amount, DateOnly? DueDate);

public record InvoiceQuoteLineDto(string Description, decimal Quantity, decimal UnitPrice, decimal Amount);
public record InvoiceQuoteDto(int TransportOrderId, string ContractCode, string? AppendixCode,
    DateTime PricingDate, string VehicleType, decimal? DistanceKm, decimal WeightTons,
    decimal? EstimatedFuelLiters, decimal OtherCosts, decimal TotalAmount,
    List<InvoiceQuoteLineDto> Lines, string? Note, bool StorageFeePending = false);
