namespace TruckingSystem.Application.Customers;

public record CustomerListItemDto(int Id, string Name, string? Phone, string CustomerType);
public record CreateCustomerRequest(string Name, string? Phone, string? TaxCode, string CustomerType);
