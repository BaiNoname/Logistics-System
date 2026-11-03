using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.GUI.GUI_ContractManagement;
using TruckingSystem.Desktop.GUI.GUI_PricingManagement;
using TruckingSystem.Desktop.GUI_CustomerManagement;
using TruckingSystem.Desktop.GUI_EmployeeManagement;
using TruckingSystem.Desktop.GUI_InvoiceManagement;
using TruckingSystem.Desktop.GUI_LocationManagement;
using TruckingSystem.Desktop.GUI_TransportOrderManagement;
using TruckingSystem.Desktop.GUI_FuelManagement;
using TruckingSystem.Desktop.GUI_TripManagement;
using TruckingSystem.Desktop.GUI_VehicleManagement;
using TruckingSystem.Desktop.GUI_WarehouseManagement;
using TruckingSystem.Desktop.GUI_DriverManagement;
using TruckingSystem.Desktop.GUI_AccountManagement;
using TruckingSystem.Desktop.GUI_SystemManagement;

namespace TruckingSystem.Desktop.GUI_Shared
{
    /// <summary>Logic — shell chính (sidebar + nội dung động). Khung tĩnh (3 panel
    /// sidebar/content/topBar) nằm ở MainForm.Designer.cs. Nội dung BÊN TRONG
    /// sidebar (danh sách nút điều hướng) vẫn tạo bằng code — vì sinh ra từ vòng
    /// lặp dữ liệu (navItems), không phải danh sách cố định Designer vẽ tay được.</summary>
    public partial class MainForm : BaseForm
    {
        /// <summary>Phát ra khi người dùng xác nhận đăng xuất — TruckingAppContext
        /// lắng nghe để quay lại LoginForm. Cờ IsLoggingOut giúp phân biệt việc
        /// Close() này là do "Đăng xuất" (quay lại Login) hay do bấm nút ✕ thật
        /// (thoát hẳn ứng dụng).</summary>
        public event EventHandler? LogoutRequested;
        public bool IsLoggingOut { get; private set; }

        private ApiServices? _api;
        private readonly List<Button> _navButtons = new();
        private Button? _activeNavButton;
        private NotificationBadge? _badge;

        // ---- CHẾ ĐỘ MÀU của khung (🌅 Bình minh / ☀️ Trưa / 🌇 Chiều / 🌙 Tối) — chọn ở thanh trên cùng, app nhớ cho lần sau.
        // Chỉ áp cho KHUNG MainForm (menu trái, logo, thanh trên, nền); các màn bên trong giữ Theme dùng chung.
        private UiMode _mode = DayScene.LoadMode();
        private readonly Panel _sideBorder = new() { Dock = DockStyle.Right, Width = 1 };
        private readonly FlowLayoutPanel _modeBar = new()
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = false,
            Anchor = AnchorStyles.Left,
            Padding = new Padding(3),
            Margin = new Padding(0),
        };
        private readonly List<(Button Button, UiMode Mode)> _modeButtons = new();

        public MainForm()
        {
            InitializeComponent();
            SetupFrame();
            ApplyMode(_mode, save: false);
        }

