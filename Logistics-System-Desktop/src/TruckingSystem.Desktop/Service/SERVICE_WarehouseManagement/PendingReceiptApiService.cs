using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_WarehouseManagement;

namespace TruckingSystem.Desktop.SERVICE_WarehouseManagement
{
    /// <summary>Hàng CHỜ NHẬP KHO (chuyến Lấy hàng đã về kho, hàng chưa lên kệ). Tách file riêng,
    /// không sửa WarehouseApiService/ApiServices — tạo bằng: new PendingReceiptApiService(api.Connection).</summary>
    public class PendingReceiptApiService
    {
        private readonly ApiConnection _conn;

        public PendingReceiptApiService(ApiConnection conn)
        {
            _conn = conn;
        }

        public async Task<List<PendingReceiptLineItem>> GetPendingAsync(int warehouseId)
            => await _conn.GetAsync<List<PendingReceiptLineItem>>($"/api/warehouses/{warehouseId}/pending-receipts")
               ?? new List<PendingReceiptLineItem>();

        /// <summary>Nhập TẤT CẢ hàng còn chờ của 1 chuyến theo Best-Fit (lỗi 1 dòng = không nhập dòng nào).</summary>
        public async Task<ReceiveGoodsResultItem> ReceiveAutoAsync(int warehouseId, int tripId)
            => await _conn.PostAsync<ReceiveGoodsResultItem>($"/api/warehouses/{warehouseId}/pending-receipts/{tripId}/auto", new { })
               ?? new ReceiveGoodsResultItem { Success = false, Message = "Không nhận được phản hồi từ server." };

        /// <summary>Nhập THỦ CÔNG 1 phần của 1 dòng hàng. binId = null: Best-Fit cho riêng số lượng này.</summary>
        public async Task<ReceiveGoodsResultItem> ReceiveManualAsync(int warehouseId, int tripId, int cargoLineId, decimal qty, int? binId)
            => await _conn.PostAsync<ReceiveGoodsResultItem>($"/api/warehouses/{warehouseId}/pending-receipts/{tripId}/manual",
                   new { cargoLineId, qty, binId })
               ?? new ReceiveGoodsResultItem { Success = false, Message = "Không nhận được phản hồi từ server." };
    }
}
