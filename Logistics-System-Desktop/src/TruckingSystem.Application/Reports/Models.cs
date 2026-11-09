namespace TruckingSystem.Application.Reports;

/// <summary>Báo cáo lãi/lỗ theo kỳ. Mọi doanh thu và chi phí được GẮN THEO ĐƠN
/// HÀNG, lọc theo NGÀY TẠO ĐƠN — nhờ vậy lãi/lỗ của mỗi đơn luôn khớp (doanh thu
/// và chi phí của cùng 1 đơn nằm chung 1 kỳ), không bị lệch khi chi phí phát
/// sinh sang tháng sau.
///   Doanh thu = tổng hóa đơn của đơn (bỏ qua hóa đơn đã hủy).
///   Chi phí   = nhiên liệu (thực đổ theo hóa đơn; chuyến chưa nhập hóa đơn thì tạm
///               tính theo định mức GPS) + chi phí phát sinh + chi phí sự cố,
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
    List<OrderProfitDto> Orders,
    // ---- Kiểm soát nhiên liệu (các chuyến đã chạy của các đơn trong kỳ) ----
    decimal FuelPlannedCost = 0,          // dự toán theo km VietMap
    decimal FuelNormCost = 0,             // định mức theo km GPS thực tế
    decimal FuelActualCost = 0,           // thực đổ (hóa đơn đã ghi)
    int TripsUsingFuelEstimate = 0,       // số chuyến chưa nhập hóa đơn xăng -> lãi/lỗ đang tạm tính theo định mức
    List<FuelControlRowDto>? FuelControl = null,
    decimal FuelPlannedCostCompared = 0,  // dự toán — chỉ các chuyến hoàn tất có đủ cả dự toán lẫn định mức
    decimal FuelNormCostCompared = 0,     // định mức — cùng tập chuyến trên (để tính "Lệch dự toán")
    int FuelComparedTripCount = 0,
    // ---- Giao đúng hạn (đơn HOÀN TẤT có hạn giao): ngày xong chặng Giao cuối ≤ hạn giao ----
    int OnTimeOrders = 0,
    int LateOrders = 0,
    int OrdersWithoutDeadline = 0);

/// <summary>1 dòng bảng "Kiểm soát nhiên liệu" — 1 chuyến đã chạy.
/// Flagged = lệch km (GPS so với VietMap) hoặc lệch tiền (thực đổ so với định mức) quá 15%.</summary>
public record FuelControlRowDto(
    int TripId, string OrderCode, string LegType, string PlateNumber, string Status, DateTime TripDate,
    decimal? PlannedKm, decimal? GpsKm, decimal? KmDeviationPct,
    decimal? ConsumptionPer100Km, decimal? UnitPrice,
    decimal? PlannedCost, decimal? NormCost, decimal? NormLiters,
    decimal ActualCost, decimal ActualLiters, decimal? CostDeviationPct,
    bool HasActual, bool Flagged, string? Note);

public record CostCategoryDto(string Category, decimal Amount);

/// <summary>Month dạng "yyyy-MM" (theo giờ Việt Nam).</summary>
public record MonthlyProfitDto(string Month, decimal Revenue, decimal Cost);

public record OrderProfitDto(
    int OrderId, string OrderCode, string CustomerName, string Status, DateTime CreatedAt,
    bool HasInvoice, decimal Revenue,
    decimal FuelCost, decimal ExpenseCost, decimal IncidentCost, decimal TotalCost, decimal Profit,
    // Kết quả giao so với hạn (đơn hoàn tất): "Đúng hạn" / "Trễ N ngày" / null (chưa xong / không có hạn).
    string? DeliveryResult = null);

/// <summary>Chi tiết TỪNG khoản chi của 1 đơn — để nhìn rõ tiền đi đâu.</summary>
public record OrderCostDetailDto(
    int OrderId, string OrderCode, string CustomerName, string Status,
    decimal Revenue, decimal TotalCost, List<CostLineDto> Lines);

public record CostLineDto(int TripId, string LegType, string TripStatus, string Category, string Description, DateTime Date, decimal Amount);
