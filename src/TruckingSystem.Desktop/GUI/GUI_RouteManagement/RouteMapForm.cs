using System.Text;
using System.Text.Json;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_RouteManagement;
using TruckingSystem.Desktop.DTO_TripManagement;

namespace TruckingSystem.Desktop.GUI_RouteManagement
{
    /// <summary>Logic — hiển thị lộ trình giữa NHIỀU điểm dừng liên tiếp trên
    /// bản đồ VietMap (qua Leaflet nhúng trong WebView2). Chặng Lấy hàng có 3
    /// điểm dừng (Kho→Đối tác→Kho, round-trip thật), chặng Giao hàng chỉ 2
    /// điểm (1 chiều). Giao diện (khung WebView2 + label) ở RouteMapForm.Designer.cs.
    ///
    /// CẬP NHẬT: khi có TripId, hoạt động Y HỆT app tài xế (navigate.html):
    ///   - CHƯA có GPS thật: hiện đường lý thuyết (xanh nét đứt) từ điểm xuất
    ///     phát ban đầu — xem trước khi xe chạy.
    ///   - CÓ GPS thật: XÓA HẲN đường lý thuyết cũ, thay bằng đường ĐỘNG tính
    ///     lại mỗi 15 giây từ VỊ TRÍ THẬT hiện tại tới (các) điểm dừng còn lại
    ///     — tránh rối mắt vì 2 đường chồng/cắt nhau khi xe đi lệch.</summary>
    public partial class RouteMapForm : BaseForm
    {
        private ApiServices? _api;
        private List<RouteStopItem> _stops = new();
        private int? _tripId;
        private System.Windows.Forms.Timer? _liveTimer;
        private int _currentStopIndex = 0;
        private bool _isCompleted = false;
        private const double ArrivalThresholdMeters = 60; // gần hơn mức này coi như đã tới điểm dừng, chuyển điểm kế tiếp

        public RouteMapForm()
        {
            InitializeComponent();
        }

        /// <summary>Dùng khi ĐÃ CÓ sẵn danh sách điểm dừng (VD gọi từ TripsControl
        /// sau khi tải qua GetTripRouteAsync).</summary>
        public RouteMapForm(ApiServices api, string title, List<RouteStopItem> stops, int? tripId = null, string? tripStatus = null) : this()
        {
            _api = api;
            _stops = stops;
            _tripId = tripId;
            _isCompleted = tripStatus == "Completed";
            Text = $"Lộ trình — {title}";
            lblInfo.Text = "Đang tải...";

            Load += async (_, _) => await LoadRouteAsync();

            // Chuyến ĐÃ Hoàn tất hẳn -> không còn gì để theo dõi trực tiếp nữa
            // (GPS đã ngừng ghi từ lúc tài xế báo xong — xem TripService.PingAsync),
            // nên KHÔNG bật Live Timer, chỉ xem lại toàn bộ đường đã đi.
            if (_tripId.HasValue && !_isCompleted)
            {
                _liveTimer = new System.Windows.Forms.Timer { Interval = 15000 }; // 15 giây/lần
                _liveTimer.Tick += async (_, _) => await RefreshLiveRouteAsync();
                FormClosed += (_, _) => _liveTimer?.Stop();
            }
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

                // Lấy GPS thật (nếu có) TRƯỚC, để quyết định vẽ lý thuyết hay
                // vẽ động ngay từ lần tải đầu tiên.
                List<WaypointItem> waypoints = new();
                if (_tripId.HasValue)
                {
                    try { waypoints = await _api.Trips.GetWaypointsAsync(_tripId.Value); }
                    catch { /* không có GPS thật cũng không sao */ }
                }

                // Chuyến ĐÃ Hoàn tất hẳn -> chỉ XEM LẠI toàn bộ đường GPS thật
                // đã đi (replay lịch sử), KHÔNG tính lộ trình lý thuyết, KHÔNG
                // tính đường động, KHÔNG gọi VietMap gì thêm — nhẹ và đúng bản
                // chất "xem lại", không phải "theo dõi trực tiếp" nữa.
                if (_isCompleted)
                {
                    lblInfo.Text = waypoints.Count > 0
                        ? $"🛰️ {waypoints.Count} điểm GPS thật đã ghi nhận (chuyến đã hoàn tất — xem lại lịch sử)"
                        : "🛰️ Chuyến đã hoàn tất nhưng không có dữ liệu GPS nào được ghi nhận.";
                    lblInfo.ForeColor = Theme.TextPrimary;

                    // Gọi 1 lần duy nhất CHỈ để lấy đúng ảnh nền (tileUrlTemplate)
                    // của VietMap — KHÔNG dùng polyline lý thuyết của kết quả này.
                    var tileProbe = await _api.Routes.CalculateAsync(
                        validStops[0].Lat!.Value, validStops[0].Lng!.Value,
                        validStops[^1].Lat!.Value, validStops[^1].Lng!.Value);

                    await webView.EnsureCoreWebView2Async();
                    webView.NavigateToString(BuildHistoryMapHtml(validStops, waypoints, tileProbe.Success ? tileProbe.TileUrlTemplate : ""));
                    return;
                }

                var segments = new List<RouteResultItem>();
                double totalDistanceKm = 0;
                int totalDurationMinutes = 0;

                if (waypoints.Count == 0)
                {
                    // CHƯA có GPS thật -> vẽ lộ trình LÝ THUYẾT từ điểm xuất
                    // phát ban đầu, y hệt hành vi cũ.
                    foreach (var (from, to) in validStops.Zip(validStops.Skip(1), (a, b) => (a, b)))
                    {
                        var seg = await _api.Routes.CalculateAsync(from.Lat!.Value, from.Lng!.Value, to.Lat!.Value, to.Lng!.Value);
                        segments.Add(seg);
                        if (seg.Success) { totalDistanceKm += seg.DistanceKm; totalDurationMinutes += seg.DurationMinutes; }
                    }
                }
                else
                {
                    // ĐÃ có GPS thật -> bỏ qua lộ trình lý thuyết hoàn toàn,
                    // tính luôn đường ĐỘNG từ vị trí GPS mới nhất, xác định
                    // đúng _currentStopIndex (điểm dừng nào đã đi qua rồi).
                    var current = waypoints[^1];
                    // Có GPS = Trip CHẮC CHẮN đã rời điểm xuất phát (index 0)
                    // rồi — không cần đoán, luôn bỏ qua điểm 0, hướng tới điểm
                    // dừng thứ 2 trước (Đối tác với chặng Lấy hàng, hoặc Điểm
                    // giao với chặng Giao hàng chỉ có 2 điểm).
                    _currentStopIndex = validStops.Count > 1 ? 1 : 0;
                    (segments, totalDistanceKm, totalDurationMinutes) = await CalculateDynamicSegmentsAsync(current.Lat, current.Lng, validStops);
                }

                UpdateInfoLabel(totalDistanceKm, totalDurationMinutes, waypoints.Count, isDynamic: waypoints.Count > 0);

                await webView.EnsureCoreWebView2Async();
                webView.NavigateToString(BuildMapHtml(segments, validStops, waypoints.Count > 0, waypoints.Count > 0 ? waypoints[^1] : null));

                if (_liveTimer != null)
                {
                    webView.CoreWebView2.NavigationCompleted += (_, _) => _liveTimer.Start();
                }
            }
            catch (Exception ex)
            {
                lblInfo.Text = "Lỗi: " + ex.Message;
                lblInfo.ForeColor = Theme.Danger;
            }
        }

