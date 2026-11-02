using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_FuelManagement
{
    public partial class CreateFuelProviderForm : BaseForm
    {
        private ApiServices? _api;

        public CreateFuelProviderForm()
        {
            InitializeComponent();
            TruckingSystem.Desktop.Common.DialogKeys.Enable(this, btnSave); // Esc = đóng, Enter = Lưu
        }

        public CreateFuelProviderForm(ApiServices api) : this()
        {
            _api = api;
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                FieldHint.Show(txtName, lblStatus, "Vui lòng nhập tên hãng xăng."); // tô đỏ + nhảy vào đúng ô
                return;
            }
            if (_api is null) return;

            btnSave.Enabled = false;
            try
            {
                await _api.Fuel.CreateProviderAsync(
                    txtName.Text.Trim(),
                    string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim(),
                    string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text.Trim());

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
