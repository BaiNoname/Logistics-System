using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_LeaveManagement;
using TruckingSystem.Desktop.SERVICE_LeaveManagement;

namespace TruckingSystem.Desktop.GUI_LeaveManagement;

/// <summary>Ngày nghỉ của một nhân viên: xem theo tháng, thêm theo khoảng ngày, xóa từng ngày hoặc cả đơn; hiện quỹ phép năm.</summary>
public sealed class LeaveForm : BaseForm
{
    private static readonly DateTime MinDate = new(2000, 1, 1);
    private static readonly DateTime MaxDate = new(2100, 12, 31);

    private static readonly Dictionary<string, string> GroupLabels = new()
    {
        ["AnnualQuota"] = "Phép năm",
        ["PaidSpecial"] = "Hưởng lương riêng",
        ["UnpaidInsurance"] = "Không lương (BHXH)",
        ["Unpaid"] = "Không lương",
    };

    private sealed record TypeItem(LeaveTypeDto Type)
    {
        public override string ToString()
            => $"{(GroupLabels.TryGetValue(Type.Group, out var g) ? g : Type.Group)} — {Type.Name}";
    }

    public LeaveForm(LeaveApiService api, int employeeId, string employeeCode, string employeeName)
    {
        Text = $"Ngày nghỉ — {employeeCode} {employeeName}"; Size = new Size(860, 640); StartPosition = FormStartPosition.CenterParent;

        // --- Thanh nhập ---
        var type = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 300 };
        var limit = new Label { AutoSize = true, Margin = new Padding(0, 8, 0, 0), ForeColor = Theme.Danger };
        var from = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 120, MinDate = MinDate, MaxDate = MaxDate, Value = DateTime.Today };
        var to = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 120, MinDate = MinDate, MaxDate = MaxDate, Value = DateTime.Today };
        var note = new TextBox { Width = 300, PlaceholderText = "Ghi chú" };
        var add = Theme.MakePrimaryButton("Thêm", 100);
        var preview = new Label { AutoSize = true, MaximumSize = new Size(800, 0), Margin = new Padding(0, 8, 0, 0) };
        Label Caption(string t) => new() { Text = t, AutoSize = true, Margin = new Padding(0, 8, 6, 0) };

        var row1 = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
        row1.Controls.AddRange(new Control[] { Caption("Loại nghỉ"), type, limit });
        var row2 = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 6, 0, 0) };
        row2.Controls.AddRange(new Control[] { Caption("Từ"), from, Caption("Đến"), to, Caption("Ghi chú"), note, add });
        var addBox = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(8) };
        addBox.Controls.AddRange(new Control[] { row1, row2, preview });

        // --- Quỹ phép + chọn tháng ---
        var quota = new Label { AutoSize = true, Margin = new Padding(16, 8, 0, 0), Font = new Font(Font, FontStyle.Bold) };
        var month = new DateTimePicker
        {
            Format = DateTimePickerFormat.Custom, CustomFormat = "MM/yyyy", ShowUpDown = true, Width = 90,
            MinDate = MinDate, MaxDate = MaxDate, Value = DateTime.Today,
        };
        var monthBar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8, 0, 8, 4) };
        monthBar.Controls.AddRange(new Control[] { Caption("Tháng"), month, quota });

        // --- Lưới ---
        var grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false };
        foreach (var (property, title) in new[]
                 { ("LeaveDate", "Ngày"), ("TypeName", "Loại"), ("IsPaid", "Có lương"), ("Note", "Ghi chú"), ("CreatedBy", "Người nhập") })
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = property, DataPropertyName = property, HeaderText = title });
        Theme.StyleGrid(grid); GridLayout.FitColumns(grid);
        grid.Columns["LeaveDate"].DefaultCellStyle.Format = "dd/MM/yyyy";
        grid.CellFormatting += (_, e) =>
        {
            if (e.RowIndex < 0 || grid.Columns[e.ColumnIndex].Name != "IsPaid" || grid.Rows[e.RowIndex].DataBoundItem is not LeaveDto d) return;
            e.Value = d.IsPaid ? "Có" : "Không"; e.FormattingApplied = true;
        };

        var menu = new ContextMenuStrip();
        var deleteDay = menu.Items.Add("🗑 Xóa ngày này");
        var deleteBatch = menu.Items.Add("🗑 Xóa cả đơn");
        ModernMenu.Apply(menu);
        grid.ContextMenuStrip = menu;
        Disposed += (_, _) => menu.Dispose();

        Controls.Add(grid); Controls.Add(monthBar); Controls.Add(addBox);
        BackColor = Theme.CardBg;
        MinimumSize = new Size(760, 480);
        ButtonStyles.Footer(this, null, FormStyle.NewCloseButton(this));
        DialogKeys.Enable(this);
        Anim.FadeIn(this);
        Load += (_, _) => Theme.UniformButtons(row2);

        var busy = false;
        var types = new List<LeaveTypeDto>();
        menu.Opening += (_, e) =>
        {
            if (busy || grid.CurrentRow?.DataBoundItem is not LeaveDto d) { e.Cancel = true; return; }
            deleteDay.Enabled = deleteBatch.Enabled = !d.Locked;
        };
        grid.CellMouseDown += (_, e) =>
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
                grid.CurrentCell = grid.Rows[e.RowIndex].Cells[Math.Max(0, e.ColumnIndex)];
        };
        async Task Run(Func<Task> work)
        {
            if (busy) return;
            busy = true; Enabled = false;
            try { await work(); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Ngày nghỉ", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            finally { busy = false; Enabled = true; }
        }
        FormClosing += (_, e) => { if (busy) e.Cancel = true; };

        async Task Reload()
        {
            var y = month.Value.Year;
            var leaves = await api.LeavesAsync(employeeId, y, month.Value.Month);
            var q = await api.QuotaAsync(employeeId, y);
            grid.DataSource = leaves.OrderBy(l => l.LeaveDate).ToList();
            quota.Text = $"Phép năm {q.Year}: đã dùng {q.Used} / {q.Quota}, còn {q.Remaining}";
        }
        void Show(LeaveResult result)
        {
            if (!result.Success) throw new InvalidOperationException(result.Message);
            var text = string.Join(Environment.NewLine, new[] { result.Message }.Concat(result.Warnings ?? new()).Where(s => !string.IsNullOrWhiteSpace(s)));
            if (text.Length > 0)
                MessageBox.Show(this, text, "Ngày nghỉ", MessageBoxButtons.OK,
                    result.Warnings is { Count: > 0 } ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        // --- Xem trước (chỉ để hiển thị; server mới là nguồn đúng) ---
        var holidayCache = new Dictionary<int, List<HolidayDto>>();
        var previewVersion = 0;
        async Task UpdatePreview()
        {
            var version = ++previewVersion;
            var start = from.Value.Date; var end = to.Value.Date;
            if (end < start) { preview.Text = "Khoảng ngày không hợp lệ (Đến trước Từ)."; return; }
            var holidays = new Dictionary<DateTime, string>();
            try
            {
                for (var y = start.Year; y <= end.Year; y++)
                {
                    if (!holidayCache.TryGetValue(y, out var list)) holidayCache[y] = list = await api.HolidaysAsync(y);
                    foreach (var h in list) holidays[h.Date.Date] = h.Name;
                }
            }
            catch { /* xem trước không có ngày lễ; server vẫn xử lý đúng */ }
            if (version != previewVersion || IsDisposed) return;
            var count = 0; var skipped = new List<string>();
            for (var d = start; d <= end; d = d.AddDays(1))
            {
                if (d.DayOfWeek == DayOfWeek.Sunday) skipped.Add($"Chủ nhật {d:dd/MM}");
                else if (holidays.TryGetValue(d, out var n)) skipped.Add($"{n} {d:dd/MM}");
                else count++;
            }
            preview.Text = $"Sẽ tạo {count} ngày" + (skipped.Count > 0 ? $"; bỏ qua: {string.Join(", ", skipped)}" : "");
        }
        void UpdateLimit() => limit.Text = type.SelectedItem is TypeItem t ? t.Type.LimitText : "";

        type.SelectedIndexChanged += (_, _) => UpdateLimit();
        from.ValueChanged += async (_, _) =>
        {
            if (to.Value < from.Value) to.Value = from.Value;
            await UpdatePreview();
        };
        to.ValueChanged += async (_, _) => await UpdatePreview();
        month.ValueChanged += async (_, _) => await Run(Reload);

        Load += async (_, _) =>
        {
            await Run(async () =>
            {
                types = await api.TypesAsync();
                type.Items.AddRange(types.Select(t => (object)new TypeItem(t)).ToArray());
                if (type.Items.Count > 0) type.SelectedIndex = 0;
                await Reload();
            });
            await UpdatePreview();
        };

        add.Click += async (_, _) =>
        {
            if (busy) return;
            if (type.SelectedItem is not TypeItem selected)
            {
                MessageBox.Show(this, "Vui lòng chọn loại nghỉ.", "Ngày nghỉ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (to.Value.Date < from.Value.Date)
            {
                MessageBox.Show(this, "Ngày kết thúc phải sau hoặc bằng ngày bắt đầu.", "Ngày nghỉ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            await Run(async () =>
            {
                var noteText = string.IsNullOrWhiteSpace(note.Text) ? null : note.Text.Trim();
                var result = await api.AddLeaveAsync(new(employeeId, selected.Type.Type, from.Value.Date, to.Value.Date, noteText));
                Show(result);
                note.Clear();
                month.Value = from.Value.Date; // ValueChanged bị bỏ qua khi đang busy, nên Reload tường minh bên dưới
                await Reload();
            });
        };
        deleteDay.Click += async (_, _) =>
        {
            if (busy || grid.CurrentRow?.DataBoundItem is not LeaveDto row || row.Locked || MessageBox.Show(this,
                $"Xóa ngày nghỉ {row.LeaveDate:dd/MM/yyyy} ({row.TypeName})?", "Xác nhận", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            await Run(async () => { Show(await api.DeleteLeaveAsync(row.Id)); await Reload(); });
        };
        deleteBatch.Click += async (_, _) =>
        {
            if (busy || grid.CurrentRow?.DataBoundItem is not LeaveDto row || row.Locked || MessageBox.Show(this,
                $"Xóa cả đơn nghỉ (gồm ngày {row.LeaveDate:dd/MM/yyyy} và các ngày cùng đơn)?", "Xác nhận", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            await Run(async () => { Show(await api.DeleteLeaveBatchAsync(row.BatchId)); await Reload(); });
        };
    }
}
