using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_ContractManagement;

namespace TruckingSystem.Desktop.GUI.GUI_ContractManagement;

public class ContractAppendicesControl : BaseUserControl
{
    private readonly ApiServices _api;
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = false, AccessibleName = "Danh sách phụ lục hợp đồng" };
    private readonly Button _create = Theme.MakePrimaryButton("Thêm mới", 110);
    private readonly Button _refresh = Theme.MakeSecondaryButton("Làm mới", 100);
    private readonly Label _status = new() { Dock = DockStyle.Fill, ForeColor = Theme.TextSecondary, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };
    private bool _busy;
    private List<ContractAppendixItem> _items = new();
    private readonly FilterBar _filters = new() { Dock = DockStyle.Fill, SearchPlaceholder = "Mã hợp đồng, phụ lục, mã bảng giá" };
    private ContractAppendixItem? SelectedItem => _grid.CurrentRow?.DataBoundItem as ContractAppendixItem;

    public ContractAppendicesControl(ApiServices api)
    {
        _api = api;
        BackColor = Theme.ContentBg;
        Padding = new Padding(20);
        Theme.StyleGrid(_grid);
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { DataPropertyName = "Code", HeaderText = "Mã phụ lục" },
            new DataGridViewTextBoxColumn { DataPropertyName = "ContractCode", HeaderText = "Mã hợp đồng" },
            new DataGridViewTextBoxColumn { Name = "ContractStatusText", DataPropertyName = "ContractStatusText", HeaderText = "Trạng thái hợp đồng" },
            new DataGridViewTextBoxColumn { Name = "StatusText", DataPropertyName = "StatusText", HeaderText = "Trạng thái phụ lục" },
            new DataGridViewTextBoxColumn { DataPropertyName = "IssueDate", HeaderText = "Ngày lập", DefaultCellStyle = new() { Format = "dd/MM/yyyy" } },
            new DataGridViewTextBoxColumn { DataPropertyName = "EffectiveDate", HeaderText = "Ngày có hiệu lực", DefaultCellStyle = new() { Format = "dd/MM/yyyy" } },
            new DataGridViewTextBoxColumn { DataPropertyName = "TariffName", HeaderText = "Bảng giá", FillWeight = 200 },
            new DataGridViewTextBoxColumn { DataPropertyName = "DiscountPercent", HeaderText = "Ưu đãi (%)", DefaultCellStyle = new() { Format = "0.##", NullValue = "—" } },
            new DataGridViewTextBoxColumn { DataPropertyName = nameof(ContractAppendixItem.LateDeliveryDiscountPercent), HeaderText = "Giảm do giao trễ (%)", DefaultCellStyle = new() { Format = "0.##", NullValue = "Theo hợp đồng" } });
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
        toolbar.Controls.Add(new Label { Text = "Phụ lục hợp đồng", Font = Theme.FontTitle, ForeColor = Theme.TextPrimary, AutoSize = true, Margin = new Padding(0, 0, 24, 12) });
        toolbar.Controls.AddRange(new Control[] { _create, _refresh });
        GridDecor.Pills(_grid, "ContractStatusText", (text, _) => text == "Đang hiệu lực" ? GridDecor.Green : GridDecor.Slate);
        GridDecor.Pills(_grid, "StatusText", (text, _) => text == "Đang áp dụng" ? GridDecor.Green : GridDecor.Slate);
        var menu = new ContextMenuStrip();
        menu.Items.Add("✏️ Sửa phụ lục", null, async (_, _) => await EditAsync(false));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("🗑 Xóa phụ lục", null, async (_, _) => await DeleteAsync());
        menu.Opening += (_, e) => e.Cancel = _busy || SelectedItem is null;
        ModernMenu.Apply(menu);
        _grid.ContextMenuStrip = menu;
        Disposed += (_, _) => menu.Dispose();
        _grid.CellMouseDown += (_, e) =>
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
                _grid.CurrentCell = _grid.Rows[e.RowIndex].Cells[Math.Max(0, e.ColumnIndex)];
        };
        _filters.AddDate("issueFrom", "Ngày lập từ");
        _filters.AddDate("issueTo", "Ngày lập đến");
        _filters.AddDate("effectiveFrom", "Hiệu lực từ");
        _filters.AddDate("effectiveTo", "Hiệu lực đến");
        _filters.FilterChanged += (_, _) => BindGrid();
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.Controls.Add(toolbar, 0, 0);
        layout.Controls.Add(_filters, 0, 1);
        layout.Controls.Add(_grid, 0, 2);
        layout.Controls.Add(_status, 0, 3);
        Controls.Add(layout);
        _create.Click += async (_, _) => await EditAsync(true);
        _refresh.Click += async (_, _) => await LoadAsync();
        _grid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await EditAsync(false); };
        Load += async (_, _) => await LoadAsync();
        UpdateActions();
        Load += (_, _) => Theme.UniformButtons(toolbar);
    }

    private void UpdateActions()
    {
        _create.Enabled = _refresh.Enabled = !_busy;
    }

    private async Task LoadAsync(int? preferredId = null)
    {
        if (_busy) return;
        var selectedId = preferredId ?? SelectedItem?.Id;
        _busy = true;
        UpdateActions();
        _status.Text = "Đang tải phụ lục...";
        try
        {
            var items = await _api.ContractAppendices.GetAppendicesAsync();
            if (IsDisposed) return;
            _items = items;
            BindGrid(selectedId);
        }
        catch (Exception ex) { if (!IsDisposed) ShowError(ex); }
        finally { _busy = false; if (!IsDisposed) UpdateActions(); }
    }

    private void BindGrid(int? preferredId = null)
    {
        var selectedId = preferredId ?? SelectedItem?.Id;
        var issueFrom = _filters.Date("issueFrom"); var issueTo = _filters.Date("issueTo");
        var effectiveFrom = _filters.Date("effectiveFrom"); var effectiveTo = _filters.Date("effectiveTo");
        var items = _items.Where(a => _filters.MatchesSearch(a.Code, a.ContractCode, a.TariffName)
            && (!issueFrom.HasValue || a.IssueDate.Date >= issueFrom.Value)
            && (!issueTo.HasValue || a.IssueDate.Date <= issueTo.Value)
            && (!effectiveFrom.HasValue || a.EffectiveDate.Date >= effectiveFrom.Value)
            && (!effectiveTo.HasValue || a.EffectiveDate.Date <= effectiveTo.Value)).ToList();
        _grid.DataSource = items;
        foreach (DataGridViewRow row in _grid.Rows)
            if (row.DataBoundItem is ContractAppendixItem item && item.Id == selectedId) _grid.CurrentCell = row.Cells[0];
        _filters.SetCount(items.Count, _items.Count);
        _status.ForeColor = Theme.TextSecondary;
        _status.Text = _items.Count == 0 ? "Chưa có phụ lục. Chọn Thêm mới để tạo."
            : $"Hiển thị {items.Count}/{_items.Count} phụ lục · Nhấp đúp để xem và sửa.";
    }

    private async Task EditAsync(bool create)
    {
        if (_busy || !create && SelectedItem is null) return;
        using var form = new ContractAppendixForm(_api, create ? null : SelectedItem);
        if (form.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync(form.SavedId);
    }

    private async Task DeleteAsync()
    {
        if (_busy || SelectedItem is not { } item) return;
        if (MessageBox.Show(FindForm(), $"Xóa phụ lục {item.Code} của hợp đồng {item.ContractCode}?\nThao tác này không thể hoàn tác.",
            "Xác nhận xóa phụ lục", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
        _busy = true;
        UpdateActions();
        try { await _api.ContractAppendices.DeleteAppendixAsync(item.Id); }
        catch (Exception ex) { if (!IsDisposed) ShowError(ex); return; }
        finally { _busy = false; if (!IsDisposed) UpdateActions(); }
        if (!IsDisposed) await LoadAsync();
    }

    private void ShowError(Exception ex)
    {
        _status.ForeColor = Theme.Danger;
        _status.Text = "Lỗi: " + ex.Message;
    }
}
