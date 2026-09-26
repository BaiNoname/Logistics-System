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
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnAddProvider = new System.Windows.Forms.Button();
            this.btnDeleteProvider = new System.Windows.Forms.Button();
            this.gridProviders = new System.Windows.Forms.DataGridView();
            this.lblStatus = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.gridProviders)).BeginInit();
            this.SuspendLayout();
            //
            // lblTitle
            //
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.AutoSize = true;
            this.lblTitle.Text = "⛽ Quản lý Nhiên liệu";
            this.lblTitle.Font = TruckingSystem.Desktop.Common.Theme.FontSectionTitle;
            this.lblTitle.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
            //
            // btnAddProvider
            //
            this.btnAddProvider.Location = new System.Drawing.Point(0, 46);
            this.btnAddProvider.Size = new System.Drawing.Size(160, 34);
            this.btnAddProvider.Text = "➕ Thêm hãng xăng";
            this.btnAddProvider.BackColor = TruckingSystem.Desktop.Common.Theme.Primary;
            this.btnAddProvider.ForeColor = System.Drawing.Color.White;
            this.btnAddProvider.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddProvider.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.btnAddProvider.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddProvider.FlatAppearance.BorderSize = 0;
            this.btnAddProvider.FlatAppearance.MouseOverBackColor = TruckingSystem.Desktop.Common.Theme.PrimaryDark;
            this.btnAddProvider.Click += new System.EventHandler(this.BtnAddProvider_Click);
            //
            // btnDeleteProvider
            //
            this.btnDeleteProvider.Location = new System.Drawing.Point(170, 46);
            this.btnDeleteProvider.Size = new System.Drawing.Size(140, 34);
            this.btnDeleteProvider.Text = "🗑️ Xóa hãng";
            this.btnDeleteProvider.BackColor = System.Drawing.Color.White;
            this.btnDeleteProvider.ForeColor = TruckingSystem.Desktop.Common.Theme.Danger;
            this.btnDeleteProvider.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteProvider.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.btnDeleteProvider.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDeleteProvider.FlatAppearance.BorderSize = 1;
            this.btnDeleteProvider.FlatAppearance.BorderColor = TruckingSystem.Desktop.Common.Theme.Danger;
            this.btnDeleteProvider.Click += new System.EventHandler(this.BtnDeleteProvider_Click);
            //
            // gridProviders
            //
            this.gridProviders.Location = new System.Drawing.Point(0, 90);
            this.gridProviders.Size = new System.Drawing.Size(1200, 440);
            this.gridProviders.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            //
            // lblStatus
            //
            this.lblStatus.Location = new System.Drawing.Point(0, 540);
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblStatus.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            //
            // FuelManagementControl
            //
            this.BackColor = TruckingSystem.Desktop.Common.Theme.ContentBg;
            this.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Padding = new System.Windows.Forms.Padding(24);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.btnAddProvider);
            this.Controls.Add(this.btnDeleteProvider);
            this.Controls.Add(this.gridProviders);
            this.Controls.Add(this.lblStatus);
            this.Name = "FuelManagementControl";
            ((System.ComponentModel.ISupportInitialize)(this.gridProviders)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnAddProvider;
        private System.Windows.Forms.Button btnDeleteProvider;
        private System.Windows.Forms.DataGridView gridProviders;
        private System.Windows.Forms.Label lblStatus;
    }
}