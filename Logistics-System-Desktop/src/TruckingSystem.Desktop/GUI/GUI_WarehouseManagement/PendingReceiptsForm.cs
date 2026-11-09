using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_WarehouseManagement;
using TruckingSystem.Desktop.SERVICE_WarehouseManagement;

namespace TruckingSystem.Desktop.GUI_WarehouseManagement
{
    /// <summary>HÀNG CHỜ NHẬP KHO của 1 kho: chuyến Lấy hàng đã về kho (đã duyệt) nhưng hàng
    /// chưa lên kệ. Vận hành chọn 1 dòng rồi:
    ///   - 🤖 Nhập TỰ ĐỘNG cả chuyến theo Best-Fit (mọi dòng còn chờ của chuyến, lỗi 1 dòng thì
    ///     không nhập dòng nào), hoặc
    ///   - ✋ Nhập THỦ CÔNG: tự chọn vị trí + số lượng (nhập nhiều lần để chia nhiều vị trí).
    /// Phiếu nhập gắn chuyến Lấy hàng — server trừ đúng phần còn chờ, không nhập trùng.
    /// Giao diện: PendingReceiptsForm.Designer.cs.</summary>
    public partial class PendingReceiptsForm : BaseForm
    {
        private readonly ApiServices? _api;
        private readonly PendingReceiptApiService? _service;
        private readonly int _warehouseId;

        private List<PendingReceiptLineItem> _pending = new();
        private List<StorageBinItem> _bins = new();
        private int _binsLoadId; // chỉ hiện kết quả của lần tải vị trí MỚI NHẤT (đổi dòng nhanh)
        private bool _busy;

        /// <summary>Đã nhập ít nhất 1 lần — màn Kho dùng để vẽ lại sơ đồ.</summary>
        public bool ReceivedAny { get; private set; }

        /// <summary>Dùng cho trình thiết kế (Designer) của Visual Studio.</summary>
        public PendingReceiptsForm()
        {
            InitializeComponent();
            TruckingSystem.Desktop.Common.DialogFit.Enable(this, root); // vừa màn hình mọi máy, bảng không bị ép mất chữ
            TruckingSystem.Desktop.Common.DialogKeys.Enable(this); // Esc = đóng
            // Chữ cho NHÂN VIÊN (không nêu tên thuật toán) — đặt bằng code, không sửa Designer.
            lblInfo.Text = "Xe Lấy hàng đã về kho (đã duyệt) nhưng hàng CHƯA lên kệ. Chọn 1 dòng rồi nhập: TỰ ĐỘNG cả chuyến (hệ thống tự chọn kệ), hoặc THỦ CÔNG — tự chọn kệ + số lượng (nhập nhiều lần để chia nhiều kệ). Chặng Giao của đơn chỉ bắt đầu được khi hàng đã nhập kho đủ.";
            lblManualHint.Text = "Chọn kệ bên dưới (chỉ hiện kệ ĐÚNG khu theo tính chất hàng). Dòng đầu = để hệ thống tự chọn kệ cho riêng số lượng này. Màu dòng: 🟩 còn nhiều chỗ · 🟨 đã dùng quá nửa · 🟥 gần đầy (≥ 85%).";
            Anim.FadeIn(this); // cửa sổ hiện dần lên
            Theme.StyleGrid(gridPending);
            Theme.StyleGrid(gridBins);
            gridPending.DataBindingComplete += (_, _) =>
            {
                if (gridPending.Columns["Key"] != null) gridPending.Columns["Key"]!.Visible = false;
                if (gridPending.Columns["Loại_hàng"] != null) gridPending.Columns["Loại_hàng"]!.FillWeight = 160;
                if (gridPending.Columns["Chờ_nhập"] != null) gridPending.Columns["Chờ_nhập"]!.DefaultCellStyle.Font = Theme.FontBold;
            };
            gridBins.DataBindingComplete += (_, _) =>
            {
                if (gridBins.Columns["Id"] != null) gridBins.Columns["Id"]!.Visible = false;
            };
            gridBins.CellFormatting += GridBins_CellFormatting;
            gridPending.SelectionChanged += async (_, _) => await OnSelectionChangedAsync();
            GridLayout.FitColumns(gridPending);
            pendingFilter.AddChoice("zone", "Khu", new[] { ZoneText("Normal"), ZoneText("Fragile"), ZoneText("Hazmat") });
            pendingFilter.AddChoice("order", "Đơn", Array.Empty<string>());
            pendingFilter.FilterChanged += async (_, _) =>
            {
                BindPending((SelectedLine() is { } cur) ? Key(cur) : null);
                await OnSelectionChangedAsync();
            };
            SetupSteps();
            // Đỏ = chặng Giao phải xuất phát hôm nay / đã quá mốc -> nhập trước; vàng = mai.
            GridDecor.Pills(gridPending, "Giao_phải_xuất_trước", (_, row) =>
                row >= 0 && row < gridPending.Rows.Count && gridPending.Rows[row].Cells["Key"].Value is string k
                && _pending.FirstOrDefault(p => Key(p) == k) is { } line
                    ? ScreenKit.StartBy(line.DeliveryLatestStart).Level switch { 2 => GridDecor.Red, 1 => GridDecor.Amber, _ => null }
                    : null);
        }

