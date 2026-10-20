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
            root = new System.Windows.Forms.TableLayoutPanel();
            lblTitle = new System.Windows.Forms.Label();
            toolbar = new System.Windows.Forms.FlowLayoutPanel();
            lblChoose = new System.Windows.Forms.Label();
            cboWarehouse = new System.Windows.Forms.ComboBox();
            btnCreateWarehouse = new System.Windows.Forms.Button();
            btnEditWarehouse = new System.Windows.Forms.Button();
            btnManageZones = new System.Windows.Forms.Button();
            btnDeleteWarehouse = new System.Windows.Forms.Button();
            btnManualStock = new System.Windows.Forms.Button();
            btnPendingReceipts = new System.Windows.Forms.Button();
            btnTripIssues = new System.Windows.Forms.Button();
            btnIssueStock = new System.Windows.Forms.Button();
            lblStatus = new System.Windows.Forms.Label();
            zonesPanel = new System.Windows.Forms.Panel();
            testPanel = new System.Windows.Forms.TableLayoutPanel();
            lblTestTitle = new System.Windows.Forms.Label();
            lblCargoLineId = new System.Windows.Forms.Label();
            lblQty = new System.Windows.Forms.Label();
            cboOrderSuggest = new System.Windows.Forms.ComboBox();
            cboCargoLineSuggest = new System.Windows.Forms.ComboBox();
            numQty = new System.Windows.Forms.NumericUpDown();
            btnSuggest = new System.Windows.Forms.Button();
            lblResult = new System.Windows.Forms.Label();
            root.SuspendLayout();
            toolbar.SuspendLayout();
            testPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numQty).BeginInit();
            SuspendLayout();
            // 
            // root
            // 
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.Controls.Add(lblTitle, 0, 0);
            root.Controls.Add(toolbar, 0, 1);
            root.Controls.Add(lblStatus, 0, 2);
            root.Controls.Add(zonesPanel, 0, 3);
            root.Controls.Add(testPanel, 0, 4);
            root.Dock = System.Windows.Forms.DockStyle.Fill;
            root.Margin = new System.Windows.Forms.Padding(0);
            root.Name = "root";
            root.RowCount = 5;
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.TabIndex = 0;
            root.Location = new System.Drawing.Point(0, 0);
            root.Padding = new System.Windows.Forms.Padding(0);
            root.Size = new System.Drawing.Size(1200, 700);
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            lblTitle.Name = "lblTitle";
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Kho trung chuyển";
            // 
            // toolbar
            // 
            toolbar.AutoSize = true;
            toolbar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            toolbar.Controls.Add(lblChoose);
            toolbar.Controls.Add(cboWarehouse);
            toolbar.Controls.Add(btnCreateWarehouse);
            toolbar.Controls.Add(btnEditWarehouse);
            toolbar.Controls.Add(btnManageZones);
            toolbar.Controls.Add(btnDeleteWarehouse);
            toolbar.Controls.Add(btnPendingReceipts);
            toolbar.Controls.Add(btnTripIssues);
            toolbar.Controls.Add(btnManualStock);
            toolbar.Controls.Add(btnIssueStock);
            toolbar.Dock = System.Windows.Forms.DockStyle.Fill;
            toolbar.Margin = new System.Windows.Forms.Padding(0);
            toolbar.Name = "toolbar";
            toolbar.TabIndex = 1;
            toolbar.WrapContents = true;
            // 
            // lblChoose
            // 
            lblChoose.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblChoose.AutoSize = true;
            lblChoose.Font = new System.Drawing.Font("Segoe UI", 10F);
            lblChoose.Margin = new System.Windows.Forms.Padding(0, 0, 6, 8);
            lblChoose.Name = "lblChoose";
            lblChoose.TabIndex = 0;
            lblChoose.Text = "Chọn kho:";
            // 
            // cboWarehouse
            // 
            cboWarehouse.Anchor = System.Windows.Forms.AnchorStyles.Left;
            cboWarehouse.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboWarehouse.MinimumSize = new System.Drawing.Size(280, 0);
            cboWarehouse.Margin = new System.Windows.Forms.Padding(0, 0, 16, 8);
            cboWarehouse.Name = "cboWarehouse";
            cboWarehouse.TabIndex = 1;
            // 
            // btnCreateWarehouse
            // 
            btnCreateWarehouse.AutoSize = true;
            btnCreateWarehouse.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnCreateWarehouse.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            btnCreateWarehouse.Cursor = System.Windows.Forms.Cursors.Hand;
            btnCreateWarehouse.FlatAppearance.BorderSize = 0;
            btnCreateWarehouse.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            btnCreateWarehouse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnCreateWarehouse.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnCreateWarehouse.ForeColor = System.Drawing.Color.White;
            btnCreateWarehouse.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            btnCreateWarehouse.MinimumSize = new System.Drawing.Size(0, 36);
            btnCreateWarehouse.Name = "btnCreateWarehouse";
            btnCreateWarehouse.Padding = new System.Windows.Forms.Padding(10, 3, 10, 3);
            btnCreateWarehouse.TabIndex = 2;
            btnCreateWarehouse.Text = "➕ Thêm kho mới";
            btnCreateWarehouse.UseVisualStyleBackColor = false;
            btnCreateWarehouse.Click += new System.EventHandler(this.BtnCreateWarehouse_Click);
            // 
            // btnEditWarehouse
            // 
            btnEditWarehouse.AutoSize = true;
            btnEditWarehouse.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnEditWarehouse.BackColor = System.Drawing.Color.White;
            btnEditWarehouse.Cursor = System.Windows.Forms.Cursors.Hand;
            btnEditWarehouse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnEditWarehouse.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnEditWarehouse.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            btnEditWarehouse.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            btnEditWarehouse.MinimumSize = new System.Drawing.Size(0, 36);
            btnEditWarehouse.Name = "btnEditWarehouse";
            btnEditWarehouse.Padding = new System.Windows.Forms.Padding(10, 3, 10, 3);
            btnEditWarehouse.TabIndex = 3;
            btnEditWarehouse.Text = "✏️ Sửa kho";
            btnEditWarehouse.UseVisualStyleBackColor = false;
            btnEditWarehouse.Click += new System.EventHandler(this.BtnEditWarehouse_Click);
            // 
            // btnManageZones
            // 
            btnManageZones.AutoSize = true;
            btnManageZones.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnManageZones.BackColor = System.Drawing.Color.White;
            btnManageZones.Cursor = System.Windows.Forms.Cursors.Hand;
            btnManageZones.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            btnManageZones.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnManageZones.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnManageZones.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            btnManageZones.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            btnManageZones.MinimumSize = new System.Drawing.Size(0, 36);
            btnManageZones.Name = "btnManageZones";
            btnManageZones.Padding = new System.Windows.Forms.Padding(10, 3, 10, 3);
            btnManageZones.TabIndex = 4;
            btnManageZones.Text = "🏗️ Quản lý khu vực";
            btnManageZones.UseVisualStyleBackColor = false;
            btnManageZones.Click += new System.EventHandler(this.BtnManageZones_Click);
            // 
            // btnDeleteWarehouse
            // 
            btnDeleteWarehouse.AutoSize = true;
            btnDeleteWarehouse.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnDeleteWarehouse.BackColor = System.Drawing.Color.White;
            btnDeleteWarehouse.Cursor = System.Windows.Forms.Cursors.Hand;
            btnDeleteWarehouse.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(220, 38, 38);
            btnDeleteWarehouse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnDeleteWarehouse.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnDeleteWarehouse.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
            btnDeleteWarehouse.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            btnDeleteWarehouse.MinimumSize = new System.Drawing.Size(0, 36);
            btnDeleteWarehouse.Name = "btnDeleteWarehouse";
            btnDeleteWarehouse.Padding = new System.Windows.Forms.Padding(10, 3, 10, 3);
            btnDeleteWarehouse.TabIndex = 5;
            btnDeleteWarehouse.Text = "🗑️ Xóa kho";
            btnDeleteWarehouse.UseVisualStyleBackColor = false;
            btnDeleteWarehouse.Click += new System.EventHandler(this.BtnDeleteWarehouse_Click);
            // 
            // btnManualStock
            // 
            btnManualStock.AutoSize = true;
            btnManualStock.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnManualStock.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            btnManualStock.Cursor = System.Windows.Forms.Cursors.Hand;
            btnManualStock.FlatAppearance.BorderSize = 0;
            btnManualStock.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnManualStock.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnManualStock.ForeColor = System.Drawing.Color.White;
            btnManualStock.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            btnManualStock.MinimumSize = new System.Drawing.Size(0, 36);
            btnManualStock.Name = "btnManualStock";
            btnManualStock.Padding = new System.Windows.Forms.Padding(10, 3, 10, 3);
            btnManualStock.TabIndex = 7;
            btnManualStock.Text = "🙋 Nhận hàng khách mang tới";
            btnManualStock.UseVisualStyleBackColor = false;
            btnManualStock.Click += new System.EventHandler(this.BtnManualStock_Click);
            // 
            // btnPendingReceipts
            // 
            btnPendingReceipts.AutoSize = true;
            btnPendingReceipts.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnPendingReceipts.BackColor = System.Drawing.Color.FromArgb(217, 119, 6);
            btnPendingReceipts.Cursor = System.Windows.Forms.Cursors.Hand;
            btnPendingReceipts.FlatAppearance.BorderSize = 0;
            btnPendingReceipts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnPendingReceipts.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnPendingReceipts.ForeColor = System.Drawing.Color.White;
            btnPendingReceipts.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            btnPendingReceipts.MinimumSize = new System.Drawing.Size(0, 36);
            btnPendingReceipts.Name = "btnPendingReceipts";
            btnPendingReceipts.Padding = new System.Windows.Forms.Padding(10, 3, 10, 3);
            btnPendingReceipts.TabIndex = 6;
            btnPendingReceipts.Text = "📦 Nhập hàng từ chuyến";
            btnPendingReceipts.UseVisualStyleBackColor = false;
            btnPendingReceipts.Click += new System.EventHandler(this.BtnPendingReceipts_Click);
            //
            // btnTripIssues — xuất kho theo chuyến giao (soạn hàng, bốc lên xe, xác nhận xuất)
            //
            btnTripIssues.AutoSize = true;
            btnTripIssues.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnTripIssues.BackColor = System.Drawing.Color.FromArgb(22, 163, 74);
            btnTripIssues.Cursor = System.Windows.Forms.Cursors.Hand;
            btnTripIssues.FlatAppearance.BorderSize = 0;
            btnTripIssues.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnTripIssues.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnTripIssues.ForeColor = System.Drawing.Color.White;
            btnTripIssues.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            btnTripIssues.MinimumSize = new System.Drawing.Size(0, 36);
            btnTripIssues.Name = "btnTripIssues";
            btnTripIssues.Padding = new System.Windows.Forms.Padding(10, 3, 10, 3);
            btnTripIssues.TabIndex = 6;
            btnTripIssues.Text = "📤 Xuất kho theo chuyến";
            btnTripIssues.UseVisualStyleBackColor = false;
            btnTripIssues.Click += new System.EventHandler(this.BtnTripIssues_Click);
            // 
            // btnIssueStock — xuất hàng tồn của đơn đã Hoàn tất / Hủy + hoàn tác + lịch sử nhập/xuất
            // 
            btnIssueStock.AutoSize = true;
            btnIssueStock.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnIssueStock.BackColor = System.Drawing.Color.White;
            btnIssueStock.Cursor = System.Windows.Forms.Cursors.Hand;
            btnIssueStock.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            btnIssueStock.FlatAppearance.BorderSize = 1;
            btnIssueStock.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnIssueStock.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnIssueStock.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            btnIssueStock.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            btnIssueStock.MinimumSize = new System.Drawing.Size(0, 36);
            btnIssueStock.Name = "btnIssueStock";
            btnIssueStock.Padding = new System.Windows.Forms.Padding(10, 3, 10, 3);
            btnIssueStock.TabIndex = 8;
            btnIssueStock.Text = "📋 Tồn kho & lịch sử";
            btnIssueStock.UseVisualStyleBackColor = false;
            btnIssueStock.Click += new System.EventHandler(this.BtnIssueStock_Click);
            // 
            // lblStatus
            // 
            lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblStatus.AutoSize = true;
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblStatus.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblStatus.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            lblStatus.Name = "lblStatus";
            lblStatus.TabIndex = 2;
            // 
            // zonesPanel
            // 
            zonesPanel.AutoScroll = true;
            zonesPanel.BackColor = System.Drawing.Color.White;
            zonesPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            zonesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            zonesPanel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            zonesPanel.Name = "zonesPanel";
            zonesPanel.TabIndex = 3;
            // 
            // testPanel
            // 
            testPanel.AutoSize = true;
            testPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            testPanel.ColumnCount = 4;
            testPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
            testPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            testPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            testPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            testPanel.Controls.Add(lblTestTitle, 0, 0);
            testPanel.Controls.Add(lblCargoLineId, 0, 1);
            testPanel.Controls.Add(lblQty, 2, 1);
            testPanel.Controls.Add(cboOrderSuggest, 0, 2);
            testPanel.Controls.Add(cboCargoLineSuggest, 1, 2);
            testPanel.Controls.Add(numQty, 2, 2);
            testPanel.Controls.Add(btnSuggest, 3, 2);
            testPanel.Controls.Add(lblResult, 0, 3);
            testPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            testPanel.Margin = new System.Windows.Forms.Padding(0);
            testPanel.Name = "testPanel";
            testPanel.RowCount = 4;
            testPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            testPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            testPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            testPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            testPanel.SetColumnSpan(lblTestTitle, 4);
            testPanel.SetColumnSpan(lblCargoLineId, 2);
            testPanel.SetColumnSpan(lblQty, 2);
            testPanel.SetColumnSpan(lblResult, 4);
            testPanel.TabIndex = 4;
            testPanel.BackColor = System.Drawing.Color.White;
            testPanel.Padding = new System.Windows.Forms.Padding(20, 14, 20, 14);
            // 
            // lblTestTitle
            // 
            lblTestTitle.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblTestTitle.AutoSize = true;
            lblTestTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            lblTestTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblTestTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            lblTestTitle.Name = "lblTestTitle";
            lblTestTitle.TabIndex = 0;
            lblTestTitle.Text = "🧪  Kiểm tra thuật toán Best-Fit — CHỈ GỢI Ý vị trí lưu trữ (không nhập kho)";
            // 
            // lblCargoLineId
            // 
            lblCargoLineId.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblCargoLineId.AutoSize = true;
            lblCargoLineId.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblCargoLineId.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblCargoLineId.Margin = new System.Windows.Forms.Padding(0, 0, 12, 4);
            lblCargoLineId.Name = "lblCargoLineId";
            lblCargoLineId.TabIndex = 1;
            lblCargoLineId.Text = "Chọn đơn hàng → dòng hàng cần thử xếp vị trí:";
            // 
            // lblQty
            // 
            lblQty.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblQty.AutoSize = true;
            lblQty.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblQty.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblQty.Margin = new System.Windows.Forms.Padding(0, 0, 12, 4);
            lblQty.Name = "lblQty";
            lblQty.TabIndex = 2;
            lblQty.Text = "Số lượng cần xếp:";
            // 
            // cboOrderSuggest
            // 
            cboOrderSuggest.Dock = System.Windows.Forms.DockStyle.Fill;
            cboOrderSuggest.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboOrderSuggest.Margin = new System.Windows.Forms.Padding(0, 0, 10, 8);
            cboOrderSuggest.Name = "cboOrderSuggest";
            cboOrderSuggest.TabIndex = 3;
            // 
            // cboCargoLineSuggest
            // 
            cboCargoLineSuggest.Dock = System.Windows.Forms.DockStyle.Fill;
            cboCargoLineSuggest.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboCargoLineSuggest.Margin = new System.Windows.Forms.Padding(0, 0, 20, 8);
            cboCargoLineSuggest.Name = "cboCargoLineSuggest";
            cboCargoLineSuggest.TabIndex = 4;
            // 
            // numQty
            // 
            numQty.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numQty.DecimalPlaces = 2;
            numQty.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
            numQty.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
            numQty.Value = new decimal(new int[] { 2, 0, 0, 0 });
            numQty.Margin = new System.Windows.Forms.Padding(0, 0, 20, 8);
            numQty.Name = "numQty";
            numQty.TabIndex = 5;
            // 
            // btnSuggest
            // 
            btnSuggest.Anchor = System.Windows.Forms.AnchorStyles.Left;
            btnSuggest.AutoSize = true;
            btnSuggest.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnSuggest.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            btnSuggest.Cursor = System.Windows.Forms.Cursors.Hand;
            btnSuggest.FlatAppearance.BorderSize = 0;
            btnSuggest.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            btnSuggest.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnSuggest.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnSuggest.ForeColor = System.Drawing.Color.White;
            btnSuggest.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            btnSuggest.MinimumSize = new System.Drawing.Size(0, 34);
            btnSuggest.Name = "btnSuggest";
            btnSuggest.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            btnSuggest.TabIndex = 6;
            btnSuggest.Text = "🔍  Gợi ý vị trí";
            btnSuggest.UseVisualStyleBackColor = false;
            btnSuggest.Click += new System.EventHandler(this.BtnSuggest_Click);
            // 
            // lblResult
            // 
            lblResult.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblResult.AutoSize = true;
            lblResult.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblResult.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            lblResult.Name = "lblResult";
            lblResult.TabIndex = 7;
            // 
            // WarehousesControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            Controls.Add(root);
            Name = "WarehousesControl";
            Size = new System.Drawing.Size(1200, 700);
            ((System.ComponentModel.ISupportInitialize)numQty).EndInit();
            testPanel.ResumeLayout(false);
            testPanel.PerformLayout();
            toolbar.ResumeLayout(false);
            toolbar.PerformLayout();
            root.ResumeLayout(false);
            root.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
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
        private System.Windows.Forms.Button btnTripIssues;
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
