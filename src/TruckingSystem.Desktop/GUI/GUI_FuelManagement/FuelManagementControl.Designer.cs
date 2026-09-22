namespace TruckingSystem.Desktop.GUI_FuelManagement
{
    partial class FuelManagementControl
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
            lblProvidersTitle = new Label();
            btnAddProvider = new Button();
            btnDeleteProvider = new Button();
            gridProviders = new DataGridView();
            lblPricesTitle = new Label();
            btnAddPrice = new Button();
            gridPrices = new DataGridView();
            lblStatus = new Label();
            ((System.ComponentModel.ISupportInitialize)gridProviders).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridPrices).BeginInit();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(0, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(285, 36);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "⛽ Quản lý Nhiên liệu";
            // 
            // lblProvidersTitle
            // 
            lblProvidersTitle.AutoSize = true;
            lblProvidersTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblProvidersTitle.Location = new Point(0, 50);
            lblProvidersTitle.Name = "lblProvidersTitle";
            lblProvidersTitle.Size = new Size(114, 28);
            lblProvidersTitle.TabIndex = 1;
            lblProvidersTitle.Text = "Hãng xăng";
            // 
            // btnAddProvider
            // 
            btnAddProvider.BackColor = Color.FromArgb(37, 99, 235);
            btnAddProvider.Cursor = Cursors.Hand;
            btnAddProvider.FlatAppearance.BorderSize = 0;
            btnAddProvider.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
            btnAddProvider.FlatStyle = FlatStyle.Flat;
            btnAddProvider.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnAddProvider.ForeColor = Color.White;
            btnAddProvider.Location = new Point(239, 39);
            btnAddProvider.Name = "btnAddProvider";
            btnAddProvider.Size = new Size(111, 37);
            btnAddProvider.TabIndex = 2;
            btnAddProvider.Text = "➕ Thêm hãng";
            btnAddProvider.UseVisualStyleBackColor = false;
            btnAddProvider.Click += BtnAddProvider_Click;
            // 
            // btnDeleteProvider
            // 
            btnDeleteProvider.BackColor = Color.White;
            btnDeleteProvider.Cursor = Cursors.Hand;
            btnDeleteProvider.FlatAppearance.BorderColor = Color.FromArgb(220, 38, 38);
            btnDeleteProvider.FlatStyle = FlatStyle.Flat;
            btnDeleteProvider.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnDeleteProvider.ForeColor = Color.FromArgb(220, 38, 38);
            btnDeleteProvider.Location = new Point(376, 39);
            btnDeleteProvider.Name = "btnDeleteProvider";
            btnDeleteProvider.Size = new Size(109, 37);
            btnDeleteProvider.TabIndex = 3;
            btnDeleteProvider.Text = "🗑️ Xóa hãng";
            btnDeleteProvider.UseVisualStyleBackColor = false;
            btnDeleteProvider.Click += BtnDeleteProvider_Click;
            // 
            // gridProviders
            // 
            gridProviders.ColumnHeadersHeight = 34;
            gridProviders.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            gridProviders.Location = new Point(0, 84);
            gridProviders.Name = "gridProviders";
            gridProviders.RowHeadersWidth = 62;
            gridProviders.Size = new Size(485, 400);
            gridProviders.TabIndex = 4;
            // 
            // lblPricesTitle
            // 
            lblPricesTitle.AutoSize = true;
            lblPricesTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPricesTitle.Location = new Point(649, 39);
            lblPricesTitle.Name = "lblPricesTitle";
            lblPricesTitle.Size = new Size(694, 28);
            lblPricesTitle.TabIndex = 5;
            lblPricesTitle.Text = "Lịch sử giá xăng (chọn 1 hãng bên trái để lọc, không chọn = xem tất cả)";
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
            btnAddPrice.Location = new Point(698, 490);
            btnAddPrice.Name = "btnAddPrice";
            btnAddPrice.Size = new Size(160, 40);
            btnAddPrice.TabIndex = 6;
            btnAddPrice.Text = "➕ Thêm giá mới";
            btnAddPrice.UseVisualStyleBackColor = false;
            btnAddPrice.Click += BtnAddPrice_Click;
            // 
            // gridPrices
            // 
            gridPrices.ColumnHeadersHeight = 34;
            gridPrices.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            gridPrices.Location = new Point(698, 84);
            gridPrices.Name = "gridPrices";
            gridPrices.RowHeadersWidth = 62;
            gridPrices.Size = new Size(602, 400);
            gridPrices.TabIndex = 7;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
            lblStatus.Location = new Point(0, 495);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(0, 25);
            lblStatus.TabIndex = 8;
            // 
            // FuelManagementControl
            // 
            AutoScaleDimensions = new SizeF(144F, 144F);
            BackColor = Color.FromArgb(241, 245, 249);
            Controls.Add(lblTitle);
            Controls.Add(lblProvidersTitle);
            Controls.Add(btnAddProvider);
            Controls.Add(btnDeleteProvider);
            Controls.Add(gridProviders);
            Controls.Add(lblPricesTitle);
            Controls.Add(btnAddPrice);
            Controls.Add(gridPrices);
            Controls.Add(lblStatus);
            Name = "FuelManagementControl";
            Size = new Size(1417, 569);
            ((System.ComponentModel.ISupportInitialize)gridProviders).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridPrices).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblProvidersTitle;
        private System.Windows.Forms.Button btnAddProvider;
        private System.Windows.Forms.Button btnDeleteProvider;
        private System.Windows.Forms.DataGridView gridProviders;
        private System.Windows.Forms.Label lblPricesTitle;
        private System.Windows.Forms.Button btnAddPrice;
        private System.Windows.Forms.DataGridView gridPrices;
        private System.Windows.Forms.Label lblStatus;
    }
}
