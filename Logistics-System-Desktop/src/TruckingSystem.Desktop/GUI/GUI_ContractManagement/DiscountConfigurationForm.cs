using TruckingSystem.Desktop.Common;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Desktop.GUI.GUI_ContractManagement;

public sealed class DiscountConfigurationForm : BaseForm
{
    private readonly NumberBox _value = new() { Maximum = 100, Dock = DockStyle.Top, Enabled = false, AccessibleName = "Ưu đãi tối đa (%)" };
    private readonly NumberBox _lateDeliveryDiscount = new() { Maximum = 100, DecimalPlaces = 2, Dock = DockStyle.Top, Enabled = false, AccessibleName = "Giảm giá khi giao trễ mặc định (%)" };
    private readonly Label _status = new() { AutoSize = true, MaximumSize = new Size(400, 0) };
    private bool _saving;

    public DiscountConfigurationForm(ApiServices api)
    {
        Text = "Quản lý hạn mức";
        ClientSize = new Size(460, 260);
        StartPosition = FormStartPosition.CenterParent;
        var body = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(24), AutoScroll = true, ColumnCount = 1 };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        body.Controls.Add(new Label { Text = _value.AccessibleName, AutoSize = true });
        body.Controls.Add(_value);
        body.Controls.Add(new Label { Text = _lateDeliveryDiscount.AccessibleName, AutoSize = true, Margin = new Padding(3, 12, 3, 3) });
        body.Controls.Add(_lateDeliveryDiscount);
        body.Controls.Add(_status);
        Controls.Add(body);
        var save = Theme.MakePrimaryButton("Lưu cấu hình", 150);
        var close = Theme.MakeSecondaryButton("Thoát", 100);
        close.DialogResult = DialogResult.Cancel;
        FormStyle.EditorFooter(this, close, save);
        AcceptButton = save;
        CancelButton = close;
        DialogKeys.Enable(this);
        FormClosing += (_, e) => e.Cancel = _saving;
        save.Enabled = false;
        Shown += async (_, _) =>
        {
            try
            {
                var limits = await api.Contracts.GetDiscountLimitsAsync();
                if (IsDisposed) return;
                _value.Value = limits.MaxDiscountPercent;
                _lateDeliveryDiscount.Value = limits.LateDeliveryDiscountPercent;
                save.Enabled = _value.Enabled = _lateDeliveryDiscount.Enabled = api.Connection.IsInRole("Admin");
            }
            catch (Exception ex) { FormStyle.Status(_status, "Lỗi tải cấu hình: " + ex.Message); }
        };
        save.Click += async (_, _) =>
        {
            if (_saving || !api.Connection.IsInRole("Admin")) return;
            var lateDiscount = _lateDeliveryDiscount.Value;
            try { Contract.ValidateLateDeliveryDiscount(lateDiscount); }
            catch (ArgumentException ex) { FieldHint.Show(_lateDeliveryDiscount, _status, ex.Message); return; }
            _saving = true;
            save.Enabled = _value.Enabled = _lateDeliveryDiscount.Enabled = false;
            try
            {
                await api.Contracts.SaveDiscountLimitsAsync(new((int)_value.Value, lateDiscount));
                _saving = false;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) { FormStyle.Status(_status, "Lỗi: " + ex.Message); }
            finally { _saving = false; if (!IsDisposed) save.Enabled = _value.Enabled = _lateDeliveryDiscount.Enabled = true; }
        };
    }
}
