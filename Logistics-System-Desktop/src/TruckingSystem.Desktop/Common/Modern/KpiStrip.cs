using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace TruckingSystem.Desktop.Common
{
    /// <summary>HÀNG THẺ SỐ LIỆU đặt ở đầu màn danh sách (dưới tiêu đề): mỗi thẻ là 1 nhóm cần chú ý
    /// (Đang chạy, Chờ duyệt, Trễ hạn...). Bấm thẻ = lọc bảng đúng nhóm đó qua FilterBar (bấm lại =
    /// bỏ lọc); thẻ đang lọc được tô nổi. Tự chia cột theo bề ngang (màn hẹp thì xuống 2 hàng).
    /// Có vạch "đang tải" chạy ngang ở đáy (Busy = true) khi màn hình đang lấy dữ liệu.
    /// Cách dùng:
    ///   kpi.AddCard("run", "🚚", "Đang chạy", Theme.Primary, "status", "Đang chạy");
    ///   kpi.BindFilter(filterBar); KpiStrip.InsertRow(root, kpi, 1);
    ///   kpi.SetValue("run", 12, "3 chuyến giao, 9 lấy hàng");</summary>
    [DesignerCategory("Code")]
    public class KpiStrip : Panel
    {
        private sealed class CardDef
        {
            public required KpiCard Card { get; init; }
            public string? FilterKey { get; init; }
            public string? FilterValue { get; init; }
            public Action? OnClick { get; init; }
        }

        private readonly List<CardDef> _defs = new();
        private FilterBar? _bar;
        private bool _busy;
        private System.Windows.Forms.Timer? _busyTimer;
        private readonly Stopwatch _busyClock = new();
        private bool _settingHeight;

        public KpiStrip()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint
                     | ControlStyles.ResizeRedraw, true);
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            Margin = new Padding(0, 2, 0, 6);
            Height = 100;
        }

        private int Gap => Math.Max(8, (int)(Font.Height * 0.75));
        // Thẻ GỌN (≈ 5 dòng chữ) và hẹp vừa đủ để 6 thẻ nằm 1 hàng trên màn laptop — không chiếm chỗ của bảng.
        private int CardHeight => (int)(Font.Height * 5.0);
        private int MinCardWidth => (int)(Font.Height * 8.5);
        private int BusyLineHeight => Math.Max(3, Font.Height / 6);

        /// <summary>Thêm thẻ. filterKey/filterValue: bấm thẻ thì FilterBar chỉ lọc ô đó = giá trị
        /// đó; filterKey = null và onClick = null: thẻ "Tất cả" (bỏ mọi bộ lọc). onClick: tự xử lý
        /// (VD mở 1 form).</summary>
        public KpiCard AddCard(string key, string icon, string title, Color accent,
            string? filterKey = null, string? filterValue = null, bool alert = false, Action? onClick = null)
        {
            var card = new KpiCard { Key = key, Icon = icon, Title = title, Accent = accent, Alert = alert };
            var def = new CardDef { Card = card, FilterKey = filterKey, FilterValue = filterValue, OnClick = onClick };
            card.Click += (_, _) => OnCardClick(def);
            _defs.Add(def);
            Controls.Add(card);
            PerformLayout();
            return card;
        }

        public void SetValue(string key, int value, string? caption = null, double? progress = null)
        {
            var def = _defs.FirstOrDefault(d => d.Card.Key == key);
            if (def is null) return;
            def.Card.Value = value;
            if (caption is not null) def.Card.Caption = caption;
            if (progress is not null) def.Card.Progress = progress;
        }

        /// <summary>Nối với thanh lọc: bấm thẻ đổi bộ lọc, đổi bộ lọc thì tô lại thẻ đang chọn.</summary>
        public void BindFilter(FilterBar bar)
        {
            _bar = bar;
            bar.FilterChanged += (_, _) => SyncSelection();
            SyncSelection();
        }

        private void OnCardClick(CardDef def)
        {
            if (def.OnClick is not null) { def.OnClick(); return; }
            if (_bar is null) return;
            if (def.FilterKey is null || (def.Card.Selected && _bar.Choice(def.FilterKey) == def.FilterValue))
                _bar.SetOnly(null, null);           // thẻ "Tất cả", hoặc bấm lại thẻ đang chọn -> bỏ lọc
            else
                _bar.SetOnly(def.FilterKey, def.FilterValue);
        }

        private void SyncSelection()
        {
            if (_bar is null) return;
            var any = false;
            foreach (var d in _defs.Where(d => d.FilterKey is not null))
            {
                d.Card.Selected = _bar.Choice(d.FilterKey!) == d.FilterValue;
                any |= d.Card.Selected;
            }
            foreach (var d in _defs.Where(d => d.FilterKey is null && d.OnClick is null))
                d.Card.Selected = !any;
        }

        /// <summary>Vạch "đang tải" chạy ngang ở đáy hàng thẻ.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Busy
        {
            get => _busy;
            set
            {
                if (_busy == value) return;
                _busy = value;
                _busyTimer?.Dispose();
                _busyTimer = null;
                if (value)
                {
                    _busyClock.Restart();
                    _busyTimer = new System.Windows.Forms.Timer { Interval = 16 };
                    _busyTimer.Tick += (_, _) => { if (!IsDisposed) Invalidate(BusyBounds()); };
                    _busyTimer.Start();
                }
                Invalidate(BusyBounds());
            }
        }

        private Rectangle BusyBounds() => new(0, Height - BusyLineHeight - 1, Width, BusyLineHeight + 1);

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            if (_defs.Count == 0 || ClientSize.Width <= 0) return;
            var gap = Gap;
            var n = _defs.Count;
            var cols = Math.Max(1, Math.Min(n, (ClientSize.Width + gap) / (MinCardWidth + gap)));
            var rows = (n + cols - 1) / cols;
            var cardW = (ClientSize.Width - gap * (cols - 1)) / cols;
            for (var i = 0; i < n; i++)
            {
                int r = i / cols, c = i % cols;
                _defs[i].Card.SetBounds(c * (cardW + gap), r * (CardHeight + gap), cardW, CardHeight);
            }
            var h = rows * CardHeight + (rows - 1) * gap + BusyLineHeight + gap / 2;
            if (Height != h && !_settingHeight)
            {
                _settingHeight = true;
                try { Height = h; } finally { _settingHeight = false; }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (!_busy) return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var track = new Rectangle(0, Height - BusyLineHeight, Width, BusyLineHeight);
            using (var tb = new SolidBrush(Anim.Mix(BackColor, Theme.Primary, 0.12))) g.FillRectangle(tb, track);
            var t = _busyClock.ElapsedMilliseconds % 1300 / 1300.0;
            var segW = Math.Max(40f, Width * 0.28f);
            var x = (float)(-segW + (Width + segW) * t);
            var seg = new RectangleF(x, track.Y, segW, track.Height);
            using var path = Anim.RoundRect(seg, track.Height / 2f);
            using var sb = new SolidBrush(Theme.Primary);
            g.FillPath(sb, path);
        }

        /// <summary>Chèn 1 hàng MỚI (AutoSize) vào TableLayoutPanel 1 cột ở vị trí <paramref name="atRow"/>,
        /// đẩy các hàng phía dưới xuống — không phải sửa file Designer.</summary>
        public static void InsertRow(TableLayoutPanel root, Control control, int atRow)
        {
            root.SuspendLayout();
            var positions = root.Controls.Cast<Control>()
                .Select(c => (Control: c, Pos: root.GetPositionFromControl(c)))
                .OrderByDescending(x => x.Pos.Row)
                .ToList();
            root.RowCount += 1;
            root.RowStyles.Insert(Math.Min(atRow, root.RowStyles.Count), new RowStyle(SizeType.AutoSize));
            foreach (var (c, pos) in positions)
                if (pos.Row >= atRow) root.SetRow(c, pos.Row + 1);
            root.Controls.Add(control, 0, atRow);
            root.SetColumnSpan(control, Math.Max(1, root.ColumnCount));
            root.ResumeLayout(true);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _busyTimer?.Dispose();
            base.Dispose(disposing);
        }
    }
}