        /// <summary>Mỗi 15 giây: lấy vị trí GPS mới nhất, kiểm tra đã tới gần
        /// điểm dừng hiện tại chưa (tự chuyển điểm kế tiếp nếu có), rồi tính
        /// lại đường ĐỘNG từ vị trí đó tới (các) điểm dừng còn lại — gọi thẳng
        /// vào trang đang mở để vẽ lại, không tải lại toàn bộ bản đồ.</summary>
        private async Task RefreshLiveRouteAsync()
        {
            if (_api is null || !_tripId.HasValue) return;
            try
            {
                var waypoints = await _api.Trips.GetWaypointsAsync(_tripId.Value);
                if (waypoints.Count == 0) return; // chuyến vẫn chưa có GPS nào, giữ nguyên lộ trình lý thuyết đang hiện

                var validStops = _stops.Where(s => s.Lat.HasValue && s.Lng.HasValue).ToList();
                var current = waypoints[^1];

                if (_currentStopIndex < validStops.Count)
                {
                    var distToTarget = HaversineMeters(current.Lat, current.Lng, validStops[_currentStopIndex].Lat!.Value, validStops[_currentStopIndex].Lng!.Value);
                    if (distToTarget < ArrivalThresholdMeters && _currentStopIndex < validStops.Count - 1)
                        _currentStopIndex++;
                }

                var (segments, totalDistanceKm, totalDurationMinutes) = await CalculateDynamicSegmentsAsync(current.Lat, current.Lng, validStops);

                var segmentsJson = JsonSerializer.Serialize(
                    segments.Where(s => s.Success).Select(s => s.Points.Select(p => new[] { p.Lat, p.Lng }).ToList()).ToList());
                var posJson = JsonSerializer.Serialize(new[] { current.Lat, current.Lng });

                await webView.ExecuteScriptAsync($"if (window.updateDynamicRoute) updateDynamicRoute({segmentsJson}, {posJson});");

                UpdateInfoLabel(totalDistanceKm, totalDurationMinutes, waypoints.Count, isDynamic: true);
            }
            catch
            {
                // Mất mạng/API lỗi 1 lần không sao — thử lại ở lần Tick kế tiếp.
            }
        }

