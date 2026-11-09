using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI.GUI_ContractManagement;

public sealed class DiscountConfigurationForm : BaseForm
{
    private readonly NumberBox _value = new() { Minimum = 0, Maximum = 100, DecimalPlaces = 0, Dock = DockStyle.Top, AccessibleName = "Phần trăm ưu đãi tối đa" };
    private readonly Label _status = new() { AutoSize = true, Dock = DockStyle.Top, MaximumSize = new Size(400, 0) };
    private bool _saving;

    public DiscountConfigurationForm(ApiServices api)
    {
        Text = "Quản lý ưu đãi";
        ClientSize = new Size(460, 240);
        StartPosition = FormStartPosition.CenterParent;
        var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24), AutoScroll = true };
        body.Controls.Add(_status);
        body.Controls.Add(_value);
        body.Controls.Add(new Label { Text = "Phần trăm ưu đãi tối đa", AutoSize = true, Dock = DockStyle.Top });
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
                var value = await api.Contracts.GetMaxDiscountPercentAsync();
                if (IsDisposed) return;
                _value.Value = value;
                save.Enabled = api.Connection.IsInRole("Admin");
            }
            catch (Exception ex) { FormStyle.Status(_status, "Lỗi tải cấu hình: " + ex.Message); }
        };
        save.Click += async (_, _) =>
        {
            if (_saving) return;
            _saving = true;
            save.Enabled = _value.Enabled = false;
            try
            {
                await api.Contracts.SaveMaxDiscountPercentAsync((int)_value.Value);
                _saving = false;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) { FormStyle.Status(_status, "Lỗi: " + ex.Message); }
            finally { _saving = false; if (!IsDisposed) save.Enabled = _value.Enabled = true; }
        };
    }
}
