using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_ResourceManagement;
using TruckingSystem.Desktop.SERVICE_ResourceManagement;

namespace TruckingSystem.Desktop.GUI_ResourceManagement
{
    /// <summary>🧭 NGUỒN LỰC THEO KHO — 1 màn để nhân viên điều phối nhìn nhanh: kho này hôm nay
    /// có những XE nào, TÀI XẾ nào chạy được, cái nào đang vướng gì (giấy tờ thiếu / sắp hết hạn,
    /// chưa đăng ký ngày làm, đang bận chuyến...). Server tính theo ĐÚNG quy tắc Chia xe / Bắt đầu
    /// chuyến (chỉ đọc). Bấm 1 xe -> bảng Tài xế đánh dấu ai đủ hạng bằng lái xe đó (và ngược lại).
    /// Nút thao tác nhanh mở đúng các form có sẵn (Giấy tờ xe, Nhiên liệu, Hồ sơ tài xế, Ngày làm).
    /// Giao diện tĩnh ở ResourcesControl.Designer.cs.</summary>
    public partial class ResourcesControl : BaseUserControl
    {
        private const int ExpiringDays = 30;
        private ApiServices? _api;
        private ResourceApiService? _service;
        private ResourceOverview _data = new();
        private bool _loadingWarehouses;
        private int _loadId;

        public ResourcesControl()
        {
            InitializeComponent();
            Theme.StyleGrid(gridVehicles);
            Theme.StyleGrid(gridDrivers);
            gridVehicles.DataBindingComplete += (_, _) => { if (gridVehicles.Columns["Id"] is { } c) c.Visible = false; UpdateButtons(); };
            gridDrivers.DataBindingComplete += (_, _) => { if (gridDrivers.Columns["EmployeeId"] is { } c) c.Visible = false; UpdateButtons(); };
            Load += async (_, _) => await LoadAsync();
        }

        public ResourcesControl(ApiServices api) : this()
        {
            _api = api;
            _service = new ResourceApiService(api.Connection);
        }

        // ================= Tải dữ liệu =================

        private int? SelectedWarehouseId => cboWarehouse.SelectedItem is ResourceWarehouse w && w.Id != 0 ? w.Id : null;

