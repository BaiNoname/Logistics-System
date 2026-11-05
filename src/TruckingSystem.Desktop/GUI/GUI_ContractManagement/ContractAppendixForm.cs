using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_ContractManagement;

namespace TruckingSystem.Desktop.GUI.GUI_ContractManagement;

public class ContractAppendixForm : BaseForm
{
    private readonly ApiServices _api;
    private readonly ContractAppendixItem? _item;
    private readonly TextBox _code = new() { MaxLength = 100, AccessibleName = "Mã phụ lục" };
    private readonly ComboBox _contract = new() { DropDownStyle = ComboBoxStyle.DropDownList, AccessibleName = "Mã hợp đồng" };
    private readonly DateTimePicker _issue = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", AccessibleName = "Ngày lập" };
    private readonly DateTimePicker _effective = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", AccessibleName = "Ngày có hiệu lực" };
    private readonly ComboBox _priceList = new() { DropDownStyle = ComboBoxStyle.DropDownList, AccessibleName = "Bảng giá" };
    private readonly ComboBox _tariff = new() { DropDownStyle = ComboBoxStyle.DropDownList, DropDownWidth = 950, AccessibleName = "Chi tiết bảng giá" };
    private readonly Label _prices = new() { AutoSize = true, MaximumSize = new Size(650, 0), ForeColor = Theme.TextSecondary, AccessibleName = "Đơn giá vận chuyển" };
    private readonly Label _status = new() { AutoSize = true, MaximumSize = new Size(650, 0), ForeColor = Theme.Danger };
    private readonly Button _save = Theme.MakePrimaryButton("Lưu", 100);
    private readonly TableLayoutPanel _fields = new() { Dock = DockStyle.Fill, AutoScroll = true, ColumnCount = 1, RowCount = 8, Padding = new Padding(20, 16, 20, 16) };
    private bool _saving;
    private List<AppendixTariffOption> _tariffRules = new();

    public int? SavedId { get; private set; }

