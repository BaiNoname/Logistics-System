using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_PricingManagement;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Desktop.GUI.GUI_PricingManagement;

public partial class PricingControl : BaseUserControl
{
    private readonly ApiServices _api = null!;
    private List<PriceListItem> _lists = new();
    private readonly FilterBar _filters = new() { Dock = DockStyle.Fill, SearchPlaceholder = "Tìm mã bảng giá..." };
    private List<TariffRuleItem> _rules = new();
    private bool _busy;
    private bool _binding;
    private bool _rulesLoaded;

    private int? _detailListId;
    private bool IsPriceLists => !_detailListId.HasValue;
    private PriceListItem? SelectedList => _listsGrid.CurrentRow?.DataBoundItem is PriceListItem item ? item : null;
    private PriceListItem? CurrentList => _priceList.SelectedItem as PriceListItem;
    private readonly Button _saveRules = Theme.MakePrimaryButton("Lưu thay đổi", 160);
    private sealed class VehicleRateRow(string vehicleType, decimal? pricePerKm, decimal? pricePerTon, TariffRuleItem? rule)
    {
        public string VehicleType { get; } = vehicleType;
        public decimal? PricePerKm { get; set; } = pricePerKm;
        public decimal? PricePerTon { get; set; } = pricePerTon;
        public TariffRuleItem? Rule { get; set; } = rule;
        public bool IsChanged => Rule is null ? PricePerKm.HasValue || PricePerTon.HasValue
            : PricePerKm != Rule.PricePerKm || PricePerTon != Rule.PricePerTon;
    }
    private IEnumerable<VehicleRateRow> VehicleRows => _rulesGrid.Rows.Cast<DataGridViewRow>()
        .Select(r => r.DataBoundItem).OfType<VehicleRateRow>();
    private bool HasChanges => VehicleRows.Any(r => r.IsChanged);
    private VehicleRateRow? SelectedVehicle => _rulesGrid.CurrentRow?.DataBoundItem as VehicleRateRow;
    private TariffRuleItem? SelectedRule => SelectedVehicle?.Rule;
    private bool CanEditRules => _rulesLoaded && CurrentList?.Status == "Active";

    public PricingControl()
    {
        InitializeComponent();
        _filters.AddDate("effectiveFrom", "Hiệu lực từ");
        _filters.AddDate("effectiveTo", "Hiệu lực đến");
        _filters.FilterChanged += (_, _) => BindLists();
        _layout.Controls.Add(_filters, 0, 1);
        Theme.StyleGrid(_listsGrid);
        GridDecor.Pills(_listsGrid, _statusTextColumn.Name, (text, _) => GridDecor.StatusTone(text));
        Theme.StyleGrid(_rulesGrid);
        _pricePerKmColumn.DefaultCellStyle.NullValue = _pricePerTonColumn.DefaultCellStyle.NullValue = "Chưa có giá";
        Load += (_, _) => Theme.UniformButtons(_toolbar);
        Disposed += (_, _) => { _rulesTab.Dispose(); _listsTab.Dispose(); };
        UpdateActions();
    }

