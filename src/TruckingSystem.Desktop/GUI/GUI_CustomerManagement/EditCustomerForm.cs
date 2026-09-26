using System.Data;
using System.Xml.Linq;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_CustomerManagement;

namespace TruckingSystem.Desktop.GUI_CustomerManagement
{
    /// <summary>Sửa thông tin khách hàng đã có — nạp sẵn dữ liệu cũ.</summary>
    public partial class EditCustomerForm : BaseForm
    {
        private ApiServices? _api;
        private int _customerId;

        public EditCustomerForm()
        {
            InitializeComponent();
            cboType.Items.AddRange(new object[] { "Individual", "Company" });
        }

        public EditCustomerForm(ApiServices api, CustomerListItem customer) : this()
        {
            _api = api;
            _customerId = customer.Id;

            txtName.Text = customer.Name;
            txtPhone.Text = customer.Phone ?? "";
            txtTaxCode.Text = customer.TaxCode ?? "";
            cboType.SelectedItem = customer.CustomerType;
            txtAddress.Text = customer.Address ?? "";
            numCreditLimit.Value = customer.CreditLimit ?? 0;
            txtGoogleCalendarId.Text = customer.GoogleCalendarId ?? "";
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                lblStatus.Text = "Vui lòng nhập tên khách hàng.";
                return;
            }
            if (_api is null) return;

            btnSave.Enabled = false;
            try
            {
                var result = await _api.Customers.UpdateCustomerAsync(
                    _customerId, txtName.Text.Trim(), txtPhone.Text.Trim(), txtTaxCode.Text.Trim(), (string)cboType.SelectedItem!,
                    string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text.Trim(),
                    numCreditLimit.Value == 0 ? null : numCreditLimit.Value,
                    string.IsNullOrWhiteSpace(txtGoogleCalendarId.Text) ? null : txtGoogleCalendarId.Text.Trim());

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
