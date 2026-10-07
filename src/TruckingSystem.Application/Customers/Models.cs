namespace TruckingSystem.Application.Customers;

public record CustomerListItemDto(int Id, string Name, string? Phone, string CustomerType, string? TaxCode, string? Address, decimal? CreditLimit, string? GoogleCalendarId);

public record CreateCustomerRequest(string Name, string? Phone, string? TaxCode, string CustomerType, string? Address, decimal? CreditLimit, string? GoogleCalendarId);

/// <summary>Sửa thông tin khách hàng — cùng cấu trúc CreateCustomerRequest.</summary>
public record UpdateCustomerRequest(string Name, string? Phone, string? TaxCode, string CustomerType, string? Address, decimal? CreditLimit, string? GoogleCalendarId);

public record CustomerActionResult(bool Success, string? Message);

public record CustomerBusinessCardInfo(
    string? Name,
    string? Phone,
    string? TaxCode,
    string? CustomerType,
    string? Address);
