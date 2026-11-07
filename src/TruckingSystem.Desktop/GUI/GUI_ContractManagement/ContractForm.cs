using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_ContractManagement;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Desktop.GUI.GUI_ContractManagement;

public class ContractForm : BaseForm
{
    private readonly ApiServices _api;
    private readonly ContractListItem? _contract;
    private readonly bool _readOnly;
    private bool _saving;
    private int _maxDiscount = 20;
    private readonly Label _discountLabel = new() { AutoSize = true, Anchor = AnchorStyles.Left };
    private readonly TextBox _code = new();
    private readonly ComboBox _customer = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker _start = new() { Format = DateTimePickerFormat.Short };
    private readonly CheckBox _hasEnd = new() { Text = "Có ngày kết thúc", AutoSize = true };
    private readonly DateTimePicker _end = new() { Format = DateTimePickerFormat.Short, Enabled = false };
    private readonly ComboBox _type = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumberBox _discount = new() { Enabled = false, DecimalPlaces = 2, Maximum = 100 };
    private readonly ComboBox _priceList = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _status = new() { Dock = DockStyle.Fill, ForeColor = Theme.Danger };
    private readonly Button _save = Theme.MakePrimaryButton("Lưu hợp đồng", 150);

    public ContractForm(ApiServices api, ContractListItem? contract = null, bool readOnly = false)
    {
        _api = api;
        _contract = contract;
        _readOnly = readOnly;
        if (contract is null)
        {
            _start.MinDate = _end.MinDate = DateTime.UtcNow.AddHours(7).Date;
            _start.Value = _end.Value = _start.MinDate;
        }
        _code.ReadOnly = contract is null;
        if (contract is null) _code.Text = "Tự sinh khi lưu: HD-[Khách hàng]-[Năm]-[STT]";
        Text = readOnly ? "Chi tiết hợp đồng" : contract is null ? "Thêm hợp đồng mới" : "Sửa hợp đồng";
        ClientSize = new Size(760, 640);
        BackColor = Theme.CardBg;
        MinimumSize = new Size(600, 460);
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, Padding = new Padding(20, 16, 20, 16), ColumnCount = 2, RowCount = 10,
            AutoScroll = true,
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64));
        for (var i = 0; i < 10; i++) layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        void Section(int row, string text)
        {
            var title = new Label { Text = text, AutoSize = true, Margin = new Padding(0, 10, 0, 6) };
            FormStyle.Section(title);
            layout.Controls.Add(title, 0, row);
            layout.SetColumnSpan(title, 2);
        }
        Section(0, "① Khách hàng và thời hạn");
        Section(5, "② Giá và ưu đãi");

        AddRow(layout, 1, contract is null ? "Mã hợp đồng (tự động):" : "Mã hợp đồng *:", _code);
        AddRow(layout, 2, "Khách hàng *:", _customer);
        AddRow(layout, 3, "Ngày bắt đầu *:", _start);
        var endPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        endPanel.AutoSize = true;
        _end.Width = 160;
        endPanel.Controls.Add(_hasEnd);
        endPanel.Controls.Add(_end);
        _hasEnd.CheckedChanged += (_, _) => _end.Enabled = _hasEnd.Checked;
        AddRow(layout, 4, "Ngày kết thúc", endPanel);
        _type.Items.AddRange(new object[] { "Hợp đồng vận chuyển một lần", "Hợp đồng vận chuyển dài hạn" });
        _type.SelectedIndexChanged += (_, _) =>
        {
            _discount.Visible = true;
            _discount.Enabled = !_readOnly && _type.SelectedIndex == 1;
            if (_type.SelectedIndex != 1) _discount.Value = 0;
            if (_type.SelectedIndex == 0 && !_readOnly) _hasEnd.Checked = true;
            _hasEnd.Enabled = !_readOnly && _type.SelectedIndex != 0;
            layout.GetControlFromPosition(0, 4)!.Text = _type.SelectedIndex == 0 ? "Ngày kết thúc *:" : "Ngày kết thúc:";
        };
        AddRow(layout, 6, "Loại hợp đồng *:", _type);
        AddRow(layout, 7, "% ưu đãi của hợp đồng", _discount);
        layout.GetControlFromPosition(0, 7)!.Dispose();
        _discountLabel.Text = "% ưu đãi của hợp đồng:";
        layout.Controls.Add(_discountLabel, 0, 7);
        AddRow(layout, 8, "Bảng giá áp dụng *:", _priceList);

        _save.Click += async (_, _) => await SaveAsync();
        var cancel = Theme.MakeSecondaryButton("Đóng", 100);
        cancel.DialogResult = DialogResult.Cancel;
        FormStyle.SaveButton(_save); // chân [Đóng] [💾 Lưu hợp đồng]
        _status.AutoSize = true;
        layout.SetColumnSpan(_status, 2);
        layout.Controls.Add(_status, 0, 9);
        Controls.Add(layout);
        FormStyle.EditorFooter(this, cancel, _save);
        DialogKeys.Enable(this);
        Anim.FadeIn(this);
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
            _code.ReadOnly = true;
            FormStyle.NoTab(_code);
            _type.Enabled = _discount.Enabled = _customer.Enabled = _start.Enabled = _hasEnd.Enabled = _end.Enabled = _priceList.Enabled = false;
            _hasEnd.Visible = _end.Visible = false;
            endPanel.Controls.Add(new Label { Text = contract.EndDate?.ToString("dd/MM/yyyy") ?? "Không thời hạn", AutoSize = true, ForeColor = Theme.TextPrimary });
            _save.Visible = false;
            AcceptButton = cancel;
            _status.ForeColor = Theme.TextSecondary;
            _status.Text = $"Trạng thái: {contract.StatusText}\nTạo: {contract.CreatedAt:dd/MM/yyyy HH:mm} · Cập nhật: {contract.UpdatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "—"}";
            Shown += async (_, _) => await LoadDiscountLimitAsync();
        }
        else Shown += async (_, _) => await LoadLookupsAsync();
    }

    private static void AddRow(TableLayoutPanel layout, int row, string label, Control input)
    {
        var caption = new Label { Text = label.EndsWith(':') ? label : label + ":", AutoSize = true, ForeColor = Theme.TextPrimary, Anchor = AnchorStyles.Left };
        input.Dock = DockStyle.Fill;
        input.Margin = new Padding(0, 4, 0, 4);
        input.TabIndex = row;
        input.AccessibleName = label.TrimEnd(' ', '*', ':');
        if (input is TextBox textBox && textBox.Multiline) input.Height = 90;
        layout.Controls.Add(caption, 0, row);
        layout.Controls.Add(input, 1, row);
    }

    private async Task LoadDiscountLimitAsync()
    {
        try
        {
            var limit = await _api.Contracts.GetMaxDiscountPercentAsync();
            if (IsDisposed) return;
            _maxDiscount = limit;
            _discountLabel.Text = $"% ưu đãi của hợp đồng:\n(0–{_maxDiscount}%)";
        }
        catch (Exception)
        {
            if (!IsDisposed) _discountLabel.Text = "% ưu đãi của hợp đồng:\n(Không tải được hạn mức)";
        }
    }

    private async Task LoadLookupsAsync()
    {
        _save.Enabled = false;
        try
        {
            var lookups = await _api.Contracts.GetLookupsAsync();
            if (IsDisposed) return;
            _maxDiscount = lookups.MaxDiscountPercent;
            _discountLabel.Text = $"% ưu đãi của hợp đồng:\n(0–{_maxDiscount}%)";
            _customer.DisplayMember = nameof(ContractOption.Name);
            _customer.ValueMember = nameof(ContractOption.Id);
            _customer.DataSource = lookups.Customers;
            _priceList.DisplayMember = nameof(ContractOption.Name);
            _priceList.ValueMember = nameof(ContractOption.Id);
            var priceLists = lookups.PriceLists.ToList();
            if (_contract is not null && !priceLists.Any(p => p.Id == (_contract.PriceListId ?? 0)))
                priceLists.Insert(0, new ContractOption { Id = _contract.PriceListId ?? 0,
                    Name = _contract.PriceListName is null ? "Chưa có bảng giá (hợp đồng cũ)" : _contract.PriceListName + " (ngừng áp dụng)" });
            _priceList.DataSource = priceLists;

            if (_contract is not null) FillContract();
            else
            {
                _priceList.SelectedIndex = lookups.PriceLists.Count > 0 ? 0 : -1;
            }
            _save.Enabled = lookups.Customers.Count > 0 && (_contract is not null || lookups.PriceLists.Count > 0);
            if (lookups.Customers.Count == 0) FormStyle.Status(_status, "Vui lòng tạo khách hàng trước khi lập hợp đồng.", StatusKind.Warning);
            else if (_contract is null && lookups.PriceLists.Count == 0)
                FormStyle.Status(_status, "Chưa có bảng giá đang áp dụng. Vui lòng tạo hoặc kích hoạt bảng giá trước khi lập hợp đồng.", StatusKind.Warning);
        }
        catch (Exception ex)
        {
            FormStyle.Status(_status, "Lỗi tải dữ liệu: " + ex.Message);
        }
    }

    private void FillContract()
    {
        if (_contract is null) return;
        _type.SelectedIndex = _contract.Type switch { "OneTime" => 0, "LongTerm" => 1, _ => -1 };
        _type.Text = _contract.Type is null ? "" : _contract.TypeText;
        _discount.Value = _contract.Type == "LongTerm" ? Math.Clamp(_contract.DiscountPercent ?? 0, 0, 100) : 0;
        if (_contract.Type is null) FormStyle.Status(_status, "Hợp đồng cũ chưa phân loại. Có thể giữ nguyên hoặc chọn loại hợp đồng.", StatusKind.Warning);
        _code.Text = _contract.Code;
        _customer.SelectedValue = _contract.CustomerId;
        _start.Value = _contract.StartDate;
        _hasEnd.Checked = _contract.EndDate.HasValue || !_readOnly && _contract.Type == "OneTime";
        if (_contract.EndDate.HasValue) _end.Value = _contract.EndDate.Value;
        _priceList.SelectedValue = _contract.PriceListId ?? 0;
        if (!_readOnly && _contract.PriceListId.HasValue && _priceList.SelectedIndex < 0)
            FormStyle.Status(_status, $"Bảng giá hiện tại: {_contract.PriceListName} (đã ngừng áp dụng). Giữ nguyên nếu không chọn bảng giá khác.", StatusKind.Warning);
    }

    private int? SelectedPriceListId => _priceList.SelectedIndex < 0 ? _contract?.PriceListId
        : _priceList.SelectedValue is int id && id > 0 ? id : null;

    private void ShowValidation(Control input, string message) => FieldHint.Show(input, _status, message);

    private async Task SaveAsync()
    {
        if (_saving || _readOnly) return;
        _status.Text = "";
        if (_contract is not null && string.IsNullOrWhiteSpace(_code.Text)) { ShowValidation(_code, "Vui lòng nhập mã hợp đồng."); return; }
        if (_customer.SelectedValue is not int customerId) { ShowValidation(_customer, "Vui lòng chọn khách hàng."); return; }
        var endDate = _hasEnd.Checked ? _end.Value.Date : (DateTime?)null;
        var type = _type.SelectedIndex switch { 0 => "OneTime", 1 => "LongTerm", _ => null };
        if (type is null && (_contract is null || _contract.Type is not null))
        { ShowValidation(_type, "Vui lòng chọn loại hợp đồng."); return; }
        try
        {
            Contract.ValidatePeriod(_start.Value.Date, endDate,
                type == "OneTime" ? ContractType.OneTime : type == "LongTerm" ? ContractType.LongTerm : null,
                _contract is null ? DateTime.UtcNow.AddHours(7).Date : null);
        }
        catch (ArgumentException ex)
        { ShowValidation(_contract is null && _start.Value.Date < DateTime.UtcNow.AddHours(7).Date ? _start : _end, ex.Message); return; }
        decimal? discountPercent = null;
        if (type == "LongTerm") discountPercent = _discount.Value; // ô số đã giới hạn 0–100, 2 chữ số lẻ
        if (!_api.Connection.IsInRole("Admin") && discountPercent > _maxDiscount
            && (_contract is null || discountPercent != _contract.DiscountPercent))
        { ShowValidation(_discount, $"Chỉ Admin được áp dụng ưu đãi vượt quá {_maxDiscount}%."); return; }
        var priceListId = SelectedPriceListId;
        if (_contract is null && !priceListId.HasValue)
        { ShowValidation(_priceList, "Vui lòng chọn bảng giá đang áp dụng."); return; }

        _saving = true;
        _save.Enabled = false;
        FormStyle.Status(_status, "Đang lưu hợp đồng...", StatusKind.Info);
        try
        {
            if (_contract is null)
                await _api.Contracts.CreateContractAsync(customerId, _code.Text.Trim(), _start.Value.Date, endDate, null, priceListId, null, type, discountPercent);
            else
                await _api.Contracts.UpdateContractAsync(_contract.Id, customerId, _code.Text.Trim(), _start.Value.Date, endDate, _contract.Terms, priceListId, null, type, discountPercent);
            _saving = false;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            FormStyle.Status(_status, "Lỗi: " + ex.Message);
        }
        finally { _saving = false; if (!IsDisposed) _save.Enabled = true; }
    }
}

public sealed class CreateContractForm(ApiServices api) : ContractForm(api) { }

public sealed class EditContractForm(ApiServices api, ContractListItem contract) : ContractForm(api, contract) { }
