using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TransportOrderManagement;
using TruckingSystem.Desktop.DTO_WarehouseManagement;

namespace TruckingSystem.Desktop.GUI_WarehouseManagement
{
    /// <summary>Màn Kho mở form này theo ĐÚNG 1 việc (đặt tên theo nghiệp vụ, không còn 1 nút
    /// "Nhập/Xuất" gộp 2 việc khác nhau):
    ///   Receive — 🙋 Nhận hàng KHÁCH TỰ MANG TỚI kho (đơn "Khách tự mang tới", đang xử lý).
    ///   Issue   — 📤 Xuất hàng TỒN của đơn đã Hoàn tất / Đã hủy + hoàn tác phiếu xuất tay + lịch sử.
    /// Hàng XE CÔNG TY chở về nhập ở màn riêng "📦 Nhập hàng từ chuyến" (PendingReceiptsForm).</summary>
    public enum ManualStockMode { Receive, Issue }

    /// <summary>
    /// Nhập/Xuất kho THỦ CÔNG — dùng cho trường hợp KHÔNG qua Trip: hàng KHÁCH TỰ MANG
    /// TỚI kho (nhập đủ rồi mới Chia xe được), hàng dư, xuất hàng còn sót của đơn đã đóng...
    /// Hàng của chuyến Lấy hàng nhập ở màn riêng "📦 Hàng chờ nhập" (PendingReceiptsForm).
    ///
    /// Màn hình cho thấy ĐẦY ĐỦ tình trạng hàng của đơn đang chọn:
    ///   - Khai báo / đã nhập / đã xuất / đang trong kho / đang GIỮ cho chuyến
    ///     giao-trả sắp chạy / còn được xuất tay bao nhiêu.
    ///   - Từng chuyến của đơn đã nhập hoặc xuất kho tới đâu.
    ///   - Lịch sử ghi rõ NGUỒN của từng phiếu (chuyến nào hay thủ công) và
    ///     người thực hiện thật (tài khoản đăng nhập), phiếu xuất tay làm nhầm
    ///     có thể HOÀN TÁC (trả hàng lại vào kho, phiếu vẫn giữ để lưu vết).
    /// </summary>
    public partial class ManualStockForm : BaseForm
    {
        private ApiServices? _api;
        private int _warehouseId;
        private List<TransportOrderListItem> _orders = new();
        private List<StorageBinItem> _availableBins = new();
        private List<WarehouseTransactionItem> _history = new();
        private OrderStockFlowItem? _flow;
        // Đổi đơn nhanh: chỉ hiện kết quả của lượt tải MỚI NHẤT (lượt cũ trả về muộn bị bỏ).
        private int _orderLoadId, _binsLoadId;
        private bool _busy;
        private ManualStockMode _mode = ManualStockMode.Receive;

        public ManualStockForm()
        {
            InitializeComponent();
            TruckingSystem.Desktop.Common.DialogKeys.Enable(this); // Esc = đóng
            // Chữ cho NHÂN VIÊN (không nêu tên thuật toán) — đặt bằng code, không sửa Designer.
            lblBinsTitle.Text = "Chọn vị trí nhập (tùy chọn — để trống = hệ thống tự chọn vị trí)";
            Anim.FadeIn(this); // cửa sổ hiện dần lên
            cboOrder.SelectedIndexChanged += async (_, _) => await OnOrderChangedAsync();
            cboCargoLine.SelectedIndexChanged += async (_, _) =>
            {
                UpdateFlowSummary();
                await LoadAvailableBinsAsync();
            };
            chkAllOrders.CheckedChanged += (_, _) => BindHistory();
            historyFilter.AddChoice("type", "Loại", new[] { "📥 Nhập", "📤 Xuất" });
            historyFilter.AddChoice("source", "Nguồn", Array.Empty<string>());
            historyFilter.AddChoice("reversed", "Hoàn tác", new[] { "Còn hiệu lực", "Đã hoàn tác" });
            historyFilter.FilterChanged += (_, _) => BindHistory();
            GridLayout.FitColumns(gridHistory);

            Theme.StyleGrid(gridHistory);
            Theme.StyleGrid(gridBins);
            Theme.StyleGrid(gridTrips);
            gridBins.CellFormatting += GridBins_CellFormatting;
            gridTrips.CellFormatting += GridTrips_CellFormatting;
            gridHistory.CellFormatting += GridHistory_CellFormatting;
            gridHistory.SelectionChanged += (_, _) => UpdateReverseButton();
            gridHistory.DataBindingComplete += (_, _) =>
            {
                if (gridHistory.Columns["Id"] != null) gridHistory.Columns["Id"]!.Visible = false;
                if (gridHistory.Columns["Ghi_chú"] != null) gridHistory.Columns["Ghi_chú"]!.FillWeight = 200;
            };
        }

