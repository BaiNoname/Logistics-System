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
            filterBar.AddChoice("scope", "Loại", new[] { ScopeShared, ScopeCustomer });
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

        private const string ScopeShared = "Dùng chung";
        private const string ScopeCustomer = "Địa chỉ riêng của khách";
        private List<TruckingSystem.Desktop.DTO_LocationManagement.LocationItem> _locations = new();

        /// <summary>Đổ lưới theo bộ lọc (tìm theo tên, địa chỉ; lọc dùng chung / riêng của khách).</summary>
        private void BindGrid()
        {
            var rows = _locations.Where(l => filterBar.Is("scope", l.CustomerId is null ? ScopeShared : ScopeCustomer)
                                             && filterBar.MatchesSearch(l.Name, l.Address)).ToList();
            grid.DataSource = rows.Select(l => new
            {
                Tên_địa_điểm = l.Name,
                Địa_chỉ = l.Address,
                Loại = l.CustomerId is null ? ScopeShared : ScopeCustomer,
                Vĩ_độ = l.Lat?.ToString("0.0000") ?? "—",
                Kinh_độ = l.Lng?.ToString("0.0000") ?? "—",
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
