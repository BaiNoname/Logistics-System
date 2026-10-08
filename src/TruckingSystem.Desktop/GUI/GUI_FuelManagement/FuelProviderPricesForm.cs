using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_FuelManagement;

namespace TruckingSystem.Desktop.GUI_FuelManagement
{
    /// <summary>Xem lịch sử giá xăng dầu của ĐÚNG 1 hãng (mở ra khi double-click
    /// hãng đó ở màn Quản lý Nhiên liệu) — có sẵn control THÊM GIÁ MỚI ngay
    /// trong form này, không cần mở thêm form riêng.</summary>
    public partial class FuelProviderPricesForm : BaseForm
    {
        private ApiServices? _api;
        private int? _providerId; // null = Giá thị trường chung (không theo hãng)
        private List<FuelPriceHistoryItem> _prices = new();

        public FuelProviderPricesForm()
        {
            InitializeComponent();
            dtEffectiveDate.Value = DateTime.Today;
            grid.DataBindingComplete += (_, _) => { if (grid.Columns["Id"] != null) grid.Columns["Id"]!.Visible = false; };
        }

        public FuelProviderPricesForm(ApiServices api, int? providerId, string providerName) : this()
        {
            _api = api;
            _providerId = providerId;
            Text = $"Giá xăng dầu — {providerName}";
            Load += async (_, _) =>
            {
                await LoadFuelTypesAsync();
                await LoadAsync();
            };
        }

        /// <summary>Dropdown loại nhiên liệu — lấy từ danh mục chuẩn ở server
        /// (GET /api/fuel/types), không cho gõ tay -> không sai tên.</summary>
        private async Task LoadFuelTypesAsync()
        {
            if (_api is null) return;
            try
            {
                var types = await _api.Connection.GetAsync<List<string>>("/api/fuel/types") ?? new();
                cboFuelType.Items.Clear();
                foreach (var t in types) cboFuelType.Items.Add(t);
                // Mặc định chọn Dầu DO (nhiên liệu của xe tải) nếu có.
                var diesel = types.FirstOrDefault(t => t.StartsWith("Dầu DO", StringComparison.OrdinalIgnoreCase));
                if (diesel != null) cboFuelType.SelectedItem = diesel;
                else if (cboFuelType.Items.Count > 0) cboFuelType.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Không tải được danh mục nhiên liệu: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }

        private async Task LoadAsync()
        {
            if (_api is null) return;
            lblStatus.Text = "Đang tải...";
            try
            {
                // providerId = null: API trả về TẤT CẢ giá -> chỉ giữ giá không gắn hãng.
                _prices = (await _api.Fuel.GetPriceHistoryAsync(_providerId))
                    .Where(p => _providerId.HasValue || p.FuelProviderId == null)
                    .ToList();
                grid.DataSource = _prices.Select(p => new
                {
                    p.Id,
                    Loại_nhiên_liệu = p.FuelType,
                    Giá_VNĐ = p.Price.ToString("N0"),
                    Ngày_hiệu_lực = p.EffectiveDate.ToString("dd/MM/yyyy"),
                }).ToList();
                lblStatus.Text = $"{_prices.Count} mức giá đã ghi nhận";
                lblStatus.ForeColor = Theme.TextSecondary;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }

        private async void BtnAddPrice_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;
            if (cboFuelType.SelectedItem is not string fuelType || string.IsNullOrWhiteSpace(fuelType))
            {
                lblStatus.Text = "Vui lòng chọn loại nhiên liệu.";
                lblStatus.ForeColor = Theme.Warning;
                return;
            }
            if (numPrice.Value <= 0)
            {
                lblStatus.Text = "Giá phải lớn hơn 0.";
                lblStatus.ForeColor = Theme.Warning;
                return;
            }

            try
            {
                await _api.Fuel.CreatePriceAsync(fuelType, numPrice.Value, dtEffectiveDate.Value.Date, _providerId);
                numPrice.Value = 0;
                await LoadAsync();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }

        /// <summary>Xóa 1 mức giá — cần thiết để "dọn đường" nếu sau này muốn
        /// xóa hẳn hãng xăng này (Backend chặn xóa hãng nếu còn lịch sử giá).</summary>
        private async void BtnDeletePrice_Click(object? sender, EventArgs e)
        {
            if (_api is null || grid.CurrentRow is null)
            {
                MessageBox.Show(FindForm(), "Vui lòng chọn 1 mức giá trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var id = (int)grid.CurrentRow.Cells["Id"].Value;

            var confirm = MessageBox.Show(FindForm(), "Xóa hẳn mức giá này? Không thể hoàn tác.", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            try
            {
                var result = await _api.Fuel.DeletePriceAsync(id);
                if (!result.Success)
                {
                    MessageBox.Show(FindForm(), result.Message ?? "Không thể xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(FindForm(), "Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}