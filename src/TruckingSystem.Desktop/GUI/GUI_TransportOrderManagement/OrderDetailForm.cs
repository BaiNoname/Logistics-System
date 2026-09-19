using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TransportOrderManagement;
using TruckingSystem.Desktop.GUI_RouteManagement;

namespace TruckingSystem.Desktop.GUI_TransportOrderManagement;

/// <summary>Logic — xem chi tiết 1 đơn: dòng hàng, chuyến đã gán, nút Chia xe (Greedy)
/// và Xem lộ trình (VietMap). Giao diện nằm ở OrderDetailForm.Designer.cs.</summary>
public partial class OrderDetailForm : BaseForm
{
    private ApiServices? _api;
    private int _orderId;
    private TransportOrderDetailItem? _currentOrder;

    public OrderDetailForm()
    {
        InitializeComponent();
        Theme.StyleGrid(gridCargo);
        Theme.StyleGrid(gridTrips);
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

            btnAllocate.Enabled = order.Trips.Count == 0;
            btnViewRoute.Enabled = order.PickupLat.HasValue && order.PickupLng.HasValue
                                && order.DropoffLat.HasValue && order.DropoffLng.HasValue;
        }
        catch (Exception ex)
        {
            lblResult.Text = "Lỗi tải chi tiết đơn: " + ex.Message;
            lblResult.ForeColor = Theme.Danger;
        }
    }

    private void BtnViewRoute_Click(object? sender, EventArgs e)
    {
        if (_api is null || _currentOrder is null) return;
        if (_currentOrder.PickupLat is null || _currentOrder.PickupLng is null
            || _currentOrder.DropoffLat is null || _currentOrder.DropoffLng is null)
        {
            MessageBox.Show(
                "Đơn này chưa chọn địa điểm từ danh sách đã lưu (chỉ gõ tay), nên chưa có tọa độ để vẽ lộ trình.\n\n" +
                "Vào mục 'Địa điểm' để thêm tọa độ, rồi tạo lại đơn bằng cách chọn từ danh sách địa điểm đã lưu.",
                "Thiếu tọa độ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var f = new RouteMapForm(
            _api, _currentOrder.Code,
            _currentOrder.PickupLat.Value, _currentOrder.PickupLng.Value,
            _currentOrder.DropoffLat.Value, _currentOrder.DropoffLng.Value);
        f.ShowDialog();
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
                lblResult.Text = $"✅ Đã gán {result.Trips.Count} xe, chở {result.TotalQtyAssigned}/{result.TotalQtyToShip}." +
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
