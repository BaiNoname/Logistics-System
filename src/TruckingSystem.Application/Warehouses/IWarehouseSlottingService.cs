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
}
