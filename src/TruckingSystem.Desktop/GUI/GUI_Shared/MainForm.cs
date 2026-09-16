using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.GUI_CustomerManagement;
using TruckingSystem.Desktop.GUI_EmployeeManagement;
using TruckingSystem.Desktop.GUI_InvoiceManagement;
using TruckingSystem.Desktop.GUI_LocationManagement;
using TruckingSystem.Desktop.GUI_TransportOrderManagement;
using TruckingSystem.Desktop.GUI_VehicleManagement;
using TruckingSystem.Desktop.GUI_WarehouseManagement;

namespace TruckingSystem.Desktop.GUI_Shared;

/// <summary>Shell chính — sidebar điều hướng bên trái + vùng nội dung tự đổi UserControl theo mục đang chọn.</summary>
public class MainForm : BaseForm
{
    /// <summary>Phát ra khi người dùng xác nhận đăng xuất — TruckingAppContext
    /// lắng nghe để quay lại LoginForm. Cờ IsLoggingOut giúp phân biệt việc
    /// Close() này là do "Đăng xuất" (quay lại Login) hay do bấm nút ✕ thật
    /// (thoát hẳn ứng dụng) — tránh lặp lại đúng lỗi cũ đã sửa.</summary>
    public event EventHandler? LogoutRequested;
    public bool IsLoggingOut { get; private set; }

    private readonly ApiServices _api;
    private readonly Panel _sidebar = new() { Dock = DockStyle.Left, Width = 260 };
    private readonly Panel _content = new() { Dock = DockStyle.Fill, Padding = new Padding(24) };
    private readonly Panel _topBar = new() { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
    private readonly List<Button> _navButtons = new();
    private Button? _activeNavButton;

    public MainForm(ApiServices api, string username)
    {
        _api = api;
        Text = "Trucking System";
        Width = 1280; Height = 800;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Theme.ContentBg;
        MinimumSize = new Size(1100, 700);

        BuildSidebar();
        BuildTopBar(username);

        Controls.Add(_content);
        Controls.Add(_topBar);
        Controls.Add(_sidebar);

        NavigateTo(_navButtons[0], () => new DashboardControl(_api));
    }

    private void BuildSidebar()
    {
        _sidebar.BackColor = Theme.SidebarBg;

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

        var navItems = new (string Icon, string Text, Func<Control> Build)[]
        {
            ("📊", "Tổng quan", () => new DashboardControl(_api)),
            ("📦", "Đơn vận chuyển", () => new TransportOrdersControl(_api)),
            ("📍", "Địa điểm", () => new LocationsControl(_api)),
            ("🚚", "Đội xe", () => new VehiclesControl(_api)),
            ("🏬", "Kho trung chuyển", () => new WarehousesControl(_api)),
            ("👥", "Khách hàng", () => new CustomersControl(_api)),
            ("🧑‍💼", "Nhân viên", () => new EmployeesControl(_api)),
            ("🧾", "Hóa đơn", () => new InvoicesControl(_api)),
        };

        var navPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(10, 10, 10, 0) };

        foreach (var item in navItems)
        {
            var btn = MakeNavButton(item.Icon, item.Text);
            btn.Click += (_, _) => NavigateTo(btn, item.Build);
            navPanel.Controls.Add(btn);
            _navButtons.Add(btn);
        }

        var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 60 };
        var logoutBtn = MakeNavButton("🚪", "Đăng xuất");
        logoutBtn.Click += (_, _) =>
        {
            var confirm = MessageBox.Show(
                "Bạn có chắc chắn muốn đăng xuất không?",
                "Xác nhận đăng xuất",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            _api.Connection.ClearToken();
            IsLoggingOut = true;
            LogoutRequested?.Invoke(this, EventArgs.Empty);
        };
        bottomPanel.Controls.Add(logoutBtn);

        _sidebar.Controls.Add(navPanel);
        _sidebar.Controls.Add(bottomPanel);
        _sidebar.Controls.Add(logoPanel);
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
        _topBar.Padding = new Padding(24, 0, 24, 0);
        var lblUser = new Label { Text = $"👤  {username}", Dock = DockStyle.Right, Width = 200, TextAlign = ContentAlignment.MiddleRight, Font = Theme.FontNormal, ForeColor = Theme.TextSecondary };
        var bottomBorder = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Theme.BorderColor };
        _topBar.Controls.Add(lblUser);
        _topBar.Controls.Add(bottomBorder);
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

        _content.Controls.Clear();
        var control = buildControl();
        control.Dock = DockStyle.Fill;
        _content.Controls.Add(control);
    }
}