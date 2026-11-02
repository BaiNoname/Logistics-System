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
            TruckingSystem.Desktop.Common.DialogFit.Enable(this, root); // vừa màn hình mọi máy, bảng không bị ép mất chữ
            TruckingSystem.Desktop.Common.DialogKeys.Enable(this, btnSave); // Esc = đóng, Enter = Lưu
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
            // Sơ đồ xem trước + "Tự tìm chỗ trống" (mẫu ⑧).
            PlacementSetup.Zone(this, inputs, numPosX, numPosY, numWidth, numHeight, lblPosX, lblPosY, lblWidth, lblHeight, lblNote,
                btnSave, () => txtName.Text.Trim(), api, warehouseId, null);
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                FieldHint.Show(txtName, lblStatus, "Vui lòng nhập tên khu vực."); // tô đỏ + nhảy vào đúng ô
                return;
            }
            if (numWidth.Value <= 0 || numHeight.Value <= 0)
            {
                FieldHint.Show(numWidth, lblStatus, "Chiều rộng/cao phải lớn hơn 0."); // tô đỏ + nhảy vào đúng ô
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
