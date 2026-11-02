using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TransportOrderManagement;

namespace TruckingSystem.Desktop.GUI_TransportOrderManagement
{
    /// <summary>KHUNG CẢNH BÁO trong Chi tiết đơn (mẫu han_giao đã duyệt):
    ///  • ⏰ Không kịp hạn: chuyến giao đã qua mốc xuất phát / đang chạy dự kiến trễ -> khung ĐỎ giải thích + nút chính
    ///    "📅 Báo khách và dời hạn (do công ty)" mở sẵn form Đổi hạn (ngày gợi ý = giao sớm nhất, bên yêu cầu = Công ty).
    ///  • 📦 Hàng nằm kho quá hạn giao: khung VÀNG tính sẵn phí (m³ × ngày × đơn giá) — mặc định công ty chịu; 2 nút
    ///    "🏢 Do công ty" / "🧾 Do khách hàng…" (bắt buộc lý do). Đã chọn rồi thì khung xanh nhạt ghi rõ + "Đổi lựa chọn".</summary>
    public partial class OrderDetailForm
    {
        private Action? _fit;
        private readonly Label _lateBox = Box(Color.FromArgb(254, 242, 242), Color.FromArgb(127, 29, 29));
        private readonly Label _storageBox = Box(Color.FromArgb(255, 251, 235), Color.FromArgb(120, 53, 15));
        private readonly Button _btnReschedule = new() { Text = "📅 Báo khách và dời hạn (do công ty)", AutoSize = true, Visible = false };
        private readonly Button _btnByCompany = new() { Text = "🏢 Do công ty", AutoSize = true, Visible = false };
        private readonly Button _btnByCustomer = new() { Text = "🧾 Do khách hàng…", AutoSize = true, Visible = false };
        private readonly FlowLayoutPanel _storageButtons = new() { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, Margin = new Padding(0, 0, 0, 8), Visible = false };

        private static Label Box(Color back, Color fore) => new()
        {
            AutoSize = true, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F), BackColor = back, ForeColor = fore,
            Padding = new Padding(12, 8, 12, 8), Margin = new Padding(0, 4, 0, 6), Visible = false,
        };

        private void SetupAlerts()
        {
            // Thứ tự: [Việc tiếp theo] → [khung đỏ trễ hạn] → [khung vàng nằm kho] → [2 nút Do công ty / Do khách].
            KpiStrip.InsertRow(root, _lateBox, root.GetRow(_nextStep) + 1);
            KpiStrip.InsertRow(root, _storageBox, root.GetRow(_lateBox) + 1);
            KpiStrip.InsertRow(root, _storageButtons, root.GetRow(_storageBox) + 1);
            foreach (var l in new[] { _lateBox, _storageBox }) l.UseMnemonic = false; // chữ "&" hiện đúng
            ButtonStyles.Primary(_btnReschedule);
            _btnReschedule.Margin = new Padding(LogicalToDeviceUnits(8), 0, 0, 0);
            _primaryBox.Controls.Add(_btnReschedule);
            _btnReschedule.Click += async (_, _) => await RescheduleAsync();
            ButtonStyles.Secondary(_btnByCompany);
            ButtonStyles.Primary(_btnByCustomer);
            _btnByCompany.Margin = new Padding(0, 0, LogicalToDeviceUnits(8), 0);
            _storageButtons.Controls.Add(_btnByCompany);
            _storageButtons.Controls.Add(_btnByCustomer);
            _btnByCompany.Click += async (_, _) => await SetResponsibilityAsync("Company");
            _btnByCustomer.Click += async (_, _) => await SetResponsibilityAsync("Customer");
        }

