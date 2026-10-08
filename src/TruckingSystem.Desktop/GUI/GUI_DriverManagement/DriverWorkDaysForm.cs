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
    /// Ngày đã có lịch tự bỏ qua, không tạo trùng.</summary>
    public partial class DriverWorkDaysForm : BaseForm
    {
        private static readonly CultureInfo Vi = new("vi-VN");

        private DriverWorkDayApiService? _service;
        private int _employeeId;
        private List<DriverWorkDayItem> _days = new();
        private bool _busy;

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
            chkShowPast.CheckedChanged += (_, _) => BindGrid();
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

        private void BindGrid()
        {
            var today = DateTime.Today;
            var shown = chkShowPast.Checked
                ? _days.OrderByDescending(d => d.Date).ToList()   // xem cả lịch sử: mới nhất lên đầu
                : _days.Where(d => d.Date.Date >= today).OrderBy(d => d.Date).ToList();

            grid.Rows.Clear();
            foreach (var d in shown)
            {
                var note = d.Date.Date == today ? "Hôm nay"
                    : d.Date.Date < today ? "Đã qua"
                    : "";
                if (d.HasOldShiftType) note = (note.Length > 0 ? note + " · " : "") + "dữ liệu ca cũ";
                var row = grid.Rows.Add(d.Id, d.Date.ToString("dd/MM/yyyy"), Weekday(d.Date), note);
                if (d.Date.Date < today) grid.Rows[row].DefaultCellStyle.ForeColor = Theme.TextSecondary;
            }
            grid.ClearSelection();

            var upcoming = _days.Count(d => d.Date.Date >= today);
            lblSummary.Text = upcoming == 0
                ? "Tài xế chưa đăng ký ngày làm nào từ hôm nay trở đi — sẽ KHÔNG được chọn khi Chia xe."
                : $"{upcoming} ngày làm sắp tới" + (chkShowPast.Checked ? $" · tổng {_days.Count} ngày (kể cả đã qua)" : "");
            lblSummary.ForeColor = upcoming == 0 ? Theme.Warning : Theme.TextPrimary;
            btnDelete.Enabled = shown.Count > 0;
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
