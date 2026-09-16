namespace TruckingSystem.Application.Routing;

public record RouteRequest(double FromLat, double FromLng, double ToLat, double ToLng);

public record RoutePoint(double Lat, double Lng);

/// <summary>Points đã được GIẢI MÃ sẵn ở Backend (thuật toán Google Polyline
/// Algorithm chuẩn 5 chữ số thập phân mà VietMap trả về) — Desktop chỉ cần vẽ
/// thẳng lên Leaflet bằng L.polyline(points), không cần tự giải mã ở phía JS.
/// TileUrlTemplate kèm sẵn API Key — Desktop không cần tự quản lý Key riêng,
/// tránh phải cấu hình VietMap Key ở 2 nơi (Backend lẫn Desktop).</summary>
public record RouteResult(bool Success, double DistanceKm, int DurationMinutes, List<RoutePoint> Points, string? TileUrlTemplate, string? Message);
