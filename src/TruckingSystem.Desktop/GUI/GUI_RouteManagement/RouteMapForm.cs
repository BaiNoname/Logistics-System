using System.Text;
using System.Text.Json;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_RouteManagement;
using TruckingSystem.Desktop.DTO_TripManagement;

namespace TruckingSystem.Desktop.GUI_RouteManagement
{
    /// <summary>Logic — hiển thị lộ trình thật giữa NHIỀU điểm dừng liên tiếp
    /// trên bản đồ VietMap (qua Leaflet nhúng trong WebView2). Chặng Lấy hàng
    /// có 3 điểm dừng (Kho→Đối tác→Kho, round-trip thật vì xe phải quay về),
    /// chặng Giao hàng chỉ 2 điểm (1 chiều) — vẽ TỪNG ĐOẠN nối tiếp nhau, cộng
    /// dồn khoảng cách/thời gian cả các đoạn. Giao diện (khung WebView2 + label
    /// thông tin) nằm ở RouteMapForm.Designer.cs.</summary>
    public partial class RouteMapForm : BaseForm
    {
        private ApiServices? _api;
        private List<RouteStopItem> _stops = new();
        private int? _tripId;

        public RouteMapForm()
        {
            InitializeComponent();
        }

        /// <summary>Dùng khi ĐÃ CÓ sẵn danh sách điểm dừng (VD gọi từ TripsControl
        /// sau khi tải qua GetTripRouteAsync).</summary>
        public RouteMapForm(ApiServices api, string title, List<RouteStopItem> stops, int? tripId = null) : this()
        {
            _api = api;
            _stops = stops;
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
                var validStops = _stops.Where(s => s.Lat.HasValue && s.Lng.HasValue).ToList();
                if (validStops.Count < 2)
                {
                    lblInfo.Text = "⚠️ Thiếu tọa độ ở 1 hoặc nhiều điểm dừng — không vẽ được lộ trình.";
                    lblInfo.ForeColor = Theme.Danger;
                    return;
                }

                // Tính lộ trình thật qua VietMap cho TỪNG ĐOẠN liên tiếp (VD 3
                // điểm dừng = 2 đoạn) — cộng dồn khoảng cách/thời gian các đoạn
                // lại để hiện đúng TỔNG thực tế (không chỉ 1 đoạn đơn lẻ).
                var segments = new List<RouteResultItem>();
                double totalDistanceKm = 0;
                int totalDurationMinutes = 0;
                foreach (var (from, to) in validStops.Zip(validStops.Skip(1), (a, b) => (a, b)))
                {
                    var seg = await _api.Routes.CalculateAsync(from.Lat!.Value, from.Lng!.Value, to.Lat!.Value, to.Lng!.Value);
                    segments.Add(seg);
                    if (seg.Success)
                    {
                        totalDistanceKm += seg.DistanceKm;
                        totalDurationMinutes += seg.DurationMinutes;
                    }
                }

                // Nếu có TripId -> tải kèm điểm GPS thật đã ghi nhận, vẽ chồng
                // lên bản đồ để so sánh với lộ trình lý thuyết.
                List<WaypointItem> waypoints = new();
                if (_tripId.HasValue)
                {
                    try { waypoints = await _api.Trips.GetWaypointsAsync(_tripId.Value); }
                    catch { /* không có GPS thật cũng không sao, vẫn hiện lộ trình lý thuyết */ }
                }

                var infoText = $"📏 {totalDistanceKm:0.0} km (lý thuyết, {validStops.Count - 1} đoạn)   •   ⏱ {totalDurationMinutes} phút   (nguồn: VietMap)";
                infoText += waypoints.Count > 0
                    ? $"   •   🛰️ {waypoints.Count} điểm GPS thật đã ghi nhận"
                    : "   •   🛰️ Chưa có dữ liệu GPS thật (xe chưa chạy hoặc chưa bật app)";
                lblInfo.Text = infoText;
                lblInfo.ForeColor = Theme.TextPrimary;

                await webView.EnsureCoreWebView2Async();
                webView.NavigateToString(BuildMapHtml(segments, validStops, waypoints));
            }
            catch (Exception ex)
            {
                lblInfo.Text = "Lỗi: " + ex.Message;
                lblInfo.ForeColor = Theme.Danger;
            }
        }

        /// <summary>Sinh HTML nhúng Leaflet + tile VietMap + vẽ NHIỀU polyline
        /// nối tiếp nhau (mỗi đoạn giữa 2 điểm dừng liên tiếp 1 polyline, cùng
        /// màu để trông liền mạch như 1 lộ trình xuyên suốt) — NÉT ĐỨT xanh =
        /// lộ trình LÝ THUYẾT, NÉT LIỀN cam = đường đi THỰC TẾ (GPS thật) —
        /// kèm marker + nhãn (Label) tại TỪNG điểm dừng để biết rõ đây là điểm
        /// gì (xuất phát/lấy hàng/về kho/giao hàng). Chạy hoàn toàn offline
        /// trong WebView2 nên nhúng thẳng API Key vào tile URL không sao.</summary>
        private static string BuildMapHtml(List<RouteResultItem> segments, List<RouteStopItem> stops, List<WaypointItem> waypoints)
        {
            var segmentsJson = JsonSerializer.Serialize(
                segments.Where(s => s.Success).Select(s => s.Points.Select(p => new[] { p.Lat, p.Lng }).ToList()).ToList());

            var stopsJson = JsonSerializer.Serialize(
                stops.Select(s => new { lat = s.Lat, lng = s.Lng, label = s.Label }).ToList());

            var actualPointsJson = JsonSerializer.Serialize(
                waypoints.Select(w => new[] { w.Lat, w.Lng }).ToList());

            var tileUrl = segments.FirstOrDefault(s => s.Success)?.TileUrlTemplate ?? "";

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
            sb.Append($"var segments = {segmentsJson};\n"); // mảng các đoạn, mỗi đoạn là mảng điểm [lat,lng]
            sb.Append($"var stops = {stopsJson};\n"); // mảng điểm dừng {lat, lng, label}
            sb.Append($"var actualPoints = {actualPointsJson};\n");

            sb.Append("""
            var map = L.map('map');
            L.tileLayer(tileUrl, { attribution: '&copy; VietMap', maxZoom: 19 }).addTo(map);

            var allBoundsPoints = [];

            // Vẽ TẤT CẢ các đoạn nối tiếp nhau — mỗi đoạn 1 polyline riêng,
            // cùng màu để trông liền mạch như 1 lộ trình xuyên suốt.
            segments.forEach(function (points) {
                if (points.length > 0) {
                    L.polyline(points, { color: '#2563EB', weight: 4, opacity: 0.7, dashArray: '10, 8' }).addTo(map);
                    allBoundsPoints = allBoundsPoints.concat(points);
                }
            });

            // Marker + nhãn tại TỪNG điểm dừng — biết rõ đây là điểm gì.
            stops.forEach(function (s, idx) {
                var icon = idx === 0 ? '📍' : (idx === stops.length - 1 ? '🏁' : '📦');
                L.marker([s.lat, s.lng]).addTo(map).bindPopup(icon + ' ' + s.label);
                allBoundsPoints.push([s.lat, s.lng]);
            });

            if (actualPoints.length > 0) {
                // Đường đi THỰC TẾ — nét liền cam, nối các điểm GPS thật
                L.polyline(actualPoints, { color: '#EA580C', weight: 5, opacity: 0.9 }).addTo(map);
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
