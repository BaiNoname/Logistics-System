namespace TruckingSystem.Desktop.GUI_TransportOrderManagement
{
    partial class OrderDetailForm
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>Bố cục: root (TableLayoutPanel 1 cột) = tiêu đề / thông tin / link theo dõi /
        /// "Dòng hàng hóa" + lưới (40%) / "Chuyến đã gán" + lưới (60%) / thanh nút (tự xuống dòng) /
        /// kết quả. Không có tọa độ cứng.</summary>
        private void InitializeComponent()
        {
            this.root = new System.Windows.Forms.TableLayoutPanel();
            this.lblHeader = new System.Windows.Forms.Label();
            this.lblInfo = new System.Windows.Forms.Label();
            this.lblTrackingLink = new System.Windows.Forms.Label();
            this.lblCargoTitle = new System.Windows.Forms.Label();
            this.gridCargo = new System.Windows.Forms.DataGridView();
            this.lblTripsTitle = new System.Windows.Forms.Label();
            this.gridTrips = new System.Windows.Forms.DataGridView();
            this.actionBar = new System.Windows.Forms.FlowLayoutPanel();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnChangeDeadline = new System.Windows.Forms.Button();
            this.btnAllocate = new System.Windows.Forms.Button();
            this.btnViewRoute = new System.Windows.Forms.Button();
            this.lblResult = new System.Windows.Forms.Label();
            this.root.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridCargo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridTrips)).BeginInit();
            this.actionBar.SuspendLayout();
            this.SuspendLayout();
            //
            // root
            //
            this.root.ColumnCount = 1;
            this.root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.root.Controls.Add(this.lblHeader, 0, 0);
            this.root.Controls.Add(this.lblInfo, 0, 1);
            this.root.Controls.Add(this.lblTrackingLink, 0, 2);
            this.root.Controls.Add(this.lblCargoTitle, 0, 3);
            this.root.Controls.Add(this.gridCargo, 0, 4);
            this.root.Controls.Add(this.lblTripsTitle, 0, 5);
            this.root.Controls.Add(this.gridTrips, 0, 6);
            this.root.Controls.Add(this.actionBar, 0, 7);
            this.root.Controls.Add(this.lblResult, 0, 8);
            this.root.Dock = System.Windows.Forms.DockStyle.Fill;
            this.root.Name = "root";
            this.root.Padding = new System.Windows.Forms.Padding(20, 16, 20, 16);
            this.root.RowCount = 9;
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.root.Size = new System.Drawing.Size(860, 680);
            this.root.TabIndex = 0;
            //
            // lblHeader
            //
            this.lblHeader.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.lblHeader.AutoSize = true;
            this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblHeader.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.lblHeader.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.TabIndex = 0;
            //
            // lblInfo
            //
            this.lblInfo.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.lblInfo.AutoSize = true;
            this.lblInfo.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblInfo.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblInfo.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.TabIndex = 1;
            //
            // lblTrackingLink
            //
            this.lblTrackingLink.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.lblTrackingLink.AutoSize = true;
            this.lblTrackingLink.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblTrackingLink.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTrackingLink.ForeColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.lblTrackingLink.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.lblTrackingLink.Name = "lblTrackingLink";
            this.lblTrackingLink.TabIndex = 2;
            this.lblTrackingLink.Click += new System.EventHandler(this.LblTrackingLink_Click);
            //
            // lblCargoTitle
            //
            this.lblCargoTitle.AutoSize = true;
            this.lblCargoTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblCargoTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblCargoTitle.Name = "lblCargoTitle";
            this.lblCargoTitle.TabIndex = 3;
            this.lblCargoTitle.Text = "Dòng hàng hóa";
            //
            // gridCargo
            //
            this.gridCargo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridCargo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridCargo.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.gridCargo.Name = "gridCargo";
            this.gridCargo.TabIndex = 4;
            //
            // lblTripsTitle
            //
            this.lblTripsTitle.AutoSize = true;
            this.lblTripsTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTripsTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblTripsTitle.Name = "lblTripsTitle";
            this.lblTripsTitle.TabIndex = 5;
            this.lblTripsTitle.Text = "Chuyến đã gán";
            //
            // gridTrips
            //
            this.gridTrips.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridTrips.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridTrips.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.gridTrips.Name = "gridTrips";
            this.gridTrips.TabIndex = 6;
            //
            // actionBar
            //
            this.actionBar.AutoSize = true;
            this.actionBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.actionBar.Controls.Add(this.btnEdit);
            this.actionBar.Controls.Add(this.btnConfirm);
            this.actionBar.Controls.Add(this.btnChangeDeadline);
            this.actionBar.Controls.Add(this.btnCancel);
            this.actionBar.Controls.Add(this.btnAllocate);
            this.actionBar.Controls.Add(this.btnViewRoute);
            this.actionBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.actionBar.Margin = new System.Windows.Forms.Padding(0);
            this.actionBar.Name = "actionBar";
            this.actionBar.TabIndex = 7;
            this.actionBar.WrapContents = true;
            //
            // btnEdit
            //
            this.btnEdit.AutoSize = true;
            this.btnEdit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnEdit.BackColor = System.Drawing.Color.White;
            this.btnEdit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEdit.FlatAppearance.BorderSize = 1;
            this.btnEdit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            this.btnEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEdit.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnEdit.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.btnEdit.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            this.btnEdit.MinimumSize = new System.Drawing.Size(0, 36);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            this.btnEdit.TabIndex = 0;
            this.btnEdit.Text = "✏️ Sửa đơn";
            this.btnEdit.UseVisualStyleBackColor = false;
            this.btnEdit.Click += new System.EventHandler(this.BtnEdit_Click);
            //
            // btnConfirm
            //
            this.btnConfirm.AutoSize = true;
            this.btnConfirm.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnConfirm.BackColor = System.Drawing.Color.FromArgb(22, 163, 74);
            this.btnConfirm.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnConfirm.FlatAppearance.BorderSize = 0;
            this.btnConfirm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConfirm.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnConfirm.ForeColor = System.Drawing.Color.White;
            this.btnConfirm.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            this.btnConfirm.MinimumSize = new System.Drawing.Size(0, 36);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            this.btnConfirm.TabIndex = 1;
            this.btnConfirm.Text = "✅ Xác nhận đơn";
            this.btnConfirm.UseVisualStyleBackColor = false;
            this.btnConfirm.Click += new System.EventHandler(this.BtnConfirm_Click);
            //
            // btnChangeDeadline — đổi / bổ sung hạn giao (đơn Đã xác nhận / Đang chạy)
            //
            this.btnChangeDeadline.AutoSize = true;
            this.btnChangeDeadline.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnChangeDeadline.BackColor = System.Drawing.Color.White;
            this.btnChangeDeadline.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnChangeDeadline.FlatAppearance.BorderSize = 1;
            this.btnChangeDeadline.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            this.btnChangeDeadline.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnChangeDeadline.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnChangeDeadline.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.btnChangeDeadline.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            this.btnChangeDeadline.MinimumSize = new System.Drawing.Size(0, 36);
            this.btnChangeDeadline.Name = "btnChangeDeadline";
            this.btnChangeDeadline.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            this.btnChangeDeadline.TabIndex = 2;
            this.btnChangeDeadline.Text = "📅 Đổi hạn giao";
            this.btnChangeDeadline.UseVisualStyleBackColor = false;
            this.btnChangeDeadline.Click += new System.EventHandler(this.BtnChangeDeadline_Click);
            //
            // btnCancel
            //
            this.btnCancel.AutoSize = true;
            this.btnCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnCancel.BackColor = System.Drawing.Color.White;
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(220, 38, 38);
            this.btnCancel.FlatAppearance.BorderSize = 1;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCancel.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            this.btnCancel.MinimumSize = new System.Drawing.Size(0, 36);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "❌ Hủy đơn";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            //
            // btnAllocate
            //
            this.btnAllocate.AutoSize = true;
            this.btnAllocate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAllocate.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            this.btnAllocate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAllocate.FlatAppearance.BorderSize = 0;
            this.btnAllocate.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            this.btnAllocate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAllocate.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnAllocate.ForeColor = System.Drawing.Color.White;
            this.btnAllocate.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            this.btnAllocate.MinimumSize = new System.Drawing.Size(0, 36);
            this.btnAllocate.Name = "btnAllocate";
            this.btnAllocate.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            this.btnAllocate.TabIndex = 3;
            this.btnAllocate.Text = "🚚  Chia xe (Greedy)";
            this.btnAllocate.UseVisualStyleBackColor = false;
            this.btnAllocate.Click += new System.EventHandler(this.BtnAllocate_Click);
            //
            // btnViewRoute
            //
            this.btnViewRoute.AutoSize = true;
            this.btnViewRoute.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnViewRoute.BackColor = System.Drawing.Color.White;
            this.btnViewRoute.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnViewRoute.Enabled = false;
            this.btnViewRoute.FlatAppearance.BorderSize = 1;
            this.btnViewRoute.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            this.btnViewRoute.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnViewRoute.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnViewRoute.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.btnViewRoute.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            this.btnViewRoute.MinimumSize = new System.Drawing.Size(0, 36);
            this.btnViewRoute.Name = "btnViewRoute";
            this.btnViewRoute.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            this.btnViewRoute.TabIndex = 4;
            this.btnViewRoute.Text = "🗺️  Xem lộ trình";
            this.btnViewRoute.UseVisualStyleBackColor = false;
            this.btnViewRoute.Click += new System.EventHandler(this.BtnViewRoute_Click);
            //
            // lblResult
            //
            this.lblResult.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.lblResult.AutoSize = true;
            this.lblResult.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblResult.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblResult.Name = "lblResult";
            this.lblResult.TabIndex = 8;
            //
            // OrderDetailForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(860, 680);
            this.Controls.Add(this.root);
            this.MinimumSize = new System.Drawing.Size(640, 500);
            this.Name = "OrderDetailForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Chi tiết đơn vận chuyển";
            this.root.ResumeLayout(false);
            this.root.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridCargo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridTrips)).EndInit();
            this.actionBar.ResumeLayout(false);
            this.actionBar.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Label lblTrackingLink;
        private System.Windows.Forms.Label lblCargoTitle;
        private System.Windows.Forms.DataGridView gridCargo;
        private System.Windows.Forms.Label lblTripsTitle;
        private System.Windows.Forms.DataGridView gridTrips;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnChangeDeadline;
        private System.Windows.Forms.Button btnAllocate;
        private System.Windows.Forms.Button btnViewRoute;
        private System.Windows.Forms.Label lblResult;
        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.FlowLayoutPanel actionBar;
    }
}
