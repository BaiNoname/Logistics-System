using System.ComponentModel;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_CustomerManagement;
using TruckingSystem.Desktop.DTO_LocationManagement;
using TruckingSystem.Desktop.DTO_TransportOrderManagement;
using TruckingSystem.Desktop.DTO_WarehouseManagement;

namespace TruckingSystem.Desktop.GUI_TransportOrderManagement
{
    /// <summary>
    /// Logic tạo/sửa đơn vận chuyển — DÙNG CHUNG 1 FORM cho cả 2 việc:
    ///   - Tạo mới: new CreateOrderForm(api)
    ///   - Sửa đơn (chỉ khi đơn còn Draft): new CreateOrderForm(api, editOrderId)
    /// Hỗ trợ NHIỀU dòng hàng hóa (CargoLine) qua lưới có thể chỉnh sửa trực
    /// tiếp (gridCargoLines) — khác bản trước đây chỉ cho đúng 1 dòng.
    /// Giao diện nằm ở CreateOrderForm.Designer.cs.
    /// </summary>
    public partial class CreateOrderForm : BaseForm
    {
        private ApiServices? _api;
        private int? _editOrderId; // null = đang Tạo mới; có giá trị = đang Sửa đơn
        private readonly BindingList<CargoLineItem> _cargoLines = new();
        private List<LocationItem> _locations = new();
        private List<LocationItem> _usableLocations = new(); // có tọa độ + đúng khách
        private bool _rebindingLocations;                    // đang đổ lại dropdown bằng code
        private readonly AutoCompleteStringCollection _cargoTypeSuggestions = new();
        private List<string> _cargoTypes = new();
        private int _contractLoadVersion;                    // bỏ kết quả tải hợp đồng cũ khi đổi khách liên tục
        private const string NoContractText = "(Không theo hợp đồng — khách lẻ)";

        // ---- HẠN GIAO (bắt buộc) + gợi ý theo ước tính số ngày chạy của tuyến ----
        private int? _estimatedDays;          // số ngày chạy ước tính (VietMap, 8 giờ lái/ngày)
        private bool _deadlineTouched;        // người dùng đã tự chọn hạn giao -> không tự ghi đè nữa
        private bool _settingDeadline;        // đang gán bằng code (không tính là người dùng chọn)
        private int _estimateVersion;         // bỏ kết quả ước tính cũ khi người dùng đổi tiếp
        private readonly System.Windows.Forms.Timer _estimateTimer = new() { Interval = 700 };
        /// <summary>Hạn giao gợi ý = ngày giao sớm nhất + 1 ngày dự phòng (nhập kho, chờ xe, kẹt đường).</summary>
        private const int DeadlineBufferDays = 1;

