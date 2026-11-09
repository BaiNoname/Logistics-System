using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_ContractManagement;

namespace TruckingSystem.Desktop.GUI.GUI_ContractManagement;

public class ContractAppendixForm : BaseForm
{
    private readonly ApiServices _api;
    private readonly ContractAppendixItem? _item;
    private readonly int? _initialContractId;
    private readonly TextBox _code = new() { MaxLength = 100, AccessibleName = "Mã phụ lục" };
    private readonly ComboBox _contract = new() { DropDownStyle = ComboBoxStyle.DropDownList, AccessibleName = "Mã hợp đồng" };
    private readonly DateTimePicker _issue = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", AccessibleName = "Ngày lập" };
    private readonly DateTimePicker _effective = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", AccessibleName = "Ngày có hiệu lực" };
    private readonly ComboBox _priceList = new() { DropDownStyle = ComboBoxStyle.DropDownList, AccessibleName = "Bảng giá" };
    private readonly NumberBox _discount = new() { DecimalPlaces = 2, Maximum = 100, AccessibleName = "% ưu đãi của hợp đồng" };
    private readonly Label _status = new() { AutoSize = true, MaximumSize = new Size(650, 0), ForeColor = Theme.Danger };
    private readonly Button _save = Theme.MakePrimaryButton("Lưu", 100);
    private readonly TableLayoutPanel _fields = new() { Dock = DockStyle.Fill, AutoScroll = true, ColumnCount = 1, RowCount = 7, Padding = new Padding(20, 16, 20, 16) };
    private bool _saving;
    private List<AppendixContractOption> _contracts = new();
    private int _maxDiscount;

    public int? SavedId { get; private set; }

    public ContractAppendixForm(ApiServices api, ContractAppendixItem? item = null, int? contractId = null)
    {
        _api = api;
        _item = item;
        _initialContractId = contractId;
        Text = item is null ? "Thêm phụ lục hợp đồng" : "Sửa phụ lục hợp đồng";
        ClientSize = new Size(720, 670);
        BackColor = Theme.CardBg;
        MinimumSize = new Size(520, 460);
        MaximizeBox = MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        _fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var row = 0; row < 7; row++) _fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
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
        _code.ReadOnly = item is null;
        _code.Text = item?.Code ?? "Tự sinh khi lưu: PL-[STT]-[Mã hợp đồng]";
        _issue.Value = item?.IssueDate ?? DateTime.UtcNow.AddHours(7).Date;
        _effective.Value = item?.EffectiveDate ?? DateTime.UtcNow.AddHours(7).Date;
        var today = DateTime.UtcNow.AddHours(7).Date;
        _issue.MinDate = item?.IssueDate.Date < today ? item.IssueDate.Date : today;
        _effective.MinDate = item?.EffectiveDate.Date < today ? item.EffectiveDate.Date : today;
        Field(item is null ? "Mã phụ lục (tự động):" : "Mã phụ lục *:", _code);
        Field("Mã hợp đồng *:", _contract);
        Field("Ngày lập *:", _issue);
        Field("Ngày có hiệu lực *:", _effective);
        Field("Bảng giá *:", _priceList);
        Field("% ưu đãi của hợp đồng *:", _discount);
        _fields.Controls.Add(_status);
        _priceList.SelectedIndexChanged += (_, _) => UpdateSaveEnabled();
        _contract.SelectedIndexChanged += (_, _) =>
        {
            if (_item is null)
                _code.Text = "Tự sinh khi lưu: PL-[STT]-" + ((_contract.SelectedItem as ContractOption)?.Name ?? "[Mã hợp đồng]");
            var selected = SelectedContract;
            _discount.Value = Math.Clamp(selected?.DiscountPercent ?? 0, 0, 100);
            _discount.Enabled = selected is not null && selected.Type != "OneTime";
            if (selected?.Type == "OneTime") _discount.Value = 0;
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
        FormStyle.Status(_status, "Đang tải hợp đồng và ưu đãi...", StatusKind.Info);
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
        _contracts = lookups.ContractDiscounts;
        _maxDiscount = lookups.MaxDiscountPercent;
        _contract.DisplayMember = nameof(ContractOption.Name);
        _contract.ValueMember = nameof(ContractOption.Id);
        var contracts = lookups.Contracts.ToList();
        if (_item is not null && !contracts.Any(c => c.Id == _item.ContractId))
            contracts.Insert(0, new ContractOption { Id = _item.ContractId, Name = $"{_item.ContractCode} ({_item.ContractStatusText})" });
        _contract.DataSource = contracts;
        if (_initialContractId.HasValue && _item is null)
        {
            _contract.SelectedValue = _initialContractId.Value;
            if (!contracts.Any(c => c.Id == _initialContractId)) _contract.SelectedIndex = -1;
            _contract.Enabled = false;
        }
        _priceList.DisplayMember = nameof(ContractOption.Name);
        _priceList.ValueMember = nameof(ContractOption.Id);
        var priceLists = lookups.PriceLists.ToList();
        var savedPriceListId = _item?.PriceListId ?? _item?.TariffRule?.PriceListId;
        if (savedPriceListId.HasValue && !priceLists.Any(p => p.Id == savedPriceListId))
            priceLists.Insert(0, new ContractOption { Id = savedPriceListId.Value, Name = (_item?.PriceListName ?? _item?.TariffRule?.PriceListName ?? "Bảng giá đã lưu") + " (ngừng áp dụng)" });
        _priceList.DataSource = priceLists;
        _priceList.SelectedIndex = -1;
        if (_item is not null)
        {
            _contract.SelectedValue = _item.ContractId;
            if (savedPriceListId.HasValue) _priceList.SelectedValue = savedPriceListId.Value;
            _discount.Value = Math.Clamp(_item.DiscountPercent ?? SelectedContract?.DiscountPercent ?? 0, 0, 100);
        }
        _status.Text = "";
        _fields.Enabled = true;
        UpdateSaveEnabled();
        if (_initialContractId.HasValue && _contract.SelectedIndex < 0)
            FormStyle.Status(_status, "Hợp đồng đã chọn không còn đang hiệu lực. Vui lòng làm mới danh sách hợp đồng.", StatusKind.Warning);
        else if (contracts.Count == 0)
            FormStyle.Status(_status, "Cần có hợp đồng đang hiệu lực để lập phụ lục.", StatusKind.Warning);
        else if (lookups.PriceLists.Count == 0 && _item is null)
            FormStyle.Status(_status, "Chưa có bảng giá đang áp dụng để lập phụ lục.", StatusKind.Warning);
    }

