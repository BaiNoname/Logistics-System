using TruckingSystem.Desktop.Common;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace TruckingSystem.Desktop.GUI_WarehouseManagement
{
    /// <summary>Tạo vị trí lưu trữ (StorageBin) mới trong 1 khu vực — cần khai
    /// báo đúng Row/Column để hiện đúng ô trong lưới sơ đồ trực quan.</summary>
    public partial class CreateBinForm : BaseForm
    {
        private ApiServices? _api;
        private int _zoneId;

        public CreateBinForm()
        {
            InitializeComponent();
        }

        public CreateBinForm(ApiServices api, int zoneId) : this()
        {
            _api = api;
            _zoneId = zoneId;
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
            if (_api is null) return;

            btnSave.Enabled = false;
            try
            {
                await _api.Warehouses.CreateBinAsync(
                    _zoneId, txtCode.Text.Trim(), numCapacity.Value, numMaxWeight.Value,
                    (int)numRow.Value, (int)numColumn.Value, numDistanceFromDoor.Value);

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
