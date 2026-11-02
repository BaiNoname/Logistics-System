using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.GUI.GUI_ChatBot;
namespace TruckingSystem.Desktop.GUI_Shared
{
    /// <summary>Màn đăng nhập. Khung tĩnh (form đăng nhập bên phải) nằm ở LoginForm.Designer.cs.
    /// BANNER TRÁI = hoạt cảnh "1 NGÀY GIAO HÀNG" (lặp ~20 giây), kể đúng mục tiêu hệ thống:
    ///   🌅 sáng sớm xe nhận hàng xuất phát → ☀️ trưa chạy đường dài (đồi, cây, mây trôi về sau) → 🌇 chiều hoàng hôn
    ///   → 🌙 tối bật đèn pha, kho hiện ra, xe dừng trước kho, bốc 3 thùng hàng vào kho → 👍 khách hàng giơ ngón cái
    ///   "Tuyệt vời!" → trời sáng lại. Dòng chữ nhỏ dưới tiêu đề đổi theo từng chặng.
    /// Vẽ tay bằng GDI+ trong 1 lần Paint của bannerPanel (DoubleBuffered -> không nháy); hình xe / kho / khách hàng ở
    /// DayScene (dùng chung với logo màn chính). Tọa độ theo khung chuẩn cao 640, phóng theo kích thước thật.
    /// Hoạt cảnh tự dừng khi cửa sổ ẩn / thu nhỏ (sau khi đăng nhập màn này chỉ bị ẩn chứ không đóng).</summary>
    public partial class LoginForm : BaseForm
    {
        /// <summary>Phát ra khi đăng nhập thành công — TruckingAppContext lắng nghe để
        /// chuyển sang MainForm.</summary>
        public event Action<ApiServices, string>? LoginSucceeded;

        /// <summary>Phát ra khi người dùng bấm nút ✕ muốn thoát hẳn ứng dụng.</summary>
        public event EventHandler? ExitRequested;

        // ---- Hoạt cảnh trên banner ----
        private const float SceneH = 640f;      // chiều cao khung chuẩn (đơn vị vẽ)
        private const double CycleSec = 20.0;   // 1 vòng "1 ngày"
        private const float Road = 480f;
        private const double DriveEnd = 0.74;   // tới đây xe thôi chạy tại chỗ (đã tới kho)

        private readonly System.Windows.Forms.Timer _animTimer = new() { Interval = 33 };
        private readonly System.Diagnostics.Stopwatch _clock = System.Diagnostics.Stopwatch.StartNew();
        private double _lastSec;
        private float _scroll; // quãng đường cảnh vật đã trôi (đơn vị vẽ)

        private static Color H(string h) => DayScene.Hex(h);

        // Màu trời (trên, dưới) theo thời điểm trong vòng: bình minh -> trưa -> chiều -> hoàng hôn -> tối -> sáng lại.
        private static readonly (double T, Color Top, Color Bottom)[] Sky =
        {
            (0.00, H("#F9A8D4"), H("#FDE68A")), (0.22, H("#38BDF8"), H("#E0F2FE")), (0.42, H("#60A5FA"), H("#FDE68A")),
            (0.55, H("#F97316"), H("#FDE68A")), (0.66, H("#6D28D9"), H("#F472B6")), (0.76, H("#0F172A"), H("#1E3A8A")),
            (0.96, H("#0F172A"), H("#1E3A8A")), (1.00, H("#F9A8D4"), H("#FDE68A")),
        };
        private static readonly (double T, float V)[] NightKeys = { (0, 0f), (0.55, 0f), (0.72, 1f), (0.95, 1f), (1, 0f) };

        // Chữ chặng (không dùng emoji — GDI+ vẽ emoji không ổn định; thay bằng chấm màu vẽ tay phía trước).
        private static readonly (double T, string Text, Color Dot)[] Captions =
        {
            (0.00, "Sáng sớm · Nhận hàng, xuất phát", H("#FB923C")), (0.20, "Buổi trưa · Vận chuyển an toàn", H("#FACC15")),
            (0.42, "Buổi chiều · Đúng lộ trình, đúng hạn", H("#F97316")), (0.66, "Buổi tối · Giao tận kho", H("#A5B4FC")),
            (0.84, "Khách hàng hài lòng", H("#22C55E")),
        };

