using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_NotificationManagement;

namespace TruckingSystem.Desktop.GUI_NotificationManagement
{
    /// <summary>Danh sách thông báo: chuyến chờ duyệt (tài xế báo xong), giấy tờ
    /// xe / giấy tờ tài xế (bằng lái, khám sức khỏe) sắp/đã hết hạn, tài xế chưa đăng ký ca ngày mai.
    /// - Mở form / bấm "Làm mới" -> server quét lại cảnh báo. Cảnh báo đã có thì
    ///   KHÔNG tạo thêm (dù đã đọc), cảnh báo hết hiệu lực tự biến mất.
    /// - Bấm vào 1 dòng -> chỉ đánh dấu đã đọc dòng đó, KHÔNG quét lại.
    /// - Lọc theo Loại / Trạng thái đọc / từ khóa ngay trên máy, không gọi API.</summary>
    public partial class NotificationsForm : BaseForm
    {
        private const string AllText = "Tất cả";
        private static readonly string[] Categories = { "Chuyến chờ duyệt", "Hạn giao", "Giấy tờ xe", "Giấy tờ tài xế", "Lịch tài xế", "Khác" };

        private ApiServices? _api;
        private List<NotificationItem> _notifications = new();
        private bool _loading;

        public NotificationsForm()
        {
            InitializeComponent();
            Theme.StyleGrid(grid);
            SetupGrid();
            SetupFilters();
        }

        public NotificationsForm(ApiServices api) : this()
        {
            _api = api;
            Load += async (_, _) => await LoadAsync();
        }

        private void SetupGrid()
        {
            grid.AutoGenerateColumns = false;
            grid.Columns.Clear();
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId", Visible = false });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colRead", HeaderText = "", FillWeight = 5 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colCategory", HeaderText = "Loại", FillWeight = 16 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTitle", HeaderText = "Tiêu đề", FillWeight = 30 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colMessage", HeaderText = "Nội dung", FillWeight = 49 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTime", HeaderText = "Thời gian", FillWeight = 15 });

