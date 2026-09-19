using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_LocationManagement;

public partial class LocationsControl : BaseUserControl
{
    private ApiServices? _api;

    public LocationsControl()
    {
        InitializeComponent();
        Theme.StyleGrid(grid);
        Resize += (_, _) => { grid.Width = Width; grid.Height = Height - grid.Top; };
        Load += async (_, _) => await LoadAsync();
    }

    public LocationsControl(ApiServices api) : this()
    {
        _api = api;
    }

    private void BtnRefresh_Click(object? sender, EventArgs e) => _ = LoadAsync();

    private async void BtnCreate_Click(object? sender, EventArgs e)
    {
        if (_api is null) return;
        using var f = new CreateLocationForm(_api);
        if (f.ShowDialog() == DialogResult.OK) await LoadAsync();
    }

    private async Task LoadAsync()
    {
        if (_api is null) return;
        lblStatus.Text = "Đang tải...";
        try
        {
            var locations = await _api.Locations.GetLocationsAsync();
            grid.DataSource = locations.Select(l => new
            {
                Tên_địa_điểm = l.Name,
                Địa_chỉ = l.Address,
                Vĩ_độ = l.Lat?.ToString("0.0000") ?? "—",
                Kinh_độ = l.Lng?.ToString("0.0000") ?? "—",
            }).ToList();
            lblStatus.Text = $"Tổng {locations.Count} địa điểm — dùng để chọn nhanh khi tạo đơn vận chuyển";
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Lỗi: " + ex.Message;
            lblStatus.ForeColor = Theme.Danger;
        }
    }
}
