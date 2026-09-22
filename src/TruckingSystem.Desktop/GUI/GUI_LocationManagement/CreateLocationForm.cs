using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_LocationManagement;

namespace TruckingSystem.Desktop.GUI_LocationManagement
{
    /// <summary>
    /// Logic — thêm địa điểm mới BẮT BUỘC chọn từ gợi ý VietMap Autocomplete
    /// (không cho gõ tay tọa độ nữa). Người dùng gõ từ khóa → hệ thống tự động
    /// gợi ý sau 300ms ngừng gõ (debounce, tránh gọi API dồn dập từng phím) →
    /// chọn 1 gợi ý → tự động lấy tọa độ thật qua Place API.
    /// </summary>
    public partial class CreateLocationForm : BaseForm
    {
        private ApiServices? _api;
        private readonly System.Windows.Forms.Timer _debounceTimer = new() { Interval = 300 };
        private PlaceSuggestionItem? _selectedDetail;
        private bool _suppressNextSearch = false; // true ngay sau khi chọn xong gợi ý — tránh tìm lại vô ích

        public CreateLocationForm()
        {
            InitializeComponent();

            txtSearch.TextChanged += (_, _) =>
            {
                if (_suppressNextSearch) { _suppressNextSearch = false; return; }
                _selectedDetail = null; // gõ lại nghĩa là chọn cũ không còn hiệu lực, phải chọn lại
                lblSelectedAddress.Text = "";
                _debounceTimer.Stop();
                _debounceTimer.Start();
            };
            _debounceTimer.Tick += async (_, _) => { _debounceTimer.Stop(); await RunSearchAsync(); };
            lstSuggestions.SelectedIndexChanged += (_, _) => SelectSuggestion();
        }

        public CreateLocationForm(ApiServices api) : this()
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
                lblStatus.ForeColor = Theme.Danger;
            }
        }

        /// <summary>Chọn xong xử lý NGAY tại chỗ — Autocomplete đã trả sẵn tọa độ
        /// trong kết quả tìm kiếm (đã xác nhận qua test thật), không cần gọi
        /// thêm API Place nào nữa.</summary>
        private void SelectSuggestion()
        {
            if (lstSuggestions.SelectedItem is not PlaceSuggestionItem suggestion) return;

            _selectedDetail = suggestion;
            lblSelectedAddress.Text = $"✅ {suggestion.Address}\n(Lat: {suggestion.Lat:0.000000}, Lng: {suggestion.Lng:0.000000})";
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
                lblStatus.Text = "Vui lòng nhập tên địa điểm.";
                lblStatus.ForeColor = Theme.Danger;
                return;
            }
            if (_selectedDetail is null)
            {
                lblStatus.Text = "Vui lòng tìm và CHỌN 1 địa chỉ từ danh sách gợi ý — không nhập tay được.";
                lblStatus.ForeColor = Theme.Danger;
                return;
            }
            if (_api is null) return;

            try
            {
                await _api.Locations.CreateLocationAsync(txtName.Text.Trim(), _selectedDetail.Address, _selectedDetail.Lat, _selectedDetail.Lng);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }
    }
}
