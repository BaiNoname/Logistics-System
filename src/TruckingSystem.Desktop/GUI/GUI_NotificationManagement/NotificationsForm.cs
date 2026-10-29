using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_NotificationManagement;

namespace TruckingSystem.Desktop.GUI_NotificationManagement
{
    /// <summary>Danh sách thông báo theo kiểu "VIỆC CẦN LÀM" (không phải "đã đọc"):
    /// - 🔵 = CHƯA XỬ LÝ. Bấm / xem 1 dòng KHÔNG làm mất 🔵 nữa — lỡ bấm xem mà chưa làm thì
    ///   vẫn còn đó, không bị sót.
    /// - Thông báo gắn với việc cụ thể (chuyến chờ duyệt, chờ nhập kho, chuyến bị chặn, hạn giao,
    ///   giấy tờ xe / tài xế, lịch tài xế) TỰ chuyển "Đã xử lý" (hoặc tự biến mất) khi việc đó
    ///   thật sự xong — server kiểm tra lại mỗi lần mở form / bấm "Làm mới". Không đánh dấu tay được.
    /// - Chỉ thông báo thông tin (loại "Khác") mới có nút "✔ Đánh dấu đã xử lý".
    /// - NHẤP ĐÚP (hoặc "↗ Mở màn xử lý") -> đóng form, MainForm mở thẳng màn hình của việc đó
    ///   (Chuyến / Đơn / Đội xe / Tài xế / Kho) và lọc sẵn đúng chuyến / đơn / xe / tài xế.
    /// - Lọc theo Loại / Trạng thái / từ khóa ngay trên máy, không gọi API.</summary>
    public partial class NotificationsForm : BaseForm
    {
        private const string AllText = "Tất cả";
        private const string StateTodo = "Chưa xử lý";
        private const string StateDone = "Đã xử lý";
        private static readonly string[] Categories = { "Chuyến chờ duyệt", "Chờ nhập kho", "Chuyến bị chặn", "Hạn giao", "Giấy tờ xe", "Giấy tờ tài xế", "Lịch tài xế", "Khác" };

        private ApiServices? _api;
        private List<NotificationItem> _notifications = new();
        private bool _loading;

        /// <summary>Việc người dùng chọn mở (nhấp đúp) — MainForm đọc sau khi form đóng với DialogResult.OK.</summary>
        public NotificationTarget? SelectedTarget { get; private set; }

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
            grid.ShowCellToolTips = true;
        }

        private void SetupFilters()
        {
            cboCategory.Items.Clear();
            cboCategory.Items.Add(AllText);
            cboCategory.Items.AddRange(Categories);
            cboCategory.SelectedIndex = 0;

            cboReadState.Items.Clear();
            cboReadState.Items.AddRange(new object[] { AllText, StateTodo, StateDone });
            cboReadState.SelectedIndex = 1; // mặc định: chỉ hiện việc CHƯA xử lý

            cboCategory.SelectedIndexChanged += (_, _) => ApplyFilter();
            cboReadState.SelectedIndexChanged += (_, _) => ApplyFilter();
            txtSearch.TextChanged += (_, _) => ApplyFilter();
        }

