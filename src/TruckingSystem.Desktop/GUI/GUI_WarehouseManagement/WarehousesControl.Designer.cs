namespace TruckingSystem.Desktop.GUI_WarehouseManagement
{
    partial class WarehousesControl
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>Bố cục: root (TableLayoutPanel 1 cột) = tiêu đề / thanh chọn kho + nút (FlowLayoutPanel tự
        /// xuống dòng) / trạng thái / SƠ ĐỒ KHO zonesPanel (Dock Fill, AutoScroll — khu/ô vẽ bằng code theo mét ×
        /// PixelsPerMeter, đã nhân DPI) / khung thử Best-Fit testPanel (TableLayoutPanel AutoSize). Không còn Resize
        /// tính tay — sửa được bằng Designer.</summary>
        private void InitializeComponent()
        {
            root = new TableLayoutPanel();
            lblTitle = new Label();
            toolbar = new FlowLayoutPanel();
            lblChoose = new Label();
            cboWarehouse = new ComboBox();
            btnCreateWarehouse = new Button();
            btnEditWarehouse = new Button();
            btnManageZones = new Button();
            btnDeleteWarehouse = new Button();
            btnPendingReceipts = new Button();
            btnManualStock = new Button();
            btnIssueStock = new Button();
            lblStatus = new Label();
            zonesPanel = new Panel();
            testPanel = new TableLayoutPanel();
            lblTestTitle = new Label();
            lblCargoLineId = new Label();
            lblQty = new Label();
            cboOrderSuggest = new ComboBox();
            cboCargoLineSuggest = new ComboBox();
            numQty = new NumericUpDown();
            btnSuggest = new Button();
            lblResult = new Label();
            root.SuspendLayout();
            toolbar.SuspendLayout();
            testPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numQty).BeginInit();
            SuspendLayout();
            // 
            // root
            // 
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            root.Controls.Add(lblTitle, 0, 0);
            root.Controls.Add(toolbar, 0, 1);
            root.Controls.Add(lblStatus, 0, 2);
            root.Controls.Add(zonesPanel, 0, 3);
            root.Controls.Add(testPanel, 0, 4);
            root.Dock = DockStyle.Fill;
            root.Location = new Point(0, 0);
            root.Margin = new Padding(0);
            root.Name = "root";
            root.RowCount = 5;
            root.RowStyles.Add(new RowStyle());
            root.RowStyles.Add(new RowStyle());
            root.RowStyles.Add(new RowStyle());
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle());
            root.Size = new Size(1800, 1050);
            root.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(0, 0);
            lblTitle.Margin = new Padding(0, 0, 0, 9);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(322, 48);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Kho trung chuyển";
            // 
            // toolbar
            // 
            toolbar.AutoSize = true;
            toolbar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            toolbar.Controls.Add(lblChoose);
            toolbar.Controls.Add(cboWarehouse);
            toolbar.Controls.Add(btnCreateWarehouse);
            toolbar.Controls.Add(btnEditWarehouse);
            toolbar.Controls.Add(btnManageZones);
            toolbar.Controls.Add(btnDeleteWarehouse);
            toolbar.Controls.Add(btnPendingReceipts);
            toolbar.Controls.Add(btnManualStock);
            toolbar.Controls.Add(btnIssueStock);
            toolbar.Dock = DockStyle.Fill;
            toolbar.Location = new Point(0, 57);
            toolbar.Margin = new Padding(0);
            toolbar.Name = "toolbar";
            toolbar.Size = new Size(1800, 132);
            toolbar.TabIndex = 1;
            // 
            // lblChoose
            // 
            lblChoose.Anchor = AnchorStyles.Left;
            lblChoose.AutoSize = true;
            lblChoose.Font = new Font("Segoe UI", 10F);
            lblChoose.Location = new Point(0, 13);
            lblChoose.Margin = new Padding(0, 0, 9, 12);
            lblChoose.Name = "lblChoose";
            lblChoose.Size = new Size(100, 28);
            lblChoose.TabIndex = 0;
            lblChoose.Text = "Chọn kho:";
            // 
            // cboWarehouse
            // 
            cboWarehouse.Anchor = AnchorStyles.Left;
            cboWarehouse.DropDownStyle = ComboBoxStyle.DropDownList;
            cboWarehouse.Location = new Point(109, 9);
            cboWarehouse.Margin = new Padding(0, 0, 24, 12);
            cboWarehouse.MinimumSize = new Size(418, 0);
            cboWarehouse.Name = "cboWarehouse";
            cboWarehouse.Size = new Size(418, 36);
            cboWarehouse.TabIndex = 1;
            // 
            // btnCreateWarehouse
            // 
            btnCreateWarehouse.AutoSize = true;
            btnCreateWarehouse.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnCreateWarehouse.BackColor = Color.FromArgb(37, 99, 235);
            btnCreateWarehouse.Cursor = Cursors.Hand;
            btnCreateWarehouse.FlatAppearance.BorderSize = 0;
            btnCreateWarehouse.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
            btnCreateWarehouse.FlatStyle = FlatStyle.Flat;
            btnCreateWarehouse.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCreateWarehouse.ForeColor = Color.White;
            btnCreateWarehouse.Location = new Point(551, 0);
            btnCreateWarehouse.Margin = new Padding(0, 0, 12, 12);
            btnCreateWarehouse.MinimumSize = new Size(0, 54);
            btnCreateWarehouse.Name = "btnCreateWarehouse";
            btnCreateWarehouse.Padding = new Padding(15, 4, 15, 4);
            btnCreateWarehouse.Size = new Size(223, 54);
            btnCreateWarehouse.TabIndex = 2;
            btnCreateWarehouse.Text = "➕ Thêm kho mới";
            btnCreateWarehouse.UseVisualStyleBackColor = false;
            btnCreateWarehouse.Click += BtnCreateWarehouse_Click;
            // 
            // btnEditWarehouse
            // 
            btnEditWarehouse.AutoSize = true;
            btnEditWarehouse.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnEditWarehouse.BackColor = Color.White;
            btnEditWarehouse.Cursor = Cursors.Hand;
            btnEditWarehouse.FlatStyle = FlatStyle.Flat;
            btnEditWarehouse.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnEditWarehouse.ForeColor = Color.FromArgb(15, 23, 42);
            btnEditWarehouse.Location = new Point(786, 0);
            btnEditWarehouse.Margin = new Padding(0, 0, 12, 12);
            btnEditWarehouse.MinimumSize = new Size(0, 54);
            btnEditWarehouse.Name = "btnEditWarehouse";
            btnEditWarehouse.Padding = new Padding(15, 4, 15, 4);
            btnEditWarehouse.Size = new Size(164, 54);
            btnEditWarehouse.TabIndex = 3;
            btnEditWarehouse.Text = "✏️ Sửa kho";
            btnEditWarehouse.UseVisualStyleBackColor = false;
            btnEditWarehouse.Click += BtnEditWarehouse_Click;
            // 
            // btnManageZones
            // 
            btnManageZones.AutoSize = true;
            btnManageZones.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnManageZones.BackColor = Color.White;
            btnManageZones.Cursor = Cursors.Hand;
            btnManageZones.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
            btnManageZones.FlatStyle = FlatStyle.Flat;
            btnManageZones.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnManageZones.ForeColor = Color.FromArgb(15, 23, 42);
            btnManageZones.Location = new Point(962, 0);
            btnManageZones.Margin = new Padding(0, 0, 12, 12);
            btnManageZones.MinimumSize = new Size(0, 54);
            btnManageZones.Name = "btnManageZones";
            btnManageZones.Padding = new Padding(15, 4, 15, 4);
            btnManageZones.Size = new Size(242, 54);
            btnManageZones.TabIndex = 4;
            btnManageZones.Text = "🏗️ Quản lý khu vực";
            btnManageZones.UseVisualStyleBackColor = false;
            btnManageZones.Click += BtnManageZones_Click;
            // 
            // btnDeleteWarehouse
            // 
            btnDeleteWarehouse.AutoSize = true;
            btnDeleteWarehouse.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnDeleteWarehouse.BackColor = Color.White;
            btnDeleteWarehouse.Cursor = Cursors.Hand;
            btnDeleteWarehouse.FlatAppearance.BorderColor = Color.FromArgb(220, 38, 38);
            btnDeleteWarehouse.FlatStyle = FlatStyle.Flat;
            btnDeleteWarehouse.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnDeleteWarehouse.ForeColor = Color.FromArgb(220, 38, 38);
            btnDeleteWarehouse.Location = new Point(1216, 0);
            btnDeleteWarehouse.Margin = new Padding(0, 0, 12, 12);
            btnDeleteWarehouse.MinimumSize = new Size(0, 54);
            btnDeleteWarehouse.Name = "btnDeleteWarehouse";
            btnDeleteWarehouse.Padding = new Padding(15, 4, 15, 4);
            btnDeleteWarehouse.Size = new Size(165, 54);
            btnDeleteWarehouse.TabIndex = 5;
            btnDeleteWarehouse.Text = "🗑️ Xóa kho";
            btnDeleteWarehouse.UseVisualStyleBackColor = false;
            btnDeleteWarehouse.Click += BtnDeleteWarehouse_Click;
            // 
            // btnPendingReceipts
            // 
            btnPendingReceipts.AutoSize = true;
            btnPendingReceipts.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnPendingReceipts.BackColor = Color.FromArgb(217, 119, 6);
            btnPendingReceipts.Cursor = Cursors.Hand;
            btnPendingReceipts.FlatAppearance.BorderSize = 0;
            btnPendingReceipts.FlatStyle = FlatStyle.Flat;
            btnPendingReceipts.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnPendingReceipts.ForeColor = Color.White;
            btnPendingReceipts.Location = new Point(1393, 0);
            btnPendingReceipts.Margin = new Padding(0, 0, 12, 12);
            btnPendingReceipts.MinimumSize = new Size(0, 54);
            btnPendingReceipts.Name = "btnPendingReceipts";
            btnPendingReceipts.Padding = new Padding(15, 4, 15, 4);
            btnPendingReceipts.Size = new Size(291, 54);
            btnPendingReceipts.TabIndex = 6;
            btnPendingReceipts.Text = "📦 Nhập hàng từ chuyến";
            btnPendingReceipts.UseVisualStyleBackColor = false;
            btnPendingReceipts.Click += BtnPendingReceipts_Click;
            // 
            // btnManualStock
            // 
            btnManualStock.AutoSize = true;
            btnManualStock.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnManualStock.BackColor = Color.FromArgb(37, 99, 235);
            btnManualStock.Cursor = Cursors.Hand;
            btnManualStock.FlatAppearance.BorderSize = 0;
            btnManualStock.FlatStyle = FlatStyle.Flat;
            btnManualStock.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnManualStock.ForeColor = Color.White;
            btnManualStock.Location = new Point(0, 66);
            btnManualStock.Margin = new Padding(0, 0, 12, 12);
            btnManualStock.MinimumSize = new Size(0, 54);
            btnManualStock.Name = "btnManualStock";
            btnManualStock.Padding = new Padding(15, 4, 15, 4);
            btnManualStock.Size = new Size(344, 54);
            btnManualStock.TabIndex = 7;
            btnManualStock.Text = "🙋 Nhận hàng khách mang tới";
            btnManualStock.UseVisualStyleBackColor = false;
            btnManualStock.Click += BtnManualStock_Click;
            // 
            // btnIssueStock
            // 
            btnIssueStock.AutoSize = true;
            btnIssueStock.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnIssueStock.BackColor = Color.White;
            btnIssueStock.Cursor = Cursors.Hand;
            btnIssueStock.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnIssueStock.FlatStyle = FlatStyle.Flat;
            btnIssueStock.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnIssueStock.ForeColor = Color.FromArgb(15, 23, 42);
            btnIssueStock.Location = new Point(356, 66);
            btnIssueStock.Margin = new Padding(0, 0, 12, 12);
            btnIssueStock.MinimumSize = new Size(0, 54);
            btnIssueStock.Name = "btnIssueStock";
            btnIssueStock.Padding = new Padding(15, 4, 15, 4);
            btnIssueStock.Size = new Size(319, 54);
            btnIssueStock.TabIndex = 8;
            btnIssueStock.Text = "📤 Xuất hàng tồn và lịch sử";
            btnIssueStock.UseVisualStyleBackColor = false;
            btnIssueStock.Click += BtnIssueStock_Click;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
            lblStatus.Location = new Point(0, 189);
            lblStatus.Margin = new Padding(0, 0, 0, 9);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(1800, 25);
            lblStatus.TabIndex = 2;
            // 
            // zonesPanel
            // 
            zonesPanel.AutoScroll = true;
            zonesPanel.BackColor = Color.White;
            zonesPanel.BorderStyle = BorderStyle.FixedSingle;
            zonesPanel.Dock = DockStyle.Fill;
            zonesPanel.Location = new Point(0, 223);
            zonesPanel.Margin = new Padding(0, 0, 0, 15);
            zonesPanel.Name = "zonesPanel";
            zonesPanel.Size = new Size(1800, 594);
            zonesPanel.TabIndex = 3;
            // 
            // testPanel
            // 
            testPanel.AutoSize = true;
            testPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            testPanel.BackColor = Color.White;
            testPanel.ColumnCount = 4;
            testPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            testPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            testPanel.ColumnStyles.Add(new ColumnStyle());
            testPanel.ColumnStyles.Add(new ColumnStyle());
            testPanel.Controls.Add(lblTestTitle, 0, 0);
            testPanel.Controls.Add(lblCargoLineId, 0, 1);
            testPanel.Controls.Add(lblQty, 2, 1);
            testPanel.Controls.Add(cboOrderSuggest, 0, 2);
            testPanel.Controls.Add(cboCargoLineSuggest, 1, 2);
            testPanel.Controls.Add(numQty, 2, 2);
            testPanel.Controls.Add(btnSuggest, 3, 2);
            testPanel.Controls.Add(lblResult, 0, 3);
            testPanel.Dock = DockStyle.Fill;
            testPanel.Location = new Point(0, 832);
            testPanel.Margin = new Padding(0);
            testPanel.Name = "testPanel";
            testPanel.Padding = new Padding(30, 21, 30, 21);
            testPanel.RowCount = 4;
            testPanel.RowStyles.Add(new RowStyle());
            testPanel.RowStyles.Add(new RowStyle());
            testPanel.RowStyles.Add(new RowStyle());
            testPanel.RowStyles.Add(new RowStyle());
            testPanel.Size = new Size(1800, 218);
            testPanel.TabIndex = 4;
            // 
            // lblTestTitle
            // 
            lblTestTitle.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblTestTitle.AutoSize = true;
            testPanel.SetColumnSpan(lblTestTitle, 4);
            lblTestTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblTestTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTestTitle.Location = new Point(30, 21);
            lblTestTitle.Margin = new Padding(0, 0, 0, 12);
            lblTestTitle.Name = "lblTestTitle";
            lblTestTitle.Size = new Size(1740, 36);
            lblTestTitle.TabIndex = 0;
            lblTestTitle.Text = "\U0001f9ea  Kiểm tra thuật toán Best-Fit — CHỈ GỢI Ý vị trí lưu trữ (không nhập kho)";
            // 
            // lblCargoLineId
            // 
            lblCargoLineId.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblCargoLineId.AutoSize = true;
            testPanel.SetColumnSpan(lblCargoLineId, 2);
            lblCargoLineId.Font = new Font("Segoe UI", 9F);
            lblCargoLineId.ForeColor = Color.FromArgb(100, 116, 139);
            lblCargoLineId.Location = new Point(30, 69);
            lblCargoLineId.Margin = new Padding(0, 0, 18, 6);
            lblCargoLineId.Name = "lblCargoLineId";
            lblCargoLineId.Size = new Size(1318, 25);
            lblCargoLineId.TabIndex = 1;
            lblCargoLineId.Text = "Chọn đơn hàng → dòng hàng cần thử xếp vị trí:";
            // 
            // lblQty
            // 
            lblQty.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblQty.AutoSize = true;
            testPanel.SetColumnSpan(lblQty, 2);
            lblQty.Font = new Font("Segoe UI", 9F);
            lblQty.ForeColor = Color.FromArgb(100, 116, 139);
            lblQty.Location = new Point(1366, 69);
            lblQty.Margin = new Padding(0, 0, 18, 6);
            lblQty.Name = "lblQty";
            lblQty.Size = new Size(386, 25);
            lblQty.TabIndex = 2;
            lblQty.Text = "Số lượng cần xếp:";
            // 
            // cboOrderSuggest
            // 
            cboOrderSuggest.Dock = DockStyle.Fill;
            cboOrderSuggest.DropDownStyle = ComboBoxStyle.DropDownList;
            cboOrderSuggest.Location = new Point(30, 100);
            cboOrderSuggest.Margin = new Padding(0, 0, 15, 12);
            cboOrderSuggest.Name = "cboOrderSuggest";
            cboOrderSuggest.Size = new Size(720, 36);
            cboOrderSuggest.TabIndex = 3;
            // 
            // cboCargoLineSuggest
            // 
            cboCargoLineSuggest.Dock = DockStyle.Fill;
            cboCargoLineSuggest.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCargoLineSuggest.Location = new Point(765, 100);
            cboCargoLineSuggest.Margin = new Padding(0, 0, 30, 12);
            cboCargoLineSuggest.Name = "cboCargoLineSuggest";
            cboCargoLineSuggest.Size = new Size(571, 36);
            cboCargoLineSuggest.TabIndex = 4;
            // 
            // numQty
            // 
            numQty.Anchor = AnchorStyles.Left;
            numQty.DecimalPlaces = 2;
            numQty.Location = new Point(1366, 108);
            numQty.Margin = new Padding(0, 0, 30, 12);
            numQty.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
            numQty.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
            numQty.Name = "numQty";
            numQty.Size = new Size(180, 34);
            numQty.TabIndex = 5;
            numQty.Value = new decimal(new int[] { 2, 0, 0, 0 });
            // 
            // btnSuggest
            // 
            btnSuggest.Anchor = AnchorStyles.Left;
            btnSuggest.AutoSize = true;
            btnSuggest.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnSuggest.BackColor = Color.FromArgb(37, 99, 235);
            btnSuggest.Cursor = Cursors.Hand;
            btnSuggest.FlatAppearance.BorderSize = 0;
            btnSuggest.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
            btnSuggest.FlatStyle = FlatStyle.Flat;
            btnSuggest.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSuggest.ForeColor = Color.White;
            btnSuggest.Location = new Point(1576, 100);
            btnSuggest.Margin = new Padding(0, 0, 0, 12);
            btnSuggest.MinimumSize = new Size(0, 51);
            btnSuggest.Name = "btnSuggest";
            btnSuggest.Padding = new Padding(15, 3, 15, 3);
            btnSuggest.Size = new Size(193, 51);
            btnSuggest.TabIndex = 6;
            btnSuggest.Text = "🔍  Gợi ý vị trí";
            btnSuggest.UseVisualStyleBackColor = false;
            btnSuggest.Click += BtnSuggest_Click;
            // 
            // lblResult
            // 
            lblResult.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblResult.AutoSize = true;
            testPanel.SetColumnSpan(lblResult, 4);
            lblResult.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblResult.Location = new Point(30, 169);
            lblResult.Margin = new Padding(0, 6, 0, 0);
            lblResult.Name = "lblResult";
            lblResult.Size = new Size(1740, 28);
            lblResult.TabIndex = 7;
            // 
            // WarehousesControl
            // 
            AutoScaleDimensions = new SizeF(144F, 144F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(241, 245, 249);
            Controls.Add(root);
            Margin = new Padding(6);
            Name = "WarehousesControl";
            Size = new Size(1800, 1050);
            root.ResumeLayout(false);
            root.PerformLayout();
            toolbar.ResumeLayout(false);
            toolbar.PerformLayout();
            testPanel.ResumeLayout(false);
            testPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numQty).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.FlowLayoutPanel toolbar;
        private System.Windows.Forms.Label lblChoose;
        private System.Windows.Forms.ComboBox cboWarehouse;
        private System.Windows.Forms.Button btnCreateWarehouse;
        private System.Windows.Forms.Button btnEditWarehouse;
        private System.Windows.Forms.Button btnManageZones;
        private System.Windows.Forms.Button btnDeleteWarehouse;
        private System.Windows.Forms.Button btnManualStock;
        private System.Windows.Forms.Button btnPendingReceipts;
        private System.Windows.Forms.Button btnIssueStock;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Panel zonesPanel;
        private System.Windows.Forms.TableLayoutPanel testPanel;
        private System.Windows.Forms.Label lblTestTitle;
        private System.Windows.Forms.Label lblCargoLineId;
        private System.Windows.Forms.Label lblQty;
        private System.Windows.Forms.ComboBox cboOrderSuggest;
        private System.Windows.Forms.ComboBox cboCargoLineSuggest;
        private System.Windows.Forms.NumericUpDown numQty;
        private System.Windows.Forms.Button btnSuggest;
        private System.Windows.Forms.Label lblResult;
    }
}