        /// <summary>Dựng phần trang trí của khung (1 lần): viền menu, logo vẽ tay, nút Đăng xuất, dải màu dưới thanh trên,
        /// 4 nút chế độ màu. Màu cụ thể do ApplyMode đặt.</summary>
        private void SetupFrame()
        {
            Text = "Logistics System";
            sidebar.Controls.Add(_sideBorder); // viền phải menu
            SetupLook(); // menu dải màu trời, mục chọn bo tròn, ô tìm + nút chọn buổi bo tròn (MainForm.Look.cs)

            // Logo: vẽ tay (trời theo chế độ + đồi + đường + xe nhỏ + chữ).
            lblLogo.Visible = false;
            logoPanel.Height = LogicalToDeviceUnits(96);
            typeof(Control).GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(logoPanel, true);
            logoPanel.Paint += (_, e) => { if (!PaintMiniLogo(e.Graphics)) DayScene.PaintLogo(e.Graphics, logoPanel.ClientSize, _mode); };
            logoPanel.Resize += (_, _) => logoPanel.Invalidate();

            btnLogout.FlatAppearance.BorderSize = 1;

            // Dải màu dưới thanh trên cùng (màu theo chế độ).
            topBarBorder.Height = LogicalToDeviceUnits(3);
            topBarBorder.Paint += (_, e) =>
            {
                var r = topBarBorder.ClientRectangle;
                if (r.Width <= 0 || r.Height <= 0) return;
                using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(r, Color.Black, Color.Black, 0f)
                {
                    InterpolationColors = new System.Drawing.Drawing2D.ColorBlend { Colors = _mode.Strip, Positions = new[] { 0f, 0.5f, 1f } },
                };
                e.Graphics.FillRectangle(brush, r);
            };
            topBarBorder.Resize += (_, _) => topBarBorder.Invalidate();

            // 4 nút chế độ màu (dạng nút gộp) ở đầu thanh trên cùng.
            var tip = new ToolTip();
            foreach (var m in DayScene.Modes)
            {
                var b = new Button
                {
                    Text = m.Name,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    Margin = new Padding(1, 0, 1, 0),
                    Padding = new Padding(6, 2, 6, 2),
                    MinimumSize = new Size(0, LogicalToDeviceUnits(32)),
                    UseVisualStyleBackColor = false,
                };
                b.FlatAppearance.BorderSize = 0;
                SetDoubleBuffered(b);
                TrackHot(b);
                var mode = m;
                b.Paint += (_, e) => PaintModeButton(e.Graphics, b, mode);
                b.Click += (_, _) => ApplyMode(m, save: true);
                tip.SetToolTip(b, $"Đổi màu khung giao diện sang chế độ {m.Name.Substring(m.Name.IndexOf(' ') + 1)}");
                _modeButtons.Add((b, m));
                _modeBar.Controls.Add(b);
            }
            // Đầu thanh trên: ô TÌM NHANH toàn hệ thống + 4 nút chế độ màu.
            _topLeft.Controls.Add(_searchHost); // ô tìm nằm trong khung bo tròn (MainForm.Look.cs)
            _topLeft.Controls.Add(_modeBar);
            topBarLayout.Controls.Add(_topLeft, 0, 0);
        }

        /// <summary>Đổi màu khung theo chế độ: menu trái, mục đang chọn, nút Đăng xuất, thanh trên, nền, logo, dải màu.</summary>
        private void ApplyMode(UiMode m, bool save)
        {
            _mode = m;
            SuspendLayout();
            sidebar.BackColor = navPanel.BackColor = sidebarBottom.BackColor = m.Side;
            _sideBorder.BackColor = m.Night ? DayScene.Hex("#1E293B") : DayScene.Hex("#E2E8F0");
            foreach (var h in _navHeaders) StyleGroupHeader(h);
            foreach (var b in _navButtons) StyleNavButton(b, b == _activeNavButton);

            btnLogout.BackColor = m.Night ? DayScene.Hex("#1E293B") : Color.White;
            btnLogout.ForeColor = m.Night ? DayScene.Hex("#FCA5A5") : DayScene.Hex("#DC2626");
            btnLogout.FlatAppearance.BorderColor = m.Night ? DayScene.Hex("#7F1D1D") : DayScene.Hex("#FECACA");
            btnLogout.FlatAppearance.MouseOverBackColor = m.Night ? DayScene.Hex("#450A0A") : DayScene.Hex("#FEE2E2");

            topBar.BackColor = m.Top;
            btnNotifications.BackColor = m.Top;
            lblAvatar.BackColor = m.Top;
            btnNotifications.Invalidate();
            lblAvatar.Invalidate();
            btnNotifications.FlatAppearance.MouseOverBackColor = m.SideHover;
            lblUser.ForeColor = m.TopText;
            lblRole.ForeColor = m.TopSubText;
            BackColor = content.BackColor = m.Content;

            _modeBar.BackColor = m.Seg;
            // Nút chính trong các màn phần của Nhân tô dải màu theo buổi (ModernLook).
            ModernLook.SetAccent(m.Strip[0], DayScene.Mix(m.Strip[1], Color.Black, 0.15f));
            content.Invalidate(true);
            _searchBox.BackColor = SearchFill;
            _searchBox.ForeColor = m.Night ? DayScene.Hex("#EEF2FF") : DayScene.Hex("#0F172A");
            _searchHost.BackColor = m.Top;
            foreach (var (b, bm) in _modeButtons)
            {
                var on = bm == m;
                b.BackColor = on ? Color.White : _modeBar.BackColor;
                b.ForeColor = on ? DayScene.Hex("#0F172A") : m.Night ? DayScene.Hex("#CBD5E1") : DayScene.Hex("#475569");
                b.FlatAppearance.MouseOverBackColor = on ? Color.White : m.Night ? DayScene.Hex("#1E293B") : DayScene.Hex("#F1F5F9");
            }
            ResumeLayout(true);
            if (_navCollapsed) RestyleNav(); // menu đang thu gọn: tô lại nhóm đang chứa màn hiện tại
            if (_navToggle is not null) { _navToggle.BackColor = m.Side; _navToggle.ForeColor = m.SideText; }
            logoPanel.Invalidate();
            topBarBorder.Invalidate();
            _searchHost.Invalidate(true);
            _modeBar.Invalidate(true);
            navPanel.Invalidate(true);
            sidebarBottom.Invalidate(true);
            if (save) DayScene.SaveMode(m);
        }

