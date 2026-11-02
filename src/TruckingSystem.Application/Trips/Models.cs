namespace TruckingSystem.Application.Trips;

// ==== Dùng cho màn hình quản lý Trip (Desktop, Dispatcher) ====

/// <summary>HasRun: chuyến ĐÃ TỪNG CHẠY (có giờ bắt đầu) — Desktop dùng để bật nút Xăng /
/// Chi phí / Sự cố đúng như server (chuyến hủy TRƯỚC khi chạy thì không ghi được).
/// HasEstimate: đã có ước tính lộ trình (số kế hoạch) chưa.</summary>
public record TripListItemDto(
    int Id, string OrderCode, string VehiclePlate, string DriverName,
    string Status, string LegType, decimal TotalQtyPlanned,
    double? FromLat, double? FromLng, double? ToLat, double? ToLng,
    bool HasRun = false, bool HasEstimate = false,
    // Chặng Giao chưa chạy, kho CHƯA đủ hàng cho chuyến vì hàng lấy về còn "Chờ nhập kho" —
    // CÙNG quy tắc server chặn Bắt đầu (Desktop dùng cờ này, không tự đoán).
    bool AwaitingReceipt = false,
    // HẠN GIAO (DeliveryDeadlineRules): hạn giao của đơn, ngày XUẤT PHÁT MUỘN NHẤT để kịp hạn
    // (chuyến chưa chạy), câu tình trạng + mức (None/Ok/Warning/Late/DoneOnTime/DoneLate) để tô màu.
    DateTime? CommittedDeliveryDate = null,
    DateTime? LatestStartDate = null,
    string DeadlineStatus = "—",
    string DeadlineLevel = "None",
    // Ngày xuất phát dự kiến chọn lúc Chia xe (null = dữ liệu cũ).
    DateTime? PlannedStartDate = null,
    // Kho trung chuyển của chuyến — để lọc theo kho.
    string? WarehouseName = null,
    // Điểm đi -> điểm đến của chặng (TripRouteText) — để nhìn danh sách biết chuyến chạy từ đâu tới đâu.
    string? FromName = null,
    string? ToName = null,
    // Ước tính lộ trình (VietMap, tính tự động lúc Chia xe) — hiện ở thẻ "Chuyến đang chọn".
    decimal? EstimatedDistanceKm = null,
    int? EstimatedDurationMinutes = null
);

public record TripActionResult(bool Success, string? Message);

/// <summary>Ghi nhận 1 lần đổ xăng thực tế cho 1 chuyến cụ thể.</summary>
/// <summary>AutoPrice = true: KHÔNG cần nhập tiền — server tự lấy đơn giá mới
/// nhất của hãng đã chọn (FuelProviderId; null = giá thị trường chung) theo đúng
/// loại nhiên liệu của xe, Cost = Liters x đơn giá. AutoPrice = false: giữ cách
/// cũ (nhập tay Cost).</summary>
public record RecordFuelRequest(decimal Liters, decimal Cost, string? StationName, int? FuelProviderId = null, bool AutoPrice = false);

/// <summary>1 lần đổ xăng đã ghi của chuyến (lịch sử).</summary>
public record FuelLogDto(int Id, DateTime Timestamp, decimal Liters, decimal UnitPrice, decimal Cost, string? StationName);

/// <summary>Đơn giá tự động cho form ghi đổ xăng. CanRecord = chuyến có đang
/// cho ghi xăng không (đã chạy, chưa hủy); Found = có tìm được đơn giá không.</summary>
public record FuelPriceQuoteDto(bool CanRecord, bool Found, string? FuelType, decimal UnitPrice,
    DateTime? EffectiveDate, string? SourceText, string? Message,
    // Mức nhiên liệu của xe (VehicleFuelLevelRules) — form ghi đổ xăng hiện "bình còn trống ~X lít".
    decimal? TankCapacityLiters = null, decimal? CurrentFuelLiters = null);

// ==== Dùng cho App tài xế (GPS ping + xác nhận đến nơi) — ĐÃ CÓ TỪ TRƯỚC,
// giữ nguyên tên/kiểu để không phá vỡ TripsController đã tồn tại ====

public record GpsPingRequest(double Lat, double Lng, decimal? SpeedKmh, decimal? Accuracy, DateTime? Timestamp);

/// <summary>1 điểm GPS thật đã ghi nhận — dùng để vẽ ĐƯỜNG ĐI THỰC TẾ trên bản
/// đồ, khác với lộ trình LÝ THUYẾT do VietMap Route API tính sẵn.</summary>
public record WaypointDto(double Lat, double Lng, DateTime Timestamp, decimal? SpeedKmh);

/// <summary>Tọa độ đầu-cuối ĐÚNG theo chặng — dùng cho trang điều hướng công khai.</summary>
/// <summary>1 điểm dừng trong lộ trình — Label giải thích RÕ đây là điểm gì
/// (VD: "Xuất phát từ Kho", "Lấy hàng tại đối tác", "Về Kho nộp hàng") để vẽ
/// đúng thứ tự các đoạn trên bản đồ, không chỉ 1 cặp Từ-Đến đơn giản.</summary>
public record RouteStopDto(double? Lat, double? Lng, string Label);

