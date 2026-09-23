using System.Text;
using System.Text.Json;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_RouteManagement;
using TruckingSystem.Desktop.DTO_TripManagement;

namespace TruckingSystem.Desktop.GUI_RouteManagement
{
    /// <summary>Logic — hiển thị lộ trình thật giữa 2 điểm tọa độ trên bản đồ VietMap
    /// (qua Leaflet nhúng trong WebView2). Giao diện (khung WebView2 + label thông
    /// tin) nằm ở RouteMapForm.Designer.cs.</summary>
    public partial class RouteMapForm : BaseForm
    {
        private ApiServices? _api;
        private double _fromLat, _fromLng, _toLat, _toLng;
        private int? _tripId;

        public RouteMapForm()
        {
            InitializeComponent();
        }

        public RouteMapForm(ApiServices api, string title, double fromLat, double fromLng, double toLat, double toLng, int? tripId = null) : this()
        {
            _api = api;
            _fromLat = fromLat; _fromLng = fromLng; _toLat = toLat; _toLng = toLng;
            _tripId = tripId;
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

                // Nếu có TripId -> tải kèm điểm GPS thật đã ghi nhận, vẽ chồng
                // lên bản đồ để so sánh với lộ trình lý thuyết.
                List<WaypointItem> waypoints = new();
                if (_tripId.HasValue)
                {
                    try { waypoints = await _api.Trips.GetWaypointsAsync(_tripId.Value); }
                    catch { /* không có GPS thật cũng không sao, vẫn hiện lộ trình lý thuyết */ }
                }

                var infoText = $"📏 {route.DistanceKm} km (lý thuyết)   •   ⏱ {route.DurationMinutes} phút   (nguồn: VietMap)";
                infoText += waypoints.Count > 0
                    ? $"   •   🛰️ {waypoints.Count} điểm GPS thật đã ghi nhận"
                    : "   •   🛰️ Chưa có dữ liệu GPS thật (xe chưa chạy hoặc chưa bật app)";
                lblInfo.Text = infoText;
                lblInfo.ForeColor = Theme.TextPrimary;

                await webView.EnsureCoreWebView2Async();
                webView.NavigateToString(BuildMapHtml(route, waypoints));
            }
            catch (Exception ex)
            {
                lblInfo.Text = "Lỗi: " + ex.Message;
                lblInfo.ForeColor = Theme.Danger;
            }
        }

        /// <summary>Sinh HTML nhúng Leaflet + tile VietMap + vẽ 2 polyline —
        /// NÉT ĐỨT xanh = lộ trình LÝ THUYẾT (VietMap Route API tính sẵn),
        /// NÉT LIỀN cam = đường đi THỰC TẾ (nối các điểm GPS thật xe đã đi
        /// qua) — giúp so sánh trực quan xe có đi lệch tuyến hay không. Chạy
        /// hoàn toàn offline trong WebView2 (không phải trang web public) nên
        /// nhúng thẳng API Key vào tile URL không sao — chỉ hiển thị nội bộ
        /// trên máy người dùng, không public ra ngoài.</summary>
        private static string BuildMapHtml(RouteResultItem route, List<WaypointItem> waypoints)
        {
            var pointsJson = JsonSerializer.Serialize(
                route.Points.Select(p => new[] { p.Lat, p.Lng }).ToList());

            var actualPointsJson = JsonSerializer.Serialize(
                waypoints.Select(w => new[] { w.Lat, w.Lng }).ToList());

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
            sb.Append($"var actualPoints = {actualPointsJson};\n");

            sb.Append("""
            var map = L.map('map');
            L.tileLayer(tileUrl, { attribution: '&copy; VietMap', maxZoom: 19 }).addTo(map);

            var allBoundsPoints = [];

            if (points.length > 0) {
                // Lộ trình LÝ THUYẾT — nét đứt xanh
                var polyline = L.polyline(points, { color: '#2563EB', weight: 4, opacity: 0.7, dashArray: '10, 8' }).addTo(map);
                allBoundsPoints = allBoundsPoints.concat(points);

                L.marker(points[0]).addTo(map).bindPopup('📦 Điểm đi (lý thuyết)');
                L.marker(points[points.length - 1]).addTo(map).bindPopup('🏁 Điểm đến (lý thuyết)');
            }

            if (actualPoints.length > 0) {
                // Đường đi THỰC TẾ — nét liền cam, nối các điểm GPS thật
                var actualLine = L.polyline(actualPoints, { color: '#EA580C', weight: 5, opacity: 0.9 }).addTo(map);
                allBoundsPoints = allBoundsPoints.concat(actualPoints);

                L.circleMarker(actualPoints[0], { radius: 6, color: '#EA580C', fillColor: '#EA580C', fillOpacity: 1 })
                    .addTo(map).bindPopup('🛰️ Bắt đầu GPS thật');
                L.circleMarker(actualPoints[actualPoints.length - 1], { radius: 6, color: '#EA580C', fillColor: '#EA580C', fillOpacity: 1 })
                    .addTo(map).bindPopup('🛰️ Điểm GPS mới nhất');
            }

            if (allBoundsPoints.length > 0) {
                map.fitBounds(L.latLngBounds(allBoundsPoints), { padding: [30, 30] });
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