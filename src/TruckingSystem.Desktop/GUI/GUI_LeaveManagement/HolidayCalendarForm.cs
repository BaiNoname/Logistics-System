using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_LeaveManagement;
using TruckingSystem.Desktop.SERVICE_LeaveManagement;

namespace TruckingSystem.Desktop.GUI_LeaveManagement;

/// <summary>Lịch nghỉ lễ theo năm: xem, thêm, xóa ngày lễ (chỉ Admin mở được từ màn Chấm công).</summary>
public sealed class HolidayCalendarForm : BaseForm
{
    private static readonly string[] DayNames = { "Chủ nhật", "Thứ hai", "Thứ ba", "Thứ tư", "Thứ năm", "Thứ sáu", "Thứ bảy" };

    public HolidayCalendarForm(LeaveApiService api)
    {
        Text = "Lịch nghỉ lễ"; Size = new Size(720, 520); StartPosition = FormStartPosition.CenterParent;
        var grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false };
        foreach (var (property, title) in new[] { ("Date", "Ngày"), ("DayOfWeek", "Thứ"), ("Name", "Tên ngày lễ") })
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = property, DataPropertyName = property == "DayOfWeek" ? "" : property, HeaderText = title });
        Theme.StyleGrid(grid); GridLayout.FitColumns(grid);
        grid.Columns["Date"].DefaultCellStyle.Format = "dd/MM/yyyy";
        grid.CellFormatting += (_, e) =>
        {
            if (e.RowIndex < 0 || grid.Rows[e.RowIndex].DataBoundItem is not HolidayDto h) return;
            if (grid.Columns[e.ColumnIndex].Name == "DayOfWeek") { e.Value = DayNames[(int)h.Date.DayOfWeek]; e.FormattingApplied = true; }
        };

        var year = new NumericUpDown { Minimum = 2000, Maximum = 2100, Value = DateTime.Today.Year, Width = 80 };
        var date = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 120, MinDate = new DateTime(2000, 1, 1), MaxDate = new DateTime(2100, 12, 31), Value = DateTime.Today };
        var name = new TextBox { Width = 220, PlaceholderText = "Tên ngày lễ" };
        var add = Theme.MakePrimaryButton("Thêm", 100);
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8) };
        Label Caption(string t) => new() { Text = t, AutoSize = true, Margin = new Padding(0, 8, 6, 0) };
        bar.Controls.AddRange(new Control[] { Caption("Năm"), year, Caption("Ngày"), date, Caption("Tên"), name, add });

        var menu = new ContextMenuStrip();
        var delete = menu.Items.Add("🗑 Xóa ngày lễ");
        ModernMenu.Apply(menu);
        grid.ContextMenuStrip = menu;
        Disposed += (_, _) => menu.Dispose();

        Controls.Add(grid); Controls.Add(bar);
        BackColor = Theme.CardBg;
        MinimumSize = new Size(620, 380);
        ButtonStyles.Footer(this, null, FormStyle.NewCloseButton(this));
        DialogKeys.Enable(this);
        Anim.FadeIn(this);
        Load += (_, _) => Theme.UniformButtons(bar);

        var busy = false;
        menu.Opening += (_, e) => e.Cancel = busy || grid.CurrentRow?.DataBoundItem is not HolidayDto;
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
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Lịch nghỉ lễ", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            finally { busy = false; Enabled = true; }
        }
        FormClosing += (_, e) => { if (busy) e.Cancel = true; };
        async Task Reload() => grid.DataSource = (await api.HolidaysAsync((int)year.Value)).OrderBy(h => h.Date).ToList();
        void Show(LeaveResult result)
        {
            if (!result.Success) throw new InvalidOperationException(result.Message);
            var text = string.Join(Environment.NewLine, new[] { result.Message }.Concat(result.Warnings ?? new()).Where(s => !string.IsNullOrWhiteSpace(s)));
            if (text.Length > 0)
                MessageBox.Show(this, text, "Lịch nghỉ lễ", MessageBoxButtons.OK,
                    result.Warnings is { Count: > 0 } ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }
        Load += async (_, _) => await Run(Reload);
        year.ValueChanged += async (_, _) => await Run(Reload);
        add.Click += async (_, _) =>
        {
            if (busy) return;
            if (string.IsNullOrWhiteSpace(name.Text))
            {
                MessageBox.Show(this, "Vui lòng nhập tên ngày lễ.", "Lịch nghỉ lễ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            await Run(async () =>
            {
                Show(await api.AddHolidayAsync(new(date.Value.Date, name.Text.Trim())));
                name.Clear();
                year.Value = date.Value.Year;
                await Reload();
            });
        };
        delete.Click += async (_, _) =>
        {
            if (busy || grid.CurrentRow?.DataBoundItem is not HolidayDto row || MessageBox.Show(this,
                $"Xóa ngày lễ {row.Name} ({row.Date:dd/MM/yyyy})?", "Xác nhận", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            await Run(async () => { Show(await api.DeleteHolidayAsync(row.Id)); await Reload(); });
        };
    }
}
