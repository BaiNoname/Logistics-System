using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_CustomerManagement
{
    /// <summary>Logic xử lý — giao diện nằm ở CreateCustomerForm.Designer.cs.</summary>
    public partial class CreateCustomerForm : BaseForm
    {
        private ApiServices? _api;
        private string selectedImagePath;// dùng để lưu đường dẫn tệp ảnh danh thiếp được chọn

        public CreateCustomerForm()
        {
            InitializeComponent();
            cboType.Items.AddRange(new object[] { "Individual", "Company" });
            cboType.SelectedIndex = 0;
        }

        public CreateCustomerForm(ApiServices api) : this()
        {
            _api = api;
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                lblStatus.Text = "Vui lòng nhập tên khách hàng.";
                return;
            }
            if (_api is null) return;

            try
            {
                await _api.Customers.CreateCustomerAsync(
                    txtName.Text.Trim(), txtPhone.Text.Trim(), txtTaxCode.Text.Trim(), (string)cboType.SelectedItem!,
                    string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text.Trim(),
                    numCreditLimit.Value == 0 ? null : numCreditLimit.Value,
                    string.IsNullOrWhiteSpace(txtGoogleCalendarId.Text) ? null : txtGoogleCalendarId.Text.Trim());
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
            }
        }

        private void btnReadWithAi_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Chọn ảnh danh thiếp";
                dialog.Filter = "Ảnh danh thiếp|*.jpg;*.jpeg;*.png;*.bmp|Tất cả tệp|*.*";
                if (dialog.ShowDialog() != DialogResult.OK) return;
                //Phần bên dưới chưa cần dùng tới hoặc không cần vì dùng để hiển thị ảnh vừa chọn và hiện thị trạng thái ra một label
                //selectedImagePath = dialog.FileName;
                //using (var source = Image.FromFile(selectedImagePath))
                //{
                //    if (pictureCard.Image != null) pictureCard.Image.Dispose();
                //    pictureCard.Image = new Bitmap(source);
                //}
                //lblFileName.Text = Path.GetFileName(selectedImagePath);
                //lblStatus.Text = "Ảnh đã sẵn sàng. Nhấn “Đọc bằng AI” khi đã cấu hình API.";
            }
        }
    }
}