        // ================= Bố cục theo BƯỚC (mẫu UX ③ đã duyệt) =================
        // ① Chọn dòng hàng cần nhập (bảng trên) → ② Chọn cách nhập: ⚡ Tự xếp cả chuyến / ✋ Tự chọn kệ (2 ô chọn có mô tả;
        // "Tự chọn kệ" mới hiện ô số lượng + bảng kệ) → ③ nút chính ở góc phải chân cửa sổ, chữ ghi rõ việc
        // ("📥 Nhập cả chuyến #12"). Dời control có sẵn bằng code, không sửa Designer.
        private bool _manualMode;
        private readonly Panel _optAuto = new(), _optManual = new();
        private readonly FlowLayoutPanel _primaryBox = new() { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, Margin = Padding.Empty, Anchor = AnchorStyles.Right };

        private void SetupSteps()
        {
            int S(int v) => LogicalToDeviceUnits(v);
            lblInfo.Text = "① Chọn dòng hàng cần nhập — hàng của các chuyến lấy hàng đã về kho (đã duyệt) nhưng chưa lên kệ. " +
                           "Chặng Giao của đơn chỉ bắt đầu được khi hàng đã nhập kho đủ.";
            lblInfo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblInfo.ForeColor = Theme.TextPrimary;

            // ② 2 ô chọn cách nhập, thay chỗ nút "Nhập TỰ ĐỘNG" ở hàng giữa.
            pendingBar.Controls.Remove(btnAutoTrip);
            var step2 = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true, Margin = Padding.Empty };
            step2.Controls.Add(new Label { Text = "② Chọn cách nhập:", AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Margin = new Padding(0, S(14), S(10), 0) });
            BuildOption(_optAuto, "⚡ Tự xếp cả chuyến", "Hệ thống tự chọn kệ phù hợp — nhanh, dùng cho hầu hết trường hợp", false);
            BuildOption(_optManual, "✋ Tự chọn kệ", "Chọn từng kệ + số lượng — khi muốn xếp theo ý mình (nhập nhiều lần để chia nhiều kệ)", true);
            step2.Controls.Add(_optAuto);
            step2.Controls.Add(_optManual);
            pendingBar.Controls.Add(step2, 0, 0);
            ButtonStyles.Secondary(btnRefresh);
            btnRefresh.Anchor = AnchorStyles.Right | AnchorStyles.Top;