        /// <summary>Gọi API: server quét cảnh báo + cập nhật việc đã xong + trả danh sách.
        /// Chỉ chạy khi mở form hoặc bấm "Làm mới".</summary>
        private async Task LoadAsync()
        {
            if (_api is null || _loading) return;
            _loading = true;
            btnRefresh.Enabled = false;
            lblStatus.ForeColor = Theme.TextSecondary;
            lblStatus.Text = "Đang kiểm tra việc cần xử lý...";
            try
            {
                _notifications = await _api.Notifications.GetMyNotificationsAsync(refresh: true) ?? new();
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
            var state = cboReadState.SelectedItem as string ?? AllText;
            var keyword = txtSearch.Text.Trim();

            var filtered = _notifications.Where(n =>
                (category == AllText || n.CategoryText == category) &&
                (state == AllText || (state == StateTodo ? !n.IsDone : n.IsDone)) &&
                (keyword.Length == 0
                    || n.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || n.Message.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                // Chưa xử lý lên trước, rồi mới nhất trước.
                .OrderBy(n => n.IsDone)
                .ThenByDescending(n => n.CreatedAt)
                .ToList();

            var selectedId = SelectedItem()?.Id;
            grid.SuspendLayout();
            grid.Rows.Clear();
            foreach (var n in filtered)
            {
                var idx = grid.Rows.Add(n.Id, n.IsDone ? "✔" : "🔵", n.CategoryText, n.Title, n.Message,
                    n.CreatedAtLocal.ToString("dd/MM/yyyy HH:mm"));
                var row = grid.Rows[idx];
                if (!n.IsDone) row.DefaultCellStyle.Font = Theme.FontBold;
                else
                {
                    row.DefaultCellStyle.ForeColor = Theme.TextSecondary;
                    row.Cells["colRead"].Style.ForeColor = Theme.Success;
                }
                row.Cells["colCategory"].Style.ForeColor = CategoryColor(n.CategoryText);
                var hint = n.Target is { } t ? "Nhấp đúp để mở: " + t.Describe() : "Thông báo thông tin — không có màn xử lý riêng";
                row.Cells["colTitle"].ToolTipText = hint;
                row.Cells["colRead"].ToolTipText = n.IsDone ? "Đã xử lý"
                    : n.IsAutoTracked ? "Chưa xử lý — sẽ tự chuyển Đã xử lý khi việc được làm xong" : "Chưa xử lý";
            }
            grid.ResumeLayout();
            grid.ClearSelection();
            if (selectedId.HasValue)
            {
                foreach (DataGridViewRow r in grid.Rows)
                    if (r.Cells["colId"].Value is int id && id == selectedId.Value) { r.Selected = true; break; }
            }

            UpdateStatus(filtered.Count);
            UpdateButtons();
        }

        private void UpdateStatus(int shownCount)
        {
            int Todo(string c) => _notifications.Count(n => !n.IsDone && n.CategoryText == c);
            var todoTotal = _notifications.Count(n => !n.IsDone);

            var parts = Categories.Where(c => Todo(c) > 0).Select(c => $"{c}: {Todo(c)}").ToList();
            lblStatus.ForeColor = todoTotal > 0 ? Theme.Primary : Theme.TextSecondary;
            lblStatus.Text =
                $"Hiển thị {shownCount}/{_notifications.Count} • " +
                (todoTotal > 0 ? $"🔵 {todoTotal} việc chưa xử lý" : "✔ Không còn việc nào chưa xử lý") +
                (parts.Count > 0 ? "   (" + string.Join(" • ", parts) + ")" : "") +
                "   — Nhấp đúp 1 dòng để mở thẳng màn hình xử lý.";
        }

        private static Color CategoryColor(string category) => category switch
        {
            "Chuyến chờ duyệt" => Theme.Success,
            "Chờ nhập kho" => Theme.Warning,
            "Chuyến bị chặn" => Theme.Danger,
            "Giấy tờ xe" => Theme.Danger,
            "Giấy tờ tài xế" => Theme.Danger,
            "Hạn giao" => Theme.Danger,
            "Lịch tài xế" => Theme.Warning,
            _ => Theme.TextSecondary,
        };

        private NotificationItem? SelectedItem()
        {
            var row = grid.SelectedRows.Count > 0 ? grid.SelectedRows[0]
                : grid.SelectedCells.Count > 0 ? grid.Rows[grid.SelectedCells[0].RowIndex] : null;
            if (row is null) return null;
            return row.Cells["colId"].Value is int id ? _notifications.FirstOrDefault(n => n.Id == id) : null;
        }

        /// <summary>"↗ Mở" chỉ bật khi dòng chọn có màn xử lý; "✔ Đánh dấu" chỉ cho thông báo
        /// thông tin (loại "Khác") chưa xử lý — việc thật thì phải làm xong mới tự hết.</summary>
        private void UpdateButtons()
        {
            var n = SelectedItem();
            btnOpen.Enabled = n?.Target is not null;
            btnMarkDone.Enabled = n is { IsDone: false, IsAutoTracked: false };
        }

        private void Grid_SelectionChanged(object? sender, EventArgs e) => UpdateButtons();

        private void Grid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            OpenSelected();
        }

        private void BtnOpen_Click(object? sender, EventArgs e) => OpenSelected();

        /// <summary>Đóng form và báo MainForm mở màn hình của việc này. KHÔNG đánh dấu gì —
        /// việc chỉ "Đã xử lý" khi làm xong thật.</summary>
        private void OpenSelected()
        {
            var n = SelectedItem();
            if (n is null) return;
            if (n.Target is not { } target)
            {
                lblStatus.ForeColor = Theme.TextSecondary;
                lblStatus.Text = "Thông báo này chỉ để thông tin, không có màn xử lý riêng — đọc xong bấm \"✔ Đánh dấu đã xử lý\".";
                return;
            }
            SelectedTarget = target;
            DialogResult = DialogResult.OK;
            Close();
        }

        private async void BtnRefresh_Click(object? sender, EventArgs e) => await LoadAsync();

        /// <summary>Đánh dấu tay — chỉ thông báo thông tin (loại "Khác"). Server cũng chặn lại
        /// nếu là thông báo gắn với việc cụ thể.</summary>
        private async void BtnMarkDone_Click(object? sender, EventArgs e)
        {
            if (_api is null || SelectedItem() is not { } n || n.IsDone || n.IsAutoTracked) return;
            btnMarkDone.Enabled = false;
            try
            {
                await _api.Notifications.MarkAsReadAsync(n.Id);
                n.IsRead = true;
                ApplyFilter();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
                UpdateButtons();
            }
        }
    }
}
