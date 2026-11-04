using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_PayrollManagement;
using TruckingSystem.Desktop.SERVICE_PayrollManagement;

namespace TruckingSystem.Desktop.GUI_PayrollManagement;

public sealed class MonthlyAdjustmentsForm : BaseForm
{
    public MonthlyAdjustmentsForm(PayrollApiService api, List<PayrollEmployeeDto> employees, int year, int month, bool locked)
    {
        Text = $"Khoản cộng / trừ tháng {month:00}/{year}"; Size = new Size(900, 550); StartPosition = FormStartPosition.CenterParent;
        var grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false };
        foreach (var (property, title) in new[] { ("EmployeeName", "Nhân viên"), ("Kind", "Loại khoản"), ("Amount", "Số tiền"), ("Reason", "Lý do") })
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = property, DataPropertyName = property, HeaderText = title });
        Theme.StyleGrid(grid); GridLayout.FitColumns(grid);
        grid.Columns["Amount"].DefaultCellStyle.Format = "N0";
        grid.CellFormatting += (_, e) => { if (e.ColumnIndex == 1 && e.Value is string kind)
            e.Value = kind switch { "Allowance" => "Phụ cấp", "Bonus" => "Thưởng", "Overtime" => "Tăng ca", "Deduction" => "Khấu trừ", _ => kind }; };
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8) };
        var employee = new ComboBox { Width = 270, DropDownStyle = ComboBoxStyle.DropDownList, DataSource = employees, DisplayMember = "FullName" };
        var add = Theme.MakePrimaryButton("Thêm khoản", 145); var delete = Theme.MakeSecondaryButton("Xóa khoản", 130);
        add.Enabled = delete.Enabled = !locked;
        bar.Controls.AddRange(new Control[] { employee, add, delete });
        var hint = new Label { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(10), Text = locked
            ? "Tháng đã chốt. Nếu cần điều chỉnh, ghi vào tháng chưa chốt và nêu rõ căn cứ tháng gốc."
            : "Các khoản được giữ khi tính lại. Sau khi sửa, quay về và bấm Tạo / tính lại bảng lương trước khi chốt." };
        Controls.Add(grid); Controls.Add(bar); Controls.Add(hint);
        var busy = false;
        async Task Run(Func<Task> work)
        {
            if (busy) return;
            busy = true; Enabled = false;
            try { await work(); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Khoản cộng/trừ", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            finally { busy = false; Enabled = true; }
        }
        FormClosing += (_, e) => { if (busy) e.Cancel = true; };
        async Task Reload() => grid.DataSource = await api.MonthlyAdjustmentsAsync(year, month);
        Load += async (_, _) => await Run(Reload);
        add.Click += async (_, _) =>
        {
            if (employee.SelectedItem is not PayrollEmployeeDto selected) return;
            using var input = new PayrollInputForm($"Cộng / trừ — {selected.FullName}", PayrollInputSection.Item);
            input.ItemProrate.Visible = false;
            input.ItemHint.Text = "Nhập số tiền thực tế; khoản này không tự chia theo công hoặc KPI.";
            input.OnSave(() => api.AddMonthlyAdjustmentAsync(new(selected.Id, year, month,
                (string)input.ItemKind.SelectedValue!, input.ItemAmount.Value, input.ItemReason.Text)));
            if (input.ShowDialog(this) == DialogResult.OK) await Run(Reload);
        };
        delete.Click += async (_, _) =>
        {
            if (grid.CurrentRow?.DataBoundItem is not MonthlyAdjustmentDto row || MessageBox.Show(this,
                $"Xóa khoản {row.Amount:N0} đ của {row.EmployeeName}?", "Xác nhận", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            await Run(async () => { var result = await api.DeleteMonthlyAdjustmentAsync(row.Id);
                if (!result.Success) throw new InvalidOperationException(result.Message); await Reload(); });
        };
    }
}
