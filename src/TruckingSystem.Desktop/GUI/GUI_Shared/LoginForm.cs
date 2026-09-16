using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_Shared;

public class LoginForm : BaseForm
{
    /// <summary>Phát ra khi đăng nhập thành công — TruckingAppContext lắng nghe để
    /// chuyển sang MainForm. LoginForm KHÔNG tự tạo MainForm nữa (tránh lỗi vòng
    /// đời ứng dụng bị gắn nhầm vào 1 Form cụ thể, gây thoát app khi đăng xuất).</summary>
    public event Action<ApiServices, string>? LoginSucceeded;

    /// <summary>Phát ra khi người dùng bấm nút ✕ muốn thoát hẳn ứng dụng.</summary>
    public event EventHandler? ExitRequested;

    private readonly TextBox _txtServerUrl = new() { Width = 320 };
    private readonly TextBox _txtUsername = new() { Width = 320 };
    private readonly TextBox _txtPassword = new() { Width = 320, PasswordChar = '●' };
    private readonly Label _lblStatus = new() { Width = 320, ForeColor = Theme.Danger, Font = Theme.FontSmall, AutoSize = false, Height = 40 };
    private Button _btnLogin = null!;

    // ---- Thành phần cho animation xe tải chạy + quăng thùng hàng ----
    private const int BannerWidth = 460;
    private const int RoadY = 460;              // vị trí "mặt đường" — xe chạy dọc theo trục Y này
    private const int LoopTotalTicks = 150;      // 1 vòng lặp = 150 tick x 30ms ≈ 4.5 giây
    private const int TossTicks = 40;            // thùng hàng chỉ bay trong 40 tick đầu mỗi vòng (~1.2s)
    private const int ArcHeight = 55;            // độ cao cú "quăng" thùng hàng lên

    // Vẽ xe tải bằng hình học GDI+ (hình chữ nhật + hình tròn) thay vì dùng
    // emoji 🚚 — vì emoji nhiều màu qua TextRenderer/GDI render SAI trên nhiều
    // máy (hiện hình méo mó, không giống xe) do GDI cổ điển không hỗ trợ tốt
    // font emoji nhiều lớp màu. Vẽ tay đảm bảo ĐÚNG hình dạng ở MỌI máy, đồng
    // thời vẽ THẲNG luôn xe quay đầu sang phải — không cần bước lật ảnh nữa.
    private readonly PictureBox _picTruck = CreateTruckPictureBox(Theme.SidebarBg, 70, 40);
    private readonly Label _lblBox = new() { Text = "📦", Font = new Font("Segoe UI Emoji", 20), ForeColor = Color.White, AutoSize = true, BackColor = Color.Transparent, Visible = false };
    private readonly System.Windows.Forms.Timer _animTimer = new() { Interval = 30 };
    private int _tick = 0;

    /// <summary>Vẽ 1 chiếc xe tải đơn giản (thùng hàng + cabin + 2 bánh xe) quay
    /// đầu sang PHẢI — khớp đúng hướng chạy trái→phải, không phụ thuộc font/emoji.</summary>
    private static PictureBox CreateTruckPictureBox(Color bgColor, int width, int height)
    {
        var bmp = new Bitmap(width, height);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(bgColor);

            using var bodyBrush = new SolidBrush(Color.White);
            using var wheelBrush = new SolidBrush(ColorTranslator.FromHtml("#94A3B8"));
            using var windowBrush = new SolidBrush(ColorTranslator.FromHtml("#93C5FD"));

            // Thùng hàng (hình chữ nhật lớn, bên TRÁI — phần đuôi xe)
            g.FillRectangle(bodyBrush, 2, 6, 38, 20);
            // Cabin (hình chữ nhật nhỏ hơn, bên PHẢI — phần đầu xe, chỗ tài xế)
            g.FillRectangle(bodyBrush, 40, 12, 20, 14);
            // Kính lái (ô nhỏ màu xanh nhạt trong cabin)
            g.FillRectangle(windowBrush, 44, 15, 12, 7);
            // 2 bánh xe (hình tròn) — 1 dưới thùng hàng, 1 dưới cabin
            g.FillEllipse(wheelBrush, 6, 24, 12, 12);
            g.FillEllipse(wheelBrush, 44, 24, 12, 12);
        }

