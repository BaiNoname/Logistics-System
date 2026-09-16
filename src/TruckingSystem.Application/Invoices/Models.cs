namespace TruckingSystem.Application.Invoices;

public record InvoiceListItemDto(int Id, string OrderCode, string CustomerName, decimal TotalAmount, string Status, DateTime IssueDate);
