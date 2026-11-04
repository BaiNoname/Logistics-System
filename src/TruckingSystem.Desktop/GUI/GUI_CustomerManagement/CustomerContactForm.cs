using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_CustomerManagement;

namespace TruckingSystem.Desktop.GUI_CustomerManagement;

public partial class CustomerContactForm : BaseForm
{
    private readonly ApiServices? _api;
    private readonly CustomerContactListItem? _contact;
    private bool _saving;

    public CustomerContactForm()
    {
        InitializeComponent();
        // Quy tắc design.md: nhãn bắt buộc có *, chân [Đóng] [💾 Lưu].
        lblPhone.Text = "Số điện thoại *";
        FormStyle.SaveButton(_save);
        FormStyle.CloseButton(this, btnCancel);
    }

    public CustomerContactForm(ApiServices api, List<CustomerListItem> customers, CustomerContactListItem? contact = null, int customerId = 0) : this()
    {
        _api = api;
        _contact = contact;
        Text = contact is null ? "Thêm người liên hệ khách hàng" : "Sửa người liên hệ khách hàng";
        _name.Text = contact?.Name ?? "";
        _phone.Text = contact?.Phone ?? "";
        _position.Text = contact?.Position ?? "";
        _customer.DataSource = customers.ToList();
        _customer.SelectedIndex = customers.FindIndex(c => c.Id == (contact?.CustomerId ?? customerId));
    }

    private async void BtnSave_Click(object? sender, EventArgs e) => await SaveAsync();

    private void CustomerContactForm_FormClosing(object? sender, FormClosingEventArgs e) => e.Cancel = _saving;

    private async Task SaveAsync()
    {
        if (_api is null || _saving) return;
        if (_customer.SelectedItem is not CustomerListItem customer)
        {
            FieldHint.Show(_customer, _status, "Vui lòng chọn khách hàng.");
            return;
        }
        if (string.IsNullOrWhiteSpace(_name.Text))
        {
            FieldHint.Show(_name, _status, "Vui lòng nhập tên người liên hệ.");
            return;
        }
        if (!IsValidPhoneNumber(_phone.Text))
        {
            FieldHint.Show(_phone, _status, "Vui lòng nhập số điện thoại hợp lệ (ví dụ: 0901234567 hoặc +84 901234567).");
            return;
        }
        _saving = true;
        _fields.Enabled = _save.Enabled = false;
        FormStyle.Status(_status, "Đang lưu...", StatusKind.Info);
        try
        {
            var result = _contact is null
                ? await _api.Customers.CreateContactAsync(customer.Id, _name.Text.Trim(), _phone.Text.Trim(), _position.Text.Trim())
                : await _api.Customers.UpdateContactAsync(_contact.Id, customer.Id, _name.Text.Trim(), _phone.Text.Trim(), _position.Text.Trim());
            if (!result.Success) { FormStyle.Status(_status, "⚠️ " + result.Message); return; }
            _saving = false;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { FormStyle.Status(_status, "Không lưu được người liên hệ: " + ex.Message); }
        finally
        {
            _saving = false;
            if (!IsDisposed) _fields.Enabled = _save.Enabled = true;
        }

    }
    public static bool IsValidPhoneNumber(string? phone)
        => Domain.Entities.CustomerContact.IsValidPhoneNumber(phone);
}
