namespace TruckingSystem.Application.Reports;

public interface IReportService
{
    /// <summary>Báo cáo lãi/lỗ cho các đơn TẠO TRONG khoảng [from, to] (ngày theo giờ Việt Nam, tính cả 2 đầu).</summary>
    Task<ProfitReportDto> GetProfitReportAsync(DateTime from, DateTime to);

    /// <summary>Toàn bộ khoản chi (nhiên liệu, phát sinh, sự cố) của 1 đơn, theo từng chuyến.</summary>
    Task<OrderCostDetailDto?> GetOrderCostDetailAsync(int orderId);
}
