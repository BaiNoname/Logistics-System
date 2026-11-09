namespace TruckingSystem.Desktop.GUI_WarehouseManagement;

partial class ManualStockForm
{
    private System.ComponentModel.IContainer components = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>Bố cục: root (TableLayoutPanel 1 cột) = tiêu đề / hàng chọn đơn-dòng hàng-số lượng (nhãn trên,
    /// ô dưới, giãn theo %) / tóm tắt tình trạng (tự xuống dòng) / lưới chuyến / lưới vị trí / hàng nút Nhập-Xuất +
    /// trạng thái / tiêu đề lịch sử + lọc + Hoàn tác / lưới lịch sử. 3 lưới chia phần cao còn lại theo %.</summary>
    private void InitializeComponent()
    {
        root = new System.Windows.Forms.TableLayoutPanel();
        lblTitle = new System.Windows.Forms.Label();
        inputs = new System.Windows.Forms.TableLayoutPanel();
        lblOrder = new System.Windows.Forms.Label();
        lblCargoLine = new System.Windows.Forms.Label();
        lblQty = new System.Windows.Forms.Label();
        cboOrder = new System.Windows.Forms.ComboBox();
        cboCargoLine = new System.Windows.Forms.ComboBox();
        numQty = new TruckingSystem.Desktop.Common.NumberBox();
        lblFlowTitle = new System.Windows.Forms.Label();
        lblFlowSummary = new System.Windows.Forms.Label();
        gridTrips = new System.Windows.Forms.DataGridView();
        lblBinsTitle = new System.Windows.Forms.Label();
        gridBins = new System.Windows.Forms.DataGridView();
        actionBar = new System.Windows.Forms.TableLayoutPanel();
        btnReceive = new System.Windows.Forms.Button();
        btnIssue = new System.Windows.Forms.Button();
        lblStatus = new System.Windows.Forms.Label();
        historyBar = new System.Windows.Forms.TableLayoutPanel();
        lblHistoryTitle = new System.Windows.Forms.Label();
        chkAllOrders = new System.Windows.Forms.CheckBox();
        btnReverse = new System.Windows.Forms.Button();
        historyFilter = new TruckingSystem.Desktop.Common.FilterBar();
        gridHistory = new System.Windows.Forms.DataGridView();
        root.SuspendLayout();
        inputs.SuspendLayout();
        actionBar.SuspendLayout();
        historyBar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numQty).BeginInit();
        ((System.ComponentModel.ISupportInitialize)gridTrips).BeginInit();
        ((System.ComponentModel.ISupportInitialize)gridBins).BeginInit();
        ((System.ComponentModel.ISupportInitialize)gridHistory).BeginInit();
        SuspendLayout();
        // 
        // root
        // 
        root.ColumnCount = 1;
        root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        root.Controls.Add(lblTitle, 0, 0);
        root.Controls.Add(inputs, 0, 1);
        root.Controls.Add(lblFlowTitle, 0, 2);
        root.Controls.Add(lblFlowSummary, 0, 3);
        root.Controls.Add(gridTrips, 0, 4);
        root.Controls.Add(lblBinsTitle, 0, 5);
        root.Controls.Add(gridBins, 0, 6);
        root.Controls.Add(actionBar, 0, 7);
        root.Controls.Add(historyBar, 0, 8);
        root.Controls.Add(historyFilter, 0, 9);
        root.Controls.Add(gridHistory, 0, 10);
        root.Dock = System.Windows.Forms.DockStyle.Fill;
        root.Margin = new System.Windows.Forms.Padding(0);
        root.Name = "root";
        root.RowCount = 11;
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
        root.TabIndex = 0;
        root.Padding = new System.Windows.Forms.Padding(20, 16, 20, 16);
        root.Size = new System.Drawing.Size(1100, 760);
        // 
        // lblTitle
        // 
        lblTitle.AutoSize = true;
        lblTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
        lblTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
        lblTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
        lblTitle.Name = "lblTitle";
        lblTitle.TabIndex = 0;
        lblTitle.Text = "📥📤 Nhập / Xuất kho thủ công";
        // 
        // inputs
        // 
        inputs.AutoSize = true;
        inputs.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        inputs.ColumnCount = 3;
        inputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
        inputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
        inputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
        inputs.Controls.Add(lblOrder, 0, 0);
        inputs.Controls.Add(lblCargoLine, 1, 0);
        inputs.Controls.Add(lblQty, 2, 0);
        inputs.Controls.Add(cboOrder, 0, 1);
        inputs.Controls.Add(cboCargoLine, 1, 1);
        inputs.Controls.Add(numQty, 2, 1);
        inputs.Dock = System.Windows.Forms.DockStyle.Fill;
        inputs.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
        inputs.Name = "inputs";
        inputs.RowCount = 2;
        inputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        inputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        inputs.TabIndex = 1;
        // 
        // lblOrder
        // 
        lblOrder.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblOrder.AutoSize = true;
        lblOrder.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblOrder.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblOrder.Margin = new System.Windows.Forms.Padding(0, 0, 12, 4);
        lblOrder.Name = "lblOrder";
        lblOrder.TabIndex = 0;
        lblOrder.Text = "Đơn vận chuyển";
        // 
        // lblCargoLine
        // 
        lblCargoLine.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblCargoLine.AutoSize = true;
        lblCargoLine.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblCargoLine.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblCargoLine.Margin = new System.Windows.Forms.Padding(0, 0, 12, 4);
        lblCargoLine.Name = "lblCargoLine";
        lblCargoLine.TabIndex = 1;
        lblCargoLine.Text = "Dòng hàng";
        // 
        // lblQty
        // 
        lblQty.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblQty.AutoSize = true;
        lblQty.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblQty.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblQty.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
        lblQty.Name = "lblQty";
        lblQty.TabIndex = 2;
        lblQty.Text = "Số lượng";
        // 
        // cboOrder
        // 
        cboOrder.Dock = System.Windows.Forms.DockStyle.Fill;
        cboOrder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        cboOrder.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
        cboOrder.Name = "cboOrder";
        cboOrder.TabIndex = 3;
        // 
        // cboCargoLine
        // 
        cboCargoLine.Dock = System.Windows.Forms.DockStyle.Fill;
        cboCargoLine.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        cboCargoLine.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
        cboCargoLine.Name = "cboCargoLine";
        cboCargoLine.TabIndex = 4;
        // 
        // numQty
        // 
        numQty.Dock = System.Windows.Forms.DockStyle.Fill;
        numQty.DecimalPlaces = 2;
        numQty.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
        numQty.Margin = new System.Windows.Forms.Padding(0);
        numQty.Name = "numQty";
        numQty.TabIndex = 5;
        // 
        // lblFlowTitle
        // 
        lblFlowTitle.AutoSize = true;
        lblFlowTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        lblFlowTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
        lblFlowTitle.Name = "lblFlowTitle";
        lblFlowTitle.TabIndex = 2;
        lblFlowTitle.Text = "📊 Tình trạng hàng của đơn (dòng hàng đang chọn) và từng chuyến";
        // 
        // lblFlowSummary
        // 
        lblFlowSummary.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblFlowSummary.AutoSize = true;
        lblFlowSummary.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
        lblFlowSummary.Font = new System.Drawing.Font("Segoe UI", 10F);
        lblFlowSummary.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
        lblFlowSummary.Name = "lblFlowSummary";
        lblFlowSummary.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
        lblFlowSummary.TabIndex = 3;
        lblFlowSummary.Text = "Chọn 1 đơn để xem.";
        // 
        // gridTrips
        // 
        gridTrips.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        gridTrips.Dock = System.Windows.Forms.DockStyle.Fill;
        gridTrips.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
        gridTrips.Name = "gridTrips";
        gridTrips.TabIndex = 4;
        // 
        // lblBinsTitle
        // 
        lblBinsTitle.AutoSize = true;
        lblBinsTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        lblBinsTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
        lblBinsTitle.Name = "lblBinsTitle";
        lblBinsTitle.TabIndex = 5;
        lblBinsTitle.Text = "Chọn vị trí nhập (tùy chọn — để trống = tự động Best-Fit)";
        // 
        // gridBins
        // 
        gridBins.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        gridBins.Dock = System.Windows.Forms.DockStyle.Fill;
        gridBins.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
        gridBins.MultiSelect = false;
        gridBins.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        gridBins.Name = "gridBins";
        gridBins.TabIndex = 6;
        // 
        // actionBar
        // 
        actionBar.AutoSize = true;
        actionBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        actionBar.ColumnCount = 3;
        actionBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        actionBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        actionBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        actionBar.Controls.Add(btnReceive, 0, 0);
        actionBar.Controls.Add(btnIssue, 1, 0);
        actionBar.Controls.Add(lblStatus, 2, 0);
        actionBar.Dock = System.Windows.Forms.DockStyle.Fill;
        actionBar.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
        actionBar.Name = "actionBar";
        actionBar.RowCount = 1;
        actionBar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        actionBar.TabIndex = 7;
        // 
        // btnReceive
        // 
        btnReceive.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnReceive.AutoSize = true;
        btnReceive.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        btnReceive.BackColor = System.Drawing.Color.FromArgb(22, 163, 74);
        btnReceive.Cursor = System.Windows.Forms.Cursors.Hand;
        btnReceive.FlatAppearance.BorderSize = 0;
        btnReceive.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnReceive.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        btnReceive.ForeColor = System.Drawing.Color.White;
        btnReceive.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
        btnReceive.MinimumSize = new System.Drawing.Size(0, 38);
        btnReceive.Name = "btnReceive";
        btnReceive.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        btnReceive.TabIndex = 0;
        btnReceive.Text = "📥 Nhập kho";
        btnReceive.UseVisualStyleBackColor = false;
        btnReceive.Click += new System.EventHandler(this.BtnReceive_Click);
        // 
        // btnIssue
        // 
        btnIssue.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnIssue.AutoSize = true;
        btnIssue.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        btnIssue.BackColor = System.Drawing.Color.FromArgb(217, 119, 6);
        btnIssue.Cursor = System.Windows.Forms.Cursors.Hand;
        btnIssue.FlatAppearance.BorderSize = 0;
        btnIssue.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnIssue.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        btnIssue.ForeColor = System.Drawing.Color.White;
        btnIssue.Margin = new System.Windows.Forms.Padding(0, 0, 16, 0);
        btnIssue.MinimumSize = new System.Drawing.Size(0, 38);
        btnIssue.Name = "btnIssue";
        btnIssue.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        btnIssue.TabIndex = 1;
        btnIssue.Text = "📤 Xuất kho";
        btnIssue.UseVisualStyleBackColor = false;
        btnIssue.Click += new System.EventHandler(this.BtnIssue_Click);
        // 
        // lblStatus
        // 
        lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblStatus.AutoSize = true;
        lblStatus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        lblStatus.Margin = new System.Windows.Forms.Padding(0);
        lblStatus.Name = "lblStatus";
        lblStatus.TabIndex = 2;
        // 
        // historyBar
        // 
        historyBar.AutoSize = true;
        historyBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        historyBar.ColumnCount = 3;
        historyBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        historyBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        historyBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        historyBar.Controls.Add(lblHistoryTitle, 0, 0);
        historyBar.Controls.Add(chkAllOrders, 1, 0);
        historyBar.Controls.Add(btnReverse, 2, 0);
        historyBar.Dock = System.Windows.Forms.DockStyle.Fill;
        historyBar.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
        historyBar.Name = "historyBar";
        historyBar.RowCount = 1;
        historyBar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        historyBar.TabIndex = 8;
        // 
        // lblHistoryTitle
        // 
        lblHistoryTitle.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblHistoryTitle.AutoSize = true;
        lblHistoryTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        lblHistoryTitle.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
        lblHistoryTitle.Name = "lblHistoryTitle";
        lblHistoryTitle.TabIndex = 0;
        lblHistoryTitle.Text = "Lịch sử nhập/xuất kho";
        // 
        // chkAllOrders
        // 
        chkAllOrders.Anchor = System.Windows.Forms.AnchorStyles.Left;
        chkAllOrders.AutoSize = true;
        chkAllOrders.Font = new System.Drawing.Font("Segoe UI", 9F);
        chkAllOrders.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
        chkAllOrders.Name = "chkAllOrders";
        chkAllOrders.TabIndex = 1;
        chkAllOrders.Text = "Hiện tất cả đơn (bỏ lọc theo đơn)";
        chkAllOrders.UseVisualStyleBackColor = true;
        // 
        // btnReverse
        // 
        btnReverse.Anchor = System.Windows.Forms.AnchorStyles.Left;
        btnReverse.AutoSize = true;
        btnReverse.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        btnReverse.BackColor = System.Drawing.Color.White;
        btnReverse.Cursor = System.Windows.Forms.Cursors.Hand;
        btnReverse.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(220, 38, 38);
        btnReverse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnReverse.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        btnReverse.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
        btnReverse.Margin = new System.Windows.Forms.Padding(0);
        btnReverse.MinimumSize = new System.Drawing.Size(0, 34);
        btnReverse.Name = "btnReverse";
        btnReverse.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
        btnReverse.TabIndex = 2;
        btnReverse.Text = "↩️ Hoàn tác phiếu xuất tay";
        btnReverse.UseVisualStyleBackColor = false;
        btnReverse.Click += new System.EventHandler(this.BtnReverse_Click);
        // 
        // historyFilter — lọc lịch sử nhập/xuất (Nhập / Xuất, nguồn, tìm đơn / loại hàng / người)
        //
        historyFilter.Dock = System.Windows.Forms.DockStyle.Fill;
        historyFilter.Name = "historyFilter";
        historyFilter.SearchPlaceholder = "Mã đơn, loại hàng, người, ghi chú...";
        historyFilter.TabIndex = 9;
        //
        // gridHistory
        // 
        gridHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        gridHistory.Dock = System.Windows.Forms.DockStyle.Fill;
        gridHistory.Margin = new System.Windows.Forms.Padding(0);
        gridHistory.Name = "gridHistory";
        gridHistory.TabIndex = 9;
        // 
        // ManualStockForm
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.White;
        ClientSize = new System.Drawing.Size(1100, 760);
        Controls.Add(root);
        MinimumSize = new System.Drawing.Size(760, 500);
        Name = "ManualStockForm";
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        Text = "Nhập / Xuất kho thủ công";
        ((System.ComponentModel.ISupportInitialize)numQty).EndInit();
        ((System.ComponentModel.ISupportInitialize)gridTrips).EndInit();
        ((System.ComponentModel.ISupportInitialize)gridBins).EndInit();
        ((System.ComponentModel.ISupportInitialize)gridHistory).EndInit();
        historyBar.ResumeLayout(false);
        historyBar.PerformLayout();
        actionBar.ResumeLayout(false);
        actionBar.PerformLayout();
        inputs.ResumeLayout(false);
        inputs.PerformLayout();
        root.ResumeLayout(false);
        root.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel root;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.TableLayoutPanel inputs;
    private System.Windows.Forms.Label lblOrder;
    private System.Windows.Forms.Label lblCargoLine;
    private System.Windows.Forms.Label lblQty;
    private System.Windows.Forms.ComboBox cboOrder;
    private System.Windows.Forms.ComboBox cboCargoLine;
    private TruckingSystem.Desktop.Common.NumberBox numQty;
    private System.Windows.Forms.Label lblFlowTitle;
    private System.Windows.Forms.Label lblFlowSummary;
    private System.Windows.Forms.DataGridView gridTrips;
    private System.Windows.Forms.Label lblBinsTitle;
    private System.Windows.Forms.DataGridView gridBins;
    private System.Windows.Forms.TableLayoutPanel actionBar;
    private System.Windows.Forms.Button btnReceive;
    private System.Windows.Forms.Button btnIssue;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.TableLayoutPanel historyBar;
    private System.Windows.Forms.Label lblHistoryTitle;
    private System.Windows.Forms.CheckBox chkAllOrders;
    private System.Windows.Forms.Button btnReverse;
    private TruckingSystem.Desktop.Common.FilterBar historyFilter;
    private System.Windows.Forms.DataGridView gridHistory;
}
