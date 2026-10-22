namespace TruckingSystem.Application.TransportOrders;

public interface ITransportOrderService
{
    Task<List<TransportOrderListItemDto>> GetAllAsync();

    /// <summary>Gợi ý "Loại hàng" khi tạo đơn: danh mục chuẩn + các loại đã dùng.</summary>
    Task<List<string>> GetCargoTypeSuggestionsAsync();
    Task<TransportOrderDetailDto?> GetByIdAsync(int id);

    /// <summary>Hợp đồng chọn được khi tạo / sửa đơn: của đúng khách, đang hiệu lực hôm nay.</summary>
    Task<List<OrderContractOptionDto>> GetContractOptionsAsync(int customerId);
    Task<TransportOrderDetailDto> CreateAsync(CreateTransportOrderRequest request);

    /// <summary>Sửa đơn — CHỈ áp dụng được khi đơn còn Draft.</summary>
    Task<OrderActionResult> UpdateAsync(int id, UpdateTransportOrderRequest request);

    /// <summary>Draft -> Confirmed. Yêu cầu đơn phải có ít nhất 1 dòng hàng.</summary>
    Task<OrderActionResult> ConfirmAsync(int id);

    /// <summary>-> Cancelled. CHỈ cho phép khi đơn còn Draft hoặc Confirmed
    /// (chưa có xe chạy) — không cho hủy khi đã InProgress/Done.</summary>
    Task<OrderActionResult> CancelAsync(int id);

    /// <summary>Đổi / bổ sung hạn giao cho đơn Đã xác nhận hoặc Đang chạy (đơn Nháp sửa qua
    /// Sửa đơn). Bắt buộc lý do, không chọn ngày đã qua, ghi nhật ký người đổi.</summary>
    Task<OrderActionResult> ChangeDeadlineAsync(int id, ChangeDeadlineRequest request, string? userId, string? userName);
}