using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_VehicleManagement;

namespace TruckingSystem.Desktop.GUI_VehicleManagement
{
    public partial class VehiclesControl : BaseUserControl
    {
        private ApiServices? _api;
        private List<VehicleListItem> _vehicles = new();

        public VehiclesControl()
        {
            InitializeComponent();
            Theme.StyleGrid(grid);
            GridLayout.FitColumns(grid); // cột vừa nội dung, nhiều cột thì cuộn ngang thay vì cắt chữ
            Load += (_, _) => Theme.UniformButtons(toolbar); // nút đều nhau, xuống dòng vẫn thẳng cột
            grid.DataBindingComplete += (_, _) => { if (grid.Columns["Id"] != null) grid.Columns["Id"]!.Visible = false; };
            // Sửa xe / Giấy tờ xe chỉ bấm được khi đã chọn 1 xe
            grid.SelectionChanged += (_, _) => btnEdit.Enabled = btnDocuments.Enabled = btnFuelLevel.Enabled = grid.CurrentRow is not null;
            grid.DataBindingComplete += (_, _) => btnEdit.Enabled = btnDocuments.Enabled = btnFuelLevel.Enabled = grid.CurrentRow is not null;
            filterBar.AddChoice("status", "Trạng thái", new[] { "Rảnh", "Đang chạy", "Bảo trì", "Ngưng hoạt động" });
            filterBar.AddChoice("docs", "Giấy tờ", new[] { DocsOk, DocsBlocked });
            filterBar.AddChoice("fuel", "Nhiên liệu", new[] { FuelLow, FuelUnknown });
            filterBar.AddChoice("warehouse", "Kho", Array.Empty<string>());
            filterBar.FilterChanged += (_, _) => BindGrid();
            SetupModernLook();
            SetupRowActions();
            Load += async (_, _) => await LoadAsync();
        }

        /// <summary>Nhấp đúp 1 xe = mở form Sửa xe; chuột phải = menu Sửa xe / Giấy tờ xe / Nhiên liệu (cùng kiểu
        /// màn Chuyến, Đơn vận chuyển). Chuột phải chọn luôn đúng dòng dưới con trỏ.</summary>
        private void SetupRowActions()
        {
            grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) BtnEdit_Click(s, e); };

            var menu = new ContextMenuStrip();
            var edit = new ToolStripMenuItem("✏️  Sửa xe") { ShortcutKeyDisplayString = "Nhấp đúp", Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            edit.Click += BtnEdit_Click;
            var docs = new ToolStripMenuItem("📄  Giấy tờ xe");
            docs.Click += BtnDocuments_Click;
            var fuel = new ToolStripMenuItem("⛽  Nhiên liệu trong bình");
            fuel.Click += BtnFuelLevel_Click;
            var refresh = new ToolStripMenuItem("🔄  Làm mới danh sách") { ShortcutKeyDisplayString = "F5" };
            refresh.Click += BtnRefresh_Click;
            menu.Items.AddRange(new ToolStripItem[] { edit, new ToolStripSeparator(), docs, fuel, new ToolStripSeparator(), refresh });
            ModernMenu.Apply(menu);
            menu.Opening += (_, e) => { if (grid.CurrentRow is null) e.Cancel = true; };
            grid.ContextMenuStrip = menu;

            grid.CellMouseDown += (_, e) =>
            {
                if (e.Button != MouseButtons.Right || e.RowIndex < 0) return;
                var col = Math.Max(0, e.ColumnIndex);
                if (grid.Rows[e.RowIndex].Cells[col].Visible) grid.CurrentCell = grid.Rows[e.RowIndex].Cells[col];
                else grid.Rows[e.RowIndex].Selected = true;
            };
            grid.KeyDown += (_, e) => { if (e.KeyCode == Keys.F5) { e.Handled = true; _ = LoadAsync(); } };
        }

