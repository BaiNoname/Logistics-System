using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_ContractManagement;

namespace TruckingSystem.Desktop.GUI.GUI_ContractManagement;

public partial class ContractsControl : BaseUserControl
{
    private ApiServices? _api;
    private List<ContractListItem> _contracts = new();

    public ContractsControl()
    {
        InitializeComponent();
        Theme.StyleGrid(grid);
        Resize += (_, _) =>
        {
            grid.Width = Width;
            grid.Height = Height - grid.Top;
            btnRefresh.Left = Width - btnRefresh.Width - 10;
            btnEdit.Left = btnRefresh.Left - btnEdit.Width - 8;
            btnCreate.Left = btnEdit.Left - btnCreate.Width - 8;
        };
        Load += async (_, _) => await LoadAsync();
    }

    public ContractsControl(ApiServices api) : this() => _api = api;

    private void BtnRefresh_Click(object? sender, EventArgs e) => _ = LoadAsync();

    private async void BtnCreate_Click(object? sender, EventArgs e)
    {
        if (_api is null) return;
        using var form = new CreateContractForm(_api);
        if (form.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
    }

    private async void BtnEdit_Click(object? sender, EventArgs e)
    {
        if (_api is null || grid.CurrentRow?.Cells["Id"].Value is not int id)
        {
            MessageBox.Show(FindForm(), "Vui lòng chọn một hợp đồng trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var contract = _contracts.FirstOrDefault(c => c.Id == id);
        if (contract is null) return;
        using var form = new EditContractForm(_api, contract);
        if (form.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
    }

    private async Task LoadAsync()
    {
        if (_api is null) return;
        //lblStatus.Text = "Đang tải...";
        lblStatus.ForeColor = Theme.TextSecondary;
        try
        {
            _contracts = await _api.Contracts.GetContractsAsync();
            grid.DataSource = _contracts.Select(c => new
            {
                c.Id,
                Mã_hợp_đồng = c.Code,
                Khách_hàng = c.CustomerName,
                Ngày_bắt_đầu = c.StartDate.ToString("dd/MM/yyyy"),
                Ngày_kết_thúc = c.EndDate?.ToString("dd/MM/yyyy") ?? "Không thời hạn",
                Bảng_giá = c.PriceListName ?? "—",
                Điều_khoản = c.Terms ?? "—",
                Ngày_tạo = c.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                Cập_nhật = c.UpdatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "—",
            }).ToList();
            if (grid.Columns["Id"] is not null) grid.Columns["Id"]!.Visible = false;
            //lblStatus.Text = $"Tổng {_contracts.Count} hợp đồng — chọn một dòng rồi bấm Sửa để chỉnh sửa";
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Lỗi: " + ex.Message;
            lblStatus.ForeColor = Theme.Danger;
        }
    }
}
