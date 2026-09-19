using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TruckingSystem.Application.Locations;

namespace TruckingSystem.Infrastructure.ExternalServices;

/// <summary>
/// Gọi VietMap Autocomplete API (đã xác nhận CÓ THẬT + đã test JSON response
/// thật) — gõ từ khóa → trả về danh sách gợi ý KÈM SẴN tọa độ (nằm trong
/// geometry.coordinates dạng GeoJSON [lng, lat] — chú ý thứ tự ngược với
/// lat/lng thông thường). Dùng CHUNG 1 API Key với VietMapRouteProvider
/// (VietMap:RouteApiKey).
///
/// Cấu trúc JSON thật (đã xác nhận qua test 19/09/2026):
/// { "code": "OK", "data": { "features": [
///     { "Id": "...", "geometry": { "coordinates": [lng, lat] },
///       "properties": { "name": "...", "address": "...", "label": "..." } }
/// ] } }
/// </summary>
public class VietMapPlaceProvider : IPlaceSearchProvider
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public VietMapPlaceProvider(HttpClient http, IConfiguration config)
    {
        _http = http;
        _apiKey = config["VietMap:RouteApiKey"]
            ?? throw new InvalidOperationException("Thiếu cấu hình VietMap:RouteApiKey trong appsettings.json.");
    }

    public async Task<List<PlaceSuggestionDto>> SearchAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return new List<PlaceSuggestionDto>();

        var url = $"https://maps.vietmap.vn/api/autocomplete?api-version=1.1&apikey={_apiKey}&text={Uri.EscapeDataString(text)}";

        string body;
        try
        {
            body = await _http.GetStringAsync(url);
        }
        catch
        {
            return new List<PlaceSuggestionDto>(); // lỗi mạng -> coi như không có gợi ý, không làm vỡ UI
        }

        var results = new List<PlaceSuggestionDto>();

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        if (!root.TryGetProperty("data", out var data)) return results;
        if (!data.TryGetProperty("features", out var features) || features.ValueKind != JsonValueKind.Array) return results;

        foreach (var feature in features.EnumerateArray())
        {
            var refId = feature.TryGetProperty("Id", out var idEl) ? idEl.GetString() ?? "" : "";
            if (string.IsNullOrEmpty(refId)) continue;

            string name = "", address = "";
            if (feature.TryGetProperty("properties", out var props))
            {
                name = GetString(props, "name") ?? "";
                address = GetString(props, "address") ?? GetString(props, "label") ?? "";
            }

            double lat = 0, lng = 0;
            if (feature.TryGetProperty("geometry", out var geom)
                && geom.TryGetProperty("coordinates", out var coords)
                && coords.ValueKind == JsonValueKind.Array)
            {
                var arr = coords.EnumerateArray().ToList();
                // GeoJSON chuẩn: [longitude, latitude] — NGƯỢC thứ tự so với cách nói thông thường
                if (arr.Count >= 2)
                {
                    lng = arr[0].GetDouble();
                    lat = arr[1].GetDouble();
                }
            }

            results.Add(new PlaceSuggestionDto(refId, name, address, lat, lng));
        }
        return results;
    }

    /// <summary>DỰ PHÒNG — hiện KHÔNG dùng trong luồng chính (SearchAsync đã tự
    /// có tọa độ, không cần gọi thêm). Endpoint bên dưới CHƯA được xác nhận
    /// bằng test thật (chỉ suy luận theo khuôn mẫu Reverse v3) — có thể lỗi
    /// 404 tương tự Geocode nếu thực sự gọi tới.</summary>
    public async Task<PlaceDetailDto?> GetDetailAsync(string refId)
    {
        if (string.IsNullOrWhiteSpace(refId)) return null;

        var url = $"https://maps.vietmap.vn/api/place/v3?apikey={_apiKey}&refid={Uri.EscapeDataString(refId)}";

        string body;
        try
        {
            body = await _http.GetStringAsync(url);
        }
        catch
        {
            return null;
        }

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        var lat = GetDouble(root, "lat");
        var lng = GetDouble(root, "lng") ?? GetDouble(root, "lon");
        if (lat is null || lng is null) return null;

        var name = GetString(root, "name") ?? GetString(root, "display") ?? "";
        var address = GetString(root, "address") ?? GetString(root, "display") ?? "";

        return new PlaceDetailDto(name, address, lat.Value, lng.Value);
    }

    private static string? GetString(JsonElement el, string prop)
        => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    private static double? GetDouble(JsonElement el, string prop)
        => el.TryGetProperty(prop, out var v) && v.ValueKind is JsonValueKind.Number ? v.GetDouble() : null;
}