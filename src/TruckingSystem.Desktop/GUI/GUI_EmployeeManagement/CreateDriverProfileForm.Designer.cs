namespace TruckingSystem.Desktop.GUI_EmployeeManagement;

partial class CreateDriverProfileForm
{
    private System.ComponentModel.IContainer components = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        lblLicenseNumber = new Label();
        txtLicenseNumber = new TextBox();
        lblLicenseClass = new Label();
        cboLicenseClass = new ComboBox();
        lblLicenseExpiry = new Label();
        dtLicenseExpiry = new DateTimePicker();
        chkHasHealthCert = new CheckBox();
        dtHealthCertExpiry = new DateTimePicker();
        lblYearsExp = new Label();
        numYearsExp = new NumericUpDown();
        lblGoogleCalendarId = new Label();
        txtGoogleCalendarId = new TextBox();
        lblWarehouse = new Label();
        cboWarehouse = new ComboBox();
        btnSave = new Button();
        lblStatus = new Label();
        ((System.ComponentModel.ISupportInitialize)numYearsExp).BeginInit();
        SuspendLayout();
        // 
        // lblLicenseNumber
        // 
        lblLicenseNumber.Font = new Font("Segoe UI", 9F);
        lblLicenseNumber.ForeColor = Color.FromArgb(100, 116, 139);
        lblLicenseNumber.Location = new Point(36, 30);
        lblLicenseNumber.Margin = new Padding(4, 0, 4, 0);
        lblLicenseNumber.Name = "lblLicenseNumber";
        lblLicenseNumber.Size = new Size(600, 27);
        lblLicenseNumber.TabIndex = 0;
        lblLicenseNumber.Text = "Số bằng lái *";
        // 
        // txtLicenseNumber
        // 
        txtLicenseNumber.Location = new Point(36, 60);
        txtLicenseNumber.Margin = new Padding(4, 4, 4, 4);
        txtLicenseNumber.Name = "txtLicenseNumber";
        txtLicenseNumber.Size = new Size(598, 34);
        txtLicenseNumber.TabIndex = 1;
        // 
        // lblLicenseClass
        // 
        lblLicenseClass.Font = new Font("Segoe UI", 9F);
        lblLicenseClass.ForeColor = Color.FromArgb(100, 116, 139);
        lblLicenseClass.Location = new Point(36, 135);
        lblLicenseClass.Margin = new Padding(4, 0, 4, 0);
        lblLicenseClass.Name = "lblLicenseClass";
        lblLicenseClass.Size = new Size(285, 27);
        lblLicenseClass.TabIndex = 2;
        lblLicenseClass.Text = "Hạng bằng";
        // 
        // cboLicenseClass
        // 
        cboLicenseClass.DropDownStyle = ComboBoxStyle.DropDownList;
        cboLicenseClass.Location = new Point(36, 165);
        cboLicenseClass.Margin = new Padding(4, 4, 4, 4);
        cboLicenseClass.Name = "cboLicenseClass";
        cboLicenseClass.Size = new Size(283, 36);
        cboLicenseClass.TabIndex = 3;
        // 
        // lblLicenseExpiry
        // 
        lblLicenseExpiry.Font = new Font("Segoe UI", 9F);
        lblLicenseExpiry.ForeColor = Color.FromArgb(100, 116, 139);
        lblLicenseExpiry.Location = new Point(351, 135);
        lblLicenseExpiry.Margin = new Padding(4, 0, 4, 0);
        lblLicenseExpiry.Name = "lblLicenseExpiry";
        lblLicenseExpiry.Size = new Size(285, 27);
        lblLicenseExpiry.TabIndex = 4;
        lblLicenseExpiry.Text = "Ngày hết hạn bằng lái *";
        // 
        // dtLicenseExpiry
        // 
        dtLicenseExpiry.Format = DateTimePickerFormat.Short;
        dtLicenseExpiry.Location = new Point(351, 165);
        dtLicenseExpiry.Margin = new Padding(4, 4, 4, 4);
        dtLicenseExpiry.Name = "dtLicenseExpiry";
        dtLicenseExpiry.Size = new Size(283, 34);
        dtLicenseExpiry.TabIndex = 5;
        // 
        // chkHasHealthCert
        // 
        chkHasHealthCert.Location = new Point(36, 240);
        chkHasHealthCert.Margin = new Padding(4, 4, 4, 4);
        chkHasHealthCert.Name = "chkHasHealthCert";
        chkHasHealthCert.Size = new Size(285, 36);
        chkHasHealthCert.TabIndex = 6;
        chkHasHealthCert.Text = "Có giấy khám sức khỏe";
        // 
        // dtHealthCertExpiry
        // 
        dtHealthCertExpiry.Enabled = false;
        dtHealthCertExpiry.Format = DateTimePickerFormat.Short;
        dtHealthCertExpiry.Location = new Point(36, 282);
        dtHealthCertExpiry.Margin = new Padding(4, 4, 4, 4);
        dtHealthCertExpiry.Name = "dtHealthCertExpiry";
        dtHealthCertExpiry.Size = new Size(283, 34);
        dtHealthCertExpiry.TabIndex = 7;
        // 
        // lblYearsExp
        // 
        lblYearsExp.Font = new Font("Segoe UI", 9F);
        lblYearsExp.ForeColor = Color.FromArgb(100, 116, 139);
        lblYearsExp.Location = new Point(362, 248);
        lblYearsExp.Margin = new Padding(4, 0, 4, 0);
        lblYearsExp.Name = "lblYearsExp";
        lblYearsExp.Size = new Size(274, 27);
        lblYearsExp.TabIndex = 8;
        lblYearsExp.Text = "Số năm kinh nghiệm";
        // 
        // numYearsExp
        // 
        numYearsExp.Location = new Point(351, 282);
        numYearsExp.Margin = new Padding(4, 4, 4, 4);
        numYearsExp.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
        numYearsExp.Name = "numYearsExp";
        numYearsExp.Size = new Size(285, 34);
        numYearsExp.TabIndex = 9;
        // 
        // lblGoogleCalendarId
        // 
        lblGoogleCalendarId.Font = new Font("Segoe UI", 9F);
        lblGoogleCalendarId.ForeColor = Color.FromArgb(100, 116, 139);
        lblGoogleCalendarId.Location = new Point(36, 327);
        lblGoogleCalendarId.Margin = new Padding(4, 0, 4, 0);
        lblGoogleCalendarId.Name = "lblGoogleCalendarId";
        lblGoogleCalendarId.Size = new Size(600, 27);
        lblGoogleCalendarId.TabIndex = 10;
        lblGoogleCalendarId.Text = "Google Calendar ID (tuỳ chọn — để đồng bộ lịch chuyến)";
        // 
        // txtGoogleCalendarId
        // 
        txtGoogleCalendarId.Location = new Point(36, 357);
        txtGoogleCalendarId.Margin = new Padding(4, 4, 4, 4);
        txtGoogleCalendarId.Name = "txtGoogleCalendarId";
        txtGoogleCalendarId.Size = new Size(598, 34);
        txtGoogleCalendarId.TabIndex = 11;
        // 
        // lblWarehouse
        // 
        lblWarehouse.Font = new Font("Segoe UI", 9F);
        lblWarehouse.ForeColor = Color.FromArgb(100, 116, 139);
        lblWarehouse.Location = new Point(36, 412);
        lblWarehouse.Margin = new Padding(4, 0, 4, 0);
        lblWarehouse.Name = "lblWarehouse";
        lblWarehouse.Size = new Size(600, 27);
        lblWarehouse.TabIndex = 13;
        lblWarehouse.Text = "Kho đóng quân (tuỳ chọn)";
        // 
        // cboWarehouse
        // 
        cboWarehouse.DropDownStyle = ComboBoxStyle.DropDownList;
        cboWarehouse.Location = new Point(36, 442);
        cboWarehouse.Margin = new Padding(4, 4, 4, 4);
        cboWarehouse.Name = "cboWarehouse";
        cboWarehouse.Size = new Size(598, 36);
        cboWarehouse.TabIndex = 14;
        // 
        // btnSave
        // 
        btnSave.BackColor = Color.FromArgb(37, 99, 235);
        btnSave.Cursor = Cursors.Hand;
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
        btnSave.FlatStyle = FlatStyle.Flat;
        btnSave.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnSave.ForeColor = Color.White;
        btnSave.Location = new Point(210, 570);
        btnSave.Margin = new Padding(4, 4, 4, 4);
        btnSave.Name = "btnSave";
        btnSave.Size = new Size(270, 57);
        btnSave.TabIndex = 15;
        btnSave.Text = "Lưu hồ sơ tài xế";
        btnSave.UseVisualStyleBackColor = false;
        btnSave.Click += BtnSave_Click;
        // 
        // lblStatus
        // 
        lblStatus.Font = new Font("Segoe UI", 9F);
        lblStatus.ForeColor = Color.FromArgb(220, 38, 38);
        lblStatus.Location = new Point(36, 518);
        lblStatus.Margin = new Padding(4, 0, 4, 0);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(600, 38);
        lblStatus.TabIndex = 12;
        // 
        // CreateDriverProfileForm
        // 
        AutoScaleDimensions = new SizeF(144F, 144F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.White;
        ClientSize = new Size(675, 660);
        Controls.Add(lblLicenseNumber);
        Controls.Add(txtLicenseNumber);
        Controls.Add(lblLicenseClass);
        Controls.Add(cboLicenseClass);
        Controls.Add(lblLicenseExpiry);
        Controls.Add(dtLicenseExpiry);
        Controls.Add(chkHasHealthCert);
        Controls.Add(dtHealthCertExpiry);
        Controls.Add(lblYearsExp);
        Controls.Add(numYearsExp);
        Controls.Add(lblGoogleCalendarId);
        Controls.Add(txtGoogleCalendarId);
        Controls.Add(lblStatus);
        Controls.Add(lblWarehouse);
        Controls.Add(cboWarehouse);
        Controls.Add(btnSave);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        Margin = new Padding(6, 6, 6, 6);
        MaximizeBox = false;
        Name = "CreateDriverProfileForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Tạo hồ sơ tài xế";
        ((System.ComponentModel.ISupportInitialize)numYearsExp).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label lblLicenseNumber;
    private System.Windows.Forms.TextBox txtLicenseNumber;
    private System.Windows.Forms.Label lblLicenseClass;
    private System.Windows.Forms.ComboBox cboLicenseClass;
    private System.Windows.Forms.Label lblLicenseExpiry;
    private System.Windows.Forms.DateTimePicker dtLicenseExpiry;
    private System.Windows.Forms.CheckBox chkHasHealthCert;
    private System.Windows.Forms.DateTimePicker dtHealthCertExpiry;
    private System.Windows.Forms.Label lblYearsExp;
    private System.Windows.Forms.NumericUpDown numYearsExp;
    private System.Windows.Forms.Label lblGoogleCalendarId;
    private System.Windows.Forms.TextBox txtGoogleCalendarId;
    private System.Windows.Forms.Label lblWarehouse;
    private System.Windows.Forms.ComboBox cboWarehouse;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.Label lblStatus;
}