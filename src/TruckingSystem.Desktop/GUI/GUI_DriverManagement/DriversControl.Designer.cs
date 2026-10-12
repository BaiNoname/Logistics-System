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

        /// <summary>Bố cục: TableLayoutPanel 1 cột — tiêu đề, thanh nút (FlowLayoutPanel tự xuống
        /// dòng khi màn hình hẹp), dòng trạng thái, lưới chiếm phần còn lại. Không dùng tọa độ cứng
        /// -> không đè chữ ở mọi kích thước / mức phóng to màn hình. Sửa được bằng Designer.</summary>
        private void InitializeComponent()
        {
            root = new System.Windows.Forms.TableLayoutPanel();
            lblTitle = new System.Windows.Forms.Label();
            toolbar = new System.Windows.Forms.FlowLayoutPanel();
            btnRefresh = new System.Windows.Forms.Button();
            btnAddDriver = new System.Windows.Forms.Button();
            btnEditProfile = new System.Windows.Forms.Button();
            btnSchedule = new System.Windows.Forms.Button();
            lblStatus = new System.Windows.Forms.Label();
            grid = new System.Windows.Forms.DataGridView();
            root.SuspendLayout();
            toolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            // 
            // root
            // 
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.Controls.Add(lblTitle, 0, 0);
            root.Controls.Add(toolbar, 0, 1);
            root.Controls.Add(lblStatus, 0, 2);
            root.Controls.Add(grid, 0, 3);
            root.Dock = System.Windows.Forms.DockStyle.Fill;
            root.Location = new System.Drawing.Point(0, 0);
            root.Name = "root";
            root.Padding = new System.Windows.Forms.Padding(0);
            root.RowCount = 4;
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.Size = new System.Drawing.Size(1200, 700);
            root.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            lblTitle.Name = "lblTitle";
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Tài xế";
            // 
            // toolbar
            // 
            toolbar.AutoSize = true;
            toolbar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            toolbar.Controls.Add(btnRefresh);
            toolbar.Controls.Add(btnAddDriver);
            toolbar.Controls.Add(btnEditProfile);
            toolbar.Controls.Add(btnSchedule);
            toolbar.Dock = System.Windows.Forms.DockStyle.Fill;
            toolbar.Margin = new System.Windows.Forms.Padding(0);
            toolbar.Name = "toolbar";
            toolbar.TabIndex = 1;
            toolbar.WrapContents = true;
            // 
            // btnRefresh
            // 
            btnRefresh.AutoSize = true;
            btnRefresh.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnRefresh.BackColor = System.Drawing.Color.White;
            btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            btnRefresh.FlatAppearance.BorderSize = 1;
            btnRefresh.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            btnRefresh.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnRefresh.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnRefresh.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            btnRefresh.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            btnRefresh.MinimumSize = new System.Drawing.Size(0, 38);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnRefresh.TabIndex = 0;
            btnRefresh.Text = "🔄 Làm mới";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
            // 
            // btnAddDriver
            // 
            btnAddDriver.AutoSize = true;
            btnAddDriver.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnAddDriver.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            btnAddDriver.Cursor = System.Windows.Forms.Cursors.Hand;
            btnAddDriver.FlatAppearance.BorderSize = 0;
            btnAddDriver.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            btnAddDriver.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnAddDriver.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnAddDriver.ForeColor = System.Drawing.Color.White;
            btnAddDriver.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            btnAddDriver.MinimumSize = new System.Drawing.Size(0, 38);
            btnAddDriver.Name = "btnAddDriver";
            btnAddDriver.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnAddDriver.TabIndex = 1;
            btnAddDriver.Text = "➕ Thêm tài xế";
            btnAddDriver.UseVisualStyleBackColor = false;
            btnAddDriver.Click += new System.EventHandler(this.BtnAddDriver_Click);
            // 
            // btnEditProfile
            // 
            btnEditProfile.AutoSize = true;
            btnEditProfile.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnEditProfile.BackColor = System.Drawing.Color.White;
            btnEditProfile.Cursor = System.Windows.Forms.Cursors.Hand;
            btnEditProfile.FlatAppearance.BorderSize = 1;
            btnEditProfile.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            btnEditProfile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            btnEditProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnEditProfile.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnEditProfile.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            btnEditProfile.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            btnEditProfile.MinimumSize = new System.Drawing.Size(0, 38);
            btnEditProfile.Name = "btnEditProfile";
            btnEditProfile.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnEditProfile.TabIndex = 2;
            btnEditProfile.Text = "✏️ Sửa hồ sơ";
            btnEditProfile.UseVisualStyleBackColor = false;
            btnEditProfile.Click += new System.EventHandler(this.BtnEditProfile_Click);
            // 
            // btnSchedule
            // 
            btnSchedule.AutoSize = true;
            btnSchedule.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnSchedule.BackColor = System.Drawing.Color.White;
            btnSchedule.Cursor = System.Windows.Forms.Cursors.Hand;
            btnSchedule.FlatAppearance.BorderSize = 1;
            btnSchedule.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            btnSchedule.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            btnSchedule.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnSchedule.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnSchedule.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            btnSchedule.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            btnSchedule.MinimumSize = new System.Drawing.Size(0, 38);
            btnSchedule.Name = "btnSchedule";
            btnSchedule.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnSchedule.TabIndex = 3;
            btnSchedule.Text = "📅 Ngày làm";
            btnSchedule.UseVisualStyleBackColor = false;
            btnSchedule.Click += new System.EventHandler(this.BtnSchedule_Click);
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblStatus.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblStatus.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            lblStatus.Name = "lblStatus";
            lblStatus.TabIndex = 2;
            // 
            // grid
            // 
            grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.Dock = System.Windows.Forms.DockStyle.Fill;
            grid.Margin = new System.Windows.Forms.Padding(0);
            grid.Name = "grid";
            grid.TabIndex = 3;
            // 
            // DriversControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            Controls.Add(root);
            Name = "DriversControl";
            Size = new System.Drawing.Size(1200, 700);
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            toolbar.ResumeLayout(false);
            toolbar.PerformLayout();
            root.ResumeLayout(false);
            root.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.FlowLayoutPanel toolbar;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnAddDriver;
        private System.Windows.Forms.Button btnEditProfile;
        private System.Windows.Forms.Button btnSchedule;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView grid;
    }
}
