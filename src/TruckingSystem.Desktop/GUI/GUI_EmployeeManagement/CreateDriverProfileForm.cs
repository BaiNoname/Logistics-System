using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_WarehouseManagement;

namespace TruckingSystem.Desktop.GUI_EmployeeManagement
{
    /// <summary>Tạo/Sửa hồ sơ tài xế (DriverProfile) cho 1 nhân viên — BẮT BUỘC
    /// phải có hồ sơ này thì nhân viên đó mới xuất hiện trong danh sách tài xế
    /// khả dụng của thuật toán Chia xe (Greedy). Dùng CHUNG 1 form cho cả 2
    /// việc — constructor thường = Tạo mới; nếu employee ĐÃ có hồ sơ, form tự
    /// phát hiện và chuyển sang chế độ Sửa (nạp sẵn dữ liệu cũ).</summary>
    public partial class CreateDriverProfileForm : BaseForm
    {
        private ApiServices? _api;
        private int _employeeId;
        private bool _isEditMode = false;

        public CreateDriverProfileForm()
        {
            InitializeComponent();
            // Đủ hạng cũ (luật 2008: B2, E, FC...) và hạng mới (luật 2024: B, C1, CE...) — khớp DriverLicenseRules.
            cboLicenseClass.Items.AddRange(new object[] { "B", "B2", "C1", "C", "CE", "D1", "D2", "D", "DE", "E", "FC" });
            cboLicenseClass.SelectedItem = "C";
            // KHÔNG điền sẵn ngày "đẹp" (VD +3 năm): người nhập lỡ bỏ qua là lưu ngày hết hạn giả, cảnh báo
            // giấy tờ sai hết. Để hôm qua (không hợp lệ) -> bắt buộc chọn đúng ngày trên bằng / giấy khám.
            dtLicenseExpiry.Value = DateTime.Today.AddDays(-1);
            dtHealthCertExpiry.Value = DateTime.Today.AddDays(-1);
            // Giấy khám sức khỏe BẮT BUỘC với tài xế (server cũng kiểm tra) — luôn nhập ngày hết hạn.
            chkHasHealthCert.Checked = true;
            chkHasHealthCert.Enabled = false;
            dtHealthCertExpiry.Enabled = true;
        }

        public CreateDriverProfileForm(ApiServices api, int employeeId, string employeeName) : this()
        {
            _api = api;
            _employeeId = employeeId;
            Text = $"Tạo hồ sơ tài xế — {employeeName}";
            Load += async (_, _) => { await LoadWarehousesAsync(); await TryLoadExistingProfileAsync(); };
        }

        /// <summary>Đổ dropdown "Kho đóng quân" — để tuỳ chọn (có thể để trống,
        /// nghĩa là tài xế CHƯA gán kho cố định, vẫn dùng được cho mọi đơn).</summary>
        private async Task LoadWarehousesAsync()
        {
            if (_api is null) return;
            await WarehouseOptions.FillAsync(cboWarehouse, _api, null, lblStatus);
        }

        /// <summary>Nhân viên này ĐÃ có hồ sơ từ trước -> tự chuyển sang chế độ
        /// Sửa, nạp sẵn dữ liệu cũ — tránh báo lỗi "đã có hồ sơ rồi" khi họ chỉ
        /// định vào xem/sửa lại thông tin.</summary>
        private async Task TryLoadExistingProfileAsync()
        {
            if (_api is null) return;
            try
            {
                var existing = await _api.Employees.GetDriverProfileAsync(_employeeId);
                if (existing is null) return; // chưa có -> giữ nguyên chế độ Tạo mới

                _isEditMode = true;
                Text = Text.Replace("Tạo hồ sơ tài xế", "Sửa hồ sơ tài xế");
                btnSave.Text = "Lưu thay đổi";

                txtLicenseNumber.Text = existing.LicenseNumber;
                // Hạng cũ không còn trong danh sách vẫn hiện đúng (không âm thầm đổi sang hạng khác).
                if (!cboLicenseClass.Items.Contains(existing.LicenseClass)) cboLicenseClass.Items.Add(existing.LicenseClass);
                cboLicenseClass.SelectedItem = existing.LicenseClass;
                dtLicenseExpiry.Value = existing.LicenseExpiryDate;
                numYearsExp.Value = existing.YearsOfExperience;
                txtGoogleCalendarId.Text = existing.GoogleCalendarId ?? "";
                WarehouseOptions.Select(cboWarehouse, existing.WarehouseId); // đã đổ dropdown ở LoadWarehousesAsync trước đó, giờ chỉ chọn lại đúng giá trị
                if (existing.HealthCertExpiryDate.HasValue)
                    dtHealthCertExpiry.Value = existing.HealthCertExpiryDate.Value;
                else
                {
                    // Hồ sơ cũ chưa có giấy khám: KHÔNG điền sẵn ngày giả (lưu nhầm thành "có giấy khám") —
                    // để ngày hôm qua (không hợp lệ) buộc người dùng chọn đúng ngày trên giấy.
                    dtHealthCertExpiry.Value = DateTime.Today.AddDays(-1);
                    lblStatus.Text = "Hồ sơ này chưa có giấy khám sức khỏe — chọn ngày hết hạn ghi trên giấy khám rồi lưu.";
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi tải hồ sơ cũ: " + ex.Message;
            }
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLicenseNumber.Text))
            {
                lblStatus.Text = "Vui lòng nhập số bằng lái.";
                return;
            }
            if (cboLicenseClass.SelectedItem is not string)
            {
                lblStatus.Text = "Vui lòng chọn hạng bằng lái.";
                return;
            }
            if (dtLicenseExpiry.Value.Date < DateTime.Today)
            {
                lblStatus.Text = "Chọn ngày hết hạn ghi trên bằng lái (bằng đã hết hạn thì nhập theo bằng mới).";
                return;
            }
            if (dtHealthCertExpiry.Value.Date < DateTime.Today)
            {
                lblStatus.Text = "Chọn ngày hết hạn ghi trên giấy khám sức khỏe (giấy cũ hết hạn thì nhập theo giấy khám mới).";
                return;
            }
            if (_api is null) return;

            btnSave.Enabled = false;
            try
            {
                DateTime? healthCert = dtHealthCertExpiry.Value;
                var googleCalendarId = string.IsNullOrWhiteSpace(txtGoogleCalendarId.Text) ? null : txtGoogleCalendarId.Text.Trim();
                var warehouseId = WarehouseOptions.Selected(cboWarehouse);
                var result = _isEditMode
                    ? await _api.Employees.UpdateDriverProfileAsync(
                        _employeeId, txtLicenseNumber.Text.Trim(), (string)cboLicenseClass.SelectedItem!,
                        dtLicenseExpiry.Value, healthCert, (int)numYearsExp.Value, googleCalendarId, warehouseId)
                    : await _api.Employees.CreateDriverProfileAsync(
                        _employeeId, txtLicenseNumber.Text.Trim(), (string)cboLicenseClass.SelectedItem!,
                        dtLicenseExpiry.Value, healthCert, (int)numYearsExp.Value, googleCalendarId, warehouseId);

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
