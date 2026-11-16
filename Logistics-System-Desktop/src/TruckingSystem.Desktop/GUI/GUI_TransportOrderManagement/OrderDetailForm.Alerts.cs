using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TransportOrderManagement;

namespace TruckingSystem.Desktop.GUI_TransportOrderManagement
{
    /// <summary>KHUNG CẢNH BÁO trong Chi tiết đơn (mẫu han_giao đã duyệt):
    ///  • ⏰ Không kịp hạn: chuyến giao đã qua mốc xuất phát / đang chạy dự kiến trễ -> khung ĐỎ giải thích + nút chính
    ///    "📅 Báo khách và dời hạn (do công ty)" mở sẵn form Đổi hạn (ngày gợi ý = giao sớm nhất, bên yêu cầu = Công ty).
    ///  • ⏰ Công ty đã dời hạn mà đã qua HẠN TÍNH TRỄ: khung đỏ nói rõ đơn đã trễ (dời hạn do công ty không xóa việc trễ).
    ///  • 📦 Hàng nằm kho sau hạn tính trễ: khung VÀNG tính sẵn phí những ngày đó — chưa chọn thì chưa tính; 2 nút
    ///    "🏢 Do công ty" / "🧾 Do khách hàng…" (bắt buộc lý do). Đã chọn rồi thì khung xanh nhạt ghi rõ + "Đổi lựa chọn".
    ///  • 📅 Lịch sử đổi hạn: bảng nhỏ có cột "Hạn tính trễ" (chỉ hiện khi đơn đã đổi hạn).</summary>
    public partial class OrderDetailForm
    {
        private Action? _fit;
        private readonly Label _lateBox = Box(Color.FromArgb(254, 242, 242), Color.FromArgb(127, 29, 29));
        private readonly Label _storageBox = Box(Color.FromArgb(255, 251, 235), Color.FromArgb(120, 53, 15));
        private readonly Button _btnReschedule = new() { Text = "📅 Báo khách và dời hạn (do công ty)", AutoSize = true, Visible = false };
        private readonly Button _btnByCompany = new() { Text = "🏢 Do công ty", AutoSize = true, Visible = false };
        private readonly Button _btnByCustomer = new() { Text = "🧾 Do khách hàng…", AutoSize = true, Visible = false };
        private readonly FlowLayoutPanel _storageButtons = new() { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, Margin = new Padding(0, 0, 0, 8), Visible = false };
        private readonly Label _historyHead = new()
        {
            AutoSize = true, Text = "📅 Lịch sử đổi hạn giao", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Theme.TextPrimary,
            Margin = new Padding(0, 6, 0, 4), Visible = false,
        };
        private readonly DataGridView _historyGrid = new() { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 8), Visible = false };

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
            KpiStrip.InsertRow(root, _historyHead, root.GetRow(_storageButtons) + 1);
            KpiStrip.InsertRow(root, _historyGrid, root.GetRow(_historyHead) + 1);
            Theme.StyleGrid(_historyGrid);
            _historyGrid.ScrollBars = ScrollBars.None;
            GridDecor.Pills(_historyGrid, "Bên_yêu_cầu", (text, _) => text.Contains("Khách") ? GridDecor.Blue : GridDecor.Amber);
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
            _btnReschedule.Visible = late is not null && order.Status is "Confirmed" or "InProgress";
            // Công ty đã dời hạn mà hôm nay đã qua hạn tính trễ -> đơn ĐÃ trễ (dời hạn không xóa việc trễ).
            var lateByBasis = order.HasLateBasis && order.LateBasisDate is DateTime basis && DateTime.Today > basis.Date
                              && order.Status is "Confirmed" or "InProgress";
            _lateBox.Visible = late is not null || lateByBasis;
            if (lateByBasis)
            {
                var b = order.LateBasisDate!.Value.Date;
                _lateBox.Text = $"⏰ Đơn đã trễ {(DateTime.Today - b).Days} ngày so với hạn tính trễ {b:dd/MM} " +
                                $"(công ty dời hạn sang {order.CommittedDeliveryDate:dd/MM} không xóa việc trễ).\n" +
                                $"Những ngày hàng nằm kho sau {b:dd/MM} do lỗi công ty thì KHÔNG tính phí lưu kho cho khách." +
                                (late is null ? "" : "\n" + late);
            }
            else if (late is not null)
            {
                _lateBox.Text = "⏰ " + late + "\nNên gọi báo khách trước rồi dời hạn — công ty dời hạn thì vẫn tính trễ theo hạn cũ, " +
                                "và ngày hàng nằm kho sau hạn do lỗi công ty thì KHÔNG tính phí lưu kho cho khách.";
                _nextStep.Text = "👉 Việc tiếp theo: gọi báo khách, rồi bấm \"📅 Báo khách và dời hạn (do công ty)\" ở góc phải.";
            }

