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
            btnCreate = Common.Theme.MakePrimaryButton("Lập hóa đơn", 154, 40);
            btnCreate.Name = "btnCreate";
            btnCreate.Click += BtnCreate_Click;
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
            lblTitle.Location = new Point(3, 16);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(250, 74);
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
            btnRefresh.Location = new Point(1006, 3);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(154, 50);
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
            lblNote.Location = new Point(224, 16);
            lblNote.Name = "lblNote";
            lblNote.Size = new Size(776, 25);
            lblNote.TabIndex = 2;
            lblNote.Text = "Lập hóa đơn nháp cho đơn đã hoàn thành và chưa có hóa đơn.";
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
            lblStatus.Location = new Point(0, 44);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(0, 25);
            lblStatus.TabIndex = 3;
            // 
            // grid
            // 
            grid.ColumnHeadersHeight = 34;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.Location = new Point(0, 76);
            grid.Name = "grid";
            grid.RowHeadersWidth = 62;
            grid.Size = new Size(1000, 560);
            grid.TabIndex = 4;
            // 
            // InvoicesControl
            // 
            AutoScaleDimensions = new SizeF(144F, 144F);
            BackColor = Color.FromArgb(241, 245, 249);
            Controls.Add(lblTitle);
            Controls.Add(btnRefresh);
            Controls.Add(lblNote);
            Controls.Add(lblStatus);
            Controls.Add(grid);
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5,
                Padding = new Padding(8), BackColor = Common.Theme.ContentBg
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var toolbar = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };
            btnRefresh.Size = btnCreate.Size;
            toolbar.Controls.Add(btnCreate);
            toolbar.Controls.Add(btnRefresh);
            lblTitle.AutoSize = true;
            lblNote.ForeColor = Common.Theme.TextSecondary;
            lblNote.AutoSize = true;
            lblNote.Dock = DockStyle.Fill;
            lblStatus.Dock = DockStyle.Fill;
            grid.Dock = DockStyle.Fill;
            root.Controls.Add(lblTitle, 0, 0);
            root.Controls.Add(lblNote, 0, 1);
            root.Controls.Add(toolbar, 0, 2);
            root.Controls.Add(lblStatus, 0, 3);
            root.Controls.Add(grid, 0, 4);
            Controls.Add(root);
            Margin = new Padding(3, 3, 3, 3);
            Name = "InvoicesControl";
            Size = new Size(1418, 568);
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Label lblNote;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView grid;
    }
}