        private static readonly Font TitleFont = new("Segoe UI", 34f, FontStyle.Bold, GraphicsUnit.Pixel);
        private static readonly Font TaglineFont = new("Segoe UI", 15f, FontStyle.Regular, GraphicsUnit.Pixel);
        private static readonly Font CaptionFont = new("Segoe UI", 13f, FontStyle.Bold, GraphicsUnit.Pixel);

        public LoginForm()
        {
            InitializeComponent();
            // Tên ứng dụng (đặt ở code — không sửa Designer).
            Text = "Logistics System — Đăng nhập";
            lblAppName.Text = "Logistics System";
            SetupBannerScene();
            SetupRightPanel();
            txtServerUrl.Text = "https://trucking.logisticssystem.dpdns.org";
            txtUsername.Text = "admin";
        }

        private void SetupBannerScene()
        {
            // Ẩn các khối cũ của banner — cảnh mới vẽ trọn trên bannerPanel.
            lblAppName.Visible = lblTagline.Visible = truckCanvas.Visible = roadLine.Visible = false;
            bannerPanel.BackColor = H("#E0F2FE");
            typeof(Control).GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(bannerPanel, true);
            bannerPanel.Paint += (_, e) => PaintScene(e.Graphics, bannerPanel.ClientSize);
            bannerPanel.Resize += (_, _) => bannerPanel.Invalidate();

            _animTimer.Tick += (_, _) =>
            {
                // Cảnh vật trôi theo thời gian thật khi xe đang chạy (dừng trôi lúc xe đã tới kho).
                var now = _clock.Elapsed.TotalSeconds;
                var dt = Math.Min(0.2, now - _lastSec);
                _lastSec = now;
                if (now / CycleSec % 1.0 < DriveEnd) _scroll += (float)(dt * 120);
                bannerPanel.Invalidate();
            };
            _animTimer.Start();
            FormClosed += (_, _) => _animTimer.Stop();
            // Thu nhỏ / ẩn cửa sổ thì ngừng vẽ, hiện lại thì chạy tiếp.
            Resize += (_, _) => _animTimer.Enabled = WindowState != FormWindowState.Minimized && Visible;
            VisibleChanged += (_, _) => _animTimer.Enabled = Visible && WindowState != FormWindowState.Minimized;
        }

        // ================= Nửa phải: nền trời nhạt theo giờ + thẻ đăng nhập =================

        private int _rightTick;

        /// <summary>Nửa phải hài hòa với cảnh bên trái: nền là bầu trời NHẠT đổi màu theo đúng giờ của hoạt cảnh (hồng sáng
        /// sớm → xanh trưa → cam chiều → xanh đêm), mây mờ, đáy có tuyến đường nét đứt nối 3 điểm giao; form đăng nhập nằm
        /// trong THẺ TRẮNG bo góc có bóng đổ (vẽ phía sau loginBox), có biểu tượng xe, ô nhập nền xám nhạt, nút có bóng.
        /// Các ô nhập / nút có sẵn ở Designer giữ nguyên (chỉ đổi màu, chữ) — không ảnh hưởng chức năng đăng nhập.</summary>
        private void SetupRightPanel()
        {
            int S(int v) => LogicalToDeviceUnits(v);
            typeof(Control).GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(rightPanel, true);
            rightPanel.Paint += (_, e) => PaintRight(e.Graphics);
            rightPanel.Resize += (_, _) => rightPanel.Invalidate();
            btnClose.BackColor = Color.Transparent;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 148, 163, 184);

            // Thẻ: nền trắng, lề trong do thẻ vẽ rộng hơn loginBox.
            loginBox.BackColor = Color.White;
            lblSub.Text = "Nhập tài khoản để bắt đầu ca làm việc.";
            foreach (var tb in new[] { txtServerUrl, txtUsername, txtPassword }) tb.BackColor = H("#F8FAFC");
            txtServerUrl.Margin = txtUsername.Margin = new Padding(0, 0, 0, S(14));
            txtPassword.Margin = new Padding(0, 0, 0, S(22));
            btnLogin.Text = "Đăng nhập  →";
            lblStatus.MinimumSize = new Size(0, S(22));

