using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_WarehouseManagement;

namespace TruckingSystem.Desktop.SERVICE_WarehouseManagement
{
    /// <summary>XUẤT KHO THEO CHUYẾN (chặng Giao) — tạo bằng: new TripIssueApiService(api.Connection).</summary>
    public class TripIssueApiService
    {
        private readonly ApiConnection _conn;

        public TripIssueApiService(ApiConnection conn)
        {
            _conn = conn;
        }

        public async Task<List<TripIssueLineItem>> GetAsync(int warehouseId)
            => await _conn.GetAsync<List<TripIssueLineItem>>($"/api/warehouses/{warehouseId}/trip-issues")
               ?? new List<TripIssueLineItem>();

        /// <summary>Xác nhận xuất ĐỦ hàng của chuyến (lỗi 1 dòng = không xuất dòng nào).</summary>
        public async Task<WarehouseActionResultItem> IssueAsync(int warehouseId, int tripId)
            => await _conn.PostAsync<WarehouseActionResultItem>($"/api/warehouses/{warehouseId}/trip-issues/{tripId}/issue", new { })
               ?? new WarehouseActionResultItem { Success = false, Message = "Không nhận được phản hồi từ server." };

        /// <summary>Hoàn tác xuất (dỡ hàng về kệ) — chỉ khi xe chưa chạy.</summary>
        public async Task<WarehouseActionResultItem> UndoAsync(int warehouseId, int tripId)
            => await _conn.PostAsync<WarehouseActionResultItem>($"/api/warehouses/{warehouseId}/trip-issues/{tripId}/undo", new { })
               ?? new WarehouseActionResultItem { Success = false, Message = "Không nhận được phản hồi từ server." };
    }
}
