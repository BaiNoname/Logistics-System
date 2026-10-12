namespace TruckingSystem.Application.Customers;

public interface ICustomerService
{
    Task<List<CustomerListItemDto>> GetAllAsync();
    Task<CustomerActionResult> CreateAsync(CreateCustomerRequest request);

    /// <summary>Sửa thông tin khách hàng đã có.</summary>
    Task<CustomerActionResult> UpdateAsync(int id, UpdateCustomerRequest request);

    Task<List<CustomerContactListItemDto>> GetContactsAsync(int? customerId = null);
    Task<CustomerActionResult> CreateContactAsync(SaveCustomerContactRequest request);
    Task<CustomerActionResult> UpdateContactAsync(int id, SaveCustomerContactRequest request);
    Task<CustomerActionResult> DeleteContactAsync(int id);
}
