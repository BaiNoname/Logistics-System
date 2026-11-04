using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_PricingManagement;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Desktop.GUI.GUI_PricingManagement;

public partial class TariffRuleForm : BaseForm
{
    private readonly ApiServices _api = null!;
    private readonly PriceListItem _priceList = null!;
    private readonly TariffRuleItem? _item;
    private bool _saving;

    public TariffRuleForm()
    {
        InitializeComponent();
        _save.Text = "Lưu dòng giá";
        Controls.Remove(_footer);
        FormStyle.EditorFooter(this, _cancel, _save);
        _footer.Dispose();
        foreach (Control block in _fields.Controls)
        {
            block.Dock = DockStyle.Top;
            block.TabIndex = _fields.Controls.GetChildIndex(block);
            foreach (Control input in block.Controls)
                input.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            FormStyle.FieldLabels(block.Controls.OfType<Label>().ToArray());
        }
        DialogKeys.Enable(this);
        Anim.FadeIn(this);
        _perKm.Maximum = _perTon.Maximum = TariffRule.MaximumPrice;
        // Chân cửa sổ chuẩn: [Đóng] [💾 Lưu].
        FormStyle.SaveButton(_save);
        FormStyle.CloseButton(this, _cancel);
    }

    public TariffRuleForm(ApiServices api, PriceListItem priceList, IReadOnlyCollection<TariffRuleItem> rules, TariffRuleItem? item = null) : this()
    {
        _api = api;
        _priceList = priceList;
        _item = item;
        Text = item is null ? "Thêm chi tiết giá" : "Sửa chi tiết giá";
        _priceListLabel.Text = "Bảng giá: " + priceList.Name;
        var usedTypes = rules
            .Where(rule => rule.PriceListId == priceList.Id && rule.Id != item?.Id)
            .Select(rule => rule.VehicleType.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        for (var i = _vehicleType.Items.Count - 1; i >= 0; i--)
            if (usedTypes.Contains(_vehicleType.Items[i]!.ToString()!))
                _vehicleType.Items.RemoveAt(i);
        if (item is not null)
        {
            _vehicleType.Text = item.VehicleType;
            _perKm.Value = item.PricePerKm;
            _perTon.Value = item.PricePerTon;
        }
        _save.Click += async (_, _) => await SaveAsync();
        FormClosing += (_, e) => e.Cancel = _saving;
    }

    private async Task SaveAsync()
    {
        if (_saving) return;
        _errors.Clear();
        ValidateChildren();
        try { TariffRule.Validate(_vehicleType.Text, _perKm.Value, _perTon.Value, 0, 0); }
        catch (ArgumentException ex)
        {
            // Báo đúng ô sai: thiếu loại xe -> ô Loại xe; còn lại là lỗi đơn giá.
            Control field = string.IsNullOrWhiteSpace(_vehicleType.Text) ? _vehicleType
                : decimal.Round(_perKm.Value, 2) != _perKm.Value || _perKm.Value > TariffRule.MaximumPrice ? _perKm
                : _perTon;
            FieldHint.Show(field, _status, ex.Message);
            return;
        }
        _saving = true;
        _fields.Enabled = _save.Enabled = false;
        FormStyle.Status(_status, "Đang lưu...", StatusKind.Info);
        try
        {
            if (_item is null)
                await _api.Pricing.CreateTariffRuleAsync(_priceList.Id, _vehicleType.Text.Trim(), _perKm.Value, _perTon.Value);
            else
                await _api.Pricing.UpdateTariffRuleAsync(_priceList.Id, _item.Id, _vehicleType.Text.Trim(), _perKm.Value, _perTon.Value);
            _saving = false;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { FormStyle.Status(_status, "Lỗi: " + ex.Message); }
        finally { _saving = false; if (!IsDisposed) _fields.Enabled = _save.Enabled = true; }
    }
}
