using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TruckingSystem.Application.Routing;

namespace TruckingSystem.Infrastructure.ExternalServices;

/// <summary>
/// Gọi VietMap Routing API v1.1 (https://maps.vietmap.vn/docs/map-api/route/)
/// để tính khoảng cách, thời gian và tuyến đường thật giữa 2 điểm tọa độ.
/// Cần cấu hình "VietMap:ApiKey" trong appsettings.json.
/// </summary>
public class VietMapRouteProvider : IRouteProvider
{
    private readonly HttpClient _http;
    private readonly string _routeApiKey;
    private readonly string _mapApiKey;

    public VietMapRouteProvider(HttpClient http, IConfiguration config)
    {
        _http = http;
        _routeApiKey = config["VietMap:RouteApiKey"]
            ?? throw new InvalidOperationException("Thiếu cấu hình VietMap:RouteApiKey trong appsettings.json.");
        _mapApiKey = config["VietMap:MapApiKey"]
            ?? throw new InvalidOperationException("Thiếu cấu hình VietMap:MapApiKey trong appsettings.json.");
    }

    // ---- BỘ NHỚ ĐỆM tuyến đường: cùng 2 điểm (làm tròn ~1 m) trong 6 giờ thì dùng lại kết quả, không gọi VietMap nữa.
    // Chia xe tự động, ước tính chuyến, mở bản đồ tuyến... hỏi lại cùng các cặp điểm rất nhiều lần — trước đây lần nào
    // cũng chờ VietMap (chậm + tốn lượt gọi API). Chỉ nhớ kết quả THÀNH CÔNG; quá 5.000 mục thì xóa bớt cho gọn.
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, (RouteResult Result, DateTime At)> Cache = new();
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(6);

    public async Task<RouteResult> GetRouteAsync(RouteRequest request)
    {
        var key = FormattableString.Invariant(
            $"{request.FromLat:F5},{request.FromLng:F5}>{request.ToLat:F5},{request.ToLng:F5}");
        if (Cache.TryGetValue(key, out var hit) && DateTime.UtcNow - hit.At < CacheTtl) return hit.Result;

        var result = await FetchRouteAsync(request);
        if (result.Success)
        {
            if (Cache.Count > 5000) Cache.Clear();
            Cache[key] = (result, DateTime.UtcNow);
        }
        return result;
    }

    private async Task<RouteResult> FetchRouteAsync(RouteRequest request)
    {
        // vehicle=car dùng tạm cho v1.1 (API v1.1 chỉ hỗ trợ car/bike/foot/motorcycle,
        // chưa có "truck" — muốn định tuyến né đường cấm tải nặng cần nâng cấp lên
        // Route v3 của VietMap sau này, có tham số vehicle=truck + capacity).
        var url = "https://maps.vietmap.vn/api/route" +
                  $"?api-version=1.1&apikey={_routeApiKey}" +
                  $"&point={request.FromLat.ToString(System.Globalization.CultureInfo.InvariantCulture)},{request.FromLng.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                  $"&point={request.ToLat.ToString(System.Globalization.CultureInfo.InvariantCulture)},{request.ToLng.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                  "&points_encoded=true&vehicle=car";

        HttpResponseMessage resp;
        string body;
        try
        {
            resp = await _http.GetAsync(url);
            body = await resp.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            return new RouteResult(false, 0, 0, new(), null, "Không kết nối được VietMap API: " + ex.Message);
        }

        if (!resp.IsSuccessStatusCode)
            return new RouteResult(false, 0, 0, new(), null, $"VietMap API trả lỗi ({(int)resp.StatusCode}): {body}");

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        if (!root.TryGetProperty("paths", out var paths) || paths.GetArrayLength() == 0)
        {
            var msg = root.TryGetProperty("messages", out var m) ? m.ToString() : "Không tìm được lộ trình giữa 2 điểm này.";
            return new RouteResult(false, 0, 0, new(), null, msg);
        }

        var path = paths[0];
        var distanceMeters = path.GetProperty("distance").GetDouble();
        var timeMs = path.GetProperty("time").GetInt64();
        var encodedPoints = path.GetProperty("points").GetString() ?? "";

        var points = DecodePolyline(encodedPoints);

        return new RouteResult(
            Success: true,
            DistanceKm: Math.Round(distanceMeters / 1000.0, 2),
            DurationMinutes: (int)Math.Ceiling(timeMs / 60000.0),
            Points: points,
            TileUrlTemplate: $"https://maps.vietmap.vn/tm/{{z}}/{{x}}/{{y}}.png?apikey={_mapApiKey}",
            Message: null
        );
    }

    /// <summary>Giải mã chuỗi Polyline theo thuật toán Google Polyline Algorithm
    /// (độ chính xác 5 chữ số thập phân) — đúng định dạng VietMap trả về ở field
    /// "points" khi points_encoded=true.</summary>
    private static List<RoutePoint> DecodePolyline(string encoded)
    {
        var result = new List<RoutePoint>();
        int index = 0, lat = 0, lng = 0;

        while (index < encoded.Length)
        {
            int shift = 0, b, deltaLat = 0;
            do
            {
                b = encoded[index++] - 63;
                deltaLat |= (b & 0x1f) << shift;
                shift += 5;
            } while (b >= 0x20);
            lat += (deltaLat & 1) != 0 ? ~(deltaLat >> 1) : (deltaLat >> 1);

            shift = 0;
            int deltaLng = 0;
            do
            {
                b = encoded[index++] - 63;
                deltaLng |= (b & 0x1f) << shift;
                shift += 5;
            } while (b >= 0x20);
            lng += (deltaLng & 1) != 0 ? ~(deltaLng >> 1) : (deltaLng >> 1);

            result.Add(new RoutePoint(lat / 1e5, lng / 1e5));
        }

        return result;
    }
}