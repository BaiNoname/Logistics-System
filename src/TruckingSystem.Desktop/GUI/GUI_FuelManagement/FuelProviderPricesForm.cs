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
        private int _providerId;

        public FuelProviderPricesForm()
        {
            InitializeComponent();
            dtEffectiveDate.Value = DateTime.Today;
        }

        public FuelProviderPricesForm(ApiServices api, int providerId, string providerName) : this()
        {
            _api = api;
            _providerId = providerId;
            Text = $"Giá xăng dầu — {providerName}";
            Load += async (_, _) => await LoadAsync();
        }

        private async Task LoadAsync()
        {
            if (_api is null) return;
            lblStatus.Text = "Đang tải...";
            try
            {
                var prices = await _api.Fuel.GetPriceHistoryAsync(_providerId);
                grid.DataSource = prices.Select(p => new
                {
                    Loại_nhiên_liệu = p.FuelType,
                    Giá_VNĐ = p.Price.ToString("N0"),
                    Ngày_hiệu_lực = p.EffectiveDate.ToString("dd/MM/yyyy"),
                }).ToList();
                lblStatus.Text = $"{prices.Count} mức giá đã ghi nhận";
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
            if (string.IsNullOrWhiteSpace(txtFuelType.Text))
            {
                lblStatus.Text = "Vui lòng nhập loại nhiên liệu (VD: Dầu DO, Xăng RON95).";
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
                await _api.Fuel.CreatePriceAsync(txtFuelType.Text.Trim(), numPrice.Value, dtEffectiveDate.Value, _providerId);
                txtFuelType.Text = "";
                numPrice.Value = 0;
                await LoadAsync();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }
    }
}