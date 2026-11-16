using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_TransportOrderManagement
{
    /// <summary>Hỏi bên chịu phí cho các ngày hàng nằm kho SAU HẠN TÍNH TRỄ — dùng chung cho Chi tiết đơn và Phí lưu kho.</summary>
    internal static class StorageDecision
    {
        /// <summary>Do công ty: hỏi xác nhận (trả ""). Do khách: hỏi lý do (bắt buộc ≥ 5 ký tự). null = người dùng bấm Để sau.</summary>
        public static string? AskReason(Control owner, string party)
        {
            if (party != "Customer")
                return Ask.Confirm(owner, "Hàng nằm kho sau hạn tính trễ do công ty?",
                    "Khách KHÔNG bị tính phí lưu kho cho những ngày này. Công ty chịu phần phí phát sinh (vẫn ghi nhật ký người chọn).",
                    "🏢 Xác nhận do công ty") ? "" : null;

            int S(int v) => owner.LogicalToDeviceUnits(v);
            using var dlg = new Form
            {
                Text = "Hàng nằm kho sau hạn tính trễ do khách hàng", FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false,
                StartPosition = FormStartPosition.CenterParent, ShowInTaskbar = false, BackColor = Color.White, Font = new Font("Segoe UI", 10F),
                AutoScaleMode = AutoScaleMode.Dpi, ClientSize = new Size(S(460), S(230)),
            };
            var head = new Label { Text = "Vì sao do khách hàng?", Font = new Font("Segoe UI", 12F, FontStyle.Bold), AutoSize = true, Location = new Point(S(18), S(14)) };
            var hint = new Label { Text = "VD: Khách hẹn lại ngày nhận hàng · Khách chưa thanh toán nên chưa giao", AutoSize = true, ForeColor = Theme.TextSecondary, Location = new Point(S(18), S(44)) };
            var box = new TextBox { Multiline = true, Location = new Point(S(18), S(70)), Size = new Size(S(424), S(70)) };
            var err = new Label { AutoSize = true, ForeColor = Theme.Danger, Location = new Point(S(18), S(146)) };
            var cancel = new Button { Text = "Để sau", AutoSize = true, DialogResult = DialogResult.Cancel, Location = new Point(S(220), S(182)) };
            var ok = new Button { Text = "🧾 Tính phí cho khách", AutoSize = true, Location = new Point(S(300), S(182)) };
            ButtonStyles.Secondary(cancel);
            ButtonStyles.Primary(ok);
            ok.Click += (_, _) =>
            {
                if (box.Text.Trim().Length < 5) { err.Text = "Ghi lý do ít nhất 5 ký tự."; box.Focus(); return; }
                dlg.DialogResult = DialogResult.OK;
            };
            dlg.Controls.AddRange(new Control[] { head, hint, box, err, cancel, ok });
            dlg.CancelButton = cancel;
            return dlg.ShowDialog(owner) == DialogResult.OK ? box.Text.Trim() : null;
        }
    }
}
