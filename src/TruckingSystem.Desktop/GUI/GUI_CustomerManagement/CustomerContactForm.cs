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
            _status.Text = "Vui lòng chọn khách hàng.";
            _customer.Focus();
            return;
        }
        if (string.IsNullOrWhiteSpace(_name.Text))
        {
            _status.Text = "Vui lòng nhập tên người liên hệ.";
            _name.Focus();
            return;
        }
        _saving = true;
        _fields.Enabled = _save.Enabled = false;
        _status.Text = "Đang lưu...";
        try
        {
            var result = _contact is null
                ? await _api.Customers.CreateContactAsync(customer.Id, _name.Text.Trim(), _phone.Text.Trim(), _position.Text.Trim())
                : await _api.Customers.UpdateContactAsync(_contact.Id, customer.Id, _name.Text.Trim(), _phone.Text.Trim(), _position.Text.Trim());
            if (!result.Success) { _status.Text = result.Message; return; }
            _saving = false;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { _status.Text = "Không lưu được người liên hệ: " + ex.Message; }
        finally
        {
            _saving = false;
            if (!IsDisposed) _fields.Enabled = _save.Enabled = true;
        }
    }
}
