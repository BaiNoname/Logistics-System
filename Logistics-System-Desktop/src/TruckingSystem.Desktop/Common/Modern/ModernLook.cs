using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TruckingSystem.Desktop.Common
{
    /// <summary>KIỂU MỚI BÊN TRONG MÀN (mẫu 4_buoi đã duyệt) — CHỈ áp cho các màn của Nhân (Tổng quan, Đơn, Chuyến, Kho, Đội xe,
    /// Tài xế, Địa điểm, Nhiên liệu, Nguồn lực theo kho, Nhật ký hệ thống, Thông báo) và các hộp thoại của chúng. Màn của Minh giữ nguyên (Theme.cs không đổi).
    ///  • Bảng: tiêu đề sáng (nền xám rất nhạt, chữ xám đậm), chỉ kẻ ngang mảnh, dòng cao hơn, dòng chọn xanh trời nhạt.
    ///  • Nút: bo góc; nút chính (màu Primary) tô dải màu theo buổi đang chọn (Bình minh hồng–cam, Trưa xanh trời, Chiều cam–hồng,
    ///    Tối chàm–tím).
    /// Chỉ đổi những giá trị còn đúng mặc định của Theme — bảng / nút màn nào đã tự chỉnh màu riêng thì giữ nguyên.
    /// Gọi tự động từ BaseForm / BaseUserControl khi mở; control thêm sau (sinh bằng code) cũng được áp.</summary>
    public static class ModernLook
    {
        private static readonly string[] MineNamespaces =
        {
            "GUI_TransportOrderManagement", "GUI_TripManagement", "GUI_WarehouseManagement", "GUI_VehicleManagement",
            "GUI_DriverManagement", "GUI_LocationManagement", "GUI_FuelManagement", "GUI_ResourceManagement", "GUI_RouteManagement",
            "GUI_AuditLogManagement", "GUI_NotificationManagement", // Nhật ký hệ thống + Thông báo
        };

        /// <summary>Màn / hộp thoại thuộc phần của Nhân.</summary>
        public static bool IsMine(Type t)
            => t.Name == "DashboardControl"
               || (t.Namespace is { } ns && MineNamespaces.Any(m => ns.EndsWith(m, StringComparison.Ordinal)));

        /// <summary>Màn / hộp thoại KHÔNG thuộc phần của Nhân (màn của Minh) -> CHỈ bo góc nút cho đồng bộ toàn ứng dụng;
        /// bảng, màu chữ, bố cục, nghiệp vụ giữ nguyên. Trừ khung chính (menu) và màn đăng nhập — 2 màn này có kiểu nút riêng.</summary>
        public static bool RoundButtonsOnly(Type t)
            => !IsMine(t) && t.Name is not ("MainForm" or "LoginForm");

        // Dải màu nút chính — MainForm đặt theo buổi đang chọn.
        public static Color AccentStart { get; private set; } = ColorTranslator.FromHtml("#0EA5E9");
        public static Color AccentEnd { get; private set; } = ColorTranslator.FromHtml("#0891B2");

        public static void SetAccent(Color start, Color end)
        {
            AccentStart = start;
            AccentEnd = end;
        }

        private static readonly Color HeaderBack = ColorTranslator.FromHtml("#F8FAFC");
        private static readonly Color HeaderFore = ColorTranslator.FromHtml("#64748B");
        private static readonly Color Line = ColorTranslator.FromHtml("#EEF2F7");
        private static readonly Color SelectRow = ColorTranslator.FromHtml("#E0F2FE");
        private static readonly Color AltRow = ColorTranslator.FromHtml("#FBFCFE");
        private static readonly Color OldSelect = ColorTranslator.FromHtml("#DBEAFE");
        private static readonly Font HeaderFont = new("Segoe UI", 9F, FontStyle.Bold);

        private static readonly ConditionalWeakTable<Control, object> Done = new();

        public static void Apply(Control root)
        {
            Visit(root);
        }

        private static void Visit(Control c)
        {
            try
            {
                if (!Done.TryGetValue(c, out _))
                {
                    Done.Add(c, new object());
                    switch (c)
                    {
                        case DataGridView grid: StyleGrid(grid); return; // không đi vào bên trong bảng (thanh cuộn, ô sửa, lớp "đang tải")
                        case Button b: RoundButton(b); break;
                    }
                    // Thứ bấm được (nút, ô chọn xổ xuống, chọn ngày, ô tích, nút tròn) -> con trỏ bàn tay.
                    if (c is ButtonBase or DateTimePicker || c is ComboBox { DropDownStyle: ComboBoxStyle.DropDownList })
                        if (c.Cursor == Cursors.Default) c.Cursor = Cursors.Hand;
                    // Control thêm sau (sinh bằng code): áp SAU khi việc thêm xong hẳn — không chen vào giữa lúc
                    // màn đang thêm / xóa control (trước đây làm lỗi "Index was out of range" ở lớp "đang tải" của bảng).
                    c.ControlAdded += (_, e) =>
                    {
                        var added = e.Control;
                        if (added is null) return;
                        if (c.IsHandleCreated && !c.IsDisposed)
                            c.BeginInvoke(new Action(() => { if (!added.IsDisposed) Visit(added); }));
                        else Visit(added);
                    };
                }
                foreach (var child in c.Controls.Cast<Control>().ToList()) Visit(child);
            }
            catch
            {
                // Chỉ là trang trí — lỗi ở đây không được làm hỏng màn hình.
            }
        }

        // ------------------------------------------------------------ chỉ bo góc nút (màn của Minh)

        private static readonly ConditionalWeakTable<Control, object> DoneButtons = new();

        /// <summary>Chỉ bo góc các nút trong <paramref name="root"/> (kể cả nút thêm sau bằng code) — không đụng bảng / màu / bố cục.
        /// Gọi SAU khi màn đã chạy xong sự kiện Load để các nút đã được tô kiểu (ButtonStyles) xong.</summary>
        public static void ApplyButtons(Control root) => VisitButtons(root);

        private static void VisitButtons(Control c)
        {
            try
            {
                if (!DoneButtons.TryGetValue(c, out _))
                {
                    DoneButtons.Add(c, new object());
                    if (c is DataGridView) return; // không đi vào bên trong bảng
                    if (c is Button b) RoundButton(b);
                    c.ControlAdded += (_, e) =>
                    {
                        var added = e.Control;
                        if (added is null) return;
                        if (c.IsHandleCreated && !c.IsDisposed)
                            c.BeginInvoke(new Action(() => { if (!added.IsDisposed) VisitButtons(added); }));
                        else VisitButtons(added);
                    };
                }
                foreach (var child in c.Controls.Cast<Control>().ToList()) VisitButtons(child);
            }
            catch
            {
                // Chỉ là trang trí — lỗi ở đây không được làm hỏng màn hình.
            }
        }

        // ------------------------------------------------------------ bảng

        private static void StyleGrid(DataGridView grid)
        {
            Theme.StyleGridLook(grid); // giữ phần chung (giãn cột, sửa tiêu đề "_"); lần sau gọi lại không áp đè
            var h = grid.ColumnHeadersDefaultCellStyle;
            if (h.BackColor == Theme.GridHeaderBg)
            {
                h.BackColor = HeaderBack;
                h.SelectionBackColor = HeaderBack;
                h.ForeColor = HeaderFore;
                h.SelectionForeColor = HeaderFore;
                h.Font = HeaderFont;
                h.Padding = new Padding(10, 8, 6, 8);
                grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            }
            if (grid.CellBorderStyle == DataGridViewCellBorderStyle.Single) grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            if (grid.GridColor == Theme.BorderColor) grid.GridColor = Line;
            if (grid.DefaultCellStyle.SelectionBackColor == OldSelect) grid.DefaultCellStyle.SelectionBackColor = SelectRow;
            if (grid.AlternatingRowsDefaultCellStyle.BackColor == Theme.GridAltRow) grid.AlternatingRowsDefaultCellStyle.BackColor = AltRow;
            if (grid.DefaultCellStyle.Padding == new Padding(8, 0, 4, 0)) grid.DefaultCellStyle.Padding = new Padding(10, 0, 6, 0);
            if (grid.RowTemplate.Height == 36) grid.RowTemplate.Height = 38;
            HookHeaderAlign(grid);
            HookKeepScroll(grid);
        }

        // ------------------------------------------------------------ tiêu đề cột thẳng hàng với số

        private static readonly ConditionalWeakTable<DataGridView, object> ScrollHooked = new();

        /// <summary>GIỮ VỊ TRÍ CUỘN khi bảng được nạp lại (lọc, Làm mới, sau khi xác nhận / sửa 1 dòng...):
        /// trước đây gán lại DataSource là bảng nhảy về đầu, dòng ở cuối phải kéo xuống lại.
        /// Cách làm: mỗi lần bảng VẼ thì nhớ dòng đang ở đầu khung nhìn (lúc nạp lại thì chưa kịp vẽ nên vẫn là
        /// vị trí người dùng đang xem); nạp xong (DataBindingComplete kiểu Reset) thì cuộn về đúng dòng đó.
        /// Màn nào sau đó tự chọn 1 dòng ngoài khung (VD mở từ thông báo) thì bảng vẫn cuộn tới dòng đó như cũ.</summary>
        private static void HookKeepScroll(DataGridView grid)
        {
            if (ScrollHooked.TryGetValue(grid, out _)) return;
            var state = new int[] { -1 };
            ScrollHooked.Add(grid, state);
            grid.Paint += (_, _) =>
            {
                if (grid.Rows.Count > 0) state[0] = grid.FirstDisplayedScrollingRowIndex;
            };
            grid.DataBindingComplete += (_, e) =>
            {
                if (e.ListChangedType != System.ComponentModel.ListChangedType.Reset) return;
                var keep = state[0];
                if (keep <= 0 || grid.Rows.Count == 0) return;
                try
                {
                    grid.FirstDisplayedScrollingRowIndex = Math.Min(keep, grid.Rows.Count - 1);
                }
                catch (InvalidOperationException)
                {
                    // Bảng chưa đủ chỗ hiện dòng — bỏ qua, chỉ là giữ vị trí cuộn.
                }
            };
        }

        private static readonly ConditionalWeakTable<DataGridView, object> AlignHooked = new();

        /// <summary>CỘT SỐ (tiền, km, lít, %…) căn PHẢI nhưng tiêu đề vẫn căn TRÁI -> số dồn về cuối ô, cách xa chữ tiêu đề,
        /// nhìn lệch. Tiêu đề tự căn THEO đúng kiểu căn của dữ liệu trong cột (phải / giữa / trái), áp cho mọi bảng của
        /// các màn ModernLook — kể cả cột đặt căn sau khi nạp dữ liệu (chạy sau các xử lý DataBindingComplete của màn).</summary>
        private static void HookHeaderAlign(DataGridView grid)
        {
            if (AlignHooked.TryGetValue(grid, out _)) return;
            AlignHooked.Add(grid, new object());
            void Sync()
            {
                if (grid.IsDisposed) return;
                foreach (DataGridViewColumn c in grid.Columns)
                {
                    var a = c.DefaultCellStyle.Alignment;
                    var want = a switch
                    {
                        DataGridViewContentAlignment.TopRight or DataGridViewContentAlignment.MiddleRight or DataGridViewContentAlignment.BottomRight
                            => DataGridViewContentAlignment.MiddleRight,
                        DataGridViewContentAlignment.TopCenter or DataGridViewContentAlignment.MiddleCenter or DataGridViewContentAlignment.BottomCenter
                            => DataGridViewContentAlignment.MiddleCenter,
                        _ => DataGridViewContentAlignment.NotSet, // trái: theo kiểu chung của tiêu đề
                    };
                    if (c.HeaderCell.Style.Alignment != want) c.HeaderCell.Style.Alignment = want;
                }
            }
            void Later() { if (!grid.IsDisposed && grid.IsHandleCreated) grid.BeginInvoke(new Action(Sync)); else Sync(); }
            grid.DataBindingComplete += (_, _) => Later();
            grid.ColumnAdded += (_, _) => Later();
            grid.HandleCreated += (_, _) => Later();
            Sync();
        }

        // ------------------------------------------------------------ nút bo góc

        private sealed class ButtonState { public bool Hot, Down; }
        private static readonly ConditionalWeakTable<Button, ButtonState> States = new();

        private static readonly object? PaintKey =
            typeof(Control).GetField("s_paintEvent", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null)
            ?? typeof(Control).GetField("EventPaint", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null);
        private static readonly PropertyInfo? EventsProp = typeof(Component).GetProperty("Events", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>Nút đã có phần tự vẽ riêng (Paint) thì không đụng tới — tránh vẽ đè.</summary>
        private static bool HasOwnPaint(Control c)
        {
            if (PaintKey is null || EventsProp?.GetValue(c) is not EventHandlerList list) return true;
            return list[PaintKey] is not null;
        }

        private static void RoundButton(Button b)
        {
            if (States.TryGetValue(b, out _)) return; // đã bo rồi — không gắn vẽ 2 lần
            if (b.FlatStyle != FlatStyle.Flat || b.Image is not null || b.BackgroundImage is not null
                || b.BackColor.A < 255 || b.Parent is DataGridView || HasOwnPaint(b)) return;
            var st = new ButtonState();
            States.Add(b, st);
            // Vẽ 1 lần vào bộ đệm rồi mới hiện (không thấy nút vuông rồi mới bo tròn).
            typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(b, true);
            b.MouseEnter += (_, _) => { st.Hot = true; b.Invalidate(); };
            b.MouseLeave += (_, _) => { st.Hot = false; st.Down = false; b.Invalidate(); };
            b.MouseDown += (_, e) => { if (e.Button == MouseButtons.Left) { st.Down = true; b.Invalidate(); } };
            b.MouseUp += (_, _) => { st.Down = false; b.Invalidate(); };
            b.Paint += (_, e) => PaintButton(e.Graphics, b, st);
        }

        private static Color ParentBack(Control c)
        {
            for (var p = c.Parent; p is not null; p = p.Parent)
                if (p.BackColor.A == 255) return p.BackColor;
            return SystemColors.Control;
        }

        private static void PaintButton(Graphics g, Button b, ButtonState st)
        {
            if (b.Width < 6 || b.Height < 6) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(ParentBack(b));
            var radius = Math.Min(b.Height / 2f, b.LogicalToDeviceUnits(9));
            var r = new RectangleF(0.5f, 0.5f, b.Width - 1.5f, b.Height - 1.5f);
            using var path = Round(r, radius);

            var primary = b.BackColor == Theme.Primary || b.BackColor == Theme.PrimaryDark;
            if (primary && b.Enabled)
            {
                var a = AccentStart; var z = AccentEnd;
                if (st.Down) { a = Mix(a, Color.Black, 0.12); z = Mix(z, Color.Black, 0.12); }
                else if (st.Hot) { a = Mix(a, Color.White, 0.10); z = Mix(z, Color.White, 0.10); }
                using var shadow = Round(new RectangleF(r.X, r.Y + 1.5f, r.Width, r.Height), radius);
                using (var sb = new SolidBrush(Color.FromArgb(40, z))) g.FillPath(sb, shadow);
                using var lg = new LinearGradientBrush(new RectangleF(r.X - 1, r.Y, r.Width + 2, r.Height), a, z, 0f);
                g.FillPath(lg, path);
            }
            else
            {
                var fill = b.BackColor;
                if (!b.Enabled) fill = Mix(fill, Color.White, 0.45);
                else if (st.Down && !b.FlatAppearance.MouseDownBackColor.IsEmpty) fill = b.FlatAppearance.MouseDownBackColor;
                else if ((st.Hot || st.Down) && !b.FlatAppearance.MouseOverBackColor.IsEmpty) fill = b.FlatAppearance.MouseOverBackColor;
                using (var fb = new SolidBrush(fill)) g.FillPath(fb, path);
                if (b.FlatAppearance.BorderSize > 0)
                {
                    var line = b.FlatAppearance.BorderColor.IsEmpty ? b.ForeColor : b.FlatAppearance.BorderColor;
                    using var pen = new Pen(b.Enabled ? line : Mix(line, Color.White, 0.4));
                    g.DrawPath(pen, path);
                }
            }

            var fore = b.Enabled ? (primary ? Color.White : b.ForeColor) : Mix(b.ForeColor, Color.White, 0.5);
            var text = new Rectangle(b.Padding.Left + 4, b.Padding.Top, b.Width - b.Padding.Horizontal - 8, b.Height - b.Padding.Vertical);
            TextRenderer.DrawText(g, b.Text, b.Font, text, fore, Flags(b.TextAlign) | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis |
                (b.UseMnemonic ? TextFormatFlags.HidePrefix : TextFormatFlags.NoPrefix));
        }

        private static TextFormatFlags Flags(ContentAlignment a)
        {
            var f = a switch
            {
                ContentAlignment.TopLeft or ContentAlignment.MiddleLeft or ContentAlignment.BottomLeft => TextFormatFlags.Left,
                ContentAlignment.TopRight or ContentAlignment.MiddleRight or ContentAlignment.BottomRight => TextFormatFlags.Right,
                _ => TextFormatFlags.HorizontalCenter,
            };
            return f | TextFormatFlags.VerticalCenter;
        }

        private static GraphicsPath Round(RectangleF r, float radius)
        {
            var path = new GraphicsPath();
            var d = Math.Max(1f, Math.Min(radius * 2, Math.Min(r.Width, r.Height)));
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static Color Mix(Color a, Color b, double k)
            => Color.FromArgb(255, (int)(a.R + (b.R - a.R) * k), (int)(a.G + (b.G - a.G) * k), (int)(a.B + (b.B - a.B) * k));
    }
}
