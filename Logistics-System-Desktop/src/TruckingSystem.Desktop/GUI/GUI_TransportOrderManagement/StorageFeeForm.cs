using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TransportOrderManagement;

namespace TruckingSystem.Desktop.GUI_TransportOrderManagement
{
    /// <summary>💰 PHÍ LƯU KHO của 1 đơn (cách tính mới): miễn phí N ngày kể từ ngày nhập kho đầu tiên, sau đó mỗi ngày hàng còn
    /// nằm kho = m³ × đơn giá kho. Ngày SAU HẠN TÍNH TRỄ thì chọn bên chịu (do khách / do công ty).
    /// Bố cục (mẫu đã chốt): dòng phụ đề (kho · miễn phí · đơn giá) → 4 mốc ngày + thanh thời gian 3 màu → bảng từng ngày
    /// (ngày miễn phí gộp 1 dòng) → khung chọn bên chịu cho ngày sau hạn tính trễ → dòng tổng. Bỏ hẳn "Ghi nhận phí bổ sung".
    /// Phần thêm đều đặt bằng code; Designer chỉ còn các control cũ (thẻ KPI cũ và ô chi tiết chữ được ẩn).</summary>
    public partial class StorageFeeForm : BaseForm
    {
        private readonly ApiServices? _api;
        private readonly TransportOrderDetailItem? _order;

        /// <summary>true nếu đã chọn bên chịu trong lúc mở form — màn gọi tải lại.</summary>
        public bool ChangedAny { get; private set; }
        private StorageFeeItem? _fee;

