using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_TripManagement
{
    public partial class CreateIncidentForm : BaseForm
    {
        private ApiServices? _api;
        private int _tripId;

        public CreateIncidentForm()
        {
            InitializeComponent();
            cboType.Items.AddRange(new object[]
            {
                new ComboOption("Accident", "Tai nạn"),
                new ComboOption("Breakdown", "Hỏng xe"),
                new ComboOption("TrafficViolation", "Vi phạm giao thông"),
                new ComboOption("Other", "Khác"),
            });
            cboType.SelectedIndex = 0;
        }

        public CreateIncidentForm(ApiServices api, int tripId) : this()
        {
            _api = api;
            _tripId = tripId;
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;
            if (txtDescription.Text.Trim().Length < 5)
            {
                lblStatus.Text = "Vui lòng mô tả sự cố (ít nhất 5 ký tự) — VD \"Thủng lốp sau trên QL1A, Km 1520\".";
                return;
            }

            btnSave.Enabled = false;
            try
            {
                await _api.Trips.CreateIncidentAsync(
                    _tripId, (cboType.SelectedItem as ComboOption)?.Value ?? "Other",
                    string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim(),
                    numCost.Value == 0 ? null : numCost.Value);

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
