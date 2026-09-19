using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_VehicleManagement;

public partial class VehiclesControl : BaseUserControl
{
    private ApiServices? _api;

    public VehiclesControl()
    {
        InitializeComponent();
        Theme.StyleGrid(grid);
        Resize += (_, _) => { grid.Width = Width; grid.Height = Height - grid.Top; };
        Load += async (_, _) => await LoadAsync();
    }

    public VehiclesControl(ApiServices api) : this()
    {
        _api = api;
    }

    private void BtnRefresh_Click(object? sender, EventArgs e) => _ = LoadAsync();

    private async void BtnCreate_Click(object? sender, EventArgs e)
    {
        if (_api is null) return;
        using var f = new CreateVehicleForm(_api);
        if (f.ShowDialog() == DialogResult.OK) await LoadAsync();
    }

    private async Task LoadAsync()
    {
        if (_api is null) return;
        lblStatus.Text = "Đang tải...";
        try
        {
            var vehicles = await _api.Vehicles.GetVehiclesAsync();
            grid.DataSource = vehicles.Select(v => new
            {
                Biển_số = v.PlateNumber,
                Loại_xe = v.VehicleType,
                Sức_chứa = v.CapacityQty,
                Trạng_thái = TranslateStatus(v.Status),
            }).ToList();
            lblStatus.Text = $"Tổng {vehicles.Count} xe";
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Lỗi: " + ex.Message;
            lblStatus.ForeColor = Theme.Danger;
        }
    }

    private static string TranslateStatus(string s) => s switch
    {
        "Available" => "Sẵn sàng", "OnTrip" => "Đang chạy", "UnderMaintenance" => "Đang bảo trì",
        "OutOfService" => "Ngừng hoạt động", _ => s,
    };
}
