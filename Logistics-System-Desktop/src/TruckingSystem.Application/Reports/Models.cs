namespace TruckingSystem.Application.Reports;

/// <summary>Báo cáo lãi/lỗ theo kỳ. Mọi doanh thu và chi phí được GẮN THEO ĐƠN
/// HÀNG, lọc theo NGÀY TẠO ĐƠN — nhờ vậy lãi/lỗ của mỗi đơn luôn khớp (doanh thu
/// và chi phí của cùng 1 đơn nằm chung 1 kỳ), không bị lệch khi chi phí phát
/// sinh sang tháng sau.
///   Doanh thu = tổng hóa đơn của đơn (bỏ qua hóa đơn đã hủy).
///   Chi phí   = nhiên liệu THỰC TIÊU của chuyến (km GPS × định mức × giá; thiếu GPS thì tạm theo km dự kiến)
///               + chi phí phát sinh + chi phí sự cố, của TẤT CẢ chuyến thuộc đơn (kể cả chuyến Trả hàng khi hủy đơn).
///   Tiền ĐỔ XĂNG là tiền mua vào BÌNH XE (xăng còn lại chạy tiếp các chuyến sau) -> KHÔNG gán vào đơn; được
///   tổng hợp theo TỪNG XE trong kỳ (VehicleFuel) để tham khảo.</summary>
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
    decimal FuelActualCost = 0,           // tiền xăng ĐÃ MUA (đổ vào bình) trong kỳ — mọi xe, theo ngày đổ
    int TripsUsingFuelEstimate = 0,       // số chuyến THIẾU GPS -> xăng của chuyến tạm tính theo km dự kiến
    List<FuelControlRowDto>? FuelControl = null,
    decimal FuelPlannedCostCompared = 0,  // dự toán — chỉ các chuyến hoàn tất có đủ cả dự toán lẫn định mức
    decimal FuelNormCostCompared = 0,     // định mức — cùng tập chuyến trên (để tính "Lệch dự toán")
    int FuelComparedTripCount = 0,
    // ---- Giao đúng hạn (đơn HOÀN TẤT có hạn giao): ngày xong chặng Giao cuối ≤ hạn giao ----
    int OnTimeOrders = 0,
    int LateOrders = 0,
    int OrdersWithoutDeadline = 0,
    // ---- Quyết toán xăng THEO XE trong kỳ ----
    decimal FuelActualLiters = 0,         // lít đã mua (đổ) trong kỳ
    decimal FuelNormLiters = 0,           // lít thực tiêu theo km của các chuyến trong báo cáo (tính vào đơn)
    List<VehicleFuelRowDto>? VehicleFuel = null);

/// <summary>1 xe trong bảng "Xăng từng xe trong kỳ" — THAM KHẢO: lít đã đổ cả kỳ so với lít định mức theo km GPS cả kỳ.
/// Xăng còn lại trong bình đầu / cuối kỳ làm số này lệch (VD đổ đầy bình cuối kỳ) -> chỉ nhắc khi lệch > 15%, KHÔNG
/// gắn cờ đỏ, không đưa vào "Việc cần chú ý". Kỳ càng dài (tháng / quý) thì phần dư trong bình càng ít ảnh hưởng.</summary>
public record VehicleFuelRowDto(
    int VehicleId, string PlateNumber, decimal? ConsumptionPer100Km,
    decimal RefuelLiters, decimal RefuelCost,
    decimal GpsKm, decimal NormLiters, int TripCount, int TripsWithoutGps,
    decimal? DeviationPct, string? Note);

/// <summary>1 dòng bảng "Kiểm soát nhiên liệu" — 1 chuyến đã chạy.
/// Flagged = km GPS lệch km tuyến dự kiến (VietMap) quá 15% — chỉ so KM. Tiền đổ xăng không so theo chuyến
/// (đổ đầy bình ở chuyến này nhưng chạy cho nhiều chuyến sau) mà quyết toán theo xe (VehicleFuelRowDto).</summary>
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
