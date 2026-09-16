namespace TruckingSystem.Application.Customers;

public interface ICustomerService
{
    Task<List<CustomerListItemDto>> GetAllAsync();
    Task<CustomerListItemDto> CreateAsync(CreateCustomerRequest request);
}
