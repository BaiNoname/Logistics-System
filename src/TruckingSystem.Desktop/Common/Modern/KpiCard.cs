using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;

namespace TruckingSystem.Desktop.Common
{
    /// <summary>THẺ SỐ LIỆU (KPI) kiểu dashboard hiện đại: tiêu đề nhỏ, con số lớn, dòng phụ, biểu
    /// tượng trong vòng tròn màu nhạt, vạch màu nhấn bên trái.
    ///  • Số ĐẾM DẦN tới giá trị mới (ease-out ~0,65 s) mỗi khi dữ liệu tải lại.
    ///  • Rê chuột: thẻ "nhấc lên" nhẹ, bóng đổ đậm hơn, viền ngả màu nhấn.
    ///  • Thẻ cảnh báo (Alert) có số &gt; 0: chấm đỏ "nhịp tim" ở góc để gây chú ý.
    ///  • Progress (0..1, tuỳ chọn): thanh tiến độ mảnh ở đáy thẻ, chạy dâng lên khi đổi giá trị.
    ///  • Selected: thẻ đang là bộ lọc hiện tại — nền ngả màu nhấn + viền đậm.
    /// Mọi kích thước tính theo cỡ chữ (Font.Height) nên đúng ở mọi mức phóng to màn hình.</summary>
    [DesignerCategory("Code")]
    public class KpiCard : Control
    {
        private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

        private int _value;
        private double _shown;
        private double _hover;
        private double _pulse;
        private double? _progress;
        private double _progressShown;
        private bool _selected;
        private System.Windows.Forms.Timer? _countTimer, _hoverTimer, _progressTimer, _pulseTimer;
        private readonly Stopwatch _pulseClock = new();
        private Font? _titleFont, _valueFont, _captionFont, _iconFont;

        public KpiCard()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint
                     | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            Cursor = Cursors.Hand;
            Margin = new Padding(0);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Key { get; set; } = "";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Icon { get; set; } = "📊";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Title { get; set; } = "";

        private string _caption = "";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Caption { get => _caption; set { _caption = value ?? ""; Invalidate(); } }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color Accent { get; set; } = Theme.Primary;

        /// <summary>Thẻ cảnh báo: có số &gt; 0 thì chấm đỏ nhấp nháy.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Alert { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Selected
        {
            get => _selected;
            set { if (_selected == value) return; _selected = value; Invalidate(); }
        }

        /// <summary>Đổi số — số trên thẻ đếm dần từ giá trị đang hiện tới giá trị mới.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Value
        {
            get => _value;
            set
            {
                var from = _shown;
                _value = value;
                _countTimer?.Dispose();
                _countTimer = Anim.Tween(this, 650, p => { _shown = from + (value - from) * p; Invalidate(); },
                    () => { _countTimer = null; _shown = value; Invalidate(); });
                UpdatePulse();
            }
        }

        /// <summary>Thanh tiến độ ở đáy thẻ (0..1); null = không vẽ.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double? Progress
        {
            get => _progress;
            set
            {
                _progress = value is double v ? Math.Clamp(v, 0, 1) : null;
                var from = _progressShown;
                var to = _progress ?? 0;
                _progressTimer?.Dispose();
                _progressTimer = Anim.Tween(this, 700, p => { _progressShown = from + (to - from) * p; Invalidate(); },
                    () => { _progressTimer = null; _progressShown = to; Invalidate(); });
            }
        }

