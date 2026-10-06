namespace TruckingSystem.Desktop.GUI_DriverManagement
{
    partial class PickEmployeeForDriverForm
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
            this.lblInfo = new System.Windows.Forms.Label();
            this.cboEmployee = new System.Windows.Forms.ComboBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // lblInfo
            //
            this.lblInfo.Location = new System.Drawing.Point(24, 20);
            this.lblInfo.Size = new System.Drawing.Size(360, 18);
            this.lblInfo.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblInfo.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblInfo.Text = "Chọn nhân viên sẽ trở thành tài xế:";
            //
            // cboEmployee
            //
            this.cboEmployee.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboEmployee.Location = new System.Drawing.Point(24, 42);
            this.cboEmployee.Size = new System.Drawing.Size(360, 23);
            //
            // lblStatus
            //
            this.lblStatus.Location = new System.Drawing.Point(24, 80);
            this.lblStatus.Size = new System.Drawing.Size(360, 25);
            this.lblStatus.ForeColor = TruckingSystem.Desktop.Common.Theme.Danger;
            this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            //
            // btnOk
            //
            this.btnOk.Location = new System.Drawing.Point(104, 120);
            this.btnOk.Size = new System.Drawing.Size(140, 40);
            this.btnOk.Text = "Tiếp tục";
            this.btnOk.BackColor = TruckingSystem.Desktop.Common.Theme.Primary;
            this.btnOk.ForeColor = System.Drawing.Color.White;
            this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOk.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.btnOk.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOk.FlatAppearance.BorderSize = 0;
            this.btnOk.FlatAppearance.MouseOverBackColor = TruckingSystem.Desktop.Common.Theme.PrimaryDark;
            this.btnOk.Click += new System.EventHandler(this.BtnOk_Click);
            //
            // btnCancel
            //
            this.btnCancel.Location = new System.Drawing.Point(250, 120);
            this.btnCancel.Size = new System.Drawing.Size(134, 40);
            this.btnCancel.Text = "Hủy";
            this.btnCancel.BackColor = System.Drawing.Color.White;
            this.btnCancel.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            //
            // PickEmployeeForDriverForm
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(410, 185);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.cboEmployee);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "PickEmployeeForDriverForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Thêm tài xế mới";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.ComboBox cboEmployee;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
}