            // ③ chân cửa sổ: [trạng thái] ........ [Đóng] [nút chính]
            bottomBar.Controls.Remove(btnManual);
            bottomBar.Controls.Remove(lblStatus);
            bottomBar.Controls.Remove(btnClose);
            ButtonStyles.Secondary(btnClose);
            ButtonStyles.Primary(btnAutoTrip);
            ButtonStyles.Primary(btnManual);
            btnAutoTrip.Margin = btnManual.Margin = new Padding(S(8), 0, 0, 0);
            btnClose.Margin = new Padding(0);
            _primaryBox.Controls.AddRange(new Control[] { btnClose, btnAutoTrip, btnManual });
            bottomBar.Controls.Add(lblStatus, 0, 0);
            bottomBar.SetColumnSpan(lblStatus, 2);
            bottomBar.Controls.Add(_primaryBox, 2, 0);
            bottomBar.Padding = new Padding(0, S(8), 0, 0);
            SetMode(false);
        }

        private void BuildOption(Panel opt, string title, string desc, bool manual)
        {
            int S(int v) => LogicalToDeviceUnits(v);
            opt.Size = new Size(S(300), S(58));
            opt.Margin = new Padding(0, S(4), S(10), S(4));
            opt.Cursor = Cursors.Hand;
            opt.BackColor = Color.White;
            var t = new Label { Text = title, AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Location = new Point(S(10), S(6)), Cursor = Cursors.Hand };
            var d = new Label
            {
                Text = desc,
                AutoSize = false,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Theme.TextSecondary,
                Location = new Point(S(10), S(27)),
                Size = new Size(S(282), S(28)),
                Cursor = Cursors.Hand
            };
            opt.Controls.Add(t);
            opt.Controls.Add(d);
            foreach (Control c in new Control[] { opt, t, d }) c.Click += (_, _) => SetMode(manual);
            opt.Paint += (_, e) =>
            {
                var on = _manualMode == manual;
                using var pen = new Pen(on ? Theme.Primary : Theme.BorderColor, on ? 2 : 1);
                e.Graphics.DrawRectangle(pen, 1, 1, opt.Width - 3, opt.Height - 3);
            };
        }

        /// <summary>Đổi cách nhập: Tự xếp (ẩn ô số lượng + bảng kệ) / Tự chọn kệ (hiện ra). Nút chính đổi theo.</summary>
        private void SetMode(bool manual)
        {
            _manualMode = manual;
            foreach (var (o, on) in new[] { (_optAuto, !manual), (_optManual, manual) })
            {
                o.BackColor = on ? Color.FromArgb(239, 246, 255) : Color.White;
                foreach (Control c in o.Controls) c.BackColor = o.BackColor;
                o.Invalidate();
            }
            root.SuspendLayout();
            manualBar.Visible = lblManualHint.Visible = gridBins.Visible = manual;
            var binsRow = root.GetRow(gridBins);
            root.RowStyles[binsRow] = manual ? new RowStyle(SizeType.Percent, 45F) : new RowStyle(SizeType.Absolute, 0);
            root.ResumeLayout(true);
            btnAutoTrip.Visible = !manual;
            btnManual.Visible = manual;
            UpdateButtons();
        }

        public PendingReceiptsForm(ApiServices api, int warehouseId, string warehouseName) : this()
        {
            _api = api;
            _service = new PendingReceiptApiService(api.Connection);
            _warehouseId = warehouseId;
            lblTitle.Text = $"📦 Nhập hàng từ chuyến — {warehouseName}";
            Load += async (_, _) => await LoadPendingAsync(null);
        }

        /// <summary>Giờ trong DB là UTC (đôi khi thiếu nhãn) -> đổi đúng giờ Việt Nam.</summary>
        private static DateTime ToVn(DateTime t)
            => t.Kind == DateTimeKind.Local ? t : DateTime.SpecifyKind(t, DateTimeKind.Utc).ToLocalTime();

        private static string Key(PendingReceiptLineItem p) => $"{p.TripId}:{p.CargoLineId}";

