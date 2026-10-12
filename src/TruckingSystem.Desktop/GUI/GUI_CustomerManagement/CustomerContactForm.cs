using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_CustomerManagement;

namespace TruckingSystem.Desktop.GUI_CustomerManagement;

public class CustomerContactForm : BaseForm
{
    private readonly ApiServices _api;
    private readonly CustomerContactListItem? _contact;
    private readonly ComboBox _customer = new() { DropDownStyle = ComboBoxStyle.DropDownList, AccessibleName = "Khách hàng của người liên hệ" };
    private readonly TextBox _name = new() { AccessibleName = "Tên người liên hệ" };
    private readonly TextBox _phone = new() { AccessibleName = "Số điện thoại" };
    private readonly TextBox _position = new() { AccessibleName = "Chức vụ" };
    private readonly Button _save = Theme.MakePrimaryButton("Lưu", 100);
    private readonly Label _status = new() { AutoSize = true, MaximumSize = new Size(440, 0), ForeColor = Theme.Danger };
    private readonly FlowLayoutPanel _fields = new() { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(24) };
    private bool _saving;

    public CustomerContactForm(ApiServices api, List<CustomerListItem> customers, CustomerContactListItem? contact = null, int customerId = 0)
    {
        _api = api;
        _contact = contact;
        Text = contact is null ? "Thêm người liên hệ khách hàng" : "Sửa người liên hệ khách hàng";
        ClientSize = new Size(500, 410);
        BackColor = Theme.CardBg;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = MinimizeBox = false;
        _customer.DisplayMember = nameof(CustomerListItem.Name);
        _customer.ValueMember = nameof(CustomerListItem.Id);
        _name.Text = contact?.Name ?? "";
        _phone.Text = contact?.Phone ?? "";
        _position.Text = contact?.Position ?? "";
        _fields.Controls.Add(Theme.MakeLabeledInput("Khách hàng *", _customer, 440));
        _fields.Controls.Add(Theme.MakeLabeledInput("Tên người liên hệ *", _name, 440));
        _fields.Controls.Add(Theme.MakeLabeledInput("Số điện thoại", _phone, 440));
        _fields.Controls.Add(Theme.MakeLabeledInput("Chức vụ", _position, 440));
        _fields.Controls.Add(_status);
        var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 68, Padding = new Padding(24, 10, 24, 10), FlowDirection = FlowDirection.RightToLeft };
        var cancel = Theme.MakeSecondaryButton("Hủy", 100);
        cancel.DialogResult = DialogResult.Cancel;
        footer.Controls.AddRange(new Control[] { _save, cancel });
        Controls.Add(_fields);
        Controls.Add(footer);
        _customer.DataSource = customers.ToList();
        _customer.SelectedIndex = customers.FindIndex(c => c.Id == (contact?.CustomerId ?? customerId));
        AcceptButton = _save;
        CancelButton = cancel;
        _save.Click += async (_, _) => await SaveAsync();
        FormClosing += (_, e) => e.Cancel = _saving;
    }

    private async Task SaveAsync()
    {
        if (_saving) return;
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
