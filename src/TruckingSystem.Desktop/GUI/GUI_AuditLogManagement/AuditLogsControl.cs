using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_AuditLogManagement;

namespace TruckingSystem.Desktop.GUI_AuditLogManagement
{
    /// <summary>Xem lại lịch sử nhật ký hệ thống (AuditLog) — lọc theo loại
    /// hành động (VD "AllocateVehicles") và khoảng thời gian. Chỉ hiện 200
    /// dòng gần nhất, tránh tải cả bảng khổng lồ về 1 lúc.</summary>
    public partial class AuditLogsControl : BaseUserControl
    {
        private ApiServices? _api;

        public AuditLogsControl()
        {
            InitializeComponent();
            Theme.StyleGrid(grid);
            chkFilterByDate.CheckedChanged += (_, _) => { dtFrom.Enabled = dtTo.Enabled = chkFilterByDate.Checked; };
            dtFrom.Value = DateTime.Today.AddDays(-7);
            dtTo.Value = DateTime.Today;
            dtFrom.Enabled = dtTo.Enabled = false;
            Resize += (_, _) => { grid.Width = Width; grid.Height = Height - grid.Top; };
            Load += async (_, _) => { await LoadActionsAsync(); await LoadAsync(); };
        }

        public AuditLogsControl(ApiServices api) : this()
        {
            _api = api;
        }

        /// <summary>Đổ dropdown lọc theo hành động — lấy đúng các giá trị ĐÃ
        /// TỪNG xuất hiện trong log thật, tránh gõ tay sai chính tả.</summary>
        private async Task LoadActionsAsync()
        {
            if (_api is null) return;
            try
            {
                var actions = await _api.AuditLogs.GetDistinctActionsAsync();
                cboAction.Items.Clear();
                cboAction.Items.Add("(Tất cả)");
                cboAction.Items.AddRange(actions.Cast<object>().ToArray());
                cboAction.SelectedIndex = 0;
            }
            catch { /* không chặn màn hình chính nếu lỗi tải danh sách lọc */ }
        }

        private async void BtnFilter_Click(object? sender, EventArgs e) => await LoadAsync();

        private async Task LoadAsync()
        {
            if (_api is null) return;
            lblStatus.Text = "Đang tải...";
            try
            {
                string? action = cboAction.SelectedItem is string s && s != "(Tất cả)" ? s : null;
                DateTime? fromDate = chkFilterByDate.Checked ? dtFrom.Value.Date : null;
                DateTime? toDate = chkFilterByDate.Checked ? dtTo.Value.Date.AddDays(1).AddSeconds(-1) : null; // hết ngày ToDate, không cắt ngang giữa ngày

                var logs = await _api.AuditLogs.GetAsync(action, fromDate, toDate);
                grid.DataSource = logs.Select(l => new
                {
                    Thời_gian = l.Timestamp.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss"),
                    Hành_động = l.Action,
                    Đối_tượng = l.EntityName + (l.EntityId.HasValue ? $" #{l.EntityId}" : ""),
                    Chi_tiết = l.Detail ?? "—",
                }).ToList();

                lblStatus.Text = $"Hiển thị {logs.Count} dòng gần nhất (tối đa 200)";
                lblStatus.ForeColor = Theme.TextSecondary;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }
    }
}
