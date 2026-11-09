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

        JsonDocument doc;
        try { doc = JsonDocument.Parse(body); }
        catch { return results; } // trả về không phải JSON (trang lỗi, hết hạn key...) -> không có gợi ý

        using (doc)
        {
            // Chấp nhận cả 2 dạng trả về của VietMap: GeoJSON {data:{features:[...]}} (v1.1)
            // và mảng phẳng [{ref_id, name, address, display, lat, lng}] (v3) — tên trường không
            // phân biệt hoa/thường (trước đây đọc sai hoa/thường -> gợi ý hiện ra TRỐNG chữ).
            var root = doc.RootElement;
            IEnumerable<JsonElement> items = Enumerable.Empty<JsonElement>();
            if (root.ValueKind == JsonValueKind.Array) items = root.EnumerateArray();
            else if (root.ValueKind == JsonValueKind.Object)
            {
                var data = Prop(root, "data");
                if (data is { ValueKind: JsonValueKind.Array } arrData) items = arrData.EnumerateArray();
                else if (data is { ValueKind: JsonValueKind.Object } objData && Prop(objData, "features") is { ValueKind: JsonValueKind.Array } feats)
                    items = feats.EnumerateArray();
                else if (Prop(root, "features") is { ValueKind: JsonValueKind.Array } rootFeats) items = rootFeats.EnumerateArray();
            }

            foreach (var item in items)
            {
                if (item.ValueKind != JsonValueKind.Object) continue;
                var props = Prop(item, "properties") is { ValueKind: JsonValueKind.Object } p ? p : item;

                var refId = Str(item, "Id") ?? Str(item, "ref_id") ?? Str(props, "ref_id") ?? Str(props, "id") ?? "";

                var name = Str(props, "name") ?? Str(props, "display") ?? Str(props, "label") ?? "";
                var address = Str(props, "address") ?? Str(props, "label") ?? Str(props, "display") ?? "";
                if (string.IsNullOrWhiteSpace(address))
                    address = string.Join(", ", new[] { Str(props, "housenumber"), Str(props, "street"), Str(props, "locality"),
                        Str(props, "county"), Str(props, "region") }.Where(x => !string.IsNullOrWhiteSpace(x)));
                if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(address)) continue; // không có gì để hiện

                double lat = 0, lng = 0;
                if (Prop(item, "geometry") is { ValueKind: JsonValueKind.Object } geom
                    && Prop(geom, "coordinates") is { ValueKind: JsonValueKind.Array } coords)
                {
                    var arr = coords.EnumerateArray().ToList();
                    // GeoJSON chuẩn: [longitude, latitude] — NGƯỢC thứ tự so với cách nói thông thường
                    if (arr.Count >= 2 && arr[0].ValueKind == JsonValueKind.Number && arr[1].ValueKind == JsonValueKind.Number)
                    {
                        lng = arr[0].GetDouble();
                        lat = arr[1].GetDouble();
                    }
                }
                if (lat == 0 && lng == 0)
                {
                    lat = Num(props, "lat") ?? Num(item, "lat") ?? 0;
                    lng = Num(props, "lng") ?? Num(props, "lon") ?? Num(item, "lng") ?? 0;
                }

                // Không có RefId lẫn tọa độ thì không dùng được (không lấy chi tiết được).
                if (string.IsNullOrEmpty(refId) && lat == 0 && lng == 0) continue;
                results.Add(new PlaceSuggestionDto(refId, name.Trim(), address.Trim(), lat, lng));
            }
        }
        return results;
    }

    /// <summary>Đọc thuộc tính KHÔNG phân biệt hoa/thường.</summary>
    private static JsonElement? Prop(JsonElement el, string name)
    {
        if (el.ValueKind != JsonValueKind.Object) return null;
        if (el.TryGetProperty(name, out var v)) return v;
        foreach (var p in el.EnumerateObject())
            if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)) return p.Value;
        return null;
    }

    private static string? Str(JsonElement el, string name)
        => Prop(el, name) is { } v ? v.ValueKind switch
        {
            JsonValueKind.String => string.IsNullOrWhiteSpace(v.GetString()) ? null : v.GetString(),
            JsonValueKind.Number => v.GetRawText(),
            _ => null,
        } : null;

    private static double? Num(JsonElement el, string name)
        => Prop(el, name) is { } v
            ? v.ValueKind == JsonValueKind.Number ? v.GetDouble()
              : v.ValueKind == JsonValueKind.String && double.TryParse(v.GetString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var d) ? d
              : null
            : null;

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

        var lat = Num(root, "lat");
        var lng = Num(root, "lng") ?? Num(root, "lon");
        if (lat is null || lng is null) return null;

        var name = Str(root, "name") ?? Str(root, "display") ?? "";
        var address = Str(root, "display") ?? Str(root, "address") ?? "";

        return new PlaceDetailDto(name, address, lat.Value, lng.Value);
    }
}