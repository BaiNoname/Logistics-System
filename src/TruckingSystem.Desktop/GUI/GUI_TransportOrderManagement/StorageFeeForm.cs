using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TransportOrderManagement;

namespace TruckingSystem.Desktop.GUI_TransportOrderManagement
{
    /// <summary>💰 PHÍ LƯU KHO của 1 đơn — do KHÁCH dời hạn giao trễ hơn / chậm nhận hàng. Thẻ tổng quan (tổng
    /// phí, số lần, số ngày tính phí, đơn giá) + bảng từng lần (cả lần Công ty yêu cầu để đối chiếu, phí = 0)
    /// với cột "Cách tính" + ô CHI TIẾT TỪNG NGÀY (m³ còn trong kho × đơn giá = tiền; ngày nào không tính và
    /// vì sao). "➕ Ghi nhận phí lưu kho bổ sung": khi nhân viên quên đổi hạn — kể cả đơn đã Hoàn tất / Hủy.
    /// "Sao chép diễn giải" sao chép đúng phần chi tiết để gửi khách / kế toán.
    /// Giao diện nằm ở StorageFeeForm.Designer.cs.</summary>
    public partial class StorageFeeForm : BaseForm
    {
        private readonly ApiServices? _api;
        private readonly TransportOrderDetailItem? _order;

        /// <summary>true nếu đã ghi nhận thêm phí trong lúc mở form — màn Chi tiết đơn tải lại dòng phí.</summary>
        public bool ChangedAny { get; private set; }
        private StorageFeeItem? _fee;

        /// <summary>Dùng cho trình thiết kế (Designer) của Visual Studio.</summary>
        public StorageFeeForm()
        {
            InitializeComponent();
            Anim.FadeIn(this);

            kpi.AddCard("total", "💰", "Tổng phí lưu kho (đ)", Theme.Warning);
            kpi.AddCard("times", "👤", "Lần khách dời hạn", Theme.Primary);
            kpi.AddCard("days", "📅", "Ngày tính phí", GridDecor.AccentViolet);
            kpi.AddCard("rate", "🏬", "Đơn giá kho (đ/m³/ngày)", GridDecor.AccentTeal);

            Theme.StyleGrid(grid);
            GridLayout.FitColumns(grid);
            GridDecor.HoverRows(grid);
            GridDecor.Pills(grid, "Bên_yêu_cầu", (text, _) => text == "Khách hàng" ? GridDecor.Amber : GridDecor.Slate);
            GridDecor.Pills(grid, "Phí_tính", (text, _) => text is "0 đ" or "—" ? null : GridDecor.Amber);
        }

        public StorageFeeForm(ApiServices api, TransportOrderDetailItem order) : this()
        {
            _api = api;
            _order = order;
            Text = $"Phí lưu kho — đơn {order.Code}";
            lblTitle.Text = $"💰 Phí lưu kho do khách dời hạn giao — đơn {order.Code} ({order.CustomerName})";
            btnRecord.Enabled = order.Status != "Draft" && order.CommittedDeliveryDate.HasValue;
            Load += async (_, _) => await LoadAsync();
        }

        private async Task LoadAsync()
        {
            if (_api is null || _order is null) return;
            kpi.Busy = true;
            try
            {
                var fee = await _api.TransportOrders.GetStorageFeeAsync(_order.Id);
                if (IsDisposed) return;
                _fee = fee;
                grid.DataSource = fee.Changes.Select(c => new
                {
                    Thời_điểm = c.ChangedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                    Loại = c.IsFeeOnly ? "Ghi nhận bổ sung" : "Đổi hạn giao",
                    Hạn_cũ = c.OldDate?.ToString("dd/MM/yyyy") ?? "(chưa có)",
                    Hạn_mới = c.IsFeeOnly ? $"nằm kho tới {c.NewDate:dd/MM/yyyy}" : c.NewDate.ToString("dd/MM/yyyy"),
                    Bên_yêu_cầu = c.RequestedBy,
                    Dời_thêm = c.DelayDays > 0 ? $"{c.DelayDays} ngày" : "—",
                    Ngày_tính_phí = c.ChargedDays > 0 ? $"{c.ChargedDays} ngày" : "—",
                    m3_ngày = c.ChargedM3Days > 0 ? $"{c.ChargedM3Days:0.##}" : "—",
                    Đơn_giá = c.RatePerM3PerDay > 0 ? $"{c.RatePerM3PerDay:N0} đ" : "—",
                    Phí_tính = $"{c.Fee:N0} đ",
                    Cách_tính = string.IsNullOrEmpty(c.Formula) ? "—" : c.Formula,
                    Ước_tính_lúc_đổi = c.EstimatedFee > 0 ? $"{c.EstimatedFee:N0} đ" : "—",
                    Lý_do = c.Reason,
                    Người_đổi = c.ChangedBy ?? "—",
                }).ToList();
                txtDetail.Text = BuildDetailText(fee);

                var customer = fee.Changes.Where(c => c.RequestedBy == "Khách hàng").ToList();
                kpi.SetValue("total", (int)Math.Min(int.MaxValue, Math.Round(fee.TotalFee)), fee.IsFinal ? "✅ đã chốt" : "⏳ tạm tính tới hôm nay");
                kpi.SetValue("times", customer.Count, $"{customer.Count(c => c.IsFeeOnly)} ghi nhận bổ sung · {fee.Changes.Count - customer.Count} lần do công ty (không tính)");
                kpi.SetValue("days", fee.Changes.Sum(c => c.ChargedDays), $"{fee.Changes.Sum(c => c.ChargedM3Days):0.##} m³·ngày");
                var rate = customer.LastOrDefault()?.RatePerM3PerDay ?? 0;
                kpi.SetValue("rate", (int)Math.Round(rate), rate > 0 ? "chốt lúc khách dời hạn / ghi nhận" : "kho chưa khai giá / không áp dụng");
                lblNote.Text = (fee.Changes.Count == 0 ? "Đơn chưa đổi hạn giao lần nào. " : "") + fee.Note +
                               "  Cách tính: mỗi ngày trong khoảng khách xin dời mà hàng còn nằm kho = m³ còn trong kho × đơn giá.";
                lblNote.ForeColor = Theme.TextSecondary;
                btnCopy.Enabled = customer.Count > 0;
            }
            catch (Exception ex)
            {
                if (!IsDisposed) { lblNote.Text = "Không tải được phí lưu kho: " + ex.Message; lblNote.ForeColor = Theme.Danger; }
            }
            finally
            {
                if (!IsDisposed) kpi.Busy = false;
            }
        }

