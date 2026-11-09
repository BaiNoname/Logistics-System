namespace TruckingSystem.Application.Locations;

/// <summary>
/// Trừu tượng hóa nhà cung cấp tìm kiếm địa điểm (gợi ý khi gõ + lấy tọa độ
/// thật) — Application chỉ biết interface này, không biết đang gọi VietMap
/// hay nhà cung cấp khác. Cài đặt thật (VietMapPlaceProvider) nằm ở
/// Infrastructure, đúng nguyên tắc đảo ngược phụ thuộc.
/// </summary>
public interface IPlaceSearchProvider
{
    /// <summary>Gõ tới đâu, gợi ý tới đó — trả về danh sách địa điểm khớp từ khóa.</summary>
    Task<List<PlaceSuggestionDto>> SearchAsync(string text);

    /// <summary>Lấy tọa độ thật (Lat/Lng) của 1 gợi ý đã chọn, qua RefId trả về từ SearchAsync.</summary>
    Task<PlaceDetailDto?> GetDetailAsync(string refId);
}