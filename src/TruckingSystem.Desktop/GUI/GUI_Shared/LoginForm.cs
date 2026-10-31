using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.GUI.GUI_ChatBot;
namespace TruckingSystem.Desktop.GUI_Shared
{
    /// <summary>Màn đăng nhập. Khung tĩnh (form đăng nhập bên phải) nằm ở LoginForm.Designer.cs.
    /// BANNER TRÁI (mẫu "Ban ngày"): bầu trời sáng, mặt trời, mây trôi, đồi cây, con đường có vạch vàng chạy,
    /// xe tải to (bánh quay, nhún nhẹ, nhả khói) chạy trái→phải và tung thùng hàng ở đầu mỗi vòng.
    /// Toàn bộ banner vẽ tay bằng GDI+ trong 1 lần Paint của bannerPanel (bật DoubleBuffered -> không nháy),
    /// không dùng hình ảnh / thư viện ngoài. Tọa độ vẽ theo khung chuẩn cao 640 rồi phóng theo kích thước thật
    /// (đúng ở mọi mức phóng to màn hình). Các nhãn chữ / truckCanvas / roadLine cũ ở Designer được ẩn
    /// (chữ tiêu đề vẽ lại trong cảnh để nền trong suốt không bị nháy).</summary>
    public partial class LoginForm : BaseForm
    {
        /// <summary>Phát ra khi đăng nhập thành công — TruckingAppContext lắng nghe để
        /// chuyển sang MainForm.</summary>
        public event Action<ApiServices, string>? LoginSucceeded;

        /// <summary>Phát ra khi người dùng bấm nút ✕ muốn thoát hẳn ứng dụng.</summary>
        public event EventHandler? ExitRequested;

        // ---- Cảnh động trên banner ----
        private const float SceneH = 640f;       // chiều cao khung chuẩn (đơn vị vẽ)
        private const double TruckLoopSec = 6.0;  // xe chạy hết 1 vòng
        private const double CloudLoopSec = 40.0; // mây trôi hết 1 bề ngang
        private const float RoadTop = 480f, RoadBottom = 560f, TruckBaseY = 482f;

        private readonly System.Windows.Forms.Timer _animTimer = new() { Interval = 30 };
        private readonly System.Diagnostics.Stopwatch _clock = System.Diagnostics.Stopwatch.StartNew();

        private static readonly Color SkyTop = ColorTranslator.FromHtml("#7DD3FC");
        private static readonly Color SkyBottom = ColorTranslator.FromHtml("#E0F2FE");
        private static readonly Color Sun = ColorTranslator.FromHtml("#FDE047");
        private static readonly Color HillFar = ColorTranslator.FromHtml("#A7F3D0");
        private static readonly Color HillNear = ColorTranslator.FromHtml("#6EE7B7");
        private static readonly Color TreeLeaf = ColorTranslator.FromHtml("#10B981");
        private static readonly Color TreeTrunk = ColorTranslator.FromHtml("#92400E");
        private static readonly Color Curb = ColorTranslator.FromHtml("#CBD5E1");
        private static readonly Color RoadTopColor = ColorTranslator.FromHtml("#475569");
        private static readonly Color RoadBottomColor = ColorTranslator.FromHtml("#334155");
        private static readonly Color LaneMark = ColorTranslator.FromHtml("#FACC15");
        private static readonly Color Grass = ColorTranslator.FromHtml("#86EFAC");
        private static readonly Color TitleColor = ColorTranslator.FromHtml("#0F172A");
        private static readonly Color TaglineColor = ColorTranslator.FromHtml("#334155");
        private static readonly Font TitleFont = new("Segoe UI", 34f, FontStyle.Bold, GraphicsUnit.Pixel);
        private static readonly Font TaglineFont = new("Segoe UI", 15f, FontStyle.Regular, GraphicsUnit.Pixel);
        private static readonly Font TruckLogoFont = new("Segoe UI", 13f, FontStyle.Bold, GraphicsUnit.Pixel);

        public LoginForm()
        {
            InitializeComponent();
            // Tên ứng dụng (đặt ở code — không sửa Designer).
            Text = "Logistics System — Đăng nhập";
            lblAppName.Text = "Logistics System";
            SetupBannerScene();
            txtServerUrl.Text = "https://trucking.logisticssystem.dpdns.org";
            txtUsername.Text = "admin";
        }

