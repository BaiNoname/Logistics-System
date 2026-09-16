using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_CustomerManagement;

public class CustomersControl : BaseUserControl
{
    private readonly ApiServices _api;
    private readonly DataGridView _grid = new() { Top = 76, Left = 0, Height = 560 };
    private readonly Label _lblStatus = new() { Font = Theme.FontSmall, ForeColor = Theme.TextSecondary, Top = 44, Left = 0, AutoSize = true };

    public CustomersControl(ApiServices api)
    {
        _api = api;
        BackColor = Theme.ContentBg;

        var title = new Label { Text = "Khách hàng", Font = Theme.FontTitle, ForeColor = Theme.TextPrimary, Top = 0, Left = 0, AutoSize = true };
        var btnRefresh = Theme.MakeSecondaryButton("🔄 Làm mới", 110);
        var btnCreate = Theme.MakePrimaryButton("➕ Thêm khách hàng", 170);
        btnRefresh.Top = 0; btnRefresh.Left = 760;
        btnCreate.Top = 0; btnCreate.Left = 880;

        btnRefresh.Click += async (_, _) => await LoadAsync();
        btnCreate.Click += async (_, _) =>
        {
            using var f = new CreateCustomerForm(_api);
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
            var customers = await _api.Customers.GetCustomersAsync();
            _grid.DataSource = customers.Select(c => new
            {
                Tên_khách_hàng = c.Name,
                Số_điện_thoại = c.Phone ?? "—",
                Loại = c.CustomerType == "Company" ? "Doanh nghiệp" : "Cá nhân",
            }).ToList();
            _lblStatus.Text = $"Tổng {customers.Count} khách hàng";
        }
        catch (Exception ex)
        {
            _lblStatus.Text = "Lỗi: " + ex.Message;
            _lblStatus.ForeColor = Theme.Danger;
        }
    }
}