        /// <summary>Gọi sau UpdateActionLayout mỗi lần tải đơn.</summary>
        private void UpdateAlerts(TransportOrderDetailItem order)
        {
            // ⏰ Không kịp hạn giao
            var late = order.LateWarning;
            _lateBox.Visible = late is not null;
            _btnReschedule.Visible = late is not null && order.Status is "Confirmed" or "InProgress";
            if (late is not null)
            {
                _lateBox.Text = "⏰ " + late + "\nNên gọi báo khách trước rồi dời hạn — trễ do công ty thì KHÔNG tính phí lưu kho cho khách.";
                _nextStep.Text = "👉 Việc tiếp theo: gọi báo khách, rồi bấm \"📅 Báo khách và dời hạn (do công ty)\" ở góc phải.";
            }

            // 📦 Hàng nằm kho quá hạn giao
            var s = order.OverdueStorage;
            _storageBox.Visible = s is not null;
            _storageButtons.Visible = s is not null;
            if (s is null) return;
            var fee = $"{s.VolumeM3:0.##} m³ × {s.Days} ngày × {s.RatePerM3PerDay:N0} đ = {s.EstimatedFee:N0} đ (ước tính)";
            switch (s.Decision)
            {
                case "Customer":
                    _storageBox.BackColor = Color.FromArgb(239, 246, 255);
                    _storageBox.ForeColor = Color.FromArgb(30, 64, 175);
                    _storageBox.Text = $"📦 Hàng nằm kho {s.Days} ngày sau hạn giao — đã xác nhận DO KHÁCH HÀNG.\n" +
                                       $"Phí lưu kho tính cho khách, tự cộng mỗi ngày tới khi hàng rời kho: {fee}. Xem chi tiết ở 💰 Phí lưu kho.";
                    _btnByCustomer.Visible = false;
                    _btnByCompany.Visible = true;
                    _btnByCompany.Text = "↩️ Đổi thành do công ty";
                    break;
                case "Company":
                    _storageBox.BackColor = Color.FromArgb(240, 253, 244);
                    _storageBox.ForeColor = Color.FromArgb(22, 101, 52);
                    _storageBox.Text = $"📦 Hàng nằm kho {s.Days} ngày sau hạn giao — đã xác nhận DO CÔNG TY (khách 0 đ).\n" +
                                       $"Công ty đang chịu: {fee}.";
                    _btnByCompany.Visible = false;
                    _btnByCustomer.Visible = true;
                    _btnByCustomer.Text = "↩️ Đổi thành do khách hàng…";
                    ButtonStyles.Secondary(_btnByCustomer);
                    break;
                default:
                    _storageBox.BackColor = Color.FromArgb(255, 251, 235);
                    _storageBox.ForeColor = Color.FromArgb(120, 53, 15);
                    _storageBox.Text = $"📦 Hàng nằm kho {s.Days} ngày sau hạn giao — CHƯA xác định do ai.\n" +
                                       $"Phí lưu kho phát sinh: {fee} — hiện CÔNG TY đang chịu (khách 0 đ).\n" +
                                       "Do khách (hẹn lại ngày nhận, chưa thanh toán…) thì chọn \"Do khách hàng\" — phí tự cộng mỗi ngày tới khi hàng rời kho.";
                    _btnByCompany.Visible = _btnByCustomer.Visible = true;
                    _btnByCompany.Text = "🏢 Do công ty";
                    _btnByCustomer.Text = "🧾 Do khách hàng…";
                    ButtonStyles.Primary(_btnByCustomer);
                    if (late is null)
                        _nextStep.Text = "👉 Việc tiếp theo: xác định hàng nằm kho quá hạn là do ai — bấm \"Do khách hàng…\" hoặc \"Do công ty\".";
                    break;
            }
        }

        private async Task RescheduleAsync()
        {
            if (_api is null || _currentOrder is null) return;
            using var f = new ChangeDeadlineForm(_api, _currentOrder);
            f.Preset(_currentOrder.SuggestedDeadline, "Company", "Xe xuất phát trễ so với mốc — đã gọi báo khách.");
            if (f.ShowDialog(this) != DialogResult.OK) return;
            Toast.Show(this, "✅ " + (f.ResultMessage ?? "Đã dời hạn giao."));
            await LoadAsync();
        }

        private async Task SetResponsibilityAsync(string party)
        {
            if (_api is null || _currentOrder is null) return;
            string? reason = null;
            if (party == "Customer")
            {
                reason = AskReason();
                if (reason is null) return;
            }
            else if (!Ask.Confirm(this, "Hàng nằm kho quá hạn do công ty?",
                         "Khách KHÔNG bị tính phí lưu kho cho những ngày này. Công ty chịu phần phí phát sinh (vẫn ghi nhật ký người chọn).",
                         "🏢 Xác nhận do công ty"))
                return;
            try
            {
                var result = await _api.TransportOrders.SetStorageResponsibilityAsync(_currentOrder.Id, party, reason);
                if (!result.Success) { Ask.Fail(this, result.Message); return; }
                Toast.Show(this, "✅ " + result.Message);
                await LoadAsync();
            }
            catch (Exception ex) { Ask.Fail(this, "Lỗi: " + ex.Message); }
        }

        /// <summary>Hộp nhỏ hỏi lý do "do khách hàng" (bắt buộc ≥ 5 ký tự). null = bấm Để sau.</summary>
        private string? AskReason()
        {
            int S(int v) => LogicalToDeviceUnits(v);
            using var dlg = new Form
            {
                Text = "Hàng nằm kho quá hạn do khách hàng", FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false,
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
            return dlg.ShowDialog(this) == DialogResult.OK ? box.Text.Trim() : null;
        }
    }
}
