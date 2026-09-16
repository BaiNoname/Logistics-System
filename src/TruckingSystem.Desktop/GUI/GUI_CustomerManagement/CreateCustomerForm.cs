using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_CustomerManagement;

public class CreateCustomerForm : BaseForm
{
    private readonly ApiServices _api;
    private readonly TextBox _txtName = new();
    private readonly TextBox _txtPhone = new();
    private readonly TextBox _txtTaxCode = new();
    private readonly ComboBox _cboType = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _lblStatus = new() { Font = Theme.FontSmall, ForeColor = Theme.Danger, Top = 300, Left = 24, AutoSize = true };

    public CreateCustomerForm(ApiServices api)
    {
        _api = api;
        Text = "Thêm khách hàng mới";
        Width = 460; Height = 400;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        BackColor = Color.White;

        _cboType.Items.AddRange(new object[] { "Individual", "Company" });
        _cboType.SelectedIndex = 0;

        var p1 = Theme.MakeLabeledInput("Tên khách hàng *", _txtName, 400); p1.Top = 20; p1.Left = 24;
        var p2 = Theme.MakeLabeledInput("Số điện thoại", _txtPhone, 400); p2.Top = 90; p2.Left = 24;
        var p3 = Theme.MakeLabeledInput("Mã số thuế (nếu có)", _txtTaxCode, 400); p3.Top = 160; p3.Left = 24;
        var p4 = Theme.MakeLabeledInput("Loại khách hàng", _cboType, 400); p4.Top = 230; p4.Left = 24;

        var btnSave = Theme.MakePrimaryButton("Lưu khách hàng", 180); btnSave.Top = 310; btnSave.Left = 244;
        btnSave.Click += Save;

        Controls.Add(p1); Controls.Add(p2); Controls.Add(p3); Controls.Add(p4);
        Controls.Add(btnSave); Controls.Add(_lblStatus);
    }

    private async void Save(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtName.Text))
        {
            _lblStatus.Text = "Vui lòng nhập tên khách hàng.";
            return;
        }
        try
        {
            await _api.Customers.CreateCustomerAsync(_txtName.Text.Trim(), _txtPhone.Text.Trim(), _txtTaxCode.Text.Trim(), (string)_cboType.SelectedItem!);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            _lblStatus.Text = "Lỗi: " + ex.Message;
        }
    }
}
