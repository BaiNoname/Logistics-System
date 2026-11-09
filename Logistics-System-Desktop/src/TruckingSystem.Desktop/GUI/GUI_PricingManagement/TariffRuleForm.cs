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
    private readonly DataGridView _vehicleTypes = new() { Dock = DockStyle.Fill, AccessibleName = "Danh sách loại xe" };

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

    public TariffRuleForm(ApiServices api, PriceListItem priceList, IReadOnlyCollection<TariffRuleItem> rules, TariffRuleItem? item = null, string? vehicleType = null) : this()
    {
        _api = api;
        _priceList = priceList;
        _item = item;
        Text = item is null ? "Thêm chi tiết giá" : "Sửa chi tiết giá";
        _priceListLabel.Text = "Bảng giá: " + priceList.Name;
        var usedTypes = rules.Where(rule => rule.PriceListId == priceList.Id && rule.Id != item?.Id)
            .Select(rule => rule.VehicleType.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var defaults = TariffRule.DefaultVehicleTypes;
        Theme.StyleGrid(_vehicleTypes);
        var typesPanel = new Panel { Height = 185, Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 8) };
        typesPanel.Controls.Add(_vehicleTypes);
        _fields.Controls.Add(typesPanel);
        _fields.Controls.SetChildIndex(typesPanel, _fields.Controls.GetChildIndex(_vehicleTypePanel));
        void BindTypes(IEnumerable<string> values)
        {
            _vehicleTypes.DataSource = defaults.Concat(values).Append(item?.VehicleType ?? "")
                .Select(t => t.Trim()).Where(t => t.Length > 0 && !usedTypes.Contains(t))
                .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(t => t)
                .Select(t => new { Loại_xe = t }).ToList();
            _vehicleTypes.ClearSelection();
        }
        BindTypes(Array.Empty<string>());
        _vehicleTypes.CellClick += (_, e) =>
        {
            if (e.RowIndex >= 0) _vehicleType.Text = _vehicleTypes.Rows[e.RowIndex].Cells[0].Value?.ToString() ?? "";
        };
        _vehicleTypes.KeyUp += (_, e) =>
        {
            if (e.KeyCode is Keys.Up or Keys.Down or Keys.Enter && _vehicleTypes.CurrentRow is { } row)
                _vehicleType.Text = row.Cells[0].Value?.ToString() ?? "";
        };
        Shown += async (_, _) =>
        {
            try { var types = await api.Pricing.GetVehicleTypesAsync(); if (!IsDisposed) BindTypes(types); }
            catch (Exception ex) { FormStyle.Status(_status, "Không tải được loại xe: " + ex.Message); }
        };
        if (item is null) _vehicleType.Text = vehicleType ?? "";
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
