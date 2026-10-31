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

        // ---- Tông màu "Ban ngày" (cùng tông màn đăng nhập) — chỉ áp cho khung MainForm, không đổi Theme dùng chung.
        private static readonly Color SideBg = ColorTranslator.FromHtml("#F0F9FF");
        private static readonly Color SideText = ColorTranslator.FromHtml("#334155");
        private static readonly Color SideHover = ColorTranslator.FromHtml("#DBEAFE");
        private static readonly Color SideActive = ColorTranslator.FromHtml("#2563EB");
        private static readonly Color SideActiveHover = ColorTranslator.FromHtml("#1D4ED8");
        private static readonly Color SideBorder = ColorTranslator.FromHtml("#E2E8F0");

        public MainForm()
        {
            InitializeComponent();
            ApplyDayTheme();
        }

        /// <summary>Đổi khung chính sang tông sáng: menu trái nền xanh trời nhạt, logo có trời – đồi – đường
        /// (giống màn đăng nhập), mục đang chọn nền xanh chữ trắng, nút Đăng xuất đỏ nhạt, dải màu dưới thanh trên.</summary>
        private void ApplyDayTheme()
        {
            Text = "Logistics System";
            sidebar.BackColor = navPanel.BackColor = sidebarBottom.BackColor = SideBg;
            sidebar.Controls.Add(new Panel { Dock = DockStyle.Right, Width = 1, BackColor = SideBorder }); // viền phải

            // Logo: vẽ tay (trời + đồi + đường + xe nhỏ + chữ) thay cho nhãn chữ trắng trên nền tối.
            lblLogo.Visible = false;
            logoPanel.Height = LogicalToDeviceUnits(96);
            typeof(Control).GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(logoPanel, true);
            logoPanel.Paint += (_, e) => PaintLogo(e.Graphics, logoPanel.ClientSize);
            logoPanel.Resize += (_, _) => logoPanel.Invalidate();

            btnLogout.BackColor = Color.White;
            btnLogout.ForeColor = ColorTranslator.FromHtml("#DC2626");
            btnLogout.FlatAppearance.BorderSize = 1;
            btnLogout.FlatAppearance.BorderColor = ColorTranslator.FromHtml("#FECACA");
            btnLogout.FlatAppearance.MouseOverBackColor = ColorTranslator.FromHtml("#FEE2E2");

            // Dải màu xanh – lá – vàng dưới thanh trên cùng (thay đường kẻ xám 1px).
            topBarBorder.Height = LogicalToDeviceUnits(3);
            topBarBorder.Paint += (_, e) =>
            {
                var r = topBarBorder.ClientRectangle;
                if (r.Width <= 0 || r.Height <= 0) return;
                using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(r, Color.Black, Color.Black, 0f)
                {
                    InterpolationColors = new System.Drawing.Drawing2D.ColorBlend
                    {
                        Colors = new[] { ColorTranslator.FromHtml("#7DD3FC"), ColorTranslator.FromHtml("#6EE7B7"), ColorTranslator.FromHtml("#FACC15") },
                        Positions = new[] { 0f, 0.5f, 1f },
                    },
                };
                e.Graphics.FillRectangle(brush, r);
            };
            topBarBorder.Resize += (_, _) => topBarBorder.Invalidate();
        }

        private static readonly Font LogoFont = new("Segoe UI", 21f, FontStyle.Bold, GraphicsUnit.Pixel);

        /// <summary>Logo menu trái, khung chuẩn 260 × 96: trời nhạt, đồi cỏ, con đường có vạch vàng, xe tải nhỏ.</summary>
        private static void PaintLogo(Graphics g, Size size)
        {
            if (size.Width <= 0 || size.Height <= 0) return;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            var scale = size.Height / 96f;
            var w = size.Width / scale;
            g.ScaleTransform(scale, scale);

            using (var sky = new System.Drawing.Drawing2D.LinearGradientBrush(new RectangleF(0, 0, w, 96), ColorTranslator.FromHtml("#BAE6FD"), SideBg, 90f))
                g.FillRectangle(sky, 0, 0, w, 96);
            using (var hill = new System.Drawing.Drawing2D.GraphicsPath())
            {
                hill.AddBezier(0, 80, w * 0.25f, 64, w * 0.45f, 70, w * 0.6f, 76);
                hill.AddBezier(w * 0.6f, 76, w * 0.75f, 80, w * 0.9f, 68, w, 72);
                hill.AddLine(w, 72, w, 86);
                hill.AddLine(w, 86, 0, 86);
                hill.CloseFigure();
                using var b = new SolidBrush(ColorTranslator.FromHtml("#A7F3D0"));
                g.FillPath(b, hill);
            }
            using (var road = new SolidBrush(ColorTranslator.FromHtml("#475569"))) g.FillRectangle(road, 0, 84, w, 12);
            using (var lane = new SolidBrush(ColorTranslator.FromHtml("#FACC15")))
                for (var x = 8f; x < w; x += 32f) g.FillRectangle(lane, x, 89, 16, 2);

            // xe tải nhỏ (dùng lại hình xe của màn đăng nhập, thu nhỏ ~1/3)
            var state = g.Save();
            g.TranslateTransform(w - 66, 64);
            g.ScaleTransform(0.36f, 0.36f);
            LoginForm.DrawTruck(g, 0, 0, 0, smoke: false);
            g.Restore(state);

            using var title = new SolidBrush(ColorTranslator.FromHtml("#0F172A"));
            g.DrawString("Logistics System", LogoFont, title, 16, 18);
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
            NavigateTo(_navButtons[0], () => new DashboardControl(_api));
        }

        /// <summary>Sinh các nút điều hướng theo quyền (role) vào navPanel (khai báo ở Designer).
        /// Không dùng tọa độ: navPanel là FlowLayoutPanel xếp dọc; bề ngang nút bám theo bề
        /// ngang navPanel (xem NavPanel_ClientSizeChanged), chiều cao nhân theo DPI.</summary>
        private void BuildNavButtons()
        {
            if (_api is null) return;

            var navItems = new (string Icon, string Text, string[] Roles, Func<Control> Build)[]
            {
                ("📊", "Tổng quan", new[] { "Admin", "Dispatcher", "Accountant", "WarehouseStaff" }, () => new DashboardControl(_api)),
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
                        navPanel.Controls.Add(header);
                        _navHeaders.Add(header);
                    }
                }

                var btn = MakeNavButton(item.Icon, item.Text);
                btn.Click += (_, _) => NavigateTo(btn, item.Build);
                navPanel.Controls.Add(btn);
                _navButtons.Add(btn);
            }
            navPanel.ResumeLayout(true);
            ResizeNavButtons();
        }

        private readonly List<Label> _navHeaders = new();

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

        /// <summary>Tiêu đề nhóm: chữ nhỏ in hoa màu xám, không bấm được.</summary>
        private Label MakeNavHeader(string text) => new()
        {
            Text = text,
            AutoSize = false,
            Width = NavButtonWidth(),
            Height = LogicalToDeviceUnits(26),
            TextAlign = ContentAlignment.BottomLeft,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = ColorTranslator.FromHtml("#64748B"),
            BackColor = SideBg,
            Padding = new Padding(LogicalToDeviceUnits(12), 0, 0, LogicalToDeviceUnits(2)),
            Margin = new Padding(0, LogicalToDeviceUnits(8), 0, 0),
        };

        private Button MakeNavButton(string icon, string text)
        {
            var btn = new Button
            {
                Text = $"  {icon}   {text}",
                Width = NavButtonWidth(),
                Height = LogicalToDeviceUnits(44),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = false,
                UseCompatibleTextRendering = false,
                FlatStyle = FlatStyle.Flat,
                BackColor = SideBg,
                ForeColor = SideText,
                Font = Theme.FontSidebar,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 2, 0, 2),
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = SideHover;
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
                header.Height = LogicalToDeviceUnits(26);
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
                _activeNavButton.BackColor = SideBg;
                _activeNavButton.ForeColor = SideText;
                _activeNavButton.FlatAppearance.MouseOverBackColor = SideHover;
            }
            navButton.BackColor = SideActive;
            navButton.ForeColor = Color.White;
            navButton.FlatAppearance.MouseOverBackColor = SideActiveHover;
            _activeNavButton = navButton;

            content.Controls.Clear();
            var control = buildControl();
            control.Dock = DockStyle.Fill;
            content.Controls.Add(control);
        }
    }
}
