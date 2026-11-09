namespace TruckingSystem.Desktop.GUI_VehicleManagement
{
    partial class ManageVehicleDocumentsForm
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>Bố cục: root (TableLayoutPanel 1 cột) = tiêu đề / thanh nút (FlowLayoutPanel tự xuống dòng) / trạng thái / lưới (chiếm phần còn lại). Không có tọa độ cứng.</summary>
        private void InitializeComponent()
        {
            root = new System.Windows.Forms.TableLayoutPanel();
            lblTitle = new System.Windows.Forms.Label();
            toolbar = new System.Windows.Forms.FlowLayoutPanel();
            btnAddDocument = new System.Windows.Forms.Button();
            btnDeleteDocument = new System.Windows.Forms.Button();
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
            root.Padding = new System.Windows.Forms.Padding(20, 16, 20, 16);
            root.Size = new System.Drawing.Size(920, 500);
            root.Name = "root";
            root.TabIndex = 0;
            root.RowCount = 4;
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            lblTitle.Text = "📄 Giấy tờ xe";
            lblTitle.Name = "lblTitle";
            lblTitle.TabIndex = 0;
            // 
            // toolbar
            // 
            toolbar.Controls.Add(btnAddDocument);
            toolbar.Controls.Add(btnDeleteDocument);
            toolbar.AutoSize = true;
            toolbar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            toolbar.Dock = System.Windows.Forms.DockStyle.Fill;
            toolbar.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            toolbar.WrapContents = true;
            toolbar.Name = "toolbar";
            toolbar.TabIndex = 1;
            // 
            // btnAddDocument
            // 
            btnAddDocument.AutoSize = true;
            btnAddDocument.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnAddDocument.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            btnAddDocument.Cursor = System.Windows.Forms.Cursors.Hand;
            btnAddDocument.FlatAppearance.BorderSize = 0;
            btnAddDocument.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            btnAddDocument.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnAddDocument.ForeColor = System.Drawing.Color.White;
            btnAddDocument.UseVisualStyleBackColor = false;
            btnAddDocument.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnAddDocument.MinimumSize = new System.Drawing.Size(0, 36);
            btnAddDocument.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnAddDocument.Text = "➕ Nhập giấy tờ mới";
            btnAddDocument.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            btnAddDocument.Name = "btnAddDocument";
            btnAddDocument.TabIndex = 0;
            btnAddDocument.Click += new System.EventHandler(this.BtnAddDocument_Click);
            // 
            // btnDeleteDocument
            // 
            btnDeleteDocument.AutoSize = true;
            btnDeleteDocument.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnDeleteDocument.BackColor = System.Drawing.Color.White;
            btnDeleteDocument.Cursor = System.Windows.Forms.Cursors.Hand;
            btnDeleteDocument.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(220, 38, 38);
            btnDeleteDocument.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnDeleteDocument.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
            btnDeleteDocument.UseVisualStyleBackColor = false;
            btnDeleteDocument.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnDeleteDocument.MinimumSize = new System.Drawing.Size(0, 36);
            btnDeleteDocument.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnDeleteDocument.Text = "🗑️ Xóa giấy tờ";
            btnDeleteDocument.Margin = new System.Windows.Forms.Padding(0);
            btnDeleteDocument.Name = "btnDeleteDocument";
            btnDeleteDocument.TabIndex = 1;
            btnDeleteDocument.Click += new System.EventHandler(this.BtnDeleteDocument_Click);
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblStatus.Margin = new System.Windows.Forms.Padding(0, 8, 0, 4);
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblStatus.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblStatus.Name = "lblStatus";
            lblStatus.TabIndex = 2;
            // 
            // grid
            // 
            grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid.Dock = System.Windows.Forms.DockStyle.Fill;
            grid.Margin = new System.Windows.Forms.Padding(0);
            grid.Name = "grid";
            grid.TabIndex = 3;
            // 
            // ManageVehicleDocumentsForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(920, 500);
            MinimumSize = new System.Drawing.Size(640, 400);
            Name = "ManageVehicleDocumentsForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Giấy tờ xe";
            Controls.Add(root);
            root.ResumeLayout(false);
            root.PerformLayout();
            toolbar.ResumeLayout(false);
            toolbar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnAddDocument;
        private System.Windows.Forms.Button btnDeleteDocument;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.FlowLayoutPanel toolbar;
    }
}
