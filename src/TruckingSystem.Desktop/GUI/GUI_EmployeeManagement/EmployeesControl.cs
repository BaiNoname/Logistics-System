using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_EmployeeManagement;

public partial class EmployeesControl : BaseUserControl
{
    private ApiServices? _api;

    public EmployeesControl()
    {
        InitializeComponent();
        Theme.StyleGrid(grid);
        Resize += (_, _) => { grid.Width = Width; grid.Height = Height - grid.Top; };
        Load += async (_, _) => await LoadAsync();
    }

    public EmployeesControl(ApiServices api) : this()
    {
        _api = api;
    }

    private void BtnRefresh_Click(object? sender, EventArgs e) => _ = LoadAsync();

    private async void BtnCreate_Click(object? sender, EventArgs e)
    {
        if (_api is null) return;
        using var f = new CreateEmployeeForm(_api);
        if (f.ShowDialog() == DialogResult.OK) await LoadAsync();
    }

    private async Task LoadAsync()
    {
        if (_api is null) return;
        lblStatus.Text = "Đang tải...";
        try
        {
            var employees = await _api.Employees.GetEmployeesAsync();
            grid.DataSource = employees.Select(e => new
            {
                Mã_NV = e.Code,
                Họ_tên = e.FullName,
                Điện_thoại = e.Phone ?? "—",
                Phòng_ban = e.DepartmentName ?? "—",
                Chức_danh = e.PositionName ?? "—",
                Trạng_thái = e.Status == "Active" ? "Đang làm" : e.Status,
            }).ToList();
            lblStatus.Text = $"Tổng {employees.Count} nhân viên";
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Lỗi: " + ex.Message;
            lblStatus.ForeColor = Theme.Danger;
        }
    }
}