        private void StyleNavButton(Button b, bool active)
        {
            b.BackColor = active ? _mode.Active : _mode.Side;
            b.ForeColor = active ? Color.White : _mode.SideText;
            b.FlatAppearance.MouseOverBackColor = active ? _mode.ActiveHover : _mode.SideHover;
            b.Invalidate(); // nền tự vẽ (PaintNavItem) không tự vẽ lại khi màu nút không đổi
        }

        public MainForm(ApiServices api, string username) : this()
        {
            _api = api;
            // Đăng nhập xong: mở app TOÀN MÀN HÌNH (phóng to), không phải cửa sổ 1280×800 giữa màn.
            WindowState = FormWindowState.Maximized;
            lblUser.Text = username;
            lblRole.Text = RoleText(api.Connection.Roles);
            BuildNavButtons();
            _badge = new NotificationBadge(btnNotifications, api);
            // Màn đầu tiên theo vai trò: mục đầu tiên tài khoản được xem (Quản trị / Điều phối / Kế toán: Tổng quan —
            // Kế toán tự vào trang Tài chính; Nhân viên kho: Kho trung chuyển).
            SetupResponsive(); // co giãn thanh trên theo bề ngang cửa sổ (MainForm.Responsive.cs)
            SetupNavCollapse(); // menu trái thu còn biểu tượng khi cửa sổ hẹp (MainForm.NavCollapse.cs)
            if (_navButtons.Count > 0) NavigateTo(_navButtons[0], _buildOf[_navButtons[0]]);
            SetupGlobalSearch();
            SetupMenuBadges();
            api.Connection.SessionExpired += OnSessionExpired;
            FormClosed += (_, _) => api.Connection.SessionExpired -= OnSessionExpired;
        }

