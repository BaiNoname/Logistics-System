using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_ContractManagement;

namespace TruckingSystem.Desktop.GUI.GUI_ContractManagement;

public class ContractForm : BaseForm
{
    private readonly ApiServices _api;
    private readonly ContractListItem? _contract;
    private readonly TextBox _code = new();
    private readonly ComboBox _customer = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker _start = new() { Format = DateTimePickerFormat.Short };
    private readonly CheckBox _hasEnd = new() { Text = "Có ngày kết thúc", AutoSize = true };
    private readonly DateTimePicker _end = new() { Format = DateTimePickerFormat.Short, Enabled = false };
    private readonly ComboBox _priceList = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _terms = new() { Multiline = true, ScrollBars = ScrollBars.Vertical };
    private readonly Label _status = new() { AutoSize = true, ForeColor = Theme.Danger };
    private readonly Button _save = Theme.MakePrimaryButton("Lưu hợp đồng", 150);

    public ContractForm(ApiServices api, ContractListItem? contract = null)
    {
        _api = api;
        _contract = contract;
        Text = contract is null ? "Thêm hợp đồng mới" : "Sửa hợp đồng";
        ClientSize = new Size(500, 540);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, Padding = new Padding(24), ColumnCount = 2, RowCount = 7,
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64));
        for (var i = 0; i < 7; i++)
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, i == 5 ? 130 : i == 6 ? 56 : 52));

        AddRow(layout, 0, "Mã hợp đồng *", _code);
        AddRow(layout, 1, "Khách hàng *", _customer);
        AddRow(layout, 2, "Ngày bắt đầu *", _start);
        var endPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        endPanel.Controls.Add(_hasEnd);
        endPanel.Controls.Add(_end);
        _hasEnd.CheckedChanged += (_, _) => _end.Enabled = _hasEnd.Checked;
        AddRow(layout, 3, "Ngày kết thúc", endPanel);
        _priceList.Items.Add(new ContractOption { Id = 0, Name = "Không chọn bảng giá" });
        AddRow(layout, 4, "Bảng giá", _priceList);
        AddRow(layout, 5, "Điều khoản", _terms);

        var footer = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        _save.Click += async (_, _) => await SaveAsync();
        var cancel = Theme.MakeSecondaryButton("Hủy", 100);
        cancel.DialogResult = DialogResult.Cancel;
        footer.Controls.Add(_save);
        footer.Controls.Add(cancel);
        footer.Controls.Add(_status);
        layout.SetColumnSpan(footer, 2);
        layout.Controls.Add(footer, 0, 6);
        Controls.Add(layout);
        AcceptButton = _save;
        CancelButton = cancel;
        Shown += async (_, _) => await LoadLookupsAsync();
    }

    private static void AddRow(TableLayoutPanel layout, int row, string label, Control input)
    {
        var caption = new Label { Text = label, AutoSize = true, ForeColor = Theme.TextSecondary, Anchor = AnchorStyles.Left };
        input.Dock = DockStyle.Fill;
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
            foreach (var priceList in lookups.PriceLists) _priceList.Items.Add(priceList);
            _priceList.DisplayMember = nameof(ContractOption.Name);
            _priceList.ValueMember = nameof(ContractOption.Id);

            if (_contract is not null)
            {
                _code.Text = _contract.Code;
                _customer.SelectedValue = _contract.CustomerId;
                _start.Value = _contract.StartDate;
                _hasEnd.Checked = _contract.EndDate.HasValue;
                if (_contract.EndDate.HasValue) _end.Value = _contract.EndDate.Value;
                _priceList.SelectedValue = _contract.PriceListId ?? 0;
                _terms.Text = _contract.Terms ?? "";
            }
            else
            {
                _priceList.SelectedIndex = 0;
            }
            _save.Enabled = true;
        }
        catch (Exception ex)
        {
            _status.Text = "Lỗi tải dữ liệu: " + ex.Message;
        }
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_code.Text)) { _status.Text = "Vui lòng nhập mã hợp đồng."; return; }
        if (_customer.SelectedValue is not int customerId) { _status.Text = "Vui lòng chọn khách hàng."; return; }
        var endDate = _hasEnd.Checked ? _end.Value.Date : (DateTime?)null;
        if (endDate.HasValue && endDate.Value < _start.Value.Date) { _status.Text = "Ngày kết thúc phải sau ngày bắt đầu."; return; }
        var priceListId = _priceList.SelectedValue is int id && id > 0 ? id : (int?)null;

        _save.Enabled = false;
        try
        {
            if (_contract is null)
                await _api.Contracts.CreateContractAsync(customerId, _code.Text.Trim(), _start.Value.Date, endDate, _terms.Text, priceListId);
            else
                await _api.Contracts.UpdateContractAsync(_contract.Id, customerId, _code.Text.Trim(), _start.Value.Date, endDate, _terms.Text, priceListId);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            _status.Text = "Lỗi: " + ex.Message;
            _save.Enabled = true;
        }
    }
}

public sealed class CreateContractForm(ApiServices api) : ContractForm(api) { }

public sealed class EditContractForm(ApiServices api, ContractListItem contract) : ContractForm(api, contract) { }
