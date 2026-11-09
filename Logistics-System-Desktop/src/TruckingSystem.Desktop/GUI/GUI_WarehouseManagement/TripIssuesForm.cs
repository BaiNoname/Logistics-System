using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_WarehouseManagement;
using TruckingSystem.Desktop.SERVICE_WarehouseManagement;

namespace TruckingSystem.Desktop.GUI_WarehouseManagement
{
    /// <summary>📤 XUẤT KHO THEO CHUYẾN — chặng Giao đã chia xe, chưa chạy: nhân viên kho soạn hàng theo
    /// cột "Lấy ở ô", bốc lên xe rồi XÁC NHẬN XUẤT (server xuất đúng tổng số lượng của chuyến). Chưa xuất
    /// đủ thì tài xế không bắt đầu chuyến giao được. Hoàn tác xuất = dỡ hàng về kệ (xe chưa chạy).
    /// Giao diện tĩnh ở TripIssuesForm.Designer.cs.</summary>
    public partial class TripIssuesForm : BaseForm
    {
        private ApiServices? _api;
        private TripIssueApiService? _service;
        private int _warehouseId;
        private List<TripIssueLineItem> _lines = new();
        private bool _busy;

        /// <summary>Có xuất / hoàn tác ít nhất 1 lần -> màn Kho vẽ lại sơ đồ.</summary>
        public bool ChangedAny { get; private set; }

        private const string StateReady = "🟢 Sẵn sàng xuất";
        private const string StateDone = "✅ Đã xuất — chờ xe chạy";

        /// <summary>Dùng cho trình thiết kế (Designer) của Visual Studio.</summary>
        public TripIssuesForm()
        {
            InitializeComponent();
            TruckingSystem.Desktop.Common.DialogFit.Enable(this, root); // vừa màn hình mọi máy, bảng không bị ép mất chữ
            TruckingSystem.Desktop.Common.DialogKeys.Enable(this); // Esc = đóng
            Anim.FadeIn(this);
            Theme.StyleGrid(grid);
            GridLayout.FitColumns(grid);
            grid.DataBindingComplete += (_, _) =>
            {
                if (grid.Columns["TripId"] is { } c) c.Visible = false;
            };
            // Tình trạng = nhãn màu; "Đã xuất" = thanh tiến độ đã xuất / cần xuất của dòng hàng.
            GridDecor.HoverRows(grid);
            GridDecor.Pills(grid, "Tình_trạng", (text, _) => GridDecor.StatusTone(text));
            // Đỏ = phải xuất hôm nay / đã quá mốc; vàng = mai; xám = còn thời gian. Rê chuột: giải thích mốc.
            GridDecor.Pills(grid, "Phải_xuất_trước", (_, row) => LineAt(row) is { } l
                ? ScreenKit.StartBy(l.LatestStart, l.Deadline).Level switch { 2 => GridDecor.Red, 1 => GridDecor.Amber, _ => GridDecor.Slate }
                : null);
            grid.CellToolTipTextNeeded += (_, e) =>
            {
                if (e.RowIndex < 0 || grid.Columns[e.ColumnIndex].Name != "Phải_xuất_trước" || LineAt(e.RowIndex) is not { } l) return;
                e.ToolTipText = l.LatestStart is DateTime ls
                    ? $"Hạn giao {l.Deadline:dd/MM/yyyy} — tuyến cần thời gian chạy, nên xe phải xuất phát chậm nhất {ls:dd/MM/yyyy}.\n{l.DeadlineText}"
                    : "Đơn chưa có hạn giao.";
            };
            GridDecor.ProgressBars(grid, "Đã_xuất", row =>
            {
                if (row < 0 || row >= grid.Rows.Count) return null;
                if (grid.Rows[row].Cells["Đã_xuất"].Value is not decimal issued
                    || grid.Rows[row].Cells["Cần_xuất"].Value is not decimal planned || planned <= 0) return null;
                var ratio = (double)(issued / planned);
                return (ratio, $"{issued:0.##} / {planned:0.##}", ratio >= 1 ? Theme.Success : GridDecor.AccentViolet);
            });
            grid.SelectionChanged += (_, _) => UpdateSelection();
            issueFilter.AddChoice("state", "Tình trạng", new[] { StateReady, "🟡 Đã xuất một phần", StateDone, "📦 Chờ nhập kho", "⏳ Chờ chặng Lấy hàng", "⚠️ Kho không đủ hàng" });
            issueFilter.FilterChanged += (_, _) => BindGrid(SelectedTripId());
            SetupSteps();
        }

