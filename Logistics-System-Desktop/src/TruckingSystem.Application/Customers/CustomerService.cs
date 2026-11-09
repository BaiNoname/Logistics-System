using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.Customers;

public class CustomerService : ICustomerService
{
    private readonly IApplicationDbContext _db;

    public CustomerService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<CustomerListItemDto>> GetAllAsync()
    {
        return await _db.Customers
            .OrderBy(c => c.Name)
            .Select(c => new CustomerListItemDto(c.Id, c.Name, c.Phone, c.CustomerType.ToString(), c.TaxCode, c.Address, c.CreditLimit, c.GoogleCalendarId))
            .ToListAsync();
    }

    public async Task<CustomerActionResult> CreateAsync(CreateCustomerRequest request)
    {
        var validation = await ValidateAsync(request.Name, request.CustomerType, request.TaxCode, request.CreditLimit);
        if (validation is not null) return new CustomerActionResult(false, validation);

        var contacts = request.Contacts ?? new List<CreateCustomerContactRequest>();
        if (contacts.Count > 100)
            return new CustomerActionResult(false, "Chỉ được thêm tối đa 100 người liên hệ cùng khách hàng.");
        for (var i = 0; i < contacts.Count; i++)
        {
            var contact = contacts[i];
            if (contact is null || string.IsNullOrWhiteSpace(contact.Name))
                return new CustomerActionResult(false, $"Vui lòng nhập tên người liên hệ {i + 1}.");
            if (!CustomerContact.IsValidPhoneNumber(contact.Phone?.Trim()))
                return new CustomerActionResult(false, $"Số điện thoại người liên hệ {i + 1} không hợp lệ.");
        }

        Enum.TryParse<CustomerType>(request.CustomerType, true, out var customerType);
        var customer = new Customer
        {
            Name = request.Name.Trim(),
            Phone = NullIfWhiteSpace(request.Phone),
            TaxCode = NullIfWhiteSpace(request.TaxCode),
            CustomerType = customerType,
            Address = NullIfWhiteSpace(request.Address),
            CreditLimit = request.CreditLimit,
            GoogleCalendarId = NullIfWhiteSpace(request.GoogleCalendarId),
            Contacts = contacts.Select(c => new CustomerContact
            {
                Name = c.Name.Trim(), Phone = c.Phone!.Trim(), Position = NullIfWhiteSpace(c.Position)
            }).ToList(),
        };
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
        return new CustomerActionResult(true, null);
    }

    public async Task<CustomerActionResult> UpdateAsync(int id, UpdateCustomerRequest request)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer is null) return new CustomerActionResult(false, "Không tìm thấy khách hàng.");

        var validation = await ValidateAsync(request.Name, request.CustomerType, request.TaxCode, request.CreditLimit, id);
        if (validation is not null) return new CustomerActionResult(false, validation);

        Enum.TryParse<CustomerType>(request.CustomerType, true, out var customerType);
        customer.Name = request.Name.Trim();
        customer.Phone = NullIfWhiteSpace(request.Phone);
        customer.TaxCode = NullIfWhiteSpace(request.TaxCode);
        customer.CustomerType = customerType;
        customer.Address = NullIfWhiteSpace(request.Address);
        customer.CreditLimit = request.CreditLimit;
        customer.GoogleCalendarId = NullIfWhiteSpace(request.GoogleCalendarId);

        await _db.SaveChangesAsync();
        return new CustomerActionResult(true, null);
    }

    public async Task<List<CustomerContactListItemDto>> GetContactsAsync(int? customerId = null)
    {
        return await _db.CustomerContacts
            .AsNoTracking()
            .Where(c => !customerId.HasValue || c.CustomerId == customerId.Value)
            .OrderBy(c => c.Customer.Name).ThenBy(c => c.Name).ThenBy(c => c.Id)
            .Select(c => new CustomerContactListItemDto(c.Id, c.CustomerId, c.Customer.Name, c.Name, c.Phone, c.Position))
            .ToListAsync();
    }

    public async Task<CustomerActionResult> CreateContactAsync(SaveCustomerContactRequest request)
    {
        var validation = await ValidateContactAsync(request);
        if (validation is not null) return new CustomerActionResult(false, validation);

        _db.CustomerContacts.Add(new CustomerContact
        {
            CustomerId = request.CustomerId,
            Name = request.Name.Trim(),
            Phone = NullIfWhiteSpace(request.Phone),
            Position = NullIfWhiteSpace(request.Position),
        });
        await _db.SaveChangesAsync();
        return new CustomerActionResult(true, null);
    }

    public async Task<CustomerActionResult> UpdateContactAsync(int id, SaveCustomerContactRequest request)
    {
        var contact = await _db.CustomerContacts.FindAsync(id);
        if (contact is null) return new CustomerActionResult(false, "Không tìm thấy người liên hệ.");

        var validation = await ValidateContactAsync(request);
        if (validation is not null) return new CustomerActionResult(false, validation);

        contact.CustomerId = request.CustomerId;
        contact.Name = request.Name.Trim();
        contact.Phone = NullIfWhiteSpace(request.Phone);
        contact.Position = NullIfWhiteSpace(request.Position);
        contact.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return new CustomerActionResult(true, null);
    }

    public async Task<CustomerActionResult> DeleteContactAsync(int id)
    {
        var contact = await _db.CustomerContacts.FindAsync(id);
        if (contact is null) return new CustomerActionResult(false, "Không tìm thấy người liên hệ.");

        _db.CustomerContacts.Remove(contact);
        await _db.SaveChangesAsync();
        return new CustomerActionResult(true, null);
    }

    private async Task<string?> ValidateContactAsync(SaveCustomerContactRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return "Tên người liên hệ không được để trống.";
        if (request.CustomerId <= 0 || !await _db.Customers.AnyAsync(c => c.Id == request.CustomerId))
            return "Khách hàng không tồn tại.";
        return null;
    }

    private async Task<string?> ValidateAsync(string name, string customerType, string? taxCode, decimal? creditLimit, int? excludedId = null)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Tên khách hàng không được để trống.";
        if (!Enum.TryParse<CustomerType>(customerType, true, out _)) return "Loại khách hàng không hợp lệ.";
        if (creditLimit < 0) return "Hạn mức tín dụng không được âm.";

        var normalizedTaxCode = NullIfWhiteSpace(taxCode);
        if (normalizedTaxCode is not null && await _db.Customers.AnyAsync(c =>
                c.TaxCode == normalizedTaxCode && (!excludedId.HasValue || c.Id != excludedId.Value)))
            return "Mã số thuế đã tồn tại.";

        return null;
    }

    private static string? NullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
