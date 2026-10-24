using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TransportOrderManagement;

namespace TruckingSystem.Desktop.GUI_TransportOrderManagement
{
    /// <summary>💰 PHÍ LƯU KHO của 1 đơn — do KHÁCH dời hạn giao trễ hơn. Thẻ tổng quan (tổng phí, số lần
    /// khách dời, số ngày tính phí, đơn giá) + bảng từng lần đổi hạn (cả lần Công ty yêu cầu để đối chiếu,
    /// phí = 0). Nút "Sao chép diễn giải" tạo đoạn văn gửi khách / kế toán.
    /// Giao diện nằm ở StorageFeeForm.Designer.cs (form chỉ xem, không có ô nhập).</summary>
    public partial class StorageFeeForm : BaseForm
    {
        private readonly ApiServices? _api;
        private readonly TransportOrderDetailItem? _order;
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
                    Hạn_cũ = c.OldDate?.ToString("dd/MM/yyyy") ?? "(chưa có)",
                    Hạn_mới = c.NewDate.ToString("dd/MM/yyyy"),
                    Bên_yêu_cầu = c.RequestedBy,
                    Dời_thêm = c.DelayDays > 0 ? $"{c.DelayDays} ngày" : "—",
                    Ngày_tính_phí = c.ChargedDays > 0 ? $"{c.ChargedDays} ngày" : "—",
                    m3_ngày = c.ChargedM3Days > 0 ? $"{c.ChargedM3Days:0.##}" : "—",
                    Đơn_giá = c.RatePerM3PerDay > 0 ? $"{c.RatePerM3PerDay:N0} đ" : "—",
                    Ước_tính_lúc_đổi = c.EstimatedFee > 0 ? $"{c.EstimatedFee:N0} đ" : "—",
                    Phí_tính = $"{c.Fee:N0} đ",
                    Lý_do = c.Reason,
                    Người_đổi = c.ChangedBy ?? "—",
                }).ToList();

                var customer = fee.Changes.Where(c => c.RequestedBy == "Khách hàng").ToList();
                kpi.SetValue("total", (int)Math.Min(int.MaxValue, Math.Round(fee.TotalFee)), fee.IsFinal ? "✅ đã chốt" : "⏳ tạm tính tới hôm nay");
                kpi.SetValue("times", customer.Count, $"{fee.Changes.Count - customer.Count} lần do công ty (không tính)");
                kpi.SetValue("days", fee.Changes.Sum(c => c.ChargedDays), $"{fee.Changes.Sum(c => c.ChargedM3Days):0.##} m³·ngày");
                var rate = customer.LastOrDefault()?.RatePerM3PerDay ?? 0;
                kpi.SetValue("rate", (int)Math.Round(rate), rate > 0 ? "chốt lúc khách dời hạn" : "kho chưa khai giá / không áp dụng");
                lblNote.Text = (fee.Changes.Count == 0 ? "Đơn chưa đổi hạn giao lần nào. " : "") + fee.Note +
                               "  Cách tính: mỗi ngày trong khoảng khách xin dời mà hàng còn nằm kho = m³ còn trong kho × đơn giá.";
                lblNote.ForeColor = Theme.TextSecondary;
                btnCopy.Enabled = fee.TotalFee > 0;
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

        /// <summary>Đoạn diễn giải gọn để dán vào email / tin nhắn gửi khách hoặc kế toán.</summary>
        private void BtnCopy_Click(object? sender, EventArgs e)
        {
            if (_fee is null || _order is null) return;
            var lines = _fee.Changes.Where(c => c.RequestedBy == "Khách hàng" && c.DelayDays > 0).Select(c =>
                $"- Dời hạn {(c.OldDate is { } o ? o.ToString("dd/MM/yyyy") : "?")} → {c.NewDate:dd/MM/yyyy}: " +
                $"{c.ChargedDays} ngày lưu kho, {c.ChargedM3Days:0.##} m³·ngày × {c.RatePerM3PerDay:N0} đ = {c.Fee:N0} đ");
            var text = $"Phí lưu kho đơn {_order.Code} ({_order.CustomerName}) do khách yêu cầu dời hạn giao:\n" +
                       string.Join("\n", lines) +
                       $"\nTổng: {_fee.TotalFee:N0} đ{(_fee.IsFinal ? "" : " (tạm tính tới hôm nay)")}.";
            Clipboard.SetText(text);
            MessageBox.Show(this, "Đã sao chép diễn giải phí lưu kho:\n\n" + text, "Đã sao chép", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