        private PendingReceiptLineItem? SelectedLine()
        {
            if (gridPending.CurrentRow?.Cells["Key"]?.Value is not string key) return null;
            return _pending.FirstOrDefault(p => Key(p) == key);
        }

        private static string ZoneText(string zoneType) => zoneType switch
        {
            "Hazmat" => "☣️ Nguy hiểm",
            "Fragile" => "🔷 Dễ vỡ",
            _ => "📦 Thường",
        };

        // ================= Tải dữ liệu =================

        private async Task LoadPendingAsync(string? keepKey)
        {
            if (_service is null) return;
            try
            {
                _pending = await _service.GetPendingAsync(_warehouseId);
                if (IsDisposed) return;
                pendingFilter.SetChoiceOptions("order", _pending.Select(p => p.OrderCode));
                BindPending(keepKey);

                if (_pending.Count == 0)
                    ShowStatus("✅ Không còn hàng nào chờ nhập kho ở kho này.", Theme.Success);
                await OnSelectionChangedAsync();
            }
            catch (Exception ex)
            {
                if (!IsDisposed) ShowStatus("Lỗi tải danh sách hàng chờ nhập: " + ex.Message, Theme.Danger);
            }
        }

        private void BindPending(string? keepKey)
        {
            var rows = _pending.Where(p =>
                    pendingFilter.Is("zone", ZoneText(p.ZoneType)) &&
                    pendingFilter.Is("order", p.OrderCode) &&
                    pendingFilter.MatchesSearch($"#{p.TripId}", p.OrderCode, p.CustomerName, p.DriverName, p.PlateNumber, p.CargoType))
                .ToList();
            pendingFilter.SetCount(rows.Count, _pending.Count);
            gridPending.DataSource = rows.Select(p => new
            {
                Key = Key(p),
                Chuyến = $"#{p.TripId}",
                Đơn = p.OrderCode,
                Khách_hàng = p.CustomerName,
                Tài_xế_xe = $"{p.DriverName} · {p.PlateNumber}",
                Về_kho_lúc = p.ArrivedAt is null ? "" : ToVn(p.ArrivedAt.Value).ToString("dd/MM HH:mm"),
                // Chặng Giao của đơn phải xuất phát trước — hàng gấp nhập trước (đã xếp lên đầu).
                Giao_phải_xuất_trước = ScreenKit.StartBy(p.DeliveryLatestStart).Text,
                Loại_hàng = p.CargoType,
                Khu = ZoneText(p.ZoneType),
                Kế_hoạch = p.QtyPlanned,
                Đã_nhập = p.QtyReceived,
                Chờ_nhập = p.QtyPending,
            }).ToList();

            var keep = keepKey is null ? -1 : rows.FindIndex(p => Key(p) == keepKey);
            if (keep >= 0 && keep < gridPending.Rows.Count)
                gridPending.CurrentCell = gridPending.Rows[keep].Cells["Chuyến"];
        }

