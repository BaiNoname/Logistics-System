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
        this.lblLicenseNumber = new System.Windows.Forms.Label();
        this.txtLicenseNumber = new System.Windows.Forms.TextBox();
        this.lblLicenseClass = new System.Windows.Forms.Label();
        this.cboLicenseClass = new System.Windows.Forms.ComboBox();
        this.lblLicenseExpiry = new System.Windows.Forms.Label();
        this.dtLicenseExpiry = new System.Windows.Forms.DateTimePicker();
        this.chkHasHealthCert = new System.Windows.Forms.CheckBox();
        this.dtHealthCertExpiry = new System.Windows.Forms.DateTimePicker();
        this.lblYearsExp = new System.Windows.Forms.Label();
        this.numYearsExp = new System.Windows.Forms.NumericUpDown();
        this.btnSave = new System.Windows.Forms.Button();
        this.lblStatus = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)(this.numYearsExp)).BeginInit();
        this.SuspendLayout();
        //
        // lblLicenseNumber
        //
        this.lblLicenseNumber.Location = new System.Drawing.Point(24, 20);
        this.lblLicenseNumber.Size = new System.Drawing.Size(400, 18);
        this.lblLicenseNumber.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblLicenseNumber.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblLicenseNumber.Text = "Số bằng lái *";
        //
        // txtLicenseNumber
        //
        this.txtLicenseNumber.Location = new System.Drawing.Point(24, 40);
        this.txtLicenseNumber.Size = new System.Drawing.Size(400, 23);
        //
        // lblLicenseClass
        //
        this.lblLicenseClass.Location = new System.Drawing.Point(24, 90);
        this.lblLicenseClass.Size = new System.Drawing.Size(190, 18);
        this.lblLicenseClass.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblLicenseClass.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblLicenseClass.Text = "Hạng bằng";
        //
        // cboLicenseClass
        //
        this.cboLicenseClass.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboLicenseClass.Location = new System.Drawing.Point(24, 110);
        this.cboLicenseClass.Size = new System.Drawing.Size(190, 23);
        //
        // lblLicenseExpiry
        //
        this.lblLicenseExpiry.Location = new System.Drawing.Point(234, 90);
        this.lblLicenseExpiry.Size = new System.Drawing.Size(190, 18);
        this.lblLicenseExpiry.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblLicenseExpiry.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblLicenseExpiry.Text = "Ngày hết hạn bằng lái *";
        //
        // dtLicenseExpiry
        //
        this.dtLicenseExpiry.Location = new System.Drawing.Point(234, 110);
        this.dtLicenseExpiry.Size = new System.Drawing.Size(190, 23);
        this.dtLicenseExpiry.Format = System.Windows.Forms.DateTimePickerFormat.Short;
        //
        // chkHasHealthCert
        //
        this.chkHasHealthCert.Location = new System.Drawing.Point(24, 160);
        this.chkHasHealthCert.Size = new System.Drawing.Size(220, 24);
        this.chkHasHealthCert.Text = "Có giấy khám sức khỏe";
        //
        // dtHealthCertExpiry
        //
        this.dtHealthCertExpiry.Location = new System.Drawing.Point(24, 188);
        this.dtHealthCertExpiry.Size = new System.Drawing.Size(190, 23);
        this.dtHealthCertExpiry.Format = System.Windows.Forms.DateTimePickerFormat.Short;
        this.dtHealthCertExpiry.Enabled = false;
        //
        // lblYearsExp
        //
        this.lblYearsExp.Location = new System.Drawing.Point(234, 165);
        this.lblYearsExp.Size = new System.Drawing.Size(190, 18);
        this.lblYearsExp.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblYearsExp.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblYearsExp.Text = "Số năm kinh nghiệm";
        //
        // numYearsExp
        //
        this.numYearsExp.Location = new System.Drawing.Point(234, 188);
        this.numYearsExp.Size = new System.Drawing.Size(190, 23);
        this.numYearsExp.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
        //
        // btnSave
        //
        this.btnSave.Location = new System.Drawing.Point(140, 280);
        this.btnSave.Size = new System.Drawing.Size(180, 38);
        this.btnSave.Text = "Lưu hồ sơ tài xế";
        this.btnSave.BackColor = TruckingSystem.Desktop.Common.Theme.Primary;
        this.btnSave.ForeColor = System.Drawing.Color.White;
        this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnSave.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
        this.btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnSave.FlatAppearance.BorderSize = 0;
        this.btnSave.FlatAppearance.MouseOverBackColor = TruckingSystem.Desktop.Common.Theme.PrimaryDark;
        this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
        //
        // lblStatus
        //
        this.lblStatus.Location = new System.Drawing.Point(24, 245);
        this.lblStatus.Size = new System.Drawing.Size(400, 25);
        this.lblStatus.ForeColor = TruckingSystem.Desktop.Common.Theme.Danger;
        this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        //
        // CreateDriverProfileForm
        //
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.BackColor = System.Drawing.Color.White;
        this.ClientSize = new System.Drawing.Size(450, 340);
        this.Controls.Add(this.lblLicenseNumber);
        this.Controls.Add(this.txtLicenseNumber);
        this.Controls.Add(this.lblLicenseClass);
        this.Controls.Add(this.cboLicenseClass);
        this.Controls.Add(this.lblLicenseExpiry);
        this.Controls.Add(this.dtLicenseExpiry);
        this.Controls.Add(this.chkHasHealthCert);
        this.Controls.Add(this.dtHealthCertExpiry);
        this.Controls.Add(this.lblYearsExp);
        this.Controls.Add(this.numYearsExp);
        this.Controls.Add(this.lblStatus);
        this.Controls.Add(this.btnSave);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.Name = "CreateDriverProfileForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Tạo hồ sơ tài xế";
        ((System.ComponentModel.ISupportInitialize)(this.numYearsExp)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
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
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.Label lblStatus;
}