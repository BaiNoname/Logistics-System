namespace TruckingSystem.Application.TransportOrders;

public interface ITransportOrderService
{
    Task<List<TransportOrderListItemDto>> GetAllAsync();
    Task<TransportOrderDetailDto?> GetByIdAsync(int id);
    Task<TransportOrderDetailDto> CreateAsync(CreateTransportOrderRequest request);

    /// <summary>Sửa đơn — CHỈ áp dụng được khi đơn còn Draft.</summary>
    Task<OrderActionResult> UpdateAsync(int id, UpdateTransportOrderRequest request);

    /// <summary>Draft -> Confirmed. Yêu cầu đơn phải có ít nhất 1 dòng hàng.</summary>
    Task<OrderActionResult> ConfirmAsync(int id);

    /// <summary>-> Cancelled. CHỈ cho phép khi đơn còn Draft hoặc Confirmed
    /// (chưa có xe chạy) — không cho hủy khi đã InProgress/Done.</summary>
    Task<OrderActionResult> CancelAsync(int id);
}