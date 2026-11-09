namespace TruckingSystem.Application.Customers;

public interface ICustomerBusinessCardReader
{
    Task<CustomerBusinessCardInfo> ReadAsync(byte[] image, string contentType, CancellationToken cancellationToken = default);
}
