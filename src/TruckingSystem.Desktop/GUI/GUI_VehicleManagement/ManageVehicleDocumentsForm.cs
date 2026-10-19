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
            grid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await OpenSelectedFileAsync(); };
        }

        /// <summary>Nhấp đúp 1 giấy tờ có file scan -> tải file từ server (kèm token
        /// đăng nhập) về thư mục tạm rồi mở bằng chương trình mặc định (PDF/ảnh).</summary>
        private async Task OpenSelectedFileAsync()
        {
            if (_api is null || grid.CurrentRow?.Cells["Id"].Value is not int id) return;
            var doc = _documents.FirstOrDefault(d => d.Id == id);
            if (doc is null || string.IsNullOrWhiteSpace(doc.FileUrl))
            {
                lblStatus.Text = "Giấy tờ này chưa đính kèm file scan.";
                return;
            }
            try
            {
                string path;
                if (doc.FileUrl.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
                {
                    var bytes = await _api.Connection.Http.GetByteArrayAsync(doc.FileUrl);
                    path = Path.Combine(Path.GetTempPath(), $"giayto-{doc.Id}{Path.GetExtension(doc.FileUrl)}");
                    await File.WriteAllBytesAsync(path, bytes);
                }
                else
                {
                    path = doc.FileUrl; // dữ liệu cũ: đường dẫn trên máy đã nhập
                    if (!File.Exists(path))
                    {
                        lblStatus.Text = "Không tìm thấy file trên máy này (giấy tờ cũ lưu đường dẫn máy khác) — hãy thêm lại giấy tờ kèm file.";
                        return;
                    }
                }
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Không mở được file: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
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
                // Gia hạn = thêm bản MỚI, bản cũ vẫn giữ làm lịch sử. Chỉ bản mới nhất của
                // mỗi loại mới quyết định xe còn hạn hay không (khớp quy tắc Chia xe).
                var latestIdByType = _documents
                    .Where(d => d.DocType != "Other")
                    .GroupBy(d => d.DocType)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.ExpiryDate).First().Id);
                string StatusOf(VehicleDocumentItem d)
                {
                    if (latestIdByType.TryGetValue(d.DocType, out var latestId) && latestId != d.Id)
                        return "↻ Đã gia hạn (bản cũ)";
                    return d.ExpiryDate.Date < today ? "⚠️ Đã hết hạn" : "✅ Còn hạn";
                }
                grid.DataSource = _documents.Select(d => new
                {
                    d.Id,
                    Loại_giấy_tờ = TranslateDocType(d.DocType),
                    Ngày_cấp = d.IssueDate.ToString("dd/MM/yyyy"),
                    Ngày_hết_hạn = d.ExpiryDate.ToString("dd/MM/yyyy"),
                    Trạng_thái = StatusOf(d),
                    File_scan = string.IsNullOrWhiteSpace(d.FileUrl) ? "" : "📎 Nhấp đúp để mở",
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
            "TransportPermit" => "Phù hiệu xe (GP vận tải)",
            _ => "Khác",
        };
    }
}