using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TransportOrderManagement;
using TruckingSystem.Desktop.DTO_TripManagement;
using TruckingSystem.Desktop.GUI_RouteManagement;

namespace TruckingSystem.Desktop.GUI_TransportOrderManagement
{
    /// <summary>Logic — xem chi tiết 1 đơn: dòng hàng, chuyến đã gán, nút Chia xe (Greedy)
    /// và Xem lộ trình (VietMap). Giao diện nằm ở OrderDetailForm.Designer.cs.</summary>
    public partial class OrderDetailForm : BaseForm
    {
        private ApiServices? _api;
        private int _orderId;
        private TransportOrderDetailItem? _currentOrder;
        private string _trackingUrl = "";

        public OrderDetailForm()
        {
            InitializeComponent();
            Theme.StyleGrid(gridCargo);
            Theme.StyleGrid(gridTrips);
            lblTrackingLink.Cursor = Cursors.Hand;
            lblTrackingLink.Click += (_, _) =>
            {
                if (string.IsNullOrEmpty(_trackingUrl)) return;
                Clipboard.SetText(_trackingUrl);
                MessageBox.Show(FindForm(), "Đã copy link theo dõi vào Clipboard:\n" + _trackingUrl, "Đã copy", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
        }

        public OrderDetailForm(ApiServices api, int orderId) : this()
        {
            _api = api;
            _orderId = orderId;
            Load += async (_, _) => await LoadAsync();
        }

        private async Task LoadAsync()
        {
            if (_api is null) return;
            try
            {
                var order = await _api.TransportOrders.GetOrderDetailAsync(_orderId);
                _currentOrder = order;

                lblHeader.Text = $"Đơn {order.Code} — {order.CustomerName}";
                lblInfo.Text = $"Trạng thái: {order.Status}   |   Tiến độ: {order.ProgressPercent}%   |   " +
                               $"{order.PickupPoint} → {order.DropoffPoint}";

                _trackingUrl = $"{_api.Connection.BaseUrl}/track.html?token={order.PublicTrackingToken}";
                lblTrackingLink.Text = "🔗 Link theo dõi công khai (bấm để copy): " + _trackingUrl;

                gridCargo.DataSource = order.CargoLines.Select(c => new
                {
                    Loại_hàng = c.CargoType,
                    Số_kiện = c.PackageCount,
                    Khối_lượng_kg = c.WeightKg,
                    Thể_tích_m3 = c.VolumeM3,
                    Dễ_vỡ = c.IsFragile ? "Có" : "",
                    Nguy_hiểm = c.IsHazmat ? "Có" : "",
                    Số_lượng = c.QtyTotal,
                }).ToList();

                gridTrips.DataSource = order.Trips.Select(t => new
                {
                    Mã_chuyến = t.Id,
                    Biển_số_xe = t.VehiclePlate,
                    Tài_xế = t.DriverName,
                    Trạng_thái = t.Status,
                    Số_lượng_chở = t.QtyPlanned,
                }).ToList();

                btnAllocate.Enabled = order.Trips.Count == 0 && order.Status == "Confirmed";
                btnViewRoute.Enabled = order.PickupLat.HasValue && order.PickupLng.HasValue
                                    && order.DropoffLat.HasValue && order.DropoffLng.HasValue;

                // Chỉ cho Sửa/Xác nhận khi đơn còn Draft; chỉ cho Hủy khi Draft hoặc Confirmed
                btnEdit.Enabled = order.Status == "Draft";
                btnConfirm.Enabled = order.Status == "Draft" && order.CargoLines.Count > 0;
                btnCancel.Enabled = order.Status is not ("Done" or "Cancelled");
            }
            catch (Exception ex)
            {
                lblResult.Text = "Lỗi tải chi tiết đơn: " + ex.Message;
                lblResult.ForeColor = Theme.Danger;
            }
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;
            using var f = new CreateOrderForm(_api, _orderId);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) _ = LoadAsync();
        }

        private async void BtnConfirm_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;
            var confirm = MessageBox.Show(FindForm(),
                "Xác nhận đơn này? Sau khi xác nhận sẽ KHÔNG sửa được nữa (chỉ có thể hủy).",
                "Xác nhận đơn hàng", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            btnConfirm.Enabled = false;
            try
            {
                var result = await _api.TransportOrders.ConfirmOrderAsync(_orderId);
                if (result.Success)
                {
                    lblResult.Text = "✅ Đã xác nhận đơn hàng.";
                    lblResult.ForeColor = Theme.Success;
                    await LoadAsync();
                }
                else
                {
                    lblResult.Text = "⚠️ " + result.Message;
                    lblResult.ForeColor = Theme.Warning;
                    btnConfirm.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                lblResult.Text = "Lỗi: " + ex.Message;
                lblResult.ForeColor = Theme.Danger;
                btnConfirm.Enabled = true;
            }
        }

        private async void BtnCancel_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;
            var confirm = MessageBox.Show(FindForm(),
                "Bạn có chắc chắn muốn HỦY đơn hàng này không? Không thể hoàn tác.",
                "Xác nhận hủy đơn", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            btnCancel.Enabled = false;
            try
            {
                var result = await _api.TransportOrders.CancelOrderAsync(_orderId);
                if (result.Success)
                {
                    lblResult.Text = "❌ Đã hủy đơn hàng." + (string.IsNullOrEmpty(result.Message) ? "" : "\n" + result.Message);
                    lblResult.ForeColor = string.IsNullOrEmpty(result.Message) ? Theme.TextSecondary : Theme.Warning;
                    // Có cảnh báo (VD xe lấy hàng đang chạy có thể đã bốc hàng) -> hiện hộp thoại để không bị bỏ sót
                    if (!string.IsNullOrEmpty(result.Message))
                        MessageBox.Show(FindForm(), result.Message, "Đã hủy đơn — cần lưu ý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    await LoadAsync();
                }
                else
                {
                    lblResult.Text = "⚠️ " + result.Message;
                    lblResult.ForeColor = Theme.Warning;
                    btnCancel.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                lblResult.Text = "Lỗi: " + ex.Message;
                lblResult.ForeColor = Theme.Danger;
                btnCancel.Enabled = true;
            }
        }

        private void BtnViewRoute_Click(object? sender, EventArgs e)
        {
            if (_api is null || _currentOrder is null) return;
            if (_currentOrder.PickupLat is null || _currentOrder.PickupLng is null
                || _currentOrder.DropoffLat is null || _currentOrder.DropoffLng is null)
            {
                MessageBox.Show(FindForm(),
                    "Đơn này chưa chọn địa điểm từ danh sách đã lưu (chỉ gõ tay), nên chưa có tọa độ để vẽ lộ trình.\n\n" +
                    "Vào mục 'Địa điểm' để thêm tọa độ, rồi tạo lại đơn bằng cách chọn từ danh sách địa điểm đã lưu.",
                    "Thiếu tọa độ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var stops = new List<RouteStopItem>
            {
                new() { Lat = _currentOrder.PickupLat, Lng = _currentOrder.PickupLng, Label = "📍 Điểm lấy hàng" },
                new() { Lat = _currentOrder.DropoffLat, Lng = _currentOrder.DropoffLng, Label = "🏁 Điểm giao hàng" },
            };
            using var f = new RouteMapForm(_api, _currentOrder.Code, stops);
            f.ShowDialog(FindForm());
        }

        private async void BtnAllocate_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;
            btnAllocate.Enabled = false;
            lblResult.Text = "Đang chạy thuật toán chia xe...";
            lblResult.ForeColor = Theme.TextSecondary;
            try
            {
                var result = await _api.TransportOrders.AllocateVehiclesAsync(_orderId);

                if (result.Success)
                {
                    var daysText = result.EstimatedTripDays > 1 ? $"\n🕒 Dự kiến chuyến kéo dài ~{result.EstimatedTripDays} ngày (tuyến đường xa)." : "";
                    lblResult.Text = $"✅ Đã gán {result.Trips.Count} xe, chở {result.TotalQtyAssigned}/{result.TotalQtyToShip}.{daysText}" +
                                      (result.Message != null ? "\n⚠️ " + result.Message : "");
                    lblResult.ForeColor = Theme.Success;
                    await LoadAsync();
                }
                else
                {
                    lblResult.Text = "⚠️ " + (result.Message ?? "Không thể chia xe.");
                    lblResult.ForeColor = Theme.Warning;
                    btnAllocate.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                lblResult.Text = "Lỗi: " + ex.Message;
                lblResult.ForeColor = Theme.Danger;
                btnAllocate.Enabled = true;
            }
        }
    }
}