        public ManualStockForm(ApiServices api, int warehouseId, ManualStockMode mode) : this()
        {
            _api = api;
            _warehouseId = warehouseId;
            _mode = mode;
            ApplyMode();
            Load += async (_, _) => { await LoadOrdersAsync(); await LoadHistoryAsync(); };
        }

        /// <summary>Chỉ hiện phần của ĐÚNG việc đang làm (ẩn nút / bảng của việc kia).</summary>
        private void ApplyMode()
        {
            var receive = _mode == ManualStockMode.Receive;
            Text = receive ? "Nhận hàng khách mang tới kho" : "Tồn kho theo đơn & lịch sử nhập/xuất";
            lblTitle.Text = receive
                ? "🙋 Nhận hàng khách tự mang tới kho"
                : "📋 Tồn kho theo đơn & lịch sử nhập/xuất";
            btnReceive.Visible = receive;
            lblBinsTitle.Visible = gridBins.Visible = receive;
            btnIssue.Visible = false; // màn Nhận: không xuất; màn Tồn kho: chỉ hiện khi có hàng xuất tay được (UpdateFlowSummary)
            if (!receive) lblQty.Visible = numQty.Visible = false;
            btnReverse.Visible = false; // chỉ hiện khi chọn 1 phiếu xuất tay hoàn tác được (UpdateReverseButton)
            if (!receive)
            {
                // Hàng bảng chọn vị trí (Percent) -> 0, nhường chỗ cho lịch sử.
                root.RowStyles[root.GetRow(gridBins)] = new RowStyle(SizeType.Absolute, 0F);
                chkAllOrders.Checked = true; // màn xuất/lịch sử: mặc định xem toàn bộ lịch sử kho
            }
        }

        /// <summary>Đơn hợp lệ cho việc đang làm — cùng điều kiện server cho phép nhập / xuất tay.</summary>
        private bool MatchesMode(TransportOrderListItem o) => _mode == ManualStockMode.Receive
            ? o.PickupMethod == "SelfDelivery" && (o.Status is "Confirmed" or "InProgress")
            // Đơn Hoàn tất / Đã hủy còn hàng -> xuất tay. Đơn ĐANG vận chuyển có hàng trong kho (VD
            // chặng Lấy hàng đã nhập kho, chặng Giao chưa chạy) cũng HIỆN để biết hàng đang nằm đâu,
            // chờ chuyến nào — nút Xuất ẩn, kèm lý do (hàng xuất theo chuyến ở màn 📤 Xuất kho theo chuyến).
            // Trước đây ẩn hẳn -> nhập kho xong mở màn này không thấy đơn đâu, tưởng mất hàng.
            : o.QtyOnHand > 0 && o.Status is ("Done" or "Cancelled" or "Confirmed" or "InProgress");

        /// <summary>Giờ trong DB là UTC (đôi khi thiếu nhãn) -> đổi đúng giờ Việt Nam.</summary>
        private static DateTime ToVn(DateTime t)
            => t.Kind == DateTimeKind.Local ? t : DateTime.SpecifyKind(t, DateTimeKind.Utc).ToLocalTime();

        private int? SelectedOrderId => cboOrder.SelectedValue as int?;
        private CargoLineOptionItem? SelectedCargo => cboCargoLine.SelectedItem as CargoLineOptionItem;

        // ================= Tải dữ liệu =================

