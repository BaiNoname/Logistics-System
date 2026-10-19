using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_PayrollManagement;

namespace TruckingSystem.Desktop.GUI_PayrollManagement;

public partial class PayrollInputForm : BaseForm
{
    private bool _saving;

    public PayrollInputForm()
    {
        InitializeComponent();
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
            PayrollInputSection.Attendance => _attendancePage,
            PayrollInputSection.Salary => _salaryPage,
            PayrollInputSection.DriverAttendance => _driverPage,
            PayrollInputSection.Item => _itemPage,
            PayrollInputSection.DeleteItem => _deletePage,
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

internal enum PayrollInputSection { Attendance, Salary, DriverAttendance, Item, DeleteItem, Payment, Details }
