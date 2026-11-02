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
                "Ctrl+N tạo đơn · Enter mở chi tiết · F5 làm mới · Ctrl+F tìm nhanh");
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

            Load += async (_, _) => await LoadAsync();
        }

        /// <summary>Nhãn màu trạng thái / hạn giao + thanh tiến độ. (Bỏ hàng 6 thẻ số liệu theo góp ý GVHD — số cần
        /// chú ý gom vào 1 dòng ngắn SummaryText; lọc nhanh dùng bộ lọc Trạng thái / Hạn giao trên bảng.)</summary>
        private void SetupModernLook()
        {
            // Cửa sổ thấp: giữ bảng tối thiểu ~8 dòng, thiếu chỗ thì cả màn có thanh cuộn kéo xuống.
            ScrollFit.Enable(this, root, grid);

            GridDecor.HoverRows(grid);
            GridDecor.Pills(grid, "Trạng_thái", (text, _) => GridDecor.StatusTone(text));
            GridDecor.Pills(grid, "Hạn_giao", (_, row) => OrderAt(row) is { } o ? o.OverdueStorageDays > 0 ? GridDecor.Red : DeadlineGroup(o) switch
            {
                DeadlineOverdue => GridDecor.Red,
                DeadlineSoon => GridDecor.Amber,
                _ => null,
            } : null);
            // "Tiến độ" = phần trăm số hàng của đơn ĐÃ GIAO tới khách — rê chuột vào ô / tiêu đề cột để xem giải thích.
            grid.CellToolTipTextNeeded += (_, e) =>
            {
                if (e.ColumnIndex < 0 || grid.Columns[e.ColumnIndex].Name != "Tiến_độ") return;
                if (e.RowIndex < 0) { e.ToolTipText = "Tiến độ = phần trăm số hàng của đơn đã giao tới khách"; return; }
                if (OrderAt(e.RowIndex) is { } o)
                    e.ToolTipText = o.Status switch
                    {
                        "Draft" => "Đơn nháp — chưa xác nhận, chưa vận chuyển",
                        "Cancelled" => "Đơn đã hủy",
                        _ => $"Đã giao {o.ProgressPercent:0.##}% số hàng của đơn tới khách",
                    };
            };
            GridDecor.ProgressBars(grid, "Tiến_độ", row =>
            {
                if (OrderAt(row) is not { } o || o.Status is "Draft" or "Cancelled") return null;
                var ratio = (double)o.ProgressPercent / 100.0;
                return (ratio, $"{o.ProgressPercent:0.##}%", ratio >= 1 ? Theme.Success : Theme.Primary);
            });
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

        /// <summary>Nhóm hạn giao của đơn (đơn đã xong / hủy thì không còn "quá hạn").</summary>
        private static string DeadlineGroup(TransportOrderListItem o)
        {
            if (o.CommittedDeliveryDate is not DateTime due) return DeadlineNone;
            var days = (due.Date - DateTime.Today).Days;
            if (days < 0) return o.Status is "Done" or "Cancelled" ? DeadlineLater : DeadlineOverdue;
            return days <= 3 ? DeadlineSoon : DeadlineLater;
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
                Trạng_thái = TranslateStatus(o.Status),
                Tiến_độ = $"{o.ProgressPercent}%",
                // Ghi luôn số ngày: "01/11 · trễ 1 ngày" / "mai" / "còn 7 ngày" (đơn đã xong / hủy chỉ ghi ngày).
                Hạn_giao = ScreenKit.Deadline(o.CommittedDeliveryDate, done: o.Status is "Done" or "Cancelled")
                           + (o.OverdueStorageDays > 0 ? " · 📦 nằm kho quá hạn — chưa rõ do ai" : ""),
            }).ToList();
            filterBar.SetCount(rows.Count, _orders.Count);
            var drafts = _orders.Count(o => o.Status == "Draft");
            var waitAlloc = _orders.Count(o => o.Status == "Confirmed" && o.ActiveTripCount == 0);
            var overdue = _orders.Count(o => o.OverdueStorageDays > 0);
            _pendingBanner.Update(string.Join(" · ", new[] { drafts > 0 ? $"{drafts} đơn nháp chờ xác nhận" : null,
                waitAlloc > 0 ? $"{waitAlloc} đơn chờ chia xe" : null,
                overdue > 0 ? $"{overdue} đơn hàng nằm kho quá hạn, chưa xác định do ai" : null }.Where(x => x is not null)),
                drafts + waitAlloc + overdue, _pendingOnly);
            if (keepId is int id)
                foreach (DataGridViewRow r in grid.Rows)
                    if (r.Cells["Id"].Value is int rid && rid == id) { grid.CurrentCell = r.Cells["Mã_đơn"]; break; }
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
                    Ask.Fail(this, result.Message ?? "Không có xe nào rảnh để chia.");
                    await LoadAsync(); // VD người khác vừa chia xe đơn này -> danh sách + nút cập nhật
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