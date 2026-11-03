using System.Drawing.Drawing2D;

namespace TruckingSystem.Desktop.GUI_Shared
{
    /// <summary>1 CHẾ ĐỘ MÀU của khung màn chính (Bình minh / Trưa / Chiều / Tối): màu menu trái, logo, thanh trên.</summary>
    public sealed record UiMode(
        string Key, string Name, Color SkyTop, Color SkyBottom, Color Hill, Color Road, Color Sun, bool Night,
        Color Side, Color SideText, Color SideHeader, Color SideHover, Color Active, Color ActiveHover,
        Color Top, Color TopText, Color TopSubText, Color Content, Color[] Strip, Color LogoText,
        Color SideMid = default, Color SideBottom = default, Color ActiveEnd = default, Color ActiveText = default,
        Color Seg = default, Color SegText = default)
    {
        /// <summary>Menu trái tô dải màu trời từ trên (Side) xuống dưới (SideMid -> SideBottom) như cảnh ở màn đăng nhập.</summary>
        public Color[] SideStops => new[] { Side, SideMid.IsEmpty ? Side : SideMid, SideBottom.IsEmpty ? Side : SideBottom };
    }

    /// <summary>VẼ TAY dùng chung cho màn đăng nhập ("1 ngày giao hàng") và logo menu trái: xe tải, kho, khách hàng,
    /// đồi, cây, thùng hàng; bảng màu 4 chế độ + lưu chế độ đã chọn. Không dùng hình ảnh / thư viện ngoài.
    /// Tọa độ theo đơn vị vẽ của nơi gọi (đã ScaleTransform sẵn).</summary>
    public static class DayScene
    {
        public static Color Hex(string h) => ColorTranslator.FromHtml(h);

        public static Color Mix(Color a, Color b, float k)
        {
            k = Math.Clamp(k, 0f, 1f);
            return Color.FromArgb(
                (int)(a.A + (b.A - a.A) * k), (int)(a.R + (b.R - a.R) * k),
                (int)(a.G + (b.G - a.G) * k), (int)(a.B + (b.B - a.B) * k));
        }

        /// <summary>Chuyển mượt 0 -> 1 trong đoạn [a, b] (smoothstep).</summary>
        public static float Seg(double t, double a, double b)
        {
            var x = Math.Clamp((t - a) / (b - a), 0, 1);
            return (float)(x * x * (3 - 2 * x));
        }

        /// <summary>Nội suy giá trị theo các mốc thời gian (t tăng dần).</summary>
        public static float Keyed((double T, float V)[] keys, double t)
        {
            for (var i = 0; i < keys.Length - 1; i++)
                if (t >= keys[i].T && t <= keys[i + 1].T)
                {
                    var k = (float)((t - keys[i].T) / Math.Max(1e-6, keys[i + 1].T - keys[i].T));
                    return keys[i].V + (keys[i + 1].V - keys[i].V) * k;
                }
            return keys[^1].V;
        }

        public static (Color Top, Color Bottom) KeyedSky((double T, Color Top, Color Bottom)[] keys, double t)
        {
            for (var i = 0; i < keys.Length - 1; i++)
                if (t >= keys[i].T && t <= keys[i + 1].T)
                {
                    var k = (float)((t - keys[i].T) / Math.Max(1e-6, keys[i + 1].T - keys[i].T));
                    return (Mix(keys[i].Top, keys[i + 1].Top, k), Mix(keys[i].Bottom, keys[i + 1].Bottom, k));
                }
            return (keys[^1].Top, keys[^1].Bottom);
        }

        // ================= hình cơ bản =================

