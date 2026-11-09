namespace TruckingSystem.Application.Locations;

public record LocationDto(int Id, string Name, string Address, double? Lat, double? Lng, int? CustomerId);
public record CreateLocationRequest(string Name, string Address, double? Lat, double? Lng, int? CustomerId);

/// <summary>1 gợi ý trả về từ VietMap Autocomplete — ĐÃ CÓ SẴN tọa độ ngay
/// trong geometry.coordinates (xác nhận qua test thật, không cần gọi thêm
/// Place API nữa).</summary>
public record PlaceSuggestionDto(string RefId, string Name, string Address, double Lat, double Lng);

/// <summary>Chi tiết đầy đủ (kèm tọa độ thật) của 1 gợi ý đã chọn — lấy qua VietMap Place API.</summary>
public record PlaceDetailDto(string Name, string Address, double Lat, double Lng);