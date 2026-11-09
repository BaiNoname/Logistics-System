using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_FuelManagement;

namespace TruckingSystem.Desktop.SERVICE_FuelManagement
{
    public class FuelApiService
    {
        private readonly ApiConnection _conn;

        public FuelApiService(ApiConnection conn)
        {
            _conn = conn;
        }

        public Task<List<FuelProviderItem>> GetProvidersAsync()
            => _conn.GetAsync<List<FuelProviderItem>>("/api/fuel/providers")!;

        public Task<FuelProviderItem> CreateProviderAsync(string name, string? contactPhone, string? address)
            => _conn.PostAsync<FuelProviderItem>("/api/fuel/providers", new { name, contactPhone, address })!;

        public Task<FuelActionResultItem> DeleteProviderAsync(int id)
            => _conn.DeleteAsync<FuelActionResultItem>($"/api/fuel/providers/{id}")!;

        /// <summary>providerId null lấy toàn bộ lịch sử giá.</summary>
        public Task<List<FuelPriceHistoryItem>> GetPriceHistoryAsync(int? providerId)
            => _conn.GetAsync<List<FuelPriceHistoryItem>>(providerId.HasValue ? $"/api/fuel/prices?providerId={providerId}" : "/api/fuel/prices")!;

        public Task<FuelPriceHistoryItem> CreatePriceAsync(string fuelType, decimal price, DateTime effectiveDate, int? fuelProviderId)
            => _conn.PostAsync<FuelPriceHistoryItem>("/api/fuel/prices", new { fuelType, price, effectiveDate, fuelProviderId })!;

        /// <summary>Xóa 1 mức giá đã ghi — cần để dọn đường trước khi có thể xóa hẳn 1 hãng xăng.</summary>
        public Task<FuelActionResultItem> DeletePriceAsync(int priceId)
            => _conn.DeleteAsync<FuelActionResultItem>($"/api/fuel/prices/{priceId}")!;
    }
}