        /// <summary>Sinh các nút điều hướng theo quyền (role) vào navPanel (khai báo ở Designer).
        /// Không dùng tọa độ: navPanel là FlowLayoutPanel xếp dọc; bề ngang nút bám theo bề
        /// ngang navPanel (xem NavPanel_ClientSizeChanged), chiều cao nhân theo DPI.</summary>
        private void BuildNavButtons()
        {
            if (_api is null) return;

            var navItems = new (string Icon, string Text, string[] Roles, Func<Control> Build)[]
            {
                // Tổng quan không gồm Nhân viên kho (màn này chỉ có số liệu tài chính / vận hành họ không được xem) -> nhân viên kho
                // đăng nhập vào thẳng màn đầu tiên của mình (Kho trung chuyển) thay vì gặp câu "không có quyền".
                ("📊", "Tổng quan", new[] { "Admin", "Dispatcher", "Accountant" }, () => new DashboardControl(_api)),
                ("📦", "Đơn vận chuyển", new[] { "Admin", "Dispatcher" }, () => new TransportOrdersControl(_api)),
                ("🛣️", "Chuyến xe", new[] { "Admin", "Dispatcher" }, () => new TripsControl(_api)),
                ("⛽", "Nhiên liệu", new[] { "Admin", "Accountant" }, () => new FuelManagementControl(_api)),
                ("📍", "Địa điểm", new[] { "Admin", "Dispatcher" }, () => new LocationsControl(_api)),
                ("🚚", "Đội xe", new[] { "Admin", "Dispatcher" }, () => new VehiclesControl(_api)),
                ("🏬", "Kho trung chuyển", new[] { "Admin", "Dispatcher", "WarehouseStaff" }, () => new WarehousesControl(_api)),
                ("👥", "Khách hàng", new[] { "Admin", "Dispatcher", "Accountant" }, () => new CustomersControl(_api)),
                ("📄", "Hợp đồng", new[] { "Admin", "Dispatcher", "Accountant" }, () => new ContractsControl(_api)),
                ("📎", "Phụ lục hợp đồng", new[] { "Admin", "Dispatcher", "Accountant" }, () => new ContractAppendicesControl(_api)),
                ("₫", "Bảng giá", new[] { "Admin", "Dispatcher", "Accountant" }, () => new PricingControl(_api)),
                ("🧑‍💼", "Nhân viên", new[] { "Admin", "Dispatcher" }, () => new EmployeesControl(_api)),
                ("₫", "Công & lương", new[] { "Admin", "Dispatcher", "Accountant" }, () => new TruckingSystem.Desktop.GUI_PayrollManagement.PayrollControl(_api)),
                ("🚕", "Tài xế", new[] { "Admin", "Dispatcher" }, () => new DriversControl(_api)),
                ("🧭", "Nguồn lực theo kho", new[] { "Admin", "Dispatcher" }, () => new TruckingSystem.Desktop.GUI_ResourceManagement.ResourcesControl(_api)),
                ("🧾", "Hóa đơn", new[] { "Admin", "Accountant" }, () => new InvoicesControl(_api)),
                ("📜", "Nhật ký hệ thống", new[] { "Admin" }, () => new TruckingSystem.Desktop.GUI_AuditLogManagement.AuditLogsControl(_api)),
                ("🔐", "Quản lý tài khoản", new[] { "Admin" }, () => new AccountsControl(_api)),
                ("⚙️", "Hệ Thống", new[] { "Admin" }, () => new SystemControl(_api)),
            };

            // MENU CHIA NHÓM (Vận hành / Tài nguyên / Kinh doanh / Nhân sự / Hệ thống) — mỗi nhóm có tiêu đề nhỏ.
            // Mục được xếp theo nhóm (giữ thứ tự trong mảng ở trên), nhóm không có mục nào được quyền xem thì ẩn
            // luôn tiêu đề. Thêm mục mới vào mảng trên là tự vào đúng nhóm theo NavGroup (không khớp -> "Hệ thống").
            var visible = navItems
                .Select((item, index) => (Item: item, Index: index, Group: NavGroup(item.Text)))
                .Where(x => _api.Connection.IsInRole(x.Item.Roles))
                .OrderBy(x => Array.IndexOf(NavGroupOrder, x.Group))
                .ThenBy(x => x.Index)
                .ToList();

            navPanel.SuspendLayout();
            string? currentGroup = null;
            foreach (var (item, _, group) in visible)
            {
                if (group != currentGroup)
                {
                    currentGroup = group;
                    if (group.Length > 0)
                    {
                        var header = MakeNavHeader(group);
                        var g = group;
                        header.Click += (_, _) => { if (!_navCollapsed) ToggleGroup(g); }; // thu gọn: bấm = bảng nổi (NavCollapse)
                        navPanel.Controls.Add(header);
                        _navHeaders.Add(header);
                    }
                }

                var btn = MakeNavButton(item.Icon, item.Text, child: group.Length > 0);
                // Mục đang có số đỏ (việc chờ) -> mở màn đã lọc sẵn đúng các việc đó; không có thì mở bình thường.
                btn.Click += (_, _) => NavigateTo(btn, PendingBuild(btn) ?? item.Build);
                _buildOf[btn] = item.Build;
                navPanel.Controls.Add(btn);
                _navButtons.Add(btn);
                _groupOf[btn] = group;
            }
            // Mở lại đúng nhóm lần trước (nếu tài khoản này còn quyền thấy nhóm đó), các nhóm khác thu gọn.
            var saved = LoadOpenGroup();
            SetOpenGroup(_navHeaders.Any(h => (string)h.Tag! == saved) ? saved : null, save: false);
            navPanel.ResumeLayout(true);
            ResizeNavButtons();
        }

