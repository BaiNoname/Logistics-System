using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_AuditLogManagement;

namespace TruckingSystem.Desktop.GUI_AuditLogManagement
{
    /// <summary>🕘 LỊCH SỬ THAY ĐỔI của 1 đơn / chuyến / xe — mở bằng chuột phải trên màn Đơn, Chuyến, Đội xe.
    /// Gồm nhật ký của chính đối tượng + bản ghi con (đơn: các chuyến, dòng hàng, đổi hạn giao, phiếu nhập/xuất;
    /// chuyến: nhiên liệu, chi phí, sự cố, phiếu nhập/xuất; xe: giấy tờ). Trả lời ngay "ai đã làm gì, lúc nào".
    /// Bảng gọn 4 cột (mới nhất trước); bấm 1 dòng -> ô dưới hiện chi tiết từng thay đổi; có nút Xuất Excel.
    /// Form dựng bằng code (không có Designer) — không mở bằng trình thiết kế của Visual Studio.</summary>
    public sealed class EntityHistoryForm : BaseForm
    {
        private readonly ApiServices _api;
        private readonly string _entityName;
        private readonly int _entityId;
        private readonly string _displayName;

        private readonly DataGridView _grid = new() { Dock = DockStyle.Fill };
        private readonly TextBox _detail = new()
        {
            Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
            BackColor = Color.FromArgb(248, 250, 252), BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10F),
        };
        private readonly Label _status = new() { Dock = DockStyle.Fill, AutoSize = false, ForeColor = Theme.TextSecondary, TextAlign = ContentAlignment.MiddleLeft };
        private List<AuditLogItem> _logs = new();

        /// <param name="entityName">TransportOrder | Trip | Vehicle (tên lớp phía server).</param>
        /// <param name="displayName">VD "đơn TO-20261029-001", "chuyến #12", "xe 51C-123.45".</param>
        public EntityHistoryForm(ApiServices api, string entityName, int entityId, string displayName)
        {
            _api = api;
            _entityName = entityName;
            _entityId = entityId;
            _displayName = displayName;

            Text = "Lịch sử thay đổi — " + displayName;
            ClientSize = new Size(980, 640);
            MinimumSize = new Size(700, 460);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Theme.ContentBg;

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(16, 12, 16, 12) };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            root.Controls.Add(new Label
            {
                Text = "🕘 Lịch sử thay đổi — " + displayName,
                Font = Theme.FontSectionTitle, ForeColor = Theme.TextPrimary, AutoSize = true, Margin = new Padding(0, 0, 0, 4),
            }, 0, 0);
            root.Controls.Add(_status, 0, 1);
            root.Controls.Add(_grid, 0, 2);
            _detail.Margin = new Padding(0, 8, 0, 0);
            root.Controls.Add(_detail, 0, 3);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Margin = new Padding(0, 8, 0, 0) };
            var close = Theme.MakeSecondaryButton("Đóng", 110);
            close.Click += (_, _) => Close();
            var export = Theme.MakeSecondaryButton("📥 Xuất Excel", 140);
            export.Click += (_, _) => ExportExcel();
            var reload = Theme.MakeSecondaryButton("🔄 Làm mới", 120);
            reload.Click += async (_, _) => await LoadAsync();
            buttons.Controls.Add(close);
            buttons.Controls.Add(export);
            buttons.Controls.Add(reload);
            root.Controls.Add(buttons, 0, 4);
            Controls.Add(root);
            CancelButton = close;

            Theme.StyleGrid(_grid);
            GridDecor.HoverRows(_grid);
            _grid.ReadOnly = true;
            _grid.AllowUserToAddRows = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.MultiSelect = false;
            GridDecor.Pills(_grid, "Hành_động", (_, row) => LogAt(row) is { } l ? AuditLabels.PillTone(l.Action) : null);
            _grid.DataBindingComplete += (_, _) =>
            {
                if (_grid.Columns["Id"] != null) _grid.Columns["Id"]!.Visible = false;
                foreach (var (name, weight) in new[] { ("Thời_gian", 20f), ("Người", 18f), ("Hành_động", 24f), ("Đối_tượng", 38f) })
                    if (_grid.Columns[name] is { } col) { col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; col.FillWeight = weight; }
            };
            _grid.SelectionChanged += (_, _) => ShowDetail();

            Load += async (_, _) => await LoadAsync();
        }

        private AuditLogItem? LogAt(int row)
            => row >= 0 && row < _grid.Rows.Count && _grid.Rows[row].Cells["Id"].Value is int id
                ? _logs.FirstOrDefault(l => l.Id == id) : null;

        private async Task LoadAsync()
        {
            _status.ForeColor = Theme.TextSecondary;
            _status.Text = "Đang tải lịch sử...";
            try
            {
                _logs = await _api.AuditLogs.GetHistoryAsync(_entityName, _entityId) ?? new();
                if (IsDisposed) return;
                _grid.DataSource = _logs.Select(l => new
                {
                    l.Id,
                    Thời_gian = l.Timestamp.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss"),
                    Người = AuditLabels.UserText(l),
                    Hành_động = AuditLabels.Action(l.Action),
                    Đối_tượng = AuditLabels.EntityText(l),
                }).ToList();
                _status.Text = _logs.Count == 0
                    ? $"Chưa có thay đổi nào được ghi nhận cho {_displayName}."
                    : $"{_logs.Count} thay đổi (mới nhất trước) — bấm 1 dòng để xem chi tiết.";
                ShowDetail();
            }
            catch (Exception ex)
            {
                if (IsDisposed) return;
                _status.ForeColor = Theme.Danger;
                _status.Text = "Không tải được lịch sử: " + ex.Message;
            }
        }

        private void ShowDetail()
        {
            var log = _grid.CurrentRow is { } r ? LogAt(r.Index) : null;
            _detail.Text = log is not null ? AuditLabels.DetailText(log)
                : _logs.Count == 0 ? "" : "Bấm 1 dòng trong bảng để xem chi tiết thay đổi.";
        }

        private void ExportExcel()
        {
            var rows = _logs.Select(l => new object?[]
            {
                l.Timestamp.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss"),
                AuditLabels.UserText(l),
                string.Join(", ", AuditLabels.Roles(l)),
                AuditLabels.Action(l.Action),
                AuditLabels.EntityText(l),
                AuditLabels.DetailSummary(l),
            }).ToList();
            ExcelExport.Export(this, "Lich su", "LichSu_" + new string(_displayName.Where(char.IsLetterOrDigit).ToArray()),
                new[] { "Thời gian", "Người", "Vai trò", "Hành động", "Đối tượng", "Nội dung" }, rows);
        }
    }
}
