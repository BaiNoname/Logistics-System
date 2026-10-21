namespace TruckingSystem.Desktop.GUI_PayrollManagement;

partial class PayrollControl
{
    private System.ComponentModel.IContainer components = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        _layout = new TableLayoutPanel();
        _title = new Label();
        _toolbar = new FlowLayoutPanel();
        _monthLabel = new Label();
        _month = new DateTimePicker();
        _refreshButton = new Button();
        _openPeriodButton = new Button();
        _employee = new ComboBox();
        _salaryButton = new Button();
        _status = new Label();
        _totals = new Label();
        _tabs = new TabControl();
        _attendancePage = new TabPage();
        _attendanceLayout = new TableLayoutPanel();
        _attendanceBar = new FlowLayoutPanel();
        _manualAttendanceButton = new Button();
        _driverAttendanceButton = new Button();
        _confirmAttendanceButton = new Button();
        _attendanceNote = new Label();
        _attendanceGrid = new DataGridView();
        _payrollPage = new TabPage();
        _payrollLayout = new TableLayoutPanel();
        _payrollBar = new FlowLayoutPanel();
        _generateButton = new Button();
        _detailsButton = new Button();
        _addItemButton = new Button();
        _deleteItemButton = new Button();
        _deleteDraftButton = new Button();
        _approveButton = new Button();
        _adjustButton = new Button();
        _payButton = new Button();
        _exportButton = new Button();
        _slipGrid = new DataGridView();
        _attendanceIdColumn = new DataGridViewTextBoxColumn();
        _attendanceCodeColumn = new DataGridViewTextBoxColumn();
        _attendanceNameColumn = new DataGridViewTextBoxColumn();
        _attendanceGroupColumn = new DataGridViewTextBoxColumn();
        _attendanceStandardDaysColumn = new DataGridViewTextBoxColumn();
        _attendancePaidDaysColumn = new DataGridViewTextBoxColumn();
        _attendanceUnpaidDaysColumn = new DataGridViewTextBoxColumn();
        _attendanceOvertimeColumn = new DataGridViewTextBoxColumn();
        _attendanceStateColumn = new DataGridViewTextBoxColumn();
        _slipEmployeeCodeColumn = new DataGridViewTextBoxColumn();
        _slipEmployeeNameColumn = new DataGridViewTextBoxColumn();
        _slipEarnedSalaryColumn = new DataGridViewTextBoxColumn();
        _slipGrossIncomeColumn = new DataGridViewTextBoxColumn();
        _slipDeductionsColumn = new DataGridViewTextBoxColumn();
        _slipNetPayColumn = new DataGridViewTextBoxColumn();
        _slipPaidAmountColumn = new DataGridViewTextBoxColumn();
        _slipRemainingAmountColumn = new DataGridViewTextBoxColumn();
        _slipPaymentStatusColumn = new DataGridViewTextBoxColumn();
        _layout.SuspendLayout();
        _toolbar.SuspendLayout();
        _tabs.SuspendLayout();
        _attendancePage.SuspendLayout();
        _attendanceLayout.SuspendLayout();
        _attendanceBar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_attendanceGrid).BeginInit();
        _payrollPage.SuspendLayout();
        _payrollLayout.SuspendLayout();
        _payrollBar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_slipGrid).BeginInit();
        SuspendLayout();
        _attendanceIdColumn.Name = "Id";
        _attendanceIdColumn.DataPropertyName = "Id";
        _attendanceIdColumn.HeaderText = "";
        _attendanceIdColumn.MinimumWidth = 85;
        _attendanceCodeColumn.Name = "Code";
        _attendanceCodeColumn.DataPropertyName = "Code";
        _attendanceCodeColumn.HeaderText = "Mã NV";
        _attendanceCodeColumn.MinimumWidth = 85;
        _attendanceNameColumn.Name = "Name";
        _attendanceNameColumn.DataPropertyName = "Name";
        _attendanceNameColumn.HeaderText = "Họ tên";
        _attendanceNameColumn.MinimumWidth = 85;
        _attendanceGroupColumn.Name = "Group";
        _attendanceGroupColumn.DataPropertyName = "Group";
        _attendanceGroupColumn.HeaderText = "Nhóm";
        _attendanceGroupColumn.MinimumWidth = 85;
        _attendanceStandardDaysColumn.Name = "StandardDays";
        _attendanceStandardDaysColumn.DataPropertyName = "StandardDays";
        _attendanceStandardDaysColumn.HeaderText = "Công chuẩn";
        _attendanceStandardDaysColumn.MinimumWidth = 85;
        _attendancePaidDaysColumn.Name = "PaidDays";
        _attendancePaidDaysColumn.DataPropertyName = "PaidDays";
        _attendancePaidDaysColumn.HeaderText = "Công hưởng";
        _attendancePaidDaysColumn.MinimumWidth = 85;
        _attendanceUnpaidDaysColumn.Name = "UnpaidDays";
        _attendanceUnpaidDaysColumn.DataPropertyName = "UnpaidDays";
        _attendanceUnpaidDaysColumn.HeaderText = "Nghỉ không lương";
        _attendanceUnpaidDaysColumn.MinimumWidth = 85;
        _attendanceOvertimeColumn.Name = "Overtime";
        _attendanceOvertimeColumn.DataPropertyName = "Overtime";
        _attendanceOvertimeColumn.HeaderText = "Giờ tăng ca";
        _attendanceOvertimeColumn.MinimumWidth = 85;
        _attendanceStateColumn.Name = "State";
        _attendanceStateColumn.DataPropertyName = "State";
        _attendanceStateColumn.HeaderText = "Xác nhận";
        _attendanceStateColumn.MinimumWidth = 85;
        _attendanceGrid.Columns.AddRange(_attendanceIdColumn, _attendanceCodeColumn, _attendanceNameColumn, _attendanceGroupColumn, _attendanceStandardDaysColumn, _attendancePaidDaysColumn, _attendanceUnpaidDaysColumn, _attendanceOvertimeColumn, _attendanceStateColumn);
        _slipEmployeeCodeColumn.Name = "EmployeeCode";
        _slipEmployeeCodeColumn.DataPropertyName = "EmployeeCode";
        _slipEmployeeCodeColumn.HeaderText = "Mã NV";
        _slipEmployeeCodeColumn.MinimumWidth = 85;
        _slipEmployeeNameColumn.Name = "EmployeeName";
        _slipEmployeeNameColumn.DataPropertyName = "EmployeeName";
        _slipEmployeeNameColumn.HeaderText = "Họ tên";
        _slipEmployeeNameColumn.MinimumWidth = 85;
        _slipEarnedSalaryColumn.Name = "EarnedSalary";
        _slipEarnedSalaryColumn.DataPropertyName = "EarnedSalary";
        _slipEarnedSalaryColumn.HeaderText = "Lương theo công";
        _slipEarnedSalaryColumn.MinimumWidth = 85;
        _slipGrossIncomeColumn.Name = "GrossIncome";
        _slipGrossIncomeColumn.DataPropertyName = "GrossIncome";
        _slipGrossIncomeColumn.HeaderText = "Tổng thu nhập";
        _slipGrossIncomeColumn.MinimumWidth = 85;
        _slipDeductionsColumn.Name = "Deductions";
        _slipDeductionsColumn.DataPropertyName = "Deductions";
        _slipDeductionsColumn.HeaderText = "Khấu trừ";
        _slipDeductionsColumn.MinimumWidth = 85;
        _slipNetPayColumn.Name = "NetPay";
        _slipNetPayColumn.DataPropertyName = "NetPay";
        _slipNetPayColumn.HeaderText = "Thực lĩnh";
        _slipNetPayColumn.MinimumWidth = 85;
        _slipPaidAmountColumn.Name = "PaidAmount";
        _slipPaidAmountColumn.DataPropertyName = "PaidAmount";
        _slipPaidAmountColumn.HeaderText = "Đã trả";
        _slipPaidAmountColumn.MinimumWidth = 85;
        _slipRemainingAmountColumn.Name = "RemainingAmount";
        _slipRemainingAmountColumn.DataPropertyName = "RemainingAmount";
        _slipRemainingAmountColumn.HeaderText = "Còn trả";
        _slipRemainingAmountColumn.MinimumWidth = 85;
        _slipPaymentStatusColumn.Name = "PaymentStatus";
        _slipPaymentStatusColumn.DataPropertyName = "PaymentStatus";
        _slipPaymentStatusColumn.HeaderText = "Chi trả";
        _slipPaymentStatusColumn.MinimumWidth = 85;
        _slipGrid.Columns.AddRange(_slipEmployeeCodeColumn, _slipEmployeeNameColumn, _slipEarnedSalaryColumn, _slipGrossIncomeColumn, _slipDeductionsColumn, _slipNetPayColumn, _slipPaidAmountColumn, _slipRemainingAmountColumn, _slipPaymentStatusColumn);
        _attendanceIdColumn.Visible = false;
        // 
        // _layout
        // 
        _layout.ColumnCount = 1;
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _layout.Controls.Add(_title, 0, 0);
        _layout.Controls.Add(_toolbar, 0, 1);
        _layout.Controls.Add(_status, 0, 2);
        _layout.Controls.Add(_totals, 0, 3);
        _layout.Controls.Add(_tabs, 0, 4);
        _layout.Dock = DockStyle.Fill;
        _layout.Location = new Point(12, 12);
        _layout.Name = "_layout";
        _layout.RowCount = 5;
        _layout.RowStyles.Add(new RowStyle());
        _layout.RowStyles.Add(new RowStyle());
        _layout.RowStyles.Add(new RowStyle());
        _layout.RowStyles.Add(new RowStyle());
        _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _layout.Size = new Size(1156, 696);
        _layout.TabIndex = 0;
        // 
        // _title
        // 
        _title.AutoSize = true;
        _title.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
        _title.ForeColor = Color.FromArgb(15, 23, 42);
        _title.Location = new Point(3, 0);
        _title.Name = "_title";
        _title.Size = new Size(351, 32);
        _title.TabIndex = 0;
        _title.Text = "Công & lương nhân viên, tài xế";
        // 
        // _toolbar
        // 
        _toolbar.AutoSize = true;
        _toolbar.Controls.Add(_monthLabel);
        _toolbar.Controls.Add(_month);
        _toolbar.Controls.Add(_refreshButton);
        _toolbar.Controls.Add(_openPeriodButton);
        _toolbar.Controls.Add(_employee);
        _toolbar.Controls.Add(_salaryButton);
        _toolbar.Dock = DockStyle.Fill;
        _toolbar.Location = new Point(3, 35);
        _toolbar.Name = "_toolbar";
        _toolbar.Padding = new Padding(0, 6, 0, 6);
        _toolbar.Size = new Size(1150, 56);
        _toolbar.TabIndex = 1;
        // 
        // _monthLabel
        // 
        _monthLabel.AutoSize = true;
        _monthLabel.Location = new Point(3, 6);
        _monthLabel.Name = "_monthLabel";
        _monthLabel.Padding = new Padding(0, 10, 0, 0);
        _monthLabel.Size = new Size(63, 29);
        _monthLabel.TabIndex = 0;
        _monthLabel.Text = "Kỳ tháng";
        // 
        // _month
        // 
        _month.AccessibleName = "Tháng tính lương";
        _month.CustomFormat = "MM/yyyy";
        _month.Format = DateTimePickerFormat.Custom;
        _month.Location = new Point(72, 9);
        _month.Name = "_month";
        _month.ShowUpDown = true;
        _month.Size = new Size(130, 25);
        _month.TabIndex = 1;
        // 
        // _refreshButton
        // 
        _refreshButton.AutoSize = true;
        _refreshButton.BackColor = Color.White;
        _refreshButton.Cursor = Cursors.Hand;
        _refreshButton.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
        _refreshButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
        _refreshButton.FlatStyle = FlatStyle.Flat;
        _refreshButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _refreshButton.ForeColor = Color.FromArgb(15, 23, 42);
        _refreshButton.Location = new Point(208, 9);
        _refreshButton.Name = "_refreshButton";
        _refreshButton.Size = new Size(140, 38);
        _refreshButton.TabIndex = 1;
        _refreshButton.Text = "Tải kỳ";
        _refreshButton.UseVisualStyleBackColor = false;
        // 
        // _openPeriodButton
        // 
        _openPeriodButton.AutoSize = true;
        _openPeriodButton.BackColor = Color.White;
        _openPeriodButton.Cursor = Cursors.Hand;
        _openPeriodButton.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
        _openPeriodButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
        _openPeriodButton.FlatStyle = FlatStyle.Flat;
        _openPeriodButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _openPeriodButton.ForeColor = Color.FromArgb(15, 23, 42);
        _openPeriodButton.Location = new Point(354, 9);
        _openPeriodButton.Name = "_openPeriodButton";
        _openPeriodButton.Size = new Size(140, 38);
        _openPeriodButton.TabIndex = 2;
        _openPeriodButton.Text = "Mở kỳ";
        _openPeriodButton.UseVisualStyleBackColor = false;
        // 
        // _employee
        // 
        _employee.AccessibleName = "Nhân viên trong kỳ";
        _employee.DisplayMember = "FullName";
        _employee.DropDownStyle = ComboBoxStyle.DropDownList;
        _employee.Location = new Point(500, 9);
        _employee.Name = "_employee";
        _employee.Size = new Size(300, 25);
        _employee.TabIndex = 3;
        // 
        // _salaryButton
        // 
        _salaryButton.AutoSize = true;
        _salaryButton.BackColor = Color.White;
        _salaryButton.Cursor = Cursors.Hand;
        _salaryButton.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
        _salaryButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
        _salaryButton.FlatStyle = FlatStyle.Flat;
        _salaryButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _salaryButton.ForeColor = Color.FromArgb(15, 23, 42);
        _salaryButton.Location = new Point(806, 9);
        _salaryButton.Name = "_salaryButton";
        _salaryButton.Size = new Size(149, 38);
        _salaryButton.TabIndex = 3;
        _salaryButton.Text = "Mức lương / lịch sử";
        _salaryButton.UseVisualStyleBackColor = false;
        // 
        // _status
        // 
        _status.AccessibleRole = AccessibleRole.StatusBar;
        _status.AutoSize = true;
        _status.Location = new Point(3, 94);
        _status.Name = "_status";
        _status.Padding = new Padding(8);
        _status.Size = new Size(16, 35);
        _status.TabIndex = 2;
        // 
        // _totals
        // 
        _totals.AutoSize = true;
        _totals.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _totals.Location = new Point(3, 129);
        _totals.Name = "_totals";
        _totals.Padding = new Padding(8);
        _totals.Size = new Size(16, 35);
        _totals.TabIndex = 3;
        // 
        // _tabs
        // 
        _tabs.Controls.Add(_attendancePage);
        _tabs.Controls.Add(_payrollPage);
        _tabs.Dock = DockStyle.Fill;
        _tabs.Location = new Point(3, 167);
        _tabs.Name = "_tabs";
        _tabs.SelectedIndex = 0;
        _tabs.Size = new Size(1150, 526);
        _tabs.TabIndex = 4;
        // 
        // _attendancePage
        // 
        _attendancePage.Controls.Add(_attendanceLayout);
        _attendancePage.Location = new Point(4, 26);
        _attendancePage.Name = "_attendancePage";
        _attendancePage.Size = new Size(1142, 496);
        _attendancePage.TabIndex = 0;
        _attendancePage.Text = "Công tháng";
        _attendancePage.UseVisualStyleBackColor = true;
        // 
        // _attendanceLayout
        // 
        _attendanceLayout.ColumnCount = 1;
        _attendanceLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _attendanceLayout.Controls.Add(_attendanceBar, 0, 0);
        _attendanceLayout.Controls.Add(_attendanceGrid, 0, 1);
        _attendanceLayout.Dock = DockStyle.Fill;
        _attendanceLayout.Location = new Point(0, 0);
        _attendanceLayout.Name = "_attendanceLayout";
        _attendanceLayout.RowCount = 2;
        _attendanceLayout.RowStyles.Add(new RowStyle());
        _attendanceLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _attendanceLayout.Size = new Size(1142, 496);
        _attendanceLayout.TabIndex = 0;
        // 
        // _attendanceBar
        // 
        _attendanceBar.AutoSize = true;
        _attendanceBar.Controls.Add(_manualAttendanceButton);
        _attendanceBar.Controls.Add(_driverAttendanceButton);
        _attendanceBar.Controls.Add(_confirmAttendanceButton);
        _attendanceBar.Controls.Add(_attendanceNote);
        _attendanceBar.Dock = DockStyle.Fill;
        _attendanceBar.Location = new Point(3, 3);
        _attendanceBar.Name = "_attendanceBar";
        _attendanceBar.Padding = new Padding(0, 6, 0, 6);
        _attendanceBar.Size = new Size(1136, 56);
        _attendanceBar.TabIndex = 0;
        // 
        // _manualAttendanceButton
        // 
        _manualAttendanceButton.AutoSize = true;
        _manualAttendanceButton.BackColor = Color.White;
        _manualAttendanceButton.Cursor = Cursors.Hand;
        _manualAttendanceButton.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
        _manualAttendanceButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
        _manualAttendanceButton.FlatStyle = FlatStyle.Flat;
        _manualAttendanceButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _manualAttendanceButton.ForeColor = Color.FromArgb(15, 23, 42);
        _manualAttendanceButton.Location = new Point(3, 9);
        _manualAttendanceButton.Name = "_manualAttendanceButton";
        _manualAttendanceButton.Size = new Size(140, 38);
        _manualAttendanceButton.TabIndex = 4;
        _manualAttendanceButton.Text = "Nhập / sửa công";
        _manualAttendanceButton.UseVisualStyleBackColor = false;
        // 
        // _driverAttendanceButton
        // 
        _driverAttendanceButton.AutoSize = true;
        _driverAttendanceButton.BackColor = Color.White;
        _driverAttendanceButton.Cursor = Cursors.Hand;
        _driverAttendanceButton.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
        _driverAttendanceButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
        _driverAttendanceButton.FlatStyle = FlatStyle.Flat;
        _driverAttendanceButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _driverAttendanceButton.ForeColor = Color.FromArgb(15, 23, 42);
        _driverAttendanceButton.Location = new Point(149, 9);
        _driverAttendanceButton.Name = "_driverAttendanceButton";
        _driverAttendanceButton.Size = new Size(140, 38);
        _driverAttendanceButton.TabIndex = 5;
        _driverAttendanceButton.Text = "Lấy công tài xế";
        _driverAttendanceButton.UseVisualStyleBackColor = false;
        // 
        // _confirmAttendanceButton
        // 
        _confirmAttendanceButton.AutoSize = true;
        _confirmAttendanceButton.BackColor = Color.White;
        _confirmAttendanceButton.Cursor = Cursors.Hand;
        _confirmAttendanceButton.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
        _confirmAttendanceButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
        _confirmAttendanceButton.FlatStyle = FlatStyle.Flat;
        _confirmAttendanceButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _confirmAttendanceButton.ForeColor = Color.FromArgb(15, 23, 42);
        _confirmAttendanceButton.Location = new Point(295, 9);
        _confirmAttendanceButton.Name = "_confirmAttendanceButton";
        _confirmAttendanceButton.Size = new Size(140, 38);
        _confirmAttendanceButton.TabIndex = 6;
        _confirmAttendanceButton.Text = "Xác nhận công";
        _confirmAttendanceButton.UseVisualStyleBackColor = false;
        // 
        // _attendanceNote
        // 
        _attendanceNote.AutoSize = true;
        _attendanceNote.Location = new Point(441, 6);
        _attendanceNote.Name = "_attendanceNote";
        _attendanceNote.Padding = new Padding(4, 10, 0, 0);
        _attendanceNote.Size = new Size(481, 29);
        _attendanceNote.TabIndex = 7;
        _attendanceNote.Text = "Kỳ 26–25 cho tất cả: nhân viên nhập công tổng hợp; tài xế lấy ngày đăng ký.";
        // 
        // _attendanceGrid
        // 
        _attendanceGrid.AccessibleName = "Công nhân viên và tài xế trong kỳ";
        _attendanceGrid.AllowUserToAddRows = false;
        _attendanceGrid.AllowUserToResizeRows = false;
        _attendanceGrid.BackgroundColor = Color.White;
        _attendanceGrid.Dock = DockStyle.Fill;
        _attendanceGrid.Location = new Point(3, 65);
        _attendanceGrid.MultiSelect = false;
        _attendanceGrid.Name = "_attendanceGrid";
        _attendanceGrid.ReadOnly = true;
        _attendanceGrid.RowHeadersVisible = false;
        _attendanceGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _attendanceGrid.Size = new Size(1136, 428);
        _attendanceGrid.TabIndex = 1;
        // 
        // _payrollPage
        // 
        _payrollPage.Controls.Add(_payrollLayout);
        _payrollPage.Location = new Point(4, 26);
        _payrollPage.Name = "_payrollPage";
        _payrollPage.Size = new Size(186, 0);
        _payrollPage.TabIndex = 1;
        _payrollPage.Text = "Bảng lương";
        _payrollPage.UseVisualStyleBackColor = true;
        // 
        // _payrollLayout
        // 
        _payrollLayout.ColumnCount = 1;
        _payrollLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _payrollLayout.Controls.Add(_payrollBar, 0, 0);
        _payrollLayout.Controls.Add(_slipGrid, 0, 1);
        _payrollLayout.Dock = DockStyle.Fill;
        _payrollLayout.Location = new Point(0, 0);
        _payrollLayout.Name = "_payrollLayout";
        _payrollLayout.RowCount = 2;
        _payrollLayout.RowStyles.Add(new RowStyle());
        _payrollLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _payrollLayout.Size = new Size(186, 0);
        _payrollLayout.TabIndex = 0;
        // 
        // _payrollBar
        // 
        _payrollBar.AutoSize = true;
        _payrollBar.Controls.Add(_generateButton);
        _payrollBar.Controls.Add(_detailsButton);
        _payrollBar.Controls.Add(_addItemButton);
        _payrollBar.Controls.Add(_deleteItemButton);
        _payrollBar.Controls.Add(_deleteDraftButton);
        _payrollBar.Controls.Add(_approveButton);
        _payrollBar.Controls.Add(_adjustButton);
        _payrollBar.Controls.Add(_payButton);
        _payrollBar.Controls.Add(_exportButton);
        _payrollBar.Dock = DockStyle.Fill;
        _payrollBar.Location = new Point(3, 3);
        _payrollBar.Name = "_payrollBar";
        _payrollBar.Padding = new Padding(0, 6, 0, 6);
        _payrollBar.Size = new Size(180, 408);
        _payrollBar.TabIndex = 0;
        // 
        // _generateButton
        // 
        _generateButton.AutoSize = true;
        _generateButton.BackColor = Color.White;
        _generateButton.Cursor = Cursors.Hand;
        _generateButton.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
        _generateButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
        _generateButton.FlatStyle = FlatStyle.Flat;
        _generateButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _generateButton.ForeColor = Color.FromArgb(15, 23, 42);
        _generateButton.Location = new Point(3, 9);
        _generateButton.Name = "_generateButton";
        _generateButton.Size = new Size(140, 38);
        _generateButton.TabIndex = 7;
        _generateButton.Text = "Lập phiếu";
        _generateButton.UseVisualStyleBackColor = false;
        // 
        // _detailsButton
        // 
        _detailsButton.AutoSize = true;
        _detailsButton.BackColor = Color.White;
        _detailsButton.Cursor = Cursors.Hand;
        _detailsButton.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
        _detailsButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
        _detailsButton.FlatStyle = FlatStyle.Flat;
        _detailsButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _detailsButton.ForeColor = Color.FromArgb(15, 23, 42);
        _detailsButton.Location = new Point(3, 53);
        _detailsButton.Name = "_detailsButton";
        _detailsButton.Size = new Size(140, 38);
        _detailsButton.TabIndex = 8;
        _detailsButton.Text = "Chi tiết phiếu";
        _detailsButton.UseVisualStyleBackColor = false;
        // 
        // _addItemButton
        // 
        _addItemButton.AutoSize = true;
        _addItemButton.BackColor = Color.White;
        _addItemButton.Cursor = Cursors.Hand;
        _addItemButton.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
        _addItemButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
        _addItemButton.FlatStyle = FlatStyle.Flat;
        _addItemButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _addItemButton.ForeColor = Color.FromArgb(15, 23, 42);
        _addItemButton.Location = new Point(3, 97);
        _addItemButton.Name = "_addItemButton";
        _addItemButton.Size = new Size(140, 38);
        _addItemButton.TabIndex = 9;
        _addItemButton.Text = "Thêm cộng / trừ";
        _addItemButton.UseVisualStyleBackColor = false;
        // 
        // _deleteItemButton
        // 
        _deleteItemButton.AutoSize = true;
        _deleteItemButton.BackColor = Color.White;
        _deleteItemButton.Cursor = Cursors.Hand;
        _deleteItemButton.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
        _deleteItemButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
        _deleteItemButton.FlatStyle = FlatStyle.Flat;
        _deleteItemButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _deleteItemButton.ForeColor = Color.FromArgb(15, 23, 42);
        _deleteItemButton.Location = new Point(3, 141);
        _deleteItemButton.Name = "_deleteItemButton";
        _deleteItemButton.Size = new Size(140, 38);
        _deleteItemButton.TabIndex = 10;
        _deleteItemButton.Text = "Xóa khoản";
        _deleteItemButton.UseVisualStyleBackColor = false;
        // 
        // _deleteDraftButton
        // 
        _deleteDraftButton.AutoSize = true;
        _deleteDraftButton.BackColor = Color.White;
        _deleteDraftButton.Cursor = Cursors.Hand;
        _deleteDraftButton.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
        _deleteDraftButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
        _deleteDraftButton.FlatStyle = FlatStyle.Flat;
        _deleteDraftButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _deleteDraftButton.ForeColor = Color.FromArgb(15, 23, 42);
        _deleteDraftButton.Location = new Point(3, 185);
        _deleteDraftButton.Name = "_deleteDraftButton";
        _deleteDraftButton.Size = new Size(140, 38);
        _deleteDraftButton.TabIndex = 11;
        _deleteDraftButton.Text = "Xóa phiếu nháp";
        _deleteDraftButton.UseVisualStyleBackColor = false;
        // 
        // _approveButton
        // 
        _approveButton.AutoSize = true;
        _approveButton.BackColor = Color.White;
        _approveButton.Cursor = Cursors.Hand;
        _approveButton.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
        _approveButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
        _approveButton.FlatStyle = FlatStyle.Flat;
        _approveButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _approveButton.ForeColor = Color.FromArgb(15, 23, 42);
        _approveButton.Location = new Point(3, 229);
        _approveButton.Name = "_approveButton";
        _approveButton.Size = new Size(140, 38);
        _approveButton.TabIndex = 12;
        _approveButton.Text = "Duyệt & khóa kỳ";
        _approveButton.UseVisualStyleBackColor = false;
        // 
        // _adjustButton
        // 
        _adjustButton.AutoSize = true;
        _adjustButton.BackColor = Color.White;
        _adjustButton.Cursor = Cursors.Hand;
        _adjustButton.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
        _adjustButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
        _adjustButton.FlatStyle = FlatStyle.Flat;
        _adjustButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _adjustButton.ForeColor = Color.FromArgb(15, 23, 42);
        _adjustButton.Location = new Point(3, 273);
        _adjustButton.Name = "_adjustButton";
        _adjustButton.Size = new Size(158, 38);
        _adjustButton.TabIndex = 13;
        _adjustButton.Text = "Điều chỉnh sau duyệt";
        _adjustButton.UseVisualStyleBackColor = false;
        // 
        // _payButton
        // 
        _payButton.AutoSize = true;
        _payButton.BackColor = Color.White;
        _payButton.Cursor = Cursors.Hand;
        _payButton.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
        _payButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
        _payButton.FlatStyle = FlatStyle.Flat;
        _payButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _payButton.ForeColor = Color.FromArgb(15, 23, 42);
        _payButton.Location = new Point(3, 317);
        _payButton.Name = "_payButton";
        _payButton.Size = new Size(140, 38);
        _payButton.TabIndex = 14;
        _payButton.Text = "Ghi nhận chi trả";
        _payButton.UseVisualStyleBackColor = false;
        // 
        // _exportButton
        // 
        _exportButton.AutoSize = true;
        _exportButton.BackColor = Color.White;
        _exportButton.Cursor = Cursors.Hand;
        _exportButton.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
        _exportButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
        _exportButton.FlatStyle = FlatStyle.Flat;
        _exportButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _exportButton.ForeColor = Color.FromArgb(15, 23, 42);
        _exportButton.Location = new Point(3, 361);
        _exportButton.Name = "_exportButton";
        _exportButton.Size = new Size(140, 38);
        _exportButton.TabIndex = 15;
        _exportButton.Text = "Xuất phiếu / in";
        _exportButton.UseVisualStyleBackColor = false;
        // 
        // _slipGrid
        // 
        _slipGrid.AccessibleName = "Bảng lương nhân viên và tài xế";
        _slipGrid.AllowUserToAddRows = false;
        _slipGrid.AllowUserToResizeRows = false;
        _slipGrid.BackgroundColor = Color.White;
        _slipGrid.Dock = DockStyle.Fill;
        _slipGrid.Location = new Point(3, 417);
        _slipGrid.MultiSelect = false;
        _slipGrid.Name = "_slipGrid";
        _slipGrid.ReadOnly = true;
        _slipGrid.RowHeadersVisible = false;
        _slipGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _slipGrid.Size = new Size(180, 1);
        _slipGrid.TabIndex = 1;
        // 
        // PayrollControl
        // 
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.FromArgb(241, 245, 249);
        Controls.Add(_layout);
        Name = "PayrollControl";
        Padding = new Padding(12);
        Size = new Size(1180, 720);
        _layout.ResumeLayout(false);
        _layout.PerformLayout();
        _toolbar.ResumeLayout(false);
        _toolbar.PerformLayout();
        _tabs.ResumeLayout(false);
        _attendancePage.ResumeLayout(false);
        _attendanceLayout.ResumeLayout(false);
        _attendanceLayout.PerformLayout();
        _attendanceBar.ResumeLayout(false);
        _attendanceBar.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_attendanceGrid).EndInit();
        _payrollPage.ResumeLayout(false);
        _payrollLayout.ResumeLayout(false);
        _payrollLayout.PerformLayout();
        _payrollBar.ResumeLayout(false);
        _payrollBar.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_slipGrid).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private TableLayoutPanel _layout = null!;
    private Label _title = null!;
    private FlowLayoutPanel _toolbar = null!;
    private Label _monthLabel = null!;
    private DateTimePicker _month = null!;
    private ComboBox _employee = null!;
    private Label _status = null!;
    private Label _totals = null!;
    private TabControl _tabs = null!;
    private TabPage _attendancePage = null!;
    private TabPage _payrollPage = null!;
    private TableLayoutPanel _attendanceLayout = null!;
    private TableLayoutPanel _payrollLayout = null!;
    private FlowLayoutPanel _attendanceBar = null!;
    private FlowLayoutPanel _payrollBar = null!;
    private Label _attendanceNote = null!;
    private DataGridView _attendanceGrid = null!;
    private DataGridView _slipGrid = null!;
    private Button _refreshButton = null!;
    private Button _openPeriodButton = null!;
    private Button _salaryButton = null!;
    private Button _manualAttendanceButton = null!;
    private Button _driverAttendanceButton = null!;
    private Button _confirmAttendanceButton = null!;
    private Button _generateButton = null!;
    private Button _detailsButton = null!;
    private Button _addItemButton = null!;
    private Button _deleteItemButton = null!;
    private Button _deleteDraftButton = null!;
    private Button _approveButton = null!;
    private Button _adjustButton = null!;
    private Button _payButton = null!;
    private Button _exportButton = null!;
    private DataGridViewTextBoxColumn _attendanceIdColumn = null!;
    private DataGridViewTextBoxColumn _attendanceCodeColumn = null!;
    private DataGridViewTextBoxColumn _attendanceNameColumn = null!;
    private DataGridViewTextBoxColumn _attendanceGroupColumn = null!;
    private DataGridViewTextBoxColumn _attendanceStandardDaysColumn = null!;
    private DataGridViewTextBoxColumn _attendancePaidDaysColumn = null!;
    private DataGridViewTextBoxColumn _attendanceUnpaidDaysColumn = null!;
    private DataGridViewTextBoxColumn _attendanceOvertimeColumn = null!;
    private DataGridViewTextBoxColumn _attendanceStateColumn = null!;
    private DataGridViewTextBoxColumn _slipEmployeeCodeColumn = null!;
    private DataGridViewTextBoxColumn _slipEmployeeNameColumn = null!;
    private DataGridViewTextBoxColumn _slipEarnedSalaryColumn = null!;
    private DataGridViewTextBoxColumn _slipGrossIncomeColumn = null!;
    private DataGridViewTextBoxColumn _slipDeductionsColumn = null!;
    private DataGridViewTextBoxColumn _slipNetPayColumn = null!;
    private DataGridViewTextBoxColumn _slipPaidAmountColumn = null!;
    private DataGridViewTextBoxColumn _slipRemainingAmountColumn = null!;
    private DataGridViewTextBoxColumn _slipPaymentStatusColumn = null!;
}
