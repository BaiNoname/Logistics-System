using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_ContractManagement;

namespace TruckingSystem.Desktop.GUI.GUI_ContractManagement;

public partial class ContractsControl : BaseUserControl
{
    private ApiServices? _api;
    private List<ContractListItem> _contracts = new();
    private bool _busy;
    private readonly TextBox _search = new() { Width = 280, PlaceholderText = "Mã hợp đồng hoặc tên khách hàng", AccessibleName = "Tìm hợp đồng" };
    private readonly ComboBox _statusFilter = new() { Width = 180, DropDownStyle = ComboBoxStyle.DropDownList, AccessibleName = "Lọc trạng thái hợp đồng" };
    private readonly Button _details = Theme.MakeSecondaryButton("Chi tiết", 100);
    private readonly Button _delete = Theme.MakeSecondaryButton("Xóa", 90);

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
        foreach (var control in new Control[] { lblTitle, btnCreate, btnEdit, _details, _delete, btnRefresh })
        {
            control.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            toolbar.Controls.Add(control);
        }
        _delete.ForeColor = Theme.Danger;
        var filters = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(0, 4, 0, 4) };
        filters.Controls.Add(new Label { Text = "Tìm kiếm", AutoSize = true, Margin = new Padding(0, 6, 8, 0) });
        filters.Controls.Add(_search);
        filters.Controls.Add(_statusFilter);
        _statusFilter.Items.AddRange(new object[] { "Tất cả trạng thái", "Chưa hiệu lực", "Đang hiệu lực", "Hết hạn" });
        _statusFilter.SelectedIndex = 0;
        _search.TextChanged += (_, _) => BindContracts();
        _statusFilter.SelectedIndexChanged += (_, _) => BindContracts();
        _details.Click += async (_, _) => await ShowDetailsAsync();
        _delete.Click += async (_, _) => await DeleteAsync();
        grid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await ShowDetailsAsync(); };
        grid.SelectionChanged += (_, _) => UpdateActions();
        grid.Dock = DockStyle.Fill;
        lblStatus.Dock = DockStyle.Fill;
        lblStatus.AutoSize = false;
        lblStatus.AutoEllipsis = true;
        layout.Controls.Add(toolbar, 0, 0);
        layout.Controls.Add(filters, 0, 1);
        layout.Controls.Add(lblStatus, 0, 2);
        layout.Controls.Add(grid, 0, 3);
        Controls.Add(layout);
        Load += (_, _) => Theme.UniformButtons(toolbar);
        UpdateActions();
        Load += async (_, _) => await LoadAsync();
    }

    public ContractsControl(ApiServices api) : this() => _api = api;

    private ContractListItem? SelectedContract
        => grid.CurrentRow?.Cells["Id"].Value is int id ? _contracts.FirstOrDefault(c => c.Id == id) : null;

    private void UpdateActions()
    {
        btnCreate.Enabled = btnRefresh.Enabled = !_busy;
        btnEdit.Enabled = _details.Enabled = _delete.Enabled = !_busy && SelectedContract is not null;
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
        var search = _search.Text.Trim();
        var contracts = _contracts.Where(c =>
            (c.Code.Contains(search, StringComparison.CurrentCultureIgnoreCase) || c.CustomerName.Contains(search, StringComparison.CurrentCultureIgnoreCase))
            && (_statusFilter.SelectedIndex == 0 || c.StatusText == _statusFilter.Text)).ToList();
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
            Chi_tiết_giá = c.TariffRule?.DisplayName ?? "—",
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
