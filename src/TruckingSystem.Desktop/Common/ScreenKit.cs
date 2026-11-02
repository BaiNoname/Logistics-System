namespace TruckingSystem.Desktop.Common
{
    /// <summary>BỘ DÙNG CHUNG cho các màn danh sách (mẫu UX đợt 6 đã duyệt):
    ///  • Overlay(grid, …): bảng KHÔNG để trắng — đang tải thì hiện "⏳ Đang tải…" (khung xám); không có dòng nào thì
    ///    ghi câu giải thích + nút làm ngay ("Bỏ lọc" khi bộ lọc đang ẩn hết, "➕ Tạo đơn" khi chưa có dữ liệu).
    ///  • Shortcuts(screen, grid, …): F5 làm mới · Ctrl+N tạo mới · Enter mở chi tiết dòng đang chọn.
    ///  • Help(screen, …): nút "?" góc trên phải màn — thẻ "Màn này dùng để làm gì · làm theo thứ tự nào · phím tắt".
    ///    Lần đầu tài khoản mở màn đó thẻ tự hiện (nhớ ở %AppData%/LogisticsSystem/help-seen.txt).</summary>
    public static class ScreenKit
    {
        // ================= Bảng trống / đang tải =================

        public sealed class GridOverlay
        {
            private readonly DataGridView _grid;
            private readonly Panel _panel = new() { BackColor = Color.White, Visible = false };
            private readonly FlowLayoutPanel _buttons = new() { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, BackColor = Color.White };
            private readonly Func<int> _total;
            private readonly string _emptyTitle, _emptyHint;
            private readonly (string Text, Action Run)? _create;
            private readonly Action? _clearFilter;
            private bool _loading;
            private string _title = "", _hint = "";

            internal GridOverlay(DataGridView grid, Func<int> total, string emptyTitle, string emptyHint, (string, Action)? create, Action? clearFilter)
            {
                _grid = grid; _total = total; _emptyTitle = emptyTitle; _emptyHint = emptyHint; _create = create; _clearFilter = clearFilter;
                grid.Controls.Add(_panel);
                _panel.Controls.Add(_buttons);
                _panel.Paint += Paint;
                _panel.Resize += (_, _) => { _panel.Invalidate(); PlaceButtons(); };
                grid.DataBindingComplete += (_, _) => Refresh();
                grid.RowsAdded += (_, _) => Refresh();
                grid.RowsRemoved += (_, _) => Refresh();
                grid.Resize += (_, _) => Place();
                grid.ColumnHeadersHeightChanged += (_, _) => Place();
            }

            /// <summary>Đặt true lúc bắt đầu tải, false khi tải xong (kể cả lỗi).</summary>
            public bool Loading
            {
                get => _loading;
                set { _loading = value; Refresh(); }
            }

            public void Refresh()
            {
                if (_grid.IsDisposed) return;
                var empty = _grid.Rows.Count == 0;
                var show = empty && (_loading || _grid.DataSource is not null);
                if (show)
                {
                    _buttons.Controls.Clear();
                    if (_loading) { _title = "⏳ Đang tải…"; _hint = ""; }
                    else if (_total() > 0)
                    {
                        _title = "Không có dòng nào khớp bộ lọc / ô tìm";
                        _hint = $"Có {_total()} dòng đang bị bộ lọc ẩn đi.";
                        if (_clearFilter is not null) AddButton("✖ Bỏ lọc", _clearFilter, primary: false);
                    }
                    else
                    {
                        _title = _emptyTitle; _hint = _emptyHint;
                        if (_create is { } c) AddButton(c.Text, c.Run, primary: true);
                    }
                    Place();
                    PlaceButtons();
                }
                _panel.Visible = show;
                if (show) { _panel.BringToFront(); _panel.Invalidate(); }
            }

            private void AddButton(string text, Action run, bool primary)
            {
                var b = new Button { Text = text, AutoSize = true, MinimumSize = new Size(_grid.LogicalToDeviceUnits(110), _grid.LogicalToDeviceUnits(34)), Margin = new Padding(6, 0, 6, 0) };
                if (primary) ButtonStyles.Primary(b); else ButtonStyles.Secondary(b);
                b.Click += (_, _) => run();
                _buttons.Controls.Add(b);
            }

            private void Place()
            {
                var top = _grid.ColumnHeadersVisible ? _grid.ColumnHeadersHeight : 0;
                _panel.Bounds = new Rectangle(1, top + 1, Math.Max(0, _grid.ClientSize.Width - 2), Math.Max(0, _grid.ClientSize.Height - top - 2));
            }

            private void PlaceButtons()
            {
                var y = _panel.Height / 2 + _grid.LogicalToDeviceUnits(18);
                _buttons.Location = new Point(Math.Max(0, (_panel.Width - _buttons.PreferredSize.Width) / 2), y);
                _buttons.Visible = _buttons.Controls.Count > 0;
            }

            private void Paint(object? sender, PaintEventArgs e)
            {
                var g = e.Graphics;
                int S(int v) => _grid.LogicalToDeviceUnits(v);
                var mid = _panel.Height / 2;
                if (_loading)
                {
                    // Khung xám "đang tải" (vài vạch giả dòng bảng).
                    using var skel = new SolidBrush(Color.FromArgb(226, 232, 240));
                    for (var i = 0; i < 4; i++)
                        g.FillRectangle(skel, S(24), S(18) + i * S(30), Math.Max(0, _panel.Width - S(48) - (i % 2) * S(120)), S(12));
                }
                using var titleFont = new Font("Segoe UI", 12F, FontStyle.Bold);
                using var hintFont = new Font("Segoe UI", 10F);
                var titleRect = new Rectangle(0, mid - S(34), _panel.Width, S(26));
                TextRenderer.DrawText(g, _title, titleFont, titleRect, Theme.TextPrimary, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPrefix);
                if (_hint.Length > 0)
                    TextRenderer.DrawText(g, _hint, hintFont, new Rectangle(S(20), mid - S(8), _panel.Width - S(40), S(22)), Theme.TextSecondary,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
        }

        /// <param name="total">Tổng số dòng TRƯỚC khi lọc (để phân biệt "chưa có dữ liệu" với "bộ lọc ẩn hết").</param>
        public static GridOverlay Overlay(DataGridView grid, Func<int> total, string emptyTitle, string emptyHint,
            (string Text, Action Run)? create = null, Action? clearFilter = null)
            => new(grid, total, emptyTitle, emptyHint, create, clearFilter);

        // ================= Phím tắt =================

        /// <summary>F5 làm mới · Ctrl+N tạo mới · Enter mở chi tiết (trên bảng). Bắt ở cả màn (KeyPreview không có
        /// ở UserControl -> gắn vào mọi control con hiện có).</summary>
        public static void Shortcuts(Control screen, DataGridView grid, Action? refresh, Action? create, Action? open)
        {
            void Handle(object? s, KeyEventArgs e)
            {
                if (e.KeyCode == System.Windows.Forms.Keys.F5 && refresh is not null) { refresh(); e.Handled = e.SuppressKeyPress = true; }
                else if (e.Control && e.KeyCode == System.Windows.Forms.Keys.N && create is not null) { create(); e.Handled = e.SuppressKeyPress = true; }
            }
            void Hook(Control c)
            {
                c.KeyDown += Handle;
                foreach (Control child in c.Controls) Hook(child);
            }
            Hook(screen);
            if (open is not null)
                grid.KeyDown += (_, e) =>
                {
                    if (e.KeyCode != System.Windows.Forms.Keys.Enter || grid.CurrentRow is null) return;
                    e.Handled = e.SuppressKeyPress = true; // không nhảy xuống dòng dưới
                    open();
                };
        }

        /// <summary>Ghi phím tắt vào chú thích của nút (rê chuột thấy ngay).</summary>
        public static void Tip(ToolTip tip, Control button, string text) => tip.SetToolTip(button, text);

        // ================= Nút "?" =================

        private static readonly string SeenFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LogisticsSystem", "help-seen.txt");

        public static void Help(Control screen, string key, string what, string[] steps, string keys)
        {
            int S(int v) => screen.LogicalToDeviceUnits(v);
            var btn = new Button
            {
                Text = "?", Size = new Size(S(34), S(34)), Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right, Cursor = Cursors.Hand, TabStop = false,
            };
            ButtonStyles.Secondary(btn);
            btn.ForeColor = Theme.Primary;
            new ToolTip().SetToolTip(btn, "Màn này dùng để làm gì?");
            screen.Controls.Add(btn);
            void PlaceBtn() => btn.Location = new Point(screen.ClientSize.Width - btn.Width - S(16), S(10));
            PlaceBtn();
            screen.Resize += (_, _) => PlaceBtn();
            btn.BringToFront();

            Panel? card = null;
            void Close() { card?.Dispose(); card = null; }
            void Open()
            {
                if (card is not null) { Close(); return; }
                card = BuildCard(screen, what, steps, keys, Close);
                screen.Controls.Add(card);
                card.Location = new Point(Math.Max(0, screen.ClientSize.Width - card.Width - S(16)), btn.Bottom + S(6));
                card.BringToFront();
                MarkSeen(key);
            }
            btn.Click += (_, _) => Open();
            screen.HandleCreated += (_, _) =>
            {
                if (!Seen(key)) screen.BeginInvoke(new Action(Open)); // lần đầu mở màn: tự hiện
            };
            screen.Disposed += (_, _) => Close();
        }

        private static Panel BuildCard(Control screen, string what, string[] steps, string keys, Action close)
        {
            int S(int v) => screen.LogicalToDeviceUnits(v);
            var card = new Panel { Width = S(360), BackColor = Color.White, Padding = new Padding(S(14), S(12), S(14), S(12)) };
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, BackColor = Color.White };
            Label L(string text, bool bold = false, Color? color = null) => new()
            {
                Text = text, AutoSize = true, MaximumSize = new Size(S(330), 0), Margin = new Padding(0, 0, 0, S(4)),
                Font = new Font("Segoe UI", bold ? 10F : 9.5F, bold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = color ?? Theme.TextPrimary,
            };
            flow.Controls.Add(L("Màn này dùng để làm gì?", true, Theme.Primary));
            flow.Controls.Add(L(what));
            if (steps.Length > 0)
            {
                flow.Controls.Add(L("Làm theo thứ tự:", true, Theme.Primary));
                for (var i = 0; i < steps.Length; i++) flow.Controls.Add(L($"{"①②③④⑤⑥"[Math.Min(i, 5)]} {steps[i]}"));
            }
            if (keys.Length > 0)
            {
                flow.Controls.Add(L("Phím tắt:", true, Theme.Primary));
                flow.Controls.Add(L(keys));
            }
            var ok = new Button { Text = "Đã hiểu", AutoSize = true, Margin = new Padding(0, S(6), 0, 0) };
            ButtonStyles.Primary(ok);
            ok.Click += (_, _) => close();
            flow.Controls.Add(ok);
            flow.Controls.Add(L("Bấm \"?\" ở góc trên phải để xem lại.", false, Theme.TextSecondary));
            card.Controls.Add(flow);
            card.Height = flow.PreferredSize.Height + card.Padding.Vertical;
            card.Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(191, 219, 254), 2);
                e.Graphics.DrawRectangle(pen, 1, 1, card.Width - 3, card.Height - 3);
            };
            return card;
        }

        private static bool Seen(string key)
        {
            try { return File.Exists(SeenFile) && File.ReadAllLines(SeenFile).Contains(key); } catch { return true; }
        }

        private static void MarkSeen(string key)
        {
            try
            {
                if (Seen(key)) return;
                Directory.CreateDirectory(Path.GetDirectoryName(SeenFile)!);
                File.AppendAllLines(SeenFile, new[] { key });
            }
            catch { }
        }

        // ================= Ngày dễ đọc =================

        /// <summary>"01/11 · trễ 1 ngày" / "03/11 · hôm nay" / "04/11 · mai" / "09/11 · còn 7 ngày".</summary>
        public static string Deadline(DateTime? date, bool done = false)
        {
            if (date is not DateTime d) return "—";
            var days = (d.Date - DateTime.Today).Days;
            var head = d.ToString("dd/MM");
            if (done) return d.ToString("dd/MM/yyyy");
            return days switch
            {
                < 0 => $"{head} · trễ {-days} ngày",
                0 => $"{head} · hôm nay",
                1 => $"{head} · mai",
                _ => $"{head} · còn {days} ngày",
            };
        }

        /// <summary>Mốc "PHẢI XUẤT PHÁT TRƯỚC" cho màn Kho: chữ + mức (2 = đỏ: hôm nay / đã qua mốc, 1 = vàng: mai, 0 = thường).
        /// VD "Hôm nay — hạn giao 05/11", "Đã quá mốc 03/11", "Mai 04/11", "05/11 · còn 2 ngày".</summary>
        public static (string Text, int Level) StartBy(DateTime? latest, DateTime? deadline = null)
        {
            if (latest is not DateTime l) return ("—", 0);
            var days = (l.Date - DateTime.Today).Days;
            var due = deadline is DateTime d ? $" — hạn giao {d:dd/MM}" : "";
            return days switch
            {
                < 0 => ($"Đã quá mốc {l:dd/MM}{due}", 2),
                0 => ($"Hôm nay{due}", 2),
                1 => ($"Mai {l:dd/MM}", 1),
                _ => ($"{l:dd/MM} · còn {days} ngày", 0),
            };
        }

        /// <summary>"Hôm nay 07:30" / "Mai 08:00" / "T5 05/11".</summary>
        public static string Day(DateTime? date, bool withTime = false)
        {
            if (date is not DateTime d) return "—";
            var days = (d.Date - DateTime.Today).Days;
            var time = withTime && d.TimeOfDay != TimeSpan.Zero ? " " + d.ToString("HH:mm") : "";
            return days switch
            {
                0 => "Hôm nay" + time,
                1 => "Mai" + time,
                -1 => "Hôm qua" + time,
                _ => d.ToString("dd/MM") + time,
            };
        }
    }
}
