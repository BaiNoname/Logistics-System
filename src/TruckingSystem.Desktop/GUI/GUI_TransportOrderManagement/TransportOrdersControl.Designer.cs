namespace TruckingSystem.Desktop.GUI_TransportOrderManagement
{
    partial class TransportOrdersControl
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
            btnAllocate = new Button();
            btnCreate = new Button();
            lblStatus = new Label();
            grid = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(0, 9);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(297, 49);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Đơn vận chuyển";
            // 
            // btnRefresh
            // 
            btnRefresh.BackColor = Color.White;
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnRefresh.ForeColor = Color.FromArgb(15, 23, 42);
            btnRefresh.Location = new Point(396, 0);
            btnRefresh.Margin = new Padding(2);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(104, 35);
            btnRefresh.TabIndex = 1;
            btnRefresh.Text = "🔄 Làm mới";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Click += BtnRefresh_Click;
            // 
            // btnAllocate
            // 
            btnAllocate.BackColor = Color.White;
            btnAllocate.Cursor = Cursors.Hand;
            btnAllocate.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
            btnAllocate.FlatStyle = FlatStyle.Flat;
            btnAllocate.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnAllocate.ForeColor = Color.FromArgb(15, 23, 42);
            btnAllocate.Location = new Point(507, 0);
            btnAllocate.Margin = new Padding(2);
            btnAllocate.Name = "btnAllocate";
            btnAllocate.Size = new Size(87, 35);
            btnAllocate.TabIndex = 2;
            btnAllocate.Text = "🚚 Chia xe";
            btnAllocate.UseVisualStyleBackColor = false;
            btnAllocate.Click += BtnAllocate_Click;
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
            btnCreate.Location = new Point(600, 0);
            btnCreate.Margin = new Padding(2);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(100, 35);
            btnCreate.TabIndex = 3;
            btnCreate.Text = "➕ Tạo đơn mới";
            btnCreate.UseVisualStyleBackColor = false;
            btnCreate.Click += BtnCreate_Click;
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
            lblStatus.TabIndex = 4;
            // 
            // grid
            // 
            grid.ColumnHeadersHeight = 34;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.Location = new Point(0, 51);
            grid.Margin = new Padding(2);
            grid.Name = "grid";
            grid.RowHeadersWidth = 62;
            grid.Size = new Size(667, 373);
            grid.TabIndex = 5;
            // 
            // TransportOrdersControl
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            BackColor = Color.FromArgb(241, 245, 249);
            Controls.Add(lblTitle);
            Controls.Add(btnRefresh);
            Controls.Add(btnAllocate);
            Controls.Add(btnCreate);
            Controls.Add(lblStatus);
            Controls.Add(grid);
            Margin = new Padding(2);
            Name = "TransportOrdersControl";
            Size = new Size(877, 379);
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnAllocate;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView grid;
    }
}
