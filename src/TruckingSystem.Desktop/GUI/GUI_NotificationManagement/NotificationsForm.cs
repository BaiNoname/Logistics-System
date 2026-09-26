using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_NotificationManagement;

namespace TruckingSystem.Desktop.GUI_NotificationManagement
{
    /// <summary>Danh sách thông báo cảnh báo tự động (giấy tờ xe sắp/đã hết
    /// hạn, tài xế chưa đăng ký lịch làm ngày mai...) — quét lại MỖI LẦN mở
    /// form này (gọi API tự động tạo thông báo mới nếu có điều kiện mới phát
    /// sinh, không tạo trùng lặp thông báo cũ chưa đọc).</summary>
    public partial class NotificationsForm : BaseForm
    {
        private ApiServices? _api;
        private List<NotificationItem> _notifications = new();

        public NotificationsForm()
        {
            InitializeComponent();
            Theme.StyleGrid(grid);
        }

        public NotificationsForm(ApiServices api) : this()
        {
            _api = api;
            Load += async (_, _) => await LoadAsync();
        }

        private async Task LoadAsync()
        {
            if (_api is null) return;
            lblStatus.Text = "Đang quét cảnh báo mới...";
            try
            {
                _notifications = await _api.Notifications.GetMyNotificationsAsync();
                grid.DataSource = _notifications.Select(n => new
                {
                    n.Id,
                    Trạng_thái = n.IsRead ? "" : "🔵",
                    Tiêu_đề = n.Title,
                    Nội_dung = n.Message,
                    Thời_gian = n.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                }).ToList();
                grid.DataBindingComplete += (_, _) => { if (grid.Columns["Id"] != null) grid.Columns["Id"]!.Visible = false; };

                var unreadCount = _notifications.Count(n => !n.IsRead);
                lblStatus.Text = unreadCount > 0 ? $"🔵 {unreadCount} thông báo chưa đọc" : "Không có thông báo mới";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }

        /// <summary>Bấm vào 1 dòng -> tự động đánh dấu đã đọc luôn, đỡ phải
        /// bấm thêm nút riêng cho từng thông báo.</summary>
        private async void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (_api is null || e.RowIndex < 0) return;
            var id = (int)grid.Rows[e.RowIndex].Cells["Id"].Value;
            var notification = _notifications.FirstOrDefault(n => n.Id == id);
            if (notification is null || notification.IsRead) return;

            try
            {
                await _api.Notifications.MarkAsReadAsync(id);
                await LoadAsync();
            }
            catch { /* không chặn UI nếu lỗi đánh dấu đã đọc — không quan trọng bằng việc xem nội dung */ }
        }

        private async void BtnMarkAllRead_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;
            try
            {
                await _api.Notifications.MarkAllAsReadAsync();
                await LoadAsync();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }
    }
}