        public CreateOrderForm()
        {
            InitializeComponent();

            // Hạn giao BẮT BUỘC (server cũng kiểm tra). Khi chọn xong địa điểm + kho, hệ thống ước
            // tính số ngày chạy và GỢI Ý hạn giao (người dùng vẫn chỉnh theo yêu cầu của khách).
            SetDeadline(DateTime.Today.AddDays(1));
            dtCommitted.ValueChanged += (_, _) =>
            {
                if (!_settingDeadline) _deadlineTouched = true;
                UpdateDeadlineHint();
            };
            _estimateTimer.Tick += async (_, _) => { _estimateTimer.Stop(); await SuggestDeadlineAsync(); };
            cboPickupLocation.SelectedIndexChanged += (_, _) => RequestEstimate();
            cboDropoffLocation.SelectedIndexChanged += (_, _) => RequestEstimate();
            cboWarehouse.SelectedIndexChanged += (_, _) => RequestEstimate();
            rbSelfDelivery.CheckedChanged += (_, _) => RequestEstimate();
            FormClosed += (_, _) => _estimateTimer.Dispose();

            // Điểm lấy/giao BẮT BUỘC chọn từ danh mục Địa điểm (có tọa độ) — ô địa
            // chỉ chỉ để hiển thị, không gõ tay nữa (địa chỉ gõ tay không có tọa độ ->
            // bản đồ, ước tính quãng đường/số ngày/nhiên liệu đều không tính được).
            // Điểm lấy và điểm giao KHÔNG được trùng: chọn bên này thì bên kia tự ẨN địa điểm đó
            // (cả địa điểm khác tên nhưng cùng vị trí) — không thể chọn nhầm. Server cũng chặn.
            cboPickupLocation.SelectedIndexChanged += (_, _) =>
            {
                txtPickup.Text = cboPickupLocation.SelectedItem is LocationItem loc && loc.Id != 0 ? loc.Address
                    : rbSelfDelivery.Checked ? SelfDeliveryPickupText : "";
                RebindOtherLocationList(cboDropoffLocation);
            };
            cboDropoffLocation.SelectedIndexChanged += (_, _) =>
            {
                txtDropoff.Text = cboDropoffLocation.SelectedItem is LocationItem loc && loc.Id != 0 ? loc.Address : "";
                RebindOtherLocationList(cboPickupLocation);
            };
            rbSelfDelivery.CheckedChanged += (_, _) => { UpdatePickupForMethod(); RebindOtherLocationList(cboDropoffLocation); };
            // Đổi khách hàng -> lọc lại địa điểm (địa chỉ riêng của khách khác không được dùng).
            // (Chế độ Sửa đơn: không đổi được khách; danh sách được lọc 1 lần trong LoadExistingOrderAsync.)
            cboCustomer.SelectedIndexChanged += async (_, _) => { if (_locations.Count > 0 && !_editOrderId.HasValue) await LoadLocationsAsync(); };
            // Đổi khách hàng -> nạp lại hợp đồng ĐANG HIỆU LỰC của đúng khách đó.
            cboCustomer.SelectedIndexChanged += async (_, _) =>
            {
                if (!_editOrderId.HasValue && cboCustomer.SelectedValue is int customerId) await LoadContractsAsync(customerId, null, null);
            };
            btnAddPickupLocation.Click += async (_, _) => await AddLocationAsync(cboPickupLocation);
            btnAddDropoffLocation.Click += async (_, _) => await AddLocationAsync(cboDropoffLocation);

            // Cột "Loại hàng": gõ là hiện danh sách gợi ý (loại chuẩn + loại đã dùng),
            // vẫn gõ được loại mới. Khi lưu, chữ gõ khác hoa/thường sẽ được chuẩn hóa
            // về đúng tên trong danh sách -> thống kê theo loại hàng không bị lệch.
            gridCargoLines.EditingControlShowing += (_, e) =>
            {
                if (e.Control is not TextBox tb) return;
                var isCargoType = gridCargoLines.CurrentCell?.OwningColumn == colCargoType;
                tb.AutoCompleteMode = isCargoType ? AutoCompleteMode.SuggestAppend : AutoCompleteMode.None;
                tb.AutoCompleteSource = isCargoType ? AutoCompleteSource.CustomSource : AutoCompleteSource.None;
                if (isCargoType) tb.AutoCompleteCustomSource = _cargoTypeSuggestions;
            };

            gridCargoLines.AutoGenerateColumns = false;
            gridCargoLines.DataSource = _cargoLines;
            btnRemoveCargoLine.Click += (_, _) => RemoveSelectedCargoLine();

            Load += async (_, _) => await LoadInitialDataAsync();
        }

        /// <summary>Chế độ TẠO MỚI.</summary>
        public CreateOrderForm(ApiServices api) : this()
        {
            _api = api;
        }

        /// <summary>Chế độ SỬA ĐƠN — chỉ cho phép khi đơn còn Draft (Backend tự
        /// kiểm tra lại lần nữa, đây chỉ là kiểm tra sớm ở phía giao diện).</summary>
        public CreateOrderForm(ApiServices api, int editOrderId) : this()
        {
            _api = api;
            _editOrderId = editOrderId;
            Text = "Sửa đơn vận chuyển";
            cboCustomer.Enabled = false; // không cho đổi khách hàng của đơn đã tạo
        }

        private async Task LoadInitialDataAsync()
        {
            await LoadCustomersAsync();
            if (!_editOrderId.HasValue && cboCustomer.SelectedValue is int firstCustomerId)
                await LoadContractsAsync(firstCustomerId, null, null);
            await LoadLocationsAsync();
            await LoadWarehousesAsync();
            await LoadCargoTypesAsync();

            if (_editOrderId.HasValue)
                await LoadExistingOrderAsync(_editOrderId.Value);
            else
                AddBlankCargoLine(); // đơn mới -> có sẵn 1 dòng trống để nhập ngay, đỡ phải bấm thêm
        }