        public static GraphicsPath Round(float x, float y, float w, float h, float r)
        {
            var path = new GraphicsPath();
            var d = Math.Max(0.5f, Math.Min(r * 2, Math.Min(w, h)));
            path.AddArc(x, y, d, d, 180, 90);
            path.AddArc(x + w - d, y, d, d, 270, 90);
            path.AddArc(x + w - d, y + h - d, d, d, 0, 90);
            path.AddArc(x, y + h - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void FillRound(Graphics g, Color c, float x, float y, float w, float h, float r)
        {
            using var b = new SolidBrush(c);
            using var p = Round(x, y, w, h, r);
            g.FillPath(b, p);
        }

        public static void FillCircle(Graphics g, Color c, float cx, float cy, float r)
        {
            using var b = new SolidBrush(c);
            g.FillEllipse(b, cx - r, cy - r, r * 2, r * 2);
        }

        /// <summary>Dãy đồi lượn sóng kéo dài cả bề ngang; off = độ trôi (cảnh lùi về sau khi xe chạy).</summary>
        public static void HillLayer(Graphics g, float w, float h, float baseY, float amp, float period, float off, Color color)
        {
            var pts = new List<PointF> { new(0, h) };
            for (var x = 0f; x <= w + 6; x += 6)
            {
                var u = (x + off) / period * Math.PI * 2;
                pts.Add(new PointF(x, baseY - amp * (float)(Math.Sin(u) * 0.6 + Math.Sin(u * 2.3 + 1) * 0.4)));
            }
            pts.Add(new PointF(w + 6, h));
            using var b = new SolidBrush(color);
            g.FillPolygon(b, pts.ToArray());
        }

        public static void Tree(Graphics g, float x, float y, float r, Color leaf)
        {
            using (var trunk = new SolidBrush(Hex("#92400E"))) g.FillRectangle(trunk, x - 2, y + r * 0.55f, 4, r * 0.9f);
            FillCircle(g, leaf, x, y, r);
        }

        public static void Cloud(Graphics g, Color c, float x, float y)
        {
            using var b = new SolidBrush(c);
            g.FillEllipse(b, x - 38, y - 13, 76, 26);
            g.FillEllipse(b, x - 4, y - 25, 48, 30);
        }

        public static void Box(Graphics g, float x, float y)
        {
            FillRound(g, Hex("#F59E0B"), x, y, 22, 18, 3);
            using var tape = new SolidBrush(Hex("#FCD34D"));
            using var band = new SolidBrush(Hex("#D97706"));
            g.FillRectangle(tape, x + 9, y, 4, 18);
            g.FillRectangle(band, x, y + 7, 22, 3);
        }

        // ================= xe tải =================

        private static readonly Font TruckLogoFont = new("Segoe UI", 12f, FontStyle.Bold, GraphicsUnit.Pixel);

        /// <summary>Xe tải quay đầu sang phải (dài ~148, cao ~70 đơn vị): thùng trắng chữ LOGISTICS + viền cam, cabin xanh,
        /// 3 bánh (quay khi chạy), bóng gầm, khói (khi chạy), đèn pha (khi trời tối, night 0..1).</summary>
        public static void DrawTruck(Graphics g, float x, float y, double t, bool moving = true, float night = 0)
        {
            var state = g.Save();
            g.TranslateTransform(x, y - (moving ? (float)Math.Abs(Math.Sin(t * 7)) * 2 : 0));

            if (moving)
                for (var i = 0; i < 3; i++)
                {
                    var p = (float)((t / 1.2 + i / 3.0) % 1.0);
                    var r = 7 * (0.6f + p);
                    FillCircle(g, Color.FromArgb((int)(180 * (1 - p)), 226, 232, 240), -6 - 30 * p, 52 - 16 * p, r);
                }

            if (night > 0.3f) // đèn pha
            {
                using var beam = new LinearGradientBrush(new PointF(146, 0), new PointF(272, 0),
                    Color.FromArgb((int)(140 * night), 253, 224, 71), Color.FromArgb(0, 253, 224, 71));
                g.FillPolygon(beam, new PointF[] { new(146, 40), new(270, 22), new(270, 74) });
            }

            using (var shadow = new SolidBrush(Color.FromArgb(64, 15, 23, 42))) g.FillEllipse(shadow, 0, 65, 144, 10);
            FillRound(g, Color.White, 0, 0, 98, 52, 6);
            using (var orange = new SolidBrush(Hex("#F97316"))) g.FillRectangle(orange, 0, 40, 98, 12);
            using (var logo = new SolidBrush(Hex("#2563EB")))
            using (var center = new StringFormat { Alignment = StringAlignment.Center })
                g.DrawString("LOGISTICS", TruckLogoFont, logo, 49, 14, center);

            using (var cab = new GraphicsPath())
            {
                cab.AddLine(100, 14, 126, 14);
                cab.AddBezier(126, 14, 131, 14, 136, 18, 138, 22);
                cab.AddLine(138, 22, 146, 36);
                cab.AddLine(146, 36, 146, 52);
                cab.AddLine(146, 52, 100, 52);
                cab.CloseFigure();
                using var blue = new SolidBrush(Hex("#2563EB"));
                g.FillPath(blue, cab);
            }
            using (var glass = new SolidBrush(night > 0.5f ? Hex("#FDE68A") : Hex("#BAE6FD")))
                g.FillPolygon(glass, new PointF[] { new(106, 19), new(124, 19), new(130, 23), new(136, 34), new(106, 34) });
            FillRound(g, Hex("#FDE047"), 140, 42, 7, 5, 2);
            FillRound(g, Hex("#1E3A8A"), 96, 50, 52, 6, 2);

            var angle = moving ? (float)(t * 10) : 0f;
            foreach (var cx in new[] { 24f, 50f, 124f }) Wheel(g, cx, 58, angle);
            g.Restore(state);
        }

        private static void Wheel(Graphics g, float cx, float cy, float angle)
        {
            FillCircle(g, Hex("#1E293B"), cx, cy, 11);
            using (var spoke = new Pen(Hex("#64748B"), 2f))
            {
                var dx = (float)Math.Sin(angle) * 9; var dy = (float)Math.Cos(angle) * 9;
                g.DrawLine(spoke, cx - dx, cy - dy, cx + dx, cy + dy);
            }
            FillCircle(g, Hex("#CBD5E1"), cx, cy, 5);
        }

        // ================= kho + khách hàng =================

        private static readonly Font SignFont = new("Segoe UI", 11f, FontStyle.Bold, GraphicsUnit.Pixel);

        /// <summary>Kho (rộng ~180, mái nhọn, cửa cuốn, biển "KHO"); cửa sổ sáng đèn khi trời tối. Đáy kho ở y = topY + 150.</summary>
        public static void Warehouse(Graphics g, float x, float topY, float night)
        {
            using (var body = new SolidBrush(Mix(Hex("#94A3B8"), Hex("#64748B"), night))) g.FillRectangle(body, x, topY + 40, 170, 110);
            using (var roof = new SolidBrush(Hex("#475569")))
                g.FillPolygon(roof, new PointF[] { new(x - 10, topY + 42), new(x + 85, topY), new(x + 180, topY + 42) });
            FillRound(g, Hex("#334155"), x + 55, topY + 70, 60, 80, 3);
            using (var line = new Pen(Hex("#64748B"), 1))
                for (var k = 0; k < 7; k++) g.DrawLine(line, x + 57, topY + 78 + k * 10, x + 113, topY + 78 + k * 10);
            var win = night > 0.5f ? Hex("#FDE047") : Hex("#E0F2FE");
            FillRound(g, win, x + 15, topY + 70, 26, 20, 2);
            FillRound(g, win, x + 130, topY + 70, 26, 20, 2);
            FillRound(g, Hex("#2563EB"), x + 50, topY + 46, 70, 18, 4);
            using var white = new SolidBrush(Color.White);
            using var center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("KHO", SignFont, white, new RectangleF(x + 50, topY + 46, 70, 18), center);
        }

        private static readonly Font BubbleFont = new("Segoe UI", 13f, FontStyle.Bold, GraphicsUnit.Pixel);

        /// <summary>Nhân vật KHÁCH HÀNG đứng (chân ở y), giơ ngón cái + bóng thoại "Tuyệt vời!" + tim bay. k = 0..1 độ hiện.</summary>
        public static void Customer(Graphics g, float x, float y, float k, double t)
        {
            if (k <= 0) return;
            var a = (int)(255 * k);
            var state = g.Save();
            g.TranslateTransform(x, y);
            Color A(Color c) => Color.FromArgb(Math.Min(a, c.A), c);

            FillRound(g, A(Hex("#16A34A")), -12, -40, 24, 32, 8);               // áo
            using (var leg = new SolidBrush(A(Hex("#1E293B")))) { g.FillRectangle(leg, -10, -10, 8, 10); g.FillRectangle(leg, 2, -10, 8, 10); }
            FillCircle(g, A(Hex("#FCD9B6")), 0, -50, 11);                       // đầu
            using (var hair = new SolidBrush(A(Hex("#78350F")))) g.FillPie(hair, -11, -65, 22, 22, 180, 180);
            FillCircle(g, A(Hex("#0F172A")), -4, -50, 1.5f);
            FillCircle(g, A(Hex("#0F172A")), 4, -50, 1.5f);
            using (var smile = new Pen(A(Hex("#0F172A")), 1.5f)) g.DrawArc(smile, -4, -51, 8, 8, 20, 140);

            // tay giơ lên + nắm tay ngón cái (vẽ hình, không dùng emoji)
            var lift = Math.Min(1f, k * 1.5f) * (float)(0.85 + 0.15 * Math.Sin(t * 8));
            var hx = 18; var hy = -34 - 18 * lift;
            using (var arm = new Pen(A(Hex("#16A34A")), 6) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawLine(arm, 10, -34, hx, hy);
            FillRound(g, A(Hex("#FCD9B6")), hx - 5, hy - 6, 10, 9, 3);                // nắm tay
            FillRound(g, A(Hex("#FCD9B6")), hx - 2, hy - 15, 4.5f, 11, 2);           // ngón cái

            if (k > 0.6f)
            {
                FillRound(g, A(Color.White), -72, -112, 100, 30, 10);
                using (var tail = new SolidBrush(A(Color.White)))
                    g.FillPolygon(tail, new PointF[] { new(-12, -83), new(-4, -72), new(2, -83) });
                using (var txt = new SolidBrush(A(Hex("#16A34A"))))
                using (var center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString("Tuyệt vời!", BubbleFont, txt, new RectangleF(-72, -112, 100, 30), center);
                for (var i = 0; i < 3; i++)
                {
                    var p = (float)((t * 0.7 + i / 3.0) % 1.0);
                    Heart(g, Color.FromArgb((int)(a * (1 - p)), 239, 68, 68), 30 + i * 9, -74 - 50 * p, 6);
                }
            }
            g.Restore(state);
        }

        private static void Heart(Graphics g, Color c, float cx, float cy, float r)
        {
            using var b = new SolidBrush(c);
            g.FillEllipse(b, cx - r, cy - r, r, r);
            g.FillEllipse(b, cx, cy - r, r, r);
            g.FillPolygon(b, new PointF[] { new(cx - r, cy - r / 2), new(cx + r, cy - r / 2), new(cx, cy + r) });
        }

        // ================= 4 chế độ màu của khung màn chính =================

        // Mẫu 4_buoi đã duyệt: menu trái mang màu bầu trời của buổi đó (dải màu trên -> dưới), mục đang chọn là viên bo tròn.
        public static readonly UiMode[] Modes =
        {
            new("dawn", "🌅 Bình minh", Hex("#F9A8D4"), Hex("#FDE68A"), Hex("#F59E0B"), Hex("#78716C"), Hex("#FB7185"), false,
                Hex("#FBCFE8"), Hex("#831843"), Hex("#BE185D"), Hex("#FDE2EF"), Hex("#EC4899"), Hex("#DB2777"),
                Color.White, Hex("#0F172A"), Hex("#64748B"), Hex("#FFF6F9"), new[] { Hex("#EC4899"), Hex("#F97316"), Hex("#FACC15") }, Hex("#831843"),
                SideMid: Hex("#FED7AA"), SideBottom: Hex("#FEF3C7"), ActiveEnd: Hex("#F97316"), ActiveText: Color.White,
                Seg: Hex("#FCE7F3"), SegText: Hex("#9D174D")),
            new("noon", "☀️ Trưa", Hex("#38BDF8"), Hex("#E0F2FE"), Hex("#4ADE80"), Hex("#475569"), Hex("#FDE047"), false,
                Hex("#0284C7"), Hex("#F0F9FF"), Hex("#BAE6FD"), Hex("#0EA5E9"), Color.White, Hex("#F0F9FF"),
                Color.White, Hex("#0F172A"), Hex("#64748B"), Hex("#F0F7FC"), new[] { Hex("#0EA5E9"), Hex("#22D3EE"), Hex("#34D399") }, Color.White,
                SideMid: Hex("#0EA5E9"), SideBottom: Hex("#38BDF8"), ActiveEnd: Hex("#F0F9FF"), ActiveText: Hex("#0369A1"),
                Seg: Hex("#E0F2FE"), SegText: Hex("#0369A1")),
            new("eve", "🌇 Chiều", Hex("#F97316"), Hex("#F472B6"), Hex("#9A3412"), Hex("#44403C"), Hex("#FDE68A"), false,
                Hex("#F97316"), Hex("#FFF1F2"), Hex("#FED7AA"), Hex("#FB923C"), Color.White, Hex("#FFF7ED"),
                Hex("#FFFBF7"), Hex("#0F172A"), Hex("#78716C"), Hex("#FDF5F0"), new[] { Hex("#F97316"), Hex("#EC4899"), Hex("#8B5CF6") }, Color.White,
                SideMid: Hex("#DB2777"), SideBottom: Hex("#6D28D9"), ActiveEnd: Hex("#FFF7ED"), ActiveText: Hex("#C2410C"),
                Seg: Hex("#FFEDD5"), SegText: Hex("#9A3412")),
            new("night", "🌙 Tối", Hex("#0F172A"), Hex("#1E3A8A"), Hex("#1E293B"), Hex("#0F172A"), Hex("#F1F5F9"), true,
                Hex("#0B1026"), Hex("#C7D2FE"), Hex("#818CF8"), Hex("#1E1B4B"), Hex("#6366F1"), Hex("#4F46E5"),
                Hex("#111735"), Hex("#EEF2FF"), Hex("#A5B4FC"), Hex("#E6EAF5"), new[] { Hex("#6366F1"), Hex("#A855F7"), Hex("#EC4899") }, Color.White,
                SideMid: Hex("#1E1B4B"), SideBottom: Hex("#312E81"), ActiveEnd: Hex("#A855F7"), ActiveText: Color.White,
                Seg: Hex("#1E1B4B"), SegText: Hex("#A5B4FC")),
        };

        public static UiMode ModeByKey(string? key) => Modes.FirstOrDefault(m => m.Key == key) ?? Modes[1];

        private static string ModeFile => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LogisticsSystem", "ui-mode.txt");

        /// <summary>Chế độ đã chọn lần trước (mặc định Trưa).</summary>
        public static UiMode LoadMode()
        {
            try { return File.Exists(ModeFile) ? ModeByKey(File.ReadAllText(ModeFile).Trim()) : Modes[1]; }
            catch { return Modes[1]; }
        }

        public static void SaveMode(UiMode mode)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ModeFile)!);
                File.WriteAllText(ModeFile, mode.Key);
            }
            catch { /* không lưu được (quyền thư mục) thì thôi — lần sau về mặc định */ }
        }

