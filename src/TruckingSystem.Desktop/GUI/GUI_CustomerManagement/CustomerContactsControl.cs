using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_CustomerManagement;

namespace TruckingSystem.Desktop.GUI_CustomerManagement;

public class CustomerContactsControl : BaseUserControl
{
    private readonly ApiServices _api;
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, AccessibleName = "Danh sách người liên hệ khách hàng" };
    private readonly ComboBox _customer = new() { Width = 320, DropDownStyle = ComboBoxStyle.DropDownList, AccessibleName = "Lọc người liên hệ theo khách hàng" };
    private readonly Button _create = Theme.MakePrimaryButton("Thêm người liên hệ", 190);
    private readonly Button _edit = Theme.MakeSecondaryButton("Sửa", 90);
    private readonly Button _delete = Theme.MakeSecondaryButton("Xóa", 90);
    private readonly Button _refresh = Theme.MakeSecondaryButton("Làm mới", 120);
    private readonly Label _status = new() { Dock = DockStyle.Fill, AutoEllipsis = true, ForeColor = Theme.TextSecondary, TextAlign = ContentAlignment.MiddleLeft };
    private List<CustomerListItem> _customers = new();
    private List<CustomerContactListItem> _contacts = new();
    private bool _busy;
    private bool _binding;
    private bool _loaded;

    private int CustomerId => (_customer.SelectedItem as CustomerListItem)?.Id ?? 0;
    private CustomerContactListItem? SelectedContact => _grid.CurrentRow?.DataBoundItem as CustomerContactListItem;

    public CustomerContactsControl(ApiServices api)
    {
        _api = api;
        BackColor = Theme.CardBg;
        Theme.StyleGrid(_grid);
        _grid.AutoGenerateColumns = false;
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { DataPropertyName = "CustomerName", HeaderText = "Khách hàng", FillWeight = 160 },
            new DataGridViewTextBoxColumn { DataPropertyName = "Name", HeaderText = "Tên người liên hệ", FillWeight = 140 },
            new DataGridViewTextBoxColumn { DataPropertyName = "Phone", HeaderText = "Số điện thoại", DefaultCellStyle = new() { NullValue = "—" } },
            new DataGridViewTextBoxColumn { DataPropertyName = "Position", HeaderText = "Chức vụ", DefaultCellStyle = new() { NullValue = "—" } });

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(0, 4, 0, 8) };
        toolbar.Controls.AddRange(new Control[] { _create, _edit, _delete, _refresh });
        _delete.ForeColor = Theme.Danger;
        var filter = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(0, 0, 0, 8) };
        filter.Controls.Add(new Label { Text = "Khách hàng", AutoSize = true, Margin = new Padding(0, 7, 12, 0) });
        filter.Controls.Add(_customer);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        layout.Controls.Add(toolbar, 0, 0);
        layout.Controls.Add(filter, 0, 1);
        layout.Controls.Add(_grid, 0, 2);
        layout.Controls.Add(_status, 0, 3);
        Controls.Add(layout);

        _create.Visible = _edit.Visible = _delete.Visible = api.Connection.IsInRole("Admin", "Dispatcher");
        _create.Click += async (_, _) => await EditAsync(create: true);
        _edit.Click += async (_, _) => await EditAsync(create: false);
        _delete.Click += async (_, _) => await DeleteAsync();
        _refresh.Click += async (_, _) => await LoadAsync();
        _customer.SelectedIndexChanged += (_, _) => { if (!_binding) ApplyFilter(); };
        _grid.SelectionChanged += (_, _) => UpdateActions();
        _grid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await EditAsync(create: false); };
        UpdateActions();
    }

    private void UpdateActions()
    {
        var canWrite = _api.Connection.IsInRole("Admin", "Dispatcher");
        _create.Enabled = canWrite && !_busy && _loaded && _customers.Count > 0;
        _edit.Enabled = _delete.Enabled = canWrite && !_busy && _loaded && SelectedContact is not null;
        _refresh.Enabled = !_busy;
        _customer.Enabled = _grid.Enabled = !_busy && _loaded;
    }

    public async Task LoadAsync()
    {
        if (_busy) return;
        var customerId = CustomerId;
        _busy = true;
        _loaded = false;
        UpdateActions();
        _status.ForeColor = Theme.TextSecondary;
        _status.Text = "Đang tải...";
        try
        {
            var customers = await _api.Customers.GetCustomersAsync();
            var contacts = await _api.Customers.GetContactsAsync();
            if (IsDisposed) return;
            _customers = customers;
            _contacts = contacts;
            _binding = true;
            _customer.DisplayMember = nameof(CustomerListItem.Name);
            _customer.ValueMember = nameof(CustomerListItem.Id);
            _customer.DataSource = new[] { new CustomerListItem { Name = "Tất cả khách hàng" } }.Concat(_customers).ToList();
            _customer.SelectedValue = _customers.Any(c => c.Id == customerId) ? customerId : 0;
            _loaded = true;
            ApplyFilter();
        }
        catch (Exception ex)
        {
            if (!IsDisposed)
            {
                _status.ForeColor = Theme.Danger;
                _status.Text = "Không tải được người liên hệ: " + ex.Message;
            }
        }
        finally
        {
            _binding = _busy = false;
            if (!IsDisposed) UpdateActions();
        }
    }

    private void ApplyFilter()
    {
        var contacts = _contacts.Where(c => CustomerId == 0 || c.CustomerId == CustomerId).ToList();
        _grid.DataSource = contacts;
        _status.ForeColor = Theme.TextSecondary;
        _status.Text = _customers.Count == 0 ? "Thêm khách hàng ở tab Khách hàng trước khi tạo người liên hệ."
            : $"Tổng {contacts.Count} người liên hệ";
        UpdateActions();
    }

    private async Task EditAsync(bool create)
    {
        if (create ? !_create.Enabled : !_edit.Enabled) return;
        using var form = new CustomerContactForm(_api, _customers, create ? null : SelectedContact, CustomerId);
        if (form.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
    }

    private async Task DeleteAsync()
    {
        if (!_delete.Enabled || SelectedContact is not { } contact) return;
        if (MessageBox.Show(FindForm(), $"Xóa người liên hệ \"{contact.Name}\" của khách hàng \"{contact.CustomerName}\"?",
            "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
        _busy = true;
        UpdateActions();
        var deleted = false;
        try
        {
            var result = await _api.Customers.DeleteContactAsync(contact.Id);
            deleted = result.Success;
            if (!result.Success && !IsDisposed)
            {
                _status.ForeColor = Theme.Danger;
                _status.Text = result.Message;
            }
        }
        catch (Exception ex)
        {
            if (!IsDisposed)
            {
                _status.ForeColor = Theme.Danger;
                _status.Text = "Không xóa được người liên hệ: " + ex.Message;
            }
        }
        finally
        {
            _busy = false;
            if (!IsDisposed) UpdateActions();
        }
        if (deleted && !IsDisposed) await LoadAsync();
    }
}