        /// <summary>Đổ dropdown chọn KHO TRUNG CHUYỂN — Dispatcher tự chọn kho
        /// phù hợp với TUYẾN ĐƯỜNG thực tế của đơn (VD: đơn ra Hà Nội thì chọn
        /// Kho Hà Nội), thay vì để hệ thống tự động gán kho đầu tiên có sẵn
        /// (sai thực tế, gây ra lộ trình vô lý không liên quan tới đơn hàng).
        /// Đánh dấu rõ kho nào CHƯA có tọa độ để tránh chọn nhầm rồi sau này
        /// không xem được lộ trình.</summary>
        private async Task LoadWarehousesAsync()
        {
            if (_api is null) return;
            try
            {
                var warehouses = await _api.Warehouses.GetWarehousesAsync();
                cboWarehouse.DataSource = warehouses.Select(w => new
                {
                    w.Id,
                    DisplayText = w.Lat.HasValue ? w.Name : $"{w.Name} (⚠️ chưa có tọa độ)",
                }).ToList();
                cboWarehouse.DisplayMember = "DisplayText";
                cboWarehouse.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Không tải được danh sách kho: " + ex.Message;
            }
        }

        private async Task LoadCustomersAsync()
        {
            if (_api is null) return;
            try
            {
                var customers = await _api.Customers.GetCustomersAsync();
                cboCustomer.DataSource = customers;
                cboCustomer.DisplayMember = nameof(CustomerListItem.Name);
                cboCustomer.ValueMember = nameof(CustomerListItem.Id);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Không tải được danh sách khách hàng: " + ex.Message;
            }
        }

        private const string SelfDeliveryPickupText = "Khách tự mang hàng tới kho";

        private async Task LoadLocationsAsync(int? selectPickupId = null, int? selectDropoffId = null)
        {
            if (_api is null) return;
            try
            {
                var keepPickup = selectPickupId ?? (cboPickupLocation.SelectedItem as LocationItem)?.Id;
                var keepDropoff = selectDropoffId ?? (cboDropoffLocation.SelectedItem as LocationItem)?.Id;

                _locations = (await _api.Locations.GetLocationsAsync()).OrderBy(l => l.Name).ToList();
                // Chỉ địa điểm CÓ TỌA ĐỘ mới chọn được (không có tọa độ thì không tính được lộ trình),
                // và là địa điểm dùng chung hoặc địa chỉ riêng của ĐÚNG khách đang chọn (server
                // từ chối địa chỉ riêng của khách khác).
                var customerId = cboCustomer.SelectedValue as int?;
                var usable = _locations
                    .Where(l => l.Lat.HasValue && l.Lng.HasValue)
                    .Where(l => l.CustomerId is null || l.CustomerId == customerId)
                    .ToList();

                _usableLocations = usable;
                _rebindingLocations = true;
                try
                {
                    cboPickupLocation.DataSource = LocationSource(cboPickupLocation, keepDropoff);
                    SelectLocation(cboPickupLocation, keepPickup);
                    cboDropoffLocation.DataSource = LocationSource(cboDropoffLocation, keepPickup);
                    SelectLocation(cboDropoffLocation, keepDropoff);
                }
                finally { _rebindingLocations = false; }
                RebindOtherLocationList(cboDropoffLocation);
                RebindOtherLocationList(cboPickupLocation);
                UpdatePickupForMethod();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Không tải được danh sách địa điểm đã lưu: " + ex.Message;
            }
        }

        /// <summary>Danh sách cho 1 dropdown: dòng "-- Chọn --" + địa điểm dùng được, BỎ địa điểm đang
        /// chọn ở dropdown kia (và địa điểm cùng vị trí với nó). Đơn khách tự mang tới thì không có
        /// điểm lấy -> điểm giao không bị loại gì.</summary>
        private List<LocationItem> LocationSource(ComboBox combo, int? excludeId)
        {
            var isPickup = combo == cboPickupLocation;
            var header = new LocationItem { Id = 0, Name = isPickup ? "-- Chọn địa điểm lấy hàng --" : "-- Chọn địa điểm giao hàng --", Address = "" };
            var exclude = excludeId is int id && id != 0 && !rbSelfDelivery.Checked
                ? _usableLocations.FirstOrDefault(l => l.Id == id) : null;
            var list = new List<LocationItem> { header };
            list.AddRange(_usableLocations.Where(l => exclude is null || !SamePlace(l, exclude)));
            return list;
        }

