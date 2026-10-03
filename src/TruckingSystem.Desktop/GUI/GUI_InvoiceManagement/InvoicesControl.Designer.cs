namespace TruckingSystem.Desktop.GUI_InvoiceManagement
{
    partial class InvoicesControl
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
            lblNote = new Label();
            lblStatus = new Label();
            grid = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(2, 11);
            lblTitle.Margin = new Padding(2, 0, 2, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(167, 49);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Hóa đơn";
            // 
            // btnRefresh
            // 
            btnRefresh.BackColor = Color.White;
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnRefresh.ForeColor = Color.FromArgb(15, 23, 42);
            btnRefresh.Location = new Point(671, 2);
            btnRefresh.Margin = new Padding(2, 2, 2, 2);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(103, 33);
            btnRefresh.TabIndex = 1;
            btnRefresh.Text = "🔄 Làm mới";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Click += BtnRefresh_Click;
            // 
            // lblNote
            // 
            lblNote.AutoSize = true;
            lblNote.Font = new Font("Segoe UI", 9F);
            lblNote.ForeColor = Color.FromArgb(217, 119, 6);
            lblNote.Location = new Point(149, 11);
            lblNote.Margin = new Padding(2, 0, 2, 0);
            lblNote.Name = "lblNote";
            lblNote.Size = new Size(518, 15);
            lblNote.TabIndex = 2;
            lblNote.Text = "(Chức năng tạo hóa đơn tự động đang trong quá trình hoàn thiện — hiện chỉ hiển thị danh sách)";
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
            lblStatus.Location = new Point(0, 29);
            lblStatus.Margin = new Padding(2, 0, 2, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(0, 15);
            lblStatus.TabIndex = 3;
            // 
            // grid
            // 
            grid.ColumnHeadersHeight = 34;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.Location = new Point(0, 51);
            grid.Margin = new Padding(2, 2, 2, 2);
            grid.Name = "grid";
            grid.RowHeadersWidth = 62;
            grid.Size = new Size(667, 373);
            grid.TabIndex = 4;
            // 
            // InvoicesControl
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            BackColor = Color.FromArgb(241, 245, 249);
            Controls.Add(lblTitle);
            Controls.Add(btnRefresh);
            Controls.Add(lblNote);
            Controls.Add(lblStatus);
            Controls.Add(grid);
            Margin = new Padding(2, 2, 2, 2);
            Name = "InvoicesControl";
            Size = new Size(945, 379);
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label lblNote;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView grid;
    }
}
