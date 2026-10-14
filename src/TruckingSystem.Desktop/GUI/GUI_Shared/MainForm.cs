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

        public MainForm()
        {
            InitializeComponent();
        }

        public MainForm(ApiServices api, string username) : this()
        {
            _api = api;
            lblUser.Text = username;
            lblRole.Text = RoleText(api.Connection.Roles);
            BuildNavButtons();
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
                ("🛣️", "Chuyến (Trip)", new[] { "Admin", "Dispatcher" }, () => new TripsControl(_api)),
                ("⛽", "Nhiên liệu", new[] { "Admin", "Accountant" }, () => new FuelManagementControl(_api)),
                ("📍", "Địa điểm", new[] { "Admin", "Dispatcher" }, () => new LocationsControl(_api)),
                ("🚚", "Đội xe", new[] { "Admin", "Dispatcher" }, () => new VehiclesControl(_api)),
                ("🏬", "Kho trung chuyển", new[] { "Admin", "Dispatcher", "WarehouseStaff" }, () => new WarehousesControl(_api)),
                ("👥", "Khách hàng", new[] { "Admin", "Dispatcher", "Accountant" }, () => new CustomersControl(_api)),
                ("📄", "Hợp đồng", new[] { "Admin", "Dispatcher", "Accountant" }, () => new ContractsControl(_api)),
                ("₫", "Bảng giá", new[] { "Admin", "Dispatcher", "Accountant" }, () => new PricingControl(_api)),
                ("🧑‍💼", "Nhân viên", new[] { "Admin", "Dispatcher" }, () => new EmployeesControl(_api)),
                ("🚕", "Tài xế", new[] { "Admin", "Dispatcher" }, () => new DriversControl(_api)),
                ("🧾", "Hóa đơn", new[] { "Admin", "Accountant" }, () => new InvoicesControl(_api)),
                ("📜", "Nhật ký hệ thống", new[] { "Admin", "Dispatcher" }, () => new TruckingSystem.Desktop.GUI_AuditLogManagement.AuditLogsControl(_api)),
                ("🔐", "Quản lý tài khoản", new[] { "Admin" }, () => new AccountsControl(_api)),
                ("⚙️", "Hệ Thống", new[] { "Admin" }, () => new SystemControl(_api)),
            };

            navPanel.SuspendLayout();
            foreach (var item in navItems)
            {
                if (!_api.Connection.IsInRole(item.Roles)) continue;

                var btn = MakeNavButton(item.Icon, item.Text);
                btn.Click += (_, _) => NavigateTo(btn, item.Build);
                navPanel.Controls.Add(btn);
                _navButtons.Add(btn);
            }
            navPanel.ResumeLayout(true);
            ResizeNavButtons();
        }

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
            f.ShowDialog(this);
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
