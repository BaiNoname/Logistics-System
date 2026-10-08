namespace TruckingSystem.Application.Warehouses;

/// <summary>
/// Use case tối ưu vị trí lưu trữ trong kho — cài đặt thuật toán Best-Fit.
/// Đây là 1 trong 2 thuật toán lõi của hệ thống (cùng với Greedy chia xe).
/// </summary>
public interface IWarehouseSlottingService
{
    /// <summary>Đề xuất vị trí lưu trữ (StorageBin) phù hợp nhất cho 1 lô hàng sắp nhập kho.</summary>
    Task<SuggestBinResponse> SuggestBinAsync(int warehouseId, SuggestBinRequest request);

    Task<List<WarehouseZoneDto>> GetZonesAsync(int warehouseId);
    Task<List<StorageBinDto>> GetBinsAsync(int zoneId);
    Task<List<WarehouseListItemDto>> GetWarehousesAsync();

    /// <summary>Tạo kho mới — cho phép nhiều kho ở nhiều địa điểm khác nhau.</summary>
    Task<WarehouseListItemDto> CreateWarehouseAsync(CreateWarehouseRequest request);

    /// <summary>Sửa thông tin kho — cho sửa cả kích thước/tọa độ (ảnh hưởng sơ đồ + lộ trình sau này).</summary>
    Task<WarehouseActionResult> UpdateWarehouseAsync(int warehouseId, UpdateWarehouseRequest request);

    /// <summary>Tạo khu vực mới trong 1 kho.</summary>
    Task<WarehouseZoneDto> CreateZoneAsync(int warehouseId, CreateZoneRequest request);

    /// <summary>Sửa khu vực — cho sửa cả tọa độ/kích thước trên sơ đồ.</summary>
    Task<WarehouseActionResult> UpdateZoneAsync(int zoneId, CreateZoneRequest request);

    /// <summary>Tạo vị trí lưu trữ mới trong 1 khu vực.</summary>
    Task<StorageBinDto> CreateBinAsync(int zoneId, CreateBinRequest request);

    /// <summary>Sửa vị trí lưu trữ — cho sửa cả Row/Column/khoảng cách cửa.
    /// CHẶN nếu vị trí đang chứa hàng và sức chứa mới nhỏ hơn lượng đang chứa.</summary>
    Task<WarehouseActionResult> UpdateBinAsync(int binId, CreateBinRequest request);

    /// <summary>Xóa kho — CHẶN nếu kho đang có tồn kho thật (WarehouseStock > 0),
    /// tránh mất dấu vết hàng hóa thật đang ở đó. Tự xóa hết Zone/Bin bên trong.</summary>
    Task<WarehouseActionResult> DeleteWarehouseAsync(int warehouseId);

    /// <summary>Xóa khu vực — CHẶN nếu còn StorageBin đang chứa hàng (CurrentUsedM3 > 0).</summary>
    Task<WarehouseActionResult> DeleteZoneAsync(int zoneId);

    /// <summary>Xóa vị trí lưu trữ — CHẶN nếu đang chứa hàng (CurrentUsedM3 > 0).</summary>
    Task<WarehouseActionResult> DeleteBinAsync(int binId);

    /// <summary>Nhập kho THẬT SỰ — tạo GoodsReceipt, cộng WarehouseStock. Nếu
    /// chosenBinId CÓ giá trị → dùng ĐÚNG vị trí đó (do người dùng tự chọn tay,
    /// bỏ qua Best-Fit); nếu null → tự động chạy Best-Fit như trước.</summary>
    Task<ReceiveGoodsResult> ReceiveGoodsAsync(int warehouseId, int transportOrderId, int cargoLineId,
        decimal qty, int? receivedByEmployeeId, int? tripId, int? chosenBinId = null, string? performedByUserName = null);

    /// <summary>Danh sách vị trí (StorageBin) còn đủ chỗ trong 1 kho, ĐÚNG khu
    /// vực phù hợp với tính chất của CargoLine — dùng cho màn Nhập kho thủ
    /// công để người dùng tự chọn tay thay vì để hệ thống tự động chọn.</summary>
    Task<List<StorageBinDto>> GetAvailableBinsForCargoAsync(int warehouseId, int cargoLineId);

    /// <summary>Xuất kho THẬT SỰ — tạo GoodsIssue, trừ dần StorageAssignment
    /// (FIFO), trừ WarehouseStock. tripId = null nếu xuất kho THỦ CÔNG (không
    /// gắn chuyến nào).</summary>
    Task<IssueGoodsResult> IssueGoodsAsync(int warehouseId, int transportOrderId, int cargoLineId,
        decimal qty, int? issuedByEmployeeId, int? tripId, string? performedByUserName = null);

    /// <summary>Lấy danh sách CargoLine của 1 đơn — dùng cho màn Nhập/Xuất kho
    /// thủ công (chọn đúng dòng hàng cần nhập/xuất).</summary>
    Task<List<CargoLineOptionDto>> GetCargoLinesForOrderAsync(int transportOrderId, int warehouseId);

    /// <summary>Lịch sử nhập/xuất kho (gộp cả GoodsReceipt + GoodsIssue), sắp
    /// xếp mới nhất trước — dùng cho màn Nhập/Xuất kho thủ công xem lại.</summary>
    Task<List<WarehouseTransactionDto>> GetTransactionHistoryAsync(int warehouseId);

    /// <summary>Nhân viên gắn với tài khoản đăng nhập (null nếu chưa gắn).</summary>
    Task<int?> ResolveEmployeeIdAsync(string? userId);

    /// <summary>Xuất kho THỦ CÔNG — chặn xuất phần hàng đang giữ cho chuyến Giao/Trả sắp chạy.</summary>
    Task<IssueGoodsResult> IssueManualAsync(int warehouseId, int transportOrderId, int cargoLineId, decimal qty, StockPerformer performer);

    /// <summary>Hoàn tác 1 phiếu xuất THỦ CÔNG làm nhầm — trả hàng lại vào kho, giữ phiếu để lưu vết.</summary>
    Task<WarehouseActionResult> ReverseManualIssueAsync(int issueId, string? userName);

    /// <summary>Toàn cảnh hàng của 1 đơn trong 1 kho: đã nhập/xuất/còn/đang giữ + tiến độ từng chuyến.</summary>
    Task<OrderStockFlowDto?> GetOrderStockFlowAsync(int transportOrderId, int warehouseId);

    /// <summary>Chỉ các đơn liên quan tới 1 kho (chọn kho này / có chuyến qua kho / đã nhập-xuất tại kho).</summary>
    Task<List<WarehouseOrderDto>> GetOrdersForWarehouseAsync(int warehouseId);

    /// <summary>Gợi ý vị trí cho màn kiểm tra Best-Fit — kiểm tra số lượng như nhập kho thật, không ghi gì.</summary>
    Task<SuggestBinResponse> PreviewSuggestBinAsync(int warehouseId, SuggestBinRequest request);
}
