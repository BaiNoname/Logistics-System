using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_AuditLogManagement;

namespace TruckingSystem.Desktop.GUI_AuditLogManagement
{
    /// <summary>NHẬT KÝ HỆ THỐNG — ai làm gì, lúc nào, trên đối tượng nào. Nguồn: bộ ghi tự động phía server (mọi
    /// thêm / sửa / xóa dữ liệu nghiệp vụ) + thao tác nghiệp vụ (Chia xe, Đổi hạn giao, Đăng nhập...).
    ///  • Chọn khoảng ngày rồi "Tải nhật ký" (mới nhất trước, mỗi lần 1000 dòng; "Tải thêm" lấy tiếp dòng cũ hơn).
    ///  • Lọc ngay trên máy theo Hành động / Vai trò / Đối tượng + ô tìm (tên người, mã đơn, biển số, nội dung...).
    ///  • Bảng gọn 5 cột; bấm 1 dòng -> ô "Chi tiết" hiện đủ: đối tượng nào, đổi field gì từ giá trị cũ → mới.</summary>
    public partial class AuditLogsControl : BaseUserControl
    {
        private const int PageSize = 1000;
        private ApiServices? _api;
        private List<AuditLogItem> _logs = new();
        private int _loadVersion;

        public AuditLogsControl()
        {
            InitializeComponent();
            Theme.StyleGrid(grid);
            GridLayout.FitColumns(grid);
            GridDecor.HoverRows(grid);
            GridDecor.Pills(grid, "Hành_động", (_, row) => LogAt(row) is { } l ? AuditLabels.Tone(l.Action) switch
            {
                "green" => GridDecor.Green,
                "red" => GridDecor.Red,
                "violet" => GridDecor.Violet,
                "slate" => GridDecor.Slate,
                "blue" => GridDecor.Blue,
                _ => GridDecor.Amber,
            } : null);
            grid.DataBindingComplete += (_, _) =>
            {
                if (grid.Columns["Id"] != null) grid.Columns["Id"]!.Visible = false;
                // Bảng chỉ để DÒ (khi nào, ai, làm gì, trên cái gì) — nội dung chi tiết xem ở ô bên dưới khi bấm dòng.
                foreach (var (name, weight) in new[] { ("Thời_gian", 22f), ("Người", 16f), ("Hành_động", 26f), ("Đối_tượng", 36f) })
                    if (grid.Columns[name] is { } col) { col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; col.FillWeight = weight; }
            };
            grid.SelectionChanged += (_, _) => ShowDetail();

            dtFrom.Value = DateTime.Today.AddDays(-7);
            dtTo.Value = DateTime.Today;

            filterBar.AddChoice("action", "Hành động", Array.Empty<string>());
            filterBar.AddChoice("role", "Vai trò", Array.Empty<string>());
            filterBar.SearchPlaceholder = "Tìm tên người, mã đơn, biển số, nội dung...";
            filterBar.AddChoice("entity", "Đối tượng", Array.Empty<string>());
            filterBar.FilterChanged += (_, _) => BindGrid();
            ScrollFit.Enable(this, root, grid);
            Load += async (_, _) => await LoadAsync(append: false);
        }

        public AuditLogsControl(ApiServices api) : this()
        {
            _api = api;
        }

        private AuditLogItem? LogAt(int row)
            => row >= 0 && row < grid.Rows.Count && grid.Rows[row].Cells["Id"].Value is int id
                ? _logs.FirstOrDefault(l => l.Id == id) : null;

        private async void BtnLoad_Click(object? sender, EventArgs e) => await LoadAsync(append: false);

        private async void BtnMore_Click(object? sender, EventArgs e) => await LoadAsync(append: true);

