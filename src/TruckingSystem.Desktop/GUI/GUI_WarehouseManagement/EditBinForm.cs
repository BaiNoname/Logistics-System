using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_WarehouseManagement;

namespace TruckingSystem.Desktop.GUI_WarehouseManagement
{
    /// <summary>Sửa vị trí lưu trữ (StorageBin) đã có — nạp sẵn dữ liệu cũ.
    /// Backend tự chặn nếu sức chứa mới nhỏ hơn lượng hàng đang chứa, hoặc
    /// tọa độ mới đè lên vị trí khác / vượt ra ngoài khu vực.</summary>
    public partial class EditBinForm : BaseForm
    {
        private ApiServices? _api;
        private int _binId;

        public EditBinForm()
        {
            InitializeComponent();
        }

        public EditBinForm(ApiServices api, StorageBinItem bin, string zoneName, decimal zoneWidth, decimal zoneHeight) : this()
        {
            _api = api;
            _binId = bin.Id;
            lblZoneInfo.Text = $"Khu vực \"{zoneName}\" — kích thước {zoneWidth}m x {zoneHeight}m (X: 0–{zoneWidth}, Y: 0–{zoneHeight})";

            txtCode.Text = bin.Code;
            numCapacity.Value = bin.CapacityM3;
            numMaxWeight.Value = bin.MaxWeightKg;
            numPosX.Value = bin.PosX;
            numPosY.Value = bin.PosY;
            numWidth.Value = bin.WidthMeters;
            numHeight.Value = bin.HeightMeters;
            numDistanceFromDoor.Value = bin.DistanceFromDoorMeters;
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text))
            {
                lblStatus.Text = "Vui lòng nhập mã vị trí.";
                return;
            }
            if (numCapacity.Value <= 0)
            {
                lblStatus.Text = "Sức chứa phải lớn hơn 0.";
                return;
            }
            if (numWidth.Value <= 0 || numHeight.Value <= 0)
            {
                lblStatus.Text = "Chiều rộng/cao vị trí phải lớn hơn 0.";
                return;
            }
            if (_api is null) return;

            btnSave.Enabled = false;
            try
            {
                var result = await _api.Warehouses.UpdateBinAsync(
                    _binId, txtCode.Text.Trim(), numCapacity.Value, numMaxWeight.Value,
                    numPosX.Value, numPosY.Value, numWidth.Value, numHeight.Value, numDistanceFromDoor.Value);

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
