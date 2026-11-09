using TruckingSystem.Desktop.Common;

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
            btnCreate = new Button();
            lblNote = new Label();
            lblStatus = new Label();
            grid = new DataGridView();
            root = new TableLayoutPanel();
            toolbar = new FlowLayoutPanel();
            filterBar = new FilterBar
            {
                Name = "filterBar", Dock = DockStyle.Fill, TabIndex = 4,
                SearchPlaceholder = "Mã hóa đơn, mã đơn, tên khách hàng"
            };
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            root.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(2, 0);
            lblTitle.Margin = new Padding(2, 0, 2, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(112, 32);
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
            btnRefresh.Location = new Point(2, 33);
            btnRefresh.Margin = new Padding(2);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(103, 27);
            btnRefresh.TabIndex = 1;
            btnRefresh.Text = "🔄 Làm mới";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Click += BtnRefresh_Click;
            btnCreate.Name = "btnCreate";
            btnCreate.Text = "Lập hóa đơn";
            btnCreate.Size = new Size(140, 38);
            btnCreate.BackColor = Common.Theme.Primary;
            btnCreate.ForeColor = Color.White;
            btnCreate.Font = Common.Theme.FontBold;
            btnCreate.FlatStyle = FlatStyle.Flat;
            btnCreate.FlatAppearance.BorderSize = 0;
            btnCreate.Cursor = Cursors.Hand;
            btnCreate.Click += BtnCreate_Click;
            btnRefresh.Size = new Size(140, 38);
            // 
            // lblNote
            // 
            lblNote.AutoSize = true;
            lblNote.Dock = DockStyle.Fill;
            lblNote.Font = new Font("Segoe UI", 9F);
            lblNote.ForeColor = Color.FromArgb(100, 116, 139);
            lblNote.Location = new Point(0, 0);
            lblNote.Margin = new Padding(2, 0, 2, 0);
            lblNote.Name = "lblNote";
            lblNote.Size = new Size(340, 15);
            lblNote.TabIndex = 2;
            lblNote.Text = "Lập hóa đơn nháp cho đơn đã hoàn thành và chưa có hóa đơn.";
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Dock = DockStyle.Fill;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
            lblStatus.Location = new Point(0, 0);
            lblStatus.Margin = new Padding(2, 0, 2, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(0, 15);
            lblStatus.TabIndex = 3;
            // 
            // grid
            // 
            grid.ColumnHeadersHeight = 34;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.Dock = DockStyle.Fill;
            grid.Location = new Point(0, 0);
            grid.Margin = new Padding(2);
            grid.Name = "grid";
            grid.RowHeadersWidth = 62;
            grid.Size = new Size(945, 379);
            grid.TabIndex = 6;
            // 
            // root
            // 
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            root.Dock = DockStyle.Fill;
            root.ColumnCount = 1;
            root.RowCount = 6;
            root.Padding = new Padding(8);
            root.Controls.Add(lblTitle, 0, 0);
            root.Controls.Add(lblNote, 0, 1);
            root.Controls.Add(toolbar, 0, 2);
            root.Controls.Add(filterBar, 0, 3);
            root.Controls.Add(lblStatus, 0, 4);
            root.Controls.Add(grid, 0, 5);
            root.Location = new Point(0, 0);
            root.Margin = new Padding(2);
            root.Name = "root";
            root.RowStyles.Add(new RowStyle());
            root.RowStyles.Add(new RowStyle());
            root.RowStyles.Add(new RowStyle());
            root.RowStyles.Add(new RowStyle());
            root.RowStyles.Add(new RowStyle());
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.Size = new Size(133, 67);
            root.TabIndex = 5;
            // 
            // toolbar
            // 
            toolbar.Location = new Point(2, 2);
            toolbar.Margin = new Padding(2);
            toolbar.Name = "toolbar";
            toolbar.AutoSize = true;
            toolbar.Dock = DockStyle.Fill;
            toolbar.WrapContents = true;
            toolbar.Controls.Add(btnCreate);
            toolbar.Controls.Add(btnRefresh);
            toolbar.Size = new Size(129, 67);
            toolbar.TabIndex = 3;
            // 
            // InvoicesControl
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            BackColor = Color.FromArgb(241, 245, 249);
            Controls.Add(root);
            Margin = new Padding(2);
            Name = "InvoicesControl";
            Size = new Size(945, 379);
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            root.ResumeLayout(false);
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
        private TableLayoutPanel root;
        private FlowLayoutPanel toolbar;
        private FilterBar filterBar;
    }
}
