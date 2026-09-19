namespace TruckingSystem.Desktop.GUI_CustomerManagement;

partial class CreateCustomerForm
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
        this.lblName = new System.Windows.Forms.Label();
        this.txtName = new System.Windows.Forms.TextBox();
        this.lblPhone = new System.Windows.Forms.Label();
        this.txtPhone = new System.Windows.Forms.TextBox();
        this.lblTaxCode = new System.Windows.Forms.Label();
        this.txtTaxCode = new System.Windows.Forms.TextBox();
        this.lblType = new System.Windows.Forms.Label();
        this.cboType = new System.Windows.Forms.ComboBox();
        this.btnSave = new System.Windows.Forms.Button();
        this.lblStatus = new System.Windows.Forms.Label();
        this.SuspendLayout();
        //
        // lblName
        //
        this.lblName.Location = new System.Drawing.Point(24, 20);
        this.lblName.Name = "lblName";
        this.lblName.Size = new System.Drawing.Size(400, 18);
        this.lblName.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblName.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblName.Text = "Tên khách hàng *";
        //
        // txtName
        //
        this.txtName.Location = new System.Drawing.Point(24, 40);
        this.txtName.Name = "txtName";
        this.txtName.Size = new System.Drawing.Size(400, 23);
        //
        // lblPhone
        //
        this.lblPhone.Location = new System.Drawing.Point(24, 90);
        this.lblPhone.Name = "lblPhone";
        this.lblPhone.Size = new System.Drawing.Size(400, 18);
        this.lblPhone.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblPhone.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblPhone.Text = "Số điện thoại";
        //
        // txtPhone
        //
        this.txtPhone.Location = new System.Drawing.Point(24, 110);
        this.txtPhone.Name = "txtPhone";
        this.txtPhone.Size = new System.Drawing.Size(400, 23);
        //
        // lblTaxCode
        //
        this.lblTaxCode.Location = new System.Drawing.Point(24, 160);
        this.lblTaxCode.Name = "lblTaxCode";
        this.lblTaxCode.Size = new System.Drawing.Size(400, 18);
        this.lblTaxCode.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblTaxCode.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblTaxCode.Text = "Mã số thuế (nếu có)";
        //
        // txtTaxCode
        //
        this.txtTaxCode.Location = new System.Drawing.Point(24, 180);
        this.txtTaxCode.Name = "txtTaxCode";
        this.txtTaxCode.Size = new System.Drawing.Size(400, 23);
        //
        // lblType
        //
        this.lblType.Location = new System.Drawing.Point(24, 230);
        this.lblType.Name = "lblType";
        this.lblType.Size = new System.Drawing.Size(400, 18);
        this.lblType.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblType.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblType.Text = "Loại khách hàng";
        //
        // cboType
        //
        this.cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboType.Location = new System.Drawing.Point(24, 250);
        this.cboType.Name = "cboType";
        this.cboType.Size = new System.Drawing.Size(400, 23);
        //
        // btnSave
        //
        this.btnSave.Location = new System.Drawing.Point(244, 310);
        this.btnSave.Name = "btnSave";
        this.btnSave.Size = new System.Drawing.Size(180, 38);
        this.btnSave.Text = "Lưu khách hàng";
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
        this.lblStatus.Location = new System.Drawing.Point(24, 300);
        this.lblStatus.Name = "lblStatus";
        this.lblStatus.AutoSize = true;
        this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblStatus.ForeColor = TruckingSystem.Desktop.Common.Theme.Danger;
        //
        // CreateCustomerForm
        //
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.BackColor = System.Drawing.Color.White;
        this.ClientSize = new System.Drawing.Size(460, 400);
        this.Controls.Add(this.lblName);
        this.Controls.Add(this.txtName);
        this.Controls.Add(this.lblPhone);
        this.Controls.Add(this.txtPhone);
        this.Controls.Add(this.lblTaxCode);
        this.Controls.Add(this.txtTaxCode);
        this.Controls.Add(this.lblType);
        this.Controls.Add(this.cboType);
        this.Controls.Add(this.btnSave);
        this.Controls.Add(this.lblStatus);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.Name = "CreateCustomerForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Thêm khách hàng mới";
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label lblName;
    private System.Windows.Forms.TextBox txtName;
    private System.Windows.Forms.Label lblPhone;
    private System.Windows.Forms.TextBox txtPhone;
    private System.Windows.Forms.Label lblTaxCode;
    private System.Windows.Forms.TextBox txtTaxCode;
    private System.Windows.Forms.Label lblType;
    private System.Windows.Forms.ComboBox cboType;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.Label lblStatus;
}
