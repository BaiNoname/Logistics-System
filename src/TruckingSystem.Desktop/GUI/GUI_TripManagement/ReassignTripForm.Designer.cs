namespace TruckingSystem.Desktop.GUI_TripManagement
{
    partial class ReassignTripForm : TruckingSystem.Desktop.Common.BaseForm
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblInfo = new System.Windows.Forms.Label();
            this.lblVehicle = new System.Windows.Forms.Label();
            this.cboVehicle = new System.Windows.Forms.ComboBox();
            this.lblDriver = new System.Windows.Forms.Label();
            this.cboDriver = new System.Windows.Forms.ComboBox();
            this.lblHint = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // lblTitle
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(16, 12);
            this.lblTitle.Text = "🔁 Đổi xe / tài xế";
            this.lblTitle.Font = TruckingSystem.Desktop.Common.Theme.FontSectionTitle;
            this.lblTitle.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
            //
            // lblInfo
            //
            this.lblInfo.Location = new System.Drawing.Point(18, 46);
            this.lblInfo.Size = new System.Drawing.Size(680, 20);
            this.lblInfo.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblInfo.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            //
            // lblVehicle
            //
            this.lblVehicle.AutoSize = true;
            this.lblVehicle.Location = new System.Drawing.Point(18, 78);
            this.lblVehicle.Text = "Xe *";
            this.lblVehicle.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblVehicle.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            //
            // cboVehicle
            //
            this.cboVehicle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboVehicle.Location = new System.Drawing.Point(18, 98);
            this.cboVehicle.Size = new System.Drawing.Size(680, 25);
            this.cboVehicle.Font = TruckingSystem.Desktop.Common.Theme.FontNormal;
            //
            // lblDriver
            //
            this.lblDriver.AutoSize = true;
            this.lblDriver.Location = new System.Drawing.Point(18, 136);
            this.lblDriver.Text = "Tài xế *";
            this.lblDriver.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblDriver.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            //
            // cboDriver
            //
            this.cboDriver.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDriver.Location = new System.Drawing.Point(18, 156);
            this.cboDriver.Size = new System.Drawing.Size(680, 25);
            this.cboDriver.Font = TruckingSystem.Desktop.Common.Theme.FontNormal;
            //
            // lblHint
            //
            this.lblHint.Location = new System.Drawing.Point(18, 194);
            this.lblHint.Size = new System.Drawing.Size(680, 40);
            this.lblHint.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblHint.ForeColor = TruckingSystem.Desktop.Common.Theme.Warning;
            //
            // lblStatus
            //
            this.lblStatus.Location = new System.Drawing.Point(18, 244);
            this.lblStatus.Size = new System.Drawing.Size(450, 40);
            this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblStatus.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            //
            // btnSave
            //
            this.btnSave.Location = new System.Drawing.Point(478, 246);
            this.btnSave.Size = new System.Drawing.Size(120, 36);
            this.btnSave.Text = "💾 Đổi";
            this.btnSave.BackColor = TruckingSystem.Desktop.Common.Theme.Primary;
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatAppearance.MouseOverBackColor = TruckingSystem.Desktop.Common.Theme.PrimaryDark;
            this.btnSave.Enabled = false;
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            //
            // btnClose
            //
            this.btnClose.Location = new System.Drawing.Point(608, 246);
            this.btnClose.Size = new System.Drawing.Size(90, 36);
            this.btnClose.Text = "Đóng";
            this.btnClose.BackColor = System.Drawing.Color.White;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = TruckingSystem.Desktop.Common.Theme.FontNormal;
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            //
            // ReassignTripForm
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(716, 298);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.lblVehicle);
            this.Controls.Add(this.cboVehicle);
            this.Controls.Add(this.lblDriver);
            this.Controls.Add(this.cboDriver);
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ReassignTripForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Đổi xe / tài xế";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Label lblVehicle;
        private System.Windows.Forms.ComboBox cboVehicle;
        private System.Windows.Forms.Label lblDriver;
        private System.Windows.Forms.ComboBox cboDriver;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnClose;
    }
}
