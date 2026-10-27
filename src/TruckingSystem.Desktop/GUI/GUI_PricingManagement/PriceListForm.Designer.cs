using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI.GUI_PricingManagement;

partial class PriceListForm
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
        components = new System.ComponentModel.Container();
        _fields = new FlowLayoutPanel();
        _status = new Label();
        _errors = new ErrorProvider(components);
        _save = new Button();
        _cancel = new Button();
        _footer = new FlowLayoutPanel();
        _name = new TextBox();
        _date = new DateTimePicker();
        _priceListStatus = new ComboBox();
        _copyInfo = new Label();
        _namePanel = new Panel();
        _nameLabel = new Label();
        _datePanel = new Panel();
        _dateLabel = new Label();
        _priceListStatusPanel = new Panel();
        _priceListStatusLabel = new Label();
        ((System.ComponentModel.ISupportInitialize)_errors).BeginInit();
        _fields.SuspendLayout();
        _footer.SuspendLayout();
        _namePanel.SuspendLayout();
        _datePanel.SuspendLayout();
        _priceListStatusPanel.SuspendLayout();
        SuspendLayout();
        _fields.Name = "_fields";
        _fields.Dock = DockStyle.Fill;
        _fields.AutoScroll = true;
        _fields.FlowDirection = FlowDirection.TopDown;
        _fields.WrapContents = false;
        _fields.Padding = new Padding(24);
        _status.Name = "_status";
        _status.AutoSize = true;
        _status.MaximumSize = new Size(440, 0);
        _status.ForeColor = Theme.Danger;
        _errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;
        _errors.ContainerControl = this;
        _save.Name = "_save";
        _save.Text = "Lưu";
        _save.Size = new Size(100, 38);
        _save.BackColor = Theme.Primary;
        _save.ForeColor = Color.White;
        _save.FlatStyle = FlatStyle.Flat;
        _save.Font = Theme.FontBold;
        _save.Cursor = Cursors.Hand;
        _save.FlatAppearance.BorderSize = 0;
        _save.FlatAppearance.MouseOverBackColor = Theme.PrimaryDark;
        _cancel.Name = "_cancel";
        _cancel.Text = "Hủy";
        _cancel.Size = new Size(100, 38);
        _cancel.BackColor = Color.White;
        _cancel.ForeColor = Theme.TextPrimary;
        _cancel.FlatStyle = FlatStyle.Flat;
        _cancel.Font = Theme.FontBold;
        _cancel.Cursor = Cursors.Hand;
        _cancel.FlatAppearance.BorderSize = 1;
        _cancel.FlatAppearance.MouseOverBackColor = Theme.GridAltRow;
        _cancel.FlatAppearance.BorderColor = Theme.BorderColor;
        _cancel.DialogResult = DialogResult.Cancel;
        _footer.Name = "_footer";
        _footer.Dock = DockStyle.Bottom;
        _footer.Height = 68;
        _footer.Padding = new Padding(24, 10, 24, 10);
        _footer.FlowDirection = FlowDirection.RightToLeft;
        _footer.Controls.Add(_save);
        _footer.Controls.Add(_cancel);
        ClientSize = new Size(500, 400);
        BackColor = Theme.CardBg;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Thêm bảng giá";
        AcceptButton = _save;
        CancelButton = _cancel;
        _name.Name = "_name";
        _name.AccessibleName = "Tên bảng giá";
        _date.Name = "_date";
        _date.Format = DateTimePickerFormat.Custom;
        _date.CustomFormat = "dd/MM/yyyy";
        _date.AccessibleName = "Ngày hiệu lực";
        _priceListStatus.Name = "_priceListStatus";
        _priceListStatus.DropDownStyle = ComboBoxStyle.DropDownList;
        _priceListStatus.AccessibleName = "Trạng thái bảng giá";
        _priceListStatus.Items.AddRange(new object[] { "Đang áp dụng", "Ngưng áp dụng" });
        _copyInfo.Name = "_copyInfo";
        _copyInfo.AutoSize = true;
        _copyInfo.MaximumSize = new Size(430, 0);
        _copyInfo.Margin = new Padding(0, 0, 0, 16);
        _copyInfo.Visible = false;
        _fields.Controls.Add(_copyInfo);
        _namePanel.Name = "_namePanel";
        _namePanel.Size = new Size(430, 56);
        _namePanel.Margin = new Padding(0, 0, 0, 8);
        _nameLabel.Name = "_nameLabel";
        _nameLabel.Text = "Tên bảng giá *";
        _nameLabel.AutoSize = true;
        _nameLabel.Font = Theme.FontSmall;
        _nameLabel.ForeColor = Theme.TextSecondary;
        _name.Location = new Point(0, 20);
        _name.Width = 430;
        _name.TabIndex = 1;
        _namePanel.Controls.Add(_nameLabel);
        _namePanel.Controls.Add(_name);
        _fields.Controls.Add(_namePanel);
        _datePanel.Name = "_datePanel";
        _datePanel.Size = new Size(430, 56);
        _datePanel.Margin = new Padding(0, 0, 0, 8);
        _dateLabel.Name = "_dateLabel";
        _dateLabel.Text = "Ngày hiệu lực *";
        _dateLabel.AutoSize = true;
        _dateLabel.Font = Theme.FontSmall;
        _dateLabel.ForeColor = Theme.TextSecondary;
        _date.Location = new Point(0, 20);
        _date.Width = 430;
        _date.TabIndex = 1;
        _datePanel.Controls.Add(_dateLabel);
        _datePanel.Controls.Add(_date);
        _fields.Controls.Add(_datePanel);
        _priceListStatusPanel.Name = "_priceListStatusPanel";
        _priceListStatusPanel.Size = new Size(430, 56);
        _priceListStatusPanel.Margin = new Padding(0, 0, 0, 8);
        _priceListStatusLabel.Name = "_priceListStatusLabel";
        _priceListStatusLabel.Text = "Trạng thái *";
        _priceListStatusLabel.AutoSize = true;
        _priceListStatusLabel.Font = Theme.FontSmall;
        _priceListStatusLabel.ForeColor = Theme.TextSecondary;
        _priceListStatus.Location = new Point(0, 20);
        _priceListStatus.Width = 430;
        _priceListStatus.TabIndex = 1;
        _priceListStatusPanel.Controls.Add(_priceListStatusLabel);
        _priceListStatusPanel.Controls.Add(_priceListStatus);
        _fields.Controls.Add(_priceListStatusPanel);
        _fields.Controls.Add(_status);
        Controls.Add(_fields);
        Controls.Add(_footer);
        Name = "PriceListForm";
        ((System.ComponentModel.ISupportInitialize)_errors).EndInit();
        _fields.ResumeLayout(false);
        _fields.PerformLayout();
        _footer.ResumeLayout(false);
        _footer.PerformLayout();
        _namePanel.ResumeLayout(false);
        _namePanel.PerformLayout();
        _datePanel.ResumeLayout(false);
        _datePanel.PerformLayout();
        _priceListStatusPanel.ResumeLayout(false);
        _priceListStatusPanel.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private FlowLayoutPanel _fields = null!;
    private Label _status = null!;
    private ErrorProvider _errors = null!;
    private Button _save = null!;
    private Button _cancel = null!;
    private FlowLayoutPanel _footer = null!;
    private TextBox _name = null!;
    private DateTimePicker _date = null!;
    private ComboBox _priceListStatus = null!;
    private Label _copyInfo = null!;
    private Panel _namePanel = null!;
    private Label _nameLabel = null!;
    private Panel _datePanel = null!;
    private Label _dateLabel = null!;
    private Panel _priceListStatusPanel = null!;
    private Label _priceListStatusLabel = null!;
}
