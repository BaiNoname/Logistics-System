using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;

namespace TruckingSystem.Desktop.Common
{
    /// <summary>Trang trí BẢNG cho dễ theo dõi (bật riêng từng bảng, không đổi dữ liệu):
    ///  • Pills: ô trạng thái vẽ thành "nhãn" bo tròn có màu nền nhạt + chữ đậm cùng tông
    ///    (xanh = ổn, vàng = cần xử lý, đỏ = bị chặn / trễ, xanh dương = đang chạy, xám = đã đóng).
    ///  • ProgressBars: ô tiến độ vẽ thành thanh ngang mảnh + chữ; thanh "dâng" từ 0 lên mỗi khi
    ///    bảng nạp dữ liệu mới.
    ///  • HoverRows: dòng đang rê chuột sáng lên nhẹ — mắt không bị lạc dòng ở bảng nhiều cột.</summary>
    public static class GridDecor
    {
        /// <summary>Cặp màu (nền nhạt, chữ đậm) của 1 nhãn.</summary>
        public sealed record Tone(Color Back, Color Fore);

        public static readonly Tone Green = new(ColorTranslator.FromHtml("#DCFCE7"), ColorTranslator.FromHtml("#166534"));
        public static readonly Tone Amber = new(ColorTranslator.FromHtml("#FEF3C7"), ColorTranslator.FromHtml("#92400E"));
        public static readonly Tone Red = new(ColorTranslator.FromHtml("#FEE2E2"), ColorTranslator.FromHtml("#991B1B"));
        public static readonly Tone Blue = new(ColorTranslator.FromHtml("#DBEAFE"), ColorTranslator.FromHtml("#1E40AF"));
        public static readonly Tone Violet = new(ColorTranslator.FromHtml("#EDE9FE"), ColorTranslator.FromHtml("#5B21B6"));
        public static readonly Tone Teal = new(ColorTranslator.FromHtml("#CCFBF1"), ColorTranslator.FromHtml("#115E59"));
        public static readonly Tone Slate = new(ColorTranslator.FromHtml("#F1F5F9"), ColorTranslator.FromHtml("#475569"));

        /// <summary>Màu nhấn đậm cho thẻ KPI cùng bộ màu với nhãn.</summary>
        public static readonly Color AccentTeal = ColorTranslator.FromHtml("#0D9488");
        public static readonly Color AccentViolet = ColorTranslator.FromHtml("#7C3AED");
        public static readonly Color AccentSlate = ColorTranslator.FromHtml("#64748B");

        private sealed class GridState
        {
            public Font? PillFont;
            public Font? SmallFont;
            public double Grow = 1;
            public System.Windows.Forms.Timer? GrowTimer;
        }

        private static readonly ConditionalWeakTable<DataGridView, GridState> States = new();

        private static GridState State(DataGridView grid)
        {
            if (States.TryGetValue(grid, out var s)) return s;
            s = new GridState();
            States.Add(grid, s);
            grid.FontChanged += (_, _) => { s.PillFont?.Dispose(); s.SmallFont?.Dispose(); s.PillFont = s.SmallFont = null; };
            grid.Disposed += (_, _) => { s.PillFont?.Dispose(); s.SmallFont?.Dispose(); s.GrowTimer?.Dispose(); };
            return s;
        }

        private static Font PillFont(DataGridView grid)
        {
            var s = State(grid);
            return s.PillFont ??= new Font(grid.Font.FontFamily, grid.Font.SizeInPoints * 0.9f, FontStyle.Bold);
        }

        private static Font SmallFont(DataGridView grid)
        {
            var s = State(grid);
            return s.SmallFont ??= new Font(grid.Font.FontFamily, grid.Font.SizeInPoints * 0.88f, FontStyle.Regular);
        }

        /// <summary>Vẽ ô của cột <paramref name="column"/> thành nhãn màu. <paramref name="tone"/> nhận
        /// (chữ trong ô, chỉ số dòng) và trả về màu; null = vẽ như ô thường.</summary>
        public static void Pills(DataGridView grid, string column, Func<string, int, Tone?> tone)
        {
            grid.CellPainting += (_, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.Graphics is null) return;
                if (grid.Columns[e.ColumnIndex].Name != column) return;
                var text = e.FormattedValue?.ToString();
                if (string.IsNullOrWhiteSpace(text)) return;
                var t = tone(text, e.RowIndex);
                if (t is null) return;

                e.PaintBackground(e.ClipBounds, (e.State & DataGridViewElementStates.Selected) != 0);
                var g = e.Graphics;
                var font = PillFont(grid);
                var state = g.Save();
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var padX = (int)(font.Height * 0.65);
                var h = (int)(font.Height * 1.45);
                var left = e.CellBounds.X + (int)(font.Height * 0.45);
                var maxW = e.CellBounds.Right - left - (int)(font.Height * 0.4);
                var textW = TextRenderer.MeasureText(g, text, font, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine).Width;
                var w = Math.Max(h, Math.Min(textW + padX * 2, maxW));
                var rect = new Rectangle(left, e.CellBounds.Y + (e.CellBounds.Height - h) / 2, w, h);
                using (var path = Anim.RoundRect(rect, h / 2f))
                using (var brush = new SolidBrush(t.Back))
                    g.FillPath(brush, path);
                TextRenderer.DrawText(g, text, font, rect, t.Fore,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine
                    | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
                g.Restore(state);
                e.Handled = true;
            };
        }