            // Biểu tượng xe (ô vuông bo góc xanh, xe trắng) phía trên chữ "Đăng nhập".
            var badge = new Panel { Size = new Size(S(46), S(46)), Margin = new Padding(0, 0, 0, S(10)), BackColor = Color.White };
            badge.Paint += (_, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = DayScene.Round(0, 0, badge.Width - 1, badge.Height - 1, S(12)))
                using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle(0, 0, badge.Width, badge.Height), H("#2563EB"), H("#38BDF8"), 45f))
                    g.FillPath(br, path);
                var k = badge.Width / 46f;
                DayScene.FillRound(g, Color.White, 10 * k, 16 * k, 17 * k, 11 * k, 2 * k);
                using (var cab = new SolidBrush(Color.White))
                    g.FillPolygon(cab, new PointF[] { new(28 * k, 19 * k), new(33 * k, 19 * k), new(36 * k, 23 * k), new(36 * k, 27 * k), new(28 * k, 27 * k) });
                DayScene.FillCircle(g, H("#1E3A8A"), 14 * k, 29 * k, 2.6f * k);
                DayScene.FillCircle(g, H("#1E3A8A"), 31 * k, 29 * k, 2.6f * k);
            };
            TruckingSystem.Desktop.Common.KpiStrip.InsertRow(loginBox, badge, 0);

            // Dòng chữ nhỏ cuối thẻ.
            var foot = new Label
            {
                Text = "© Logistics System · Đồ án tốt nghiệp",
                AutoSize = true,
                Anchor = AnchorStyles.None,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = H("#94A3B8"),
                Margin = new Padding(0, S(4), 0, 0),
            };
            TruckingSystem.Desktop.Common.KpiStrip.InsertRow(loginBox, foot, loginBox.RowCount);

            // Bóng xanh nhẹ dưới nút Đăng nhập.
            loginBox.Paint += (_, e) =>
            {
                var r = btnLogin.Bounds;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                for (var i = 1; i <= 4; i++)
                    DayScene.FillRound(e.Graphics, Color.FromArgb(18, 37, 99, 235), r.X + 4 - i, r.Y + 3 + i, r.Width - 8 + 2 * i, r.Height, S(8));
            };

            // Nền đổi màu chậm -> chỉ vẽ lại nửa phải ~3 lần / giây (nhẹ máy), vẽ lại cả nút ✕ / ChatBot nằm trên nền.
            _animTimer.Tick += (_, _) =>
            {
                if (++_rightTick % 10 != 0) return;
                rightPanel.Invalidate(false);
                btnClose.Invalidate();
            };
        }

        private void PaintRight(Graphics g)
        {
            var size = rightPanel.ClientSize;
            if (size.Width <= 0 || size.Height <= 0) return;
            int S(int v) => LogicalToDeviceUnits(v);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            var t = _clock.Elapsed.TotalSeconds / CycleSec % 1.0;
            var night = DayScene.Keyed(NightKeys, t);
            var (top, bottom) = DayScene.KeyedSky(Sky, t);
            Color Soft(Color c, float dayK, float nightK) =>
                DayScene.Mix(DayScene.Mix(c, Color.White, dayK), DayScene.Mix(c, H("#1E293B"), nightK), night);
            var rect = new Rectangle(0, 0, size.Width, size.Height);
            using (var bg = new System.Drawing.Drawing2D.LinearGradientBrush(rect, Soft(top, 0.72f, 0.3f), Soft(bottom, 0.85f, 0.2f), 90f))
                g.FillRectangle(bg, rect);

            // Mây mờ (ban ngày)
            var cloud = Color.FromArgb((int)(150 * (1 - night)), Color.White);
            if (cloud.A > 0)
            {
                var st = g.Save();
                g.ScaleTransform(S(100) / 100f, S(100) / 100f);
                DayScene.Cloud(g, cloud, 90, 90);
                DayScene.Cloud(g, cloud, size.Width * 100f / S(100) - 90, 150);
                g.Restore(st);
            }

            // Tuyến đường nét chấm nối 3 điểm giao ở đáy
            var y = size.Height - S(30);
            var w = size.Width;
            var routeColor = DayScene.Mix(H("#93C5FD"), H("#64748B"), night);
            using (var pen = new Pen(routeColor, S(3))
            {
                DashPattern = new[] { 0.1f, 3.5f },
                DashCap = System.Drawing.Drawing2D.DashCap.Round,
                StartCap = System.Drawing.Drawing2D.LineCap.Round,
                EndCap = System.Drawing.Drawing2D.LineCap.Round
            })
                g.DrawBezier(pen, -10, y, w * 0.3f, y - S(24), w * 0.6f, y + S(22), w + 10, y - S(8));
            var pin = DayScene.Mix(H("#3B82F6"), H("#A5B4FC"), night);
            foreach (var px in new[] { 0.08f, 0.5f, 0.78f })
            {
                var cx = w * px;
                var cy = BezierY(px, y, y - S(24), y + S(22), y - S(8));
                DrawPin(g, pin, cx, cy, S(1));
            }

            // Thẻ trắng bo góc + bóng đổ, nằm sau loginBox (rộng hơn loginBox 1 khoảng lề)
            var card = loginBox.Bounds;
            card.Inflate(S(28), S(24));
            for (var i = 1; i <= 6; i++)
                DayScene.FillRound(g, Color.FromArgb(night > 0.5f ? 22 : 10, 15, 23, 42), card.X - i, card.Y - i + S(8), card.Width + 2 * i, card.Height + 2 * i, S(16) + i);
            DayScene.FillRound(g, Color.White, card.X, card.Y, card.Width, card.Height, S(16));
            using (var border = DayScene.Round(card.X, card.Y, card.Width, card.Height, S(16)))
            using (var bp = new Pen(H("#E2E8F0")))
                g.DrawPath(bp, border);
        }

        /// <summary>Tung độ của đường cong Bezier tại t (đúng tại các mốc 0 / 1, gần đúng ở giữa) — để ghim nằm trên đường.</summary>
        private static float BezierY(float t, float y0, float y1, float y2, float y3)
        {
            var u = 1 - t;
            return u * u * u * y0 + 3 * u * u * t * y1 + 3 * u * t * t * y2 + t * t * t * y3;
        }

        private static void DrawPin(Graphics g, Color c, float x, float y, float k)
        {
            using var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddBezier(x, y, x - 10 * k, y - 14 * k, x - 10 * k, y - 26 * k, x, y - 26 * k);
            path.AddBezier(x, y - 26 * k, x + 10 * k, y - 26 * k, x + 10 * k, y - 14 * k, x, y);
            path.CloseFigure();
            using (var b = new SolidBrush(c)) g.FillPath(b, path);
            DayScene.FillCircle(g, Color.White, x, y - 17 * k, 4 * k);
        }

        // ================= Vẽ cảnh =================

        private static float Wrap(float v, float m) => ((v % m) + m) % m;

        private void PaintScene(Graphics g, Size size)
        {
            if (size.Width <= 0 || size.Height <= 0) return;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            var scale = size.Height / SceneH;
            var w = size.Width / scale;
            g.ScaleTransform(scale, scale);

            var time = _clock.Elapsed.TotalSeconds;
            var t = time / CycleSec % 1.0;
            var night = DayScene.Keyed(NightKeys, t);
            var (top, bottom) = DayScene.KeyedSky(Sky, t);
            Color N(string day, string dark) => DayScene.Mix(H(day), H(dark), night);

            // Trời
            using (var sky = new System.Drawing.Drawing2D.LinearGradientBrush(new RectangleF(0, 0, w, Road), top, bottom, 90f))
                g.FillRectangle(sky, 0, 0, w, Road);

            // Sao
            if (night > 0)
                using (var star = new SolidBrush(Color.FromArgb((int)(255 * night), Color.White)))
                    for (var i = 0; i < 40; i++)
                    {
                        var s = i % 3 == 0 ? 2.5f : 1.5f;
                        g.FillRectangle(star, (i * 97) % w, (i * 53) % 300, s, s);
                    }

            // Mặt trời: mọc bên trái -> lên đỉnh buổi trưa -> lặn bên phải
            var sp = t / 0.64;
            if (sp < 1)
            {
                var sx = 30 + (w - 60) * (float)sp;
                var sy = 360 - 280 * (float)Math.Sin(Math.PI * sp);
                var sun = sp < 0.3 ? DayScene.Mix(H("#FB923C"), H("#FDE047"), (float)(sp / 0.3))
                        : sp > 0.7 ? DayScene.Mix(H("#FDE047"), H("#F97316"), (float)((sp - 0.7) / 0.3)) : H("#FDE047");
                DayScene.FillCircle(g, Color.FromArgb(64, sun), sx, sy, 54);
                DayScene.FillCircle(g, sun, sx, sy, 34);
            }
            // Trăng khuyết
            if (night > 0)
            {
                var mx = w - 110 + 20 * (float)Math.Sin(t * 3);
                DayScene.FillCircle(g, Color.FromArgb((int)(255 * night), H("#F1F5F9")), mx, 110, 26);
                DayScene.FillCircle(g, Color.FromArgb((int)(255 * night), top), mx + 11, 103, 23);
            }

            // Mây (ban ngày), trôi chậm
            var cloudColor = Color.FromArgb((int)(215 * (1 - night)), Color.White);
            for (var i = 0; i < 3; i++)
                DayScene.Cloud(g, cloudColor, Wrap(i * 170 - _scroll * 0.15f, w + 120) - 60, 200 + i * 30);

            // Đồi + cây trôi về sau (thị sai: lớp xa trôi chậm hơn)
            DayScene.HillLayer(g, w, SceneH, 400, 40, 520, _scroll * 0.25f, N("#A7F3D0", "#1E293B"));
            DayScene.HillLayer(g, w, SceneH, 440, 22, 360, _scroll * 0.5f, N("#6EE7B7", "#334155"));
            var leaf = N("#10B981", "#14532D");
            for (var i = 0; i < 5; i++)
                DayScene.Tree(g, Wrap(i * 130 - _scroll * 0.5f, w + 100) - 50, 440 - (i % 2) * 8, 12 + (i % 3) * 3, leaf);

            // Kho trôi vào cuối chặng
            var whX = w - 185 + (1 - DayScene.Seg(t, 0.60, 0.74)) * 320;
            if (t > 0.60) DayScene.Warehouse(g, whX, 330, night);

            // Đường: lề, mặt đường, vạch vàng (đứng yên khi xe đã dừng), cỏ
            using (var curb = new SolidBrush(N("#CBD5E1", "#64748B"))) g.FillRectangle(curb, 0, Road - 10, w, 10);
            using (var road = new System.Drawing.Drawing2D.LinearGradientBrush(new RectangleF(0, Road, w, 80), N("#475569", "#1E293B"), N("#334155", "#0F172A"), 90f))
                g.FillRectangle(road, 0, Road, w, 80);
            var lane = H("#FACC15");
            for (var x = -Wrap(_scroll, 50); x < w; x += 50) DayScene.FillRound(g, lane, x, 517.5f, 28, 5, 2.5f);
            using (var grass = new SolidBrush(N("#86EFAC", "#14532D"))) g.FillRectangle(grass, 0, 560, w, SceneH - 560 + 2);

            // Xe: chạy tại chỗ (cảnh trôi) -> cuối chặng tiến tới trước kho rồi dừng
            var truckX = 50 + 70 * DayScene.Seg(t, 0.68, 0.76);
            DayScene.DrawTruck(g, truckX, 482, time, moving: t < DriveEnd, night: night);

            // Bốc 3 thùng hàng bay vào cửa kho
            for (var i = 0; i < 3; i++)
            {
                var a = 0.78 + i * 0.03;
                var k = DayScene.Seg(t, a, a + 0.04);
                if (t > a && k < 1)
                {
                    var bx = truckX + 40 + (whX + 75 - truckX - 40) * k;
                    var by = 470 - 60 * (float)Math.Sin(Math.PI * k) - 20 * k;
                    DayScene.Box(g, bx, by);
                }
            }

            // Khách hàng giơ ngón cái
            var ck = DayScene.Seg(t, 0.85, 0.89) * (1 - DayScene.Seg(t, 0.96, 0.99));
            DayScene.Customer(g, w - 36, 476, ck, time);

            // Chữ: tiêu đề, mô tả, chặng hiện tại (đổi màu sáng khi trời tối)
            using (var title = new SolidBrush(N("#0F172A", "#FFFFFF"))) g.DrawString(lblAppName.Text, TitleFont, title, 36, 70);
            using (var tag = new SolidBrush(N("#334155", "#CBD5E1"))) g.DrawString(lblTagline.Text, TaglineFont, tag, 39, 122);
            var (cap, dot) = (Captions[0].Text, Captions[0].Dot);
            foreach (var c in Captions) if (t >= c.T) (cap, dot) = (c.Text, c.Dot);
            var capSize = g.MeasureString(cap, CaptionFont);
            DayScene.FillRound(g, night > 0.5f ? Color.FromArgb(40, Color.White) : Color.FromArgb(190, Color.White),
                38, 172, capSize.Width + 38, 28, 14);
            DayScene.FillCircle(g, dot, 56, 186, 6);
            using (var capBrush = new SolidBrush(N("#0F172A", "#FFFFFF"))) g.DrawString(cap, CaptionFont, capBrush, 66, 177);
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
                lblStatus.Text = "❌ " + ex.Message; // đã là câu dễ hiểu (ApiConnection đổi lỗi mạng sang tiếng Việt)
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
