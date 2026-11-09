using System.Drawing.Drawing2D;

namespace TruckingSystem.Desktop.GUI_Shared
{
    /// <summary>KHUNG CHÍNH KIỂU MỚI (mẫu 4_buoi đã duyệt) — tự vẽ bằng GDI+:
    ///  • Menu trái tô dải màu bầu trời của buổi đang chọn (trên -> dưới), buổi Tối có thêm sao.
    ///  • Mục đang chọn là viên bo tròn (dải màu + bóng nhẹ); rê chuột: viên mờ. Nhóm đang chứa màn hiện tại (đang đóng): viên mờ.
    ///  • Nút Đăng xuất / Thu gọn menu cùng nền; ô tìm nhanh và hàng nút chọn buổi bo tròn hẳn.
    /// Chữ trên nút giữ nguyên (số đỏ / tìm nhanh / thông báo tra nút theo chữ) — chỉ đổi cách vẽ.</summary>
    public partial class MainForm
    {
        private readonly HashSet<Button> _hot = new();
        private readonly Panel _searchHost = new() { Anchor = AnchorStyles.Left, Margin = new Padding(0, 0, 14, 0) };

        private static void SetDoubleBuffered(Control c)
            => typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(c, true);

        /// <summary>Gọi 1 lần trong SetupFrame: nền dải màu cho menu, nút đáy menu, ô tìm bo tròn, hàng nút chọn buổi.</summary>
        private void SetupLook()
        {
            SetDoubleBuffered(navPanel);
            SetDoubleBuffered(sidebarBottom);
            navPanel.Paint += (_, e) => FillSide(e.Graphics, navPanel);
            sidebarBottom.Paint += (_, e) => FillSide(e.Graphics, sidebarBottom);
            navPanel.Scroll += (_, _) => navPanel.Invalidate();
            navPanel.MouseWheel += (_, _) => navPanel.Invalidate();
            sidebar.Resize += (_, _) => { navPanel.Invalidate(true); sidebarBottom.Invalidate(true); };

            SetDoubleBuffered(btnLogout);
            TrackHot(btnLogout);
            btnLogout.Paint += (_, e) => PaintLogout(e.Graphics);

            // Ô tìm nhanh: khung bo tròn chứa ô chữ không viền.
            SetDoubleBuffered(_searchHost);
            _searchBox.BorderStyle = BorderStyle.None;
            _searchBox.Margin = Padding.Empty;
            _searchHost.Height = LogicalToDeviceUnits(34);
            _searchHost.Width = LogicalToDeviceUnits(340);
            _searchHost.Controls.Add(_searchBox);
            _searchHost.Resize += (_, _) => PlaceSearchBox();
            _searchHost.Paint += (_, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(_mode.Top);
                var r = _searchHost.ClientRectangle;
                using var path = DayScene.Round(0.5f, 0.5f, r.Width - 1.5f, r.Height - 1.5f, r.Height / 2f);
                using var fill = new SolidBrush(SearchFill);
                using var pen = new Pen(_searchBox.Focused ? _mode.Strip[0] : SearchLine, _searchBox.Focused ? 1.6f : 1f);
                g.FillPath(fill, path);
                g.DrawPath(pen, path);
            };
            _searchBox.GotFocus += (_, _) => _searchHost.Invalidate();
            _searchBox.LostFocus += (_, _) => _searchHost.Invalidate();
            _searchBox.FontChanged += (_, _) => PlaceSearchBox();

            // Chuông thông báo màu vàng, nền tròn khi rê chuột; avatar tròn có chữ cái đầu tên (thay ô vuông 👤).
            SetDoubleBuffered(btnNotifications);
            TrackHot(btnNotifications);
            btnNotifications.Paint += (_, e) => PaintBell(e.Graphics); // gắn trước số đỏ (NotificationBadge) -> số vẽ chồng lên
            SetDoubleBuffered(lblAvatar);
            lblAvatar.Paint += (_, e) => PaintAvatar(e.Graphics);

            // Hàng nút chọn buổi: nền viên thuốc, nút đang chọn là viên trắng (Tối: viên tím).
            SetDoubleBuffered(_modeBar);
            _modeBar.Paint += (_, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(_mode.Top);
                var r = _modeBar.ClientRectangle;
                DayScene.FillRound(g, _mode.Seg, 0, 0, r.Width - 1, r.Height - 1, r.Height / 2f);
            };
        }

        private static readonly Font BellFont = new("Segoe UI Emoji", 15F);
        private static readonly Font AvatarFont = new("Segoe UI Semibold", 12F);