        /// <summary>Cùng địa điểm, hoặc 2 địa điểm khác tên nhưng lệch nhau dưới ~11 m.</summary>
        private static bool SamePlace(LocationItem a, LocationItem b)
            => a.Id == b.Id || (a.Lat is double alat && b.Lat is double blat && a.Lng is double alng && b.Lng is double blng
                                && Math.Abs(alat - blat) < 0.0001 && Math.Abs(alng - blng) < 0.0001);

        /// <summary>Đổ lại dropdown <paramref name="target"/> theo lựa chọn hiện tại của dropdown kia —
        /// giữ nguyên lựa chọn của target nếu vẫn hợp lệ.</summary>
        private void RebindOtherLocationList(ComboBox target)
        {
            if (_rebindingLocations || _usableLocations.Count == 0) return;
            var other = target == cboPickupLocation ? cboDropoffLocation : cboPickupLocation;
            var otherId = (other.SelectedItem as LocationItem)?.Id;
            var keep = (target.SelectedItem as LocationItem)?.Id;
            var source = LocationSource(target, otherId);
            if (target.DataSource is List<LocationItem> current && current.Select(l => l.Id).SequenceEqual(source.Select(l => l.Id))) return;
            _rebindingLocations = true;
            try
            {
                target.DataSource = source;
                SelectLocation(target, keep);
            }
            finally { _rebindingLocations = false; }
            // Lựa chọn cũ bị loại (trùng điểm bên kia) -> target về "-- Chọn --": cập nhật ô địa chỉ.
            if (target == cboDropoffLocation)
                txtDropoff.Text = cboDropoffLocation.SelectedItem is LocationItem d && d.Id != 0 ? d.Address : "";
            else if (!rbSelfDelivery.Checked)
                txtPickup.Text = cboPickupLocation.SelectedItem is LocationItem p && p.Id != 0 ? p.Address : "";
        }

        private static void SelectLocation(ComboBox combo, int? locationId)
        {
            if (combo.DataSource is not List<LocationItem> items) return;
            var index = locationId.HasValue ? items.FindIndex(l => l.Id == locationId.Value) : -1;
            combo.SelectedIndex = index >= 0 ? index : 0;
        }

        private void SetDeadline(DateTime date)
        {
            _settingDeadline = true;
            try { dtCommitted.Value = date; }
            finally { _settingDeadline = false; }
        }

        /// <summary>Gom các lần đổi liên tiếp (đổ dữ liệu, chọn nhanh) thành 1 lần gọi ước tính.</summary>
        private void RequestEstimate()
        {
            _estimateTimer.Stop();
            _estimateTimer.Start();
        }

        /// <summary>Gọi server ước tính số ngày chạy của tuyến (cùng cách tính với Chia xe) rồi gợi ý
        /// hạn giao = ngày giao sớm nhất + 1 ngày dự phòng. Người dùng đã tự chọn hạn thì chỉ cập
        /// nhật dòng gợi ý / cảnh báo, không ghi đè.</summary>
        private async Task SuggestDeadlineAsync()
        {
            if (_api is null) return;
            var dropoff = cboDropoffLocation.SelectedItem as LocationItem;
            var pickup = cboPickupLocation.SelectedItem as LocationItem;
            if (dropoff is null || dropoff.Id == 0 || cboWarehouse.SelectedValue is not int warehouseId
                || (!rbSelfDelivery.Checked && (pickup is null || pickup.Id == 0)))
            {
                _estimatedDays = null;
                UpdateDeadlineHint();
                return;
            }

            var version = ++_estimateVersion;
            lblDeadlineHint.Text = "🕒 Đang ước tính số ngày chạy của tuyến...";
            try
            {
                var days = await _api.TransportOrders.EstimateRouteDaysAsync(
                    rbSelfDelivery.Checked ? null : pickup!.Id, dropoff.Id, warehouseId,
                    rbSelfDelivery.Checked ? "SelfDelivery" : "CompanyPickup");
                if (version != _estimateVersion || IsDisposed) return; // đã đổi tiếp -> bỏ kết quả cũ
                _estimatedDays = days;
                if (!_deadlineTouched) // đơn mới, hoặc đơn cũ chưa có hạn giao
                    SetDeadline(EarliestDelivery(days).AddDays(DeadlineBufferDays));
            }
            catch
            {
                if (version != _estimateVersion || IsDisposed) return;
                _estimatedDays = null; // lỗi ước tính không chặn tạo đơn
            }
            UpdateDeadlineHint();
        }

