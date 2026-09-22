using TruckingSystem.Desktop.Common;

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
        }

        public CreateVehicleForm(ApiServices api) : this()
        {
            _api = api;
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
                    numCapacityQty.Value, numCapacityKg.Value, numCapacityM3.Value);

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
