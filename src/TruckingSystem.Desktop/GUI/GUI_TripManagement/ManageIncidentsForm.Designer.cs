namespace TruckingSystem.Desktop.GUI_TripManagement
{
    partial class ManageIncidentsForm
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>Bố cục: root (TableLayoutPanel 1 cột) = tiêu đề / thanh nút (tự xuống dòng) /
        /// trạng thái / lưới (chiếm phần còn lại). Không có tọa độ cứng.</summary>
        private void InitializeComponent()
        {
            root = new System.Windows.Forms.TableLayoutPanel();
            lblTitle = new System.Windows.Forms.Label();
            toolbar = new System.Windows.Forms.FlowLayoutPanel();
            btnAddIncident = new System.Windows.Forms.Button();
            btnDeleteIncident = new System.Windows.Forms.Button();
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
            root.Name = "root";
            root.Padding = new System.Windows.Forms.Padding(20, 16, 20, 16);
            root.RowCount = 4;
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.Size = new System.Drawing.Size(920, 520);
            root.TabIndex = 0;
            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            lblTitle.Name = "lblTitle";
            lblTitle.TabIndex = 0;
            lblTitle.Text = "⚠️ Sự cố giữa đường";
            //
            // toolbar
            //
            toolbar.AutoSize = true;
            toolbar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            toolbar.Controls.Add(btnAddIncident);
            toolbar.Controls.Add(btnDeleteIncident);
            toolbar.Dock = System.Windows.Forms.DockStyle.Fill;
            toolbar.Margin = new System.Windows.Forms.Padding(0);
            toolbar.Name = "toolbar";
            toolbar.TabIndex = 1;
            toolbar.WrapContents = true;
            //
            // btnAddIncident
            //
            btnAddIncident.AutoSize = true;
            btnAddIncident.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnAddIncident.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            btnAddIncident.Cursor = System.Windows.Forms.Cursors.Hand;
            btnAddIncident.FlatAppearance.BorderSize = 0;
            btnAddIncident.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            btnAddIncident.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnAddIncident.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnAddIncident.ForeColor = System.Drawing.Color.White;
            btnAddIncident.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            btnAddIncident.MinimumSize = new System.Drawing.Size(0, 38);
            btnAddIncident.Name = "btnAddIncident";
            btnAddIncident.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnAddIncident.TabIndex = 0;
            btnAddIncident.Text = "➕ Ghi nhận sự cố";
            btnAddIncident.UseVisualStyleBackColor = false;
            btnAddIncident.Click += new System.EventHandler(this.BtnAddIncident_Click);
            //
            // btnDeleteIncident
            //
            btnDeleteIncident.AutoSize = true;
            btnDeleteIncident.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnDeleteIncident.BackColor = System.Drawing.Color.White;
            btnDeleteIncident.Cursor = System.Windows.Forms.Cursors.Hand;
            btnDeleteIncident.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(220, 38, 38);
            btnDeleteIncident.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnDeleteIncident.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnDeleteIncident.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
            btnDeleteIncident.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            btnDeleteIncident.MinimumSize = new System.Drawing.Size(0, 38);
            btnDeleteIncident.Name = "btnDeleteIncident";
            btnDeleteIncident.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnDeleteIncident.TabIndex = 1;
            btnDeleteIncident.Text = "🗑️ Xóa sự cố";
            btnDeleteIncident.UseVisualStyleBackColor = false;
            btnDeleteIncident.Click += new System.EventHandler(this.BtnDeleteIncident_Click);
            //
            // lblStatus
            //
            lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblStatus.AutoSize = true;
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblStatus.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblStatus.Margin = new System.Windows.Forms.Padding(0, 4, 0, 8);
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
            // ManageIncidentsForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(920, 520);
            Controls.Add(root);
            MinimumSize = new System.Drawing.Size(640, 400);
            Name = "ManageIncidentsForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Sự cố";
            root.ResumeLayout(false);
            root.PerformLayout();
            toolbar.ResumeLayout(false);
            toolbar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.FlowLayoutPanel toolbar;
        private System.Windows.Forms.Button btnAddIncident;
        private System.Windows.Forms.Button btnDeleteIncident;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView grid;
    }
}
