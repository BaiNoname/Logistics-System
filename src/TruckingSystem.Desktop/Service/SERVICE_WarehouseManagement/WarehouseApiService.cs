using TruckingSystem.Desktop.DTO_TransportOrderManagement;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_WarehouseManagement;

namespace TruckingSystem.Desktop.SERVICE_WarehouseManagement
{
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
        public Task<WarehouseListItem> CreateWarehouseAsync(string name, string? address, decimal? capacityM3, decimal? lengthMeters, decimal? widthMeters, double? lat, double? lng, string? doorSide = null, decimal? doorOffsetMeters = null)
            => _conn.PostAsync<WarehouseListItem>("/api/warehouses", new { name, address, capacityM3, lengthMeters, widthMeters, lat, lng, doorSide, doorOffsetMeters })!;

        /// <summary>Sửa thông tin kho đã có.</summary>
        public Task<WarehouseActionResultItem> UpdateWarehouseAsync(int warehouseId, string name, string? address, decimal? capacityM3, decimal? lengthMeters, decimal? widthMeters, double? lat, double? lng, string? doorSide = null, decimal? doorOffsetMeters = null)
            => _conn.PutAsync<WarehouseActionResultItem>($"/api/warehouses/{warehouseId}", new { name, address, capacityM3, lengthMeters, widthMeters, lat, lng, doorSide, doorOffsetMeters })!;

        /// <summary>Tạo khu vực mới trong 1 kho.</summary>
        public Task<WarehouseZoneItem> CreateZoneAsync(int warehouseId, string name, string zoneType, decimal posX, decimal posY, decimal widthMeters, decimal heightMeters)
            => _conn.PostAsync<WarehouseZoneItem>($"/api/warehouses/{warehouseId}/zones",
                new { name, zoneType, posX, posY, widthMeters, heightMeters })!;

        /// <summary>Sửa khu vực đã có.</summary>
        public Task<WarehouseActionResultItem> UpdateZoneAsync(int zoneId, string name, string zoneType, decimal posX, decimal posY, decimal widthMeters, decimal heightMeters)
            => _conn.PutAsync<WarehouseActionResultItem>($"/api/warehouses/zones/{zoneId}",
                new { name, zoneType, posX, posY, widthMeters, heightMeters })!;

        /// <summary>Tạo vị trí lưu trữ mới trong 1 khu vực.</summary>
        public Task<StorageBinItem> CreateBinAsync(int zoneId, string code, decimal capacityM3, decimal maxWeightKg, decimal posX, decimal posY, decimal widthMeters, decimal heightMeters, decimal distanceFromDoorMeters)
            => _conn.PostAsync<StorageBinItem>($"/api/warehouses/zones/{zoneId}/bins",
                new { code, capacityM3, maxWeightKg, posX, posY, widthMeters, heightMeters, distanceFromDoorMeters })!;

        /// <summary>Sửa vị trí lưu trữ đã có.</summary>
        public Task<WarehouseActionResultItem> UpdateBinAsync(int binId, string code, decimal capacityM3, decimal maxWeightKg, decimal posX, decimal posY, decimal widthMeters, decimal heightMeters, decimal distanceFromDoorMeters)
            => _conn.PutAsync<WarehouseActionResultItem>($"/api/warehouses/zones/bins/{binId}",
                new { code, capacityM3, maxWeightKg, posX, posY, widthMeters, heightMeters, distanceFromDoorMeters })!;

        /// <summary>Nhập kho thủ công (không gắn Trip). chosenBinId có giá trị →
        /// dùng đúng vị trí đó; null → tự động Best-Fit.</summary>
        public Task<ReceiveGoodsResultItem> ReceiveManualAsync(int warehouseId, int transportOrderId, int cargoLineId, decimal qty, int? chosenBinId = null)
            => _conn.PostAsync<ReceiveGoodsResultItem>($"/api/warehouses/{warehouseId}/receive-manual",
                new { transportOrderId, cargoLineId, qty, chosenBinId })!;

        /// <summary>Danh sách vị trí còn trống, đúng khu vực phù hợp — để người
        /// dùng tự chọn tay khi nhập kho thủ công.</summary>
        public Task<List<StorageBinItem>> GetAvailableBinsForCargoAsync(int warehouseId, int cargoLineId)
            => _conn.GetAsync<List<StorageBinItem>>($"/api/warehouses/{warehouseId}/cargo-lines/{cargoLineId}/available-bins")!;

        /// <summary>Xuất kho thủ công (không gắn Trip).</summary>
        public Task<WarehouseActionResultItem> IssueManualAsync(int warehouseId, int transportOrderId, int cargoLineId, decimal qty)
            => _conn.PostAsync<WarehouseActionResultItem>($"/api/warehouses/{warehouseId}/issue-manual",
                new { transportOrderId, cargoLineId, qty })!;

        public Task<List<CargoLineOptionItem>> GetCargoLinesForOrderAsync(int transportOrderId, int warehouseId)
            => _conn.GetAsync<List<CargoLineOptionItem>>($"/api/warehouses/orders/{transportOrderId}/cargo-lines?warehouseId={warehouseId}")!;

        /// <summary>Chỉ các đơn liên quan tới kho này (chọn kho / có chuyến qua kho / đã nhập-xuất tại kho).</summary>
        public Task<List<TransportOrderListItem>> GetOrdersForWarehouseAsync(int warehouseId)
            => _conn.GetAsync<List<TransportOrderListItem>>($"/api/warehouses/{warehouseId}/orders")!;

        /// <summary>Toàn cảnh hàng của 1 đơn trong kho: đã nhập/xuất/còn/đang giữ cho chuyến + tiến độ từng chuyến.</summary>
        public Task<OrderStockFlowItem> GetOrderStockFlowAsync(int warehouseId, int transportOrderId)
            => _conn.GetAsync<OrderStockFlowItem>($"/api/warehouses/{warehouseId}/orders/{transportOrderId}/stock-flow")!;

        /// <summary>Hoàn tác 1 phiếu xuất THỦ CÔNG làm nhầm — trả hàng lại vào kho.</summary>
        public Task<WarehouseActionResultItem> ReverseManualIssueAsync(int issueId)
            => _conn.PostAsync<WarehouseActionResultItem>($"/api/warehouses/issues/{issueId}/reverse", new { })!;

        public Task<List<WarehouseTransactionItem>> GetTransactionHistoryAsync(int warehouseId)
            => _conn.GetAsync<List<WarehouseTransactionItem>>($"/api/warehouses/{warehouseId}/transactions")!;

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
}