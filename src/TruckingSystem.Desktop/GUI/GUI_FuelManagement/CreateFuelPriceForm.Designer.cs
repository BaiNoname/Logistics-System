namespace TruckingSystem.Desktop.GUI_FuelManagement
{
    partial class CreateFuelPriceForm
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
            this.lblFuelType = new System.Windows.Forms.Label();
            this.cboFuelType = new System.Windows.Forms.ComboBox();
            this.lblPrice = new System.Windows.Forms.Label();
            this.numPrice = new System.Windows.Forms.NumericUpDown();
            this.lblEffectiveDate = new System.Windows.Forms.Label();
            this.dtEffectiveDate = new System.Windows.Forms.DateTimePicker();
            this.lblProvider = new System.Windows.Forms.Label();
            this.cboProvider = new System.Windows.Forms.ComboBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numPrice)).BeginInit();
            this.SuspendLayout();
            //
            // lblFuelType
            //
            this.lblFuelType.Location = new System.Drawing.Point(24, 20);
            this.lblFuelType.Size = new System.Drawing.Size(360, 18);
            this.lblFuelType.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblFuelType.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblFuelType.Text = "Loại nhiên liệu *";
            //
            // cboFuelType
            //
            this.cboFuelType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFuelType.Location = new System.Drawing.Point(24, 40);
            this.cboFuelType.Size = new System.Drawing.Size(360, 23);
            //
            // lblPrice
            //
            this.lblPrice.Location = new System.Drawing.Point(24, 90);
            this.lblPrice.Size = new System.Drawing.Size(170, 18);
            this.lblPrice.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblPrice.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblPrice.Text = "Giá (VNĐ/lít) *";
            //
            // numPrice
            //
            this.numPrice.Location = new System.Drawing.Point(24, 110);
            this.numPrice.Size = new System.Drawing.Size(170, 23);
            this.numPrice.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            this.numPrice.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            this.numPrice.Value = new decimal(new int[] { 20000, 0, 0, 0 });
            //
            // lblEffectiveDate
            //
            this.lblEffectiveDate.Location = new System.Drawing.Point(214, 90);
            this.lblEffectiveDate.Size = new System.Drawing.Size(170, 18);
            this.lblEffectiveDate.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblEffectiveDate.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblEffectiveDate.Text = "Ngày hiệu lực *";
            //
            // dtEffectiveDate
            //
            this.dtEffectiveDate.Location = new System.Drawing.Point(214, 110);
            this.dtEffectiveDate.Size = new System.Drawing.Size(170, 23);
            this.dtEffectiveDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            //
            // lblProvider
            //
            this.lblProvider.Location = new System.Drawing.Point(24, 160);
            this.lblProvider.Size = new System.Drawing.Size(360, 18);
            this.lblProvider.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblProvider.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblProvider.Text = "Áp dụng cho hãng xăng nào";
            //
            // cboProvider
            //
            this.cboProvider.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboProvider.Location = new System.Drawing.Point(24, 180);
            this.cboProvider.Size = new System.Drawing.Size(360, 23);
            //
            // btnSave
            //
            this.btnSave.Location = new System.Drawing.Point(100, 260);
            this.btnSave.Size = new System.Drawing.Size(200, 40);
            this.btnSave.Text = "Lưu giá xăng";
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
            this.lblStatus.Location = new System.Drawing.Point(24, 225);
            this.lblStatus.Size = new System.Drawing.Size(360, 25);
            this.lblStatus.ForeColor = TruckingSystem.Desktop.Common.Theme.Danger;
            this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            //
            // CreateFuelPriceForm
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(410, 330);
            this.Controls.Add(this.lblFuelType);
            this.Controls.Add(this.cboFuelType);
            this.Controls.Add(this.lblPrice);
            this.Controls.Add(this.numPrice);
            this.Controls.Add(this.lblEffectiveDate);
            this.Controls.Add(this.dtEffectiveDate);
            this.Controls.Add(this.lblProvider);
            this.Controls.Add(this.cboProvider);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnSave);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "CreateFuelPriceForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Thêm giá xăng mới";
            ((System.ComponentModel.ISupportInitialize)(this.numPrice)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblFuelType;
        private System.Windows.Forms.ComboBox cboFuelType;
        private System.Windows.Forms.Label lblPrice;
        private System.Windows.Forms.NumericUpDown numPrice;
        private System.Windows.Forms.Label lblEffectiveDate;
        private System.Windows.Forms.DateTimePicker dtEffectiveDate;
        private System.Windows.Forms.Label lblProvider;
        private System.Windows.Forms.ComboBox cboProvider;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label lblStatus;
    }
}