            // Nội dung dài -> xuống dòng, dòng tự cao theo nội dung.
            grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            grid.DefaultCellStyle.Padding = new Padding(6, 6, 6, 6);
        }

        private void SetupFilters()
        {
            cboCategory.Items.Clear();
            cboCategory.Items.Add(AllText);
            cboCategory.Items.AddRange(Categories);
            cboCategory.SelectedIndex = 0;

            cboReadState.Items.Clear();
            cboReadState.Items.AddRange(new object[] { AllText, "Chưa đọc", "Đã đọc" });
            cboReadState.SelectedIndex = 0;

            cboCategory.SelectedIndexChanged += (_, _) => ApplyFilter();
            cboReadState.SelectedIndexChanged += (_, _) => ApplyFilter();
            txtSearch.TextChanged += (_, _) => ApplyFilter();
        }

        /// <summary>Gọi API: server quét cảnh báo + trả danh sách. Chỉ chạy khi mở
        /// form hoặc bấm "Làm mới".</summary>
        private async Task LoadAsync()
        {
            if (_api is null || _loading) return;
            _loading = true;
            btnRefresh.Enabled = false;
            lblStatus.ForeColor = Theme.TextSecondary;
            lblStatus.Text = "Đang quét cảnh báo mới...";
            try
            {
                _notifications = await _api.Notifications.GetMyNotificationsAsync() ?? new();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
            finally
            {
                _loading = false;
                btnRefresh.Enabled = true;
            }
        }

        /// <summary>Lọc + vẽ lại lưới từ dữ liệu ĐÃ TẢI (không gọi API).</summary>
        private void ApplyFilter()
        {
            var category = cboCategory.SelectedItem as string ?? AllText;
            var readState = cboReadState.SelectedItem as string ?? AllText;
            var keyword = txtSearch.Text.Trim();

            var filtered = _notifications.Where(n =>
                (category == AllText || n.CategoryText == category) &&
                (readState == AllText || (readState == "Chưa đọc" ? !n.IsRead : n.IsRead)) &&
                (keyword.Length == 0
                    || n.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || n.Message.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                // Chưa đọc lên trước, rồi mới nhất trước.
                .OrderBy(n => n.IsRead)
                .ThenByDescending(n => n.CreatedAt)
                .ToList();

            var selectedId = grid.CurrentRow?.Cells["colId"].Value as int?;
            grid.SuspendLayout();
            grid.Rows.Clear();
            foreach (var n in filtered)
            {
                var idx = grid.Rows.Add(n.Id, n.IsRead ? "" : "🔵", n.CategoryText, n.Title, n.Message,
                    n.CreatedAtLocal.ToString("dd/MM/yyyy HH:mm"));
                var row = grid.Rows[idx];
                if (!n.IsRead) row.DefaultCellStyle.Font = Theme.FontBold;
                else row.DefaultCellStyle.ForeColor = Theme.TextSecondary;
                row.Cells["colCategory"].Style.ForeColor = CategoryColor(n.CategoryText);
            }
            grid.ResumeLayout();
            grid.ClearSelection();
            if (selectedId.HasValue)
            {
                foreach (DataGridViewRow r in grid.Rows)
                    if (r.Cells["colId"].Value is int id && id == selectedId.Value) { r.Selected = true; break; }
            }

            UpdateStatus(filtered.Count);
        }

        private void UpdateStatus(int shownCount)
        {
            int Unread(string c) => _notifications.Count(n => !n.IsRead && n.CategoryText == c);
            var unreadTotal = _notifications.Count(n => !n.IsRead);

            lblStatus.ForeColor = unreadTotal > 0 ? Theme.Primary : Theme.TextSecondary;
            lblStatus.Text =
                $"Hiển thị {shownCount}/{_notifications.Count} • " +
                (unreadTotal > 0 ? $"🔵 {unreadTotal} chưa đọc" : "Không có thông báo chưa đọc") +
                $"   (Chuyến chờ duyệt: {Unread("Chuyến chờ duyệt")} • Hạn giao: {Unread("Hạn giao")} • Giấy tờ xe: {Unread("Giấy tờ xe")} • Giấy tờ tài xế: {Unread("Giấy tờ tài xế")} • Lịch tài xế: {Unread("Lịch tài xế")})";
            btnMarkAllRead.Enabled = unreadTotal > 0;
        }

        private static Color CategoryColor(string category) => category switch
        {
            "Chuyến chờ duyệt" => Theme.Success,
            "Giấy tờ xe" => Theme.Danger,
            "Giấy tờ tài xế" => Theme.Danger,
            "Hạn giao" => Theme.Danger,
            "Lịch tài xế" => Theme.Warning,
            _ => Theme.TextSecondary,
        };

        /// <summary>Bấm vào 1 dòng -> đánh dấu đã đọc dòng đó. Chỉ cập nhật tại
        /// chỗ, KHÔNG tải lại / quét lại (trước đây tải lại toàn bộ sau mỗi lần
        /// bấm, gây cảm giác "tự thêm dòng").</summary>
        private async void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (_api is null || e.RowIndex < 0) return;
            if (grid.Rows[e.RowIndex].Cells["colId"].Value is not int id) return;
            var notification = _notifications.FirstOrDefault(n => n.Id == id);
            if (notification is null || notification.IsRead) return;

            try
            {
                await _api.Notifications.MarkAsReadAsync(id);
                notification.IsRead = true;
                ApplyFilter();
            }
            catch { /* không chặn UI nếu lỗi đánh dấu đã đọc — không quan trọng bằng việc xem nội dung */ }
        }

        private async void BtnRefresh_Click(object? sender, EventArgs e) => await LoadAsync();

        private async void BtnMarkAllRead_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;
            try
            {
                await _api.Notifications.MarkAllAsReadAsync();
                foreach (var n in _notifications) n.IsRead = true;
                ApplyFilter();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }
    }
}
