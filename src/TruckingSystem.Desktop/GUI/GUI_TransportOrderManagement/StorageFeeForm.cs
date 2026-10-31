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

            // 4 thẻ theo câu hỏi của người dùng: ĐÃ phát sinh bao nhiêu · nếu hàng còn nằm kho tới hết hạn mới thì
            // DỰ KIẾN bao nhiêu · đã tính được mấy ngày / khách xin dời mấy ngày · đơn giá.
            kpi.AddCard("total", "💰", "Phí đã phát sinh (đ)", Theme.Warning);
            kpi.AddCard("projected", "📈", "Dự kiến nếu hàng nằm kho hết hạn mới (đ)", Theme.Primary);
            kpi.AddCard("days", "📅", "Ngày đã tính phí", GridDecor.AccentViolet);
            kpi.AddCard("rate", "🏬", "Đơn giá kho (đ/m³/ngày)", GridDecor.AccentTeal);

            // Câu tóm tắt NGAY DƯỚI các thẻ: giải thích bằng lời vì sao ra con số này (thêm bằng code, không sửa Designer).
            KpiStrip.InsertRow(root, _summary, root.GetRow(grid));
            // Label tự xuống dòng theo bề ngang form (AutoSize + MaximumSize).
            void FitSummary() => _summary.MaximumSize = new Size(Math.Max(200, root.ClientSize.Width - root.Padding.Horizontal), 0);
            root.SizeChanged += (_, _) => FitSummary();
            FitSummary();

            Theme.StyleGrid(grid);
            GridLayout.FitColumns(grid);
            GridDecor.HoverRows(grid);
            GridDecor.Pills(grid, "Bên_yêu_cầu", (text, _) => text == "Khách hàng" ? GridDecor.Amber : GridDecor.Slate);
            GridDecor.Pills(grid, "Tình_trạng", (text, _) => text.StartsWith("✅") ? GridDecor.Green
                : text.StartsWith("⏳") ? GridDecor.Blue : GridDecor.Slate);
        }

        private readonly Label _summary = new()
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            Font = new Font("Segoe UI", 10.5F),
            ForeColor = Theme.TextPrimary,
            BackColor = Color.FromArgb(239, 246, 255),
            Padding = new Padding(12, 10, 12, 10),
            Margin = new Padding(0, 4, 0, 10),
        };

        public StorageFeeForm(ApiServices api, TransportOrderDetailItem order) : this()
        {
            _api = api;
            _order = order;
            Text = $"Phí lưu kho — đơn {order.Code}";
            lblTitle.Text = $"💰 Phí lưu kho do khách dời hạn giao — đơn {order.Code} ({order.CustomerName})";
            btnRecord.Enabled = order.Status != "Draft" && order.CommittedDeliveryDate.HasValue;
            Load += async (_, _) => await LoadAsync();
        }

        private static bool IsFuture(StorageFeeDayItem d) => d.NotChargedReason?.StartsWith("chưa tới ngày") == true;

        /// <summary>Phí 1 ngày ước tính lúc đổi hạn (m³ lúc đổi × đơn giá) — dùng cho các ngày CHƯA TỚI.</summary>
        private static decimal PerDayEstimate(StorageFeeLineItem c)
            => c.DelayDays > 0 && c.EstimatedFee > 0 ? Math.Round(c.EstimatedFee / c.DelayDays, 0) : 0;

        /// <summary>Dự kiến của 1 lần dời = đã tính + (số ngày chưa tới × phí 1 ngày ước tính).</summary>
        private static decimal Projected(StorageFeeLineItem c) => c.Fee + c.Days.Count(IsFuture) * PerDayEstimate(c);

        private static string Status(StorageFeeLineItem c)
        {
            if (c.RequestedBy != "Khách hàng") return "Không tính (công ty yêu cầu)";
            var future = c.Days.Count(IsFuture);
            var total = c.Days.Count;
            if (future == 0) return "✅ Đã chốt";
            if (future == total) return $"⏳ Chưa tới ngày ({total} ngày phía trước)";
            return $"⏳ Đang tính ({total - future}/{total} ngày)";
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
                // Bảng GỌN: mỗi lần dời 1 dòng với Phí đã tính / Phí dự kiến / Tình trạng — chi tiết từng ngày ở ô dưới.
                grid.DataSource = fee.Changes.Select(c => new
                {
                    Thời_điểm = c.ChangedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                    Bên_yêu_cầu = c.RequestedBy,
                    Dời_hạn = c.IsFeeOnly
                        ? $"Ghi nhận bổ sung: nằm kho sau {c.OldDate:dd/MM} tới {c.NewDate:dd/MM}"
                        : $"{(c.OldDate is { } o ? o.ToString("dd/MM") : "(chưa có)")} → {c.NewDate:dd/MM}" + (c.DelayDays > 0 ? $" ({c.DelayDays} ngày)" : ""),
                    Phí_đã_tính = $"{c.Fee:N0} đ",
                    Phí_dự_kiến = c.RequestedBy == "Khách hàng" ? $"{Projected(c):N0} đ" : "—",
                    Tình_trạng = Status(c),
                    Lý_do = c.Reason,
                    Người_ghi = c.ChangedBy ?? "—",
                }).ToList();
                txtDetail.Text = BuildDetailText(fee);

                var customer = fee.Changes.Where(c => c.RequestedBy == "Khách hàng").ToList();
                var projected = customer.Sum(Projected);
                var chargedDays = customer.Sum(c => c.ChargedDays);
                var allDays = customer.Sum(c => c.Days.Count);
                kpi.SetValue("total", (int)Math.Min(int.MaxValue, Math.Round(fee.TotalFee)), fee.IsFinal ? "✅ đã chốt" : "⏳ tính tới hôm nay");
                kpi.SetValue("projected", (int)Math.Min(int.MaxValue, Math.Round(projected)),
                    fee.IsFinal ? "= phí đã chốt" : "nếu hàng còn nằm kho tới hết các ngày khách xin dời");
                kpi.SetValue("days", chargedDays, $"trên {allDays} ngày khách xin dời · {customer.Sum(c => c.ChargedM3Days):0.##} m³·ngày");
                var rate = customer.LastOrDefault()?.RatePerM3PerDay ?? 0;
                kpi.SetValue("rate", (int)Math.Round(rate), rate > 0 ? "chốt lúc khách dời hạn" : "kho chưa khai giá / không áp dụng");

                _summary.Text = SummaryText(fee, customer, projected);
                _summary.BackColor = fee.IsFinal ? Color.FromArgb(240, 253, 244) : Color.FromArgb(239, 246, 255);
                lblNote.Text = "Cách tính: mỗi ngày trong khoảng khách xin dời mà hàng có nằm kho (kể cả ngày xuất kho) = số m³ nằm kho trong ngày × đơn giá. " +
                               "Lần dời do công ty yêu cầu không tính phí cho khách.";
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

        /// <summary>1–2 câu bằng lời thường: phí hiện tại vì sao như vậy, và sẽ thành bao nhiêu.</summary>
        private static string SummaryText(StorageFeeItem fee, List<StorageFeeLineItem> customer, decimal projected)
        {
            if (fee.Changes.Count == 0) return "Đơn chưa đổi hạn giao lần nào → không có phí lưu kho.";
            if (customer.Count == 0) return "Các lần đổi hạn đều do công ty yêu cầu → không tính phí lưu kho cho khách.";
            if (fee.IsFinal)
                return $"✅ Đã chốt: khách phải trả {fee.TotalFee:N0} đ phí lưu kho — tính theo {customer.Sum(c => c.ChargedDays)} ngày hàng thực sự nằm kho trong các ngày khách xin dời.";

            var futureDays = customer.SelectMany(c => c.Days.Where(IsFuture)).Select(d => d.Day).OrderBy(d => d).ToList();
            var perDay = customer.Where(c => c.Days.Any(IsFuture)).Select(PerDayEstimate).LastOrDefault();
            var text = fee.TotalFee == 0
                ? "⏳ Hiện tại 0 đ vì các ngày khách xin dời chưa tới."
                : $"⏳ Đến hôm nay đã phát sinh {fee.TotalFee:N0} đ.";
            if (futureDays.Count > 0)
                text += $" Còn {futureDays.Count} ngày ({futureDays[0]:dd/MM}" + (futureDays.Count > 1 ? $"–{futureDays[^1]:dd/MM}" : "") + ")" +
                        (perDay > 0 ? $": mỗi ngày hàng còn nằm kho cộng thêm khoảng {perDay:N0} đ" : "") +
                        $" → nếu hàng nằm kho tới hết hạn mới, tổng khoảng {projected:N0} đ. Hàng xuất kho sớm thì thôi tính từ hôm sau.";
            return text;
        }

        /// <summary>CHI TIẾT TỪNG NGÀY — gom các ngày liên tiếp cùng tình trạng cho dễ đọc. VD:
        ///   27/10 – 28/10   5 m³/ngày × 10.000 đ = 100.000 đ
        ///   01/11 – 05/11   chưa tới ngày — mỗi ngày còn nằm kho sẽ cộng ~190.000 đ</summary>
        private string BuildDetailText(StorageFeeItem fee)
        {
            var sb = new System.Text.StringBuilder();
            var orderText = _order is null ? "" : $"đơn {_order.Code} ({_order.CustomerName})";
            sb.AppendLine($"PHÍ LƯU KHO DO KHÁCH DỜI HẠN — {orderText}");
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
                    ? $"{no}. Ghi nhận bổ sung ({c.ChangedAt.ToLocalTime():dd/MM/yyyy HH:mm}): khách chậm nhận hàng sau hạn {c.OldDate:dd/MM/yyyy}, hàng nằm kho tới {c.NewDate:dd/MM/yyyy}"
                    : $"{no}. Khách dời hạn ({c.ChangedAt.ToLocalTime():dd/MM/yyyy HH:mm}): {(c.OldDate is { } o ? o.ToString("dd/MM/yyyy") : "?")} → {c.NewDate:dd/MM/yyyy}, dời {c.DelayDays} ngày");
                sb.AppendLine($"   Đơn giá: {c.RatePerM3PerDay:N0} đ/m³/ngày · Lý do: {c.Reason}{(c.ChangedBy is null ? "" : $" · Người ghi: {c.ChangedBy}")}");
                var perDay = PerDayEstimate(c);
                // Gom ngày liên tiếp cùng tình trạng (cùng m³ nếu có tính / cùng lý do nếu không tính).
                var groups = new List<(DateTime From, DateTime To, StorageFeeDayItem Sample, int Count, decimal Fee)>();
                foreach (var d in c.Days)
                {
                    if (groups.Count > 0 && groups[^1].To.AddDays(1) == d.Day
                        && groups[^1].Sample.NotChargedReason == d.NotChargedReason && groups[^1].Sample.VolumeM3 == d.VolumeM3)
                        groups[^1] = (groups[^1].From, d.Day, groups[^1].Sample, groups[^1].Count + 1, groups[^1].Fee + d.Fee);
                    else groups.Add((d.Day, d.Day, d, 1, d.Fee));
                }
                foreach (var g in groups)
                {
                    var range = g.Count == 1 ? $"{g.From:dd/MM}        " : $"{g.From:dd/MM} – {g.To:dd/MM}";
                    sb.AppendLine(g.Sample.NotChargedReason is null
                        ? $"   ✔ {range}   {g.Count} ngày × {g.Sample.VolumeM3:0.##} m³ × {c.RatePerM3PerDay:N0} đ = {g.Fee:N0} đ"
                        : IsFuture(g.Sample)
                            ? $"   ⏳ {range}   chưa tới ngày" + (perDay > 0 ? $" — mỗi ngày hàng còn nằm kho sẽ cộng khoảng {perDay:N0} đ" : "")
                            : $"   ✖ {range}   không tính: {g.Sample.NotChargedReason}");
                }
                sb.AppendLine($"   → Đã tính: {c.ChargedDays} ngày = {c.Fee:N0} đ" +
                              (c.Days.Any(IsFuture) ? $"   ·   Dự kiến nếu nằm kho tới hết hạn mới: {Projected(c):N0} đ" : ""));
            }
            sb.AppendLine();
            sb.AppendLine(fee.IsFinal
                ? $"TỔNG PHÍ LƯU KHO (ĐÃ CHỐT): {fee.TotalFee:N0} đ"
                : $"TỔNG ĐẾN HÔM NAY: {fee.TotalFee:N0} đ   ·   DỰ KIẾN TỐI ĐA: {customer.Sum(Projected):N0} đ");
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