        private async Task LoadAsync()
        {
            if (_service is null) return;
            var loadId = ++_loadId;
            lblStatus.Text = "Đang tải...";
            lblStatus.ForeColor = Theme.TextSecondary;
            try
            {
                var data = await _service.GetOverviewAsync(SelectedWarehouseId);
                if (IsDisposed || loadId != _loadId) return; // đã đổi kho tiếp -> bỏ kết quả cũ
                _data = data;
                BindWarehouses();
                BindSummary();
                BindVehicles();
                BindDrivers();
                lblStatus.Text = $"Cập nhật lúc {DateTime.Now:HH:mm} — \"Chạy được hôm nay\" tính đúng như Chia xe: xe Rảnh + đủ 3 giấy tờ còn hạn; " +
                                 "tài xế có hồ sơ, bằng lái / khám sức khỏe còn hạn, đã đăng ký ngày làm hôm nay, không bận chuyến khác.";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi tải dữ liệu: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }

        private void BindWarehouses()
        {
            var keep = SelectedWarehouseId;
            _loadingWarehouses = true;
            try
            {
                var items = new List<ResourceWarehouse> { new() { Id = 0, Name = "Tất cả kho" } };
                items.AddRange(_data.Warehouses);
                cboWarehouse.DataSource = items;
                cboWarehouse.DisplayMember = nameof(ResourceWarehouse.Name);
                var idx = keep is int k ? items.FindIndex(w => w.Id == k) : 0;
                cboWarehouse.SelectedIndex = idx >= 0 ? idx : 0;
            }
            finally { _loadingWarehouses = false; }
        }

        private void BindSummary()
        {
            var s = _data.Summary;
            lblSumVehicles.Text = $"🚚 Xe chạy được hôm nay\n{s.VehiclesReady} / {s.VehiclesTotal}";
            lblSumVehicles.ForeColor = s.VehiclesReady == 0 && s.VehiclesTotal > 0 ? Theme.Danger : Theme.TextPrimary;
            lblSumDrivers.Text = $"👷 Tài xế chạy được hôm nay\n{s.DriversReady} / {s.DriversTotal}";
            lblSumDrivers.ForeColor = s.DriversReady == 0 && s.DriversTotal > 0 ? Theme.Danger : Theme.TextPrimary;
            lblSumDocs.Text = $"📄 Giấy tờ: ⛔ {s.DocsExpiredOrMissing} thiếu / hết hạn\n🟡 {s.DocsExpiringSoon} sắp hết hạn ({ExpiringDays} ngày)";
            lblSumDocs.ForeColor = s.DocsExpiredOrMissing > 0 ? Theme.Danger : s.DocsExpiringSoon > 0 ? Theme.Warning : Theme.Success;
            lblSumTrips.Text = $"🛣️ Chuyến hôm nay\n{s.TripsStartingToday} sẽ xuất phát · {s.TripsRunning} đang chạy";
        }

        // ================= Hiển thị =================

        /// <summary>Ô giấy tờ: "✅ 12/05/2027" / "🟡 20/10/2026 (còn 6 ngày)" / "⛔ Hết hạn 01/10/2026" / "⛔ Chưa nhập".</summary>
        private static string DocText(DateTime? expiry, bool required = true)
        {
            if (expiry is not DateTime d) return required ? "⛔ Chưa nhập" : "— chưa nhập";
            var days = (d.Date - DateTime.Today).Days;
            if (days < 0) return $"⛔ Hết hạn {d:dd/MM/yyyy}";
            if (days <= ExpiringDays) return $"🟡 {d:dd/MM/yyyy} (còn {days} ngày)";
            return $"✅ {d:dd/MM/yyyy}";
        }

        private static string VehicleStatusText(string s) => s switch
        {
            "Available" => "Rảnh",
            "OnTrip" => "Đang chạy",
            "UnderMaintenance" => "Bảo trì",
            "OutOfService" => "Ngưng hoạt động",
            _ => s,
        };

        private static string FuelText(VehicleResource v)
            => v.FuelCurrentLiters is not decimal cur ? "— chưa khai"
             : v.FuelTankCapacityLiters is decimal cap && cap > 0 ? $"{(cur / cap <= 0.15m ? "⚠️ " : "")}~{cur:0} / {cap:0} L" : $"~{cur:0} L";

        private static bool VehicleHasProblem(VehicleResource v) => !v.CanRunToday || v.ExpiringSoonCount > 0;
        private static bool DriverHasProblem(DriverResource d) => !d.CanRunToday || d.ExpiringSoonCount > 0;

        private void BindVehicles()
        {
            var keep = SelectedVehicle()?.Id;
            var rows = _data.Vehicles.Where(v => !chkVehicleProblems.Checked || VehicleHasProblem(v)).ToList();
            gridVehicles.DataSource = rows.Select(v => new
            {
                v.Id,
                Biển_số = v.PlateNumber,
                Loại_xe = $"{v.VehicleType} ({v.CapacityKg / 1000m:0.##} tấn)",
                Cần_bằng = v.RequiredLicense,
                Kho = v.WarehouseName ?? "(mọi kho)",
                Trạng_thái = VehicleStatusText(v.Status),
                Đăng_kiểm = DocText(v.RegistrationExpiry),
                Bảo_hiểm = DocText(v.InsuranceExpiry),
                Phù_hiệu = DocText(v.PermitExpiry),
                Nhiên_liệu = FuelText(v),
                Chuyến_hiện_tại = v.CurrentTrip ?? "—",
                Chạy_được_hôm_nay = v.CanRunToday ? "✅ Được" : "⛔ " + v.BlockReason,
                Tài_xế_đang_chọn = "", // điền khi chọn 1 tài xế (CellFormatting)
            }).ToList();
            grpVehicles.Text = $"🚚 Xe ({rows.Count}/{_data.Vehicles.Count})";
            Reselect(gridVehicles, "Id", keep, "Biển_số");
        }

        private void BindDrivers()
        {
            var keep = SelectedDriver()?.EmployeeId;
            var rows = _data.Drivers.Where(d => !chkDriverProblems.Checked || DriverHasProblem(d)).ToList();
            gridDrivers.DataSource = rows.Select(d => new
            {
                d.EmployeeId,
                Mã_NV = d.Code,
                Họ_tên = d.FullName,
                Điện_thoại = d.Phone ?? "—",
                Kho = !d.HasProfile ? "—" : d.WarehouseName ?? "(mọi kho)",
                Hạng_bằng = d.LicenseClass ?? "—",
                Bằng_lái = d.HasProfile ? DocText(d.LicenseExpiry) : "⛔ Chưa có hồ sơ",
                Khám_sức_khỏe = d.HasProfile ? DocText(d.HealthCertExpiry, required: false) : "—",
                Ngày_làm = (d.WorksToday ? "✅ Hôm nay" : "❌ Hôm nay chưa ĐK") + $" · 7 ngày tới: {d.WorkDaysNext7}",
                Chuyến_hiện_tại = d.CurrentTrip is null ? "—" : d.CurrentTrip + (d.CurrentVehiclePlate is null ? "" : $" · xe {d.CurrentVehiclePlate}"),
                Chạy_được_hôm_nay = d.CanRunToday ? "✅ Được" : "⛔ " + d.BlockReason,
                Xe_đang_chọn = "", // điền khi chọn 1 xe (CellFormatting)
            }).ToList();
            grpDrivers.Text = $"👷 Tài xế ({rows.Count}/{_data.Drivers.Count})";
            Reselect(gridDrivers, "EmployeeId", keep, "Mã_NV");
        }

        private static void Reselect(DataGridView grid, string idColumn, int? id, string visibleColumn)
        {
            grid.ClearSelection();
            if (id is not int want) { grid.CurrentCell = null; return; }
            foreach (DataGridViewRow r in grid.Rows)
                if (r.Cells[idColumn].Value is int v && v == want) { grid.CurrentCell = r.Cells[visibleColumn]; r.Selected = true; return; }
            grid.CurrentCell = null;
        }

        private VehicleResource? SelectedVehicle()
            => gridVehicles.CurrentRow?.Cells["Id"].Value is int id && gridVehicles.CurrentRow.Selected
                ? _data.Vehicles.FirstOrDefault(v => v.Id == id) : null;

        private DriverResource? SelectedDriver()
            => gridDrivers.CurrentRow?.Cells["EmployeeId"].Value is int id && gridDrivers.CurrentRow.Selected
                ? _data.Drivers.FirstOrDefault(d => d.EmployeeId == id) : null;

        private void UpdateButtons()
        {
            btnVehicleDocs.Enabled = btnVehicleFuel.Enabled = SelectedVehicle() is not null;
            var d = SelectedDriver();
            btnDriverProfile.Enabled = d is not null;
            btnDriverWorkDays.Enabled = d is { HasProfile: true };
            btnDriverProfile.Text = d is { HasProfile: false } ? "📝 Tạo hồ sơ tài xế" : "📝 Hồ sơ tài xế";
        }

        // ================= Ghép xe – tài xế (theo hạng bằng) =================

        private void GridVehicles_SelectionChanged(object? sender, EventArgs e)
        {
            UpdateButtons();
            gridDrivers.Invalidate(); // cột "Xe đang chọn" + tô màu dòng tài xế lái được
            var v = SelectedVehicle();
            if (v is not null)
            {
                var can = _data.Drivers.Count(d => d.DrivableVehicleIds.Contains(v.Id));
                var ready = _data.Drivers.Count(d => d.DrivableVehicleIds.Contains(v.Id) && d.CanRunToday);
                lblHint.Text = $"Xe {v.PlateNumber} cần bằng {v.RequiredLicense}: {can} tài xế đủ hạng bằng, trong đó {ready} người chạy được hôm nay (tô xanh ở bảng Tài xế).";
            }
        }

        private void GridDrivers_SelectionChanged(object? sender, EventArgs e)
        {
            UpdateButtons();
            gridVehicles.Invalidate();
            var d = SelectedDriver();
            if (d is not null)
                lblHint.Text = d.LicenseClass is null
                    ? $"{d.FullName} chưa có hồ sơ / hạng bằng — chưa lái được xe nào."
                    : $"{d.FullName} (bằng {d.LicenseClass}) lái được {d.DrivableVehicleIds.Count} xe trong danh sách (tô xanh ở bảng Xe).";
        }

        private void GridVehicles_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.CellStyle is null) return;
            var name = gridVehicles.Columns[e.ColumnIndex].Name;
            if (name == "Tài_xế_đang_chọn")
            {
                var d = SelectedDriver();
                var vid = gridVehicles.Rows[e.RowIndex].Cells["Id"].Value as int?;
                if (d is null || vid is null) { e.Value = "—"; e.CellStyle.ForeColor = Theme.TextSecondary; }
                else if (d.DrivableVehicleIds.Contains(vid.Value)) { e.Value = "✔ Lái được"; e.CellStyle.ForeColor = Theme.Success; e.CellStyle.Font = Theme.FontBold; }
                else { e.Value = "✖ Không đủ hạng bằng"; e.CellStyle.ForeColor = Theme.TextSecondary; }
                e.FormattingApplied = true;
                return;
            }
            ColorStatusCell(name, e);
        }

