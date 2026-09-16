using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TransportOrderManagement;
using TruckingSystem.Desktop.GUI_RouteManagement;

namespace TruckingSystem.Desktop.GUI_TransportOrderManagement;

/// <summary>Xem chi tiết 1 đơn: thông tin chung, danh sách dòng hàng, danh sách chuyến đã gán,
/// và nút "Chia xe" gọi thuật toán Greedy — đây là nơi thể hiện trực tiếp thuật toán chạy thật.</summary>
public class OrderDetailForm : BaseForm
{
    private readonly ApiServices _api;
    private readonly int _orderId;

    private readonly Label _lblHeader = new() { Font = Theme.FontSectionTitle, ForeColor = Theme.TextPrimary, Top = 20, Left = 24, AutoSize = true };
    private readonly Label _lblInfo = new() { Font = Theme.FontNormal, ForeColor = Theme.TextSecondary, Top = 50, Left = 24, AutoSize = true };

    private readonly Label _lblCargoTitle = new() { Text = "Dòng hàng hóa", Font = Theme.FontBold, Top = 100, Left = 24, AutoSize = true };
    private readonly DataGridView _gridCargo = new() { Top = 125, Left = 24, Width = 700, Height = 130 };

    private readonly Label _lblTripsTitle = new() { Text = "Chuyến đã gán", Font = Theme.FontBold, Top = 270, Left = 24, AutoSize = true };
    private readonly DataGridView _gridTrips = new() { Top = 295, Left = 24, Width = 700, Height = 150 };

    private readonly Button _btnAllocate;
    private readonly Button _btnViewRoute;
    private readonly Label _lblResult = new() { Top = 500, Left = 24, Width = 700, Height = 60, Font = Theme.FontSmall };

    private TransportOrderDetailItem? _currentOrder;

    public OrderDetailForm(ApiServices api, int orderId)
    {
        _api = api;
        _orderId = orderId;

        Text = "Chi tiết đơn vận chuyển";
        Width = 780; Height = 610;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        BackColor = Color.White;

        Theme.StyleGrid(_gridCargo);
        Theme.StyleGrid(_gridTrips);

        _btnAllocate = Theme.MakePrimaryButton("🚚  Chia xe (Greedy)", 200, 40);
        _btnAllocate.Top = 460; _btnAllocate.Left = 24;
        _btnAllocate.Click += async (_, _) => await RunAllocateAsync();

        _btnViewRoute = Theme.MakeSecondaryButton("🗺️  Xem lộ trình", 180, 40);
        _btnViewRoute.Top = 460; _btnViewRoute.Left = 234;
        _btnViewRoute.Enabled = false; // chỉ bật khi cả 2 điểm có tọa độ
        _btnViewRoute.Click += (_, _) => OpenRouteMap();

        Controls.Add(_lblHeader);
        Controls.Add(_lblInfo);
        Controls.Add(_lblCargoTitle);
        Controls.Add(_gridCargo);
        Controls.Add(_lblTripsTitle);
        Controls.Add(_gridTrips);
        Controls.Add(_btnAllocate);
        Controls.Add(_btnViewRoute);
        Controls.Add(_lblResult);

        Load += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            var order = await _api.TransportOrders.GetOrderDetailAsync(_orderId);
            _currentOrder = order;

            _lblHeader.Text = $"Đơn {order.Code} — {order.CustomerName}";
            _lblInfo.Text = $"Trạng thái: {order.Status}   |   Tiến độ: {order.ProgressPercent}%   |   " +
                            $"{order.PickupPoint} → {order.DropoffPoint}";

            _gridCargo.DataSource = order.CargoLines.Select(c => new
            {
                Loại_hàng = c.CargoType,
                Số_kiện = c.PackageCount,
                Khối_lượng_kg = c.WeightKg,
                Thể_tích_m3 = c.VolumeM3,
                Dễ_vỡ = c.IsFragile ? "Có" : "",
                Nguy_hiểm = c.IsHazmat ? "Có" : "",
                Số_lượng = c.QtyTotal,
            }).ToList();

            _gridTrips.DataSource = order.Trips.Select(t => new
            {
                Mã_chuyến = t.Id,
                Biển_số_xe = t.VehiclePlate,
                Tài_xế = t.DriverName,
                Trạng_thái = t.Status,
                Số_lượng_chở = t.QtyPlanned,
            }).ToList();

            // Nếu đơn đã có chuyến, không cần chia xe lại nữa
            _btnAllocate.Enabled = order.Trips.Count == 0;

            // Chỉ xem được lộ trình nếu CẢ 2 điểm lấy/giao đều chọn từ Location đã lưu (có tọa độ)
            _btnViewRoute.Enabled = order.PickupLat.HasValue && order.PickupLng.HasValue
                                 && order.DropoffLat.HasValue && order.DropoffLng.HasValue;
        }
        catch (Exception ex)
        {
            _lblResult.Text = "Lỗi tải chi tiết đơn: " + ex.Message;
            _lblResult.ForeColor = Theme.Danger;
        }
    }

    private void OpenRouteMap()
    {
        if (_currentOrder is null) return;
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

    private async Task RunAllocateAsync()
    {
        _btnAllocate.Enabled = false;
        _lblResult.Text = "Đang chạy thuật toán chia xe...";
        _lblResult.ForeColor = Theme.TextSecondary;
        try
        {
            var result = await _api.TransportOrders.AllocateVehiclesAsync(_orderId);

            if (result.Success)
            {
                _lblResult.Text = $"✅ Đã gán {result.Trips.Count} xe, chở {result.TotalQtyAssigned}/{result.TotalQtyToShip}." +
                                   (result.Message != null ? "\n⚠️ " + result.Message : "");
                _lblResult.ForeColor = Theme.Success;
                await LoadAsync(); // tải lại để thấy danh sách chuyến vừa tạo
            }
            else
            {
                _lblResult.Text = "⚠️ " + (result.Message ?? "Không thể chia xe.");
                _lblResult.ForeColor = Theme.Warning;
                _btnAllocate.Enabled = true;
            }
        }
        catch (Exception ex)
        {
            _lblResult.Text = "Lỗi: " + ex.Message;
            _lblResult.ForeColor = Theme.Danger;
            _btnAllocate.Enabled = true;
        }
    }
}