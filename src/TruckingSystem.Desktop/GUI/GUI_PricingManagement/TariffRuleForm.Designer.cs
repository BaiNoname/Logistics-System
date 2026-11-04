using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI.GUI_PricingManagement;

partial class TariffRuleForm
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
        _fields = new TableLayoutPanel();
        _status = new Label();
        _errors = new ErrorProvider(components);
        _save = new Button();
        _cancel = new Button();
        _footer = new FlowLayoutPanel();
        _priceListLabel = new Label();
        _vehicleType = new ComboBox();
        _vehicleTypePanel = new Panel();
        _vehicleTypeLabel = new Label();
        _perKm = new NumberBox();
        _perKmPanel = new Panel();
        _perKmLabel = new Label();
        _perTon = new NumberBox();
        _perTonPanel = new Panel();
        _perTonLabel = new Label();
        ((System.ComponentModel.ISupportInitialize)_errors).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_perKm).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_perTon).BeginInit();
        _fields.SuspendLayout();
        _footer.SuspendLayout();
        _vehicleTypePanel.SuspendLayout();
        _perKmPanel.SuspendLayout();
        _perTonPanel.SuspendLayout();
        SuspendLayout();
        _fields.Name = "_fields";
        _fields.Dock = DockStyle.Fill;
        _fields.AutoScroll = true;
        _fields.ColumnCount = 1;
        _fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _fields.Padding = new Padding(24);
        _status.Name = "_status";
        _status.AutoSize = true;
        _status.MaximumSize = new Size(480, 0);
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
        ClientSize = new Size(540, 395);
        BackColor = Theme.CardBg;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Thêm chi tiết giá";
        AcceptButton = _save;
        CancelButton = _cancel;
        _priceListLabel.Name = "_priceListLabel";
        _priceListLabel.Text = "Bảng giá:";
        _priceListLabel.AutoSize = true;
        _priceListLabel.MaximumSize = new Size(470, 0);
        _priceListLabel.ForeColor = Theme.TextPrimary;
        _priceListLabel.Font = Theme.FontBold;
        _priceListLabel.Margin = new Padding(0, 0, 0, 16);
        _fields.Controls.Add(_priceListLabel);
        _vehicleType.Name = "_vehicleType";
        _vehicleType.DropDownStyle = ComboBoxStyle.DropDown;
        _vehicleType.AccessibleName = "Loại xe";
        _vehicleType.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        _vehicleType.AutoCompleteSource = AutoCompleteSource.ListItems;
        _vehicleType.Items.AddRange(new object[]
        {
            "Xe van 1 tấn",
            "Xe tải 1,5 tấn",
            "Xe tải 2,5 tấn",
            "Xe tải 5 tấn",
            "Xe tải 8 tấn",
            "Xe tải 15 tấn",
            "Đầu kéo container 20 feet",
            "Đầu kéo container 40 feet"
        });
        _vehicleTypePanel.Name = "_vehicleTypePanel";
        _vehicleTypePanel.Size = new Size(470, 56);
        _vehicleTypePanel.Margin = new Padding(0, 0, 0, 8);
        _vehicleTypeLabel.Name = "_vehicleTypeLabel";
        _vehicleTypeLabel.Text = "Loại xe * (chọn hoặc nhập)";
        _vehicleTypeLabel.AutoSize = true;
        _vehicleTypeLabel.Font = Theme.FontSmall;
        _vehicleTypeLabel.ForeColor = Theme.TextSecondary;
        _vehicleType.Location = new Point(0, 20);
        _vehicleType.Width = 470;
        _vehicleType.TabIndex = 1;
        _vehicleTypePanel.Controls.Add(_vehicleTypeLabel);
        _vehicleTypePanel.Controls.Add(_vehicleType);
        _fields.Controls.Add(_vehicleTypePanel);
        _perKm.Name = "_perKm";
        _perKm.Minimum = 0;
        _perKm.DecimalPlaces = 2;
        _perKm.ThousandsSeparator = true;
        _perKm.Increment = 1000;
        _perKm.AccessibleName = "Giá mỗi km";
        _perKmPanel.Name = "_perKmPanel";
        _perKmPanel.Size = new Size(470, 56);
        _perKmPanel.Margin = new Padding(0, 0, 0, 8);
        _perKmLabel.Name = "_perKmLabel";
        _perKmLabel.Text = "Giá/km (đ)";
        _perKmLabel.AutoSize = true;
        _perKmLabel.Font = Theme.FontSmall;
        _perKmLabel.ForeColor = Theme.TextSecondary;
        _perKm.Location = new Point(0, 20);
        _perKm.Width = 470;
        _perKm.TabIndex = 1;
        _perKmPanel.Controls.Add(_perKmLabel);
        _perKmPanel.Controls.Add(_perKm);
        _fields.Controls.Add(_perKmPanel);
        _perTon.Name = "_perTon";
        _perTon.Minimum = 0;
        _perTon.DecimalPlaces = 2;
        _perTon.ThousandsSeparator = true;
        _perTon.Increment = 1000;
        _perTon.AccessibleName = "Giá mỗi tấn";
        _perTonPanel.Name = "_perTonPanel";
        _perTonPanel.Size = new Size(470, 56);
        _perTonPanel.Margin = new Padding(0, 0, 0, 8);
        _perTonLabel.Name = "_perTonLabel";
        _perTonLabel.Text = "Giá/tấn (đ)";
        _perTonLabel.AutoSize = true;
        _perTonLabel.Font = Theme.FontSmall;
        _perTonLabel.ForeColor = Theme.TextSecondary;
        _perTon.Location = new Point(0, 20);
        _perTon.Width = 470;
        _perTon.TabIndex = 1;
        _perTonPanel.Controls.Add(_perTonLabel);
        _perTonPanel.Controls.Add(_perTon);
        _fields.Controls.Add(_perTonPanel);
        _fields.Controls.Add(_status);
        Controls.Add(_fields);
        Controls.Add(_footer);
        Name = "TariffRuleForm";
        ((System.ComponentModel.ISupportInitialize)_errors).EndInit();
        ((System.ComponentModel.ISupportInitialize)_perKm).EndInit();
        ((System.ComponentModel.ISupportInitialize)_perTon).EndInit();
        _fields.ResumeLayout(false);
        _fields.PerformLayout();
        _footer.ResumeLayout(false);
        _footer.PerformLayout();
        _vehicleTypePanel.ResumeLayout(false);
        _vehicleTypePanel.PerformLayout();
        _perKmPanel.ResumeLayout(false);
        _perKmPanel.PerformLayout();
        _perTonPanel.ResumeLayout(false);
        _perTonPanel.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private TableLayoutPanel _fields = null!;
    private Label _status = null!;
    private ErrorProvider _errors = null!;
    private Button _save = null!;
    private Button _cancel = null!;
    private FlowLayoutPanel _footer = null!;
    private Label _priceListLabel = null!;
    private ComboBox _vehicleType = null!;
    private Panel _vehicleTypePanel = null!;
    private Label _vehicleTypeLabel = null!;
    private NumberBox _perKm = null!;
    private Panel _perKmPanel = null!;
    private Label _perKmLabel = null!;
    private NumberBox _perTon = null!;
    private Panel _perTonPanel = null!;
    private Label _perTonLabel = null!;
}
