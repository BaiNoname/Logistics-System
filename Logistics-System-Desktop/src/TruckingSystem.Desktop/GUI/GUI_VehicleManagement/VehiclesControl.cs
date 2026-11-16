using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_VehicleManagement;

namespace TruckingSystem.Desktop.GUI_VehicleManagement
{
    public partial class VehiclesControl : BaseUserControl
    {
        private ApiServices? _api;
        private List<VehicleListItem> _vehicles = new();
        private ScreenKit.GridOverlay _overlay = null!; // bảng trống / đang tải (Common/ScreenKit.cs)


        public VehiclesControl()
        {
            InitializeComponent();
            _overlay = ScreenKit.Overlay(grid, () => _vehicles.Count, "Chưa có xe nào",
                "Thêm xe đầu tiên kèm giấy tờ (đăng kiểm, bảo hiểm) để xe được chia chuyến.", ("➕ Thêm xe", () => btnCreate.PerformClick()),
                () => filterBar.ClearFilters());
            ScreenKit.Shortcuts(this, grid, null, () => btnCreate.PerformClick(), () => ShowDetail());
            ScreenKit.Help(this, "vehicles", "Danh sách xe của công ty: tình trạng, giấy tờ, sức chứa.",
                new[] { "Thêm xe (➕) và khai giấy tờ còn hạn", "Nhấp đúp 1 xe để xem đủ thông tin, sửa, thêm giấy tờ", "Xe thiếu / hết hạn giấy tờ sẽ không được chia chuyến" },
                "Ctrl+N thêm xe · Enter xem chi tiết · F5 làm mới",
                new[]
                {
                    ColorLegend.Header("Cột Trạng thái"),
                    ColorLegend.Pill("Rảnh", GridDecor.Green, "Sẵn sàng nhận chuyến"),
                    ColorLegend.Pill("Đang chạy", GridDecor.Blue, "Đang chạy 1 chuyến"),
                    ColorLegend.Pill("Bảo trì", GridDecor.Amber, "Đang bảo trì — không chia chuyến"),
                    ColorLegend.Pill("Ngưng hoạt động", GridDecor.Slate, "Không dùng nữa"),
                    ColorLegend.Header("Cột Giấy tờ"),
                    ColorLegend.Pill("✅ Hợp lệ", GridDecor.Green, "Đủ giấy tờ còn hạn"),
                    ColorLegend.Pill("⛔ …", GridDecor.Red, "Thiếu / hết hạn giấy tờ — xe KHÔNG được chia chuyến"),
                    ColorLegend.Header("Cột Nhiên liệu (thanh)"),
                    ColorLegend.Bar(Theme.Success, "Còn trên 35% bình"),
                    ColorLegend.Bar(Theme.Warning, "Còn 15–35% — nên đổ thêm"),
                    ColorLegend.Bar(Theme.Danger, "Còn 15% trở xuống — sắp hết"),
                });
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
            FilterMemory.Attach(filterBar, "vehicles"); // nhớ bộ lọc khi chuyển màn rồi quay lại
            SetupModernLook();
            SetupRowActions();
            // Ẩn / khóa sẵn các nút theo "chưa chọn dòng" TRƯỚC khi màn hiện — không còn cảnh hiện cả loạt nút rồi tải xong mới ẩn đi.
            btnEdit.Enabled = btnDocuments.Enabled = btnFuelLevel.Enabled = false;
            Load += async (_, _) => await LoadAsync();
        }

        /// <summary>Nhấp đúp 1 xe = mở form Sửa xe; chuột phải = menu Sửa xe / Giấy tờ xe / Nhiên liệu (cùng kiểu
        /// màn Chuyến, Đơn vận chuyển). Chuột phải chọn luôn đúng dòng dưới con trỏ.</summary>
        private void SetupRowActions()
        {
            // Thanh trên chỉ giữ việc hay làm: Thêm xe + Giấy tờ xe. "Sửa xe" = nhấp đúp / chuột phải, "Nhiên liệu trong
            // bình" ít dùng -> chỉ ở chuột phải (bớt nút trùng).
            // Giống màn Tài xế / Nguồn lực: nhấp đúp = XEM ĐẦY ĐỦ, sửa xe / giấy tờ / nhiên liệu ngay trong cửa sổ đó
            // (cửa sổ vẫn mở, tự cập nhật). Thanh nút chỉ còn Thêm xe + Làm mới; chuột phải: Xem đầy đủ / sửa + Lịch sử.
            btnEdit.Visible = false;
            btnFuelLevel.Visible = false;
            btnDocuments.Visible = false;
            grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) ShowDetail(); };

