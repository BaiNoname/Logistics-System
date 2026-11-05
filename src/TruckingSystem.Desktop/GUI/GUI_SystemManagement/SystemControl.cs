using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.SERVICE_SystemManagement;

namespace TruckingSystem.Desktop.GUI_SystemManagement;

public class SystemControl : BaseUserControl
{
    private readonly ApiServices _api;
    private readonly bool _positionsMode;
    private readonly int? _departmentId;
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill };
    private readonly Label _status = new() { Dock = DockStyle.Fill, ForeColor = Theme.TextSecondary, AutoSize = true };
    private readonly Button _edit = Theme.MakeSecondaryButton("Sửa", 85);
    private readonly Button _delete = Theme.MakeSecondaryButton("Xóa", 85);
    private readonly FlowLayoutPanel _actions = new() { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
    private List<SystemItem> _items = new();
    private string Kind => _positionsMode ? "positions" : "departments";
    private string ItemLabel => _positionsMode ? "chức danh" : "phòng ban";

    public SystemControl(ApiServices api) : this(api, false, null) { }

    private SystemControl(ApiServices api, bool positionsMode, int? departmentId)
    {
        _api = api;
        _positionsMode = positionsMode;
        _departmentId = departmentId;
        Theme.StyleGrid(_grid);
        _grid.SelectionChanged += (_, _) => UpdateButtons();
        _grid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await EditAsync(); };
        if (!positionsMode)
        {
            var menu = new ContextMenuStrip();
            var open = menu.Items.Add("Xem chức danh");
            open.Click += (_, _) => { if (SelectedItem() is { } item) OpenPositions(item.Id, item.Name); };
            menu.Opening += (_, e) => e.Cancel = SelectedItem() is null || !_grid.Enabled;
            ModernMenu.Apply(menu);
            _grid.ContextMenuStrip = menu;
            _grid.CellMouseDown += (_, e) =>
            {
                if (e.Button != MouseButtons.Right) return;
                _grid.ClearSelection();
                if (e.RowIndex < 0) { _grid.CurrentCell = null; return; }
                _grid.CurrentCell = _grid.Rows[e.RowIndex].Cells.Cast<DataGridViewCell>().First(c => c.Visible);
                _grid.Rows[e.RowIndex].Selected = true;
            };
            Disposed += (_, _) => menu.Dispose();
        }
        var create = Theme.MakePrimaryButton("Thêm " + ItemLabel, 165);
        var refresh = Theme.MakeSecondaryButton("Làm mới", 100);
        _actions.Controls.AddRange(new Control[] { create, _edit, _delete, refresh });
        if (!positionsMode)
        {
            var unassigned = Theme.MakeSecondaryButton("Chức danh chưa phân phòng", 240);
            unassigned.Click += (_, _) => OpenPositions(null, "Chưa phân phòng ban");
            _actions.Controls.Add(unassigned);
        }
        create.Click += async (_, _) => await OpenEditorAsync(null);
        _edit.Click += async (_, _) => await EditAsync();
        _delete.Click += async (_, _) => await DeleteAsync();
        refresh.Click += async (_, _) => await LoadCurrentAsync();
        ButtonStyles.DangerLink(_delete);
        Padding = new Padding(16);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(new Label { Text = positionsMode ? "Chức danh" : "Hệ thống", Font = Theme.FontTitle, AutoSize = true }, 0, 0);
        layout.Controls.Add(_actions, 0, 1);
        layout.Controls.Add(new Label { Text = positionsMode ? "Chức danh thuộc phòng ban đang mở. Chọn Sửa để chuyển phòng ban." : "Chuột phải vào phòng ban để xem và quản lý chức danh.", AutoSize = true, ForeColor = Theme.TextSecondary }, 0, 2);
        layout.Controls.Add(_grid, 0, 3);
        layout.Controls.Add(_status, 0, 4);
        Controls.Add(layout);
        UpdateButtons();
        Load += async (_, _) => { Theme.UniformButtons(_actions); await LoadCurrentAsync(); };
    }

    private SystemItem? SelectedItem() => _grid.CurrentRow?.Cells["Id"].Value is int id ? _items.FirstOrDefault(i => i.Id == id) : null;
    private void UpdateButtons()
    {
        _edit.Enabled = SelectedItem() is not null;
        _delete.Enabled = SelectedItem() is { EmployeeCount: 0 };
    }
    private async Task LoadCurrentAsync()
    {
        _status.ForeColor = Theme.TextSecondary;
        _status.Text = "Đang tải...";
        try
        {
            var items = await _api.System.GetAsync(Kind);
            _items = _positionsMode ? items.Where(i => i.DepartmentId == _departmentId).ToList() : items;
            _grid.DataSource = _items.Select(i => new { i.Id, Tên = i.Name, Số_nhân_viên = i.EmployeeCount }).ToList();
            if (_grid.Columns["Id"] is { } column) column.Visible = false;
            UpdateButtons();
            _status.Text = $"{_items.Count} {ItemLabel}";
        }
        catch (Exception ex) { _status.ForeColor = Theme.Danger; _status.Text = "Không tải được dữ liệu: " + ex.Message; }
    }
    private async Task EditAsync() { if (SelectedItem() is { } item) await OpenEditorAsync(item); }
    private async Task OpenEditorAsync(SystemItem? item)
    {
        using var editor = new SystemItemEditorForm(_api, !_positionsMode, item, _departmentId);
        if (editor.ShowDialog(FindForm()) != DialogResult.OK) return;
        Toast.Show(this, "✅ Đã lưu " + ItemLabel + ".");
        await LoadCurrentAsync();
    }
    private void OpenPositions(int? departmentId, string name)
    {
        using var dialog = new BaseForm { Text = "Chức danh — " + name, ClientSize = new Size(900, 560), MinimumSize = new Size(620, 380), StartPosition = FormStartPosition.CenterParent };
        dialog.Controls.Add(new SystemControl(_api, true, departmentId) { Dock = DockStyle.Fill });
        ButtonStyles.Footer(dialog, null, FormStyle.NewCloseButton(dialog));
        ModernLook.Apply(dialog);
        DialogKeys.Enable(dialog);
        dialog.ShowDialog(FindForm());
    }
    private async Task DeleteAsync()
    {
        if (SelectedItem() is not { EmployeeCount: 0 } item) return;
        if (MessageBox.Show(FindForm(), $"Xóa {ItemLabel} \"{item.Name}\"?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        _actions.Enabled = _grid.Enabled = false;
        try { await _api.System.DeleteAsync(Kind, item.Id); await LoadCurrentAsync(); }
        catch (Exception ex) { MessageBox.Show(FindForm(), ex.Message, "Không xóa được dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { _actions.Enabled = _grid.Enabled = true; }
    }
}
