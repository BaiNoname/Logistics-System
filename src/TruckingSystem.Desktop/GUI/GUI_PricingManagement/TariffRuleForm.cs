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
        _perKm.Maximum = _perTon.Maximum = _fragile.Maximum = _hazmat.Maximum = TariffRule.MaximumPrice;
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
            _fragile.Value = item.SurchargeFragile;
            _hazmat.Value = item.SurchargeHazmat;
        }
        _save.Click += async (_, _) => await SaveAsync();
        FormClosing += (_, e) => e.Cancel = _saving;
    }

    private async Task SaveAsync()
    {
        if (_saving) return;
        _errors.Clear();
        ValidateChildren();
        try { TariffRule.Validate(_vehicleType.Text, _perKm.Value, _perTon.Value, _fragile.Value, _hazmat.Value); }
        catch (ArgumentException ex)
        {
            _errors.SetError(_vehicleType, ex.Message);
            _status.Text = ex.Message;
            _vehicleType.Focus();
            return;
        }
        _saving = true;
        _fields.Enabled = _save.Enabled = false;
        _status.Text = "Đang lưu...";
        try
        {
            if (_item is null)
                await _api.Pricing.CreateTariffRuleAsync(_priceList.Id, _vehicleType.Text.Trim(), _perKm.Value, _perTon.Value, _fragile.Value, _hazmat.Value);
            else
                await _api.Pricing.UpdateTariffRuleAsync(_priceList.Id, _item.Id, _vehicleType.Text.Trim(), _perKm.Value, _perTon.Value, _fragile.Value, _hazmat.Value);
            _saving = false;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { _status.Text = "Lỗi: " + ex.Message; }
        finally { _saving = false; if (!IsDisposed) _fields.Enabled = _save.Enabled = true; }
    }
}
