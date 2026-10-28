using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_InvoiceManagement;

partial class InvoiceDetailForm
{
    private Label lblTitle = null!;
    private Label lblCustomer = null!;
    private Label lblNote = null!;
    private Label lblTotal = null!;
    private DataGridView gridLines = null!;

    private void InitializeComponent()
    {
        SuspendLayout();
        Text = "Chi tiết hóa đơn";
        ClientSize = new Size(900, 500);
        MinimumSize = new Size(520, 360);
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
        lblCustomer = new Label { AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(3, 8, 3, 8) };
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
        lblTotal = new Label { AutoSize = true, Dock = DockStyle.Fill, Font = Theme.FontBold, TextAlign = ContentAlignment.MiddleRight, Margin = new Padding(3, 12, 3, 12) };
        var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        var close = Theme.MakeSecondaryButton("Đóng");
        close.DialogResult = DialogResult.Cancel;
        buttons.Controls.Add(close);
        root.Controls.Add(lblTitle, 0, 0);
        root.Controls.Add(lblCustomer, 0, 1);
        root.Controls.Add(lblNote, 0, 2);
        root.Controls.Add(gridLines, 0, 3);
        root.Controls.Add(lblTotal, 0, 4);
        root.Controls.Add(buttons, 0, 5);
        Controls.Add(root);
        CancelButton = close;
        ResumeLayout(true);
    }
}