        // ================= Bố cục theo BƯỚC (mẫu UX ⑤ đã duyệt) =================
        // ① Chọn chuyến (bảng trên) → ② Danh sách soạn hàng theo KỆ có ô tích (tích khi đã bốc lên xe)
        // → ③ nút chính "📤 Xuất kho chuyến #15 (2/3)" chỉ bấm được khi tích đủ. Hoàn tác = chữ đỏ góc trái, tách xa.
        private readonly CheckedListBox _pick = new()
        {
            Dock = DockStyle.Fill,
            CheckOnClick = true,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 10.5F),
            IntegralHeight = false,
        };
        private readonly Label _why = new() { AutoSize = true, Font = new Font("Segoe UI", 9F), ForeColor = Theme.TextSecondary, Margin = new Padding(0, 4, 0, 8) };
        private readonly Dictionary<int, HashSet<string>> _checked = new(); // chuyến -> các dòng soạn đã tích
        private List<(string Key, string Text)> _pickItems = new();
        private int? _pickTrip;
        private bool _fillingPick, _pickStale;

        private void SetupSteps()
        {
            int S(int v) => LogicalToDeviceUnits(v);
            lblInfo.Text = "① Chọn chuyến sắp chạy — chặng Giao đã chia xe, chưa chạy. Chưa xuất đủ hàng thì tài xế không bắt đầu chuyến được.";
            lblInfo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblInfo.ForeColor = Theme.TextPrimary;

            // ② checklist ngay dưới dòng thông tin chuyến; bảng chuyến 55% / checklist 45%.
            var hintRow = root.GetRow(lblTripHint);
            KpiStrip.InsertRow(root, _pick, hintRow + 1);
            KpiStrip.InsertRow(root, _why, hintRow + 2);
            root.RowStyles[root.GetRow(grid)] = new RowStyle(SizeType.Percent, 55F);
            root.RowStyles[root.GetRow(_pick)] = new RowStyle(SizeType.Percent, 45F);
            root.RowStyles[root.GetRow(_why)] = new RowStyle(SizeType.AutoSize);
            lblTripHint.Margin = new Padding(0, S(4), 0, S(4));
            _pick.ItemCheck += (_, e) =>
            {
                if (_fillingPick || _pickTrip is not int t || e.Index < 0 || e.Index >= _pickItems.Count) return;
                var set = _checked.TryGetValue(t, out var h) ? h : _checked[t] = new HashSet<string>();
                if (e.NewValue == CheckState.Checked) set.Add(_pickItems[e.Index].Key); else set.Remove(_pickItems[e.Index].Key);
                BeginInvoke(new Action(UpdatePrimary)); // ItemCheck chạy TRƯỚC khi ô đổi trạng thái
            };

            // ③ chân cửa sổ: [↩️ Hoàn tác…] [🔄] .. trạng thái .. [Đóng] [📤 Xuất kho chuyến #N (x/y)]
            bottomBar.SuspendLayout();
            bottomBar.Controls.Clear();
            bottomBar.ColumnStyles.Clear();
            foreach (var w in new[] { 0, 0, 100, 0, 0 })
                bottomBar.ColumnStyles.Add(w == 0 ? new ColumnStyle(SizeType.AutoSize) : new ColumnStyle(SizeType.Percent, 100F));
            ButtonStyles.DangerLink(btnUndo);
            ButtonStyles.Secondary(btnRefresh);
            ButtonStyles.Secondary(btnClose);
            ButtonStyles.Primary(btnIssue);
            btnUndo.Text = "↩️ Hoàn tác lần xuất trước…";
            btnRefresh.Text = "🔄";
            btnRefresh.Margin = new Padding(S(8), 0, S(12), 0);
            btnClose.Margin = new Padding(0, 0, S(8), 0);
            btnIssue.Margin = Padding.Empty;
            bottomBar.Controls.Add(btnUndo, 0, 0);
            bottomBar.Controls.Add(btnRefresh, 1, 0);
            bottomBar.Controls.Add(lblStatus, 2, 0);
            bottomBar.Controls.Add(btnClose, 3, 0);
            bottomBar.Controls.Add(btnIssue, 4, 0);
            bottomBar.Padding = new Padding(0, S(6), 0, 0);
            bottomBar.ResumeLayout(true);
            new ToolTip().SetToolTip(btnRefresh, "Tải lại danh sách");
        }