        private void PaintBell(Graphics g)
        {
            var b = btnNotifications;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(_mode.Top);
            if (_hot.Contains(b))
                using (var hb = new SolidBrush(_mode.Night ? Color.FromArgb(40, 255, 255, 255) : DayScene.Hex("#FEF3C7")))
                    g.FillEllipse(hb, 2, 2, b.Width - 5, b.Height - 5);
            TextRenderer.DrawText(g, "🔔", BellFont, b.ClientRectangle, _mode.Night ? DayScene.Hex("#FCD34D") : DayScene.Hex("#F59E0B"),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
        }

        private void PaintAvatar(Graphics g)
        {
            var a = lblAvatar;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(_mode.Top);
            var d = Math.Min(a.Width, a.Height) - 2;
            var r = new Rectangle((a.Width - d) / 2, (a.Height - d) / 2, d, d);
            using (var br = new LinearGradientBrush(r, _mode.Strip[0], _mode.Strip[Math.Min(2, _mode.Strip.Length - 1)], 45f))
                g.FillEllipse(br, r);
            var name = lblUser.Text.Trim();
            var letter = name.Length > 0 ? char.ToUpperInvariant(name[0]).ToString() : "?";
            TextRenderer.DrawText(g, letter, AvatarFont, r, Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        private const int WM_SETREDRAW = 0x000B;

        /// <summary>Mở màn mới MƯỢT: tạm ngưng vẽ vùng nội dung trong lúc dựng màn (không còn cảnh nút / bảng hiện lần lượt),
        /// dựng xong mới vẽ 1 lần.</summary>
        private void BeginContentSwap()
        {
            if (content.IsHandleCreated) SendMessage(content.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
        }

        private void EndContentSwap()
        {
            if (!content.IsHandleCreated) return;
            BeginInvoke(new Action(() =>
            {
                if (IsDisposed || !content.IsHandleCreated) return;
                SendMessage(content.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
                content.Refresh(); // vẽ lại cả màn + control con 1 lượt
            }));
        }

        private Color SearchFill => _mode.Night ? DayScene.Hex("#1A2148") : DayScene.Hex("#F8FAFC");
        private Color SearchLine => _mode.Night ? DayScene.Hex("#2E3670") : DayScene.Hex("#E2E8F0");

        private void PlaceSearchBox()
        {
            int S(int v) => LogicalToDeviceUnits(v);
            _searchBox.Left = S(14);
            _searchBox.Width = Math.Max(S(40), _searchHost.Width - S(26));
            _searchBox.Top = Math.Max(0, (_searchHost.Height - _searchBox.Height) / 2);
        }

        private void TrackHot(Button b)
        {
            b.MouseEnter += (_, _) => { _hot.Add(b); b.Invalidate(); };
            b.MouseLeave += (_, _) => { _hot.Remove(b); b.Invalidate(); };
        }

        /// <summary>Tô nền dải màu trời của menu cho phần control <paramref name="c"/> đang chiếm (tính theo vị trí trong menu trái
        /// -> các nút nối liền nhau thành 1 dải). Buổi Tối: rắc sao nhỏ.</summary>
        private void FillSide(Graphics g, Control c)
        {
            if (c.Width <= 0 || c.Height <= 0 || !c.IsHandleCreated || !sidebar.IsHandleCreated) return;
            var top = sidebar.PointToClient(c.PointToScreen(Point.Empty)).Y;
            var h = Math.Max(2, sidebar.Height);
            var rect = new Rectangle(0, -top - 1, Math.Max(1, c.Width), h + 2);
            using (var br = new LinearGradientBrush(rect, _mode.Side, _mode.Side, 90f)
                   { InterpolationColors = new ColorBlend { Colors = _mode.SideStops, Positions = new[] { 0f, 0.55f, 1f } } })
                g.FillRectangle(br, c.ClientRectangle);
            if (!_mode.Night) return;
            var left = sidebar.PointToClient(c.PointToScreen(Point.Empty)).X;
            var sw = Math.Max(1, sidebar.Width);
            var step = LogicalToDeviceUnits(37);
            using var star = new SolidBrush(Color.FromArgb(120, 255, 255, 255));
            for (var i = 0; i < 60; i++)
            {
                float x = (i * 97) % sw - left, y = i * step * 0.53f % h - top;
                if (x < -2 || y < -2 || x > c.Width || y > c.Height) continue;
                var s = i % 3 == 0 ? 2f : 1.2f;
                g.FillEllipse(star, x, y, s, s);
            }
        }

        /// <summary>Nút đang chọn (menu thu gọn: biểu tượng nhóm đang chứa màn hiện tại).</summary>
        private bool IsNavActive(Button b)
            => b == _activeNavButton || (_navCollapsed && _navHeaders.Contains(b) && HoldsCurrent(b));

        private bool HoldsCurrent(Button header)
            => _activeNavButton is not null && _groupOf.TryGetValue(_activeNavButton, out var g) && g == (string)header.Tag!;

        /// <summary>Vẽ 1 mục menu (gắn ĐẦU TIÊN vào Paint — mũi tên nhóm, biểu tượng khi thu gọn, số đỏ vẽ chồng lên sau).</summary>
        private void PaintNavItem(Graphics g, Button b)
        {
            int S(int v) => LogicalToDeviceUnits(v);
            if (b.Width <= S(12) || b.Height <= S(12)) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            FillSide(g, b);
            var active = IsNavActive(b);
            var r = new RectangleF(S(1), S(3), b.Width - S(2) - 1, b.Height - S(6) - 1);
            var radius = S(11);
            if (active)
            {
                using (var shadow = DayScene.Round(r.X, r.Y + S(2), r.Width, r.Height, radius))
                using (var sb = new SolidBrush(Color.FromArgb(_mode.Night ? 90 : 45, _mode.Night ? _mode.ActiveEnd : Color.FromArgb(15, 23, 42))))
                    g.FillPath(sb, shadow);
                using var path = DayScene.Round(r.X, r.Y, r.Width, r.Height, radius);
                using var fill = new LinearGradientBrush(new RectangleF(r.X - 1, r.Y, r.Width + 2, r.Height),
                    _mode.Active, _mode.ActiveEnd.IsEmpty ? _mode.Active : _mode.ActiveEnd, 0f);
                g.FillPath(fill, path);
            }
            else if (_hot.Contains(b) || (!_navCollapsed && _navHeaders.Contains(b) && HoldsCurrent(b) && (string)b.Tag! != _openGroup))
            {
                using var path = DayScene.Round(r.X, r.Y, r.Width, r.Height, radius);
                using var fill = new SolidBrush(Color.FromArgb(_mode.Night ? 40 : 70, 255, 255, 255));
                g.FillPath(fill, path);
            }
            if (_navCollapsed) return; // thu gọn: biểu tượng do PaintCollapsedIcon vẽ
            var tr = new Rectangle(b.Padding.Left + S(4), 0, Math.Max(0, b.Width - b.Padding.Horizontal - S(4)), b.Height);
            TextRenderer.DrawText(g, b.Text, b.Font, tr, active ? _mode.ActiveText : _mode.SideText,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
        }

        /// <summary>Nút "« Thu gọn menu": chữ trên nền dải màu.</summary>
        private void PaintNavToggle(Graphics g, Button b)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            FillSide(g, b);
            if (_hot.Contains(b))
                DayScene.FillRound(g, Color.FromArgb(_mode.Night ? 40 : 70, 255, 255, 255), LogicalToDeviceUnits(2), 1, b.Width - LogicalToDeviceUnits(4), b.Height - 2, LogicalToDeviceUnits(9));
            TextRenderer.DrawText(g, b.Text, b.Font, b.ClientRectangle, _mode.SideText,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
        }

        /// <summary>Đăng xuất: viên bo tròn viền mảnh (đỏ nhạt ở buổi Tối / Bình minh, trắng mờ ở Trưa / Chiều).</summary>
        private void PaintLogout(Graphics g)
        {
            var b = btnLogout;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            FillSide(g, b);
            var lightBottom = _mode.SideStops[2].GetBrightness() > 0.75f;
            Color fill, line, text;
            if (_mode.Night) { fill = Color.FromArgb(_hot.Contains(b) ? 60 : 28, 244, 63, 94); line = Color.FromArgb(120, 244, 63, 94); text = DayScene.Hex("#FDA4AF"); }
            else if (lightBottom) { fill = Color.FromArgb(_hot.Contains(b) ? 255 : 170, 255, 255, 255); line = DayScene.Hex("#FBCFE8"); text = DayScene.Hex("#BE123C"); }
            else { fill = Color.FromArgb(_hot.Contains(b) ? 70 : 38, 255, 255, 255); line = Color.FromArgb(110, 255, 255, 255); text = Color.White; }
            var r = new RectangleF(0.5f, 0.5f, b.Width - 1.5f, b.Height - 1.5f);
            using (var path = DayScene.Round(r.X, r.Y, r.Width, r.Height, LogicalToDeviceUnits(11)))
            using (var fb = new SolidBrush(fill))
            using (var pen = new Pen(line))
            {
                g.FillPath(fb, path);
                g.DrawPath(pen, path);
            }
            var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix |
                        (_navCollapsed ? TextFormatFlags.HorizontalCenter : TextFormatFlags.Left);
            var tr = _navCollapsed ? b.ClientRectangle : new Rectangle(LogicalToDeviceUnits(8), 0, b.Width - LogicalToDeviceUnits(8), b.Height);
            TextRenderer.DrawText(g, b.Text, b.Font, tr, text, flags);
        }

        /// <summary>1 nút chọn buổi: viên trắng (Tối: viên tím) khi đang chọn, viên mờ khi rê chuột.</summary>
        private void PaintModeButton(Graphics g, Button b, UiMode m)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(_mode.Seg);
            var on = m == _mode;
            var radius = b.Height / 2f;
            if (on)
            {
                using (var shadow = DayScene.Round(1, 2, b.Width - 2, b.Height - 2, radius))
                using (var sb = new SolidBrush(Color.FromArgb(35, 15, 23, 42)))
                    g.FillPath(sb, shadow);
                DayScene.FillRound(g, _mode.Night ? _mode.Active : Color.White, 0, 0, b.Width - 1, b.Height - 2, radius);
            }
            else if (_hot.Contains(b))
                DayScene.FillRound(g, Color.FromArgb(_mode.Night ? 50 : 140, 255, 255, 255), 0, 0, b.Width - 1, b.Height - 2, radius);
            var color = on ? (_mode.Night ? Color.White : DayScene.Hex("#0F172A")) : _mode.SegText;
            TextRenderer.DrawText(g, b.Text, b.Font, new Rectangle(0, 0, b.Width, b.Height - 1), color,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
        }
    }
}
