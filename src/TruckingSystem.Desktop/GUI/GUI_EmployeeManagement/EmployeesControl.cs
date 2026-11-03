using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_EmployeeManagement;

namespace TruckingSystem.Desktop.GUI_EmployeeManagement
{
    public partial class EmployeesControl : BaseUserControl
    {
        private ApiServices? _api;
        private List<EmployeeListItem> _employees = new();

        public EmployeesControl()
        {
            InitializeComponent();
            Theme.StyleGrid(grid);
            Resize += (_, _) => { grid.Width = Width; grid.Height = Height - grid.Top; };
            grid.DataBindingComplete += (_, _) => { if (grid.Columns["Id"] != null) grid.Columns["Id"]!.Visible = false; };
            Load += async (_, _) => await LoadAsync();
        }

        public EmployeesControl(ApiServices api) : this()
        {
            _api = api;
            btnCreate.Visible = btnEdit.Visible = api.Connection.IsInRole("Admin");
        }

        private void BtnRefresh_Click(object? sender, EventArgs e) => _ = LoadAsync();

        private async void BtnCreate_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;
            using var f = new CreateEmployeeForm(_api);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
        }

        private async void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (_api is null || grid.CurrentRow is null)
            {
                MessageBox.Show(FindForm(), "Vui lòng chọn 1 nhân viên trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var employeeId = (int)grid.CurrentRow.Cells["Id"].Value;
            var employee = _employees.First(e => e.Id == employeeId);
            using var f = new CreateEmployeeForm(_api, employee);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
        }

        private void BtnSchedule_Click(object? sender, EventArgs e)
        {
            if (_api is null || grid.CurrentRow is null)
            {
                MessageBox.Show(FindForm(), "Vui lòng chọn 1 nhân viên trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var employeeId = (int)grid.CurrentRow.Cells["Id"].Value;
            var employee = _employees.First(e => e.Id == employeeId);

            using var f = new ManageDriverScheduleForm(_api, employeeId, employee.FullName);
            f.ShowDialog(FindForm());
        }

        private async void BtnMakeDriver_Click(object? sender, EventArgs e)
        {
            if (_api is null || grid.CurrentRow is null)
            {
                MessageBox.Show(FindForm(), "Vui lòng chọn 1 nhân viên trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var employeeId = (int)grid.CurrentRow.Cells["Id"].Value;
            var employee = _employees.First(e => e.Id == employeeId);

            // Form tự phát hiện nhân viên ĐÃ có hồ sơ hay chưa và tự chuyển
            // Tạo mới / Sửa cho đúng — không cần chặn ở đây nữa.
            using var f = new CreateDriverProfileForm(_api, employeeId, employee.FullName);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
        }

        private async Task LoadAsync()
        {
            if (_api is null) return;
            lblStatus.Text = "Đang tải...";
            try
            {
                _employees = await _api.Employees.GetEmployeesAsync();
                grid.DataSource = _employees.Select(e => new
                {
                    e.Id,
                    Mã_NV = e.Code,
                    Họ_tên = e.FullName,
                    Ngày_sinh = e.DateOfBirth?.ToString("dd/MM/yyyy") ?? "—",
                    Giới_tính = e.Gender switch { "Male" => "Nam", "Female" => "Nữ", "Other" => "Khác", _ => "Chưa cập nhật" },
                    Căn_Cước = e.IdentityCardNo ?? "—",
                    Điện_thoại = e.Phone ?? "—",
                    Địa_chỉ = e.Address ?? "—",
                    Ngày_tuyển_dụng = e.HireDate.ToString("dd/MM/yyyy"),
                    Phòng_ban = e.DepartmentName ?? "—",
                    Chức_danh = e.PositionName ?? "—",
                    Trạng_thái = e.Status == "Active" ? "Đang làm" : e.Status,
                    //Là_tài_xế = e.IsDriver ? "✅ Có" : "—",
                }).ToList();
                //lblStatus.Text = $"Tổng {_employees.Count} nhân viên — chọn 1 dòng rồi bấm \"Tạo hồ sơ tài xế\" nếu cần";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }
    }
}