        /// <summary>CHI TIẾT TỪNG NGÀY của mọi lần khách dời / ghi nhận bổ sung — để kiểm tra, đối chiếu và gửi
        /// khách / kế toán. VD:
        ///   28/10/2026   5 m³ × 500.000 đ = 2.500.000 đ
        ///   30/10/2026   — không tính: hàng không nằm trong kho</summary>
        private string BuildDetailText(StorageFeeItem fee)
        {
            var sb = new System.Text.StringBuilder();
            var orderText = _order is null ? "" : $"đơn {_order.Code} ({_order.CustomerName})";
            sb.AppendLine($"PHÍ LƯU KHO DO KHÁCH — {orderText}");
            var customer = fee.Changes.Where(c => c.RequestedBy == "Khách hàng" && c.Days.Count > 0).ToList();
            if (customer.Count == 0)
            {
                sb.AppendLine("Chưa có lần khách dời hạn / ghi nhận bổ sung nào → không có phí lưu kho.");
                return sb.ToString().Replace("\r\n", "\n").Replace("\n", "\r\n");
            }
            var no = 0;
            foreach (var c in customer)
            {
                no++;
                sb.AppendLine();
                sb.AppendLine(c.IsFeeOnly
                    ? $"{no}. Ghi nhận bổ sung {c.ChangedAt.ToLocalTime():dd/MM/yyyy HH:mm} — khách chậm nhận hàng sau hạn {c.OldDate:dd/MM/yyyy}, nằm kho tới {c.NewDate:dd/MM/yyyy}"
                    : $"{no}. Khách dời hạn {c.ChangedAt.ToLocalTime():dd/MM/yyyy HH:mm}: {(c.OldDate is { } o ? o.ToString("dd/MM/yyyy") : "?")} → {c.NewDate:dd/MM/yyyy} (dời {c.DelayDays} ngày)");
                sb.AppendLine($"   Đơn giá chốt: {c.RatePerM3PerDay:N0} đ/m³/ngày · Lý do: {c.Reason}{(c.ChangedBy is null ? "" : $" · Người ghi: {c.ChangedBy}")}");
                foreach (var d in c.Days)
                {
                    sb.AppendLine(d.NotChargedReason is null
                        ? $"   {d.Day:dd/MM/yyyy}   {d.VolumeM3,6:0.##} m³ × {c.RatePerM3PerDay:N0} đ = {d.Fee:N0} đ"
                        : $"   {d.Day:dd/MM/yyyy}   — không tính: {d.NotChargedReason}");
                }
                sb.AppendLine($"   ⇒ {c.ChargedDays} ngày tính phí, {c.ChargedM3Days:0.##} m³·ngày × {c.RatePerM3PerDay:N0} đ = {c.Fee:N0} đ");
            }
            sb.AppendLine();
            sb.AppendLine($"TỔNG PHÍ LƯU KHO: {fee.TotalFee:N0} đ{(fee.IsFinal ? " (đã chốt)" : " (tạm tính tới hôm nay)")}");
            return sb.ToString().Replace("\r\n", "\n").Replace("\n", "\r\n");
        }

        /// <summary>Sao chép đúng phần chi tiết đang hiện — dán vào email / tin nhắn gửi khách hoặc kế toán.</summary>
        private void BtnCopy_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDetail.Text)) return;
            Clipboard.SetText(txtDetail.Text);
            lblNote.Text = "📋 Đã sao chép phần chi tiết phí lưu kho — dán vào email / tin nhắn gửi khách hoặc kế toán.";
            lblNote.ForeColor = Theme.Success;
        }

        /// <summary>➕ Ghi nhận BỔ SUNG phí lưu kho do khách (nhân viên quên đổi hạn) — kể cả đơn đã Hoàn tất / Hủy.
        /// Không đổi hạn giao; ghi xong tải lại bảng + chi tiết.</summary>
        private async void BtnRecord_Click(object? sender, EventArgs e)
        {
            if (_api is null || _order is null) return;
            using var f = new RecordStorageFeeForm(_api, _order);
            if (f.ShowDialog(this) != DialogResult.OK) return;
            ChangedAny = true;
            await LoadAsync();
            if (!IsDisposed && f.ResultMessage is { } msg)
            {
                lblNote.Text = "✅ " + msg.Replace("\n", "  ");
                lblNote.ForeColor = Theme.Success;
            }
        }
    }
}
