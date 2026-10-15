namespace TruckingSystem.Desktop.GUI_WarehouseManagement
{
    partial class PendingReceiptsForm : TruckingSystem.Desktop.Common.BaseForm
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>Bố cục: root (TableLayoutPanel 1 cột) = tiêu đề / giải thích / lưới hàng chờ / hàng nút
        /// (Nhập tự động ... Tải lại) / tiêu đề nhập thủ công + số lượng / gợi ý / lưới vị trí / hàng nút Nhập + trạng thái
        /// + Đóng. 2 lưới chia phần cao còn lại theo %, nhãn dài tự xuống dòng — không tọa độ cứng.</summary>
        private void InitializeComponent()
        {
            root = new System.Windows.Forms.TableLayoutPanel();
            lblTitle = new System.Windows.Forms.Label();
            lblInfo = new System.Windows.Forms.Label();
            gridPending = new System.Windows.Forms.DataGridView();
            pendingBar = new System.Windows.Forms.TableLayoutPanel();
            btnAutoTrip = new System.Windows.Forms.Button();
            btnRefresh = new System.Windows.Forms.Button();
            manualBar = new System.Windows.Forms.TableLayoutPanel();
            lblManualTitle = new System.Windows.Forms.Label();
            lblQty = new System.Windows.Forms.Label();
            numQty = new System.Windows.Forms.NumericUpDown();
            lblManualHint = new System.Windows.Forms.Label();
            gridBins = new System.Windows.Forms.DataGridView();
            bottomBar = new System.Windows.Forms.TableLayoutPanel();
            btnManual = new System.Windows.Forms.Button();
            lblStatus = new System.Windows.Forms.Label();
            btnClose = new System.Windows.Forms.Button();
            root.SuspendLayout();
            pendingBar.SuspendLayout();
            manualBar.SuspendLayout();
            bottomBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridPending).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numQty).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridBins).BeginInit();
            SuspendLayout();
            // 
            // root
            // 
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.Controls.Add(lblTitle, 0, 0);
            root.Controls.Add(lblInfo, 0, 1);
            root.Controls.Add(gridPending, 0, 2);
            root.Controls.Add(pendingBar, 0, 3);
            root.Controls.Add(manualBar, 0, 4);
            root.Controls.Add(lblManualHint, 0, 5);
            root.Controls.Add(gridBins, 0, 6);
            root.Controls.Add(bottomBar, 0, 7);
            root.Dock = System.Windows.Forms.DockStyle.Fill;
            root.Margin = new System.Windows.Forms.Padding(0);
            root.Name = "root";
            root.RowCount = 8;
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 55F));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 45F));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.TabIndex = 0;
            root.Padding = new System.Windows.Forms.Padding(16, 12, 16, 12);
            root.Size = new System.Drawing.Size(1072, 704);
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
            lblTitle.Text = "📦 Nhập hàng từ chuyến";
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
            lblInfo.Text = "Xe Lấy hàng đã về kho (đã duyệt) nhưng hàng CHƯA lên kệ. Chọn 1 dòng rồi nhập: TỰ ĐỘNG cả chuyến theo Best-Fit, hoặc THỦ CÔNG — tự chọn vị trí + số lượng (nhập nhiều lần để chia nhiều vị trí). Chặng Giao của đơn chỉ bắt đầu được khi hàng đã nhập kho đủ.";
            // 
            // gridPending
            // 
            gridPending.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridPending.Dock = System.Windows.Forms.DockStyle.Fill;
            gridPending.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            gridPending.Name = "gridPending";
            gridPending.TabIndex = 2;
            // 
            // pendingBar
            // 
            pendingBar.AutoSize = true;
            pendingBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            pendingBar.ColumnCount = 2;
            pendingBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            pendingBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            pendingBar.Controls.Add(btnAutoTrip, 0, 0);
            pendingBar.Controls.Add(btnRefresh, 1, 0);
            pendingBar.Dock = System.Windows.Forms.DockStyle.Fill;
            pendingBar.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            pendingBar.Name = "pendingBar";
            pendingBar.RowCount = 1;
            pendingBar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            pendingBar.TabIndex = 3;
            // 
            // btnAutoTrip
            // 
            btnAutoTrip.Anchor = System.Windows.Forms.AnchorStyles.Left;
            btnAutoTrip.AutoSize = true;
            btnAutoTrip.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnAutoTrip.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            btnAutoTrip.Cursor = System.Windows.Forms.Cursors.Hand;
            btnAutoTrip.Enabled = false;
            btnAutoTrip.FlatAppearance.BorderSize = 0;
            btnAutoTrip.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnAutoTrip.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnAutoTrip.ForeColor = System.Drawing.Color.White;
            btnAutoTrip.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            btnAutoTrip.MinimumSize = new System.Drawing.Size(0, 38);
            btnAutoTrip.Name = "btnAutoTrip";
            btnAutoTrip.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnAutoTrip.TabIndex = 0;
            btnAutoTrip.Text = "🤖 Nhập TỰ ĐỘNG cả chuyến (Best-Fit)";
            btnAutoTrip.UseVisualStyleBackColor = false;
            btnAutoTrip.Click += new System.EventHandler(this.BtnAutoTrip_Click);
            // 
            // btnRefresh
            // 
            btnRefresh.Anchor = System.Windows.Forms.AnchorStyles.Right;
            btnRefresh.AutoSize = true;
            btnRefresh.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnRefresh.BackColor = System.Drawing.Color.White;
            btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnRefresh.Font = new System.Drawing.Font("Segoe UI", 9F);
            btnRefresh.Margin = new System.Windows.Forms.Padding(0);
            btnRefresh.MinimumSize = new System.Drawing.Size(0, 38);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnRefresh.TabIndex = 1;
            btnRefresh.Text = "🔄 Tải lại";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
            // 
            // manualBar
            // 
            manualBar.AutoSize = true;
            manualBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            manualBar.ColumnCount = 3;
            manualBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            manualBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            manualBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            manualBar.Controls.Add(lblManualTitle, 0, 0);
            manualBar.Controls.Add(lblQty, 1, 0);
            manualBar.Controls.Add(numQty, 2, 0);
            manualBar.Dock = System.Windows.Forms.DockStyle.Fill;
            manualBar.Margin = new System.Windows.Forms.Padding(0, 0, 0, 2);
            manualBar.Name = "manualBar";
            manualBar.RowCount = 1;
            manualBar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            manualBar.TabIndex = 4;
            // 
            // lblManualTitle
            // 
            lblManualTitle.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblManualTitle.AutoSize = true;
            lblManualTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblManualTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblManualTitle.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            lblManualTitle.Name = "lblManualTitle";
            lblManualTitle.TabIndex = 0;
            lblManualTitle.Text = "✋ Nhập THỦ CÔNG dòng hàng đang chọn";
            // 
            // lblQty
            // 
            lblQty.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblQty.AutoSize = true;
            lblQty.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblQty.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            lblQty.Name = "lblQty";
            lblQty.TabIndex = 1;
            lblQty.Text = "Số lượng:";
            // 
            // numQty
            // 
            numQty.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numQty.Enabled = false;
            numQty.Maximum = new decimal(new int[] { 0, 0, 0, 0 });
            numQty.Margin = new System.Windows.Forms.Padding(0);
            numQty.Name = "numQty";
            numQty.TabIndex = 2;
            // 
            // lblManualHint
            // 
            lblManualHint.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblManualHint.AutoSize = true;
            lblManualHint.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblManualHint.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblManualHint.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            lblManualHint.Name = "lblManualHint";
            lblManualHint.TabIndex = 5;
            lblManualHint.Text = "Chọn vị trí bên dưới (chỉ hiện vị trí ĐÚNG khu theo tính chất hàng). Dòng đầu = để Best-Fit tự chọn cho riêng số lượng này.";
            // 
            // gridBins
            // 
            gridBins.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridBins.Dock = System.Windows.Forms.DockStyle.Fill;
            gridBins.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            gridBins.Name = "gridBins";
            gridBins.TabIndex = 6;
            // 
            // bottomBar
            // 
            bottomBar.AutoSize = true;
            bottomBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            bottomBar.ColumnCount = 3;
            bottomBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            bottomBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            bottomBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            bottomBar.Controls.Add(btnManual, 0, 0);
            bottomBar.Controls.Add(lblStatus, 1, 0);
            bottomBar.Controls.Add(btnClose, 2, 0);
            bottomBar.Dock = System.Windows.Forms.DockStyle.Fill;
            bottomBar.Margin = new System.Windows.Forms.Padding(0);
            bottomBar.Name = "bottomBar";
            bottomBar.RowCount = 1;
            bottomBar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            bottomBar.TabIndex = 7;
            // 
            // btnManual
            // 
            btnManual.Anchor = System.Windows.Forms.AnchorStyles.Left;
            btnManual.AutoSize = true;
            btnManual.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnManual.BackColor = System.Drawing.Color.White;
            btnManual.Cursor = System.Windows.Forms.Cursors.Hand;
            btnManual.Enabled = false;
            btnManual.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnManual.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnManual.ForeColor = System.Drawing.Color.FromArgb(37, 99, 235);
            btnManual.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            btnManual.MinimumSize = new System.Drawing.Size(0, 38);
            btnManual.Name = "btnManual";
            btnManual.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnManual.TabIndex = 0;
            btnManual.Text = "📥 Nhập vào vị trí đã chọn";
            btnManual.UseVisualStyleBackColor = false;
            btnManual.Click += new System.EventHandler(this.BtnManual_Click);
            // 
            // lblStatus
            // 
            lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblStatus.AutoSize = true;
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblStatus.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblStatus.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.TabIndex = 1;
            // 
            // btnClose
            // 
            btnClose.Anchor = System.Windows.Forms.AnchorStyles.Right;
            btnClose.AutoSize = true;
            btnClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnClose.BackColor = System.Drawing.Color.White;
            btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnClose.Font = new System.Drawing.Font("Segoe UI", 9F);
            btnClose.Margin = new System.Windows.Forms.Padding(0);
            btnClose.MinimumSize = new System.Drawing.Size(0, 38);
            btnClose.Name = "btnClose";
            btnClose.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnClose.TabIndex = 2;
            btnClose.Text = "Đóng";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // PendingReceiptsForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(1072, 704);
            Controls.Add(root);
            MinimumSize = new System.Drawing.Size(760, 500);
            MinimizeBox = false;
            Name = "PendingReceiptsForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Nhập hàng từ chuyến (hàng xe công ty chở về)";
            ((System.ComponentModel.ISupportInitialize)gridPending).EndInit();
            ((System.ComponentModel.ISupportInitialize)numQty).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridBins).EndInit();
            bottomBar.ResumeLayout(false);
            bottomBar.PerformLayout();
            manualBar.ResumeLayout(false);
            manualBar.PerformLayout();
            pendingBar.ResumeLayout(false);
            pendingBar.PerformLayout();
            root.ResumeLayout(false);
            root.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.DataGridView gridPending;
        private System.Windows.Forms.TableLayoutPanel pendingBar;
        private System.Windows.Forms.Button btnAutoTrip;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.TableLayoutPanel manualBar;
        private System.Windows.Forms.Label lblManualTitle;
        private System.Windows.Forms.Label lblQty;
        private System.Windows.Forms.NumericUpDown numQty;
        private System.Windows.Forms.Label lblManualHint;
        private System.Windows.Forms.DataGridView gridBins;
        private System.Windows.Forms.TableLayoutPanel bottomBar;
        private System.Windows.Forms.Button btnManual;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnClose;
    }
}
