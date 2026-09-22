using System.Text;
using System.Text.Json;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_RouteManagement;

namespace TruckingSystem.Desktop.GUI_RouteManagement
{
    /// <summary>Logic — hiển thị lộ trình thật giữa 2 điểm tọa độ trên bản đồ VietMap
    /// (qua Leaflet nhúng trong WebView2). Giao diện (khung WebView2 + label thông
    /// tin) nằm ở RouteMapForm.Designer.cs.</summary>
    public partial class RouteMapForm : BaseForm
    {
        private ApiServices? _api;
        private double _fromLat, _fromLng, _toLat, _toLng;

        public RouteMapForm()
        {
            InitializeComponent();
        }

        public RouteMapForm(ApiServices api, string title, double fromLat, double fromLng, double toLat, double toLng) : this()
        {
            _api = api;
            _fromLat = fromLat; _fromLng = fromLng; _toLat = toLat; _toLng = toLng;
            Text = $"Lộ trình — {title}";
            lblInfo.Text = "Đang tính toán lộ trình qua VietMap...";

            Load += async (_, _) => await LoadRouteAsync();
        }

        private async Task LoadRouteAsync()
        {
            if (_api is null) return;
            try
            {
                var route = await _api.Routes.CalculateAsync(_fromLat, _fromLng, _toLat, _toLng);

                if (!route.Success)
                {
                    lblInfo.Text = "⚠️ " + (route.Message ?? "Không tính được lộ trình.");
                    lblInfo.ForeColor = Theme.Danger;
                    return;
                }

                lblInfo.Text = $"📏 {route.DistanceKm} km   •   ⏱ {route.DurationMinutes} phút   (nguồn: VietMap)";
                lblInfo.ForeColor = Theme.TextPrimary;

                await webView.EnsureCoreWebView2Async();
                webView.NavigateToString(BuildMapHtml(route));
            }
            catch (Exception ex)
            {
                lblInfo.Text = "Lỗi: " + ex.Message;
                lblInfo.ForeColor = Theme.Danger;
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
}
