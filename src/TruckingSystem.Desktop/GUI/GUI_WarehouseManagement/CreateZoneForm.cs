using System.Xml.Linq;
using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_WarehouseManagement
{
    /// <summary>Tạo khu vực (Zone) mới trong 1 kho — cần khai báo đúng vị trí
    /// (PosX, PosY) và kích thước (WidthMeters, HeightMeters) thật để vẽ đúng
    /// chỗ trong sơ đồ tổng quan của kho.</summary>
    public partial class CreateZoneForm : BaseForm
    {
        private ApiServices? _api;
        private int _warehouseId;

        public CreateZoneForm()
        {
            InitializeComponent();
            // Hiện tiếng Việt, gửi đúng mã cho server (trước đây hiện "Normal/Fragile/Hazmat").
            cboZoneType.Items.AddRange(new object[]
            {
                new ComboOption("Normal", "Hàng thường"),
                new ComboOption("Fragile", "Hàng dễ vỡ"),
                new ComboOption("Hazmat", "Hàng nguy hiểm"),
            });
            cboZoneType.SelectedIndex = 0;
        }

        public CreateZoneForm(ApiServices api, int warehouseId) : this()
        {
            _api = api;
            _warehouseId = warehouseId;
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                lblStatus.Text = "Vui lòng nhập tên khu vực.";
                return;
            }
            if (numWidth.Value <= 0 || numHeight.Value <= 0)
            {
                lblStatus.Text = "Chiều rộng/cao phải lớn hơn 0.";
                return;
            }
            if (_api is null) return;

            btnSave.Enabled = false;
            try
            {
                await _api.Warehouses.CreateZoneAsync(
                    _warehouseId, txtName.Text.Trim(), ((ComboOption)cboZoneType.SelectedItem!).Value,
                    numPosX.Value, numPosY.Value, numWidth.Value, numHeight.Value);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }
    }
}
