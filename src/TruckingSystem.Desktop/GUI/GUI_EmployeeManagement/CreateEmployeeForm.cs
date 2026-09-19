using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_EmployeeManagement;

namespace TruckingSystem.Desktop.GUI_EmployeeManagement;

public partial class CreateEmployeeForm : BaseForm
{
    private ApiServices? _api;

    public CreateEmployeeForm()
    {
        InitializeComponent();
        Load += async (_, _) => await LoadLookupsAsync();
    }

    public CreateEmployeeForm(ApiServices api) : this()
    {
        _api = api;
    }

    private async Task LoadLookupsAsync()
    {
        if (_api is null) return;
        try
        {
            var depts = await _api.Employees.GetDepartmentsAsync();
            var positions = await _api.Employees.GetPositionsAsync();
            cboDept.DataSource = depts;
            cboDept.DisplayMember = nameof(LookupItem.Name);
            cboDept.ValueMember = nameof(LookupItem.Id);
            cboPos.DataSource = positions;
            cboPos.DisplayMember = nameof(LookupItem.Name);
            cboPos.ValueMember = nameof(LookupItem.Id);
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

        try
        {
            int? deptId = cboDept.SelectedValue as int?;
            int? posId = cboPos.SelectedValue as int?;
            await _api.Employees.CreateEmployeeAsync(txtCode.Text.Trim(), txtFullName.Text.Trim(), txtPhone.Text.Trim(), deptId, posId);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Lỗi: " + ex.Message;
        }
    }
}
