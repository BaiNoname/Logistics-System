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
            .Select(c => new CustomerListItemDto(c.Id, c.Name, c.Phone, c.CustomerType.ToString()))
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
        };
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
        return new CustomerListItemDto(customer.Id, customer.Name, customer.Phone, customer.CustomerType.ToString());
    }
}
