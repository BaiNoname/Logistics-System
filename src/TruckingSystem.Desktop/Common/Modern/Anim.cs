using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace TruckingSystem.Desktop.Common
{
    /// <summary>Hiệu ứng chuyển động NHẸ cho giao diện (WinForms không có sẵn): đếm số, hiện dần
    /// cửa sổ, đổi màu khi rê chuột, thanh tiến độ "dâng lên". Tất cả chạy bằng Timer của
    /// WinForms trên luồng giao diện (~60 khung hình/giây), tự dừng khi control bị đóng.
    /// Thời lượng ngắn (150–700 ms) + đường cong ease-out: nhìn mượt, không làm chậm thao tác.</summary>
    public static class Anim
    {
        /// <summary>Nhanh lúc đầu, chậm dần về cuối — cảm giác "đặt xuống" tự nhiên.</summary>
        public static double EaseOutCubic(double t)
        {
            t = Math.Clamp(t, 0, 1);
            return 1 - Math.Pow(1 - t, 3);
        }

        /// <summary>Chạy 1 hiệu ứng trong <paramref name="durationMs"/>: gọi <paramref name="step"/>(p)
        /// với p tăng dần 0 → 1 (đã qua ease-out). Trả về Timer — gọi Dispose() để dừng giữa chừng.</summary>
        public static System.Windows.Forms.Timer Tween(Control owner, int durationMs, Action<double> step, Action? done = null)
        {
            var timer = new System.Windows.Forms.Timer { Interval = 15 };
            var clock = Stopwatch.StartNew();
            timer.Tick += (_, _) =>
            {
                if (owner.IsDisposed || owner.Disposing)
                {
                    timer.Dispose();
                    return;
                }
                var t = durationMs <= 0 ? 1.0 : clock.ElapsedMilliseconds / (double)durationMs;
                step(EaseOutCubic(t));
                if (t >= 1)
                {
                    timer.Dispose();
                    done?.Invoke();
                }
            };
            timer.Start();
            return timer;
        }

        /// <summary>Pha trộn 2 màu (t = 0 → a, t = 1 → b).</summary>
        public static Color Mix(Color a, Color b, double t)
        {
            t = Math.Clamp(t, 0, 1);
            int Lerp(int x, int y) => (int)Math.Round(x + (y - x) * t);
            return Color.FromArgb(Lerp(a.A, b.A), Lerp(a.R, b.R), Lerp(a.G, b.G), Lerp(a.B, b.B));
        }

        /// <summary>Cửa sổ hiện DẦN lên (~0,16 giây) khi mở — gọi trong constructor của form.</summary>
        public static void FadeIn(Form form, int durationMs = 160)
        {
            form.Opacity = 0;
            form.Shown += (_, _) => Tween(form, durationMs, p => form.Opacity = p, () => form.Opacity = 1);
        }

        /// <summary>Hình chữ nhật bo góc (dùng cho thẻ, nhãn trạng thái, thanh tiến độ).</summary>
        public static GraphicsPath RoundRect(RectangleF r, float radius)
        {
            var path = new GraphicsPath();
            var d = Math.Max(0.1f, Math.Min(radius * 2, Math.Min(r.Width, r.Height)));
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
