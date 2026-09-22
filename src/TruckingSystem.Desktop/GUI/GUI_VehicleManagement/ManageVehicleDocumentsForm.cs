using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_VehicleManagement;

namespace TruckingSystem.Desktop.GUI_VehicleManagement
{
    /// <summary>Danh sách giấy tờ (đăng kiểm/bảo hiểm/giấy phép vận tải) của 1
    /// xe cụ thể — Greedy đã kiểm tra hạn các giấy tờ này khi chia xe, đây là
    /// nơi thực sự NHẬP các giấy tờ đó vào hệ thống.</summary>
    public partial class ManageVehicleDocumentsForm : BaseForm
    {
        private ApiServices? _api;
        private int _vehicleId;
        private List<VehicleDocumentItem> _documents = new();

        public ManageVehicleDocumentsForm()
        {
            InitializeComponent();
            Theme.StyleGrid(grid);
        }

        public ManageVehicleDocumentsForm(ApiServices api, int vehicleId, string plateNumber) : this()
        {
            _api = api;
            _vehicleId = vehicleId;
            Text = $"Giấy tờ xe — {plateNumber}";
            Load += async (_, _) => await LoadAsync();
        }

        private async Task LoadAsync()
        {
            if (_api is null) return;
            lblStatus.Text = "Đang tải...";
            try
            {
                _documents = await _api.Vehicles.GetDocumentsAsync(_vehicleId);
                var today = DateTime.Today;
                grid.DataSource = _documents.Select(d => new
                {
                    d.Id,
                    Loại_giấy_tờ = TranslateDocType(d.DocType),
                    Ngày_cấp = d.IssueDate.ToString("dd/MM/yyyy"),
                    Ngày_hết_hạn = d.ExpiryDate.ToString("dd/MM/yyyy"),
                    Trạng_thái = d.ExpiryDate < today ? "⚠️ Đã hết hạn" : "✅ Còn hạn",
                }).ToList();
                grid.DataBindingComplete += (_, _) => { if (grid.Columns["Id"] != null) grid.Columns["Id"]!.Visible = false; };
                lblStatus.Text = $"{_documents.Count} giấy tờ";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }

        private async void BtnAddDocument_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;
            using var f = new CreateVehicleDocumentForm(_api, _vehicleId);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
        }

        private async void BtnDeleteDocument_Click(object? sender, EventArgs e)
        {
            if (_api is null || grid.CurrentRow is null)
            {
                MessageBox.Show(FindForm(), "Vui lòng chọn 1 giấy tờ trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var id = (int)grid.CurrentRow.Cells["Id"].Value;

            var confirm = MessageBox.Show(FindForm(), "Xóa hẳn giấy tờ này? Không thể hoàn tác.",
                "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            try
            {
                var result = await _api.Vehicles.DeleteDocumentAsync(id);
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

        private static string TranslateDocType(string s) => s switch
        {
            "Registration" => "Đăng kiểm",
            "Insurance" => "Bảo hiểm",
            "TransportPermit" => "Giấy phép vận tải",
            _ => "Khác",
        };
    }
}