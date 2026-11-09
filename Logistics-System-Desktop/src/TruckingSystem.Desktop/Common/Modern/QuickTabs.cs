namespace TruckingSystem.Desktop.Common
{
    /// <summary>HÀNG TAB NHANH phía trên bảng ("🔥 Cần xử lý (3) · 🚚 Đang chạy (2) · 📋 Tất cả (40)") — nhân viên
    /// mở màn là thấy ngay VIỆC CẦN LÀM, không phải tự chỉnh nhiều ô lọc. Kết hợp (AND) với bộ lọc chi tiết bên dưới.
    /// Dùng: tabs.Add("todo", "🔥 Cần xử lý", "Gợi ý..."); tabs.Changed += ...; tabs.Selected; tabs.SetCount("todo", n).
    /// Dựng bằng code, không cần Designer.</summary>
    public sealed class QuickTabs : FlowLayoutPanel
    {
        private readonly Dictionary<string, (Button Button, string Text)> _tabs = new();
        private readonly ToolTip _tip = new();
        private string? _selected;

        public event EventHandler? Changed;

        public QuickTabs()
        {
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            WrapContents = true;
            Dock = DockStyle.Fill;
            Margin = new Padding(0, 2, 0, 6);
            Padding = new Padding(0);
            BackColor = Color.Transparent;
        }

        /// <summary>Tab đang chọn (key).</summary>
        public string? Selected => _selected;

        public void Add(string key, string text, string? tooltip = null)
        {
            var b = new Button
            {
                Text = text,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Padding = new Padding(10, 3, 10, 3),
                Margin = new Padding(0, 0, 6, 0),
                MinimumSize = new Size(0, 34),
                UseVisualStyleBackColor = false,
                TabStop = false,
            };
            b.Click += (_, _) => SelectTab(key);
            if (tooltip is not null) _tip.SetToolTip(b, tooltip);
            _tabs[key] = (b, text);
            Controls.Add(b);
            if (_selected is null) _selected = key;
            Restyle(key);
        }

        /// <summary>Chọn tab (raiseEvent = false khi chọn bằng code trước lúc có dữ liệu).</summary>
        public void SelectTab(string key, bool raiseEvent = true)
        {
            if (!_tabs.ContainsKey(key) || key == _selected) return;
            var old = _selected;
            _selected = key;
            if (old is not null) Restyle(old);
            Restyle(key);
            if (raiseEvent) Changed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Hiện số dòng của tab: "🔥 Cần xử lý (3)".</summary>
        public void SetCount(string key, int count)
        {
            if (_tabs.TryGetValue(key, out var t)) t.Button.Text = $"{t.Text} ({count})";
        }

        private void Restyle(string key)
        {
            var b = _tabs[key].Button;
            var on = key == _selected;
            b.BackColor = on ? Theme.Primary : Color.White;
            b.ForeColor = on ? Color.White : Theme.TextPrimary;
            b.FlatAppearance.BorderColor = on ? Theme.Primary : Theme.BorderColor;
            b.FlatAppearance.MouseOverBackColor = on ? Theme.PrimaryDark : Theme.GridAltRow;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _tip.Dispose();
            base.Dispose(disposing);
        }
    }
}
