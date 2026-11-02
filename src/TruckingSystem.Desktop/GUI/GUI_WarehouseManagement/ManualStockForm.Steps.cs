using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_WarehouseManagement;

namespace TruckingSystem.Desktop.GUI_WarehouseManagement
{
    /// <summary>BỐ CỤC THEO VIỆC (mẫu UX ⑥ đã duyệt) — 1 form, 2 cửa sổ rõ việc, sắp xếp bằng code (không sửa Designer):
    ///  • Receive = "🧍 Nhận hàng khách mang tới": ① Đơn của khách → ② Loại hàng + số lượng → ③ Xếp vào kệ
    ///    (2 ô chọn: ⚡ Tự xếp / ✋ Tự chọn kệ — bảng kệ chỉ hiện khi tự chọn). Chân cửa sổ: [Đóng] [📥 Nhận 30 thùng vào kho].
    ///    Không còn bảng chuyến + lịch sử trong cửa sổ này (xem ở "Tồn kho & lịch sử").
    ///  • Issue = "📋 Tồn kho & lịch sử": 2 tab gạch chân —
    ///      "Đang tồn (theo đơn)": bảng hàng đang nằm trong kho theo đơn + kệ; đơn đã Hủy / Hoàn tất còn hàng
    ///      được nhắc ở dòng cảnh báo, chọn dòng đó mới hiện "📤 Xuất trả hàng sót…".
    ///      "Lịch sử nhập / xuất": bảng lịch sử + hoàn tác phiếu xuất tay (chữ đỏ).</summary>
    public partial class ManualStockForm
    {
        private bool _pickBin; // Nhận hàng: true = ✋ tự chọn kệ, false = ⚡ hệ thống tự xếp
        private readonly Panel _optAuto = new(), _optManual = new();
        private readonly Button _btnClose = new() { Text = "Đóng", AutoSize = true, DialogResult = DialogResult.Cancel };
        private readonly UnderlineTabs _tabs = new();
        private readonly DataGridView _stockGrid = new() { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 8) };
        private readonly Label _leftover = new()
        {
            AutoSize = true, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F), ForeColor = Color.FromArgb(146, 64, 14),
            BackColor = Color.FromArgb(254, 243, 199), Padding = new Padding(10, 6, 10, 6), Margin = new Padding(0, 0, 0, 8), Visible = false,
        };
        private List<StockRow> _stock = new();
        private string? _wantCargoType; // chọn 1 dòng ở bảng tồn -> sau khi tải dòng hàng của đơn thì chọn đúng loại hàng này

        private sealed record StockRow(int OrderId, string OrderCode, string Customer, string OrderStatus, string CargoType, decimal Qty, string Bins);

        private int S(int v) => LogicalToDeviceUnits(v);

        /// <summary>Hiện / ẩn 1 hàng của bảng bố cục: ẩn thì hàng cao 0 (hàng % vẫn giữ chỗ dù control ẩn).</summary>
        private void ShowRow(Control c, bool on, float percent = 0)
        {
            c.Visible = on;
            var row = root.GetRow(c);
            if (row < 0 || row >= root.RowStyles.Count) return;
            root.RowStyles[row] = !on ? new RowStyle(SizeType.Absolute, 0)
                : percent > 0 ? new RowStyle(SizeType.Percent, percent) : new RowStyle(SizeType.AutoSize);
        }

        private void SetupLayout()
        {
            root.SuspendLayout();
            BuildFooter();
            if (_mode == ManualStockMode.Receive) SetupReceive(); else SetupStock();
            root.ResumeLayout(true);
        }

        /// <summary>Chân cửa sổ: [trạng thái] ........ [Đóng] [nút chính].</summary>
        private void BuildFooter()
        {
            actionBar.SuspendLayout();
            actionBar.Controls.Clear();
            actionBar.ColumnStyles.Clear();
            actionBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            actionBar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            actionBar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            actionBar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            ButtonStyles.Secondary(_btnClose);
            _btnClose.MinimumSize = new Size(S(90), S(38));
            _btnClose.Margin = new Padding(S(8), 0, 0, 0);
            _btnClose.Click += (_, _) => Close();
            CancelButton = _btnClose;
            ButtonStyles.Primary(btnReceive);
            ButtonStyles.Primary(btnIssue);
            btnReceive.Margin = btnIssue.Margin = new Padding(S(8), 0, 0, 0);
            lblStatus.Anchor = AnchorStyles.Left;
            actionBar.Controls.Add(lblStatus, 0, 0);
            actionBar.Controls.Add(_btnClose, 1, 0);
            actionBar.Controls.Add(btnIssue, 2, 0);
            actionBar.Controls.Add(btnReceive, 3, 0);
            actionBar.Padding = new Padding(0, S(8), 0, 0);
            actionBar.ResumeLayout(true);
        }

        // ================= 🧍 Nhận hàng khách mang tới =================

        private void SetupReceive()
        {
            lblTitle.Text = "🧍 Nhận hàng khách mang tới";
            lblOrder.Text = "① Đơn của khách (chỉ đơn \"khách tự mang tới\" còn chờ nhận)";
            lblCargoLine.Text = "② Hàng nhận được — loại hàng";
            lblQty.Text = "Số lượng";
            foreach (var l in new[] { lblOrder, lblCargoLine }) l.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);

            // Bỏ bảng chuyến + lịch sử khỏi cửa sổ nhận hàng (xem ở "Tồn kho & lịch sử").
            ShowRow(lblFlowTitle, false);
            ShowRow(gridTrips, false);
            ShowRow(historyBar, false);
            ShowRow(historyFilter, false);
            ShowRow(gridHistory, false);
            lblFlowSummary.Font = new Font("Segoe UI", 9F);

            // ③ Xếp vào kệ: 2 ô chọn, bảng kệ chỉ hiện khi "Tự chọn kệ".
            var step3 = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true, Dock = DockStyle.Fill, Margin = new Padding(0, S(4), 0, S(4)) };
            step3.Controls.Add(new Label { Text = "③ Xếp vào kệ:", AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Margin = new Padding(0, S(18), S(10), 0) });
            BuildOption(_optAuto, "⚡ Tự xếp", "Hệ thống chọn kệ phù hợp (đúng khu, gần cửa nếu hàng gấp) — dùng cho hầu hết trường hợp", false);
            BuildOption(_optManual, "✋ Tự chọn kệ", "Chọn 1 kệ trong bảng bên dưới (màu: 🟩 còn nhiều chỗ · 🟨 quá nửa · 🟥 gần đầy)", true);
            step3.Controls.Add(_optAuto);
            step3.Controls.Add(_optManual);
            KpiStrip.InsertRow(root, step3, root.GetRow(lblBinsTitle));
            lblBinsTitle.Text = "Chọn 1 kệ (dòng đầu = để hệ thống tự chọn):";
            ShowRow(gridBins, true, 100);
            numQty.ValueChanged += (_, _) => UpdatePrimaryText();
            SetPickBin(false);
        }

        private void BuildOption(Panel opt, string title, string desc, bool manual)
        {
            opt.Size = new Size(S(330), S(60));
            opt.Margin = new Padding(0, S(4), S(10), S(4));
            opt.Cursor = Cursors.Hand;
            var t = new Label { Text = title, AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Location = new Point(S(10), S(6)), Cursor = Cursors.Hand };
            var d = new Label
            {
                Text = desc, AutoSize = false, Font = new Font("Segoe UI", 8.5F), ForeColor = Theme.TextSecondary,
                Location = new Point(S(10), S(27)), Size = new Size(S(312), S(30)), Cursor = Cursors.Hand,
            };
            opt.Controls.Add(t);
            opt.Controls.Add(d);
            foreach (Control c in new Control[] { opt, t, d }) c.Click += (_, _) => SetPickBin(manual);
            opt.Paint += (_, e) =>
            {
                var on = _pickBin == manual;
                using var pen = new Pen(on ? Theme.Primary : Theme.BorderColor, on ? 2 : 1);
                e.Graphics.DrawRectangle(pen, 1, 1, opt.Width - 3, opt.Height - 3);
            };
        }

        private void SetPickBin(bool manual)
        {
            _pickBin = manual;
            foreach (var (o, on) in new[] { (_optAuto, !manual), (_optManual, manual) })
            {
                o.BackColor = on ? Color.FromArgb(239, 246, 255) : Color.White;
                foreach (Control c in o.Controls) c.BackColor = o.BackColor;
                o.Invalidate();
            }
            lblBinsTitle.Visible = gridBins.Visible = manual;
            UpdatePrimaryText();
        }

        /// <summary>Nút chính ghi rõ việc: "📥 Nhận 30 Thùng carton vào kho" / "📤 Xuất trả 2 Kiện máy".</summary>
        private void UpdatePrimaryText()
        {
            var cargo = SelectedCargo?.CargoType ?? "hàng";
            if (_mode == ManualStockMode.Receive)
                btnReceive.Text = $"📥 Nhận {numQty.Value:0.##} {cargo} vào kho";
            else
                btnIssue.Text = $"📤 Xuất trả hàng sót ({numQty.Value:0.##} {cargo})…";
        }

        // ================= 📋 Tồn kho & lịch sử =================

        private void SetupStock()
        {
            lblTitle.Text = "📋 Tồn kho và lịch sử";
            Text = "Tồn kho và lịch sử nhập/xuất";
            _tabs.Add("stock", "📦 Đang tồn (theo đơn)", "Hàng đang nằm trong kho này — theo đơn và kệ");
            _tabs.Add("history", "🕘 Lịch sử nhập / xuất", "Mọi phiếu nhập / xuất của kho này");
            KpiStrip.InsertRow(root, _tabs, root.GetRow(inputs));
            KpiStrip.InsertRow(root, _leftover, root.GetRow(inputs));
            KpiStrip.InsertRow(root, _stockGrid, root.GetRow(inputs));
            lblOrder.Text = "Đơn đang chọn";
            lblCargoLine.Text = "Loại hàng";

            Theme.StyleGrid(_stockGrid);
            GridLayout.FitColumns(_stockGrid);
            _stockGrid.DataBindingComplete += (_, _) =>
            {
                if (_stockGrid.Columns["OrderId"] is { } c) c.Visible = false;
                if (_stockGrid.Columns["Ở_kệ"] is { } b) b.FillWeight = 160;
            };
            _stockGrid.CellFormatting += (_, e) =>
            {
                if (e.RowIndex < 0 || _stockGrid.Columns[e.ColumnIndex].Name != "Trạng_thái_đơn" || e.Value is not string s) return;
                if (s.Contains("hủy") || s.Contains("hoàn tất")) { e.CellStyle!.ForeColor = Theme.Danger; e.CellStyle.Font = Theme.FontBold; }
            };
            _stockGrid.SelectionChanged += (_, _) => SelectStockRow();
            _tabs.Changed += (_, _) => ApplyTab();
            _tabs.SelectTab("stock", raiseEvent: false);
            ApplyTab();
        }

        private void ApplyTab()
        {
            var stock = _tabs.Selected != "history";
            root.SuspendLayout();
            ShowRow(_stockGrid, stock, 55);
            ShowRow(_leftover, stock && _leftover.Text.Length > 0);
            ShowRow(inputs, stock);
            ShowRow(lblFlowTitle, false);
            ShowRow(lblFlowSummary, stock);
            ShowRow(gridTrips, false);
            ShowRow(lblBinsTitle, false);
            ShowRow(gridBins, false);
            ShowRow(historyBar, !stock);
            ShowRow(historyFilter, !stock);
            ShowRow(gridHistory, !stock, 100);
            root.ResumeLayout(true);
            // Hoàn tác = việc nguy hiểm -> chữ đỏ, chỉ ở tab lịch sử.
            ButtonStyles.DangerLink(btnReverse);
            if (stock) btnReverse.Visible = false; else UpdateReverseButton();
            if (!stock) btnIssue.Visible = false;
            else UpdateFlowSummary();
        }

        /// <summary>Hàng đang tồn theo đơn + kệ — gom từ nội dung các kệ của kho (cùng dữ liệu sơ đồ kho).</summary>
        private async Task LoadStockAsync()
        {
            if (_api is null || _mode != ManualStockMode.Issue) return;
            try
            {
                var zones = await _api.Warehouses.GetZonesAsync(_warehouseId);
                var bins = (await Task.WhenAll(zones.Select(z => _api.Warehouses.GetBinsAsync(z.Id)))).SelectMany(b => b).ToList();
                if (IsDisposed) return;
                var byCode = _orders.ToDictionary(o => o.Code, o => o);
                _stock = bins
                    .SelectMany(b => b.Contents.Select(c => (Bin: b.Code, c)))
                    .GroupBy(x => (x.c.OrderCode, x.c.CargoType))
                    .Select(g =>
                    {
                        var o = byCode.GetValueOrDefault(g.Key.OrderCode);
                        return new StockRow(o?.Id ?? 0, g.Key.OrderCode, g.First().c.CustomerName, StatusText(o?.Status), g.Key.CargoType,
                            g.Sum(x => x.c.QtyStored), string.Join(", ", g.Select(x => x.Bin).Distinct().OrderBy(x => x)));
                    })
                    .Where(r => r.Qty > 0)
                    .OrderBy(r => r.OrderStatus.Contains("hủy") || r.OrderStatus.Contains("hoàn tất") ? 0 : 1)
                    .ThenBy(r => r.OrderCode)
                    .ToList();
                _stockGrid.DataSource = _stock.Select(r => new
                {
                    r.OrderId,
                    Đơn = r.OrderCode,
                    Khách_hàng = r.Customer,
                    Trạng_thái_đơn = r.OrderStatus,
                    Loại_hàng = r.CargoType,
                    Đang_tồn = r.Qty,
                    Ở_kệ = r.Bins,
                }).ToList();
                var left = _stock.Where(r => r.OrderStatus.Contains("hủy") || r.OrderStatus.Contains("hoàn tất")).ToList();
                _leftover.Text = left.Count == 0 ? ""
                    : "⚠️ " + string.Join(" · ", left.Take(3).Select(r => $"{r.OrderCode} {r.OrderStatus} nhưng còn {r.Qty:0.##} {r.CargoType}"))
                      + (left.Count > 3 ? $" và {left.Count - 3} dòng khác" : "")
                      + " trong kho — chọn dòng đó rồi bấm \"📤 Xuất trả hàng sót…\".";
                if (_stock.Count == 0) ShowStatus("Kho này đang không tồn hàng của đơn nào.", Theme.TextSecondary);
                ApplyTab();
            }
            catch (Exception ex)
            {
                if (!IsDisposed) ShowStatus("Lỗi tải tồn kho: " + ex.Message, Theme.Danger);
            }
        }

        private static string StatusText(string? s) => s switch
        {
            "Done" => "🔒 Đã hoàn tất",
            "Cancelled" => "🔒 Đã hủy",
            "InProgress" => "Đang vận chuyển",
            "Confirmed" => "Đã xác nhận",
            null => "—",
            _ => s,
        };

        /// <summary>Chọn 1 dòng tồn -> chọn sẵn đúng đơn + loại hàng ở ô bên dưới (để xem tình trạng / xuất trả hàng sót).</summary>
        private void SelectStockRow()
        {
            // Đọc theo ô (không theo số thứ tự dòng — bấm tiêu đề cột sắp xếp lại vẫn đúng).
            if (_stockGrid.CurrentRow?.Cells["OrderId"]?.Value is not int orderId || orderId == 0) return;
            var cargoType = _stockGrid.CurrentRow.Cells["Loại_hàng"]?.Value as string;
            var r = _stock.FirstOrDefault(x => x.OrderId == orderId && x.CargoType == cargoType);
            if (r is null) return;
            _wantCargoType = r.CargoType;
            if (SelectedOrderId == r.OrderId)
            {
                if (cboCargoLine.DataSource is List<CargoLineOptionItem> lines && lines.FirstOrDefault(l => l.CargoType == r.CargoType) is { } line)
                    cboCargoLine.SelectedItem = line;
                _wantCargoType = null;
            }
            else cboOrder.SelectedValue = r.OrderId;
        }
    }
}
