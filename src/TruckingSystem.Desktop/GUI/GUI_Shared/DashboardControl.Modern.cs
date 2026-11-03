using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_ReportManagement;

namespace TruckingSystem.Desktop.GUI_Shared
{
    /// <summary>MÀN TỔNG QUAN KIỂU MỚI (mẫu tong_quan đã duyệt) — chỉ đổi cách trình bày, số liệu / cách tính / chỗ bấm giữ nguyên:
    ///  • Đầu trang: "Tổng quan" + dòng ngày / giờ cập nhật bên dưới; bên phải là nút chọn kỳ 1 chạm (Tháng này · Tháng trước ·
    ///    3 tháng · Năm nay · Tự chọn) + nút ⟳. Cửa sổ hẹp: nút chọn kỳ thu thành ô xổ.
    ///  • Thẻ số liệu chia ĐỀU hết bề ngang, cao bằng nhau (hẹp: 2 × 2). Số to nét vừa, đơn vị (tỷ / tr / %) nhỏ xám bên cạnh;
    ///    so với kỳ trước là nhãn màu (xanh tốt / đỏ xấu).
    ///  • Biểu đồ tháng (cột bo góc + số trên cột + đường Lãi) NGANG HÀNG với "Cần chú ý" (danh sách thẻ, việc đỏ lên đầu,
    ///    số đếm to bên phải, bấm cả dòng để mở). Hẹp: xếp dọc.</summary>
    public partial class DashboardControl
    {
        private readonly CardGrid _mainCards = new() { Dock = DockStyle.Fill };
        private readonly CardGrid _finCards = new() { Dock = DockStyle.Fill };
        private readonly CardGrid _fuelCards = new() { Dock = DockStyle.Fill };
        private readonly AttentionView _attnView = new();
        private readonly TableLayoutPanel _ovRow = new() { ColumnCount = 2, RowCount = 1, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 14), Padding = Padding.Empty };
        private readonly TableLayoutPanel _header = new()
        {
            Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 2, RowCount = 1,
            Margin = new Padding(0, 0, 0, 10),
        };
        private readonly FlowLayoutPanel _headerRight = new()
        {
            AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, Anchor = AnchorStyles.Right, Margin = Padding.Empty,
        };
        private readonly PeriodPills _pills = new();
        private readonly Button _btnReload = new() { Text = "⟳", FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 12F, FontStyle.Bold) };
        private int _ovRowIndex = -1;
        private bool? _ovNarrow; // null = chưa dựng lần nào
        private bool _ovPending;

        private int S(int v) => LogicalToDeviceUnits(v);

        private static readonly Font F_LabelFont = new("Segoe UI Semibold", 10F);
        private static readonly Font F_SegoeUI825FFontStyleBold = new("Segoe UI", 8.25F, FontStyle.Bold); // dùng lại, không tạo mới mỗi lần vẽ
        private static readonly Font F_SegoeUI85F = new("Segoe UI", 8.5F); // dùng lại, không tạo mới mỗi lần vẽ
        private static readonly Font F_SegoeUI875F = new("Segoe UI", 8.75F); // dùng lại, không tạo mới mỗi lần vẽ
        private static readonly Font F_SegoeUI975F = new("Segoe UI", 9.75F); // dùng lại, không tạo mới mỗi lần vẽ
        private static readonly Font F_SegoeUI9F = new("Segoe UI", 9F); // dùng lại, không tạo mới mỗi lần vẽ
        private static readonly Font F_SegoeUI9FFontStyleBold = new("Segoe UI", 9F, FontStyle.Bold); // dùng lại, không tạo mới mỗi lần vẽ
        private static readonly Font F_SegoeUIEmoji105F = new("Segoe UI Emoji", 10.5F); // dùng lại, không tạo mới mỗi lần vẽ
        private static readonly Font F_SegoeUIEmoji12F = new("Segoe UI Emoji", 12F); // dùng lại, không tạo mới mỗi lần vẽ
        private static readonly Font F_SegoeUISemibold11F = new("Segoe UI Semibold", 11F); // dùng lại, không tạo mới mỗi lần vẽ
        private static readonly Font F_SegoeUISemibold21F = new("Segoe UI Semibold", 21F); // dùng lại, không tạo mới mỗi lần vẽ
        private static readonly Font F_SegoeUISemibold85F = new("Segoe UI Semibold", 8.5F); // dùng lại, không tạo mới mỗi lần vẽ
        private static readonly Font F_SegoeUISemibold9F = new("Segoe UI Semibold", 9F); // dùng lại, không tạo mới mỗi lần vẽ

        /// <summary>Đầu trang mới: tiêu đề + dòng trạng thái (trái), nút chọn kỳ + ⟳ (phải).</summary>
        private void BuildHeader()
        {
            _lblTitle.Text = "Tổng quan";
            _lblTitle.Font = new Font("Segoe UI Semibold", 19F);
            _lblTitle.Margin = Padding.Empty;
            _lblStatus.Font = new Font("Segoe UI", 9.5F);
            _lblStatus.Margin = new Padding(1, 2, 0, 0);
            var left = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Anchor = AnchorStyles.Left, Margin = Padding.Empty,
            };
            left.Controls.Add(_lblTitle);
            left.Controls.Add(_lblStatus); // chuyển dòng "ngày · cập nhật lúc" từ thanh lọc lên dưới tiêu đề

            _pills.Options = PeriodOptions.Select(o => o.Key == "q" ? "3 tháng" : o.Key == "c" ? "📅 Tự chọn" : o.Text).ToArray();
            _pills.Margin = new Padding(0, 0, S(8), 0);
            _pills.Anchor = AnchorStyles.Right;
            _pills.Picked += (_, i) => _periodBox.SelectedIndex = i;
            _periodBox.SelectedIndexChanged += (_, _) => _pills.Selected = _periodBox.SelectedIndex;
            _pills.Selected = _periodBox.SelectedIndex;
            _periodBox.Margin = new Padding(0, S(4), S(8), 0);
            _periodBox.Anchor = AnchorStyles.Right;
            _periodBox.Visible = false; // rộng: dùng hàng nút chọn kỳ; hẹp mới hiện ô xổ (ApplyHeaderWidth)

            ButtonStyles.Secondary(_btnReload);
            _btnReload.Size = new Size(S(38), S(38));
            _btnReload.Margin = Padding.Empty;
            _btnReload.Anchor = AnchorStyles.Right;
            _tip.SetToolTip(_btnReload, "Tải lại số liệu (F5)");
            _btnReload.Click += async (_, _) => await LoadAsync();

            _headerRight.Controls.Add(_pills);
            _headerRight.Controls.Add(_periodBox);
            _headerRight.Controls.Add(_btnReload);
            _header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _header.Controls.Add(left, 0, 0);
            _header.Controls.Add(_headerRight, 1, 0);
            _header.Resize += (_, _) => ApplyHeaderWidth();
            lblPeriod.Visible = false;
        }

        /// <summary>Hẹp: nút chọn kỳ thu thành ô xổ (đỡ tràn).</summary>
        private void ApplyHeaderWidth()
        {
            var wide = _header.Width >= _pills.Width + S(420);
            if (_pills.Visible != wide) _pills.Visible = wide;
            if (_periodBox.Visible == wide) _periodBox.Visible = !wide;
        }

        /// <summary>Hàng biểu đồ tháng + "Cần chú ý": rộng thì 2 cột (64% / 36%), hẹp thì xếp dọc.</summary>
        private void BuildOverviewRow()
        {
            _chartMonthly.Dock = DockStyle.Fill;
            _attnView.Dock = DockStyle.Fill;
            _ovRow.Controls.Add(_chartMonthly, 0, 0);
            _ovRow.Controls.Add(_attnView, 1, 0);
            ApplyOverviewLayout(force: true);
            // Đổi cỡ: xếp lại SAU khi lượt đổi cỡ hiện tại xong (không sửa bố cục ngay trong lúc đang xếp).
            dashLayout.Resize += (_, _) =>
            {
                if (_ovPending || !IsHandleCreated) return;
                _ovPending = true;
                BeginInvoke(new Action(() => { _ovPending = false; if (!IsDisposed) ApplyOverviewLayout(); }));
            };
            _attnView.HeightChanged += (_, _) => ApplyOverviewLayout(force: true);
        }

        private void ApplyOverviewLayout(bool force = false)
        {
            var w = dashLayout.ClientSize.Width;
            if (w <= 0) return;
            var narrow = w < S(980);
            var chartH = S(330);
            var attnH = _attnView.DesiredHeight;
            if (narrow != _ovNarrow || force)
            {
                _ovNarrow = narrow;
                _ovRow.SuspendLayout();
                _ovRow.ColumnStyles.Clear();
                _ovRow.RowStyles.Clear();
                if (narrow)
                {
                    _ovRow.ColumnCount = 1;
                    _ovRow.RowCount = 2;
                    _ovRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                    _ovRow.RowStyles.Add(new RowStyle(SizeType.Absolute, S(300)));
                    _ovRow.RowStyles.Add(new RowStyle(SizeType.Absolute, attnH));
                    _ovRow.SetCellPosition(_chartMonthly, new TableLayoutPanelCellPosition(0, 0));
                    _ovRow.SetCellPosition(_attnView, new TableLayoutPanelCellPosition(0, 1));
                    _chartMonthly.Margin = new Padding(0, 0, 0, S(12));
                    _attnView.Margin = Padding.Empty;
                }
                else
                {
                    _ovRow.ColumnCount = 2;
                    _ovRow.RowCount = 1;
                    _ovRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64F));
                    _ovRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36F));
                    _ovRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                    _ovRow.SetCellPosition(_chartMonthly, new TableLayoutPanelCellPosition(0, 0));
                    _ovRow.SetCellPosition(_attnView, new TableLayoutPanelCellPosition(1, 0));
                    _chartMonthly.Margin = new Padding(0, 0, S(7), 0);
                    _attnView.Margin = new Padding(S(7), 0, 0, 0);
                }
                _ovRow.ResumeLayout(true);
            }
            else if (narrow && _ovRow.RowStyles.Count == 2) _ovRow.RowStyles[1].Height = attnH;

            var total = narrow ? S(300) + S(12) + attnH : Math.Max(chartH, attnH);
            total += _ovRow.Margin.Vertical;
            if (_ovRowIndex >= 0 && _ovRowIndex < _rows.Count)
            {
                var (c, p, h) = _rows[_ovRowIndex];
                if (h != total)
                {
                    _rows[_ovRowIndex] = (c, p, total);
                    var show = (_pages.Selected ?? PageOverview) == PageOverview; // Visible còn false khi màn chưa hiện
                    dashLayout.RowStyles[_ovRowIndex] = new RowStyle(SizeType.Absolute, show ? total : 0);
                }
            }
        }

        /// <summary>Khung biểu đồ: thẻ trắng bo góc có bóng nhẹ + tiêu đề. Trả về vùng vẽ bên trong thẻ.</summary>
        private Rectangle DrawCard(Graphics g, Rectangle rect, string title)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var card = new Rectangle(rect.X + S(2), rect.Y + S(2), rect.Width - S(4) - 1, rect.Height - S(6) - 1);
            if (card.Width < 20 || card.Height < 20) return Rectangle.Empty;
            DrawCardShape(g, card, S(14), hover: 0);
            var f = F_SegoeUISemibold11F;
            TextRenderer.DrawText(g, title, f, new Point(card.X + S(16), card.Y + S(14)), Theme.TextPrimary, TextFormatFlags.NoPrefix);
            return card;
        }

        /// <summary>Thẻ trắng bo góc + bóng mềm nhiều lớp (dùng chung cho thẻ số, biểu đồ, Cần chú ý).</summary>
        internal static void DrawCardShape(Graphics g, Rectangle card, int radius, double hover)
        {
            for (var i = 1; i <= 3; i++)
            {
                var sh = card;
                sh.Offset(0, i);
                using var sp = Anim.RoundRect(sh, radius);
                using var sb = new SolidBrush(Color.FromArgb((int)(8 + 8 * hover), 15, 23, 42));
                g.FillPath(sb, sp);
            }
            using var path = Anim.RoundRect(card, radius);
            g.FillPath(Brushes.White, path);
            using var pen = new Pen(Color.FromArgb(14, 15, 23, 42));
            g.DrawPath(pen, path);
        }

        private static void DrawLegendItem(Graphics g, ref float x, float y, Color color, string text, Font f, bool line, int s)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (line)
            {
                using var pen = new Pen(color, Math.Max(2, s / 4f)) { StartCap = LineCap.Round, EndCap = LineCap.Round };
                g.DrawLine(pen, x, y + s * 0.5f, x + s * 1.3f, y + s * 0.5f);
                x += s * 1.3f + s * 0.4f;
            }
            else
            {
                using var b = new SolidBrush(color);
                using var p = Anim.RoundRect(new RectangleF(x, y, s, s), s / 3f);
                g.FillPath(b, p);
                x += s + s * 0.4f;
            }
            var size = TextRenderer.MeasureText(g, text, f, Size.Empty, TextFormatFlags.NoPadding);
            TextRenderer.DrawText(g, text, f, new Point((int)x, (int)(y + s / 2f - size.Height / 2f)), Theme.TextSecondary, TextFormatFlags.NoPadding);
            x += size.Width + s;
        }

        private static readonly Color RevColor = ColorTranslator.FromHtml("#0EA5E9");
        private static readonly Color CostColor = ColorTranslator.FromHtml("#FBBF24");
        private static readonly Color ProfitColor = ColorTranslator.FromHtml("#16A34A");

        /// <summary>Cột đôi bo góc theo tháng (doanh thu / chi phí) + đường Lãi, số trên cột doanh thu; tháng lỗ ghi nhãn đỏ.</summary>
        private void PaintMonthlyChartModern(Graphics g, Rectangle rect)
        {
            var card = DrawCard(g, rect, "Doanh thu và chi phí theo tháng");
            if (card.IsEmpty) return;
            var small = F_SegoeUI85F;
            var bold = F_SegoeUISemibold85F;
            var ls = S(10);
            var legendW = 0f;
            foreach (var t in new[] { "Doanh thu", "Chi phí", "Lãi" })
                legendW += TextRenderer.MeasureText(g, t, small, Size.Empty, TextFormatFlags.NoPadding).Width + ls * 2.6f;
            var lx = card.Right - S(16) - legendW;
            if (lx > card.X + S(260))
            {
                var ly = card.Y + S(18);
                DrawLegendItem(g, ref lx, ly, RevColor, "Doanh thu", small, false, ls);
                DrawLegendItem(g, ref lx, ly, CostColor, "Chi phí", small, false, ls);
                DrawLegendItem(g, ref lx, ly, ProfitColor, "Lãi", small, true, ls);
            }

            var data = _report?.Monthly ?? new List<MonthlyProfitItem>();
            if (data.Count == 0) { DrawEmpty(g, card, "Chưa có dữ liệu trong kỳ này"); return; }

            int left = card.X + S(64), top = card.Y + S(58), right = card.Right - S(18), bottom = card.Bottom - S(34);
            float plotW = right - left, plotH = bottom - top;
            if (plotW <= 20 || plotH <= 20) return;
            var max = data.Max(m => Math.Max(m.Revenue, m.Cost));
            var minProfit = Math.Min(0, data.Min(m => m.Revenue - m.Cost));
            if (max <= 0) max = 1;
            var span = max - minProfit;
            float Y(decimal v) => bottom - (float)((v - minProfit) / span) * plotH;
            var zero = Y(0);

            using var gridPen = new Pen(ColorTranslator.FromHtml("#F1F5F9"));
            using var axisPen = new Pen(ColorTranslator.FromHtml("#E2E8F0"));
            for (var i = 0; i <= 4; i++)
            {
                var v = minProfit + span * i / 4;
                var y = Y(v);
                g.DrawLine(i == 0 && minProfit == 0 ? axisPen : gridPen, left, y, right, y);
                var label = ReportFormat.Money(v);
                var sz = TextRenderer.MeasureText(g, label, small, Size.Empty, TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, label, small, new Point(left - sz.Width - S(8), (int)(y - sz.Height / 2f)), ColorTranslator.FromHtml("#94A3B8"), TextFormatFlags.NoPadding);
            }
            if (minProfit < 0) g.DrawLine(axisPen, left, zero, right, zero);

            var groupW = plotW / data.Count;
            var barW = Math.Min(S(30), groupW * 0.3f);
            var pts = new List<PointF>();
            using var revBrush = new SolidBrush(RevColor);
            using var costBrush = new SolidBrush(CostColor);
            for (var i = 0; i < data.Count; i++)
            {
                var m = data[i];
                var cx = left + groupW * i + groupW / 2;
                void Bar(Brush b, float x, decimal v)
                {
                    var y = Y(v);
                    var h = zero - y;
                    if (h < 1) return;
                    using var p = TopRound(new RectangleF(x, y, barW, h), Math.Min(S(6), barW / 2));
                    g.FillPath(b, p);
                }
                Bar(revBrush, cx - barW - S(2), m.Revenue);
                Bar(costBrush, cx + S(2), m.Cost);
                if (m.Revenue > 0 && groupW > S(70))
                {
                    var t = ReportFormat.Money(m.Revenue);
                    var sz = TextRenderer.MeasureText(g, t, bold, Size.Empty, TextFormatFlags.NoPadding);
                    TextRenderer.DrawText(g, t, bold, new Point((int)(cx - S(2) - barW / 2 - sz.Width / 2f), (int)(Y(m.Revenue) - sz.Height - S(3))),
                        ColorTranslator.FromHtml("#0369A1"), TextFormatFlags.NoPadding);
                }
                pts.Add(new PointF(cx, Y(m.Revenue - m.Cost)));

                var label = MonthLabel(m.Month);
                var loss = m.Cost > m.Revenue;
                var lsz = TextRenderer.MeasureText(g, label, bold, Size.Empty, TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, label, bold, new Point((int)(cx - lsz.Width / 2f), bottom + S(8)),
                    loss ? Theme.Danger : ColorTranslator.FromHtml("#475569"), TextFormatFlags.NoPadding);
            }
            if (pts.Count > 1)
                using (var lp = new Pen(ProfitColor, Math.Max(2f, S(3))) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
                    g.DrawLines(lp, pts.ToArray());
            using var dotPen = new Pen(ProfitColor, Math.Max(2f, S(2)));
            var r = S(4);
            foreach (var p in pts)
            {
                g.FillEllipse(Brushes.White, p.X - r, p.Y - r, r * 2, r * 2);
                g.DrawEllipse(dotPen, p.X - r, p.Y - r, r * 2, r * 2);
            }
        }

        /// <summary>Hình chữ nhật bo 2 góc trên (cột biểu đồ).</summary>
        private static GraphicsPath TopRound(RectangleF r, float radius)
        {
            var path = new GraphicsPath();
            var d = Math.Max(1f, Math.Min(radius * 2, Math.Min(r.Width, r.Height * 2)));
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddLine(r.Right, r.Y + d / 2, r.Right, r.Bottom);
            path.AddLine(r.Right, r.Bottom, r.X, r.Bottom);
            path.CloseFigure();
            return path;
        }

        // =====================================================================================================================

        /// <summary>Lưới thẻ chia ĐỀU bề ngang: 4 cột khi rộng, 2 cột khi hẹp; mọi thẻ cao bằng nhau.</summary>
        [DesignerCategory("Code")]
        internal sealed class CardGrid : Panel
        {
            public CardGrid()
            {
                DoubleBuffered = true;
                AutoSize = true; // để bảng bố cục hỏi chiều cao theo bề ngang (GetPreferredSize)
                Margin = new Padding(0, 0, 0, 14);
            }

            private int Sx(int v) => LogicalToDeviceUnits(v);
            private int Gap => Sx(14);
            private int CardH => Sx(Controls.OfType<DashCard>().Any(c => c.Link is not null) ? 150 : 122);
            private int Cols(int w) => Controls.Count == 0 ? 1 : Math.Min(Controls.Count, w >= Sx(880) ? 4 : 2);

            private int HeightFor(int w)
            {
                var n = Controls.Count;
                if (n == 0) return 0;
                var c = Cols(w);
                var rows = (n + c - 1) / c;
                return rows * CardH + (rows - 1) * Gap;
            }

            public override Size GetPreferredSize(Size proposed)
            {
                var w = proposed.Width is > 0 and < 100000 ? proposed.Width : Width;
                return new Size(w, HeightFor(w));
            }

            protected override void OnLayout(LayoutEventArgs levent)
            {
                base.OnLayout(levent);
                var w = ClientSize.Width;
                if (w <= 0) return;
                var c = Cols(w);
                var cw = (w - Gap * (c - 1)) / c;
                var i = 0;
                foreach (Control x in Controls)
                {
                    x.Bounds = new Rectangle(i % c * (cw + Gap), i / c * (CardH + Gap), cw, CardH);
                    i++;
                }
            }

            private int _lastCols = -1;
            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                var c = Cols(Width);
                if (c != _lastCols && IsHandleCreated)
                {
                    _lastCols = c; // đổi số cột (4 <-> 2) -> chiều cao đổi: báo bảng bố cục xếp lại SAU lượt đổi cỡ này
                    BeginInvoke(new Action(() => { if (!IsDisposed) Parent?.PerformLayout(); }));
                }
                PerformLayout();
            }

            // Thêm / bớt thẻ: KHÔNG bắt bảng bố cục bên ngoài xếp lại ngay giữa lúc đang thêm (dễ lỗi "Index was out of range");
            // màn gọi dashLayout.PerformLayout() 1 lần sau khi dựng xong cả hàng thẻ (LoadAsync).
            protected override void OnControlAdded(ControlEventArgs e) { base.OnControlAdded(e); _lastCols = -1; }
        }

        /// <summary>1 thẻ số liệu: ô biểu tượng bo tròn + nhãn; số to (đơn vị nhỏ xám bên cạnh); nhãn màu so sánh; dòng "Xem … →".</summary>
        [DesignerCategory("Code")]
        internal sealed class DashCard : Control
        {
            private double _hover;
            private System.Windows.Forms.Timer? _anim;
            private static readonly Regex NumUnit = new(@"^\s*([-+−]?[\d.,/]+)\s*(\D.*)?$");

            public string IconText { get; init; } = "";
            public string Label { get; init; } = "";
            public string Value { get; init; } = "";
            public Color ValueColor { get; init; } = Theme.TextPrimary;
            public Color Accent { get; init; } = Theme.Primary;
            public string Sub { get; init; } = "";
            public Color? SubColor { get; init; }
            public string? Link { get; init; }
            public Action? Clicked { get; init; }

            public DashCard()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint
                         | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
                BackColor = Color.Transparent;
            }

            private int Sx(float v) => (int)Math.Round(LogicalToDeviceUnits(100) * v / 100f);

            protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); Animate(1); }
            protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); Animate(0); }
            protected override void OnClick(EventArgs e) { base.OnClick(e); Clicked?.Invoke(); }
            protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); Cursor = Clicked is null ? Cursors.Default : Cursors.Hand; }

            private void Animate(double to)
            {
                if (Clicked is null) return;
                var from = _hover;
                _anim?.Dispose();
                _anim = Anim.Tween(this, 150, p => { _hover = from + (to - from) * p; Invalidate(); }, () => { _anim = null; _hover = to; Invalidate(); });
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing) _anim?.Dispose();
                base.Dispose(disposing);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var lift = (int)Math.Round(Sx(2) * _hover);
                var card = new Rectangle(Sx(2), Sx(2) - lift, Width - Sx(4) - 1, Height - Sx(7) - 1);
                if (card.Width < 30 || card.Height < 30) return;
                DrawCardShape(g, card, Sx(14), _hover);
                if (_hover > 0)
                {
                    using var hp = Anim.RoundRect(card, Sx(14));
                    using var pen = new Pen(Color.FromArgb((int)(90 * _hover), Accent), 1.2f);
                    g.DrawPath(pen, hp);
                }

                // Ô biểu tượng + nhãn.
                var chip = new Rectangle(card.X + Sx(16), card.Y + Sx(14), Sx(32), Sx(32));
                using (var cb = new SolidBrush(Color.FromArgb(34, Accent)))
                using (var cp = Anim.RoundRect(chip, Sx(10)))
                    g.FillPath(cb, cp);
                {
                    var iconFont = F_SegoeUIEmoji12F;
                    TextRenderer.DrawText(g, IconText, iconFont, chip, Accent, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                }
                TextRenderer.DrawText(g, Label, F_LabelFont, new Rectangle(chip.Right + Sx(10), chip.Y, card.Right - chip.Right - Sx(24), chip.Height),
                        Theme.TextSecondary, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);

                // Số to + đơn vị nhỏ.
                var m = NumUnit.Match(Value);
                var num = m.Success ? m.Groups[1].Value : Value;
                var unit = m.Success ? m.Groups[2].Value.Trim() : "";
                var nf = F_SegoeUISemibold21F;
                var uf = F_SegoeUISemibold11F;
                var ny = chip.Bottom + Sx(4);
                var nsz = TextRenderer.MeasureText(g, num, nf, Size.Empty, TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, num, nf, new Point(card.X + Sx(15), ny), ValueColor, TextFormatFlags.NoPadding);
                if (unit.Length > 0)
                {
                    var usz = TextRenderer.MeasureText(g, unit, uf, Size.Empty, TextFormatFlags.NoPadding);
                    TextRenderer.DrawText(g, unit, uf, new Point(card.X + Sx(15) + nsz.Width + Sx(5), ny + nsz.Height - usz.Height - Sx(4)),
                        Theme.TextSecondary, TextFormatFlags.NoPadding);
                }

                // Dòng phụ: nhãn màu (so với kỳ trước) + phần chữ xám.
                var sy = ny + nsz.Height + Sx(4);
                var x = card.X + Sx(16);
                var sf = F_SegoeUI875F;
                if (Sub.Length > 0)
                {
                    string pill = "", rest = Sub;
                    if (SubColor is not null)
                    {
                        var k = Sub.IndexOf(" so với ", StringComparison.Ordinal);
                        if (k > 0) { pill = Sub[..k]; rest = Sub[(k + 1)..]; }
                        else { pill = Sub; rest = ""; }
                    }
                    if (pill.Length > 0)
                    {
                        var pf = F_SegoeUI825FFontStyleBold;
                        var psz = TextRenderer.MeasureText(g, pill, pf, Size.Empty, TextFormatFlags.NoPadding);
                        var pr = new Rectangle(x, sy, psz.Width + Sx(14), psz.Height + Sx(5));
                        using (var pb = new SolidBrush(Color.FromArgb(30, SubColor!.Value)))
                        using (var pp = Anim.RoundRect(pr, pr.Height / 2f))
                            g.FillPath(pb, pp);
                        TextRenderer.DrawText(g, pill, pf, pr, SubColor.Value, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                        x = pr.Right + Sx(6);
                        sy += (pr.Height - sf.Height) / 2;
                    }
                    if (rest.Length > 0)
                        TextRenderer.DrawText(g, rest, sf, new Rectangle(x, sy, card.Right - x - Sx(12), sf.Height + Sx(2)), Theme.TextSecondary,
                            TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                }

                // Dòng "Xem … →".
                if (Link is not null)
                {
                    var ly = card.Bottom - Sx(34);
                    using (var lp = new Pen(ColorTranslator.FromHtml("#F1F5F9"))) g.DrawLine(lp, card.X + Sx(16), ly, card.Right - Sx(16), ly);
                    var lf = F_SegoeUISemibold9F;
                    var linkColor = Anim.Mix(ModernLook.AccentStart, Color.Black, 0.2);
                    var lr = new Rectangle(card.X + Sx(16), ly, card.Width - Sx(32), Sx(34));
                    TextRenderer.DrawText(g, Link, lf, lr, linkColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
                    TextRenderer.DrawText(g, "→", lf, new Rectangle(lr.X + (int)(Sx(4) * _hover), lr.Y, lr.Width, lr.Height), linkColor,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.Right | TextFormatFlags.SingleLine);
                }
            }
        }

        /// <summary>Khung "Cần chú ý": mỗi việc 1 dòng thẻ (ô biểu tượng màu, chữ, số đếm to bên phải, ›), việc đỏ xếp trên;
        /// bấm cả dòng để mở. Đáy: thanh "Xe sẵn sàng chạy lúc này".</summary>
        [DesignerCategory("Code")]
        internal sealed class AttentionView : Control
        {
            internal sealed record Item(string Icon, string Text, int Count, bool Danger, Action Go);

            private List<Item> _items = new();
            private int _ready, _total;
            private int _hot = -1;
            public event EventHandler? HeightChanged;

            public AttentionView()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint
                         | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
                BackColor = Color.Transparent;
            }

            private int Sx(int v) => LogicalToDeviceUnits(v);
            private int RowH => Sx(46);
            private int Top0 => Sx(58);

            public int DesiredHeight => Top0 + Math.Max(1, _items.Count) * (RowH + Sx(6)) + (_total > 0 ? Sx(64) : Sx(14)) + Sx(8);

            public void SetData(IEnumerable<Item> items, int ready, int total)
            {
                var old = DesiredHeight;
                _items = items.OrderByDescending(i => i.Danger).ToList();
                _ready = ready;
                _total = total;
                _hot = -1;
                Invalidate();
                if (DesiredHeight != old) HeightChanged?.Invoke(this, EventArgs.Empty);
            }

            private Rectangle Card => new(Sx(2), Sx(2), Width - Sx(4) - 1, Height - Sx(6) - 1);

            private Rectangle RowRect(int i)
            {
                var c = Card;
                return new Rectangle(c.X + Sx(14), c.Y + Top0 + i * (RowH + Sx(6)), c.Width - Sx(28), RowH);
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                var hot = -1;
                for (var i = 0; i < _items.Count; i++) if (RowRect(i).Contains(e.Location)) { hot = i; break; }
                if (hot == _hot) return;
                _hot = hot;
                Cursor = hot >= 0 ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }

            protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); if (_hot >= 0) { _hot = -1; Invalidate(); } }

            protected override void OnMouseClick(MouseEventArgs e)
            {
                base.OnMouseClick(e);
                for (var i = 0; i < _items.Count; i++)
                    if (RowRect(i).Contains(e.Location)) { _items[i].Go(); return; }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var card = Card;
                if (card.Width < 40 || card.Height < 40) return;
                DrawCardShape(g, card, Sx(14), 0);

                var tf = F_SegoeUISemibold11F;
                TextRenderer.DrawText(g, "Cần chú ý", tf, new Point(card.X + Sx(16), card.Y + Sx(14)), Theme.TextPrimary, TextFormatFlags.NoPrefix);
                var danger = _items.Any(i => i.Danger);
                var badge = _items.Count == 0 ? "Ổn" : $"{_items.Count} việc";
                var badgeColor = _items.Count == 0 ? Theme.Success : danger ? Theme.Danger : Theme.Warning;
                var bf = F_SegoeUI825FFontStyleBold;
                var bsz = TextRenderer.MeasureText(g, badge, bf, Size.Empty, TextFormatFlags.NoPadding);
                var br = new Rectangle(card.Right - Sx(16) - bsz.Width - Sx(14), card.Y + Sx(16), bsz.Width + Sx(14), bsz.Height + Sx(6));
                Pill(g, br, badgeColor, badge, bf);

                var rf = F_SegoeUI975F;
                var cf = F_SegoeUI9FFontStyleBold;
                var icf = F_SegoeUIEmoji105F;
                if (_items.Count == 0)
                {
                    var r0 = RowRect(0);
                    TextRenderer.DrawText(g, "✅ Không có gì bất thường trong kỳ này.", rf, r0, Theme.Success,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPrefix);
                }
                for (var i = 0; i < _items.Count; i++)
                {
                    var it = _items[i];
                    var r = RowRect(i);
                    if (r.Bottom > card.Bottom - Sx(8)) break;
                    var tone = it.Danger ? Theme.Danger : Theme.Warning;
                    using (var rb = new SolidBrush(i == _hot ? ColorTranslator.FromHtml("#EEF2F7") : ColorTranslator.FromHtml("#F8FAFC")))
                    using (var rp = Anim.RoundRect(r, Sx(10)))
                        g.FillPath(rb, rp);
                    var chip = new Rectangle(r.X + Sx(9), r.Y + (r.Height - Sx(28)) / 2, Sx(28), Sx(28));
                    using (var cb = new SolidBrush(Color.FromArgb(30, tone)))
                    using (var cp = Anim.RoundRect(chip, Sx(8)))
                        g.FillPath(cb, cp);
                    TextRenderer.DrawText(g, it.Icon, icf, chip, tone, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

                    var count = it.Count > 99 ? "99+" : it.Count.ToString();
                    var csz = TextRenderer.MeasureText(g, count, cf, Size.Empty, TextFormatFlags.NoPadding);
                    var arrowW = Sx(18);
                    var cr = new Rectangle(r.Right - arrowW - Sx(8) - csz.Width - Sx(16), r.Y + (r.Height - csz.Height - Sx(6)) / 2, csz.Width + Sx(16), csz.Height + Sx(6));
                    Pill(g, cr, tone, count, cf);
                    TextRenderer.DrawText(g, "›", tf, new Rectangle(r.Right - arrowW - Sx(4), r.Y, arrowW, r.Height), ColorTranslator.FromHtml("#94A3B8"),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    TextRenderer.DrawText(g, it.Text, rf, new Rectangle(chip.Right + Sx(10), r.Y, cr.X - chip.Right - Sx(16), r.Height), Theme.TextPrimary,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
                }

                if (_total > 0)
                {
                    var fy = card.Bottom - Sx(58);
                    using (var lp = new Pen(ColorTranslator.FromHtml("#F1F5F9"))) g.DrawLine(lp, card.X + Sx(16), fy, card.Right - Sx(16), fy);
                    var ff = F_SegoeUI9F;
                    var fb = F_SegoeUI9FFontStyleBold;
                    var lr = new Rectangle(card.X + Sx(16), fy + Sx(10), card.Width - Sx(32), Sx(18));
                    TextRenderer.DrawText(g, "🚛 Xe sẵn sàng chạy lúc này", ff, lr, ColorTranslator.FromHtml("#475569"), TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                    TextRenderer.DrawText(g, $"{_ready} / {_total}", fb, lr, Theme.TextPrimary, TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
                    var track = new Rectangle(lr.X, lr.Bottom + Sx(8), lr.Width, Sx(8));
                    using (var tb = new SolidBrush(ColorTranslator.FromHtml("#E2E8F0")))
                    using (var tp = Anim.RoundRect(track, track.Height / 2f))
                        g.FillPath(tb, tp);
                    var fw = (int)Math.Round(track.Width * (double)_ready / _total);
                    if (fw > track.Height)
                    {
                        var fill = new Rectangle(track.X, track.Y, fw, track.Height);
                        using var lg = new LinearGradientBrush(new Rectangle(track.X - 1, track.Y, track.Width + 2, track.Height), ModernLook.AccentStart, ModernLook.AccentEnd, 0f);
                        using var fp = Anim.RoundRect(fill, track.Height / 2f);
                        g.FillPath(lg, fp);
                    }
                }
            }

            private static void Pill(Graphics g, Rectangle r, Color tone, string text, Font f)
            {
                using (var b = new SolidBrush(Color.FromArgb(30, tone)))
                using (var p = Anim.RoundRect(r, r.Height / 2f))
                    g.FillPath(b, p);
                TextRenderer.DrawText(g, text, f, r, tone, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
        }

        /// <summary>Nút chọn kỳ dạng viên thuốc liền nhau; kỳ đang chọn tô dải màu theo buổi.</summary>
        [DesignerCategory("Code")]
        internal sealed class PeriodPills : Control
        {
            private string[] _options = Array.Empty<string>();
            private int _selected = -1, _hot = -1;
            private Rectangle[] _rects = Array.Empty<Rectangle>();
            public event EventHandler<int>? Picked;

            public PeriodPills()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint
                         | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
                BackColor = Color.Transparent;
                Font = new Font("Segoe UI Semibold", 9.5F);
                Cursor = Cursors.Hand;
            }

            public string[] Options { get => _options; set { _options = value; Measure(); } }
            public int Selected { get => _selected; set { if (_selected == value) return; _selected = value; Invalidate(); } }

            private int Sx(int v) => LogicalToDeviceUnits(v);

            private void Measure()
            {
                var x = Sx(4);
                _rects = new Rectangle[_options.Length];
                for (var i = 0; i < _options.Length; i++)
                {
                    var w = TextRenderer.MeasureText(_options[i], Font).Width + Sx(16);
                    _rects[i] = new Rectangle(x, Sx(4), w, Sx(30));
                    x += w + Sx(2);
                }
                Size = new Size(x + Sx(2), Sx(38));
                Invalidate();
            }

            protected override void OnFontChanged(EventArgs e) { base.OnFontChanged(e); Measure(); }
            protected override void OnDpiChangedAfterParent(EventArgs e) { base.OnDpiChangedAfterParent(e); Measure(); }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                var hot = Array.FindIndex(_rects, r => r.Contains(e.Location));
                if (hot != _hot) { _hot = hot; Invalidate(); }
            }

            protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hot = -1; Invalidate(); }

            protected override void OnMouseClick(MouseEventArgs e)
            {
                base.OnMouseClick(e);
                var i = Array.FindIndex(_rects, r => r.Contains(e.Location));
                if (i >= 0 && i != _selected) Picked?.Invoke(this, i);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var bg = new Rectangle(0, 0, Width - 1, Height - 1);
                using (var path = Anim.RoundRect(bg, bg.Height / 2f))
                {
                    g.FillPath(Brushes.White, path);
                    using var pen = new Pen(Theme.BorderColor);
                    g.DrawPath(pen, path);
                }
                for (var i = 0; i < _rects.Length; i++)
                {
                    var r = _rects[i];
                    var on = i == _selected;
                    if (on)
                    {
                        using var lg = new LinearGradientBrush(new Rectangle(r.X - 1, r.Y, r.Width + 2, r.Height), ModernLook.AccentStart, ModernLook.AccentEnd, 0f);
                        using var p = Anim.RoundRect(r, r.Height / 2f);
                        g.FillPath(lg, p);
                    }
                    else if (i == _hot)
                    {
                        using var hb = new SolidBrush(ColorTranslator.FromHtml("#F1F5F9"));
                        using var p = Anim.RoundRect(r, r.Height / 2f);
                        g.FillPath(hb, p);
                    }
                    TextRenderer.DrawText(g, _options[i], Font, r, on ? Color.White : ColorTranslator.FromHtml("#475569"),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
                }
            }
        }
    }
}
