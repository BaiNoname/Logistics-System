using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_SurchargeManagement;

namespace TruckingSystem.Desktop.GUI.GUI_SurchargeManagement;

public sealed class SurchargeControl : BaseUserControl
{
    private readonly ApiServices _api;
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = false,
        MultiSelect = false, ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
    private readonly Button _create = ActionButton("Tạo nháp");
    private readonly Button _detail = ActionButton("Chi tiết");
    private readonly Button _copy = ActionButton("Sao chép");
    private readonly Button _publish = ActionButton("Công bố");
    private readonly Label _status = new() { Dock = DockStyle.Bottom, Height = 42 };
    private bool _busy;
    private SurchargeListItem? Selected => _grid.CurrentRow?.DataBoundItem as SurchargeListItem;
    private static Button ActionButton(string text) => new() { Text = text, AutoSize = true };

    public SurchargeControl(ApiServices api)
    {
        _api = api;
        Dock = DockStyle.Fill;
        Padding = new Padding(12);
        Theme.StyleGrid(_grid);
        foreach (var (property, label) in new[] { ("Name", "Tên bảng"), ("EffectiveDate", "Ngày hiệu lực"), ("Status", "Trạng thái") })
        {
            var column = new DataGridViewTextBoxColumn { HeaderText = label, DataPropertyName = property, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill };
            if (property == "EffectiveDate") column.DefaultCellStyle.Format = "dd/MM/yyyy";
            _grid.Columns.Add(column);
        }
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true };
        var refresh = ActionButton("Làm mới");
        bar.Controls.AddRange([new Label { Text = "Bảng phụ phí", Font = Theme.FontBold, AutoSize = true, Padding = new Padding(0, 6, 10, 0) },
            _create, _detail, _copy, _publish, refresh]);
        Controls.Add(_grid);
        Controls.Add(bar);
        Controls.Add(_status);
        _create.Visible = _copy.Visible = _publish.Visible = api.Connection.IsInRole("Admin", "Accountant");
        _grid.SelectionChanged += (_, _) => UpdateActions();
        _create.Click += async (_, _) => await EditAsync(null);
        _detail.Click += async (_, _) => await EditAsync(Selected);
        _copy.Click += async (_, _) => await EditAsync(Selected, true);
        _grid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0 && !_busy) await EditAsync(Selected); };
        refresh.Click += async (_, _) => await ReloadAsync();
        _publish.Click += async (_, _) =>
        {
            if (_busy || Selected is not { } item) return;
            if (MessageBox.Show(this, $"Công bố {item.Name} từ {item.EffectiveDate:dd/MM/yyyy}?\nDễ vỡ {item.FragilePercent:0.##}% · Nguy hiểm {item.HazmatPercent:0.##}%. Bảng sẽ bị khóa.",
                "Công bố bảng phụ phí", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            _busy = true;
            UpdateActions();
            try { await _api.Surcharges.PublishAsync(item); }
            catch (Exception ex) { _status.Text = ex.Message; return; }
            finally { _busy = false; UpdateActions(); }
            await ReloadAsync(item.Id);
        };
        UpdateActions();
        Load += async (_, _) => await ReloadAsync();
    }

    private void UpdateActions()
    {
        _create.Enabled = !_busy;
        _copy.Enabled = _detail.Enabled = !_busy && Selected != null;
        _publish.Enabled = !_busy && Selected?.Published == false;
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
            _status.Text = rows.Count == 0 ? "Chưa có bảng phụ phí." : "Chọn Chi tiết để xem hai loại phụ phí. Chỉ bản nháp mới được sửa.";
        }
        catch (Exception ex) { _status.Text = ex.Message; }
        finally { _busy = false; if (!IsDisposed) UpdateActions(); }
    }
}
