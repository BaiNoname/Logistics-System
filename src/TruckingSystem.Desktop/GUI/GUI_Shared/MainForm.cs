using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
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

        public MainForm()
        {
            InitializeComponent();
        }

        public MainForm(ApiServices api, string username) : this()
        {
            _api = api;
            BuildSidebar();
            BuildTopBar(username);
            NavigateTo(_navButtons[0], () => new DashboardControl(_api));
        }

        private void BuildSidebar()
        {
            if (_api is null) return;

            var logoPanel = new Panel { Dock = DockStyle.Top, Height = 70 };
            var logo = new Label
            {
                Text = "🚚 TruckingSystem",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
            };
            logoPanel.Controls.Add(logo);

            var navItems = new (string Icon, string Text, string[] Roles, Func<Control> Build)[]
            {
                ("📊", "Tổng quan", new[] { "Admin", "Dispatcher", "Accountant", "WarehouseStaff" }, () => new DashboardControl(_api)),
                ("📦", "Đơn vận chuyển", new[] { "Admin", "Dispatcher" }, () => new TransportOrdersControl(_api)),
                ("🛣️", "Chuyến (Trip)", new[] { "Admin", "Dispatcher" }, () => new TripsControl(_api)),
                ("⛽", "Nhiên liệu", new[] { "Admin", "Accountant" }, () => new FuelManagementControl(_api)),
                ("📍", "Địa điểm", new[] { "Admin", "Dispatcher" }, () => new LocationsControl(_api)),
                ("🚚", "Đội xe", new[] { "Admin", "Dispatcher" }, () => new VehiclesControl(_api)),
                ("🏬", "Kho trung chuyển", new[] { "Admin", "Dispatcher", "WarehouseStaff" }, () => new WarehousesControl(_api)),
                ("👥", "Khách hàng", new[] { "Admin", "Dispatcher", "Accountant" }, () => new CustomersControl(_api)),
                ("🧑‍💼", "Nhân viên", new[] { "Admin", "Dispatcher" }, () => new EmployeesControl(_api)),
                ("🚕", "Tài xế", new[] { "Admin", "Dispatcher" }, () => new DriversControl(_api)),
                ("🧾", "Hóa đơn", new[] { "Admin", "Accountant" }, () => new InvoicesControl(_api)),
                ("📜", "Nhật ký hệ thống", new[] { "Admin", "Dispatcher" }, () => new TruckingSystem.Desktop.GUI_AuditLogManagement.AuditLogsControl(_api)),
                ("🔐", "Quản lý tài khoản", new[] { "Admin" }, () => new AccountsControl(_api)),
                ("⚙️", "Hệ Thống", new[] { "Admin" }, () => new SystemControl(_api)),
            };

            var navPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(10, 10, 10, 0) };

            foreach (var item in navItems)
            {
                if (!_api.Connection.IsInRole(item.Roles)) continue;

                var btn = MakeNavButton(item.Icon, item.Text);
                btn.Click += (_, _) => NavigateTo(btn, item.Build);
                navPanel.Controls.Add(btn);
                _navButtons.Add(btn);
            }

            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 60 };
            var logoutBtn = MakeNavButton("🚪", "Đăng xuất");
            logoutBtn.Click += (_, _) =>
            {
                var confirm = MessageBox.Show(FindForm(),
                    "Bạn có chắc chắn muốn đăng xuất không?",
                    "Xác nhận đăng xuất",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes) return;

                _api.Connection.ClearToken();
                IsLoggingOut = true;
                LogoutRequested?.Invoke(this, EventArgs.Empty);
            };
            bottomPanel.Controls.Add(logoutBtn);

            sidebar.Controls.Add(navPanel);
            sidebar.Controls.Add(bottomPanel);
            sidebar.Controls.Add(logoPanel);
        }

        private Button MakeNavButton(string icon, string text)
        {
            var btn = new Button
            {
                Text = $"  {icon}   {text}",
                Width = 238,
                Height = 44,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = false,
                UseCompatibleTextRendering = false,
                FlatStyle = FlatStyle.Flat,
                BackColor = Theme.SidebarBg,
                ForeColor = Theme.SidebarText,
                Font = Theme.FontSidebar,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 2, 0, 2),
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ColorTranslator.FromHtml("#334155");
            return btn;
        }

        private void BuildTopBar(string username)
        {
            var lblUser = new Label { Text = $"👤  {username}", Dock = DockStyle.Right, Width = 200, TextAlign = ContentAlignment.MiddleRight, Font = Theme.FontNormal, ForeColor = Theme.TextSecondary };

            // Nút chuông thông báo — quét cảnh báo tự động (giấy tờ xe sắp/đã
            // hết hạn, tài xế chưa đăng ký lịch làm ngày mai...) mỗi lần mở.
            var btnNotifications = new Button
            {
                Text = "🔔",
                Dock = DockStyle.Right,
                Width = 60,
                Font = new Font("Segoe UI", 14F),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Cursor = Cursors.Hand,
            };
            btnNotifications.FlatAppearance.BorderSize = 0;
            btnNotifications.Click += (_, _) =>
            {
                if (_api is null) return;
                using var f = new TruckingSystem.Desktop.GUI_NotificationManagement.NotificationsForm(_api);
                f.ShowDialog(this);
            };

            var bottomBorder = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Theme.BorderColor };
            topBar.Controls.Add(lblUser);
            topBar.Controls.Add(btnNotifications);
            topBar.Controls.Add(bottomBorder);
        }

        private void NavigateTo(Button navButton, Func<Control> buildControl)
        {
            if (_activeNavButton != null)
            {
                _activeNavButton.BackColor = Theme.SidebarBg;
                _activeNavButton.ForeColor = Theme.SidebarText;
            }
            navButton.BackColor = Theme.SidebarBgActive;
            navButton.ForeColor = Theme.SidebarTextActive;
            _activeNavButton = navButton;

            content.Controls.Clear();
            var control = buildControl();
            control.Dock = DockStyle.Fill;
            content.Controls.Add(control);
        }
    }
}
