using System.Drawing.Drawing2D;

namespace TruckingSystem.Desktop.Common
{
    /// <summary>Chấm ĐỎ có số trên nút 🔔 — số thông báo CHƯA XỬ LÝ của người đang đăng nhập (99+ nếu quá
    /// nhiều; 0 thì ẩn chấm). Tự tải lại mỗi 2 phút và ngay khi đóng cửa sổ Thông báo (gọi RefreshAsync).
    /// Vẽ thẳng lên nút (sự kiện Paint) — không thêm control nào vào Designer của MainForm.</summary>
    public sealed class NotificationBadge : IDisposable
    {
        private readonly Button _button;
        private readonly ApiServices _api;
        private readonly System.Windows.Forms.Timer _timer = new() { Interval = 2 * 60 * 1000 };
        private readonly ToolTip _tip = new();
        private int _count;
        private bool _loading;

        public NotificationBadge(Button button, ApiServices api)
        {
            _button = button;
            _api = api;
            _button.Paint += Button_Paint;
            _timer.Tick += async (_, _) => await RefreshAsync();
            _timer.Start();
            _button.Disposed += (_, _) => Dispose();
            _ = RefreshAsync();
        }

        /// <summary>Số thông báo chưa xử lý đang hiển thị trên chấm đỏ.</summary>
        public int Count => _count;

        public async Task RefreshAsync()
        {
            if (_loading || _button.IsDisposed) return;
            _loading = true;
            try
            {
                var list = await _api.Notifications.GetMyNotificationsAsync();
                if (_button.IsDisposed) return;
                SetCount(list?.Count(n => !n.IsDone) ?? 0);
            }
            catch
            {
                // Mất kết nối tạm thời: giữ số cũ, lần sau tải lại — không làm phiền người dùng.
            }
            finally { _loading = false; }
        }

        public void SetCount(int count)
        {
            count = Math.Max(0, count);
            if (count == _count) return;
            _count = count;
            var tip = count == 0 ? "Thông báo" : $"Thông báo — {count} việc chưa xử lý";
            _tip.SetToolTip(_button, tip);
            _button.Invalidate();
        }

        private void Button_Paint(object? sender, PaintEventArgs e)
        {
            if (_count <= 0) return;
            var text = _count > 99 ? "99+" : _count.ToString();
            using var font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            var textSize = TextRenderer.MeasureText(e.Graphics, text, font, Size.Empty, TextFormatFlags.NoPadding);
            var height = Math.Max(_button.LogicalToDeviceUnits(16), textSize.Height + _button.LogicalToDeviceUnits(2));
            var width = Math.Max(height, textSize.Width + _button.LogicalToDeviceUnits(8));
            var rect = new Rectangle(_button.ClientSize.Width - width - 1, 1, width, height);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = new GraphicsPath())
            {
                var r = height;
                path.AddArc(rect.X, rect.Y, r, r, 90, 180);
                path.AddArc(rect.Right - r, rect.Y, r, r, 270, 180);
                path.CloseFigure();
                using var brush = new SolidBrush(Color.FromArgb(220, 38, 38));
                using var border = new Pen(Color.White, _button.LogicalToDeviceUnits(2));
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(border, path);
            }
            TextRenderer.DrawText(e.Graphics, text, font, rect, Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }

        public void Dispose()
        {
            _timer.Dispose();
            _tip.Dispose();
            _button.Paint -= Button_Paint;
        }
    }
}
