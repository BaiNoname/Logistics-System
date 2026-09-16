using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_EmployeeManagement;

namespace TruckingSystem.Desktop.GUI_EmployeeManagement;

public class CreateEmployeeForm : BaseForm
{
    private readonly ApiServices _api;
    private readonly TextBox _txtCode = new();
    private readonly TextBox _txtFullName = new();
    private readonly TextBox _txtPhone = new();
    private readonly ComboBox _cboDept = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _cboPos = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _lblStatus = new() { Font = Theme.FontSmall, ForeColor = Theme.Danger, Top = 370, Left = 24, AutoSize = true };

    public CreateEmployeeForm(ApiServices api)
    {
        _api = api;
        Text = "Thêm nhân viên mới";
        Width = 460; Height = 470;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        BackColor = Color.White;

        var p1 = Theme.MakeLabeledInput("Mã nhân viên *", _txtCode, 400); p1.Top = 20; p1.Left = 24;
        var p2 = Theme.MakeLabeledInput("Họ tên *", _txtFullName, 400); p2.Top = 90; p2.Left = 24;
        var p3 = Theme.MakeLabeledInput("Số điện thoại", _txtPhone, 400); p3.Top = 160; p3.Left = 24;
        var p4 = Theme.MakeLabeledInput("Phòng ban", _cboDept, 400); p4.Top = 230; p4.Left = 24;
        var p5 = Theme.MakeLabeledInput("Chức danh", _cboPos, 400); p5.Top = 300; p5.Left = 24;

        var btnSave = Theme.MakePrimaryButton("Lưu nhân viên", 180); btnSave.Top = 380; btnSave.Left = 244;
        btnSave.Click += Save;

        Controls.Add(p1); Controls.Add(p2); Controls.Add(p3); Controls.Add(p4); Controls.Add(p5);
        Controls.Add(btnSave); Controls.Add(_lblStatus);

        Load += async (_, _) => await LoadLookupsAsync();
    }

    private async Task LoadLookupsAsync()
    {
        try
        {
            var depts = await _api.Employees.GetDepartmentsAsync();
            var positions = await _api.Employees.GetPositionsAsync();
            _cboDept.DataSource = depts;
            _cboDept.DisplayMember = nameof(LookupItem.Name);
            _cboDept.ValueMember = nameof(LookupItem.Id);
            _cboPos.DataSource = positions;
            _cboPos.DisplayMember = nameof(LookupItem.Name);
            _cboPos.ValueMember = nameof(LookupItem.Id);
        }
        catch (Exception ex)
        {
            _lblStatus.Text = "Không tải được danh sách phòng ban/chức danh: " + ex.Message;
        }
    }

    private async void Save(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtCode.Text) || string.IsNullOrWhiteSpace(_txtFullName.Text))
        {
            _lblStatus.Text = "Vui lòng nhập đủ mã nhân viên và họ tên.";
            return;
        }
        try
        {
            int? deptId = _cboDept.SelectedValue as int?;
            int? posId = _cboPos.SelectedValue as int?;
            await _api.Employees.CreateEmployeeAsync(_txtCode.Text.Trim(), _txtFullName.Text.Trim(), _txtPhone.Text.Trim(), deptId, posId);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            _lblStatus.Text = "Lỗi: " + ex.Message;
        }
    }
}
