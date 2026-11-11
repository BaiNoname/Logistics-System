using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_TripManagement
{
    /// <summary>Nội dung thẻ thông tin (chuyến hoặc cả đơn) — màn Chuyến tính sẵn rồi đưa vào <see cref="SelectedTripBar.ShowCard"/>.</summary>
    public sealed class TripCardInfo
    {
        public string Title { get; set; } = "";
        /// <summary>Nhãn màu cạnh tiêu đề (chặng, trạng thái, hạn...).</summary>
        public List<(string Text, GridDecor.Tone Tone)> Pills { get; } = new();
        /// <summary>"thuộc đơn" + chữ bấm được (mở thẻ của cả đơn).</summary>
        public string? LinkPrefix { get; set; }
        public string? LinkText { get; set; }
        public string FromCaption { get; set; } = "ĐI TỪ";
        public string ToCaption { get; set; } = "ĐẾN";
        public string? From { get; set; }
        public string? To { get; set; }
        /// <summary>Chữ giữa đường đi: km, thời gian, "qua kho ...".</summary>
        public string? RouteMid { get; set; }
        /// <summary>Các ô thông tin xếp ngang: tiêu đề + các dòng (tên — giá trị).</summary>
        public List<(string Title, List<(string Key, string Value)> Rows)> Boxes { get; } = new();
        public string Next { get; set; } = "";
        /// <summary>Đang bị chặn / cần làm ở màn khác -> ô việc tiếp theo màu cam.</summary>
        public bool Blocked { get; set; }
        /// <summary>Đã xong -> ô việc tiếp theo màu xanh lá.</summary>
        public bool Done { get; set; }
        /// <summary>Chữ trên nút chính cạnh "việc tiếp theo" (null = không có nút).</summary>
        public string? ActionText { get; set; }
        /// <summary>Màu vạch bên trái thẻ (theo trạng thái / màu nhóm đơn).</summary>
        public Color Accent { get; set; } = Theme.TextSecondary;
    }

    /// <summary>THẺ THÔNG TIN nằm ngay trên bảng chuyến (bản mới — mẫu "cách A" đã duyệt):
    ///  • Dòng đầu: "Chuyến #34" + nhãn chặng / trạng thái / hạn + "thuộc đơn DH-0012" (bấm để xem cả đơn).
    ///  • Đường đi: điểm đi bên trái → điểm đến bên phải, giữa ghi km · thời gian.
    ///  • 3 ô thông tin xếp ngang: Xe và tài xế · Thời gian · Hàng và kho (thay cho 1 dòng chữ dài nối bằng "·").
    ///  • Ô "việc tiếp theo" tô màu + NÚT CHÍNH ngay bên cạnh (Bắt đầu / Hoàn tất / Duyệt...).
    /// Bấm dòng ĐƠN trong bảng -> cùng thẻ này hiện thông tin cả đơn (tiến độ các chặng, việc tiếp theo của đơn).
    /// Toàn bộ vẽ bằng code (SelectedTripBar.Designer.cs giữ nguyên, phần cũ không dùng nữa).</summary>
    public partial class SelectedTripBar : UserControl
    {
        /// <summary>Bấm chữ đơn ("thuộc đơn DH-0012").</summary>
        public event EventHandler? LinkClicked;
        /// <summary>Bấm nút chính cạnh "việc tiếp theo".</summary>
        public event EventHandler? ActionClicked;

        private static readonly Color CardBack = Color.White;
        private static readonly Color Sunk = ColorTranslator.FromHtml("#F8FAFC");
        private static readonly Color Line = ColorTranslator.FromHtml("#E2E8F0");
        private static readonly Color Line2 = ColorTranslator.FromHtml("#EEF2F7");
        private static readonly Color Muted = ColorTranslator.FromHtml("#64748B");
        private static readonly Color Ink = ColorTranslator.FromHtml("#0F172A");
        private static readonly Color RouteLine = ColorTranslator.FromHtml("#0EA5E9");

        private readonly Font _fTitle = new("Segoe UI", 12F, FontStyle.Bold);
        private readonly Font _fPill = new("Segoe UI", 8.5F, FontStyle.Bold);
        private readonly Font _fText = new("Segoe UI", 9.5F);
        private readonly Font _fBold = new("Segoe UI", 9.5F, FontStyle.Bold);
        private readonly Font _fLink = new("Segoe UI", 9.5F, FontStyle.Bold | FontStyle.Underline);
        private readonly Font _fCaption = new("Segoe UI", 7.5F, FontStyle.Bold);
        private readonly Font _fSmall = new("Segoe UI", 8.5F);

        private readonly Button _action = new();
        private TripCardInfo? _info;
        private string _emptyTitle = "Chưa chọn chuyến";
        private string _emptyText = "Bấm 1 dòng trong bảng bên dưới để xem thông tin và việc cần làm.";
        private Rectangle _linkRect = Rectangle.Empty;
        private Rectangle _calloutRect = Rectangle.Empty;
        private int _height;

        /// <summary>Dòng gợi ý chuột góc phải (mỗi màn tự đặt).</summary>
        public string Hint { get; set; } = "🖱 Chuột phải: mọi thao tác · Nhấp đúp: xem lộ trình";

        public SelectedTripBar()
        {
            InitializeComponent();
            Dock = DockStyle.Top;
            // Bố cục cũ (Designer: 3 dòng chữ) không dùng nữa — thẻ mới tự vẽ.
            Controls.Remove(layout);
            Padding = new Padding(0);
            BackColor = CardBack;
            DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

            ButtonStyles.Primary(_action);
            _action.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _action.Visible = false;
            _action.TabStop = true;
            _action.Click += (_, _) => ActionClicked?.Invoke(this, EventArgs.Empty);
            Controls.Add(_action);

            MouseMove += (_, e) => Cursor = _linkRect.Contains(e.Location) ? Cursors.Hand : Cursors.Default;
            MouseClick += (_, e) => { if (e.Button == MouseButtons.Left && _linkRect.Contains(e.Location)) LinkClicked?.Invoke(this, EventArgs.Empty); };
            Disposed += (_, _) =>
            {
                foreach (var f in new[] { _fTitle, _fPill, _fText, _fBold, _fLink, _fCaption, _fSmall }) f.Dispose();
            };
            Recalc();
        }

        /// <summary>Không có chuyến / đơn nào được chọn (bảng trống / đang tải).</summary>
        public void ShowEmpty(string? message = null, string? title = null)
        {
            _info = null;
            _emptyTitle = title ?? "Chưa chọn chuyến";
            _emptyText = message ?? "Bấm 1 dòng chuyến để xem thông tin và việc cần làm · bấm dòng ĐƠN để xem cả đơn.";
            _action.Visible = false;
            Recalc();
        }

        public void ShowCard(TripCardInfo info)
        {
            _info = info;
            _action.Text = info.ActionText ?? "";
            _action.Visible = !string.IsNullOrWhiteSpace(info.ActionText);
            Recalc();
        }

        private int S(int v) => LogicalToDeviceUnits(v);

        // ------------------------------------------------------------ kích thước

        private int BoxHeight()
        {
            if (_info is null || _info.Boxes.Count == 0) return 0;
            var rows = _info.Boxes.Max(b => b.Rows.Count);
            return S(26) + rows * S(22) + S(8);
        }

        private void Recalc()
        {
            int h;
            if (_info is null) h = S(14) + S(26) + S(22) + S(14);
            else
            {
                h = S(12) + S(28) + S(8);
                if (_info.From is not null || _info.To is not null) h += S(48) + S(8);
                var bh = BoxHeight();
                if (bh > 0) h += bh + S(8);
                if (!string.IsNullOrWhiteSpace(_info.Next)) h += S(42) + S(4);
                h += S(10);
            }
            _height = h;
            PlaceAction();
            if (Height != h) Height = h;
            Parent?.PerformLayout();
            Invalidate();
        }

        public override Size GetPreferredSize(Size proposedSize)
            => new(Math.Max(proposedSize.Width, S(300)), _height > 0 ? _height : S(80));

        protected override void OnDpiChangedAfterParent(EventArgs e) { base.OnDpiChangedAfterParent(e); Recalc(); }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            PlaceAction();
        }

        /// <summary>Nút chính nằm ngay bên phải ô "việc tiếp theo" (trên nền trắng — góc bo của nút không lộ màu khác).</summary>
        private void PlaceAction()
        {
            if (_info is null || !_action.Visible || string.IsNullOrWhiteSpace(_info.Next)) return;
            var calloutTop = _height - S(10) - S(4) - S(42);
            var w = Math.Max(S(130), TextRenderer.MeasureText(_action.Text, _action.Font).Width + S(28));
            var h = S(32);
            _action.Bounds = new Rectangle(Width - S(16) - w, calloutTop + (S(42) - h) / 2, w, h);
        }

        // ------------------------------------------------------------ vẽ

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(CardBack);
            using (var pen = new Pen(Line)) g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);

            var accent = _info?.Accent ?? Line;
            using (var b = new SolidBrush(accent)) g.FillRectangle(b, 0, 0, S(5), Height);

            var left = S(5) + S(14);
            var right = Width - S(16);
            _linkRect = Rectangle.Empty;

            if (_info is null)
            {
                TextRenderer.DrawText(g, _emptyTitle, _fTitle, new Rectangle(left, S(14), right - left, S(26)), Ink, Single());
                TextRenderer.DrawText(g, _emptyText, _fText, new Rectangle(left, S(14) + S(26), right - left, S(22)), Muted, Single());
                return;
            }
            var info = _info;
            var y = S(12);

            // 1) Dòng đầu: tiêu đề · nhãn · "thuộc đơn ..." · gợi ý chuột (bên phải, chỉ khi còn chỗ).
            var x = left;
            var lineH = S(28);
            x += Draw(g, info.Title, _fTitle, x, y, lineH, Ink) + S(10);
            foreach (var (text, tone) in info.Pills)
            {
                var w = TextRenderer.MeasureText(g, text, _fPill, Size.Empty, TextFormatFlags.NoPadding).Width + S(18);
                if (x + w > right) break;
                var r = new Rectangle(x, y + (lineH - S(22)) / 2, w, S(22));
                using (var path = Anim.RoundRect(r, S(11)))
                using (var bb = new SolidBrush(tone.Back)) g.FillPath(bb, path);
                TextRenderer.DrawText(g, text, _fPill, r, tone.Fore, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
                x += w + S(6);
            }
            if (!string.IsNullOrWhiteSpace(info.LinkText))
            {
                x += S(4);
                if (!string.IsNullOrWhiteSpace(info.LinkPrefix)) x += Draw(g, info.LinkPrefix!, _fText, x, y, lineH, Muted) + S(4);
                var lw = Math.Min(TextRenderer.MeasureText(g, info.LinkText, _fLink, Size.Empty, TextFormatFlags.NoPadding).Width + S(4), Math.Max(0, right - x));
                if (lw > 0)
                {
                    _linkRect = new Rectangle(x, y, lw, lineH);
                    TextRenderer.DrawText(g, info.LinkText, _fLink, _linkRect, Theme.Primary, Single());
                    x += lw + S(12);
                }
            }
            var hint = Hint;
            var hw = TextRenderer.MeasureText(g, hint, _fSmall, Size.Empty, TextFormatFlags.NoPadding).Width + S(4);
            if (x + hw <= right)
                TextRenderer.DrawText(g, hint, _fSmall, new Rectangle(right - hw, y, hw, lineH), Muted, Single());
            y += lineH + S(8);

            // 2) Đường đi.
            if (info.From is not null || info.To is not null)
            {
                var r = new Rectangle(left, y, right - left, S(48));
                using (var path = Anim.RoundRect(r, S(10)))
                using (var bb = new SolidBrush(Sunk)) g.FillPath(bb, path);
                var pad = S(14);
                var midW = Math.Min(S(260), r.Width / 4);
                var sideW = (r.Width - pad * 2 - midW - S(20)) / 2;
                var fromR = new Rectangle(r.X + pad, r.Y + S(6), sideW, r.Height - S(12));
                var toR = new Rectangle(r.Right - pad - sideW, r.Y + S(6), sideW, r.Height - S(12));
                var midR = new Rectangle(fromR.Right + S(10), r.Y + S(6), midW, r.Height - S(12));
                DrawPoint(g, info.FromCaption, info.From ?? "?", fromR, TextFormatFlags.Left);
                DrawPoint(g, info.ToCaption, info.To ?? "?", toR, TextFormatFlags.Right);
                if (!string.IsNullOrWhiteSpace(info.RouteMid))
                    TextRenderer.DrawText(g, info.RouteMid, _fSmall, new Rectangle(midR.X, midR.Y, midR.Width, midR.Height / 2 + S(2)), Muted,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.Bottom | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
                var ly = midR.Y + midR.Height / 2 + S(6);
                using (var pen = new Pen(RouteLine, S(2)) { DashStyle = DashStyle.Dash })
                    g.DrawLine(pen, midR.X, ly, midR.Right - S(8), ly);
                using (var bb = new SolidBrush(RouteLine))
                    g.FillPolygon(bb, new[] { new Point(midR.Right, ly), new Point(midR.Right - S(8), ly - S(4)), new Point(midR.Right - S(8), ly + S(4)) });
                y = r.Bottom + S(8);
            }

            // 3) Các ô thông tin xếp ngang.
            var bh = BoxHeight();
            if (bh > 0)
            {
                var n = info.Boxes.Count;
                var gap = S(8);
                var bw = (right - left - gap * (n - 1)) / n;
                for (var i = 0; i < n; i++)
                {
                    var (title, rows) = info.Boxes[i];
                    var r = new Rectangle(left + i * (bw + gap), y, bw, bh);
                    using (var path = Anim.RoundRect(r, S(10)))
                    using (var pen = new Pen(Line2)) g.DrawPath(pen, path);
                    var ix = r.X + S(12); var iw = r.Width - S(24);
                    TextRenderer.DrawText(g, title.ToUpperInvariant(), _fCaption, new Rectangle(ix, r.Y + S(6), iw, S(18)), Muted, Single());
                    var ry = r.Y + S(26);
                    foreach (var (key, value) in rows)
                    {
                        var kw = Math.Min(TextRenderer.MeasureText(g, key, _fText, Size.Empty, TextFormatFlags.NoPadding).Width + S(4), iw * 11 / 20);
                        TextRenderer.DrawText(g, key, _fText, new Rectangle(ix, ry, kw, S(22)), Muted, Single());
                        TextRenderer.DrawText(g, value, _fBold, new Rectangle(ix + kw + S(8), ry, iw - kw - S(8), S(22)), Ink,
                            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis
                            | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
                        ry += S(22);
                    }
                }
                y += bh + S(8);
            }

            // 4) Việc tiếp theo (+ nút chính nằm trong ô này, sát phải).
            if (!string.IsNullOrWhiteSpace(info.Next))
            {
                var tone = info.Done ? GridDecor.Green : info.Blocked ? GridDecor.Amber : GridDecor.Blue;
                var calloutRight = _action.Visible ? _action.Left - S(10) : right;
                var r = new Rectangle(left, y, Math.Max(S(40), calloutRight - left), S(42));
                _calloutRect = r;
                using (var path = Anim.RoundRect(r, S(10)))
                using (var bb = new SolidBrush(tone.Back)) g.FillPath(bb, path);
                var textRight = r.Right - S(12);
                TextRenderer.DrawText(g, "👉 " + info.Next, _fBold, new Rectangle(r.X + S(12), r.Y, Math.Max(0, textRight - r.X - S(12)), r.Height), tone.Fore,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
        }

        private void DrawPoint(Graphics g, string caption, string text, Rectangle r, TextFormatFlags align)
        {
            TextRenderer.DrawText(g, caption, _fCaption, new Rectangle(r.X, r.Y, r.Width, S(16)), Muted, align | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(g, text, _fBold, new Rectangle(r.X, r.Y + S(16), r.Width, r.Height - S(16)), Ink,
                align | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        private static TextFormatFlags Single()
            => TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis
               | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding; // đo và vẽ cùng kiểu -> không bị cắt "…" oan

        /// <summary>Vẽ 1 đoạn chữ trên dòng đầu, trả về bề ngang đã dùng.</summary>
        private int Draw(Graphics g, string text, Font font, int x, int y, int h, Color color)
        {
            var w = TextRenderer.MeasureText(g, text, font, Size.Empty, TextFormatFlags.NoPadding).Width;
            TextRenderer.DrawText(g, text, font, new Rectangle(x, y, w + 2, h), color, Single() & ~TextFormatFlags.EndEllipsis);
            return w;
        }
    }
}