            // 📦 Hàng nằm kho sau hạn tính trễ
            var s = order.OverdueStorage;
            _storageBox.Visible = s is not null;
            _storageButtons.Visible = s is not null;
            if (s is null) return;
            var fee = $"~{s.VolumeM3:0.##} m³/ngày × {s.Days} ngày × {s.RatePerM3PerDay:N0} đ = {s.EstimatedFee:N0} đ";
            var after = $"sau hạn tính trễ {(s.LateBasisDate ?? order.LateBasisDate ?? order.CommittedDeliveryDate):dd/MM}";
            switch (s.Decision)
            {
                case "Customer":
                    _storageBox.BackColor = Color.FromArgb(239, 246, 255);
                    _storageBox.ForeColor = Color.FromArgb(30, 64, 175);
                    _storageBox.Text = $"📦 Hàng nằm kho {s.Days} ngày {after} — đã xác nhận DO KHÁCH HÀNG.\n" +
                                       $"Phí lưu kho tính cho khách, tự cộng mỗi ngày tới khi hàng rời kho: {fee}. Xem chi tiết ở 💰 Phí lưu kho.";
                    _btnByCustomer.Visible = false;
                    _btnByCompany.Visible = true;
                    _btnByCompany.Text = "↩️ Đổi thành do công ty";
                    break;
                case "Company":
                    _storageBox.BackColor = Color.FromArgb(240, 253, 244);
                    _storageBox.ForeColor = Color.FromArgb(22, 101, 52);
                    _storageBox.Text = $"📦 Hàng nằm kho {s.Days} ngày {after} — đã xác nhận DO CÔNG TY (khách 0 đ).\n" +
                                       $"Công ty đang chịu: {fee}.";
                    _btnByCompany.Visible = false;
                    _btnByCustomer.Visible = true;
                    _btnByCustomer.Text = "↩️ Đổi thành do khách hàng…";
                    ButtonStyles.Secondary(_btnByCustomer);
                    break;
                default:
                    _storageBox.BackColor = Color.FromArgb(255, 251, 235);
                    _storageBox.ForeColor = Color.FromArgb(120, 53, 15);
                    _storageBox.Text = $"📦 Hàng nằm kho {s.Days} ngày {after} (đã hết ngày miễn phí) — CHƯA xác định do ai.\n" +
                                       $"Phí những ngày này: {fee} — chưa tính cho khách, phí lưu kho chưa chốt được tới khi chọn.\n" +
                                       "Do khách (hẹn lại ngày nhận, chưa thanh toán…) thì chọn \"Do khách hàng\" — phí tự cộng mỗi ngày tới khi hàng rời kho.";
                    _btnByCompany.Visible = _btnByCustomer.Visible = true;
                    _btnByCompany.Text = "🏢 Do công ty";
                    _btnByCustomer.Text = "🧾 Do khách hàng…";
                    ButtonStyles.Primary(_btnByCustomer);
                    if (late is null)
                        _nextStep.Text = "👉 Việc tiếp theo: xác định hàng nằm kho sau hạn tính trễ là do ai — bấm \"Do khách hàng…\" hoặc \"Do công ty\".";
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
            var reason = StorageDecision.AskReason(this, party);
            if (reason is null) return;
            try
            {
                var result = await _api.TransportOrders.SetStorageResponsibilityAsync(_currentOrder.Id, party, party == "Customer" ? reason : null);
                if (!result.Success)
                {
                    // VD kho chưa khai báo phí lưu kho -> nút mở Sửa kho của đúng kho, xong chọn lại.
                    await TruckingSystem.Desktop.GUI_Shared.QuickFix.ShowAsync(this, _api, "Chưa ghi nhận được bên chịu phí", result.Message, result.Fixes,
                        "🧾 Chọn lại bên chịu phí", () => SetResponsibilityAsync(party));
                    return;
                }
                Toast.Show(this, "✅ " + result.Message);
                await LoadAsync();
            }
            catch (Exception ex) { Ask.Fail(this, "Lỗi: " + ex.Message); }
        }

        /// <summary>Bảng lịch sử đổi hạn (chỉ hiện khi đơn đã đổi hạn): cột "Hạn tính trễ" cho thấy lần nào đổi mốc
        /// (khách dời) và lần nào giữ nguyên (công ty dời).</summary>
        private void ShowDeadlineHistory(List<DeadlineChangeItem> history)
        {
            var show = history.Count > 0;
            _historyHead.Visible = _historyGrid.Visible = show;
            if (!show) return;
            DateTime? prev = null;
            var first = true;
            var rows = new List<HistoryRow>();
            foreach (var h in history)
            {
                if (first) { prev = h.OldDate; first = false; }
                var changed = prev?.Date != h.LateBasisAfter?.Date;
                rows.Add(new HistoryRow(
                    h.ChangedAt.ToLocalTime().ToString("dd/MM HH:mm"),
                    $"{(h.OldDate is { } o ? o.ToString("dd/MM") : "(chưa có)")} → {h.NewDate:dd/MM}",
                    h.RequestedBy == "Khách hàng" ? "👤 Khách hàng" : "🏢 Công ty",
                    (h.LateBasisAfter is { } b ? b.ToString("dd/MM") : "—") +
                    (prev is null ? "  (hạn đầu tiên)" : !changed ? "  (giữ nguyên)" : h.RequestedBy == "Khách hàng" ? "  (đổi theo khách)" : "  (lấy hạn sớm hơn)"),
                    h.Reason));
                prev = h.LateBasisAfter;
            }
            _historyGrid.DataSource = rows;
            _historyGrid.Height = _historyGrid.ColumnHeadersHeight + rows.Count * _historyGrid.RowTemplate.Height + 4;
            GridLayout.FitColumns(_historyGrid);
        }

        private sealed record HistoryRow(string Lần_đổi, string Hạn_cũ_mới, string Bên_yêu_cầu, string Hạn_tính_trễ, string Lý_do);
    }
}
