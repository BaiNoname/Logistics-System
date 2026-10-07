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
            lblTitle = new Label();
            lblFuelType = new Label();
            txtFuelType = new TextBox();
            lblPrice = new Label();
            numPrice = new NumericUpDown();
            lblEffectiveDate = new Label();
            dtEffectiveDate = new DateTimePicker();
            btnAddPrice = new Button();
            btnDeletePrice = new Button();
            lblStatus = new Label();
            grid = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)numPrice).BeginInit();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(30, 24);
            lblTitle.Margin = new Padding(4, 0, 4, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(306, 36);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "⛽ Lịch sử giá xăng dầu";
            // 
            // lblFuelType
            // 
            lblFuelType.AutoSize = true;
            lblFuelType.Font = new Font("Segoe UI", 9F);
            lblFuelType.ForeColor = Color.FromArgb(100, 116, 139);
            lblFuelType.Location = new Point(30, 84);
            lblFuelType.Margin = new Padding(4, 0, 4, 0);
            lblFuelType.Name = "lblFuelType";
            lblFuelType.Size = new Size(124, 25);
            lblFuelType.TabIndex = 1;
            lblFuelType.Text = "Loại nhiên liệu";
            // 
            // txtFuelType
            // 
            txtFuelType.Location = new Point(30, 114);
            txtFuelType.Margin = new Padding(4, 4, 4, 4);
            txtFuelType.Name = "txtFuelType";
            txtFuelType.PlaceholderText = "VD: Dầu DO, Xăng RON95";
            txtFuelType.Size = new Size(298, 34);
            txtFuelType.TabIndex = 2;
            // 
            // lblPrice
            // 
            lblPrice.AutoSize = true;
            lblPrice.Font = new Font("Segoe UI", 9F);
            lblPrice.ForeColor = Color.FromArgb(100, 116, 139);
            lblPrice.Location = new Point(360, 84);
            lblPrice.Margin = new Padding(4, 0, 4, 0);
            lblPrice.Name = "lblPrice";
            lblPrice.Size = new Size(63, 25);
            lblPrice.TabIndex = 3;
            lblPrice.Text = "Giá (đ)";
            // 
            // numPrice
            // 
            numPrice.Location = new Point(360, 114);
            numPrice.Margin = new Padding(4, 4, 4, 4);
            numPrice.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            numPrice.Name = "numPrice";
            numPrice.Size = new Size(225, 34);
            numPrice.TabIndex = 4;
            numPrice.ThousandsSeparator = true;
            // 
            // lblEffectiveDate
            // 
            lblEffectiveDate.AutoSize = true;
            lblEffectiveDate.Font = new Font("Segoe UI", 9F);
            lblEffectiveDate.ForeColor = Color.FromArgb(100, 116, 139);
            lblEffectiveDate.Location = new Point(615, 84);
            lblEffectiveDate.Margin = new Padding(4, 0, 4, 0);
            lblEffectiveDate.Name = "lblEffectiveDate";
            lblEffectiveDate.Size = new Size(120, 25);
            lblEffectiveDate.TabIndex = 5;
            lblEffectiveDate.Text = "Ngày hiệu lực";
            // 
            // dtEffectiveDate
            // 
            dtEffectiveDate.Format = DateTimePickerFormat.Short;
            dtEffectiveDate.Location = new Point(615, 114);
            dtEffectiveDate.Margin = new Padding(4, 4, 4, 4);
            dtEffectiveDate.Name = "dtEffectiveDate";
            dtEffectiveDate.Size = new Size(223, 34);
            dtEffectiveDate.TabIndex = 6;
            // 
            // btnAddPrice
            // 
            btnAddPrice.BackColor = Color.FromArgb(37, 99, 235);
            btnAddPrice.Cursor = Cursors.Hand;
            btnAddPrice.FlatAppearance.BorderSize = 0;
            btnAddPrice.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
            btnAddPrice.FlatStyle = FlatStyle.Flat;
            btnAddPrice.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnAddPrice.ForeColor = Color.White;
            btnAddPrice.Location = new Point(885, 112);
            btnAddPrice.Margin = new Padding(4, 4, 4, 4);
            btnAddPrice.Name = "btnAddPrice";
            btnAddPrice.Size = new Size(210, 45);
            btnAddPrice.TabIndex = 7;
            btnAddPrice.Text = "➕ Thêm giá";
            btnAddPrice.UseVisualStyleBackColor = false;
            btnAddPrice.Click += BtnAddPrice_Click;
            // 
            // btnDeletePrice
            // 
            btnDeletePrice.BackColor = Color.White;
            btnDeletePrice.Cursor = Cursors.Hand;
            btnDeletePrice.FlatAppearance.BorderColor = Color.FromArgb(220, 38, 38);
            btnDeletePrice.FlatStyle = FlatStyle.Flat;
            btnDeletePrice.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnDeletePrice.ForeColor = Color.FromArgb(220, 38, 38);
            btnDeletePrice.Location = new Point(1110, 112);
            btnDeletePrice.Margin = new Padding(4, 4, 4, 4);
            btnDeletePrice.Name = "btnDeletePrice";
            btnDeletePrice.Size = new Size(225, 45);
            btnDeletePrice.TabIndex = 8;
            btnDeletePrice.Text = "🗑️ Xóa giá đã chọn";
            btnDeletePrice.UseVisualStyleBackColor = false;
            btnDeletePrice.Click += BtnDeletePrice_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
            lblStatus.Location = new Point(30, 172);
            lblStatus.Margin = new Padding(4, 0, 4, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(0, 25);
            lblStatus.TabIndex = 9;
            // 
            // grid
            // 
            grid.ColumnHeadersHeight = 34;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.Location = new Point(13, 218);
            grid.Margin = new Padding(4, 4, 4, 4);
            grid.Name = "grid";
            grid.RowHeadersWidth = 62;
            grid.Size = new Size(1442, 570);
            grid.TabIndex = 10;
            // 
            // FuelProviderPricesForm
            // 
            AutoScaleDimensions = new SizeF(144F, 144F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(1485, 818);
            Controls.Add(lblTitle);
            Controls.Add(lblFuelType);
            Controls.Add(txtFuelType);
            Controls.Add(lblPrice);
            Controls.Add(numPrice);
            Controls.Add(lblEffectiveDate);
            Controls.Add(dtEffectiveDate);
            Controls.Add(btnAddPrice);
            Controls.Add(btnDeletePrice);
            Controls.Add(lblStatus);
            Controls.Add(grid);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(6, 6, 6, 6);
            MaximizeBox = false;
            Name = "FuelProviderPricesForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Giá xăng dầu";
            ((System.ComponentModel.ISupportInitialize)numPrice).EndInit();
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
            PerformLayout();
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
        private System.Windows.Forms.Button btnDeletePrice;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView grid;
    }
}