using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_DriverManagement;

namespace TruckingSystem.Desktop.GUI_DriverManagement
{
    /// <summary>Danh sách TÀI XẾ RIÊNG (tách khỏi màn Nhân viên) — mục đích
    /// chính: xem nhanh ai đang "Đang làm / Sẵn sàng / Không sẵn sàng" để
    /// đối chiếu TRƯỚC khi bấm "Chia xe" cho 1 đơn, vì cột Trạng thái ở đây
    /// tính ĐÚNG Y HỆT điều kiện thật mà thuật toán Greedy dùng.
    /// CHỈ XEM — việc tạo hồ sơ tài xế / đăng ký lịch làm việc vẫn thao tác
    /// ở màn Nhân viên (EmployeesControl) như cũ, không lặp lại ở đây.</summary>
    public partial class DriversControl : BaseUserControl
    {
        private ApiServices? _api;

        public DriversControl()
        {
            InitializeComponent();
            Theme.StyleGrid(grid);
            Resize += (_, _) => { grid.Width = Width; grid.Height = Height - grid.Top; };
            grid.DataBindingComplete += (_, _) => { if (grid.Columns["EmployeeId"] != null) grid.Columns["EmployeeId"]!.Visible = false; };
            grid.CellFormatting += Grid_CellFormatting;
            Load += async (_, _) => await LoadAsync();
        }

        public DriversControl(ApiServices api) : this()
        {
            _api = api;
        }

        private void BtnRefresh_Click(object? sender, EventArgs e) => _ = LoadAsync();

        private async Task LoadAsync()
        {
            if (_api is null) return;
            lblStatus.Text = "Đang tải...";
            try
            {
                var drivers = await _api.Drivers.GetDriversAsync();
                grid.DataSource = drivers.Select(d => new
                {
                    d.EmployeeId,
                    Mã_NV = d.Code,
                    Họ_tên = d.FullName,
                    Điện_thoại = d.Phone ?? "—",
                    Số_bằng_lái = d.LicenseNumber,
                    Hạng = d.LicenseClass,
                    Hạn_bằng_lái = d.LicenseExpiryDate.ToString("dd/MM/yyyy"),
                    Kinh_nghiệm = $"{d.YearsOfExperience} năm",
                    Kho_đóng_quân = d.WarehouseName ?? "— (mọi kho)",
                    Trạng_thái = d.Availability,
                }).ToList();
                lblStatus.Text = $"Tổng {drivers.Count} tài xế — Trạng thái khớp đúng điều kiện thật dùng khi Chia xe";
                lblStatus.ForeColor = Theme.TextSecondary;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }

        /// <summary>Tô màu cột Trạng thái cho dễ nhìn ngay — không cần đọc
        /// chữ: xanh lá = sẵn sàng chia xe ngay, vàng = đang bận, đỏ = thiếu
        /// đăng ký lịch, chưa thể chọn khi chia xe dù có đang rảnh thật.</summary>
        private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (grid.Columns[e.ColumnIndex].Name != "Trạng_thái" || e.Value is null) return;

            e.CellStyle!.ForeColor = e.Value.ToString() switch
            {
                "Sẵn sàng" => Theme.Success,
                "Đang làm" => Theme.Warning,
                "Không sẵn sàng" => Theme.Danger,
                _ => Theme.TextSecondary,
            };
            e.CellStyle.Font = new Font(grid.Font, FontStyle.Bold);
        }
    }
}