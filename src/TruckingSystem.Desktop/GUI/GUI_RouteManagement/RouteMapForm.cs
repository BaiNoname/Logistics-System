using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_RouteManagement;

namespace TruckingSystem.Desktop.GUI_RouteManagement;

/// <summary>
/// Hiển thị lộ trình thật giữa 2 điểm tọa độ trên bản đồ VietMap (qua Leaflet
/// nhúng trong WebView2). Gọi Backend để lấy tuyến đường đã tính sẵn (khoảng
/// cách, thời gian, danh sách điểm vẽ polyline) — Form chỉ có nhiệm vụ hiển thị.
/// </summary>
public class RouteMapForm : BaseForm
{
    private readonly ApiServices _api;
    private readonly double _fromLat, _fromLng, _toLat, _toLng;

    private readonly WebView2 _webView = new() { Dock = DockStyle.Fill };
    private readonly Label _lblInfo = new() { Dock = DockStyle.Top, Height = 36, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 0, 0), Font = Theme.FontBold, BackColor = Theme.CardBg };

    public RouteMapForm(ApiServices api, string title, double fromLat, double fromLng, double toLat, double toLng)
    {
        _api = api;
        _fromLat = fromLat; _fromLng = fromLng; _toLat = toLat; _toLng = toLng;

        Text = $"Lộ trình — {title}";
        Width = 900; Height = 700;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.White;

        _lblInfo.Text = "Đang tính toán lộ trình qua VietMap...";

        Controls.Add(_webView);
        Controls.Add(_lblInfo);

        Load += async (_, _) => await LoadRouteAsync();
    }

    private async Task LoadRouteAsync()
    {
        try
        {
            var route = await _api.Routes.CalculateAsync(_fromLat, _fromLng, _toLat, _toLng);

            if (!route.Success)
            {
                _lblInfo.Text = "⚠️ " + (route.Message ?? "Không tính được lộ trình.");
                _lblInfo.ForeColor = Theme.Danger;
                return;
            }

            _lblInfo.Text = $"📏 {route.DistanceKm} km   •   ⏱ {route.DurationMinutes} phút   (nguồn: VietMap)";
            _lblInfo.ForeColor = Theme.TextPrimary;

            await _webView.EnsureCoreWebView2Async();
            _webView.NavigateToString(BuildMapHtml(route));
        }
        catch (Exception ex)
        {
            _lblInfo.Text = "Lỗi: " + ex.Message;
            _lblInfo.ForeColor = Theme.Danger;
        }
    }

    /// <summary>Sinh HTML nhúng Leaflet + tile VietMap + vẽ polyline từ danh sách
    /// điểm Backend đã tính sẵn. Chạy hoàn toàn offline trong WebView2 (không
    /// phải trang web public) nên nhúng thẳng API Key vào tile URL không sao —
    /// chỉ hiển thị nội bộ trên máy người dùng, không public ra ngoài.</summary>
    private static string BuildMapHtml(RouteResultItem route)
    {
        var pointsJson = JsonSerializer.Serialize(
            route.Points.Select(p => new[] { p.Lat, p.Lng }).ToList());

        var tileUrl = route.TileUrlTemplate ?? "";
        var fromPoint = route.Points.FirstOrDefault();
        var toPoint = route.Points.LastOrDefault();

        var sb = new StringBuilder();
        sb.Append("""
        <!DOCTYPE html>
        <html>
        <head>
        <meta charset="utf-8" />
        <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />
        <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
        <style>html, body, #map { height: 100%; margin: 0; padding: 0; }</style>
        </head>
        <body>
        <div id="map"></div>
        <script>
        """);

        sb.Append($"var tileUrl = {JsonSerializer.Serialize(tileUrl)};\n");
        sb.Append($"var points = {pointsJson};\n");

        sb.Append("""
        var map = L.map('map');
        L.tileLayer(tileUrl, { attribution: '&copy; VietMap', maxZoom: 19 }).addTo(map);

        if (points.length > 0) {
            var polyline = L.polyline(points, { color: '#2563EB', weight: 5, opacity: 0.85 }).addTo(map);
            map.fitBounds(polyline.getBounds(), { padding: [30, 30] });

            L.marker(points[0]).addTo(map).bindPopup('📦 Điểm lấy hàng');
            L.marker(points[points.length - 1]).addTo(map).bindPopup('🏁 Điểm giao hàng');
        } else {
            map.setView([10.7769, 106.7009], 12); // fallback: trung tâm TP.HCM
        }
        </script>
        </body>
        </html>
        """);

        return sb.ToString();
    }
}
