using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.SERVICE_SystemManagement;

namespace TruckingSystem.Desktop.GUI_SystemManagement;

public class SystemControl : BaseUserControl
{
    private readonly ApiServices _api;
    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill };
    private readonly DataGridView _departmentsGrid = new() { Dock = DockStyle.Fill };
    private readonly DataGridView _positionsGrid = new() { Dock = DockStyle.Fill };
    private readonly Label _status = new() { Dock = DockStyle.Fill, ForeColor = Theme.TextSecondary, TextAlign = ContentAlignment.MiddleLeft };
    private readonly Button _edit = Theme.MakeSecondaryButton("Sửa", 85);
    private readonly Button _delete = Theme.MakeSecondaryButton("Xóa", 85);
    private readonly FlowLayoutPanel _actions = new() { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
    private List<SystemItem> _departments = new();
    private List<SystemItem> _positions = new();

    private bool IsDepartments => _tabs.SelectedIndex == 0;
    private string Kind => IsDepartments ? "departments" : "positions";
    private string Label => IsDepartments ? "phòng ban" : "chức danh";
    private DataGridView Grid => IsDepartments ? _departmentsGrid : _positionsGrid;

    public SystemControl(ApiServices api)
    {
        _api = api;
        Theme.StyleGrid(_departmentsGrid);
        Theme.StyleGrid(_positionsGrid);
        _departmentsGrid.SelectionChanged += (_, _) => UpdateButtons();
        _positionsGrid.SelectionChanged += (_, _) => UpdateButtons();
        _departmentsGrid.CellDoubleClick += async (_, _) => await EditAsync();
        _positionsGrid.CellDoubleClick += async (_, _) => await EditAsync();

        var departmentsTab = new TabPage("Phòng ban") { BackColor = Color.White };
        var positionsTab = new TabPage("Chức danh") { BackColor = Color.White };
        departmentsTab.Controls.Add(_departmentsGrid);
        positionsTab.Controls.Add(_positionsGrid);
        _tabs.TabPages.Add(departmentsTab);
        _tabs.TabPages.Add(positionsTab);
        _tabs.SelectedIndexChanged += (_, _) => { UpdateButtons(); UpdateStatus(); };

        var create = Theme.MakePrimaryButton("Thêm mới", 110);
        var refresh = Theme.MakeSecondaryButton("Làm mới", 100);
        _actions.Controls.AddRange(new Control[] { create, _edit, _delete, refresh });
        create.Click += async (_, _) => await CreateAsync();
        _edit.Click += async (_, _) => await EditAsync();
        _delete.Click += async (_, _) => await DeleteAsync();
        refresh.Click += async (_, _) => await LoadCurrentAsync();

        Padding = new Padding(16);
        ButtonStyles.DangerLink(_delete);
        var header = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 1, RowCount = 2 };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        header.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        header.Controls.Add(new Label { Text = "Hệ thống", Font = Theme.FontTitle, ForeColor = Theme.TextPrimary, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        header.Controls.Add(_actions, 0, 1);

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3 };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(_tabs, 0, 1);
        layout.Controls.Add(_status, 0, 2);
        Controls.Add(layout);

        UpdateButtons();
        Load += (_, _) => Theme.UniformButtons(_actions);
        Load += async (_, _) => { await LoadCurrentAsync(); _tabs.SelectedIndex = 1; await LoadCurrentAsync(); _tabs.SelectedIndex = 0; };
    }

    private SystemItem? SelectedItem()
    {
        if (Grid.CurrentRow?.Cells["Id"].Value is not int id) return null;
        return (IsDepartments ? _departments : _positions).FirstOrDefault(item => item.Id == id);
    }

    private void UpdateButtons()
    {
        var selected = SelectedItem();
        _edit.Enabled = selected is not null;
        _delete.Enabled = selected is { EmployeeCount: 0 };
    }

    private void UpdateStatus() => _status.Text = $"{(IsDepartments ? _departments : _positions).Count} {Label}";

    private async Task LoadCurrentAsync()
    {
        _status.ForeColor = Theme.TextSecondary;
        _status.Text = "Đang tải...";
        try
        {
            var kind = Kind;
            var items = await _api.System.GetAsync(kind);
            if (kind == "departments") _departments = items;
            else _positions = items;
            var grid = kind == "departments" ? _departmentsGrid : _positionsGrid;
            grid.DataSource = items.Select(item => new
            {
                item.Id,
                Tên = item.Name,
                Số_nhân_viên = item.EmployeeCount,
            }).ToList();
            if (grid.Columns["Id"] is not null) grid.Columns["Id"]!.Visible = false;
            UpdateButtons();
            UpdateStatus();
        }
        catch (Exception ex)
        {
            _status.ForeColor = Theme.Danger;
            _status.Text = "Không tải được dữ liệu: " + ex.Message;
        }
    }

    private async Task CreateAsync()
    {
        var name = PromptName($"Thêm {Label}");
        if (name is null) return;
        await SaveAsync(() => _api.System.CreateAsync(Kind, name));
    }

    private async Task EditAsync()
    {
        var item = SelectedItem();
        if (item is null) return;
        var name = PromptName($"Sửa {Label}", item.Name);
        if (name is null || name == item.Name) return;
        await SaveAsync(() => _api.System.UpdateAsync(Kind, item.Id, name));
    }

    private async Task SaveAsync(Func<Task<SystemItem?>> action)
    {
        _actions.Enabled = false;
        _tabs.Enabled = false;
        try
        {
            await action();
            await LoadCurrentAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(FindForm(), ex.Message, "Không lưu được dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { _actions.Enabled = _tabs.Enabled = true; }
    }

    private async Task DeleteAsync()
    {
        var item = SelectedItem();
        if (item is null || item.EmployeeCount != 0) return;
        if (MessageBox.Show(FindForm(), $"Xóa {Label} \"{item.Name}\"?", "Xác nhận xóa",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

        _actions.Enabled = false;
        _tabs.Enabled = false;
        try
        {
            await _api.System.DeleteAsync(Kind, item.Id);
            await LoadCurrentAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(FindForm(), ex.Message, "Không xóa được dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { _actions.Enabled = _tabs.Enabled = true; }
    }

    private string? PromptName(string title, string value = "")
    {
        using var dialog = new BaseForm
        {
            Text = title, ClientSize = new Size(470, 240), MinimumSize = new Size(430, 240),
            MaximizeBox = false, MinimizeBox = false, StartPosition = FormStartPosition.CenterParent,
            BackColor = Color.White,
        };
        var fields = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(20, 16, 20, 16) };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var label = new Label { Text = "Tên *:", AutoSize = true, ForeColor = Theme.TextPrimary };
        var input = new TextBox { Text = value, Dock = DockStyle.Top, MaxLength = 100, AccessibleName = "Tên" };
        var status = new Label { AutoSize = true, Dock = DockStyle.Fill, ForeColor = Theme.Danger };
        fields.Controls.Add(label); fields.Controls.Add(input); fields.Controls.Add(status);
        dialog.Controls.Add(fields);
        var save = Theme.MakePrimaryButton("Lưu thông tin", 150);
        FormStyle.EditorFooter(dialog, FormStyle.NewCloseButton(dialog), save);
        ModernLook.Apply(dialog);
        DialogKeys.Enable(dialog, save);
        Anim.FadeIn(dialog);
        save.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(input.Text))
                FieldHint.Show(input, status, "Vui lòng nhập tên.");
            else dialog.DialogResult = DialogResult.OK;
        };

        dialog.AcceptButton = save;
        input.SelectAll();
        return dialog.ShowDialog(FindForm()) == DialogResult.OK ? input.Text.Trim() : null;
    }
}
