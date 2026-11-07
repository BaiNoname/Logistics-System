using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_ContractManagement;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Desktop.GUI.GUI_ContractManagement;

public partial class ContractsControl : BaseUserControl
{
    private ApiServices? _api;
    private List<ContractListItem> _contracts = new();
    private bool _busy;
    private readonly FilterBar _filters = new() { Dock = DockStyle.Fill, SearchPlaceholder = "Mã hợp đồng, khách hàng, mã bảng giá" };
    private readonly Button _discountSettings = Theme.MakeSecondaryButton("Quản lý ưu đãi", 170);
    private readonly ToolStripMenuItem _details = new("📄 Xem chi tiết");
    private readonly ToolStripMenuItem _edit = new("✏️ Sửa hợp đồng");
    private readonly ToolStripMenuItem _appendix = new("➕ Thêm phụ lục");
    private readonly ToolStripMenuItem _delete = new("🗑 Xóa hợp đồng");

    public ContractsControl()
    {
        InitializeComponent();
        Theme.StyleGrid(grid);
        GridDecor.Pills(grid, "Trạng_thái", (text, _) => GridDecor.StatusTone(text));
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
        lblTitle.Margin = new Padding(0, 0, 24, 8);
        foreach (var control in new Control[] { lblTitle, btnCreate, _discountSettings, btnRefresh })
        {
            control.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            toolbar.Controls.Add(control);
        }
        var menu = new ContextMenuStrip(components ??= new System.ComponentModel.Container());
        _details.Font = Theme.FontBold;
        _details.ShortcutKeyDisplayString = "Nhấp đúp";
        menu.Items.AddRange([_details, new ToolStripSeparator(), _edit, _appendix, new ToolStripSeparator(), _delete]);
        _edit.Click += BtnEdit_Click;
        _appendix.Click += async (_, _) => await AddAppendixAsync();
        menu.Opening += (_, e) =>
        {
            UpdateActions();
            e.Cancel = _api is null || _busy || SelectedContract is null;
        };
        ModernMenu.Apply(menu);
        grid.ContextMenuStrip = menu;
        grid.CellMouseDown += (_, e) =>
        {
            if (e.Button != MouseButtons.Right || e.RowIndex < 0 || _busy) return;
            grid.CurrentCell = grid.Rows[e.RowIndex].Cells["Mã_hợp_đồng"];
        };
        _discountSettings.Click += (_, _) =>
        {
            if (_api is null || _busy || !_api.Connection.IsInRole("Admin")) return;
            using var form = new DiscountConfigurationForm(_api);
            form.ShowDialog(FindForm());
        };
        _filters.AddChoice("status", "Trạng thái", new[] { "Chưa hiệu lực", "Đang hiệu lực", "Hết hạn" });
        _filters.AddChoice("type", "Loại hợp đồng", new[] {
            new ContractListItem { Type = "OneTime" }.TypeText,
            new ContractListItem { Type = "LongTerm" }.TypeText, "Chưa phân loại" });
        _filters.AddDate("startFrom", "Bắt đầu từ");
        _filters.AddDate("startTo", "Bắt đầu đến");
        _filters.AddDate("endFrom", "Kết thúc từ");
        _filters.AddDate("endTo", "Kết thúc đến");
        _filters.FilterChanged += (_, _) => BindContracts();
        _details.Click += async (_, _) => await ShowDetailsAsync();
        _delete.Click += async (_, _) => await DeleteAsync();
        grid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await ShowDetailsAsync(); };
        grid.SelectionChanged += (_, _) => UpdateActions();
        grid.Dock = DockStyle.Fill;
        lblStatus.Dock = DockStyle.Fill;
        lblStatus.AutoSize = false;
        lblStatus.AutoEllipsis = true;
        layout.Controls.Add(toolbar, 0, 0);
        layout.Controls.Add(_filters, 0, 1);
        layout.Controls.Add(lblStatus, 0, 2);
        layout.Controls.Add(grid, 0, 3);
        Controls.Add(layout);
        Load += (_, _) => Theme.UniformButtons(toolbar);
        UpdateActions();
        Load += async (_, _) => await LoadAsync();
    }

    public ContractsControl(ApiServices api) : this() { _api = api; UpdateActions(); }

    private ContractListItem? SelectedContract
        => grid.CurrentRow?.Cells["Id"].Value is int id ? _contracts.FirstOrDefault(c => c.Id == id) : null;

    private void UpdateActions()
    {
        btnCreate.Enabled = btnRefresh.Enabled = !_busy;
        _discountSettings.Visible = _api?.Connection.IsInRole("Admin") == true;
        _discountSettings.Enabled = !_busy;
        _edit.Enabled = _details.Enabled = _delete.Enabled = !_busy && SelectedContract is not null;
        _appendix.Enabled = !_busy && SelectedContract is { } contract && IsActive(contract);
        _appendix.ToolTipText = _appendix.Enabled ? "Thêm phụ lục cho hợp đồng được chọn."
            : "Chỉ được thêm phụ lục cho hợp đồng đang hiệu lực.";
    }

    private async void BtnRefresh_Click(object? sender, EventArgs e) => await LoadAsync();

    private async void BtnCreate_Click(object? sender, EventArgs e)
    {
        if (_api is null || _busy) return;
        using var form = new CreateContractForm(_api);
        if (form.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
    }

    private async void BtnEdit_Click(object? sender, EventArgs e)
    {
        if (_api is null || _busy || SelectedContract is not { } contract) return;
        using var form = new EditContractForm(_api, contract);
        if (form.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
    }

    private async Task ShowDetailsAsync()
    {
        if (_api is null || _busy || SelectedContract is not { } selected) return;
        _busy = true;
        UpdateActions();
        try
        {
            var contract = await _api.Contracts.GetContractAsync(selected.Id);
            if (contract is null) throw new InvalidOperationException("Không tìm thấy hợp đồng. Vui lòng làm mới danh sách.");
            using var form = new ContractForm(_api, contract, readOnly: true);
            form.ShowDialog(FindForm());
        }
        catch (Exception ex) { ShowError(ex); }
        finally { _busy = false; UpdateActions(); }
    }

    private static bool IsActive(ContractListItem contract)
        => Contract.GetStatus(contract.StartDate, contract.EndDate, DateTime.UtcNow.AddHours(7)) == ContractStatus.Active;

    private async Task AddAppendixAsync()
    {
        if (_api is null || _busy || SelectedContract is not { } contract || !IsActive(contract)) return;
        var saved = false;
        _busy = true;
        UpdateActions();
        try
        {
            using var form = new ContractAppendixForm(_api, contractId: contract.Id);
            saved = form.ShowDialog(FindForm()) == DialogResult.OK;
        }
        catch (Exception ex) { ShowError(ex); }
        finally { _busy = false; UpdateActions(); }
        if (saved) await LoadAsync();
    }

    private async Task DeleteAsync()
    {
        if (_api is null || _busy || SelectedContract is not { } contract) return;
        if (MessageBox.Show(FindForm(), $"Xóa hợp đồng {contract.Code} của {contract.CustomerName}?\nThao tác này không thể hoàn tác.",
            "Xác nhận xóa hợp đồng", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
        _busy = true;
        UpdateActions();
        try { await _api.Contracts.DeleteContractAsync(contract.Id); }
        catch (Exception ex) { ShowError(ex); return; }
        finally { _busy = false; UpdateActions(); }
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        if (_api is null || _busy) return;
        _busy = true;
        UpdateActions();
        lblStatus.Text = "Đang tải hợp đồng...";
        lblStatus.ForeColor = Theme.TextSecondary;
        try
        {
            _contracts = await _api.Contracts.GetContractsAsync();
            BindContracts();
        }
        catch (Exception ex) { ShowError(ex); }
        finally { _busy = false; UpdateActions(); }
    }

    private void BindContracts()
    {
        var selectedId = SelectedContract?.Id;
        var startFrom = _filters.Date("startFrom"); var startTo = _filters.Date("startTo");
        var endFrom = _filters.Date("endFrom"); var endTo = _filters.Date("endTo");
        var contracts = _contracts.Where(c =>
            _filters.MatchesSearch(c.Code, c.CustomerName, c.PriceListName)
            && _filters.Is("status", c.StatusText) && _filters.Is("type", c.TypeText)
            && (!startFrom.HasValue || c.StartDate.Date >= startFrom.Value)
            && (!startTo.HasValue || c.StartDate.Date <= startTo.Value)
            && (!endFrom.HasValue || c.EndDate?.Date >= endFrom.Value)
            && (!endTo.HasValue || c.EndDate?.Date <= endTo.Value)).ToList();
        _filters.SetCount(contracts.Count, _contracts.Count);
        grid.DataSource = contracts.Select(c => new
        {
            c.Id,
            Mã_hợp_đồng = c.Code,
            Khách_hàng = c.CustomerName,
            Trạng_thái = c.StatusText,
            Ngày_bắt_đầu = c.StartDate.ToString("dd/MM/yyyy"),
            Ngày_kết_thúc = c.EndDate?.ToString("dd/MM/yyyy") ?? "Không thời hạn",
            Loại_hợp_đồng = c.TypeText,
            Ưu_đãi = c.DiscountPercent.HasValue ? $"{c.DiscountPercent:0.##}%" : "—",
            Bảng_giá = c.PriceListName ?? "—",
            Phụ_lục = c.AppendixText,
        }).ToList();
        if (grid.Columns["Id"] is not null) grid.Columns["Id"]!.Visible = false;
        foreach (DataGridViewRow row in grid.Rows)
            if (row.Cells["Id"].Value is int id && id == selectedId) grid.CurrentCell = row.Cells["Mã_hợp_đồng"];
        lblStatus.ForeColor = Theme.TextSecondary;
        lblStatus.Text = contracts.Count == 0 ? "Không có hợp đồng phù hợp." : $"Hiển thị {contracts.Count}/{_contracts.Count} hợp đồng · Nhấp đúp để xem chi tiết";
        UpdateActions();
    }

    private void ShowError(Exception ex)
    {
        lblStatus.Text = "Lỗi: " + ex.Message;
        lblStatus.ForeColor = Theme.Danger;
        MessageBox.Show(FindForm(), ex.Message, "Hợp đồng", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
