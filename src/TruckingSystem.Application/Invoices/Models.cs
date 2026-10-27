namespace TruckingSystem.Application.Invoices;

public record InvoiceListItemDto(int Id, string OrderCode, string CustomerName, decimal TotalAmount, string Status, DateTime IssueDate);

public record InvoiceOrderOptionDto(int Id, string Code, string CustomerName, string PickupPoint, string DropoffPoint);

public record CreateInvoiceRequest(int TransportOrderId, decimal Amount, DateOnly? DueDate);

public record InvoiceQuoteLineDto(string Description, decimal Quantity, decimal UnitPrice, decimal Amount);
public record InvoiceQuoteDto(int TransportOrderId, string ContractCode, string? AppendixCode,
    DateTime PricingDate, string VehicleType, decimal? DistanceKm, decimal WeightTons,
    decimal? EstimatedFuelLiters, decimal OtherCosts, decimal TotalAmount,
    List<InvoiceQuoteLineDto> Lines, string? Note);
