using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_SurchargeManagement;

namespace TruckingSystem.Desktop.GUI.GUI_SurchargeManagement;

public sealed class SurchargeDetailForm : BaseForm
{
    private readonly TextBox _name = new() { MaxLength = 150 };
    private readonly DateTimePicker _date = new() { Format = DateTimePickerFormat.Short };
    private readonly NumericUpDown _fragile = new() { DecimalPlaces = 2, Increment = 0.1m };
    private readonly NumericUpDown _hazmat = new() { DecimalPlaces = 2, Increment = 0.1m };
    private readonly Button _save = new() { Text = "Lưu nháp", AutoSize = true };
    private readonly Label _status = new() { AutoSize = true, ForeColor = Theme.Danger };
    public int SavedId { get; private set; }

    public SurchargeDetailForm(ApiServices api, SurchargeListItem? item = null, bool copy = false)
    {
        Text = copy ? "Sao chép bảng phụ phí" : item == null ? "Tạo bảng phụ phí" : "Chi tiết bảng phụ phí";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(570, 315);
        var editable = api.Connection.IsInRole("Admin", "Accountant") && (copy || item?.Published != true);
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(16), AutoScroll = true };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Controls.Add(panel);
        void Field(string label, Control field)
        {
            var row = panel.RowCount++;
            panel.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
            field.Dock = DockStyle.Top;
            field.AccessibleName = label;
            panel.Controls.Add(field, 1, row);
        }
        _name.Text = copy ? item!.Name + " (bản sao)" : item?.Name ?? "";
        _date.Value = copy || item == null ? DateTime.UtcNow.AddHours(7).Date.AddDays(1) : item.EffectiveDate;
        _fragile.Value = item?.FragilePercent ?? 0;
        _hazmat.Value = item?.HazmatPercent ?? 0;
        _name.ReadOnly = !editable;
        _date.Enabled = editable;
        _fragile.Enabled = _hazmat.Enabled = editable && !copy;
        Field("Tên bảng", _name);
        Field("Ngày hiệu lực", _date);
        Field("Loại phụ phí", new Label { Text = "Tỷ lệ trên cước km + cước tấn (%)", AutoSize = true });
        Field("Hàng dễ vỡ", _fragile);
        Field("Hàng nguy hiểm", _hazmat);
        _save.Visible = editable;
        var actions = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
        var close = new Button { Text = "Đóng", DialogResult = DialogResult.Cancel, AutoSize = true };
        actions.Controls.AddRange([_save, close]);
        panel.Controls.Add(actions, 1, panel.RowCount++);
        panel.Controls.Add(_status, 0, panel.RowCount++);
        panel.SetColumnSpan(_status, 2);
        CancelButton = close;
        if (editable) AcceptButton = _save;
        bool saving = false;
        FormClosing += (_, e) => e.Cancel = saving;
        _save.Click += async (_, _) =>
        {
            saving = true;
            panel.Enabled = false;
            try
            {
                var saved = await api.Surcharges.SaveAsync(new(item?.Id ?? 0, _name.Text, _date.Value,
                    false, item?.Revision ?? 0, _fragile.Value, _hazmat.Value), copy);
                SavedId = saved!.Id;
                saving = false;
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex) { _status.Text = ex.Message; }
            finally { saving = false; panel.Enabled = true; }
        };
    }
}