        private static DateTime EarliestDelivery(int days) => DateTime.Today.AddDays(Math.Max(1, days) - 1);

        private void UpdateDeadlineHint()
        {
            if (_estimatedDays is not int days)
            {
                lblDeadlineHint.Text = "Chọn địa điểm lấy / giao và kho để hệ thống ước tính số ngày chạy và gợi ý hạn giao.";
                lblDeadlineHint.ForeColor = Theme.TextSecondary;
                return;
            }
            var earliest = EarliestDelivery(days);
            var suggested = earliest.AddDays(DeadlineBufferDays);
            var text = $"🕒 Tuyến ước tính ~{days} ngày chạy — nếu chia xe hôm nay, giao sớm nhất {earliest:dd/MM/yyyy}. " +
                       $"Gợi ý hạn giao: {suggested:dd/MM/yyyy} (đã cộng {DeadlineBufferDays} ngày dự phòng).";
            if (dtCommitted.Value.Date < earliest)
            {
                lblDeadlineHint.Text = text + "\n⚠️ Hạn giao đang chọn SỚM HƠN thời gian chạy ước tính — khi Chia xe sẽ bị cảnh báo trễ hạn.";
                lblDeadlineHint.ForeColor = Theme.Warning;
            }
            else
            {
                lblDeadlineHint.Text = text;
                lblDeadlineHint.ForeColor = Theme.TextSecondary;
            }
        }

        /// <summary>Khách tự mang hàng tới kho -> không cần điểm lấy hàng.</summary>
        private void UpdatePickupForMethod()
        {
            var self = rbSelfDelivery.Checked;
            cboPickupLocation.Enabled = btnAddPickupLocation.Enabled = !self;
            if (self) txtPickup.Text = SelfDeliveryPickupText;
            else txtPickup.Text = cboPickupLocation.SelectedItem is LocationItem loc && loc.Id != 0 ? loc.Address : "";
        }

        /// <summary>"＋ Địa điểm mới" — mở form tạo Địa điểm (tìm trên VietMap, có
        /// tọa độ), lưu xong tự chọn luôn địa điểm vừa tạo.</summary>
        private async Task AddLocationAsync(ComboBox target)
        {
            if (_api is null) return;
            var oldIds = _locations.Select(l => l.Id).ToHashSet();
            using var f = new TruckingSystem.Desktop.GUI_LocationManagement.CreateLocationForm(_api);
            if (f.ShowDialog(this) != DialogResult.OK) return;

            var keepPickup = (cboPickupLocation.SelectedItem as LocationItem)?.Id;
            var keepDropoff = (cboDropoffLocation.SelectedItem as LocationItem)?.Id;
            await LoadLocationsAsync(keepPickup, keepDropoff);

            var created = _locations.Where(l => !oldIds.Contains(l.Id)).OrderByDescending(l => l.Id).FirstOrDefault();
            if (created == null) return;
            SelectLocation(target, created.Id);
            if ((target.SelectedItem as LocationItem)?.Id != created.Id)
                lblStatus.Text = $"Địa điểm vừa tạo \"{created.Name}\" trùng vị trí với điểm đã chọn ở ô còn lại — không dùng được cho ô này.";
        }

        private async Task LoadCargoTypesAsync()
        {
            if (_api is null) return;
            try
            {
                _cargoTypes = await _api.Connection.GetAsync<List<string>>("/api/transport-orders/cargo-types") ?? new();
                _cargoTypeSuggestions.Clear();
                _cargoTypeSuggestions.AddRange(_cargoTypes.ToArray());
            }
            catch
            {
                // Không có gợi ý thì vẫn gõ tay được — không chặn việc tạo đơn.
            }
        }

