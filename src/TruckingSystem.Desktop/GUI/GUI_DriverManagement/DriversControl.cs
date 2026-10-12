using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_DriverManagement;
using TruckingSystem.Desktop.DTO_EmployeeManagement;
using TruckingSystem.Desktop.GUI_EmployeeManagement;

namespace TruckingSystem.Desktop.GUI_DriverManagement
{
    /// <summary>Danh sách TÀI XẾ RIÊNG (tách khỏi màn Nhân viên) — mục đích
    /// chính: xem nhanh ai đang "Đang làm / Sẵn sàng / Không sẵn sàng" để
    /// đối chiếu TRƯỚC khi bấm "Chia xe" cho 1 đơn, vì cột Trạng thái ở đây
    /// tính ĐÚNG Y HỆT điều kiện thật mà thuật toán Greedy dùng.
    /// Tái dùng form hồ sơ tài xế có sẵn từ màn Nhân viên
    /// CreateDriverProfileForm; ngày làm việc dùng DriverWorkDaysForm (đăng ký
    /// theo ngày, không chọn ca) — chỉ gọi đúng employeeId đang chọn trong lưới này.</summary>
    public partial class DriversControl : BaseUserControl
    {
        private ApiServices? _api;
        private List<DriverListItem> _drivers = new();

        public DriversControl()
        {
            InitializeComponent();
            Theme.StyleGrid(grid);
            grid.DataBindingComplete += (_, _) => { if (grid.Columns["EmployeeId"] != null) grid.Columns["EmployeeId"]!.Visible = false; };
            grid.CellFormatting += Grid_CellFormatting;
            // Hồ sơ / Lịch làm việc chỉ bấm được khi đã chọn 1 tài xế
            grid.SelectionChanged += (_, _) => btnEditProfile.Enabled = btnSchedule.Enabled = grid.CurrentRow is not null;
            grid.DataBindingComplete += (_, _) => btnEditProfile.Enabled = btnSchedule.Enabled = grid.CurrentRow is not null;
            Load += async (_, _) => await LoadAsync();
        }

        public DriversControl(ApiServices api) : this()
        {
            _api = api;
        }

        private void BtnRefresh_Click(object? sender, EventArgs e) => _ = LoadAsync();

        /// <summary>Thêm tài xế MỚI = chọn 1 nhân viên CHƯA phải tài xế, rồi
        /// mở đúng form tạo hồ sơ tài xế đã có sẵn (CreateDriverProfileForm)
        /// — form đó tự lo phần tạo DriverProfile, ở đây chỉ cần chọn ĐÚNG
        /// nhân viên để truyền employeeId vào.</summary>
        private async void BtnAddDriver_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;

            List<EmployeeListItem> allEmployees;
            try
            {
                allEmployees = await _api.Employees.GetEmployeesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(FindForm(), "Lỗi tải danh sách nhân viên: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Nhân viên ĐÃ NGHỈ VIỆC không được làm tài xế mới (Chia xe cũng chỉ chọn nhân viên đang làm).
            var candidates = allEmployees.Where(x => !x.IsDriver && x.Status != "Terminated").ToList();
            if (candidates.Count == 0)
            {
                MessageBox.Show(FindForm(),
                    "Không còn nhân viên nào (đang làm việc) chưa là tài xế.\nCần vào màn Nhân viên tạo nhân viên mới trước, rồi quay lại đây thêm hồ sơ tài xế cho người đó.",
                    "Không còn ai để thêm", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var picker = new PickEmployeeForDriverForm(candidates);
            if (picker.ShowDialog(FindForm()) != DialogResult.OK || picker.SelectedEmployeeId is null) return;

            var employeeId = picker.SelectedEmployeeId.Value;
            var fullName = candidates.First(x => x.Id == employeeId).FullName;

            using var f = new CreateDriverProfileForm(_api, employeeId, fullName);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
        }

        /// <summary>Sửa hồ sơ tài xế ĐANG CHỌN — dùng lại ĐÚNG form cũ,
        /// form này tự nhận ra đã có hồ sơ nên tự chuyển sang chế độ Sửa.</summary>
        private async void BtnEditProfile_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;
            if (!TryGetSelected(out var employeeId, out var fullName)) return;

            using var f = new CreateDriverProfileForm(_api, employeeId, fullName);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
        }

        /// <summary>Ngày làm việc của tài xế — đăng ký THEO NGÀY (không chọn ca),
        /// form riêng của màn Tài xế, không dùng form lịch ca bên màn Nhân viên.</summary>
        private void BtnSchedule_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;
            if (!TryGetSelected(out var employeeId, out var fullName)) return;

            using var f = new DriverWorkDaysForm(_api, employeeId, fullName);
            f.ShowDialog(FindForm());
            if (f.Changed) _ = LoadAsync(); // Availability có thể đổi (Sẵn sàng/Không sẵn sàng) sau khi thêm/xóa ngày
        }

        private bool TryGetSelected(out int employeeId, out string fullName)
        {
            employeeId = 0;
            fullName = "";
            if (grid.CurrentRow is null)
            {
                MessageBox.Show(FindForm(), "Vui lòng chọn 1 tài xế trong danh sách trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            var id = (int)grid.CurrentRow.Cells["EmployeeId"].Value;
            employeeId = id;
            fullName = _drivers.First(d => d.EmployeeId == id).FullName;
            return true;
        }

        private async Task LoadAsync()
        {
            if (_api is null) return;
            lblStatus.Text = "Đang tải...";
            try
            {
                _drivers = await _api.Drivers.GetDriversAsync();
                grid.DataSource = _drivers.Select(d => new
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
                lblStatus.Text = $"Tổng {_drivers.Count} tài xế — \"Sẵn sàng\" = đang làm việc, bằng lái còn hạn, có ngày làm hôm nay. " +
                                 "Khi Chia xe còn xét kho đóng quân và chuyến dài nhiều ngày phải đăng ký đủ các ngày.";
                lblStatus.ForeColor = Theme.TextSecondary;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }

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