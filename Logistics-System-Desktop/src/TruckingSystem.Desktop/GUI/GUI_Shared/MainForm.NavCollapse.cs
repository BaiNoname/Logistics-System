namespace TruckingSystem.Desktop.GUI_Shared
{
    /// <summary>MENU TRÁI THU GỌN CÒN BIỂU TƯỢNG (mẫu menu_thu_gon đã duyệt):
    ///  • Tự thu (260 -> 64 px) khi bề ngang cửa sổ &lt; ~1100 px (ở 100%), tự mở lại khi kéo rộng.
    ///  • Nút « / » ở đáy menu: tự thu / mở bằng tay — app nhớ cho lần sau (nav-collapsed.txt: "1" / "0"; không có = tự động).
    ///  • Khi thu: chỉ còn biểu tượng nhóm (Vận hành, Tài nguyên…) và mục lẻ (Tổng quan); bấm biểu tượng nhóm -> bảng nổi
    ///    các mục con kèm số việc chờ; rê chuột -> tên. Số đỏ của nhóm = tổng việc chờ các mục con. Nhóm đang chứa màn hiện tại
    ///    tô xanh. Logo còn hình xe tải, Đăng xuất còn 🚪.
    /// Chữ trên nút giữ nguyên (chỉ ẩn khi vẽ) vì số đỏ / thông báo / tìm nhanh tra nút theo chữ.</summary>
    public partial class MainForm
    {
        private bool _navCollapsed;
        private bool? _navPinned; // null = tự động theo bề ngang; true = luôn thu; false = luôn mở
        private Button? _navToggle;
        private string _logoutText = "";
        private readonly ToolTip _navTip = new() { InitialDelay = 250 };
        private ContextMenuStrip? _flyout;

        private static string NavCollapseFile => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LogisticsSystem", "nav-collapsed.txt");

        private void SetupNavCollapse()
        {
            try
            {
                var s = File.Exists(NavCollapseFile) ? File.ReadAllText(NavCollapseFile).Trim() : "";
                _navPinned = s == "1" ? true : s == "0" ? false : null;
            }
            catch { }
            _logoutText = btnLogout.Text;
            int S(int v) => LogicalToDeviceUnits(v);
            _navToggle = new Button
            {
                Dock = DockStyle.Top, Height = S(30), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, TabStop = false,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold), UseVisualStyleBackColor = false, Margin = Padding.Empty,
            };
            _navToggle.FlatAppearance.BorderSize = 0;
            SetDoubleBuffered(_navToggle);
            TrackHot(_navToggle);
            _navToggle.Paint += (_, e) => PaintNavToggle(e.Graphics, _navToggle);
            _navToggle.Click += (_, _) =>
            {
                _navPinned = !_navCollapsed;
                try { Directory.CreateDirectory(Path.GetDirectoryName(NavCollapseFile)!); File.WriteAllText(NavCollapseFile, _navPinned == true ? "1" : "0"); } catch { }
                ApplyNavCollapse();
            };
            sidebarBottom.Height += S(36);
            sidebarBottom.Controls.Add(_navToggle);
            _navToggle.BringToFront(); // Dock Top trên nút Đăng xuất (Dock Fill)
            btnLogout.BringToFront();

            // Vẽ biểu tượng giữa nút khi thu gọn (chữ được ẩn bằng màu chữ = màu nền).
            foreach (var b in _navButtons.Concat(_navHeaders)) b.Paint += (_, e) => PaintCollapsedIcon(e.Graphics, b);
            foreach (var h in _navHeaders)
            {
                var header = h;
                // Hoãn tới sau khi nút nhả chuột xong mới mở bảng nổi (mở ngay trong Click có lúc bảng vừa hiện đã bị đóng).
                header.Click += (_, _) => { if (_navCollapsed) BeginInvoke(new Action(() => ShowFlyout(header))); };
            }
            Resize += (_, _) => ApplyNavCollapse();
            Shown += (_, _) => ApplyNavCollapse();
        }

        private bool IsNarrow => ClientSize.Width < LogicalToDeviceUnits(1100);

        private void ApplyNavCollapse()
        {
            if (WindowState == FormWindowState.Minimized || IsDisposed || _navToggle is null) return;
            var collapse = _navPinned ?? IsNarrow;
            _navToggle.Text = collapse ? "»" : "«  Thu gọn menu";
            _navToggle.BackColor = _mode.Side;
            _navToggle.ForeColor = _mode.SideText;
            _navTip.SetToolTip(_navToggle, collapse ? "Mở rộng menu" : "Thu gọn menu (chỉ còn biểu tượng)");
            if (collapse == _navCollapsed && sidebar.Width == SidebarWidth(collapse)) return;
            _navCollapsed = collapse;
            SuspendLayout();
            sidebar.Width = SidebarWidth(collapse);
            navPanel.Padding = collapse ? new Padding(LogicalToDeviceUnits(6), LogicalToDeviceUnits(10), LogicalToDeviceUnits(6), 0)
                                        : new Padding(LogicalToDeviceUnits(10), LogicalToDeviceUnits(10), LogicalToDeviceUnits(10), 0);
            sidebarBottom.Padding = collapse ? new Padding(LogicalToDeviceUnits(6), 8, LogicalToDeviceUnits(6), 8) : new Padding(LogicalToDeviceUnits(10), 8, LogicalToDeviceUnits(10), 8);
            btnLogout.Text = collapse ? "🚪" : _logoutText;
            btnLogout.TextAlign = collapse ? ContentAlignment.MiddleCenter : ContentAlignment.MiddleLeft;
            _navTip.SetToolTip(btnLogout, collapse ? "Đăng xuất" : "");
            ApplyNavVisibility();
            ApplyNavPadding();
            ResumeLayout(true);
            ResizeNavButtons();
            RestyleNav();
            logoPanel.Invalidate();
        }

        private int SidebarWidth(bool collapsed) => LogicalToDeviceUnits(collapsed ? 64 : 260);

        /// <summary>Thu gọn: ẩn mục con (xem qua bảng nổi); mở: theo nhóm đang mở như cũ.</summary>
        private void ApplyNavVisibility()
        {
            if (_navCollapsed)
                foreach (var (btn, g) in _groupOf) btn.Visible = g.Length == 0;
            else SetOpenGroup(_openGroup, save: false);
        }

        /// <summary>Màu nút khi thu / mở (gọi sau StyleNavButton / StyleGroupHeader).</summary>
        private void RestyleNav()
        {
            foreach (var b in _navButtons) StyleNavButton(b, b == _activeNavButton);
            foreach (var h in _navHeaders) StyleGroupHeader(h);
            if (!_navCollapsed) return;
            foreach (var h in _navHeaders)
            {
                var holds = _activeNavButton is not null && _groupOf.TryGetValue(_activeNavButton, out var g) && g == (string)h.Tag!;
                if (holds) { h.BackColor = _mode.Active; h.FlatAppearance.MouseOverBackColor = _mode.ActiveHover; }
            }
            // Tên khi rê chuột — mục đang có số đỏ đã có chú thích riêng (số việc chờ) nên không chồng thêm.
            foreach (var b in _navButtons) _navTip.SetToolTip(b, CountFor(b) > 0 ? "" : NavName(b));
            foreach (var h in _navHeaders) _navTip.SetToolTip(h, GroupCount((string)h.Tag!) > 0 ? "" : NavName(h) + " — bấm để xem các mục");
        }

        private readonly Dictionary<Button, Padding> _navPadding = new();

        /// <summary>Thu gọn: đẩy chữ ra ngoài mép nút (chữ vẫn giữ để tra cứu), biểu tượng vẽ ở giữa; mở: trả lại lề cũ.</summary>
        private void ApplyNavPadding()
        {
            foreach (var b in _navButtons.Concat(_navHeaders))
            {
                if (!_navPadding.ContainsKey(b)) _navPadding[b] = b.Padding;
                b.Padding = _navCollapsed ? new Padding(LogicalToDeviceUnits(400), 0, 0, 0) : _navPadding[b];
                if (!_navCollapsed) _navTip.SetToolTip(b, "");
            }
        }

        private void PaintCollapsedIcon(Graphics g, Button b)
        {
            if (!_navCollapsed) return;
            using var f = new Font("Segoe UI Emoji", 14F);
            TextRenderer.DrawText(g, NavIcon(b), f, b.ClientRectangle, IsNavActive(b) ? _mode.ActiveText : _mode.SideText,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }

        /// <summary>"  📦   Đơn vận chuyển" -> "📦".</summary>
        private static string NavIcon(Button b) => b.Text.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";

        private static string NavName(Button b)
        {
            var parts = b.Text.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 1 ? parts[1].Trim() : b.Text.Trim();
        }

        /// <summary>Bấm biểu tượng nhóm khi thu gọn: bảng nổi các mục con (kèm số việc chờ), bấm 1 mục -> mở màn đó.</summary>
        private void ShowFlyout(Button header)
        {
            _flyout?.Dispose();
            _flyout = new ContextMenuStrip { ShowImageMargin = false, Font = new Font("Segoe UI", 10.5F) };
            var group = (string)header.Tag!;
            var title = new ToolStripMenuItem(NavIcon(header) + "  " + NavName(header)) { Enabled = false, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold) };
            _flyout.Items.Add(title);
            _flyout.Items.Add(new ToolStripSeparator());
            foreach (var (btn, g) in _groupOf.Where(kv => kv.Value == group))
            {
                var count = CountFor(btn);
                var item = new ToolStripMenuItem($"{NavIcon(btn)}  {NavName(btn)}{(count > 0 ? $"   ({count})" : "")}".Replace("&", "&&"))
                {
                    Font = btn == _activeNavButton ? new Font("Segoe UI", 10.5F, FontStyle.Bold) : _flyout.Font,
                    ForeColor = count > 0 ? Color.FromArgb(185, 28, 28) : SystemColors.ControlText,
                };
                var target = btn;
                // Mục con đang ẩn (menu thu gọn) -> PerformClick() của nút ẩn KHÔNG chạy gì cả. Gọi thẳng mở màn.
                item.Click += (_, _) => OpenNavButton(target);
                _flyout.Items.Add(item);
            }
            if (_flyout.Items.Count <= 2) return; // nhóm không có mục nào tài khoản được xem
            _flyout.Show(header.PointToScreen(new Point(header.Width + LogicalToDeviceUnits(4), 0)), ToolStripDropDownDirection.BelowRight);
        }

        /// <summary>Mở màn của 1 mục menu như bấm vào nó — chạy được cả khi mục đang ẩn (menu thu gọn / nhóm đang đóng),
        /// lúc đó Button.PerformClick() không làm gì. Dùng cho bảng nổi và các lối tắt ở màn Tổng quan.</summary>
        internal void OpenNavButton(Button target)
        {
            if (_buildOf.TryGetValue(target, out var build)) NavigateTo(target, PendingBuild(target) ?? build);
            else target.PerformClick();
        }

        /// <summary>Mở màn theo TÊN mục menu (VD "Hợp đồng"); false = tài khoản không có mục đó.</summary>
        internal bool OpenNavByName(string name)
        {
            var btn = _navButtons.FirstOrDefault(b => string.Equals(NavName(b), name, StringComparison.OrdinalIgnoreCase));
            if (btn is null) return false;
            OpenNavButton(btn);
            return true;
        }

        /// <summary>Logo khi thu gọn: chỉ hình xe tải trên nền trời theo chế độ.</summary>
        private bool PaintMiniLogo(Graphics g)
        {
            if (!_navCollapsed) return false;
            using var bg = new SolidBrush(_mode.Side);
            g.FillRectangle(bg, logoPanel.ClientRectangle);
            using var f = new Font("Segoe UI Emoji", 20F);
            TextRenderer.DrawText(g, "🚚", f, logoPanel.ClientRectangle, Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            return true;
        }
    }
}
