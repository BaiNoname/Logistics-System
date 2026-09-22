namespace TruckingSystem.Desktop.GUI_FuelManagement
{
    partial class CreateFuelProviderForm
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
            this.lblAddress = new System.Windows.Forms.Label();
            this.txtAddress = new System.Windows.Forms.TextBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // lblName
            //
            this.lblName.Location = new System.Drawing.Point(24, 20);
            this.lblName.Size = new System.Drawing.Size(360, 18);
            this.lblName.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblName.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblName.Text = "Tên hãng xăng * (VD: Petrolimex)";
            //
            // txtName
            //
            this.txtName.Location = new System.Drawing.Point(24, 40);
            this.txtName.Size = new System.Drawing.Size(360, 23);
            //
            // lblPhone
            //
            this.lblPhone.Location = new System.Drawing.Point(24, 90);
            this.lblPhone.Size = new System.Drawing.Size(360, 18);
            this.lblPhone.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblPhone.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblPhone.Text = "Số điện thoại liên hệ (tuỳ chọn)";
            //
            // txtPhone
            //
            this.txtPhone.Location = new System.Drawing.Point(24, 110);
            this.txtPhone.Size = new System.Drawing.Size(360, 23);
            //
            // lblAddress
            //
            this.lblAddress.Location = new System.Drawing.Point(24, 160);
            this.lblAddress.Size = new System.Drawing.Size(360, 18);
            this.lblAddress.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblAddress.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblAddress.Text = "Địa chỉ (tuỳ chọn)";
            //
            // txtAddress
            //
            this.txtAddress.Location = new System.Drawing.Point(24, 180);
            this.txtAddress.Size = new System.Drawing.Size(360, 23);
            //
            // btnSave
            //
            this.btnSave.Location = new System.Drawing.Point(100, 250);
            this.btnSave.Size = new System.Drawing.Size(200, 40);
            this.btnSave.Text = "Lưu hãng xăng";
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
            this.lblStatus.Location = new System.Drawing.Point(24, 215);
            this.lblStatus.Size = new System.Drawing.Size(360, 25);
            this.lblStatus.ForeColor = TruckingSystem.Desktop.Common.Theme.Danger;
            this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            //
            // CreateFuelProviderForm
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(410, 320);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.lblPhone);
            this.Controls.Add(this.txtPhone);
            this.Controls.Add(this.lblAddress);
            this.Controls.Add(this.txtAddress);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnSave);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "CreateFuelProviderForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Thêm hãng xăng mới";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblPhone;
        private System.Windows.Forms.TextBox txtPhone;
        private System.Windows.Forms.Label lblAddress;
        private System.Windows.Forms.TextBox txtAddress;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label lblStatus;
    }
}