        private void SetupBannerScene()
        {
            // Ẩn các khối cũ của banner (nền xanh đậm + xe nhỏ) — cảnh mới vẽ trọn trên bannerPanel.
            lblAppName.Visible = lblTagline.Visible = truckCanvas.Visible = roadLine.Visible = false;
            bannerPanel.BackColor = SkyBottom;
            typeof(Control).GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(bannerPanel, true);
            bannerPanel.Paint += (_, e) => PaintScene(e.Graphics, bannerPanel.ClientSize);
            bannerPanel.Resize += (_, _) => bannerPanel.Invalidate();

            _animTimer.Tick += (_, _) => bannerPanel.Invalidate();
            _animTimer.Start();
            FormClosed += (_, _) => { _animTimer.Stop(); _sceneCache?.Dispose(); _sceneCache = null; };
            // Thu nhỏ cửa sổ xuống thanh tác vụ thì ngừng vẽ, mở lại thì chạy tiếp.
            Resize += (_, _) => _animTimer.Enabled = WindowState != FormWindowState.Minimized && Visible;
            VisibleChanged += (_, _) => _animTimer.Enabled = Visible && WindowState != FormWindowState.Minimized;
        }

        // ================= Vẽ cảnh =================

        // Phần ĐỨNG YÊN của cảnh (trời, mặt trời, đồi, cây, mặt đường, chữ) vẽ 1 lần vào ảnh đệm, chỉ vẽ lại
        // khi đổi kích thước; mỗi khung hình chỉ dán ảnh này rồi vẽ phần ĐỘNG (mây, vạch đường, xe) -> nhẹ máy.
        private Bitmap? _sceneCache;

        private void PaintScene(Graphics g, Size size)
        {
            if (size.Width <= 0 || size.Height <= 0) return;
            var scale = size.Height / SceneH;
            var w = size.Width / scale; // bề ngang theo đơn vị vẽ

            if (_sceneCache is null || _sceneCache.Size != size)
            {
                _sceneCache?.Dispose();
                _sceneCache = new Bitmap(size.Width, size.Height);
                using var cg = Graphics.FromImage(_sceneCache);
                cg.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                cg.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                cg.ScaleTransform(scale, scale);
                PaintStatic(cg, w);
            }
            g.DrawImageUnscaled(_sceneCache, 0, 0);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.ScaleTransform(scale, scale);
            var t = _clock.Elapsed.TotalSeconds;

            // Mây trôi sang trái, lặp vô hạn (nằm giữa chữ và đồi nên không đè lên phần tĩnh nào)
            var cloudShift = (float)(t / CloudLoopSec % 1.0) * w;
            using (var cloud = new SolidBrush(Color.FromArgb(230, Color.White)))
                for (var k = 0; k < 2; k++)
                {
                    var ox = -cloudShift + k * w;
                    DrawCloud(g, cloud, ox + w * 0.18f, 250, 1f);
                    DrawCloud(g, cloud, ox + w * 0.66f, 200, 0.78f);
                }

            // Vạch vàng chạy
            var laneShift = (float)(t / 0.9 % 1.0) * 50f;
            using (var lane = new SolidBrush(LaneMark))
                for (var x = -50f - laneShift; x < w; x += 50f)
                    FillRound(g, lane, x, 517.5f, 28, 5, 2.5f);

            // Xe tải + thùng hàng
            var phase = t / TruckLoopSec % 1.0;
            var truckX = (float)(-200 + phase * (w + 260));
            DrawTossedBox(g, truckX, phase);
            DrawTruck(g, truckX, TruckBaseY - (float)(Math.Abs(Math.Sin(t * Math.PI / 0.45)) * 2), t);
        }

