using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_EmployeeManagement;

namespace TruckingSystem.Desktop.GUI_EmployeeManagement
{
    public partial class CreateEmployeeForm : BaseForm
    {
        private ApiServices? _api;
        private int? _employeeId;
        private int? _departmentId;
        private int? _positionId;

        private sealed record StatusOption(string Value, string Text)
        {
            public override string ToString() => Text;
        }

        public CreateEmployeeForm()
        {
            InitializeComponent();
            cboEmployeeStatus.DataSource = new[]
            {
                new StatusOption("Active", "Đang làm"),
                new StatusOption("OnLeave", "Nghỉ phép"),
                new StatusOption("Terminated", "Đã nghỉ việc"),
            };
            cboEmployeeStatus.DisplayMember = nameof(StatusOption.Text);
            cboEmployeeStatus.ValueMember = nameof(StatusOption.Value);
            Load += async (_, _) => await LoadLookupsAsync();
        }

        public CreateEmployeeForm(ApiServices api) : this()
        {
            _api = api;
        }

        public CreateEmployeeForm(ApiServices api, EmployeeListItem employee) : this(api)
        {
            _employeeId = employee.Id;
            _departmentId = employee.DepartmentId;
            _positionId = employee.PositionId;
            txtCode.Text = employee.Code;
            txtFullName.Text = employee.FullName;
            txtPhone.Text = employee.Phone ?? "";
            cboEmployeeStatus.SelectedValue = employee.Status;
            Text = "Sửa thông tin nhân viên";
            btnSave.Text = "Lưu thay đổi";
        }

        private async Task LoadLookupsAsync()
        {
            if (_api is null) return;
            try
            {
                var depts = await _api.Employees.GetDepartmentsAsync();
                var positions = await _api.Employees.GetPositionsAsync();
                depts.Insert(0, new LookupItem { Id = 0, Name = "-- Không chọn --" });
                positions.Insert(0, new LookupItem { Id = 0, Name = "-- Không chọn --" });
                cboDept.DataSource = depts;
                cboDept.DisplayMember = nameof(LookupItem.Name);
                cboDept.ValueMember = nameof(LookupItem.Id);
                cboPos.DataSource = positions;
                cboPos.DisplayMember = nameof(LookupItem.Name);
                cboPos.ValueMember = nameof(LookupItem.Id);
                cboDept.SelectedValue = _departmentId ?? 0;
                cboPos.SelectedValue = _positionId ?? 0;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Không tải được danh sách phòng ban/chức danh: " + ex.Message;
            }
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text) || string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                lblStatus.Text = "Vui lòng nhập đủ mã nhân viên và họ tên.";
                return;
            }
            if (_api is null) return;

            btnSave.Enabled = false;
            try
            {
                int? deptId = cboDept.SelectedValue is int selectedDeptId && selectedDeptId > 0 ? selectedDeptId : null;
                int? posId = cboPos.SelectedValue is int selectedPosId && selectedPosId > 0 ? selectedPosId : null;
                var status = (string)cboEmployeeStatus.SelectedValue!;
                var result = _employeeId.HasValue
                    ? await _api.Employees.UpdateEmployeeAsync(_employeeId.Value, txtCode.Text.Trim(), txtFullName.Text.Trim(), txtPhone.Text.Trim(), deptId, posId, status)
                    : await _api.Employees.CreateEmployeeAsync(txtCode.Text.Trim(), txtFullName.Text.Trim(), txtPhone.Text.Trim(), deptId, posId);

                if (!result.Success)
                {
                    lblStatus.Text = "⚠️ " + result.Message;
                    return;
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }
    }
}
