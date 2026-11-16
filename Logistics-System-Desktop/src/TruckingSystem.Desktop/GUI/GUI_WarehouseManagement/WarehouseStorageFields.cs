using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_WarehouseManagement
{
    /// <summary>Ô "📦 Số ngày lưu kho miễn phí" đặt CẠNH ô "💰 Phí lưu kho" (Tạo kho / Sửa kho) + dòng ví dụ tự tính theo 2 ô.
    /// Thêm bằng code (không sửa Designer): dời ô phí sang cột phải, thêm ô số ngày ở cột trái, thêm 1 hàng ví dụ bên dưới.</summary>
    internal static class WarehouseStorageFields
    {
        public static NumberBox Setup(TableLayoutPanel inputs, Label lblFee, NumberBox numFee, int freeDays = 5)
        {
            var feeRow = inputs.GetRow(lblFee);
            var numRow = inputs.GetRow(numFee);
            inputs.SuspendLayout();

            lblFee.Text = "💰 Phí lưu kho (đồng / m³ / ngày)";
            inputs.SetColumnSpan(lblFee, 1);
            inputs.SetColumnSpan(numFee, 1);
            inputs.SetColumn(lblFee, 1);
            inputs.SetColumn(numFee, 1);

            var lblFree = new Label
            {
                Text = "📦 Số ngày lưu kho miễn phí *",
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Font = lblFee.Font,
                ForeColor = lblFee.ForeColor,
                Margin = lblFee.Margin,
            };
            var numFree = new NumberBox
            {
                DecimalPlaces = 0,
                Minimum = 0,
                Maximum = 60,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 8, 12),
            };
            numFree.Value = Math.Clamp(freeDays, 0, 60);
            inputs.Controls.Add(lblFree, 0, feeRow);
            inputs.Controls.Add(numFree, 0, numRow);

            var example = new Label
            {
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(30, 64, 175),
                BackColor = Color.FromArgb(239, 246, 255),
                Padding = new Padding(10, 6, 10, 6),
                Margin = new Padding(0, 0, 0, 12),
            };
            inputs.RowCount += 1;
            inputs.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            inputs.Controls.Add(example, 0, inputs.RowCount - 1);
            inputs.SetColumnSpan(example, Math.Max(1, inputs.ColumnCount));
            inputs.ResumeLayout(true);

            void Fit() => example.MaximumSize = new Size(Math.Max(200, inputs.ClientSize.Width - 4), 0);
            void Refresh()
            {
                var days = (int)numFree.Value;
                var rate = numFee.Value;
                var start = DateTime.Today;
                var from = start.AddDays(days);
                example.Text = "Tính từ ngày hàng NHẬP KHO: miễn phí " + (days == 0 ? "0 ngày (tính phí ngay từ ngày đầu)" : $"{days} ngày") +
                               ", quá số ngày đó mỗi ngày hàng còn nằm kho = m³ × đơn giá." +
                               (rate <= 0
                                   ? " Phí 0 đ = kho không tính phí lưu kho."
                                   : $"\nVí dụ: hàng 12 m³ nhập ngày {start:dd/MM}" +
                                     (days > 0 ? $" thì miễn phí tới hết {from.AddDays(-1):dd/MM}, từ {from:dd/MM}" : ", từ ngày đó") +
                                     $" mỗi ngày tính 12 × {rate:N0} = {12 * rate:N0} đ cho tới khi xuất kho.");
            }
            numFree.ValueChanged += (_, _) => Refresh();
            numFee.ValueChanged += (_, _) => Refresh();
            inputs.SizeChanged += (_, _) => Fit();
            Fit();
            Refresh();
            return numFree;
        }
    }
}
