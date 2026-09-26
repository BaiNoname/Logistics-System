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

        /// <summary>Lộ trình chi tiết nhiều điểm dừng của 1 chặng — chặng Lấy
        /// hàng có 3 điểm (Kho→Đối tác→Kho, round-trip thật).</summary>
        public Task<TripRouteItem> GetTripRouteAsync(int id)
            => _conn.GetAsync<TripRouteItem>($"/api/trips/{id}/route")!;

        /// <summary>Tính khoảng cách/thời gian ước tính từ VietMap và LƯU LẠI
        /// vào chuyến (phục vụ thống kê so sánh Ước tính vs Thực tế sau này).</summary>
        public Task<TripRouteEstimateItem> CalculateEstimateAsync(int id)
            => _conn.PostAsync<TripRouteEstimateItem>($"/api/trips/{id}/calculate-estimate", new { })!;

        /// <summary>Danh sách sự cố đã ghi nhận cho 1 chuyến.</summary>
        public Task<List<IncidentReportItem>> GetIncidentsAsync(int id)
            => _conn.GetAsync<List<IncidentReportItem>>($"/api/trips/{id}/incidents")!;

        /// <summary>Ghi nhận sự cố mới.</summary>
        public Task<IncidentReportItem> CreateIncidentAsync(int id, string type, string? description, decimal? cost)
            => _conn.PostAsync<IncidentReportItem>($"/api/trips/{id}/incidents", new { type, description, cost })!;

        public Task<TripActionResultItem> DeleteIncidentAsync(int incidentId)
            => _conn.DeleteAsync<TripActionResultItem>($"/api/trips/incidents/{incidentId}")!;

        /// <summary>Đồng bộ lịch chuyến lên Google Calendar của tài xế + khách hàng.</summary>
        public Task<CalendarSyncResultItem> SyncCalendarAsync(int id)
            => _conn.PostAsync<CalendarSyncResultItem>($"/api/trips/{id}/sync-calendar", new { })!;

        /// <summary>Danh sách chi phí khác (ngoài xăng) đã ghi cho 1 chuyến.</summary>
        public Task<List<TripExpenseItem>> GetExpensesAsync(int id)
            => _conn.GetAsync<List<TripExpenseItem>>($"/api/trips/{id}/expenses")!;

        public Task<TripExpenseItem> CreateExpenseAsync(int id, string type, decimal amount, string? note)
            => _conn.PostAsync<TripExpenseItem>($"/api/trips/{id}/expenses", new { type, amount, note })!;

        public Task<TripActionResultItem> DeleteExpenseAsync(int expenseId)
            => _conn.DeleteAsync<TripActionResultItem>($"/api/trips/expenses/{expenseId}")!;
    }
}
