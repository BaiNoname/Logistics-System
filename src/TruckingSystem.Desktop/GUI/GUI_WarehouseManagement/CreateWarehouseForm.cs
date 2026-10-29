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
        private PlaceSuggestionItem? _selectedAddress;
        private readonly PlaceSearchBinder _placeSearch;

        public CreateWarehouseForm()
        {
            InitializeComponent();
            // Chữ cho NHÂN VIÊN (không nêu tên thuật toán) — đặt bằng code, không sửa Designer.
            lblCapacity.Text = "Sức chứa (m³) — dùng để tự xếp hàng vào kho, tuỳ chọn";

            // Ô tìm địa chỉ VietMap: gõ -> danh sách gợi ý -> BẤM 1 dòng -> có địa chỉ + tọa độ.
            _placeSearch = new PlaceSearchBinder(txtSearch, lstSuggestions, lblSelectedAddress, item =>
            {
                _selectedAddress = item;
                if (item is not null) lblStatus.Text = "";
            });

            // Vị trí cửa: chọn tường trước, rồi mới nhập được khoảng cách dọc tường.
            WarehouseDoorHelper.FillSideCombo(cboDoorSide);
            cboDoorSide.SelectedIndexChanged += (_, _) => numDoorOffset.Enabled = cboDoorSide.SelectedIndex > 0;
        }

        public CreateWarehouseForm(ApiServices api) : this()
        {
            _api = api;
            _placeSearch.Api = api;
        }



        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                lblStatus.Text = "Vui lòng nhập tên kho.";
                return;
            }
            if (_api is null) return;
            // Địa chỉ (có tọa độ) BẮT BUỘC: mọi chặng đều đi qua kho — thiếu tọa độ thì Chia xe không
            // ước tính được quãng đường / số ngày chuyến.
            if (_selectedAddress is null)
            {
                lblStatus.Text = "Vui lòng tìm và CHỌN địa chỉ kho từ danh sách gợi ý.";
                return;
            }
            if (numLength.Value <= 0 || numWidth.Value <= 0)
            {
                lblStatus.Text = "Vui lòng nhập Chiều dài và Chiều rộng kho (mét) — dùng để vẽ sơ đồ kho.";
                return;
            }

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
                    _selectedAddress.Address,
                    numCapacity.Value == 0 ? null : numCapacity.Value,
                    numLength.Value,
                    numWidth.Value,
                    _selectedAddress.Lat, _selectedAddress.Lng,
                    doorSide, doorOffset, numStorageFee.Value);

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