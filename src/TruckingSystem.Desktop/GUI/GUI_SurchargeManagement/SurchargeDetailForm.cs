using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_SurchargeManagement;

namespace TruckingSystem.Desktop.GUI.GUI_SurchargeManagement;

public sealed class SurchargeDetailForm : BaseForm
{
    private readonly TextBox _name = new() { MaxLength = 150 };
    private readonly DateTimePicker _date = new() { Format = DateTimePickerFormat.Short };
    private readonly NumberBox _fragile = new() { DecimalPlaces = 2, Increment = 0.1m };
    private readonly NumberBox _hazmat = new() { DecimalPlaces = 2, Increment = 0.1m };
    private readonly Button _save = Theme.MakePrimaryButton("Lưu và áp dụng", 160);
    private readonly Label _status = new() { AutoSize = true, ForeColor = Theme.Danger, Margin = new Padding(0, 8, 0, 0) };
    public int SavedId { get; private set; }

    public SurchargeDetailForm(ApiServices api, SurchargeListItem? item = null, bool copy = false)
    {
        Text = copy ? "Sao chép bảng phụ phí" : item == null ? "Tạo bảng phụ phí" : "Chi tiết bảng phụ phí";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(570, 370);
        BackColor = Theme.CardBg;
        var editable = api.Connection.IsInRole("Admin", "Accountant") && (copy || item?.Published != true);
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(20, 16, 20, 16), AutoScroll = true };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Controls.Add(panel);
        DialogKeys.Enable(this);
        Anim.FadeIn(this);
        MinimumSize = new Size(460, 340);
        void Field(string label, Control field)
        {
            var row = panel.RowCount++;
            panel.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, ForeColor = Theme.TextPrimary, Margin = new Padding(0, 4, 12, 4) }, 0, row);
            field.Dock = DockStyle.Top;
            field.Margin = new Padding(0, 4, 0, 4);
            field.AccessibleName = label.TrimEnd(' ', '*');
            panel.Controls.Add(field, 1, row);
        }
        _name.Text = copy ? item!.Name + " (bản sao)" : item?.Name ?? "";
        _date.Value = copy || item == null ? DateTime.UtcNow.AddHours(7).Date.AddDays(1) : item.EffectiveDate;
        _fragile.Value = item?.FragilePercent ?? 0;
        _hazmat.Value = item?.HazmatPercent ?? 0;
        _name.ReadOnly = !editable;
        _date.Enabled = editable;
        _fragile.Enabled = _hazmat.Enabled = editable && !copy;
        Field("Tên bảng *:", _name);
        Field("Ngày hiệu lực *:", _date);
        Field("Hàng dễ vỡ (%)", _fragile);
        Field("Hàng nguy hiểm (%)", _hazmat);
        var hint = new Label
        {
            Text = "Phụ phí tính theo tỷ lệ % trên cước km + cước tấn.\nBảng được khóa sau khi lưu và tính phí từ ngày hiệu lực (sớm nhất ngày mai).", AutoSize = true,
            MaximumSize = new Size(260, 0),
            Font = Theme.FontSmall, ForeColor = Theme.TextSecondary, Margin = new Padding(0, 2, 0, 4),
        };
        panel.Controls.Add(hint, 1, panel.RowCount++);
        panel.Controls.Add(_status, 0, panel.RowCount++);
        panel.SetColumnSpan(_status, 2);

        // Chân cửa sổ chuẩn: [Đóng] [💾 Lưu và áp dụng] — chỉ 1 nút chính.
        _save.Visible = editable;
        FormStyle.SaveButton(_save);
        var close = Theme.MakeSecondaryButton("Đóng", 100);
        close.DialogResult = DialogResult.Cancel;
        FormStyle.EditorFooter(this, close, _save);
        CancelButton = close;
        if (editable) AcceptButton = _save;
        bool saving = false;
        FormClosing += (_, e) => e.Cancel = saving;
        _save.Click += async (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_name.Text))
            {
                FieldHint.Show(_name, _status, "Vui lòng nhập tên bảng phụ phí.");
                return;
            }
            saving = true;
            panel.Enabled = _save.Enabled = false;
            FormStyle.Status(_status, "Đang lưu...", StatusKind.Info);
            try
            {
                var saved = await api.Surcharges.SaveAsync(new(item?.Id ?? 0, _name.Text, _date.Value,
                    false, item?.Revision ?? 0, _fragile.Value, _hazmat.Value), copy);
                SavedId = saved!.Id;
                saving = false;
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex) { FormStyle.Status(_status, "Lỗi: " + ex.Message); }
            finally { saving = false; panel.Enabled = _save.Enabled = true; }
        };
    }
}
