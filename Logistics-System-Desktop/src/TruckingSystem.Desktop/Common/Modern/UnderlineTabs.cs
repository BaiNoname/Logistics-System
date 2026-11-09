namespace TruckingSystem.Desktop.Common
{
    /// <summary>TAB GẠCH CHÂN (kiểu tab trình duyệt) để ĐỔI TRANG trong 1 màn — nhìn khác hẳn nút bấm / nút lọc, tránh nhầm
    /// "đổi trang" với "chọn kỳ" (góp ý UX). Cùng cách dùng với QuickTabs: Add / SelectTab / Selected / Changed.</summary>
    public sealed class UnderlineTabs : FlowLayoutPanel
    {
        private readonly Dictionary<string, Label> _tabs = new();
        private readonly ToolTip _tip = new();
        private string? _selected;

        public event EventHandler? Changed;
        public string? Selected => _selected;

        public UnderlineTabs()
        {
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            WrapContents = false;
            Dock = DockStyle.Fill;
            Margin = new Padding(0, 4, 0, 12);
            Padding = new Padding(0);
            DoubleBuffered = true;
            ResizeRedraw = true;
            Paint += (_, e) =>
            {
                using var line = new Pen(Theme.BorderColor, 2);
                e.Graphics.DrawLine(line, 0, Height - 1, Width, Height - 1);
            };
        }

        public void Add(string key, string text, string? tooltip = null)
        {
            var l = new Label
            {
                Text = text, AutoSize = true, Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Padding = new Padding(14, 8, 14, 10), Margin = new Padding(0, 0, 2, 0),
            };
            l.Click += (_, _) => SelectTab(key);
            l.MouseEnter += (_, _) => { if (key != _selected) l.ForeColor = Theme.TextPrimary; };
            l.MouseLeave += (_, _) => Restyle(key);
            l.Paint += (_, e) =>
            {
                if (key != _selected) return;
                using var b = new SolidBrush(Theme.Primary);
                e.Graphics.FillRectangle(b, 0, l.Height - LogicalToDeviceUnits(3), l.Width, LogicalToDeviceUnits(3));
            };
            if (tooltip is not null) _tip.SetToolTip(l, tooltip);
            _tabs[key] = l;
            Controls.Add(l);
            _selected ??= key;
            Restyle(key);
        }

        public void SelectTab(string key, bool raiseEvent = true)
        {
            if (!_tabs.ContainsKey(key) || key == _selected) return;
            var old = _selected;
            _selected = key;
            if (old is not null) Restyle(old);
            Restyle(key);
            if (raiseEvent) Changed?.Invoke(this, EventArgs.Empty);
        }

        private void Restyle(string key)
        {
            var l = _tabs[key];
            l.ForeColor = key == _selected ? Theme.Primary : Theme.TextSecondary;
            l.Invalidate();
        }
    }
}
