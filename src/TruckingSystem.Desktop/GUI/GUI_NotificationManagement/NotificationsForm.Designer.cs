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
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnMarkAllRead = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.grid = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            //
            // lblTitle
            //
            this.lblTitle.Location = new System.Drawing.Point(20, 16);
            this.lblTitle.AutoSize = true;
            this.lblTitle.Text = "🔔 Thông báo";
            this.lblTitle.Font = TruckingSystem.Desktop.Common.Theme.FontSectionTitle;
            this.lblTitle.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
            //
            // btnMarkAllRead
            //
            this.btnMarkAllRead.Location = new System.Drawing.Point(640, 16);
            this.btnMarkAllRead.Size = new System.Drawing.Size(180, 34);
            this.btnMarkAllRead.Text = "✅ Đánh dấu tất cả đã đọc";
            this.btnMarkAllRead.BackColor = System.Drawing.Color.White;
            this.btnMarkAllRead.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
            this.btnMarkAllRead.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMarkAllRead.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.btnMarkAllRead.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMarkAllRead.FlatAppearance.BorderSize = 1;
            this.btnMarkAllRead.Click += new System.EventHandler(this.BtnMarkAllRead_Click);
            //
            // lblStatus
            //
            this.lblStatus.Location = new System.Drawing.Point(20, 56);
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblStatus.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            //
            // grid
            //
            this.grid.Location = new System.Drawing.Point(20, 80);
            this.grid.Size = new System.Drawing.Size(800, 420);
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.grid.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.Grid_CellClick);
            //
            // NotificationsForm
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(840, 520);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.btnMarkAllRead);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.grid);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "NotificationsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Thông báo";
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnMarkAllRead;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView grid;
    }
}
