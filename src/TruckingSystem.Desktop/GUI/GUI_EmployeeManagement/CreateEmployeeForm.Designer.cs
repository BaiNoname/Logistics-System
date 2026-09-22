namespace TruckingSystem.Desktop.GUI_EmployeeManagement
{
    partial class CreateEmployeeForm
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
            this.lblCode = new System.Windows.Forms.Label();
            this.txtCode = new System.Windows.Forms.TextBox();
            this.lblFullName = new System.Windows.Forms.Label();
            this.txtFullName = new System.Windows.Forms.TextBox();
            this.lblPhone = new System.Windows.Forms.Label();
            this.txtPhone = new System.Windows.Forms.TextBox();
            this.lblDept = new System.Windows.Forms.Label();
            this.cboDept = new System.Windows.Forms.ComboBox();
            this.lblPos = new System.Windows.Forms.Label();
            this.cboPos = new System.Windows.Forms.ComboBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // lblCode
            //
            this.lblCode.Location = new System.Drawing.Point(24, 20);
            this.lblCode.Size = new System.Drawing.Size(400, 18);
            this.lblCode.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblCode.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblCode.Text = "Mã nhân viên *";
            //
            // txtCode
            //
            this.txtCode.Location = new System.Drawing.Point(24, 40);
            this.txtCode.Size = new System.Drawing.Size(400, 23);
            //
            // lblFullName
            //
            this.lblFullName.Location = new System.Drawing.Point(24, 90);
            this.lblFullName.Size = new System.Drawing.Size(400, 18);
            this.lblFullName.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblFullName.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblFullName.Text = "Họ tên *";
            //
            // txtFullName
            //
            this.txtFullName.Location = new System.Drawing.Point(24, 110);
            this.txtFullName.Size = new System.Drawing.Size(400, 23);
            //
            // lblPhone
            //
            this.lblPhone.Location = new System.Drawing.Point(24, 160);
            this.lblPhone.Size = new System.Drawing.Size(400, 18);
            this.lblPhone.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblPhone.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblPhone.Text = "Số điện thoại";
            //
            // txtPhone
            //
            this.txtPhone.Location = new System.Drawing.Point(24, 180);
            this.txtPhone.Size = new System.Drawing.Size(400, 23);
            //
            // lblDept
            //
            this.lblDept.Location = new System.Drawing.Point(24, 230);
            this.lblDept.Size = new System.Drawing.Size(400, 18);
            this.lblDept.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblDept.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblDept.Text = "Phòng ban";
            //
            // cboDept
            //
            this.cboDept.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDept.Location = new System.Drawing.Point(24, 250);
            this.cboDept.Size = new System.Drawing.Size(400, 23);
            //
            // lblPos
            //
            this.lblPos.Location = new System.Drawing.Point(24, 300);
            this.lblPos.Size = new System.Drawing.Size(400, 18);
            this.lblPos.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblPos.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblPos.Text = "Chức danh";
            //
            // cboPos
            //
            this.cboPos.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPos.Location = new System.Drawing.Point(24, 320);
            this.cboPos.Size = new System.Drawing.Size(400, 23);
            //
            // btnSave
            //
            this.btnSave.Location = new System.Drawing.Point(244, 380);
            this.btnSave.Size = new System.Drawing.Size(180, 38);
            this.btnSave.Text = "Lưu nhân viên";
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
            this.lblStatus.Location = new System.Drawing.Point(24, 370);
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblStatus.ForeColor = TruckingSystem.Desktop.Common.Theme.Danger;
            //
            // CreateEmployeeForm
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(460, 470);
            this.Controls.Add(this.lblCode);
            this.Controls.Add(this.txtCode);
            this.Controls.Add(this.lblFullName);
            this.Controls.Add(this.txtFullName);
            this.Controls.Add(this.lblPhone);
            this.Controls.Add(this.txtPhone);
            this.Controls.Add(this.lblDept);
            this.Controls.Add(this.cboDept);
            this.Controls.Add(this.lblPos);
            this.Controls.Add(this.cboPos);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.lblStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "CreateEmployeeForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Thêm nhân viên mới";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblCode;
        private System.Windows.Forms.TextBox txtCode;
        private System.Windows.Forms.Label lblFullName;
        private System.Windows.Forms.TextBox txtFullName;
        private System.Windows.Forms.Label lblPhone;
        private System.Windows.Forms.TextBox txtPhone;
        private System.Windows.Forms.Label lblDept;
        private System.Windows.Forms.ComboBox cboDept;
        private System.Windows.Forms.Label lblPos;
        private System.Windows.Forms.ComboBox cboPos;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label lblStatus;
    }
}