        private async Task OnSelectionChangedAsync()
        {
            var line = SelectedLine();
            UpdateButtons();
            if (line is null)
            {
                lblManualTitle.Text = "✋ Nhập THỦ CÔNG dòng hàng đang chọn";
                numQty.Maximum = 0;
                gridBins.DataSource = null;
                return;
            }

            lblManualTitle.Text = $"✋ Nhập THỦ CÔNG: \"{line.CargoType}\" — chuyến #{line.TripId}, còn chờ {line.QtyPending:0.##}";
            // Hàng đếm nguyên chiếc -> không cho số lẻ (server cũng từ chối 2,5 máy).
            numQty.DecimalPlaces = line.WholeUnits ? 0 : 2;
            numQty.Increment = line.WholeUnits ? 1 : 0.01m;
            numQty.Minimum = 0;
            numQty.Maximum = line.QtyPending;
            numQty.Value = line.QtyPending;

            var loadId = ++_binsLoadId;
            try
            {
                var bins = _api is null ? new List<StorageBinItem>() : await _api.Warehouses.GetAvailableBinsForCargoAsync(_warehouseId, line.CargoLineId);
                if (IsDisposed || loadId != _binsLoadId) return; // đã chọn dòng khác trong lúc chờ
                _bins = bins;
                var rows = new List<object>
                {
                    new { Id = 0, Vị_trí = "🤖 Hệ thống tự chọn kệ (có thể chia nhiều kệ)", Đã_dùng = "", Còn_trống_m3 = "", Tải_tối_đa_kg = "", Cách_cửa_m = "" }
                };
                rows.AddRange(_bins.Select(b => (object)new
                {
                    Id = b.Id,
                    Vị_trí = b.Code,
                    Đã_dùng = $"{(b.CapacityM3 == 0 ? 0 : b.CurrentUsedM3 / b.CapacityM3 * 100):0}%",
                    Còn_trống_m3 = Math.Max(0, b.CapacityM3 - b.CurrentUsedM3).ToString("0.00"),
                    Tải_tối_đa_kg = b.MaxWeightKg.ToString("0.##"),
                    Cách_cửa_m = b.DistanceFromDoorMeters.ToString("0.0"),
                }));
                gridBins.DataSource = rows;
                if (gridBins.Rows.Count > 0) gridBins.CurrentCell = gridBins.Rows[0].Cells["Vị_trí"];
                if (_bins.Count == 0)
                    ShowStatus($"⚠️ Kho chưa có kệ nào thuộc khu {ZoneText(line.ZoneType)} — thêm khu / kệ trước khi nhập loại hàng này.", Theme.Warning);
            }
            catch (Exception ex)
            {
                if (!IsDisposed && loadId == _binsLoadId) ShowStatus("Lỗi tải danh sách kệ: " + ex.Message, Theme.Danger);
            }
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            var line = SelectedLine();
            btnAutoTrip.Enabled = !_busy && line is not null;
            btnManual.Enabled = !_busy && line is not null && line.QtyPending > 0;
            numQty.Enabled = !_busy && line is not null;
            btnRefresh.Enabled = !_busy;
            btnAutoTrip.Text = line is null ? "📥 Nhập cả chuyến (tự xếp kệ)" : $"📥 Nhập cả chuyến #{line.TripId} (tự xếp kệ)";
            btnManual.Text = "📥 Nhập vào kệ đã chọn";
        }

        private void SetBusy(bool busy)
        {
            _busy = busy;
            UseWaitCursor = busy;
            UpdateButtons();
        }

        private void ShowStatus(string text, Color color)
        {
            lblStatus.Text = text;
            lblStatus.ForeColor = color;
        }

        // ================= Thao tác =================

        private async void BtnAutoTrip_Click(object? sender, EventArgs e)
        {
            if (_service is null || _busy) return;
            var line = SelectedLine();
            if (line is null) return;

            var tripLines = _pending.Where(p => p.TripId == line.TripId).ToList();
            if (!Ask.Confirm(this, $"Nhập cả chuyến #{line.TripId} vào kho?",
                    $"Đơn {line.OrderCode}:\n" + string.Join("\n", tripLines.Select(p => $"• {p.CargoType}: {p.QtyPending:0.##}")) +
                    "\n\nHệ thống tự chọn kệ phù hợp (có thể chia nhiều kệ). Nếu 1 dòng không xếp được thì không nhập dòng nào.",
                    $"📥 Nhập cả chuyến #{line.TripId}")) return;

            SetBusy(true);
            try
            {
                var result = await _service.ReceiveAutoAsync(_warehouseId, line.TripId);
                if (IsDisposed) return;
                if (result.Success) ReceivedAny = true;
                await LoadPendingAsync(result.Success ? null : Key(line)); // lỗi hay không cũng tải lại: số liệu có thể vừa đổi
                // Hiện kết quả SAU khi tải lại (tải lại có thể ghi đè dòng trạng thái) — để biết hàng nằm ở vị trí nào.
                if (!IsDisposed)
                    ShowStatus((result.Success ? "✅ " : "⚠️ ") + (result.Message ?? (result.Success ? "Đã nhập kho." : "Không nhập được.")),
                        result.Success ? Theme.Success : Theme.Warning);
            }
            catch (Exception ex)
            {
                if (!IsDisposed) ShowStatus("Lỗi: " + ex.Message, Theme.Danger);
            }
            finally
            {
                if (!IsDisposed) SetBusy(false);
            }
        }

