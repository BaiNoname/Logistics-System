using System.ComponentModel;
using System.Drawing.Drawing2D;
using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI.GUI_ChatBot;

/// <summary>Bảng màu riêng của khung chat: nền sáng ấm, chữ đậm dịu, một màu nhấn.</summary>
internal static class ChatPalette
{
    public static readonly Color Background = ColorTranslator.FromHtml("#FAF9F5");
    public static readonly Color UserBubble = ColorTranslator.FromHtml("#F0EEE6");
    public static readonly Color Surface = Color.White;
    public static readonly Color Border = ColorTranslator.FromHtml("#E3E0D6");
    public static readonly Color Text = ColorTranslator.FromHtml("#1F1E1D");
    public static readonly Color Muted = ColorTranslator.FromHtml("#73726C");
    public static readonly Color Accent = ColorTranslator.FromHtml("#C96442");
    public static readonly Color AccentDark = ColorTranslator.FromHtml("#A9502F");
    public static readonly Color Hover = ColorTranslator.FromHtml("#F2F0E8");
}

/// <summary>Panel bo góc, tô nền và viền tùy chọn; dùng cho bong bóng tin nhắn, thẻ báo giá, ô nhập.</summary>
public class RoundedPanel : Panel
{
    public RoundedPanel()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
    }

    [DefaultValue(12)] public int Radius { get; set; } = 12;
    public Color FillColor { get; set; } = Color.White;
    public Color BorderColor { get; set; } = Color.Empty;

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? BackColor);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = Anim.RoundRect(new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), Radius);
        using (var fill = new SolidBrush(FillColor)) e.Graphics.FillPath(fill, path);
        if (BorderColor != Color.Empty) using (var pen = new Pen(BorderColor)) e.Graphics.DrawPath(pen, path);
    }
}

/// <summary>Nút dạng viên thuốc (chip) cho các lựa chọn của bot và câu hỏi gợi ý.</summary>
public class ChipButton : Button
{
    private bool hover;

    public ChipButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Font = new Font("Segoe UI", 9.75F);
        Cursor = Cursors.Hand;
        AutoSize = false;
        Margin = new Padding(0, 0, 8, 8);
    }

    /// <summary>Đặt chữ và tự tính kích thước theo chữ, không vượt <paramref name="maxWidth"/>.</summary>
    public void Fit(int maxWidth)
    {
        var size = TextRenderer.MeasureText(Text, Font, new Size(Math.Max(60, maxWidth - 28), 0), TextFormatFlags.WordBreak);
        Size = new Size(Math.Min(maxWidth, size.Width + 30), size.Height + 16);
    }

    protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnEnabledChanged(EventArgs e) { Cursor = Enabled ? Cursors.Hand : Cursors.Default; base.OnEnabledChanged(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent?.BackColor ?? ChatPalette.Background);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = Anim.RoundRect(new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), Math.Min(16, Height / 2f));
        using (var fill = new SolidBrush(Enabled && (hover || Focused) ? ChatPalette.Hover : ChatPalette.Surface)) g.FillPath(fill, path);
        using (var pen = new Pen(Enabled && Focused ? ChatPalette.Accent : ChatPalette.Border)) g.DrawPath(pen, path);
        TextRenderer.DrawText(g, Text, Font, new Rectangle(14, 0, Width - 28, Height),
            Enabled ? ChatPalette.Text : ChatPalette.Muted,
            TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak | TextFormatFlags.Left);
    }
}

/// <summary>Nút gửi tròn có mũi tên lên.</summary>
public class SendButton : Button
{
    public SendButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Cursor = Cursors.Hand;
    }

    protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Parent is RoundedPanel rounded ? rounded.FillColor : Parent?.BackColor ?? Color.White);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var d = Math.Min(Width, Height) - 1;
        using (var fill = new SolidBrush(Enabled ? ChatPalette.Accent : ChatPalette.Border)) g.FillEllipse(fill, 0, 0, d, d);
        using var pen = new Pen(Color.White, 2.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        float c = d / 2f, s = d * 0.22f;
        g.DrawLine(pen, c, c + s, c, c - s);
        g.DrawLine(pen, c - s * 0.8f, c - s * 0.2f, c, c - s);
        g.DrawLine(pen, c + s * 0.8f, c - s * 0.2f, c, c - s);
    }
}

/// <summary>Biểu tượng tròn của trợ lý cạnh mỗi lời bot.</summary>
internal sealed class AssistantMark : Control
{
    public AssistantMark()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        Size = new Size(28, 28);
        Font = new Font("Segoe UI", 10F, FontStyle.Bold);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? ChatPalette.Background);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using (var fill = new SolidBrush(ChatPalette.Accent)) e.Graphics.FillEllipse(fill, 0, 0, Width - 1, Height - 1);
        TextRenderer.DrawText(e.Graphics, "T", Font, ClientRectangle, Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}

