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
        private bool _warehousesLoaded;
        private string _currentStatus = "";

        public EditVehicleForm()
        {
            InitializeComponent();
            // Server bắt buộc sức chứa > 0 ở cả 3 chiều — không cho chọn 0 ngay trên form.
            foreach (var n in new[] { numCapacityQty, numCapacityKg, numCapacityM3 })
                n.Minimum = n.DecimalPlaces == 0 ? 1 : 1m / (decimal)Math.Pow(10, n.DecimalPlaces);
            // Loại nhiên liệu nạp từ bảng giá ở màn Nhiên liệu (xem FuelTypeOptions) — không gán cứng.
            cboFuelType.Items.Add("");
        }

        public EditVehicleForm(ApiServices api, VehicleListItem vehicle) : this()
        {
            _api = api;
            _vehicleId = vehicle.Id;
            _currentWarehouseId = vehicle.WarehouseId;

            txtPlateNumber.Text = vehicle.PlateNumber;
            VehicleTypeOptions.Fill(cboVehicleType, vehicle.VehicleType);
            // Kẹp trong giới hạn ô nhập: xe cũ lưu sức chứa 0 (trước khi bắt buộc > 0) vẫn mở được để sửa.
            numCapacityQty.Value = Math.Clamp(vehicle.CapacityQty, numCapacityQty.Minimum, numCapacityQty.Maximum);
            numCapacityKg.Value = Math.Clamp(vehicle.CapacityKg, numCapacityKg.Minimum, numCapacityKg.Maximum);
            numCapacityM3.Value = Math.Clamp(vehicle.CapacityM3, numCapacityM3.Minimum, numCapacityM3.Maximum);
            // Đổi loại xe -> tự điền tải trọng/thể tích gợi ý (giữ nguyên số cũ nếu không đổi loại).
            cboVehicleType.SelectedIndexChanged += (_, _) =>
            {
                var preset = VehicleTypeOptions.Find(cboVehicleType.SelectedItem as string);
                if (preset is null) return;
                numCapacityKg.Value = Math.Min(numCapacityKg.Maximum, preset.CapacityKg);
                numCapacityM3.Value = Math.Min(numCapacityM3.Maximum, preset.CapacityM3);
            };
            FillStatusOptions(vehicle.Status);
            numFuelConsumption.Value = vehicle.FuelConsumptionPer100Km ?? 0;

            // Chờ nạp xong danh sách Kho + Loại nhiên liệu mới cho Lưu — trước đây bấm Lưu
            // sớm sẽ gửi Kho = trống và Loại nhiên liệu = trống, xóa mất dữ liệu cũ.
            btnSave.Enabled = false;
            Load += async (_, _) =>
            {
                await LoadWarehousesAsync();
                await FuelTypeOptions.FillAsync(cboFuelType, api, vehicle.FuelType, lblStatus);
                btnSave.Enabled = true;
            };
        }

        /// <summary>Trạng thái hiện tiếng Việt. "Đang chạy" do hệ thống tự gán khi Chia xe
        /// nên KHÔNG cho chọn tay — chỉ hiện khi xe đang ở trạng thái đó (để giữ nguyên).
        /// Xe đang chạy vẫn chuyển được sang Bảo trì / Ngưng hoạt động (xe hỏng giữa đường).</summary>
        private void FillStatusOptions(string current)
        {
            _currentStatus = current;
            cboStatus.Items.Clear();
            cboStatus.Items.Add(current == "OnTrip"
                ? new ComboOption("OnTrip", "Đang chạy (hệ thống gán)")
                : new ComboOption("Available", "Rảnh"));
            cboStatus.Items.Add(new ComboOption("UnderMaintenance", "Bảo trì"));
            cboStatus.Items.Add(new ComboOption("OutOfService", "Ngưng hoạt động"));
            var match = cboStatus.Items.Cast<ComboOption>().FirstOrDefault(o => o.Value == current);
            cboStatus.SelectedItem = match ?? cboStatus.Items[0];
        }

        /// <summary>Đổ dropdown "Kho đóng quân" và chọn sẵn kho hiện tại của xe.</summary>
        private async Task LoadWarehousesAsync()
        {
            if (_api is null) return;
            _warehousesLoaded = await WarehouseOptions.FillAsync(cboWarehouse, _api, _currentWarehouseId, lblStatus);
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPlateNumber.Text))
            {
                lblStatus.Text = "Vui lòng nhập biển số xe.";
                return;
            }
            // Loại xe, loại nhiên liệu, định mức là BẮT BUỘC (server cũng kiểm tra lại).
            if (string.IsNullOrWhiteSpace(cboVehicleType.SelectedItem as string))
            {
                lblStatus.Text = "Vui lòng chọn Loại xe.";
                return;
            }
            if (string.IsNullOrWhiteSpace(cboFuelType.SelectedItem as string))
            {
                lblStatus.Text = "Vui lòng chọn Loại nhiên liệu.";
                return;
            }
            if (numFuelConsumption.Value <= 0)
            {
                lblStatus.Text = "Vui lòng nhập Định mức nhiên liệu (lít/100 km) lớn hơn 0.";
                return;
            }
            if (_api is null) return;

            btnSave.Enabled = false;
            try
            {
                var result = await _api.Vehicles.UpdateVehicleAsync(
                    _vehicleId, txtPlateNumber.Text.Trim(), (cboVehicleType.SelectedItem as string ?? "").Trim(),
                    numCapacityQty.Value, numCapacityKg.Value, numCapacityM3.Value,
                    (cboStatus.SelectedItem as ComboOption)?.Value ?? _currentStatus,
                    numFuelConsumption.Value == 0 ? null : numFuelConsumption.Value,
                    string.IsNullOrEmpty((string)(cboFuelType.SelectedItem ?? "")) ? null : (string)cboFuelType.SelectedItem!,
                    _warehousesLoaded ? WarehouseOptions.Selected(cboWarehouse) : _currentWarehouseId);

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
