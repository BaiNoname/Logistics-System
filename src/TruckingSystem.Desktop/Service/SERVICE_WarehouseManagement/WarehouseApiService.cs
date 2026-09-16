using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_WarehouseManagement;

namespace TruckingSystem.Desktop.SERVICE_WarehouseManagement;

public class WarehouseApiService
{
    private readonly ApiConnection _conn;

    public WarehouseApiService(ApiConnection conn)
    {
        _conn = conn;
    }

    public Task<List<WarehouseListItem>> GetWarehousesAsync()
        => _conn.GetAsync<List<WarehouseListItem>>("/api/warehouses")!;

    public Task<List<WarehouseZoneItem>> GetZonesAsync(int warehouseId)
        => _conn.GetAsync<List<WarehouseZoneItem>>($"/api/warehouses/{warehouseId}/zones")!;

    public Task<List<StorageBinItem>> GetBinsAsync(int zoneId)
        => _conn.GetAsync<List<StorageBinItem>>($"/api/warehouses/zones/{zoneId}/bins")!;

    public async Task<SuggestBinResult> SuggestBinAsync(int warehouseId, int cargoLineId, decimal qty)
    {
        var (_, body) = await _conn.PostRawAsync($"/api/warehouses/{warehouseId}/suggest-bin",
            new { cargoLineId, qtyToStore = qty });
        return Newtonsoft.Json.JsonConvert.DeserializeObject<SuggestBinResult>(body)
               ?? new SuggestBinResult { Found = false, Message = "Không đọc được phản hồi." };
    }
}
