using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_VehicleManagement
{
    public partial class CreateVehicleDocumentForm : BaseForm
    {
        private ApiServices? _api;
        private int _vehicleId;

        public CreateVehicleDocumentForm()
        {
            InitializeComponent();
            cboDocType.Items.AddRange(new object[] { "Registration", "Insurance", "TransportPermit", "Other" });
            cboDocType.SelectedIndex = 0;
            dtIssueDate.Value = DateTime.Today;
            dtExpiryDate.Value = DateTime.Today.AddYears(1);
        }

        public CreateVehicleDocumentForm(ApiServices api, int vehicleId) : this()
        {
            _api = api;
            _vehicleId = vehicleId;
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (dtExpiryDate.Value <= dtIssueDate.Value)
            {
                lblStatus.Text = "Ngày hết hạn phải sau ngày cấp.";
                return;
            }
            if (_api is null) return;

            btnSave.Enabled = false;
            try
            {
                await _api.Vehicles.CreateDocumentAsync(
                    _vehicleId, (string)cboDocType.SelectedItem!, dtIssueDate.Value, dtExpiryDate.Value,
                    string.IsNullOrWhiteSpace(txtFileUrl.Text) ? null : txtFileUrl.Text.Trim());

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