        private void PaintStatic(Graphics g, float w)
        {
            using (var sky = new System.Drawing.Drawing2D.LinearGradientBrush(new RectangleF(0, 0, w, RoadTop), SkyTop, SkyBottom, 90f))
                g.FillRectangle(sky, 0, 0, w, RoadTop);

            var sun = new PointF(w - 70, 92);
            using (var glow = new SolidBrush(Color.FromArgb(64, Sun))) g.FillEllipse(glow, sun.X - 54, sun.Y - 54, 108, 108);
            using (var core = new SolidBrush(Sun)) g.FillEllipse(core, sun.X - 34, sun.Y - 34, 68, 68);

            var sx = w / 460f;
            using (var far = new SolidBrush(HillFar))
            using (var farPath = Hill(sx, new[] { (0f, 420f), (80f, 340f), (170f, 390f), (260f, 440f), (340f, 370f), (420f, 300f), (460f, 380f) }))
                g.FillPath(far, farPath);
            using (var near = new SolidBrush(HillNear))
            using (var nearPath = Hill(sx, new[] { (0f, 450f), (110f, 400f), (220f, 440f), (330f, 480f), (460f, 425f) }))
                g.FillPath(near, nearPath);

            DrawTree(g, 60 * sx, 438, 14);
            DrawTree(g, 390 * sx, 428, 16);
            DrawTree(g, 415 * sx, 436, 11);

            using (var curb = new SolidBrush(Curb)) g.FillRectangle(curb, 0, RoadTop - 10, w, 10);
            using (var road = new System.Drawing.Drawing2D.LinearGradientBrush(new RectangleF(0, RoadTop, w, RoadBottom - RoadTop), RoadTopColor, RoadBottomColor, 90f))
                g.FillRectangle(road, 0, RoadTop, w, RoadBottom - RoadTop);
            using (var grass = new SolidBrush(Grass)) g.FillRectangle(grass, 0, RoadBottom, w, SceneH - RoadBottom + 2);

            using (var title = new SolidBrush(TitleColor)) g.DrawString(lblAppName.Text, TitleFont, title, 38, 72);
            using (var tag = new SolidBrush(TaglineColor)) g.DrawString(lblTagline.Text, TaglineFont, tag, 41, 124);
        }

        private static void DrawCloud(Graphics g, Brush b, float x, float y, float k)
        {
            g.FillEllipse(b, x - 38 * k, y - 14 * k, 76 * k, 28 * k);
            g.FillEllipse(b, x - 2 * k, y - 26 * k, 48 * k, 32 * k);
        }

        /// <summary>Đồi: chuỗi đường cong bậc 2 (điểm đầu, [điều khiển, điểm cuối]...) rồi khép xuống đáy cảnh.</summary>
        private static System.Drawing.Drawing2D.GraphicsPath Hill(float sx, (float X, float Y)[] pts)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            PointF P((float X, float Y) p) => new(p.X * sx, p.Y);
            for (var i = 0; i + 2 < pts.Length; i += 2)
            {
                var p0 = P(pts[i]); var q = P(pts[i + 1]); var p2 = P(pts[i + 2]);
                // bậc 2 -> bậc 3 (GDI+ chỉ có Bezier bậc 3)
                var c1 = new PointF(p0.X + 2f / 3 * (q.X - p0.X), p0.Y + 2f / 3 * (q.Y - p0.Y));
                var c2 = new PointF(p2.X + 2f / 3 * (q.X - p2.X), p2.Y + 2f / 3 * (q.Y - p2.Y));
                path.AddBezier(p0, c1, c2, p2);
            }
            var last = P(pts[^1]);
            path.AddLine(last, new PointF(last.X, SceneH));
            path.AddLine(new PointF(last.X, SceneH), new PointF(0, SceneH));
            path.CloseFigure();
            return path;
        }

        private static void DrawTree(Graphics g, float x, float y, float r)
        {
            using var trunk = new SolidBrush(TreeTrunk);
            g.FillRectangle(trunk, x - 2, y + r * 0.55f, 4, r * 0.85f);
            using var leaf = new SolidBrush(TreeLeaf);
            g.FillEllipse(leaf, x - r, y - r, r * 2, r * 2);
        }

        private static void FillRound(Graphics g, Brush b, float x, float y, float w, float h, float r)
        {
            using var path = new System.Drawing.Drawing2D.GraphicsPath();
            var d = r * 2;
            path.AddArc(x, y, d, d, 180, 90);
            path.AddArc(x + w - d, y, d, d, 270, 90);
            path.AddArc(x + w - d, y + h - d, d, d, 0, 90);
            path.AddArc(x, y + h - d, d, d, 90, 90);
            path.CloseFigure();
            g.FillPath(b, path);
        }

