using System.Drawing.Drawing2D;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TransportOrderManagement;

namespace TruckingSystem.Desktop.GUI_TransportOrderManagement
{
    /// <summary>Đầu màn Phí lưu kho: 4 MỐC NGÀY (Nhập kho · Hết miễn phí · Hạn tính trễ · Xuất hết) + THANH THỜI GIAN mỗi ô
    /// 1 ngày hàng nằm kho, 3 màu: xanh lá = miễn phí, xanh dương = tính phí khách, vàng = sau hạn tính trễ (xám nhạt khi
    /// công ty chịu) + chú thích. Tự vẽ, không dùng Designer.</summary>
    internal sealed class StorageTimelineBar : Control
    {
        private static readonly Color FreeColor = ColorTranslator.FromHtml("#BBF7D0");
        private static readonly Color ChargedColor = ColorTranslator.FromHtml("#BFDBFE");
        private static readonly Color AfterColor = ColorTranslator.FromHtml("#FDE68A");
        private static readonly Color CompanyColor = ColorTranslator.FromHtml("#E2E8F0");

        private StorageFeeItem? _fee;

        public StorageTimelineBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9F);
            Height = 130;
        }

        private float Sc => DeviceDpi / 96f;

        public void SetData(StorageFeeItem fee)
        {
            _fee = fee;
            Height = (int)Math.Round((fee.Days.Count == 0 ? 62 : 128) * Sc);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_fee is null || Width <= 0) return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var s = Sc;
            var f = _fee;

            // ---- 4 mốc ngày ----
            var noFree = f.FreeUntil is { } fu0 && f.ReceivedDate is { } r0 && fu0 < r0;
            var marks = new (string Caption, string Value, Color Fore)[]
            {
                ("Nhập kho", f.ReceivedDate is { } r ? r.ToString("dd/MM") : "chưa nhập", Theme.TextPrimary),
                ("Hết miễn phí", f.RatePerM3PerDay <= 0 ? "không tính phí" : noFree ? "không miễn phí" : f.FreeUntil is { } fu ? fu.ToString("dd/MM") : "—",
                    ColorTranslator.FromHtml("#166534")),
                ("Hạn tính trễ", f.LateBasisDate is { } b ? b.ToString("dd/MM") : "chưa có hạn", ColorTranslator.FromHtml("#92400E")),
                ("Xuất hết", f.LastIssueDate is { } li ? li.ToString("dd/MM") : f.StillInStock ? $"còn {f.VolumeNowM3:0.##} m³" : "—", Theme.TextPrimary),
            };
            var gap = 8 * s;
            var boxW = (Width - gap * 3 - 1) / 4f;
            var boxH = 54 * s;
            using var capFont = new Font("Segoe UI", 8.5F);
            using var valFont = new Font("Segoe UI", 12F, FontStyle.Bold);
            using var border = new Pen(Theme.BorderColor);
            using var boxBack = new SolidBrush(ColorTranslator.FromHtml("#F8FAFC"));
            const TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding;
            for (var i = 0; i < marks.Length; i++)
            {
                var rect = new RectangleF(i * (boxW + gap), 0, boxW, boxH);
                using (var path = Anim.RoundRect(rect, 8 * s)) { g.FillPath(boxBack, path); g.DrawPath(border, path); }
                TextRenderer.DrawText(g, marks[i].Caption, capFont,
                    Rectangle.Round(new RectangleF(rect.X + 10 * s, rect.Y + 6 * s, rect.Width - 16 * s, 18 * s)), Theme.TextSecondary, flags);
                TextRenderer.DrawText(g, marks[i].Value, valFont,
                    Rectangle.Round(new RectangleF(rect.X + 10 * s, rect.Y + 24 * s, rect.Width - 16 * s, 26 * s)), marks[i].Fore, flags);
            }
            if (f.Days.Count == 0) return;

            // ---- thanh thời gian: mỗi ô 1 ngày hàng nằm kho ----
            var top = boxH + 12 * s;
            var barH = 18 * s;
            var cellGap = f.Days.Count > 60 ? 0 : 2 * s;
            var cellW = (Width - 1 - cellGap * (f.Days.Count - 1)) / f.Days.Count;
            for (var i = 0; i < f.Days.Count; i++)
            {
                var color = f.Days[i].Kind switch
                {
                    "Free" => FreeColor,
                    "AfterBasis" => f.AfterBasisDecision == "Company" ? CompanyColor : AfterColor,
                    _ => ChargedColor,
                };
                using var br = new SolidBrush(color);
                var cell = new RectangleF(i * (cellW + cellGap), top, Math.Max(1, cellW), barH);
                if (cellW >= 6 * s)
                {
                    using var p = Anim.RoundRect(cell, 3 * s);
                    g.FillPath(br, p);
                }
                else g.FillRectangle(br, cell);
            }

            // ---- chú thích ----
            var freeCount = f.Days.Count(d => d.Kind == "Free");
            var legend = new List<(Color Color, string Text)>();
            if (freeCount > 0) legend.Add((FreeColor, $"Miễn phí ({freeCount} ngày)"));
            legend.Add((ChargedColor, "Tính phí khách"));
            if (f.AfterBasisDays > 0)
                legend.Add(f.AfterBasisDecision switch
                {
                    "Customer" => (AfterColor, "Sau hạn tính trễ, do khách (tính phí)"),
                    "Company" => (CompanyColor, "Sau hạn tính trễ, do công ty (0 đ)"),
                    _ => (AfterColor, "Sau hạn tính trễ, chờ chọn bên chịu"),
                });
            var x = 0f;
            var ly = top + barH + 10 * s;
            foreach (var (c, text) in legend)
            {
                using var br = new SolidBrush(c);
                using (var p = Anim.RoundRect(new RectangleF(x, ly + 3 * s, 12 * s, 12 * s), 3 * s)) g.FillPath(br, p);
                var size = TextRenderer.MeasureText(g, text, capFont, Size.Empty, TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, text, capFont, new Point((int)(x + 17 * s), (int)(ly + 1 * s)), Theme.TextSecondary, TextFormatFlags.NoPadding);
                x += 17 * s + size.Width + 18 * s;
            }
        }
    }
}
