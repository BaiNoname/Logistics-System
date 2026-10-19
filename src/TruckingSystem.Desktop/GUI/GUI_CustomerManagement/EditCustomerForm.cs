using System.Data;
using System.Text.RegularExpressions;
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
            // Hiện tiếng Việt (khớp danh sách khách hàng), gửi đúng mã cho server.
            cboType.Items.AddRange(new object[]
            {
                new ComboOption("Individual", "Cá nhân"),
                new ComboOption("Company", "Doanh nghiệp"),
            });
        }

        public EditCustomerForm(ApiServices api, CustomerListItem customer) : this()
        {
            _api = api;
            _customerId = customer.Id;

            txtName.Text = customer.Name;
            txtPhone.Text = customer.Phone ?? "";
            txtTaxCode.Text = customer.TaxCode ?? "";
            cboType.SelectedItem = cboType.Items.Cast<ComboOption>().FirstOrDefault(o => o.Value == customer.CustomerType)
                                   ?? cboType.Items[0];
            txtAddress.Text = customer.Address ?? "";
            // Hạn mức đã lưu có thể lớn hơn giới hạn ô nhập (dữ liệu cũ / nhập qua API) -> nới giới
            // hạn thay vì để form crash (ArgumentOutOfRangeException) ngay khi mở.
            var credit = customer.CreditLimit ?? 0;
            if (credit > numCreditLimit.Maximum) numCreditLimit.Maximum = credit;
            if (credit < numCreditLimit.Minimum) numCreditLimit.Minimum = credit;
            numCreditLimit.Value = credit;
            txtGoogleCalendarId.Text = customer.GoogleCalendarId ?? "";
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                lblStatus.Text = "Vui lòng nhập tên khách hàng.";
                return;
            }
            if (!IsValidPhoneNumber(txtPhone.Text))
            {
                lblStatus.ForeColor = Theme.Danger;
                lblStatus.Text = "Vui lòng nhập số điện thoại hợp lệ (ví dụ: +84 123456789).";
                txtPhone.Focus();
                return;
            }
            if (_api is null || !btnSave.Enabled) return;

            btnSave.Enabled = false;
            try
            {
                var result = await _api.Customers.UpdateCustomerAsync(
                    _customerId, txtName.Text.Trim(), txtPhone.Text.Trim(), txtTaxCode.Text.Trim(), ((ComboOption)cboType.SelectedItem!).Value,
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
                if (!IsDisposed) btnSave.Enabled = true;
            }
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
