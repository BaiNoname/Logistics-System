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
    private Button btnSave = null!;
    private Button btnCancel = null!;
    private TableLayoutPanel fields = null!;

    private void InitializeComponent()
    {
        SuspendLayout();
        Text = "Lập hóa đơn";
        ClientSize = new Size(600, 460);
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
        fields = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, RowCount = 7 };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var row = 0; row < 7; row++) fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        cboOrder = new ComboBox
        {
            Name = "cboOrder", Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList,
            DisplayMember = nameof(InvoiceOrderOption.DisplayName), AccessibleName = "Đơn hàng đã hoàn thành",
            TabIndex = 0
        };
        lblRoute = new Label { AutoSize = true, Dock = DockStyle.Fill, ForeColor = Theme.TextSecondary, Margin = new Padding(3, 6, 3, 12) };
        numAmount = new NumericUpDown
        {
            Name = "numAmount", Dock = DockStyle.Top, DecimalPlaces = 2, ThousandsSeparator = true,
            Maximum = Domain.Entities.Invoice.MaximumAmount, AccessibleName = "Cước phí (VNĐ)", TabIndex = 1
        };
        dtDueDate = new DateTimePicker
        {
            Name = "dtDueDate", Dock = DockStyle.Top, ShowCheckBox = true,
            Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy",
            Value = DateTime.UtcNow.AddHours(7).Date, Checked = false,
            AccessibleName = "Hạn thanh toán (tùy chọn)", TabIndex = 2
        };
        fields.Controls.Add(new Label { Text = "Đơn hàng đã hoàn thành *", AutoSize = true, Margin = new Padding(3, 8, 3, 4) }, 0, 0);
        fields.Controls.Add(cboOrder, 0, 1);
        fields.Controls.Add(lblRoute, 0, 2);
        fields.Controls.Add(new Label { Text = "Cước phí (VNĐ) *", AutoSize = true, Margin = new Padding(3, 8, 3, 4) }, 0, 3);
        fields.Controls.Add(numAmount, 0, 4);
        fields.Controls.Add(new Label { Text = "Hạn thanh toán (tích chọn để nhập)", AutoSize = true, Margin = new Padding(3, 16, 3, 4) }, 0, 5);
        fields.Controls.Add(dtDueDate, 0, 6);
        scroll.Controls.Add(fields);
        lblStatus = new Label { Name = "lblStatus", AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(3, 12, 3, 12) };
        var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        btnSave = Theme.MakePrimaryButton("Lưu hóa đơn");
        btnSave.Name = "btnSave";
        btnSave.Enabled = false;
        btnSave.Click += BtnSave_Click;
        btnCancel = Theme.MakeSecondaryButton("Hủy");
        btnCancel.DialogResult = DialogResult.Cancel;
        buttons.Controls.Add(btnSave);
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
