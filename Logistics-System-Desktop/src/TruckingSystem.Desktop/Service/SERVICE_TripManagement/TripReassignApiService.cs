using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TripManagement;

namespace TruckingSystem.Desktop.SERVICE_TripManagement
{
    /// <summary>Gỡ chuyến bị kẹt (chưa chạy): đổi xe/tài xế, trả khách tại kho. Tách file
    /// riêng, không sửa TripApiService/ApiServices — tạo bằng: new TripReassignApiService(api.Connection).</summary>
    public class TripReassignApiService
    {
        private readonly ApiConnection _conn;

        public TripReassignApiService(ApiConnection conn)
        {
            _conn = conn;
        }

        public async Task<ReassignOptionsItem> GetOptionsAsync(int tripId)
            => await _conn.GetAsync<ReassignOptionsItem>($"/api/trips/{tripId}/reassign-options")
               ?? new ReassignOptionsItem { TripId = tripId, Message = "Không nhận được phản hồi từ server." };

        public async Task<TripActionResultItem> ReassignAsync(int tripId, int vehicleId, int driverEmployeeId)
            => await _conn.PostAsync<TripActionResultItem>($"/api/trips/{tripId}/reassign", new { vehicleId, driverEmployeeId })
               ?? new TripActionResultItem { Success = false, Message = "Không nhận được phản hồi từ server." };

        public async Task<TripActionResultItem> ReturnAtWarehouseAsync(int tripId)
            => await _conn.PostAsync<TripActionResultItem>($"/api/trips/{tripId}/return-at-warehouse", new { })
               ?? new TripActionResultItem { Success = false, Message = "Không nhận được phản hồi từ server." };
    }
}
