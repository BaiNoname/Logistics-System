using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_SurchargeManagement;

namespace TruckingSystem.Desktop.GUI.GUI_SurchargeManagement;

public sealed class SurchargeControl : BaseUserControl
{
    private readonly ApiServices _api;
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = false,
        MultiSelect = false, ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
    private readonly Button _create = Theme.MakePrimaryButton("Thêm bảng phụ phí");
    private readonly Button _detail = Theme.MakeSecondaryButton("Chi tiết");
    private readonly Button _copy = Theme.MakeSecondaryButton("Sao chép");
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
            _create, _detail, _copy, refresh]);
        Controls.Add(_grid);
        Controls.Add(bar);
        Controls.Add(_status);
        _create.Visible = _copy.Visible = api.Connection.IsInRole("Admin", "Accountant");
        _grid.SelectionChanged += (_, _) => UpdateActions();
        _create.Click += async (_, _) => await EditAsync(null);
        _detail.Click += async (_, _) => await EditAsync(Selected);
        _copy.Click += async (_, _) => await EditAsync(Selected, true);
        _grid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0 && !_busy) await EditAsync(Selected); };
        refresh.Click += async (_, _) => await ReloadAsync();
        UpdateActions();
        Load += (_, _) => Theme.UniformButtons(bar);
        Load += async (_, _) => await ReloadAsync();
    }

    private void UpdateActions()
    {
        _create.Enabled = !_busy;
        _copy.Enabled = _detail.Enabled = !_busy && Selected != null;
    }

    private async Task EditAsync(SurchargeListItem? item, bool copy = false)
    {
        if (_busy) return;
        using var form = new SurchargeDetailForm(_api, item, copy);
        if (form.ShowDialog(FindForm()) == DialogResult.OK) await ReloadAsync(form.SavedId);
    }

    private async Task ReloadAsync(int? selected = null)
    {
        if (_busy) return;
        _busy = true;
        UpdateActions();
        try
        {
            var rows = await _api.Surcharges.ListAsync();
            if (IsDisposed) return;
            _grid.DataSource = rows;
            foreach (DataGridViewRow row in _grid.Rows)
                if (row.DataBoundItem is SurchargeListItem item && item.Id == selected) _grid.CurrentCell = row.Cells[0];
            FormStyle.Status(_status, rows.Count == 0 ? "Chưa có bảng phụ phí." : "Bảng được xác nhận và khóa khi lưu, tính phí từ ngày hiệu lực. Bản nháp cũ: mở Chi tiết để lưu và áp dụng.", StatusKind.Info);
        }
        catch (Exception ex) { FormStyle.Status(_status, ex.Message); }
        finally { _busy = false; if (!IsDisposed) UpdateActions(); }
    }
}
