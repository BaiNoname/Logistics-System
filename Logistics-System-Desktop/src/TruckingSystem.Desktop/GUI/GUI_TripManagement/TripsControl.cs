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
        private ScreenKit.GridOverlay _overlay = null!; // bảng trống / đang tải (Common/ScreenKit.cs)



        public TripsControl()
        {
            // Giao diện (vị trí, cỡ chữ, nút) nằm hết trong TripsControl.Designer.cs — bố cục tự co
            // giãn (TableLayoutPanel + FlowLayoutPanel), không còn tính tọa độ bằng code.
            InitializeComponent();
            _overlay = ScreenKit.Overlay(grid, () => _trips.Count, "Chưa có chuyến xe nào",
                "Chuyến được tạo khi bạn 🚚 Chia xe cho đơn ở màn Đơn vận chuyển.", null,
                () => { _pendingOnly = false; filterBar.ClearFilters(); BindGrid(); });
            // Enter: dòng ĐƠN -> thu gọn / mở; dòng chuyến -> xem lộ trình.
            ScreenKit.Shortcuts(this, grid, null, null, () =>
            {
                if (CurrentGroupCode() is string code) ToggleGroup(code);
                else BtnViewRoute_Click(this, EventArgs.Empty);
            });
            ScreenKit.Help(this, "trips", "Theo dõi và điều hành từng chuyến xe (Lấy hàng · Giao hàng · Trả hàng).",
                new[] { "Chọn 1 chuyến — thẻ \"Chuyến đang chọn\" cho biết việc tiếp theo", "Bấm nút xanh: Bắt đầu / Hoàn tất / Duyệt",
                        "Chi phí, xăng, sự cố: menu 💰 hoặc chuột phải vào dòng" },
                "Enter xem lộ trình · chuột phải = mọi thao tác · F5 làm mới · Ctrl+F tìm nhanh",
                new[]
                {
                    ColorLegend.Header("Ô vuông ở dòng ĐƠN + dải màu bên trái các chuyến (trạng thái đơn)"),
                    ColorLegend.Bar(ColorLegend.Gray, "Chưa bắt đầu — chưa xe nào chạy"),
                    ColorLegend.Bar(ColorLegend.Amber, "Đang thực hiện — đã có chuyến chạy, chưa xong hết"),
                    ColorLegend.Bar(ColorLegend.Green, "Hoàn tất — mọi chuyến đã xong"),
                    ColorLegend.Bar(ColorLegend.Red, "Đã hủy — mọi chuyến của đơn bị hủy"),
                    ColorLegend.Note("Chuỗi chặng ở dòng đơn: ✓ đã xong · ▶ đang chạy · ⏳ đang chờ (duyệt / kho) · ○ chưa chạy"),
                    ColorLegend.Header("Cột Chặng"),
                    ColorLegend.Pill("Lấy hàng", GridDecor.Teal, "Xe đi lấy hàng của khách (về kho)"),
                    ColorLegend.Pill("Giao hàng", GridDecor.Blue, "Xe chở hàng đi giao cho người nhận"),
                    ColorLegend.Pill("Trả hàng", GridDecor.Violet, "Chở hàng trả lại"),
                    ColorLegend.Header("Cột Trạng thái (chuyến)"),
                    ColorLegend.Pill("Đã lên kế hoạch", GridDecor.Teal, "Chưa chạy, chờ xuất phát"),
                    ColorLegend.Pill("Đang chạy", GridDecor.Blue, "Xe đang trên đường"),
                    ColorLegend.Pill("🟡 Chờ xác nhận", GridDecor.Amber, "Đang chờ người khác làm: duyệt, lấy hàng, nhập kho"),
                    ColorLegend.Pill("📤 Chờ kho xuất", GridDecor.Violet, "Kho chưa xuất hàng lên xe"),
                    ColorLegend.Pill("Hoàn tất", GridDecor.Green, "Chuyến đã xong"),
                    ColorLegend.Pill("Đã hủy", GridDecor.Red, "Chuyến bị hủy"),
                    ColorLegend.Header("Cột Hạn"),
                    ColorLegend.Pill("🟢 Kịp", GridDecor.Green, "Kịp hạn / đã giao đúng hạn"),
                    ColorLegend.Pill("🟡 Sắp tới mốc", GridDecor.Amber, "Sắp tới mốc xuất phát muộn nhất"),
                    ColorLegend.Pill("🔴 Trễ", GridDecor.Red, "Đã trễ hạn / đã giao trễ"),
                    ColorLegend.Header("Ô \"việc tiếp theo\" trên thẻ phía trên"),
                    ColorLegend.Pill("👉 …", GridDecor.Blue, "Việc bình thường — bấm nút bên cạnh"),
                    ColorLegend.Pill("👉 …", GridDecor.Amber, "Đang vướng / cần làm ở màn khác (VD màn Kho)"),
                    ColorLegend.Pill("👉 …", GridDecor.Green, "Đã xong, chỉ xem lại"),
                });
            lblTitle.Text = "Quản lý chuyến xe"; // bỏ chữ "(Trip)" — đặt bằng code, không sửa Designer
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
            FilterMemory.Attach(filterBar, "trips"); // nhớ bộ lọc khi chuyển màn rồi quay lại
            SetupModernLook();
            SetupActions(); // thanh nút gọn + menu chuột phải + thẻ "Chuyến đang chọn" (TripsControl.Actions.cs)
            SetupGrouping(); // gom chuyến theo đơn: dòng đơn + dải màu + thẻ đơn (TripsControl.Groups.cs)
            // Dải nhắc việc (giải thích số đỏ trên menu): chuyến tài xế báo xong chờ duyệt.
            KpiStrip.InsertRow(root, _pendingBanner, root.GetRow(_selectedBar));
            _pendingBanner.FilterToggled += (_, on) => { _pendingOnly = on; BindGrid(); };
            // Ẩn / khóa sẵn các nút theo "chưa chọn dòng" TRƯỚC khi màn hiện — không còn cảnh hiện cả loạt nút rồi tải xong mới ẩn đi.
            UpdatePrimaryButtons(null); // (không gọi UpdateActionButtons: thẻ sẽ báo nhầm "Chưa có chuyến nào" khi chưa tải xong)
            Load += async (_, _) => await LoadAsync();
        }

        /// <summary>Thẻ số liệu + nhãn màu trạng thái + thanh tiến độ kho + sáng dòng khi rê chuột.</summary>
        private void SetupModernLook()
        {
            // Bỏ hàng 6 thẻ số liệu (góp ý GVHD: rối mắt) — số cần chú ý gom vào 1 dòng ngắn (SummaryText),
            // lọc nhanh dùng bộ lọc Trạng thái / Hạn giao / Xuất phát ngay bên trên bảng.
            // Cửa sổ thấp: giữ bảng tối thiểu ~8 dòng, thiếu chỗ thì cả màn có thanh cuộn kéo xuống.
            ScrollFit.Enable(this, root, grid);

            GridDecor.HoverRows(grid);
            // Đã hủy = ĐỎ (quy ước 4 màu của màn Đơn / Chuyến) — riêng màn này, không đổi màu chung của app.
            GridDecor.Pills(grid, "Trạng_thái", (text, _) => text == "Đã hủy" ? GridDecor.Red : GridDecor.StatusTone(text));
            GridDecor.Pills(grid, "Chặng", (text, _) => text.Contains("Lấy") ? GridDecor.Teal
                : text.Contains("Giao") ? GridDecor.Blue : GridDecor.Violet);
            // Rê chuột vào ô "Hạn" -> câu đầy đủ (trễ mấy ngày / phải xuất phát trước lúc nào), khỏi phải bấm dòng.
            grid.CellToolTipTextNeeded += (_, e) =>
            {
                if (e.RowIndex >= 0 && grid.Columns[e.ColumnIndex].Name == "Hạn" && TripAt(e.RowIndex) is { } t
                    && !string.IsNullOrWhiteSpace(t.DeadlineStatus) && t.DeadlineStatus != "—")
                    e.ToolTipText = t.DeadlineStatus;
            };
            GridDecor.Pills(grid, "Hạn", (_, row) => TripAt(row)?.DeadlineLevel switch
            {
                "Ok" or "DoneOnTime" => GridDecor.Green,
                "Warning" => GridDecor.Amber,
                "Late" or "DoneLate" => GridDecor.Red,
                _ => null,
            });
            // Cột "Kho" (tiến độ nhập/xuất) bỏ khỏi bảng cho gọn — xem ở thẻ "Chuyến đang chọn".
        }

        private TripListItem? TripAt(int row)
            => row >= 0 && row < grid.Rows.Count && grid.Rows[row].Cells["Id"].Value is int id
                ? _trips.FirstOrDefault(t => t.Id == id) : null;

        /// <summary>1 dòng tóm tắt thay cho 6 thẻ số liệu: tổng số chuyến + CHỈ những việc cần chú ý (có số > 0).</summary>
        private string SummaryText()
        {
            var parts = new List<string> { $"Tổng {_trips.Select(t => t.OrderCode).Distinct().Count()} đơn · {_trips.Count} chuyến" };
            void Add(int n, string text) { if (n > 0) parts.Add($"{text}: {n}"); }
            Add(_trips.Count(t => t.Status == "InProgress"), "🚚 đang chạy");
            Add(_trips.Count(t => t.Status == "PendingConfirmation"), "📝 chờ duyệt");
            Add(_trips.Count(IsWaitingForReceipt), "📦 chờ nhập kho");
            Add(_trips.Count(IsWaitingForIssue), "📤 chờ kho xuất");
            Add(_trips.Count(t => t.Status == "Planned" && t.PlannedStartDate?.Date == DateTime.Today), "🗓 xuất phát hôm nay");
            Add(_trips.Count(t => t.DeadlineLevel == "Late"), "🔴 trễ hạn");
            Add(_trips.Count(t => t.DeadlineLevel == "Warning"), "🟡 sắp tới mốc xuất phát");
            return string.Join("   ·   ", parts) + "   —   chuột phải vào 1 chuyến để xem mọi thao tác";
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

        /// <summary>Nhãn hạn NGẮN cho cột "Hạn" (câu đầy đủ ở thẻ "Chuyến đang chọn").</summary>
        private static string DeadlineShort(TripListItem t) => t.DeadlineLevel switch
        {
            "Late" => "🔴 Trễ",
            "Warning" => "🟡 Sắp tới mốc",
            "Ok" => "🟢 Kịp",
            "DoneOnTime" => "✅ Đúng hạn",
            "DoneLate" => "⛔ Giao trễ",
            _ => "—",
        };

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

        /// <summary>Mở từ màn Đơn: chỉ hiện chuyến của đơn này, chọn sẵn chuyến cần làm tiếp (không có thì chọn dòng đơn).</summary>
        public void FocusOrder(string orderCode)
        {
            _focusOrderCode = orderCode;
            filterBar.ShowOnlySearch(orderCode);
        }

        private string? _focusOrderCode;

        private readonly PendingBanner _pendingBanner = new();
        private bool _pendingOnly;

        /// <summary>Mở từ số đỏ trên menu trái: bỏ bộ lọc cũ, chỉ hiện chuyến chờ duyệt.</summary>
        public void ShowPendingOnly()
        {
            _pendingOnly = true;
            FilterMemory.Cancel(filterBar);
        }

        private void BindGrid()
        {
            var keepId = _trips.Count > 0 && _focusTripId is int focus ? focus : GetSelectedTripId();
            var keepGroup = keepId is null ? CurrentGroupCode() : null; // đang chọn dòng ĐƠN -> giữ dòng đơn đó
            var jumpToFocus = _trips.Count > 0 && (_focusTripId is not null || _focusOrderCode is not null); // mở từ thông báo / màn Đơn
            var focusOrder = _trips.Count > 0 ? _focusOrderCode : null;
            if (_trips.Count > 0) _focusOrderCode = null;
            if (_trips.Count > 0) _focusTripId = null;
            // Giữ nguyên VỊ TRÍ CUỘN khi dựng lại bảng (thu gọn / mở đơn, lọc, tải lại) — không nhảy về đầu danh sách.
            var keepScroll = grid.Rows.Count > 0 ? grid.FirstDisplayedScrollingRowIndex : -1;
            var keepHScroll = grid.HorizontalScrollingOffset;
            // "#15" -> đúng chuyến 15 (không lẫn #150, #151...).
            var exactId = System.Text.RegularExpressions.Regex.Match(filterBar.SearchText, @"^#(\d+)$") is { Success: true } m
                ? int.Parse(m.Groups[1].Value) : (int?)null;
            var filtered = _trips.Where(t =>
                    (!_pendingOnly || t.Status == "PendingConfirmation") &&
                    (exactId is null || t.Id == exactId) &&
                    filterBar.Is("status", StatusText(t)) &&
                    filterBar.Is("leg", TranslateLegType(t.LegType)) &&
                    filterBar.Is("deadline", DeadlineGroup(t)) &&
                    MatchesStart(t, filterBar.Choice("start")) &&
                    filterBar.Is("warehouse", t.WarehouseName ?? "") &&
                    filterBar.MatchesSearch($"#{t.Id}", t.OrderCode, t.VehiclePlate, t.DriverName, t.WarehouseName, t.FromName, t.ToName))
                .ToList();
            var rows = filtered;
            // GOM THEO ĐƠN (TripsControl.Groups.cs): mỗi đơn 1 dòng đầu nhóm (mã đơn, chuỗi chặng, tiến độ, việc
            // tiếp theo) + các chuyến của đơn ngay bên dưới (├ / └, cùng dải màu). Cột "Mã đơn" bỏ vì đã nằm ở dòng đơn.
            // Chi tiết từng chuyến hiện ở thẻ phía trên khi bấm 1 dòng. Ô tìm vẫn tìm theo mã đơn, kho, điểm đi, điểm đến.
            grid.DataSource = BuildGroupedRows(rows, keepId);
            filterBar.SetCount(rows.Count, _trips.Count);
            var waitApprove = _trips.Count(t => t.Status == "PendingConfirmation");
            _pendingBanner.Update($"{waitApprove} chuyến tài xế báo xong, chờ duyệt", waitApprove, _pendingOnly);
            if (keepId is int id)
            {
                foreach (DataGridViewRow r in grid.Rows)
                    if (r.Cells["Id"].Value is int rid && rid == id) { grid.CurrentCell = r.Cells["Chuyến"]; break; }
            }
            else if (keepGroup is not null) SelectGroupRow(keepGroup);
            else if (focusOrder is not null && _groups.TryGetValue(focusOrder, out var fg))
            {
                // Mở từ màn Đơn: chọn chuyến cần làm tiếp của đơn (không có thì chọn dòng đơn để xem cả đơn).
                if (fg.NextTripId is int nextId)
                {
                    foreach (DataGridViewRow r in grid.Rows)
                        if (r.Cells["Id"].Value is int rid && rid == nextId) { grid.CurrentCell = r.Cells["Chuyến"]; break; }
                }
                else SelectGroupRow(focusOrder);
            }
            if (!jumpToFocus) RestoreScroll(keepScroll, keepHScroll);
            UpdateActionButtons();
        }

        /// <summary>Đưa bảng về đúng dòng đang ở đầu khung nhìn trước khi dựng lại (nếu dòng đang chọn vẫn nằm trong khung).</summary>
        private void RestoreScroll(int firstRow, int hOffset)
        {
            try
            {
                if (firstRow >= 0 && grid.Rows.Count > 0)
                    grid.FirstDisplayedScrollingRowIndex = Math.Min(firstRow, grid.Rows.Count - 1);
                if (hOffset > 0) grid.HorizontalScrollingOffset = hOffset;
            }
            catch (InvalidOperationException)
            {
                // Bảng chưa vẽ xong / không đủ chỗ hiện dòng — bỏ qua, chỉ là giữ vị trí cuộn.
            }
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
                    Ask.Fail(this, result.Message ?? "Không thể bắt đầu chuyến.");
                    await LoadAsync(); // trạng thái có thể vừa bị người khác đổi -> tải lại, không để dữ liệu cũ
                    return;
                }
                await LoadAsync();
                Toast.Show(this, $"▶️ Đã bắt đầu chuyến #{tripId.Value} — tài xế đã được báo trên app.");
            }
            catch (Exception ex)
            {
                Ask.Fail(this, "Lỗi: " + ex.Message);
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
            // Thông báo nhẹ góc dưới, tự tắt (bấm để tắt ngay) — không bắt bấm OK.
            if (!string.IsNullOrWhiteSpace(message)) Toast.Show(this, "✅ " + message);
        }

        private async void BtnComplete_Click(object? sender, EventArgs e)
        {
            var tripId = GetSelectedTripId();
            if (tripId is null) { ShowNoSelection(); return; }

            var completeLeg = _trips.FirstOrDefault(t => t.Id == tripId.Value)?.LegType;
            // Chuyến giao cuối mà hàng nằm kho quá hạn chưa chọn do ai -> hỏi luôn trong hộp (TripsControl.Finish.cs).
            var (go, party, reason) = await AskFinishAsync(tripId.Value, $"Hoàn tất chuyến #{tripId.Value}?",
                    "Xe đã tới nơi, hàng đã giao / nhận xong." +
                    (completeLeg == "Pickup" ? "\n\nHàng lấy về sẽ ở trạng thái \"Chờ nhập kho\" — nhập kho ở màn Kho → 📦 Nhập hàng từ chuyến." : ""),
                    $"✅ Hoàn tất chuyến #{tripId.Value}");
            if (!go) return;

            try
            {
                var result = await _api!.Trips.CompleteTripAsync(tripId.Value, party, reason);
                if (!result.Success)
                {
                    Ask.Fail(this, result.Message ?? "Không thể hoàn tất chuyến.");
                    await LoadAsync(); // trạng thái có thể vừa bị người khác đổi -> tải lại, không để dữ liệu cũ
                    return;
                }
                await LoadAsync();
                ShowSuccessNote(result.Message);
            }
            catch (Exception ex)
            {
                Ask.Fail(this, "Lỗi: " + ex.Message);
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

            if (!Ask.Confirm(this, $"Trả hàng tại kho cho chuyến #{trip.Id}?",
                    $"Khách nhận lại hàng của đơn {trip.OrderCode} ngay tại kho: hàng xuất kho, chuyến trả hàng đóng lại, xe + tài xế được giải phóng.\n\nKhông hoàn tác được.",
                    "🏬 Trả hàng cho khách", danger: true)) return;

            try
            {
                var result = await new SERVICE_TripManagement.TripReassignApiService(_api.Connection).ReturnAtWarehouseAsync(trip.Id);
                if (result.Success) Toast.Show(this, "✅ " + (result.Message ?? $"Đã trả hàng tại kho — chuyến #{trip.Id} đã đóng."));
                else Ask.Fail(this, result.Message ?? "Không thực hiện được.");
                await LoadAsync(); // cả khi thất bại: trạng thái có thể vừa bị thao tác khác đổi
            }
            catch (Exception ex)
            {
                Ask.Fail(this, "Lỗi: " + ex.Message);
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
                "Pickup" => "Sau khi duyệt, hàng ở trạng thái \"CHỜ NHẬP KHO\" — vào màn Kho → 📦 Nhập hàng từ chuyến để nhập (tự chọn vị trí hoặc để hệ thống tự xếp). " +
                            "Chặng Giao chỉ bắt đầu được khi hàng đã nhập kho đủ.",
                "Return" => "Sau khi duyệt, hàng sẽ được XUẤT KHO trả lại đối tác và xe được trả về trạng thái rảnh.",
                _ => "Sau khi duyệt, chuyến giao hàng hoàn tất; xe được trả về trạng thái rảnh nếu không còn chuyến nào khác.",
            };
            var (go, party, reason) = await AskFinishAsync(tripId.Value, $"Duyệt chuyến #{tripId.Value}?", approveText, $"✅ Duyệt chuyến #{tripId.Value}");
            if (!go) return;

            try
            {
                var result = await _api!.Trips.ApproveCompletionAsync(tripId.Value, party, reason);
                if (!result.Success)
                {
                    Ask.Fail(this, result.Message ?? "Không thể duyệt.");
                    await LoadAsync(); // trạng thái có thể vừa bị người khác đổi -> tải lại, không để dữ liệu cũ
                    return;
                }
                await LoadAsync();
                ShowSuccessNote(result.Message);
            }
            catch (Exception ex)
            {
                Ask.Fail(this, "Lỗi: " + ex.Message);
            }
        }

        private async void BtnReject_Click(object? sender, EventArgs e)
        {
            var tripId = GetSelectedTripId();
            if (tripId is null) { ShowNoSelection(); return; }

            if (!Ask.Confirm(this, $"Từ chối xác nhận chuyến #{tripId.Value}?",
                    "Chuyến quay lại trạng thái Đang chạy để tài xế báo lại (kiểm hàng + chụp ảnh lại).\n\n" +
                    "Chỉ từ chối khi tài xế báo NHẦM (hàng thật sự chưa tới nơi) hoặc ảnh / danh sách hàng không khớp.",
                    "↩️ Từ chối, trả về tài xế", danger: true)) return;

            try
            {
                var result = await _api!.Trips.RejectCompletionAsync(tripId.Value);
                if (!result.Success)
                {
                    Ask.Fail(this, result.Message ?? "Không thể từ chối.");
                    await LoadAsync(); // trạng thái có thể vừa bị người khác đổi -> tải lại, không để dữ liệu cũ
                    return;
                }
                await LoadAsync();
                Toast.Show(this, $"↩️ Đã trả chuyến #{tripId.Value} về tài xế để báo lại.", ToastKind.Info);
            }
            catch (Exception ex)
            {
                Ask.Fail(this, "Lỗi: " + ex.Message);
            }
        }

        private async void BtnFuel_Click(object? sender, EventArgs e)
        {
            var tripId = GetSelectedTripId();
            if (tripId is null) { ShowNoSelection(); return; }

            using var f = new RecordFuelForm(_api!, tripId.Value);
            if (f.ShowDialog(FindForm()) == DialogResult.OK)
            {
                Toast.Show(this, "✅ Đã ghi nhận đổ xăng.");
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
                Toast.Show(this, "🗓️ Đồng bộ lịch Google\n" + message,
                    result.DriverSynced && result.CustomerSynced ? ToastKind.Success : ToastKind.Warning);
            }
            catch (Exception ex)
            {
                Ask.Fail(this, "Lỗi: " + ex.Message);
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
                    Ask.Fail(this, est.Message);
                    return;
                }

                // Không so "tiền đã đổ" với "tiêu hao" theo TỪNG chuyến: đổ đầy bình ở chuyến này thì xăng còn lại chạy
                // tiếp các chuyến sau -> so theo chuyến luôn lệch oan. Đổi chiếu tiền đổ được quyết toán THEO XE trong kỳ.
                var message = $"📏 Khoảng cách THẬT (từ GPS): {est.ActualDistanceKm:0.0} km\n" +
                              $"⛽ Chuyến này tiêu hao (theo định mức): {est.EstimatedLiters:0.0} lít\n" +
                              $"💰 Tiền xăng TÍNH VÀO ĐƠN: {est.EstimatedCost:N0} đ\n\n" +
                              $"🧾 Đã đổ ở chuyến này: {est.ActualFuelCost:N0} đ — đổ vào BÌNH XE, xăng còn lại chạy tiếp chuyến sau.\n" +
                              "Đối chiếu tiền đổ với tiêu hao xem theo từng xe: Tổng quan → Nhiên liệu → Xăng từng xe trong kỳ.";

                MessageBox.Show(FindForm(), message, "So sánh chi phí nhiên liệu", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Ask.Fail(this, "Lỗi: " + ex.Message);
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
            MessageBox.Show(FindForm(), message + "\n\n(Đã sao chép sẵn — chỉ cần dán vào app tài xế)", "Thông tin kết nối tài xế", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    Ask.Fail(this, result.Message);
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
                Ask.Fail(this, "Lỗi: " + ex.Message);
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
                Ask.Fail(this, "Chuyến này thiếu tọa độ (điểm khách hàng chưa chọn từ gợi ý VietMap, hoặc kho chưa có tọa độ) — không vẽ được lộ trình.");
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
                Ask.Fail(this, "Lỗi tải lộ trình: " + ex.Message);
            }
        }

        private int? GetSelectedTripId()
        {
            // Dòng ĐƠN không có mã chuyến -> null (các thao tác chuyến báo "chọn 1 chuyến").
            return grid.CurrentRow?.Cells["Id"].Value as int?;
        }

        private void ShowNoSelection()
            => Toast.Show(this, "👆 Chọn 1 chuyến trong danh sách trước.", ToastKind.Info);

        private async Task LoadAsync()
        {
            if (_api is null) return;
            lblStatus.Text = "Đang tải...";
            lblStatus.ForeColor = Theme.TextSecondary;
            _overlay.Loading = true;
            try
            {
                _trips = await _api.Trips.GetTripsAsync();
                try { _progress = (await _api.Trips.GetWarehouseProgressAsync()).ToDictionary(p => p.TripId); }
                catch { _progress = new(); } // API cũ chưa có -> chỉ thiếu cột Kho, không chặn cả màn hình
                filterBar.SetChoiceOptions("warehouse", _trips.Select(t => t.WarehouseName ?? ""));
                BindGrid();
                lblStatus.Text = SummaryText();
                lblStatus.ForeColor = _trips.Any(t => t.DeadlineLevel == "Late") ? Theme.Danger : Theme.TextSecondary;
                UpdateActionButtons();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
            finally { _overlay.Loading = false; }
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

            UpdatePrimaryButtons(trip);
            if (trip is null)
            {
                if (CurrentGroupCode() is string code) { ShowOrderCard(code); return; } // đang chọn dòng ĐƠN
                _cardAction = null;
                _selectedBar.ShowEmpty(_trips.Count == 0 ? "Chưa có chuyến nào (hoặc bộ lọc không khớp chuyến nào)." : null);
                return;
            }
            var blocked = st == "Planned" && (IsWaitingForPickup(trip) || IsWaitingForReceipt(trip) || IsWaitingForIssue(trip));
            var nextStep = st switch
            {
                "Planned" when IsWaitingForPickup(trip) => "Chờ chuyến lấy hàng của đơn hoàn tất mới bắt đầu được",
                "Planned" when IsWaitingForReceipt(trip) => "Hàng lấy về CHƯA nhập kho — nhập ở màn Kho → 📦 Nhập hàng từ chuyến rồi mới bắt đầu được",
                "Planned" when IsWaitingForIssue(trip) => "Kho CHƯA xác nhận xuất hàng lên xe — màn Kho → 📤 Xuất kho theo chuyến, xuất xong mới bắt đầu được",
                "Planned" => "Sẵn sàng — bấm ▶️ Bắt đầu chuyến khi xe xuất phát (chuột phải: đổi xe / tài xế, gửi link cho tài xế...)",
                "InProgress" => "Đang chạy — bấm ✅ Hoàn tất khi xe tới nơi (chuột phải: ghi xăng / chi phí / sự cố)",
                "PendingConfirmation" => "Tài xế đã báo xong — xem ảnh hàng / lộ trình rồi ✅ Duyệt hoặc ❌ Từ chối",
                "Completed" when trip.LegType == "Pickup" && WarehouseText(trip).Contains("chờ nhập", StringComparison.OrdinalIgnoreCase) => "Đã về kho — hàng CHỜ NHẬP KHO (màn Kho → 📦 Nhập hàng từ chuyến)",
                // Theo HasRun (server): chuyến không chạy xe (VD trả khách tại kho) thì không có xăng/chi phí để bổ sung.
                "Completed" => hasRun ? "Đã hoàn tất — chỉ xem lại / bổ sung chi phí" : "Đã đóng mà không chạy xe (trả khách tại kho) — chỉ xem lại",
                "Cancelled" => hasRun ? "Đã hủy giữa đường — xem lại / ghi chi phí đã phát sinh" : "Đã hủy trước khi chạy — chỉ xem lại",
                _ => "",
            };
            // Thẻ mới: dòng đầu + đường đi + 3 ô (Xe và tài xế · Thời gian · Hàng và kho) + việc tiếp theo kèm nút chính.
            _selectedBar.ShowCard(BuildTripCard(trip, nextStep,
                blocked || (st == "Completed" && nextStep.Contains("CHỜ NHẬP KHO"))));
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
