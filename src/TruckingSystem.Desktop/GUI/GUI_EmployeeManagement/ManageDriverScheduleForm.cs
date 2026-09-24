using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_EmployeeManagement;

namespace TruckingSystem.Desktop.GUI_EmployeeManagement
{
    /// <summary>Quản lý lịch làm việc (ca làm) của 1 tài xế — Greedy (Chia xe)
    /// dùng dữ liệu này để CHỈ chọn tài xế thật sự đang trong ca hôm nay, thay
    /// vì coi mọi tài xế Active đều sẵn sàng bất kể có đăng ký ca hay không.</summary>
    public partial class ManageDriverScheduleForm : BaseForm
    {
        private ApiServices? _api;
        private int _employeeId;
        private List<DriverScheduleItem> _schedules = new();

        public ManageDriverScheduleForm()
        {
            InitializeComponent();
            Theme.StyleGrid(grid);
            cboShiftType.Items.AddRange(new object[] { "Morning", "Afternoon", "Night", "FullDay" });
            cboShiftType.SelectedIndex = 0;
            dtShiftDate.Value = DateTime.Today;
        }

        public ManageDriverScheduleForm(ApiServices api, int employeeId, string employeeName) : this()
        {
            _api = api;
            _employeeId = employeeId;
            Text = $"Lịch làm việc — {employeeName}";
            Load += async (_, _) => await LoadAsync();
        }

        private async Task LoadAsync()
        {
            if (_api is null) return;
            lblStatus.Text = "Đang tải...";
            try
            {
                _schedules = await _api.Employees.GetSchedulesAsync(_employeeId);
                grid.DataSource = _schedules.Select(s => new
                {
                    s.Id,
                    Ngày = s.ShiftDate.ToString("dd/MM/yyyy"),
                    Ca_làm = TranslateShift(s.ShiftType),
                }).ToList();
                grid.DataBindingComplete += (_, _) => { if (grid.Columns["Id"] != null) grid.Columns["Id"]!.Visible = false; };
                lblStatus.Text = $"{_schedules.Count} ca đã đăng ký";
                lblStatus.ForeColor = Theme.TextSecondary;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }

        private async void BtnAddShift_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;
            try
            {
                var result = await _api.Employees.CreateScheduleAsync(_employeeId, dtShiftDate.Value, (string)cboShiftType.SelectedItem!);
                if (!result.Success)
                {
                    lblStatus.Text = "⚠️ " + result.Message;
                    lblStatus.ForeColor = Theme.Warning;
                    return;
                }
                await LoadAsync();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }

        private async void BtnDeleteShift_Click(object? sender, EventArgs e)
        {
            if (_api is null || grid.CurrentRow is null)
            {
                MessageBox.Show(FindForm(), "Vui lòng chọn 1 ca làm việc trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var id = (int)grid.CurrentRow.Cells["Id"].Value;

            try
            {
                var result = await _api.Employees.DeleteScheduleAsync(id);
                if (!result.Success)
                {
                    MessageBox.Show(FindForm(), result.Message ?? "Không thể xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(FindForm(), "Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string TranslateShift(string s) => s switch
        {
            "Morning" => "🌅 Sáng",
            "Afternoon" => "☀️ Chiều",
            "Night" => "🌙 Tối",
            _ => "📅 Cả ngày",
        };
    }
}