        /// <summary>Danh sách soạn hàng của chuyến: mỗi dòng hàng chia theo kệ (lô nhập trước lấy trước) — "Kệ A-03 — lấy 20 thùng".</summary>
        private static List<(string Key, string Text)> PickList(List<TripIssueLineItem> lines)
        {
            var list = new List<(string, string)>();
            foreach (var l in lines.Where(x => x.QtyPending > 0))
            {
                var left = l.QtyPending;
                var bins = System.Text.RegularExpressions.Regex.Matches(l.BinsText ?? "", @"([^,(]+?)\s*\(([\d.,]+)\)");
                foreach (System.Text.RegularExpressions.Match m in bins)
                {
                    if (left <= 0) break;
                    if (!decimal.TryParse(m.Groups[2].Value.Replace(",", "."), System.Globalization.NumberStyles.Number,
                            System.Globalization.CultureInfo.InvariantCulture, out var stored) || stored <= 0) continue;
                    var take = Math.Min(left, stored);
                    left -= take;
                    var code = m.Groups[1].Value.Trim();
                    list.Add(($"{l.CargoLineId}|{code}", $"Kệ {code} — lấy {take:0.##} {l.CargoType}   (đơn {l.OrderCode})"));
                }
                if (left > 0)
                    list.Add(($"{l.CargoLineId}|?", $"{l.CargoType}: lấy {left:0.##}" + (bins.Count == 0 ? "   (chưa rõ kệ)" : "   (còn thiếu trong kho)") + $"   (đơn {l.OrderCode})"));
            }
            return list;
        }

        private void FillPick(int? tripId)
        {
            _fillingPick = true;
            _pick.BeginUpdate();
            try
            {
                _pick.Items.Clear();
                _pickTrip = tripId;
                _pickStale = false;
                _pickItems = tripId is int id ? PickList(TripLines(id)) : new();
                var set = tripId is int t && _checked.TryGetValue(t, out var h) ? h : null;
                foreach (var (key, text) in _pickItems) _pick.Items.Add(text, set?.Contains(key) == true);
            }
            finally { _pick.EndUpdate(); _fillingPick = false; }
        }

