namespace TruckingSystem.Desktop.GUI_Shared
{
    /// <summary>3 NÚT CỬA SỔ góc trên phải màn đăng nhập: [─ Thu nhỏ] [☐ Phóng to / ❐ Thu về] [✕ Thoát].
    /// Form đăng nhập không có viền Windows (để vẽ hoạt cảnh), nên tự làm 2 nút ─ ☐ cùng kiểu với nút ✕ có sẵn.
    /// Phóng to vừa vùng làm việc (không che thanh Taskbar). Nhấp đúp vào nền cũng phóng to / thu về.</summary>
    public partial class LoginForm
    {
        private Button? _btnMax;

        private void SetupWindowButtons()
        {
            int S(int v) => LogicalToDeviceUnits(v);
            var bar = new FlowLayoutPanel
            {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight, BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Right, Margin = btnClose.Margin, Padding = Padding.Empty,
            };
            Button Make(string text, string tip, EventHandler click)
            {
                var b = new Button
                {
                    Text = text, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, MinimumSize = btnClose.MinimumSize,
                    FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, ForeColor = btnClose.ForeColor, Font = btnClose.Font,
                    Cursor = Cursors.Hand, Margin = new Padding(0, 0, S(2), 0), TabStop = false, UseVisualStyleBackColor = false,
                };
                b.FlatAppearance.BorderSize = 0;
                b.FlatAppearance.MouseOverBackColor = btnClose.FlatAppearance.MouseOverBackColor;
                b.Click += click;
                _winTip.SetToolTip(b, tip);
                return b;
            }

            var min = Make("─", "Thu nhỏ", (_, _) => WindowState = FormWindowState.Minimized);
            _btnMax = Make("☐", "Phóng to", (_, _) => ToggleMaximize());

            // Đưa nút ✕ có sẵn vào cùng hàng (giữ nguyên sự kiện thoát của nó).
            var cell = rightPanel.GetCellPosition(btnClose);
            var span = rightPanel.GetColumnSpan(btnClose);
            rightPanel.SuspendLayout();
            rightPanel.Controls.Remove(btnClose);
            btnClose.Margin = Padding.Empty;
            btnClose.TabStop = false;
            _winTip.SetToolTip(btnClose, "Thoát ứng dụng");
            bar.Controls.AddRange(new Control[] { min, _btnMax, btnClose });
            rightPanel.Controls.Add(bar, cell.Column, cell.Row);
            rightPanel.SetColumnSpan(bar, span);
            rightPanel.ResumeLayout(true);

            // Nền đổi màu theo giờ -> vẽ lại cả 2 nút mới cùng lúc với nút ✕.
            _animTimer.Tick += (_, _) => { if (_rightTick % 10 == 0) { min.Invalidate(); _btnMax.Invalidate(); } };
            rightPanel.DoubleClick += (_, _) => ToggleMaximize();
            Resize += (_, _) => { UpdateMaxButton(); FitLoginLayout(); };
            FitLoginLayout();
            FormClosed += (_, _) => _winTip.Dispose();
        }

        private readonly ToolTip _winTip = new();

        private void ToggleMaximize()
        {
            if (WindowState == FormWindowState.Maximized)
                WindowState = FormWindowState.Normal;
            else
            {
                // Form không viền phóng to sẽ che cả Taskbar -> giới hạn vào vùng làm việc của màn hình đang đứng.
                var work = Screen.FromControl(this).WorkingArea;
                var screen = Screen.FromControl(this).Bounds;
                MaximizedBounds = new Rectangle(work.X - screen.X, work.Y - screen.Y, work.Width, work.Height);
                WindowState = FormWindowState.Maximized;
            }
        }

        /// <summary>GIỮ TỈ LỆ KHI PHÓNG TO: trước đây cảnh bên trái cố định 460px, thẻ đăng nhập giãn theo 70% nửa phải ->
        /// phóng to toàn màn hình thì cảnh bé tí, ô nhập kéo dài cả nghìn pixel, nhìn vỡ.
        ///  • Cảnh bên trái chiếm ~46% bề ngang (như tỉ lệ 460/1000 lúc thường), tối thiểu 460.
        ///  • Thẻ đăng nhập rộng tối đa 420, luôn nằm giữa nửa phải.
        /// Ở cỡ thường (1000×640) kết quả y như cũ: cảnh 460, thẻ 378.</summary>
        private void FitLoginLayout()
        {
            int S(int v) => LogicalToDeviceUnits(v);
            var cw = ClientSize.Width;
            if (cw <= 0 || WindowState == FormWindowState.Minimized) return;
            var bw = Math.Max(S(460), (int)(cw * 0.46f));
            if (cw - bw < S(480)) bw = Math.Max(S(300), cw - S(480)); // chừa đủ chỗ cho thẻ đăng nhập
            var rw = cw - bw;
            var cardW = Math.Max(S(300), Math.Min(S(420), (int)(rw * 0.70f)));

            SuspendLayout();
            bannerPanel.Width = bw;
            rightPanel.SuspendLayout();
            rightPanel.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 50F);
            rightPanel.ColumnStyles[1] = new ColumnStyle(SizeType.Absolute, cardW);
            rightPanel.ColumnStyles[2] = new ColumnStyle(SizeType.Percent, 50F);
            rightPanel.ResumeLayout(true);
            ResumeLayout(true);
            bannerPanel.Invalidate();
            rightPanel.Invalidate(true);
        }

        private void UpdateMaxButton()
        {
            if (_btnMax is null) return;
            var max = WindowState == FormWindowState.Maximized;
            _btnMax.Text = max ? "❐" : "☐";
            _winTip.SetToolTip(_btnMax, max ? "Thu về" : "Phóng to");
        }
    }
}
