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

        // Đang chờ server (Bắt đầu / Ước tính...) -> khóa nút, chống bấm 2 lần gửi 2 yêu cầu.
        private bool _busy;

        // Hàng thẻ số liệu ở đầu màn (bấm thẻ = lọc nhanh đúng nhóm đó).
        private readonly KpiStrip _kpi = new();

        public TripsControl()
        {
            // Giao diện (vị trí, cỡ chữ, nút) nằm hết trong TripsControl.Designer.cs — bố cục tự co
            // giãn (TableLayoutPanel + FlowLayoutPanel), không còn tính tọa độ bằng code.
            InitializeComponent();
            Theme.StyleGrid(grid);
            GridLayout.FitColumns(grid); // cột vừa nội dung, nhiều cột thì cuộn ngang thay vì cắt chữ
            Load += (_, _) => Theme.UniformButtons(toolbar); // nút đều nhau, xuống dòng vẫn thẳng cột
            grid.DataBindingComplete += (_, _) => { if (grid.Columns["Id"] != null) grid.Columns["Id"]!.Visible = false; };
            grid.SelectionChanged += (_, _) => UpdateActionButtons();

            // Bộ lọc (Common/FilterBar) — lọc ngay trên máy, không gọi lại API. Trạng thái lọc theo
            // ĐÚNG chữ đang hiện trên bảng (gồm các trạng thái chờ của chặng Giao).
            filterBar.AddChoice("status", "Trạng thái", new[] { "Đã lên kế hoạch", WaitingPickupText, WaitingReceiptText, WaitingIssueText,
                "Đang chạy", "🟡 Chờ xác nhận", "Hoàn tất", "Đã hủy" });
            filterBar.AddChoice("leg", "Chặng", new[] { "🔽 Lấy hàng", "🔼 Giao hàng", "🔙 Trả hàng" });
            filterBar.AddChoice("deadline", "Hạn giao", new[] { DlLate, DlWarn, DlOk, DlOnTime, DlDoneLate, DlNone });
            filterBar.AddChoice("start", "Xuất phát", new[] { StartToday, StartTomorrow, StartWeek, StartPast });
            filterBar.AddChoice("warehouse", "Kho", Array.Empty<string>());
            filterBar.FilterChanged += (_, _) => BindGrid();
            SetupModernLook();
            Load += async (_, _) => await LoadAsync();
        }

        /// <summary>Thẻ số liệu + nhãn màu trạng thái + thanh tiến độ kho + sáng dòng khi rê chuột.</summary>
        private void SetupModernLook()
        {
            _kpi.AddCard("all", "🛣️", "Tất cả chuyến", Theme.Primary);
            _kpi.AddCard("today", "🗓️", "Xuất phát hôm nay", GridDecor.AccentTeal, "start", StartToday);
            _kpi.AddCard("running", "🚚", "Đang chạy", Theme.PrimaryDark, "status", "Đang chạy");
            _kpi.AddCard("approve", "📝", "Chờ duyệt", Theme.Warning, "status", "🟡 Chờ xác nhận", alert: true);
            _kpi.AddCard("issue", "📤", "Chờ kho xuất", GridDecor.AccentViolet, "status", WaitingIssueText, alert: true);
            _kpi.AddCard("late", "⏰", "Trễ hạn giao", Theme.Danger, "deadline", DlLate, alert: true);
            _kpi.BindFilter(filterBar);
            KpiStrip.InsertRow(root, _kpi, 1);
            // Cửa sổ thấp: giữ bảng tối thiểu ~8 dòng, thiếu chỗ thì cả màn có thanh cuộn kéo xuống.
            ScrollFit.Enable(this, root, grid);

            GridDecor.HoverRows(grid);
            GridDecor.Pills(grid, "Trạng_thái", (text, _) => GridDecor.StatusTone(text));
            GridDecor.Pills(grid, "Chặng", (text, _) => text.Contains("Lấy") ? GridDecor.Teal
                : text.Contains("Giao") ? GridDecor.Blue : GridDecor.Violet);
            GridDecor.Pills(grid, "Tình_trạng_hạn", (_, row) => TripAt(row)?.DeadlineLevel switch
            {
                "Ok" or "DoneOnTime" => GridDecor.Green,
                "Warning" => GridDecor.Amber,
                "Late" or "DoneLate" => GridDecor.Red,
                _ => null,
            });
            GridDecor.ProgressBars(grid, "Kho", row =>
            {
                var t = TripAt(row);
                if (t is null || t.Status == "Cancelled" || !_progress.TryGetValue(t.Id, out var p) || p.QtyPlanned <= 0) return null;
                var ratio = (double)(p.QtyDone / p.QtyPlanned);
                var color = ratio >= 1 ? Theme.Success : t.LegType == "Pickup" ? GridDecor.AccentTeal : GridDecor.AccentViolet;
                return (ratio, WarehouseText(t), color);
            });
        }

        private TripListItem? TripAt(int row)
            => row >= 0 && row < grid.Rows.Count && grid.Rows[row].Cells["Id"].Value is int id
                ? _trips.FirstOrDefault(t => t.Id == id) : null;

        /// <summary>Số trên các thẻ — tính từ dữ liệu đã tải (không gọi thêm API).</summary>
        private void UpdateKpis()
        {
            var today = DateTime.Today;
            var open = _trips.Count(t => t.Status is "Planned" or "InProgress" or "PendingConfirmation");
            _kpi.SetValue("all", _trips.Count, $"{open} chuyến chưa xong");
            var startToday = _trips.Where(t => t.PlannedStartDate?.Date == today).ToList();
            _kpi.SetValue("today", startToday.Count, $"{startToday.Count(t => t.Status == "Planned")} chuyến chưa xuất phát");
            var running = _trips.Where(t => t.Status == "InProgress").ToList();
            _kpi.SetValue("running", running.Count,
                $"{running.Count(t => t.LegType == "Pickup")} lấy · {running.Count(t => t.LegType == "Delivery")} giao · {running.Count(t => t.LegType == "Return")} trả");
            _kpi.SetValue("approve", _trips.Count(t => t.Status == "PendingConfirmation"), "tài xế đã báo xong");
            var waitingReceipt = _trips.Count(IsWaitingForReceipt);
            _kpi.SetValue("issue", _trips.Count(IsWaitingForIssue), waitingReceipt > 0 ? $"+{waitingReceipt} chuyến chờ nhập kho" : "hàng đã nhập kho đủ");
            var warn = _trips.Count(t => t.DeadlineLevel == "Warning");
            _kpi.SetValue("late", _trips.Count(t => t.DeadlineLevel == "Late"), warn > 0 ? $"{warn} chuyến sắp tới mốc xuất phát" : "không có chuyến sắp trễ");
        }

        private const string DlLate = "🔴 Trễ / không còn kịp";
        private const string DlWarn = "🟡 Sắp tới mốc xuất phát";
        private const string DlOk = "🟢 Kịp hạn";
        private const string DlOnTime = "✅ Đã giao đúng hạn";
        private const string DlDoneLate = "⛔ Đã giao trễ";
        private const string DlNone = "Không áp dụng / chưa có hạn";
        private const string StartToday = "Hôm nay";
        private const string StartTomorrow = "Ngày mai";
        private const string StartWeek = "Trong 7 ngày tới";
        private const string StartPast = "Đã qua mà chưa chạy";

        private static string DeadlineGroup(TripListItem t) => t.DeadlineLevel switch
        {
            "Late" => DlLate,
            "Warning" => DlWarn,
            "Ok" => DlOk,
            "DoneOnTime" => DlOnTime,
            "DoneLate" => DlDoneLate,
            _ => DlNone,
        };

        /// <summary>Lọc theo NGÀY XUẤT PHÁT DỰ KIẾN (chọn lúc Chia xe).</summary>
        private static bool MatchesStart(TripListItem t, string? choice)
        {
            if (choice is null) return true;
            if (t.PlannedStartDate is not DateTime d) return false;
            var days = (d.Date - DateTime.Today).Days;
            return choice switch
            {
                StartToday => days == 0,
                StartTomorrow => days == 1,
                StartWeek => days >= 0 && days <= 7,
                StartPast => days < 0 && t.Status == "Planned",
                _ => true,
            };
        }

        /// <summary>Đổ lưới theo bộ lọc, giữ dòng đang chọn.</summary>
        /// <summary>Mở từ thông báo: chỉ hiện + chọn sẵn chuyến #id (gõ "#15" vào ô tìm cũng lọc ĐÚNG #15).</summary>
        public void FocusTrip(int tripId)
        {
            _focusTripId = tripId;
            filterBar.ShowOnlySearch($"#{tripId}");
        }

        private int? _focusTripId;

        private void BindGrid()
        {
            var keepId = _trips.Count > 0 && _focusTripId is int focus ? focus : GetSelectedTripId();
            if (_trips.Count > 0) _focusTripId = null;
            // "#15" -> đúng chuyến 15 (không lẫn #150, #151...).
            var exactId = System.Text.RegularExpressions.Regex.Match(filterBar.SearchText, @"^#(\d+)$") is { Success: true } m
                ? int.Parse(m.Groups[1].Value) : (int?)null;
            var rows = _trips.Where(t =>
                    (exactId is null || t.Id == exactId) &&
                    filterBar.Is("status", StatusText(t)) &&
                    filterBar.Is("leg", TranslateLegType(t.LegType)) &&
                    filterBar.Is("deadline", DeadlineGroup(t)) &&
                    MatchesStart(t, filterBar.Choice("start")) &&
                    filterBar.Is("warehouse", t.WarehouseName ?? "") &&
                    filterBar.MatchesSearch($"#{t.Id}", t.OrderCode, t.VehiclePlate, t.DriverName, t.WarehouseName, t.FromName, t.ToName))
                .ToList();
            grid.DataSource = rows.Select(t => new
            {
                t.Id,
                Mã_đơn = t.OrderCode,
                Biển_số_xe = t.VehiclePlate,
                Tài_xế = t.DriverName,
                Chặng = TranslateLegType(t.LegType),
                Điểm_đi = t.FromName ?? "—",
                Điểm_đến = t.ToName ?? "—",
                Trạng_thái = StatusText(t),
                Số_lượng = t.TotalQtyPlanned,
                Kho = WarehouseText(t),
                Xuất_phát_dự_kiến = t.PlannedStartDate?.ToString("dd/MM/yyyy") ?? "—",
                Hạn_giao = t.CommittedDeliveryDate?.ToString("dd/MM/yyyy") ?? "— (chưa có hạn)",
                Xuất_phát_muộn_nhất = t.LatestStartDate?.ToString("dd/MM/yyyy") ?? "—",
                Tình_trạng_hạn = t.DeadlineStatus,
            }).ToList();
            filterBar.SetCount(rows.Count, _trips.Count);
            if (keepId is int id)
                foreach (DataGridViewRow r in grid.Rows)
                    if (r.Cells["Id"].Value is int rid && rid == id) { grid.CurrentCell = r.Cells["Mã_đơn"]; break; }
            UpdateActionButtons();
        }

        public TripsControl(ApiServices api) : this()
        {
            _api = api;
        }

        private void BtnRefresh_Click(object? sender, EventArgs e) => _ = LoadAsync();

        private async void BtnStart_Click(object? sender, EventArgs e)
        {
            if (_busy) return;
            var tripId = GetSelectedTripId();
            if (tripId is null) { ShowNoSelection(); return; }

            SetBusy(true);
            try
            {
                var result = await _api!.Trips.StartTripAsync(tripId.Value);
                if (!result.Success)
                {
                    MessageBox.Show(FindForm(), result.Message ?? "Không thể bắt đầu chuyến.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    await LoadAsync(); // trạng thái có thể vừa bị người khác đổi -> tải lại, không để dữ liệu cũ
                    return;
                }
                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(FindForm(), "Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { SetBusy(false); }
        }

        /// <summary>Khóa / mở các nút thao tác trong lúc chờ server.</summary>
        private void SetBusy(bool busy)
        {
            _busy = busy;
            if (IsDisposed) return;
            UseWaitCursor = busy;
            if (busy)
            {
                btnStart.Enabled = btnComplete.Enabled = btnApprove.Enabled = btnReject.Enabled = false;
                btnQuickEstimate.Enabled = btnReassign.Enabled = btnReturnAtWarehouse.Enabled = false;
            }
            else UpdateActionButtons();
        }

        /// <summary>Thông báo của server khi thao tác THÀNH CÔNG (VD duyệt chuyến Lấy hàng: hàng
        /// chuyển sang Chờ nhập kho) — hiện cho Vận hành biết bước tiếp theo.</summary>
        private void ShowSuccessNote(string? message)
        {
            if (!string.IsNullOrWhiteSpace(message))
                MessageBox.Show(FindForm(), message, "Đã xong", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void BtnComplete_Click(object? sender, EventArgs e)
        {
            var tripId = GetSelectedTripId();
            if (tripId is null) { ShowNoSelection(); return; }

            var completeLeg = _trips.FirstOrDefault(t => t.Id == tripId.Value)?.LegType;
            var confirm = MessageBox.Show(FindForm(),
                "Xác nhận chuyến này đã hoàn tất (xe đã tới nơi, hàng đã giao/nhận xong)?" +
                (completeLeg == "Pickup" ? "\n\nHàng lấy về sẽ ở trạng thái \"Chờ nhập kho\" — nhập kho ở màn Kho → 📦 Nhập hàng từ chuyến." : ""),
                "Xác nhận hoàn tất chuyến", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                var result = await _api!.Trips.CompleteTripAsync(tripId.Value);
                if (!result.Success)
                {
                    MessageBox.Show(FindForm(), result.Message ?? "Không thể hoàn tất chuyến.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    await LoadAsync(); // trạng thái có thể vừa bị người khác đổi -> tải lại, không để dữ liệu cũ
                    return;
                }
                await LoadAsync();
                ShowSuccessNote(result.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(FindForm(), "Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>Xem danh sách hàng + ảnh hàng tài xế đã gửi (không duyệt).</summary>
        private void BtnCargoPhotos_Click(object? sender, EventArgs e)
        {
            var tripId = GetSelectedTripId();
            if (tripId is null) { ShowNoSelection(); return; }
            var trip = _trips.FirstOrDefault(t => t.Id == tripId.Value);
            if (trip is null || _api is null) return;
            using var f = new TripCargoReviewForm(_api, trip.Id, $"chuyến #{trip.Id} · {TranslateLegType(trip.LegType)} · đơn {trip.OrderCode}", reviewMode: false);
            f.ShowDialog(FindForm());
        }

        /// <summary>Đổi xe / tài xế cho chuyến chưa chạy (xe hỏng, giấy tờ hoặc bằng lái hết
        /// hạn, tài xế nghỉ...) — trước đây chuyến kẹt mãi, không bắt đầu cũng không hủy được.</summary>
        private void BtnReassign_Click(object? sender, EventArgs e)
        {
            var tripId = GetSelectedTripId();
            if (tripId is null) { ShowNoSelection(); return; }
            var trip = _trips.FirstOrDefault(t => t.Id == tripId.Value);
            if (trip is null || _api is null) return;

            using var f = new ReassignTripForm(_api, trip.Id, $"chuyến #{trip.Id} · {TranslateLegType(trip.LegType)} · đơn {trip.OrderCode}");
            f.ShowDialog(FindForm());
            // Tải lại CẢ khi không đổi được (server từ chối vì chuyến vừa được bắt đầu / đổi bởi người khác).
            _ = LoadAsync();
        }

        /// <summary>Chuyến TRẢ HÀNG chưa chạy -> khách nhận lại hàng tại kho: xuất kho, đóng
        /// chuyến, giải phóng xe + tài xế (khi xe của chuyến trả hàng không chạy được).</summary>
        private async void BtnReturnAtWarehouse_Click(object? sender, EventArgs e)
        {
            var tripId = GetSelectedTripId();
            if (tripId is null) { ShowNoSelection(); return; }
            var trip = _trips.FirstOrDefault(t => t.Id == tripId.Value);
            if (trip is null || _api is null) return;

            var confirm = MessageBox.Show(FindForm(),
                $"Khách nhận lại hàng NGAY TẠI KHO cho chuyến trả hàng #{trip.Id} (đơn {trip.OrderCode})?\n\n" +
                "Hàng sẽ được xuất kho, chuyến trả hàng đóng lại và xe + tài xế được giải phóng. Không hoàn tác được.",
                "Trả khách tại kho", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                var result = await new SERVICE_TripManagement.TripReassignApiService(_api.Connection).ReturnAtWarehouseAsync(trip.Id);
                MessageBox.Show(FindForm(), result.Message ?? (result.Success ? "Đã xong." : "Không thực hiện được."),
                    result.Success ? "Đã trả hàng" : "Thông báo", MessageBoxButtons.OK,
                    result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                await LoadAsync(); // cả khi thất bại: trạng thái có thể vừa bị thao tác khác đổi
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

            // BƯỚC KIỂM TRA: xem danh sách hàng tài xế đã tick + ẢNH HÀNG trước khi duyệt.
            var reviewTrip = _trips.FirstOrDefault(t => t.Id == tripId.Value);
            using (var review = new TripCargoReviewForm(_api!, tripId.Value,
                       $"chuyến #{tripId.Value} · {TranslateLegType(reviewTrip?.LegType ?? "")} · đơn {reviewTrip?.OrderCode}", reviewMode: true))
            {
                var decision = review.ShowDialog(FindForm());
                if (decision == DialogResult.No) { BtnReject_Click(sender, e); return; }
                if (decision != DialogResult.Yes) return;
            }

            // Nội dung theo ĐÚNG chặng (khớp server): Lấy hàng -> nhập kho, xe vẫn "Đang chạy"
            // để chạy tiếp chặng Giao; Giao hàng -> xe rảnh nếu không còn chuyến khác;
            // Trả hàng -> xuất kho trả đối tác, xe rảnh.
            var approveLeg = _trips.FirstOrDefault(t => t.Id == tripId.Value)?.LegType;
            var approveText = approveLeg switch
            {
                "Pickup" => "Sau khi duyệt, hàng ở trạng thái \"CHỜ NHẬP KHO\" — vào màn Kho → 📦 Nhập hàng từ chuyến để nhập (thủ công hoặc Best-Fit). " +
                            "Chặng Giao chỉ bắt đầu được khi hàng đã nhập kho đủ.",
                "Return" => "Sau khi duyệt, hàng sẽ được XUẤT KHO trả lại đối tác và xe được trả về trạng thái rảnh.",
                _ => "Sau khi duyệt, chuyến giao hàng hoàn tất; xe được trả về trạng thái rảnh nếu không còn chuyến nào khác.",
            };
            var confirm = MessageBox.Show(FindForm(),
                "Duyệt xác nhận của tài xế cho chuyến này? " + approveText,
                "Duyệt xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                var result = await _api!.Trips.ApproveCompletionAsync(tripId.Value);
                if (!result.Success)
                {
                    MessageBox.Show(FindForm(), result.Message ?? "Không thể duyệt.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    await LoadAsync(); // trạng thái có thể vừa bị người khác đổi -> tải lại, không để dữ liệu cũ
                    return;
                }
                await LoadAsync();
                ShowSuccessNote(result.Message);
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
                "Từ chối xác nhận của tài xế? Chuyến sẽ quay lại trạng thái Đang chạy để tài xế báo lại " +
                "(kiểm hàng + chụp ảnh lại).\n\n" +
                "⚠️ CHỈ từ chối khi tài xế báo NHẦM (hàng thật sự chưa tới nơi) hoặc ảnh / danh sách hàng không khớp.",
                "Từ chối xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            try
            {
                var result = await _api!.Trips.RejectCompletionAsync(tripId.Value);
                if (!result.Success)
                {
                    MessageBox.Show(FindForm(), result.Message ?? "Không thể từ chối.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    await LoadAsync(); // trạng thái có thể vừa bị người khác đổi -> tải lại, không để dữ liệu cũ
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
            if (_busy) return;
            var tripId = GetSelectedTripId();
            if (tripId is null) { ShowNoSelection(); return; }
            var trip = _trips.FirstOrDefault(t => t.Id == tripId.Value);
            if (trip is null || _api is null) return;

            SetBusy(true);
            try
            {
                // Gọi endpoint MỚI — vừa tính vừa LƯU LẠI vào Trip (không chỉ
                // hiện tạm thời rồi bỏ đi), để sau này làm được bảng thống kê
                // so sánh Ước tính (VietMap) vs Thực tế (StartTime/EndTime,
                // GPS thật) cho từng chuyến.
                var result = await _api.Trips.CalculateEstimateAsync(trip.Id);
                // Có số liệu kèm lời nhắn (VD chuyến đã chạy — giữ nguyên số kế hoạch cũ) = thông tin,
                // không phải lỗi; không có số liệu thì mới là "không ước tính được".
                if (result.Message is not null && result.DistanceKm <= 0)
                {
                    MessageBox.Show(FindForm(), result.Message, "Không ước tính được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (result.Message is not null)
                {
                    MessageBox.Show(FindForm(), $"{result.Message}\n\n📏 {result.DistanceKm:0.0} km · ⏱ {result.DurationMinutes / 60} giờ {result.DurationMinutes % 60} phút",
                        "Ước tính lộ trình", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var hours = result.DurationMinutes / 60;
                var minutes = result.DurationMinutes % 60;
                var legText = LegRouteText(trip.LegType);

                MessageBox.Show(FindForm(),
                    $"Đơn {trip.OrderCode} — Chặng {legText}\n\n" +
                    $"📏 Khoảng cách ước tính: {result.DistanceKm:0.0} km\n" +
                    $"⏱ Thời gian ước tính: {(hours > 0 ? $"{hours} giờ " : "")}{minutes} phút\n\n" +
                    $"✅ Đã lưu lại vào chuyến để đối chiếu với số liệu thực tế sau này.\n" +
                    $"(Nguồn: VietMap — số liệu THEO ĐƯỜNG THẬT, không phải đường chim bay)",
                    "Ước tính lộ trình", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _ = LoadAsync(); // cập nhật "đã có ước tính" cho nút
            }
            catch (Exception ex)
            {
                MessageBox.Show(FindForm(), "Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { SetBusy(false); }
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

                var legName = LegRouteText(trip.LegType);
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
            _kpi.Busy = true;
            try
            {
                _trips = await _api.Trips.GetTripsAsync();
                try { _progress = (await _api.Trips.GetWarehouseProgressAsync()).ToDictionary(p => p.TripId); }
                catch { _progress = new(); } // API cũ chưa có -> chỉ thiếu cột Kho, không chặn cả màn hình
                filterBar.SetChoiceOptions("warehouse", _trips.Select(t => t.WarehouseName ?? ""));
                BindGrid();
                UpdateKpis();
                var late = _trips.Count(t => t.DeadlineLevel == "Late");
                var warn = _trips.Count(t => t.DeadlineLevel == "Warning");
                lblStatus.Text = $"Tổng {_trips.Count} chuyến — chọn 1 dòng rồi bấm Bắt đầu/Hoàn tất để đổi trạng thái" +
                                 (late + warn > 0 ? $"   ⏰ Hạn giao: {late} chuyến trễ / không còn kịp, {warn} chuyến phải xuất phát trong hôm nay–ngày mai" : "");
                UpdateActionButtons();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
            finally { _kpi.Busy = false; }
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
                if (done > 0) return $"📥 Đã nhập {done:0.##}/{planned:0.##}" + (t.Status == "Completed" ? " — còn chờ nhập" : "");
                if (t.Status == "Completed") return $"📦 Chờ nhập kho {planned - done:0.##}/{planned:0.##}";
                return t.Status == "PendingConfirmation" ? "⏳ Duyệt xong → chờ nhập kho" : $"Chưa về kho (0/{planned:0.##})";
            }
            var verb = t.LegType == "Return" ? "xuất trả" : "xuất";
            if (planned > 0 && done >= planned) return $"✅ Đã {verb} {done:0.##}/{planned:0.##}";
            if (done > 0) return $"📤 Đã {verb} {done:0.##}/{planned:0.##}";
            return t.LegType == "Return" ? $"Xuất khi trả xong (0/{planned:0.##})"
                : t.Status == "Planned" ? $"📤 Chờ kho xuất (0/{planned:0.##})"
                : $"Chưa xuất (0/{planned:0.##})";
        }

        /// <summary>Chặng GIAO còn "Đã lên kế hoạch" nhưng đơn vẫn còn chặng LẤY
        /// HÀNG chưa Hoàn tất (chưa chạy / đang chạy / chờ duyệt) — hàng chưa về
        /// kho nên chưa được bắt đầu giao. Backend cũng chặn y hệt (StartAsync),
        /// ở đây chỉ để Vận hành nhìn danh sách là biết ngay.</summary>
        private bool IsWaitingForPickup(TripListItem t)
            => t.LegType == "Delivery" && t.Status == "Planned"
               && _trips.Any(p => p.OrderCode == t.OrderCode && p.LegType == "Pickup"
                                  && p.Status is not ("Completed" or "Cancelled"));

        private const string WaitingReceiptText = "⏳ Chờ nhập kho";

        /// <summary>Chặng GIAO chưa chạy, lấy hàng đã xong nhưng hàng của chuyến lấy hàng CHƯA
        /// NHẬP KHO đủ (màn Kho → Hàng chờ nhập) — server cũng chặn bắt đầu y hệt.</summary>
        private bool IsWaitingForReceipt(TripListItem t)
            => t.AwaitingReceipt && !IsWaitingForPickup(t); // server tính (cùng quy tắc chặn Bắt đầu)

        private const string WaitingIssueText = "📤 Chờ kho xuất";

        /// <summary>Chặng GIAO chưa chạy, hàng đã nhập kho nhưng KHO CHƯA XÁC NHẬN XUẤT đủ lên xe
        /// (màn Kho → 📤 Xuất kho theo chuyến) — server chặn Bắt đầu cho tới khi xuất xong.</summary>
        private bool IsWaitingForIssue(TripListItem t)
            => t.LegType == "Delivery" && t.Status == "Planned" && !IsWaitingForPickup(t) && !IsWaitingForReceipt(t)
               && _progress.TryGetValue(t.Id, out var p) && p.QtyPlanned > 0 && p.QtyDone < p.QtyPlanned;

        private string StatusText(TripListItem t)
            => IsWaitingForPickup(t) ? WaitingPickupText
             : IsWaitingForReceipt(t) ? WaitingReceiptText
             : IsWaitingForIssue(t) ? WaitingIssueText
             : TranslateStatus(t.Status);

        /// <summary>Bật/tắt TỪNG NÚT theo đúng trạng thái chuyến đang chọn — chỉ
        /// cho bấm những thao tác hợp lệ (trước đây mọi nút đều bấm được ở mọi
        /// trạng thái, VD Duyệt cả chuyến chưa chạy / đã hoàn tất):
        ///   Bắt đầu            : Đã lên kế hoạch (+ chặng Giao: lấy hàng đã xong)
        ///   Hoàn tất           : Đang chạy
        ///   Duyệt / Từ chối    : Chờ xác nhận
        ///   Đổ xăng, So sánh xăng : đã chạy (Đang chạy / Chờ xác nhận / Hoàn tất)
        ///   Chi phí, Sự cố     : đã chạy hoặc đã hủy (chi phí phát sinh trước khi hủy)
        ///   Đồng bộ lịch, Thông tin tài xế : Đã lên kế hoạch / Đang chạy
        ///   Ước tính lộ trình  : Đã lên kế hoạch (là SỐ KẾ HOẠCH — chuyến đã chạy thì giữ nguyên)
        ///   Xem lộ trình       : mọi trạng thái</summary>
        private void UpdateActionButtons()
        {
            var id = grid.CurrentRow?.Cells["Id"]?.Value as int?;
            var trip = id is null ? null : _trips.FirstOrDefault(t => t.Id == id.Value);
            var st = trip?.Status ?? "";

            // "Đã từng chạy" lấy từ server (có giờ bắt đầu) — chuyến hủy GIỮA ĐƯỜNG vẫn ghi
            // được xăng/chi phí; chuyến hủy TRƯỚC khi chạy hoặc trả khách tại kho thì không.
            var hasRun = trip?.HasRun == true;
            var active = st is "Planned" or "InProgress";

            // Đang chờ server (_busy): giữ khóa các nút thao tác, kể cả khi đổi dòng chọn.
            btnStart.Enabled = !_busy && trip is not null && st == "Planned" && !IsWaitingForPickup(trip) && !IsWaitingForReceipt(trip) && !IsWaitingForIssue(trip);
            btnComplete.Enabled = !_busy && st == "InProgress";
            btnApprove.Enabled = !_busy && st == "PendingConfirmation";
            btnReject.Enabled = !_busy && st == "PendingConfirmation";
            // Chuyến hủy vẫn mở được form xăng: xem lịch sử; ghi thêm chỉ khi chuyến đã từng chạy (server quyết định).
            btnFuel.Enabled = hasRun || st == "Cancelled"; // chuyến hủy vẫn mở được để XEM lịch sử
            btnFuelEstimate.Enabled = hasRun;
            btnExpenses.Enabled = hasRun;
            btnIncidents.Enabled = hasRun;
            btnSyncCalendar.Enabled = active;
            btnDriverInfo.Enabled = active;
            // Ước tính = SỐ KẾ HOẠCH: chuyến chưa chạy, hoặc chuyến đã chạy mà CHƯA có ước tính
            // (VD app tự bắt đầu chuyến trước khi kịp bấm) — đã có thì server giữ nguyên.
            btnQuickEstimate.Enabled = !_busy && trip is not null && st != "Cancelled" && (st == "Planned" || !trip.HasEstimate);
            btnViewRoute.Enabled = trip is not null;
            // Gỡ chuyến kẹt: chỉ chuyến CHƯA CHẠY; "Trả khách tại kho" chỉ cho chuyến Trả hàng.
            btnReassign.Enabled = !_busy && st == "Planned";
            btnReturnAtWarehouse.Enabled = !_busy && st == "Planned" && trip?.LegType == "Return";
            btnCargoPhotos.Enabled = trip?.HasRun == true; // ảnh hàng tài xế chụp lúc xác nhận chặng

            if (trip is null) return;
            lblStatus.ForeColor = Theme.TextSecondary;
            lblStatus.Text = $"Tổng {_trips.Count} chuyến   •   Đang chọn chuyến #{trip.Id} · {TranslateLegType(trip.LegType)} · " +
                StatusText(trip) + " — " + st switch
                {
                    "Planned" when IsWaitingForPickup(trip) => "chờ chuyến lấy hàng của đơn hoàn tất mới bắt đầu được",
                    "Planned" when IsWaitingForReceipt(trip) => "hàng lấy về CHƯA nhập kho — nhập ở màn Kho → 📦 Nhập hàng từ chuyến rồi mới bắt đầu được",
                    "Planned" when IsWaitingForIssue(trip) => "kho CHƯA xác nhận xuất hàng lên xe — màn Kho → 📤 Xuất kho theo chuyến, xuất xong mới bắt đầu được",
                    "Planned" => "có thể: Bắt đầu, Ước tính lộ trình, Đồng bộ lịch, gửi thông tin cho tài xế",
                    "InProgress" => "có thể: Hoàn tất, ghi Đổ xăng / Chi phí / Sự cố, theo dõi lộ trình",
                    "PendingConfirmation" => "tài xế đã báo xong — xem ảnh hàng / lộ trình rồi Duyệt hoặc Từ chối",
                    "Completed" when trip.LegType == "Pickup" && WarehouseText(trip).Contains("chờ nhập", StringComparison.OrdinalIgnoreCase) => "đã về kho — hàng CHỜ NHẬP KHO (màn Kho → 📦 Nhập hàng từ chuyến)",
                    // Theo HasRun (server): chuyến không chạy xe (VD trả khách tại kho) thì không có xăng/chi phí để bổ sung.
                    "Completed" => hasRun ? "đã hoàn tất — chỉ xem lại / bổ sung chi phí" : "đã đóng mà không chạy xe (trả khách tại kho) — chỉ xem lại",
                    "Cancelled" => hasRun ? "đã hủy giữa đường — xem lại / ghi chi phí đã phát sinh" : "đã hủy trước khi chạy — chỉ xem lại",
                    _ => "",
                };
        }

        /// <summary>Mô tả lộ trình theo chặng — khớp server (chặng Trả hàng chở về NGƯỜI GỬI).</summary>
        private static string LegRouteText(string legType) => legType switch
        {
            "Pickup" => "Lấy hàng (round-trip qua Kho)",
            "Return" => "Trả hàng (Kho → Người gửi)",
            _ => "Giao hàng (Kho → Khách nhận)",
        };

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
