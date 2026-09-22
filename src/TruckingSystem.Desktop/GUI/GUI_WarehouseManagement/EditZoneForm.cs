using System.Xml.Linq;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_WarehouseManagement;

namespace TruckingSystem.Desktop.GUI_WarehouseManagement
{
    /// <summary>Sửa khu vực (Zone) đã có — nạp sẵn tọa độ/kích thước cũ.</summary>
    public partial class EditZoneForm : BaseForm
    {
        private ApiServices? _api;
        private int _zoneId;

        public EditZoneForm()
        {
            InitializeComponent();
            cboZoneType.Items.AddRange(new object[] { "Normal", "Fragile", "Hazmat" });
        }

        public EditZoneForm(ApiServices api, WarehouseZoneItem zone) : this()
        {
            _api = api;
            _zoneId = zone.Id;

            txtName.Text = zone.Name;
            cboZoneType.SelectedItem = zone.ZoneType;
            numPosX.Value = zone.PosX;
            numPosY.Value = zone.PosY;
            numWidth.Value = zone.WidthMeters;
            numHeight.Value = zone.HeightMeters;
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
                var result = await _api.Warehouses.UpdateZoneAsync(
                    _zoneId, txtName.Text.Trim(), (string)cboZoneType.SelectedItem!,
                    numPosX.Value, numPosY.Value, numWidth.Value, numHeight.Value);

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
                lblStatus.Text = "Lỗi: " + ex.Message;
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }
    }
}
