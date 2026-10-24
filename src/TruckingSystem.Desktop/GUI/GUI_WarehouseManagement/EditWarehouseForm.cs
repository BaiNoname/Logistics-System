using System.Xml.Linq;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_LocationManagement;
using TruckingSystem.Desktop.DTO_WarehouseManagement;

namespace TruckingSystem.Desktop.GUI_WarehouseManagement
{
    /// <summary>Sửa thông tin kho đã có — khác CreateWarehouseForm ở chỗ nạp
    /// sẵn dữ liệu cũ. Nếu không đụng vào ô tìm địa chỉ, giữ nguyên tọa độ cũ;
    /// chỉ đổi khi người dùng tìm và chọn lại địa chỉ mới.</summary>
    public partial class EditWarehouseForm : BaseForm
    {
        private ApiServices? _api;
        private int _warehouseId;
        private readonly System.Windows.Forms.Timer _debounceTimer = new() { Interval = 300 };
        private PlaceSuggestionItem? _newSelectedAddress; // null = giữ nguyên địa chỉ/tọa độ cũ
        private string? _oldAddress;
        private double? _oldLat, _oldLng;
        private bool _suppressNextSearch = false;

        public EditWarehouseForm()
        {
            InitializeComponent();

            txtSearch.TextChanged += (_, _) =>
            {
                if (_suppressNextSearch) { _suppressNextSearch = false; return; }
                _newSelectedAddress = null;
                lblSelectedAddress.Text = "";
                _debounceTimer.Stop();
                _debounceTimer.Start();
            };
            _debounceTimer.Tick += async (_, _) => { _debounceTimer.Stop(); await RunSearchAsync(); };
            lstSuggestions.SelectedIndexChanged += (_, _) => SelectSuggestion();

            // Vị trí cửa: chọn tường trước, rồi mới nhập được khoảng cách dọc tường.
            WarehouseDoorHelper.FillSideCombo(cboDoorSide);
            cboDoorSide.SelectedIndexChanged += (_, _) => numDoorOffset.Enabled = cboDoorSide.SelectedIndex > 0;
        }

        public EditWarehouseForm(ApiServices api, WarehouseListItem warehouse) : this()
        {
            _api = api;
            _warehouseId = warehouse.Id;
            _oldAddress = warehouse.Address;
            _oldLat = warehouse.Lat;
            _oldLng = warehouse.Lng;

            txtName.Text = warehouse.Name;
            // SetSafe: số đã lưu vượt giới hạn ô nhập (dữ liệu cũ / nhập qua API) -> nới giới hạn
            // thay vì crash (ArgumentOutOfRangeException) ngay khi mở form.
            SetSafe(numCapacity, warehouse.CapacityM3 ?? 0);
            SetSafe(numLength, warehouse.LengthMeters ?? 0);
            SetSafe(numWidth, warehouse.WidthMeters ?? 0);
            SetSafe(numStorageFee, warehouse.StorageFeePerM3PerDay);

            // Nạp lại vị trí cửa đã khai báo (nếu có)
            cboDoorSide.SelectedIndex = WarehouseDoorHelper.ToIndex(warehouse.DoorSide);
            numDoorOffset.Value = Math.Clamp(warehouse.DoorOffsetMeters ?? 0, numDoorOffset.Minimum, numDoorOffset.Maximum);

            _suppressNextSearch = true;
            txtSearch.Text = warehouse.Address ?? "";
            if (!string.IsNullOrEmpty(warehouse.Address))
            {
                lblSelectedAddress.Text = warehouse.Lat.HasValue
                    ? $"✅ (địa chỉ hiện tại) {warehouse.Address}"
                    : $"⚠️ (địa chỉ cũ, CHƯA có tọa độ) {warehouse.Address}";
                lblSelectedAddress.ForeColor = warehouse.Lat.HasValue ? Theme.Success : Theme.Warning;
            }
        }

