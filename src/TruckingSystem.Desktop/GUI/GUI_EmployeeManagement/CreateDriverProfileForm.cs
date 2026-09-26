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
            cboLicenseClass.Items.AddRange(new object[] { "B2", "C", "D", "E", "FC" });
            cboLicenseClass.SelectedIndex = 0;
            dtLicenseExpiry.Value = DateTime.Today.AddYears(3);
            dtHealthCertExpiry.Value = DateTime.Today.AddYears(1);
            chkHasHealthCert.CheckedChanged += (_, _) => dtHealthCertExpiry.Enabled = chkHasHealthCert.Checked;
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
            try
            {
                var warehouses = await _api.Warehouses.GetWarehousesAsync();
                var items = new List<object> { new { Id = (int?)null, Name = "(Không cố định — dùng được mọi kho)" } };
                items.AddRange(warehouses.Select(w => new { Id = (int?)w.Id, w.Name }));
                cboWarehouse.DataSource = items;
                cboWarehouse.DisplayMember = "Name";
                cboWarehouse.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Không tải được danh sách kho: " + ex.Message;
            }
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
                cboLicenseClass.SelectedItem = existing.LicenseClass;
                dtLicenseExpiry.Value = existing.LicenseExpiryDate;
                numYearsExp.Value = existing.YearsOfExperience;
                txtGoogleCalendarId.Text = existing.GoogleCalendarId ?? "";
                cboWarehouse.SelectedValue = existing.WarehouseId; // đã đổ dropdown ở LoadWarehousesAsync trước đó, giờ chỉ chọn lại đúng giá trị
                if (existing.HealthCertExpiryDate.HasValue)
                {
                    chkHasHealthCert.Checked = true;
                    dtHealthCertExpiry.Value = existing.HealthCertExpiryDate.Value;
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
            if (_api is null) return;

            btnSave.Enabled = false;
            try
            {
                var healthCert = chkHasHealthCert.Checked ? dtHealthCertExpiry.Value : (DateTime?)null;
                var googleCalendarId = string.IsNullOrWhiteSpace(txtGoogleCalendarId.Text) ? null : txtGoogleCalendarId.Text.Trim();
                var warehouseId = cboWarehouse.SelectedValue as int?;
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
