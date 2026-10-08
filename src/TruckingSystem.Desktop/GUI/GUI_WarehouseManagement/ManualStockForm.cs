using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TransportOrderManagement;
using TruckingSystem.Desktop.DTO_WarehouseManagement;

namespace TruckingSystem.Desktop.GUI_WarehouseManagement
{
    /// <summary>
    /// Nhập/Xuất kho THỦ CÔNG — dùng cho trường hợp KHÔNG qua Trip (hàng dư, sửa
    /// sai lệch tồn kho...). Luồng tự động (gắn Trip) vẫn chạy như cũ.
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

        public ManualStockForm()
        {
            InitializeComponent();
            cboOrder.SelectedIndexChanged += async (_, _) => await OnOrderChangedAsync();
            cboCargoLine.SelectedIndexChanged += async (_, _) =>
            {
                UpdateFlowSummary();
                await LoadAvailableBinsAsync();
            };
            chkAllOrders.CheckedChanged += (_, _) => BindHistory();

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

        public ManualStockForm(ApiServices api, int warehouseId) : this()
        {
            _api = api;
            _warehouseId = warehouseId;
            Load += async (_, _) => { await LoadOrdersAsync(); await LoadHistoryAsync(); };
        }

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
                _orders = await _api.Warehouses.GetOrdersForWarehouseAsync(_warehouseId);
                if (_orders.Count == 0)
                {
                    lblFlowSummary.Text = "Kho này chưa có đơn nào (chưa có đơn chọn kho này, chưa có chuyến đi qua kho, chưa nhập/xuất hàng tại kho).";
                    btnReceive.Enabled = btnIssue.Enabled = false;
                }
                cboOrder.DataSource = _orders;
                cboOrder.DisplayMember = nameof(TransportOrderListItem.DisplayText);
                cboOrder.ValueMember = nameof(TransportOrderListItem.Id);
            }
            catch (Exception ex)
            {
                ShowStatus("Lỗi tải danh sách đơn: " + ex.Message, Theme.Danger);
            }
        }

        private async Task OnOrderChangedAsync()
        {
            await LoadCargoLinesAsync();
            await LoadFlowAsync();
            BindHistory();
        }

        private async Task LoadCargoLinesAsync(int? keepCargoLineId = null)
        {
            if (_api is null || SelectedOrderId is not int orderId) return;
            try
            {
                var lines = await _api.Warehouses.GetCargoLinesForOrderAsync(orderId, _warehouseId);
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
        private async Task LoadFlowAsync()
        {
            _flow = null;
            gridTrips.DataSource = null;
            if (_api is null || SelectedOrderId is not int orderId) { UpdateFlowSummary(); return; }
            try
            {
                _flow = await _api.Warehouses.GetOrderStockFlowAsync(_warehouseId, orderId);
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

            var remainingToReceive = Math.Max(0, f.QtyTotal - f.QtyReceived);
            lblFlowSummary.Text =
                $"Khai báo {f.QtyTotal:0.##}   •   📥 Đã nhập {f.QtyReceived:0.##}   •   📤 Đã xuất {f.QtyIssued:0.##}   •   " +
                $"🏬 Trong kho {f.QtyOnHand:0.##}   •   🔒 Giữ cho chuyến giao/trả {f.QtyReserved:0.##}   •   ✋ Có thể xuất tay {f.QtyFreeForManualIssue:0.##}\n" +
                $"{f.StatusText}   ({_flow!.PickupMethod})";
            lblFlowSummary.ForeColor = f.StatusText.Contains("⚠️") ? Theme.Danger : Theme.TextPrimary;

            btnReceive.Enabled = remainingToReceive > 0;
            btnIssue.Enabled = f.QtyFreeForManualIssue > 0;
            numQty.Value = Math.Min(numQty.Maximum, remainingToReceive > 0 ? remainingToReceive : f.QtyFreeForManualIssue);
        }

        /// <summary>Danh sách vị trí còn trống đúng khu vực — kèm dòng "Tự động
        /// (Best-Fit)" ở đầu để người dùng vẫn có thể để hệ thống tự chọn.</summary>
        private async Task LoadAvailableBinsAsync()
        {
            if (_api is null || SelectedCargo is not CargoLineOptionItem cargo) { gridBins.DataSource = null; return; }
            try
            {
                _availableBins = await _api.Warehouses.GetAvailableBinsForCargoAsync(_warehouseId, cargo.Id);

                var rows = new List<object>
                {
                    new { Id = 0, Vị_trí = "🤖 Tự động (Best-Fit)", Đã_dùng = "", Còn_trống_m3 = "", Cách_cửa_m = "" }
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

        /// <summary>Chỉ cho hoàn tác phiếu XUẤT THỦ CÔNG chưa hoàn tác.</summary>
        private void UpdateReverseButton()
        {
            var h = SelectedHistory();
            btnReverse.Enabled = h is not null && h.Type == "Xuất" && h.IsManual && !h.IsReversed;
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
            if (_api is null) return;
            if (SelectedOrderId is not int orderId || SelectedCargo is not CargoLineOptionItem cargo)
            {
                ShowStatus("Vui lòng chọn đủ đơn hàng và dòng hàng.", Theme.Warning);
                return;
            }
            if (numQty.Value <= 0)
            {
                ShowStatus("Số lượng phải lớn hơn 0.", Theme.Warning);
                return;
            }

            var f = _flow?.CargoLines.FirstOrDefault(c => c.CargoLineId == cargo.Id);
            var actionText = isReceive ? "NHẬP" : "XUẤT";
            var referenceQty = f is null ? 0 : isReceive ? Math.Max(0, f.QtyTotal - f.QtyReceived) : f.QtyFreeForManualIssue;

            var confirmText = $"Xác nhận {actionText} THỦ CÔNG {numQty.Value:0.##} \"{cargo.CargoType}\" của đơn {(f is null ? "" : _flow!.OrderCode)}?\n\n" +
                (isReceive
                    ? $"Còn được nhập tối đa {referenceQty:0.##}."
                    : $"Có thể xuất tay tối đa {referenceQty:0.##} (đã trừ {(f?.QtyReserved ?? 0):0.##} đang giữ cho chuyến giao/trả).\n" +
                      "Lưu ý: hàng của chuyến giao sẽ TỰ xuất kho khi chuyến bắt đầu — KHÔNG cần xuất tay.");
            if (numQty.Value < referenceQty)
                confirmText += $"\n\n⚠️ Số lượng nhỏ hơn mức tối đa — sau thao tác vẫn còn {referenceQty - numQty.Value:0.##}.";

            if (MessageBox.Show(FindForm(), confirmText, $"Xác nhận {actionText.ToLower()} kho", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            int? chosenBinId = null;
            if (isReceive && gridBins.CurrentRow?.Cells["Id"].Value is int selectedId && selectedId != 0)
                chosenBinId = selectedId;

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
                ShowStatus("Lỗi: " + ex.Message, Theme.Danger);
            }
        }

        private async void BtnReverse_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;
            var h = SelectedHistory();
            if (h is null || h.Type != "Xuất" || !h.IsManual || h.IsReversed)
            {
                ShowStatus("Chọn 1 dòng XUẤT THỦ CÔNG (chưa hoàn tác) trong lịch sử để hoàn tác.", Theme.Warning);
                return;
            }

            var confirm = MessageBox.Show(FindForm(),
                $"Hoàn tác phiếu xuất tay {h.Qty:0.##} \"{h.CargoType}\" của đơn {h.OrderCode} ({ToVn(h.OccurredAt):dd/MM HH:mm})?\n\n" +
                "Hàng sẽ được cộng lại vào tồn kho và xếp lại vào ô kệ (Best-Fit). Phiếu xuất vẫn được giữ trong lịch sử, đánh dấu \"đã hoàn tác\".",
                "Hoàn tác phiếu xuất tay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                var result = await _api.Warehouses.ReverseManualIssueAsync(h.Id);
                ShowStatus((result.Success ? "✅ " : "⚠️ ") + result.Message, result.Success ? Theme.Success : Theme.Warning);
                await RefreshAllAsync(SelectedCargo?.Id);
            }
            catch (Exception ex)
            {
                ShowStatus("Lỗi: " + ex.Message, Theme.Danger);
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
