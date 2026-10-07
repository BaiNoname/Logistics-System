using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.GUI.GUI_ChatBot;
namespace TruckingSystem.Desktop.GUI_Shared
{
    /// <summary>Logic — đăng nhập + animation xe tải chạy trên banner. Khung tĩnh
    /// (banner, form đăng nhập) nằm ở LoginForm.Designer.cs. Riêng xe tải (vẽ tay
    /// bằng GDI+) và animation (Timer) vẫn ở code vì bản chất là nội dung ĐỘNG,
    /// Designer không vẽ hoạt ảnh được.</summary>
    public partial class LoginForm : BaseForm
    {
        /// <summary>Phát ra khi đăng nhập thành công — TruckingAppContext lắng nghe để
        /// chuyển sang MainForm.</summary>
        public event Action<ApiServices, string>? LoginSucceeded;

        /// <summary>Phát ra khi người dùng bấm nút ✕ muốn thoát hẳn ứng dụng.</summary>
        public event EventHandler? ExitRequested;

        // ---- Animation xe tải chạy + quăng thùng hàng ----
        private const int BannerWidth = 460;
        private const int RoadY = 460;
        private const int LoopTotalTicks = 150;
        private const int TossTicks = 40;
        private const int ArcHeight = 55;

        private PictureBox? _picTruck;
        private Label? _lblBox;
        private readonly System.Windows.Forms.Timer _animTimer = new() { Interval = 30 };
        private int _tick = 0;

        public LoginForm()
        {
            InitializeComponent();
            SetupTruckAnimation();
            txtServerUrl.Text = "https://trucking.logisticssystem.dpdns.org";
            txtUsername.Text = "admin";
        }

        /// <summary>Vẽ xe tải bằng hình học GDI+ (không dùng emoji — emoji nhiều màu
        /// qua GDI render sai trên nhiều máy) và khởi động Timer animation. Tách
        /// riêng khỏi InitializeComponent vì đây là nội dung ĐỘNG, không phải khai
        /// báo giao diện tĩnh.</summary>
        private void SetupTruckAnimation()
        {
            _picTruck = CreateTruckPictureBox(Theme.SidebarBg, 70, 40);
            _picTruck.Top = RoadY;
            _lblBox = new Label { Text = "📦", Font = new Font("Segoe UI Emoji", 20), ForeColor = Color.White, AutoSize = true, BackColor = Color.Transparent, Visible = false };

            bannerPanel.Controls.Add(_lblBox); // thêm thùng hàng TRƯỚC xe để xe "đè" lên khi cần
            bannerPanel.Controls.Add(_picTruck);

            _animTimer.Tick += AnimTimer_Tick;
            _animTimer.Start();
            FormClosed += (_, _) => _animTimer.Stop();
        }

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

                g.FillRectangle(bodyBrush, 2, 6, 38, 20);
                g.FillRectangle(bodyBrush, 40, 12, 20, 14);
                g.FillRectangle(windowBrush, 44, 15, 12, 7);
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

        /// <summary>Mỗi 30ms tính lại 1 khung hình: xe chạy trái→phải lặp vô hạn,
        /// thùng hàng bay theo quỹ đạo parabol trong ~40 tick đầu mỗi vòng.</summary>
        private void AnimTimer_Tick(object? sender, EventArgs e)
        {
            if (_picTruck is null || _lblBox is null) return;

            _tick = (_tick + 1) % LoopTotalTicks;

            double truckProgress = (double)_tick / LoopTotalTicks;
            int truckX = (int)(-90 + truckProgress * (BannerWidth + 110));
            _picTruck.Left = truckX;

            if (_tick < TossTicks)
            {
                double t = (double)_tick / TossTicks;
                int liftOffset = (int)(ArcHeight * 4 * t * (1 - t));

                _lblBox.Left = truckX + 4;
                _lblBox.Top = RoadY - liftOffset - 6;
                _lblBox.Visible = true;
            }
            else
            {
                _lblBox.Visible = false;
            }
        }

        private void BtnClose_Click(object? sender, EventArgs e) => ExitRequested?.Invoke(this, EventArgs.Empty);

        private async void BtnLogin_Click(object? sender, EventArgs e)
        {
            lblStatus.Text = "";
            if (string.IsNullOrWhiteSpace(txtServerUrl.Text) ||
                string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                lblStatus.Text = "❌ Vui lòng nhập đầy đủ máy chủ, tài khoản và mật khẩu.";
                return;
            }

            btnLogin.Enabled = false;
            btnLogin.Text = "Đang đăng nhập...";
            try
            {
                var api = new ApiServices(txtServerUrl.Text.Trim());
                var result = await api.Auth.LoginAsync(txtUsername.Text.Trim(), txtPassword.Text);

                if (!result.Success)
                {
                    lblStatus.Text = "❌ " + (result.Message ?? "Đăng nhập thất bại.");
                    return;
                }

                if (!api.Connection.IsInRole("Admin", "Dispatcher", "Accountant", "WarehouseStaff"))
                {
                    api.Connection.ClearToken();
                    lblStatus.Text = "❌ Tài khoản này không có quyền sử dụng ứng dụng Desktop.";
                    return;
                }

                LoginSucceeded?.Invoke(api, txtUsername.Text.Trim());
                return;
            }
            catch (HttpRequestException ex)
            {
                lblStatus.Text = "❌ Không kết nối được server: " + ex.Message;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "❌ Lỗi: " + ex.Message;
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text = "Đăng nhập";
            }
        }

        private void btnOpenChatBot_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Bạn có muốn mở ChatBot hỗ trợ không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    var chatBot = new ChatBotForm();
                    chatBot.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this,
                        "Không thể mở ChatBot vì thiếu thông tin hoặc có lỗi xảy ra:\n" + ex.Message,
                        "Lỗi ChatBot",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }
    }
}