        /// <summary>Thùng hàng tung lên theo đường parabol trong 24% đầu mỗi vòng chạy.</summary>
        private static void DrawTossedBox(Graphics g, float truckX, double phase)
        {
            if (phase > 0.24) return;
            var k = (float)(phase / 0.24);
            var x = truckX + 20 + 80 * k;
            var y = TruckBaseY - 280 * k * (1 - k);
            using var box = new SolidBrush(ColorTranslator.FromHtml("#F59E0B"));
            using var tape = new SolidBrush(ColorTranslator.FromHtml("#FCD34D"));
            using var band = new SolidBrush(ColorTranslator.FromHtml("#D97706"));
            FillRound(g, box, x, y, 26, 22, 3);
            g.FillRectangle(tape, x + 11, y, 4, 22);
            g.FillRectangle(band, x, y + 8, 26, 4);
        }

        /// <summary>Xe tải quay đầu sang phải: thùng trắng có chữ + viền cam, cabin xanh có kính, đèn vàng,
        /// 3 bánh quay (có nan để thấy quay), bóng dưới gầm và khói phía sau.</summary>
        internal static void DrawTruck(Graphics g, float x, float y, double t, bool smoke = true)
        {
            var state = g.Save();
            g.TranslateTransform(x, y);

            // Khói: 3 cụm lệch pha, bay lên sau xe rồi tan
            for (var i = 0; smoke && i < 3; i++)
            {
                var p = (float)((t / 1.2 + i / 3.0) % 1.0);
                var r = 7 * (0.6f + p);
                using var puff = new SolidBrush(Color.FromArgb((int)(180 * (1 - p)), 226, 232, 240));
                g.FillEllipse(puff, -6 - 30 * p - r, 52 - 16 * p - r, r * 2, r * 2);
            }

            using (var shadow = new SolidBrush(Color.FromArgb(64, 15, 23, 42))) g.FillEllipse(shadow, 0, 65, 144, 10);

            using (var white = new SolidBrush(Color.White)) FillRound(g, white, 0, 0, 98, 52, 6);
            using (var orange = new SolidBrush(ColorTranslator.FromHtml("#F97316"))) g.FillRectangle(orange, 0, 40, 98, 12);
            using (var logo = new SolidBrush(ColorTranslator.FromHtml("#2563EB")))
            using (var center = new StringFormat { Alignment = StringAlignment.Center })
                g.DrawString("LOGISTICS", TruckLogoFont, logo, 49, 13, center);

            using (var cab = new System.Drawing.Drawing2D.GraphicsPath())
            {
                cab.AddLine(100, 14, 126, 14);
                cab.AddBezier(126, 14, 131, 14, 136, 18, 138, 22);
                cab.AddLine(138, 22, 146, 36);
                cab.AddLine(146, 36, 146, 52);
                cab.AddLine(146, 52, 100, 52);
                cab.CloseFigure();
                using var blue = new SolidBrush(ColorTranslator.FromHtml("#2563EB"));
                g.FillPath(blue, cab);
            }
            using (var glass = new SolidBrush(ColorTranslator.FromHtml("#BAE6FD")))
                g.FillPolygon(glass, new PointF[] { new(106, 19), new(124, 19), new(130, 23), new(136, 34), new(106, 34) });
            using (var lamp = new SolidBrush(ColorTranslator.FromHtml("#FDE047"))) FillRound(g, lamp, 140, 42, 7, 5, 2);
            using (var bumper = new SolidBrush(ColorTranslator.FromHtml("#1E3A8A"))) FillRound(g, bumper, 96, 50, 52, 6, 2);

            var angle = (float)(t / 0.6 % 1.0 * 360);
            foreach (var cx in new[] { 24f, 50f, 124f }) DrawWheel(g, cx, 58, angle);
            g.Restore(state);
        }

        private static void DrawWheel(Graphics g, float cx, float cy, float angle)
        {
            using var tire = new SolidBrush(ColorTranslator.FromHtml("#1E293B"));
            using var hub = new SolidBrush(ColorTranslator.FromHtml("#CBD5E1"));
            using var spoke = new Pen(ColorTranslator.FromHtml("#64748B"), 2f);
            g.FillEllipse(tire, cx - 11, cy - 11, 22, 22);
            var rad = angle * Math.PI / 180;
            var dx = (float)(Math.Sin(rad) * 9); var dy = (float)(Math.Cos(rad) * 9);
            g.DrawLine(spoke, cx - dx, cy - dy, cx + dx, cy + dy);
            g.FillEllipse(hub, cx - 5, cy - 5, 10, 10);
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
