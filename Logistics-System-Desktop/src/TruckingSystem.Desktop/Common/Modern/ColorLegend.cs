namespace TruckingSystem.Desktop.Common
{
    /// <summary>BẢNG Ý NGHĨA MÀU hiện trong thẻ "?" của từng màn (ScreenKit.Help(..., colors)) — để nhân viên biết
    /// mỗi màu nhãn / dải màu nghĩa là gì. 3 kiểu dòng:
    ///  • Header("Cột Trạng thái")                 : tiêu đề nhóm nhỏ.
    ///  • Pill("Đang chạy", GridDecor.Blue, "…")   : nhãn mẫu (đúng màu như trên bảng) + giải thích.
    ///  • Bar(màu, "…")                            : ô màu đặc (dải màu mép trái / ô vuông) + giải thích.
    ///  • Note("…")                                : 1 dòng chữ xám giải thích thêm.</summary>
    public static class ColorLegend
    {
        public sealed record Item(string? Sample, GridDecor.Tone? Tone, Color? Swatch, string Text, bool IsHeader = false);

        public static Item Header(string text) => new(null, null, null, text, true);
        public static Item Pill(string sample, GridDecor.Tone tone, string meaning) => new(sample, tone, null, meaning);
        public static Item Bar(Color color, string meaning) => new(null, null, color, meaning);
        public static Item Note(string text) => new(null, null, null, text);

        /// <summary>Chữ màu (không nền) — VD cột "Việc cần làm".</summary>
        public static Item Text(string sample, Color fore, string meaning) => new(sample, new GridDecor.Tone(Color.White, fore), null, meaning);

        // 4 màu trạng thái dùng chung màn Đơn + Chuyến (dải màu mép trái / ô vuông dòng đơn).
        public static readonly Color Gray = ColorTranslator.FromHtml("#94A3B8");
        public static readonly Color Amber = ColorTranslator.FromHtml("#F59E0B");
        public static readonly Color Green = ColorTranslator.FromHtml("#16A34A");
        public static readonly Color Red = ColorTranslator.FromHtml("#DC2626");

        /// <summary>Dựng phần "Ý nghĩa màu" vào thẻ trợ giúp.</summary>
        internal static void AddTo(FlowLayoutPanel flow, Control screen, IReadOnlyList<Item> items, int maxTextWidth)
        {
            int S(int v) => screen.LogicalToDeviceUnits(v);
            foreach (var it in items)
            {
                if (it.IsHeader)
                {
                    flow.Controls.Add(new Label
                    {
                        Text = it.Text, AutoSize = true, MaximumSize = new Size(maxTextWidth, 0), Margin = new Padding(0, S(6), 0, S(2)),
                        Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Theme.TextSecondary,
                    });
                    continue;
                }
                var row = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true,
                    Margin = new Padding(0, 0, 0, S(3)), BackColor = Color.White,
                };
                var used = 0;
                if (it.Swatch is Color sw)
                {
                    row.Controls.Add(new Panel { Size = new Size(S(14), S(14)), BackColor = sw, Margin = new Padding(0, S(3), S(8), 0) });
                    used = S(22);
                }
                else if (it.Sample is not null && it.Tone is not null)
                {
                    var pill = new Label
                    {
                        Text = it.Sample, AutoSize = true, BackColor = it.Tone.Back, ForeColor = it.Tone.Fore,
                        Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Padding = new Padding(S(6), S(1), S(6), S(1)),
                        Margin = new Padding(0, 0, S(8), 0), UseMnemonic = false,
                    };
                    row.Controls.Add(pill);
                    used = pill.PreferredSize.Width + S(8);
                }
                row.Controls.Add(new Label
                {
                    Text = it.Text, AutoSize = true, MaximumSize = new Size(Math.Max(S(120), maxTextWidth - used), 0),
                    Font = new Font("Segoe UI", 9F), ForeColor = it.Swatch is null && it.Sample is null ? Theme.TextSecondary : Theme.TextPrimary,
                    Margin = new Padding(0, S(1), 0, 0), UseMnemonic = false,
                });
                flow.Controls.Add(row);
            }
        }
    }
}