    public PricingControl(ApiServices api) : this()
    {
        _api = api;
        _create.Click += async (_, _) => await EditAsync(create: true);
        _copy.Click += async (_, _) => await CopyAsync();
        _edit.Click += async (_, _) => await EditAsync(create: false);
        _delete.Click += async (_, _) => await DeleteAsync();
        _refresh.Click += async (_, _) => { if (ConfirmDiscard()) await LoadAsync(); };
        _toolbar.Controls.Add(_saveRules);
        _saveRules.Click += async (_, _) => await SaveRulesAsync();
        _rulesGrid.DataError += (_, e) =>
        {
            e.ThrowException = false;
            e.Cancel = true;
            _status.ForeColor = Theme.Danger;
            _status.Text = "Đơn giá phải là số hợp lệ, tối đa 2 chữ số thập phân.";
        };
        var menu = new ContextMenuStrip(components);
        var detailMenu = (ToolStripMenuItem)menu.Items.Add("📄 Xem chi tiết bảng giá", null, async (_, _) => await ShowDetailsAsync());
        detailMenu.Font = Theme.FontBold;
        detailMenu.ShortcutKeyDisplayString = "Nhấp đúp";
        menu.Items.Add(new ToolStripSeparator());
        var copyMenu = menu.Items.Add("📋 Tạo mới và sao chép", null, async (_, _) => await CopyAsync());
        var editMenu = menu.Items.Add("✏️ Sửa bảng giá", null, async (_, _) => await EditAsync(create: false));
        menu.Items.Add(new ToolStripSeparator());
        var deleteMenu = menu.Items.Add("🗑 Xóa bảng giá", null, async (_, _) => await DeleteAsync());
        menu.Opening += (_, e) =>
        {
            UpdateActions();
            e.Cancel = _busy || SelectedList is null;
            copyMenu.Enabled = _copy.Enabled;
            editMenu.Enabled = _edit.Enabled;
            deleteMenu.Enabled = _delete.Enabled;
            editMenu.ToolTipText = editMenu.Enabled ? "Sửa thông tin bảng giá đang áp dụng."
                : "Bảng giá ngừng áp dụng không được sửa. Có thể sao chép để tạo bảng giá mới.";
            deleteMenu.ToolTipText = deleteMenu.Enabled ? "Xóa bảng giá và toàn bộ chi tiết giá sau khi xác nhận."
                : "Không thể xóa bảng giá đang được hợp đồng sử dụng.";
        };
        ModernMenu.Apply(menu);
        _listsGrid.ContextMenuStrip = menu;
        _listsGrid.CellMouseDown += (_, e) =>
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
                _listsGrid.CurrentCell = _listsGrid.Rows[e.RowIndex].Cells[Math.Max(0, e.ColumnIndex)];
        };
        _listsGrid.SelectionChanged += (_, _) =>
        {
            if (!_binding && !_busy && SelectedList is { } list) _priceList.SelectedValue = list.Id;
            UpdateActions();
        };
        _rulesGrid.CurrentCellChanged += (_, _) => UpdateActions();
        _priceList.SelectedIndexChanged += async (_, _) =>
        {
            if (!IsPriceLists && !_binding && !_busy) await LoadRulesAsync();
        };
        _listsGrid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await ShowDetailsAsync(); };
        Load += async (_, _) => await LoadAsync();
        UpdateActions();
    }

    private PricingControl(ApiServices api, int listId) : this(api)
    {
        _detailListId = listId;
        _layout.Controls.Remove(_listsTab);
        _listsTab.Visible = false;
        _filters.Visible = false;
        _rulesTab.Controls.Remove(_rulesLayout);
        _layout.Controls.Add(_rulesLayout, 0, 2);
        _title.Text = "Chi tiết bảng giá";
        _priceList.Visible = false;
        UpdateActions();
    }

    private async Task ShowDetailsAsync()
    {
        if (_busy || SelectedList is not { } list) return;
        using var form = new BaseForm
        {
            Text = $"Chi tiết bảng giá — {list.Name}", StartPosition = FormStartPosition.CenterParent,
            Size = new Size(1100, 650), MinimumSize = new Size(800, 450),
            ShowInTaskbar = false, MinimizeBox = false
        };
        var detail = new PricingControl(_api, list.Id) { Dock = DockStyle.Fill };
        form.Controls.Add(detail);
        form.FormClosing += (_, e) => e.Cancel = detail._busy || !detail.ConfirmDiscard();
        ButtonStyles.Footer(form, null, FormStyle.NewCloseButton(form));
        ModernLook.Apply(form);
        Anim.FadeIn(form);
        form.ShowDialog(FindForm());
        await LoadAsync(list.Id);
    }

    private void UpdateActions()
    {
        _create.Visible = IsPriceLists;
        _create.Enabled = !_busy && IsPriceLists;
        _saveRules.Visible = !IsPriceLists;
        _saveRules.Enabled = !_busy && CanEditRules;
        _rulesGrid.ReadOnly = _busy || !CanEditRules;
        _vehicleTypeColumn.ReadOnly = true;
        _copy.Visible = false;
        _edit.Visible = false;
        _delete.Visible = !IsPriceLists;
        _copy.Enabled = !_busy && IsPriceLists && SelectedList is not null;
        _edit.Enabled = !_busy && (IsPriceLists ? SelectedList?.Status == "Active" : CanEditRules && SelectedRule is not null);
        _delete.Enabled = !_busy && (IsPriceLists ? SelectedList?.ContractCount == 0 : CanEditRules && SelectedRule is not null);
        _refresh.Enabled = !_busy;
        _listsGrid.Enabled = _priceList.Enabled = !_busy;
        _filters.Enabled = !_busy;
    }

    private void BindLists(int? selectedId = null)
    {
        selectedId ??= SelectedList?.Id;
        var from = _filters.Date("effectiveFrom");
        var to = _filters.Date("effectiveTo");
        var rows = _lists.Where(item => _filters.MatchesSearch(item.Name)
            && (!from.HasValue || item.EffectiveDate.Date >= from.Value)
            && (!to.HasValue || item.EffectiveDate.Date <= to.Value)).ToList();
        _listsGrid.DataSource = rows;
        foreach (DataGridViewRow row in _listsGrid.Rows)
            if (row.DataBoundItem is PriceListItem item && item.Id == selectedId) _listsGrid.CurrentCell = row.Cells[0];
        _filters.SetCount(rows.Count, _lists.Count);
        if (!_binding && SelectedList is { } selected) _priceList.SelectedValue = selected.Id;
        UpdateActions();
    }

    private void UpdateStatus()
    {
        _status.ForeColor = Theme.TextSecondary;
        _status.Text = IsPriceLists
            ? _lists.Count == 0 ? "Chưa có bảng giá. Chọn Thêm mới để tạo." : $"{_lists.Count} bảng giá · Chuột phải để xem chi tiết. Bảng giá ngừng áp dụng không được sửa; bảng giá đang được hợp đồng sử dụng không thể xóa."
            : CurrentList is null ? "Tạo bảng giá trước khi thêm chi tiết giá."
            : !_rulesLoaded ? "Chưa tải được chi tiết giá. Chọn Làm mới để thử lại."
            : !CanEditRules ? $"{CurrentList.Name}: {_rules.Count} dòng giá · {CurrentList.StatusText} · Chỉ xem chi tiết giá."
            : $"{CurrentList.Name}: {_rules.Count} dòng giá · Nhập giá/km và giá/tấn trực tiếp rồi chọn Lưu thay đổi (nhập đủ số đồng).";
    }

    private async Task LoadAsync(int? preferredId = null)
    {
        if (_busy) return;
        var selectedId = _detailListId ?? preferredId ?? CurrentList?.Id ?? SelectedList?.Id;
        _busy = true;
        _rulesLoaded = false;
        UpdateActions();
        _status.Text = "Đang tải bảng giá...";
        try
        {
            _lists = await _api.Pricing.GetPriceListsAsync();
            if (_detailListId.HasValue) _lists = _lists.Where(p => p.Id == _detailListId).ToList();
            if (IsDisposed) return;
            _binding = true;
            _priceList.DisplayMember = nameof(PriceListItem.Name);
            _priceList.ValueMember = nameof(PriceListItem.Id);
            _priceList.DataSource = _lists.ToList();
            if (selectedId.HasValue && _lists.Any(p => p.Id == selectedId))
            {
                _priceList.SelectedValue = selectedId.Value;
            }
            BindLists(selectedId);
            if (IsPriceLists && SelectedList is { } selected) _priceList.SelectedValue = selected.Id;
            _binding = false;
            if (!IsPriceLists) await FetchRulesAsync();
            if (!IsDisposed) UpdateStatus();
        }
        catch (Exception ex) { ShowError(ex); }
        finally { _binding = false; _busy = false; if (!IsDisposed) UpdateActions(); }
    }

    private async Task FetchRulesAsync()
    {
        _rules = new();
        _rulesGrid.DataSource = new List<VehicleRateRow>();
        if (CurrentList is { } list)
        {
            _filterLabel.Text = "Bảng giá: " + list.Name;
            _rules = await _api.Pricing.GetTariffRulesAsync(list.Id);
            var types = await _api.Pricing.GetVehicleTypesAsync();
            if (IsDisposed) return;
            var rows = _rules.Select(r => new VehicleRateRow(r.VehicleType, r.PricePerKm, r.PricePerTon, r)).ToList();
            rows.AddRange(types.Select(t => t.Trim()).Where(t => t.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(t => !_rules.Any(r => string.Equals(r.VehicleType.Trim(), t, StringComparison.OrdinalIgnoreCase)))
                .Select(t => new VehicleRateRow(t, null, null, null)));
            _rulesGrid.DataSource = rows.OrderBy(r => r.VehicleType).ToList();
        }
        _rulesLoaded = true;
    }

    private async Task LoadRulesAsync()
    {
        _busy = true;
        _rulesLoaded = false;
        UpdateActions();
        _status.Text = "Đang tải chi tiết giá...";
        try { await FetchRulesAsync(); if (!IsDisposed) UpdateStatus(); }
        catch (Exception ex) { ShowError(ex); }
        finally { _busy = false; if (!IsDisposed) UpdateActions(); }
    }

    private bool ConfirmDiscard()
    {
        if (IsPriceLists) return true;
        if (!_rulesGrid.EndEdit()) return false;
        return !HasChanges || MessageBox.Show(FindForm(), "Có đơn giá chưa lưu. Bỏ các thay đổi này?",
            "Đơn giá chưa lưu", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2) == DialogResult.Yes;
    }

    private async Task SaveRulesAsync()
    {
        if (_busy || !CanEditRules || CurrentList is not { } list || !_rulesGrid.EndEdit()) return;
        var changed = VehicleRows.Where(r => r.IsChanged).ToList();
        if (changed.Count == 0) { UpdateStatus(); return; }
        foreach (var row in changed)
        {
            try
            {
                if (!row.PricePerKm.HasValue || !row.PricePerTon.HasValue)
                    throw new ArgumentException("Nhập đủ giá/km và giá/tấn; nhập 0 nếu không tính loại giá đó.");
                TariffRule.Validate(row.VehicleType, row.PricePerKm.Value, row.PricePerTon.Value, 0, 0);
            }
            catch (ArgumentException ex)
            {
                var gridRow = _rulesGrid.Rows.Cast<DataGridViewRow>().Single(r => r.DataBoundItem == row);
                gridRow.ErrorText = ex.Message;
                _rulesGrid.CurrentCell = gridRow.Cells[_pricePerKmColumn.Index];
                _status.ForeColor = Theme.Danger;
                _status.Text = row.VehicleType + ": " + ex.Message;
                return;
            }
        }
        _busy = true;
        UpdateActions();
        _status.Text = "Đang lưu đơn giá...";
        try
        {
            _rules = await _api.Pricing.SaveTariffRulesAsync(list.Id, changed.Select(r =>
                new SaveTariffRuleRow(r.Rule?.Id, r.VehicleType, r.PricePerKm!.Value, r.PricePerTon!.Value)).ToList());
            if (IsDisposed) return;
            foreach (var row in changed)
            {
                row.Rule = row.Rule is { } original ? _rules.Single(r => r.Id == original.Id)
                    : _rules.Single(r => r.VehicleType == row.VehicleType);
                _rulesGrid.Rows.Cast<DataGridViewRow>().Single(r => r.DataBoundItem == row).ErrorText = "";
            }
            UpdateStatus();
            _status.Text = $"Đã lưu {changed.Count} dòng giá.";
        }
        catch (Exception ex)
        {
            _status.ForeColor = Theme.Danger;
            _status.Text = "Lỗi lưu: " + ex.Message + " Dữ liệu nhập được giữ lại để thử lại.";
        }
        finally { _busy = false; if (!IsDisposed) UpdateActions(); }
    }

    private async Task EditAsync(bool create)
    {
        if (_busy || !IsPriceLists) return;
        try
        {
            if (!create && SelectedList?.Status != "Active") return;
            using var form = new PriceListForm(_api, create ? null : SelectedList);
            if (form.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync(form.SavedId);
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private async Task CopyAsync()
    {
        if (_busy || !IsPriceLists || SelectedList is not { } list) return;
        try
        {
            using var form = new PriceListForm(_api, list, copy: true);
            if (form.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync(form.SavedId);
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private async Task DeleteAsync()
    {
        if (_busy) return;
        if (!ConfirmDiscard()) return;
        var isList = IsPriceLists;
        var list = isList ? SelectedList : CurrentList;
        var rule = SelectedRule;
        if (list is null || isList && list.ContractCount > 0 || !isList && (!CanEditRules || rule is null)) return;
        var message = isList
            ? $"Xóa bảng giá \"{list.Name}\" và toàn bộ {list.RuleCount} dòng giá đi kèm?\nThao tác này không thể hoàn tác."
            : $"Xóa giá của loại xe \"{rule!.VehicleType}\" khỏi bảng giá \"{list.Name}\"?\nThao tác này không thể hoàn tác.";
        if (MessageBox.Show(FindForm(), message, "Xác nhận xóa", MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
        _busy = true;
        UpdateActions();
        try
        {
            if (isList) await _api.Pricing.DeletePriceListAsync(list.Id);
            else await _api.Pricing.DeleteTariffRuleAsync(list.Id, rule!.Id);
        }
        catch (Exception ex) { ShowError(ex); return; }
        finally { _busy = false; UpdateActions(); }
        await LoadAsync();
    }

    private void ShowError(Exception ex)
    {
        if (IsDisposed) return;
        _status.ForeColor = Theme.Danger;
        _status.Text = "Lỗi: " + ex.Message;
        MessageBox.Show(FindForm(), ex.Message, "Bảng giá", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
