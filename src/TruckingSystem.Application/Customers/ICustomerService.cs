namespace TruckingSystem.Application.Customers;

public interface ICustomerService
{
    Task<List<CustomerListItemDto>> GetAllAsync();
    Task<CustomerListItemDto> CreateAsync(CreateCustomerRequest request);

    /// <summary>Sửa thông tin khách hàng đã có.</summary>
    Task<CustomerActionResult> UpdateAsync(int id, UpdateCustomerRequest request);
}