        private async Task LoadOrdersAsync()
        {
            if (_api is null) return;
            try
            {
                // CHỈ đơn liên quan tới kho đang mở (trước đây hiện mọi đơn của mọi kho)
                _orders = (await _api.Warehouses.GetOrdersForWarehouseAsync(_warehouseId)).Where(MatchesMode)
                    // Màn xuất: đơn xuất tay được (Hoàn tất / Đã hủy) lên trước.
                    .OrderBy(o => _mode == ManualStockMode.Issue && o.Status is not ("Done" or "Cancelled") ? 1 : 0)
                    .ToList();
                if (_orders.Count == 0)
                {
                    lblFlowSummary.Text = _mode == ManualStockMode.Receive
                        ? "Không có đơn \"Khách tự mang tới kho\" nào đang chờ nhận hàng tại kho này. " +
                          "(Hàng do XE CÔNG TY chở về thì nhập ở nút 📦 Nhập hàng từ chuyến.)"
                        : "Kho này hiện không còn hàng của đơn nào — xem lịch sử nhập/xuất ở bảng dưới.";
                    btnReceive.Enabled = btnIssue.Enabled = false;
                }
                cboOrder.DataSource = _orders;
                cboOrder.DisplayMember = _mode == ManualStockMode.Issue
                    ? nameof(TransportOrderListItem.StockDisplayText)
                    : nameof(TransportOrderListItem.DisplayText);
                cboOrder.ValueMember = nameof(TransportOrderListItem.Id);
            }
            catch (Exception ex)
            {
                ShowStatus("Lỗi tải danh sách đơn: " + ex.Message, Theme.Danger);
            }
        }

        private async Task OnOrderChangedAsync()
        {
            var loadId = ++_orderLoadId;
            await LoadCargoLinesAsync(loadId: loadId);
            if (loadId != _orderLoadId || IsDisposed) return;
            await LoadFlowAsync(loadId);
            if (loadId != _orderLoadId || IsDisposed) return;
            BindHistory();
        }

        private async Task LoadCargoLinesAsync(int? keepCargoLineId = null, int? loadId = null)
        {
            if (_api is null || SelectedOrderId is not int orderId) return;
            try
            {
                var lines = await _api.Warehouses.GetCargoLinesForOrderAsync(orderId, _warehouseId);
                if (IsDisposed || (loadId is not null && loadId != _orderLoadId)) return; // đã đổi sang đơn khác
                cboCargoLine.DataSource = lines;
                cboCargoLine.DisplayMember = nameof(CargoLineOptionItem.CargoType);
                cboCargoLine.ValueMember = nameof(CargoLineOptionItem.Id);
                var keep = keepCargoLineId is null ? null : lines.FirstOrDefault(l => l.Id == keepCargoLineId);
                if (keep is not null) cboCargoLine.SelectedItem = keep;
            }
            catch (Exception ex)
            {
                ShowStatus("Lỗi tải dòng hàng: " + ex.Message, Theme.Danger);
            }
        }

        /// <summary>Tải toàn cảnh hàng của đơn: từng dòng hàng + từng chuyến.</summary>
        private async Task LoadFlowAsync(int? loadId = null)
        {
            _flow = null;
            gridTrips.DataSource = null;
            if (_api is null || SelectedOrderId is not int orderId) { UpdateFlowSummary(); return; }
            try
            {
                var flow = await _api.Warehouses.GetOrderStockFlowAsync(_warehouseId, orderId);
                if (IsDisposed || (loadId is not null && loadId != _orderLoadId) || SelectedOrderId != orderId) return; // đã đổi đơn
                _flow = flow;
                gridTrips.DataSource = _flow.Trips.Select(t => new
                {
                    Chuyến = $"#{t.TripId}",
                    Chặng = t.LegType,
                    Tài_xế = t.DriverName,
                    Trạng_thái = t.Status,
                    Kế_hoạch = t.QtyPlanned,
                    Đã_nhập_xuất = t.QtyDone,
                    Tình_trạng_kho = t.ProgressText,
                }).ToList();
                if (gridTrips.Columns["Tình_trạng_kho"] != null) gridTrips.Columns["Tình_trạng_kho"]!.FillWeight = 220;
                if (gridTrips.Columns["Đã_nhập_xuất"] != null) gridTrips.Columns["Đã_nhập_xuất"]!.HeaderText = "Đã nhập/xuất";
            }
            catch (Exception ex)
            {
                ShowStatus("Lỗi tải tình trạng hàng: " + ex.Message, Theme.Danger);
            }
            UpdateFlowSummary();
        }

