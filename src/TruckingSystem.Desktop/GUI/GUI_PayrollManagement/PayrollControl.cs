using System.Text;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_PayrollManagement;
using TruckingSystem.Domain.Payroll;

namespace TruckingSystem.Desktop.GUI_PayrollManagement;

public partial class PayrollControl : BaseUserControl
{
    private ApiServices _api = null!;
    private readonly List<Button> _draftButtons = new();
    private readonly List<Button> _approvedButtons = new();
    private readonly List<Button> _slipButtons = new();
    private List<PayrollEmployeeDto> _employees = new();
    private List<AttendanceDto> _attendance = new();
    private List<PayrollSlipDto> _slips = new();
    private PayrollPeriodDto? _period;
    private bool _loading;
    private bool _busy;
    private bool CanViewPay => _api is not null && _api.Connection.IsInRole("Admin", "Accountant");
    private PayrollEmployeeDto? Employee => _employee.SelectedItem as PayrollEmployeeDto;
    private PayrollSlipDto? Slip => _slips.FirstOrDefault(s => s.EmployeeId == Employee?.Id);

    public PayrollControl()
    {
        InitializeComponent();
        _attendanceGrid.AutoGenerateColumns = _slipGrid.AutoGenerateColumns = false;
        Theme.StyleGrid(_attendanceGrid);
        GridLayout.FitColumns(_attendanceGrid);
        Theme.StyleGrid(_slipGrid);
        GridLayout.FitColumns(_slipGrid);
        _draftButtons.AddRange(new[] { _manualAttendanceButton, _driverAttendanceButton, _confirmAttendanceButton,
            _generateButton, _addItemButton, _deleteItemButton, _deleteDraftButton, _approveButton });
        _approvedButtons.AddRange(new[] { _adjustButton, _payButton });
        _slipButtons.AddRange(new[] { _detailsButton, _exportButton });
        BindButton(_refreshButton, LoadAsync);
        BindButton(_openPeriodButton, async () =>
        {
            await CheckAsync(_api.Payroll.CreatePeriodAsync(new(_month.Value.Year, _month.Value.Month)));
            await LoadAsync();
        });
        BindButton(_salaryButton, SalaryAsync);
        BindButton(_manualAttendanceButton, AttendanceAsync);
        BindButton(_driverAttendanceButton, DriverAttendanceAsync);
        BindButton(_confirmAttendanceButton, async () =>
        {
            if (!RequireEmployee() || _period is null) return;
            if (!Confirm($"Xác nhận số công của {Employee!.FullName} trong kỳ {_period.Month:00}/{_period.Year}?")) return;
            await CheckAsync(_api.Payroll.ConfirmAttendanceAsync(_period.Id, Employee.Id, _period.Revision));
            await LoadAsync();
        });
        BindButton(_generateButton, async () =>
        {
            if (!RequireEmployee() || _period is null) return;
            await CheckAsync(_api.Payroll.GenerateAsync(_period.Id, Employee!.Id, _period.Revision));
            await LoadAsync();
        });
        BindButton(_detailsButton, DetailsAsync);
        BindButton(_addItemButton, () => ItemAsync(false));
        BindButton(_deleteItemButton, DeleteItemAsync);
        BindButton(_deleteDraftButton, async () =>
        {
            if (!RequireSlip() || _period is null) return;
            if (!Confirm($"Xóa phiếu nháp và các khoản cộng/trừ của {Slip!.EmployeeName}? Công đã nhập vẫn được giữ.")) return;
            await CheckAsync(_api.Payroll.DeleteDraftAsync(Slip.Id, _period.Revision));
            await LoadAsync();
        });
        BindButton(_approveButton, async () =>
        {
            if (_period is null) return;
            if (!Confirm($"Duyệt và khóa kỳ {_period.Month:00}/{_period.Year}: {_slips.Count} phiếu, tổng thực lĩnh {_slips.Sum(s => s.NetPay):N0} đ?")) return;
            await CheckAsync(_api.Payroll.ApproveAsync(_period.Id, _period.Revision));
            await LoadAsync();
        });
        BindButton(_adjustButton, () => ItemAsync(true));
        BindButton(_payButton, PayAsync);
        BindButton(_exportButton, ExportAsync);
        foreach (DataGridViewColumn column in _slipGrid.Columns)
            if (column.Name is not ("EmployeeCode" or "EmployeeName" or "PaymentStatus"))
            {
                column.DefaultCellStyle.Format = "N0";
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        _attendanceGrid.SelectionChanged += (_, _) =>
        {
            if (!_loading && _attendanceGrid.CurrentRow?.Cells["Id"].Value is int id) SelectEmployee(id);
        };
        _slipGrid.CellFormatting += (_, e) =>
        {
            if (e.ColumnIndex >= 0 && _slipGrid.Columns[e.ColumnIndex].Name == "PaymentStatus" && e.Value is string state)
                e.Value = state switch { "Paid" => "Đủ", "PartiallyPaid" => "Một phần", _ => "Chưa trả" };
        };
        _slipGrid.SelectionChanged += (_, _) =>
        {
            if (!_loading && _slipGrid.CurrentRow?.DataBoundItem is PayrollSlipDto slip) SelectEmployee(slip.EmployeeId);
        };
        _employee.SelectedIndexChanged += (_, _) => UpdateButtons();
        _month.ValueChanged += async (_, _) => { if (!_loading) await RunAsync(LoadAsync); };
        Load += async (_, _) => await RunAsync(LoadAsync);
    }

    public PayrollControl(ApiServices api) : this()
    {
        _month.Value = DateTime.UtcNow.AddHours(7).Date;
        _api = api;
        _title.Text = CanViewPay ? "Công & lương nhân viên, tài xế" : "Công nhân viên, tài xế";
        _openPeriodButton.Visible = _salaryButton.Visible = CanViewPay;
        _manualAttendanceButton.Visible = _driverAttendanceButton.Visible = _confirmAttendanceButton.Visible
            = api.Connection.IsInRole("Admin", "Dispatcher");
        _approveButton.Visible = _adjustButton.Visible = api.Connection.IsInRole("Admin");
        if (!CanViewPay)
        {
            _tabs.TabPages.Remove(_payrollPage);
            Disposed += (_, _) => _payrollPage.Dispose();
        }
        UpdateButtons();
    }

    private async Task LoadAsync()
    {
        _loading = true;
        var employeeId = Employee?.Id;
        try
        {
            var periods = await _api.Payroll.PeriodsAsync();
            _period = periods.FirstOrDefault(p => p.Year == _month.Value.Year && p.Month == _month.Value.Month);
            _employees = await _api.Payroll.EmployeesAsync(_month.Value.Year, _month.Value.Month);
            var range = PayrollRules.PeriodRange(_month.Value.Year, _month.Value.Month);
            _attendance = _period is null ? new() : await _api.Payroll.AttendanceAsync(_period.Id);
            _slips = _period is null || !CanViewPay ? new() : await _api.Payroll.SlipsAsync(_period.Id);
            _employee.DataSource = _employees;
            if (employeeId.HasValue) SelectEmployee(employeeId.Value);
            _attendanceGrid.DataSource = _employees.Select(e =>
            {
                var a = _attendance.FirstOrDefault(a => a.EmployeeId == e.Id);
                return new { e.Id, e.Code, Name = e.FullName, Group = e.IsDriver ? "Tài xế" : "Nhân viên", a?.StandardDays,
                    PaidDays = a is null ? (decimal?)null : a.WorkedDays + a.PaidLeaveDays + a.HolidayDays,
                    UnpaidDays = a?.UnpaidLeaveDays, Overtime = a is null ? (decimal?)null : a.WeekdayOvertimeHours + a.RestDayOvertimeHours + a.HolidayOvertimeHours,
                    State = a is null ? "Chưa nhập" : _period?.ApprovedAt is null && (a.WorkFrom != range.From || a.WorkTo != range.To)
                        ? "Cần nhập lại công kỳ 26–25" : a.ConfirmedAt.HasValue ? "Đã xác nhận" : "Chưa xác nhận" };
            }).ToList();
            _slipGrid.DataSource = _slips;
            _status.Text = _period is null ? "Chưa mở kỳ. Admin/kế toán mở kỳ trước khi nhập công."
                : $"Kỳ {_period.Month:00}/{_period.Year} — {(_period.ApprovedAt.HasValue ? "Đã duyệt và khóa" : "Đang lập")} · {_employees.Count} nhân viên · {_attendance.Count(a => a.ConfirmedAt.HasValue && (_period.ApprovedAt.HasValue || (a.WorkFrom == range.From && a.WorkTo == range.To)))} đã xác nhận công";
            _status.Text += _period?.ApprovedAt is not null
                ? "\nKỳ đã duyệt: khoảng công được giữ theo từng phiếu lương."
                : $"\nKỳ công cho tất cả nhân viên/tài xế: {range.From:dd/MM/yyyy}–{range.To:dd/MM/yyyy}; chốt ngày 25.";
            _totals.Text = CanViewPay ? $"{_slips.Count} phiếu · Thực lĩnh: {_slips.Sum(s => s.NetPay):N0} đ · Đã trả: {_slips.Sum(s => s.PaidAmount):N0} đ · Còn trả: {_slips.Sum(s => s.RemainingAmount):N0} đ" : "";
        }
        catch
        {
            _period = null;
            _slips.Clear();
            _status.Text = "Không tải được kỳ; nhấn Tải kỳ để thử lại.";
            throw;
        }
        finally { _loading = false; UpdateButtons(); }
    }

    private async Task SalaryAsync()
    {
        if (!RequireEmployee()) return;
        var employee = Employee!;
        var history = await _api.Payroll.SalariesAsync(employee.Id);
        using var form = new PayrollInputForm($"Mức lương — {employee.Code} · {employee.FullName}", PayrollInputSection.Salary);
        form.SalaryHistory.DataSource = history;
        if (_api.Connection.IsInRole("Admin"))
        {
            var start = PayrollRules.PeriodRange(_month.Value.Year, _month.Value.Month).From;
            if (employee.HireDate.Date > start) start = employee.HireDate.Date;
            if (history.Count > 0 && start <= history[^1].EffectiveFrom)
            {
                start = new DateTime(history[^1].EffectiveFrom.Year, history[^1].EffectiveFrom.Month, 26);
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

    private async Task AttendanceAsync()
    {
        if (!RequireEmployee() || _period is null) return;
        if (Employee!.IsDriver) throw new InvalidOperationException("Chọn Lấy công tài xế để tổng hợp từ lịch đăng ký.");
        var employee = Employee!;
        var a = _attendance.FirstOrDefault(a => a.EmployeeId == employee.Id);
        using var form = new PayrollInputForm($"Công tháng — {employee.FullName}", PayrollInputSection.Attendance);
        var range = PayrollRules.PeriodRange(_period.Year, _period.Month);
        form.AttendancePeriod.Text = $"Kỳ công: {range.From:dd/MM/yyyy}–{range.To:dd/MM/yyyy}; chốt ngày 25.";
        form.AttendanceWarning.Visible = a is not null && (a.WorkFrom != range.From || a.WorkTo != range.To);
        form.EmploymentDates.Text = $"Vào làm: {employee.HireDate:dd/MM/yyyy} · Nghỉ việc: {employee.TerminationDate:dd/MM/yyyy}";
        PayrollInputForm.SetNumber(form.StandardDays, a?.StandardDays ?? 0, (range.To - range.From).Days + 1);
        PayrollInputForm.SetNumber(form.WorkedDays, a?.WorkedDays ?? 0);
        PayrollInputForm.SetNumber(form.PaidLeaveDays, a?.PaidLeaveDays ?? 0);
        PayrollInputForm.SetNumber(form.HolidayDays, a?.HolidayDays ?? 0);
        PayrollInputForm.SetNumber(form.UnpaidLeaveDays, a?.UnpaidLeaveDays ?? 0);
        PayrollInputForm.SetNumber(form.WeekdayOvertimeHours, a?.WeekdayOvertimeHours ?? 0);
        PayrollInputForm.SetNumber(form.RestDayOvertimeHours, a?.RestDayOvertimeHours ?? 0);
        PayrollInputForm.SetNumber(form.HolidayOvertimeHours, a?.HolidayOvertimeHours ?? 0);
        form.AttendanceNote.Text = a?.Note ?? "";
        form.OnSave(() => _api.Payroll.SaveAttendanceAsync(_period.Id, employee.Id, new(_period.Revision,
            form.StandardDays.Value, form.WorkedDays.Value, form.PaidLeaveDays.Value, form.HolidayDays.Value,
            form.UnpaidLeaveDays.Value, form.WeekdayOvertimeHours.Value, form.RestDayOvertimeHours.Value,
            form.HolidayOvertimeHours.Value, form.AttendanceNote.Text)));
        if (form.ShowDialog(this) == DialogResult.OK) await LoadAsync();
    }

    private async Task DriverAttendanceAsync()
    {
        if (!RequireEmployee() || _period is null) return;
        var employee = Employee!;
        if (!employee.IsDriver) throw new InvalidOperationException("Chọn tài xế để lấy công từ lịch.");
        var source = await _api.Payroll.DriverWorkDaysAsync(_period.Id, employee.Id);
        var attendance = _attendance.FirstOrDefault(a => a.EmployeeId == employee.Id);
        using var form = new PayrollInputForm($"Lấy công tài xế — {employee.FullName}", PayrollInputSection.DriverAttendance);
        form.DriverPeriod.Text = $"Kỳ công: {source.FromDate:dd/MM/yyyy}–{source.ToDate:dd/MM/yyyy}\n{source.Dates.Count} ngày đăng ký riêng biệt = {source.Dates.Count} công. Chốt ngày 25.";
        PayrollInputForm.SetNumber(form.DriverStandardDays, attendance?.StandardDays ?? 0, (source.ToDate - source.FromDate).Days + 1);
        form.DriverDates.Text = string.Join(Environment.NewLine, source.Dates.Select(d => d.ToString("dd/MM/yyyy")));
        form.OnSave(() => _api.Payroll.ImportDriverAttendanceAsync(_period.Id, employee.Id, new(_period.Revision, form.DriverStandardDays.Value)));
        if (form.ShowDialog(this) == DialogResult.OK) await LoadAsync();
    }

    private async Task ItemAsync(bool adjustment)
    {
        if (!RequireSlip() || _period is null) return;
        var slip = Slip!;
        using var form = new PayrollInputForm($"{(adjustment ? "Điều chỉnh sau duyệt" : "Khoản cộng / trừ")} — {slip.EmployeeName}", PayrollInputSection.Item);
        var kind = form.ItemKind;
        var prorate = form.ItemProrate;
        prorate.Enabled = !adjustment;
        kind.SelectedIndexChanged += (_, _) => { prorate.Enabled = !adjustment && (string?)kind.SelectedValue == "Allowance"; if (!prorate.Enabled) prorate.Checked = false; };
        if (adjustment) form.ItemHint.Text = "Giữ nguyên phiếu duyệt gốc; khoản này được lưu riêng cùng người điều chỉnh.";
        form.OnSave(() => _api.Payroll.AddItemAsync(slip.Id, new(_period.Revision, (string)kind.SelectedValue!, form.ItemAmount.Value, prorate.Checked, form.ItemReason.Text), adjustment));
        if (form.ShowDialog(this) == DialogResult.OK) await LoadAsync();
    }

    private async Task DeleteItemAsync()
    {
        if (!RequireSlip() || _period is null) return;
        var slip = Slip!;
        if (slip.Items.Count == 0) throw new InvalidOperationException("Phiếu chưa có khoản cộng/trừ.");
        using var form = new PayrollInputForm($"Xóa khoản nháp — {slip.EmployeeName}", PayrollInputSection.DeleteItem);
        var item = form.ItemToDelete;
        item.DisplayMember = "Text";
        item.ValueMember = "Id";
        item.DataSource = slip.Items.Select(i => new { i.Id, Text = $"{i.CalculatedAmount:N0} đ — {i.Reason}" }).ToList();
        form.OnSave(() => _api.Payroll.DeleteItemAsync(slip.Id, (int)item.SelectedValue!, _period.Revision));
        if (form.ShowDialog(this) == DialogResult.OK) await LoadAsync();
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
        form.DetailItems.DataSource = s.Items.Select(i => new { Kind = i.Kind switch { "Allowance" => "Phụ cấp", "Bonus" => "Thưởng", "Overtime" => "Tăng ca", _ => "Khấu trừ" },
            Amount = i.Amount.ToString("N0"), ProrateByAttendance = i.ProrateByAttendance ? "Có" : "Không", CalculatedAmount = i.CalculatedAmount.ToString("N0"), i.Reason, IsAdjustment = i.IsAdjustment ? "Có" : "Không" }).ToList();
        form.DetailPayments.DataSource = s.Payments.Select(p => new { Date = p.PaidOn.ToString("dd/MM/yyyy"), Amount = p.Amount.ToString("N0"), Method = p.Method == "Cash" ? "Tiền mặt" : "Chuyển khoản", p.Reference, p.RecordedBy }).ToList();
        form.DetailDriverDates.Visible = s.IsDriver;
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
    private void UpdateButtons()
    {
        foreach (var b in _draftButtons) b.Enabled = _period is { ApprovedAt: null };
        foreach (var b in _approvedButtons) b.Enabled = _period?.ApprovedAt is not null && Slip is not null;
        foreach (var b in _slipButtons) b.Enabled = Slip is not null;
        if (_manualAttendanceButton is not null) _manualAttendanceButton.Enabled = _period is { ApprovedAt: null } && Employee is { IsDriver: false };
        if (_driverAttendanceButton is not null) _driverAttendanceButton.Enabled = _period is { ApprovedAt: null } && Employee is { IsDriver: true };
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
