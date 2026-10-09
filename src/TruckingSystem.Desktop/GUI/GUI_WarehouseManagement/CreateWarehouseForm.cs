using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_LocationManagement;

namespace TruckingSystem.Desktop.GUI_WarehouseManagement
{
    /// <summary>Tạo kho mới — cho phép công ty có NHIỀU kho ở nhiều địa điểm khác
    /// nhau (TP.HCM, Hà Nội, Đà Nẵng...). Địa chỉ BẮT BUỘC chọn từ gợi ý VietMap
    /// Autocomplete (giống CreateLocationForm) — không nhập tay, để đảm bảo có
    /// tọa độ chuẩn nếu sau này cần dùng cho tính năng khác.</summary>
    public partial class CreateWarehouseForm : BaseForm
    {
        private ApiServices? _api;
        private readonly System.Windows.Forms.Timer _debounceTimer = new() { Interval = 300 };
        private PlaceSuggestionItem? _selectedAddress;
        private bool _suppressNextSearch = false;

        public CreateWarehouseForm()
        {
            InitializeComponent();

            txtSearch.TextChanged += (_, _) =>
            {
                if (_suppressNextSearch) { _suppressNextSearch = false; return; }
                _selectedAddress = null;
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

        public CreateWarehouseForm(ApiServices api) : this()
        {
            _api = api;
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

            _selectedAddress = suggestion;
            lblSelectedAddress.Text = $"✅ {suggestion.Address}";
            lblSelectedAddress.ForeColor = Theme.Success;
            lstSuggestions.Visible = false;

            _suppressNextSearch = true;
            txtSearch.Text = suggestion.Address;
            lblStatus.Text = "";
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                lblStatus.Text = "Vui lòng nhập tên kho.";
                return;
            }
            if (_api is null) return;

            var doorError = WarehouseDoorHelper.Validate(cboDoorSide.SelectedIndex, numDoorOffset.Value, numLength.Value, numWidth.Value);
            if (doorError is not null)
            {
                lblStatus.Text = doorError;
                return;
            }
            var doorSide = WarehouseDoorHelper.ToApiValue(cboDoorSide.SelectedIndex);
            decimal? doorOffset = doorSide is null ? null : numDoorOffset.Value;
            // Tọa độ (0, 0) = chưa có tọa độ thật (điểm giữa biển) — server cũng từ chối.
            if (_selectedAddress is { Lat: 0, Lng: 0 })
            {
                lblStatus.Text = "Địa chỉ đã chọn không có tọa độ hợp lệ (0, 0) — hãy chọn địa chỉ khác trong danh sách gợi ý.";
                return;
            }

            btnSave.Enabled = false;
            try
            {
                await _api.Warehouses.CreateWarehouseAsync(
                    txtName.Text.Trim(),
                    _selectedAddress?.Address, // null nếu chưa chọn địa chỉ — vẫn cho phép tạo kho không có địa chỉ
                    numCapacity.Value == 0 ? null : numCapacity.Value,
                    numLength.Value == 0 ? null : numLength.Value,
                    numWidth.Value == 0 ? null : numWidth.Value,
                    _selectedAddress?.Lat, _selectedAddress?.Lng,
                    doorSide, doorOffset);

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