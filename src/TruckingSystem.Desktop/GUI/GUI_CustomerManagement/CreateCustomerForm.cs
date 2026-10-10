using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_CustomerManagement
{
    /// <summary>Logic xử lý — giao diện nằm ở CreateCustomerForm.Designer.cs.</summary>
    public partial class CreateCustomerForm : BaseForm
    {
        private ApiServices? _api;
        private CancellationTokenSource? _cardScanCancellation;

        public CreateCustomerForm()
        {
            InitializeComponent();
            // Hiện tiếng Việt (khớp danh sách khách hàng), gửi đúng mã cho server.
            cboType.Items.AddRange(new object[]
            {
                new ComboOption("Individual", "Cá nhân"),
                new ComboOption("Company", "Doanh nghiệp"),
            });
            cboType.SelectedIndex = 0;
        }

        public CreateCustomerForm(ApiServices api) : this()
        {
            _api = api;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _cardScanCancellation?.Cancel();
            base.OnFormClosed(e);
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                lblStatus.Text = "Vui lòng nhập tên khách hàng.";
                MessageBox.Show(this, lblStatus.Text, "Thiếu thông tin khách hàng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_api is null) return;

            try
            {
                var result = await _api.Customers.CreateCustomerAsync(
                    txtName.Text.Trim(), txtPhone.Text.Trim(), txtTaxCode.Text.Trim(), ((ComboOption)cboType.SelectedItem!).Value,
                    string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text.Trim(),
                    numCreditLimit.Value == 0 ? null : numCreditLimit.Value,
                    string.IsNullOrWhiteSpace(txtGoogleCalendarId.Text) ? null : txtGoogleCalendarId.Text.Trim());
                if (IsDisposed || Disposing) return;

                if (!result.Success)
                {
                    lblStatus.Text = "⚠️ " + result.Message;
                    MessageBox.Show(this, result.Message, "Không thể lưu khách hàng", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                if (IsDisposed || Disposing) return;
                lblStatus.Text = "Lỗi: " + ex.Message;
                MessageBox.Show(this, ex.Message, "Lỗi lưu khách hàng", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnReadWithAi_Click(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Chọn ảnh danh thiếp",
                Filter = "Ảnh danh thiếp|*.jpg;*.jpeg;*.png"
            };
            if (dialog.ShowDialog() != DialogResult.OK) return;
            if (_api is null)
            {
                MessageBox.Show(this, "Không kết nối được dịch vụ khách hàng.", "Lỗi đọc danh thiếp", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using var scanCancellation = new CancellationTokenSource();
            _cardScanCancellation = scanCancellation;
            SetCardScanning(true);
            lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
            lblStatus.Text = "Đang đọc danh thiếp...";
            try
            {
                var info = await _api.Customers.ReadBusinessCardAsync(dialog.FileName, scanCancellation.Token);
                if (IsDisposed || Disposing) return;
                if (!string.IsNullOrWhiteSpace(info.Name)) txtName.Text = info.Name;
                if (!string.IsNullOrWhiteSpace(info.Phone)) txtPhone.Text = info.Phone;
                if (!string.IsNullOrWhiteSpace(info.TaxCode)) txtTaxCode.Text = info.TaxCode;
                if (!string.IsNullOrWhiteSpace(info.Address)) txtAddress.Text = info.Address;
                if (!string.IsNullOrWhiteSpace(info.CustomerType))
                {
                    var type = info.CustomerType.Equals("Company", StringComparison.OrdinalIgnoreCase) ? "Company" :
                        info.CustomerType.Equals("Individual", StringComparison.OrdinalIgnoreCase) ? "Individual" : null;
                    if (type is not null) cboType.SelectedItem = type;
                }
                lblStatus.ForeColor = Color.FromArgb(22, 163, 74);
                lblStatus.Text = "Đã đọc xong. Vui lòng kiểm tra thông tin.";
            }
            catch (OperationCanceledException) when (scanCancellation.IsCancellationRequested) { }
            catch (Exception ex)
            {
                if (IsDisposed || Disposing) return;
                lblStatus.ForeColor = Color.FromArgb(220, 38, 38);
                lblStatus.Text = "Không đọc được danh thiếp.";
                var message = ex is OperationCanceledException ? "Yêu cầu quét danh thiếp quá thời gian chờ. Vui lòng thử lại." : ex.Message;
                MessageBox.Show(this, message, "Lỗi đọc danh thiếp", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _cardScanCancellation = null;
                if (!IsDisposed && !Disposing) SetCardScanning(false);
            }
        }

        private void SetCardScanning(bool scanning)
        {
            foreach (var control in new Control[] { txtName, txtPhone, txtTaxCode, txtAddress, cboType, btnSave, btnReadWithAi })
                control.Enabled = !scanning;
            UseWaitCursor = scanning;
        }
    }
}
