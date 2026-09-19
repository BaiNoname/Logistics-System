using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_CustomerManagement;

public partial class CustomersControl : BaseUserControl
{
    private ApiServices? _api;

    public CustomersControl()
    {
        InitializeComponent();
        Theme.StyleGrid(grid);
        Resize += (_, _) => { grid.Width = Width; grid.Height = Height - grid.Top; };
        Load += async (_, _) => await LoadAsync();
    }

    public CustomersControl(ApiServices api) : this()
    {
        _api = api;
    }

    private void BtnRefresh_Click(object? sender, EventArgs e) => _ = LoadAsync();

    private async void BtnCreate_Click(object? sender, EventArgs e)
    {
        if (_api is null) return;
        using var f = new CreateCustomerForm(_api);
        if (f.ShowDialog() == DialogResult.OK) await LoadAsync();
    }

    private async Task LoadAsync()
    {
        if (_api is null) return;
        lblStatus.Text = "Đang tải...";
        try
        {
            var customers = await _api.Customers.GetCustomersAsync();
            grid.DataSource = customers.Select(c => new
            {
                Tên_khách_hàng = c.Name,
                Số_điện_thoại = c.Phone ?? "—",
                Loại = c.CustomerType == "Company" ? "Doanh nghiệp" : "Cá nhân",
            }).ToList();
            lblStatus.Text = $"Tổng {customers.Count} khách hàng";
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Lỗi: " + ex.Message;
            lblStatus.ForeColor = Theme.Danger;
        }
    }
}
