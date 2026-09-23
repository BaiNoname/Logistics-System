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

public record ConfirmArrivalRequest(string? Note);

public record ServiceResult(bool Success, string? Message);