using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_LocationManagement;

public class LocationsControl : BaseUserControl
{
    private readonly ApiServices _api;
    private readonly DataGridView _grid = new() { Top = 76, Left = 0, Height = 560 };
    private readonly Label _lblStatus = new() { Font = Theme.FontSmall, ForeColor = Theme.TextSecondary, Top = 44, Left = 0, AutoSize = true };

    public LocationsControl(ApiServices api)
    {
        _api = api;
        BackColor = Theme.ContentBg;

        var title = new Label { Text = "Địa điểm", Font = Theme.FontTitle, ForeColor = Theme.TextPrimary, Top = 0, Left = 0, AutoSize = true };
        var btnRefresh = Theme.MakeSecondaryButton("🔄 Làm mới", 110);
        var btnCreate = Theme.MakePrimaryButton("➕ Thêm địa điểm", 170);
        btnRefresh.Top = 0; btnRefresh.Left = 760;
        btnCreate.Top = 0; btnCreate.Left = 880;

        btnRefresh.Click += async (_, _) => await LoadAsync();
        btnCreate.Click += async (_, _) =>
        {
            using var f = new CreateLocationForm(_api);
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
            var locations = await _api.Locations.GetLocationsAsync();
            _grid.DataSource = locations.Select(l => new
            {
                Tên_địa_điểm = l.Name,
                Địa_chỉ = l.Address,
                Vĩ_độ = l.Lat?.ToString("0.0000") ?? "—",
                Kinh_độ = l.Lng?.ToString("0.0000") ?? "—",
            }).ToList();
            _lblStatus.Text = $"Tổng {locations.Count} địa điểm — dùng để chọn nhanh khi tạo đơn vận chuyển";
        }
        catch (Exception ex)
        {
            _lblStatus.Text = "Lỗi: " + ex.Message;
            _lblStatus.ForeColor = Theme.Danger;
        }
    }
}