        // ------------------------------------------------------------ rê chuột

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); AnimateHover(1); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); AnimateHover(0); }

        private void AnimateHover(double target)
        {
            var from = _hover;
            _hoverTimer?.Dispose();
            _hoverTimer = Anim.Tween(this, 160, p => { _hover = from + (target - from) * p; Invalidate(); },
                () => { _hoverTimer = null; _hover = target; Invalidate(); });
        }

        // ------------------------------------------------------------ chấm cảnh báo nhịp tim

        private void UpdatePulse()
        {
            var on = Alert && _value > 0;
            if (on && _pulseTimer is null)
            {
                _pulseClock.Restart();
                _pulseTimer = new System.Windows.Forms.Timer { Interval = 40 };
                _pulseTimer.Tick += (_, _) =>
                {
                    if (IsDisposed) return;
                    _pulse = _pulseClock.ElapsedMilliseconds % 1600 / 1600.0;
                    Invalidate(AlertDotBounds(inflate: true));
                };
                _pulseTimer.Start();
            }
            else if (!on && _pulseTimer is not null)
            {
                _pulseTimer.Dispose();
                _pulseTimer = null;
                Invalidate();
            }
        }

        // ------------------------------------------------------------ vẽ

        private float U => Font.Height / 17f; // 1 "đơn vị" theo cỡ chữ (17 px ở 100%)
        private int S(float v) => (int)Math.Round(v * U);

        private Rectangle CardBounds()
        {
            var lift = (int)Math.Round(S(2) * _hover);
            return new Rectangle(S(2), S(3) - lift, Width - S(4) - 1, Height - S(9));
        }

        private Rectangle IconBounds(Rectangle card)
        {
            var size = S(34);
            return new Rectangle(card.Right - size - S(12), card.Y + S(10), size, size);
        }

        private Rectangle AlertDotBounds(bool inflate)
        {
            var icon = IconBounds(CardBounds());
            var d = S(10);
            var r = new Rectangle(icon.Right - d + S(2), icon.Y - S(2), d, d);
            if (inflate) r.Inflate(S(10), S(10));
            return r;
        }

        private void EnsureFonts()
        {
            if (_titleFont is not null) return;
            _titleFont = new Font(Font.FontFamily, Font.SizeInPoints * 0.9f, FontStyle.Bold);
            _valueFont = new Font(Font.FontFamily, Font.SizeInPoints * 1.65f, FontStyle.Bold);
            _captionFont = new Font(Font.FontFamily, Font.SizeInPoints * 0.88f, FontStyle.Regular);
            _iconFont = new Font("Segoe UI Emoji", Font.SizeInPoints * 1.15f, FontStyle.Regular);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            DisposeFonts();
            base.OnFontChanged(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            EnsureFonts();
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            var card = CardBounds();
            if (card.Width < 20 || card.Height < 20) return;
            var radius = S(12);

            // Bóng đổ mềm (vài lớp mờ dần), đậm hơn khi rê chuột.
            for (var i = 1; i <= 3; i++)
            {
                var shadow = card;
                shadow.Offset(0, i + (int)Math.Round(S(2) * _hover));
                using var sp = Anim.RoundRect(shadow, radius);
                using var sb = new SolidBrush(Color.FromArgb((int)(9 + 9 * _hover), 15, 23, 42));
                g.FillPath(sb, sp);
            }

            using var path = Anim.RoundRect(card, radius);
            var bg = _selected ? Anim.Mix(Color.White, Accent, 0.08) : Color.White;
            using (var bb = new SolidBrush(bg)) g.FillPath(bb, path);

            // Vạch màu nhấn bên trái (cắt theo góc bo).
            var clip = g.Clip;
            g.SetClip(path);
            using (var ab = new SolidBrush(Accent)) g.FillRectangle(ab, card.X, card.Y, S(5), card.Height);

            // Thanh tiến độ mảnh ở đáy thẻ.
            if (_progress is not null)
            {
                var track = new Rectangle(card.X, card.Bottom - S(5), card.Width, S(5));
                using (var tb = new SolidBrush(Anim.Mix(Color.White, Accent, 0.15))) g.FillRectangle(tb, track);
                var fill = track;
                fill.Width = (int)Math.Round(track.Width * _progressShown);
                using var fb = new LinearGradientBrush(track, Anim.Mix(Accent, Color.White, 0.25), Accent, LinearGradientMode.Horizontal);
                if (fill.Width > 0) g.FillRectangle(fb, fill);
            }
            g.Clip = clip;

            var border = _selected ? Accent : Anim.Mix(Theme.BorderColor, Accent, 0.45 * _hover);
            using (var pen = new Pen(border, _selected ? S(2) : 1)) g.DrawPath(pen, path);

            // Biểu tượng trong vòng tròn màu nhạt.
            var icon = IconBounds(card);
            using (var ib = new SolidBrush(Color.FromArgb(_selected ? 46 : 30, Accent))) g.FillEllipse(ib, icon);
            TextRenderer.DrawText(g, Icon, _iconFont, icon, Accent,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            // Chữ: tiêu đề (xám, in hoa nhỏ) — con số — dòng phụ.
            var left = card.X + S(18);
            var textWidth = icon.Left - left - S(6);
            var titleRect = new Rectangle(left, card.Y + S(9), textWidth, _titleFont!.Height);
            TextRenderer.DrawText(g, Title.ToUpper(Vi), _titleFont, titleRect, Theme.TextSecondary,
                TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);

            var valueText = ((int)Math.Round(_shown)).ToString("N0", Vi);
            var valueRect = new Rectangle(left - S(1), titleRect.Bottom, card.Right - left - S(10), _valueFont!.Height);
            TextRenderer.DrawText(g, valueText, _valueFont, valueRect, _value > 0 && Alert ? Accent : Theme.TextPrimary,
                TextFormatFlags.Left | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);

            if (_caption.Length > 0)
            {
                var capRect = new Rectangle(left, valueRect.Bottom - S(2), card.Right - left - S(12), _captionFont!.Height);
                TextRenderer.DrawText(g, _caption, _captionFont, capRect, Theme.TextSecondary,
                    TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            }

            // Chấm cảnh báo + vòng sóng lan ra (nhịp tim).
            if (Alert && _value > 0)
            {
                var dot = AlertDotBounds(inflate: false);
                var grow = (float)(_pulse * S(9));
                var ring = RectangleF.Inflate(dot, grow, grow);
                using (var rb = new SolidBrush(Color.FromArgb((int)(110 * (1 - _pulse)), Theme.Danger))) g.FillEllipse(rb, ring);
                using (var db = new SolidBrush(Theme.Danger)) g.FillEllipse(db, dot);
                using var wp = new Pen(Color.White, Math.Max(1, S(1.5f)));
                g.DrawEllipse(wp, dot);
            }
        }

        private void DisposeFonts()
        {
            _titleFont?.Dispose(); _valueFont?.Dispose(); _captionFont?.Dispose(); _iconFont?.Dispose();
            _titleFont = _valueFont = _captionFont = _iconFont = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _countTimer?.Dispose(); _hoverTimer?.Dispose(); _progressTimer?.Dispose(); _pulseTimer?.Dispose();
                DisposeFonts();
            }
            base.Dispose(disposing);
        }
    }
}
