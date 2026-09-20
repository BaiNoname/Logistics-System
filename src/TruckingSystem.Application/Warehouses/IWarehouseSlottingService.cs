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

    /// <summary>Tạo khu vực mới trong 1 kho.</summary>
    Task<WarehouseZoneDto> CreateZoneAsync(int warehouseId, CreateZoneRequest request);

    /// <summary>Tạo vị trí lưu trữ mới trong 1 khu vực.</summary>
    Task<StorageBinDto> CreateBinAsync(int zoneId, CreateBinRequest request);

    /// <summary>Xóa kho — CHẶN nếu kho đang có tồn kho thật (WarehouseStock > 0),
    /// tránh mất dấu vết hàng hóa thật đang ở đó. Tự xóa hết Zone/Bin bên trong.</summary>
    Task<WarehouseActionResult> DeleteWarehouseAsync(int warehouseId);

    /// <summary>Xóa khu vực — CHẶN nếu còn StorageBin đang chứa hàng (CurrentUsedM3 > 0).</summary>
    Task<WarehouseActionResult> DeleteZoneAsync(int zoneId);

    /// <summary>Xóa vị trí lưu trữ — CHẶN nếu đang chứa hàng (CurrentUsedM3 > 0).</summary>
    Task<WarehouseActionResult> DeleteBinAsync(int binId);

    /// <summary>Nhập kho THẬT SỰ — tạo GoodsReceipt, tự chạy Best-Fit chọn vị
    /// trí, cộng WarehouseStock. Dùng chung bởi VehicleAllocationService
    /// (SelfDelivery) và TripService (chặng Pickup hoàn tất).</summary>
    Task<ReceiveGoodsResult> ReceiveGoodsAsync(int warehouseId, int transportOrderId, int cargoLineId,
        decimal qty, int receivedByEmployeeId, int? tripId);

    /// <summary>Xuất kho THẬT SỰ — tạo GoodsIssue, trừ dần StorageAssignment
    /// (FIFO), trừ WarehouseStock. Dùng bởi TripService khi chặng Delivery bắt đầu.</summary>
    Task<IssueGoodsResult> IssueGoodsAsync(int warehouseId, int transportOrderId, int cargoLineId,
        decimal qty, int issuedByEmployeeId, int tripId);
}