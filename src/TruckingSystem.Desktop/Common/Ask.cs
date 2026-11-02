namespace TruckingSystem.Desktop.Common
{
    /// <summary>HỘP HỎI LẠI kiểu mới (mẫu UX A3 đã duyệt) — thay cho MessageBox Yes/No:
    ///  • Tiêu đề là câu hỏi cụ thể ("Hoàn tất chuyến #12?"), nội dung ngắn.
    ///  • Nút ghi RÕ VIỆC ("✅ Hoàn tất chuyến #12") + "Để sau" — không còn Yes / No.
    ///  • danger = true: nút chính màu đỏ, Enter KHÔNG tự bấm (tránh lỡ tay), Esc = Để sau.
    /// Báo "đã xong" / lỗi nghiệp vụ thì dùng Toast (tự tắt), không bật hộp nữa.
    /// Dùng: if (!Ask.Confirm(this, "Hoàn tất chuyến #12?", "Xe đã tới nơi…", "✅ Hoàn tất chuyến #12")) return;</summary>
    public static class Ask
    {
        public static bool Confirm(Control? owner, string title, string message, string okText, bool danger = false, string cancelText = "Để sau")
        {
            using var dlg = new Form
            {
                Text = title.TrimEnd('?', ' '),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false, MinimizeBox = false, ShowInTaskbar = false,
                StartPosition = owner is null ? FormStartPosition.CenterScreen : FormStartPosition.CenterParent,
                AutoScaleMode = AutoScaleMode.Dpi, AutoScaleDimensions = new SizeF(96F, 96F),
                BackColor = Color.White, Font = new Font("Segoe UI", 10F),
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            };
            int S(int v) => dlg.LogicalToDeviceUnits(v);
            var root = new TableLayoutPanel
            {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1, Dock = DockStyle.Fill,
                Padding = new Padding(S(20), S(16), S(20), S(12)), MinimumSize = new Size(S(380), 0),
            };
            var head = new Label
            {
                Text = title, AutoSize = true, MaximumSize = new Size(S(480), 0), Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = danger ? Theme.Danger : Theme.TextPrimary, Margin = new Padding(0, 0, 0, S(8)),
            };
            var body = new Label
            {
                Text = message.Trim(), AutoSize = true, MaximumSize = new Size(S(480), 0), ForeColor = Theme.TextPrimary,
                Margin = new Padding(0, 0, 0, S(16)),
            };
            var cancel = new Button { Text = cancelText, AutoSize = true, MinimumSize = new Size(S(90), S(36)), DialogResult = DialogResult.Cancel };
            var ok = new Button { Text = okText, AutoSize = true, MinimumSize = new Size(S(120), S(36)), DialogResult = DialogResult.OK, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            ButtonStyles.Secondary(cancel);
            ButtonStyles.Primary(ok);
            if (danger)
            {
                ok.BackColor = Theme.Danger;
                ok.FlatAppearance.MouseOverBackColor = Color.FromArgb(185, 28, 28);
            }
            var foot = new FlowLayoutPanel
            {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill, WrapContents = false, Margin = Padding.Empty,
            };
            ok.Margin = new Padding(S(8), 0, 0, 0);
            cancel.Margin = Padding.Empty;
            foot.Controls.Add(ok);
            foot.Controls.Add(cancel);
            root.Controls.Add(head);
            root.Controls.Add(body);
            root.Controls.Add(foot);
            dlg.Controls.Add(root);
            dlg.CancelButton = cancel;
            if (!danger) dlg.AcceptButton = ok;
            dlg.Shown += (_, _) => (danger ? cancel : ok).Focus();
            var parent = owner?.FindForm() ?? owner as Form;
            return (parent is null ? dlg.ShowDialog() : dlg.ShowDialog(parent)) == DialogResult.OK;
        }

        /// <summary>Báo lỗi / chưa làm được bằng Toast cam (tự tắt), không chặn màn hình.</summary>
        public static void Fail(Control? anchor, string? message)
        {
            message = string.IsNullOrWhiteSpace(message) ? "Không thực hiện được." : message.Trim();
            Toast.Show(anchor, message.StartsWith("⚠") ? message : "⚠️ " + message, ToastKind.Warning);
        }
    }
}
