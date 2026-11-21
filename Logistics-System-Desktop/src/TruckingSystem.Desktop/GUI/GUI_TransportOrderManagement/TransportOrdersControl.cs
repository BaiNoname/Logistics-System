using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TransportOrderManagement;

namespace TruckingSystem.Desktop.GUI_TransportOrderManagement
{
    public partial class TransportOrdersControl : BaseUserControl
    {
        private ApiServices? _api;
        private List<TransportOrderListItem> _orders = new();
        // Đang Chia xe (VietMap + thuật toán có thể mất vài giây) -> khóa nút, chống bấm 2 lần.
        private bool _allocating;
        private ScreenKit.GridOverlay _overlay = null!; // bảng trống / đang tải (Common/ScreenKit.cs)


        public TransportOrdersControl()
        {
            InitializeComponent();
            // Bảng trống có hướng dẫn · phím tắt · nút "?" (mẫu UX đợt 6).
            _overlay = ScreenKit.Overlay(grid, () => _orders.Count, "Chưa có đơn vận chuyển nào",
                "Tạo đơn đầu tiên cho khách — sau đó xác nhận và chia xe.", ("➕ Tạo đơn mới", () => btnCreate.PerformClick()),
                () => { _pendingOnly = false; filterBar.ClearFilters(); BindGrid(); });
            ScreenKit.Shortcuts(this, grid, null, () => btnCreate.PerformClick(), () => _ = OpenDetailAsync());
            ScreenKit.Help(this, "orders", "Quản lý đơn của khách từ lúc tạo đến khi giao xong.",
                new[] { "Tạo đơn (➕ Tạo đơn mới)", "Kiểm tra thông tin rồi ✅ Xác nhận đơn", "🚚 Chia xe — hệ thống tự xếp xe + tài xế", "Theo dõi tiến độ; nhấp đúp 1 đơn để xem chi tiết" },
                "Ctrl+N tạo đơn · Enter mở chi tiết · F5 làm mới · Ctrl+F tìm nhanh",
                new[]
                {
                    ColorLegend.Header("Dải màu bên trái mỗi đơn"),
                    ColorLegend.Bar(ColorLegend.Gray, "Nháp, hoặc đã xác nhận nhưng chưa chạy"),
                    ColorLegend.Bar(ColorLegend.Amber, "Đang vận chuyển"),
                    ColorLegend.Bar(ColorLegend.Green, "Hoàn tất"),
                    ColorLegend.Bar(ColorLegend.Red, "Đã hủy"),
                    ColorLegend.Header("Cột Trạng thái"),
                    ColorLegend.Pill("Nháp", GridDecor.Slate, "Chưa xác nhận, còn sửa được"),
                    ColorLegend.Pill("Đã xác nhận", GridDecor.Teal, "Đã chốt với khách, chờ chia xe / chờ xe chạy"),
                    ColorLegend.Pill("Đang vận chuyển · 40%", GridDecor.Blue, "Đang chạy — số % = phần hàng đã giao tới khách"),
                    ColorLegend.Pill("Hoàn tất", GridDecor.Green, "Đã giao xong"),
                    ColorLegend.Pill("Đã hủy", GridDecor.Red, "Đơn bị hủy"),
                    ColorLegend.Header("Cột Việc cần làm"),
                    ColorLegend.Text("✅ Xác nhận đơn", Theme.Primary, "Việc bình thường"),
                    ColorLegend.Text("🚚 Chia xe", Theme.Warning, "Cần làm gấp / đang vướng"),
                    ColorLegend.Header("Cột Hạn giao"),
                    ColorLegend.Pill("01/11 · trễ 1 ngày", GridDecor.Red, "Quá hạn giao, hoặc hàng nằm kho quá hạn"),
                    ColorLegend.Pill("13/11 · mai", GridDecor.Amber, "Tới hạn trong 3 ngày"),
                    ColorLegend.Note("Không tô màu = còn xa hạn, hoặc đơn đã xong / đã hủy."),
                });
            new ToolTip().SetToolTip(btnCreate, "Tạo đơn mới (Ctrl+N)");
            new ToolTip().SetToolTip(btnRefresh, "Tải lại danh sách (F5)");
            Theme.StyleGrid(grid);
            GridLayout.FitColumns(grid); // cột vừa nội dung, nhiều cột thì cuộn ngang thay vì cắt chữ

            grid.DataBindingComplete += (_, _) =>
            {
                if (grid.Columns["Id"] != null) grid.Columns["Id"]!.Visible = false;
            };
            grid.CellDoubleClick += async (_, _) => await OpenDetailAsync();
            // "Chia xe" chỉ bấm được với đơn ĐÃ XÁC NHẬN (Nháp phải xác nhận trước;
            // đơn đang chạy / hoàn tất / đã hủy không chia xe được nữa).
            grid.SelectionChanged += (_, _) => UpdateActionButtons();
            grid.DataBindingComplete += (_, _) => UpdateActionButtons();

            // Bộ lọc (Common/FilterBar): trạng thái, hạn giao, khách hàng, kho — lọc ngay trên máy.
            filterBar.AddChoice("status", "Trạng thái", new[] { "Nháp", "Đã xác nhận", "Đang vận chuyển", "Hoàn tất", "Đã hủy" });
            filterBar.AddChoice("deadline", "Hạn giao", new[] { DeadlineOverdue, DeadlineSoon, DeadlineLater, DeadlineNone });
            filterBar.AddChoice("customer", "Khách hàng", Array.Empty<string>());
            filterBar.AddChoice("warehouse", "Kho", Array.Empty<string>());
            filterBar.FilterChanged += (_, _) => BindGrid();
            FilterMemory.Attach(filterBar, "orders"); // nhớ bộ lọc khi chuyển màn rồi quay lại
            SetupModernLook();
            SetupActions(); // thanh nút gọn + menu chuột phải + thẻ "Đơn đang chọn" (TransportOrdersControl.Actions.cs)
            // Dải nhắc việc (giải thích số đỏ trên menu): đơn nháp chờ xác nhận + đơn chờ chia xe, bật / tắt "chỉ xem các việc này".
            KpiStrip.InsertRow(root, _pendingBanner, root.GetRow(_card));
            _pendingBanner.FilterToggled += (_, on) => { _pendingOnly = on; BindGrid(); };

            // Ẩn / khóa sẵn các nút theo "chưa chọn dòng" TRƯỚC khi màn hiện — không còn cảnh hiện cả loạt nút rồi tải xong mới ẩn đi.
            // (không gọi UpdateSelectionUi: thẻ sẽ báo nhầm "Chưa có đơn nào" khi chưa tải xong)
            if (_btnEdit != null) _btnEdit.Visible = false;
            if (_btnConfirm != null) _btnConfirm.Visible = false;
            btnAllocate.Visible = false;
            foreach (var b in new[] { _btnDetail, _btnDeadline, _btnTools })
                if (b != null) b.Enabled = false;
            Load += async (_, _) => await LoadAsync();
        }