/// <summary>
/// Bảng dữ liệu tự vẽ (tiêu đề, sọc dòng, kẻ ngang): cột chữ dài nhất giãn theo bề ngang và xuống dòng khi hẹp,
/// các cột khác rộng vừa nội dung (tối đa 40% bề ngang, dài hơn thì xuống dòng); cột số canh phải.
/// </summary>
internal sealed class DataTableView : Control
{
    private readonly string[] headers;
    private readonly List<string[]> rows;
    private readonly HashSet<int> numeric;
    private readonly Font headFont;
    private int[] widths = [];
    private int[] heights = [];
    private int pad;

    public DataTableView(string[] headers, List<string[]> rows, IEnumerable<int> numericColumns, Font font, Font headFont)
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        this.headers = headers; this.rows = rows; this.headFont = headFont;
        numeric = numericColumns.ToHashSet();
        Font = font;
        BackColor = ChatPalette.Surface;
    }

    private const TextFormatFlags Wrap = TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix | TextFormatFlags.TextBoxControl;

    /// <summary>Tính độ rộng cột và chiều cao theo bề ngang <paramref name="width"/>; <paramref name="padding"/> đã nhân DPI.</summary>
    public void Fit(int width, int padding)
    {
        pad = padding;
        var n = headers.Length;
        var natural = new int[n];
        for (var c = 0; c < n; c++)
        {
            var w = TextRenderer.MeasureText(headers[c], headFont, Size.Empty, TextFormatFlags.NoPrefix).Width;
            foreach (var row in rows) w = Math.Max(w, TextRenderer.MeasureText(row[c], Font, Size.Empty, TextFormatFlags.NoPrefix).Width);
            natural[c] = w + 2 * pad;
        }
        var stretch = Enumerable.Range(0, n).Where(c => !numeric.Contains(c)).DefaultIfEmpty(0).MaxBy(c => natural[c]);
        widths = natural.Select((w, c) => c == stretch ? 0 : Math.Min(w, Math.Max(6 * pad, width * 2 / 5))).ToArray();
        widths[stretch] = Math.Max(width - widths.Sum(), 6 * pad);
        heights = new int[rows.Count + 1];
        heights[0] = RowHeight(headers, headFont);
        for (var r = 0; r < rows.Count; r++) heights[r + 1] = RowHeight(rows[r], Font);
        Size = new Size(widths.Sum(), heights.Sum());
    }

    private int RowHeight(string[] cells, Font font)
        => cells.Select((text, c) => TextRenderer.MeasureText(text, font, new Size(Math.Max(1, widths[c] - 2 * pad), 0), Wrap).Height).Max()
           + (int)(pad * 1.2);

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(ChatPalette.Surface);
        if (widths.Length == 0) return;
        using var line = new Pen(ChatPalette.Border);
        var y = 0;
        for (var r = 0; r <= rows.Count; r++)
        {
            var cells = r == 0 ? headers : rows[r - 1];
            var back = r == 0 ? ChatPalette.UserBubble : r % 2 == 0 ? ChatPalette.Background : ChatPalette.Surface;
            using (var fill = new SolidBrush(back)) g.FillRectangle(fill, 0, y, Width, heights[r]);
            var x = 0;
            for (var c = 0; c < cells.Length; c++)
            {
                var bounds = new Rectangle(x + pad, y, widths[c] - 2 * pad, heights[r]);
                var align = numeric.Contains(c) ? TextFormatFlags.Right : TextFormatFlags.Left;
                TextRenderer.DrawText(g, cells[c], r == 0 ? headFont : Font, bounds, r == 0 ? ChatPalette.Muted : ChatPalette.Text,
                    Wrap | align | TextFormatFlags.VerticalCenter);
                x += widths[c];
            }
            y += heights[r];
            g.DrawLine(line, 0, y - 1, Width, y - 1);
        }
    }
}

/// <summary>Một dòng hội thoại; tự sắp xếp con theo bề ngang cột chat.</summary>
internal sealed class ChatRow : Panel
{
    private readonly Action<ChatRow, int> arrange;

    public ChatRow(Action<ChatRow, int> arrange)
    {
        this.arrange = arrange;
        DoubleBuffered = true;
        BackColor = ChatPalette.Background;
        Margin = new Padding(0);
    }

    public void Arrange(int width, int left)
    {
        SuspendLayout();
        Width = width;
        Margin = new Padding(left, 0, 0, 0);
        arrange(this, width);
        ResumeLayout(false);
    }
}