    private AppendixContractOption? SelectedContract => _contract.SelectedValue is int id ? _contracts.FirstOrDefault(c => c.Id == id) : null;

    private void UpdateSaveEnabled()
        => _save.Enabled = !_saving && SelectedContract is not null && _priceList.SelectedValue is int id && id > 0;

    private async Task SaveAsync()
    {
        if (_saving) return;
        var contractId = _contract.SelectedValue is int c ? c : 0;
        var priceListId = _priceList.SelectedValue is int p ? p : 0;
        var tariffRuleId = priceListId == _item?.TariffRule?.PriceListId ? _item.TariffRule.Id : (int?)null;
        try
        {
            TruckingSystem.Domain.Entities.ContractAppendix.Validate(_item is null ? "Tự động" : _code.Text, contractId, _issue.Value.Date, _effective.Value.Date, tariffRuleId, priceListId,
                DateTime.UtcNow.AddHours(7).Date, _item?.IssueDate, _item?.EffectiveDate);
            var type = SelectedContract?.Type == "OneTime" ? TruckingSystem.Domain.Enums.ContractType.OneTime : TruckingSystem.Domain.Enums.ContractType.LongTerm;
            TruckingSystem.Domain.Entities.Contract.ValidateDiscount(type, _discount.Value);
            if (!_api.Connection.IsInRole("Admin") && _discount.Value > _maxDiscount)
            { FieldHint.Show(_discount, _status, $"Ưu đãi không được vượt quá {_maxDiscount}%."); return; }
        }
        catch (ArgumentException ex)
        {
            // Báo lỗi ngay tại ô sai (thứ tự kiểm tra giống thứ tự ô trên màn hình).
            Control field = ex.ParamName == "issueDate" ? _issue : ex.ParamName == "effectiveDate" ? _effective
                : string.IsNullOrWhiteSpace(_code.Text) || _code.Text.Trim().Length > 100 ? _code
                : contractId <= 0 ? _contract
                : priceListId <= 0 ? _priceList : _discount;
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
                var saved = await _api.ContractAppendices.CreateAppendixAsync("", contractId, _issue.Value.Date, _effective.Value.Date, tariffRuleId, _discount.Value, priceListId);
                SavedId = saved?.Id;
            }
            else
            {
                await _api.ContractAppendices.UpdateAppendixAsync(_item.Id, _code.Text.Trim(), contractId, _issue.Value.Date, _effective.Value.Date, tariffRuleId, _discount.Value, priceListId);
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
