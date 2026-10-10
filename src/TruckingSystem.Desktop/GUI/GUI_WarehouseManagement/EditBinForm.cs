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
            // Nới giới hạn ô nhập nếu giá trị đang lưu vượt giới hạn mặc định (VD vị trí tạo qua
            // API) — trước đây gán thẳng làm form lỗi ngay khi mở.
            SetSafe(numCapacity, bin.CapacityM3);
            SetSafe(numMaxWeight, bin.MaxWeightKg);
            SetSafe(numPosX, bin.PosX);
            SetSafe(numPosY, bin.PosY);
            SetSafe(numWidth, bin.WidthMeters);
            SetSafe(numHeight, bin.HeightMeters);
            SetSafe(numDistanceFromDoor, bin.DistanceFromDoorMeters);
        }

        private static void SetSafe(NumericUpDown n, decimal value)
        {
            if (value > n.Maximum) n.Maximum = value;
            if (value < n.Minimum) n.Minimum = value;
            n.Value = value;
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
