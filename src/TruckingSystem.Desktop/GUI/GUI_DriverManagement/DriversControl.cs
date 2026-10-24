using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_DriverManagement;
using TruckingSystem.Desktop.GUI_EmployeeManagement;

namespace TruckingSystem.Desktop.GUI_DriverManagement
{
    /// <summary>Danh sách TÀI XẾ — lấy từ nhân viên ĐANG LÀM có CHỨC DANH Tài xế (khai báo ở
    /// màn Nhân viên), không còn nút "Thêm tài xế" chọn nhân viên bất kỳ. Muốn có tài xế mới:
    /// tạo nhân viên với chức danh Tài xế bên màn Nhân viên, người đó tự xuất hiện ở đây.
    /// Chọn 1 người -> "Hồ sơ tài xế": chưa có hồ sơ thì Tạo, có rồi thì Sửa giấy tờ (dùng lại
    /// CreateDriverProfileForm — form tự nhận chế độ Tạo/Sửa). Chưa có hồ sơ = chưa Chia xe được.
    /// Cột Trạng thái tính ĐÚNG Y HỆT điều kiện thật mà thuật toán Chia xe (Greedy) dùng.
    /// Ngày làm việc dùng DriverWorkDaysForm (đăng ký theo ngày, chỉ khi đã có hồ sơ).</summary>
    public partial class DriversControl : BaseUserControl
    {
        private ApiServices? _api;
        private List<DriverListItem> _drivers = new();

        public DriversControl()
        {
            InitializeComponent();
            Theme.StyleGrid(grid);
            GridLayout.FitColumns(grid); // cột vừa nội dung, nhiều cột thì cuộn ngang thay vì cắt chữ
            Load += (_, _) => Theme.UniformButtons(toolbar); // nút đều nhau, xuống dòng vẫn thẳng cột
            grid.DataBindingComplete += (_, _) =>
            {
                if (grid.Columns["EmployeeId"] != null) grid.Columns["EmployeeId"]!.Visible = false;
                UpdateButtons();
            };
            grid.CellFormatting += Grid_CellFormatting;
            grid.SelectionChanged += (_, _) => UpdateButtons();
            filterBar.AddChoice("availability", "Trạng thái", new[] { "Sẵn sàng", "Đang làm", "Không sẵn sàng" });
            filterBar.AddChoice("profile", "Hồ sơ", new[] { "Đủ", "Bằng lái sắp hết hạn", "Bằng lái hết hạn", "Chưa có hồ sơ" });
            filterBar.AddChoice("warning", "Cảnh báo", new[] { WarnYes, WarnNo });
            filterBar.AddChoice("warehouse", "Kho", Array.Empty<string>());
            filterBar.FilterChanged += (_, _) => BindGrid();
            SetupModernLook();
            Load += async (_, _) => await LoadAsync();
        }

        private readonly KpiStrip _kpi = new();

        /// <summary>Thẻ số liệu tài xế (bấm = lọc nhanh) + nhãn màu trạng thái / hồ sơ.</summary>
        private void SetupModernLook()
        {
            _kpi.AddCard("all", "🧑", "Tổng tài xế", Theme.Primary);
            _kpi.AddCard("ready", "🟢", "Sẵn sàng hôm nay", Theme.Success, "availability", "Sẵn sàng");
            _kpi.AddCard("busy", "🚚", "Đang chạy chuyến", Theme.PrimaryDark, "availability", "Đang làm");
            _kpi.AddCard("notready", "⏸️", "Chưa sẵn sàng", GridDecor.AccentSlate, "availability", "Không sẵn sàng");
            _kpi.AddCard("license", "🪪", "Bằng lái sắp hết hạn", Theme.Warning, "profile", "Bằng lái sắp hết hạn", alert: true);
            _kpi.AddCard("warn", "⚠️", "Cần lưu ý", Theme.Danger, "warning", WarnYes, alert: true);
            _kpi.BindFilter(filterBar);
            KpiStrip.InsertRow(root, _kpi, 1);
            // Cửa sổ thấp: giữ bảng tối thiểu ~8 dòng, thiếu chỗ thì cả màn có thanh cuộn kéo xuống.
            ScrollFit.Enable(this, root, grid);

            GridDecor.HoverRows(grid);
            GridDecor.Pills(grid, "Trạng_thái", (text, _) => GridDecor.StatusTone(text));
            GridDecor.Pills(grid, "Hồ_sơ", (text, _) => GridDecor.StatusTone(text));
        }

