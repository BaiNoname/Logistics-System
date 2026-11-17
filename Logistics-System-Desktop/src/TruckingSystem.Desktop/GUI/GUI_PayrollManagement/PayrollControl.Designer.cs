using TruckingSystem.Desktop.Common;
namespace TruckingSystem.Desktop.GUI_PayrollManagement;

partial class PayrollControl
{
    private DateTimePicker _month = null!;
    private FilterBar _filters = null!;
    private Label _status = null!, _totals = null!;
    private DataGridView _slipGrid = null!;
    private Button _refreshButton = null!, _calculationErrorButton = null!, _approveButton = null!, _payButton = null!,
        _exportButton = null!, _deleteDraftButton = null!, _kpiButton = null!;

    private void InitializeComponent()
    {
        Dock = DockStyle.Fill;
        BackColor = Theme.ContentBg;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, Padding = new Padding(12) };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var title = new Label { Text = "Bảng lương tháng", Font = Theme.FontTitle, AutoSize = true };
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
        _month = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "MM/yyyy", ShowUpDown = true, Width = 110, AccessibleName = "Tháng lương" };
        _refreshButton = Theme.MakeSecondaryButton("Tải lại", 110);
        _calculationErrorButton = Theme.MakeSecondaryButton("Xem lỗi tính lương", 165);
        _calculationErrorButton.Visible = false;
        _approveButton = Theme.MakePrimaryButton("Chốt lương", 130);
        _kpiButton = Theme.MakeSecondaryButton("Cấu hình KPI cho tài xế", 225);
        toolbar.Controls.AddRange(new Control[] { _month, _refreshButton, _kpiButton, _approveButton, _calculationErrorButton });
        _status = new Label { AutoSize = true, Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 8) };
        _filters = new FilterBar { Dock = DockStyle.Fill, SearchPlaceholder = "Mã, tên nhân viên..." };
        _slipGrid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false };
        _slipGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "EmployeeId", DataPropertyName = "EmployeeId", Visible = false });
        foreach (var (name, label) in new[] { ("EmployeeCode", "Mã nhân viên"), ("EmployeeName", "Nhân viên"), ("MonthlySalary", "Lương cơ bản"),
            ("WorkedDays", "Ngày công"), ("KpiPoints", "Điểm KPI"), ("KpiTarget", "Điểm chuẩn"), ("KpiRatio", "Tỷ lệ KPI"), ("EarnedSalary", "Lương tính"),
            ("GrossIncome", "Tổng thu nhập"), ("Deductions", "Khấu trừ"), ("NetPay", "Thực lĩnh"), ("PaidAmount", "Đã trả"), ("PaymentStatus", "Chi trả") })
            _slipGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = name, DataPropertyName = name, HeaderText = label,
                DefaultCellStyle = new DataGridViewCellStyle { Format = name is "WorkedDays" or "KpiPoints" or "KpiTarget" ? "0.######" : "N0" } });
        var footer = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        _totals = ButtonStyles.NextStep();
        _totals.Dock = DockStyle.None;
        _exportButton = Theme.MakeSecondaryButton("Xuất phiếu", 125);
        _payButton = Theme.MakePrimaryButton("Ghi nhận chi trả", 160);
        _deleteDraftButton = Theme.MakeSecondaryButton("Xóa nháp cũ", 140);
        footer.Controls.AddRange(new Control[] { _totals, _exportButton, _payButton, _deleteDraftButton });
        layout.Controls.Add(title, 0, 0); layout.Controls.Add(toolbar, 0, 1); layout.Controls.Add(_filters, 0, 2);
        layout.Controls.Add(_status, 0, 3); layout.Controls.Add(_slipGrid, 0, 4); layout.Controls.Add(footer, 0, 5); Controls.Add(layout);
    }
}
