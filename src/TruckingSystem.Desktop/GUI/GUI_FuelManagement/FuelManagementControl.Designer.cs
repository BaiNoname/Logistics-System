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
            btnAddProvider = new Button();
            btnDeleteProvider = new Button();
            gridProviders = new DataGridView();
            lblStatus = new Label();
            ((System.ComponentModel.ISupportInitialize)gridProviders).BeginInit();
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
            // btnAddProvider
            // 
            btnAddProvider.BackColor = Color.FromArgb(37, 99, 235);
            btnAddProvider.Cursor = Cursors.Hand;
            btnAddProvider.FlatAppearance.BorderSize = 0;
            btnAddProvider.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
            btnAddProvider.FlatStyle = FlatStyle.Flat;
            btnAddProvider.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnAddProvider.ForeColor = Color.White;
            btnAddProvider.Location = new Point(0, 46);
            btnAddProvider.Name = "btnAddProvider";
            btnAddProvider.Size = new Size(160, 34);
            btnAddProvider.TabIndex = 1;
            btnAddProvider.Text = "➕ Thêm hãng xăng";
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
            btnDeleteProvider.Location = new Point(170, 46);
            btnDeleteProvider.Name = "btnDeleteProvider";
            btnDeleteProvider.Size = new Size(140, 34);
            btnDeleteProvider.TabIndex = 2;
            btnDeleteProvider.Text = "🗑️ Xóa hãng";
            btnDeleteProvider.UseVisualStyleBackColor = false;
            btnDeleteProvider.Click += BtnDeleteProvider_Click;
            // 
            // gridProviders
            // 
            gridProviders.ColumnHeadersHeight = 34;
            gridProviders.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            gridProviders.Location = new Point(0, 90);
            gridProviders.Name = "gridProviders";
            gridProviders.RowHeadersWidth = 62;
            gridProviders.Size = new Size(1200, 440);
            gridProviders.TabIndex = 3;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
            lblStatus.Location = new Point(0, 540);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(0, 25);
            lblStatus.TabIndex = 4;
            // 
            // FuelManagementControl
            // 
            AutoScaleDimensions = new SizeF(144F, 144F);
            BackColor = Color.FromArgb(241, 245, 249);
            Controls.Add(lblTitle);
            Controls.Add(btnAddProvider);
            Controls.Add(btnDeleteProvider);
            Controls.Add(gridProviders);
            Controls.Add(lblStatus);
            Name = "FuelManagementControl";
            Padding = new Padding(24);
            Size = new Size(1417, 568);
            ((System.ComponentModel.ISupportInitialize)gridProviders).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnAddProvider;
        private System.Windows.Forms.Button btnDeleteProvider;
        private System.Windows.Forms.DataGridView gridProviders;
        private System.Windows.Forms.Label lblStatus;
    }
}