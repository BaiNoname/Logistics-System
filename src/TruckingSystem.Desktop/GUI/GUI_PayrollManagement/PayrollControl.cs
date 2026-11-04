using System.Text;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_PayrollManagement;
using TruckingSystem.Domain.Payroll;

namespace TruckingSystem.Desktop.GUI_PayrollManagement;

public partial class PayrollControl : BaseUserControl
{
    private ApiServices _api = null!;
    private List<PayrollEmployeeDto> _employees = new();
    private List<PayrollSlipDto> _slips = new();
    private PayrollPeriodDto? _period;
    private bool _loading, _busy;
    private PayrollEmployeeDto? Employee => _employee.SelectedItem as PayrollEmployeeDto;
    private PayrollSlipDto? Slip => _slips.FirstOrDefault(s => s.EmployeeId == Employee?.Id);

    public PayrollControl()
    {
        InitializeComponent();
        Theme.StyleGrid(_slipGrid); GridLayout.FitColumns(_slipGrid);
        BindButton(_salaryButton, SalaryAsync);
        BindButton(_generateButton, async () =>
        {
            if (!Confirm("Tạo/tính lại toàn bộ bảng lương tháng? Các khoản cộng/trừ theo tháng được giữ nguyên. Thiếu dữ liệu thì không cập nhật ai.")) return;
            await CheckAsync(_api.Payroll.GenerateMonthAsync(new(_month.Value.Year, _month.Value.Month, _period?.Revision ?? 0)));
            await LoadAsync();
        });
        BindButton(_approveButton, async () =>
        {
            if (_period is null || !Confirm($"Chốt {_slips.Count} phiếu, tổng {_slips.Sum(s => s.NetPay):N0} đ? Sau khi chốt không tính lại.")) return;
            await CheckAsync(_api.Payroll.ApproveAsync(_period.Id, _period.Revision)); await LoadAsync();
        });
        BindButton(_kpiButton, async () =>
        {
            using var form = new PayrollKpiPolicyForm(await _api.Payroll.PoliciesAsync(),
                (await _api.Payroll.KpiRatesAsync()).Select(r => r.VehicleType).ToList(), _api.Payroll.SavePolicyAsync);
            form.ShowDialog(this);
        });
        BindButton(_adjustmentsButton, async () =>
        {
            using var form = new MonthlyAdjustmentsForm(_api.Payroll, _employees, _month.Value.Year, _month.Value.Month, _period?.ApprovedAt is not null);
            form.ShowDialog(this); await LoadAsync();
        });
        BindButton(_deleteDraftButton, async () =>
        {
            if (!RequireSlip() || _period is null || !Confirm("Xóa phiếu nháp cũ? Hãy xuất đối chiếu và chuyển các khoản tay vào danh sách cộng/trừ tháng trước.")) return;
            await CheckAsync(_api.Payroll.DeleteDraftAsync(Slip!.Id, _period.Revision)); await LoadAsync();
        });
        BindButton(_payButton, PayAsync); BindButton(_detailsButton, DetailsAsync); BindButton(_exportButton, ExportAsync);
        _month.ValueChanged += async (_, _) => { if (!_loading) await RunAsync(LoadAsync); };
        _employee.SelectedIndexChanged += (_, _) => UpdateButtons();
        _slipGrid.SelectionChanged += (_, _) => { if (!_loading && _slipGrid.CurrentRow?.DataBoundItem is PayrollSlipDto s) SelectEmployee(s.EmployeeId); };
        _slipGrid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await RunAsync(DetailsAsync); };
        _slipGrid.CellFormatting += (_, e) =>
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0 && _slipGrid.Columns[e.ColumnIndex].Name == "KpiRatio"
                && _slipGrid.Rows[e.RowIndex].DataBoundItem is PayrollSlipDto slip)
            {
                e.Value = slip.CalculationVersion == 2 && slip.IsDriver && slip.KpiTarget > 0 ? (slip.KpiPoints / slip.KpiTarget).ToString("P2") : "—";
                e.FormattingApplied = true;
            }
            if (e.ColumnIndex >= 0 && _slipGrid.Columns[e.ColumnIndex].Name == "PaymentStatus" && e.Value is string state)
                e.Value = state switch { "Paid" => "Đủ", "PartiallyPaid" => "Một phần", _ => "Chưa trả" };
        };
        Load += async (_, _) => await RunAsync(LoadAsync);
    }

    public PayrollControl(ApiServices api) : this()
    {
        _api = api; _month.Value = DateTime.UtcNow.AddHours(7).Date; UpdateButtons();
    }

    private async Task LoadAsync()
    {
        _loading = true;
        var selected = Employee?.Id;
        try
        {
            _period = (await _api.Payroll.PeriodsAsync()).SingleOrDefault(p => p.Year == _month.Value.Year && p.Month == _month.Value.Month);
            _employees = await _api.Payroll.EmployeesAsync(_month.Value.Year, _month.Value.Month);
            _slips = _period is null ? new() : await _api.Payroll.SlipsAsync(_period.Id);
            _employee.DataSource = _employees;
            if (selected.HasValue) SelectEmployee(selected.Value);
            _slipGrid.DataSource = _slips;
            _status.Text = _period?.ApprovedAt is not null ? "Đã chốt — số liệu được giữ nguyên."
                : "Bản nháp — bấm Tạo / tính lại bảng lương để lấy công, chuyến và khoản cộng/trừ mới nhất.";
            _totals.Text = $"Tháng {_month.Value:MM/yyyy} · {_slips.Count}/{_employees.Count} phiếu · Thực lĩnh {_slips.Sum(s => s.NetPay):N0} đ · Đã trả {_slips.Sum(s => s.PaidAmount):N0} đ";
        }
        catch { _period = null; _slips.Clear(); _status.Text = "Không tải được dữ liệu; chọn lại tháng để thử lại."; throw; }
        finally { _loading = false; UpdateButtons(); }
    }

    private void UpdateButtons()
    {
        var canPay = _api is not null && _api.Connection.IsInRole("Admin", "Accountant");
        _generateButton.Enabled = canPay && _period?.ApprovedAt is null;
        _approveButton.Enabled = canPay && _api!.Connection.IsInRole("Admin") && _period is { ApprovedAt: null } && _slips.Count > 0;
        _payButton.Enabled = canPay && _period?.ApprovedAt is not null && Slip is not null;
        _detailsButton.Enabled = _exportButton.Enabled = canPay && Slip is not null;
        _salaryButton.Enabled = canPay && Employee is not null;
        _adjustmentsButton.Enabled = canPay;
        _kpiButton.Visible = _api is not null && _api.Connection.IsInRole("Admin");
        _deleteDraftButton.Visible = canPay && _period is { ApprovedAt: null } && Slip is { CalculationVersion: 0 };
    }

    private async Task SalaryAsync()
    {
        if (!RequireEmployee()) return;
        var employee = Employee!;
        var history = await _api.Payroll.SalariesAsync(employee.Id);
        using var form = new PayrollInputForm($"Mức lương — {employee.Code} · {employee.FullName}", PayrollInputSection.Salary);
        form.SalaryHistory.DataSource = history;
        var current = history.SingleOrDefault(s => s.Status == "Active");
        form.ShowSalarySummary(current is not null
            ? $"Đang áp dụng: {current.MonthlySalary:N0} đ/tháng · Từ {current.EffectiveFrom:dd/MM/yyyy}"
            : history.Count == 0 ? "Chưa có mức lương" : "Chưa có mức lương đang áp dụng");
        if (_api.Connection.IsInRole("Admin"))
        {
            var start = PayrollRules.PeriodRange(_month.Value.Year, _month.Value.Month).From;
            if (employee.HireDate.Date > start) start = employee.HireDate.Date;
            if (history.Count > 0 && start <= history[^1].EffectiveFrom)
            {
                start = new DateTime(history[^1].EffectiveFrom.Year, history[^1].EffectiveFrom.Month, 1);
                if (start <= history[^1].EffectiveFrom) start = start.AddMonths(1);
            }
            form.SalaryDate.Value = start;
            form.OnSave(() => _api.Payroll.AddSalaryAsync(employee.Id, new(form.SalaryDate.Value.Date, form.SalaryAmount.Value, form.SalaryReason.Text)));
        }
        else
        {
            form.SalaryEditor.Visible = false;
            form.ReadOnly();
        }
        form.ShowDialog(this);
    }

    private async Task PayAsync()
    {
        if (!RequireSlip() || _period is null) return;
        var slip = Slip!;
        if (slip.RemainingAmount == 0) throw new InvalidOperationException("Phiếu đã được trả đủ.");
        using var form = new PayrollInputForm($"Ghi nhận chi trả — {slip.EmployeeName}", PayrollInputSection.Payment);
        form.PaymentBalance.Text = $"Còn phải trả: {slip.RemainingAmount:N0} đ";
        PayrollInputForm.SetNumber(form.PaymentAmount, slip.RemainingAmount, slip.RemainingAmount);
        form.PaymentDate.Value = DateTime.UtcNow.AddHours(7).Date;
        form.OnSave(() => _api.Payroll.PayAsync(slip.Id, new(_period.Revision, form.PaymentAmount.Value, form.PaymentDate.Value.Date,
            (string)form.PaymentMethod.SelectedValue!, form.PaymentReference.Text)));
        if (form.ShowDialog(this) == DialogResult.OK) await LoadAsync();
    }

    private Task DetailsAsync()
    {
        if (!RequireSlip()) return Task.CompletedTask;
        var s = Slip!;
        using var form = new PayrollInputForm($"Chi tiết phiếu — {s.EmployeeName}", PayrollInputSection.Details) { Size = new Size(950, 700) };
        form.ReadOnly();
        form.DetailPeriod.Visible = s.WorkFrom.HasValue && s.WorkTo.HasValue;
        if (s.WorkFrom.HasValue && s.WorkTo.HasValue)
            form.DetailPeriod.Text = $"{(s.IsDriver ? "Tài xế — lịch đăng ký" : "Nhân viên — công tổng hợp")}: {s.WorkFrom:dd/MM/yyyy}–{s.WorkTo:dd/MM/yyyy}";
        form.DetailSummary.Text = $"{s.EmployeeCode} — {s.EmployeeName}\r\n{s.DepartmentName} · {s.PositionName}\r\nLương tháng: {s.MonthlySalary:N0} đ; hiệu lực {s.SalaryEffectiveFrom:dd/MM/yyyy}\r\nCông chuẩn: {s.StandardDays}; làm: {s.WorkedDays}; nghỉ hưởng: {s.PaidLeaveDays}; lễ: {s.HolidayDays}; không lương: {s.UnpaidLeaveDays}\r\nTăng ca thường/nghỉ/lễ: {s.WeekdayOvertimeHours}/{s.RestDayOvertimeHours}/{s.HolidayOvertimeHours} giờ\r\nThực lĩnh duyệt gốc: {s.ApprovedNetPay:N0} đ; hiện tại: {s.NetPay:N0} đ; đã trả: {s.PaidAmount:N0} đ; còn: {s.RemainingAmount:N0} đ\r\nXác nhận công: {s.AttendanceConfirmedBy} · {s.AttendanceConfirmedAt.AddHours(7):dd/MM/yyyy HH:mm} (VN)\r\n{s.AttendanceNote}";
        if (s.CalculationVersion == 2 && s.IsDriver)
            form.DetailSummary.Text = $"{s.EmployeeCode} — {s.EmployeeName}\r\nLCB: {s.MonthlySalary:N0} đ\r\nP: {s.KpiPoints:0.######}; điểm chuẩn: {s.KpiTarget}; KPI: {(s.KpiTarget > 0 ? s.KpiPoints / s.KpiTarget : 0):P2}\r\nLương theo KPI: {s.EarnedSalary:N0} đ\r\nThực lĩnh: {s.NetPay:N0} đ\r\n{s.AttendanceNote}";
        form.DetailItems.DataSource = s.Items.Select(i => new { Kind = i.Kind switch { "Allowance" => "Phụ cấp", "Bonus" => "Thưởng", "Overtime" => "Tăng ca", _ => "Khấu trừ" },
            Amount = i.Amount.ToString("N0"), ProrateByAttendance = i.ProrateByAttendance ? "Có" : "Không", CalculatedAmount = i.CalculatedAmount.ToString("N0"), i.Reason, IsAdjustment = i.IsAdjustment ? "Có" : "Không" }).ToList();
        form.DetailPayments.DataSource = s.Payments.Select(p => new { Date = p.PaidOn.ToString("dd/MM/yyyy"), Amount = p.Amount.ToString("N0"), Method = p.Method == "Cash" ? "Tiền mặt" : "Chuyển khoản", p.Reference, p.RecordedBy }).ToList();
        form.DetailDriverDates.Visible = s.IsDriver && s.CalculationVersion != 2;
        form.DetailDriverDates.Text = "Ngày đăng ký đã dùng tính lương: " + s.DriverWorkDates;
        form.ShowDialog(this);
        return Task.CompletedTask;
    }

    private async Task ExportAsync()
    {
        if (!RequireSlip() || _period is null) return;
        using var save = new SaveFileDialog { Filter = "Phiếu lương HTML (*.html)|*.html", FileName = $"phieu-luong-{_period.Year}-{_period.Month:00}-{Slip!.Id}.html" };
        if (save.ShowDialog(this) != DialogResult.OK) return;
        var html = await _api.Payroll.ExportAsync(Slip.Id);
        await File.WriteAllTextAsync(save.FileName, html, Encoding.UTF8);
        MessageBox.Show(this, $"Đã lưu phiếu lương tại:\n{save.FileName}\nMở bằng trình duyệt và chọn In để in hoặc lưu PDF.", "Xuất phiếu", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void SelectEmployee(int id)
    {
        var employee = _employees.FirstOrDefault(e => e.Id == id);
        if (employee is not null) _employee.SelectedItem = employee;
    }
    private bool RequireEmployee()
    {
        if (Employee is null) throw new InvalidOperationException("Chọn nhân viên trong kỳ trước.");
        return true;
    }
    private bool RequireSlip()
    {
        RequireEmployee();
        if (Slip is null) throw new InvalidOperationException("Nhân viên được chọn chưa có phiếu lương trong kỳ.");
        return true;
    }
    private async Task RunAsync(Func<Task> action)
    {
        if (_busy || _api is null) return;
        _busy = true;
        Enabled = false;
        try { await action(); }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Công & lương nhân viên", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        finally { if (!IsDisposed) Enabled = true; _busy = false; }
    }
    private static async Task CheckAsync(Task<PayrollResult> task)
    {
        var result = await task;
        if (!result.Success) throw new InvalidOperationException(result.Message);
    }
    private bool Confirm(string text) => MessageBox.Show(this, text, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    private void BindButton(Button button, Func<Task> action)
        => button.Click += async (_, _) => await RunAsync(action);
}