        /// <summary>Dòng tóm tắt cho dòng hàng đang chọn + bật/tắt nút cho đúng:
        /// không còn gì để nhập -> mờ nút Nhập; không còn hàng TỰ DO -> mờ nút Xuất.</summary>
        private void UpdateFlowSummary()
        {
            var cargo = SelectedCargo;
            var f = cargo is null ? null : _flow?.CargoLines.FirstOrDefault(c => c.CargoLineId == cargo.Id);
            if (f is null)
            {
                lblFlowSummary.Text = SelectedOrderId is null ? "Chọn 1 đơn để xem." : "Đang tải...";
                btnReceive.Enabled = btnIssue.Enabled = false;
                return;
            }

            // Số nhập / xuất tay được do SERVER tính theo đúng quy tắc sẽ áp dụng khi bấm
            // (trước đây form tự tính "khai báo − đã nhập", bật nút cả khi server sẽ từ chối).
            var canReceive = f.QtyCanReceive;
            // Chỉ nêu lý do của ĐÚNG việc đang làm.
            var notes = new[] { _mode != ManualStockMode.Receive || f.ReceiveBlockReason is null ? null : "📥 " + f.ReceiveBlockReason,
                                _mode != ManualStockMode.Issue || f.IssueBlockReason is null || f.QtyOnHand <= 0 ? null : "📤 " + f.IssueBlockReason }
                .Where(n => n is not null);
            lblFlowSummary.Text =
                $"Khai báo {f.QtyTotal:0.##}   •   📥 Đã nhập {f.QtyReceived:0.##} (nhập tay được {canReceive:0.##})   •   📤 Đã xuất {f.QtyIssued:0.##}   •   " +
                $"🏬 Trong kho {f.QtyOnHand:0.##}   •   🔒 Giữ cho chuyến giao/trả {f.QtyReserved:0.##}   •   ✋ Có thể xuất tay {f.QtyFreeForManualIssue:0.##}\n" +
                $"{f.StatusText}   ({_flow!.PickupMethod})" +
                (notes.Any() ? "   |   " + string.Join("   |   ", notes) : ""); // nhãn chỉ cao 2 dòng
            lblFlowSummary.ForeColor = f.StatusText.Contains("⚠️") ? Theme.Danger : Theme.TextPrimary;

            btnReceive.Enabled = !_busy && canReceive > 0;
            btnIssue.Enabled = !_busy && f.QtyFreeForManualIssue > 0;
            // Màn xem tồn kho: hàng rời kho THEO CHUYẾN (📤 Xuất kho theo chuyến cho chặng Giao, chuyến Trả
            // hàng, trả khách khi hủy đơn) nên gần như không bao giờ phải xuất tay. Ô số lượng + nút Xuất CHỈ HIỆN khi
            // dòng hàng đang chọn thật sự có hàng xuất tay được (hàng còn sót của đơn Hoàn tất / Đã hủy)
            // — trước đây nút luôn hiện nhưng mờ, người dùng không hiểu để làm gì.
            if (_mode == ManualStockMode.Issue)
                lblQty.Visible = numQty.Visible = btnIssue.Visible = f.QtyFreeForManualIssue > 0;
            // Hàng đếm theo đơn vị NGUYÊN (số lượng khai là số nguyên) -> ô nhập không cho số lẻ
            // (server cũng từ chối 2,5 máy); hàng số lượng lẻ cho 2 chữ số thập phân.
            var wholeUnits = f.QtyTotal == Math.Floor(f.QtyTotal);
            numQty.DecimalPlaces = wholeUnits ? 0 : 2;
            numQty.Increment = wholeUnits ? 1 : 0.01m;
            numQty.Value = Math.Min(numQty.Maximum, _mode == ManualStockMode.Receive ? canReceive : f.QtyFreeForManualIssue);
        }

        /// <summary>Danh sách vị trí còn trống đúng khu vực — kèm dòng "Tự động
        /// (Best-Fit)" ở đầu để người dùng vẫn có thể để hệ thống tự chọn.</summary>
        private async Task LoadAvailableBinsAsync()
        {
            if (_api is null || _mode != ManualStockMode.Receive || SelectedCargo is not CargoLineOptionItem cargo) { gridBins.DataSource = null; return; }
            var loadId = ++_binsLoadId;
            try
            {
                var bins = await _api.Warehouses.GetAvailableBinsForCargoAsync(_warehouseId, cargo.Id);
                if (IsDisposed || loadId != _binsLoadId) return; // đã chọn dòng hàng khác
                _availableBins = bins;

                var rows = new List<object>
                {
                    new { Id = 0, Vị_trí = "🤖 Hệ thống tự chọn vị trí", Đã_dùng = "", Còn_trống_m3 = "", Cách_cửa_m = "" }
                };
                rows.AddRange(_availableBins.Select(b => (object)new
                {
                    Id = b.Id,
                    Vị_trí = b.Code,
                    Đã_dùng = $"{(b.CapacityM3 == 0 ? 0 : b.CurrentUsedM3 / b.CapacityM3 * 100):0}%",
                    Còn_trống_m3 = (b.CapacityM3 - b.CurrentUsedM3).ToString("0.00"),
                    Cách_cửa_m = b.DistanceFromDoorMeters.ToString("0.0"),
                }));
                gridBins.DataSource = rows;
                if (gridBins.Columns["Id"] != null) gridBins.Columns["Id"]!.Visible = false;
                if (gridBins.Rows.Count > 0) gridBins.Rows[0].Selected = true; // mặc định chọn "Tự động"
            }
            catch (Exception ex)
            {
                ShowStatus("Lỗi tải danh sách vị trí: " + ex.Message, Theme.Danger);
            }
        }