    public ContractAppendixForm(ApiServices api, ContractAppendixItem? item = null)
    {
        _api = api;
        _item = item;
        Text = item is null ? "Thêm phụ lục hợp đồng" : "Sửa phụ lục hợp đồng";
        ClientSize = new Size(720, 670);
        BackColor = Theme.CardBg;
        MinimumSize = new Size(520, 460);
        MaximizeBox = MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        _fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var row = 0; row < 8; row++) _fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        void Field(string label, Control input)
        {
            var block = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, Margin = new Padding(0, 0, 0, 8) };
            block.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            block.Controls.Add(new Label { Text = label, AutoSize = true, ForeColor = Theme.TextPrimary }, 0, 0);
            input.Dock = DockStyle.Top;
            block.Controls.Add(input, 0, 1);
            block.TabIndex = _fields.Controls.Count;
            _fields.Controls.Add(block);
        }
        _code.Text = item?.Code ?? "";
        _issue.Value = item?.IssueDate ?? DateTime.UtcNow.AddHours(7).Date;
        _effective.Value = item?.EffectiveDate ?? DateTime.UtcNow.AddHours(7).Date;
        Field("Mã phụ lục *:", _code);
        Field("Mã hợp đồng *:", _contract);
        Field("Ngày lập *:", _issue);
        Field("Ngày có hiệu lực *:", _effective);
        Field("Bảng giá *:", _priceList);
        Field("Chi tiết bảng giá *:", _tariff);
        _fields.Controls.Add(_prices);
        _fields.Controls.Add(_status);
        _priceList.SelectedIndexChanged += (_, _) => BindTariffRules();
        _contract.SelectedIndexChanged += (_, _) => UpdateSaveEnabled();
        _tariff.SelectedIndexChanged += (_, _) =>
        {
            _prices.Text = (_tariff.SelectedItem as AppendixTariffOption)?.Details ?? "";
            UpdateSaveEnabled();
        };
        var cancel = Theme.MakeSecondaryButton("Đóng", 100);
        cancel.DialogResult = DialogResult.Cancel;
        _save.Text = "Lưu phụ lục";
        _save.Width = 150;
        FormStyle.SaveButton(_save); // chân [Đóng] [💾 Lưu phụ lục]
        Controls.Add(_fields);
        FormStyle.EditorFooter(this, cancel, _save);
        DialogKeys.Enable(this);
        Anim.FadeIn(this);
        AcceptButton = _save;
        CancelButton = cancel;
        _save.Enabled = _fields.Enabled = false;
        _save.Click += async (_, _) => await SaveAsync();
        Shown += async (_, _) => await LoadLookupsAsync();
        FormClosing += (_, e) => e.Cancel = _saving;
    }

    private async Task LoadLookupsAsync()
    {
        FormStyle.Status(_status, "Đang tải hợp đồng và chi tiết bảng giá...", StatusKind.Info);
        try
        {
            var lookups = await _api.ContractAppendices.GetLookupsAsync();
            if (IsDisposed) return;
            BindLookups(lookups);
        }
        catch (Exception ex) { if (!IsDisposed) FormStyle.Status(_status, "Lỗi tải dữ liệu: " + ex.Message); }
    }

    private void BindLookups(ContractAppendixLookups lookups)
    {
        _tariffRules = lookups.TariffRules;
        _contract.DisplayMember = nameof(ContractOption.Name);
        _contract.ValueMember = nameof(ContractOption.Id);
        _contract.DataSource = lookups.Contracts;
        _priceList.DisplayMember = nameof(ContractOption.Name);
        _priceList.ValueMember = nameof(ContractOption.Id);
        _priceList.DataSource = lookups.PriceLists;
        _priceList.SelectedIndex = -1;
        BindTariffRules();
        if (_item is not null)
        {
            _contract.SelectedValue = _item.ContractId;
            _priceList.SelectedValue = _item.TariffRule.PriceListId;
            if (_priceList.SelectedIndex >= 0) _tariff.SelectedValue = _item.TariffRule.Id;
        }
        _fields.Enabled = true;
        UpdateSaveEnabled();
        if (lookups.Contracts.Count == 0)
            FormStyle.Status(_status, "Cần có hợp đồng để lập phụ lục.", StatusKind.Warning);
        else if (lookups.PriceLists.Count == 0 && _item is null)
            FormStyle.Status(_status, "Chưa có bảng giá đang áp dụng để lập phụ lục.", StatusKind.Warning);
    }

    private void BindTariffRules()
    {
        var priceListId = _priceList.SelectedValue is int id ? id : 0;
        var rules = _tariffRules.Where(r => r.PriceListId == priceListId).ToList();
        _tariff.DisplayMember = nameof(AppendixTariffOption.Name);
        _tariff.ValueMember = nameof(AppendixTariffOption.Id);
        _tariff.DataSource = rules;
        _tariff.SelectedIndex = -1;
        _tariff.Enabled = rules.Count > 0;
        _prices.Text = "";
        FormStyle.Status(_status, priceListId > 0 && rules.Count == 0 ? "Bảng giá này chưa có chi tiết. Vui lòng chọn bảng giá khác." : "", StatusKind.Warning);
        if (_priceList.SelectedIndex < 0 && _item is not null)
        {
            _prices.Text = $"Bảng giá hiện tại: {_item.TariffRule.Name}\n{_item.TariffRule.Details}";
            FormStyle.Status(_status, "Giữ nguyên giá đã lưu nếu không chọn bảng giá đang áp dụng khác.", StatusKind.Info);
        }
        UpdateSaveEnabled();
    }

    private int SelectedTariffRuleId => _priceList.SelectedIndex < 0 ? _item?.TariffRule.Id ?? 0
        : _tariff.SelectedValue is int id ? id : 0;

    private void UpdateSaveEnabled()
        => _save.Enabled = !_saving && _contract.SelectedItem is ContractOption && SelectedTariffRuleId > 0;

    private async Task SaveAsync()
    {
        if (_saving) return;
        var contractId = _contract.SelectedValue is int c ? c : 0;
        var tariffRuleId = SelectedTariffRuleId;
        try
        {
            TruckingSystem.Domain.Entities.ContractAppendix.Validate(_code.Text, contractId, _issue.Value.Date, _effective.Value.Date, tariffRuleId);
        }
        catch (ArgumentException ex)
        {
            // Báo lỗi ngay tại ô sai (thứ tự kiểm tra giống thứ tự ô trên màn hình).
            Control field = string.IsNullOrWhiteSpace(_code.Text) || _code.Text.Trim().Length > 100 ? _code
                : contractId <= 0 ? _contract
                : tariffRuleId <= 0 ? (_priceList.SelectedIndex < 0 ? _priceList : _tariff)
                : _effective;
            FieldHint.Show(field, _status, ex.Message);
            return;
        }
        _saving = true;
        _fields.Enabled = _save.Enabled = false;
        FormStyle.Status(_status, "Đang lưu...", StatusKind.Info);
        try
        {
            if (_item is null)
            {
                var saved = await _api.ContractAppendices.CreateAppendixAsync(_code.Text.Trim(), contractId, _issue.Value.Date, _effective.Value.Date, tariffRuleId);
                SavedId = saved?.Id;
            }
            else
            {
                await _api.ContractAppendices.UpdateAppendixAsync(_item.Id, _code.Text.Trim(), contractId, _issue.Value.Date, _effective.Value.Date, tariffRuleId);
                SavedId = _item.Id;
            }
            _saving = false;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { FormStyle.Status(_status, "Lỗi: " + ex.Message); }
        finally
        {
            _saving = false;
            if (!IsDisposed) { _fields.Enabled = true; UpdateSaveEnabled(); }
        }
    }
}