        /// <summary>Tính các đoạn đường THẬT (qua VietMap) từ (fromLat,fromLng)
        /// đi qua hết các điểm dừng CÒN LẠI (từ _currentStopIndex trở đi).</summary>
        private async Task<(List<RouteResultItem> Segments, double TotalKm, int TotalMin)> CalculateDynamicSegmentsAsync(
            double fromLat, double fromLng, List<RouteStopItem> validStops)
        {
            var segments = new List<RouteResultItem>();
            double totalKm = 0;
            int totalMin = 0;
            var curLat = fromLat;
            var curLng = fromLng;

            for (int i = _currentStopIndex; i < validStops.Count; i++)
            {
                var to = validStops[i];
                var seg = await _api!.Routes.CalculateAsync(curLat, curLng, to.Lat!.Value, to.Lng!.Value);
                segments.Add(seg);
                if (seg.Success) { totalKm += seg.DistanceKm; totalMin += seg.DurationMinutes; }
                curLat = to.Lat!.Value;
                curLng = to.Lng!.Value;
            }
            return (segments, totalKm, totalMin);
        }

        private static double HaversineMeters(double lat1, double lng1, double lat2, double lng2)
        {
            const double earthRadiusM = 6371000;
            var dLat = (lat2 - lat1) * Math.PI / 180.0;
            var dLng = (lng2 - lng1) * Math.PI / 180.0;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                    Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            return earthRadiusM * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private void UpdateInfoLabel(double totalDistanceKm, int totalDurationMinutes, int waypointCount, bool isDynamic)
        {
            var modeText = isDynamic ? "còn lại, tính từ vị trí hiện tại" : "lý thuyết";
            var infoText = $"📏 {totalDistanceKm:0.0} km ({modeText})   •   ⏱ {totalDurationMinutes} phút   (nguồn: VietMap)   •   🛰️ {waypointCount} điểm GPS";
            if (isDynamic) infoText += "   (🔴 đang theo dõi trực tiếp)";
            lblInfo.Text = infoText;
            lblInfo.ForeColor = Theme.TextPrimary;
        }

        /// <summary>Sinh HTML đơn giản — CHỈ vẽ đường GPS thật đã ghi nhận (nét
        /// liền cam) cho 1 chuyến ĐÃ HOÀN TẤT, xem như "xem lại lịch sử", kèm
        /// marker điểm bắt đầu/kết thúc hành trình thật. Không vẽ lộ trình lý
        /// thuyết, không có logic động/polling nào — tĩnh hoàn toàn.</summary>
        private static string BuildHistoryMapHtml(List<RouteStopItem> stops, List<WaypointItem> waypoints, string? tileUrlTemplate)
        {
            var stopsJson = JsonSerializer.Serialize(
                stops.Select(s => new { lat = s.Lat, lng = s.Lng, label = s.Label }).ToList());
            var pointsJson = JsonSerializer.Serialize(waypoints.Select(w => new[] { w.Lat, w.Lng }).ToList());
            var tileUrl = tileUrlTemplate ?? "";

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
            sb.Append($"var stops = {stopsJson};\n");
            sb.Append($"var actualPoints = {pointsJson};\n");

            sb.Append("""
            var map = L.map('map');
            L.tileLayer(tileUrl, { attribution: '&copy; VietMap', maxZoom: 19 }).addTo(map);

            var allBoundsPoints = [];

            stops.forEach(function (s, idx) {
                var icon = idx === 0 ? '📍' : (idx === stops.length - 1 ? '🏁' : '📦');
                L.marker([s.lat, s.lng]).addTo(map).bindPopup(icon + ' ' + s.label);
                allBoundsPoints.push([s.lat, s.lng]);
            });

            if (actualPoints.length > 0) {
                L.polyline(actualPoints, { color: '#EA580C', weight: 5, opacity: 0.9 }).addTo(map);
                allBoundsPoints = allBoundsPoints.concat(actualPoints);

                L.circleMarker(actualPoints[0], { radius: 6, color: '#EA580C', fillColor: '#EA580C', fillOpacity: 1 })
                    .addTo(map).bindPopup('🛰️ Bắt đầu hành trình');
                L.circleMarker(actualPoints[actualPoints.length - 1], { radius: 6, color: '#EA580C', fillColor: '#EA580C', fillOpacity: 1 })
                    .addTo(map).bindPopup('🛰️ Kết thúc hành trình');
            }

            if (allBoundsPoints.length > 0) {
                map.fitBounds(L.latLngBounds(allBoundsPoints), { padding: [30, 30] });
            } else {
                map.setView([10.7769, 106.7009], 12);
            }
            </script>
            </body>
            </html>
            """);

            return sb.ToString();
        }

        /// <summary>Sinh HTML nhúng Leaflet + tile VietMap. isDynamic=false: vẽ
        /// lộ trình lý thuyết (xanh nét đứt, không đổi). isDynamic=true: vẽ
        /// ngay đường động ban đầu (xanh nét liền) + 1 chấm vị trí hiện tại,
        /// không có đường lý thuyết nào cả.
        ///
        /// Hàm JS "updateDynamicRoute" gắn vào window để C# gọi lại mỗi 15
        /// giây — tự xóa đường ĐỘNG cũ (và đường lý thuyết CŨ nếu đây là lần
        /// đầu chuyển từ lý thuyết sang động) rồi vẽ lại bằng dữ liệu mới,
        /// không đụng gì tới zoom hiện tại của Dispatcher.</summary>
        private static string BuildMapHtml(List<RouteResultItem> segments, List<RouteStopItem> stops, bool isDynamic, WaypointItem? currentPos)
        {
            var segmentsJson = JsonSerializer.Serialize(
                segments.Where(s => s.Success).Select(s => s.Points.Select(p => new[] { p.Lat, p.Lng }).ToList()).ToList());

            var stopsJson = JsonSerializer.Serialize(
                stops.Select(s => new { lat = s.Lat, lng = s.Lng, label = s.Label }).ToList());

            var tileUrl = segments.FirstOrDefault(s => s.Success)?.TileUrlTemplate ?? "";
            var currentPosJson = currentPos != null ? JsonSerializer.Serialize(new[] { currentPos.Lat, currentPos.Lng }) : "null";

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
            sb.Append($"var segments = {segmentsJson};\n");
            sb.Append($"var stops = {stopsJson};\n");
            sb.Append($"var isDynamic = {(isDynamic ? "true" : "false")};\n");
            sb.Append($"var initialPos = {currentPosJson};\n");

            sb.Append("""
            var map = L.map('map');
            L.tileLayer(tileUrl, { attribution: '&copy; VietMap', maxZoom: 19 }).addTo(map);

            var allBoundsPoints = [];
            var theoreticalLines = [];  // đường lý thuyết — chỉ tồn tại nếu isDynamic=false lúc đầu
            var dynamicLines = [];      // đường động — cập nhật lại mỗi lần poll
            var currentPosMarker = null;

            // Marker + nhãn tại TỪNG điểm dừng — biết rõ đây là điểm gì.
            stops.forEach(function (s, idx) {
                var icon = idx === 0 ? '📍' : (idx === stops.length - 1 ? '🏁' : '📦');
                L.marker([s.lat, s.lng]).addTo(map).bindPopup(icon + ' ' + s.label);
                allBoundsPoints.push([s.lat, s.lng]);
            });

            if (!isDynamic) {
                // CHƯA có GPS thật — vẽ đường lý thuyết xanh nét đứt, y hệt
                // hành vi cũ, không có gì thay đổi khi xem TRƯỚC lúc xe chạy.
                segments.forEach(function (points) {
                    if (points.length > 0) {
                        theoreticalLines.push(L.polyline(points, { color: '#2563EB', weight: 4, opacity: 0.7, dashArray: '10, 8' }).addTo(map));
                        allBoundsPoints = allBoundsPoints.concat(points);
                    }
                });
            }

            // ---- Đường ĐỘNG (xanh, nét liền) — gọi lại được nhiều lần từ C#
            // qua window.updateDynamicRoute mỗi khi có GPS mới. Lần gọi ĐẦU
            // TIÊN sẽ tự xóa nốt đường lý thuyết cũ (nếu còn) — từ đây về sau
            // CHỈ còn duy nhất đường động, không rối mắt vì 2 đường chồng nhau. ----
            function updateDynamicRoute(newSegments, pos) {
                theoreticalLines.forEach(function (l) { map.removeLayer(l); });
                theoreticalLines = [];

                dynamicLines.forEach(function (l) { map.removeLayer(l); });
                dynamicLines = [];

                newSegments.forEach(function (points) {
                    if (points.length > 0) {
                        dynamicLines.push(L.polyline(points, { color: '#2563EB', weight: 5, opacity: 0.9 }).addTo(map));
                    }
                });

                if (currentPosMarker) { map.removeLayer(currentPosMarker); }
                currentPosMarker = L.circleMarker(pos, { radius: 8, color: '#EA580C', fillColor: '#EA580C', fillOpacity: 1, weight: 2 })
                    .addTo(map).bindPopup('🛰️ Vị trí hiện tại').openPopup();
            }
            window.updateDynamicRoute = updateDynamicRoute;

            if (isDynamic && segments.length > 0) {
                updateDynamicRoute(segments, initialPos);
                allBoundsPoints.push(initialPos);
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