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
    private readonly ToolStripMenuItem _edit = new("✏️ Sửa");
    private readonly ToolStripMenuItem _delete = new("🗑 Xóa");
    private readonly FlowLayoutPanel _actions = new() { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
    private List<SystemItem> _items = new();
    private bool _loading;
    private bool CanManage => !_loading && _grid.Enabled && _actions.Enabled && _api.Connection.IsInRole("Admin");
    private string Kind => _positionsMode ? "positions" : "departments";
    private string ItemLabel => _positionsMode ? "chức danh" : "phòng ban";

    public SystemControl(ApiServices api) : this(api, false, null) { }

    private SystemControl(ApiServices api, bool positionsMode, int? departmentId)
    {
        _api = api;
        _positionsMode = positionsMode;
        _departmentId = departmentId;
        _edit.Text = "✏️ Sửa " + ItemLabel;
        _delete.Text = "🗑 Xóa " + ItemLabel;
        Theme.StyleGrid(_grid);
        _grid.SelectionChanged += (_, _) => UpdateMenu();
        _grid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await EditAsync(); };
        var menu = new ContextMenuStrip();
        if (!positionsMode)
        {
            var open = menu.Items.Add("📋 Xem chức danh");
            open.Font = Theme.FontBold;
            open.Click += (_, _) => { if (CanManage && SelectedItem() is { } item) OpenPositions(item.Id, item.Name); };
            menu.Items.Add(new ToolStripSeparator());
        }
        menu.Items.Add(_edit);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(_delete);
        menu.Opening += (_, e) => { UpdateMenu(); e.Cancel = !CanManage || SelectedItem() is null; };
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
        _grid.MouseDown += (_, e) =>
        {
            if (e.Button != MouseButtons.Right || _grid.HitTest(e.X, e.Y).RowIndex >= 0) return;
            _grid.ClearSelection();
            _grid.CurrentCell = null;
        };
        Disposed += (_, _) => menu.Dispose();
        var create = Theme.MakePrimaryButton("Thêm " + ItemLabel, 165);
        var refresh = Theme.MakeSecondaryButton("Làm mới", 100);
        _actions.Controls.AddRange(new Control[] { create, refresh });
        create.Click += async (_, _) => await OpenEditorAsync(null);
        _edit.Click += async (_, _) => await EditAsync();
        _delete.Click += async (_, _) => await DeleteAsync();
        refresh.Click += async (_, _) => await LoadCurrentAsync();
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
        layout.Controls.Add(new Label { Text = positionsMode ? "Chuột phải vào chức danh để sửa hoặc xóa; chọn Sửa để chuyển phòng ban." : "Chuột phải vào phòng ban để xem chức danh, sửa hoặc xóa.", AutoSize = true, ForeColor = Theme.TextSecondary }, 0, 2);
        layout.Controls.Add(_grid, 0, 3);
        layout.Controls.Add(_status, 0, 4);
        Controls.Add(layout);
        UpdateMenu();
        Load += async (_, _) => { Theme.UniformButtons(_actions); await LoadCurrentAsync(); };
    }

    private SystemItem? SelectedItem() => _grid.CurrentRow?.Cells["Id"].Value is int id ? _items.FirstOrDefault(i => i.Id == id) : null;
    private void UpdateMenu()
    {
        _edit.Enabled = CanManage && SelectedItem() is not null;
        _delete.Enabled = CanManage && SelectedItem() is { EmployeeCount: 0 };
        _delete.ToolTipText = _delete.Enabled ? "Xóa " + ItemLabel + " được chọn."
            : "Không thể xóa " + ItemLabel + " đang có nhân viên sử dụng.";
    }
    private async Task LoadCurrentAsync()
    {
        if (_loading) return;
        _loading = true;
        UpdateMenu();
        _status.ForeColor = Theme.TextSecondary;
        _status.Text = "Đang tải...";
        try
        {
            var items = await _api.System.GetAsync(Kind);
            _items = _positionsMode ? items.Where(i => i.DepartmentId == _departmentId).ToList() : items;
            _grid.DataSource = _items.Select(i => new { i.Id, Tên = i.Name, Số_nhân_viên = i.EmployeeCount }).ToList();
            if (_grid.Columns["Id"] is { } column) column.Visible = false;
            _status.Text = $"{_items.Count} {ItemLabel}";
        }
        catch (Exception ex) { _status.ForeColor = Theme.Danger; _status.Text = "Không tải được dữ liệu: " + ex.Message; }
        finally { _loading = false; UpdateMenu(); }
    }
    private async Task EditAsync() { if (CanManage && SelectedItem() is { } item) await OpenEditorAsync(item); }
    private async Task OpenEditorAsync(SystemItem? item)
    {
        if (!CanManage) return;
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
        if (!CanManage || SelectedItem() is not { EmployeeCount: 0 } item) return;
        if (MessageBox.Show(FindForm(), $"Xóa {ItemLabel} \"{item.Name}\"?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        _actions.Enabled = _grid.Enabled = false;
        try { await _api.System.DeleteAsync(Kind, item.Id); await LoadCurrentAsync(); }
        catch (Exception ex) { MessageBox.Show(FindForm(), ex.Message, "Không xóa được dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { _actions.Enabled = _grid.Enabled = true; UpdateMenu(); }
    }
}