        /// <summary>Nhãn màu trạng thái / giấy tờ + thanh nhiên liệu. (Bỏ hàng 6 thẻ số liệu theo góp ý GVHD — số cần
        /// chú ý gom vào 1 dòng ngắn SummaryText; lọc nhanh dùng bộ lọc Trạng thái / Giấy tờ / Nhiên liệu trên bảng.)</summary>
        private void SetupModernLook()
        {
            // Cửa sổ thấp: giữ bảng tối thiểu ~8 dòng, thiếu chỗ thì cả màn có thanh cuộn kéo xuống.
            ScrollFit.Enable(this, root, grid);

            GridDecor.HoverRows(grid);
            GridDecor.Pills(grid, "Trạng_thái", (text, _) => GridDecor.StatusTone(text));
            GridDecor.Pills(grid, "Giấy_tờ", (text, _) => text == DocsOk ? GridDecor.Green : GridDecor.Red);
            GridDecor.ProgressBars(grid, "Nhiên_liệu", row =>
            {
                if (VehicleAt(row) is not { } v || v.FuelCurrentLiters is not decimal cur
                    || v.FuelTankCapacityLiters is not decimal cap || cap <= 0) return null;
                var ratio = (double)(cur / cap);
                var color = ratio <= 0.15 ? Theme.Danger : ratio <= 0.35 ? Theme.Warning : Theme.Success;
                return (ratio, FuelText(v), color);
            });
        }

        private VehicleListItem? VehicleAt(int row)
            => row >= 0 && row < grid.Rows.Count && grid.Rows[row].Cells["Id"].Value is int id
                ? _vehicles.FirstOrDefault(v => v.Id == id) : null;

        /// <summary>1 dòng tóm tắt thay cho 6 thẻ số liệu: tổng số xe + CHỈ những việc cần chú ý (có số > 0).</summary>
        private string SummaryText()
        {
            var parts = new List<string> { $"Tổng {_vehicles.Count} xe" };
            void Add(int n, string text) { if (n > 0) parts.Add($"{text}: {n}"); }
            Add(_vehicles.Count(v => v.Status == "Available" && v.DocumentBlockReason is null), "🟢 rảnh, chia được ngay");
            Add(_vehicles.Count(v => v.Status == "OnTrip"), "🚚 đang chạy");
            Add(_vehicles.Count(v => v.Status == "UnderMaintenance"), "🔧 bảo trì");
            Add(_vehicles.Count(v => v.DocumentBlockReason is not null), "📄 giấy tờ thiếu / hết hạn");
            Add(_vehicles.Count(v => MatchesFuel(v, FuelLow)), "⛽ nhiên liệu thấp");
            return string.Join("   ·   ", parts) + "   —   nhấp đúp 1 xe để sửa, chuột phải để xem thêm thao tác";
        }

        private const string DocsOk = "✅ Hợp lệ";
        private const string DocsBlocked = "⛔ Thiếu / hết hạn";
        private const string FuelLow = "⚠️ Thấp (≤ 15%)";
        private const string FuelUnknown = "Chưa khai mức";
        private const string AnyWarehouse = "(dùng cho mọi kho)";

        private static bool MatchesFuel(VehicleListItem v, string? choice) => choice switch
        {
            null => true,
            FuelLow => v.FuelCurrentLiters is decimal c && v.FuelTankCapacityLiters is decimal cap && cap > 0 && c / cap <= 0.15m,
            FuelUnknown => v.FuelCurrentLiters is null,
            _ => true,
        };

        /// <summary>Đổ lưới theo bộ lọc, giữ dòng đang chọn.</summary>
        /// <summary>Mở từ thông báo (nhấp đúp): bỏ các bộ lọc khác, lọc sẵn theo biển số.</summary>
        public void FocusOn(string search) => filterBar.ShowOnlySearch(search);

        private void BindGrid()
        {
            var keepId = grid.CurrentRow?.Cells["Id"].Value as int?;
            var rows = _vehicles.Where(v =>
                    filterBar.Is("status", TranslateStatus(v.Status)) &&
                    filterBar.Is("docs", v.DocumentBlockReason is null ? DocsOk : DocsBlocked) &&
                    MatchesFuel(v, filterBar.Choice("fuel")) &&
                    filterBar.Is("warehouse", v.WarehouseName ?? AnyWarehouse) &&
                    filterBar.MatchesSearch(v.PlateNumber, v.VehicleType, v.WarehouseName, v.FuelType))
                .ToList();
            grid.DataSource = rows.Select(v => new
            {
                v.Id,
                Biển_số = v.PlateNumber,
                Loại_xe = v.VehicleType,
                Sức_chứa = v.CapacityQty,
                Kho = v.WarehouseName ?? AnyWarehouse,
                Trạng_thái = TranslateStatus(v.Status),
                // Lý do giấy tờ (server tính, cùng quy tắc Chia xe / Bắt đầu chuyến) — xe "Rảnh"
                // nhưng thiếu / hết hạn giấy tờ sẽ KHÔNG được chia xe.
                Giấy_tờ = v.DocumentBlockReason is null ? DocsOk : "⛔ " + v.DocumentBlockReason,
                Nhiên_liệu = FuelText(v),
            }).ToList();
            filterBar.SetCount(rows.Count, _vehicles.Count);
            if (keepId is int id)
                foreach (DataGridViewRow r in grid.Rows)
                    if (r.Cells["Id"].Value is int rid && rid == id) { grid.CurrentCell = r.Cells["Biển_số"]; break; }
        }