        /// <summary>Nút chính ghi rõ chuyến + đã tích bao nhiêu dòng; mờ đi kèm lý do khi chưa bấm được.</summary>
        private void UpdatePrimary()
        {
            if (IsDisposed) return;
            var first = SelectedTripId() is int id ? TripLines(id).FirstOrDefault() : null;
            var total = _pickItems.Count;
            var done = _pickTrip is int t && _checked.TryGetValue(t, out var h) ? _pickItems.Count(i => h.Contains(i.Key)) : 0;
            btnIssue.Text = first is null ? "📤 Xuất kho chuyến" : $"📤 Xuất kho chuyến #{first.TripId} ({done}/{total})";
            btnIssue.Enabled = !_busy && first is { CanIssue: true } && done >= total;
            btnUndo.Visible = first is { CanUndo: true };
            btnUndo.Enabled = !_busy;
            _pick.Enabled = first is { CanIssue: true };
            (_why.Text, _why.ForeColor) = first switch
            {
                null => ("", Theme.TextSecondary),
                { CanIssue: false, BlockReason: { } r } => ("Chưa xuất được: " + r, Theme.Danger),
                { CanIssue: false } => (total == 0 ? "Chuyến này đã xuất đủ hàng — chờ tài xế chạy." : "Chuyến này chưa xuất được.", Theme.TextSecondary),
                _ when done < total => ($"③ Tích đủ {total}/{total} dòng (đã bốc lên xe) mới bấm \"Xuất kho\" được — còn {total - done} dòng.", Theme.TextSecondary),
                _ => ("③ Đã soạn đủ — bấm \"📤 Xuất kho\" ở góc phải để xác nhận.", Theme.Success),
            };
        }

        public TripIssuesForm(ApiServices api, int warehouseId, string warehouseName) : this()
        {
            _api = api;
            _service = new TripIssueApiService(api.Connection);
            _warehouseId = warehouseId;
            lblTitle.Text = $"📤 Xuất kho theo chuyến — {warehouseName}";
            Load += async (_, _) => await LoadAsync(null);
        }

        private int? SelectedTripId() => grid.CurrentRow?.Cells["TripId"]?.Value as int?;

        /// <summary>Dòng hàng của 1 dòng bảng (theo chuyến — các dòng cùng chuyến có cùng mốc).</summary>
        private TripIssueLineItem? LineAt(int row)
            => row >= 0 && row < grid.Rows.Count && grid.Rows[row].Cells["TripId"].Value is int id ? _lines.FirstOrDefault(l => l.TripId == id) : null;
        private List<TripIssueLineItem> TripLines(int tripId) => _lines.Where(l => l.TripId == tripId).ToList();

        private async Task LoadAsync(int? keepTripId)
        {
            if (_service is null) return;
            try
            {
                _lines = await _service.GetAsync(_warehouseId);
                if (IsDisposed) return;
                BindGrid(keepTripId);
                var trips = _lines.Select(l => l.TripId).Distinct().Count();
                var ready = _lines.Where(l => l.CanIssue).Select(l => l.TripId).Distinct().Count();
                ShowStatus(trips == 0 ? "Không có chuyến giao nào đang chờ ở kho này."
                    : $"{trips} chuyến giao chưa chạy — {ready} chuyến sẵn sàng xuất.", Theme.TextSecondary);
            }
            catch (Exception ex)
            {
                if (!IsDisposed) ShowStatus("Lỗi tải danh sách: " + ex.Message, Theme.Danger);
            }
        }

        private void BindGrid(int? keepTripId)
        {
            _pickStale = true; // số liệu mới -> dựng lại danh sách soạn
            var rows = _lines.Where(l =>
                    issueFilter.Is("state", l.TripState) &&
                    issueFilter.MatchesSearch($"#{l.TripId}", l.OrderCode, l.CustomerName, l.PlateNumber, l.DriverName, l.CargoType, l.Destination))
                .ToList();
            issueFilter.SetCount(rows.Select(r => r.TripId).Distinct().Count(), _lines.Select(l => l.TripId).Distinct().Count());
            grid.DataSource = rows.Select(l => new
            {
                l.TripId,
                Chuyến = $"#{l.TripId}",
                Đơn = l.OrderCode,
                Khách_hàng = l.CustomerName,
                Xe_tài_xế = $"{l.PlateNumber} · {l.DriverName}",
                Giao_đến = l.Destination,
                // Mốc PHẢI xuất phát trước (cùng quy tắc màn Chuyến) — chuyến gấp đã được xếp lên đầu.
                Phải_xuất_trước = ScreenKit.StartBy(l.LatestStart, l.Deadline).Text,
                Loại_hàng = l.CargoType,
                Cần_xuất = l.QtyPlanned,
                Đã_xuất = l.QtyIssued,
                Trong_kho = l.QtyOnHand,
                Lấy_ở_kệ = l.BinsText,
                Tình_trạng = l.TripState,
            }).ToList();
            if (keepTripId is int keep)
                foreach (DataGridViewRow r in grid.Rows)
                    if (r.Cells["TripId"].Value is int id && id == keep) { grid.CurrentCell = r.Cells["Chuyến"]; break; }
            UpdateSelection();
        }

