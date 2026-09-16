using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_EmployeeManagement;

public class EmployeesControl : BaseUserControl
{
    private readonly ApiServices _api;
    private readonly DataGridView _grid = new() { Top = 76, Left = 0, Height = 560 };
    private readonly Label _lblStatus = new() { Font = Theme.FontSmall, ForeColor = Theme.TextSecondary, Top = 44, Left = 0, AutoSize = true };

    public EmployeesControl(ApiServices api)
    {
        _api = api;
        BackColor = Theme.ContentBg;

        var title = new Label { Text = "Nhân viên", Font = Theme.FontTitle, ForeColor = Theme.TextPrimary, Top = 0, Left = 0, AutoSize = true };
        var btnRefresh = Theme.MakeSecondaryButton("🔄 Làm mới", 110);
        var btnCreate = Theme.MakePrimaryButton("➕ Thêm nhân viên", 170);
        btnRefresh.Top = 0; btnRefresh.Left = 760;
        btnCreate.Top = 0; btnCreate.Left = 880;

        btnRefresh.Click += async (_, _) => await LoadAsync();
        btnCreate.Click += async (_, _) =>
        {
            using var f = new CreateEmployeeForm(_api);
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
            var employees = await _api.Employees.GetEmployeesAsync();
            _grid.DataSource = employees.Select(e => new
            {
                Mã_NV = e.Code,
                Họ_tên = e.FullName,
                Điện_thoại = e.Phone ?? "—",
                Phòng_ban = e.DepartmentName ?? "—",
                Chức_danh = e.PositionName ?? "—",
                Trạng_thái = e.Status == "Active" ? "Đang làm" : e.Status,
            }).ToList();
            _lblStatus.Text = $"Tổng {employees.Count} nhân viên";
        }
        catch (Exception ex)
        {
            _lblStatus.Text = "Lỗi: " + ex.Message;
            _lblStatus.ForeColor = Theme.Danger;
        }
    }
}