        private void UpdateKpis()
        {
            var withProfile = _drivers.Count(d => d.HasProfile);
            _kpi.SetValue("all", _drivers.Count, $"{withProfile} người đã có hồ sơ",
                _drivers.Count == 0 ? 0 : withProfile / (double)_drivers.Count);
            var ready = _drivers.Count(d => d.Availability == "Sẵn sàng");
            _kpi.SetValue("ready", ready, "có ngày làm hôm nay, giấy tờ còn hạn",
                _drivers.Count == 0 ? 0 : ready / (double)_drivers.Count);
            _kpi.SetValue("busy", _drivers.Count(d => d.Availability == "Đang làm"), "đang có chuyến chưa xong");
            _kpi.SetValue("notready", _drivers.Count(d => d.Availability == "Không sẵn sàng"),
                $"{_drivers.Count(d => !d.HasProfile)} chưa có hồ sơ");
            _kpi.SetValue("license", _drivers.Count(d => d.ProfileStatus == "Bằng lái sắp hết hạn"),
                $"{_drivers.Count(d => d.ProfileStatus == "Bằng lái hết hạn")} đã hết hạn");
            _kpi.SetValue("warn", _drivers.Count(d => !string.IsNullOrEmpty(d.Warning)), "xem cột Cảnh báo");
        }

        private const string WarnYes = "Có cảnh báo";
        private const string WarnNo = "Không";
        private const string AnyWarehouse = "— (mọi kho)";

        private static string WarehouseText(DriverListItem d) => !d.HasProfile ? "—" : d.WarehouseName ?? AnyWarehouse;

        /// <summary>Đổ lưới theo bộ lọc, giữ dòng đang chọn.</summary>
        /// <summary>Mở từ thông báo (nhấp đúp): bỏ các bộ lọc khác, lọc sẵn theo mã tài xế.</summary>
        public void FocusOn(string search) => filterBar.ShowOnlySearch(search);

        private void BindGrid(int? selectEmployeeId = null)
        {
            selectEmployeeId ??= Selected()?.EmployeeId;
            var rows = _drivers.Where(d =>
                    filterBar.Is("availability", d.Availability) &&
                    filterBar.Is("profile", d.ProfileStatus) &&
                    filterBar.Is("warning", string.IsNullOrEmpty(d.Warning) ? WarnNo : WarnYes) &&
                    filterBar.Is("warehouse", WarehouseText(d)) &&
                    filterBar.MatchesSearch(d.Code, d.FullName, d.Phone, d.LicenseNumber, d.LicenseClass, d.PositionName, d.WarehouseName))
                .ToList();
            grid.DataSource = rows.Select(d => new
            {
                d.EmployeeId,
                Mã_NV = d.Code,
                Họ_tên = d.FullName,
                Điện_thoại = d.Phone ?? "—",
                Chức_danh = d.PositionName ?? "—",
                Hồ_sơ = d.ProfileStatus,
                Số_bằng_lái = d.LicenseNumber ?? "—",
                Hạng = d.LicenseClass ?? "—",
                Hạn_bằng_lái = d.LicenseExpiryDate?.ToString("dd/MM/yyyy") ?? "—",
                Kinh_nghiệm = d.YearsOfExperience is int y ? $"{y} năm" : "—",
                Kho_đóng_quân = WarehouseText(d),
                Trạng_thái = d.Availability,
                Cảnh_báo = d.Warning ?? "",
            }).ToList();
            filterBar.SetCount(rows.Count, _drivers.Count);

            if (selectEmployeeId is int sel)
            {
                foreach (DataGridViewRow row in grid.Rows)
                {
                    if (row.Cells["EmployeeId"].Value is int id && id == sel)
                    {
                        grid.CurrentCell = row.Cells["Mã_NV"];
                        break;
                    }
                }
            }
            UpdateButtons();
        }

        public DriversControl(ApiServices api) : this()
        {
            _api = api;
        }

        private void BtnRefresh_Click(object? sender, EventArgs e) => _ = LoadAsync();

        /// <summary>Nút hồ sơ đổi chữ theo dòng đang chọn: chưa có hồ sơ -> "Tạo hồ sơ",
        /// có rồi -> "Sửa hồ sơ". Ngày làm chỉ bấm được khi đã có hồ sơ (API cũng chặn).</summary>
        private void UpdateButtons()
        {
            var d = Selected();
            btnEditProfile.Enabled = d is not null;
            btnEditProfile.Text = d is null ? "📝 Hồ sơ tài xế"
                : d.HasProfile ? "✏️ Sửa hồ sơ" : "📝 Tạo hồ sơ";
            btnSchedule.Enabled = d is { HasProfile: true };
        }

