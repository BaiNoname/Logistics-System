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
            SetupRowMenu();
            Load += async (_, _) => await LoadAsync();
        }

        /// <summary>Chuột phải 1 tài xế = Hồ sơ / Ngày làm — cùng kiểu màn Đơn, Chuyến, Đội xe (trước đây màn này chưa có).
        /// Nhấp đúp vẫn mở hồ sơ. Chuột phải chọn luôn đúng dòng dưới con trỏ.</summary>
        private void SetupRowMenu()
        {
            var menu = new ContextMenuStrip();
            var profile = new ToolStripMenuItem("📝  Hồ sơ tài xế") { ShortcutKeyDisplayString = "Nhấp đúp", Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            profile.Click += async (_, _) => await OpenProfileAsync();
            var workDays = new ToolStripMenuItem("📅  Ngày làm việc");
            workDays.Click += (s, e) => BtnSchedule_Click(s, e);
            var refresh = new ToolStripMenuItem("🔄  Làm mới danh sách") { ShortcutKeyDisplayString = "F5" };
            refresh.Click += (_, _) => _ = LoadAsync();
            menu.Items.AddRange(new ToolStripItem[] { profile, workDays, new ToolStripSeparator(), refresh });
            ModernMenu.Apply(menu);
            menu.Opening += (_, e) =>
            {
                var d = Selected();
                if (d is null) { e.Cancel = true; return; }
                profile.Text = d.HasProfile ? "✏️  Sửa hồ sơ tài xế" : "📝  Tạo hồ sơ tài xế";
                workDays.Enabled = d.HasProfile;
                workDays.ToolTipText = d.HasProfile ? "" : "Chưa có hồ sơ tài xế — tạo hồ sơ trước rồi mới đăng ký ngày làm";
            };
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

        /// <summary>Nhãn màu trạng thái / hồ sơ. (Bỏ hàng 6 thẻ số liệu theo góp ý GVHD — số cần chú ý gom vào
        /// 1 dòng ngắn SummaryText; lọc nhanh dùng bộ lọc Sẵn sàng / Hồ sơ / Cảnh báo trên bảng.)</summary>
        private void SetupModernLook()
        {
            // Cửa sổ thấp: giữ bảng tối thiểu ~8 dòng, thiếu chỗ thì cả màn có thanh cuộn kéo xuống.
            ScrollFit.Enable(this, root, grid);

            GridDecor.HoverRows(grid);
            GridDecor.Pills(grid, "Trạng_thái", (text, _) => GridDecor.StatusTone(text));
            GridDecor.Pills(grid, "Hồ_sơ", (text, _) => GridDecor.StatusTone(text));
        }

        private readonly ToolTip _statusTip = new() { AutoPopDelay = 20000 };

        /// <summary>1 dòng tóm tắt thay cho 6 thẻ số liệu: tổng số tài xế + CHỈ những việc cần chú ý (có số > 0).</summary>
        private string SummaryText()
        {
            var parts = new List<string> { $"Tổng {_drivers.Count} tài xế" };
            void Add(int n, string text) { if (n > 0) parts.Add($"{text}: {n}"); }
            Add(_drivers.Count(d => d.Availability == "Sẵn sàng"), "🟢 sẵn sàng hôm nay");
            Add(_drivers.Count(d => d.Availability == "Đang làm"), "🚚 đang chạy chuyến");
            Add(_drivers.Count(d => !d.HasProfile), "📝 chưa có hồ sơ (chưa chia xe được)");
            Add(_drivers.Count(d => d.ProfileStatus == "Bằng lái sắp hết hạn"), "🪪 bằng lái sắp hết hạn");
            Add(_drivers.Count(d => d.ProfileStatus == "Bằng lái hết hạn"), "⛔ bằng lái hết hạn");
            Add(_drivers.Count(d => !string.IsNullOrEmpty(d.Warning)), "⚠️ cần lưu ý (xem cột Cảnh báo)");
            return string.Join("   ·   ", parts) + "   —   rê chuột vào dòng này để xem cách tính \"Sẵn sàng\"";
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
            try
            {
                _drivers = await _api.Drivers.GetDriversAsync();
                filterBar.SetChoiceOptions("warehouse", _drivers.Select(WarehouseText));
                BindGrid(selectEmployeeId);
                lblStatus.Text = SummaryText();
                lblStatus.ForeColor = _drivers.Any(d => !string.IsNullOrEmpty(d.Warning)) ? Theme.Warning : Theme.TextSecondary;
                // Giải thích dài trước đây nằm ở dòng trạng thái -> chuyển vào chú thích khi rê chuột.
                _statusTip.SetToolTip(lblStatus,
                    "Tài xế = nhân viên Đang làm có chức danh Tài xế (thêm mới ở màn Nhân viên).\n" +
                    "\"Sẵn sàng\" = có hồ sơ, bằng lái còn hạn, có ngày làm hôm nay; khi Chia xe còn xét kho đóng quân\n" +
                    "và chuyến dài nhiều ngày phải đăng ký đủ các ngày.");
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
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
