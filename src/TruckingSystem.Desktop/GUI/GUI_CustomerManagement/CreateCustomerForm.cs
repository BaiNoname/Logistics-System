using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_CustomerManagement
{
    /// <summary>Logic xử lý — giao diện nằm ở CreateCustomerForm.Designer.cs.</summary>
    public partial class CreateCustomerForm : BaseForm
    {
        private ApiServices? _api;

        public CreateCustomerForm()
        {
            InitializeComponent();
            cboType.Items.AddRange(new object[] { "Individual", "Company" });
            cboType.SelectedIndex = 0;
        }

        public CreateCustomerForm(ApiServices api) : this()
        {
            _api = api;
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                lblStatus.Text = "Vui lòng nhập tên khách hàng.";
                return;
            }
            if (_api is null) return;

            try
            {
                await _api.Customers.CreateCustomerAsync(
                    txtName.Text.Trim(), txtPhone.Text.Trim(), txtTaxCode.Text.Trim(), (string)cboType.SelectedItem!,
                    string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text.Trim(),
                    numCreditLimit.Value == 0 ? null : numCreditLimit.Value,
                    string.IsNullOrWhiteSpace(txtGoogleCalendarId.Text) ? null : txtGoogleCalendarId.Text.Trim());
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
            }
        }
    }
}