        return new PictureBox
        {
            Image = bmp,
            Width = width,
            Height = height,
            SizeMode = PictureBoxSizeMode.AutoSize,
            BackColor = bgColor,
        };
    }

    public LoginForm()
    {
        Text = "Trucking System — Đăng nhập";
        Width = 1000; Height = 640;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Theme.Primary;

        var bannerPanel = new Panel { Dock = DockStyle.Left, Width = BannerWidth, BackColor = Theme.SidebarBg };
        var lblAppName = new Label { Text = "Trucking System", Font = new Font("Segoe UI", 26, FontStyle.Bold), ForeColor = Color.White, Top = 200, Left = 40, AutoSize = true };
        var lblTagline = new Label
        {
            Text = "Hệ thống quản lý vận tải và\nkho trung chuyển hàng hóa nội địa",
            Font = new Font("Segoe UI", 11),
            ForeColor = Theme.SidebarText,
            Top = 255,
            Left = 40,
            AutoSize = true,
        };

        // Vạch kẻ đường đơn giản, ngay phía trên vị trí xe chạy — cho có cảm giác "con đường"
        var roadLine = new Panel { Top = RoadY + 46, Left = 0, Width = BannerWidth, Height = 2, BackColor = ColorTranslator.FromHtml("#334155") };

        bannerPanel.Controls.Add(lblAppName);
        bannerPanel.Controls.Add(lblTagline);
        bannerPanel.Controls.Add(roadLine);
        bannerPanel.Controls.Add(_lblBox);   // thêm thùng hàng TRƯỚC xe để xe "đè" lên khi cần
        bannerPanel.Controls.Add(_picTruck);

        _picTruck.Top = RoadY;
        _animTimer.Tick += AnimTimer_Tick;
        _animTimer.Start();
        FormClosed += (_, _) => _animTimer.Stop();

        var rightPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        var lblTitle = new Label { Text = "Đăng nhập", Font = Theme.FontTitle, ForeColor = Theme.TextPrimary, Top = 140, Left = 90, AutoSize = true };
        var lblSub = new Label { Text = "Nhập thông tin tài khoản để tiếp tục", Font = Theme.FontNormal, ForeColor = Theme.TextSecondary, Top = 180, Left = 90, AutoSize = true };

        StyleTextBox(_txtServerUrl);
        StyleTextBox(_txtUsername);
        StyleTextBox(_txtPassword);
        _txtServerUrl.Text = "https://localhost:49243";
        _txtUsername.Text = "admin";

        var p1 = Theme.MakeLabeledInput("SERVER URL", _txtServerUrl, 320); p1.Top = 220; p1.Left = 90;
        var p2 = Theme.MakeLabeledInput("TÀI KHOẢN", _txtUsername, 320); p2.Top = 290; p2.Left = 90;
        var p3 = Theme.MakeLabeledInput("MẬT KHẨU", _txtPassword, 320); p3.Top = 360; p3.Left = 90;

        _btnLogin = Theme.MakePrimaryButton("Đăng nhập", 320, 44);
        _btnLogin.Top = 440; _btnLogin.Left = 90;
        _btnLogin.Click += LoginClick;

        _lblStatus.Top = 495; _lblStatus.Left = 90;

        var btnClose = new Button
        {
            Text = "✕",
            Width = 36,
            Height = 36,
            Top = 10,
            Left = Width - 46,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Theme.TextSecondary,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
        };
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.Click += (_, _) => ExitRequested?.Invoke(this, EventArgs.Empty);

        rightPanel.Controls.Add(lblTitle);
        rightPanel.Controls.Add(lblSub);
        rightPanel.Controls.Add(p1);
        rightPanel.Controls.Add(p2);
        rightPanel.Controls.Add(p3);
        rightPanel.Controls.Add(_btnLogin);
        rightPanel.Controls.Add(_lblStatus);
        rightPanel.Controls.Add(btnClose);

        Controls.Add(rightPanel);
        Controls.Add(bannerPanel);

        AcceptButton = _btnLogin;
    }

    private static void StyleTextBox(TextBox tb)
    {
        tb.BorderStyle = BorderStyle.FixedSingle;
        tb.Font = Theme.FontNormal;
        tb.Height = 32;
    }

    /// <summary>
    /// Mỗi 30ms tính lại 1 khung hình:
    /// - Xe tải di chuyển đều từ mép trái (ngoài khung hình) sang mép phải, lặp vô hạn.
    /// - Trong ~40 tick đầu của MỖI vòng lặp (lúc xe vừa xuất phát), thùng hàng
    ///   bay lên theo quỹ đạo parabol (lên cao giữa chừng rồi rơi trở lại) — mô
    ///   phỏng cảnh "thùng hàng bị quăng lên" ngay khi xe bắt đầu chạy.
    /// </summary>
    private void AnimTimer_Tick(object? sender, EventArgs e)
    {
        _tick = (_tick + 1) % LoopTotalTicks;

        // Xe chạy từ ngoài mép trái (-90) tới ngoài mép phải (BannerWidth + 20)
        double truckProgress = (double)_tick / LoopTotalTicks;
        int truckX = (int)(-90 + truckProgress * (BannerWidth + 110));
        _picTruck.Left = truckX;

        if (_tick < TossTicks)
        {
            // t chạy từ 0 -> 1 trong giai đoạn quăng hàng
            double t = (double)_tick / TossTicks;
            // Parabol: 4*t*(1-t) = 0 tại t=0 và t=1, đạt đỉnh (=1) tại t=0.5
            int liftOffset = (int)(ArcHeight * 4 * t * (1 - t));

            // Xe đã lật hướng (đầu xe bên phải), nên thùng xe giờ nằm ở BÊN TRÁI
            // hình xe — đặt thùng hàng gần mép trái thay vì mép phải như trước.
            _lblBox.Left = truckX + 4;
            _lblBox.Top = RoadY - liftOffset - 6;  // bay lên cao rồi rơi trở lại đúng vị trí ban đầu
            _lblBox.Visible = true;
        }
        else
        {
            _lblBox.Visible = false; // hàng đã "rơi vào thùng xe", ẩn đi tới vòng lặp kế tiếp
        }
    }

    private async void LoginClick(object? sender, EventArgs e)
    {
        _lblStatus.Text = "";
        _btnLogin.Enabled = false;
        _btnLogin.Text = "Đang đăng nhập...";
        try
        {
            var api = new ApiServices(_txtServerUrl.Text.Trim());
            var result = await api.Auth.LoginAsync(_txtUsername.Text.Trim(), _txtPassword.Text);

            if (!result.Success)
            {
                _lblStatus.Text = "❌ " + (result.Message ?? "Đăng nhập thất bại.");
                return;
            }

            LoginSucceeded?.Invoke(api, _txtUsername.Text.Trim());
            return; // Không đóng gì cả ở đây — TruckingAppContext sẽ tự Hide() Form này khi cần
        }
        catch (HttpRequestException ex)
        {
            _lblStatus.Text = "❌ Không kết nối được server: " + ex.Message;
        }
        catch (Exception ex)
        {
            _lblStatus.Text = "❌ Lỗi: " + ex.Message;
        }
        finally
        {
            _btnLogin.Enabled = true;
            _btnLogin.Text = "Đăng nhập";
        }
    }
}