        private async Task LoadExistingOrderAsync(int orderId)
        {
            if (_api is null) return;
            try
            {
                var order = await _api.TransportOrders.GetOrderDetailAsync(orderId);

                if (order.Status != "Draft")
                {
                    lblStatus.Text = "⚠️ Đơn này không còn ở trạng thái Nháp — không thể sửa. Đóng form này lại.";
                    btnSave.Enabled = false;
                    return;
                }

                cboCustomer.SelectedValue = order.CustomerId;
                await LoadContractsAsync(order.CustomerId, order.ContractId, order.ContractCode);
                rbCompanyPickup.Checked = order.PickupMethod == "CompanyPickup";
                rbSelfDelivery.Checked = order.PickupMethod == "SelfDelivery";
                // Đơn cũ: chọn lại đúng Địa điểm theo địa chỉ đã lưu. Không khớp
                // (đơn tạo bằng địa chỉ gõ tay trước đây) -> phải chọn lại.
                // Ưu tiên chọn đúng Địa điểm theo Id đã lưu; đơn cũ không có Id thì khớp theo địa chỉ.
                // Lọc lại địa điểm theo ĐÚNG khách của đơn rồi chọn lại địa điểm đã lưu.
                await LoadLocationsAsync(order.PickupLocationId ?? MatchLocationId(order.PickupPoint),
                                         order.DropoffLocationId ?? MatchLocationId(order.DropoffPoint));
                UpdatePickupForMethod();
                if ((cboDropoffLocation.SelectedItem as LocationItem)?.Id is null or 0
                    || (!rbSelfDelivery.Checked && (cboPickupLocation.SelectedItem as LocationItem)?.Id is null or 0))
                    lblStatus.Text = "⚠️ Đơn này dùng địa chỉ gõ tay (không có tọa độ) — hãy chọn lại Địa điểm lấy/giao trước khi lưu.";
                cboWarehouse.SelectedValue = order.WarehouseId;
                if (order.CommittedDeliveryDate.HasValue)
                {
                    SetDeadline(order.CommittedDeliveryDate.Value.Date);
                    _deadlineTouched = true; // giữ đúng hạn đã hẹn, không tự gợi ý đè
                }
                else
                {
                    lblStatus.Text = "⚠️ Đơn này chưa có Hạn giao cam kết — giờ bắt buộc, hãy chọn hạn giao trước khi lưu.";
                }

                _cargoLines.Clear();
                foreach (var c in order.CargoLines) _cargoLines.Add(c);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi tải đơn hàng: " + ex.Message;
            }
        }

        /// <summary>Dropdown "Hợp đồng": hợp đồng ĐANG HIỆU LỰC của khách (server lọc). Khách chỉ có 1
        /// hợp đồng -> chọn sẵn; không có -> "(Không theo hợp đồng)". Sửa đơn: giữ đúng hợp đồng đã
        /// lưu, kể cả khi hợp đồng đó nay đã hết hạn (lúc Xác nhận server sẽ báo để chọn lại).</summary>
        private async Task LoadContractsAsync(int customerId, int? selectId, string? selectCode)
        {
            if (_api is null) return;
            var version = ++_contractLoadVersion;
            var items = new List<OrderContractOption> { new() { Id = 0, Code = NoContractText } };
            try
            {
                var options = await _api.TransportOrders.GetContractOptionsAsync(customerId);
                if (version != _contractLoadVersion) return; // đã đổi sang khách khác
                items.AddRange(options);
                if (selectId is int keep && options.All(o => o.Id != keep))
                    items.Add(new OrderContractOption { Id = keep, Code = selectCode ?? $"#{keep}", Inactive = true });
            }
            catch (Exception ex)
            {
                if (version != _contractLoadVersion) return;
                lblStatus.Text = "Không tải được danh sách hợp đồng: " + ex.Message;
            }

            cboContract.BeginUpdate();
            cboContract.Items.Clear();
            foreach (var item in items) cboContract.Items.Add(item);
            cboContract.EndUpdate();
            var selected = selectId is int id ? items.FirstOrDefault(i => i.Id == id)
                : items.Count == 2 ? items[1]  // khách có đúng 1 hợp đồng đang hiệu lực -> chọn sẵn
                : null;
            cboContract.SelectedItem = selected ?? items[0];
        }

        private int? MatchLocationId(string? address)
        {
            if (string.IsNullOrWhiteSpace(address)) return null;
            return _locations.FirstOrDefault(l => l.Lat.HasValue &&
                string.Equals(l.Address.Trim(), address.Trim(), StringComparison.OrdinalIgnoreCase))?.Id;
        }

