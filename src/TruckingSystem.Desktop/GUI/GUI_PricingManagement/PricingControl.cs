using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_PricingManagement;

namespace TruckingSystem.Desktop.GUI.GUI_PricingManagement;

public partial class PricingControl : BaseUserControl
{
    private readonly ApiServices _api = null!;
    private List<PriceListItem> _lists = new();
    private List<TariffRuleItem> _rules = new();
    private bool _busy;
    private bool _binding;
    private bool _rulesLoaded;

    private bool IsPriceLists => _tabs.SelectedIndex == 0;
    private PriceListItem? SelectedList => _listsGrid.CurrentRow?.DataBoundItem is PriceListItem item ? item : null;
    private PriceListItem? CurrentList => _priceList.SelectedItem as PriceListItem;
    private TariffRuleItem? SelectedRule => _rulesGrid.CurrentRow?.DataBoundItem as TariffRuleItem;
    private bool CanEditRules => _rulesLoaded && CurrentList?.Status == "Active";

    public PricingControl()
    {
        InitializeComponent();
        Theme.StyleGrid(_listsGrid);
        Theme.StyleGrid(_rulesGrid);
        UpdateActions();
    }

    public PricingControl(ApiServices api) : this()
    {
        _api = api;
        _create.Click += async (_, _) => await EditAsync(create: true);
        _copy.Click += async (_, _) => await CopyAsync();
        _edit.Click += async (_, _) => await EditAsync(create: false);
        _delete.Click += async (_, _) => await DeleteAsync();
        _refresh.Click += async (_, _) => await LoadAsync();
        _tabs.SelectedIndexChanged += (_, _) => { UpdateActions(); UpdateStatus(); };
        _listsGrid.SelectionChanged += (_, _) =>
        {
            if (!_binding && !_busy && SelectedList is { } list) _priceList.SelectedValue = list.Id;
            UpdateActions();
        };
        _rulesGrid.SelectionChanged += (_, _) => UpdateActions();
        _priceList.SelectedIndexChanged += async (_, _) =>
        {
            if (!_binding && !_busy) await LoadRulesAsync();
        };
        _listsGrid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await EditAsync(create: false); };
        _rulesGrid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await EditAsync(create: false); };
        Load += async (_, _) => await LoadAsync();
        UpdateActions();
    }

    private void UpdateActions()
    {
        _create.Enabled = !_busy && (IsPriceLists || CanEditRules);
        _copy.Visible = IsPriceLists;
        _copy.Enabled = !_busy && IsPriceLists && SelectedList is not null;
        _edit.Enabled = !_busy && (IsPriceLists ? SelectedList is not null : CanEditRules && SelectedRule is not null);
        _delete.Enabled = _edit.Enabled && (!IsPriceLists || SelectedList?.ContractCount == 0);
        _refresh.Enabled = !_busy;
        _tabs.Enabled = !_busy;
    }

    private void UpdateStatus()
    {
        _status.ForeColor = Theme.TextSecondary;
        _status.Text = IsPriceLists
            ? _lists.Count == 0 ? "Chưa có bảng giá. Chọn Thêm mới để tạo." : $"{_lists.Count} bảng giá · Bảng giá đang được hợp đồng sử dụng không thể xóa."
            : CurrentList is null ? "Tạo bảng giá trước khi thêm chi tiết giá."
            : !_rulesLoaded ? "Chưa tải được chi tiết giá. Chọn Làm mới để thử lại."
            : !CanEditRules ? $"{CurrentList.Name}: {_rules.Count} dòng giá · {CurrentList.StatusText} · Chỉ xem chi tiết giá."
            : $"{CurrentList.Name}: {_rules.Count} dòng giá · Nhấp đúp để sửa.";
    }

    private async Task LoadAsync(int? preferredId = null)
    {
        if (_busy) return;
        var selectedId = preferredId ?? CurrentList?.Id ?? SelectedList?.Id;
        _busy = true;
        _rulesLoaded = false;
        UpdateActions();
        _status.Text = "Đang tải bảng giá...";
        try
        {
            _lists = await _api.Pricing.GetPriceListsAsync();
            if (IsDisposed) return;
            _binding = true;
            _listsGrid.DataSource = _lists;
            _priceList.DisplayMember = nameof(PriceListItem.Name);
            _priceList.ValueMember = nameof(PriceListItem.Id);
            _priceList.DataSource = _lists.ToList();
            if (selectedId.HasValue && _lists.Any(p => p.Id == selectedId))
            {
                _priceList.SelectedValue = selectedId.Value;
                foreach (DataGridViewRow row in _listsGrid.Rows)
                    if (row.DataBoundItem is PriceListItem p && p.Id == selectedId) _listsGrid.CurrentCell = row.Cells[0];
            }
            _binding = false;
            await FetchRulesAsync();
            if (!IsDisposed) UpdateStatus();
        }
        catch (Exception ex) { ShowError(ex); }
        finally { _binding = false; _busy = false; if (!IsDisposed) UpdateActions(); }
    }

    private async Task FetchRulesAsync()
    {
        _rules = new();
        _rulesGrid.DataSource = _rules;
        if (CurrentList is { } list) _rules = await _api.Pricing.GetTariffRulesAsync(list.Id);
        if (IsDisposed) return;
        _rulesGrid.DataSource = _rules;
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

    private async Task EditAsync(bool create)
    {
        if (_busy) return;
        try
        {
            if (IsPriceLists)
            {
                if (!create && SelectedList is null) return;
                using var form = new PriceListForm(_api, create ? null : SelectedList);
                if (form.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync(form.SavedId);
            }
            else
            {
                if (!CanEditRules || CurrentList is not { } list || !create && SelectedRule is null) return;
                using var form = new TariffRuleForm(_api, list, _rules, create ? null : SelectedRule);
                if (form.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync(list.Id);
            }
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
