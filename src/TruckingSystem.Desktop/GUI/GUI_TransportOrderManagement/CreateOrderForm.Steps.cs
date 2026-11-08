using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_TransportOrderManagement
{
    /// <summary>MÀN TẠO / SỬA ĐƠN chia 3 KHỐI đánh số (mẫu UX B3 đã duyệt) — sắp bằng code, không sửa Designer:
    ///  ① Khách hàng và hợp đồng · ② Lấy hàng, giao hàng và kho · ③ Hàng hóa (nút "➕ Thêm dòng hàng" rõ ràng thay cho
    ///  "bấm dòng trống cuối bảng"). Dải TÓM TẮT ngay trên chân cửa sổ: số dòng hàng, tổng kg / m³, có hàng dễ vỡ /
    ///  nguy hiểm, hạn giao đang chọn. Chân cửa sổ: [Đóng] [💾 Lưu đơn].</summary>
    public partial class CreateOrderForm
    {
        private readonly Label _summary = new()
        {
            AutoSize = true, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5F),
            ForeColor = Color.FromArgb(30, 64, 175), BackColor = Color.FromArgb(239, 246, 255),
            Padding = new Padding(10, 6, 10, 6), Margin = new Padding(0, 6, 0, 0),
        };

        private Label Section(string text) => new()
        {
            Text = text, AutoSize = true, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Theme.Primary, Margin = new Padding(0, LogicalToDeviceUnits(10), 0, LogicalToDeviceUnits(6)),
        };

        private void SetupSections()
        {
            int S(int v) => LogicalToDeviceUnits(v);
            fields.SuspendLayout();
            // Chèn từ dưới lên để chỉ số hàng không lệch.
            KpiStrip.InsertRow(fields, Section("② Lấy hàng, giao hàng và kho"), fields.GetRow(lblPickupMethod));
            KpiStrip.InsertRow(fields, Section("① Khách hàng và hợp đồng"), fields.GetRow(lblCustomer));
            fields.ResumeLayout(true);

            lblCargoTitle.Text = "③ Hàng hóa";
            lblCargoTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblCargoTitle.ForeColor = Theme.Primary;
            var add = new Button { Text = "➕ Thêm dòng hàng", AutoSize = true, Anchor = AnchorStyles.Right, Margin = new Padding(0, 0, S(8), 0) };
            ButtonStyles.Secondary(add);
            add.Click += (_, _) => { AddBlankCargoLine(); UpdateSummary(); };
            cargoHeader.ColumnCount = 3;
            cargoHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            cargoHeader.Controls.Remove(btnRemoveCargoLine);
            cargoHeader.Controls.Add(add, 1, 0);
            cargoHeader.Controls.Add(btnRemoveCargoLine, 2, 0);
            ButtonStyles.Secondary(btnRemoveCargoLine);

            // Dải tóm tắt ngay trên dòng trạng thái / chân cửa sổ.
            KpiStrip.InsertRow(root, _summary, root.GetRow(lblStatus));

            // Chân: [Đóng] [💾 Lưu đơn] (buttonBar xếp phải -> trái).
            var close = new Button { Text = "Đóng", AutoSize = true, MinimumSize = new Size(S(90), btnSave.MinimumSize.Height), DialogResult = DialogResult.Cancel };
            ButtonStyles.Secondary(close);
            close.Margin = new Padding(0, 0, S(8), 0);
            buttonBar.Controls.Add(close);
            CancelButton = close;
            ButtonStyles.Primary(btnSave);
            if (!btnSave.Text.StartsWith("💾")) btnSave.Text = "💾 " + btnSave.Text;

            _cargoLines.ListChanged += (_, _) => UpdateSummary();
            gridCargoLines.CellValueChanged += (_, _) => UpdateSummary();
            dtCommitted.ValueChanged += (_, _) => UpdateSummary();
            UpdateSummary();
        }

        private void UpdateSummary()
        {
            var lines = _cargoLines.ToList();
            var kg = lines.Sum(c => c.WeightKg);
            var m3 = lines.Sum(c => c.VolumeM3);
            var flags = new List<string>();
            if (lines.Any(c => c.IsHazmat)) flags.Add("☣️ có hàng nguy hiểm");
            if (lines.Any(c => c.IsFragile)) flags.Add("🔷 có hàng dễ vỡ");
            _summary.Text = $"Tóm tắt: {lines.Count} dòng hàng · {kg:#,##0.##} kg · {m3:#,##0.##} m³"
                            + (flags.Count > 0 ? " · " + string.Join(" · ", flags) : "")
                            + $" · hạn giao {dtCommitted.Value:dd/MM/yyyy}"
                            + (_contractBlocked && _contractBlockNote.Length > 0 ? " · ⛔ " + _contractBlockNote : "")
                            + (_estimatedDays is int d ? $" · tuyến chạy ~{d} ngày" : "");
        }
    }
}
