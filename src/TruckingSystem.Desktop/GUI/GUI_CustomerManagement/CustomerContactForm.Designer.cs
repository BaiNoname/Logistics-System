namespace TruckingSystem.Desktop.GUI_CustomerManagement;

partial class CustomerContactForm
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
        _fields = new FlowLayoutPanel();
        customerPanel = new Panel();
        lblCustomer = new Label();
        _customer = new ComboBox();
        namePanel = new Panel();
        lblName = new Label();
        _name = new TextBox();
        phonePanel = new Panel();
        lblPhone = new Label();
        _phone = new TextBox();
        positionPanel = new Panel();
        lblPosition = new Label();
        _position = new TextBox();
        _status = new Label();
        footer = new FlowLayoutPanel();
        _save = new Button();
        btnCancel = new Button();
        _fields.SuspendLayout();
        customerPanel.SuspendLayout();
        namePanel.SuspendLayout();
        phonePanel.SuspendLayout();
        positionPanel.SuspendLayout();
        footer.SuspendLayout();
        SuspendLayout();
        //
        // _fields
        //
        _fields.AutoScroll = true;
        _fields.Controls.Add(customerPanel);
        _fields.Controls.Add(namePanel);
        _fields.Controls.Add(phonePanel);
        _fields.Controls.Add(positionPanel);
        _fields.Controls.Add(_status);
        _fields.Dock = DockStyle.Fill;
        _fields.FlowDirection = FlowDirection.TopDown;
        _fields.Location = new Point(0, 0);
        _fields.Name = "_fields";
        _fields.Padding = new Padding(24);
        _fields.Size = new Size(500, 342);
        _fields.TabIndex = 0;
        _fields.WrapContents = false;
        //
        // customerPanel
        //
        customerPanel.Controls.Add(lblCustomer);
        customerPanel.Controls.Add(_customer);
        customerPanel.Location = new Point(24, 24);
        customerPanel.Margin = new Padding(0, 0, 0, 8);
        customerPanel.Name = "customerPanel";
        customerPanel.Size = new Size(440, 56);
        customerPanel.TabIndex = 0;
        //
        // lblCustomer
        //
        lblCustomer.AutoSize = true;
        lblCustomer.Font = new Font("Segoe UI", 9F);
        lblCustomer.ForeColor = Color.FromArgb(100, 116, 139);
        lblCustomer.Location = new Point(0, 0);
        lblCustomer.Name = "lblCustomer";
        lblCustomer.TabIndex = 0;
        lblCustomer.Text = "Khách hàng *";
        //
        // _customer
        //
        _customer.AccessibleName = "Khách hàng của người liên hệ";
        _customer.DisplayMember = "Name";
        _customer.DropDownStyle = ComboBoxStyle.DropDownList;
        _customer.FormattingEnabled = true;
        _customer.Location = new Point(0, 20);
        _customer.Name = "_customer";
        _customer.Size = new Size(440, 25);
        _customer.TabIndex = 1;
        _customer.ValueMember = "Id";
        //
        // namePanel
        //
        namePanel.Controls.Add(lblName);
        namePanel.Controls.Add(_name);
        namePanel.Location = new Point(24, 88);
        namePanel.Margin = new Padding(0, 0, 0, 8);
        namePanel.Name = "namePanel";
        namePanel.Size = new Size(440, 56);
        namePanel.TabIndex = 1;
        //
        // lblName
        //
        lblName.AutoSize = true;
        lblName.Font = new Font("Segoe UI", 9F);
        lblName.ForeColor = Color.FromArgb(100, 116, 139);
        lblName.Location = new Point(0, 0);
        lblName.Name = "lblName";
        lblName.TabIndex = 0;
        lblName.Text = "Tên người liên hệ *";
        //
        // _name
        //
        _name.AccessibleName = "Tên người liên hệ";
        _name.Location = new Point(0, 20);
        _name.Name = "_name";
        _name.Size = new Size(440, 25);
        _name.TabIndex = 1;
        //
        // phonePanel
        //
        phonePanel.Controls.Add(lblPhone);
        phonePanel.Controls.Add(_phone);
        phonePanel.Location = new Point(24, 152);
        phonePanel.Margin = new Padding(0, 0, 0, 8);
        phonePanel.Name = "phonePanel";
        phonePanel.Size = new Size(440, 56);
        phonePanel.TabIndex = 2;
        //
        // lblPhone
        //
        lblPhone.AutoSize = true;
        lblPhone.Font = new Font("Segoe UI", 9F);
        lblPhone.ForeColor = Color.FromArgb(100, 116, 139);
        lblPhone.Location = new Point(0, 0);
        lblPhone.Name = "lblPhone";
        lblPhone.TabIndex = 0;
        lblPhone.Text = "Số điện thoại";
        //
        // _phone
        //
        _phone.AccessibleName = "Số điện thoại";
        _phone.Location = new Point(0, 20);
        _phone.Name = "_phone";
        _phone.Size = new Size(440, 25);
        _phone.TabIndex = 1;
        //
        // positionPanel
        //
        positionPanel.Controls.Add(lblPosition);
        positionPanel.Controls.Add(_position);
        positionPanel.Location = new Point(24, 216);
        positionPanel.Margin = new Padding(0, 0, 0, 8);
        positionPanel.Name = "positionPanel";
        positionPanel.Size = new Size(440, 56);
        positionPanel.TabIndex = 3;
        //
        // lblPosition
        //
        lblPosition.AutoSize = true;
        lblPosition.Font = new Font("Segoe UI", 9F);
        lblPosition.ForeColor = Color.FromArgb(100, 116, 139);
        lblPosition.Location = new Point(0, 0);
        lblPosition.Name = "lblPosition";
        lblPosition.TabIndex = 0;
        lblPosition.Text = "Chức vụ";
        //
        // _position
        //
        _position.AccessibleName = "Chức vụ";
        _position.Location = new Point(0, 20);
        _position.Name = "_position";
        _position.Size = new Size(440, 25);
        _position.TabIndex = 1;
        //
        // _status
        //
        _status.AutoSize = true;
        _status.ForeColor = Color.FromArgb(220, 38, 38);
        _status.Location = new Point(27, 280);
        _status.MaximumSize = new Size(440, 0);
        _status.Name = "_status";
        _status.TabIndex = 4;
        //
        // footer
        //
        footer.Controls.Add(_save);
        footer.Controls.Add(btnCancel);
        footer.Dock = DockStyle.Bottom;
        footer.FlowDirection = FlowDirection.RightToLeft;
        footer.Location = new Point(0, 342);
        footer.Name = "footer";
        footer.Padding = new Padding(24, 10, 24, 10);
        footer.Size = new Size(500, 68);
        footer.TabIndex = 1;
        //
        // _save
        //
        _save.BackColor = Color.FromArgb(37, 99, 235);
        _save.Cursor = Cursors.Hand;
        _save.FlatAppearance.BorderSize = 0;
        _save.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
        _save.FlatStyle = FlatStyle.Flat;
        _save.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _save.ForeColor = Color.White;
        _save.Location = new Point(349, 13);
        _save.Name = "_save";
        _save.Size = new Size(100, 38);
        _save.TabIndex = 0;
        _save.Text = "Lưu";
        _save.UseVisualStyleBackColor = false;
        _save.Click += BtnSave_Click;
        //
        // btnCancel
        //
        btnCancel.BackColor = Color.White;
        btnCancel.Cursor = Cursors.Hand;
        btnCancel.DialogResult = DialogResult.Cancel;
        btnCancel.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
        btnCancel.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
        btnCancel.FlatStyle = FlatStyle.Flat;
        btnCancel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnCancel.ForeColor = Color.FromArgb(15, 23, 42);
        btnCancel.Location = new Point(243, 13);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(100, 38);
        btnCancel.TabIndex = 1;
        btnCancel.Text = "Hủy";
        btnCancel.UseVisualStyleBackColor = false;
        //
        // CustomerContactForm
        //
        AcceptButton = _save;
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.White;
        CancelButton = btnCancel;
        ClientSize = new Size(500, 410);
        Controls.Add(_fields);
        Controls.Add(footer);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "CustomerContactForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Thêm người liên hệ khách hàng";
        FormClosing += CustomerContactForm_FormClosing;
        _fields.ResumeLayout(false);
        _fields.PerformLayout();
        customerPanel.ResumeLayout(false);
        customerPanel.PerformLayout();
        namePanel.ResumeLayout(false);
        namePanel.PerformLayout();
        phonePanel.ResumeLayout(false);
        phonePanel.PerformLayout();
        positionPanel.ResumeLayout(false);
        positionPanel.PerformLayout();
        footer.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private FlowLayoutPanel _fields = null!;
    private Panel customerPanel = null!;
    private Label lblCustomer = null!;
    private ComboBox _customer = null!;
    private Panel namePanel = null!;
    private Label lblName = null!;
    private TextBox _name = null!;
    private Panel phonePanel = null!;
    private Label lblPhone = null!;
    private TextBox _phone = null!;
    private Panel positionPanel = null!;
    private Label lblPosition = null!;
    private TextBox _position = null!;
    private Label _status = null!;
    private FlowLayoutPanel footer = null!;
    private Button _save = null!;
    private Button btnCancel = null!;
}