        // ================= Menu trái THU GỌN THEO NHÓM (chỉ mở 1 nhóm 1 lúc) =================
        // Tổng quan đứng riêng; mỗi nhóm (Vận hành, Tài nguyên, Kinh doanh, Nhân sự, Hệ thống) là 1 nút — bấm thì xổ các
        // mục con ra, nhóm đang mở khác tự đóng lại -> menu luôn ngắn. Mở 1 màn (kể cả từ chuông thông báo) tự mở đúng
        // nhóm của nó. Nhóm đang chứa màn hiện tại mà đang thu gọn thì tên nhóm tô màu nhấn để biết mình đang ở đâu.
        // App nhớ nhóm đang mở cho lần sau.

        private readonly List<Button> _navHeaders = new();
        private readonly Dictionary<Button, string> _groupOf = new();
        private readonly Dictionary<Button, Func<Control>> _buildOf = new();
        private string? _openGroup;

        private static readonly Dictionary<string, (string Icon, string Name)> GroupLabels = new()
        {
            ["VẬN HÀNH"] = ("🚚", "Vận hành"),
            ["TÀI NGUYÊN"] = ("🧰", "Tài nguyên"),
            ["KINH DOANH"] = ("💼", "Kinh doanh"),
            ["NHÂN SỰ"] = ("👥", "Nhân sự"),
            ["HỆ THỐNG"] = ("⚙️", "Hệ thống"),
        };

        private void ToggleGroup(string group) => SetOpenGroup(_openGroup == group ? null : group, save: true);

        private void SetOpenGroup(string? group, bool save)
        {
            _openGroup = group;
            navPanel.SuspendLayout();
            foreach (var (btn, g) in _groupOf)
                btn.Visible = g.Length == 0 || (!_navCollapsed && g == group); // menu thu gọn: mục con xem qua bảng nổi
            foreach (var h in _navHeaders) StyleGroupHeader(h);
            navPanel.ResumeLayout(true);
            if (save) SaveOpenGroup(group);
        }

        private void StyleGroupHeader(Button h)
        {
            var g = (string)h.Tag!;
            var open = g == _openGroup;
            var holdsCurrent = _activeNavButton is not null && _groupOf.TryGetValue(_activeNavButton, out var ag) && ag == g;
            h.BackColor = _mode.Side;
            h.ForeColor = _mode.SideText; // nhóm đang chứa màn hiện tại: viên mờ (PaintNavItem)
            h.FlatAppearance.MouseOverBackColor = _mode.SideHover;
            h.Invalidate(); // vẽ lại mũi tên ▸ / ▾
        }

        private static string NavStateFile => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LogisticsSystem", "nav-group.txt");

        private static string? LoadOpenGroup()
        {
            try { return File.Exists(NavStateFile) ? File.ReadAllText(NavStateFile).Trim() : null; }
            catch { return null; }
        }