        private void GridDrivers_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.CellStyle is null) return;
            var name = gridDrivers.Columns[e.ColumnIndex].Name;
            if (name == "Xe_đang_chọn")
            {
                var v = SelectedVehicle();
                var did = gridDrivers.Rows[e.RowIndex].Cells["EmployeeId"].Value as int?;
                var driver = did is null ? null : _data.Drivers.FirstOrDefault(x => x.EmployeeId == did);
                if (v is null || driver is null) { e.Value = "—"; e.CellStyle.ForeColor = Theme.TextSecondary; }
                else if (driver.DrivableVehicleIds.Contains(v.Id)) { e.Value = "✔ Lái được"; e.CellStyle.ForeColor = Theme.Success; e.CellStyle.Font = Theme.FontBold; }
                else { e.Value = "✖ Không đủ hạng bằng"; e.CellStyle.ForeColor = Theme.TextSecondary; }
                e.FormattingApplied = true;
                return;
            }
            ColorStatusCell(name, e);
        }

        /// <summary>Tô màu ô theo ký hiệu đầu chữ: ✅ xanh, 🟡 cam, ⛔ / ❌ đỏ.</summary>
        private static void ColorStatusCell(string column, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value is not string text || e.CellStyle is null) return;
            if (text.StartsWith("✅")) e.CellStyle.ForeColor = Theme.Success;
            else if (text.StartsWith("🟡") || text.StartsWith("⚠️")) e.CellStyle.ForeColor = Theme.Warning;
            else if (text.StartsWith("⛔") || text.StartsWith("❌")) e.CellStyle.ForeColor = Theme.Danger;
            if (column is "Chạy_được_hôm_nay") e.CellStyle.Font = Theme.FontBold;
        }

        // ================= Thao tác =================

        private void CboWarehouse_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_loadingWarehouses) return;
            _ = LoadAsync();
        }

        private void BtnRefresh_Click(object? sender, EventArgs e) => _ = LoadAsync();

        private void ProblemsOnly_CheckedChanged(object? sender, EventArgs e)
        {
            if (sender == chkVehicleProblems) BindVehicles(); else BindDrivers();
        }

        private async void BtnVehicleDocs_Click(object? sender, EventArgs e)
        {
            if (_api is null || SelectedVehicle() is not VehicleResource v) return;
            using var f = new TruckingSystem.Desktop.GUI_VehicleManagement.ManageVehicleDocumentsForm(_api, v.Id, v.PlateNumber);
            f.ShowDialog(FindForm());
            await LoadAsync();
        }

        private async void BtnVehicleFuel_Click(object? sender, EventArgs e)
        {
            if (_api is null || SelectedVehicle() is not VehicleResource v) return;
            using var f = new TruckingSystem.Desktop.GUI_VehicleManagement.VehicleFuelLevelForm(_api, v.Id, v.PlateNumber);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
        }

        private async void BtnDriverProfile_Click(object? sender, EventArgs e)
        {
            if (_api is null || SelectedDriver() is not DriverResource d) return;
            using var f = new TruckingSystem.Desktop.GUI_EmployeeManagement.CreateDriverProfileForm(_api, d.EmployeeId, d.FullName);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
        }

        private async void BtnDriverWorkDays_Click(object? sender, EventArgs e)
        {
            if (_api is null || SelectedDriver() is not DriverResource d) return;
            using var f = new TruckingSystem.Desktop.GUI_DriverManagement.DriverWorkDaysForm(_api, d.EmployeeId, d.FullName);
            f.ShowDialog(FindForm());
            if (f.Changed) await LoadAsync();
        }
    }
}
