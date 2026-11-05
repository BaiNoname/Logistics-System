using System.Text.RegularExpressions;
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
            FormStyle.EditorFooter(this, FormStyle.NewCloseButton(this), btnSave);
            DialogKeys.Enable(this);
            Anim.FadeIn(this);
            var today = DateTime.UtcNow.AddHours(7).Date;
            var latestBirthDate = today.AddYears(-18);
            if (!TruckingSystem.Domain.Entities.Employee.IsValidNewEmployeeDateOfBirth(latestBirthDate, today))
                latestBirthDate = latestBirthDate.AddDays(-1);
            dtpDateOfBirth.MaxDate = latestBirthDate;
            dtpDateOfBirth.ShowCheckBox = false;
            lblDateOfBirth.Text = "Ngày sinh * (trên 18 tuổi):";
            dtpDateOfBirth.AccessibleName = lblDateOfBirth.Text.TrimEnd(':');
            lblEmployeeStatus.Visible = cboEmployeeStatus.Visible = false;
            lblTerminationDate.Visible = dtpTerminationDate.Visible = false;
            tableLayoutPanel1.SetColumn(lblHireDate, 0);
            tableLayoutPanel1.SetColumn(dtpHireDate, 0);
            dtpHireDate.Value = DateTime.UtcNow.AddHours(7).Date;
            cboEmployeeStatus.DataSource = new[]
            {
                new StatusOption("Active", "Đang làm"),
                new StatusOption("OnLeave", "Nghỉ phép"),
                new StatusOption("Terminated", "Đã nghỉ việc"),
            };
            cboEmployeeStatus.DisplayMember = nameof(StatusOption.Text);
            cboEmployeeStatus.ValueMember = nameof(StatusOption.Value);
            Load += async (_, _) => await LoadLookupsAsync();
            _ = new AddressSuggestBinder(txtAddress, () => _api); // gợi ý địa chỉ VietMap khi gõ
        }

        public CreateEmployeeForm(ApiServices api) : this()
        {
            _api = api;
        }

        public CreateEmployeeForm(ApiServices api, EmployeeListItem employee) : this(api)
        {
            _employeeId = employee.Id;
            dtpDateOfBirth.MaxDate = DateTime.UtcNow.AddHours(7).Date;
            lblDateOfBirth.Text = "Ngày sinh (Delete để xóa):";
            dtpDateOfBirth.AccessibleName = lblDateOfBirth.Text.TrimEnd(':');
            lblEmployeeStatus.Visible = cboEmployeeStatus.Visible = true;
            lblTerminationDate.Visible = dtpTerminationDate.Visible = true;
            tableLayoutPanel1.SetColumn(lblHireDate, 1);
            tableLayoutPanel1.SetColumn(dtpHireDate, 1);
            _departmentId = employee.DepartmentId;
            _positionId = employee.PositionId;
            txtCode.Text = employee.Code;
            txtFullName.Text = employee.FullName;
            dtpDateOfBirth.Value = employee.DateOfBirth ?? dtpDateOfBirth.MaxDate;
            ConfigureOptionalDate(dtpDateOfBirth, employee.DateOfBirth.HasValue);
            rdoMale.Checked = employee.Gender == "Male";
            rdoFemale.Checked = employee.Gender == "Female";
            txtPhone.Text = employee.Phone ?? "";
            txtIdentityCardNo.Text = employee.IdentityCardNo ?? "";
            txtAddress.Text = employee.Address ?? "";
            cboEmployeeStatus.SelectedValue = employee.Status;
            dtpHireDate.Value = employee.HireDate < dtpHireDate.MinDate ? dtpHireDate.MinDate : employee.HireDate;
            dtpTerminationDate.Value = employee.TerminationDate ?? DateTime.UtcNow.AddHours(7).Date;
            lblTerminationDate.Text = "Ngày nghỉ việc (Delete để xóa):";
            dtpTerminationDate.AccessibleName = lblTerminationDate.Text.TrimEnd(':');
            ConfigureOptionalDate(dtpTerminationDate, employee.TerminationDate.HasValue);
            Text = "Sửa thông tin nhân viên";
            btnSave.Text = "💾 Lưu thay đổi";
        }

        private async Task LoadLookupsAsync()
        {
            if (_api is null) return;
            try
            {
                var depts = await _api.Employees.GetDepartmentsAsync();
                var positions = await _api.Employees.GetPositionsAsync();
                depts.Insert(0, new LookupItem { Id = 0, Name = "-- Không chọn --" });

                cboDept.DataSource = depts;
                cboDept.DisplayMember = nameof(LookupItem.Name);
                cboDept.ValueMember = nameof(LookupItem.Id);
                void FilterPositions()
                {
                    var selected = cboPos.SelectedValue as int? ?? _positionId ?? 0;
                    var departmentId = cboDept.SelectedValue as int?;
                    cboPos.DataSource = new[] { new LookupItem { Id = 0, Name = "-- Không chọn --" } }
                        .Concat(positions.Where(p => p.DepartmentId == null || p.DepartmentId == departmentId)).ToList();
                    cboPos.SelectedValue = selected;
                    if (cboPos.SelectedIndex < 0) cboPos.SelectedValue = 0;
                }
                cboDept.SelectedValueChanged += (_, _) => FilterPositions();
                cboPos.DisplayMember = nameof(LookupItem.Name);
                cboPos.ValueMember = nameof(LookupItem.Id);
                cboDept.SelectedValue = _departmentId ?? 0;
                FilterPositions();
                cboPos.SelectedValue = _positionId ?? 0;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Không tải được danh sách phòng ban/chức danh: " + ex.Message;
            }
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                FieldHint.Show(txtCode, lblStatus, "Vui lòng nhập mã nhân viên.");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                FieldHint.Show(txtFullName, lblStatus, "Vui lòng nhập họ tên nhân viên.");
                return;
            }
            if (!IsValidPhoneNumber(txtPhone.Text))
            {
                FieldHint.Show(txtPhone, lblStatus, "Vui lòng nhập số điện thoại hợp lệ.");
                return;
            }
            if (cboDept.SelectedIndex == 0 || cboPos.SelectedIndex == 0)
            {
                FieldHint.Show(cboDept.SelectedIndex == 0 ? cboDept : cboPos, lblStatus, "Vui lòng chọn phòng ban và chức danh.");
                return;
            }
            if (!rdoMale.Checked && !rdoFemale.Checked)
            {
                FieldHint.Show(pnlGender, lblStatus, "Vui lòng chọn giới tính Nam hoặc Nữ.");
                return;
            }
            if (_api is null) return;

            if (!_employeeId.HasValue && !TruckingSystem.Domain.Entities.Employee.IsValidNewEmployeeDateOfBirth(
                SelectedDate(dtpDateOfBirth), DateTime.UtcNow.AddHours(7).Date))
            {
                FieldHint.Show(dtpDateOfBirth, lblStatus, "Vui lòng chọn ngày sinh từ năm 1900 và nhân viên phải trên 18 tuổi.");
                return;
            }
            if (!TruckingSystem.Domain.Entities.Employee.IsValidDateOfBirth(
                SelectedDate(dtpDateOfBirth), DateTime.UtcNow.AddHours(7).Date))
            {
                FieldHint.Show(dtpDateOfBirth, lblStatus, "Ngày sinh phải từ năm 1900 đến ngày hiện tại.");
                return;
            }
            if (_employeeId.HasValue && SelectedDate(dtpTerminationDate) is DateTime terminationDate && terminationDate < dtpHireDate.Value.Date)
            {
                FieldHint.Show(dtpTerminationDate, lblStatus, "Ngày nghỉ việc không được trước ngày vào làm.");
                return;
            }
            if (_employeeId.HasValue && (string?)cboEmployeeStatus.SelectedValue == "Terminated" && SelectedDate(dtpTerminationDate) is null)
            {
                FieldHint.Show(dtpTerminationDate, lblStatus, "Vui lòng nhập ngày nghỉ việc thực tế.");
                return;
            }
            btnSave.Enabled = false;
            try
            {
                int? deptId = cboDept.SelectedValue is int selectedDeptId && selectedDeptId > 0 ? selectedDeptId : null;
                int? posId = cboPos.SelectedValue is int selectedPosId && selectedPosId > 0 ? selectedPosId : null;
                var status = (string)cboEmployeeStatus.SelectedValue!;
                DateTime? dateOfBirth = SelectedDate(dtpDateOfBirth);
                var gender = rdoMale.Checked ? "Male" : "Female";
                var result = _employeeId.HasValue
                    ? await _api.Employees.UpdateEmployeeAsync(_employeeId.Value, txtCode.Text.Trim(), txtFullName.Text.Trim(), txtPhone.Text.Trim(), deptId, posId, status, txtIdentityCardNo.Text.Trim(), txtAddress.Text.Trim(), dtpHireDate.Value.Date, SelectedDate(dtpTerminationDate), dateOfBirth, gender)
                    : await _api.Employees.CreateEmployeeAsync(txtCode.Text.Trim(), txtFullName.Text.Trim(), txtPhone.Text.Trim(), deptId, posId, txtIdentityCardNo.Text.Trim(), txtAddress.Text.Trim(), dtpHireDate.Value.Date, null, dateOfBirth, gender);

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

        private static DateTime? SelectedDate(DateTimePicker picker)
            => picker.CustomFormat == " " ? null : picker.Value.Date;

        private static void ConfigureOptionalDate(DateTimePicker picker, bool hasValue)
        {
            picker.Format = DateTimePickerFormat.Custom;
            picker.CustomFormat = hasValue ? UiStyle.DateFormat : " ";
            picker.ValueChanged += (_, _) => picker.CustomFormat = UiStyle.DateFormat;
            picker.CloseUp += (_, _) => picker.CustomFormat = UiStyle.DateFormat;
            picker.KeyDown += (_, e) =>
            {
                if (e.KeyCode is not (Keys.Delete or Keys.Back)) return;
                picker.CustomFormat = " ";
                e.SuppressKeyPress = true;
            };
        }

        public static bool IsValidPhoneNumber(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            return Regex.IsMatch(
                phone,
                @"^(?:0[35789][0-9]{8}|\+[1-9][0-9]{0,2} [0-9]{4,14})$"
            );
        }
    }
}