/// <summary>Lộ trình ĐẦY ĐỦ của 1 chặng — CHẶNG LẤY HÀNG có 3 điểm dừng (Kho
/// xuất phát RỖNG -> Đối tác lấy hàng -> VỀ ĐÚNG Kho đó nộp hàng, round-trip
/// THẬT vì xe phải quay về, không phải đi 1 chiều rồi biến mất) — CHẶNG GIAO
/// HÀNG chỉ 2 điểm (Kho -> Điểm giao, 1 chiều, xong việc cho đơn này).</summary>
public record TripRouteDto(string OrderCode, string LegType, List<RouteStopDto> Stops);

/// <summary>So sánh chi phí xăng ƯỚC TÍNH (tính từ khoảng cách GPS thật x định
/// mức tiêu hao x giá xăng mới nhất) với chi phí THỰC TẾ đã ghi tay qua
/// FuelLog — dùng để đo lường mức độ chênh lệch/hiệu quả vận hành.</summary>
public record FuelEstimateDto(
    decimal ActualDistanceKm,
    decimal? EstimatedLiters,
    decimal? EstimatedCost,
    decimal ActualFuelCost,
    decimal? VarianceCost,
    string? Message
);

public record IncidentReportDto(int Id, string Type, string? Description, DateTime Timestamp, decimal? Cost);

/// <summary>Kết quả đồng bộ lịch — báo riêng từng bên (tài xế/khách hàng) vì
/// 1 bên có thể lỗi (VD: chưa chia sẻ Calendar) trong khi bên kia vẫn thành công.</summary>
public record CalendarSyncResultDto(bool DriverSynced, string? DriverMessage, bool CustomerSynced, string? CustomerMessage);

public record TripExpenseDto(int Id, string Type, decimal Amount, string? Note);

/// <summary>Kết quả tính + LƯU LẠI ước tính khoảng cách/thời gian từ VietMap
/// cho 1 chuyến — lưu vào Trip.EstimatedDistanceKm/EstimatedDurationMinutes
/// để sau này làm bảng thống kê so sánh với số liệu THỰC TẾ.</summary>
public record TripRouteEstimateDto(decimal DistanceKm, int DurationMinutes, string? Message);

/// <summary>Chi phí phát sinh KHÁC ngoài xăng dầu (phí cầu đường, bến bãi, bốc
/// xếp, phạt...) — dùng để tính ĐỦ tổng chi phí thực tế của 1 chuyến, không
/// chỉ riêng xăng, phục vụ tính doanh thu trừ chi phí chính xác hơn.</summary>
public record CreateTripExpenseRequest(string Type, decimal Amount, string? Note);

/// <summary>Báo cáo sự cố giữa đường (tai nạn/hỏng xe/vi phạm giao thông/khác)
/// — tài xế hoặc Dispatcher ghi nhận khi có sự cố xảy ra trong lúc chạy chuyến.</summary>
public record CreateIncidentReportRequest(string Type, string? Description, decimal? Cost);

/// <summary>ConfirmedCargoLineIds: các dòng hàng tài xế đã tick "nhận/giao ĐỦ" trên app —
/// phải đủ TẤT CẢ dòng hàng của chuyến (đối tác bắt buộc giao đủ; thiếu thì không xác nhận).</summary>
public record ConfirmArrivalRequest(string? Note, List<int>? ConfirmedCargoLineIds = null);

public record ServiceResult(bool Success, string? Message);

// ---- Đổi xe / tài xế cho chuyến CHƯA CHẠY (chuyến kẹt: xe hỏng, giấy tờ/bằng lái hết hạn...) ----

/// <summary>1 lựa chọn xe / tài xế. Eligible = dùng được; Reason = lý do không dùng được
/// (hoặc lưu ý, VD tài xế chưa đăng ký ngày làm hôm nay).</summary>
/// <summary>CannotDrive (chỉ với tài xế): xe (Id) mà hạng bằng của tài xế KHÔNG lái được -> lý do —
/// để màn Đổi xe / tài xế báo ngay khi chọn cặp không hợp, không phải bấm Lưu mới biết.</summary>
public record ReassignOptionDto(int Id, string Text, bool Eligible, string? Reason, Dictionary<int, string>? CannotDrive = null);

/// <summary>AffectedTripIds: các chuyến sẽ được đổi CÙNG lúc (chặng Lấy hàng + Giao hàng
/// chưa chạy của cùng xe trong đơn đi cùng nhau). Message != null: không đổi được.</summary>
public record ReassignOptionsDto(int TripId, List<int> AffectedTripIds, int CurrentVehicleId, int CurrentDriverEmployeeId,
    List<ReassignOptionDto> Vehicles, List<ReassignOptionDto> Drivers, string? Message);

public record ReassignTripRequest(int VehicleId, int DriverEmployeeId);