        private async Task LoadAsync(bool append)
        {
            if (_api is null) return;
            if (dtFrom.Value.Date > dtTo.Value.Date)
            {
                lblStatus.Text = "⚠️ \"Từ ngày\" phải trước hoặc bằng \"đến ngày\".";
                lblStatus.ForeColor = Theme.Warning;
                return;
            }
            var version = ++_loadVersion;
            lblStatus.Text = "Đang tải nhật ký...";
            lblStatus.ForeColor = Theme.TextSecondary;
            btnLoad.Enabled = btnMore.Enabled = false;
            try
            {
                var page = await _api.AuditLogs.GetAsync(null, dtFrom.Value.Date, dtTo.Value.Date, null, null, null,
                    append ? _logs.Count : 0, PageSize);
                if (version != _loadVersion || IsDisposed) return;
                _logs = append ? _logs.Concat(page).GroupBy(l => l.Id).Select(g => g.First()).ToList() : page;

                filterBar.SetChoiceOptions("action", _logs.Select(l => AuditLabels.Action(l.Action)));
                filterBar.SetChoiceOptions("role", _logs.SelectMany(AuditLabels.Roles));
                filterBar.SetChoiceOptions("entity", _logs.Select(l => AuditLabels.Entity(l.EntityName)));
                BindGrid();
                btnMore.Enabled = page.Count == PageSize; // còn dòng cũ hơn trong khoảng ngày
                lblStatus.Text = $"Đã tải {_logs.Count} dòng nhật ký từ {dtFrom.Value:dd/MM/yyyy} đến {dtTo.Value:dd/MM/yyyy}" +
                                 (btnMore.Enabled ? " — còn dòng cũ hơn, bấm \"Tải thêm\"." : ".") +
                                 "   Bấm 1 dòng để xem chi tiết thay đổi.";
            }
            catch (Exception ex)
            {
                if (version != _loadVersion || IsDisposed) return;
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
            finally
            {
                if (version == _loadVersion && !IsDisposed) btnLoad.Enabled = true;
            }
        }

        private static string UserText(AuditLogItem l) => string.IsNullOrWhiteSpace(l.UserName) ? AuditLabels.SystemText : l.UserName!;

        /// <summary>Lọc vai trò: 1 người có thể nhiều vai trò -> khớp nếu CÓ vai trò đang chọn.</summary>
        private bool RoleMatches(AuditLogItem l) => filterBar.Choice("role") is not string r || AuditLabels.Roles(l).Contains(r);

        private void BindGrid()
        {
            var keepId = grid.CurrentRow?.Cells["Id"]?.Value as int?;
            var rows = _logs.Where(l =>
                    filterBar.Is("action", AuditLabels.Action(l.Action)) &&
                    RoleMatches(l) &&
                    filterBar.Is("entity", AuditLabels.Entity(l.EntityName)) &&
                    filterBar.MatchesSearch(l.Detail, l.UserName, AuditLabels.Entity(l.EntityName), l.EntityId?.ToString()))
                .ToList();
            grid.DataSource = rows.Select(l => new
            {
                l.Id,
                Thời_gian = l.Timestamp.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss"),
                Người = UserText(l),
                Hành_động = AuditLabels.Action(l.Action),
                Đối_tượng = AuditLabels.Entity(l.EntityName) + (l.EntityId.HasValue ? $" #{l.EntityId}" : ""),
            }).ToList();
            filterBar.SetCount(rows.Count, _logs.Count);
            if (keepId is int id)
                foreach (DataGridViewRow r in grid.Rows)
                    if (r.Cells["Id"].Value is int rid && rid == id) { grid.CurrentCell = r.Cells["Thời_gian"]; break; }
            ShowDetail();
        }

        /// <summary>Ô chi tiết: thời gian, người, hành động, đối tượng + MỖI THAY ĐỔI 1 DÒNG ("Trạng thái: A → B").</summary>
        private void ShowDetail()
        {
            var log = grid.CurrentRow?.Cells["Id"]?.Value is int id ? _logs.FirstOrDefault(l => l.Id == id) : null;
            if (log is null)
            {
                txtDetail.Text = _logs.Count == 0 ? "Không có nhật ký nào trong khoảng ngày này." : "Bấm 1 dòng trong bảng để xem chi tiết.";
                return;
            }
            var lines = new List<string>
            {
                $"🕒 {log.Timestamp.ToLocalTime():dd/MM/yyyy HH:mm:ss}    👤 {UserText(log)}" +
                (string.IsNullOrWhiteSpace(log.UserName) ? "" : $" ({string.Join(", ", AuditLabels.Roles(log))})") +
                $"    {AuditLabels.Action(log.Action)}    " +
                $"📁 {AuditLabels.Entity(log.EntityName)}{(log.EntityId.HasValue ? $" #{log.EntityId}" : "")}",
                "",
            };
            var detail = log.Detail ?? "(không có nội dung)";
            var json = AuditLabels.JsonLines(detail);
            if (json is not null)
            {
                lines.Add("Thông tin ghi nhận:");
                lines.AddRange(json.Select(p => "   • " + p));
                txtDetail.Text = string.Join(Environment.NewLine, lines);
                return;
            }
            var cut = detail.IndexOf(" — ", StringComparison.Ordinal);
            if (cut > 0)
            {
                lines.Add(detail[..cut]);
                lines.AddRange(detail[(cut + 3)..].Split(" · ").Select(p => "   • " + AuditLabels.PrettyPart(p)));
            }
            else lines.Add(detail);
            txtDetail.Text = string.Join(Environment.NewLine, lines);
        }
    }
}
