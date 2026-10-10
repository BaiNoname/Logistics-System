using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_ContractManagement;

namespace TruckingSystem.Desktop.GUI.GUI_ContractManagement;

public class ContractForm : BaseForm
{
    private readonly ApiServices _api;
    private readonly ContractListItem? _contract;
    private readonly bool _readOnly;
    private bool _saving;
    private readonly ErrorProvider _errors = new() { BlinkStyle = ErrorBlinkStyle.NeverBlink };
    private readonly TextBox _code = new();
    private readonly ComboBox _customer = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker _start = new() { Format = DateTimePickerFormat.Short };
    private readonly CheckBox _hasEnd = new() { Text = "Có ngày kết thúc", AutoSize = true };
    private readonly DateTimePicker _end = new() { Format = DateTimePickerFormat.Short, Enabled = false };
    private readonly ComboBox _priceList = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _terms = new() { Multiline = true, ScrollBars = ScrollBars.Vertical };
    private readonly Label _status = new() { Dock = DockStyle.Fill, ForeColor = Theme.Danger };
    private readonly Button _save = Theme.MakePrimaryButton("Lưu hợp đồng", 150);

    public ContractForm(ApiServices api, ContractListItem? contract = null, bool readOnly = false)
    {
        _api = api;
        _contract = contract;
        _readOnly = readOnly;
        _errors.ContainerControl = this;
        Text = readOnly ? "Chi tiết hợp đồng" : contract is null ? "Thêm hợp đồng mới" : "Sửa hợp đồng";
        ClientSize = new Size(600, 560);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, Padding = new Padding(24), ColumnCount = 2, RowCount = 8,
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64));
        for (var i = 0; i < 8; i++)
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, i == 5 ? 120 : i == 6 ? 64 : 48));

        AddRow(layout, 0, "Mã hợp đồng *", _code);
        AddRow(layout, 1, "Khách hàng *", _customer);
        AddRow(layout, 2, "Ngày bắt đầu *", _start);
        var endPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        _end.Width = 160;
        endPanel.Controls.Add(_hasEnd);
        endPanel.Controls.Add(_end);
        _hasEnd.CheckedChanged += (_, _) => _end.Enabled = _hasEnd.Checked;
        AddRow(layout, 3, "Ngày kết thúc", endPanel);
        AddRow(layout, 4, "Bảng giá", _priceList);
        AddRow(layout, 5, "Điều khoản", _terms);

        var footer = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        _save.Click += async (_, _) => await SaveAsync();
        var cancel = Theme.MakeSecondaryButton(readOnly ? "Đóng" : "Hủy", 100);
        cancel.DialogResult = DialogResult.Cancel;
        footer.Controls.Add(_save);
        footer.Controls.Add(cancel);
        layout.SetColumnSpan(_status, 2);
        layout.Controls.Add(_status, 0, 6);
        layout.SetColumnSpan(footer, 2);
        layout.Controls.Add(footer, 0, 7);
        Controls.Add(layout);
        AcceptButton = _save;
        CancelButton = cancel;
        FormClosing += (_, e) => e.Cancel = _saving;
        if (readOnly && contract is not null)
        {
            _customer.DisplayMember = nameof(ContractOption.Name);
            _customer.ValueMember = nameof(ContractOption.Id);
            _customer.DataSource = new[] { new ContractOption { Id = contract.CustomerId, Name = contract.CustomerName } };
            _priceList.DisplayMember = nameof(ContractOption.Name);
            _priceList.ValueMember = nameof(ContractOption.Id);
            _priceList.DataSource = new[] { new ContractOption { Id = contract.PriceListId ?? 0, Name = contract.PriceListName ?? "Không chọn bảng giá" } };
            FillContract();
            _code.ReadOnly = _terms.ReadOnly = true;
            _customer.Enabled = _start.Enabled = _hasEnd.Enabled = _end.Enabled = _priceList.Enabled = false;
            _hasEnd.Visible = _end.Visible = false;
            endPanel.Controls.Add(new Label { Text = contract.EndDate?.ToString("dd/MM/yyyy") ?? "Không thời hạn", AutoSize = true, ForeColor = Theme.TextPrimary });
            _save.Visible = false;
            AcceptButton = cancel;
            _status.ForeColor = Theme.TextSecondary;
            _status.Text = $"Trạng thái: {contract.StatusText}\nTạo: {contract.CreatedAt:dd/MM/yyyy HH:mm} · Cập nhật: {contract.UpdatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "—"}";
        }
        else Shown += async (_, _) => await LoadLookupsAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _errors.Dispose();
        base.Dispose(disposing);
    }

    private static void AddRow(TableLayoutPanel layout, int row, string label, Control input)
    {
        var caption = new Label { Text = label, AutoSize = true, ForeColor = Theme.TextSecondary, Anchor = AnchorStyles.Left };
        input.Dock = DockStyle.Fill;
        input.AccessibleName = label.TrimEnd(' ', '*');
        if (input is TextBox textBox && textBox.Multiline) input.Height = 90;
        layout.Controls.Add(caption, 0, row);
        layout.Controls.Add(input, 1, row);
    }

    private async Task LoadLookupsAsync()
    {
        _save.Enabled = false;
        try
        {
            var lookups = await _api.Contracts.GetLookupsAsync();
            _customer.DisplayMember = nameof(ContractOption.Name);
            _customer.ValueMember = nameof(ContractOption.Id);
            _customer.DataSource = lookups.Customers;
            _priceList.DisplayMember = nameof(ContractOption.Name);
            _priceList.ValueMember = nameof(ContractOption.Id);
            _priceList.DataSource = new[] { new ContractOption { Id = 0, Name = "Không chọn bảng giá" } }
                .Concat(lookups.PriceLists).ToList();

            if (_contract is not null) FillContract();
            else
            {
                _priceList.SelectedIndex = 0;
            }
            _save.Enabled = lookups.Customers.Count > 0;
            if (!_save.Enabled) _status.Text = "Vui lòng tạo khách hàng trước khi lập hợp đồng.";
        }
        catch (Exception ex)
        {
            _status.Text = "Lỗi tải dữ liệu: " + ex.Message;
        }
    }

    private void FillContract()
    {
        if (_contract is null) return;
        _code.Text = _contract.Code;
        _customer.SelectedValue = _contract.CustomerId;
        _start.Value = _contract.StartDate;
        _hasEnd.Checked = _contract.EndDate.HasValue;
        if (_contract.EndDate.HasValue) _end.Value = _contract.EndDate.Value;
        _priceList.SelectedValue = _contract.PriceListId ?? 0;
        _terms.Text = _contract.Terms ?? "";
    }

    private void ShowValidation(Control input, string message)
    {
        _status.Text = message;
        _errors.SetError(input, message);
        input.Focus();
    }

    private async Task SaveAsync()
    {
        if (_saving || _readOnly) return;
        _errors.Clear();
        _status.Text = "";
        if (string.IsNullOrWhiteSpace(_code.Text)) { ShowValidation(_code, "Vui lòng nhập mã hợp đồng."); return; }
        if (_customer.SelectedValue is not int customerId) { ShowValidation(_customer, "Vui lòng chọn khách hàng."); return; }
        var endDate = _hasEnd.Checked ? _end.Value.Date : (DateTime?)null;
        if (endDate.HasValue && endDate.Value < _start.Value.Date) { ShowValidation(_end, "Ngày kết thúc không được trước ngày bắt đầu."); return; }
        var priceListId = _priceList.SelectedValue is int id && id > 0 ? id : (int?)null;

        _saving = true;
        _save.Enabled = false;
        _status.Text = "Đang lưu hợp đồng...";
        try
        {
            if (_contract is null)
                await _api.Contracts.CreateContractAsync(customerId, _code.Text.Trim(), _start.Value.Date, endDate, _terms.Text, priceListId);
            else
                await _api.Contracts.UpdateContractAsync(_contract.Id, customerId, _code.Text.Trim(), _start.Value.Date, endDate, _terms.Text, priceListId);
            _saving = false;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            _status.Text = "Lỗi: " + ex.Message;
        }
        finally { _saving = false; if (!IsDisposed) _save.Enabled = true; }
    }
}

public sealed class CreateContractForm(ApiServices api) : ContractForm(api) { }

public sealed class EditContractForm(ApiServices api, ContractListItem contract) : ContractForm(api, contract) { }
