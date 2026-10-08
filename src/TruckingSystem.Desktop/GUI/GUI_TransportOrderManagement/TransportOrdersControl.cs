using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TransportOrderManagement;

namespace TruckingSystem.Desktop.GUI_TransportOrderManagement
{
    public partial class TransportOrdersControl : BaseUserControl
    {
        private ApiServices? _api;
        private List<TransportOrderListItem> _orders = new();

        public TransportOrdersControl()
        {
            InitializeComponent();
            Theme.StyleGrid(grid);

            grid.DataBindingComplete += (_, _) =>
            {
                if (grid.Columns["Id"] != null) grid.Columns["Id"]!.Visible = false;
            };
            Resize += (_, _) => { grid.Width = Width; grid.Height = Height - grid.Top; };
            grid.CellDoubleClick += async (_, _) => await OpenDetailAsync();
            // "Chia xe" chỉ bấm được với đơn ĐÃ XÁC NHẬN (Nháp phải xác nhận trước;
            // đơn đang chạy / hoàn tất / đã hủy không chia xe được nữa).
            grid.SelectionChanged += (_, _) => UpdateActionButtons();
            grid.DataBindingComplete += (_, _) => UpdateActionButtons();

            Load += async (_, _) => await LoadAsync();
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
            try
            {
                _orders = await _api.TransportOrders.GetTransportOrdersAsync();
                grid.DataSource = _orders.Select(o => new
                {
                    o.Id,
                    Mã_đơn = o.Code,
                    Khách_hàng = o.CustomerName,
                    Trạng_thái = TranslateStatus(o.Status),
                    Tiến_độ = $"{o.ProgressPercent}%",
                    Điểm_lấy = o.PickupPoint,
                    Điểm_giao = o.DropoffPoint,
                }).ToList();
                lblStatus.Text = $"Tổng {_orders.Count} đơn hàng — chọn 1 dòng rồi bấm \"Chia xe\" để chạy thuật toán Greedy";
                lblStatus.ForeColor = Theme.TextSecondary;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
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

            var orderId = (int)grid.CurrentRow.Cells["Id"].Value;
            var orderCode = _orders.First(o => o.Id == orderId).Code;

            try
            {
                var result = await _api.TransportOrders.AllocateVehiclesAsync(orderId);

                if (!result.Trips.Any())
                {
                    MessageBox.Show(FindForm(), result.Message ?? "Không có xe nào rảnh để chia.", "Không thể chia xe", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var lines = result.Trips.Select(a => $"  • Xe {a.VehiclePlate} — chở {a.QtyAssigned} (Chuyến #{a.TripId})");
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
        }

        private void UpdateActionButtons()
        {
            var id = grid.CurrentRow?.Cells["Id"]?.Value as int?;
            var order = id is null ? null : _orders.FirstOrDefault(o => o.Id == id.Value);
            btnAllocate.Enabled = order?.Status == "Confirmed";
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