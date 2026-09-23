using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_WarehouseManagement;

namespace TruckingSystem.Desktop.GUI_WarehouseManagement
{
    /// <summary>Danh sách vị trí lưu trữ (StorageBin) của 1 khu vực cụ thể —
    /// thêm vị trí mới với đúng tọa độ + kích thước THẬT (mét) để hiện đúng
    /// chỗ trên sơ đồ, không đè lên vị trí khác.</summary>
    public partial class ManageBinsForm : BaseForm
    {
        private ApiServices? _api;
        private int _zoneId;
        private string _zoneName = "";
        private decimal _zoneWidth;
        private decimal _zoneHeight;
        private List<StorageBinItem> _bins = new();

        public ManageBinsForm()
        {
            InitializeComponent();
            Theme.StyleGrid(grid);
        }

        public ManageBinsForm(ApiServices api, int zoneId, string zoneName, decimal zoneWidth, decimal zoneHeight) : this()
        {
            _api = api;
            _zoneId = zoneId;
            _zoneName = zoneName;
            _zoneWidth = zoneWidth;
            _zoneHeight = zoneHeight;
            Text = $"Vị trí lưu trữ — {zoneName}";
            Load += async (_, _) => await LoadAsync();
        }

        private async Task LoadAsync()
        {
            if (_api is null) return;
            lblStatus.Text = "Đang tải...";
            try
            {
                _bins = await _api.Warehouses.GetBinsAsync(_zoneId);
                grid.DataSource = _bins.Select(b => new
                {
                    b.Id,
                    Mã_vị_trí = b.Code,
                    Vị_trí_X = b.PosX,
                    Vị_trí_Y = b.PosY,
                    Rộng_m = b.WidthMeters,
                    Cao_m = b.HeightMeters,
                    Sức_chứa_m3 = b.CapacityM3,
                    Đã_dùng_m3 = b.CurrentUsedM3,
                    Tải_tối_đa_kg = b.MaxWeightKg,
                }).ToList();
                grid.DataBindingComplete += (_, _) => { if (grid.Columns["Id"] != null) grid.Columns["Id"]!.Visible = false; };
                lblStatus.Text = $"{_bins.Count} vị trí lưu trữ";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }

        private async void BtnDeleteBin_Click(object? sender, EventArgs e)
        {
            if (_api is null || grid.CurrentRow is null)
            {
                MessageBox.Show(FindForm(), "Vui lòng chọn 1 vị trí trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var binId = (int)grid.CurrentRow.Cells["Id"].Value;
            var binCode = _bins.First(b => b.Id == binId).Code;

            var confirm = MessageBox.Show(FindForm(), $"Xóa hẳn vị trí \"{binCode}\"? Không thể hoàn tác.",
                "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            try
            {
                var result = await _api.Warehouses.DeleteBinAsync(binId);
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

        private async void BtnEditBin_Click(object? sender, EventArgs e)
        {
            if (_api is null || grid.CurrentRow is null)
            {
                MessageBox.Show(FindForm(), "Vui lòng chọn 1 vị trí trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var binId = (int)grid.CurrentRow.Cells["Id"].Value;
            var bin = _bins.First(b => b.Id == binId);

            using var f = new EditBinForm(_api, bin, _zoneName, _zoneWidth, _zoneHeight);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
        }

        private async void BtnAddBin_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;
            using var f = new CreateBinForm(_api, _zoneId, _zoneName, _zoneWidth, _zoneHeight);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
        }
    }
}
