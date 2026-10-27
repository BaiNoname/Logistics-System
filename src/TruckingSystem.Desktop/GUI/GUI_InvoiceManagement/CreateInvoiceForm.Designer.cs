using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_InvoiceManagement;

namespace TruckingSystem.Desktop.GUI_InvoiceManagement;

partial class CreateInvoiceForm
{
    private ComboBox cboOrder = null!;
    private NumericUpDown numAmount = null!;
    private DateTimePicker dtDueDate = null!;
    private Label lblRoute = null!;
    private Label lblStatus = null!;
    private Label lblPricing = null!;
    private Label lblOperatingCosts = null!;
    private DataGridView gridQuote = null!;
    private Button btnRecalculate = null!;
    private Button btnSave = null!;
    private Button btnCancel = null!;
    private TableLayoutPanel fields = null!;

    private void InitializeComponent()
    {
        SuspendLayout();
        Text = "Lập hóa đơn";
        ClientSize = new Size(850, 760);
        MinimumSize = new Size(460, 400);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Theme.CardBg;
        MinimizeBox = false;
        MaximizeBox = false;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, Padding = new Padding(20), ColumnCount = 1, RowCount = 3
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        fields = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, RowCount = 10 };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var row = 0; row < 10; row++) fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        cboOrder = new ComboBox
        {
            Name = "cboOrder", Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList,
            DisplayMember = nameof(InvoiceOrderOption.DisplayName), AccessibleName = "Đơn hàng đã hoàn thành",
            TabIndex = 0
        };
        lblRoute = new Label { AutoSize = true, Dock = DockStyle.Fill, ForeColor = Theme.TextSecondary, Margin = new Padding(3, 6, 3, 12) };
        lblPricing = new Label { Name = "lblPricing", AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(3, 6, 3, 12) };
        lblOperatingCosts = new Label { Name = "lblOperatingCosts", AutoSize = true, Dock = DockStyle.Fill, ForeColor = Theme.TextSecondary, Margin = new Padding(3, 10, 3, 12) };
        gridQuote = new DataGridView
        {
            Name = "gridQuote", Dock = DockStyle.Top, Height = 160, ReadOnly = true,
            AutoGenerateColumns = false, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
            AccessibleName = "Chi tiết tiền hóa đơn", TabIndex = 1
        };
        gridQuote.Columns.AddRange(
            new DataGridViewTextBoxColumn { DataPropertyName = "Description", HeaderText = "Khoản tính", FillWeight = 240 },
            new DataGridViewTextBoxColumn { DataPropertyName = "Quantity", HeaderText = "Số lượng", DefaultCellStyle = new() { Format = "0.######" } },
            new DataGridViewTextBoxColumn { DataPropertyName = "UnitPrice", HeaderText = "Đơn giá (đ)", DefaultCellStyle = new() { Format = "N2" } },
            new DataGridViewTextBoxColumn { DataPropertyName = "Amount", HeaderText = "Thành tiền (đ)", DefaultCellStyle = new() { Format = "N2" } });
        numAmount = new NumericUpDown
        {
            Name = "numAmount", Dock = DockStyle.Top, DecimalPlaces = 2, ThousandsSeparator = true,
            Maximum = Domain.Entities.Invoice.MaximumAmount, AccessibleName = "Tổng tiền tự động (VNĐ)", TabIndex = 2,
            ReadOnly = true, Increment = 0
        };
        dtDueDate = new DateTimePicker
        {
            Name = "dtDueDate", Dock = DockStyle.Top, ShowCheckBox = true,
            Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy",
            Value = DateTime.UtcNow.AddHours(7).Date, Checked = false,
            AccessibleName = "Hạn thanh toán (tùy chọn)", TabIndex = 3
        };
        fields.Controls.Add(new Label { Text = "Đơn hàng đã hoàn thành *", AutoSize = true, Margin = new Padding(3, 8, 3, 4) }, 0, 0);
        fields.Controls.Add(cboOrder, 0, 1);
        fields.Controls.Add(lblRoute, 0, 2);
        fields.Controls.Add(lblPricing, 0, 3);
        fields.Controls.Add(gridQuote, 0, 4);
        fields.Controls.Add(lblOperatingCosts, 0, 5);
        fields.Controls.Add(new Label { Text = "Tổng tiền theo hợp đồng/phụ lục (VNĐ)", AutoSize = true, Margin = new Padding(3, 8, 3, 4) }, 0, 6);
        fields.Controls.Add(numAmount, 0, 7);
        fields.Controls.Add(new Label { Text = "Hạn thanh toán (tích chọn để nhập)", AutoSize = true, Margin = new Padding(3, 16, 3, 4) }, 0, 8);
        fields.Controls.Add(dtDueDate, 0, 9);
        scroll.Controls.Add(fields);
        lblStatus = new Label { Name = "lblStatus", AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(3, 12, 3, 12) };
        var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        btnSave = Theme.MakePrimaryButton("Lưu hóa đơn");
        btnSave.Name = "btnSave";
        btnSave.Enabled = false;
        btnSave.Click += BtnSave_Click;
        btnCancel = Theme.MakeSecondaryButton("Hủy");
        btnCancel.DialogResult = DialogResult.Cancel;
        btnRecalculate = Theme.MakeSecondaryButton("Tính lại");
        btnRecalculate.Name = "btnRecalculate";
        buttons.Controls.Add(btnSave);
        buttons.Controls.Add(btnRecalculate);
        buttons.Controls.Add(btnCancel);
        root.Controls.Add(scroll, 0, 0);
        root.Controls.Add(lblStatus, 0, 1);
        root.Controls.Add(buttons, 0, 2);
        Controls.Add(root);
        AcceptButton = btnSave;
        CancelButton = btnCancel;
        ResumeLayout(true);
    }
}
