using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TripManagement;
using TruckingSystem.Desktop.GUI_RouteManagement;

namespace TruckingSystem.Desktop.GUI_TripManagement
{
    /// <summary>Danh sách TOÀN BỘ chuyến (Trip) trong hệ thống — khác OrderDetailForm
    /// chỉ hiện Trip của 1 đơn cụ thể. Cho phép Dispatcher đổi trạng thái thủ công
    /// (Planned→InProgress→Completed) vì chưa tích hợp GPS thật để tự động.</summary>
    public partial class TripsControl : BaseUserControl
    {
        private ApiServices? _api;
        private List<TripListItem> _trips = new();
        private Dictionary<int, TripWarehouseProgressItem> _progress = new();

        public TripsControl()
        {
            InitializeComponent();
            Theme.StyleGrid(grid);
            Resize += (_, _) => { grid.Width = Width; grid.Height = Height - grid.Top; };
            grid.DataBindingComplete += (_, _) => { if (grid.Columns["Id"] != null) grid.Columns["Id"]!.Visible = false; };
            grid.CellFormatting += Grid_CellFormatting;
            grid.SelectionChanged += (_, _) => UpdateActionButtons();
            Load += async (_, _) => await LoadAsync();
        }

        /// <summary>Tô nổi bật dòng đang "Chờ xác nhận" (vàng đậm) — Dispatcher
        /// dễ nhận ra ngay có chuyến cần Duyệt mà không phải đọc kỹ từng dòng.</summary>
        private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (grid.Columns[e.ColumnIndex].Name == "Kho" && e.Value is string kho)
            {
                e.CellStyle!.ForeColor = kho.StartsWith("✅") ? Theme.Success
                    : kho.StartsWith("📥") || kho.StartsWith("📤") ? Theme.Primary
                    : Theme.TextSecondary;
                return;
            }
            if (grid.Columns[e.ColumnIndex].Name != "Trạng_thái" || e.Value is null) return;
            if (e.Value.ToString() == "🟡 Chờ xác nhận")
            {
                e.CellStyle!.ForeColor = Theme.Warning;
                e.CellStyle.Font = new Font(grid.Font, FontStyle.Bold);
            }
            else if (e.Value.ToString() == WaitingPickupText)
            {
                e.CellStyle!.ForeColor = Theme.TextSecondary;
                e.CellStyle.Font = new Font(grid.Font, FontStyle.Italic);
            }
        }

        public TripsControl(ApiServices api) : this()
        {
            _api = api;
        }

        private void BtnRefresh_Click(object? sender, EventArgs e) => _ = LoadAsync();

