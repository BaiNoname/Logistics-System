namespace TruckingSystem.Desktop.GUI_CustomerManagement
{
    partial class CustomersControl
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
            lblTitle = new Label();
            btnRefresh = new Button();
            btnCreate = new Button();
            btnEdit = new Button();
            lblStatus = new Label();
            grid = new DataGridView();
            tabs = new TabControl();
            customersTab = new TabPage("Khách hàng");
            contactsTab = new TabPage("Người liên hệ khách hàng");
            _contactsControl = new CustomerContactsControl();
            var customerLayout = new TableLayoutPanel();
            toolbar = new FlowLayoutPanel();
            filterBar = new TruckingSystem.Desktop.Common.FilterBar { Dock = DockStyle.Fill, AutoSize = true };
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            lblTitle.Name = "lblTitle";
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Khách hàng";
            // 
            // toolbar
            // 
            toolbar.AutoSize = true;
            toolbar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            toolbar.Controls.Add(btnCreate);
            toolbar.Controls.Add(btnEdit);
            toolbar.Controls.Add(btnRefresh);
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
            // btnCreate
            // 
            btnCreate.AutoSize = true;
            btnCreate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnCreate.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            btnCreate.Cursor = System.Windows.Forms.Cursors.Hand;
            btnCreate.FlatAppearance.BorderSize = 0;
            btnCreate.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            btnCreate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnCreate.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnCreate.ForeColor = System.Drawing.Color.White;
            btnCreate.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            btnCreate.MinimumSize = new System.Drawing.Size(0, 38);
            btnCreate.Name = "btnCreate";
            btnCreate.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnCreate.TabIndex = 1;
            btnCreate.Text = "➕ Thêm khách hàng";
            btnCreate.UseVisualStyleBackColor = false;
            btnCreate.Click += new System.EventHandler(this.BtnCreate_Click);
            // 
            // btnEdit
            // 
            btnEdit.AutoSize = true;
            btnEdit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnEdit.BackColor = System.Drawing.Color.White;
            btnEdit.Cursor = System.Windows.Forms.Cursors.Hand;
            btnEdit.FlatAppearance.BorderSize = 1;
            btnEdit.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            btnEdit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            btnEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnEdit.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnEdit.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            btnEdit.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            btnEdit.MinimumSize = new System.Drawing.Size(0, 38);
            btnEdit.Name = "btnEdit";
            btnEdit.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnEdit.TabIndex = 2;
            btnEdit.Text = "✏️ Sửa";
            btnEdit.UseVisualStyleBackColor = false;
            btnEdit.Click += new System.EventHandler(this.BtnEdit_Click);
            // 
            // filterBar — tìm nhanh + lọc (Common/FilterBar); các ô chọn thêm ở .cs
            // 
            filterBar.Dock = System.Windows.Forms.DockStyle.Fill;
            filterBar.Name = "filterBar";
            filterBar.SearchPlaceholder = "Tên khách, số điện thoại, mã số thuế, địa chỉ...";
            filterBar.TabIndex = 2;
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
            // CustomersControl
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            BackColor = Color.FromArgb(241, 245, 249);
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 44;
            toolbar.Dock = DockStyle.Fill;
            toolbar.AutoSize = true;
            toolbar.WrapContents = true;
            toolbar.Padding = new Padding(0, 4, 0, 8);
            btnRefresh.Size = new Size(120, 38);
            btnCreate.Size = new Size(190, 38);
            btnEdit.Size = new Size(100, 38);
            lblStatus.AutoSize = false;
            lblStatus.Dock = DockStyle.Fill;
            lblStatus.AutoEllipsis = true;
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            grid.Dock = DockStyle.Fill;
            grid.AccessibleName = "Danh sách khách hàng";
            customerLayout.Dock = DockStyle.Fill;
            customerLayout.ColumnCount = 1;
            customerLayout.RowCount = 4;
            customerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            customerLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            customerLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            customerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            customerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            customerLayout.Controls.Add(toolbar, 0, 0);
            customerLayout.Controls.Add(filterBar, 0, 1);
            customerLayout.Controls.Add(grid, 0, 2);
            customerLayout.Controls.Add(lblStatus, 0, 3);
            customersTab.BackColor = Color.White;
            customersTab.Padding = new Padding(8);
            customersTab.Controls.Add(customerLayout);
            contactsTab.BackColor = Color.White;
            contactsTab.Padding = new Padding(8);
            _contactsControl.Dock = DockStyle.Fill;
            _contactsControl.Name = "_contactsControl";
            _contactsControl.TabIndex = 0;
            contactsTab.Controls.Add(_contactsControl);
            tabs.Dock = DockStyle.Fill;
            tabs.AccessibleName = "Quản lý khách hàng và người liên hệ";
            tabs.TabPages.AddRange(new[] { customersTab, contactsTab });
            Controls.Add(tabs);
            Controls.Add(lblTitle);
            Margin = new Padding(2, 2, 2, 2);
            Name = "CustomersControl";
            Size = new System.Drawing.Size(1200, 700);
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            toolbar.ResumeLayout(false);
            toolbar.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.FlowLayoutPanel toolbar;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Label lblStatus;
        private TruckingSystem.Desktop.Common.FilterBar filterBar;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage customersTab;
        private System.Windows.Forms.TabPage contactsTab;
        private CustomerContactsControl _contactsControl = null!;
    }
}
