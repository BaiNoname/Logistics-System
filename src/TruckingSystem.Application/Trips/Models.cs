namespace TruckingSystem.Application.Trips;

// ==== Dùng cho màn hình quản lý Trip (Desktop, Dispatcher) ====

public record TripListItemDto(
    int Id, string OrderCode, string VehiclePlate, string DriverName,
    string Status, string LegType, decimal TotalQtyPlanned,
    double? FromLat, double? FromLng, double? ToLat, double? ToLng
);

public record TripActionResult(bool Success, string? Message);

/// <summary>Ghi nhận 1 lần đổ xăng thực tế cho 1 chuyến cụ thể.</summary>
public record RecordFuelRequest(decimal Liters, decimal Cost, string? StationName);

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

public record ConfirmArrivalRequest(string? Note);

public record ServiceResult(bool Success, string? Message);