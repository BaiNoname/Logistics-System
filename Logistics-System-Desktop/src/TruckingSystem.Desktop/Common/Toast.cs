namespace TruckingSystem.Desktop.Common
{
    public enum ToastKind { Success, Info, Warning }

    /// <summary>THÔNG BÁO NHẸ ở góc dưới bên phải cửa sổ, tự tắt sau vài giây (bấm vào để tắt ngay) — thay cho hộp thoại
    /// "Thành công" phải bấm OK. Chỉ dùng cho thông báo XONG VIỆC; lỗi và câu hỏi xác nhận vẫn dùng hộp thoại.
    /// Nhiều thông báo cùng lúc thì xếp chồng lên nhau. Dùng: Toast.Show(this, "✅ Đã nhập kho chuyến #12");</summary>
    public static class Toast
    {
        private static readonly Font TextFont = new("Segoe UI", 10F);

        public static void Show(Control? anchor, string message, ToastKind kind = ToastKind.Success)
        {
            if (anchor is null || anchor.IsDisposed || string.IsNullOrWhiteSpace(message)) return;
            var form = anchor as Form ?? anchor.FindForm();
            if (form is null || form.IsDisposed) return;
            form = form.Owner is { IsDisposed: false } owner && (form.Disposing || !form.Visible) ? owner : form;

            int S(int v) => form.LogicalToDeviceUnits(v);
            var accent = kind switch { ToastKind.Warning => Theme.Warning, ToastKind.Info => Theme.Primary, _ => Theme.Success };
            var maxText = S(380);
            var textSize = TextRenderer.MeasureText(message.Trim(), TextFont, new Size(maxText, 0), TextFormatFlags.WordBreak);
            var panel = new ToastPanel
            {
                Size = new Size(Math.Min(maxText, textSize.Width) + S(40), textSize.Height + S(24)),
                Cursor = Cursors.Hand,
                Tag = "toast",
            };
            panel.Paint += (_, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = Round(new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), S(10)))
                using (var bg = new SolidBrush(Color.FromArgb(15, 23, 42)))
                    g.FillPath(bg, path);
                using (var bar = new SolidBrush(accent)) g.FillRectangle(bar, S(8), S(10), S(4), panel.Height - S(20));
                TextRenderer.DrawText(g, message.Trim(), TextFont, new Rectangle(S(22), S(12), panel.Width - S(32), panel.Height - S(20)),
                    Color.White, TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
            };

            form.Controls.Add(panel);
            panel.BringToFront();
            Relayout(form);

            void Close()
            {
                if (panel.IsDisposed) return;
                var f = panel.FindForm();
                panel.Dispose();
                if (f is not null && !f.IsDisposed) Relayout(f);
            }
            panel.Click += (_, _) => Close();
            var timer = new System.Windows.Forms.Timer { Interval = Math.Clamp(3000 + message.Length * 40, 3000, 10000) };
            timer.Tick += (_, _) => { timer.Dispose(); Close(); };
            timer.Start();
            panel.Disposed += (_, _) => timer.Dispose();
            form.Resize += (_, _) => { if (!panel.IsDisposed) Relayout(form); };
        }

        /// <summary>Xếp các thông báo đang hiện từ góc dưới phải đi lên.</summary>
        private static void Relayout(Form form)
        {
            var gap = form.LogicalToDeviceUnits(10);
            var y = form.ClientSize.Height - form.LogicalToDeviceUnits(18);
            foreach (var p in form.Controls.OfType<ToastPanel>().Reverse())
            {
                y -= p.Height;
                p.Location = new Point(form.ClientSize.Width - p.Width - form.LogicalToDeviceUnits(18), y);
                p.BringToFront();
                y -= gap;
            }
        }

        private static System.Drawing.Drawing2D.GraphicsPath Round(Rectangle r, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            var d = radius * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private sealed class ToastPanel : Panel
        {
            public ToastPanel()
            {
                DoubleBuffered = true;
                BackColor = Color.Transparent;
                SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            }
        }
    }
}
