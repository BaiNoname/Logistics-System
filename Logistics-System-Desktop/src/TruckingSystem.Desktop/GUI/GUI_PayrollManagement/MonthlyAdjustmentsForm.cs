using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_PayrollManagement;
using TruckingSystem.Desktop.SERVICE_PayrollManagement;

namespace TruckingSystem.Desktop.GUI_PayrollManagement;

public sealed class MonthlyAdjustmentsForm : BaseForm
{
    public MonthlyAdjustmentsForm(PayrollApiService api, PayrollEmployeeDto employee, int year, int month, bool locked)
    {
        Text = $"Khoản cộng / trừ tháng {month:00}/{year} — {employee.Code} · {employee.FullName}"; Size = new Size(900, 550); StartPosition = FormStartPosition.CenterParent;
        var grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false };
        foreach (var (property, title) in new[] { ("EmployeeName", "Nhân viên"), ("Kind", "Loại khoản"), ("Amount", "Số tiền"), ("Reason", "Lý do") })
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = property, DataPropertyName = property, HeaderText = title });
        Theme.StyleGrid(grid); GridLayout.FitColumns(grid);
        grid.Columns["Amount"].DefaultCellStyle.Format = "N0";
        grid.CellFormatting += (_, e) => { if (e.ColumnIndex == 1 && e.Value is string kind)
            e.Value = kind switch { "Allowance" => "Phụ cấp", "Bonus" => "Thưởng", "Overtime" => "Tăng ca", "Deduction" => "Khấu trừ", _ => kind }; };
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8) };
        var add = Theme.MakePrimaryButton("Thêm khoản", 145);
        var menu = new ContextMenuStrip();
        var delete = menu.Items.Add("🗑 Xóa khoản");
        ModernMenu.Apply(menu);
        grid.ContextMenuStrip = menu;
        Disposed += (_, _) => menu.Dispose();
        add.Enabled = delete.Enabled = !locked;
        delete.ToolTipText = locked ? "Tháng đã chốt, không được xóa khoản cộng/trừ." : "Xóa khoản được chọn sau khi xác nhận.";
        bar.Controls.AddRange(new Control[] { new Label { Text = $"{employee.Code} · {employee.FullName}", AutoSize = true, Margin = new Padding(0, 8, 16, 0) }, add });
        var hint = new Label { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(10), Text = locked
            ? "Tháng đã chốt. Nếu cần điều chỉnh, ghi vào tháng chưa chốt và nêu rõ căn cứ tháng gốc."
            : "Hệ thống đưa các thay đổi vào bản nháp trong lần tính tự động tiếp theo. Tải lại bảng lương để đối soát trước khi chốt." };
        Controls.Add(grid); Controls.Add(bar); Controls.Add(hint);
        SizeChanged += (_, _) => hint.MaximumSize = new Size(Math.Max(1, ClientSize.Width), 0);
        hint.Font = Theme.FontSmall;
        hint.ForeColor = Theme.TextSecondary;
        BackColor = Theme.CardBg;
        MinimumSize = new Size(620, 420);
        ButtonStyles.Footer(this, null, FormStyle.NewCloseButton(this));
        DialogKeys.Enable(this);
        Anim.FadeIn(this);
        Load += (_, _) => Theme.UniformButtons(bar);
        var busy = false;
        menu.Opening += (_, e) => e.Cancel = busy || grid.CurrentRow?.DataBoundItem is not MonthlyAdjustmentDto;
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
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Khoản cộng/trừ", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            finally { busy = false; Enabled = true; }
        }
        FormClosing += (_, e) => { if (busy) e.Cancel = true; };
        async Task Reload() => grid.DataSource = (await api.MonthlyAdjustmentsAsync(year, month)).Where(a => a.EmployeeId == employee.Id).ToList();
        Load += async (_, _) => await Run(Reload);
        add.Click += async (_, _) =>
        {
            if (locked || busy) return;
            using var input = new PayrollInputForm($"Cộng / trừ — {employee.FullName}", PayrollInputSection.Item);
            input.ItemProrate.Visible = false;
            input.ItemHint.Text = "Nhập số tiền thực tế; khoản này không tự chia theo công hoặc KPI.";
            input.OnSave(() => api.AddMonthlyAdjustmentAsync(new(employee.Id, year, month,
                (string)input.ItemKind.SelectedValue!, input.ItemAmount.Value, input.ItemReason.Text)));
            if (input.ShowDialog(this) == DialogResult.OK) await Run(Reload);
        };
        delete.Click += async (_, _) =>
        {
            if (locked || busy || grid.CurrentRow?.DataBoundItem is not MonthlyAdjustmentDto row || row.EmployeeId != employee.Id || MessageBox.Show(this,
                $"Xóa khoản {row.Amount:N0} đ của {row.EmployeeName}?", "Xác nhận", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            await Run(async () => { var result = await api.DeleteMonthlyAdjustmentAsync(row.Id);
                if (!result.Success) throw new InvalidOperationException(result.Message); await Reload(); });
        };
    }
}
