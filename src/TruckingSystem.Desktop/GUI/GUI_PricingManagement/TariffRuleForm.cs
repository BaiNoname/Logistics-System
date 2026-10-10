using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_PricingManagement;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Desktop.GUI.GUI_PricingManagement;

public class TariffRuleForm : BaseForm
{
    private readonly ApiServices _api;
    private readonly PriceListItem _priceList;
    private readonly TariffRuleItem? _item;
    private readonly ComboBox _vehicleType = new() { DropDownStyle = ComboBoxStyle.DropDown, AccessibleName = "Loại xe", AutoCompleteMode = AutoCompleteMode.SuggestAppend, AutoCompleteSource = AutoCompleteSource.ListItems };
    private readonly NumericUpDown _perKm = MakePriceInput("Giá mỗi km");
    private readonly NumericUpDown _perTon = MakePriceInput("Giá mỗi tấn");
    private readonly NumericUpDown _fragile = MakePriceInput("Phụ phí hàng dễ vỡ");
    private readonly NumericUpDown _hazmat = MakePriceInput("Phụ phí hàng nguy hiểm");
    private readonly Button _save = Theme.MakePrimaryButton("Lưu", 100);
    private readonly Label _status = new() { AutoSize = true, MaximumSize = new Size(480, 0), ForeColor = Theme.Danger };
    private readonly ErrorProvider _errors = new() { BlinkStyle = ErrorBlinkStyle.NeverBlink };
    private readonly FlowLayoutPanel _fields = new() { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(24) };
    private bool _saving;

    public TariffRuleForm(ApiServices api, PriceListItem priceList, TariffRuleItem? item = null)
    {
        _api = api;
        _priceList = priceList;
        _item = item;
        _errors.ContainerControl = this;
        Text = item is null ? "Thêm chi tiết giá" : "Sửa chi tiết giá";
        ClientSize = new Size(540, 530);
        BackColor = Theme.CardBg;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        _fields.Controls.Add(new Label { Text = "Bảng giá: " + priceList.Name, AutoSize = true, MaximumSize = new Size(470, 0), ForeColor = Theme.TextPrimary, Font = Theme.FontBold, Margin = new Padding(0, 0, 0, 16) });
        _fields.Controls.Add(Theme.MakeLabeledInput("Loại xe * (chọn hoặc nhập)", _vehicleType, 470));
        _fields.Controls.Add(Theme.MakeLabeledInput("Giá/km (đ)", _perKm, 470));
        _fields.Controls.Add(Theme.MakeLabeledInput("Giá/tấn (đ)", _perTon, 470));
        _fields.Controls.Add(Theme.MakeLabeledInput("Phụ phí hàng dễ vỡ (đ)", _fragile, 470));
        _fields.Controls.Add(Theme.MakeLabeledInput("Phụ phí hàng nguy hiểm (đ)", _hazmat, 470));
        _fields.Controls.Add(_status);
        if (item is not null)
        {
            _vehicleType.Text = item.VehicleType;
            _perKm.Value = item.PricePerKm;
            _perTon.Value = item.PricePerTon;
            _fragile.Value = item.SurchargeFragile;
            _hazmat.Value = item.SurchargeHazmat;
        }
        var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 68, Padding = new Padding(24, 10, 24, 10), FlowDirection = FlowDirection.RightToLeft };
        var cancel = Theme.MakeSecondaryButton("Hủy", 100);
        cancel.DialogResult = DialogResult.Cancel;
        footer.Controls.AddRange(new Control[] { _save, cancel });
        Controls.Add(_fields);
        Controls.Add(footer);
        AcceptButton = _save;
        CancelButton = cancel;
        _save.Click += async (_, _) => await SaveAsync();
        FormClosing += (_, e) => e.Cancel = _saving;
        Shown += async (_, _) =>
        {
            try
            {
                var types = await _api.Pricing.GetVehicleTypesAsync();
                if (!IsDisposed) _vehicleType.Items.AddRange(types.Cast<object>().ToArray());
            }
            catch (Exception) { if (!IsDisposed) _status.Text = "Chưa tải được danh sách loại xe. Bạn vẫn có thể nhập loại xe."; }
        };
    }

    private static NumericUpDown MakePriceInput(string name)
        => new() { Minimum = 0, Maximum = TariffRule.MaximumPrice, DecimalPlaces = 2, ThousandsSeparator = true, Increment = 1000, AccessibleName = name };

    protected override void Dispose(bool disposing)
    {
        if (disposing) _errors.Dispose();
        base.Dispose(disposing);
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