        /// <summary>Nhãn màu trạng thái / hạn giao + thanh tiến độ. (Bỏ hàng 6 thẻ số liệu theo góp ý GVHD — số cần
        /// chú ý gom vào 1 dòng ngắn SummaryText; lọc nhanh dùng bộ lọc Trạng thái / Hạn giao trên bảng.)</summary>
        private void SetupModernLook()
        {
            // Cửa sổ thấp: giữ bảng tối thiểu ~8 dòng, thiếu chỗ thì cả màn có thanh cuộn kéo xuống.
            ScrollFit.Enable(this, root, grid);

            GridDecor.HoverRows(grid);
            // Dải màu mép trái theo TRẠNG THÁI ĐƠN — cùng quy ước 4 màu với màn Chuyến:
            // xám nháp / chưa chạy · vàng đang vận chuyển · xanh lá hoàn tất · đỏ đã hủy.
            grid.RowPostPaint += (_, e) =>
            {
                if (OrderAt(e.RowIndex) is not { } o) return;
                using var b = new SolidBrush(StatusColor(o.Status));
                e.Graphics.FillRectangle(b, LogicalToDeviceUnits(3), e.RowBounds.Y, LogicalToDeviceUnits(4), e.RowBounds.Height);
            };
            // Cột "Việc cần làm": xanh đậm = việc bình thường, cam = cần làm gấp / đang vướng, xám = không còn việc.
            grid.CellFormatting += (_, e) =>
            {
                if (e.RowIndex < 0 || grid.Columns[e.ColumnIndex].Name != "Việc_cần_làm" || e.CellStyle is null) return;
                var text = e.Value as string ?? "";
                e.CellStyle.ForeColor = text == "—" ? Theme.TextSecondary
                    : text.StartsWith("🚚") || text.StartsWith("⏰") || text.StartsWith("📦") ? Theme.Warning : Theme.Primary;
                e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
                if (text != "—") e.CellStyle.Font = Theme.FontBold;
            };
            // Màu nhãn theo trạng thái GỐC của đơn (chữ có thể kèm "· 40%" khi đang giao dở).
            GridDecor.Pills(grid, "Trạng_thái", (text, row) =>
            {
                var plain = OrderAt(row) is { } so ? TranslateStatus(so.Status) : text;
                return plain == "Đã hủy" ? GridDecor.Red : GridDecor.StatusTone(plain); // hủy = ĐỎ (quy ước 4 màu)
            });
            GridDecor.Pills(grid, "Hạn_giao", (_, row) => OrderAt(row) is { } o ? o.OverdueStorageDays > 0 ? GridDecor.Red : DeadlineGroup(o) switch
            {
                DeadlineOverdue => GridDecor.Red,
                DeadlineSoon => GridDecor.Amber,
                _ => null,
            } : null);
            // Cột "Tiến độ %" đã bỏ (trùng ý với Trạng thái): đơn đang giao DỞ ghi % ngay trong nhãn trạng thái
            // ("Đang vận chuyển · 40%"); thẻ đơn phía trên vẫn có dòng "Đã giao x%".
        }

