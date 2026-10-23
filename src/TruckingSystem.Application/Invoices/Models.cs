namespace TruckingSystem.Application.Invoices;

public record InvoiceListItemDto(int Id, string OrderCode, string CustomerName, decimal TotalAmount, string Status, DateTime IssueDate);

public record InvoiceOrderOptionDto(int Id, string Code, string CustomerName, string PickupPoint, string DropoffPoint);

public record CreateInvoiceRequest(int TransportOrderId, decimal Amount, DateOnly? DueDate);
