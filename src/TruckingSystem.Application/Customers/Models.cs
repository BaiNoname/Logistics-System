namespace TruckingSystem.Application.Customers;

public record CustomerListItemDto(int Id, string Name, string? Phone, string CustomerType, string? TaxCode, string? Address, decimal? CreditLimit, string? GoogleCalendarId);

public record CreateCustomerRequest(string Name, string? Phone, string? TaxCode, string CustomerType, string? Address, decimal? CreditLimit, string? GoogleCalendarId);

/// <summary>Sửa thông tin khách hàng — cùng cấu trúc CreateCustomerRequest.</summary>
public record UpdateCustomerRequest(string Name, string? Phone, string? TaxCode, string CustomerType, string? Address, decimal? CreditLimit, string? GoogleCalendarId);

public record CustomerActionResult(bool Success, string? Message);

public record CustomerContactListItemDto(int Id, int CustomerId, string CustomerName, string Name, string? Phone, string? Position);

public record SaveCustomerContactRequest(int CustomerId, string Name, string? Phone, string? Position);

public record CustomerBusinessCardInfo(
    string? Name,
    string? Phone,
    string? TaxCode,
    string? CustomerType,
    string? Address)
{
    public string? FullName { get; init; }
    public string? Company { get; init; }
    public string? Title { get; init; }
    public string? Email { get; init; }
    public string? Website { get; init; }
    public string? RawResponse { get; init; }
    public string? ErrorMessage { get; init; }

    public bool HasExtractedFields() => new[]
        { Name, Phone, TaxCode, CustomerType, Address, FullName, Company, Title, Email, Website }
        .Any(value => !string.IsNullOrWhiteSpace(value));
}
