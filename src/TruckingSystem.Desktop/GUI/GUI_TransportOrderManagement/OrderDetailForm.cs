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
            Anim.FadeIn(this);
            Theme.StyleGrid(gridCargo);
            Theme.StyleGrid(gridTrips);
            GridDecor.HoverRows(gridTrips);
            GridDecor.Pills(gridTrips, "Trạng_thái", (text, _) => GridDecor.StatusTone(text));
            GridDecor.Pills(gridTrips, "Chặng", (text, _) => text.Contains("Lấy") ? GridDecor.Teal
                : text.Contains("Giao") ? GridDecor.Blue : GridDecor.Violet);
            dtPlannedStart.MinDate = DateTime.Today;
            dtPlannedStart.MaxDate = DateTime.Today.AddDays(3); // khớp server: chia xe trước tối đa 3 ngày
            dtPlannedStart.Value = DateTime.Today; // mặc định xuất phát hôm nay
        }

        /// <summary>Bấm link theo dõi -> copy vào Clipboard (gắn sự kiện trong Designer).</summary>
        private void LblTrackingLink_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_trackingUrl)) return;
            Clipboard.SetText(_trackingUrl);
            MessageBox.Show(FindForm(), "Đã copy link theo dõi vào Clipboard:\n" + _trackingUrl, "Đã copy", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

                lblHeader.Text = $"Đơn {order.Code} — {order.CustomerName}" +
                                 (string.IsNullOrWhiteSpace(order.ContractCode) ? "   ·   Không theo hợp đồng" : $"   ·   Hợp đồng {order.ContractCode}");
                lblInfo.Text = $"Trạng thái: {TruckingSystem.Desktop.GUI_Shared.ReportFormat.OrderStatus(order.Status)}   |   Tiến độ: {order.ProgressPercent}%   |   " +
                               $"{order.PickupPoint} → {order.DropoffPoint}   |   Hạn giao: " +
                               (order.CommittedDeliveryDate is { } due ? due.ToString("dd/MM/yyyy") : "⚠️ chưa có hạn");

                _ = LoadStorageFeeAsync(order);

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

                // Cột Chặng: cùng 1 lô hàng đi qua chặng Lấy hàng rồi Giao hàng (và Trả hàng khi
                // hủy) — không có cột này thì cộng "số lượng chở" ra gấp đôi/gấp ba.
                gridTrips.DataSource = order.Trips.Select(t => new
                {
                    Mã_chuyến = t.Id,
                    Chặng = LegText(t.LegType),
                    Điểm_đi = t.FromName ?? "—",
                    Điểm_đến = t.ToName ?? "—",
                    Biển_số_xe = t.VehiclePlate,
                    Tài_xế = t.DriverName,
                    Trạng_thái = TruckingSystem.Desktop.GUI_Shared.ReportFormat.TripStatus(t.Status),
                    Số_lượng_chở = t.QtyPlanned,
                }).ToList();

                btnAllocate.Enabled = order.Trips.Count == 0 && order.Status == "Confirmed";
                lblPlannedStart.Visible = dtPlannedStart.Visible = btnAllocate.Enabled;
                btnViewRoute.Enabled = BuildRouteStops(order).Count >= 2;

                // Chỉ cho Sửa/Xác nhận khi đơn còn Draft; chỉ cho Hủy khi Draft hoặc Confirmed
                btnEdit.Enabled = order.Status == "Draft";
                // Đổi / bổ sung hạn giao: đơn Đã xác nhận / Đang chạy (Nháp sửa qua Sửa đơn; Hoàn tất / Hủy thì khóa).
                btnChangeDeadline.Enabled = order.Status is "Confirmed" or "InProgress";
                btnConfirm.Enabled = order.Status == "Draft" && order.CargoLines.Count > 0;
                // Khớp server: KHÔNG hủy được khi đã có xe giao hàng xuất kho (đang chạy / chờ
                // duyệt / đã giao), hoặc còn chuyến lấy hàng đang chờ duyệt (phải duyệt trước).
                var deliveryLeft = order.Trips.Any(t => t.LegType == "Delivery" && t.Status is "InProgress" or "PendingConfirmation" or "Completed");
                var pickupPending = order.Trips.Any(t => t.LegType == "Pickup" && t.Status == "PendingConfirmation");
                btnCancel.Enabled = order.Status is not ("Done" or "Cancelled") && !deliveryLeft && !pickupPending;
            }
            catch (Exception ex)
            {
                lblResult.Text = "Lỗi tải chi tiết đơn: " + ex.Message;
                lblResult.ForeColor = Theme.Danger;
            }
        }

        private readonly ToolTip _feeTip = new() { AutoPopDelay = 20000, InitialDelay = 200 };

        /// <summary>💰 Phí lưu kho: bảng từng lần đổi hạn giao (bên yêu cầu, số ngày tính phí, m³, đơn giá) + tổng.</summary>
        private async void BtnStorageFee_Click(object? sender, EventArgs e)
        {
            if (_api is null || _currentOrder is null) return;
            using var f = new StorageFeeForm(_api, _currentOrder);
            f.ShowDialog(FindForm());
            if (f.ChangedAny) await LoadAsync(); // vừa ghi nhận phí bổ sung -> cập nhật dòng "💰 Phí lưu kho"
        }

        /// <summary>PHÍ LƯU KHO do khách dời hạn giao — hiện ngay dòng thông tin đơn (rê chuột để xem từng lần
        /// đổi hạn). Không có lần dời nào do khách thì không hiện gì.</summary>
        private async Task LoadStorageFeeAsync(TransportOrderDetailItem order)
        {
            if (_api is null) return;
            try
            {
                var fee = await _api.TransportOrders.GetStorageFeeAsync(order.Id);
                if (IsDisposed || _currentOrder?.Id != order.Id || fee.Changes.Count == 0) return;
                var lines = fee.Changes.Select(c =>
                    $"• {c.ChangedAt.ToLocalTime():dd/MM HH:mm} — {c.RequestedBy} yêu cầu: " +
                    $"{(c.OldDate is { } o ? o.ToString("dd/MM") : "(chưa có)")} → {c.NewDate:dd/MM}" +
                    (c.Fee > 0 ? $" — {c.ChargedDays} ngày, {c.ChargedM3Days:0.##} m³·ngày × {c.RatePerM3PerDay:N0} đ = {c.Fee:N0} đ"
                               : c.RequestedBy == "Khách hàng" && c.DelayDays > 0 ? " — chưa phát sinh (hàng chưa nằm kho trong các ngày dời)" : " — không tính phí") +
                    $"\n   Lý do: {c.Reason}");
                _feeTip.SetToolTip(lblInfo, $"PHÍ LƯU KHO DO KHÁCH DỜI HẠN GIAO\n{string.Join("\n", lines)}\n\n{fee.Note}");
                if (fee.TotalFee > 0 || fee.Changes.Any(c => c.RequestedBy == "Khách hàng"))
                    lblInfo.Text += $"   |   💰 Phí lưu kho (khách dời hạn): {fee.TotalFee:N0} đ{(fee.IsFinal ? "" : " (tạm tính)")} — rê chuột xem chi tiết";
            }
            catch
            {
                // Không lấy được phí lưu kho không chặn màn chi tiết đơn.
            }
        }

        /// <summary>Đổi hạn giao (khách xin dời, thỏa thuận lùi hạn) hoặc bổ sung hạn cho đơn cũ —
        /// bắt buộc lý do, server ghi nhật ký. Tình trạng hạn của các chuyến tự tính lại.</summary>
        private async void BtnChangeDeadline_Click(object? sender, EventArgs e)
        {
            if (_api is null || _currentOrder is null) return;
            using var f = new ChangeDeadlineForm(_api, _currentOrder);
            if (f.ShowDialog(FindForm()) != DialogResult.OK) return;
            lblResult.Text = "✅ " + (f.ResultMessage ?? "Đã đổi hạn giao.");
            lblResult.ForeColor = Theme.Success;
            await LoadAsync();
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
                    await LoadAsync(); // đơn có thể vừa bị người khác đổi -> bật/tắt nút theo dữ liệu mới
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
                    await LoadAsync();
                }
            }
            catch (Exception ex)
            {
                lblResult.Text = "Lỗi: " + ex.Message;
                lblResult.ForeColor = Theme.Danger;
                btnCancel.Enabled = true;
            }
        }

        private static string LegText(string legType) => legType switch
        {
            "Pickup" => "Lấy hàng",
            "Return" => "Trả hàng",
            _ => "Giao hàng",
        };

        /// <summary>Lộ trình của đơn ĐI QUA KHO trung chuyển — đúng như các chuyến và ước
        /// tính chạy thật: (Điểm lấy hàng →) Kho → Điểm giao hàng. Đơn khách tự mang tới
        /// không có chặng lấy hàng: Kho → Điểm giao.</summary>
        private static List<RouteStopItem> BuildRouteStops(TransportOrderDetailItem order)
        {
            var stops = new List<RouteStopItem>();
            if (order.PickupMethod != "SelfDelivery" && order.PickupLat.HasValue && order.PickupLng.HasValue)
                stops.Add(new() { Lat = order.PickupLat, Lng = order.PickupLng, Label = "📍 Điểm lấy hàng" });
            if (order.WarehouseLat.HasValue && order.WarehouseLng.HasValue)
                stops.Add(new() { Lat = order.WarehouseLat, Lng = order.WarehouseLng, Label = "🏬 Kho " + order.WarehouseName });
            if (order.DropoffLat.HasValue && order.DropoffLng.HasValue)
                stops.Add(new() { Lat = order.DropoffLat, Lng = order.DropoffLng, Label = "🏁 Điểm giao hàng" });
            return stops;
        }

        private void BtnViewRoute_Click(object? sender, EventArgs e)
        {
            if (_api is null || _currentOrder is null) return;
            var stops = BuildRouteStops(_currentOrder);
            if (stops.Count < 2)
            {
                MessageBox.Show(FindForm(),
                    "Chưa đủ tọa độ để vẽ lộ trình (cần kho trung chuyển và điểm giao có tọa độ" +
                    (_currentOrder.PickupMethod == "SelfDelivery" ? "" : ", cùng điểm lấy hàng") + ").\n\n" +
                    "Kiểm tra tọa độ của kho (màn Kho) và địa điểm (màn Địa điểm).",
                    "Thiếu tọa độ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
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
                // Ngày xuất phát dự kiến: giấy tờ xe, bằng lái, khám sức khỏe, ngày làm tài xế được
                // kiểm tra trên đúng khoảng [ngày này .. ngày cuối chuyến].
                var plannedStart = dtPlannedStart.Value.Date;
                if (plannedStart < DateTime.Today)
                {
                    lblResult.Text = "⚠️ Ngày xuất phát dự kiến đã qua — chọn từ hôm nay trở đi.";
                    lblResult.ForeColor = Theme.Warning;
                    btnAllocate.Enabled = true;
                    return;
                }
                var result = await _api.TransportOrders.AllocateVehiclesAsync(_orderId, plannedStart: plannedStart);

                // Không kịp hạn giao -> KHÔNG chặn hẳn: hỏi người điều phối (có thể đã thỏa thuận lại
                // với khách). Đồng ý thì chia, hệ thống ghi nhật ký người đã chấp nhận trễ hạn.
                if (!result.Success && result.NeedsLateConfirmation)
                {
                    var answer = MessageBox.Show(FindForm(),
                        (result.Message ?? "Chuyến dự kiến không kịp hạn giao.") +
                        "\n\nVẫn chia xe cho đơn này?\n(Hệ thống sẽ ghi nhật ký bạn đã xác nhận chia trễ hạn — nên báo trước cho khách.)",
                        "Không kịp hạn giao", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                    if (answer != DialogResult.Yes)
                    {
                        lblResult.Text = "⏰ Chưa chia xe — " + (result.Message ?? "không kịp hạn giao.");
                        lblResult.ForeColor = Theme.Warning;
                        await LoadAsync();
                        return;
                    }
                    lblResult.Text = "Đang chia xe (đã xác nhận trễ hạn)...";
                    result = await _api.TransportOrders.AllocateVehiclesAsync(_orderId, acceptLate: true, plannedStart: plannedStart);
                }

                if (result.Success)
                {
                    var daysText = result.EstimatedTripDays > 1 ? $"\n🕒 Dự kiến chuyến kéo dài ~{result.EstimatedTripDays} ngày (tuyến đường xa)." : "";
                    lblResult.Text = $"✅ Đã gán {result.Trips.Count} xe, chở {result.TotalQtyAssigned}/{result.TotalQtyToShip}.{daysText}" +
                                      (result.Message != null ? "\n⚠️ " + result.Message : "");
                    lblResult.ForeColor = result.Message != null ? Theme.Warning : Theme.Success;
                    await LoadAsync();
                }
                else
                {
                    lblResult.Text = "⚠️ " + (result.Message ?? "Không thể chia xe.");
                    lblResult.ForeColor = Theme.Warning;
                    await LoadAsync(); // VD người khác vừa chia xe xong -> nút Chia xe tự tắt
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