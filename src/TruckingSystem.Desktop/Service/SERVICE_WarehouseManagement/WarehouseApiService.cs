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

    /// <summary>Tạo kho mới — cho phép có nhiều kho ở nhiều địa điểm (TP.HCM, Hà Nội, Đà Nẵng...).</summary>
    public Task<WarehouseListItem> CreateWarehouseAsync(string name, string? address, decimal? capacityM3, decimal? lengthMeters, decimal? widthMeters)
        => _conn.PostAsync<WarehouseListItem>("/api/warehouses", new { name, address, capacityM3, lengthMeters, widthMeters })!;

    /// <summary>Tạo khu vực mới trong 1 kho.</summary>
    public Task<WarehouseZoneItem> CreateZoneAsync(int warehouseId, string name, string zoneType, decimal posX, decimal posY, decimal widthMeters, decimal heightMeters)
        => _conn.PostAsync<WarehouseZoneItem>($"/api/warehouses/{warehouseId}/zones",
            new { name, zoneType, posX, posY, widthMeters, heightMeters })!;

    /// <summary>Tạo vị trí lưu trữ mới trong 1 khu vực.</summary>
    public Task<StorageBinItem> CreateBinAsync(int zoneId, string code, decimal capacityM3, decimal maxWeightKg, int row, int column)
        => _conn.PostAsync<StorageBinItem>($"/api/warehouses/zones/{zoneId}/bins",
            new { code, capacityM3, maxWeightKg, row, column })!;

    public Task<WarehouseActionResultItem> DeleteWarehouseAsync(int warehouseId)
        => _conn.DeleteAsync<WarehouseActionResultItem>($"/api/warehouses/{warehouseId}")!;

    public Task<WarehouseActionResultItem> DeleteZoneAsync(int zoneId)
        => _conn.DeleteAsync<WarehouseActionResultItem>($"/api/warehouses/zones/{zoneId}")!;

    public Task<WarehouseActionResultItem> DeleteBinAsync(int binId)
        => _conn.DeleteAsync<WarehouseActionResultItem>($"/api/warehouses/zones/bins/{binId}")!;

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