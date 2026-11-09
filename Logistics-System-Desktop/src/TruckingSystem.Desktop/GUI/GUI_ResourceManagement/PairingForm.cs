using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_ResourceManagement
{
    /// <summary>GHÉP XE – TÀI XẾ (chuột phải 1 xe / 1 tài xế ở màn Nguồn lực theo kho): dòng đầu cho biết đang đi cùng
    /// ai / xe nào trên chuyến; bảng dưới liệt kê các tài xế lái được xe này (đủ hạng bằng) — hoặc các xe tài xế này
    /// lái được — kèm tình trạng hôm nay, người / xe chạy được xếp lên trước. Thay cho 2 cột "Trên chuyến" và
    /// "Hợp … đang chọn" ở bảng ngoài (bảng ngoài gọn hơn).
    /// Form dựng bằng code (không có Designer) — không mở bằng trình thiết kế của Visual Studio.</summary>
    public sealed class PairingForm : BaseForm
    {
        public PairingForm(string title, string onTripText, string listTitle, object rows, int count)
        {
            Text = title;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(LogicalToDeviceUnits(820), LogicalToDeviceUnits(520));
            MinimumSize = new Size(LogicalToDeviceUnits(520), LogicalToDeviceUnits(360));
            BackColor = Color.White;

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(18, 14, 18, 12) };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            root.Controls.Add(new Label { Text = title, AutoSize = true, Font = Theme.FontSectionTitle, ForeColor = Theme.TextPrimary, Margin = new Padding(0, 0, 0, 8) }, 0, 0);
            root.Controls.Add(new Label
            {
                Text = "🚚 Đang trên chuyến: " + onTripText, AutoSize = true, Font = Theme.FontBold,
                ForeColor = onTripText == "—" ? Theme.TextSecondary : Theme.Primary,
                BackColor = Color.FromArgb(239, 246, 255), Padding = new Padding(10, 8, 10, 8), Margin = new Padding(0, 0, 0, 10),
            }, 0, 1);
            root.Controls.Add(new Label
            {
                Text = $"{listTitle} ({count})", AutoSize = true, Font = Theme.FontBold, ForeColor = Theme.TextPrimary,
                Margin = new Padding(0, 0, 0, 4),
            }, 0, 2);

            var grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, ScrollBars = ScrollBars.Both };
            Theme.StyleGrid(grid);
            GridLayout.FitColumns(grid);
            grid.CellFormatting += (_, e) =>
            {
                if (e.Value is not string t || e.CellStyle is null) return;
                if (t.StartsWith("✅")) e.CellStyle.ForeColor = Theme.Success;
                else if (t.StartsWith("🟡") || t.StartsWith("⚠️")) e.CellStyle.ForeColor = Theme.Warning;
                else if (t.StartsWith("⛔") || t.StartsWith("❌")) e.CellStyle.ForeColor = Theme.Danger;
            };
            grid.DataSource = rows;
            root.Controls.Add(grid, 0, 3);

            var close = Theme.MakeSecondaryButton("Đóng", 100);
            close.Anchor = AnchorStyles.Right;
            close.Margin = new Padding(0, 10, 0, 0);
            close.Click += (_, _) => Close();
            CancelButton = close;
            root.Controls.Add(close, 0, 4);
            Controls.Add(root);
        }
    }
}
