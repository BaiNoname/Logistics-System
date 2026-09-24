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
public record TripRouteDto(string OrderCode, string LegType, double? FromLat, double? FromLng, double? ToLat, double? ToLng);

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

/// <summary>Báo cáo sự cố giữa đường (tai nạn/hỏng xe/vi phạm giao thông/khác)
/// — tài xế hoặc Dispatcher ghi nhận khi có sự cố xảy ra trong lúc chạy chuyến.</summary>
public record CreateIncidentReportRequest(string Type, string? Description, decimal? Cost);

public record ConfirmArrivalRequest(string? Note);

public record ServiceResult(bool Success, string? Message);