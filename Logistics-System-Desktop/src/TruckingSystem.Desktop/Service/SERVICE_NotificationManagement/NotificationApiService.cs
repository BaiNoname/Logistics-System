using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_NotificationManagement;

namespace TruckingSystem.Desktop.SERVICE_NotificationManagement
{
    public class NotificationApiService
    {
        private readonly ApiConnection _conn;

        public NotificationApiService(ApiConnection conn)
        {
            _conn = conn;
        }

        /// <summary>Quét cảnh báo mới (giấy tờ xe, lịch tài xế) rồi trả về
        /// toàn bộ thông báo gần nhất.</summary>
        /// <param name="refresh">true = bắt server quét lại ngay (mở màn Thông báo); chấm đỏ dùng false.</param>
        public Task<List<NotificationItem>> GetMyNotificationsAsync(bool refresh = false)
            => _conn.GetAsync<List<NotificationItem>>(refresh ? "/api/notifications?refresh=true" : "/api/notifications")!;

        public Task MarkAsReadAsync(int id)
            => _conn.PostAsync<object>($"/api/notifications/{id}/mark-read", new { });

        public Task MarkAllAsReadAsync()
            => _conn.PostAsync<object>("/api/notifications/mark-all-read", new { });
    }
}
