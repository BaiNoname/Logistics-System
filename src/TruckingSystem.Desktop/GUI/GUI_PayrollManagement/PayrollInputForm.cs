using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_PayrollManagement;

namespace TruckingSystem.Desktop.GUI_PayrollManagement;

public partial class PayrollInputForm : BaseForm
{
    private bool _saving;

    public PayrollInputForm()
    {
        InitializeComponent();
        foreach (var (property, title, format) in new[]
        {
            ("Id", "ID", ""), ("EmployeeId", "ID nhân viên", ""),
            ("MonthlySalary", "Lương tháng", "N0"), ("Status", "Trạng thái", ""),
            ("EffectiveFrom", "Hiệu lực từ", "dd/MM/yyyy"), ("EffectiveTo", "Hiệu lực đến", "dd/MM/yyyy"),
            ("UpdatedAt", "Cập nhật (VN)", "dd/MM/yyyy HH:mm"), ("Reason", "Căn cứ", "")
        })
            SalaryHistory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = property, DataPropertyName = property, HeaderText = title,
                DefaultCellStyle = new DataGridViewCellStyle { Format = format }
            });
        SalaryHistory.CellFormatting += (_, e) =>
        {
            if (e.ColumnIndex < 0) return;
            if (SalaryHistory.Columns[e.ColumnIndex].Name == "Status" && e.Value is string status)
                e.Value = status switch { "Active" => "Đang áp dụng", "Pending" => "Chưa hiệu lực", "Expired" => "Hết hiệu lực", _ => status };
            if (SalaryHistory.Columns[e.ColumnIndex].Name == "UpdatedAt" && e.Value is DateTime updated)
            {
                e.Value = updated.AddHours(7).ToString("dd/MM/yyyy HH:mm");
                e.FormattingApplied = true;
            }
        };
        foreach (var (grid, columns) in new[]
        {
            (DetailItems, new[] { ("Kind", "Khoản"), ("Amount", "Số tiền"), ("ProrateByAttendance", "Theo công"),
                ("CalculatedAmount", "Thành tiền"), ("Reason", "Căn cứ / KPI chuyến"), ("IsAdjustment", "Điều chỉnh") }),
            (DetailPayments, new[] { ("Date", "Ngày"), ("Amount", "Số tiền"), ("Method", "Hình thức"),
                ("Reference", "Chứng từ"), ("RecordedBy", "Người ghi") })
        })
            foreach (var (property, title) in columns)
                grid.Columns.Add(new DataGridViewTextBoxColumn { Name = property, DataPropertyName = property, HeaderText = title });
        foreach (var grid in new[] { SalaryHistory, DetailItems, DetailPayments })
        {
            grid.AutoGenerateColumns = false;
            Theme.StyleGrid(grid);
            GridLayout.FitColumns(grid);
        }
        BindChoices(ItemKind, ("Allowance", "Phụ cấp"), ("Bonus", "Thưởng"), ("Overtime", "Tăng ca"), ("Deduction", "Khấu trừ (thuế, bảo hiểm...)"));
        BindChoices(PaymentMethod, ("BankTransfer", "Chuyển khoản"), ("Cash", "Tiền mặt"));
        FormClosing += (_, e) => { if (_saving) e.Cancel = true; };
    }

    internal PayrollInputForm(string title, PayrollInputSection section) : this()
    {
        Text = title;
        var page = section switch
        {
            PayrollInputSection.Salary => _salaryPage,
            PayrollInputSection.Item => _itemPage,
            PayrollInputSection.Payment => _paymentPage,
            PayrollInputSection.Details => _detailsPage,
            _ => throw new ArgumentOutOfRangeException(nameof(section))
        };
        foreach (var other in _sections.TabPages.Cast<TabPage>().Where(p => p != page).ToArray())
        {
            _sections.TabPages.Remove(other);
            other.Dispose();
        }
    }

    public void OnSave(Func<Task<PayrollResult>> save)
    {
        _save.Click += async (_, _) =>
        {
            _saving = true;
            _save.Enabled = false;
            _sections.Enabled = false;
            try
            {
                var result = await save();
                if (!result.Success) throw new InvalidOperationException(result.Message);
                _saving = false;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                _error.Text = ex.Message;
                _error.Visible = true;
                MessageBox.Show(this, ex.Message, "Kiểm tra dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally { _saving = false; _save.Enabled = true; _sections.Enabled = true; }
        };
    }

    public void ReadOnly() => _save.Visible = false;

    internal void ShowSalarySummary(string text)
    {
        var summary = new Label { Text = text, AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(12, 8, 12, 8) };
        _salaryPageLayout.RowCount = 3;
        _salaryPageLayout.RowStyles.Add(new RowStyle());
        _salaryPageLayout.SetRow(SalaryHistory, 1);
        _salaryPageLayout.SetRow(SalaryEditor, 2);
        _salaryPageLayout.Controls.Add(summary, 0, 0);
        _salaryPageLayout.SetColumnSpan(summary, 2);
    }
    internal static void SetNumber(NumericUpDown input, decimal value, decimal? maximum = null)
    {
        if (maximum.HasValue) input.Maximum = maximum.Value;
        input.Value = Math.Clamp(value, input.Minimum, input.Maximum);
    }
    private static void BindChoices(ComboBox input, params (string Value, string Text)[] choices)
    {
        input.DisplayMember = nameof(ChoiceItem.Text);
        input.ValueMember = nameof(ChoiceItem.Value);
        input.DataSource = choices.Select(c => new ChoiceItem(c.Value, c.Text)).ToList();
    }
    private record ChoiceItem(string Value, string Text);
}

internal enum PayrollInputSection { Salary, Item, Payment, Details }
