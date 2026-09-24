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

        /// <summary>Toàn bộ điểm GPS thật đã ghi nhận — vẽ đường đi thực tế trên bản đồ.</summary>
        public Task<List<WaypointItem>> GetWaypointsAsync(int id)
            => _conn.GetAsync<List<WaypointItem>>($"/api/trips/{id}/waypoints")!;

        /// <summary>So sánh chi phí xăng ước tính (từ GPS thật) với chi phí thực tế.</summary>
        public Task<FuelEstimateItem> GetFuelEstimateAsync(int id)
            => _conn.GetAsync<FuelEstimateItem>($"/api/trips/{id}/fuel-estimate")!;

        /// <summary>Danh sách sự cố đã ghi nhận cho 1 chuyến.</summary>
        public Task<List<IncidentReportItem>> GetIncidentsAsync(int id)
            => _conn.GetAsync<List<IncidentReportItem>>($"/api/trips/{id}/incidents")!;

        /// <summary>Ghi nhận sự cố mới.</summary>
        public Task<IncidentReportItem> CreateIncidentAsync(int id, string type, string? description, decimal? cost)
            => _conn.PostAsync<IncidentReportItem>($"/api/trips/{id}/incidents", new { type, description, cost })!;

        public Task<TripActionResultItem> DeleteIncidentAsync(int incidentId)
            => _conn.DeleteAsync<TripActionResultItem>($"/api/trips/incidents/{incidentId}")!;
    }
}
