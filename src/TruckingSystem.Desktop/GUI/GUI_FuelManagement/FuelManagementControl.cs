using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_FuelManagement;

namespace TruckingSystem.Desktop.GUI_FuelManagement
{
    /// <summary>Quản lý hãng xăng (FuelProvider) — CHỈ hiện danh sách hãng ở
    /// đây. Double-click vào 1 hãng để mở form riêng xem/thêm giá xăng dầu
    /// của ĐÚNG hãng đó (FuelProviderPricesForm) — tách biệt rõ ràng, không
    /// còn hiện giá kế bên phải gây rối mắt như trước.</summary>
    public partial class FuelManagementControl : BaseUserControl
    {
        private ApiServices? _api;
        private List<FuelProviderItem> _providers = new();

        public FuelManagementControl()
        {
            InitializeComponent();
            Theme.StyleGrid(gridProviders);
            gridProviders.DataBindingComplete += (_, _) => { if (gridProviders.Columns["Id"] != null) gridProviders.Columns["Id"]!.Visible = false; };
            gridProviders.CellDoubleClick += GridProviders_CellDoubleClick;
            Resize += (_, _) => { gridProviders.Width = Width; gridProviders.Height = Height - gridProviders.Top; };
            Load += async (_, _) => await LoadProvidersAsync();
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
                // Dòng đầu: "Giá thị trường chung" (giá KHÔNG gắn hãng, Id = 0) — dùng làm
                // giá dự phòng khi hãng được chọn lúc ghi xăng chưa có giá. Trước đây không có
                // chỗ nào nhập được loại giá này.
                var rows = new[] { new { Id = 0, Tên_hãng = "🌐 Giá thị trường chung (không theo hãng)", Điện_thoại = "—", Địa_chỉ = "—" } }
                    .Concat(_providers.Select(p => new
                    {
                        p.Id,
                        Tên_hãng = p.Name,
                        Điện_thoại = p.ContactPhone ?? "—",
                        Địa_chỉ = p.Address ?? "—",
                    }))
                    .ToList();
                gridProviders.DataSource = rows;
                lblStatus.Text = $"{_providers.Count} hãng xăng — double-click vào 1 hãng để xem/thêm giá";
                lblStatus.ForeColor = Theme.TextSecondary;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi tải danh sách hãng xăng: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }

        /// <summary>Double-click vào 1 hãng -> mở form riêng xem/thêm giá của
        /// ĐÚNG hãng đó, thay vì hiện chung 1 danh sách giá kế bên gây rối.</summary>
        private void GridProviders_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (_api is null || e.RowIndex < 0) return;
            var id = (int)gridProviders.Rows[e.RowIndex].Cells["Id"].Value;
            if (id == 0)
            {
                using var market = new FuelProviderPricesForm(_api, null, "Giá thị trường chung");
                market.ShowDialog(FindForm());
                return;
            }
            var provider = _providers.FirstOrDefault(p => p.Id == id);
            if (provider is null) return;

            using var f = new FuelProviderPricesForm(_api, provider.Id, provider.Name);
            f.ShowDialog(FindForm());
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
            if (id == 0)
            {
                MessageBox.Show(FindForm(), "\"Giá thị trường chung\" không phải 1 hãng — không xóa được. Muốn bỏ giá nào thì mở ra và xóa từng mức giá.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(FindForm(), "Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}