        /// <summary>Vẽ ô của cột <paramref name="column"/> thành thanh tiến độ. <paramref name="get"/>
        /// nhận chỉ số dòng, trả về (tỉ lệ 0..1, chữ hiện kèm, màu) — null = vẽ như ô thường.</summary>
        public static void ProgressBars(DataGridView grid, string column, Func<int, (double Ratio, string Text, Color Color)?> get)
        {
            var s = State(grid);
            grid.DataBindingComplete += (_, _) =>
            {
                // Thanh "dâng" từ 0 lên mỗi lần bảng nạp dữ liệu mới.
                s.GrowTimer?.Dispose();
                s.Grow = 0;
                s.GrowTimer = Anim.Tween(grid, 600, p =>
                {
                    s.Grow = p;
                    if (grid.Columns.Contains(column)) grid.InvalidateColumn(grid.Columns[column]!.Index);
                }, () => s.GrowTimer = null);
            };
            grid.CellPainting += (_, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.Graphics is null) return;
                if (grid.Columns[e.ColumnIndex].Name != column) return;
                var data = get(e.RowIndex);
                if (data is null) return;
                var (ratio, text, color) = data.Value;

                e.PaintBackground(e.ClipBounds, (e.State & DataGridViewElementStates.Selected) != 0);
                var g = e.Graphics;
                var font = SmallFont(grid);
                var state = g.Save();
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var pad = (int)(font.Height * 0.5);
                var barH = Math.Max(4, font.Height / 3);
                var inner = Rectangle.Inflate(e.CellBounds, -pad, 0);
                var textRect = new Rectangle(inner.X, e.CellBounds.Y + (int)(font.Height * 0.25), inner.Width, font.Height);
                TextRenderer.DrawText(g, text, font, textRect, Theme.TextPrimary,
                    TextFormatFlags.Left | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
                var track = new Rectangle(inner.X, textRect.Bottom + (int)(font.Height * 0.2), inner.Width, barH);
                using (var tp = Anim.RoundRect(track, barH / 2f))
                using (var tb = new SolidBrush(ColorTranslator.FromHtml("#E2E8F0")))
                    g.FillPath(tb, tp);
                var fillW = (int)Math.Round(track.Width * Math.Clamp(ratio, 0, 1) * s.Grow);
                if (fillW > 0)
                {
                    var fill = new Rectangle(track.X, track.Y, Math.Max(fillW, barH), barH);
                    using var fp = Anim.RoundRect(fill, barH / 2f);
                    using var fb = new SolidBrush(color);
                    g.FillPath(fb, fp);
                }
                g.Restore(state);
                e.Handled = true;
            };
        }

        /// <summary>Dòng đang rê chuột sáng nhẹ (không ảnh hưởng dòng đang chọn).</summary>
        public static void HoverRows(DataGridView grid)
        {
            var hover = ColorTranslator.FromHtml("#EEF4FF");
            var hot = -1;
            void Set(int row, Color c)
            {
                if (row >= 0 && row < grid.Rows.Count) grid.Rows[row].DefaultCellStyle.BackColor = c;
            }
            grid.CellMouseEnter += (_, e) =>
            {
                if (e.RowIndex == hot) return;
                Set(hot, Color.Empty);
                hot = e.RowIndex;
                Set(hot, hover);
            };
            grid.MouseLeave += (_, _) => { Set(hot, Color.Empty); hot = -1; };
            grid.DataBindingComplete += (_, _) => hot = -1;
        }

        /// <summary>Bộ màu chung cho chữ trạng thái tiếng Việt hay gặp ở các màn của hệ thống.</summary>
        public static Tone? StatusTone(string text)
        {
            var s = text.Trim();
            if (s.StartsWith("🔴") || s.StartsWith("⛔") || s.StartsWith("⚠️") || s.StartsWith("🚫")) return Red;
            if (s.StartsWith("🟡") || s.StartsWith("⏳") || s.StartsWith("📦") || s.StartsWith("⏰")) return Amber;
            if (s.StartsWith("📤")) return Violet;
            if (s.StartsWith("✅") || s.StartsWith("🟢")) return Green;
            return s switch
            {
                "Rảnh" or "Sẵn sàng" or "Hoàn tất" or "Đủ" or "Kịp hạn" => Green,
                "Đang chạy" or "Đang vận chuyển" or "Đang làm" => Blue,
                "Đã xác nhận" or "Đã lên kế hoạch" => Teal,
                "Nháp" or "Đã hủy" or "Ngưng hoạt động" => Slate,
                "Bảo trì" or "Bằng lái sắp hết hạn" or "Chờ xác nhận" => Amber,
                "Không sẵn sàng" or "Bằng lái hết hạn" or "Chưa có hồ sơ" => Red,
                _ => null,
            };
        }
    }
}