        private TransportOrderListItem? OrderAt(int row)
            => row >= 0 && row < grid.Rows.Count && grid.Rows[row].Cells["Id"].Value is int id
                ? _orders.FirstOrDefault(o => o.Id == id) : null;

        /// <summary>1 dòng tóm tắt thay cho 6 thẻ số liệu: tổng số đơn + CHỈ những việc cần chú ý (có số > 0).</summary>
        private string SummaryText()
        {
            var parts = new List<string> { $"Tổng {_orders.Count} đơn" };
            void Add(int n, string text) { if (n > 0) parts.Add($"{text}: {n}"); }
            Add(_orders.Count(o => o.Status == "Draft"), "📝 nháp chờ xác nhận");
            Add(_orders.Count(o => o.Status == "Confirmed" && o.ActiveTripCount == 0), "🧭 chờ chia xe");
            Add(_orders.Count(o => o.Status == "InProgress"), "🚚 đang vận chuyển");
            Add(_orders.Count(o => DeadlineGroup(o) == DeadlineOverdue), "⏰ quá hạn giao");
            Add(_orders.Count(o => DeadlineGroup(o) == DeadlineSoon && o.Status is not ("Done" or "Cancelled")), "🟡 tới hạn trong 3 ngày");
            return string.Join("   ·   ", parts) + "   —   chuột phải vào 1 đơn để xem mọi thao tác";
        }

        public TransportOrdersControl(ApiServices api) : this()
        {
            _api = api;
        }

        private void BtnRefresh_Click(object? sender, EventArgs e) => _ = LoadAsync();

        private async void BtnCreate_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;
            using var f = new CreateOrderForm(_api);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
        }

        private async void BtnAllocate_Click(object? sender, EventArgs e) => await AllocateSelectedAsync();

