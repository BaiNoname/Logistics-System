using System.Text;
using System.Text.Json;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_RouteManagement;
using TruckingSystem.Desktop.DTO_TripManagement;
using Microsoft.Web.WebView2.Core;

namespace TruckingSystem.Desktop.GUI_RouteManagement
{
    /// <summary>Logic — hiển thị lộ trình nhiều điểm dừng trên bản đồ VietMap
    /// (Leaflet nhúng trong WebView2). Giao diện ở RouteMapForm.Designer.cs.
    ///
    /// Hoạt động Y HỆT app tài xế (navigate.html):
    ///   - Icon: điểm khởi hành xanh lá, kho vàng, điểm đến đỏ, vị trí xe là
    ///     mũi tên xám xoay theo hướng chạy (hướng tính từ 2 điểm GPS gần nhất).
    ///   - CHƯA có GPS: hiện đường lý thuyết (nét đứt) để xem trước.
    ///   - CÓ GPS: mỗi 15 giây lấy vị trí mới, CẮT BỎ phần đường đã đi qua ngay
    ///     trên bản đồ (không gọi VietMap). Chỉ gọi lại VietMap khi xe lệch
    ///     tuyến quá 100m — giống Google Maps.
    ///   - Chuyến ĐÃ hoàn tất / CHỜ duyệt: xem lại đường GPS thật đã đi, kèm
    ///     VIDEO HÀNH TRÌNH (nếu tài xế bật quay): bấm 1 chỗ trên đường -> phát
    ///     video đúng lúc xe đi qua chỗ đó; video chạy thì mũi tên trên bản đồ
    ///     chạy theo; nút "Kiểm tra" đối chiếu SHA-256 chứng minh video chưa bị sửa.</summary>
    public partial class RouteMapForm : BaseForm
    {
        private ApiServices? _api;
        private List<RouteStopItem> _stops = new();
        private int? _tripId;
        private System.Windows.Forms.Timer? _liveTimer;
        private int _currentStopIndex = 0;
        private bool _isCompleted = false;      // Hoàn tất HOẶC Chờ duyệt -> GPS đã ngừng, chỉ xem lại
        private bool _webMessageHooked = false;
        private bool _hasDynamicRoute = false; // đã chuyển từ đường lý thuyết sang đường động (bám GPS) chưa
        private bool _refreshing = false;      // tránh 2 lần Tick chồng nhau khi mạng chậm

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
            // Chờ duyệt cũng xem như "xem lại": GPS đã ngừng ghi từ lúc tài xế báo
            // xong, và người duyệt cần xem lại đường đi + video trước khi duyệt.
            _isCompleted = tripStatus is "Completed" or "PendingConfirmation";
            Text = $"Lộ trình — {title}";
            lblInfo.Text = "Đang tải...";

            Load += async (_, _) => await LoadRouteAsync();

            // Chuyến ĐÃ hoàn tất -> GPS đã ngừng ghi, không bật theo dõi trực tiếp.
            if (_tripId.HasValue && !_isCompleted)
            {
                _liveTimer = new System.Windows.Forms.Timer { Interval = 15000 }; // 15 giây/lần
                _liveTimer.Tick += async (_, _) => await RefreshLiveRouteAsync();
                FormClosed += (_, _) => _liveTimer?.Stop();
            }
        }

        private List<RouteStopItem> ValidStops() => _stops.Where(s => s.Lat.HasValue && s.Lng.HasValue).ToList();

