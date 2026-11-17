using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_ChatBot;
using TruckingSystem.Desktop.SERVICE_ChatBot;
namespace TruckingSystem.Desktop.GUI.GUI_ChatBot;

/// <summary>Khung chat dạng hội thoại: tin của khách trong bong bóng bên phải, lời trợ lý bên trái kèm nút lựa chọn và thẻ báo giá.</summary>
public partial class ChatBotForm : Form
{
    private const int MaxColumn = 760;
    private static readonly string[] Examples =
    [
        "Công thức tính cước",
        "Hướng dẫn sử dụng"
    ];
    private readonly ChatBotApiService service;
    private readonly List<ChipButton> activeChips = [];
    private readonly Font messageFont = new("Segoe UI", 11F);
    private readonly Font boldFont = new("Segoe UI Semibold", 10.5F, FontStyle.Bold);
    private readonly Font smallFont = new("Segoe UI", 9F);
    private string? context;
    private bool busy;
    private ChatRow? typingRow;
    private Label? typingLabel;
    private int typingStep;

    public ChatBotForm(ApiConnection connection)
    {
        InitializeComponent(); service = new(connection);
        // Cột chat luôn vừa bề ngang nên chỉ cuộn dọc.
        flowMessages.AutoScroll = false;
        flowMessages.HorizontalScroll.Maximum = 0;
        flowMessages.HorizontalScroll.Visible = false;
        flowMessages.AutoScroll = true;
        Load += (_, _) => { LayoutComposer(); ArrangeRows(); };
        btnSend.Click += async (_, _) => await Send();
        txtQuestion.KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.Enter && !e.Shift) { e.SuppressKeyPress = true; await Send(); }
        };
        txtQuestion.TextChanged += (_, _) => { btnSend.Enabled = !busy && !string.IsNullOrWhiteSpace(txtQuestion.Text); LayoutComposer(); };
        composerBox.Click += (_, _) => txtQuestion.Focus();
        composerHost.Resize += (_, _) => LayoutComposer();
        flowMessages.ClientSizeChanged += (_, _) => ArrangeRows();
        flowMessages.Resize += (_, _) => ArrangeRows();
        btnClear.Click += (_, _) => { if (!busy) Reset(); };
        typingTimer.Tick += (_, _) =>
        {
            if (typingLabel == null) return;
            typingStep = (typingStep + 1) % 4;
            typingLabel.Text = "Đang soạn câu trả lời" + new string('.', typingStep);
        };
        mnuCopy.Click += (_, _) =>
        {
            for (var c = messageMenu.SourceControl; c != null; c = c.Parent)
                if (c is ChatRow { Tag: string text }) { Clipboard.SetText(text); return; }
        };
        Shown += (_, _) => txtQuestion.Focus();
        FormClosed += (_, _) => { messageFont.Dispose(); boldFont.Dispose(); smallFont.Dispose(); };
        Reset();
    }

    private int S(int value) => (int)Math.Round(value * DeviceDpi / 96f);

    private void Reset()
    {
        context = null;
        activeChips.Clear();
        txtQuestion.Clear();
        btnSend.Enabled = false;
        ClearRows();
        AddRow(BotRow("Xin chào! Mình giúp bạn ước tính cước theo bảng giá đang áp dụng. Hãy cho mình biết hàng gì, nặng bao nhiêu, " +
            "lấy ở đâu và giao đến đâu — hoặc bấm một gợi ý bên dưới.",
            Examples.Select(text => (text, (Func<Task>)(() => { txtQuestion.Text = text; return Send(); }))).ToList()));
    }

    private async Task Send(ChatOption? selection = null)
    {
        if (busy || (selection == null && string.IsNullOrWhiteSpace(txtQuestion.Text))) return;
        var question = selection?.Label ?? txtQuestion.Text.Trim();
        var previousChips = activeChips.ToList();
        foreach (var chip in previousChips) chip.Enabled = false;
        activeChips.Clear();
        SetBusy(true);
        AddRow(UserRow(question));
        if (selection == null) txtQuestion.Clear();
        ShowTyping();
        try
        {
            var response = await service.SendAsync(selection == null ? question : "", context, selection?.Id)
                ?? throw new InvalidOperationException("Không nhận được phản hồi từ máy chủ.");
            if (IsDisposed) return;
            context = response.Context;
            HideTyping();
            AddRow(BotRow(response.Message,
                response.Options.Select(o => (o.Label, (Func<Task>)(() => Send(o)))).ToList(), response.Quote, response.Tables));
        }
        catch (Exception ex)
        {
            if (IsDisposed) return;
            HideTyping();
            // Ngữ cảnh chưa đổi nên lựa chọn cũ vẫn dùng được; trả lại nội dung để khách gửi lại.
            foreach (var chip in previousChips) chip.Enabled = true;
            activeChips.AddRange(previousChips);
            if (selection == null && txtQuestion.TextLength == 0) txtQuestion.Text = question;
            AddRow(BotRow($"{ex.Message}\nNội dung được giữ lại để thử lại.", [], error: true));
        }
        finally
        {
            if (!IsDisposed) { SetBusy(false); txtQuestion.Focus(); txtQuestion.SelectionStart = txtQuestion.TextLength; }
        }
    }

    private void SetBusy(bool value)
    {
        busy = value;
        btnClear.Enabled = !value;
        btnSend.Enabled = !value && !string.IsNullOrWhiteSpace(txtQuestion.Text);
    }

    private void ShowTyping()
    {
        typingStep = 0;
        typingLabel = MessageLabel("Đang soạn câu trả lời", ChatPalette.Background);
        typingLabel.ForeColor = ChatPalette.Muted;
        typingLabel.Font = smallFont;
        var mark = new AssistantMark { Size = new Size(S(28), S(28)) };
        typingRow = new ChatRow((row, width) =>
        {
            typingLabel.Location = new Point(S(40), S(8));
            row.Height = S(44);
        });
        typingRow.Controls.AddRange([mark, typingLabel]);
        AddRow(typingRow);
        typingTimer.Start();
    }

    private void HideTyping()
    {
        typingTimer.Stop();
        if (typingRow == null) return;
        flowMessages.Controls.Remove(typingRow);
        typingRow.Dispose();
        typingRow = null; typingLabel = null;
    }

    private Label MessageLabel(string text, Color back) => new()
    {
        AutoSize = true, Text = text, Font = messageFont, ForeColor = ChatPalette.Text, BackColor = back,
        UseMnemonic = false, ContextMenuStrip = messageMenu, Margin = Padding.Empty
    };

    private ChatRow UserRow(string text)
    {
        var bubble = new RoundedPanel { FillColor = ChatPalette.UserBubble, Radius = S(16) };
        var label = MessageLabel(text, ChatPalette.UserBubble);
        bubble.Controls.Add(label);
        var row = new ChatRow((row, width) =>
        {
            label.MaximumSize = new Size((int)(width * 0.75) - S(32), 0);
            label.Location = new Point(S(16), S(10));
            bubble.Size = new Size(label.PreferredSize.Width + S(32), label.PreferredSize.Height + S(20));
            bubble.Location = new Point(width - bubble.Width, S(4));
            row.Height = bubble.Bottom + S(18);
        }) { Tag = text };
        row.Controls.Add(bubble);
        return row;
    }

    private ChatRow BotRow(string text, List<(string Label, Func<Task> Action)> choices, Quote? quote = null,
        List<ChatTable>? tables = null, bool error = false)
    {
        var mark = new AssistantMark { Size = new Size(S(28), S(28)) };
        var label = MessageLabel(text, ChatPalette.Background);
        if (error) label.ForeColor = Theme.Danger;
        var cards = new List<(RoundedPanel Panel, Action<int> Arrange)>();
        var shown = (tables ?? []).Where(t => t.Columns.Count > 0 && t.Rows.Count > 0).ToList();
        cards.AddRange(shown.Select(TableCard));
        if (quote != null) cards.Add(QuoteCard(quote));
        FlowLayoutPanel? chipPanel = null;
        if (choices.Count > 0)
        {
            chipPanel = new FlowLayoutPanel { BackColor = ChatPalette.Background, WrapContents = true, Margin = Padding.Empty };
            foreach (var (caption, action) in choices)
            {
                var chip = new ChipButton { Text = caption };
                chip.Click += async (_, _) => { if (!busy && chip.Enabled) await action(); };
                chipPanel.Controls.Add(chip);
                activeChips.Add(chip);
            }
        }
        var row = new ChatRow((row, width) =>
        {
            var x = S(40); var inner = width - x;
            mark.Location = new Point(0, S(2));
            label.MaximumSize = new Size(inner, 0);
            label.Location = new Point(x, S(4));
            var y = label.Location.Y + label.PreferredSize.Height + S(12);
            foreach (var card in cards)
            {
                card.Arrange(inner);
                card.Panel.Location = new Point(x, y);
                y = card.Panel.Bottom + S(12);
            }
            if (chipPanel != null)
            {
                foreach (ChipButton chip in chipPanel.Controls) chip.Fit(inner - S(8));
                var size = chipPanel.GetPreferredSize(new Size(inner, 0));
                chipPanel.Bounds = new Rectangle(x, y, inner, size.Height);
                y = chipPanel.Bottom + S(4);
            }
            row.Height = y + S(12);
        }) { Tag = string.Join("\n\n", shown.Select(TableText).Prepend(text)) };
        row.Controls.Add(mark);
        row.Controls.Add(label);
        foreach (var card in cards) row.Controls.Add(card.Panel);
        if (chipPanel != null) row.Controls.Add(chipPanel);
        return row;
    }

    /// <summary>Bản chữ của bảng cho chức năng Sao chép (cột cách nhau bằng tab, dán vào Excel được).</summary>
    private static string TableText(ChatTable table)
        => string.Join("\n", new[] { table.Title, table.Subtitle ?? "" }
            .Concat(table.Rows.Prepend(table.Columns).Select(r => string.Join("\t", r)))
            .Append(table.Note ?? "").Where(t => t.Length > 0));

    private Label CardLabel(RoundedPanel panel, string text, Font font, Color color)
    {
        var label = new Label { AutoSize = true, Text = text, Font = font, ForeColor = color, BackColor = ChatPalette.Surface, UseMnemonic = false, ContextMenuStrip = messageMenu };
        panel.Controls.Add(label);
        return label;
    }

    /// <summary>Thẻ bảng: tiêu đề, phụ đề, bảng dữ liệu, ghi chú.</summary>
    private (RoundedPanel Panel, Action<int> Arrange) TableCard(ChatTable table)
    {
        var panel = new RoundedPanel { FillColor = ChatPalette.Surface, BorderColor = ChatPalette.Border, Radius = S(14) };
        var title = CardLabel(panel, table.Title, boldFont, ChatPalette.Text);
        var subtitle = string.IsNullOrWhiteSpace(table.Subtitle) ? null : CardLabel(panel, table.Subtitle, smallFont, ChatPalette.Muted);
        var columns = table.Columns.ToArray();
        var rows = table.Rows.Select(r => columns.Select((_, i) => i < r.Count ? r[i] : "").ToArray()).ToList();
        var grid = new DataTableView(columns, rows, table.NumericColumns, smallFont, boldFont) { ContextMenuStrip = messageMenu };
        panel.Controls.Add(grid);
        var note = string.IsNullOrWhiteSpace(table.Note) ? null : CardLabel(panel, table.Note, smallFont, ChatPalette.Muted);
        return (panel, width =>
        {
            int pad = S(16), inner = width - 2 * pad, y = S(14);
            foreach (var label in panel.Controls.OfType<Label>()) label.MaximumSize = new Size(inner, 0);
            void Place(Label label, int gap) { label.Location = new Point(pad, y); y += label.PreferredSize.Height + gap; }
            Place(title, subtitle is null ? S(10) : S(2));
            if (subtitle != null) Place(subtitle, S(10));
            grid.Fit(inner, S(10));
            grid.Location = new Point(pad, y);
            y = grid.Bottom + (note is null ? S(14) : S(10));
            if (note != null) Place(note, S(14));
            panel.Size = new Size(width, y);
        });
    }

    /// <summary>Thẻ báo giá: nguồn, kho và cách lấy hàng, các chặng, từng khoản tiền, tổng cước.</summary>
    private (RoundedPanel Panel, Action<int> Arrange) QuoteCard(Quote quote)
    {
        var panel = new RoundedPanel { FillColor = ChatPalette.Surface, BorderColor = ChatPalette.Border, Radius = S(14) };
        var method = quote.PickupMethod == "company" ? "Kho cử xe tới lấy" : "Tự mang đến kho";
        var title = CardLabel(panel, $"Báo giá tham khảo · {quote.VehicleType}", boldFont, ChatPalette.Text);
        var source = CardLabel(panel, $"Nguồn: {quote.PriceList}, hiệu lực {quote.EffectiveDate:dd/MM/yyyy} · tra lúc {quote.QuotedAtUtc.ToUniversalTime().AddHours(7):dd/MM/yyyy HH:mm} (giờ VN)\n" +
            $"Kho: {quote.Warehouse} · {method}", smallFont, ChatPalette.Muted);
        var legs = quote.Legs.Select(leg => (
            Head: CardLabel(panel, $"{leg.Name}: {leg.From} → {leg.To}", messageFont, ChatPalette.Text),
            Km: CardLabel(panel, $"{leg.DistanceKm:N2} km", smallFont, ChatPalette.Muted))).ToList();
        var lineRule = new Panel { BackColor = ChatPalette.Border, Height = 1 };
        panel.Controls.Add(lineRule);
        var lines = quote.Lines.Select(l => (
            Name: CardLabel(panel, l.Name, smallFont, ChatPalette.Text),
            Detail: CardLabel(panel, l.Detail, smallFont, ChatPalette.Muted),
            Amount: CardLabel(panel, $"{l.Amount:N0} đ", smallFont, ChatPalette.Text))).ToList();
        var totalRule = new Panel { BackColor = ChatPalette.Border, Height = 1 };
        panel.Controls.Add(totalRule);
        var totalCaption = CardLabel(panel, "Tổng cước tham khảo", boldFont, ChatPalette.Text);
        var total = CardLabel(panel, $"{quote.Total:N0} đồng", boldFont, ChatPalette.Accent);
        return (panel, width =>
        {
            int pad = S(16), inner = width - 2 * pad, y = S(14);
            foreach (var label in panel.Controls.OfType<Label>()) label.MaximumSize = new Size(inner, 0);
            void Place(Label label, int gap) { label.Location = new Point(pad, y); y += label.PreferredSize.Height + gap; }
            Place(title, S(2));
            Place(source, S(10));
            foreach (var (head, km) in legs) { Place(head, S(2)); Place(km, S(8)); }
            lineRule.Bounds = new Rectangle(pad, y, inner, 1);
            y += S(10);
            foreach (var (name, detail, amount) in lines)
            {
                amount.MaximumSize = Size.Empty;
                var textWidth = Math.Max(S(80), inner - amount.PreferredSize.Width - S(12));
                name.MaximumSize = new Size(textWidth, 0);
                detail.MaximumSize = new Size(textWidth, 0);
                amount.Location = new Point(width - pad - amount.PreferredSize.Width, y);
                Place(name, 0);
                Place(detail, S(8));
            }
            totalRule.Bounds = new Rectangle(pad, y, inner, 1);
            y += S(10);
            totalCaption.Location = new Point(pad, y);
            total.Location = new Point(width - pad - total.PreferredSize.Width, y);
            y += Math.Max(totalCaption.PreferredSize.Height, total.PreferredSize.Height) + S(14);
            panel.Size = new Size(width, y);
        });
    }

    private (int Width, int Left) Column()
    {
        // Chừa sẵn chỗ thanh cuộn dọc để bề ngang không đổi khi thanh cuộn xuất hiện.
        var available = flowMessages.Width - SystemInformation.VerticalScrollBarWidth;
        var width = Math.Max(S(260), Math.Min(S(MaxColumn), available - S(40)));
        return (width, Math.Max(0, (available - width) / 2));
    }

    private void AddRow(ChatRow row)
    {
        var (width, left) = Column();
        row.Arrange(width, left);
        flowMessages.Controls.Add(row);
        flowMessages.ScrollControlIntoView(row);
        flowMessages.VerticalScroll.Value = flowMessages.VerticalScroll.Maximum;
        flowMessages.PerformLayout();
    }

    private void ArrangeRows()
    {
        var (width, left) = Column();
        flowMessages.SuspendLayout();
        foreach (var row in flowMessages.Controls.OfType<ChatRow>()) row.Arrange(width, left);
        flowMessages.ResumeLayout(true);
    }

    private void ClearRows()
    {
        HideTyping();
        var rows = flowMessages.Controls.Cast<Control>().ToList();
        flowMessages.Controls.Clear();
        foreach (var row in rows) row.Dispose();
    }

    /// <summary>Ô nhập giữa màn hình, cao dần theo nội dung (tối đa 6 dòng) như khung chat quen thuộc.</summary>
    private void LayoutComposer()
    {
        var available = composerHost.ClientSize.Width;
        var width = Math.Max(S(260), Math.Min(S(MaxColumn), available - S(40)));
        var left = Math.Max(0, (available - width) / 2);
        var textWidth = width - S(18) - S(56);
        var line = txtQuestion.Font.Height;
        var text = txtQuestion.Text.EndsWith('\n') || txtQuestion.TextLength == 0 ? txtQuestion.Text + "A" : txtQuestion.Text;
        var measured = TextRenderer.MeasureText(text, txtQuestion.Font, new Size(textWidth, 0),
            TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl).Height;
        var textHeight = Math.Clamp(measured, line, line * 6);
        txtQuestion.ScrollBars = measured > line * 6 ? ScrollBars.Vertical : ScrollBars.None;
        var boxHeight = Math.Max(S(56), textHeight + S(30));
        composerBox.Bounds = new Rectangle(left, S(4), width, boxHeight);
        txtQuestion.Bounds = new Rectangle(S(18), S(15), textWidth, textHeight);
        btnSend.Bounds = new Rectangle(width - S(44), boxHeight - S(44), S(32), S(32));
        lblNotice.Bounds = new Rectangle(left, composerBox.Bottom + S(6), width, S(18));
        var needed = lblNotice.Bottom + S(12);
        if (layout.RowStyles[2].Height != needed) layout.RowStyles[2].Height = needed;
        composerBox.Invalidate();
    }
}
