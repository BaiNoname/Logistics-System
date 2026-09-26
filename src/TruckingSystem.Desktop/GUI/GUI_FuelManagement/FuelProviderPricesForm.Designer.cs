namespace TruckingSystem.Desktop.GUI_FuelManagement
{
    partial class FuelProviderPricesForm
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
            this.lblFuelType = new System.Windows.Forms.Label();
            this.txtFuelType = new System.Windows.Forms.TextBox();
            this.lblPrice = new System.Windows.Forms.Label();
            this.numPrice = new System.Windows.Forms.NumericUpDown();
            this.lblEffectiveDate = new System.Windows.Forms.Label();
            this.dtEffectiveDate = new System.Windows.Forms.DateTimePicker();
            this.btnAddPrice = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.grid = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.numPrice)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            //
            // lblTitle
            //
            this.lblTitle.Location = new System.Drawing.Point(20, 16);
            this.lblTitle.AutoSize = true;
            this.lblTitle.Text = "⛽ Lịch sử giá xăng dầu";
            this.lblTitle.Font = TruckingSystem.Desktop.Common.Theme.FontSectionTitle;
            this.lblTitle.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
            //
            // lblFuelType
            //
            this.lblFuelType.Location = new System.Drawing.Point(20, 56);
            this.lblFuelType.AutoSize = true;
            this.lblFuelType.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblFuelType.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblFuelType.Text = "Loại nhiên liệu";
            //
            // txtFuelType
            //
            this.txtFuelType.Location = new System.Drawing.Point(20, 76);
            this.txtFuelType.Size = new System.Drawing.Size(200, 23);
            this.txtFuelType.PlaceholderText = "VD: Dầu DO, Xăng RON95";
            //
            // lblPrice
            //
            this.lblPrice.Location = new System.Drawing.Point(240, 56);
            this.lblPrice.AutoSize = true;
            this.lblPrice.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblPrice.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblPrice.Text = "Giá (đ)";
            //
            // numPrice
            //
            this.numPrice.Location = new System.Drawing.Point(240, 76);
            this.numPrice.Size = new System.Drawing.Size(150, 23);
            this.numPrice.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            this.numPrice.ThousandsSeparator = true;
            //
            // lblEffectiveDate
            //
            this.lblEffectiveDate.Location = new System.Drawing.Point(410, 56);
            this.lblEffectiveDate.AutoSize = true;
            this.lblEffectiveDate.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblEffectiveDate.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblEffectiveDate.Text = "Ngày hiệu lực";
            //
            // dtEffectiveDate
            //
            this.dtEffectiveDate.Location = new System.Drawing.Point(410, 76);
            this.dtEffectiveDate.Size = new System.Drawing.Size(150, 23);
            this.dtEffectiveDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            //
            // btnAddPrice
            //
            this.btnAddPrice.Location = new System.Drawing.Point(590, 75);
            this.btnAddPrice.Size = new System.Drawing.Size(140, 30);
            this.btnAddPrice.Text = "➕ Thêm giá";
            this.btnAddPrice.BackColor = TruckingSystem.Desktop.Common.Theme.Primary;
            this.btnAddPrice.ForeColor = System.Drawing.Color.White;
            this.btnAddPrice.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddPrice.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.btnAddPrice.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddPrice.FlatAppearance.BorderSize = 0;
            this.btnAddPrice.FlatAppearance.MouseOverBackColor = TruckingSystem.Desktop.Common.Theme.PrimaryDark;
            this.btnAddPrice.Click += new System.EventHandler(this.BtnAddPrice_Click);
            //
            // lblStatus
            //
            this.lblStatus.Location = new System.Drawing.Point(20, 115);
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblStatus.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            //
            // grid
            //
            this.grid.Location = new System.Drawing.Point(20, 145);
            this.grid.Size = new System.Drawing.Size(760, 380);
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            //
            // FuelProviderPricesForm
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(800, 545);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblFuelType);
            this.Controls.Add(this.txtFuelType);
            this.Controls.Add(this.lblPrice);
            this.Controls.Add(this.numPrice);
            this.Controls.Add(this.lblEffectiveDate);
            this.Controls.Add(this.dtEffectiveDate);
            this.Controls.Add(this.btnAddPrice);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.grid);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "FuelProviderPricesForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Giá xăng dầu";
            ((System.ComponentModel.ISupportInitialize)(this.numPrice)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblFuelType;
        private System.Windows.Forms.TextBox txtFuelType;
        private System.Windows.Forms.Label lblPrice;
        private System.Windows.Forms.NumericUpDown numPrice;
        private System.Windows.Forms.Label lblEffectiveDate;
        private System.Windows.Forms.DateTimePicker dtEffectiveDate;
        private System.Windows.Forms.Button btnAddPrice;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView grid;
    }
}