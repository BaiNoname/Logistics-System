using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_WarehouseManagement;

namespace TruckingSystem.Desktop.GUI_VehicleManagement
{
    /// <summary>
    /// Phần LOGIC — xử lý dữ liệu, gọi API. Phần GIAO DIỆN (vị trí, kích thước
    /// từng control) nằm riêng ở file CreateVehicleForm.Designer.cs — mở file đó
    /// bằng Visual Studio (View Designer) để kéo-thả chỉnh sửa trực quan, KHÔNG
    /// cần sửa code tay nữa.
    /// </summary>
    public partial class CreateVehicleForm : BaseForm
    {
        private ApiServices? _api;

        /// <summary>Constructor KHÔNG tham số — bắt buộc phải có để Visual Studio
        /// Designer mở được giao diện kéo-thả (Designer tự tạo Form để hiển thị
        /// xem trước, không truyền được ApiServices thật vào lúc thiết kế).</summary>
        public CreateVehicleForm()
        {
            InitializeComponent();
            // Server bắt buộc sức chứa > 0 ở cả 3 chiều — không cho chọn 0 ngay trên form.
            foreach (var n in new[] { numCapacityQty, numCapacityKg, numCapacityM3 })
                n.Minimum = n.DecimalPlaces == 0 ? 1 : 1m / (decimal)Math.Pow(10, n.DecimalPlaces);
            // Loại nhiên liệu nạp từ bảng giá ở màn Nhiên liệu (xem FuelTypeOptions) — không gán cứng.
            cboFuelType.Items.Add("");
            cboFuelType.SelectedIndex = 0;

            // Loại xe chọn từ danh mục (VehicleTypeOptions) — chọn là tự điền tải trọng/thể tích gợi ý.
            VehicleTypeOptions.Fill(cboVehicleType, null);
            ApplyVehicleTypePreset();
            cboVehicleType.SelectedIndexChanged += (_, _) => ApplyVehicleTypePreset();
        }

        private void ApplyVehicleTypePreset()
        {
            var preset = VehicleTypeOptions.Find(cboVehicleType.SelectedItem as string);
            if (preset is null) return;
            numCapacityKg.Value = Math.Min(numCapacityKg.Maximum, preset.CapacityKg);
            numCapacityM3.Value = Math.Min(numCapacityM3.Maximum, preset.CapacityM3);
        }

        public CreateVehicleForm(ApiServices api) : this()
        {
            _api = api;
            Load += async (_, _) =>
            {
                await LoadWarehousesAsync();
                await FuelTypeOptions.FillAsync(cboFuelType, api, null, lblStatus);
            };
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
            if (_api is null) return; // đang ở chế độ xem Designer, không có API thật

            btnSave.Enabled = false;
            try
            {
                await _api.Vehicles.CreateVehicleAsync(
                    txtPlateNumber.Text.Trim(), (cboVehicleType.SelectedItem as string ?? "").Trim(),
                    numCapacityQty.Value, numCapacityKg.Value, numCapacityM3.Value,
                    numFuelConsumption.Value == 0 ? null : numFuelConsumption.Value,
                    string.IsNullOrEmpty((string)cboFuelType.SelectedItem!) ? null : (string)cboFuelType.SelectedItem!,
                    cboWarehouse.SelectedValue as int?);

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
