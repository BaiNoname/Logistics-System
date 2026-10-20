namespace TruckingSystem.Desktop.GUI_WarehouseManagement
{
    partial class TripIssuesForm : TruckingSystem.Desktop.Common.BaseForm
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>Bố cục: root (TableLayoutPanel 1 cột) = tiêu đề / giải thích / thanh lọc / lưới chuyến
        /// giao chờ xuất (mỗi dòng 1 loại hàng) / lý do của chuyến đang chọn / hàng nút (Xác nhận xuất ·
        /// Hoàn tác · Tải lại · trạng thái · Đóng). Lưới chiếm phần cao còn lại — không tọa độ cứng.</summary>
        private void InitializeComponent()
        {
            root = new System.Windows.Forms.TableLayoutPanel();
            lblTitle = new System.Windows.Forms.Label();
            lblInfo = new System.Windows.Forms.Label();
            issueFilter = new TruckingSystem.Desktop.Common.FilterBar();
            grid = new System.Windows.Forms.DataGridView();
            lblTripHint = new System.Windows.Forms.Label();
            bottomBar = new System.Windows.Forms.TableLayoutPanel();
            btnIssue = new System.Windows.Forms.Button();
            btnUndo = new System.Windows.Forms.Button();
            btnRefresh = new System.Windows.Forms.Button();
            lblStatus = new System.Windows.Forms.Label();
            btnClose = new System.Windows.Forms.Button();
            root.SuspendLayout();
            bottomBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            //
            // root
            //
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.Controls.Add(lblTitle, 0, 0);
            root.Controls.Add(lblInfo, 0, 1);
            root.Controls.Add(issueFilter, 0, 2);
            root.Controls.Add(grid, 0, 3);
            root.Controls.Add(lblTripHint, 0, 4);
            root.Controls.Add(bottomBar, 0, 5);
            root.Dock = System.Windows.Forms.DockStyle.Fill;
            root.Margin = new System.Windows.Forms.Padding(0);
            root.Name = "root";
            root.Padding = new System.Windows.Forms.Padding(16, 12, 16, 12);
            root.RowCount = 6;
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.Size = new System.Drawing.Size(1120, 680);
            root.TabIndex = 0;
            //
            // lblTitle
            //
            lblTitle.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            lblTitle.Name = "lblTitle";
            lblTitle.TabIndex = 0;
            lblTitle.Text = "📤 Xuất kho theo chuyến";
            //
            // lblInfo
            //
            lblInfo.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblInfo.AutoSize = true;
            lblInfo.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblInfo.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblInfo.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            lblInfo.Name = "lblInfo";
            lblInfo.TabIndex = 1;
            lblInfo.Text = "Chuyến GIAO đã chia xe, chưa chạy. Soạn hàng theo cột \"Lấy ở ô\", bốc lên đúng xe rồi bấm Xác nhận xuất — hệ thống xuất ĐỦ số lượng của chuyến. Chỉ khi kho đã xuất đủ, tài xế mới bắt đầu chuyến giao được. Xuất nhầm / cần đổi xe / hủy đơn khi xe chưa chạy: Hoàn tác xuất (dỡ hàng về kệ).";
            //
            // issueFilter
            //
            issueFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            issueFilter.Name = "issueFilter";
            issueFilter.SearchPlaceholder = "Chuyến, đơn, khách, xe, loại hàng...";
            issueFilter.TabIndex = 2;
            //
            // grid
            //
            grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid.Dock = System.Windows.Forms.DockStyle.Fill;
            grid.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            grid.Name = "grid";
            grid.TabIndex = 3;
            //
            // lblTripHint
            //
            lblTripHint.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblTripHint.AutoSize = true;
            lblTripHint.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            lblTripHint.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblTripHint.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            lblTripHint.Name = "lblTripHint";
            lblTripHint.TabIndex = 4;
            lblTripHint.Text = "Chọn 1 dòng để xem chuyến.";
            //
            // bottomBar
            //
            bottomBar.AutoSize = true;
            bottomBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            bottomBar.ColumnCount = 5;
            bottomBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            bottomBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            bottomBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            bottomBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            bottomBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            bottomBar.Controls.Add(btnIssue, 0, 0);
            bottomBar.Controls.Add(btnUndo, 1, 0);
            bottomBar.Controls.Add(btnRefresh, 2, 0);
            bottomBar.Controls.Add(lblStatus, 3, 0);
            bottomBar.Controls.Add(btnClose, 4, 0);
            bottomBar.Dock = System.Windows.Forms.DockStyle.Fill;
            bottomBar.Margin = new System.Windows.Forms.Padding(0);
            bottomBar.Name = "bottomBar";
            bottomBar.RowCount = 1;
            bottomBar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            bottomBar.TabIndex = 5;
            //
            // btnIssue
            //
            btnIssue.Anchor = System.Windows.Forms.AnchorStyles.Left;
            btnIssue.AutoSize = true;
            btnIssue.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnIssue.BackColor = System.Drawing.Color.FromArgb(22, 163, 74);
            btnIssue.Cursor = System.Windows.Forms.Cursors.Hand;
            btnIssue.FlatAppearance.BorderSize = 0;
            btnIssue.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnIssue.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnIssue.ForeColor = System.Drawing.Color.White;
            btnIssue.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            btnIssue.MinimumSize = new System.Drawing.Size(0, 38);
            btnIssue.Name = "btnIssue";
            btnIssue.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnIssue.TabIndex = 0;
            btnIssue.Text = "📤 Xác nhận xuất kho chuyến này";
            btnIssue.UseVisualStyleBackColor = false;
            btnIssue.Enabled = false;
            btnIssue.Click += new System.EventHandler(this.BtnIssue_Click);
            //
            // btnUndo
            //
            btnUndo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            btnUndo.AutoSize = true;
            btnUndo.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnUndo.BackColor = System.Drawing.Color.White;
            btnUndo.Cursor = System.Windows.Forms.Cursors.Hand;
            btnUndo.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            btnUndo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnUndo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnUndo.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            btnUndo.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            btnUndo.MinimumSize = new System.Drawing.Size(0, 38);
            btnUndo.Name = "btnUndo";
            btnUndo.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnUndo.TabIndex = 1;
            btnUndo.Text = "↩️ Hoàn tác xuất";
            btnUndo.UseVisualStyleBackColor = false;
            btnUndo.Enabled = false;
            btnUndo.Click += new System.EventHandler(this.BtnUndo_Click);
            //
            // btnRefresh
            //
            btnRefresh.Anchor = System.Windows.Forms.AnchorStyles.Left;
            btnRefresh.AutoSize = true;
            btnRefresh.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnRefresh.BackColor = System.Drawing.Color.White;
            btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            btnRefresh.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnRefresh.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnRefresh.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            btnRefresh.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            btnRefresh.MinimumSize = new System.Drawing.Size(0, 38);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnRefresh.TabIndex = 2;
            btnRefresh.Text = "🔄 Tải lại";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
            //
            // lblStatus
            //
            lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblStatus.AutoSize = true;
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblStatus.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblStatus.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.TabIndex = 3;
            //
            // btnClose
            //
            btnClose.Anchor = System.Windows.Forms.AnchorStyles.Left;
            btnClose.AutoSize = true;
            btnClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnClose.BackColor = System.Drawing.Color.White;
            btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            btnClose.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnClose.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnClose.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            btnClose.Margin = new System.Windows.Forms.Padding(0);
            btnClose.MinimumSize = new System.Drawing.Size(0, 38);
            btnClose.Name = "btnClose";
            btnClose.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnClose.TabIndex = 4;
            btnClose.Text = "Đóng";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Anchor = System.Windows.Forms.AnchorStyles.Right;
            btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            //
            // TripIssuesForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(1120, 680);
            Controls.Add(root);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(760, 480);
            Name = "TripIssuesForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Xuất kho theo chuyến (chặng Giao)";
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            bottomBar.ResumeLayout(false);
            bottomBar.PerformLayout();
            root.ResumeLayout(false);
            root.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblInfo;
        private TruckingSystem.Desktop.Common.FilterBar issueFilter;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.Label lblTripHint;
        private System.Windows.Forms.TableLayoutPanel bottomBar;
        private System.Windows.Forms.Button btnIssue;
        private System.Windows.Forms.Button btnUndo;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnClose;
    }
}