        private static readonly Font LogoFont = new("Segoe UI", 20f, FontStyle.Bold, GraphicsUnit.Pixel);

        /// <summary>Logo menu trái (khung chuẩn rộng theo panel × cao 96): trời theo chế độ (Tối có sao + trăng), mặt trời,
        /// đồi, con đường có vạch vàng, xe tải nhỏ, chữ "Logistics System".</summary>
        public static void PaintLogo(Graphics g, Size size, UiMode m)
        {
            if (size.Width <= 0 || size.Height <= 0) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            var scale = size.Height / 96f;
            var w = size.Width / scale;
            g.ScaleTransform(scale, scale);

            using (var sky = new LinearGradientBrush(new RectangleF(0, 0, w, 96), m.SkyTop, m.SkyBottom, 90f))
                g.FillRectangle(sky, 0, 0, w, 96);
            if (m.Night)
                using (var star = new SolidBrush(Color.White))
                    for (var i = 0; i < 16; i++) g.FillRectangle(star, (i * 37) % w, (i * 23) % 60, 1.5f, 1.5f);
            FillCircle(g, m.Sun, w - 20, 24, 10);
            if (m.Night) FillCircle(g, m.SkyTop, w - 16, 20, 9); // trăng khuyết

            using (var hill = new GraphicsPath())
            {
                hill.AddBezier(0, 80, w * 0.25f, 64, w * 0.45f, 70, w * 0.6f, 76);
                hill.AddBezier(w * 0.6f, 76, w * 0.75f, 80, w * 0.9f, 68, w, 72);
                hill.AddLine(w, 72, w, 86);
                hill.AddLine(w, 86, 0, 86);
                hill.CloseFigure();
                using var b = new SolidBrush(m.Hill);
                g.FillPath(b, hill);
            }
            using (var road = new SolidBrush(m.Road)) g.FillRectangle(road, 0, 84, w, 12);
            using (var lane = new SolidBrush(Hex("#FACC15")))
                for (var x = 8f; x < w; x += 32f) g.FillRectangle(lane, x, 89, 16, 2);

            var st = g.Save();
            g.TranslateTransform(w - 66, 64);
            g.ScaleTransform(0.36f, 0.36f);
            DrawTruck(g, 0, 0, 0, moving: false, night: m.Night ? 1 : 0);
            g.Restore(st);

            if (m.LogoText.GetBrightness() > 0.8f) // chữ trắng trên nền trời sáng: thêm bóng mờ cho dễ đọc
                using (var shadow = new SolidBrush(Color.FromArgb(90, 15, 23, 42)))
                    g.DrawString("Logistics System", LogoFont, shadow, 15, 23.5f);
            using var title = new SolidBrush(m.LogoText);
            g.DrawString("Logistics System", LogoFont, title, 14, 22);
        }
    }
}
