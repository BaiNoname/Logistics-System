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
            cboFuelType.Items.AddRange(new object[] { "", "Dầu DO", "Xăng RON95", "Xăng RON92", "Xăng E5" });
            cboFuelType.SelectedIndex = 0;
        }

        public CreateVehicleForm(ApiServices api) : this()
        {
            _api = api;
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
            if (_api is null) return; // đang ở chế độ xem Designer, không có API thật

            btnSave.Enabled = false;
            try
            {
                await _api.Vehicles.CreateVehicleAsync(
                    txtPlateNumber.Text.Trim(), txtVehicleType.Text.Trim(),
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
