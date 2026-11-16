using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TripManagement;

namespace TruckingSystem.Desktop.GUI_TripManagement
{
    /// <summary>HỘP HOÀN TẤT / DUYỆT CHUYẾN có chốt phí lưu kho (mẫu duyet_luu_kho đã duyệt):
    ///  • Chuyến GIAO cuối làm đơn Hoàn tất mà hàng từng nằm kho sau hạn tính trễ CHƯA chọn do ai -> trong hộp có khung vàng
    ///    (số ngày, phí ước tính) + 2 lựa chọn: 🏢 Do công ty (chọn sẵn) / 🧾 Do khách hàng (hiện ô lý do, ≥ 5 ký tự mới bấm được).
    ///  • Đã chọn từ trước -> chỉ ghi lại 1 dòng xanh. Không có hàng nằm kho quá hạn -> như hộp hỏi lại thường.
    /// Máy chủ cũng kiểm tra (thiếu lựa chọn thì từ chối) — đây chỉ là chỗ hỏi cho tiện, 1 lần bấm là xong.</summary>
    public partial class TripsControl
    {
        private async Task<(bool Ok, string? Party, string? Reason)> AskFinishAsync(int tripId, string title, string body, string okText)
        {
            var check = _api is null ? null : await _api.Trips.GetFinishCheckAsync(tripId);
            var needChoice = check is { IsLastDelivery: true, OverdueDays: > 0, Decision: null };
            if (check is { IsLastDelivery: true } c && c.OverdueDays > 0 && c.Decision is not null)
                body += $"\n\n📦 Phí lưu kho (hàng nằm kho {c.OverdueDays} ngày sau hạn tính trễ) đã xác nhận " +
                        (c.Decision == "Customer" ? $"DO KHÁCH HÀNG: {c.EstimatedFee:N0} đ" : "DO CÔNG TY (khách 0 đ)") +
                        (string.IsNullOrWhiteSpace(c.DecisionReason) ? "." : $" — {c.DecisionReason}.");
            if (check is { IsLastDelivery: true }) title = title.TrimEnd('?') + $" — đơn {check.OrderCode} sẽ HOÀN TẤT?";
            if (!needChoice) return (Ask.Confirm(this, title, body, okText), null, null);
            return ShowStorageChoice(title, body, okText, check!);
        }

        private (bool Ok, string? Party, string? Reason) ShowStorageChoice(string title, string body, string okText, FinishCheckItem check)
        {
            var owner = FindForm();
            int S(int v) => LogicalToDeviceUnits(v);
            using var dlg = new Form
            {
                Text = title.TrimEnd('?'), FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false,
                ShowInTaskbar = false, StartPosition = FormStartPosition.CenterParent, BackColor = Color.White, Font = new Font("Segoe UI", 10F),
                AutoScaleMode = AutoScaleMode.Dpi, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            };
            var root = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(S(18), S(14), S(18), S(10)), Dock = DockStyle.Fill,
            };
            Label L(string text, Font font, Color color, int bottom = 6) => new()
            {
                Text = text, AutoSize = true, MaximumSize = new Size(S(500), 0), Font = font, ForeColor = color, UseMnemonic = false,
                Margin = new Padding(0, 0, 0, S(bottom)),
            };
            root.Controls.Add(L(title, new Font("Segoe UI", 12F, FontStyle.Bold), Theme.TextPrimary));
            root.Controls.Add(L(body, new Font("Segoe UI", 10F), Theme.TextSecondary, 10));

            // Khung vàng: số ngày + phí + 2 lựa chọn.
            var box = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.FromArgb(255, 251, 235), Padding = new Padding(S(10), S(8), S(10), S(8)), Margin = new Padding(0, 0, 0, S(8)),
                MinimumSize = new Size(S(500), 0),
            };
            var amber = Color.FromArgb(120, 53, 15);
            box.Controls.Add(L($"📦 Hàng đã nằm kho {check.OverdueDays} ngày sau hạn tính trễ ({check.Due:dd/MM/yyyy})",
                new Font("Segoe UI", 10F, FontStyle.Bold), amber, 2));
            box.Controls.Add(L($"Phí lưu kho ước tính: {check.EstimatedFee:N0} đ — chọn bên chịu trước khi duyệt:", new Font("Segoe UI", 9.5F), amber));
            var byCompany = new RadioButton { Text = "🏢 Do công ty — khách 0 đ, công ty chịu phần phí này", AutoSize = true, Checked = true, UseMnemonic = false };
            var byCustomer = new RadioButton { Text = $"🧾 Do khách hàng — tính {check.EstimatedFee:N0} đ cho khách (ghi lý do)", AutoSize = true, UseMnemonic = false };
            var reason = new TextBox { Width = S(460), Visible = false, PlaceholderText = "Lý do — VD: Khách hẹn lại ngày nhận hàng", Margin = new Padding(S(20), S(2), 0, S(2)) };
            box.Controls.AddRange(new Control[] { byCompany, byCustomer, reason });
            root.Controls.Add(box);

            var hint = new Label { AutoSize = true, ForeColor = Color.FromArgb(148, 163, 184), Font = new Font("Segoe UI", 9F), Anchor = AnchorStyles.Left };
            var cancel = new Button { Text = "Để sau", AutoSize = true, MinimumSize = new Size(S(90), S(36)), DialogResult = DialogResult.Cancel };
            var ok = new Button { Text = okText, AutoSize = true, MinimumSize = new Size(S(140), S(36)), DialogResult = DialogResult.OK, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            ButtonStyles.Secondary(cancel);
            ButtonStyles.Primary(ok);
            var foot = new TableLayoutPanel { AutoSize = true, ColumnCount = 3, MinimumSize = new Size(S(500), 0), Margin = Padding.Empty };
            foot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            foot.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            foot.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            cancel.Margin = new Padding(0, 0, S(8), 0);
            ok.Margin = Padding.Empty;
            foot.Controls.Add(hint, 0, 0);
            foot.Controls.Add(cancel, 1, 0);
            foot.Controls.Add(ok, 2, 0);
            root.Controls.Add(foot);
            dlg.Controls.Add(root);
            dlg.CancelButton = cancel;

            void Update()
            {
                reason.Visible = byCustomer.Checked;
                var valid = !byCustomer.Checked || reason.Text.Trim().Length >= 5;
                ok.Enabled = valid;
                hint.Text = valid ? "" : "Ghi lý do (≥ 5 ký tự) để duyệt";
            }
            byCompany.CheckedChanged += (_, _) => Update();
            byCustomer.CheckedChanged += (_, _) => { Update(); if (byCustomer.Checked) reason.Focus(); };
            reason.TextChanged += (_, _) => Update();
            Update();

            var result = owner is null ? dlg.ShowDialog() : dlg.ShowDialog(owner);
            if (result != DialogResult.OK) return (false, null, null);
            return (true, byCustomer.Checked ? "Customer" : "Company", byCustomer.Checked ? reason.Text.Trim() : null);
        }
    }
}
