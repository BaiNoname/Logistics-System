using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_InvoiceManagement;

partial class InvoiceDetailForm
{
    private Label lblTitle = null!;
    private Label lblCustomer = null!;
    private Label lblInvoiceInfo = null!;
    private Label lblOrderInfo = null!;
    private Label lblPaymentSummary = null!;
    private Label lblNote = null!;
    private Label lblTotal = null!;
    private DataGridView gridLines = null!;
    private DataGridView gridPayments = null!;

    private void InitializeComponent()
    {
        SuspendLayout();
        Text = "Chi tiết hóa đơn";
        ClientSize = new Size(1060, 700);
        MinimumSize = new Size(660, 500);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Theme.CardBg;
        MinimizeBox = false;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, Padding = new Padding(20), ColumnCount = 1, RowCount = 6
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        lblTitle = new Label { AutoSize = true, Dock = DockStyle.Fill, Font = Theme.FontSectionTitle };
        lblInvoiceInfo = new Label { AutoSize = true, Dock = DockStyle.Fill, Font = Theme.FontBold, Margin = new Padding(3, 10, 3, 12) };
        lblCustomer = new Label { AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(3, 0, 12, 12), UseMnemonic = false };
        lblOrderInfo = new Label { AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(12, 0, 3, 12), UseMnemonic = false };
        var information = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2, RowCount = 1 };
        information.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        information.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        information.Controls.Add(lblCustomer, 0, 0);
        information.Controls.Add(lblOrderInfo, 1, 0);
        lblNote = new Label { AutoSize = true, Dock = DockStyle.Fill, ForeColor = Theme.TextSecondary, Margin = new Padding(3, 0, 3, 12) };
        gridLines = new DataGridView
        {
            Name = "gridLines", Dock = DockStyle.Fill, AutoGenerateColumns = false,
            ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
            AccessibleName = "Các khoản phí tạo thành hóa đơn", TabIndex = 0
        };
        gridLines.Columns.AddRange(
            new DataGridViewTextBoxColumn { DataPropertyName = "Description", HeaderText = "Khoản phí", FillWeight = 240, MinimumWidth = 180, DefaultCellStyle = new() { WrapMode = DataGridViewTriState.True } },
            new DataGridViewTextBoxColumn { DataPropertyName = "Quantity", HeaderText = "Số lượng", MinimumWidth = 90, DefaultCellStyle = new() { Format = "0.######", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { DataPropertyName = "UnitPrice", HeaderText = "Đơn giá (đ)", MinimumWidth = 110, DefaultCellStyle = new() { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } },
            new DataGridViewTextBoxColumn { DataPropertyName = "Amount", HeaderText = "Thành tiền (đ)", MinimumWidth = 120, DefaultCellStyle = new() { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } });
        gridPayments = new DataGridView
        {
            Name = "gridPayments", Dock = DockStyle.Fill, AutoGenerateColumns = false,
            ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
            AccessibleName = "Lịch sử thu tiền của hóa đơn", TabIndex = 0
        };
        gridPayments.Columns.AddRange(
            new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "Mã giao dịch", MinimumWidth = 90 },
            new DataGridViewTextBoxColumn { DataPropertyName = "PaymentDate", HeaderText = "Ngày thu tiền", MinimumWidth = 150, DefaultCellStyle = new() { Format = "dd/MM/yyyy HH:mm" } },
            new DataGridViewTextBoxColumn { DataPropertyName = "Method", HeaderText = "Phương thức", MinimumWidth = 130 },
            new DataGridViewTextBoxColumn { DataPropertyName = "Amount", HeaderText = "Số tiền (đ)", MinimumWidth = 120, DefaultCellStyle = new() { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } });
        lblPaymentSummary = new Label { AutoSize = true, Dock = DockStyle.Bottom, Padding = new Padding(8), ForeColor = Theme.TextSecondary };
        var tabs = new TabControl { Dock = DockStyle.Fill, MinimumSize = new Size(0, 200) };
        var fees = new TabPage("Chi tiết khoản phí") { Padding = new Padding(4) };
        fees.Controls.Add(gridLines);
        var payments = new TabPage("Lịch sử thu tiền") { Padding = new Padding(4) };
        payments.Controls.Add(gridPayments);
        payments.Controls.Add(lblPaymentSummary);
        tabs.TabPages.Add(fees);
        tabs.TabPages.Add(payments);
        lblTotal = new Label { AutoSize = true, Dock = DockStyle.Fill, Font = Theme.FontBold, TextAlign = ContentAlignment.MiddleRight, Margin = new Padding(3, 12, 3, 12) };
        var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        var close = Theme.MakeSecondaryButton("Đóng");
        close.DialogResult = DialogResult.Cancel;
        buttons.Controls.Add(close);
        root.Controls.Add(lblTitle, 0, 0);
        root.Controls.Add(lblInvoiceInfo, 0, 1);
        root.Controls.Add(information, 0, 2);
        root.Controls.Add(tabs, 0, 3);
        root.Controls.Add(lblNote, 0, 4);
        root.Controls.Add(lblTotal, 0, 5);
        Controls.Add(root);
        ButtonStyles.Footer(this, null, close);
        buttons.Dispose();
        DialogFit.Enable(this, root);
        DialogKeys.Enable(this);
        Anim.FadeIn(this);
        CancelButton = close;
        ResumeLayout(true);
    }
}
