using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_EmployeeManagement;

/// <summary>Tạo hồ sơ tài xế (DriverProfile) cho 1 nhân viên đã có sẵn — BẮT
/// BUỘC phải có hồ sơ này thì nhân viên đó mới xuất hiện trong danh sách tài
/// xế khả dụng của thuật toán Chia xe (Greedy).</summary>
public partial class CreateDriverProfileForm : BaseForm
{
    private ApiServices? _api;
    private int _employeeId;

    public CreateDriverProfileForm()
    {
        InitializeComponent();
        cboLicenseClass.Items.AddRange(new object[] { "B2", "C", "D", "E", "FC" });
        cboLicenseClass.SelectedIndex = 0;
        dtLicenseExpiry.Value = DateTime.Today.AddYears(3);
        dtHealthCertExpiry.Value = DateTime.Today.AddYears(1);
        chkHasHealthCert.CheckedChanged += (_, _) => dtHealthCertExpiry.Enabled = chkHasHealthCert.Checked;
    }

    public CreateDriverProfileForm(ApiServices api, int employeeId, string employeeName) : this()
    {
        _api = api;
        _employeeId = employeeId;
        Text = $"Tạo hồ sơ tài xế — {employeeName}";
    }

    private async void BtnSave_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtLicenseNumber.Text))
        {
            lblStatus.Text = "Vui lòng nhập số bằng lái.";
            return;
        }
        if (_api is null) return;

        btnSave.Enabled = false;
        try
        {
            var result = await _api.Employees.CreateDriverProfileAsync(
                _employeeId, txtLicenseNumber.Text.Trim(), (string)cboLicenseClass.SelectedItem!,
                dtLicenseExpiry.Value, chkHasHealthCert.Checked ? dtHealthCertExpiry.Value : null,
                (int)numYearsExp.Value);

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