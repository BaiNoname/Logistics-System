using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_TripManagement
{
    /// <summary>Form nhỏ ghi nhận 1 lần đổ xăng thực tế cho 1 chuyến cụ thể.</summary>
    public partial class RecordFuelForm : BaseForm
    {
        private ApiServices? _api;
        private int _tripId;

        public RecordFuelForm()
        {
            InitializeComponent();
        }

        public RecordFuelForm(ApiServices api, int tripId) : this()
        {
            _api = api;
            _tripId = tripId;
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (numLiters.Value <= 0 || numCost.Value <= 0)
            {
                lblStatus.Text = "Vui lòng nhập số lít và chi phí lớn hơn 0.";
                return;
            }
            if (_api is null) return;

            btnSave.Enabled = false;
            try
            {
                var result = await _api.Trips.RecordFuelAsync(_tripId, numLiters.Value, numCost.Value, txtStationName.Text.Trim());
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