        private async Task LoadRouteAsync()
        {
            if (_api is null) return;
            try
            {
                var validStops = ValidStops();
                if (validStops.Count < 2)
                {
                    lblInfo.Text = "⚠️ Thiếu tọa độ ở 1 hoặc nhiều điểm dừng — không vẽ được lộ trình.";
                    lblInfo.ForeColor = Theme.Danger;
                    return;
                }

                List<WaypointItem> waypoints = new();
                if (_tripId.HasValue)
                {
                    try { waypoints = await _api.Trips.GetWaypointsAsync(_tripId.Value); }
                    catch { /* không có GPS thật cũng không sao */ }
                }

                if (_isCompleted)
                {
                    // Video hành trình là TÙY CHỌN — không có video vẫn xem GPS bình thường.
                    List<TripVideoItem> videos = new();
                    if (_tripId.HasValue)
                    {
                        try { videos = await _api.TripVideos.GetSegmentsAsync(_tripId.Value); }
                        catch { /* lỗi tải video không chặn việc xem lộ trình */ }
                    }

                    lblInfo.Text = (waypoints.Count > 0
                        ? $"🛰️ {waypoints.Count} điểm GPS thật đã ghi nhận (xem lại lịch sử)"
                        : "🛰️ Không có dữ liệu GPS nào được ghi nhận.")
                        + (videos.Count > 0
                            ? $"   •   🎥 {videos.Count} đoạn video — bấm vào đường màu tím để xem đúng lúc xe đi qua"
                            : "   •   (chuyến này không quay video)");
                    lblInfo.ForeColor = Theme.TextPrimary;

                    if (videos.Count > 0)
                    {
                        // Nới rộng cửa sổ để có chỗ cho khung video bên phải
                        var screen = Screen.FromControl(this).WorkingArea;
                        Width = Math.Min(screen.Width - 40, Math.Max(Width, 1400));
                        Height = Math.Min(screen.Height - 40, Math.Max(Height, 800));
                        Left = screen.Left + (screen.Width - Width) / 2;
                        Top = screen.Top + (screen.Height - Height) / 2;
                    }

                    // Gọi 1 lần CHỈ để lấy ảnh nền (tileUrlTemplate) của VietMap.
                    var tileProbe = await _api.Routes.CalculateAsync(
                        validStops[0].Lat!.Value, validStops[0].Lng!.Value,
                        validStops[^1].Lat!.Value, validStops[^1].Lng!.Value);

                    await webView.EnsureCoreWebView2Async();
                    if (!_webMessageHooked)
                    {
                        webView.CoreWebView2.WebMessageReceived += WebView_WebMessageReceived;
                        _webMessageHooked = true;
                    }
                    webView.NavigateToString(BuildHistoryMapHtml(validStops, waypoints, videos,
                        tileProbe.Success ? tileProbe.TileUrlTemplate : ""));
                    return;
                }

                List<RouteResultItem> theoretical = new();
                List<RouteResultItem>? dynamicSegs = null;
                double totalKm = 0;
                int totalMin = 0;
                WaypointItem? current = null;

                if (waypoints.Count == 0)
                {
                    // CHƯA có GPS -> đường lý thuyết từ điểm xuất phát.
                    foreach (var (from, to) in validStops.Zip(validStops.Skip(1), (a, b) => (a, b)))
                    {
                        var seg = await _api.Routes.CalculateAsync(from.Lat!.Value, from.Lng!.Value, to.Lat!.Value, to.Lng!.Value);
                        theoretical.Add(seg);
                        if (seg.Success) { totalKm += seg.DistanceKm; totalMin += seg.DurationMinutes; }
                    }
                }
                else
                {
                    // CÓ GPS -> xe chắc chắn đã rời điểm xuất phát (index 0),
                    // tính đường từ vị trí thật tới các điểm dừng còn lại.
                    current = waypoints[^1];
                    _currentStopIndex = validStops.Count > 1 ? 1 : 0;
                    (dynamicSegs, totalKm, totalMin) = await CalculateDynamicSegmentsAsync(current.Lat, current.Lng, validStops);
                    _hasDynamicRoute = true;
                }

                UpdateInfoLabel(totalKm, totalMin, waypoints.Count, isDynamic: waypoints.Count > 0);

                var tileUrl = (dynamicSegs ?? theoretical).FirstOrDefault(s => s.Success)?.TileUrlTemplate ?? "";
                await webView.EnsureCoreWebView2Async();
                webView.NavigateToString(BuildMapHtml(tileUrl, validStops, theoretical, dynamicSegs, current, HeadingOf(waypoints)));

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

        /// <summary>Mỗi 15 giây: đưa vị trí GPS mới nhất vào bản đồ. Trang tự
        /// cắt phần đường đã đi qua và báo lại trạng thái:
        ///   ok       -> chỉ cập nhật số km/phút còn lại;
        ///   arrived  -> đã tới 1 điểm dừng trung gian, chuyển sang điểm kế tiếp;
        ///   offroute / noroute -> xe lệch tuyến (hoặc chưa có tuyến) -> gọi VietMap tính lại.</summary>
        private async Task RefreshLiveRouteAsync()
        {
            if (_api is null || !_tripId.HasValue || _refreshing) return;
            _refreshing = true;
            try
            {
                var waypoints = await _api.Trips.GetWaypointsAsync(_tripId.Value);
                if (waypoints.Count == 0) return; // chưa có GPS, giữ đường lý thuyết

                var validStops = ValidStops();
                var current = waypoints[^1];
                var heading = HeadingOf(waypoints);

                if (!_hasDynamicRoute)
                {
                    // GPS ĐẦU TIÊN: bỏ đường lý thuyết, tính đường từ vị trí thật.
                    _currentStopIndex = validStops.Count > 1 ? 1 : 0;
                    await RerouteAsync(current, heading, validStops, waypoints.Count);
                    return;
                }

                var raw = await webView.ExecuteScriptAsync(
                    $"window.updatePosition ? updatePosition({Js(current.Lat)}, {Js(current.Lng)}, {Js(heading)}) : null");
                if (string.IsNullOrEmpty(raw) || raw == "null") return;

                using var doc = JsonDocument.Parse(raw);
                var status = doc.RootElement.GetProperty("status").GetString();

                if (status == "arrived")
                    _currentStopIndex = Math.Min(_currentStopIndex + 1, validStops.Count - 1);

                if (status is "offroute" or "noroute")
                {
                    await RerouteAsync(current, heading, validStops, waypoints.Count);
                    return;
                }

                UpdateInfoLabel(doc.RootElement.GetProperty("km").GetDouble(),
                                doc.RootElement.GetProperty("min").GetInt32(),
                                waypoints.Count, isDynamic: true);
            }
            catch
            {
                // Mất mạng/API lỗi 1 lần không sao — thử lại ở lần Tick kế tiếp.
            }
            finally
            {
                _refreshing = false;
            }
        }

        /// <summary>Gọi VietMap tính lại đường từ vị trí hiện tại tới các điểm
        /// dừng còn lại, rồi thay toàn bộ đường đang vẽ trên bản đồ.</summary>
        private async Task RerouteAsync(WaypointItem current, double? heading, List<RouteStopItem> validStops, int waypointCount)
        {
            var (segments, totalKm, totalMin) = await CalculateDynamicSegmentsAsync(current.Lat, current.Lng, validStops);
            var legsJson = LegsJson(segments);
            var posJson = JsonSerializer.Serialize(new[] { current.Lat, current.Lng });
            await webView.ExecuteScriptAsync($"if (window.setRoute) setRoute({legsJson}, {posJson}, {Js(heading)});");
            _hasDynamicRoute = true;
            UpdateInfoLabel(totalKm, totalMin, waypointCount, isDynamic: true);
        }

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

        /// <summary>Hướng di chuyển (độ, 0 = Bắc) tính từ điểm GPS mới nhất và
        /// điểm GPS gần nhất trước đó cách nó >= 5m (bỏ qua nhiễu khi xe đứng yên).</summary>
        private static double? HeadingOf(List<WaypointItem> waypoints)
        {
            if (waypoints.Count < 2) return null;
            var last = waypoints[^1];
            for (int i = waypoints.Count - 2; i >= 0; i--)
            {
                var prev = waypoints[i];
                if (HaversineMeters(prev.Lat, prev.Lng, last.Lat, last.Lng) >= 5)
                {
                    double toRad(double x) => x * Math.PI / 180.0;
                    var y = Math.Sin(toRad(last.Lng - prev.Lng)) * Math.Cos(toRad(last.Lat));
                    var x = Math.Cos(toRad(prev.Lat)) * Math.Sin(toRad(last.Lat)) -
                            Math.Sin(toRad(prev.Lat)) * Math.Cos(toRad(last.Lat)) * Math.Cos(toRad(last.Lng - prev.Lng));
                    return (Math.Atan2(y, x) * 180.0 / Math.PI + 360.0) % 360.0;
                }
            }
            return null;
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

        /// <summary>Số -> chuỗi JS theo chuẩn invariant (dấu chấm thập phân),
        /// tránh lỗi khi máy cài định dạng số tiếng Việt (dấu phẩy).</summary>
        private static string Js(double? value) => JsonSerializer.Serialize(value);

        private static string LegsJson(List<RouteResultItem> segments) => JsonSerializer.Serialize(
            segments.Where(s => s.Success && s.Points.Count > 1).Select(s => new
            {
                points = s.Points.Select(p => new[] { p.Lat, p.Lng }).ToList(),
                distKm = s.DistanceKm,
                durMin = s.DurationMinutes,
            }).ToList());

        private void UpdateInfoLabel(double totalDistanceKm, int totalDurationMinutes, int waypointCount, bool isDynamic)
        {
            var modeText = isDynamic ? "còn lại, tính từ vị trí hiện tại" : "lý thuyết";
            var infoText = $"📏 {totalDistanceKm:0.0} km ({modeText})   •   ⏱ {totalDurationMinutes} phút   (nguồn: VietMap)   •   🛰️ {waypointCount} điểm GPS";
            if (isDynamic) infoText += "   (🔴 đang theo dõi trực tiếp)";
            lblInfo.Text = infoText;
            lblInfo.ForeColor = Theme.TextPrimary;
        }

        private static string HtmlHead(string bodyHtml = "<div id=\"map\"></div>", string extraCss = "") =>
            "<!DOCTYPE html><html><head><meta charset=\"utf-8\" />" +
            "<link rel=\"stylesheet\" href=\"https://unpkg.com/leaflet@1.9.4/dist/leaflet.css\" />" +
            "<script src=\"https://unpkg.com/leaflet@1.9.4/dist/leaflet.js\"></script>" +
            "<style>html, body, #map { height: 100%; margin: 0; padding: 0; }\n" + SharedCss + extraCss + "</style>" +
            "</head><body>" + bodyHtml + "<script>\n";

        /// <summary>Trang trong bản đồ bấm nút "Kiểm tra" 1 đoạn video -> gọi API
        /// đối chiếu SHA-256 rồi trả kết quả lại cho trang hiển thị.</summary>
        private async void WebView_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            if (_api is null) return;
            int segmentId;
            try
            {
                using var doc = JsonDocument.Parse(e.WebMessageAsJson);
                if (doc.RootElement.GetProperty("type").GetString() != "verify") return;
                segmentId = doc.RootElement.GetProperty("id").GetInt32();
            }
            catch { return; }

            string resultJson;
            try
            {
                var r = await _api.TripVideos.VerifyAsync(segmentId);
                resultJson = JsonSerializer.Serialize(new { intact = r.Intact, message = r.Message });
            }
            catch (Exception ex)
            {
                resultJson = JsonSerializer.Serialize(new { intact = false, message = "Không kiểm tra được: " + ex.Message });
            }
            try { await webView.ExecuteScriptAsync($"window.showVerifyResult && showVerifyResult({segmentId}, {resultJson});"); }
            catch { /* form đã đóng */ }
        }

        /// <summary>Thời điểm -> mili-giây UTC cho JavaScript. Giờ GPS và giờ video
        /// đều là UTC; dữ liệu đọc từ API có thể ở dạng Local (đã đổi múi giờ) hoặc
        /// Unspecified (giá trị UTC nhưng thiếu nhãn) -> chuẩn hóa về cùng 1 mốc.</summary>
        private static long ToEpochMs(DateTime t)
        {
            var utc = t.Kind == DateTimeKind.Local ? t.ToUniversalTime() : DateTime.SpecifyKind(t, DateTimeKind.Utc);
            return new DateTimeOffset(utc).ToUnixTimeMilliseconds();
        }

        /// <summary>Chuyến ĐÃ hoàn tất / Chờ duyệt: đường GPS thật đã đi (cam) +
        /// các điểm dừng + (nếu có) khung xem video hành trình đồng bộ với bản đồ.</summary>
        private string BuildHistoryMapHtml(List<RouteStopItem> stops, List<WaypointItem> waypoints, List<TripVideoItem> videos, string? tileUrlTemplate)
        {
            var track = waypoints
                .Select(w => new { lat = w.Lat, lng = w.Lng, t = ToEpochMs(w.Timestamp), spd = (double?)w.SpeedKmh })
                .OrderBy(w => w.t)
                .ToList();
            var videoJson = videos
                .Select(v => new
                {
                    id = v.Id,
                    start = ToEpochMs(v.StartedAt),
                    end = ToEpochMs(v.EndedAt),
                    url = _api!.TripVideos.AbsoluteUrl(v.StreamUrl),
                    size = v.SizeBytes,
                })
                .OrderBy(v => v.start)
                .ToList();

            var sb = new StringBuilder(HtmlHead(
                "<div id=\"wrap\"><div id=\"map\"></div><div id=\"side\"><h3 id=\"sideTitle\"></h3>" +
                "<video id=\"player\" controls preload=\"metadata\"></video><div id=\"clock\"></div><div id=\"msg\"></div>" +
                "<div id=\"timeline\" title=\"Thanh thời gian cả chuyến — khối tím là đoạn có video\"></div><div id=\"segList\"></div></div></div>",
                HistoryCss));
            sb.Append($"var tileUrl = {JsonSerializer.Serialize(tileUrlTemplate ?? "")};\n");
            sb.Append($"var stops = {StopsJson(stops)};\n");
            sb.Append($"var track = {JsonSerializer.Serialize(track)};\n");
            sb.Append($"var videos = {JsonSerializer.Serialize(videoJson)};\n");
            sb.Append(SharedJs);
            sb.Append(HistoryJs);
            sb.Append("\n</script></body></html>");
            return sb.ToString();
        }

        /// <summary>Bản đồ theo dõi. theoretical: đường lý thuyết nét đứt (khi
        /// chưa có GPS). dynamicSegs: đường bám GPS (khi đã có GPS). Hai hàm JS
        /// để C# gọi lại: setRoute (thay tuyến mới sau khi tính lại) và
        /// updatePosition (di chuyển mũi tên + cắt phần đường đã đi).</summary>
        private static string BuildMapHtml(string tileUrl, List<RouteStopItem> stops, List<RouteResultItem> theoretical,
            List<RouteResultItem>? dynamicSegs, WaypointItem? currentPos, double? heading)
        {
            var sb = new StringBuilder(HtmlHead());
            sb.Append($"var tileUrl = {JsonSerializer.Serialize(tileUrl)};\n");
            sb.Append($"var stops = {StopsJson(stops)};\n");
            sb.Append($"var theoreticalSegs = {JsonSerializer.Serialize(theoretical.Where(s => s.Success).Select(s => s.Points.Select(p => new[] { p.Lat, p.Lng }).ToList()).ToList())};\n");
            sb.Append($"var dynamicLegs = {(dynamicSegs is null ? "null" : LegsJson(dynamicSegs))};\n");
            sb.Append($"var initialPos = {(currentPos is null ? "null" : JsonSerializer.Serialize(new[] { currentPos.Lat, currentPos.Lng }))};\n");
            sb.Append($"var initialHeading = {Js(heading)};\n");
            sb.Append(SharedJs);
            sb.Append("""

var map = L.map('map');
if (tileUrl) L.tileLayer(tileUrl, { attribution: '&copy; VietMap', maxZoom: 19 }).addTo(map);
addStopMarkers(map, stops);
addLegend(map);

var bounds = stops.map(function (s) { return [s.lat, s.lng]; });
var theoreticalLines = [];
theoreticalSegs.forEach(function (pts) {
    if (pts.length > 1) {
        theoreticalLines.push(L.polyline(pts, { color: ROUTE_INNER, weight: 4, opacity: 0.8, dashArray: '10, 8' }).addTo(map));
        bounds = bounds.concat(pts);
    }
});

// Thay toàn bộ tuyến (sau khi C# gọi VietMap tính lại). Lần đầu gọi sẽ xóa luôn
// đường lý thuyết — từ đây chỉ còn đúng 1 tuyến bám theo xe.
window.setRoute = function (newLegs, pos, heading) {
    theoreticalLines.forEach(function (l) { map.removeLayer(l); });
    theoreticalLines = [];
    setLegs(map, newLegs);
    if (pos) { updateGpsMarker(map, pos[0], pos[1], heading); trimToPosition(pos[0], pos[1]); }
    return remainingStats();
};

// GPS mới: di chuyển + xoay mũi tên, cắt phần đường đã đi qua. Không đụng zoom
// của người điều phối.
window.updatePosition = function (lat, lng, heading) {
    updateGpsMarker(map, lat, lng, heading);
    var r = trimToPosition(lat, lng);
    var st = remainingStats();
    return { status: r, km: st.km, min: st.min };
};

if (dynamicLegs && initialPos) {
    setRoute(dynamicLegs, initialPos, initialHeading);
    bounds.push(initialPos);
    legs.forEach(function (l) { bounds = bounds.concat(l.points); });
}
if (bounds.length > 0) map.fitBounds(L.latLngBounds(bounds), { padding: [40, 40] });
else map.setView([10.7769, 106.7009], 12); // fallback: trung tâm TP.HCM
</script></body></html>
""");
            return sb.ToString();
        }

        private static string StopsJson(List<RouteStopItem> stops) => JsonSerializer.Serialize(
            stops.Select(s => new { lat = s.Lat, lng = s.Lng, label = s.Label }).ToList());

        // ===== CSS + JS dùng chung (giống hệt navigate.html của app tài xế) =====
        private const string SharedCss = """
.pin-icon { background: none !important; border: none !important; }
.stop-pin {
    width: 32px; height: 32px; box-sizing: border-box;
    border-radius: 50% 50% 50% 0; transform: rotate(-45deg);
    border: 3px solid #fff; box-shadow: 0 2px 6px rgba(0,0,0,.45);
    display: flex; align-items: center; justify-content: center;
}
.stop-pin span { transform: rotate(45deg); font-size: 14px; line-height: 1; }
.gps-wrap { position: relative; width: 44px; height: 44px; }
.gps-halo {
    position: absolute; left: 4px; top: 4px; width: 36px; height: 36px; border-radius: 50%;
    background: rgba(107,114,128,.25); animation: gpsPulse 2s ease-out infinite;
}
.gps-arrow { position: absolute; left: 6px; top: 6px; transition: transform .4s ease; filter: drop-shadow(0 1px 2px rgba(0,0,0,.5)); }
@keyframes gpsPulse { 0% { transform: scale(.6); opacity: 1; } 100% { transform: scale(1.4); opacity: 0; } }
.map-legend {
    background: rgba(255,255,255,.95); padding: 6px 10px; border-radius: 6px;
    box-shadow: 0 1px 4px rgba(0,0,0,.3); font: 12px Arial, sans-serif; line-height: 20px;
}
.map-legend i {
    display: inline-block; width: 12px; height: 12px; border-radius: 50%;
    border: 2px solid #fff; box-shadow: 0 0 0 1px #999; margin-right: 6px; vertical-align: -2px;
}
.map-legend svg { margin-right: 6px; vertical-align: -3px; }
""";

        private const string SharedJs = """
// ================= DÙNG CHUNG: icon, chú thích, toán học, cắt tuyến =================
const MAP_COLORS = { start: '#16A34A', warehouse: '#EAB308', dest: '#DC2626', gps: '#6B7280' };
const KIND_TEXT = { start: 'Điểm khởi hành', warehouse: 'Kho', dest: 'Điểm đến' };
const ROUTE_CASING = '#1A4FBF', ROUTE_INNER = '#4C8DF6';
const OFF_ROUTE_THRESHOLD_M = 100;  // lệch quá 100m so với đường còn lại -> coi là đi lệch tuyến, tính lại
const ARRIVAL_THRESHOLD_M = 60;     // đoạn còn lại tới điểm dừng < 60m -> coi như đã tới, chuyển điểm kế tiếp
const SEARCH_AHEAD_M = 1500;        // chỉ dò vị trí xe trên 1,5km đường phía trước (tránh "bắt nhầm" chiều về của tuyến khứ hồi)

function escapeHtml(s) {
    return String(s ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
}

// Điểm 0 luôn là điểm khởi hành (xanh lá); điểm có chữ "Kho" là kho (vàng);
// còn lại là điểm đến / điểm lấy-trả hàng của đối tác (đỏ).
function stopKind(label, idx) {
    if (idx === 0) return 'start';
    if (/kho/i.test(label || '')) return 'warehouse';
    return 'dest';
}
function stopGlyph(kind, idx, total) {
    if (kind === 'start') return '🚚';
    if (kind === 'warehouse') return '🏭';
    return idx === total - 1 ? '🏁' : '📦';
}

// Vẽ marker cho các điểm dừng. Các điểm trùng tọa độ (VD chặng Lấy hàng:
// xuất phát từ Kho và quay về đúng Kho đó) gộp thành 1 ghim 2 màu.
function addStopMarkers(map, stops) {
    const groups = [];
    stops.forEach((s, idx) => {
        const kind = stopKind(s.label, idx);
        const item = { s, idx, kind, glyph: stopGlyph(kind, idx, stops.length) };
        const g = groups.find(g => haversineMeters(g.lat, g.lng, s.lat, s.lng) < 30);
        if (g) g.items.push(item); else groups.push({ lat: s.lat, lng: s.lng, items: [item] });
    });
    groups.forEach(g => {
        const first = g.items[0], last = g.items[g.items.length - 1];
        const bg = g.items.length === 1
            ? MAP_COLORS[first.kind]
            : `linear-gradient(135deg, ${MAP_COLORS[first.kind]} 50%, ${MAP_COLORS[last.kind]} 50%)`;
        const icon = L.divIcon({
            className: 'pin-icon',
            html: `<div class="stop-pin" style="background:${bg}"><span>${first.glyph}</span></div>`,
            iconSize: [32, 32], iconAnchor: [16, 39], popupAnchor: [0, -36]
        });
        const popup = g.items.map(i =>
            `<b style="color:${MAP_COLORS[i.kind]}">${i.glyph} ${KIND_TEXT[i.kind]}</b><br>${escapeHtml(i.s.label)}`).join('<hr style="margin:4px 0">');
        L.marker([g.lat, g.lng], { icon, zIndexOffset: 500 }).addTo(map).bindPopup(popup);
    });
}

function arrowSvg(size) {
    return `<svg class="gps-arrow" viewBox="0 0 32 32" width="${size}" height="${size}">` +
        `<path d="M16 2 L28 29 L16 22 L4 29 Z" fill="${MAP_COLORS.gps}" stroke="#fff" stroke-width="2.5" stroke-linejoin="round"/></svg>`;
}

function addLegend(map, extraHtml) {
    const c = L.control({ position: 'bottomleft' });
    c.onAdd = () => {
        const d = L.DomUtil.create('div', 'map-legend');
        d.innerHTML =
            `<div><i style="background:${MAP_COLORS.start}"></i>Điểm khởi hành</div>` +
            `<div><i style="background:${MAP_COLORS.warehouse}"></i>Kho</div>` +
            `<div><i style="background:${MAP_COLORS.dest}"></i>Điểm đến</div>` +
            (extraHtml ?? `<div>${arrowSvg(14).replace('class="gps-arrow"', '')}Vị trí xe (GPS)</div>`);
        return d;
    };
    c.addTo(map);
}

// ---- Toán học: khoảng cách thật (mét) ----
function haversineMeters(lat1, lng1, lat2, lng2) {
    const R = 6371000, toRad = x => x * Math.PI / 180;
    const dLat = toRad(lat2 - lat1), dLng = toRad(lng2 - lng1);
    const a = Math.sin(dLat / 2) ** 2 + Math.cos(toRad(lat1)) * Math.cos(toRad(lat2)) * Math.sin(dLng / 2) ** 2;
    return R * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
}
function bearingDeg(lat1, lng1, lat2, lng2) {
    const toRad = x => x * Math.PI / 180;
    const y = Math.sin(toRad(lng2 - lng1)) * Math.cos(toRad(lat2));
    const x = Math.cos(toRad(lat1)) * Math.sin(toRad(lat2)) - Math.sin(toRad(lat1)) * Math.cos(toRad(lat2)) * Math.cos(toRad(lng2 - lng1));
    return (Math.atan2(y, x) * 180 / Math.PI + 360) % 360;
}
function lengthM(pts) {
    let m = 0;
    for (let i = 0; i < pts.length - 1; i++) m += haversineMeters(pts[i][0], pts[i][1], pts[i + 1][0], pts[i + 1][1]);
    return m;
}
// Chiếu điểm P lên ĐOẠN THẳNG A-B (hệ mét cục bộ quanh A) -> khoảng cách + tọa độ điểm chiếu.
function projectOnSegment(p, a, b) {
    const mLat = 111320, mLng = 111320 * Math.cos(a[0] * Math.PI / 180);
    const px = (p[1] - a[1]) * mLng, py = (p[0] - a[0]) * mLat;
    const bx = (b[1] - a[1]) * mLng, by = (b[0] - a[0]) * mLat;
    const lenSq = bx * bx + by * by;
    let t = lenSq === 0 ? 0 : (px * bx + py * by) / lenSq;
    t = Math.max(0, Math.min(1, t));
    const qx = t * bx, qy = t * by;
    return { dist: Math.hypot(px - qx, py - qy), point: [a[0] + qy / mLat, a[1] + qx / mLng] };
}

// ---- Vị trí xe: mũi tên xám xoay theo hướng di chuyển ----
let gpsMarker = null, gpsLastPos = null, gpsHeading = null;
function updateGpsMarker(map, lat, lng, heading) {
    // Ưu tiên hướng do thiết bị GPS báo; không có thì tự tính từ 2 vị trí liên tiếp
    // (chỉ khi xe đã nhích >= 5m, tránh mũi tên quay loạn khi xe đứng yên).
    if (heading != null && !isNaN(heading)) gpsHeading = heading;
    else if (gpsLastPos && haversineMeters(gpsLastPos[0], gpsLastPos[1], lat, lng) >= 5)
        gpsHeading = bearingDeg(gpsLastPos[0], gpsLastPos[1], lat, lng);
    if (!gpsLastPos || haversineMeters(gpsLastPos[0], gpsLastPos[1], lat, lng) >= 5) gpsLastPos = [lat, lng];

    if (!gpsMarker) {
        const icon = L.divIcon({ className: 'pin-icon', html: `<div class="gps-wrap"><div class="gps-halo"></div>${arrowSvg(32)}</div>`, iconSize: [44, 44], iconAnchor: [22, 22] });
        gpsMarker = L.marker([lat, lng], { icon, zIndexOffset: 1000 }).addTo(map).bindPopup('🛰️ Vị trí xe hiện tại');
    } else {
        gpsMarker.setLatLng([lat, lng]);
    }
    const el = gpsMarker.getElement();
    const svg = el && el.querySelector('.gps-arrow');
    if (svg && gpsHeading != null) svg.style.transform = `rotate(${gpsHeading}deg)`;
}

// ---- Tuyến đường còn lại: mỗi "leg" = đoạn tới 1 điểm dừng ----
// leg = { points, lines: [viền, lõi], distKm, durMin, origLenM }
let legs = [];
function clearLegs(map) {
    legs.forEach(l => l.lines.forEach(x => map.removeLayer(x)));
    legs = [];
}
function setLegs(map, newLegs) {
    clearLegs(map);
    newLegs.filter(l => l.points && l.points.length > 1).forEach(l => {
        const lines = [
            L.polyline(l.points, { color: ROUTE_CASING, weight: 9, opacity: 0.9 }).addTo(map),
            L.polyline(l.points, { color: ROUTE_INNER, weight: 5, opacity: 1 }).addTo(map)
        ];
        legs.push({ points: l.points.slice(), lines, distKm: l.distKm || 0, durMin: l.durMin || 0, origLenM: lengthM(l.points) });
    });
}
// Giống Google Maps: mỗi lần có GPS mới, chiếu vị trí xe lên đường còn lại
// và CẮT BỎ phần đã đi qua — đường luôn bắt đầu ngay tại mũi tên của xe.
// Trả về: 'ok' | 'arrived' (vừa tới 1 điểm dừng trung gian) | 'offroute' | 'noroute'.
function trimToPosition(lat, lng) {
    if (legs.length === 0) return 'noroute';
    const leg = legs[0], pts = leg.points;
    if (pts.length < 2) return 'ok';

    let best = { dist: Infinity, i: -1, point: null }, ahead = 0;
    for (let i = 0; i < pts.length - 1; i++) {
        const pr = projectOnSegment([lat, lng], pts[i], pts[i + 1]);
        if (pr.dist < best.dist) best = { dist: pr.dist, i, point: pr.point };
        ahead += haversineMeters(pts[i][0], pts[i][1], pts[i + 1][0], pts[i + 1][1]);
        if (ahead > SEARCH_AHEAD_M) break;
    }
    if (best.dist > OFF_ROUTE_THRESHOLD_M) return 'offroute';

    leg.points = [best.point].concat(pts.slice(best.i + 1));
    leg.lines.forEach(l => l.setLatLngs(leg.points));

    if (lengthM(leg.points) < ARRIVAL_THRESHOLD_M && legs.length > 1) {
        leg.lines.forEach(l => l.remove());
        legs.shift();
        return 'arrived';
    }
    return 'ok';
}
function remainingStats() {
    let m = 0, min = 0;
    legs.forEach(l => {
        const len = lengthM(l.points);
        m += len;
        min += l.origLenM > 0 ? l.durMin * len / l.origLenM : l.durMin;
    });
    return { km: Math.round(m / 100) / 10, min: Math.round(min), hasRoute: legs.length > 0 };
}
""";

        // ===== Xem lại chuyến + video hành trình =====
        private const string HistoryCss = """
#wrap { display: flex; height: 100%; }
#map { flex: 1; height: 100%; }
#side {
    width: 440px; display: none; flex-direction: column; border-left: 1px solid #E2E8F0;
    background: #F8FAFC; font: 13px 'Segoe UI', Arial, sans-serif; color: #0F172A;
}
#side.show { display: flex; }
#side h3 { margin: 10px 12px 6px; font-size: 14px; }
#player { width: 100%; max-height: 52vh; background: #000; display: block; }
#clock { padding: 8px 12px; font-weight: bold; min-height: 18px; }
#clock .sub { font-weight: normal; color: #64748B; }
#msg { padding: 0 12px 6px; color: #B45309; min-height: 16px; }
#timeline { position: relative; height: 18px; margin: 4px 12px 10px; background: #E2E8F0; border-radius: 4px; cursor: pointer; }
#timeline .blk { position: absolute; top: 0; bottom: 0; background: #7C3AED; opacity: .8; border-radius: 2px; }
#timeline .cur { position: absolute; top: -3px; bottom: -3px; width: 2px; background: #DC2626; display: none; }
#segList { flex: 1; overflow-y: auto; border-top: 1px solid #E2E8F0; }
.seg { display: flex; align-items: center; gap: 6px; padding: 6px 12px; border-bottom: 1px solid #EEF2F7; cursor: pointer; }
.seg:hover { background: #EEF2FF; }
.seg.active { background: #DDD6FE; }
.seg .time { flex: 1; }
.seg button { font: 12px 'Segoe UI'; border: 1px solid #CBD5E1; background: #fff; border-radius: 4px; padding: 2px 6px; cursor: pointer; }
.seg .vr { font-size: 12px; }
@media (max-width: 1000px) { #wrap { flex-direction: column; } #side { width: auto; height: 46%; border-left: none; border-top: 1px solid #E2E8F0; } #player { max-height: 28vh; } }
""";

        private const string HistoryJs = """
// ================= XEM LẠI CHUYẾN + VIDEO HÀNH TRÌNH =================
// track: [{lat,lng,t(ms UTC),spd}] — điểm GPS thật; videos: [{id,start,end,url,size}]
var map = L.map('map');
if (tileUrl) L.tileLayer(tileUrl, { attribution: '&copy; VietMap', maxZoom: 19 }).addTo(map);
addStopMarkers(map, stops);

var VIDEO_COLOR = '#7C3AED';
var CLICK_TOLERANCE_PX = 25;
var player = document.getElementById('player');
var clockEl = document.getElementById('clock');
var msgEl = document.getElementById('msg');
var timelineEl = document.getElementById('timeline');
var cursorEl = null;
var currentSeg = null;
var pendingSeek = false; // đang chờ video tải xong để tua -> chưa cập nhật vị trí theo video

function fmtTime(t) {
    return new Date(t).toLocaleTimeString('vi-VN', { timeZone: 'Asia/Ho_Chi_Minh', hour12: false });
}
function fmtDate(t) {
    return new Date(t).toLocaleDateString('vi-VN', { timeZone: 'Asia/Ho_Chi_Minh' });
}

addLegend(map,
    '<div><span style="display:inline-block;width:14px;height:4px;background:#EA580C;margin-right:6px;vertical-align:3px"></span>Đường GPS thật đã đi</div>' +
    (videos.length ? '<div><span style="display:inline-block;width:14px;height:6px;background:' + VIDEO_COLOR + ';margin-right:6px;vertical-align:2px"></span>Đoạn có video (bấm để xem)</div>' : ''));

// ---- Đường GPS thật ----
var pts = track.map(function (p) { return [p.lat, p.lng]; });
var bounds = stops.map(function (s) { return [s.lat, s.lng]; }).concat(pts);
if (pts.length > 0) {
    L.polyline(pts, { color: '#EA580C', weight: 5, opacity: 0.9 }).addTo(map);
    L.circleMarker(pts[0], { radius: 6, color: '#fff', weight: 2, fillColor: '#EA580C', fillOpacity: 1 })
        .addTo(map).bindPopup('🛰️ Bắt đầu hành trình — ' + fmtTime(track[0].t));
    L.circleMarker(pts[pts.length - 1], { radius: 6, color: '#fff', weight: 2, fillColor: '#EA580C', fillOpacity: 1 })
        .addTo(map).bindPopup('🛰️ Kết thúc hành trình — ' + fmtTime(track[track.length - 1].t));
}

// Vị trí trên đường GPS tại thời điểm t (nội suy giữa 2 điểm GPS liền kề).
function positionAt(t) {
    if (track.length === 0) return null;
    if (t <= track[0].t) return { lat: track[0].lat, lng: track[0].lng, spd: track[0].spd, heading: null };
    var last = track[track.length - 1];
    if (t >= last.t) return { lat: last.lat, lng: last.lng, spd: last.spd, heading: null };
    var lo = 0, hi = track.length - 1;
    while (hi - lo > 1) { var mid = (lo + hi) >> 1; if (track[mid].t <= t) lo = mid; else hi = mid; }
    var a = track[lo], b = track[hi];
    var u = b.t === a.t ? 0 : (t - a.t) / (b.t - a.t);
    var moved = haversineMeters(a.lat, a.lng, b.lat, b.lng) >= 3;
    return {
        lat: a.lat + (b.lat - a.lat) * u,
        lng: a.lng + (b.lng - a.lng) * u,
        spd: a.spd != null && b.spd != null ? a.spd + (b.spd - a.spd) * u : (a.spd ?? b.spd),
        heading: moved ? bearingDeg(a.lat, a.lng, b.lat, b.lng) : null,
    };
}

// Phần đường GPS nằm trong khoảng thời gian [t0, t1] — để tô màu đoạn có video.
function trackBetween(t0, t1) {
    var out = [];
    var p0 = positionAt(t0); if (p0) out.push([p0.lat, p0.lng]);
    track.forEach(function (p) { if (p.t > t0 && p.t < t1) out.push([p.lat, p.lng]); });
    var p1 = positionAt(t1); if (p1) out.push([p1.lat, p1.lng]);
    return out;
}

// Bấm 1 chỗ trên bản đồ -> tìm điểm GẦN NHẤT trên đường GPS (chiếu vuông góc
// lên từng đoạn giữa 2 điểm GPS) -> suy ra thời điểm xe đi qua chỗ đó.
function timeAtLatLng(latlng) {
    if (track.length === 0) return null;
    var click = map.latLngToContainerPoint(latlng);
    var best = { d: Infinity, t: null };
    if (track.length === 1) {
        var p = map.latLngToContainerPoint([track[0].lat, track[0].lng]);
        best = { d: click.distanceTo(p), t: track[0].t };
    }
    for (var i = 0; i < track.length - 1; i++) {
        var a = map.latLngToContainerPoint([track[i].lat, track[i].lng]);
        var b = map.latLngToContainerPoint([track[i + 1].lat, track[i + 1].lng]);
        var dx = b.x - a.x, dy = b.y - a.y, len2 = dx * dx + dy * dy;
        var u = len2 === 0 ? 0 : Math.max(0, Math.min(1, ((click.x - a.x) * dx + (click.y - a.y) * dy) / len2));
        var d = Math.hypot(click.x - (a.x + u * dx), click.y - (a.y + u * dy));
        if (d < best.d) best = { d: d, t: track[i].t + u * (track[i + 1].t - track[i].t) };
    }
    return best.d <= CLICK_TOLERANCE_PX ? best.t : null;
}

function segmentAt(t) {
    for (var i = 0; i < videos.length; i++)
        if (t >= videos[i].start && t < videos[i].end + 500) return videos[i];
    return null;
}

function showPosition(t) {
    var pos = positionAt(t);
    if (pos) updateGpsMarker(map, pos.lat, pos.lng, pos.heading);
    var spd = pos && pos.spd != null ? ' <span class="sub">• ~' + Math.round(pos.spd) + ' km/h</span>' : '';
    clockEl.innerHTML = '🕒 ' + fmtTime(t) + ' <span class="sub">' + fmtDate(t) + '</span>' + spd;
    if (cursorEl && timeRange) {
        cursorEl.style.display = 'block';
        cursorEl.style.left = (100 * (t - timeRange.min) / (timeRange.max - timeRange.min)) + '%';
    }
}

function highlightRow(seg) {
    document.querySelectorAll('.seg').forEach(function (el) {
        el.classList.toggle('active', seg != null && el.dataset.id == seg.id);
    });
}

function loadSegment(seg, offsetSec) {
    msgEl.textContent = '';
    highlightRow(seg);
    pendingSeek = true;
    var seek = function () {
        pendingSeek = false;
        var dur = isFinite(player.duration) ? player.duration : offsetSec + 1;
        player.currentTime = Math.max(0, Math.min(offsetSec, dur - 0.2));
        player.play().catch(function () { });
    };
    if (currentSeg !== seg) {
        currentSeg = seg;
        player.src = seg.url;
        player.addEventListener('loadedmetadata', seek, { once: true });
        player.load();
    } else {
        seek();
    }
}

// Phát video đúng thời điểm t (ms UTC). Không có video lúc đó -> báo rõ.
function playAt(t) {
    var seg = segmentAt(t);
    showPosition(t);
    if (!seg) {
        player.pause();
        highlightRow(null);
        msgEl.textContent = 'Không có video lúc ' + fmtTime(t) + ' (tài xế không bật quay ở đoạn này).';
        return false;
    }
    loadSegment(seg, (t - seg.start) / 1000);
    return true;
}
window.playAt = playAt;

player.addEventListener('timeupdate', function () {
    if (currentSeg && !pendingSeek) showPosition(currentSeg.start + player.currentTime * 1000);
});
// Hết 1 đoạn -> tự phát tiếp đoạn liền sau (nếu 2 đoạn sát nhau), xem liền mạch.
player.addEventListener('ended', function () {
    if (!currentSeg) return;
    var idx = videos.indexOf(currentSeg);
    var next = videos[idx + 1];
    if (next && next.start - currentSeg.end < 5000) loadSegment(next, 0);
});
player.addEventListener('error', function () {
    if (currentSeg) msgEl.textContent = 'Không phát được đoạn video này (link hết hạn hoặc file lỗi) — đóng và mở lại màn hình.';
});

map.on('click', function (e) {
    if (!videos.length) return;
    var t = timeAtLatLng(e.latlng);
    if (t != null) playAt(t);
});

// ---- Bảng điều khiển video ----
var timeRange = null;
if (videos.length) {
    document.getElementById('side').classList.add('show');

    // Tô màu đoạn đường có video
    videos.forEach(function (v) {
        var seg = trackBetween(v.start, v.end);
        if (seg.length > 1) L.polyline(seg, { color: VIDEO_COLOR, weight: 8, opacity: 0.75 }).addTo(map);
    });

    // Thanh thời gian: cả chuyến (GPS + video), khối tím = có video
    var times = videos.map(function (v) { return v.start; }).concat(videos.map(function (v) { return v.end; }));
    if (track.length) times.push(track[0].t, track[track.length - 1].t);
    timeRange = { min: Math.min.apply(null, times), max: Math.max.apply(null, times) };
    if (timeRange.max === timeRange.min) timeRange.max = timeRange.min + 1;
    videos.forEach(function (v) {
        var b = document.createElement('div');
        b.className = 'blk';
        b.style.left = (100 * (v.start - timeRange.min) / (timeRange.max - timeRange.min)) + '%';
        b.style.width = Math.max(0.4, 100 * (v.end - v.start) / (timeRange.max - timeRange.min)) + '%';
        timelineEl.appendChild(b);
    });
    cursorEl = document.createElement('div');
    cursorEl.className = 'cur';
    timelineEl.appendChild(cursorEl);
    timelineEl.addEventListener('click', function (e) {
        var r = timelineEl.getBoundingClientRect();
        playAt(timeRange.min + (e.clientX - r.left) / r.width * (timeRange.max - timeRange.min));
    });

    // Danh sách đoạn
    var list = document.getElementById('segList');
    videos.forEach(function (v) {
        var row = document.createElement('div');
        row.className = 'seg';
        row.dataset.id = v.id;
        row.innerHTML =
            '<span class="time">🎬 ' + fmtTime(v.start) + ' → ' + fmtTime(v.end) +
            ' <span style="color:#64748B">· ' + (v.size / 1048576).toFixed(1) + ' MB</span></span>' +
            '<span class="vr" id="vr-' + v.id + '"></span>' +
            '<button title="Đối chiếu mã SHA-256 để chứng minh video chưa bị chỉnh sửa">Kiểm tra</button>';
        row.addEventListener('click', function () { loadSegment(v, 0); });
        row.querySelector('button').addEventListener('click', function (e) {
            e.stopPropagation();
            document.getElementById('vr-' + v.id).textContent = '⏳';
            if (window.chrome && window.chrome.webview) window.chrome.webview.postMessage({ type: 'verify', id: v.id });
        });
        list.appendChild(row);
    });
    document.getElementById('sideTitle').textContent =
        '🎥 Video hành trình — ' + videos.length + ' đoạn, ' + Math.round(videos.reduce(function (s, v) { return s + (v.end - v.start); }, 0) / 60000) + ' phút';
    clockEl.innerHTML = '<span class="sub">Bấm vào đoạn đường màu tím trên bản đồ (hoặc thanh thời gian) để xem video đúng lúc xe đi qua.</span>';
}

// C# gọi lại sau khi đối chiếu SHA-256 xong
window.showVerifyResult = function (id, r) {
    var el = document.getElementById('vr-' + id);
    if (!el) return;
    if (!r) { el.textContent = '❓'; el.title = 'Không kiểm tra được'; return; }
    el.textContent = r.intact ? '✅' : '⚠️';
    el.title = r.message;
    msgEl.textContent = 'Đoạn ' + id + ': ' + r.message;
};

if (bounds.length > 0) map.fitBounds(L.latLngBounds(bounds), { padding: [40, 40] });
else map.setView([10.7769, 106.7009], 12);
setTimeout(function () { map.invalidateSize(); }, 50);
""";
    }
}
