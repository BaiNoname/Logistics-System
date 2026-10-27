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

        public TransportOrdersControl()
        {
            InitializeComponent();
            Theme.StyleGrid(grid);
            GridLayout.FitColumns(grid); // cột vừa nội dung, nhiều cột thì cuộn ngang thay vì cắt chữ
            Load += (_, _) => Theme.UniformButtons(toolbar); // nút đều nhau, xuống dòng vẫn thẳng cột

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
            SetupModernLook();

            Load += async (_, _) => await LoadAsync();
        }

        private readonly KpiStrip _kpi = new();

        /// <summary>Thẻ số liệu (bấm = lọc nhanh) + nhãn màu trạng thái / hạn giao + thanh tiến độ.</summary>
        private void SetupModernLook()
        {
            _kpi.AddCard("all", "📦", "Tất cả đơn", Theme.Primary);
            _kpi.AddCard("draft", "📝", "Nháp", GridDecor.AccentSlate, "status", "Nháp");
            _kpi.AddCard("confirmed", "🧭", "Chờ chia xe", GridDecor.AccentTeal, "status", "Đã xác nhận", alert: true);
            _kpi.AddCard("running", "🚚", "Đang vận chuyển", Theme.PrimaryDark, "status", "Đang vận chuyển");
            _kpi.AddCard("overdue", "⏰", "Quá hạn giao", Theme.Danger, "deadline", DeadlineOverdue, alert: true);
            _kpi.AddCard("done", "✅", "Hoàn tất", Theme.Success, "status", "Hoàn tất");
            _kpi.BindFilter(filterBar);
            KpiStrip.InsertRow(root, _kpi, 1);
            // Cửa sổ thấp: giữ bảng tối thiểu ~8 dòng, thiếu chỗ thì cả màn có thanh cuộn kéo xuống.
            ScrollFit.Enable(this, root, grid);

            GridDecor.HoverRows(grid);
            GridDecor.Pills(grid, "Trạng_thái", (text, _) => GridDecor.StatusTone(text));
            GridDecor.Pills(grid, "Hạn_giao", (_, row) => OrderAt(row) is { } o ? DeadlineGroup(o) switch
            {
                DeadlineOverdue => GridDecor.Red,
                DeadlineSoon => GridDecor.Amber,
                _ => null,
            } : null);
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

        private void UpdateKpis()
        {
            _kpi.SetValue("all", _orders.Count, $"{_orders.Count(o => o.Status is "Draft" or "Confirmed" or "InProgress")} đơn đang xử lý");
            _kpi.SetValue("draft", _orders.Count(o => o.Status == "Draft"), "chờ xác nhận với khách");
            var confirmed = _orders.Where(o => o.Status == "Confirmed").ToList();
            _kpi.SetValue("confirmed", confirmed.Count(o => o.ActiveTripCount == 0),
                $"{confirmed.Count} đơn đã xác nhận");
            var running = _orders.Where(o => o.Status == "InProgress").ToList();
            _kpi.SetValue("running", running.Count,
                running.Count == 0 ? "không có đơn đang chạy" : $"tiến độ TB {running.Average(o => (double)o.ProgressPercent):0}%",
                running.Count == 0 ? 0 : running.Average(o => (double)o.ProgressPercent) / 100.0);
            var soon = _orders.Count(o => DeadlineGroup(o) == DeadlineSoon && o.Status is not ("Done" or "Cancelled"));
            _kpi.SetValue("overdue", _orders.Count(o => DeadlineGroup(o) == DeadlineOverdue), $"{soon} đơn tới hạn trong 3 ngày");
            var closed = _orders.Count(o => o.Status is "Done" or "Cancelled");
            _kpi.SetValue("done", _orders.Count(o => o.Status == "Done"),
                $"{_orders.Count(o => o.Status == "Cancelled")} đơn đã hủy",
                closed == 0 ? 0 : _orders.Count(o => o.Status == "Done") / (double)Math.Max(1, _orders.Count));
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
            _kpi.Busy = true;
            try
            {
                _orders = await _api.TransportOrders.GetTransportOrdersAsync();
                filterBar.SetChoiceOptions("customer", _orders.Select(o => o.CustomerName));
                filterBar.SetChoiceOptions("warehouse", _orders.Select(o => o.WarehouseName ?? ""));
                BindGrid();
                UpdateKpis();
                lblStatus.Text = $"Tổng {_orders.Count} đơn hàng — chọn 1 dòng rồi bấm \"Chia xe\" để chạy thuật toán Greedy";
                lblStatus.ForeColor = Theme.TextSecondary;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
            finally { _kpi.Busy = false; }
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
        public void FocusOn(string search) => filterBar.ShowOnlySearch(search);

        private void BindGrid()
        {
            var keepId = grid.CurrentRow?.Cells["Id"].Value as int?;
            var rows = _orders.Where(o =>
                    filterBar.Is("status", TranslateStatus(o.Status)) &&
                    filterBar.Is("deadline", DeadlineGroup(o)) &&
                    filterBar.Is("customer", o.CustomerName) &&
                    filterBar.Is("warehouse", o.WarehouseName ?? "") &&
                    filterBar.MatchesSearch(o.Code, o.CustomerName, o.PickupPoint, o.DropoffPoint, o.WarehouseName, o.ContractCode))
                .ToList();
            grid.DataSource = rows.Select(o => new
            {
                o.Id,
                Mã_đơn = o.Code,
                Khách_hàng = o.CustomerName,
                Mã_HĐ = o.ContractCode ?? "—",
                Trạng_thái = TranslateStatus(o.Status),
                Tiến_độ = $"{o.ProgressPercent}%",
                Hạn_giao = o.CommittedDeliveryDate?.ToString("dd/MM/yyyy") ?? "—",
                Kho = o.WarehouseName ?? "—",
                Điểm_lấy = o.PickupPoint,
                Điểm_giao = o.DropoffPoint,
            }).ToList();
            filterBar.SetCount(rows.Count, _orders.Count);
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
                MessageBox.Show(FindForm(), "Vui lòng chọn 1 đơn hàng trong danh sách trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    MessageBox.Show(FindForm(), result.Message ?? "Không có xe nào rảnh để chia.", "Không thể chia xe", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    await LoadAsync(); // VD người khác vừa chia xe đơn này -> danh sách + nút cập nhật
                    return;
                }

                var lines = result.Trips.Select(a => $"  • Xe {a.VehiclePlate} — chở {a.QtyAssigned:#,##0.##} (Chuyến #{a.TripId})");
                var daysLine = result.EstimatedTripDays > 1 ? $"\n🕒 Dự kiến chuyến kéo dài ~{result.EstimatedTripDays} ngày (tuyến đường xa)." : "";
                var summary = $"Đơn {orderCode}\n" +
                              $"Tổng cần chở: {result.TotalQtyToShip}  |  Đã gán: {result.TotalQtyAssigned}\n" +
                              daysLine + "\n" +
                              $"Thuật toán Greedy đã chia thành {result.Trips.Count} chuyến:\n" +
                              string.Join("\n", lines) +
                              (result.Message != null ? $"\n\n⚠️ {result.Message}" : "");

                MessageBox.Show(FindForm(), summary, "Kết quả chia xe (Greedy)", MessageBoxButtons.OK,
                    result.Message != null ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(FindForm(), "Lỗi khi chia xe: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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