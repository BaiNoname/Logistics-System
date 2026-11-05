using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_TransportOrderManagement
{
    /// <summary>VỪA MỌI CỠ MÀN HÌNH:
    ///  • Phần ô nhập (① ②) cao theo nội dung, luôn hiện ĐỦ (trước: cố định 60% chiều cao, cuộn trong 1 khung nhỏ -> bị che).
    ///  • Bảng ③ Hàng hóa lấy phần còn lại (tối thiểu tiêu đề + 3 dòng).
    ///  • CHÂN CỬA SỔ ĐỨNG YÊN: dòng tóm tắt, cảnh báo và [Đóng] [💾 Lưu đơn] luôn thấy ở đáy; màn thấp thiếu chỗ thì chỉ
    ///    phần giữa cuộn. Cửa sổ tự nới tới hết chiều cao màn hình (DialogFit).
    /// Gọi SAU SetupSections (khối tóm tắt đã được chèn).</summary>
    public partial class CreateOrderForm
    {
        private void SetupFitLayout()
        {
            int S(int v) => LogicalToDeviceUnits(v);
            SuspendLayout();
            root.SuspendLayout();

            // 1) Ô nhập đặt thẳng vào root, hàng tự cao theo nội dung.
            var row = root.GetRow(fieldsPanel);
            var margin = fieldsPanel.Margin;
            fieldsPanel.Controls.Remove(fields);
            root.Controls.Remove(fieldsPanel);
            fieldsPanel.Dispose();
            fields.Dock = DockStyle.Fill;
            fields.Margin = margin;
            root.Controls.Add(fields, 0, row);
            root.RowStyles[row] = new RowStyle(SizeType.AutoSize);
            root.RowStyles[root.GetRow(gridCargoLines)] = new RowStyle(SizeType.Percent, 100F);

            // 2) Chân cửa sổ cố định: tóm tắt + cảnh báo + nút.
            var footer = new TableLayoutPanel
            {
                ColumnCount = 1, RowCount = 3, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Bottom,
                Padding = new Padding(root.Padding.Left, S(8), root.Padding.Right, root.Padding.Bottom), BackColor = BackColor,
            };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            var r = 0;
            foreach (var c in new Control[] { _summary, lblStatus, buttonBar })
            {
                root.Controls.Remove(c);
                footer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                footer.Controls.Add(c, 0, r++);
            }
            footer.Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(226, 232, 240));
                e.Graphics.DrawLine(pen, 0, 0, footer.Width, 0);
            };
            root.Padding = new Padding(root.Padding.Left, root.Padding.Top, root.Padding.Right, 0);

            // 3) Phần giữa nằm trong khung riêng — thiếu chỗ thì khung này cuộn, chân đứng yên.
            var host = new Panel { Dock = DockStyle.Fill, BackColor = BackColor };
            Controls.Remove(root);
            host.Controls.Add(root);
            Controls.Add(footer);
            Controls.Add(host);
            host.BringToFront(); // Fill xếp sau cùng -> chiếm phần còn lại phía trên chân

            root.ResumeLayout(true);
            ResumeLayout(true);
            DialogFit.Enable(this, root, host);
        }
    }
}