        private async void BtnStart_Click(object? sender, EventArgs e)
        {
            var tripId = GetSelectedTripId();
            if (tripId is null) { ShowNoSelection(); return; }

            try
            {
                var result = await _api!.Trips.StartTripAsync(tripId.Value);
                if (!result.Success)
                {
                    MessageBox.Show(FindForm(), result.Message ?? "Không thể bắt đầu chuyến.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(FindForm(), "Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnComplete_Click(object? sender, EventArgs e)
        {
            var tripId = GetSelectedTripId();
            if (tripId is null) { ShowNoSelection(); return; }

            var confirm = MessageBox.Show(FindForm(),
                "Xác nhận chuyến này đã hoàn tất (xe đã tới nơi, hàng đã giao/nhận xong)?",
                "Xác nhận hoàn tất chuyến", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                var result = await _api!.Trips.CompleteTripAsync(tripId.Value);
                if (!result.Success)
                {
                    MessageBox.Show(FindForm(), result.Message ?? "Không thể hoàn tất chuyến.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(FindForm(), "Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnApprove_Click(object? sender, EventArgs e)
        {
            var tripId = GetSelectedTripId();
            if (tripId is null) { ShowNoSelection(); return; }

            var confirm = MessageBox.Show(FindForm(),
                "Duyệt xác nhận của tài xế cho chuyến này? Sau khi duyệt, hệ thống sẽ tự nhập/xuất kho và trả xe về trạng thái rảnh.",
                "Duyệt xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                var result = await _api!.Trips.ApproveCompletionAsync(tripId.Value);
                if (!result.Success)
                {
                    MessageBox.Show(FindForm(), result.Message ?? "Không thể duyệt.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(FindForm(), "Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnReject_Click(object? sender, EventArgs e)
        {
            var tripId = GetSelectedTripId();
            if (tripId is null) { ShowNoSelection(); return; }

            var confirm = MessageBox.Show(FindForm(),
                "Từ chối xác nhận của tài xế? Chuyến sẽ quay lại trạng thái Đang chạy để tài xế báo lại.",
                "Từ chối xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            try
            {
                var result = await _api!.Trips.RejectCompletionAsync(tripId.Value);
                if (!result.Success)
                {
                    MessageBox.Show(FindForm(), result.Message ?? "Không thể từ chối.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(FindForm(), "Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnFuel_Click(object? sender, EventArgs e)
        {
            var tripId = GetSelectedTripId();
            if (tripId is null) { ShowNoSelection(); return; }

            using var f = new RecordFuelForm(_api!, tripId.Value);
            if (f.ShowDialog(FindForm()) == DialogResult.OK)
            {
                MessageBox.Show(FindForm(), "Đã ghi nhận đổ xăng thành công.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void BtnSyncCalendar_Click(object? sender, EventArgs e)
        {
            var tripId = GetSelectedTripId();
            if (tripId is null) { ShowNoSelection(); return; }
            if (_api is null) return;

            try
            {
                var result = await _api.Trips.SyncCalendarAsync(tripId.Value);
                var message = $"Tài xế: {(result.DriverSynced ? "✅ Đã đồng bộ" : "⚠️ " + (result.DriverMessage ?? "Không đồng bộ được"))}\n" +
                              $"Khách hàng: {(result.CustomerSynced ? "✅ Đã đồng bộ" : "⚠️ " + (result.CustomerMessage ?? "Không đồng bộ được"))}";
                MessageBox.Show(FindForm(), message, "Kết quả đồng bộ Google Calendar", MessageBoxButtons.OK,
                    result.DriverSynced || result.CustomerSynced ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(FindForm(), "Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExpenses_Click(object? sender, EventArgs e)
        {
            var tripId = GetSelectedTripId();
            if (tripId is null) { ShowNoSelection(); return; }
            var trip = _trips.FirstOrDefault(t => t.Id == tripId.Value);
            if (trip is null || _api is null) return;

            using var f = new ManageTripExpensesForm(_api, tripId.Value, $"Đơn {trip.OrderCode}");
            f.ShowDialog(FindForm());
        }

        private void BtnIncidents_Click(object? sender, EventArgs e)
        {
            var tripId = GetSelectedTripId();
            if (tripId is null) { ShowNoSelection(); return; }
            var trip = _trips.FirstOrDefault(t => t.Id == tripId.Value);
            if (trip is null || _api is null) return;

            using var f = new ManageIncidentsForm(_api, tripId.Value, $"Đơn {trip.OrderCode}");
            f.ShowDialog(FindForm());
        }

        private async void BtnFuelEstimate_Click(object? sender, EventArgs e)
        {
            var tripId = GetSelectedTripId();
            if (tripId is null) { ShowNoSelection(); return; }
            if (_api is null) return;

            try
            {
                var est = await _api.Trips.GetFuelEstimateAsync(tripId.Value);

                if (est.Message is not null && est.EstimatedCost is null)
                {
                    MessageBox.Show(FindForm(), est.Message, "Chưa ước tính được", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var variance = est.VarianceCost ?? 0;
                var varianceText = variance > 0
                    ? $"⚠️ Tốn THỰC TẾ nhiều hơn ước tính {variance:N0} đ"
                    : variance < 0
                        ? $"✅ Tiết kiệm hơn ước tính {Math.Abs(variance):N0} đ"
                        : "Đúng khớp ước tính";

                var message = $"📏 Khoảng cách THẬT (từ GPS): {est.ActualDistanceKm:0.0} km\n" +
                              $"⛽ Ước tính tiêu hao: {est.EstimatedLiters:0.0} lít\n" +
                              $"💰 Chi phí ƯỚC TÍNH: {est.EstimatedCost:N0} đ\n" +
                              $"💰 Chi phí THỰC TẾ (đã ghi): {est.ActualFuelCost:N0} đ\n\n" +
                              varianceText;

                MessageBox.Show(FindForm(), message, "So sánh chi phí nhiên liệu", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(FindForm(), "Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDriverInfo_Click(object? sender, EventArgs e)
        {
            var tripId = GetSelectedTripId();
            if (tripId is null) { ShowNoSelection(); return; }
            if (_api is null) return;

            var serverUrl = _api.Connection.BaseUrl;
            var message = $"Dán 2 thông tin sau vào app tài xế (Delivery GPS Tracker):\n\n" +
                          $"Server URL:\n{serverUrl}\n\n" +
                          $"Trip ID:\n{tripId}";

            Clipboard.SetText($"Server URL: {serverUrl}\nTrip ID: {tripId}");
            MessageBox.Show(FindForm(), message + "\n\n(Đã copy vào Clipboard)", "Thông tin kết nối tài xế", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void BtnQuickEstimate_Click(object? sender, EventArgs e)
        {
            var tripId = GetSelectedTripId();
            if (tripId is null) { ShowNoSelection(); return; }
            var trip = _trips.FirstOrDefault(t => t.Id == tripId.Value);
            if (trip is null || _api is null) return;

            try
            {
                // Gọi endpoint MỚI — vừa tính vừa LƯU LẠI vào Trip (không chỉ
                // hiện tạm thời rồi bỏ đi), để sau này làm được bảng thống kê
                // so sánh Ước tính (VietMap) vs Thực tế (StartTime/EndTime,
                // GPS thật) cho từng chuyến.
                var result = await _api.Trips.CalculateEstimateAsync(trip.Id);
                if (result.Message is not null)
                {
                    MessageBox.Show(FindForm(), result.Message, "Không ước tính được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var hours = result.DurationMinutes / 60;
                var minutes = result.DurationMinutes % 60;
                var legText = trip.LegType == "Pickup" ? "Lấy hàng (round-trip qua Kho)" : "Giao hàng";

                MessageBox.Show(FindForm(),
                    $"Đơn {trip.OrderCode} — Chặng {legText}\n\n" +
                    $"📏 Khoảng cách ước tính: {result.DistanceKm:0.0} km\n" +
                    $"⏱ Thời gian ước tính: {(hours > 0 ? $"{hours} giờ " : "")}{minutes} phút\n\n" +
                    $"✅ Đã lưu lại vào chuyến để đối chiếu với số liệu thực tế sau này.\n" +
                    $"(Nguồn: VietMap — số liệu THEO ĐƯỜNG THẬT, không phải đường chim bay)",
                    "Ước tính lộ trình", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(FindForm(), "Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnViewRoute_Click(object? sender, EventArgs e)
        {
            var tripId = GetSelectedTripId();
            if (tripId is null) { ShowNoSelection(); return; }
            var trip = _trips.FirstOrDefault(t => t.Id == tripId.Value);
            if (trip is null || _api is null) return;

            if (trip.FromLat is null || trip.FromLng is null || trip.ToLat is null || trip.ToLng is null)
            {
                MessageBox.Show(FindForm(),
                    "Chuyến này thiếu tọa độ (điểm khách hàng chưa chọn từ gợi ý VietMap, hoặc kho chưa có tọa độ) — không vẽ được lộ trình.",
                    "Thiếu tọa độ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Tải danh sách điểm dừng ĐẦY ĐỦ (chặng Lấy hàng có 3 điểm:
                // Kho→Đối tác→Kho, round-trip thật) thay vì chỉ 1 cặp From/To
                // như trước — khớp đúng với cách tính chi phí xăng/số ngày ước
                // tính đã sửa (đều tính đủ cả 2 chiều cho chặng Lấy hàng).
                var route = await _api.Trips.GetTripRouteAsync(trip.Id);

                var legName = trip.LegType == "Pickup" ? "Lấy hàng (round-trip qua Kho)" : "Giao hàng (Kho → Khách nhận)";
                using var f = new RouteMapForm(
                    _api, $"Lộ trình chặng {legName} — Đơn {trip.OrderCode}",
                    route.Stops, tripId: trip.Id, tripStatus: trip.Status);
                f.ShowDialog(FindForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show(FindForm(), "Lỗi tải lộ trình: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int? GetSelectedTripId()
        {
            if (grid.CurrentRow is null) return null;
            return (int)grid.CurrentRow.Cells["Id"].Value;
        }

        private void ShowNoSelection()
            => MessageBox.Show(FindForm(), "Vui lòng chọn 1 chuyến trong danh sách trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

        private async Task LoadAsync()
        {
            if (_api is null) return;
            lblStatus.Text = "Đang tải...";
            try
            {
                _trips = await _api.Trips.GetTripsAsync();
                try { _progress = (await _api.Trips.GetWarehouseProgressAsync()).ToDictionary(p => p.TripId); }
                catch { _progress = new(); } // API cũ chưa có -> chỉ thiếu cột Kho, không chặn cả màn hình
                grid.DataSource = _trips.Select(t => new
                {
                    t.Id,
                    Mã_đơn = t.OrderCode,
                    Biển_số_xe = t.VehiclePlate,
                    Tài_xế = t.DriverName,
                    Chặng = TranslateLegType(t.LegType),
                    Trạng_thái = IsWaitingForPickup(t) ? WaitingPickupText : TranslateStatus(t.Status),
                    Số_lượng = t.TotalQtyPlanned,
                    Kho = WarehouseText(t),
                }).ToList();
                lblStatus.Text = $"Tổng {_trips.Count} chuyến — chọn 1 dòng rồi bấm Bắt đầu/Hoàn tất để đổi trạng thái";
                UpdateActionButtons();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }

        private const string WaitingPickupText = "⏳ Chờ lấy hàng xong";

        /// <summary>Cột "Kho": chuyến Lấy hàng đã NHẬP kho bao nhiêu, chuyến Giao/
        /// Trả đã XUẤT kho bao nhiêu — so với số lượng kế hoạch của chuyến.</summary>
        private string WarehouseText(TripListItem t)
        {
            if (t.Status == "Cancelled") return "—";
            if (!_progress.TryGetValue(t.Id, out var p)) return "";
            var planned = p.QtyPlanned;
            var done = p.QtyDone;
            if (t.LegType == "Pickup")
            {
                if (planned > 0 && done >= planned) return $"✅ Đã nhập {done:0.##}/{planned:0.##}";
                if (done > 0) return $"📥 Đã nhập {done:0.##}/{planned:0.##}";
                return t.Status == "PendingConfirmation" ? "⏳ Duyệt xong mới nhập kho" : $"Chưa nhập (0/{planned:0.##})";
            }
            var verb = t.LegType == "Return" ? "xuất trả" : "xuất";
            if (planned > 0 && done >= planned) return $"✅ Đã {verb} {done:0.##}/{planned:0.##}";
            if (done > 0) return $"📤 Đã {verb} {done:0.##}/{planned:0.##}";
            return t.LegType == "Return" ? $"Xuất khi trả xong (0/{planned:0.##})" : $"Xuất khi bắt đầu (0/{planned:0.##})";
        }

        /// <summary>Chặng GIAO còn "Đã lên kế hoạch" nhưng đơn vẫn còn chặng LẤY
        /// HÀNG chưa Hoàn tất (chưa chạy / đang chạy / chờ duyệt) — hàng chưa về
        /// kho nên chưa được bắt đầu giao. Backend cũng chặn y hệt (StartAsync),
        /// ở đây chỉ để Vận hành nhìn danh sách là biết ngay.</summary>
        private bool IsWaitingForPickup(TripListItem t)
            => t.LegType == "Delivery" && t.Status == "Planned"
               && _trips.Any(p => p.OrderCode == t.OrderCode && p.LegType == "Pickup"
                                  && p.Status is not ("Completed" or "Cancelled"));

        /// <summary>Bật/tắt TỪNG NÚT theo đúng trạng thái chuyến đang chọn — chỉ
        /// cho bấm những thao tác hợp lệ (trước đây mọi nút đều bấm được ở mọi
        /// trạng thái, VD Duyệt cả chuyến chưa chạy / đã hoàn tất):
        ///   Bắt đầu            : Đã lên kế hoạch (+ chặng Giao: lấy hàng đã xong)
        ///   Hoàn tất           : Đang chạy
        ///   Duyệt / Từ chối    : Chờ xác nhận
        ///   Đổ xăng, So sánh xăng : đã chạy (Đang chạy / Chờ xác nhận / Hoàn tất)
        ///   Chi phí, Sự cố     : đã chạy hoặc đã hủy (chi phí phát sinh trước khi hủy)
        ///   Đồng bộ lịch, Thông tin tài xế : Đã lên kế hoạch / Đang chạy
        ///   Ước tính lộ trình  : chưa hủy
        ///   Xem lộ trình       : mọi trạng thái</summary>
        private void UpdateActionButtons()
        {
            var id = grid.CurrentRow?.Cells["Id"]?.Value as int?;
            var trip = id is null ? null : _trips.FirstOrDefault(t => t.Id == id.Value);
            var st = trip?.Status ?? "";

            var hasRun = st is "InProgress" or "PendingConfirmation" or "Completed";
            var active = st is "Planned" or "InProgress";

            btnStart.Enabled = trip is not null && st == "Planned" && !IsWaitingForPickup(trip);
            btnComplete.Enabled = st == "InProgress";
            btnApprove.Enabled = st == "PendingConfirmation";
            btnReject.Enabled = st == "PendingConfirmation";
            btnFuel.Enabled = hasRun;
            btnFuelEstimate.Enabled = hasRun;
            btnExpenses.Enabled = hasRun || st == "Cancelled";
            btnIncidents.Enabled = hasRun || st == "Cancelled";
            btnSyncCalendar.Enabled = active;
            btnDriverInfo.Enabled = active;
            btnQuickEstimate.Enabled = trip is not null && st != "Cancelled";
            btnViewRoute.Enabled = trip is not null;

            if (trip is null) return;
            lblStatus.ForeColor = Theme.TextSecondary;
            lblStatus.Text = $"Tổng {_trips.Count} chuyến   •   Đang chọn chuyến #{trip.Id} · {TranslateLegType(trip.LegType)} · " +
                (IsWaitingForPickup(trip) ? WaitingPickupText : TranslateStatus(trip.Status)) + " — " + st switch
                {
                    "Planned" when IsWaitingForPickup(trip) => "chờ chuyến lấy hàng của đơn hoàn tất mới bắt đầu được",
                    "Planned" => "có thể: Bắt đầu, Ước tính lộ trình, Đồng bộ lịch, gửi thông tin cho tài xế",
                    "InProgress" => "có thể: Hoàn tất, ghi Đổ xăng / Chi phí / Sự cố, theo dõi lộ trình",
                    "PendingConfirmation" => "tài xế đã báo xong — xem lộ trình rồi Duyệt hoặc Từ chối",
                    "Completed" => "đã hoàn tất — chỉ xem lại / bổ sung chi phí",
                    "Cancelled" => "đã hủy — chỉ xem lại / ghi chi phí đã phát sinh",
                    _ => "",
                };
        }

        private static string TranslateLegType(string s) => s switch
        {
            "Pickup" => "🔽 Lấy hàng",
            "Delivery" => "🔼 Giao hàng",
            "Return" => "🔙 Trả hàng",
            _ => s,
        };

        private static string TranslateStatus(string s) => s switch
        {
            "Planned" => "Đã lên kế hoạch",
            "InProgress" => "Đang chạy",
            "PendingConfirmation" => "🟡 Chờ xác nhận",
            "Completed" => "Hoàn tất",
            "Cancelled" => "Đã hủy",
            _ => s,
        };
    }
}
