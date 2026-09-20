using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_WarehouseManagement;

namespace TruckingSystem.Desktop.GUI_WarehouseManagement;

/// <summary>Danh sách khu vực (Zone) của 1 kho — thêm khu vực mới, hoặc chọn 1
/// khu vực để quản lý các vị trí lưu trữ (StorageBin) bên trong nó.</summary>
public partial class ManageZonesForm : BaseForm
{
    private ApiServices? _api;
    private int _warehouseId;
    private List<WarehouseZoneItem> _zones = new();

    public ManageZonesForm()
    {
        InitializeComponent();
        Theme.StyleGrid(grid);
        grid.CellDoubleClick += (_, _) => OpenBinsForSelected();
    }

    public ManageZonesForm(ApiServices api, int warehouseId) : this()
    {
        _api = api;
        _warehouseId = warehouseId;
        Load += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        if (_api is null) return;
        lblStatus.Text = "Đang tải...";
        try
        {
            _zones = await _api.Warehouses.GetZonesAsync(_warehouseId);
            grid.DataSource = _zones.Select(z => new
            {
                z.Id,
                Tên_khu_vực = z.Name,
                Loại = TranslateZoneType(z.ZoneType),
                Vị_trí_X = z.PosX,
                Vị_trí_Y = z.PosY,
                Rộng_m = z.WidthMeters,
                Cao_m = z.HeightMeters,
                Số_vị_trí = z.BinCount,
            }).ToList();
            grid.DataBindingComplete += (_, _) => { if (grid.Columns["Id"] != null) grid.Columns["Id"]!.Visible = false; };
            lblStatus.Text = $"{_zones.Count} khu vực — double-click 1 dòng để quản lý vị trí lưu trữ bên trong";
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Lỗi: " + ex.Message;
            lblStatus.ForeColor = Theme.Danger;
        }
    }

    private async void BtnDeleteZone_Click(object? sender, EventArgs e)
    {
        if (_api is null || grid.CurrentRow is null)
        {
            MessageBox.Show("Vui lòng chọn 1 khu vực trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var zoneId = (int)grid.CurrentRow.Cells["Id"].Value;
        var zoneName = _zones.First(z => z.Id == zoneId).Name;

        var confirm = MessageBox.Show($"Xóa hẳn khu vực \"{zoneName}\"? Không thể hoàn tác.",
            "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes) return;

        try
        {
            var result = await _api.Warehouses.DeleteZoneAsync(zoneId);
            if (!result.Success)
            {
                MessageBox.Show(result.Message ?? "Không thể xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnAddZone_Click(object? sender, EventArgs e)
    {
        if (_api is null) return;
        using var f = new CreateZoneForm(_api, _warehouseId);
        if (f.ShowDialog() == DialogResult.OK) await LoadAsync();
    }

    private void OpenBinsForSelected()
    {
        if (_api is null || grid.CurrentRow is null) return;
        var zoneId = (int)grid.CurrentRow.Cells["Id"].Value;
        var zoneName = _zones.First(z => z.Id == zoneId).Name;

        using var f = new ManageBinsForm(_api, zoneId, zoneName);
        f.ShowDialog();
        _ = LoadAsync(); // cập nhật lại số vị trí sau khi có thể vừa thêm
    }

    private static string TranslateZoneType(string s) => s switch
    {
        "Fragile" => "Hàng dễ vỡ",
        "Hazmat" => "Hàng nguy hiểm",
        _ => "Hàng thường",
    };
}