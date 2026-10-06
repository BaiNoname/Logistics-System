namespace TruckingSystem.Desktop.GUI_DriverManagement
{
    partial class DriversControl
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            lblTitle = new Label();
            btnRefresh = new Button();
            btnAddDriver = new Button();
            btnEditProfile = new Button();
            btnSchedule = new Button();
            lblStatus = new Label();
            grid = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(0, 11);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(200, 60);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Tài xế";
            // 
            // btnRefresh
            // 
            btnRefresh.BackColor = Color.White;
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnRefresh.ForeColor = Color.FromArgb(15, 23, 42);
            btnRefresh.Location = new Point(1060, 11);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(157, 46);
            btnRefresh.TabIndex = 1;
            btnRefresh.Text = "🔄 Làm mới";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Click += BtnRefresh_Click;
            // 
            // btnAddDriver
            // 
            btnAddDriver.BackColor = Color.FromArgb(37, 99, 235);
            btnAddDriver.Cursor = Cursors.Hand;
            btnAddDriver.FlatAppearance.BorderSize = 0;
            btnAddDriver.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
            btnAddDriver.FlatStyle = FlatStyle.Flat;
            btnAddDriver.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnAddDriver.ForeColor = Color.White;
            btnAddDriver.Location = new Point(587, 11);
            btnAddDriver.Name = "btnAddDriver";
            btnAddDriver.Size = new Size(180, 46);
            btnAddDriver.TabIndex = 2;
            btnAddDriver.Text = "➕ Thêm tài xế";
            btnAddDriver.UseVisualStyleBackColor = false;
            btnAddDriver.Click += BtnAddDriver_Click;
            // 
            // btnEditProfile
            // 
            btnEditProfile.BackColor = Color.White;
            btnEditProfile.Cursor = Cursors.Hand;
            btnEditProfile.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
            btnEditProfile.FlatStyle = FlatStyle.Flat;
            btnEditProfile.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnEditProfile.ForeColor = Color.FromArgb(15, 23, 42);
            btnEditProfile.Location = new Point(773, 11);
            btnEditProfile.Name = "btnEditProfile";
            btnEditProfile.Size = new Size(170, 46);
            btnEditProfile.TabIndex = 3;
            btnEditProfile.Text = "✏️ Sửa hồ sơ";
            btnEditProfile.UseVisualStyleBackColor = false;
            btnEditProfile.Click += BtnEditProfile_Click;
            // 
            // btnSchedule
            // 
            btnSchedule.BackColor = Color.White;
            btnSchedule.Cursor = Cursors.Hand;
            btnSchedule.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
            btnSchedule.FlatStyle = FlatStyle.Flat;
            btnSchedule.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSchedule.ForeColor = Color.FromArgb(15, 23, 42);
            btnSchedule.Location = new Point(951, 11);
            btnSchedule.Name = "btnSchedule";
            btnSchedule.Size = new Size(101, 46);
            btnSchedule.TabIndex = 4;
            btnSchedule.Text = "📅 Lịch";
            btnSchedule.UseVisualStyleBackColor = false;
            btnSchedule.Click += BtnSchedule_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
            lblStatus.Location = new Point(0, 44);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(0, 25);
            lblStatus.TabIndex = 5;
            // 
            // grid
            // 
            grid.ColumnHeadersHeight = 34;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.Location = new Point(0, 76);
            grid.Name = "grid";
            grid.RowHeadersWidth = 62;
            grid.Size = new Size(1217, 560);
            grid.TabIndex = 6;
            // 
            // DriversControl
            // 
            AutoScaleDimensions = new SizeF(144F, 144F);
            BackColor = Color.FromArgb(241, 245, 249);
            Controls.Add(lblTitle);
            Controls.Add(btnRefresh);
            Controls.Add(btnAddDriver);
            Controls.Add(btnEditProfile);
            Controls.Add(btnSchedule);
            Controls.Add(lblStatus);
            Controls.Add(grid);
            Name = "DriversControl";
            Size = new Size(1450, 568);
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnAddDriver;
        private System.Windows.Forms.Button btnEditProfile;
        private System.Windows.Forms.Button btnSchedule;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView grid;
    }
}