        private async Task LoadHistoryAsync()
        {
            if (_api is null) return;
            try
            {
                _history = await _api.Warehouses.GetTransactionHistoryAsync(_warehouseId);
                BindHistory();
            }
            catch (Exception ex)
            {
                ShowStatus("Lỗi tải lịch sử: " + ex.Message, Theme.Danger);
            }
        }

        /// <summary>Lịch sử luôn là của KHO đang mở; mặc định lọc thêm theo ĐƠN đang chọn.</summary>
        private void BindHistory()
        {
            var rows = _history.AsEnumerable();
            if (!chkAllOrders.Checked && SelectedOrderId is int orderId)
                rows = rows.Where(h => h.OrderId == orderId);
            var scoped = rows.ToList();
            historyFilter.SetChoiceOptions("source", scoped.Select(h => h.Source));
            rows = scoped.Where(h =>
                historyFilter.Is("type", h.Type == "Nhập" ? "📥 Nhập" : "📤 Xuất") &&
                historyFilter.Is("source", h.Source) &&
                historyFilter.Is("reversed", h.IsReversed ? "Đã hoàn tác" : "Còn hiệu lực") &&
                historyFilter.MatchesSearch(h.OrderCode, h.CargoType, h.ByEmployeeName, h.Note, ToVn(h.OccurredAt)));
            var shownRows = rows.ToList();
            historyFilter.SetCount(shownRows.Count, scoped.Count);
            rows = shownRows;

            gridHistory.DataSource = rows.Select(h => new
            {
                h.Id,
                Loại = h.Type == "Nhập" ? "📥 Nhập" : "📤 Xuất",
                Thời_gian = ToVn(h.OccurredAt).ToString("dd/MM/yyyy HH:mm"),
                Đơn_hàng = h.OrderCode,
                Loại_hàng = h.CargoType,
                Số_lượng = h.Qty,
                Nguồn = h.Source,
                Người_thực_hiện = h.ByEmployeeName,
                Ghi_chú = h.Note ?? "",
            }).ToList();
            lblHistoryTitle.Text = chkAllOrders.Checked || SelectedOrderId is null
                ? "Lịch sử nhập/xuất của kho này — tất cả đơn"
                : $"Lịch sử nhập/xuất kho — đơn {_orders.FirstOrDefault(o => o.Id == SelectedOrderId)?.Code}";
            UpdateReverseButton();
        }

        private WarehouseTransactionItem? SelectedHistory()
        {
            if (gridHistory.CurrentRow?.Cells["Id"]?.Value is not int id) return null;
            var type = gridHistory.CurrentRow.Cells["Loại"].Value?.ToString() ?? "";
            return _history.FirstOrDefault(h => h.Id == id && (type.Contains("Nhập") ? h.Type == "Nhập" : h.Type == "Xuất"));
        }

        /// <summary>Chỉ cho hoàn tác phiếu XUẤT THỦ CÔNG chưa hoàn tác (không gồm phiếu tự động trả khách khi hủy đơn)
        /// (server tính sẵn CanReverse — cùng điều kiện lúc hoàn tác thật).</summary>
        private void UpdateReverseButton()
        {
            var h = SelectedHistory();
            var canReverse = h is not null && h.Type == "Xuất" && h.CanReverse;
            btnReverse.Enabled = !_busy && canReverse;
            // Chỉ hiện khi đang chọn đúng 1 phiếu xuất tay hoàn tác được (hiếm) — không để nút mờ thường trực.
            btnReverse.Visible = _mode == ManualStockMode.Issue && canReverse;
        }

