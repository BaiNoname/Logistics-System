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
            Theme.StyleGrid(grid);
            GridLayout.FitColumns(grid);
            grid.DataBindingComplete += (_, _) =>
            {
                if (grid.Columns["TripId"] is { } c) c.Visible = false;
                foreach (DataGridViewRow row in grid.Rows)
                    if (row.Cells["Tình_trạng"].Value is string st)
                        row.Cells["Tình_trạng"].Style.ForeColor = st.StartsWith("🟢") ? Theme.Success
                            : st.StartsWith("✅") ? Theme.Primary : st.StartsWith("⚠️") ? Theme.Danger : Theme.Warning;
            };
            grid.SelectionChanged += (_, _) => UpdateSelection();
            issueFilter.AddChoice("state", "Tình trạng", new[] { StateReady, "🟡 Đã xuất một phần", StateDone, "📦 Chờ nhập kho", "⏳ Chờ chặng Lấy hàng", "⚠️ Kho không đủ hàng" });
            issueFilter.FilterChanged += (_, _) => BindGrid(SelectedTripId());
            Load += (_, _) => Theme.UniformButtons(bottomBar);
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
                Xuất_phát_dự_kiến = l.PlannedStartDate?.ToString("dd/MM/yyyy") ?? "—",
                Loại_hàng = l.CargoType,
                Cần_xuất = l.QtyPlanned,
                Đã_xuất = l.QtyIssued,
                Trong_kho = l.QtyOnHand,
                Lấy_ở_ô = l.BinsText,
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
            btnIssue.Enabled = !_busy && first is { CanIssue: true };
            btnUndo.Enabled = !_busy && first is { CanUndo: true };
            if (first is null)
            {
                lblTripHint.Text = "Chọn 1 dòng để xem chuyến.";
                lblTripHint.ForeColor = Theme.TextSecondary;
                return;
            }
            lblTripHint.Text = $"Chuyến #{first.TripId} — xe {first.PlateNumber} · {first.DriverName} — {first.TripState}" +
                               (first.BlockReason is null ? "" : $": {first.BlockReason}");
            lblTripHint.ForeColor = first.BlockReason is null ? Theme.TextPrimary : Theme.Danger;
        }

        private async void BtnIssue_Click(object? sender, EventArgs e)
        {
            if (_service is null || _busy || SelectedTripId() is not int tripId) return;
            var lines = TripLines(tripId);
            var list = string.Join("\n", lines.Where(l => l.QtyPending > 0)
                .Select(l => $"• {l.CargoType}: {l.QtyPending:0.##}  (lấy ở ô: {l.BinsText})"));
            var confirm = MessageBox.Show(this,
                $"Xác nhận ĐÃ SOẠN và BỐC LÊN XE {lines[0].PlateNumber} hàng của chuyến #{tripId}?\n\n{list}\n\nTồn kho sẽ bị trừ đúng các số lượng này.",
                "Xác nhận xuất kho", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;
            await RunAsync(() => _service.IssueAsync(_warehouseId, tripId), tripId);
        }

        private async void BtnUndo_Click(object? sender, EventArgs e)
        {
            if (_service is null || _busy || SelectedTripId() is not int tripId) return;
            var confirm = MessageBox.Show(this,
                $"Hoàn tác xuất kho chuyến #{tripId}? Hàng được dỡ khỏi xe và xếp lại vào kệ (xe chưa chạy).",
                "Hoàn tác xuất kho", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;
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
                    MessageBox.Show(this, result.Message ?? "Đã xong.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(this, result.Message ?? "Không thực hiện được.", "Chưa thực hiện được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
