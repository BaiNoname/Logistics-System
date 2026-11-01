using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_LocationManagement
{
    public partial class LocationsControl : BaseUserControl
    {
        private ApiServices? _api;

        public LocationsControl()
        {
            InitializeComponent();
            Theme.StyleGrid(grid);
            GridLayout.FitColumns(grid); // cột vừa nội dung, nhiều cột thì cuộn ngang thay vì cắt chữ
            Load += (_, _) => Theme.UniformButtons(toolbar); // nút đều nhau, xuống dòng vẫn thẳng cột
            // Không có bộ lọc chọn: "Loại" (dùng chung / riêng của khách) luôn là "Dùng chung" vì chưa có chỗ tạo địa điểm
            // riêng cho khách; tọa độ thì luôn có (form + máy chủ bắt buộc chọn từ gợi ý VietMap). Chỉ cần ô tìm nhanh.
            filterBar.FilterChanged += (_, _) => BindGrid();
            Load += async (_, _) => await LoadAsync();
        }

        public LocationsControl(ApiServices api) : this()
        {
            _api = api;
        }

        private void BtnRefresh_Click(object? sender, EventArgs e) => _ = LoadAsync();

        private async void BtnCreate_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;
            using var f = new CreateLocationForm(_api);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
        }

        private List<TruckingSystem.Desktop.DTO_LocationManagement.LocationItem> _locations = new();

        /// <summary>Đổ lưới theo ô tìm nhanh (tên, địa chỉ). Chỉ 2 cột Tên + Địa chỉ: bỏ Loại (luôn "Dùng chung"),
        /// Vĩ độ / Kinh độ (số thô, địa điểm nào cũng có vì bắt buộc chọn từ gợi ý VietMap).</summary>
        private void BindGrid()
        {
            var rows = _locations.Where(l => filterBar.MatchesSearch(l.Name, l.Address)).ToList();
            grid.DataSource = rows.Select(l => new
            {
                Tên_địa_điểm = l.Name,
                Địa_chỉ = l.Address,
            }).ToList();
            filterBar.SetCount(rows.Count, _locations.Count);
        }

        private async Task LoadAsync()
        {
            if (_api is null) return;
            lblStatus.Text = "Đang tải...";
            try
            {
                _locations = await _api.Locations.GetLocationsAsync();
                BindGrid();
                lblStatus.Text = $"Tổng {_locations.Count} địa điểm — dùng để chọn nhanh khi tạo đơn vận chuyển";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }
    }
}
