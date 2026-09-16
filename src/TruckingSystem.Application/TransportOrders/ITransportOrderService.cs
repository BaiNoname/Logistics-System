namespace TruckingSystem.Application.TransportOrders;

public interface ITransportOrderService
{
    Task<List<TransportOrderListItemDto>> GetAllAsync();
    Task<TransportOrderDetailDto?> GetByIdAsync(int id);
    Task<TransportOrderDetailDto> CreateAsync(CreateTransportOrderRequest request);
}