            var menu = new ContextMenuStrip();
            var detail = new ToolStripMenuItem("🔍  Xem đầy đủ / sửa xe, giấy tờ, nhiên liệu") { ShortcutKeyDisplayString = "Nhấp đúp", Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            detail.Click += (_, _) => ShowDetail();
            var history = new ToolStripMenuItem("🕘  Lịch sử thay đổi (ai làm gì, lúc nào)");
            history.Click += (_, _) => ShowHistory();
            menu.Items.AddRange(new ToolStripItem[] { detail, history });
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

        /// <summary>🕘 Nhật ký của xe đang chọn + giấy tờ xe (ai sửa trạng thái, biển số, gia hạn giấy tờ...).</summary>
        private void ShowHistory(IWin32Window? owner = null)
        {
            if (_api is null || grid.CurrentRow?.Cells["Id"]?.Value is not int id) return;
            var plate = _vehicles.FirstOrDefault(v => v.Id == id)?.PlateNumber ?? $"#{id}";
            using var f = new TruckingSystem.Desktop.GUI_AuditLogManagement.EntityHistoryForm(_api, "Vehicle", id, "xe " + plate);
            f.ShowDialog(owner ?? FindForm());
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
            return string.Join("   ·   ", parts) + "   —   nhấp đúp 1 xe để xem đầy đủ / sửa";
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

        /// <summary>Mở từ thông báo "Giấy tờ xe … sắp / đã hết hạn": tải xong thì mở THẲNG cửa sổ Giấy tờ xe của đúng xe.</summary>
        public void OpenDocumentsOnLoad(string plate)
        {
            FocusOn(plate);
            _pendingDocsPlate = plate;
        }

        private string? _pendingDocsPlate;

        /// <summary>Mở từ hộp sửa nhanh (Chia xe thiếu xe): chỉ hiện xe thiếu / hết hạn giấy tờ.</summary>
        public void ShowDocsBlocked()
        {
            FilterMemory.Cancel(filterBar);
            filterBar.SetOnly("docs", DocsBlocked);
        }

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

        // ================= Xem đầy đủ (nhấp đúp) + sửa ngay trong cửa sổ =================

        private void ShowDetail()
        {
            if (_api is null || grid.CurrentRow?.Cells["Id"]?.Value is not int id) return;
            VehicleListItem? V() => _vehicles.FirstOrDefault(x => x.Id == id); // lấy lại sau mỗi lần sửa
            var plate = V()?.PlateNumber ?? $"#{id}";
            using var f = new TruckingSystem.Desktop.GUI_ResourceManagement.ResourceDetailForm($"🚚 Xe {plate}",
                () => V() is not { } v
                    ? new[] { ("Không còn thấy xe này", "— có thể đã bị lọc / xóa") }
                    : new (string, string)[]
                    {
                        ("# Tình trạng", ""),
                        ("Trạng thái", TranslateStatus(v.Status)),
                        ("Giấy tờ", v.DocumentBlockReason is null ? "✅ Hợp lệ — chia xe được" : "⛔ " + v.DocumentBlockReason),
                        ("Nhiên liệu", FuelText(v) + (v.FuelLevelMeasuredAt is DateTime m ? $" · đo lúc {m.ToLocalTime():dd/MM HH:mm}" : "")),
                        ("# Thông tin xe", ""),
                        ("Biển số / loại", $"{v.PlateNumber} · {v.VehicleType}"),
                        ("Tải trọng", $"{v.CapacityKg / 1000m:0.##} tấn · {v.CapacityM3:0.##} m³ · {v.CapacityQty:0.##} đơn vị"),
                        ("Kho", v.WarehouseName ?? AnyWarehouse),
                        ("Tiêu hao", v.FuelConsumptionPer100Km is decimal c ? $"{c:0.#} L/100 km · {v.FuelType ?? "—"}" : "— chưa khai"),
                    },
                () => V() is not { } v ? Array.Empty<(string, bool, Func<IWin32Window, Task>)>()
                    : new (string, bool, Func<IWin32Window, Task>)[]
                    {
                        ("✏️ Sửa xe", true, owner => EditAsync(v, owner)),
                        ("📄 Giấy tờ xe", true, owner => DocumentsAsync(v, owner)),
                        ("⛽ Nhiên liệu", true, owner => FuelLevelAsync(v, owner)),
                        ("🕘 Lịch sử", true, owner => { ShowHistory(owner); return Task.CompletedTask; }),
                    });
            f.ShowDialog(FindForm());
        }

        private async Task EditAsync(VehicleListItem v, IWin32Window? owner)
        {
            if (_api is null) return;
            bool ok;
            using (var f = new EditVehicleForm(_api, v)) ok = f.ShowDialog(owner) == DialogResult.OK;
            if (ok) await LoadAsync();
        }

        private async Task DocumentsAsync(VehicleListItem v, IWin32Window? owner)
        {
            if (_api is null) return;
            using (var f = new ManageVehicleDocumentsForm(_api, v.Id, v.PlateNumber)) f.ShowDialog(owner);
            await LoadAsync(); // giấy tờ có thể vừa thêm/xóa -> cột "Giấy tờ" cập nhật
        }

        private async Task FuelLevelAsync(VehicleListItem v, IWin32Window? owner)
        {
            if (_api is null) return;
            bool ok;
            using (var f = new VehicleFuelLevelForm(_api, v.Id, v.PlateNumber)) ok = f.ShowDialog(owner) == DialogResult.OK;
            if (ok) await LoadAsync();
        }

        private void BtnDocuments_Click(object? sender, EventArgs e)
        {
            if (_api is null || grid.CurrentRow is null)
            {
                Toast.Show(this, "👆 " + "Vui lòng chọn 1 xe trước.", ToastKind.Info);
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
                Toast.Show(this, "👆 " + "Vui lòng chọn 1 xe trước.", ToastKind.Info);
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
                Toast.Show(this, "👆 " + "Vui lòng chọn 1 xe trước.", ToastKind.Info);
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
            _overlay.Loading = true;
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
            finally { _overlay.Loading = false; }
            if (_pendingDocsPlate is { } plate && _vehicles.FirstOrDefault(x => x.PlateNumber == plate) is { } target)
            {
                _pendingDocsPlate = null;
                BeginInvoke(new Action(async () => { if (!IsDisposed) await DocumentsAsync(target, FindForm()); }));
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