namespace TruckingSystem.Desktop.GUI_NotificationManagement
{
    partial class NotificationsForm
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
            btnMarkAllRead = new Button();
            lblStatus = new Label();
            grid = new DataGridView();
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
            lblTitle.Size = new Size(191, 36);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "🔔 Thông báo";
            // 
            // btnMarkAllRead
            // 
            btnMarkAllRead.BackColor = Color.White;
            btnMarkAllRead.Cursor = Cursors.Hand;
            btnMarkAllRead.FlatStyle = FlatStyle.Flat;
            btnMarkAllRead.Font = new Font("Segoe UI", 9F);
            btnMarkAllRead.ForeColor = Color.FromArgb(15, 23, 42);
            btnMarkAllRead.Location = new Point(960, 24);
            btnMarkAllRead.Margin = new Padding(4, 4, 4, 4);
            btnMarkAllRead.Name = "btnMarkAllRead";
            btnMarkAllRead.Size = new Size(270, 51);
            btnMarkAllRead.TabIndex = 1;
            btnMarkAllRead.Text = "✅ Đánh dấu tất cả đã đọc";
            btnMarkAllRead.UseVisualStyleBackColor = false;
            btnMarkAllRead.Click += BtnMarkAllRead_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
            lblStatus.Location = new Point(30, 84);
            lblStatus.Margin = new Padding(4, 0, 4, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(0, 25);
            lblStatus.TabIndex = 2;
            // 
            // grid
            // 
            grid.ColumnHeadersHeight = 34;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.Location = new Point(-2, 120);
            grid.Margin = new Padding(4, 4, 4, 4);
            grid.Name = "grid";
            grid.RowHeadersWidth = 62;
            grid.Size = new Size(1262, 630);
            grid.TabIndex = 3;
            grid.CellClick += Grid_CellClick;
            // 
            // NotificationsForm
            // 
            AutoScaleDimensions = new SizeF(144F, 144F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(1260, 780);
            Controls.Add(lblTitle);
            Controls.Add(btnMarkAllRead);
            Controls.Add(lblStatus);
            Controls.Add(grid);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(6, 6, 6, 6);
            MaximizeBox = false;
            Name = "NotificationsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Thông báo";
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnMarkAllRead;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView grid;
    }
}
