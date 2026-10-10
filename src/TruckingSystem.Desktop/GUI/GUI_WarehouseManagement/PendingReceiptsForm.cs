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
        }

        public PendingReceiptsForm(ApiServices api, int warehouseId, string warehouseName) : this()
        {
            _api = api;
            _service = new PendingReceiptApiService(api.Connection);
            _warehouseId = warehouseId;
            lblTitle.Text = $"📦 Hàng chờ nhập kho — {warehouseName}";
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
                gridPending.DataSource = _pending.Select(p => new
                {
                    Key = Key(p),
                    Chuyến = $"#{p.TripId}",
                    Đơn = p.OrderCode,
                    Khách_hàng = p.CustomerName,
                    Tài_xế_xe = $"{p.DriverName} · {p.PlateNumber}",
                    Về_kho_lúc = p.ArrivedAt is null ? "" : ToVn(p.ArrivedAt.Value).ToString("dd/MM HH:mm"),
                    Loại_hàng = p.CargoType,
                    Khu = ZoneText(p.ZoneType),
                    Kế_hoạch = p.QtyPlanned,
                    Đã_nhập = p.QtyReceived,
                    Chờ_nhập = p.QtyPending,
                }).ToList();

                var keep = keepKey is null ? -1 : _pending.FindIndex(p => Key(p) == keepKey);
                if (keep >= 0 && keep < gridPending.Rows.Count)
                    gridPending.CurrentCell = gridPending.Rows[keep].Cells["Chuyến"];

                if (_pending.Count == 0)
                    ShowStatus("✅ Không còn hàng nào chờ nhập kho ở kho này.", Theme.Success);
                await OnSelectionChangedAsync();
            }
            catch (Exception ex)
            {
                if (!IsDisposed) ShowStatus("Lỗi tải danh sách hàng chờ nhập: " + ex.Message, Theme.Danger);
            }
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
                    new { Id = 0, Vị_trí = "🤖 Best-Fit tự chọn (có thể chia nhiều vị trí)", Đã_dùng = "", Còn_trống_m3 = "", Tải_tối_đa_kg = "", Cách_cửa_m = "" }
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
                    ShowStatus($"⚠️ Kho chưa có vị trí nào thuộc khu {ZoneText(line.ZoneType)} — thêm khu vực/vị trí trước khi nhập loại hàng này.", Theme.Warning);
            }
            catch (Exception ex)
            {
                if (!IsDisposed && loadId == _binsLoadId) ShowStatus("Lỗi tải danh sách vị trí: " + ex.Message, Theme.Danger);
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
            btnAutoTrip.Text = line is null ? "🤖 Nhập TỰ ĐỘNG cả chuyến (Best-Fit)" : $"🤖 Nhập TỰ ĐỘNG cả chuyến #{line.TripId} (Best-Fit)";
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
            var confirm = MessageBox.Show(this,
                $"Nhập TỰ ĐỘNG (Best-Fit) toàn bộ hàng còn chờ của chuyến #{line.TripId} (đơn {line.OrderCode})?\n\n" +
                string.Join("\n", tripLines.Select(p => $"• {p.CargoType}: {p.QtyPending:0.##}")) +
                "\n\nHệ thống tự chọn vị trí phù hợp (có thể chia nhiều vị trí). Nếu 1 dòng không xếp được thì không nhập dòng nào.",
                "Nhập kho tự động", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

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
            var binText = binId is null ? "vị trí do Best-Fit tự chọn" : $"vị trí {_bins.FirstOrDefault(b => b.Id == binId)?.Code}";
            var confirm = MessageBox.Show(this,
                $"Nhập {qty:0.##} \"{line.CargoType}\" của chuyến #{line.TripId} vào {binText}?" +
                (qty < line.QtyPending ? $"\n\nSau khi nhập còn chờ {line.QtyPending - qty:0.##} — nhập tiếp vào vị trí khác." : ""),
                "Nhập kho thủ công", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

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
