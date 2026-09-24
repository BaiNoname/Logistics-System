namespace TruckingSystem.Desktop.GUI_TripManagement
{
    partial class CreateIncidentForm
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
            this.lblType = new System.Windows.Forms.Label();
            this.cboType = new System.Windows.Forms.ComboBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.lblCost = new System.Windows.Forms.Label();
            this.numCost = new System.Windows.Forms.NumericUpDown();
            this.btnSave = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numCost)).BeginInit();
            this.SuspendLayout();
            //
            // lblType
            //
            this.lblType.Location = new System.Drawing.Point(24, 20);
            this.lblType.Size = new System.Drawing.Size(360, 18);
            this.lblType.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblType.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblType.Text = "Loại sự cố *";
            //
            // cboType
            //
            this.cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboType.Location = new System.Drawing.Point(24, 40);
            this.cboType.Size = new System.Drawing.Size(360, 23);
            //
            // lblDescription
            //
            this.lblDescription.Location = new System.Drawing.Point(24, 90);
            this.lblDescription.Size = new System.Drawing.Size(360, 18);
            this.lblDescription.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblDescription.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblDescription.Text = "Mô tả chi tiết (tuỳ chọn)";
            //
            // txtDescription
            //
            this.txtDescription.Location = new System.Drawing.Point(24, 110);
            this.txtDescription.Size = new System.Drawing.Size(360, 60);
            this.txtDescription.Multiline = true;
            //
            // lblCost
            //
            this.lblCost.Location = new System.Drawing.Point(24, 185);
            this.lblCost.Size = new System.Drawing.Size(360, 18);
            this.lblCost.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblCost.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblCost.Text = "Chi phí phát sinh (đ, tuỳ chọn)";
            //
            // numCost
            //
            this.numCost.Location = new System.Drawing.Point(24, 205);
            this.numCost.Size = new System.Drawing.Size(200, 23);
            this.numCost.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            //
            // btnSave
            //
            this.btnSave.Location = new System.Drawing.Point(100, 275);
            this.btnSave.Size = new System.Drawing.Size(200, 40);
            this.btnSave.Text = "Lưu sự cố";
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
            this.lblStatus.Location = new System.Drawing.Point(24, 240);
            this.lblStatus.Size = new System.Drawing.Size(360, 25);
            this.lblStatus.ForeColor = TruckingSystem.Desktop.Common.Theme.Danger;
            this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            //
            // CreateIncidentForm
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(410, 335);
            this.Controls.Add(this.lblType);
            this.Controls.Add(this.cboType);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.lblCost);
            this.Controls.Add(this.numCost);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnSave);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "CreateIncidentForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Ghi nhận sự cố";
            ((System.ComponentModel.ISupportInitialize)(this.numCost)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.ComboBox cboType;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Label lblCost;
        private System.Windows.Forms.NumericUpDown numCost;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label lblStatus;
    }
}
