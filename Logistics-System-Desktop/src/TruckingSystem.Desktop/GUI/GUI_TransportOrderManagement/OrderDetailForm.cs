using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TransportOrderManagement;
using TruckingSystem.Desktop.DTO_TripManagement;
using TruckingSystem.Desktop.GUI_RouteManagement;
using TruckingSystem.Desktop.GUI_Shared;

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
            _fit = TruckingSystem.Desktop.Common.DialogFit.Enable(this, root); // vừa màn hình mọi máy, bảng không bị ép mất chữ
            TruckingSystem.Desktop.Common.DialogKeys.Enable(this); // Esc = đóng
            // Chữ cho NHÂN VIÊN (không nêu tên thuật toán) — đặt bằng code, không sửa Designer.
            btnAllocate.Text = "🚚  Chia xe tự động";
            // Đổi hạn giao / Phí lưu kho đã có ở màn danh sách (nút "📅 Hạn giao và phí lưu kho ▾" + chuột phải) -> ẩn ở
            // đây cho gọn, màn chi tiết chủ yếu để XEM. Phí lưu kho hiện ở dòng thông tin (rê chuột xem chi tiết) + bảng
            // lịch sử đổi hạn (có cột Hạn tính trễ). actionBar là FlowLayoutPanel nên các nút còn lại tự dồn lại.
            btnChangeDeadline.Visible = false;
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
            SetupActionLayout();
        }

        // ================= Bố cục nút theo quy tắc UX (1 nút chính, nguy hiểm tách riêng, việc tiếp theo) =================
        private readonly Button _btnFixAllocate = new() { AutoSize = true, Visible = false, Margin = new Padding(0, 0, 0, 6) };
        private FixHintItem? _fixAllocate;
        private readonly Label _nextStep = ButtonStyles.NextStep();
        private readonly Label _why = new() { AutoSize = true, ForeColor = Color.FromArgb(148, 163, 184), Font = new Font("Segoe UI", 8.5F), Margin = new Padding(0, 0, 0, 6) };
        private readonly FlowLayoutPanel _primaryBox = new() { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, Margin = Padding.Empty };
        private readonly Button _btnClose = Theme.MakeSecondaryButton("Đóng", 100);

        /// <summary>Chi tiết đơn: khung "👉 Việc tiếp theo" dưới dòng thông tin; hàng nút phụ (Sửa đơn, Xem lộ trình…) giữ ở
        /// giữa; CHÂN cửa sổ: [Hủy đơn… chữ đỏ] ........ [Đóng] [NÚT CHÍNH theo trạng thái: Xác nhận đơn / Chia xe].
        /// Trước đây 5 nút ngang hàng, 3 màu, nút Hủy đứng sát nút chính. Dời control có sẵn bằng code, không sửa Designer.</summary>
        private void SetupActionLayout()
        {
            KpiStrip.InsertRow(root, _nextStep, root.GetRow(lblTrackingLink));
            actionBar.Controls.Add(_why);
            ButtonStyles.Secondary(_btnFixAllocate);
            actionBar.Controls.Add(_btnFixAllocate);
            _btnFixAllocate.Click += async (_, _) =>
            {
                if (_api is null || _fixAllocate is null) return;
                if (await QuickFix.RunAsync(this, _api, _fixAllocate)) await LoadAsync(); // sửa xong -> nút Chia xe tự bật lại nếu đủ điều kiện
            };
            actionBar.SetFlowBreak(btnViewRoute, true);

            // Nút nguy hiểm -> chữ đỏ góc trái chân cửa sổ.
            actionBar.Controls.Remove(btnCancel);
            btnCancel.Text = "Hủy đơn…";
            ButtonStyles.DangerLink(btnCancel);
            // Nút chính (Xác nhận / Chia xe kèm ngày xuất phát) -> góc phải chân cửa sổ, cạnh Đóng.
            foreach (var c in new Control[] { btnConfirm, lblPlannedStart, dtPlannedStart, btnAllocate }) actionBar.Controls.Remove(c);
            lblPlannedStart.Anchor = dtPlannedStart.Anchor = AnchorStyles.Left;
            _primaryBox.Controls.AddRange(new Control[] { lblPlannedStart, dtPlannedStart, btnConfirm, btnAllocate });
            foreach (var b in new[] { btnEdit, btnViewRoute }) ButtonStyles.Secondary(b);
            ButtonStyles.Primary(btnConfirm);
            ButtonStyles.Primary(btnAllocate);
            _btnClose.Click += (_, _) => Close();
            CancelButton = _btnClose;
            ButtonStyles.Footer(this, btnCancel, _btnClose, _primaryBox);
            SetupAlerts(); // ⏰ không kịp hạn + 📦 nằm kho quá hạn (OrderDetailForm.Alerts.cs)
        }

        /// <summary>Hiện / ẩn nút + câu "Việc tiếp theo" theo trạng thái đơn (gọi sau khi tải đơn).</summary>
        private void UpdateActionLayout(TransportOrderDetailItem order)
        {
            var draft = order.Status == "Draft";
            var waitAlloc = order.Status == "Confirmed" && order.Trips.Count == 0;
            btnEdit.Visible = draft;
            btnConfirm.Visible = draft;
            btnAllocate.Visible = lblPlannedStart.Visible = dtPlannedStart.Visible = waitAlloc;
            btnCancel.Visible = btnCancel.Enabled;
            // Chưa chia được (khách chưa mang hàng tới đủ / kho thiếu khu) -> mờ nút + ghi lý do NGAY, không bấm rồi mới báo lỗi.
            var blocked = waitAlloc ? order.AllocateBlockReason : null;
            if (blocked is not null) btnAllocate.Enabled = false;
            _why.Text = draft ? "🚚 Chia xe: cần xác nhận đơn trước"
                : blocked is not null ? "🚚 Chia xe chưa được: " + blocked : "";
            _why.Visible = _why.Text.Length > 0;
            // Lối tắt sửa ngay cạnh lý do (mở sơ đồ kho để thêm khu / nhận hàng khách mang tới).
            _fixAllocate = blocked is not null ? order.AllocateFixes?.FirstOrDefault() : null;
            _btnFixAllocate.Text = _fixAllocate?.Text ?? "";
            _btnFixAllocate.Visible = _fixAllocate is not null;
            _nextStep.Text = "👉 Việc tiếp theo: " + order.Status switch
            {
                "Draft" when order.CargoLines.Count == 0 => "đơn chưa có dòng hàng — bấm Sửa đơn để thêm hàng rồi mới xác nhận được.",
                "Draft" => "kiểm tra thông tin rồi bấm ✅ Xác nhận đơn — xác nhận xong mới chia xe được.",
                "Confirmed" when order.Trips.Count == 0 && blocked is not null && order.PickupMethod == "SelfDelivery"
                    => "chờ khách mang hàng tới kho — nhân viên kho nhập đủ hàng (màn Kho → Nhận hàng khách mang tới) rồi mới chia xe.",
                "Confirmed" when order.Trips.Count == 0 && blocked is not null
                    => "kho trung chuyển của đơn chưa có khu phù hợp — thêm khu ở màn Kho → Quản lý khu vực rồi mới chia xe.",
                "Confirmed" when order.Trips.Count == 0 => "chọn ngày xuất phát rồi bấm 🚚 Chia xe tự động để hệ thống xếp xe + tài xế.",
                "Confirmed" or "InProgress" => "đơn đang được vận chuyển — theo dõi từng chuyến ở bảng bên dưới (màn Chuyến xe để thao tác).",
                "Done" => "đơn đã hoàn tất — không còn việc cần làm.",
                "Cancelled" => "đơn đã hủy.",
                _ => "—",
            };
        }

        /// <summary>Bấm link theo dõi -> copy vào Clipboard (gắn sự kiện trong Designer).</summary>
        private void LblTrackingLink_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_trackingUrl)) return;
            Clipboard.SetText(_trackingUrl);
            Toast.Show(this, "🔗 Đã sao chép link theo dõi — dán gửi khách được ngay.", ToastKind.Info);
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
                                 (string.IsNullOrWhiteSpace(order.ContractCode) ? "   ·   ⚠️ Chưa có hợp đồng" : $"   ·   Hợp đồng {order.ContractCode}");
                lblInfo.Text = $"Trạng thái: {TruckingSystem.Desktop.GUI_Shared.ReportFormat.OrderStatus(order.Status)}   |   Tiến độ: {order.ProgressPercent}%   |   " +
                               $"{order.PickupPoint} → {order.DropoffPoint}   |   Hạn giao: " +
                               (order.CommittedDeliveryDate is { } due ? due.ToString("dd/MM/yyyy") : "⚠️ chưa có hạn") +
                               (order.HasLateBasis ? $"   |   Hạn tính trễ: {order.LateBasisDate:dd/MM/yyyy} (công ty đã dời hạn)" : "");

                _ = LoadStorageFeeAsync(order);

                _trackingUrl = $"{_api.Connection.BaseUrl}/track.html?token={order.PublicTrackingToken}";
                lblTrackingLink.Text = "🔗 Link theo dõi cho khách (bấm để sao chép): " + _trackingUrl;

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
                UpdateActionLayout(order);
                UpdateAlerts(order);
                _fit?.Invoke(); // hiện thêm khung cảnh báo -> nới cửa sổ cho bảng hàng / chuyến vẫn thấy đủ
            }
            catch (Exception ex)
            {
                lblResult.Text = "Lỗi tải chi tiết đơn: " + ex.Message;
                lblResult.ForeColor = Theme.Danger;
            }
        }

        private readonly ToolTip _feeTip = new() { AutoPopDelay = 20000, InitialDelay = 200 };

        /// <summary>PHÍ LƯU KHO (miễn phí N ngày từ ngày nhập kho) — hiện ngay dòng thông tin đơn (rê chuột xem chi tiết) +
        /// bảng LỊCH SỬ ĐỔI HẠN có cột "Hạn tính trễ" (lần nào làm đổi mốc, lần nào không).</summary>
        private async Task LoadStorageFeeAsync(TransportOrderDetailItem order)
        {
            if (_api is null) return;
            try
            {
                var fee = await _api.TransportOrders.GetStorageFeeAsync(order.Id);
                if (IsDisposed || _currentOrder?.Id != order.Id) return;
                ShowDeadlineHistory(fee.History);
                if (fee.ReceivedDate is not DateTime received || fee.RatePerM3PerDay <= 0) { _fit?.Invoke(); return; }

                var today = DateTime.Today;
                var stage = fee.StillInStock
                    ? $"ngày thứ {(today - received.Date).Days + 1}" + (fee.FreeUntil is DateTime fu && today <= fu.Date ? $", miễn phí tới {fu:dd/MM}" : "")
                    : fee.LastIssueDate is DateTime li ? $"đã xuất hết {li:dd/MM}" : "hàng không còn trong kho";
                lblInfo.Text += $"   |   💰 Phí lưu kho: {stage}, {(fee.IsFinal ? "đã chốt" : "tạm tính")} {fee.TotalFee:N0} đ" +
                                (fee.PendingFee > 0 ? " (còn ngày chờ chọn bên chịu)" : "");
                var charged = fee.Days.Count(d => d.Kind == "Charged");
                _feeTip.SetToolTip(lblInfo,
                    $"PHÍ LƯU KHO — {fee.WarehouseName}: miễn phí {fee.FreeDays} ngày từ ngày nhập kho, {fee.RatePerM3PerDay:N0} đ/m³/ngày\n" +
                    $"Nhập kho {received:dd/MM} · hết miễn phí {fee.FreeUntil:dd/MM} · hạn tính trễ " +
                    (fee.LateBasisDate is DateTime b ? b.ToString("dd/MM") : "—") + "\n" +
                    $"{charged} ngày tính phí" + (fee.AfterBasisDays > 0 ? $" · {fee.AfterBasisDays} ngày sau hạn tính trễ" : "") +
                    $" · khách: {fee.TotalFee:N0} đ" + (fee.CompanyBorneFee > 0 ? $" · công ty chịu: {fee.CompanyBorneFee:N0} đ" : "") +
                    $"\n\n{fee.Note}\nXem từng ngày: màn Đơn → 📅 Hạn giao và phí lưu kho → Xem phí lưu kho.");
                _fit?.Invoke();
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
            if (!Ask.Confirm(this, $"Xác nhận đơn {_currentOrder?.Code}?",
                    "Sau khi xác nhận sẽ KHÔNG sửa được đơn nữa (chỉ có thể hủy). Bước tiếp theo: chia xe.",
                    "✅ Xác nhận đơn")) return;

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
            if (!Ask.Confirm(this, $"Hủy đơn {_currentOrder?.Code}?", "Đơn bị hủy hẳn, không hoàn tác được.",
                    "❌ Hủy đơn", danger: true, cancelText: "Giữ đơn")) return;

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
        internal static List<RouteStopItem> BuildRouteStops(TransportOrderDetailItem order)
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
                Ask.Fail(this,
                    "Chưa đủ tọa độ để vẽ lộ trình (cần kho trung chuyển và điểm giao có tọa độ" +
                    (_currentOrder.PickupMethod == "SelfDelivery" ? "" : ", cùng điểm lấy hàng") + ").\n\n" +
                    "Kiểm tra tọa độ của kho (màn Kho) và địa điểm (màn Địa điểm).");
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
                    if (!Ask.Confirm(this, "Không kịp hạn giao — vẫn chia xe?",
                        (result.Message ?? "Chuyến dự kiến không kịp hạn giao.") +
                        "\n\nHệ thống sẽ ghi nhật ký bạn đã xác nhận chia trễ hạn — nên báo trước cho khách.",
                        "🚚 Vẫn chia xe (trễ hạn)", danger: true, cancelText: "Chưa chia"))
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
                    // Hộp SỬA NHANH: đăng ký ngày làm cho tài xế còn thiếu / xem xe bị chặn / nhập hàng / sơ đồ kho;
                    // "Chọn ngày xuất phát khác" thì nhảy tới ô ngày ngay trên cửa sổ này.
                    await QuickFix.ShowAsync(this, _api, "Chưa chia xe được đơn này", result.Message, result.Fixes,
                        "🚚 Chia xe lại ngay", async () => { await LoadAsync(); if (btnAllocate.Enabled) BtnAllocate_Click(this, EventArgs.Empty); },
                        _ => { dtPlannedStart.Focus(); lblResult.Text = "🗓️ Chọn ngày xuất phát khác rồi bấm 🚚 Chia xe tự động."; lblResult.ForeColor = Theme.Primary; });
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