        private void UpdateSelection()
        {
            var tripId = SelectedTripId();
            var lines = tripId is int id ? TripLines(id) : new List<TripIssueLineItem>();
            var first = lines.FirstOrDefault();
            if (_pickStale || tripId != _pickTrip) FillPick(tripId);
            UpdatePrimary();
            if (first is null)
            {
                lblTripHint.Text = "② Soạn hàng — chọn 1 chuyến ở bảng trên để xem danh sách cần lấy.";
                lblTripHint.ForeColor = Theme.TextSecondary;
                return;
            }
            lblTripHint.Text = $"② Soạn hàng chuyến #{first.TripId} — tích từng dòng khi đã bốc lên xe {first.PlateNumber} · {first.DriverName}";
            lblTripHint.ForeColor = Theme.TextPrimary;
        }

        private async void BtnIssue_Click(object? sender, EventArgs e)
        {
            if (_service is null || _busy || SelectedTripId() is not int tripId) return;
            var lines = TripLines(tripId);
            var list = string.Join("\n", lines.Where(l => l.QtyPending > 0)
                .Select(l => $"• {l.CargoType}: {l.QtyPending:0.##}  (lấy ở kệ: {l.BinsText})"));
            if (!Ask.Confirm(this, $"Xuất kho chuyến #{tripId} lên xe {lines[0].PlateNumber}?",
                    $"Hàng đã soạn và bốc lên xe:\n{list}\n\nTồn kho sẽ bị trừ đúng các số lượng này.",
                    $"📤 Xuất kho chuyến #{tripId}")) return;
            await RunAsync(() => _service.IssueAsync(_warehouseId, tripId), tripId);
        }

        private async void BtnUndo_Click(object? sender, EventArgs e)
        {
            if (_service is null || _busy || SelectedTripId() is not int tripId) return;
            if (!Ask.Confirm(this, $"Hoàn tác xuất kho chuyến #{tripId}?",
                    "Hàng được dỡ khỏi xe và xếp lại lên kệ (xe chưa chạy).", "↩️ Hoàn tác xuất kho", danger: true)) return;
            await RunAsync(() => _service.UndoAsync(_warehouseId, tripId), tripId);
        }

        private async Task RunAsync(Func<Task<WarehouseActionResultItem>> action, int tripId)
        {
            _busy = true;
            UpdateSelection();
            try
            {
                var result = await action();
                if (IsDisposed) return;
                if (result.Success)
                {
                    ChangedAny = true;
                    _checked.Remove(tripId);
                    Toast.Show(this, "✅ " + (result.Message ?? "Đã xong."));
                }
                else
                {
                    Ask.Fail(this, result.Message ?? "Không thực hiện được.");
                }
                await LoadAsync(tripId);
            }
            catch (Exception ex)
            {
                if (!IsDisposed) ShowStatus("Lỗi: " + ex.Message, Theme.Danger);
            }
            finally
            {
                _busy = false;
                if (!IsDisposed) UpdateSelection();
            }
        }

        private async void BtnRefresh_Click(object? sender, EventArgs e) => await LoadAsync(SelectedTripId());

        private void BtnClose_Click(object? sender, EventArgs e) => Close();

        private void ShowStatus(string message, Color color)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = color;
        }
    }
}