        private async Task RunSearchAsync()
        {
            if (_api is null) return;
            var text = txtSearch.Text.Trim();
            if (text.Length < 2) { lstSuggestions.Items.Clear(); lstSuggestions.Visible = false; return; }

            try
            {
                var results = await _api.Locations.SearchPlacesAsync(text);
                lstSuggestions.Items.Clear();
                foreach (var r in results) lstSuggestions.Items.Add(r);
                lstSuggestions.Visible = results.Count > 0;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi tìm kiếm: " + ex.Message;
            }
        }

        private void SelectSuggestion()
        {
            if (lstSuggestions.SelectedItem is not PlaceSuggestionItem suggestion) return;

            _newSelectedAddress = suggestion;
            lblSelectedAddress.Text = $"✅ {suggestion.Address}";
            lblSelectedAddress.ForeColor = Theme.Success;
            lstSuggestions.Visible = false;

            _suppressNextSearch = true;
            txtSearch.Text = suggestion.Address;
            lblStatus.Text = "";
        }

        private static void SetSafe(NumericUpDown n, decimal value)
        {
            if (value > n.Maximum) n.Maximum = value;
            if (value < n.Minimum) n.Minimum = value;
            n.Value = value;
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                lblStatus.Text = "Vui lòng nhập tên kho.";
                return;
            }
            if (_api is null || !btnSave.Enabled) return;

            var doorError = WarehouseDoorHelper.Validate(cboDoorSide.SelectedIndex, numDoorOffset.Value, numLength.Value, numWidth.Value);
            if (doorError is not null)
            {
                lblStatus.Text = doorError;
                return;
            }
            var doorSide = WarehouseDoorHelper.ToApiValue(cboDoorSide.SelectedIndex);
            decimal? doorOffset = doorSide is null ? null : numDoorOffset.Value;

            // Nếu đã chọn địa chỉ mới -> dùng địa chỉ/tọa độ mới; nếu không đụng
            // gì -> giữ nguyên địa chỉ/tọa độ cũ (kể cả khi cũ chưa có tọa độ).
            var address = _newSelectedAddress?.Address ?? _oldAddress;
            var lat = _newSelectedAddress?.Lat ?? _oldLat;
            var lng = _newSelectedAddress?.Lng ?? _oldLng;
            // Kho cũ chưa có địa chỉ / tọa độ: bắt buộc chọn lại từ bản đồ (server cũng kiểm tra).
            if (string.IsNullOrWhiteSpace(address) || lat is null || lng is null)
            {
                lblStatus.Text = "Kho chưa có địa chỉ / tọa độ — vui lòng tìm và CHỌN địa chỉ kho từ danh sách gợi ý.";
                return;
            }
            if (numLength.Value <= 0 || numWidth.Value <= 0)
            {
                lblStatus.Text = "Vui lòng nhập Chiều dài và Chiều rộng kho (mét) — dùng để vẽ sơ đồ kho.";
                return;
            }
            // Tọa độ (0, 0) = chưa có tọa độ thật (điểm giữa biển) — server cũng từ chối.
            if (lat == 0 && lng == 0)
            {
                lblStatus.Text = "Tọa độ kho (0, 0) không hợp lệ — hãy tìm và chọn lại địa chỉ kho từ danh sách gợi ý.";
                return;
            }

            btnSave.Enabled = false;
            try
            {
                var result = await _api.Warehouses.UpdateWarehouseAsync(
                    _warehouseId, txtName.Text.Trim(), address,
                    numCapacity.Value == 0 ? null : numCapacity.Value,
                    numLength.Value == 0 ? null : numLength.Value,
                    numWidth.Value == 0 ? null : numWidth.Value,
                    lat, lng,
                    doorSide, doorOffset, numStorageFee.Value);

                if (!result.Success)
                {
                    lblStatus.Text = "⚠️ " + result.Message;
                    return;
                }
                // Dời tọa độ kho: server báo đã tính lại ước tính cho các chuyến chưa chạy.
                if (!string.IsNullOrWhiteSpace(result.Message))
                    MessageBox.Show(this, result.Message, "Đã lưu kho", MessageBoxButtons.OK, MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
            }
            finally
            {
                if (!IsDisposed) btnSave.Enabled = true;
            }
        }
    }
}