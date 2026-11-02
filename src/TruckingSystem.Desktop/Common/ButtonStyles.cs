namespace TruckingSystem.Desktop.Common
{
    /// <summary>QUY TẮC NÚT thống nhất cho mọi cửa sổ (theo mẫu UX đã duyệt):
    ///  • Primary: nút CHÍNH (việc nên làm tiếp theo) — nền xanh dương đặc, mỗi cửa sổ chỉ 1 nút.
    ///  • Secondary: các nút khác — nền trắng, viền xám.
    ///  • DangerLink: việc nguy hiểm (Hủy đơn, Xóa, Hoàn tác…) — chữ đỏ không nền, đặt góc dưới TRÁI, tách xa nút chính.
    /// + Footer: chân cửa sổ cố định [nguy hiểm] ........ [Đóng] [Nút chính].</summary>
    public static class ButtonStyles
    {
        public static void Primary(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.BackColor = Theme.Primary;
            b.ForeColor = Color.White;
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Theme.PrimaryDark;
            b.UseVisualStyleBackColor = false;
            b.Cursor = Cursors.Hand;
        }

        public static void Secondary(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.BackColor = Color.White;
            b.ForeColor = Theme.TextPrimary;
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = Theme.BorderColor;
            b.FlatAppearance.MouseOverBackColor = Theme.GridAltRow;
            b.UseVisualStyleBackColor = false;
            b.Cursor = Cursors.Hand;
        }

        public static void DangerLink(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.BackColor = Color.Transparent;
            b.ForeColor = Theme.Danger;
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(254, 226, 226);
            b.UseVisualStyleBackColor = false;
            b.Cursor = Cursors.Hand;
        }

        /// <summary>Chân cửa sổ: nút nguy hiểm (nếu có) bên trái, các nút bên phải theo thứ tự truyền vào (Đóng trước,
        /// nút chính sau cùng). Gắn Dock = Bottom vào form, đưa phần nội dung (Dock = Fill) lên trước để không bị che.</summary>
        public static TableLayoutPanel Footer(Form form, Control? danger, params Control[] right)
        {
            int S(int v) => form.LogicalToDeviceUnits(v);
            var bar = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2, RowCount = 1, BackColor = Theme.GridAltRow,
                Padding = new Padding(S(14), S(8), S(14), S(8)),
            };
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            if (danger is not null)
            {
                danger.Anchor = AnchorStyles.Left;
                bar.Controls.Add(danger, 0, 0);
            }
            var flow = new FlowLayoutPanel
            {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight, Anchor = AnchorStyles.Right, Margin = Padding.Empty,
            };
            foreach (var c in right) { c.Margin = new Padding(S(8), 0, 0, 0); flow.Controls.Add(c); }
            bar.Controls.Add(flow, 1, 0);
            bar.Paint += (_, e) =>
            {
                using var pen = new Pen(Theme.BorderColor);
                e.Graphics.DrawLine(pen, 0, 0, bar.Width, 0);
            };
            form.Controls.Add(bar);
            foreach (var c in form.Controls.Cast<Control>().Where(c => c.Dock == DockStyle.Fill).ToList()) c.BringToFront();
            return bar;
        }

        /// <summary>Khung "👉 Việc tiếp theo: …" (nền xanh nhạt) — đặt trong cửa sổ chi tiết, đổi chữ theo trạng thái.</summary>
        public static Label NextStep() => new()
        {
            AutoSize = true, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F),
            ForeColor = Color.FromArgb(30, 64, 175), BackColor = Color.FromArgb(239, 246, 255),
            Padding = new Padding(10, 7, 10, 7), Margin = new Padding(0, 4, 0, 8),
        };
    }
}
