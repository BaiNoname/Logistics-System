using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_WarehouseManagement;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace TruckingSystem.Desktop.GUI_WarehouseManagement
{
    /// <summary>Sửa vị trí lưu trữ (StorageBin) đã có — nạp sẵn dữ liệu cũ.
    /// Backend tự chặn nếu sức chứa mới nhỏ hơn lượng hàng đang chứa.</summary>
    public partial class EditBinForm : BaseForm
    {
        private ApiServices? _api;
        private int _binId;

        public EditBinForm()
        {
            InitializeComponent();
        }

        public EditBinForm(ApiServices api, StorageBinItem bin) : this()
        {
            _api = api;
            _binId = bin.Id;

            txtCode.Text = bin.Code;
            numCapacity.Value = bin.CapacityM3;
            numMaxWeight.Value = bin.MaxWeightKg;
            numRow.Value = bin.Row;
            numColumn.Value = bin.Column;
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
            if (_api is null) return;

            btnSave.Enabled = false;
            try
            {
                var result = await _api.Warehouses.UpdateBinAsync(
                    _binId, txtCode.Text.Trim(), numCapacity.Value, numMaxWeight.Value,
                    (int)numRow.Value, (int)numColumn.Value, numDistanceFromDoor.Value);

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
