using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_FuelManagement;

namespace TruckingSystem.Desktop.GUI_FuelManagement
{
    public partial class CreateFuelPriceForm : BaseForm
    {
        private ApiServices? _api;

        public CreateFuelPriceForm()
        {
            InitializeComponent();
            cboFuelType.Items.AddRange(new object[] { "Dầu DO", "Xăng RON95", "Xăng RON92", "Xăng E5" });
            cboFuelType.SelectedIndex = 0;
            dtEffectiveDate.Value = DateTime.Today;
        }

        public CreateFuelPriceForm(ApiServices api, List<FuelProviderItem> providers) : this()
        {
            _api = api;

            var options = new List<FuelProviderItem> { new FuelProviderItem { Id = 0, Name = "(Giá thị trường chung — không theo hãng cụ thể)" } };
            options.AddRange(providers);
            cboProvider.DataSource = options;
            cboProvider.DisplayMember = nameof(FuelProviderItem.Name);
            cboProvider.ValueMember = nameof(FuelProviderItem.Id);
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (numPrice.Value <= 0)
            {
                lblStatus.Text = "Giá phải lớn hơn 0.";
                return;
            }
            if (_api is null) return;

            int? providerId = cboProvider.SelectedValue is int pid && pid != 0 ? pid : null;

            btnSave.Enabled = false;
            try
            {
                await _api.Fuel.CreatePriceAsync((string)cboFuelType.SelectedItem!, numPrice.Value, dtEffectiveDate.Value, providerId);

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
