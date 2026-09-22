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

            btnSave.Enabled = false;
            try
            {
                await _api.Warehouses.CreateWarehouseAsync(
                    txtName.Text.Trim(),
                    _selectedAddress?.Address, // null nếu chưa chọn địa chỉ — vẫn cho phép tạo kho không có địa chỉ
                    numCapacity.Value == 0 ? null : numCapacity.Value,
                    numLength.Value == 0 ? null : numLength.Value,
                    numWidth.Value == 0 ? null : numWidth.Value,
                    _selectedAddress?.Lat, _selectedAddress?.Lng);

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
