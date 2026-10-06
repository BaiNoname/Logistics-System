using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_ReportManagement;

namespace TruckingSystem.Desktop.SERVICE_ReportManagement
{
    public class ReportApiService
    {
        private readonly ApiConnection _conn;

        public ReportApiService(ApiConnection conn)
        {
            _conn = conn;
        }

        /// <summary>Báo cáo lãi/lỗ cho các đơn tạo trong kỳ [from, to].</summary>
        public Task<ProfitReport> GetProfitReportAsync(DateTime from, DateTime to)
            => _conn.GetAsync<ProfitReport>($"/api/reports/profit?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}")!;

        /// <summary>Chi tiết từng khoản chi của 1 đơn.</summary>
        public Task<OrderCostDetail> GetOrderCostDetailAsync(int orderId)
            => _conn.GetAsync<OrderCostDetail>($"/api/reports/orders/{orderId}/costs")!;
    }
}
