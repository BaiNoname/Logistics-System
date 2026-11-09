using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_SurchargeManagement;

namespace TruckingSystem.Desktop.GUI.GUI_SurchargeManagement;

public sealed class SurchargeControl : BaseUserControl
{
    private readonly ApiServices _api;
    private List<SurchargeListItem> _items = new();
    private readonly FilterBar _filters = new() { Dock = DockStyle.Fill, SearchPlaceholder = "Tìm mã phụ phí..." };
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = false,
        MultiSelect = false, ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
    private readonly Button _create = Theme.MakePrimaryButton("Thêm bảng phụ phí");
    private readonly Label _status = new() { Dock = DockStyle.Bottom, Height = 42, ForeColor = Theme.TextSecondary, TextAlign = ContentAlignment.MiddleLeft };
    private bool _busy;
    private SurchargeListItem? Selected => _grid.CurrentRow?.DataBoundItem as SurchargeListItem;

    public SurchargeControl(ApiServices api)
    {
        _api = api;
        Dock = DockStyle.Fill;
        Padding = new Padding(20);
        BackColor = Theme.ContentBg;
        Theme.StyleGrid(_grid);
        foreach (var (property, label) in new[] { ("Name", "Tên bảng"), ("EffectiveDate", "Ngày hiệu lực"), ("Status", "Trạng thái") })
        {
            var column = new DataGridViewTextBoxColumn { Name = property, HeaderText = label, DataPropertyName = property, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill };
            if (property == "EffectiveDate") column.DefaultCellStyle.Format = "dd/MM/yyyy";
            _grid.Columns.Add(column);
        }
        GridDecor.Pills(_grid, nameof(SurchargeListItem.Status), (text, _) => GridDecor.StatusTone(text));
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true };
        var refresh = Theme.MakeSecondaryButton("Làm mới");
        bar.Controls.AddRange([new Label { Text = "Bảng phụ phí", Font = Theme.FontTitle, ForeColor = Theme.TextPrimary, AutoSize = true, Margin = new Padding(0, 0, 24, 8) },
            _create, refresh]);
        _filters.AddDate("effectiveFrom", "Hiệu lực từ");
        _filters.AddDate("effectiveTo", "Hiệu lực đến");
        _filters.FilterChanged += (_, _) => BindGrid();
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Margin = Padding.Empty };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.Controls.Add(bar, 0, 0);
        layout.Controls.Add(_filters, 0, 1);
        layout.Controls.Add(_grid, 0, 2);
        layout.Controls.Add(_status, 0, 3);
        Controls.Add(layout);
        _create.Visible = api.Connection.IsInRole("Admin", "Accountant");
        _create.Click += async (_, _) => await EditAsync(null);
        _grid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0 && !_busy) await EditAsync(Selected); };
        refresh.Click += async (_, _) => await ReloadAsync();
        UpdateActions();
        Load += (_, _) => Theme.UniformButtons(bar);
        Load += async (_, _) => await ReloadAsync();
    }

    private void UpdateActions()
    {
        _create.Enabled = !_busy;
        _filters.Enabled = !_busy;
    }

    private void BindGrid(int? selected = null)
    {
        selected ??= Selected?.Id;
        var from = _filters.Date("effectiveFrom");
        var to = _filters.Date("effectiveTo");
        var rows = _items.Where(item => _filters.MatchesSearch(item.Name)
            && (!from.HasValue || item.EffectiveDate.Date >= from.Value)
            && (!to.HasValue || item.EffectiveDate.Date <= to.Value)).ToList();
        _grid.DataSource = rows;
        foreach (DataGridViewRow row in _grid.Rows)
            if (row.DataBoundItem is SurchargeListItem item && item.Id == selected) _grid.CurrentCell = row.Cells[0];
        _filters.SetCount(rows.Count, _items.Count);
    }

    private async Task EditAsync(SurchargeListItem? item)
    {
        if (_busy) return;
        using var form = new SurchargeDetailForm(_api, item);
        if (form.ShowDialog(FindForm()) == DialogResult.OK) await ReloadAsync(form.SavedId);
    }

    private async Task ReloadAsync(int? selected = null)
    {
        if (_busy) return;
        selected ??= Selected?.Id;
        _busy = true;
        UpdateActions();
        try
        {
            var rows = await _api.Surcharges.ListAsync();
            if (IsDisposed) return;
            _items = rows;
            BindGrid(selected);
            FormStyle.Status(_status, rows.Count == 0 ? "Chưa có bảng phụ phí." : "Chỉ bảng mới thêm đã xác nhận đang áp dụng. Phí từng đơn vẫn tính theo ngày hiệu lực tương ứng.", StatusKind.Info);
        }
        catch (Exception ex) { FormStyle.Status(_status, ex.Message); }
        finally { _busy = false; if (!IsDisposed) UpdateActions(); }
    }
}
