using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_FuelManagement;

namespace TruckingSystem.Desktop.GUI_FuelManagement
{
    /// <summary>Quản lý hãng xăng (FuelProvider) và lịch sử giá xăng (FuelPriceHistory)
    /// — dùng để tra cứu chi phí xăng dự kiến, đối chiếu với ActualFuelCost ghi
    /// nhận thực tế ở màn Chuyến (Trip).</summary>
    public partial class FuelManagementControl : BaseUserControl
    {
        private ApiServices? _api;
        private List<FuelProviderItem> _providers = new();

        public FuelManagementControl()
        {
            InitializeComponent();
            Theme.StyleGrid(gridProviders);
            Theme.StyleGrid(gridPrices);
            gridProviders.DataBindingComplete += (_, _) => { if (gridProviders.Columns["Id"] != null) gridProviders.Columns["Id"]!.Visible = false; };
            gridProviders.SelectionChanged += async (_, _) => await LoadPricesAsync();
            Resize += (_, _) =>
            {
                gridProviders.Width = Width / 2 - 30;
                gridPrices.Width = Width - gridPrices.Left - 20;
                gridProviders.Height = Height - gridProviders.Top - 10;
                gridPrices.Height = Height - gridPrices.Top - 10;
            };
            Load += async (_, _) => { await LoadProvidersAsync(); await LoadPricesAsync(); };
        }

        public FuelManagementControl(ApiServices api) : this()
        {
            _api = api;
        }

        private async Task LoadProvidersAsync()
        {
            if (_api is null) return;
            try
            {
                _providers = await _api.Fuel.GetProvidersAsync();
                gridProviders.DataSource = _providers.Select(p => new
                {
                    p.Id,
                    Tên_hãng = p.Name,
                    Điện_thoại = p.ContactPhone ?? "—",
                    Địa_chỉ = p.Address ?? "—",
                }).ToList();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi tải danh sách hãng xăng: " + ex.Message;
            }
        }

        /// <summary>Chưa chọn hãng nào -> hiện TOÀN BỘ lịch sử giá (gồm cả giá thị
        /// trường chung); chọn 1 hãng -> lọc đúng hãng đó.</summary>
        private async Task LoadPricesAsync()
        {
            if (_api is null) return;
            try
            {
                int? providerId = gridProviders.CurrentRow is null ? null : (int)gridProviders.CurrentRow.Cells["Id"].Value;
                var prices = await _api.Fuel.GetPriceHistoryAsync(providerId);
                gridPrices.DataSource = prices.Select(p => new
                {
                    Loại_nhiên_liệu = p.FuelType,
                    Giá_VNĐ = p.Price,
                    Ngày_hiệu_lực = p.EffectiveDate.ToString("dd/MM/yyyy"),
                    Hãng = p.FuelProviderName,
                }).ToList();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi tải lịch sử giá: " + ex.Message;
            }
        }

        private async void BtnAddProvider_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;
            using var f = new CreateFuelProviderForm(_api);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadProvidersAsync();
        }

        private async void BtnDeleteProvider_Click(object? sender, EventArgs e)
        {
            if (_api is null || gridProviders.CurrentRow is null)
            {
                MessageBox.Show(FindForm(), "Vui lòng chọn 1 hãng xăng trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var id = (int)gridProviders.CurrentRow.Cells["Id"].Value;
            var confirm = MessageBox.Show(FindForm(), "Xóa hẳn hãng xăng này? Không thể hoàn tác.", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            try
            {
                var result = await _api.Fuel.DeleteProviderAsync(id);
                if (!result.Success)
                {
                    MessageBox.Show(FindForm(), result.Message ?? "Không thể xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                await LoadProvidersAsync();
                await LoadPricesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(FindForm(), "Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnAddPrice_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;
            using var f = new CreateFuelPriceForm(_api, _providers);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadPricesAsync();
        }
    }
}
