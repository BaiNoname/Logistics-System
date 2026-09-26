using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_VehicleManagement;
using TruckingSystem.Desktop.DTO_WarehouseManagement;

namespace TruckingSystem.Desktop.GUI_VehicleManagement
{
    /// <summary>Sửa thông tin xe đã có sẵn — khác CreateVehicleForm ở chỗ nạp sẵn
    /// dữ liệu cũ và cho đổi cả Trạng thái thủ công (VD: đánh dấu đang bảo dưỡng
    /// — Greedy sẽ tự loại trừ xe không phải Available khi chia xe).</summary>
    public partial class EditVehicleForm : BaseForm
    {
        private ApiServices? _api;
        private int _vehicleId;
        private int? _currentWarehouseId;

        public EditVehicleForm()
        {
            InitializeComponent();
            cboStatus.Items.AddRange(new object[] { "Available", "OnTrip", "UnderMaintenance", "OutOfService" });
            cboFuelType.Items.AddRange(new object[] { "", "Dầu DO", "Xăng RON95", "Xăng RON92", "Xăng E5" });
        }

        public EditVehicleForm(ApiServices api, VehicleListItem vehicle) : this()
        {
            _api = api;
            _vehicleId = vehicle.Id;
            _currentWarehouseId = vehicle.WarehouseId;

            txtPlateNumber.Text = vehicle.PlateNumber;
            txtVehicleType.Text = vehicle.VehicleType;
            numCapacityQty.Value = vehicle.CapacityQty;
            numCapacityKg.Value = vehicle.CapacityKg;
            numCapacityM3.Value = vehicle.CapacityM3;
            cboStatus.SelectedItem = vehicle.Status;
            numFuelConsumption.Value = vehicle.FuelConsumptionPer100Km ?? 0;
            cboFuelType.SelectedItem = vehicle.FuelType ?? "";
            Load += async (_, _) => await LoadWarehousesAsync();
        }

        /// <summary>Đổ dropdown "Kho đóng quân" — để tuỳ chọn (có thể để trống,
        /// nghĩa là xe CHƯA gán kho cố định, vẫn dùng được cho mọi đơn).</summary>
        private async Task LoadWarehousesAsync()
        {
            if (_api is null) return;
            try
            {
                var warehouses = await _api.Warehouses.GetWarehousesAsync();
                var items = new List<object> { new { Id = (int?)null, Name = "(Không cố định — dùng được mọi kho)" } };
                items.AddRange(warehouses.Select(w => new { Id = (int?)w.Id, w.Name }));
                cboWarehouse.DataSource = items;
                cboWarehouse.DisplayMember = "Name";
                cboWarehouse.ValueMember = "Id";
                cboWarehouse.SelectedValue = _currentWarehouseId;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Không tải được danh sách kho: " + ex.Message;
            }
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPlateNumber.Text))
            {
                lblStatus.Text = "Vui lòng nhập biển số xe.";
                return;
            }
            if (_api is null) return;

            btnSave.Enabled = false;
            try
            {
                var result = await _api.Vehicles.UpdateVehicleAsync(
                    _vehicleId, txtPlateNumber.Text.Trim(), txtVehicleType.Text.Trim(),
                    numCapacityQty.Value, numCapacityKg.Value, numCapacityM3.Value,
                    (string)cboStatus.SelectedItem!,
                    numFuelConsumption.Value == 0 ? null : numFuelConsumption.Value,
                    string.IsNullOrEmpty((string)(cboFuelType.SelectedItem ?? "")) ? null : (string)cboFuelType.SelectedItem!,
                    cboWarehouse.SelectedValue as int?);

                if (!result.Success)
                {
                    lblStatus.Text = "⚠️ " + result.Message;
                    return;
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
