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

    /// <summary>Phí lưu kho ước tính nếu đổi hạn giao (hiện trên hộp Đổi hạn giao trước khi lưu).</summary>
    Task<StorageFeePreviewDto?> PreviewStorageFeeAsync(int id, DateTime newDate, string? requestedBy);

    /// <summary>Phí lưu kho của đơn do KHÁCH dời hạn giao (từng lần đổi + tổng) — đưa vào hóa đơn.</summary>
    Task<StorageFeeDto?> GetStorageFeeAsync(int id);

    /// <summary>Ghi nhận BỔ SUNG phí lưu kho do khách (không đổi hạn giao) — cả đơn đã Hoàn tất / Hủy.</summary>
    Task<OrderActionResult> RecordStorageFeeAsync(int id, RecordStorageFeeRequest request, string? userId, string? userName);
    /// <summary>Hàng nằm kho quá hạn giao: chọn do khách (tính phí) hay do công ty (0 đ).</summary>
    Task<OrderActionResult> SetStorageResponsibilityAsync(int id, StorageResponsibilityRequest request, string? userId, string? userName);
}