        // ================= Tô màu =================

        private void GridBins_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (gridBins.Rows[e.RowIndex].DataBoundItem is null) return;
            var binId = (int)gridBins.Rows[e.RowIndex].Cells["Id"].Value;
            if (binId == 0) return;

            var bin = _availableBins.FirstOrDefault(b => b.Id == binId);
            if (bin is null) return;

            var pct = bin.CapacityM3 == 0 ? 0 : (double)(bin.CurrentUsedM3 / bin.CapacityM3);
            gridBins.Rows[e.RowIndex].DefaultCellStyle.BackColor = pct switch
            {
                >= 0.85 => Color.FromArgb(254, 226, 226),
                >= 0.5 => Color.FromArgb(254, 243, 199),
                _ => Color.FromArgb(220, 252, 231),
            };
        }

        private void GridTrips_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (gridTrips.Columns[e.ColumnIndex].Name != "Tình_trạng_kho" || e.Value is not string text) return;
            e.CellStyle!.ForeColor = text.StartsWith("✅") ? Theme.Success
                : text.StartsWith("⏳") ? Theme.Warning
                : text.StartsWith("—") ? Theme.TextSecondary
                : Theme.Primary;
            e.CellStyle.Font = Theme.FontBold;
        }

        private void GridHistory_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (gridHistory.Rows[e.RowIndex].Cells["Id"].Value is not int id) return;
            var type = gridHistory.Rows[e.RowIndex].Cells["Loại"].Value?.ToString() ?? "";
            var h = _history.FirstOrDefault(x => x.Id == id && (type.Contains("Nhập") ? x.Type == "Nhập" : x.Type == "Xuất"));
            if (h is null) return;

            if (h.IsReversed)
            {
                e.CellStyle!.ForeColor = Theme.TextSecondary;
                if (gridHistory.Columns[e.ColumnIndex].Name != "Ghi_chú")
                    e.CellStyle.Font = _strikeFont ??= new Font(gridHistory.Font, FontStyle.Strikeout);
            }
            else if (h.IsManual)
            {
                e.CellStyle!.BackColor = Color.FromArgb(255, 251, 235); // vàng rất nhạt = thao tác tay
            }
        }

        private Font? _strikeFont;

        // ================= Thao tác =================

        private void ShowStatus(string text, Color color)
        {
            lblStatus.Text = text;
            lblStatus.ForeColor = color;
        }

        private async void BtnReceive_Click(object? sender, EventArgs e) => await SubmitAsync(isReceive: true);
        private async void BtnIssue_Click(object? sender, EventArgs e) => await SubmitAsync(isReceive: false);

        private async Task SubmitAsync(bool isReceive)
        {
            if (_api is null || _busy) return;
            if (SelectedOrderId is not int orderId || SelectedCargo is not CargoLineOptionItem cargo)
            {
                ShowStatus("Vui lòng chọn đủ đơn hàng và dòng hàng.", Theme.Warning);
                return;
            }
            // Hàng nguyên chiếc: ô hiện số đã làm tròn nhưng giá trị gõ vào có thể còn lẻ (2,5) -> làm tròn.
            if (numQty.DecimalPlaces == 0) numQty.Value = Math.Round(numQty.Value);
            if (numQty.Value <= 0)
            {
                ShowStatus("Số lượng phải lớn hơn 0.", Theme.Warning);
                return;
            }

            var f = _flow?.CargoLines.FirstOrDefault(c => c.CargoLineId == cargo.Id);
            var actionText = isReceive ? "NHẬP" : "XUẤT";
            var referenceQty = f is null ? 0 : isReceive ? f.QtyCanReceive : f.QtyFreeForManualIssue;

            var confirmText = $"Xác nhận {actionText} THỦ CÔNG {numQty.Value:0.##} \"{cargo.CargoType}\" của đơn {(f is null ? "" : _flow!.OrderCode)}?\n\n" +
                (isReceive
                    ? $"Còn được nhập tối đa {referenceQty:0.##}."
                    : $"Có thể xuất tay tối đa {referenceQty:0.##} (đã trừ {(f?.QtyReserved ?? 0):0.##} đang giữ cho chuyến giao/trả).\n" +
                      "Lưu ý: hàng của chuyến giao xuất ở màn 📤 Xuất kho theo chuyến — KHÔNG xuất tay ở đây.");
            if (numQty.Value < referenceQty)
                confirmText += $"\n\n⚠️ Số lượng nhỏ hơn mức tối đa — sau thao tác vẫn còn {referenceQty - numQty.Value:0.##}.";

            if (MessageBox.Show(FindForm(), confirmText, $"Xác nhận {actionText.ToLower()} kho", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            int? chosenBinId = null;
            if (isReceive && gridBins.CurrentRow?.Cells["Id"].Value is int selectedId && selectedId != 0)
                chosenBinId = selectedId;

            // Khóa nút trong lúc chờ server — bấm 2 lần trước đây có thể nhập/xuất 2 lần.
            _busy = true;
            btnReceive.Enabled = btnIssue.Enabled = btnReverse.Enabled = false;
            try
            {
                if (isReceive)
                {
                    var result = await _api.Warehouses.ReceiveManualAsync(_warehouseId, orderId, cargo.Id, numQty.Value, chosenBinId);
                    var binCodesText = result.BinCodesUsed.Count > 0 ? string.Join(", ", result.BinCodesUsed) : "(chưa xếp được)";
                    ShowStatus(result.Success
                        ? $"✅ Đã nhập kho — vị trí {binCodesText}" + (result.Message is null ? "" : "\n" + result.Message)
                        : "⚠️ " + result.Message, result.Success ? Theme.Success : Theme.Warning);
                }
                else
                {
                    var result = await _api.Warehouses.IssueManualAsync(_warehouseId, orderId, cargo.Id, numQty.Value);
                    ShowStatus(result.Success ? "✅ Đã xuất kho." : "⚠️ " + result.Message, result.Success ? Theme.Success : Theme.Warning);
                }
                await RefreshAllAsync(cargo.Id);
            }
            catch (Exception ex)
            {
                if (!IsDisposed) ShowStatus("Lỗi: " + ex.Message, Theme.Danger);
            }
            finally
            {
                _busy = false;
                if (!IsDisposed) { UpdateFlowSummary(); UpdateReverseButton(); }
            }
        }

        private async void BtnReverse_Click(object? sender, EventArgs e)
        {
            if (_api is null || _busy) return;
            var h = SelectedHistory();
            if (h is null || h.Type != "Xuất" || !h.CanReverse)
            {
                ShowStatus("Chọn 1 dòng XUẤT THỦ CÔNG chưa hoàn tác trong lịch sử để hoàn tác (phiếu tự động trả khách khi hủy đơn thì không hoàn tác được).", Theme.Warning);
                return;
            }

            var confirm = MessageBox.Show(FindForm(),
                $"Hoàn tác phiếu xuất tay {h.Qty:0.##} \"{h.CargoType}\" của đơn {h.OrderCode} ({ToVn(h.OccurredAt):dd/MM HH:mm})?\n\n" +
                "Hàng sẽ được cộng lại vào tồn kho và xếp lại vào ô kệ (hệ thống tự chọn vị trí). Phiếu xuất vẫn được giữ trong lịch sử, đánh dấu \"đã hoàn tác\".",
                "Hoàn tác phiếu xuất tay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            _busy = true;
            btnReceive.Enabled = btnIssue.Enabled = btnReverse.Enabled = false;
            try
            {
                var result = await _api.Warehouses.ReverseManualIssueAsync(h.Id);
                ShowStatus((result.Success ? "✅ " : "⚠️ ") + result.Message, result.Success ? Theme.Success : Theme.Warning);
                await RefreshAllAsync(SelectedCargo?.Id);
            }
            catch (Exception ex)
            {
                if (!IsDisposed) ShowStatus("Lỗi: " + ex.Message, Theme.Danger);
            }
            finally
            {
                _busy = false;
                if (!IsDisposed) { UpdateFlowSummary(); UpdateReverseButton(); }
            }
        }

        /// <summary>Sau mỗi thao tác: tải lại MỌI số liệu (dòng hàng, tình trạng,
        /// vị trí, lịch sử) — giữ nguyên dòng hàng đang chọn.</summary>
        private async Task RefreshAllAsync(int? keepCargoLineId)
        {
            await LoadCargoLinesAsync(keepCargoLineId);
            await LoadFlowAsync();
            await LoadAvailableBinsAsync();
            await LoadHistoryAsync();
        }
    }
}
