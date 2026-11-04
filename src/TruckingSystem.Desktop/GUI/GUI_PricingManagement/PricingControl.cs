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

    private int? _detailListId;
    private bool IsPriceLists => !_detailListId.HasValue;
    private PriceListItem? SelectedList => _listsGrid.CurrentRow?.DataBoundItem is PriceListItem item ? item : null;
    private PriceListItem? CurrentList => _priceList.SelectedItem as PriceListItem;
    private TariffRuleItem? SelectedRule => _rulesGrid.CurrentRow?.DataBoundItem as TariffRuleItem;
    private bool CanEditRules => _rulesLoaded && CurrentList?.Status == "Active";

    public PricingControl()
    {
        InitializeComponent();
        Theme.StyleGrid(_listsGrid);
        GridDecor.Pills(_listsGrid, _statusTextColumn.Name, (text, _) => GridDecor.StatusTone(text));
        Theme.StyleGrid(_rulesGrid);
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
        _refresh.Click += async (_, _) => await LoadAsync();
        var menu = new ContextMenuStrip(components);
        menu.Items.Add("Xem chi tiết bảng giá", null, async (_, _) => await ShowDetailsAsync());
        menu.Opening += (_, e) => e.Cancel = _busy || SelectedList is null;
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
        _rulesGrid.SelectionChanged += (_, _) => UpdateActions();
        _priceList.SelectedIndexChanged += async (_, _) =>
        {
            if (!IsPriceLists && !_binding && !_busy) await LoadRulesAsync();
        };
        _listsGrid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await EditAsync(create: false); };
        _rulesGrid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await EditAsync(create: false); };
        Load += async (_, _) => await LoadAsync();
        UpdateActions();
    }

    private PricingControl(ApiServices api, int listId) : this(api)
    {
        _detailListId = listId;
        _layout.Controls.Remove(_listsTab);
        _listsTab.Visible = false;
        _rulesTab.Controls.Remove(_rulesLayout);
        _layout.Controls.Add(_rulesLayout, 0, 1);
        _title.Text = "Chi tiết bảng giá";
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
        form.Controls.Add(new PricingControl(_api, list.Id) { Dock = DockStyle.Fill });
        ButtonStyles.Footer(form, null, FormStyle.NewCloseButton(form));
        Anim.FadeIn(form);
        form.ShowDialog(FindForm());
        await LoadAsync(list.Id);
    }

    private void UpdateActions()
    {
        _create.Enabled = !_busy && (IsPriceLists || CanEditRules);
        _copy.Visible = IsPriceLists;
        _copy.Enabled = !_busy && IsPriceLists && SelectedList is not null;
        _edit.Enabled = !_busy && (IsPriceLists ? SelectedList?.Status == "Active" : CanEditRules && SelectedRule is not null);
        _delete.Enabled = !_busy && (IsPriceLists ? SelectedList?.ContractCount == 0 : CanEditRules && SelectedRule is not null);
        _refresh.Enabled = !_busy;
        _listsGrid.Enabled = _priceList.Enabled = !_busy;
    }

    private void UpdateStatus()
    {
        _status.ForeColor = Theme.TextSecondary;
        _status.Text = IsPriceLists
            ? _lists.Count == 0 ? "Chưa có bảng giá. Chọn Thêm mới để tạo." : $"{_lists.Count} bảng giá · Chuột phải để xem chi tiết. Bảng giá ngừng áp dụng không được sửa; bảng giá đang được hợp đồng sử dụng không thể xóa."
            : CurrentList is null ? "Tạo bảng giá trước khi thêm chi tiết giá."
            : !_rulesLoaded ? "Chưa tải được chi tiết giá. Chọn Làm mới để thử lại."
            : !CanEditRules ? $"{CurrentList.Name}: {_rules.Count} dòng giá · {CurrentList.StatusText} · Chỉ xem chi tiết giá."
            : $"{CurrentList.Name}: {_rules.Count} dòng giá · Nhấp đúp để sửa.";
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
            if (!IsPriceLists) await FetchRulesAsync();
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
                if (!create && SelectedList?.Status != "Active") return;
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
