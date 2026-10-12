namespace TruckingSystem.Desktop.GUI_VehicleManagement
{
    partial class VehiclesControl
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
            btnRefresh = new System.Windows.Forms.Button();
            btnCreate = new System.Windows.Forms.Button();
            btnEdit = new System.Windows.Forms.Button();
            btnDocuments = new System.Windows.Forms.Button();
            lblStatus = new System.Windows.Forms.Label();
            grid = new System.Windows.Forms.DataGridView();
            root.SuspendLayout();
            toolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            // 
            // root
            // 
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.Controls.Add(lblTitle, 0, 0);
            root.Controls.Add(toolbar, 0, 1);
            root.Controls.Add(lblStatus, 0, 2);
            root.Controls.Add(grid, 0, 3);
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
            lblTitle.Text = "Đội xe";
            // 
            // toolbar
            // 
            toolbar.AutoSize = true;
            toolbar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            toolbar.Controls.Add(btnRefresh);
            toolbar.Controls.Add(btnCreate);
            toolbar.Controls.Add(btnEdit);
            toolbar.Controls.Add(btnDocuments);
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
            btnCreate.Text = "➕ Thêm xe mới";
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
            btnEdit.Text = "✏️ Sửa xe";
            btnEdit.UseVisualStyleBackColor = false;
            btnEdit.Click += new System.EventHandler(this.BtnEdit_Click);
            // 
            // btnDocuments
            // 
            btnDocuments.AutoSize = true;
            btnDocuments.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnDocuments.BackColor = System.Drawing.Color.White;
            btnDocuments.Cursor = System.Windows.Forms.Cursors.Hand;
            btnDocuments.FlatAppearance.BorderSize = 1;
            btnDocuments.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            btnDocuments.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            btnDocuments.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnDocuments.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnDocuments.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            btnDocuments.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            btnDocuments.MinimumSize = new System.Drawing.Size(0, 38);
            btnDocuments.Name = "btnDocuments";
            btnDocuments.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnDocuments.TabIndex = 3;
            btnDocuments.Text = "📄 Giấy tờ xe";
            btnDocuments.UseVisualStyleBackColor = false;
            btnDocuments.Click += new System.EventHandler(this.BtnDocuments_Click);
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
            // VehiclesControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            Controls.Add(root);
            Name = "VehiclesControl";
            Size = new System.Drawing.Size(1200, 700);
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
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
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnDocuments;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView grid;
    }
}
