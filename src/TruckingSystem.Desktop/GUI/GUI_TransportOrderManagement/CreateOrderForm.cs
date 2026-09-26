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

        public CreateOrderForm()
        {
            InitializeComponent();

            dtCommitted.Enabled = false;
            chkHasCommittedDate.CheckedChanged += (_, _) => dtCommitted.Enabled = chkHasCommittedDate.Checked;

            cboPickupLocation.SelectedIndexChanged += (_, _) =>
            {
                if (cboPickupLocation.SelectedItem is LocationItem loc && loc.Id != 0) txtPickup.Text = loc.Address;
            };
            cboDropoffLocation.SelectedIndexChanged += (_, _) =>
            {
                if (cboDropoffLocation.SelectedItem is LocationItem loc && loc.Id != 0) txtDropoff.Text = loc.Address;
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

        private async Task LoadLocationsAsync()
        {
            if (_api is null) return;
            try
            {
                _locations = await _api.Locations.GetLocationsAsync();
                var blank = new LocationItem { Id = 0, Name = "-- Không chọn, tự gõ --", Address = "" };

                var pickupSource = new List<LocationItem> { blank };
                pickupSource.AddRange(_locations);
                var dropoffSource = new List<LocationItem> { blank };
                dropoffSource.AddRange(_locations);

                cboPickupLocation.DataSource = pickupSource;
                cboDropoffLocation.DataSource = dropoffSource;
                cboPickupLocation.SelectedIndex = 0;
                cboDropoffLocation.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Không tải được danh sách địa điểm đã lưu: " + ex.Message;
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
                txtPickup.Text = order.PickupPoint;
                txtDropoff.Text = order.DropoffPoint;
                cboWarehouse.SelectedValue = order.WarehouseId;
                if (order.CommittedDeliveryDate.HasValue)
                {
                    chkHasCommittedDate.Checked = true;
                    dtCommitted.Value = order.CommittedDeliveryDate.Value;
                }
                rbCompanyPickup.Checked = order.PickupMethod == "CompanyPickup";
                rbSelfDelivery.Checked = order.PickupMethod == "SelfDelivery";

                _cargoLines.Clear();
                foreach (var c in order.CargoLines) _cargoLines.Add(c);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi tải đơn hàng: " + ex.Message;
            }
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
            if (string.IsNullOrWhiteSpace(txtPickup.Text) || string.IsNullOrWhiteSpace(txtDropoff.Text))
            {
                lblStatus.Text = "Vui lòng nhập đủ điểm lấy/giao hàng.";
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
            var cargoLinesList = _cargoLines.ToList();
            var pickupMethod = rbSelfDelivery.Checked ? "SelfDelivery" : "CompanyPickup";
            int? pickupLocId = (cboPickupLocation.SelectedItem as LocationItem)?.Id is int pid && pid != 0 ? pid : null;
            int? dropoffLocId = (cboDropoffLocation.SelectedItem as LocationItem)?.Id is int did && did != 0 ? did : null;
            var committedDate = chkHasCommittedDate.Checked ? dtCommitted.Value : (DateTime?)null;

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
