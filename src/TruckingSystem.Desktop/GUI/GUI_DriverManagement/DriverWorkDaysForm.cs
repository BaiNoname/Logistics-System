using System.Globalization;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_DriverManagement;
using TruckingSystem.Desktop.SERVICE_DriverManagement;

namespace TruckingSystem.Desktop.GUI_DriverManagement
{
    /// <summary>Ngày làm việc của 1 tài xế — đăng ký THEO NGÀY, không chọn ca.
    /// Chia xe chỉ cần biết tài xế có đi làm ngày đó không (chuyến dài nhiều ngày
    /// thì cần đủ mọi ngày). Đăng ký theo khoảng ngày (từ – đến), tính cả Thứ 7 và
    /// Chủ nhật (tài xế không nghỉ cố định cuối tuần) — ngày nghỉ thì xóa riêng ngày đó.
    /// Ngày đã có lịch tự bỏ qua, không tạo trùng.
    /// THỐNG KÊ THEO THÁNG (góp ý của GVHD): số ngày làm trong tháng, các CHUỖI LÀM LIÊN TỤC
    /// (từ ngày – đến ngày, bao nhiêu ngày; chuỗi nối từ/sang tháng khác vẫn tính đủ độ dài) và
    /// các QUÃNG NGHỈ xen giữa — kèm lịch tháng tô màu. Chuỗi quá 6 ngày được cảnh báo (Bộ luật
    /// Lao động: mỗi tuần người lao động nghỉ ít nhất 24 giờ liên tục).</summary>
    public partial class DriverWorkDaysForm : BaseForm
    {
        private static readonly CultureInfo Vi = new("vi-VN");

        private DriverWorkDayApiService? _service;
        private int _employeeId;
        private List<DriverWorkDayItem> _days = new();
        private bool _busy;
        /// <summary>Ngày 1 của tháng đang xem.</summary>
        private DateTime _month = new(DateTime.Today.Year, DateTime.Today.Month, 1);
        /// <summary>Làm liên tục quá số ngày này thì cảnh báo (cần xếp ngày nghỉ trong tuần).</summary>
        private const int MaxConsecutiveDays = 6;

        public DriverWorkDaysForm()
        {
            InitializeComponent();
            Theme.StyleGrid(grid);
            SetupGrid();
            dtFrom.Value = DateTime.Today;
            dtTo.Value = DateTime.Today.AddDays(6);
        }

        public DriverWorkDaysForm(ApiServices api, int employeeId, string driverName) : this()
        {
            _service = new DriverWorkDayApiService(api.Connection);
            _employeeId = employeeId;
            lblTitle.Text = $"📅 Ngày làm việc — {driverName}";
            Text = $"Ngày làm việc — {driverName}";

            Load += async (_, _) => await LoadAsync();
            calendarPanel.MouseClick += CalendarPanel_MouseClick;
            dtFrom.ValueChanged += (_, _) => { if (dtTo.Value.Date < dtFrom.Value.Date) dtTo.Value = dtFrom.Value; UpdateHint(); };
            dtTo.ValueChanged += (_, _) => UpdateHint();
            UpdateHint();
        }

        /// <summary>true nếu đã đăng ký / xóa ít nhất 1 ngày (màn Tài xế tải lại trạng thái).</summary>
        public bool Changed { get; private set; }

