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
        private readonly AutoCompleteStringCollection _cargoTypeSuggestions = new();
        private List<string> _cargoTypes = new();

        public CreateOrderForm()
        {
            InitializeComponent();

            dtCommitted.Enabled = false;
            chkHasCommittedDate.CheckedChanged += (_, _) => dtCommitted.Enabled = chkHasCommittedDate.Checked;

            // Điểm lấy/giao BẮT BUỘC chọn từ danh mục Địa điểm (có tọa độ) — ô địa
            // chỉ chỉ để hiển thị, không gõ tay nữa (địa chỉ gõ tay không có tọa độ ->
            // bản đồ, ước tính quãng đường/số ngày/nhiên liệu đều không tính được).
            cboPickupLocation.SelectedIndexChanged += (_, _) =>
                txtPickup.Text = cboPickupLocation.SelectedItem is LocationItem loc && loc.Id != 0 ? loc.Address
                    : rbSelfDelivery.Checked ? SelfDeliveryPickupText : "";
            cboDropoffLocation.SelectedIndexChanged += (_, _) =>
                txtDropoff.Text = cboDropoffLocation.SelectedItem is LocationItem loc && loc.Id != 0 ? loc.Address : "";
            rbSelfDelivery.CheckedChanged += (_, _) => UpdatePickupForMethod();
            // Đổi khách hàng -> lọc lại địa điểm (địa chỉ riêng của khách khác không được dùng).
            // (Chế độ Sửa đơn: không đổi được khách; danh sách được lọc 1 lần trong LoadExistingOrderAsync.)
            cboCustomer.SelectedIndexChanged += async (_, _) => { if (_locations.Count > 0 && !_editOrderId.HasValue) await LoadLocationsAsync(); };
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

                var pickupSource = new List<LocationItem> { new() { Id = 0, Name = "-- Chọn địa điểm lấy hàng --", Address = "" } };
                pickupSource.AddRange(usable);
                var dropoffSource = new List<LocationItem> { new() { Id = 0, Name = "-- Chọn địa điểm giao hàng --", Address = "" } };
                dropoffSource.AddRange(usable);

                cboPickupLocation.DataSource = pickupSource;
                cboDropoffLocation.DataSource = dropoffSource;
                SelectLocation(cboPickupLocation, keepPickup);
                SelectLocation(cboDropoffLocation, keepDropoff);
                UpdatePickupForMethod();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Không tải được danh sách địa điểm đã lưu: " + ex.Message;
            }
        }

        private static void SelectLocation(ComboBox combo, int? locationId)
        {
            if (combo.DataSource is not List<LocationItem> items) return;
            var index = locationId.HasValue ? items.FindIndex(l => l.Id == locationId.Value) : -1;
            combo.SelectedIndex = index >= 0 ? index : 0;
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
            if (created != null) SelectLocation(target, created.Id);
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
                    chkHasCommittedDate.Checked = true;
                    dtCommitted.Value = order.CommittedDeliveryDate.Value;
                }

                _cargoLines.Clear();
                foreach (var c in order.CargoLines) _cargoLines.Add(c);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi tải đơn hàng: " + ex.Message;
            }
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
            var committedDate = chkHasCommittedDate.Checked ? dtCommitted.Value.Date : (DateTime?)null;
            if (committedDate.HasValue && committedDate.Value < DateTime.Today && !_editOrderId.HasValue)
            {
                lblStatus.Text = "Hạn giao cam kết không được là ngày đã qua.";
                return;
            }

            btnSave.Enabled = false;
            try
            {
                if (_editOrderId.HasValue)
                {
                    var result = await _api.TransportOrders.UpdateTransportOrderAsync(
                        _editOrderId.Value, txtPickup.Text.Trim(), txtDropoff.Text.Trim(), warehouseId, committedDate,
                        cargoLinesList, pickupLocId, dropoffLocId, pickupMethod);

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
                        cargoLinesList, pickupLocId, dropoffLocId, pickupMethod);
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
