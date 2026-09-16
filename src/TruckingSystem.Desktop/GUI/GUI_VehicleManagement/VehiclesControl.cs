using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_VehicleManagement;

public class VehiclesControl : BaseUserControl
{
    private readonly ApiServices _api;
    private readonly DataGridView _grid = new() { Top = 76, Left = 0, Height = 560 };
    private readonly Label _lblStatus = new() { Font = Theme.FontSmall, ForeColor = Theme.TextSecondary, Top = 44, Left = 0, AutoSize = true };

    public VehiclesControl(ApiServices api)
    {
        _api = api;
        BackColor = Theme.ContentBg;

        var title = new Label { Text = "Đội xe", Font = Theme.FontTitle, ForeColor = Theme.TextPrimary, Top = 0, Left = 0, AutoSize = true };
        var btnRefresh = Theme.MakeSecondaryButton("🔄 Làm mới", 110);
        var btnCreate = Theme.MakePrimaryButton("➕ Thêm xe mới", 150);
        btnRefresh.Top = 0; btnRefresh.Left = 780;
        btnCreate.Top = 0; btnCreate.Left = 900;

        btnRefresh.Click += async (_, _) => await LoadAsync();
        btnCreate.Click += async (_, _) =>
        {
            using var f = new CreateVehicleForm(_api);
            if (f.ShowDialog() == DialogResult.OK) await LoadAsync();
        };

        Theme.StyleGrid(_grid);
        Resize += (_, _) => { _grid.Width = Width; _grid.Height = Height - _grid.Top; };

        Controls.Add(_grid);
        Controls.Add(_lblStatus);
        Controls.Add(btnRefresh);
        Controls.Add(btnCreate);
        Controls.Add(title);

        Load += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _lblStatus.Text = "Đang tải...";
        try
        {
            var vehicles = await _api.Vehicles.GetVehiclesAsync();
            _grid.DataSource = vehicles.Select(v => new
            {
                Biển_số = v.PlateNumber,
                Loại_xe = v.VehicleType,
                Sức_chứa = v.CapacityQty,
                Trạng_thái = TranslateStatus(v.Status),
            }).ToList();
            _lblStatus.Text = $"Tổng {vehicles.Count} xe";
        }
        catch (Exception ex)
        {
            _lblStatus.Text = "Lỗi: " + ex.Message;
            _lblStatus.ForeColor = Theme.Danger;
        }
    }

    private static string TranslateStatus(string s) => s switch
    {
        "Available" => "Sẵn sàng", "OnTrip" => "Đang chạy", "UnderMaintenance" => "Đang bảo trì",
        "OutOfService" => "Ngừng hoạt động", _ => s,
    };
}
