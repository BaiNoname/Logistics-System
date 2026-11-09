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
        private CustomerListItem? _customer;

        public EditCustomerForm()
        {
            InitializeComponent();
            DialogFit.Enable(this, root);
            DialogKeys.Enable(this);
            Anim.FadeIn(this);
            ApplyDesign();
            _ = new AddressSuggestBinder(txtAddress, () => _api); // gợi ý địa chỉ VietMap khi gõ
        }

        /// <summary>Quy tắc design.md: nhãn bắt buộc có *, chân [Đóng] [💾 Lưu thay đổi], Esc = đóng, Enter = lưu.</summary>
        private void ApplyDesign()
        {
            lblPhone.Text = "Số điện thoại *:";
            var close = FormStyle.NewCloseButton(this);
            close.Margin = new Padding(8, 0, 0, 0);
            root.Controls.Remove(buttonBar);
            FormStyle.EditorFooter(this, close, btnSave);
            buttonBar.Dispose();
            FormStyle.SaveButton(btnSave);
            AcceptButton = btnSave;
        }

        public EditCustomerForm(ApiServices api, CustomerListItem customer) : this()
        {
            _api = api;
            _customerId = customer.Id;
            _customer = customer;

            txtName.Text = customer.Name;
            txtPhone.Text = customer.Phone ?? "";
            txtAddress.Text = customer.Address ?? "";
            txtGoogleCalendarId.Text = customer.GoogleCalendarId ?? "";
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                FieldHint.Show(txtName, lblStatus, "Vui lòng nhập tên khách hàng.");
                return;
            }
            if (!IsValidPhoneNumber(txtPhone.Text))
            {
                FieldHint.Show(txtPhone, lblStatus, "Vui lòng nhập số điện thoại hợp lệ (ví dụ: 0901234567 hoặc +84 901234567).");
                return;
            }
            if (_api is null || !btnSave.Enabled) return;

            btnSave.Enabled = false;
            try
            {
                // Giữ nguyên dữ liệu lịch sử của các trường đã bỏ khỏi giao diện.
                var result = await _api.Customers.UpdateCustomerAsync(
                    _customerId, txtName.Text.Trim(), txtPhone.Text.Trim(), _customer?.TaxCode, _customer?.CustomerType ?? "Individual",
                    string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text.Trim(),
                    _customer?.CreditLimit,
                    string.IsNullOrWhiteSpace(txtGoogleCalendarId.Text) ? null : txtGoogleCalendarId.Text.Trim());

                if (!result.Success)
                {
                    FormStyle.Status(lblStatus, "⚠️ " + result.Message);
                    return;
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                FormStyle.Status(lblStatus, "Lỗi: " + ex.Message);
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
