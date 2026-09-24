namespace TruckingSystem.Desktop.GUI_EmployeeManagement
{
    partial class EmployeesControl
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
            btnCreate = new Button();
            btnMakeDriver = new Button();
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
            lblTitle.Size = new Size(400, 60);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Nhân viên";
            // 
            // btnRefresh
            // 
            btnRefresh.BackColor = Color.White;
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnRefresh.ForeColor = Color.FromArgb(15, 23, 42);
            btnRefresh.Location = new Point(683, 11);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(157, 46);
            btnRefresh.TabIndex = 1;
            btnRefresh.Text = "🔄 Làm mới";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Click += BtnRefresh_Click;
            // 
            // btnCreate
            // 
            btnCreate.BackColor = Color.FromArgb(37, 99, 235);
            btnCreate.Cursor = Cursors.Hand;
            btnCreate.FlatAppearance.BorderSize = 0;
            btnCreate.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
            btnCreate.FlatStyle = FlatStyle.Flat;
            btnCreate.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCreate.ForeColor = Color.White;
            btnCreate.Location = new Point(438, 11);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(214, 46);
            btnCreate.TabIndex = 2;
            btnCreate.Text = "➕ Thêm nhân viên";
            btnCreate.UseVisualStyleBackColor = false;
            btnCreate.Click += BtnCreate_Click;
            // 
            // btnMakeDriver
            // 
            btnMakeDriver.BackColor = Color.White;
            btnMakeDriver.Cursor = Cursors.Hand;
            btnMakeDriver.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
            btnMakeDriver.FlatStyle = FlatStyle.Flat;
            btnMakeDriver.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnMakeDriver.ForeColor = Color.FromArgb(15, 23, 42);
            btnMakeDriver.Location = new Point(872, 11);
            btnMakeDriver.Name = "btnMakeDriver";
            btnMakeDriver.Size = new Size(180, 46);
            btnMakeDriver.TabIndex = 3;
            btnMakeDriver.Text = "🚗 Hồ sơ tài xế";
            btnMakeDriver.UseVisualStyleBackColor = false;
            btnMakeDriver.Click += BtnMakeDriver_Click;
            // 
            // btnSchedule
            // 
            btnSchedule.BackColor = Color.White;
            btnSchedule.Cursor = Cursors.Hand;
            btnSchedule.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
            btnSchedule.FlatStyle = FlatStyle.Flat;
            btnSchedule.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSchedule.ForeColor = Color.FromArgb(15, 23, 42);
            btnSchedule.Location = new Point(1060, 11);
            btnSchedule.Name = "btnSchedule";
            btnSchedule.Size = new Size(180, 46);
            btnSchedule.TabIndex = 4;
            btnSchedule.Text = "📅 Lịch làm việc";
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
            lblStatus.TabIndex = 4;
            // 
            // grid
            // 
            grid.ColumnHeadersHeight = 34;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.Location = new Point(0, 76);
            grid.Name = "grid";
            grid.RowHeadersWidth = 62;
            grid.Size = new Size(1000, 560);
            grid.TabIndex = 5;
            // 
            // EmployeesControl
            // 
            AutoScaleDimensions = new SizeF(144F, 144F);
            BackColor = Color.FromArgb(241, 245, 249);
            Controls.Add(lblTitle);
            Controls.Add(btnRefresh);
            Controls.Add(btnCreate);
            Controls.Add(btnMakeDriver);
            Controls.Add(btnSchedule);
            Controls.Add(lblStatus);
            Controls.Add(grid);
            Name = "EmployeesControl";
            Size = new Size(1450, 568);
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnMakeDriver;
        private System.Windows.Forms.Button btnSchedule;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView grid;
    }
}