        private void AddBlankCargoLine()
        {
            _cargoLines.Add(new CargoLineItem { CargoType = "Hàng khô đóng kiện", PackageCount = 1, WeightKg = 100, VolumeM3 = 1, QtyTotal = 1 });
        }

        private void RemoveSelectedCargoLine()
        {
            if (gridCargoLines.CurrentRow is null) return;
            var index = gridCargoLines.CurrentRow.Index;
            if (index >= 0 && index < _cargoLines.Count) _cargoLines.RemoveAt(index);
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (cboCustomer.SelectedValue is null)
            {
                lblStatus.Text = "Vui lòng chọn khách hàng.";
                return;
            }
            var pickupLoc = cboPickupLocation.SelectedItem as LocationItem;
            var dropoffLoc = cboDropoffLocation.SelectedItem as LocationItem;
            if (!rbSelfDelivery.Checked && (pickupLoc is null || pickupLoc.Id == 0))
            {
                lblStatus.Text = "Vui lòng chọn Địa điểm lấy hàng (chưa có thì bấm \"＋ Địa điểm mới\").";
                return;
            }
            if (dropoffLoc is null || dropoffLoc.Id == 0)
            {
                lblStatus.Text = "Vui lòng chọn Địa điểm giao hàng (chưa có thì bấm \"＋ Địa điểm mới\").";
                return;
            }
            if (_cargoLines.Count == 0)
            {
                lblStatus.Text = "Vui lòng thêm ít nhất 1 dòng hàng hóa.";
                return;
            }
            if (cboWarehouse.SelectedValue is not int warehouseId)
            {
                lblStatus.Text = "Vui lòng chọn kho trung chuyển.";
                return;
            }
            if (_api is null) return;

            gridCargoLines.EndEdit(); // đảm bảo ô đang gõ dở được chốt lại trước khi đọc dữ liệu
            // Chuẩn hóa loại hàng: khớp (không phân biệt hoa/thường, khoảng trắng) với
            // tên trong danh sách gợi ý thì dùng đúng tên đó.
            foreach (var line in _cargoLines)
            {
                var typed = (line.CargoType ?? "").Trim();
                var canonical = _cargoTypes.FirstOrDefault(t => string.Equals(t, typed, StringComparison.OrdinalIgnoreCase));
                line.CargoType = canonical ?? typed;
            }
            var cargoLinesList = _cargoLines.ToList();
            var pickupMethod = rbSelfDelivery.Checked ? "SelfDelivery" : "CompanyPickup";
            int? pickupLocId = rbSelfDelivery.Checked || pickupLoc is null || pickupLoc.Id == 0 ? null : pickupLoc.Id;
            int? dropoffLocId = dropoffLoc.Id;
            // Chỉ lấy NGÀY (ô chọn chỉ hiện ngày; trước đây gửi kèm giờ lúc mở form -> lệch
            // quy tắc "hàng gấp trong 3 ngày" và giờ trên lịch).
            DateTime? committedDate = dtCommitted.Value.Date; // bắt buộc
            if (committedDate.Value < DateTime.Today && !_editOrderId.HasValue)
            {
                lblStatus.Text = "Hạn giao cam kết không được là ngày đã qua.";
                return;
            }

            int? contractId = cboContract.SelectedItem is OrderContractOption c && c.Id > 0 ? c.Id : null;

            btnSave.Enabled = false;
            try
            {
                if (_editOrderId.HasValue)
                {
                    var result = await _api.TransportOrders.UpdateTransportOrderAsync(
                        _editOrderId.Value, txtPickup.Text.Trim(), txtDropoff.Text.Trim(), warehouseId, committedDate,
                        cargoLinesList, pickupLocId, dropoffLocId, pickupMethod, contractId);

                    if (!result.Success)
                    {
                        lblStatus.Text = "⚠️ " + result.Message;
                        return;
                    }
                }
                else
                {
                    await _api.TransportOrders.CreateTransportOrderAsync(
                        (int)cboCustomer.SelectedValue, txtPickup.Text.Trim(), txtDropoff.Text.Trim(), warehouseId, committedDate,
                        cargoLinesList, pickupLocId, dropoffLocId, pickupMethod, contractId);
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi khi lưu: " + ex.Message;
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }
    }
}