        private async Task LoadAsync()
        {
            if (_api is null) return;
            lblStatus.Text = "Đang tải...";
            lblStatus.ForeColor = Theme.TextSecondary;
            _overlay.Loading = true;
            try
            {
                _orders = await _api.TransportOrders.GetTransportOrdersAsync();
                filterBar.SetChoiceOptions("customer", _orders.Select(o => o.CustomerName));
                filterBar.SetChoiceOptions("warehouse", _orders.Select(o => o.WarehouseName ?? ""));
                BindGrid();
                lblStatus.Text = SummaryText();
                lblStatus.ForeColor = _orders.Any(o => DeadlineGroup(o) == DeadlineOverdue) ? Theme.Danger : Theme.TextSecondary;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
            finally { _overlay.Loading = false; }
        }

        private const string DeadlineOverdue = "Quá hạn (chưa xong)";
        private const string DeadlineSoon = "Trong 3 ngày tới";
        private const string DeadlineLater = "Còn hơn 3 ngày";
        private const string DeadlineNone = "Chưa có hạn";

        /// <summary>Nhóm hạn giao của đơn (đơn đã xong / hủy thì không còn "quá hạn"). "Quá hạn" xét theo HẠN TÍNH TRỄ:
        /// công ty đã dời hạn thì qua hạn cũ vẫn là quá hạn (không bị che thành "còn hạn").</summary>
        private static string DeadlineGroup(TransportOrderListItem o)
        {
            if (o.CommittedDeliveryDate is not DateTime due) return DeadlineNone;
            var lateBy = (o.LateBasisDate ?? due).Date;
            if (lateBy < DateTime.Today) return o.Status is "Done" or "Cancelled" ? DeadlineLater : DeadlineOverdue;
            var days = (due.Date - DateTime.Today).Days;
            return days <= 3 ? DeadlineSoon : DeadlineLater;
        }

        /// <summary>Chữ cột Hạn giao: công ty đã dời hạn thì ghi "16/11 · trễ theo 12/11"; còn lại như cũ ("01/11 · còn 2 ngày").</summary>
        private static string DeadlineText(TransportOrderListItem o)
        {
            var done = o.Status is "Done" or "Cancelled";
            if (!done && o.HasLateBasis && o.CommittedDeliveryDate is DateTime due && o.LateBasisDate is DateTime basis)
                return $"{due:dd/MM} · trễ theo {basis:dd/MM}";
            return ScreenKit.Deadline(o.CommittedDeliveryDate, done);
        }

        /// <summary>Đổ lưới theo bộ lọc (không gọi lại API).</summary>
        /// <summary>Mở từ thông báo (nhấp đúp): bỏ các bộ lọc khác, lọc sẵn theo mã đơn.</summary>
        public void FocusOn(string search)
        {
            filterBar.ShowOnlySearch(search);
        }

        private readonly PendingBanner _pendingBanner = new();
        private bool _pendingOnly;

        private static bool IsPending(TransportOrderListItem o)
            => o.Status == "Draft" || (o.Status == "Confirmed" && o.ActiveTripCount == 0) || o.OverdueStorageDays > 0;

        /// <summary>Mở từ số đỏ trên menu trái: bỏ bộ lọc cũ, chỉ hiện đơn chờ xác nhận / chờ chia xe.</summary>
        public void ShowPendingOnly()
        {
            _pendingOnly = true;
            FilterMemory.Cancel(filterBar);
        }

        private void BindGrid()
        {
            var keepId = grid.CurrentRow?.Cells["Id"].Value as int?;
            var rows = _orders.Where(o =>
                    (!_pendingOnly || IsPending(o)) &&
                    filterBar.Is("status", TranslateStatus(o.Status)) &&
                    filterBar.Is("deadline", DeadlineGroup(o)) &&
                    filterBar.Is("customer", o.CustomerName) &&
                    filterBar.Is("warehouse", o.WarehouseName ?? "") &&
                    filterBar.MatchesSearch(o.Code, o.CustomerName, o.PickupPoint, o.DropoffPoint, o.WarehouseName, o.ContractCode))
                .ToList();
            // CHỈ các cột cần để nhận ra đơn và biết việc cần làm; hợp đồng, kho, điểm lấy / giao, số chuyến
            // hiện ở thẻ "Đơn đang chọn" khi bấm 1 dòng. Ô tìm vẫn tìm theo cả các thông tin đó.
            grid.DataSource = rows.Select(o => new
            {
                o.Id,
                Mã_đơn = o.Code,
                Khách_hàng = o.CustomerName,
                Trạng_thái = StatusWithProgress(o),
                Việc_cần_làm = TodoText(o), // bước tiếp theo, ghi ngắn (chi tiết + nút ở thẻ phía trên)
                // Ghi luôn số ngày: "01/11 · trễ 1 ngày" / "mai" / "còn 7 ngày" (đơn đã xong / hủy chỉ ghi ngày).
                Hạn_giao = DeadlineText(o) + (o.OverdueStorageDays > 0 ? " · 📦 nằm kho sau hạn — chưa rõ do ai" : ""),
            }).ToList();
            filterBar.SetCount(rows.Count, _orders.Count);
            var drafts = _orders.Count(o => o.Status == "Draft");
            var waitAlloc = _orders.Count(o => o.Status == "Confirmed" && o.ActiveTripCount == 0);
            var overdue = _orders.Count(o => o.OverdueStorageDays > 0);
            _pendingBanner.Update(string.Join(" · ", new[] { drafts > 0 ? $"{drafts} đơn nháp chờ xác nhận" : null,
                waitAlloc > 0 ? $"{waitAlloc} đơn chờ chia xe" : null,
                overdue > 0 ? $"{overdue} đơn hàng nằm kho sau hạn tính trễ, chưa xác định do ai" : null }.Where(x => x is not null)),
                drafts + waitAlloc + overdue, _pendingOnly);
            if (keepId is int id)
                foreach (DataGridViewRow r in grid.Rows)
                    if (r.Cells["Id"].Value is int rid && rid == id) { grid.CurrentCell = r.Cells["Mã_đơn"]; break; }
        }

        /// <summary>Chọn lại đúng đơn (sau khi tải lại / sửa nhanh) — đơn bị bộ lọc ẩn thì giữ nguyên dòng đang chọn.</summary>
        private void SelectOrder(int orderId)
        {
            foreach (DataGridViewRow r in grid.Rows)
                if (r.Cells["Id"].Value is int rid && rid == orderId) { grid.CurrentCell = r.Cells["Mã_đơn"]; return; }
        }

        private async Task OpenDetailAsync()
        {
            if (_api is null || grid.CurrentRow is null) return;
            var orderId = (int)grid.CurrentRow.Cells["Id"].Value;

            using var f = new OrderDetailForm(_api, orderId);
            f.ShowDialog(FindForm());
            await LoadAsync();
        }

        private async Task AllocateSelectedAsync()
        {
            if (_api is null) return;
            if (grid.CurrentRow is null)
            {
                Toast.Show(this, "👆 " + "Vui lòng chọn 1 đơn hàng trong danh sách trước.", ToastKind.Info);
                return;
            }

            if (_allocating) return;
            var orderId = (int)grid.CurrentRow.Cells["Id"].Value;
            var orderCode = _orders.First(o => o.Id == orderId).Code;

            _allocating = true;
            btnAllocate.Enabled = false;
            UseWaitCursor = true;
            lblStatus.Text = $"Đang chia xe cho đơn {orderCode}...";
            try
            {
                var result = await _api.TransportOrders.AllocateVehiclesAsync(orderId);

                if (!result.Trips.Any())
                {
                    await LoadAsync(); // VD người khác vừa chia xe đơn này -> danh sách + nút cập nhật
                    // Hộp SỬA NHANH: đăng ký ngày làm cho đúng tài xế còn thiếu, xem xe bị chặn vì giấy tờ, nhập hàng…
                    // Ngày xuất phát khác chọn ở Chi tiết đơn -> mở Chi tiết đơn.
                    _ = BeginInvoke(new Action(async () => await TruckingSystem.Desktop.GUI_Shared.QuickFix.ShowAsync(this, _api,
                        $"Chưa chia xe được đơn {orderCode}", result.Message ?? "Không có xe nào rảnh để chia.", result.Fixes,
                        $"🚚 Chia xe lại đơn {orderCode} ngay", async () => { SelectOrder(orderId); await AllocateSelectedAsync(); },
                        fix => { SelectOrder(orderId); _ = RunAsync(_actDetail); })));
                    return;
                }

                var lines = result.Trips.Select(a => $"  • Xe {a.VehiclePlate} — chở {a.QtyAssigned:#,##0.##} (Chuyến #{a.TripId})");
                var daysLine = result.EstimatedTripDays > 1 ? $"\n🕒 Dự kiến chuyến kéo dài ~{result.EstimatedTripDays} ngày (tuyến đường xa)." : "";
                var summary = $"Đơn {orderCode}\n" +
                              $"Tổng cần chở: {result.TotalQtyToShip}  |  Đã gán: {result.TotalQtyAssigned}\n" +
                              daysLine + "\n" +
                              $"Đã chia xe tự động thành {result.Trips.Count} chuyến:\n" +
                              string.Join("\n", lines) +
                              (result.Message != null ? $"\n\n⚠️ {result.Message}" : "");

                MessageBox.Show(FindForm(), summary, "Kết quả chia xe", MessageBoxButtons.OK,
                    result.Message != null ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                await LoadAsync();
            }
            catch (Exception ex)
            {
                Ask.Fail(this, "Lỗi khi chia xe: " + ex.Message);
            }
            finally
            {
                _allocating = false;
                if (!IsDisposed) { UseWaitCursor = false; UpdateActionButtons(); }
            }
        }

        private void UpdateActionButtons()
        {
            var id = grid.CurrentRow?.Cells["Id"]?.Value as int?;
            var order = id is null ? null : _orders.FirstOrDefault(o => o.Id == id.Value);
            // Chỉ chia xe cho đơn Đã xác nhận mà CHƯA có chuyến nào — đơn đã chia xe
            // vẫn giữ trạng thái "Đã xác nhận" tới khi xe xuất phát, trước đây nút
            // vẫn sáng và bấm vào thì bị server từ chối.
            btnAllocate.Enabled = !_allocating && order?.Status == "Confirmed" && order.ActiveTripCount == 0;
            UpdateSelectionUi(order);
        }

        /// <summary>Chữ cột Trạng thái: đơn đang vận chuyển đã giao được 1 phần thì kèm % ("Đang vận chuyển · 40%").</summary>
        private static string StatusWithProgress(TransportOrderListItem o)
            => o.Status == "InProgress" && o.ProgressPercent > 0 && o.ProgressPercent < 100
                ? $"{TranslateStatus(o.Status)} · {o.ProgressPercent:0.##}%"
                : TranslateStatus(o.Status);

        /// <summary>Bước tiếp theo của đơn — ghi NGẮN cho cột "Việc cần làm".</summary>
        private string TodoText(TransportOrderListItem o)
        {
            if (o.OverdueStorageDays > 0) return "📦 Xử lý hàng nằm kho sau hạn";
            var overdue = DeadlineGroup(o) == DeadlineOverdue;
            // Nhắc SỚM sắp hết ngày miễn phí lưu kho (còn 1–2 ngày) để kịp cho xe đi giao, tránh phát sinh phí cho khách.
            var freeSoon = o.FreeDaysLeft is 1 or 2 ? $"📦 Còn {o.FreeDaysLeft} ngày miễn phí lưu kho" : null;
            return o.Status switch
            {
                "Draft" => "✅ Xác nhận đơn",
                "Confirmed" when o.ActiveTripCount == 0 => "🚚 Chia xe",
                "Confirmed" => overdue ? "⏰ Quá hạn — chờ xe chạy" : freeSoon ?? "⏳ Chờ xe xuất phát",
                "InProgress" => overdue ? "⏰ Quá hạn — theo dõi gấp" : freeSoon ?? "🛣️ Theo dõi chuyến",
                // Đơn đã hủy nhưng chuyến Trả hàng chưa xong -> vẫn còn việc (không ghi "—").
                "Cancelled" when o.OpenReturnTripId > 0 => $"🚚 Còn chuyến Trả hàng #{o.OpenReturnTripId}",
                _ => "—",
            };
        }

        /// <summary>4 màu trạng thái đơn (giống màn Chuyến).</summary>
        private static Color StatusColor(string status) => status switch
        {
            "InProgress" => ColorTranslator.FromHtml("#F59E0B"),
            "Done" => ColorTranslator.FromHtml("#16A34A"),
            "Cancelled" => ColorTranslator.FromHtml("#DC2626"),
            _ => ColorTranslator.FromHtml("#94A3B8"), // Nháp / Đã xác nhận (chưa chạy)
        };

        private static string TranslateStatus(string s) => s switch
        {
            "Draft" => "Nháp",
            "Confirmed" => "Đã xác nhận",
            "InProgress" => "Đang vận chuyển",
            "Done" => "Hoàn tất",
            "Cancelled" => "Đã hủy",
            _ => s,
        };
    }
}