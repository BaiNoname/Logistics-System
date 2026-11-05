using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.SERVICE_SystemManagement;

namespace TruckingSystem.Desktop.GUI_SystemManagement;

public class SystemItemEditorForm : BaseForm
{
    private readonly TextBox _name = new() { Dock = DockStyle.Top, MaxLength = 100 };
    private readonly NumberBox _count = new() { Minimum = 0, Maximum = 100, Dock = DockStyle.Top, AccessibleName = "Số chức danh cần thêm" };
    private readonly List<TextBox> _positions = new();
    private readonly TableLayoutPanel _positionFields = new() { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1 };
    private readonly Panel _section = new() { Dock = DockStyle.Top, AutoSize = true, Visible = false };
    private readonly Label _status = new() { AutoSize = true, ForeColor = Theme.Danger, Dock = DockStyle.Top };
    private bool _saving;

    public SystemItemEditorForm(ApiServices api, bool department, SystemItem? item = null, int? departmentId = null)
    {
        Text = $"{(item is null ? "Thêm" : "Sửa")} {(department ? "phòng ban" : "chức danh")}";
        ClientSize = new Size(560, department && item is null ? 560 : 340);
        MinimumSize = new Size(460, 320);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Theme.CardBg;
        var fields = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, Padding = new Padding(20, 16, 20, 16) };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        void Field(string text, Control input)
        {
            fields.Controls.Add(new Label { Text = text, AutoSize = true, Margin = new Padding(3, 8, 3, 4) });
            input.AccessibleName = text.TrimEnd(':');
            input.Dock = DockStyle.Top;
            input.TabIndex = fields.Controls.Count;
            fields.Controls.Add(input);
        }
        _name.Text = item?.Name ?? "";
        Field(department ? "Tên phòng ban *:" : "Tên chức danh *:", _name);
        var departments = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "Name", ValueMember = "Id" };
        if (!department) Field("Phòng ban *:", departments);
        if (department && item is null)
        {
            var toggle = Theme.MakeSecondaryButton("Hiện thêm chức danh", 210);
            toggle.TabIndex = fields.Controls.Count;
            fields.Controls.Add(toggle);
            _positionFields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            var heading = new Label { Text = "Chức danh thuộc phòng ban", AutoSize = true };
            FormStyle.Section(heading);
            var sectionFields = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1 };
            sectionFields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            sectionFields.Controls.Add(heading);
            sectionFields.Controls.Add(new Label { Text = "Số chức danh cần thêm (0–100):", AutoSize = true });
            sectionFields.Controls.Add(_count);
            sectionFields.Controls.Add(_positionFields);
            _section.Controls.Add(sectionFields);
            _section.TabIndex = fields.Controls.Count;
            fields.Controls.Add(_section);
            toggle.Click += (_, _) =>
            {
                _section.Visible = !_section.Visible;
                toggle.Text = _section.Visible ? "Ẩn thêm chức danh" : "Hiện thêm chức danh";
            };
            _count.ValueChanged += (_, _) =>
            {
                while (_positions.Count < _count.Value)
                {
                    var index = _positions.Count + 1;
                    var input = new TextBox { MaxLength = 100, Dock = DockStyle.Top, AccessibleName = $"Tên chức danh {index}" };
                    _positions.Add(input);
                    var row = new TableLayoutPanel { AutoSize = true, Dock = DockStyle.Top, ColumnCount = 1 };
                    row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                    row.Controls.Add(new Label { Text = $"Tên chức danh {index} *:", AutoSize = true, Margin = new Padding(3, 8, 3, 4) });
                    row.Controls.Add(input);
                    _positionFields.Controls.Add(row);
                }
                for (var i = 0; i < _positions.Count; i++) _positions[i].Parent!.Visible = i < _count.Value;
            };
        }
        fields.Controls.Add(_status);
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        scroll.Controls.Add(fields);
        Controls.Add(scroll);
        var save = Theme.MakePrimaryButton(department ? "Lưu phòng ban" : "Lưu chức danh", 155);
        FormStyle.EditorFooter(this, FormStyle.NewCloseButton(this), save);
        DialogKeys.Enable(this);
        Anim.FadeIn(this);
        FormClosing += (_, e) => e.Cancel = _saving;
        if (!department)
            Load += async (_, _) =>
            {
                save.Enabled = false;
                try
                {
                    departments.DataSource = await api.System.GetAsync("departments");
                    departments.SelectedValue = departmentId ?? item?.DepartmentId ?? 0;
                }
                catch (Exception ex) { _status.Text = "Không tải được phòng ban: " + ex.Message; }
                finally { save.Enabled = true; }
            };
        save.Click += async (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_name.Text)) { FieldHint.Show(_name, _status, "Vui lòng nhập tên."); return; }
            int? selectedDepartment = departments.SelectedValue is int id ? id : null;
            if (!department && selectedDepartment is null) { FieldHint.Show(departments, _status, "Vui lòng chọn phòng ban."); return; }
            var names = _positions.Take((int)_count.Value).Select(p => p.Text.Trim()).ToList();
            for (var i = 0; i < names.Count; i++)
            {
                if (names[i].Length == 0 || names.Take(i).Contains(names[i], StringComparer.OrdinalIgnoreCase))
                {
                    _section.Visible = true;
                    scroll.ScrollControlIntoView(_positions[i]);
                    FieldHint.Show(_positions[i], _status, $"Vui lòng nhập tên chức danh {i + 1} không rỗng và không trùng.");
                    return;
                }
            }
            _saving = true;
            fields.Enabled = save.Enabled = false;
            try
            {
                var kind = department ? "departments" : "positions";
                if (item is null) await api.System.CreateAsync(kind, _name.Text.Trim(), selectedDepartment, names);
                else await api.System.UpdateAsync(kind, item.Id, _name.Text.Trim(), selectedDepartment);
                _saving = false;
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex) { _status.Text = "Không lưu được dữ liệu: " + ex.Message; }
            finally { _saving = false; fields.Enabled = save.Enabled = true; }
        };
    }
}