        private static void SaveOpenGroup(string? group)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(NavStateFile)!);
                File.WriteAllText(NavStateFile, group ?? "");
            }
            catch { /* không lưu được thì thôi */ }
        }

        /// <summary>Thứ tự nhóm trên menu trái; "" = mục đứng riêng trên cùng (Tổng quan).</summary>
        private static readonly string[] NavGroupOrder = { "", "VẬN HÀNH", "TÀI NGUYÊN", "KINH DOANH", "NHÂN SỰ", "HỆ THỐNG" };

        /// <summary>Mục menu thuộc nhóm nào (theo tên hiển thị).</summary>
        private static string NavGroup(string text)
        {
            var t = string.Join(" ", text.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (t == "Tổng quan") return "";
            if (t is "Đơn vận chuyển" or "Chuyến xe" or "Chuyến (Trip)" or "Kho trung chuyển") return "VẬN HÀNH";
            if (t is "Đội xe" or "Tài xế" or "Nguồn lực theo kho" or "Địa điểm" or "Nhiên liệu") return "TÀI NGUYÊN";
            if (t is "Khách hàng" or "Hợp đồng" or "Phụ lục hợp đồng" or "Bảng giá" or "Hóa đơn") return "KINH DOANH";
            if (t == "Nhân viên" || t.Contains("lương", StringComparison.OrdinalIgnoreCase)) return "NHÂN SỰ";
            return "HỆ THỐNG";
        }

        /// <summary>Nút NHÓM (bấm để xổ / thu các mục con): biểu tượng + tên nhóm chữ đậm, mũi tên ▸ (đang thu) / ▾ (đang mở)
        /// vẽ ở mép phải.</summary>
        private Button MakeNavHeader(string group)
        {
            var (icon, name) = GroupLabels.TryGetValue(group, out var l) ? l : ("📁", group);
            var h = new Button
            {
                Text = $"  {icon}   {name}",
                Tag = group,
                Width = NavButtonWidth(),
                Height = LogicalToDeviceUnits(44),
                TextAlign = ContentAlignment.MiddleLeft,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, LogicalToDeviceUnits(4), 0, 2),
                UseVisualStyleBackColor = false,
            };
            h.FlatAppearance.BorderSize = 0;
            SetDoubleBuffered(h);
            TrackHot(h);
            h.Paint += (_, e) => PaintNavItem(e.Graphics, h); // nền + chữ trước, mũi tên vẽ chồng lên sau
            h.Paint += (_, e) =>
            {
                if (_navCollapsed) return; // thu gọn: không vẽ mũi tên
                var arrow = (string)h.Tag! == _openGroup ? "▾" : "▸";
                var r = new Rectangle(h.Width - LogicalToDeviceUnits(32), 0, LogicalToDeviceUnits(24), h.Height);
                TextRenderer.DrawText(e.Graphics, arrow, h.Font, r, h.ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            };
            return h;
        }

        /// <param name="child">Mục con của 1 nhóm -> thụt vào 1 chút cho thấy thuộc nhóm phía trên.</param>
        private Button MakeNavButton(string icon, string text, bool child = false)
        {
            var btn = new Button
            {
                Text = $"  {icon}   {text}",
                Padding = new Padding(child ? LogicalToDeviceUnits(16) : 0, 0, 0, 0),
                Width = NavButtonWidth(),
                Height = LogicalToDeviceUnits(44),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = false,
                UseCompatibleTextRendering = false,
                FlatStyle = FlatStyle.Flat,
                BackColor = _mode.Side,
                ForeColor = _mode.SideText,
                Font = Theme.FontSidebar,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 2, 0, 2),
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = _mode.SideHover;
            SetDoubleBuffered(btn);
            TrackHot(btn);
            btn.Paint += (_, e) => PaintNavItem(e.Graphics, btn); // gắn đầu tiên: số đỏ / biểu tượng thu gọn vẽ chồng lên sau
            return btn;
        }

        /// <summary>Bề ngang nút = bề ngang vùng trong của navPanel (đã trừ thanh cuộn nếu có)
        /// trừ padding — nút luôn kín sidebar ở mọi mức DPI, không phát sinh cuộn ngang.</summary>
        private int NavButtonWidth() => Math.Max(0, navPanel.ClientSize.Width - navPanel.Padding.Horizontal);

        private void ResizeNavButtons()
        {
            var width = NavButtonWidth();
            var height = LogicalToDeviceUnits(44);
            foreach (var btn in _navButtons)
            {
                btn.Width = width;
                btn.Height = height;
            }
            foreach (var header in _navHeaders)
            {
                header.Width = width;
                header.Height = height;
            }
        }

        private void NavPanel_ClientSizeChanged(object? sender, EventArgs e) => ResizeNavButtons();

        protected override void OnDpiChanged(DpiChangedEventArgs e)
        {
            base.OnDpiChanged(e);
            ResizeNavButtons();
        }

        private void BtnLogout_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;

            var confirm = MessageBox.Show(FindForm(),
                "Bạn có chắc chắn muốn đăng xuất không?",
                "Xác nhận đăng xuất",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            _api.Connection.ClearToken();
            IsLoggingOut = true;
            LogoutRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Nút chuông thông báo — quét cảnh báo tự động (giấy tờ xe sắp/đã
        /// hết hạn, tài xế chưa đăng ký lịch làm ngày mai...) mỗi lần mở.</summary>
        /// <summary>Tên vai trò tiếng Việt hiển thị dưới tên đăng nhập (mã role trong hệ thống
        /// vẫn là tiếng Anh: Admin, Dispatcher...). Nhiều role thì nối bằng dấu phẩy.</summary>
        private static string RoleText(IEnumerable<string> roles)
        {
            var names = roles.Select(r => r switch
            {
                "Admin" => "Quản trị viên",
                "Dispatcher" => "Điều phối viên",
                "Accountant" => "Kế toán",
                "WarehouseStaff" => "Nhân viên kho",
                "Driver" => "Tài xế",
                _ => r,
            }).ToList();
            return names.Count == 0 ? "" : string.Join(", ", names);
        }

        private void BtnNotifications_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;
            using var f = new TruckingSystem.Desktop.GUI_NotificationManagement.NotificationsForm(_api);
            var dialogResult = f.ShowDialog(this);
            _ = _badge?.RefreshAsync(); // vừa xử lý xong việc nào thì số đỏ giảm ngay
            if (dialogResult != DialogResult.OK || f.SelectedTarget is not { } target) return;

            var (navText, build) = TruckingSystem.Desktop.GUI_NotificationManagement.NotificationNavigator.Resolve(_api, target);
            var navButton = _navButtons.FirstOrDefault(b => b.Text.TrimEnd().EndsWith(navText));
            if (navButton is null)
            {
                MessageBox.Show(this, $"Tài khoản của bạn không có quyền vào màn \"{navText}\" để xử lý việc này.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            NavigateTo(navButton, build);
        }

        private void NavigateTo(Button navButton, Func<Control> buildControl)
        {
            if (_activeNavButton != null)
            {
                StyleNavButton(_activeNavButton, false);
            }
            StyleNavButton(navButton, true);
            _activeNavButton = navButton;
            // Mở 1 màn (từ menu hoặc chuông thông báo) -> tự mở đúng nhóm của màn đó.
            if (_groupOf.TryGetValue(navButton, out var g) && g.Length > 0 && g != _openGroup) SetOpenGroup(g, save: true);
            else foreach (var h in _navHeaders) StyleGroupHeader(h);

            // GIẢI PHÓNG màn cũ (trước đây chỉ Controls.Clear(): mỗi lần bấm menu, màn cũ — bảng, thẻ, đồng hồ hẹn giờ
            // bên trong — vẫn nằm trong bộ nhớ và có cái vẫn chạy ngầm -> dùng lâu app chậm dần).
            BeginContentSwap(); // ngưng vẽ trong lúc đổi màn -> màn mới hiện ra 1 lần, không hiện lần lượt (MainForm.Look.cs)
            try
            {
                var old = content.Controls.Cast<Control>().ToList();
                content.Controls.Clear();
                foreach (var c in old) c.Dispose();
                var control = buildControl();
                control.Dock = DockStyle.Fill;
                content.Controls.Add(control);
            }
            finally { EndContentSwap(); }
            if (_navCollapsed) RestyleNav();
            RefreshMenuBadgesSoon(); // vừa rời màn trước (có thể vừa làm xong việc) -> đếm lại số việc chờ trên menu
        }
    }
}
