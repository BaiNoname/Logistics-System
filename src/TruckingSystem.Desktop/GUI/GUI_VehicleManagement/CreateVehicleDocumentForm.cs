using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_VehicleManagement
{
    public partial class CreateVehicleDocumentForm : BaseForm
    {
        private ApiServices? _api;
        private int _vehicleId;
        private string? _selectedFilePath; // file trên máy, sẽ UPLOAD lên server khi Lưu

        /// <summary>Hiện tiếng Việt, gửi lên server đúng giá trị enum.</summary>
        private sealed record DocTypeOption(string Value, string Text)
        {
            public override string ToString() => Text;
        }

        public CreateVehicleDocumentForm()
        {
            InitializeComponent();
            cboDocType.Items.AddRange(new object[]
            {
                new DocTypeOption("Registration", "Đăng kiểm"),
                new DocTypeOption("Insurance", "Bảo hiểm"),
                new DocTypeOption("TransportPermit", "Phù hiệu xe (GP vận tải)"),
                new DocTypeOption("Other", "Khác"),
            });
            cboDocType.SelectedIndex = 0;
            dtIssueDate.Value = DateTime.Today;
            dtExpiryDate.Value = DateTime.Today.AddYears(1);
        }

        public CreateVehicleDocumentForm(ApiServices api, int vehicleId) : this()
        {
            _api = api;
            _vehicleId = vehicleId;
        }

        /// <summary>Chọn file scan trên máy — không gõ đường dẫn tay nữa. File được
        /// UPLOAD lên server khi bấm Lưu, nên máy khác (laptop, kế toán) cũng mở được.</summary>
        private void BtnBrowseFile_Click(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Chọn file scan giấy tờ xe",
                Filter = "File scan (*.pdf;*.jpg;*.jpeg;*.png)|*.pdf;*.jpg;*.jpeg;*.png",
            };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            var info = new FileInfo(dialog.FileName);
            if (info.Length > 10 * 1024 * 1024)
            {
                lblStatus.Text = "File quá lớn (tối đa 10 MB).";
                return;
            }
            _selectedFilePath = dialog.FileName;
            txtFileUrl.Text = info.Name;
            lblStatus.Text = "";
        }

        private async Task<(bool Ok, string? FileUrl, string? Error)> UploadSelectedFileAsync()
        {
            if (_api is null || string.IsNullOrEmpty(_selectedFilePath)) return (true, null, null);

            using var content = new MultipartFormDataContent();
            var bytes = await File.ReadAllBytesAsync(_selectedFilePath);
            var fileContent = new ByteArrayContent(bytes);
            content.Add(fileContent, "file", Path.GetFileName(_selectedFilePath));

            var response = await _api.Connection.Http.PostAsync("/api/vehicles/documents/upload", content);
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) return (false, null, $"Upload file lỗi ({(int)response.StatusCode}).");

            var result = Newtonsoft.Json.Linq.JObject.Parse(json);
            var ok = result.Value<bool?>("success") ?? result.Value<bool?>("Success") ?? false;
            var url = result.Value<string>("fileUrl") ?? result.Value<string>("FileUrl");
            var message = result.Value<string>("message") ?? result.Value<string>("Message");
            return ok ? (true, url, null) : (false, null, message ?? "Upload file thất bại.");
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
                var upload = await UploadSelectedFileAsync();
                if (!upload.Ok)
                {
                    lblStatus.Text = "⚠️ " + upload.Error;
                    return;
                }

                var docType = (cboDocType.SelectedItem as DocTypeOption)?.Value ?? "Other";
                await _api.Vehicles.CreateDocumentAsync(
                    _vehicleId, docType, dtIssueDate.Value, dtExpiryDate.Value, upload.FileUrl);

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