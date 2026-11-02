using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_WarehouseManagement
{
    /// <summary>SƠ ĐỒ NHỎ xem trước (mẫu UX ⑦ ⑧ đã duyệt): vẽ khung kho (hoặc khung 1 khu), các khu / kệ đã có,
    /// và ô ĐANG THÊM / SỬA (viền xanh nét đứt; đỏ khi đè lên ô khác hoặc tràn ra ngoài). Tọa độ theo mét,
    /// gốc ở góc trên-trái (X sang phải, Y xuống dưới) — đúng quy ước sơ đồ màn Kho.</summary>
    public sealed class LayoutPreview : Panel
    {
        public sealed record Box(string Label, RectangleF Rect, Color Fill, bool Strong = false);

        private SizeF _area = new(10, 10);
        private string _areaLabel = "";
        private List<Box> _boxes = new();
        private RectangleF? _current;
        private string _currentLabel = "";
        private bool _currentBad;

        public LayoutPreview()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.FromArgb(248, 250, 252);
            BorderStyle = BorderStyle.FixedSingle;
        }

        public void SetArea(decimal width, decimal height, string label)
        {
            _area = new SizeF((float)Math.Max(0.1m, width), (float)Math.Max(0.1m, height));
            _areaLabel = label;
            Invalidate();
        }

        public void SetBoxes(IEnumerable<Box> boxes) { _boxes = boxes.ToList(); Invalidate(); }

        public void SetCurrent(RectangleF? rect, string label, bool bad)
        {
            _current = rect;
            _currentLabel = label;
            _currentBad = bad;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int S(int v) => LogicalToDeviceUnits(v);
            var pad = S(14);
            var avail = new SizeF(Width - 2 * pad, Height - 2 * pad - S(14));
            if (avail.Width <= 10 || avail.Height <= 10) return;
            var scale = Math.Min(avail.Width / _area.Width, avail.Height / _area.Height);
            var origin = new PointF(pad + (avail.Width - _area.Width * scale) / 2, pad + S(14));
            RectangleF Map(RectangleF r) => new(origin.X + r.X * scale, origin.Y + r.Y * scale, r.Width * scale, r.Height * scale);

            using var small = new Font("Segoe UI", 8F);
            using var bold = new Font("Segoe UI", 8F, FontStyle.Bold);
            TextRenderer.DrawText(g, _areaLabel, bold, new Point(pad, S(2)), Theme.TextSecondary);

            var areaRect = Map(new RectangleF(0, 0, _area.Width, _area.Height));
            using (var floor = new SolidBrush(Color.White)) g.FillRectangle(floor, areaRect);
            using (var wall = new Pen(Color.FromArgb(71, 85, 105), 2)) g.DrawRectangle(wall, areaRect.X, areaRect.Y, areaRect.Width, areaRect.Height);

            foreach (var b in _boxes)
            {
                var r = Map(b.Rect);
                using var fill = new SolidBrush(Color.FromArgb(b.Strong ? 200 : 120, b.Fill));
                using var border = new Pen(b.Strong ? Theme.Primary : Color.FromArgb(148, 163, 184), b.Strong ? 2 : 1);
                g.FillRectangle(fill, r);
                g.DrawRectangle(border, r.X, r.Y, r.Width, r.Height);
                if (r.Width > S(26) && r.Height > S(12))
                    TextRenderer.DrawText(g, b.Label, b.Strong ? bold : small, Rectangle.Round(r), Theme.TextPrimary,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }

            if (_current is { } cur && cur.Width > 0 && cur.Height > 0)
            {
                var r = Map(cur);
                var color = _currentBad ? Theme.Danger : Theme.Primary;
                using var fill = new SolidBrush(Color.FromArgb(60, color));
                using var pen = new Pen(color, 2) { DashStyle = DashStyle.Dash };
                g.FillRectangle(fill, r);
                g.DrawRectangle(pen, r.X, r.Y, r.Width, r.Height);
                TextRenderer.DrawText(g, _currentLabel, bold, Rectangle.Round(r), color,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
            }
        }

        // ================= Hình học dùng chung =================

        /// <summary>Lỗi vị trí (null = hợp lệ): tràn ra ngoài khung hoặc đè lên ô khác (nêu tên ô bị đè).</summary>
        public static string? Problem(RectangleF r, SizeF area, IEnumerable<Box> others, string areaName)
        {
            const float eps = 0.001f;
            if (r.Width <= 0 || r.Height <= 0) return null;
            if (r.X < -eps || r.Y < -eps || r.Right > area.Width + eps || r.Bottom > area.Height + eps)
                return $"Tràn ra ngoài {areaName} ({area.Width:0.##} × {area.Height:0.##} m) — dời vào trong hoặc thu nhỏ lại.";
            var hit = others.FirstOrDefault(o => r.X < o.Rect.Right - eps && o.Rect.X < r.Right - eps && r.Y < o.Rect.Bottom - eps && o.Rect.Y < r.Bottom - eps);
            return hit is null ? null : $"Đang đè lên {hit.Label} — dời chỗ khác, thu nhỏ lại, hoặc bấm \"📍 Tự tìm chỗ trống\".";
        }

        /// <summary>Chỗ trống đầu tiên (quét từ góc trên-trái, bước 0,5 m) vừa ô rộng × dài; null = không còn chỗ.</summary>
        public static PointF? FindFreeSpot(float w, float h, SizeF area, IEnumerable<Box> others)
        {
            if (w <= 0 || h <= 0 || w > area.Width || h > area.Height) return null;
            var list = others.ToList();
            const float step = 0.5f;
            for (float y = 0; y + h <= area.Height + 0.001f; y += step)
                for (float x = 0; x + w <= area.Width + 0.001f; x += step)
                    if (Problem(new RectangleF(x, y, w, h), area, list, "") is null) return new PointF(x, y);
            return null;
        }

        public static Color ZoneColor(string zoneType) => zoneType switch
        {
            "Fragile" => Color.FromArgb(191, 219, 254),
            "Hazmat" => Color.FromArgb(254, 202, 202),
            _ => Color.FromArgb(226, 232, 240),
        };

        public static Color BinColor(decimal used, decimal cap)
        {
            var pct = cap <= 0 ? 0 : used / cap;
            return pct >= 0.85m ? Color.FromArgb(254, 202, 202) : pct >= 0.5m ? Color.FromArgb(253, 230, 138) : Color.FromArgb(187, 247, 208);
        }
    }

    /// <summary>Gắn sơ đồ xem trước + dòng cảnh báo + nút "📍 Tự tìm chỗ trống" vào form Thêm / Sửa khu, kệ (mẫu ⑧),
    /// bằng code — không sửa Designer. Sơ đồ vẽ lại mỗi khi gõ số; vị trí sai (đè / tràn) -> chữ đỏ + mờ nút Lưu.</summary>
    public sealed class PlacementHelper
    {
        private readonly LayoutPreview _preview = new() { Dock = DockStyle.Fill };
        private readonly Label _warn = new() { AutoSize = true, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Margin = new Padding(0, 4, 0, 4) };
        private readonly Button _find = new() { Text = "📍 Tự tìm chỗ trống vừa kích thước này", AutoSize = true, Margin = new Padding(0, 4, 0, 4) };
        private readonly NumericUpDown _x, _y, _w, _h;
        private readonly Button _save;
        private readonly string _areaName;
        private SizeF _area;
        private List<LayoutPreview.Box> _others = new();
        private Func<string> _label;
        private bool _ready;

        public PlacementHelper(Form form, TableLayoutPanel inputs, NumericUpDown x, NumericUpDown y, NumericUpDown w, NumericUpDown h,
            Button save, string areaName, Func<string> label)
        {
            _x = x; _y = y; _w = w; _h = h; _save = save; _areaName = areaName; _label = label;
            int S(int v) => form.LogicalToDeviceUnits(v);
            ButtonStyles.Secondary(_find);
            _find.Click += (_, _) => FindSpot();
            var row = inputs.RowCount;
            inputs.RowCount += 3;
            inputs.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            inputs.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            inputs.RowStyles.Add(new RowStyle(SizeType.Absolute, S(230)));
            inputs.Controls.Add(_find, 0, row);
            inputs.Controls.Add(_warn, 0, row + 1);
            inputs.Controls.Add(_preview, 0, row + 2);
            foreach (var c in new Control[] { _find, _warn, _preview }) inputs.SetColumnSpan(c, Math.Max(1, inputs.ColumnCount));
            _preview.MinimumSize = new Size(0, S(220));
            form.ClientSize = new Size(Math.Max(form.ClientSize.Width, S(560)), form.ClientSize.Height + S(300));
            foreach (var n in new[] { x, y, w, h }) n.ValueChanged += (_, _) => Update();
            _warn.Text = "Đang tải sơ đồ…";
            _warn.ForeColor = Theme.TextSecondary;
        }

        /// <summary>Nạp khung (kho hoặc khu) + các ô đã có (không gồm ô đang sửa).</summary>
        public void SetData(decimal areaW, decimal areaH, string areaLabel, IEnumerable<LayoutPreview.Box> others)
        {
            _area = new SizeF((float)areaW, (float)areaH);
            _others = others.ToList();
            _preview.SetArea(areaW, areaH, areaLabel);
            _preview.SetBoxes(_others);
            _ready = true;
            Update();
        }

        public void Update()
        {
            if (!_ready) return;
            var r = new RectangleF((float)_x.Value, (float)_y.Value, (float)_w.Value, (float)_h.Value);
            var problem = LayoutPreview.Problem(r, _area, _others, _areaName);
            _preview.SetCurrent(r, _label() + " (đang sửa)", problem is not null);
            _warn.Text = problem is null
                ? (r.Width > 0 && r.Height > 0 ? "✅ Vị trí hợp lệ — không đè lên ô khác." : "Nhập kích thước rộng / dài để xem trên sơ đồ.")
                : "⚠️ " + problem;
            _warn.ForeColor = problem is null ? Theme.Success : Theme.Danger;
            _save.Enabled = problem is null;
        }

        private void FindSpot()
        {
            if (!_ready) return;
            var p = LayoutPreview.FindFreeSpot((float)_w.Value, (float)_h.Value, _area, _others);
            if (p is null)
            {
                _warn.Text = $"⚠️ Không còn chỗ trống vừa {_w.Value:0.##} × {_h.Value:0.##} m trong {_areaName} — thu nhỏ lại.";
                _warn.ForeColor = Theme.Danger;
                return;
            }
            SetSafe(_x, (decimal)p.Value.X);
            SetSafe(_y, (decimal)p.Value.Y);
        }

        private static void SetSafe(NumericUpDown n, decimal v)
        {
            v = Math.Round(v, n.DecimalPlaces);
            if (v > n.Maximum) n.Maximum = v;
            if (v < n.Minimum) v = n.Minimum;
            n.Value = v;
        }
    }
}
