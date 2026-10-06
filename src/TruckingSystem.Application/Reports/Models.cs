namespace TruckingSystem.Application.Reports;

/// <summary>Báo cáo lãi/lỗ theo kỳ. Mọi doanh thu và chi phí được GẮN THEO ĐƠN
/// HÀNG, lọc theo NGÀY TẠO ĐƠN — nhờ vậy lãi/lỗ của mỗi đơn luôn khớp (doanh thu
/// và chi phí của cùng 1 đơn nằm chung 1 kỳ), không bị lệch khi chi phí phát
/// sinh sang tháng sau.
///   Doanh thu = tổng hóa đơn của đơn (bỏ qua hóa đơn đã hủy).
///   Chi phí   = nhiên liệu thực tế đã đổ + chi phí phát sinh + chi phí sự cố,
///               của TẤT CẢ chuyến thuộc đơn (kể cả chuyến Trả hàng khi hủy đơn).</summary>
public record ProfitReportDto(
    DateTime From, DateTime To,
    decimal Revenue, decimal Collected,
    decimal FuelCost, decimal ExpenseCost, decimal IncidentCost, decimal TotalCost,
    decimal Profit, decimal? MarginPercent,
    int TotalOrders, int DoneOrders, int CancelledOrders, int UninvoicedDoneOrders,
    decimal CancelledOrdersCost,
    List<CostCategoryDto> CostBreakdown,
    List<MonthlyProfitDto> Monthly,
    List<OrderProfitDto> Orders);

public record CostCategoryDto(string Category, decimal Amount);

/// <summary>Month dạng "yyyy-MM" (theo giờ Việt Nam).</summary>
public record MonthlyProfitDto(string Month, decimal Revenue, decimal Cost);

public record OrderProfitDto(
    int OrderId, string OrderCode, string CustomerName, string Status, DateTime CreatedAt,
    bool HasInvoice, decimal Revenue,
    decimal FuelCost, decimal ExpenseCost, decimal IncidentCost, decimal TotalCost, decimal Profit);

/// <summary>Chi tiết TỪNG khoản chi của 1 đơn — để nhìn rõ tiền đi đâu.</summary>
public record OrderCostDetailDto(
    int OrderId, string OrderCode, string CustomerName, string Status,
    decimal Revenue, decimal TotalCost, List<CostLineDto> Lines);

public record CostLineDto(int TripId, string LegType, string TripStatus, string Category, string Description, DateTime Date, decimal Amount);
