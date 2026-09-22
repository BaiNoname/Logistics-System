using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TripManagement;

namespace TruckingSystem.Desktop.SERVICE_TripManagement
{
    public class TripApiService
    {
        private readonly ApiConnection _conn;

        public TripApiService(ApiConnection conn)
        {
            _conn = conn;
        }

        public Task<List<TripListItem>> GetTripsAsync()
            => _conn.GetAsync<List<TripListItem>>("/api/trips")!;

        /// <summary>Planned -> InProgress.</summary>
        public Task<TripActionResultItem> StartTripAsync(int id)
            => _conn.PostAsync<TripActionResultItem>($"/api/trips/{id}/start", new { })!;

        /// <summary>InProgress -> Completed.</summary>
        public Task<TripActionResultItem> CompleteTripAsync(int id)
            => _conn.PostAsync<TripActionResultItem>($"/api/trips/{id}/complete", new { })!;

        /// <summary>Ghi nhận 1 lần đổ xăng thực tế.</summary>
        public Task<TripActionResultItem> RecordFuelAsync(int id, decimal liters, decimal cost, string? stationName)
            => _conn.PostAsync<TripActionResultItem>($"/api/trips/{id}/fuel", new { liters, cost, stationName })!;
    }
}
