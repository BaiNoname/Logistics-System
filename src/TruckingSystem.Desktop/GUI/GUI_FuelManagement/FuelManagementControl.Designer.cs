namespace TruckingSystem.Desktop.GUI_FuelManagement
{
    partial class FuelManagementControl
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
            btnAddProvider = new System.Windows.Forms.Button();
            btnDeleteProvider = new System.Windows.Forms.Button();
            lblStatus = new System.Windows.Forms.Label();
            gridProviders = new System.Windows.Forms.DataGridView();
            root.SuspendLayout();
            toolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridProviders).BeginInit();
            SuspendLayout();
            // 
            // root
            // 
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.Controls.Add(lblTitle, 0, 0);
            root.Controls.Add(toolbar, 0, 1);
            root.Controls.Add(lblStatus, 0, 2);
            root.Controls.Add(gridProviders, 0, 3);
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
            lblTitle.Text = "⛽ Quản lý Nhiên liệu";
            // 
            // toolbar
            // 
            toolbar.AutoSize = true;
            toolbar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            toolbar.Controls.Add(btnAddProvider);
            toolbar.Controls.Add(btnDeleteProvider);
            toolbar.Dock = System.Windows.Forms.DockStyle.Fill;
            toolbar.Margin = new System.Windows.Forms.Padding(0);
            toolbar.Name = "toolbar";
            toolbar.TabIndex = 1;
            toolbar.WrapContents = true;
            // 
            // btnAddProvider
            // 
            btnAddProvider.AutoSize = true;
            btnAddProvider.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnAddProvider.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            btnAddProvider.Cursor = System.Windows.Forms.Cursors.Hand;
            btnAddProvider.FlatAppearance.BorderSize = 0;
            btnAddProvider.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            btnAddProvider.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnAddProvider.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnAddProvider.ForeColor = System.Drawing.Color.White;
            btnAddProvider.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            btnAddProvider.MinimumSize = new System.Drawing.Size(0, 38);
            btnAddProvider.Name = "btnAddProvider";
            btnAddProvider.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnAddProvider.TabIndex = 0;
            btnAddProvider.Text = "➕ Thêm hãng xăng";
            btnAddProvider.UseVisualStyleBackColor = false;
            btnAddProvider.Click += new System.EventHandler(this.BtnAddProvider_Click);
            // 
            // btnDeleteProvider
            // 
            btnDeleteProvider.AutoSize = true;
            btnDeleteProvider.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnDeleteProvider.BackColor = System.Drawing.Color.White;
            btnDeleteProvider.Cursor = System.Windows.Forms.Cursors.Hand;
            btnDeleteProvider.FlatAppearance.BorderSize = 1;
            btnDeleteProvider.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(220, 38, 38);
            btnDeleteProvider.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(254, 242, 242);
            btnDeleteProvider.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnDeleteProvider.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnDeleteProvider.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
            btnDeleteProvider.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            btnDeleteProvider.MinimumSize = new System.Drawing.Size(0, 38);
            btnDeleteProvider.Name = "btnDeleteProvider";
            btnDeleteProvider.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnDeleteProvider.TabIndex = 1;
            btnDeleteProvider.Text = "🗑️ Xóa hãng";
            btnDeleteProvider.UseVisualStyleBackColor = false;
            btnDeleteProvider.Click += new System.EventHandler(this.BtnDeleteProvider_Click);
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
            // gridProviders
            // 
            gridProviders.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            gridProviders.Dock = System.Windows.Forms.DockStyle.Fill;
            gridProviders.Margin = new System.Windows.Forms.Padding(0);
            gridProviders.Name = "gridProviders";
            gridProviders.TabIndex = 3;
            // 
            // FuelManagementControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            Controls.Add(root);
            Name = "FuelManagementControl";
            Size = new System.Drawing.Size(1200, 700);
            ((System.ComponentModel.ISupportInitialize)gridProviders).EndInit();
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
        private System.Windows.Forms.Button btnAddProvider;
        private System.Windows.Forms.Button btnDeleteProvider;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView gridProviders;
    }
}