        private readonly StorageTimelineBar _timeline = new() { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 10) };
        private readonly Label _summary = new()
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Theme.TextSecondary,
            Margin = new Padding(0, 0, 0, 8),
        };
        private readonly Label _decisionBox = new()
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            Font = new Font("Segoe UI", 10F),
            Padding = new Padding(12, 8, 12, 8),
            Margin = new Padding(0, 0, 0, 4),
            Visible = false,
        };
        private readonly FlowLayoutPanel _decisionButtons = new()
        {
            AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true, Margin = new Padding(0, 0, 0, 8), Visible = false,
        };
        private readonly Button _btnCustomer = new() { AutoSize = true };
        private readonly Button _btnCompany = new() { AutoSize = true };

        /// <summary>Dùng cho trình thiết kế (Designer) của Visual Studio.</summary>
        public StorageFeeForm()
        {
            InitializeComponent();
            TruckingSystem.Desktop.Common.DialogFit.Enable(this, root); // vừa màn hình mọi máy, bảng không bị ép mất chữ
            Anim.FadeIn(this);

            // Thẻ KPI cũ và ô chi tiết chữ (Designer) không dùng nữa: ẩn + thu hàng về AutoSize để bảng từng ngày chiếm chỗ.
            kpi.Visible = false;
            txtDetail.Visible = false;
            root.RowStyles[root.GetRow(txtDetail)] = new RowStyle(SizeType.AutoSize);
            root.RowStyles[root.GetRow(grid)] = new RowStyle(SizeType.Percent, 100F);

            // Thêm bằng code (không sửa Designer): phụ đề → thanh thời gian → [bảng] → khung chọn bên chịu.
            KpiStrip.InsertRow(root, _summary, root.GetRow(kpi));
            KpiStrip.InsertRow(root, _timeline, root.GetRow(kpi));
            KpiStrip.InsertRow(root, _decisionBox, root.GetRow(grid) + 1);
            KpiStrip.InsertRow(root, _decisionButtons, root.GetRow(_decisionBox) + 1);
            _decisionBox.UseMnemonic = false;
            void FitText()
            {
                var w = Math.Max(200, root.ClientSize.Width - root.Padding.Horizontal);
                _summary.MaximumSize = new Size(w, 0);
                _decisionBox.MaximumSize = new Size(w, 0);
            }
            root.SizeChanged += (_, _) => FitText();
            FitText();

            ButtonStyles.Primary(_btnCustomer);
            ButtonStyles.Secondary(_btnCompany);
            _btnCustomer.Margin = new Padding(0, 0, LogicalToDeviceUnits(8), 0);
            _decisionButtons.Controls.Add(_btnCustomer);
            _decisionButtons.Controls.Add(_btnCompany);
            _btnCustomer.Click += async (_, _) => await DecideAsync("Customer");
            _btnCompany.Click += async (_, _) => await DecideAsync("Company");

            Theme.StyleGrid(grid);
            GridLayout.FitColumns(grid);
            GridDecor.HoverRows(grid);
            GridDecor.Pills(grid, "Tính_phí", (text, _) => text.StartsWith("Miễn phí") ? GridDecor.Green
                : text.StartsWith("Tính phí") ? GridDecor.Blue
                : text.Contains("công ty") ? GridDecor.Slate : GridDecor.Amber);
        }

        public StorageFeeForm(ApiServices api, TransportOrderDetailItem order) : this()
        {
            _api = api;
            _order = order;
            Text = $"Phí lưu kho — đơn {order.Code}";
            lblTitle.Text = $"💰 Phí lưu kho của đơn {order.Code} ({order.CustomerName})";
            Load += async (_, _) => await LoadAsync();
        }

        private async Task LoadAsync()
        {
            if (_api is null || _order is null) return;
            try
            {
                var fee = await _api.TransportOrders.GetStorageFeeAsync(_order.Id);
                if (IsDisposed) return;
                _fee = fee;

                _summary.Text = fee.RatePerM3PerDay <= 0
                    ? $"{fee.WarehouseName} · kho không tính phí lưu kho"
                    : $"{fee.WarehouseName} · miễn phí {fee.FreeDays} ngày kể từ ngày nhập kho · {fee.RatePerM3PerDay:N0} đ/m³/ngày · đơn giá hiện tại của kho";
                _timeline.SetData(fee);
                grid.DataSource = BuildRows(fee);
                UpdateDecision(fee);

                var parts = new List<string> { $"Phí tính cho khách: {fee.TotalFee:N0} đ{(fee.IsFinal ? " (đã chốt)" : " (tạm tính)")}" };
                if (fee.CompanyBorneFee > 0) parts.Add($"Công ty chịu {fee.CompanyBorneFee:N0} đ");
                if (fee.PendingFee > 0) parts.Add($"Chờ chọn bên chịu {fee.PendingFee:N0} đ");
                lblNote.Text = string.Join("   ·   ", parts) + "\n" + fee.Note;
                lblNote.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                lblNote.ForeColor = fee.PendingFee > 0 ? Theme.Warning : fee.IsFinal ? Theme.Success : Theme.TextPrimary;
                btnCopy.Enabled = fee.Days.Count > 0;
            }
            catch (Exception ex)
            {
                if (!IsDisposed) { lblNote.Text = "Không tải được phí lưu kho: " + ex.Message; lblNote.ForeColor = Theme.Danger; }
            }
        }

        /// <summary>Bảng từng ngày: các ngày miễn phí gộp 1 dòng; mỗi ngày tính phí 1 dòng (m³, tiền); ngày sau hạn tính trễ
        /// ghi rõ đang chờ chọn / do khách / do công ty.</summary>
        private sealed record FeeRow(string Ngày, string m3_nằm_kho, string Tính_phí, string Tiền);

        private static List<FeeRow> BuildRows(StorageFeeItem fee)
        {
            var rows = new List<FeeRow>();
            var free = fee.Days.Where(d => d.Kind == "Free").ToList();
            if (free.Count > 0)
            {
                var range = free.Count == 1 ? $"{free[0].Day:dd/MM}" : $"{free[0].Day:dd/MM} → {free[^1].Day:dd/MM}";
                var events = string.Join(", ", free.Where(d => d.Event is not null).Select(d => $"{d.Day:dd/MM} {d.Event}"));
                rows.Add(new FeeRow(range + (events.Length > 0 ? $"  ({events})" : ""),
                    free.Max(d => d.VolumeM3).ToString("0.##"), $"Miễn phí (ngày 1–{free.Count})", "0"));
            }
            foreach (var d in fee.Days.Where(d => d.Kind != "Free"))
            {
                var after = d.Kind == "AfterBasis";
                rows.Add(new FeeRow(
                    $"{d.Day:dd/MM}" + (d.Event is null ? "" : $"  ({d.Event})"),
                    d.VolumeM3.ToString("0.##"),
                    !after ? "Tính phí"
                        : fee.AfterBasisDecision switch
                        {
                            "Customer" => "Sau hạn tính trễ · do khách",
                            "Company" => "Sau hạn tính trễ · do công ty",
                            _ => "Sau hạn tính trễ · chờ chọn",
                        },
                    !after || fee.AfterBasisDecision == "Customer" ? d.Fee.ToString("N0")
                        : fee.AfterBasisDecision == "Company" ? $"0 (công ty chịu {d.Fee:N0})" : $"{d.Fee:N0} ?"));
            }
            if (rows.Count == 0)
                rows.Add(new FeeRow("—", "", fee.ReceivedDate is null ? "Hàng chưa nhập kho" : "Không có ngày nào", "0"));
            return rows;
        }

        /// <summary>Khung chọn bên chịu cho các ngày sau hạn tính trễ (cùng cơ chế với Chi tiết đơn / hộp duyệt chuyến).</summary>
        private void UpdateDecision(StorageFeeItem fee)
        {
            var show = fee.AfterBasisDays > 0;
            _decisionBox.Visible = _decisionButtons.Visible = show;
            if (!show) return;
            var after = fee.Days.Where(d => d.Kind == "AfterBasis").ToList();
            var range = after.Count == 1 ? $"{after[0].Day:dd/MM}" : $"{after[0].Day:dd/MM}–{after[^1].Day:dd/MM}";
            var amount = after.Sum(d => d.Fee);
            var head = $"Hàng nằm kho {fee.AfterBasisDays} ngày SAU hạn tính trễ {fee.LateBasisDate:dd/MM} ({range}).";
            switch (fee.AfterBasisDecision)
            {
                case "Customer":
                    _decisionBox.BackColor = Color.FromArgb(239, 246, 255);
                    _decisionBox.ForeColor = Color.FromArgb(30, 64, 175);
                    _decisionBox.Text = $"🧾 {head} Đã chọn DO KHÁCH → tính {amount:N0} đ." +
                                        (string.IsNullOrWhiteSpace(fee.AfterBasisReason) ? "" : $" Lý do: {fee.AfterBasisReason}");
                    _btnCustomer.Visible = false;
                    _btnCompany.Visible = true;
                    _btnCompany.Text = "↩️ Đổi thành do công ty (0 đ)";
                    break;
                case "Company":
                    _decisionBox.BackColor = Color.FromArgb(240, 253, 244);
                    _decisionBox.ForeColor = Color.FromArgb(22, 101, 52);
                    _decisionBox.Text = $"🏢 {head} Đã chọn DO CÔNG TY → khách 0 đ (công ty chịu {amount:N0} đ).";
                    _btnCompany.Visible = false;
                    _btnCustomer.Visible = true;
                    _btnCustomer.Text = $"↩️ Đổi thành do khách → tính {amount:N0} đ";
                    break;
                default:
                    _decisionBox.BackColor = Color.FromArgb(255, 251, 235);
                    _decisionBox.ForeColor = Color.FromArgb(120, 53, 15);
                    _decisionBox.Text = fee.IsFinal
                        ? $"⚠️ {head} Chưa chọn bên chịu — đơn đã đóng nên đang mặc định công ty chịu (khách 0 đ). Có thể chọn lại:"
                        : $"⚠️ {head} Chọn bên chịu phí những ngày này (chưa chọn thì chưa chốt được phí / hóa đơn):";
                    _btnCustomer.Visible = _btnCompany.Visible = true;
                    _btnCustomer.Text = $"🧾 Do khách (khách hẹn nhận trễ…) → tính {amount:N0} đ";
                    _btnCompany.Text = "🏢 Do công ty (thiếu xe, giao trễ…) → 0 đ";
                    break;
            }
        }

        private async Task DecideAsync(string party)
        {
            if (_api is null || _order is null) return;
            var reason = StorageDecision.AskReason(this, party);
            if (reason is null) return;
            try
            {
                var result = await _api.TransportOrders.SetStorageResponsibilityAsync(_order.Id, party, party == "Customer" ? reason : null);
                if (!result.Success)
                {
                    // VD kho chưa khai báo phí lưu kho -> nút mở Sửa kho của đúng kho, xong chọn lại.
                    await TruckingSystem.Desktop.GUI_Shared.QuickFix.ShowAsync(this, _api, "Chưa ghi nhận được bên chịu phí", result.Message, result.Fixes,
                        "🧾 Chọn lại bên chịu phí", () => DecideAsync(party));
                    return;
                }
                ChangedAny = true;
                Toast.Show(this, "✅ " + result.Message);
                await LoadAsync();
            }
            catch (Exception ex) { Ask.Fail(this, "Lỗi: " + ex.Message); }
        }

        /// <summary>Diễn giải chữ để gửi khách / kế toán (cùng số với bảng).</summary>
        private string BuildDetailText(StorageFeeItem fee)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"PHÍ LƯU KHO — đơn {_order?.Code} ({_order?.CustomerName})");
            sb.AppendLine($"{fee.WarehouseName}: miễn phí {fee.FreeDays} ngày kể từ ngày nhập kho, sau đó {fee.RatePerM3PerDay:N0} đ/m³/ngày.");
            sb.AppendLine($"Nhập kho: {fee.ReceivedDate:dd/MM/yyyy} · Hết miễn phí: {fee.FreeUntil:dd/MM/yyyy} · Hạn tính trễ: " +
                          (fee.LateBasisDate is { } b ? b.ToString("dd/MM/yyyy") : "—") +
                          " · Xuất hết: " + (fee.LastIssueDate is { } li ? li.ToString("dd/MM/yyyy") : fee.StillInStock ? "còn hàng trong kho" : "—"));
            sb.AppendLine();
            foreach (var d in fee.Days.Where(d => d.Kind != "Free"))
            {
                var after = d.Kind == "AfterBasis";
                var tail = !after ? "" : fee.AfterBasisDecision switch
                {
                    "Customer" => "  (sau hạn tính trễ, do khách)",
                    "Company" => "  (sau hạn tính trễ, công ty chịu — không tính)",
                    _ => "  (sau hạn tính trễ, chưa chọn bên chịu — chưa tính)",
                };
                sb.AppendLine($"  {d.Day:dd/MM}   {d.VolumeM3:0.##} m³ × {fee.RatePerM3PerDay:N0} đ = {d.Fee:N0} đ{tail}");
            }
            sb.AppendLine();
            sb.AppendLine($"TỔNG PHÍ LƯU KHO TÍNH CHO KHÁCH{(fee.IsFinal ? " (ĐÃ CHỐT)" : " (TẠM TÍNH)")}: {fee.TotalFee:N0} đ");
            return sb.ToString().Replace("\r\n", "\n").Replace("\n", "\r\n");
        }

        /// <summary>Sao chép diễn giải — dán vào email / tin nhắn gửi khách hoặc kế toán.</summary>
        private void BtnCopy_Click(object? sender, EventArgs e)
        {
            if (_fee is null) return;
            try { Clipboard.SetText(BuildDetailText(_fee)); }
            catch { return; }
            Toast.Show(this, "📋 Đã sao chép diễn giải phí lưu kho — dán vào email / tin nhắn gửi khách hoặc kế toán.", ToastKind.Info);
        }
    }
}