        public VehiclesControl(ApiServices api) : this()
        {
            _api = api;
        }

        private void BtnRefresh_Click(object? sender, EventArgs e) => _ = LoadAsync();

        private async void BtnCreate_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;
            using var f = new CreateVehicleForm(_api);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
        }

        private void BtnDocuments_Click(object? sender, EventArgs e)
        {
            if (_api is null || grid.CurrentRow is null)
            {
                MessageBox.Show(FindForm(), "Vui lòng chọn 1 xe trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var id = (int)grid.CurrentRow.Cells["Id"].Value;
            var vehicle = _vehicles.First(v => v.Id == id);

            using var f = new ManageVehicleDocumentsForm(_api, id, vehicle.PlateNumber);
            f.ShowDialog(FindForm());
            _ = LoadAsync(); // giấy tờ có thể vừa thêm/xóa -> cột "Giấy tờ" cập nhật
        }

        /// <summary>⛽ Mức nhiên liệu: xem nhiên liệu hiện có (ước tính), khai dung tích bình / mức đo thực tế.</summary>
        private async void BtnFuelLevel_Click(object? sender, EventArgs e)
        {
            if (_api is null || grid.CurrentRow is null)
            {
                MessageBox.Show(FindForm(), "Vui lòng chọn 1 xe trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var id = (int)grid.CurrentRow.Cells["Id"].Value;
            var vehicle = _vehicles.First(v => v.Id == id);
            using var f = new VehicleFuelLevelForm(_api, vehicle.Id, vehicle.PlateNumber);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
        }

        /// <summary>"120 / 300 L (40%)" — ước tính; chưa khai thì gợi ý khai báo.</summary>
        private static string FuelText(VehicleListItem v)
        {
            if (v.FuelCurrentLiters is not decimal cur) return v.FuelTankCapacityLiters is decimal c ? $"? / {c:0.#} L (chưa khai mức)" : "— chưa khai";
            if (v.FuelTankCapacityLiters is not decimal cap || cap <= 0) return $"~{cur:0.#} L";
            var pct = cur / cap * 100;
            return $"{(pct <= 15 ? "⚠️ " : "")}~{cur:0.#} / {cap:0.#} L ({pct:0}%)";
        }

        private async void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (_api is null || grid.CurrentRow is null)
            {
                MessageBox.Show(FindForm(), "Vui lòng chọn 1 xe trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var id = (int)grid.CurrentRow.Cells["Id"].Value;
            var vehicle = _vehicles.First(v => v.Id == id);

            using var f = new EditVehicleForm(_api, vehicle);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
        }

        private async Task LoadAsync()
        {
            if (_api is null) return;
            lblStatus.Text = "Đang tải...";
            lblStatus.ForeColor = Theme.TextSecondary;
            try
            {
                _vehicles = await _api.Vehicles.GetVehiclesAsync();
                filterBar.SetChoiceOptions("warehouse", _vehicles.Select(v => v.WarehouseName ?? AnyWarehouse));
                BindGrid();
                lblStatus.Text = SummaryText();
                lblStatus.ForeColor = _vehicles.Any(v => v.DocumentBlockReason is not null) ? Theme.Danger : Theme.TextSecondary;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }

        private static string TranslateStatus(string s) => s switch
        {
            // Cùng cách gọi với form Sửa xe.
            "Available" => "Rảnh",
            "OnTrip" => "Đang chạy",
            "UnderMaintenance" => "Bảo trì",
            "OutOfService" => "Ngưng hoạt động",
            _ => s,
        };
    }
}