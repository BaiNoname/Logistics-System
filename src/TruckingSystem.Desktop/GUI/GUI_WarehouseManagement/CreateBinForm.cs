using TruckingSystem.Desktop.Common;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace TruckingSystem.Desktop.GUI_WarehouseManagement
{
    /// <summary>Tạo vị trí lưu trữ (StorageBin) mới trong 1 khu vực — khai báo
    /// tọa độ + kích thước THẬT (mét), tương tự cách Khu vực định vị trong
    /// Kho. Backend tự kiểm tra không đè lên vị trí khác và không vượt ra
    /// ngoài ranh giới khu vực.</summary>
    public partial class CreateBinForm : BaseForm
    {
        private ApiServices? _api;
        private int _zoneId;

        public CreateBinForm()
        {
            InitializeComponent();
        }

        public CreateBinForm(ApiServices api, int zoneId, string zoneName, decimal zoneWidth, decimal zoneHeight) : this()
        {
            _api = api;
            _zoneId = zoneId;
            lblZoneInfo.Text = $"Khu vực \"{zoneName}\" — kích thước {zoneWidth}m x {zoneHeight}m (X: 0–{zoneWidth}, Y: 0–{zoneHeight})";
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
                await _api.Warehouses.CreateBinAsync(
                    _zoneId, txtCode.Text.Trim(), numCapacity.Value, numMaxWeight.Value,
                    numPosX.Value, numPosY.Value, numWidth.Value, numHeight.Value, numDistanceFromDoor.Value);

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