        /// <summary>Tạo / Sửa hồ sơ tài xế cho người ĐANG CHỌN — dùng lại đúng form cũ,
        /// form tự nhận ra đã có hồ sơ hay chưa để chuyển chế độ.</summary>
        private async void BtnEditProfile_Click(object? sender, EventArgs e) => await OpenProfileAsync();

        private async void Grid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            await OpenProfileAsync();
        }

        private async Task OpenProfileAsync()
        {
            if (_api is null) return;
            var d = SelectedOrWarn();
            if (d is null) return;

            using var f = new CreateDriverProfileForm(_api, d.EmployeeId, d.FullName);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync(d.EmployeeId);
        }

        /// <summary>Ngày làm việc của tài xế — đăng ký THEO NGÀY (không chọn ca),
        /// form riêng của màn Tài xế, không dùng form lịch ca bên màn Nhân viên.</summary>
        private void BtnSchedule_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;
            var d = SelectedOrWarn();
            if (d is null) return;
            if (!d.HasProfile)
            {
                MessageBox.Show(FindForm(), $"{d.FullName} chưa có hồ sơ tài xế — bấm \"Tạo hồ sơ\" trước rồi mới đăng ký ngày làm.",
                    "Chưa có hồ sơ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var f = new DriverWorkDaysForm(_api, d.EmployeeId, d.FullName);
            f.ShowDialog(FindForm());
            if (f.Changed) _ = LoadAsync(d.EmployeeId); // Availability có thể đổi (Sẵn sàng/Không sẵn sàng) sau khi thêm/xóa ngày
        }

        private DriverListItem? Selected()
        {
            if (grid.CurrentRow?.Cells["EmployeeId"].Value is not int id) return null;
            return _drivers.FirstOrDefault(d => d.EmployeeId == id);
        }

        private DriverListItem? SelectedOrWarn()
        {
            var d = Selected();
            if (d is null)
                MessageBox.Show(FindForm(), "Vui lòng chọn 1 tài xế trong danh sách trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return d;
        }

        /// <param name="selectEmployeeId">Chọn lại đúng người vừa thao tác sau khi tải lại.</param>
        private async Task LoadAsync(int? selectEmployeeId = null)
        {
            if (_api is null) return;
            selectEmployeeId ??= Selected()?.EmployeeId;
            lblStatus.Text = "Đang tải...";
            lblStatus.ForeColor = Theme.TextSecondary;
            _kpi.Busy = true;
            try
            {
                _drivers = await _api.Drivers.GetDriversAsync();
                filterBar.SetChoiceOptions("warehouse", _drivers.Select(WarehouseText));
                BindGrid(selectEmployeeId);
                UpdateKpis();

                var noProfile = _drivers.Count(d => !d.HasProfile);
                var warned = _drivers.Count(d => !string.IsNullOrEmpty(d.Warning));
                lblStatus.Text = $"Tổng {_drivers.Count} tài xế (nhân viên Đang làm có chức danh Tài xế)" +
                                 (noProfile > 0 ? $" — {noProfile} người chưa có hồ sơ (chưa Chia xe được)" : "") +
                                 (warned > 0 ? $" — {warned} người cần lưu ý (xem cột Cảnh báo)" : "") +
                                 ". Thêm tài xế mới: tạo nhân viên với chức danh Tài xế ở màn Nhân viên. " +
                                 "\"Sẵn sàng\" = có hồ sơ, bằng lái còn hạn, có ngày làm hôm nay; khi Chia xe còn xét kho đóng quân " +
                                 "và chuyến dài nhiều ngày phải đăng ký đủ các ngày.";
                lblStatus.ForeColor = Theme.TextSecondary;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
            finally { _kpi.Busy = false; }
        }

        private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            // Trạng thái / Hồ sơ vẽ thành nhãn màu (GridDecor.Pills) — ở đây chỉ còn tô cột Cảnh báo.
            if (e.Value is null || e.CellStyle is null) return;
            if (grid.Columns[e.ColumnIndex].Name == "Cảnh_báo" && !string.IsNullOrEmpty(e.Value.ToString()))
                e.CellStyle.ForeColor = Theme.Warning;
        }
    }
}
