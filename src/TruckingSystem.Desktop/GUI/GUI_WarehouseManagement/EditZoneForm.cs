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
            // Hiện tiếng Việt, gửi đúng mã cho server (trước đây hiện "Normal/Fragile/Hazmat").
            cboZoneType.Items.AddRange(new object[]
            {
                new ComboOption("Normal", "Hàng thường"),
                new ComboOption("Fragile", "Hàng dễ vỡ"),
                new ComboOption("Hazmat", "Hàng nguy hiểm"),
            });
        }

        public EditZoneForm(ApiServices api, WarehouseZoneItem zone) : this()
        {
            _api = api;
            _zoneId = zone.Id;

            txtName.Text = zone.Name;
            cboZoneType.SelectedItem = cboZoneType.Items.Cast<ComboOption>().FirstOrDefault(o => o.Value == zone.ZoneType)
                                       ?? cboZoneType.Items[0];
            // SetSafe: số đã lưu vượt giới hạn ô nhập (dữ liệu cũ / nhập qua API) -> nới giới hạn
            // thay vì crash (ArgumentOutOfRangeException) ngay khi mở form.
            SetSafe(numPosX, zone.PosX);
            SetSafe(numPosY, zone.PosY);
            SetSafe(numWidth, zone.WidthMeters);
            SetSafe(numHeight, zone.HeightMeters);
        }

        private static void SetSafe(NumericUpDown n, decimal value)
        {
            if (value > n.Maximum) n.Maximum = value;
            if (value < n.Minimum) n.Minimum = value;
            n.Value = value;
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
                    _zoneId, txtName.Text.Trim(), ((ComboOption)cboZoneType.SelectedItem!).Value,
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
