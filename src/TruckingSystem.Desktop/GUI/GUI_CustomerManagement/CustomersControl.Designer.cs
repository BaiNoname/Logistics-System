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
            var toolbar = new FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(0, 0);
            lblTitle.Margin = new Padding(2, 0, 2, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(267, 36);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Khách hàng";
            // 
            // btnRefresh
            // 
            btnRefresh.BackColor = Color.White;
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnRefresh.ForeColor = Color.FromArgb(15, 23, 42);
            btnRefresh.Location = new Point(516, 3);
            btnRefresh.Margin = new Padding(2, 2, 2, 2);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(73, 35);
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
            btnCreate.Location = new Point(593, 3);
            btnCreate.Margin = new Padding(2, 2, 2, 2);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(113, 35);
            btnCreate.TabIndex = 2;
            btnCreate.Text = "➕ Thêm khách hàng";
            btnCreate.UseVisualStyleBackColor = false;
            btnCreate.Click += BtnCreate_Click;
            // 
            // btnEdit
            // 
            btnEdit.BackColor = Color.White;
            btnEdit.Cursor = Cursors.Hand;
            btnEdit.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnEdit.ForeColor = Color.FromArgb(15, 23, 42);
            btnEdit.Location = new Point(710, 3);
            btnEdit.Margin = new Padding(2, 2, 2, 2);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(100, 35);
            btnEdit.TabIndex = 3;
            btnEdit.Text = "✏️ Sửa";
            btnEdit.UseVisualStyleBackColor = false;
            btnEdit.Click += BtnEdit_Click;
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
            toolbar.Controls.AddRange(new Control[] { btnCreate, btnEdit, btnRefresh });
            lblStatus.AutoSize = false;
            lblStatus.Dock = DockStyle.Fill;
            lblStatus.AutoEllipsis = true;
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            grid.Dock = DockStyle.Fill;
            grid.AccessibleName = "Danh sách khách hàng";
            customerLayout.Dock = DockStyle.Fill;
            customerLayout.ColumnCount = 1;
            customerLayout.RowCount = 3;
            customerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            customerLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            customerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            customerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            customerLayout.Controls.Add(toolbar, 0, 0);
            customerLayout.Controls.Add(grid, 0, 1);
            customerLayout.Controls.Add(lblStatus, 0, 2);
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
            Size = new Size(967, 379);
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage customersTab;
        private System.Windows.Forms.TabPage contactsTab;
        private CustomerContactsControl _contactsControl = null!;
    }
}
