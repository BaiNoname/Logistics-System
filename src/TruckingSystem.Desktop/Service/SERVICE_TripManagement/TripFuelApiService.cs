using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TripManagement;

namespace TruckingSystem.Desktop.SERVICE_TripManagement
{
    /// <summary>Ghi nhận đổ xăng theo chuyến: lịch sử, đơn giá tự động, thêm, xóa.
    /// Tách file riêng để không phải sửa TripApiService/ApiServices hiện có —
    /// tạo bằng: new TripFuelApiService(api.Connection).</summary>
    public class TripFuelApiService
    {
        private readonly ApiConnection _conn;

        public TripFuelApiService(ApiConnection conn)
        {
            _conn = conn;
        }

        public async Task<List<FuelLogItem>> GetLogsAsync(int tripId)
            => await _conn.GetAsync<List<FuelLogItem>>($"/api/trips/{tripId}/fuel-logs") ?? new List<FuelLogItem>();

        /// <summary>providerId = null -> giá thị trường chung.</summary>
        public async Task<FuelPriceQuoteItem> GetPriceQuoteAsync(int tripId, int? providerId)
        {
            var query = providerId.HasValue ? $"?providerId={providerId.Value}" : "";
            return await _conn.GetAsync<FuelPriceQuoteItem>($"/api/trips/{tripId}/fuel-price{query}") ?? new FuelPriceQuoteItem();
        }

        /// <summary>Chỉ gửi số lít + hãng — server tự tính tiền theo bảng giá.</summary>
        public async Task<TripActionResultItem> RecordAsync(int tripId, decimal liters, int? fuelProviderId, string? stationName)
            => await _conn.PostAsync<TripActionResultItem>($"/api/trips/{tripId}/fuel",
                   new { liters, cost = 0m, stationName, fuelProviderId, autoPrice = true })
               ?? new TripActionResultItem { Success = false, Message = "Không nhận được phản hồi từ server." };

        public async Task<TripActionResultItem> DeleteAsync(int fuelLogId)
            => await _conn.DeleteAsync<TripActionResultItem>($"/api/trips/fuel-logs/{fuelLogId}")
               ?? new TripActionResultItem { Success = false, Message = "Không nhận được phản hồi từ server." };
    }
}
