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

    public async Task<CustomerListItemDto> CreateAsync(CreateCustomerRequest request)
    {
        var customer = new Customer
        {
            Name = request.Name,
            Phone = request.Phone,
            TaxCode = request.TaxCode,
            CustomerType = request.CustomerType == "Company" ? CustomerType.Company : CustomerType.Individual,
            Address = request.Address,
            CreditLimit = request.CreditLimit,
            GoogleCalendarId = request.GoogleCalendarId,
        };
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
        return new CustomerListItemDto(customer.Id, customer.Name, customer.Phone, customer.CustomerType.ToString(), customer.TaxCode, customer.Address, customer.CreditLimit, customer.GoogleCalendarId);
    }

    public async Task<CustomerActionResult> UpdateAsync(int id, UpdateCustomerRequest request)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer is null) return new CustomerActionResult(false, "Không tìm thấy khách hàng.");

        customer.Name = request.Name;
        customer.Phone = request.Phone;
        customer.TaxCode = request.TaxCode;
        customer.CustomerType = request.CustomerType == "Company" ? CustomerType.Company : CustomerType.Individual;
        customer.Address = request.Address;
        customer.CreditLimit = request.CreditLimit;
        customer.GoogleCalendarId = request.GoogleCalendarId;

        await _db.SaveChangesAsync();
        return new CustomerActionResult(true, null);
    }
}