        private void SetupGrid()
        {
            grid.AutoGenerateColumns = false;
            grid.Columns.Clear();
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colId", Visible = false });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDate", HeaderText = "Ngày", FillWeight = 25 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colWeekday", HeaderText = "Thứ", FillWeight = 25 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colNote", HeaderText = "Ghi chú", FillWeight = 50 });
        }

        private void UpdateHint()
        {
            var from = dtFrom.Value.Date;
            var to = dtTo.Value.Date;
            if (to < from)
            {
                lblHint.Text = "\"Đến ngày\" phải sau hoặc bằng \"Từ ngày\".";
                lblHint.ForeColor = Theme.Danger;
                return;
            }
            var count = (int)(to - from).TotalDays + 1;
            lblHint.Text = $"Sẽ đăng ký {count} ngày (ngày đã có lịch sẽ tự bỏ qua).";
            lblHint.ForeColor = Theme.TextSecondary;
        }

        private async Task LoadAsync()
        {
            if (_service is null) return;
            try
            {
                _days = await _service.GetWorkDaysAsync(_employeeId);
                BindGrid();
            }
            catch (Exception ex)
            {
                ShowStatus("Lỗi tải dữ liệu: " + ex.Message, Theme.Danger);
            }
        }

        private void BtnPrevMonth_Click(object? sender, EventArgs e) { _month = _month.AddMonths(-1); BindGrid(); }
        private void BtnNextMonth_Click(object? sender, EventArgs e) { _month = _month.AddMonths(1); BindGrid(); }

        private HashSet<DateTime> WorkDateSet() => _days.Select(d => d.Date.Date).ToHashSet();

        /// <summary>Các CHUỖI ngày làm liên tục (tính trên toàn bộ lịch, không cắt theo tháng —
        /// chuỗi nối từ tháng trước / sang tháng sau vẫn đúng độ dài thật).</summary>
        private List<(DateTime Start, DateTime End)> WorkRuns()
        {
            var runs = new List<(DateTime Start, DateTime End)>();
            foreach (var d in _days.Select(x => x.Date.Date).Distinct().OrderBy(x => x))
            {
                if (runs.Count > 0 && runs[^1].End.AddDays(1) == d) runs[^1] = (runs[^1].Start, d);
                else runs.Add((d, d));
            }
            return runs;
        }

        private static int Len((DateTime Start, DateTime End) r) => (int)(r.End - r.Start).TotalDays + 1;

        private static string Range((DateTime Start, DateTime End) r)
            => r.Start == r.End ? $"{r.Start:dd/MM}" : $"{r.Start:dd/MM}–{r.End:dd/MM}";

        private void BindGrid()
        {
            var today = DateTime.Today;
            var monthEnd = _month.AddMonths(1).AddDays(-1);
            lblMonth.Text = $"Tháng {_month:MM/yyyy}";

            // Danh sách bên phải: các ngày làm của THÁNG đang xem.
            var shown = _days.Where(d => d.Date.Date >= _month && d.Date.Date <= monthEnd).OrderBy(d => d.Date).ToList();
            var runs = WorkRuns();
            grid.Rows.Clear();
            foreach (var d in shown)
            {
                var run = runs.First(r => d.Date.Date >= r.Start && d.Date.Date <= r.End);
                var dayInRun = (int)(d.Date.Date - run.Start).TotalDays + 1;
                var note = d.Date.Date == today ? "Hôm nay" : d.Date.Date < today ? "Đã qua" : "";
                if (Len(run) > 1) note = (note.Length > 0 ? note + " · " : "") + $"ngày {dayInRun}/{Len(run)} liên tục";
                if (d.HasOldShiftType) note = (note.Length > 0 ? note + " · " : "") + "dữ liệu ca cũ";
                var row = grid.Rows.Add(d.Id, d.Date.ToString("dd/MM/yyyy"), Weekday(d.Date), note);
                if (d.Date.Date < today) grid.Rows[row].DefaultCellStyle.ForeColor = Theme.TextSecondary;
                if (dayInRun > MaxConsecutiveDays) grid.Rows[row].DefaultCellStyle.ForeColor = Theme.Danger;
            }
            grid.ClearSelection();

            BindMonthStats(runs, monthEnd);
            calendarPanel.Invalidate();

            var upcoming = _days.Count(d => d.Date.Date >= today);
            lblSummary.Text = upcoming == 0
                ? "Tài xế chưa đăng ký ngày làm nào từ hôm nay trở đi — sẽ KHÔNG được chọn khi Chia xe."
                : $"{upcoming} ngày làm sắp tới (từ hôm nay) · tổng {_days.Count} ngày đã đăng ký";
            lblSummary.ForeColor = upcoming == 0 ? Theme.Warning : Theme.TextPrimary;
            btnDelete.Enabled = shown.Any(d => d.Date.Date >= today);
        }

        /// <summary>Thống kê tháng: số ngày làm / nghỉ, các chuỗi làm liên tục, các quãng nghỉ.</summary>
        private void BindMonthStats(List<(DateTime Start, DateTime End)> runs, DateTime monthEnd)
        {
            var daysInMonth = (int)(monthEnd - _month).TotalDays + 1;
            var work = WorkDateSet();
            var workInMonth = work.Count(d => d >= _month && d <= monthEnd);
            var monthRuns = runs.Where(r => r.End >= _month && r.Start <= monthEnd).ToList();

            // Quãng nghỉ trong tháng = các khoảng ngày KHÔNG đăng ký làm, xen giữa / đầu / cuối tháng.
            var gaps = new List<(DateTime Start, DateTime End)>();
            DateTime? gapStart = null;
            for (var d = _month; d <= monthEnd; d = d.AddDays(1))
            {
                if (!work.Contains(d)) { gapStart ??= d; continue; }
                if (gapStart is { } gs) { gaps.Add((gs, d.AddDays(-1))); gapStart = null; }
            }
            if (gapStart is { } last) gaps.Add((last, monthEnd));

            if (workInMonth == 0)
            {
                lblMonthStats.Text = $"Tháng {_month:MM/yyyy}: chưa có ngày làm nào ({daysInMonth} ngày đều trống).";
                lblMonthStats.ForeColor = Theme.TextSecondary;
                return;
            }

            string RunText((DateTime Start, DateTime End) r)
            {
                var extra = r.Start < _month ? ", nối từ tháng trước" : r.End > monthEnd ? ", nối sang tháng sau" : "";
                var warn = Len(r) > MaxConsecutiveDays ? " ⚠️" : "";
                return $"{Range(r)} ({Len(r)} ngày{extra}){warn}";
            }

            var longest = monthRuns.Max(Len);
            var tooLong = monthRuns.Where(r => Len(r) > MaxConsecutiveDays).ToList();
            lblMonthStats.Text =
                $"📊 Tháng {_month:MM/yyyy}: {workInMonth}/{daysInMonth} ngày làm · {daysInMonth - workInMonth} ngày nghỉ · " +
                $"{monthRuns.Count} đợt làm liên tục, dài nhất {longest} ngày\n" +
                $"🟦 Làm liên tục: {string.Join(" · ", monthRuns.Select(RunText))}\n" +
                $"⬜ Quãng nghỉ: {(gaps.Count == 0 ? "không có" : string.Join(" · ", gaps.Select(g => $"{Range(g)} ({Len(g)} ngày)")))}" +
                (tooLong.Count > 0
                    ? $"\n⚠️ Có đợt làm liên tục quá {MaxConsecutiveDays} ngày — nên xếp ngày nghỉ (Bộ luật Lao động: mỗi tuần nghỉ ít nhất 24 giờ liên tục)."
                    : "");
            lblMonthStats.ForeColor = tooLong.Count > 0 ? Theme.Danger : Theme.TextPrimary;
        }

        // ================= Lịch tháng (vẽ) =================

        private const int CalendarRows = 6; // tối đa 6 tuần / tháng

        /// <summary>Ô của 1 ngày trong lịch (hàng 0 là tiêu đề Thứ). Tuần bắt đầu từ Thứ 2.</summary>
        private (Rectangle Rect, DateTime Date)[] CalendarCells()
        {
            var rc = calendarPanel.ClientRectangle;
            var headerH = LogicalToDeviceUnits(26);
            var cellW = rc.Width / 7f;
            var cellH = (rc.Height - headerH) / (float)CalendarRows;
            var offset = ((int)_month.DayOfWeek + 6) % 7; // Thứ 2 = 0
            var first = _month.AddDays(-offset);
            var cells = new (Rectangle, DateTime)[7 * CalendarRows];
            for (var i = 0; i < cells.Length; i++)
            {
                var col = i % 7;
                var row = i / 7;
                cells[i] = (Rectangle.Round(new RectangleF(col * cellW, headerH + row * cellH, cellW, cellH)), first.AddDays(i));
            }
            return cells;
        }

        private void CalendarPanel_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var headerH = LogicalToDeviceUnits(26);
            var cellW = calendarPanel.ClientSize.Width / 7f;
            string[] heads = { "T2", "T3", "T4", "T5", "T6", "T7", "CN" };
            using var headFont = new Font(Font.FontFamily, 9F, FontStyle.Bold);
            using var dayFont = new Font(Font.FontFamily, 10F, FontStyle.Bold);
            using var smallFont = new Font(Font.FontFamily, 7.5F);
            for (var c = 0; c < 7; c++)
                TextRenderer.DrawText(g, heads[c], headFont, Rectangle.Round(new RectangleF(c * cellW, 0, cellW, headerH)),
                    c == 6 ? Theme.Danger : Theme.TextSecondary, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            var work = WorkDateSet();
            var runs = WorkRuns();
            var today = DateTime.Today;
            var pad = LogicalToDeviceUnits(2);
            using var border = new Pen(Theme.BorderColor);
            using var todayPen = new Pen(Theme.Warning, LogicalToDeviceUnits(2));
            foreach (var (rect, date) in CalendarCells())
            {
                var inMonth = date.Month == _month.Month && date.Year == _month.Year;
                var box = Rectangle.Inflate(rect, -pad, -pad);
                var isWork = work.Contains(date);
                Color fill = Color.White, text = inMonth ? Theme.TextPrimary : Color.FromArgb(203, 213, 225);
                string? sub = null;
                if (isWork)
                {
                    var run = runs.First(r => date >= r.Start && date <= r.End);
                    var n = (int)(date - run.Start).TotalDays + 1;
                    var over = n > MaxConsecutiveDays;
                    fill = !inMonth ? Color.FromArgb(241, 245, 249)
                         : over ? Color.FromArgb(254, 226, 226)
                         : date < today ? Color.FromArgb(226, 232, 240)
                         : Color.FromArgb(219, 234, 254);
                    text = !inMonth ? Color.FromArgb(148, 163, 184) : over ? Theme.Danger : date < today ? Theme.TextSecondary : Color.FromArgb(29, 78, 216);
                    if (Len(run) > 1) sub = $"{n}/{Len(run)}"; // ngày thứ mấy trong đợt làm liên tục
                }
                using (var b = new SolidBrush(fill)) g.FillRectangle(b, box);
                g.DrawRectangle(border, box);
                if (date == today) g.DrawRectangle(todayPen, box);

                var dayRect = sub is null ? box : new Rectangle(box.X, box.Y, box.Width, box.Height * 2 / 3);
                TextRenderer.DrawText(g, date.Day.ToString(), dayFont, dayRect, text, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                if (sub is not null)
                    TextRenderer.DrawText(g, sub, smallFont, new Rectangle(box.X, box.Y + box.Height / 2, box.Width, box.Height / 2),
                        text, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private void CalendarPanel_Resize(object? sender, EventArgs e) => calendarPanel.Invalidate();

        /// <summary>Bấm 1 ngày làm trên lịch -> chọn đúng dòng đó trong danh sách (để xóa nhanh).</summary>
        private void CalendarPanel_MouseClick(object? sender, MouseEventArgs e)
        {
            var hit = CalendarCells().FirstOrDefault(c => c.Rect.Contains(e.Location));
            if (hit.Rect.IsEmpty) return;
            if (hit.Date.Month != _month.Month || hit.Date.Year != _month.Year) { _month = new DateTime(hit.Date.Year, hit.Date.Month, 1); BindGrid(); }
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.Cells["colId"].Value is int id && _days.FirstOrDefault(d => d.Id == id)?.Date.Date == hit.Date)
                {
                    grid.CurrentCell = row.Cells["colDate"];
                    row.Selected = true;
                    return;
                }
            }
            grid.ClearSelection();
        }

        private static string Weekday(DateTime d)
        {
            var name = Vi.DateTimeFormat.GetDayName(d.DayOfWeek);
            return name.Length > 0 ? char.ToUpper(name[0]) + name[1..] : name;
        }

        private async void BtnRegister_Click(object? sender, EventArgs e)
        {
            if (_service is null || _busy) return;
            var from = dtFrom.Value.Date;
            var to = dtTo.Value.Date;
            if (to < from)
            {
                ShowStatus("\"Đến ngày\" phải sau hoặc bằng \"Từ ngày\".", Theme.Danger);
                return;
            }
            if (from < DateTime.Today)
            {
                ShowStatus("Không đăng ký được ngày đã qua.", Theme.Danger);
                return;
            }

            _busy = true;
            btnRegister.Enabled = false;
            try
            {
                var result = await _service.RegisterAsync(_employeeId, from, to);
                if (!result.Success)
                {
                    ShowStatus("⚠️ " + result.Message, Theme.Warning);
                    return;
                }
                Changed = true;
                await LoadAsync();
                ShowStatus("✅ " + result.Message, Theme.Success);
            }
            catch (Exception ex)
            {
                ShowStatus("Lỗi: " + ex.Message, Theme.Danger);
            }
            finally
            {
                _busy = false;
                btnRegister.Enabled = true;
            }
        }

        private async void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (_service is null || _busy) return;
            if (grid.CurrentRow?.Cells["colId"].Value is not int id)
            {
                ShowStatus("Chọn 1 ngày trong danh sách để xóa.", Theme.Danger);
                return;
            }
            var day = _days.FirstOrDefault(d => d.Id == id);
            if (day is null) return;
            if (day.Date.Date < DateTime.Today)
            {
                ShowStatus("Không xóa được ngày đã qua — đây là lịch sử làm việc.", Theme.Warning);
                return;
            }

            var confirm = MessageBox.Show(this,
                $"Xóa ngày làm {Weekday(day.Date)}, {day.Date:dd/MM/yyyy}?",
                "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            _busy = true;
            try
            {
                var result = await _service.DeleteAsync(id);
                if (!result.Success)
                {
                    ShowStatus("⚠️ " + result.Message, Theme.Warning);
                    return;
                }
                Changed = true;
                await LoadAsync();
                ShowStatus(result.Message ?? "Đã xóa.", Theme.Success);
            }
            catch (Exception ex)
            {
                ShowStatus("Lỗi: " + ex.Message, Theme.Danger);
            }
            finally
            {
                _busy = false;
            }
        }

        private void BtnClose_Click(object? sender, EventArgs e)
        {
            DialogResult = Changed ? DialogResult.OK : DialogResult.Cancel;
            Close();
        }

        private void ShowStatus(string message, Color color)
        {
            lblStatus.ForeColor = color;
            lblStatus.Text = message;
        }
    }
}
