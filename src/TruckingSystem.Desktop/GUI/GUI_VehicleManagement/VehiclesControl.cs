using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_VehicleManagement;

namespace TruckingSystem.Desktop.GUI_VehicleManagement
{
    public partial class VehiclesControl : BaseUserControl
    {
        private ApiServices? _api;
        private List<VehicleListItem> _vehicles = new();

        public VehiclesControl()
        {
            InitializeComponent();
            Theme.StyleGrid(grid);
            grid.DataBindingComplete += (_, _) => { if (grid.Columns["Id"] != null) grid.Columns["Id"]!.Visible = false; };
            Resize += (_, _) => { grid.Width = Width; grid.Height = Height - grid.Top; };
            // Sửa xe / Giấy tờ xe chỉ bấm được khi đã chọn 1 xe
            grid.SelectionChanged += (_, _) => btnEdit.Enabled = btnDocuments.Enabled = grid.CurrentRow is not null;
            grid.DataBindingComplete += (_, _) => btnEdit.Enabled = btnDocuments.Enabled = grid.CurrentRow is not null;
            Load += async (_, _) => await LoadAsync();
        }

        public VehiclesControl(ApiServices api) : this()
        {
            _api = api;
        }

        private void BtnRefresh_Click(object? sender, EventArgs e) => _ = LoadAsync();

        private async void BtnCreate_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;
            using var f = new CreateVehicleForm(_api);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
        }

        private void BtnDocuments_Click(object? sender, EventArgs e)
        {
            if (_api is null || grid.CurrentRow is null)
            {
                MessageBox.Show(FindForm(), "Vui lòng chọn 1 xe trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var id = (int)grid.CurrentRow.Cells["Id"].Value;
            var vehicle = _vehicles.First(v => v.Id == id);

            using var f = new ManageVehicleDocumentsForm(_api, id, vehicle.PlateNumber);
            f.ShowDialog(FindForm());
            _ = LoadAsync(); // giấy tờ có thể vừa thêm/xóa -> cột "Giấy tờ" cập nhật
        }

        private async void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (_api is null || grid.CurrentRow is null)
            {
                MessageBox.Show(FindForm(), "Vui lòng chọn 1 xe trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var id = (int)grid.CurrentRow.Cells["Id"].Value;
            var vehicle = _vehicles.First(v => v.Id == id);

            using var f = new EditVehicleForm(_api, vehicle);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
        }

        private async Task LoadAsync()
        {
            if (_api is null) return;
            lblStatus.Text = "Đang tải...";
            try
            {
                _vehicles = await _api.Vehicles.GetVehiclesAsync();
                grid.DataSource = _vehicles.Select(v => new
                {
                    v.Id,
                    Biển_số = v.PlateNumber,
                    Loại_xe = v.VehicleType,
                    Sức_chứa = v.CapacityQty,
                    Trạng_thái = TranslateStatus(v.Status),
                    // Lý do giấy tờ (server tính, cùng quy tắc Chia xe / Bắt đầu chuyến) — xe "Rảnh"
                    // nhưng hết Đăng kiểm sẽ KHÔNG được chia xe.
                    Giấy_tờ = v.DocumentBlockReason is null ? "✅ Hợp lệ" : "⛔ " + v.DocumentBlockReason,
                }).ToList();
                lblStatus.Text = $"Tổng {_vehicles.Count} xe — chọn 1 dòng rồi bấm Sửa để chỉnh sửa";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }

        private static string TranslateStatus(string s) => s switch
        {
            // Cùng cách gọi với form Sửa xe.
            "Available" => "Rảnh",
            "OnTrip" => "Đang chạy",
            "UnderMaintenance" => "Bảo trì",
            "OutOfService" => "Ngưng hoạt động",
            _ => s,
        };
    }
}