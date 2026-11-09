using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_CustomerManagement;

namespace TruckingSystem.Desktop.GUI_CustomerManagement;

public partial class CustomerContactsControl : BaseUserControl
{
    private ApiServices? _api;
    private List<CustomerListItem> _customers = new();
    private List<CustomerContactListItem> _contacts = new();
    private readonly int? _customerId;
    private bool _busy;
    private bool _loaded;
    private readonly ToolStripMenuItem _edit = new("✏️ Sửa người liên hệ");
    private readonly ToolStripMenuItem _delete = new("🗑 Xóa người liên hệ");

    private int CustomerId => _customerId ?? 0;
    private CustomerContactListItem? SelectedContact => _grid.CurrentRow?.DataBoundItem as CustomerContactListItem;

    public CustomerContactsControl()
    {
        InitializeComponent();
        Theme.StyleGrid(_grid);
        var menu = new ContextMenuStrip();
        menu.Items.AddRange([_edit, new ToolStripSeparator(), _delete]);
        _edit.Click += BtnEdit_Click;
        _delete.Click += BtnDelete_Click;
        menu.Opening += (_, e) =>
        {
            UpdateActions();
            e.Cancel = _busy || !_loaded || SelectedContact is null || _api?.Connection.IsInRole("Admin", "Dispatcher") != true;
        };
        ModernMenu.Apply(menu);
        _grid.ContextMenuStrip = menu;
        _grid.CellMouseDown += (_, e) =>
        {
            if (e.Button != MouseButtons.Right || _busy) return;
            _grid.ClearSelection();
            _grid.CurrentCell = e.RowIndex < 0 ? null : _grid.Rows[e.RowIndex].Cells.Cast<DataGridViewCell>().First(c => c.Visible);
        };
        _grid.MouseDown += (_, e) =>
        {
            if (e.Button != MouseButtons.Right || _grid.HitTest(e.X, e.Y).RowIndex >= 0) return;
            _grid.ClearSelection();
            _grid.CurrentCell = null;
        };
        Disposed += (_, _) => menu.Dispose();
    }

    public CustomerContactsControl(ApiServices api) : this() => SetApi(api);

    public CustomerContactsControl(ApiServices api, int customerId) : this(api) => _customerId = customerId;

    internal void SetApi(ApiServices api)
    {
        _api = api;
        _create.Visible = api.Connection.IsInRole("Admin", "Dispatcher");
        UpdateActions();
    }

    private async void BtnCreate_Click(object? sender, EventArgs e) => await EditAsync(create: true);
    private async void BtnEdit_Click(object? sender, EventArgs e) => await EditAsync(create: false);
    private async void BtnDelete_Click(object? sender, EventArgs e) => await DeleteAsync();
    private async void BtnRefresh_Click(object? sender, EventArgs e) => await LoadAsync();
    private void Grid_SelectionChanged(object? sender, EventArgs e) => UpdateActions();
    private async void Grid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e) { if (e.RowIndex >= 0) await EditAsync(create: false); }

    private void UpdateActions()
    {
        if (_api is null) return;
        var canWrite = _api.Connection.IsInRole("Admin", "Dispatcher");
        _create.Enabled = canWrite && !_busy && _loaded && _customers.Count > 0;
        _edit.Enabled = _delete.Enabled = canWrite && !_busy && _loaded && SelectedContact is not null;
        _refresh.Enabled = !_busy;
        _grid.Enabled = !_busy && _loaded;
    }

    public async Task LoadAsync()
    {
        if (_api is null || _busy) return;
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
            _customers = customers.Where(c => !_customerId.HasValue || c.Id == _customerId).ToList();
            _contacts = contacts;
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
            _busy = false;
            if (!IsDisposed) UpdateActions();
        }
    }

    private void ApplyFilter()
    {
        var contacts = _contacts.Where(c => CustomerId == 0 || c.CustomerId == CustomerId).ToList();
        _grid.DataSource = contacts;
        _status.ForeColor = Theme.TextSecondary;
        _status.Text = _customers.Count == 0 ? "Thêm khách hàng ở màn Khách hàng trước khi tạo người liên hệ."
            : $"Tổng {contacts.Count} người liên hệ";
        UpdateActions();
    }

    private async Task EditAsync(bool create)
    {
        if (_api is null || !_api.Connection.IsInRole("Admin", "Dispatcher") || (create ? !_create.Enabled : !_edit.Enabled)) return;
        using var form = new CustomerContactForm(_api, _customers, create ? null : SelectedContact, CustomerId);
        if (form.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
    }

    private async Task DeleteAsync()
    {
        if (_api is null || !_api.Connection.IsInRole("Admin", "Dispatcher") || !_delete.Enabled || SelectedContact is not { } contact) return;
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
