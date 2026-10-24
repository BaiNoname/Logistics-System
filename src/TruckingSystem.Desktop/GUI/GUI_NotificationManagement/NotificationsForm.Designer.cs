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

        /// <summary>Bố cục: root (TableLayoutPanel 1 cột) = hàng tiêu đề (tiêu đề bên trái, thanh nút
        /// bên phải) / hàng lọc (nhãn + ô chọn + ô tìm, giãn theo % bề ngang) / dòng trạng thái /
        /// lưới chiếm phần còn lại. Không có tọa độ cứng -> không đè chữ ở mọi mức phóng to.</summary>
        private void InitializeComponent()
        {
            root = new System.Windows.Forms.TableLayoutPanel();
            lblTitle = new System.Windows.Forms.Label();
            buttonBar = new System.Windows.Forms.FlowLayoutPanel();
            btnRefresh = new System.Windows.Forms.Button();
            btnOpen = new System.Windows.Forms.Button();
            btnMarkDone = new System.Windows.Forms.Button();
            filters = new System.Windows.Forms.TableLayoutPanel();
            lblCategory = new System.Windows.Forms.Label();
            cboCategory = new System.Windows.Forms.ComboBox();
            lblReadState = new System.Windows.Forms.Label();
            cboReadState = new System.Windows.Forms.ComboBox();
            txtSearch = new System.Windows.Forms.TextBox();
            lblStatus = new System.Windows.Forms.Label();
            grid = new System.Windows.Forms.DataGridView();
            root.SuspendLayout();
            buttonBar.SuspendLayout();
            filters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            //
            // root
            //
            root.ColumnCount = 2;
            root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            root.Controls.Add(lblTitle, 0, 0);
            root.Controls.Add(buttonBar, 1, 0);
            root.Controls.Add(filters, 0, 1);
            root.Controls.Add(lblStatus, 0, 2);
            root.Controls.Add(grid, 0, 3);
            root.SetColumnSpan(filters, 2);
            root.SetColumnSpan(lblStatus, 2);
            root.SetColumnSpan(grid, 2);
            root.Dock = System.Windows.Forms.DockStyle.Fill;
            root.Name = "root";
            root.Padding = new System.Windows.Forms.Padding(20, 16, 20, 16);
            root.RowCount = 4;
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.Size = new System.Drawing.Size(1000, 620);
            root.TabIndex = 0;
            //
            // lblTitle
            //
            lblTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblTitle.Margin = new System.Windows.Forms.Padding(0, 0, 12, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.TabIndex = 0;
            lblTitle.Text = "🔔 Thông báo";
            //
            // buttonBar
            //
            buttonBar.Anchor = System.Windows.Forms.AnchorStyles.Right;
            buttonBar.AutoSize = true;
            buttonBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            buttonBar.Controls.Add(btnRefresh);
            buttonBar.Controls.Add(btnOpen);
            buttonBar.Controls.Add(btnMarkDone);
            buttonBar.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            buttonBar.Name = "buttonBar";
            buttonBar.TabIndex = 1;
            buttonBar.WrapContents = false;
            //
            // btnRefresh
            //
            btnRefresh.AutoSize = true;
            btnRefresh.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnRefresh.BackColor = System.Drawing.Color.White;
            btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnRefresh.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            btnRefresh.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            btnRefresh.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            btnRefresh.MinimumSize = new System.Drawing.Size(0, 34);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            btnRefresh.TabIndex = 1;
            btnRefresh.Text = "🔄 Làm mới";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
            //
            // btnOpen
            //
            btnOpen.AutoSize = true;
            btnOpen.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnOpen.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            btnOpen.Cursor = System.Windows.Forms.Cursors.Hand;
            btnOpen.Enabled = false;
            btnOpen.FlatAppearance.BorderSize = 0;
            btnOpen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnOpen.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            btnOpen.ForeColor = System.Drawing.Color.White;
            btnOpen.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            btnOpen.MinimumSize = new System.Drawing.Size(0, 34);
            btnOpen.Name = "btnOpen";
            btnOpen.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            btnOpen.TabIndex = 2;
            btnOpen.Text = "↗ Mở màn xử lý";
            btnOpen.UseVisualStyleBackColor = false;
            btnOpen.Click += new System.EventHandler(this.BtnOpen_Click);
            //
            // btnMarkDone
            //
            btnMarkDone.AutoSize = true;
            btnMarkDone.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnMarkDone.BackColor = System.Drawing.Color.White;
            btnMarkDone.Cursor = System.Windows.Forms.Cursors.Hand;
            btnMarkDone.Enabled = false;
            btnMarkDone.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnMarkDone.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            btnMarkDone.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            btnMarkDone.Margin = new System.Windows.Forms.Padding(0);
            btnMarkDone.MinimumSize = new System.Drawing.Size(0, 34);
            btnMarkDone.Name = "btnMarkDone";
            btnMarkDone.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            btnMarkDone.TabIndex = 3;
            btnMarkDone.Text = "✔ Đánh dấu đã xử lý";
            btnMarkDone.UseVisualStyleBackColor = false;
            btnMarkDone.Click += new System.EventHandler(this.BtnMarkDone_Click);
            //
            // filters
            //
            filters.AutoSize = true;
            filters.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            filters.ColumnCount = 5;
            filters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            filters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            filters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            filters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            filters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
            filters.Controls.Add(lblCategory, 0, 0);
            filters.Controls.Add(cboCategory, 1, 0);
            filters.Controls.Add(lblReadState, 2, 0);
            filters.Controls.Add(cboReadState, 3, 0);
            filters.Controls.Add(txtSearch, 4, 0);
            filters.Dock = System.Windows.Forms.DockStyle.Fill;
            filters.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            filters.Name = "filters";
            filters.RowCount = 1;
            filters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            filters.TabIndex = 1;
            //
            // lblCategory
            //
            lblCategory.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblCategory.AutoSize = true;
            lblCategory.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblCategory.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblCategory.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            lblCategory.Name = "lblCategory";
            lblCategory.TabIndex = 3;
            lblCategory.Text = "Loại:";
            //
            // cboCategory
            //
            cboCategory.Dock = System.Windows.Forms.DockStyle.Fill;
            cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboCategory.Font = new System.Drawing.Font("Segoe UI", 9F);
            cboCategory.Margin = new System.Windows.Forms.Padding(0, 0, 16, 0);
            cboCategory.Name = "cboCategory";
            cboCategory.TabIndex = 4;
            //
            // lblReadState
            //
            lblReadState.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblReadState.AutoSize = true;
            lblReadState.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblReadState.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblReadState.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            lblReadState.Name = "lblReadState";
            lblReadState.TabIndex = 5;
            lblReadState.Text = "Trạng thái:";
            //
            // cboReadState
            //
            cboReadState.Dock = System.Windows.Forms.DockStyle.Fill;
            cboReadState.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboReadState.Font = new System.Drawing.Font("Segoe UI", 9F);
            cboReadState.Margin = new System.Windows.Forms.Padding(0, 0, 16, 0);
            cboReadState.Name = "cboReadState";
            cboReadState.TabIndex = 6;
            //
            // txtSearch
            //
            txtSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            txtSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            txtSearch.Margin = new System.Windows.Forms.Padding(0);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "🔍 Tìm theo biển số, tên tài xế, mã chuyến, mã đơn...";
            txtSearch.TabIndex = 7;
            //
            // lblStatus
            //
            lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblStatus.AutoSize = true;
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblStatus.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblStatus.Margin = new System.Windows.Forms.Padding(0, 4, 0, 8);
            lblStatus.Name = "lblStatus";
            lblStatus.TabIndex = 8;
            //
            // grid
            //
            grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid.Dock = System.Windows.Forms.DockStyle.Fill;
            grid.Margin = new System.Windows.Forms.Padding(0);
            grid.Name = "grid";
            grid.TabIndex = 9;
            grid.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.Grid_CellDoubleClick);
            grid.SelectionChanged += new System.EventHandler(this.Grid_SelectionChanged);
            //
            // NotificationsForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(1000, 620);
            Controls.Add(root);
            MinimumSize = new System.Drawing.Size(760, 460);
            Name = "NotificationsForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Thông báo";
            buttonBar.ResumeLayout(false);
            buttonBar.PerformLayout();
            filters.ResumeLayout(false);
            filters.PerformLayout();
            root.ResumeLayout(false);
            root.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.FlowLayoutPanel buttonBar;
        private System.Windows.Forms.TableLayoutPanel filters;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Button btnMarkDone;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.ComboBox cboCategory;
        private System.Windows.Forms.Label lblReadState;
        private System.Windows.Forms.ComboBox cboReadState;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView grid;
    }
}