        private async void BtnManual_Click(object? sender, EventArgs e)
        {
            if (_service is null || _busy) return;
            var line = SelectedLine();
            if (line is null) return;

            // Ô số nguyên: giá trị gõ vào có thể còn lẻ dù ô hiện số đã làm tròn.
            if (numQty.DecimalPlaces == 0) numQty.Value = Math.Round(numQty.Value);
            var qty = numQty.Value;
            if (qty <= 0) { ShowStatus("Số lượng nhập phải lớn hơn 0.", Theme.Warning); return; }
            if (qty > line.QtyPending) { ShowStatus($"Chỉ còn chờ nhập {line.QtyPending:0.##}.", Theme.Warning); return; }

            int? binId = gridBins.CurrentRow?.Cells["Id"]?.Value is int id && id != 0 ? id : null;
            var binText = binId is null ? "kệ do hệ thống tự chọn" : $"kệ {_bins.FirstOrDefault(b => b.Id == binId)?.Code}";
            if (!Ask.Confirm(this, $"Nhập {qty:0.##} \"{line.CargoType}\" vào {binText}?",
                    $"Hàng của chuyến #{line.TripId}." +
                    (qty < line.QtyPending ? $"\n\nSau khi nhập còn chờ {line.QtyPending - qty:0.##} — nhập tiếp vào kệ khác." : ""),
                    $"📥 Nhập {qty:0.##} vào kệ")) return;

            SetBusy(true);
            try
            {
                var result = await _service.ReceiveManualAsync(_warehouseId, line.TripId, line.CargoLineId, qty, binId);
                if (IsDisposed) return;
                if (result.Success) ReceivedAny = true;
                await LoadPendingAsync(Key(line)); // giữ dòng đang chọn (nếu còn chờ) để nhập tiếp phần còn lại
                if (!IsDisposed)
                    ShowStatus((result.Success ? "✅ " : "⚠️ ") + (result.Message ?? (result.Success ? "Đã nhập kho." : "Không nhập được.")),
                        result.Success ? Theme.Success : Theme.Warning);
            }
            catch (Exception ex)
            {
                if (!IsDisposed) ShowStatus("Lỗi: " + ex.Message, Theme.Danger);
            }
            finally
            {
                if (!IsDisposed) SetBusy(false);
            }
        }

        private async void BtnRefresh_Click(object? sender, EventArgs e)
        {
            if (_busy) return;
            await LoadPendingAsync(SelectedLine() is { } l ? Key(l) : null);
        }

        private void BtnClose_Click(object? sender, EventArgs e) => Close();

        private void GridBins_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || gridBins.Rows[e.RowIndex].Cells["Id"].Value is not int binId || binId == 0) return;
            var bin = _bins.FirstOrDefault(b => b.Id == binId);
            if (bin is null) return;
            var pct = bin.CapacityM3 == 0 ? 0 : (double)(bin.CurrentUsedM3 / bin.CapacityM3);
            gridBins.Rows[e.RowIndex].DefaultCellStyle.BackColor = pct switch
            {
                >= 0.85 => Color.FromArgb(254, 226, 226),
                >= 0.5 => Color.FromArgb(254, 243, 199),
                _ => Color.FromArgb(220, 252, 231),
            };
        }
    }
}
