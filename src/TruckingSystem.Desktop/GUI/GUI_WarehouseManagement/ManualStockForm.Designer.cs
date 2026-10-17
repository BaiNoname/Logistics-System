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
        root = new TableLayoutPanel();
        lblTitle = new Label();
        inputs = new TableLayoutPanel();
        lblOrder = new Label();
        lblCargoLine = new Label();
        lblQty = new Label();
        cboOrder = new ComboBox();
        cboCargoLine = new ComboBox();
        numQty = new NumericUpDown();
        lblFlowTitle = new Label();
        lblFlowSummary = new Label();
        gridTrips = new DataGridView();
        lblBinsTitle = new Label();
        gridBins = new DataGridView();
        actionBar = new TableLayoutPanel();
        btnReceive = new Button();
        btnIssue = new Button();
        lblStatus = new Label();
        historyBar = new TableLayoutPanel();
        lblHistoryTitle = new Label();
        chkAllOrders = new CheckBox();
        btnReverse = new Button();
        historyFilter = new TruckingSystem.Desktop.Common.FilterBar();
        gridHistory = new DataGridView();
        root.SuspendLayout();
        inputs.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numQty).BeginInit();
        ((System.ComponentModel.ISupportInitialize)gridTrips).BeginInit();
        ((System.ComponentModel.ISupportInitialize)gridBins).BeginInit();
        actionBar.SuspendLayout();
        historyBar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridHistory).BeginInit();
        SuspendLayout();
        // 
        // root
        // 
        root.ColumnCount = 1;
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
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
        root.Dock = DockStyle.Fill;
        root.Location = new Point(0, 0);
        root.Margin = new Padding(0);
        root.Name = "root";
        root.Padding = new Padding(30, 24, 30, 24);
        root.RowCount = 11;
        root.RowStyles.Add(new RowStyle());
        root.RowStyles.Add(new RowStyle());
        root.RowStyles.Add(new RowStyle());
        root.RowStyles.Add(new RowStyle());
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        root.RowStyles.Add(new RowStyle());
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        root.RowStyles.Add(new RowStyle());
        root.RowStyles.Add(new RowStyle());
        root.RowStyles.Add(new RowStyle());
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        root.Size = new Size(1650, 1140);
        root.TabIndex = 0;
        // 
        // lblTitle
        // 
        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
        lblTitle.Location = new Point(30, 24);
        lblTitle.Margin = new Padding(0, 0, 0, 15);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(416, 36);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "📥📤 Nhập / Xuất kho thủ công";
        // 
        // inputs
        // 
        inputs.AutoSize = true;
        inputs.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        inputs.ColumnCount = 3;
        inputs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
        inputs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
        inputs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
        inputs.Controls.Add(lblOrder, 0, 0);
        inputs.Controls.Add(lblCargoLine, 1, 0);
        inputs.Controls.Add(lblQty, 2, 0);
        inputs.Controls.Add(cboOrder, 0, 1);
        inputs.Controls.Add(cboCargoLine, 1, 1);
        inputs.Controls.Add(numQty, 2, 1);
        inputs.Dock = DockStyle.Fill;
        inputs.Location = new Point(30, 75);
        inputs.Margin = new Padding(0, 0, 0, 12);
        inputs.Name = "inputs";
        inputs.RowCount = 2;
        inputs.RowStyles.Add(new RowStyle());
        inputs.RowStyles.Add(new RowStyle());
        inputs.Size = new Size(1590, 67);
        inputs.TabIndex = 1;
        // 
        // lblOrder
        // 
        lblOrder.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblOrder.AutoSize = true;
        lblOrder.Font = new Font("Segoe UI", 9F);
        lblOrder.ForeColor = Color.FromArgb(100, 116, 139);
        lblOrder.Location = new Point(0, 0);
        lblOrder.Margin = new Padding(0, 0, 18, 6);
        lblOrder.Name = "lblOrder";
        lblOrder.Size = new Size(618, 25);
        lblOrder.TabIndex = 0;
        lblOrder.Text = "Đơn vận chuyển";
        // 
        // lblCargoLine
        // 
        lblCargoLine.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblCargoLine.AutoSize = true;
        lblCargoLine.Font = new Font("Segoe UI", 9F);
        lblCargoLine.ForeColor = Color.FromArgb(100, 116, 139);
        lblCargoLine.Location = new Point(636, 0);
        lblCargoLine.Margin = new Padding(0, 0, 18, 6);
        lblCargoLine.Name = "lblCargoLine";
        lblCargoLine.Size = new Size(697, 25);
        lblCargoLine.TabIndex = 1;
        lblCargoLine.Text = "Dòng hàng";
        // 
        // lblQty
        // 
        lblQty.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblQty.AutoSize = true;
        lblQty.Font = new Font("Segoe UI", 9F);
        lblQty.ForeColor = Color.FromArgb(100, 116, 139);
        lblQty.Location = new Point(1351, 0);
        lblQty.Margin = new Padding(0, 0, 0, 6);
        lblQty.Name = "lblQty";
        lblQty.Size = new Size(239, 25);
        lblQty.TabIndex = 2;
        lblQty.Text = "Số lượng";
        // 
        // cboOrder
        // 
        cboOrder.Dock = DockStyle.Fill;
        cboOrder.DropDownStyle = ComboBoxStyle.DropDownList;
        cboOrder.Location = new Point(0, 31);
        cboOrder.Margin = new Padding(0, 0, 18, 0);
        cboOrder.Name = "cboOrder";
        cboOrder.Size = new Size(618, 36);
        cboOrder.TabIndex = 3;
        // 
        // cboCargoLine
        // 
        cboCargoLine.Dock = DockStyle.Fill;
        cboCargoLine.DropDownStyle = ComboBoxStyle.DropDownList;
        cboCargoLine.Location = new Point(636, 31);
        cboCargoLine.Margin = new Padding(0, 0, 18, 0);
        cboCargoLine.Name = "cboCargoLine";
        cboCargoLine.Size = new Size(697, 36);
        cboCargoLine.TabIndex = 4;
        // 
        // numQty
        // 
        numQty.DecimalPlaces = 2;
        numQty.Dock = DockStyle.Fill;
        numQty.Location = new Point(1351, 31);
        numQty.Margin = new Padding(0);
        numQty.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
        numQty.Name = "numQty";
        numQty.Size = new Size(239, 34);
        numQty.TabIndex = 5;
        // 
        // lblFlowTitle
        // 
        lblFlowTitle.AutoSize = true;
        lblFlowTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblFlowTitle.Location = new Point(30, 154);
        lblFlowTitle.Margin = new Padding(0, 0, 0, 6);
        lblFlowTitle.Name = "lblFlowTitle";
        lblFlowTitle.Size = new Size(661, 28);
        lblFlowTitle.TabIndex = 2;
        lblFlowTitle.Text = "📊 Tình trạng hàng của đơn (dòng hàng đang chọn) và từng chuyến";
        // 
        // lblFlowSummary
        // 
        lblFlowSummary.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblFlowSummary.AutoSize = true;
        lblFlowSummary.BackColor = Color.FromArgb(241, 245, 249);
        lblFlowSummary.Font = new Font("Segoe UI", 10F);
        lblFlowSummary.Location = new Point(30, 188);
        lblFlowSummary.Margin = new Padding(0, 0, 0, 9);
        lblFlowSummary.Name = "lblFlowSummary";
        lblFlowSummary.Padding = new Padding(12, 6, 12, 6);
        lblFlowSummary.Size = new Size(1590, 40);
        lblFlowSummary.TabIndex = 3;
        lblFlowSummary.Text = "Chọn 1 đơn để xem.";
        // 
        // gridTrips
        // 
        gridTrips.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        gridTrips.Dock = DockStyle.Fill;
        gridTrips.Location = new Point(30, 237);
        gridTrips.Margin = new Padding(0, 0, 0, 15);
        gridTrips.Name = "gridTrips";
        gridTrips.RowHeadersWidth = 62;
        gridTrips.Size = new Size(1590, 147);
        gridTrips.TabIndex = 4;
        // 
        // lblBinsTitle
        // 
        lblBinsTitle.AutoSize = true;
        lblBinsTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblBinsTitle.Location = new Point(30, 399);
        lblBinsTitle.Margin = new Padding(0, 0, 0, 6);
        lblBinsTitle.Name = "lblBinsTitle";
        lblBinsTitle.Size = new Size(561, 28);
        lblBinsTitle.TabIndex = 5;
        lblBinsTitle.Text = "Chọn vị trí nhập (tùy chọn — để trống = tự động Best-Fit)";
        // 
        // gridBins
        // 
        gridBins.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        gridBins.Dock = DockStyle.Fill;
        gridBins.Location = new Point(30, 433);
        gridBins.Margin = new Padding(0, 0, 0, 15);
        gridBins.MultiSelect = false;
        gridBins.Name = "gridBins";
        gridBins.RowHeadersWidth = 62;
        gridBins.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        gridBins.Size = new Size(1590, 147);
        gridBins.TabIndex = 6;
        // 
        // actionBar
        // 
        actionBar.AutoSize = true;
        actionBar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        actionBar.ColumnCount = 3;
        actionBar.ColumnStyles.Add(new ColumnStyle());
        actionBar.ColumnStyles.Add(new ColumnStyle());
        actionBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        actionBar.Controls.Add(btnReceive, 0, 0);
        actionBar.Controls.Add(btnIssue, 1, 0);
        actionBar.Controls.Add(lblStatus, 2, 0);
        actionBar.Dock = DockStyle.Fill;
        actionBar.Location = new Point(30, 595);
        actionBar.Margin = new Padding(0, 0, 0, 15);
        actionBar.Name = "actionBar";
        actionBar.RowCount = 1;
        actionBar.RowStyles.Add(new RowStyle());
        actionBar.Size = new Size(1590, 57);
        actionBar.TabIndex = 7;
        // 
        // btnReceive
        // 
        btnReceive.Anchor = AnchorStyles.Left;
        btnReceive.AutoSize = true;
        btnReceive.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        btnReceive.BackColor = Color.FromArgb(22, 163, 74);
        btnReceive.Cursor = Cursors.Hand;
        btnReceive.FlatAppearance.BorderSize = 0;
        btnReceive.FlatStyle = FlatStyle.Flat;
        btnReceive.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnReceive.ForeColor = Color.White;
        btnReceive.Location = new Point(0, 0);
        btnReceive.Margin = new Padding(0, 0, 12, 0);
        btnReceive.MinimumSize = new Size(0, 57);
        btnReceive.Name = "btnReceive";
        btnReceive.Padding = new Padding(15, 6, 15, 6);
        btnReceive.Size = new Size(178, 57);
        btnReceive.TabIndex = 0;
        btnReceive.Text = "📥 Nhập kho";
        btnReceive.UseVisualStyleBackColor = false;
        btnReceive.Click += BtnReceive_Click;
        // 
        // btnIssue
        // 
        btnIssue.Anchor = AnchorStyles.Left;
        btnIssue.AutoSize = true;
        btnIssue.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        btnIssue.BackColor = Color.FromArgb(217, 119, 6);
        btnIssue.Cursor = Cursors.Hand;
        btnIssue.FlatAppearance.BorderSize = 0;
        btnIssue.FlatStyle = FlatStyle.Flat;
        btnIssue.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnIssue.ForeColor = Color.White;
        btnIssue.Location = new Point(190, 0);
        btnIssue.Margin = new Padding(0, 0, 24, 0);
        btnIssue.MinimumSize = new Size(0, 57);
        btnIssue.Name = "btnIssue";
        btnIssue.Padding = new Padding(15, 6, 15, 6);
        btnIssue.Size = new Size(171, 57);
        btnIssue.TabIndex = 1;
        btnIssue.Text = "📤 Xuất kho";
        btnIssue.UseVisualStyleBackColor = false;
        btnIssue.Click += BtnIssue_Click;
        // 
        // lblStatus
        // 
        lblStatus.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblStatus.AutoSize = true;
        lblStatus.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblStatus.Location = new Point(385, 14);
        lblStatus.Margin = new Padding(0);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(1205, 28);
        lblStatus.TabIndex = 2;
        // 
        // historyBar
        // 
        historyBar.AutoSize = true;
        historyBar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        historyBar.ColumnCount = 3;
        historyBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        historyBar.ColumnStyles.Add(new ColumnStyle());
        historyBar.ColumnStyles.Add(new ColumnStyle());
        historyBar.Controls.Add(lblHistoryTitle, 0, 0);
        historyBar.Controls.Add(chkAllOrders, 1, 0);
        historyBar.Controls.Add(btnReverse, 2, 0);
        historyBar.Dock = DockStyle.Fill;
        historyBar.Location = new Point(30, 667);
        historyBar.Margin = new Padding(0, 0, 0, 9);
        historyBar.Name = "historyBar";
        historyBar.RowCount = 1;
        historyBar.RowStyles.Add(new RowStyle());
        historyBar.Size = new Size(1590, 51);
        historyBar.TabIndex = 8;
        // 
        // lblHistoryTitle
        // 
        lblHistoryTitle.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblHistoryTitle.AutoSize = true;
        lblHistoryTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblHistoryTitle.Location = new Point(0, 11);
        lblHistoryTitle.Margin = new Padding(0, 0, 18, 0);
        lblHistoryTitle.Name = "lblHistoryTitle";
        lblHistoryTitle.Size = new Size(946, 28);
        lblHistoryTitle.TabIndex = 0;
        lblHistoryTitle.Text = "Lịch sử nhập/xuất kho";
        // 
        // chkAllOrders
        // 
        chkAllOrders.Anchor = AnchorStyles.Left;
        chkAllOrders.AutoSize = true;
        chkAllOrders.Font = new Font("Segoe UI", 9F);
        chkAllOrders.Location = new Point(964, 11);
        chkAllOrders.Margin = new Padding(0, 0, 18, 0);
        chkAllOrders.Name = "chkAllOrders";
        chkAllOrders.Size = new Size(302, 29);
        chkAllOrders.TabIndex = 1;
        chkAllOrders.Text = "Hiện tất cả đơn (bỏ lọc theo đơn)";
        chkAllOrders.UseVisualStyleBackColor = true;
        // 
        // btnReverse
        // 
        btnReverse.Anchor = AnchorStyles.Left;
        btnReverse.AutoSize = true;
        btnReverse.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        btnReverse.BackColor = Color.White;
        btnReverse.Cursor = Cursors.Hand;
        btnReverse.FlatAppearance.BorderColor = Color.FromArgb(220, 38, 38);
        btnReverse.FlatStyle = FlatStyle.Flat;
        btnReverse.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnReverse.ForeColor = Color.FromArgb(220, 38, 38);
        btnReverse.Location = new Point(1284, 0);
        btnReverse.Margin = new Padding(0);
        btnReverse.MinimumSize = new Size(0, 51);
        btnReverse.Name = "btnReverse";
        btnReverse.Padding = new Padding(15, 3, 15, 3);
        btnReverse.Size = new Size(306, 51);
        btnReverse.TabIndex = 2;
        btnReverse.Text = "↩️ Hoàn tác phiếu xuất tay";
        btnReverse.UseVisualStyleBackColor = false;
        btnReverse.Click += BtnReverse_Click;
        // 
        // historyFilter
        // 
        historyFilter.AutoSize = true;
        historyFilter.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        historyFilter.Dock = DockStyle.Fill;
        historyFilter.Location = new Point(30, 727);
        historyFilter.Margin = new Padding(0, 0, 0, 9);
        historyFilter.Name = "historyFilter";
        historyFilter.SearchPlaceholder = "Mã đơn, loại hàng, người, ghi chú...";
        historyFilter.Size = new Size(1590, 54);
        historyFilter.TabIndex = 9;
        // 
        // gridHistory
        // 
        gridHistory.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        gridHistory.Dock = DockStyle.Fill;
        gridHistory.Location = new Point(30, 790);
        gridHistory.Margin = new Padding(0);
        gridHistory.Name = "gridHistory";
        gridHistory.RowHeadersWidth = 62;
        gridHistory.Size = new Size(1590, 326);
        gridHistory.TabIndex = 9;
        // 
        // ManualStockForm
        // 
        AutoScaleDimensions = new SizeF(144F, 144F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.White;
        ClientSize = new Size(1650, 1140);
        Controls.Add(root);
        Margin = new Padding(6, 6, 6, 6);
        MinimumSize = new Size(1129, 722);
        Name = "ManualStockForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Nhập / Xuất kho thủ công";
        root.ResumeLayout(false);
        root.PerformLayout();
        inputs.ResumeLayout(false);
        inputs.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)numQty).EndInit();
        ((System.ComponentModel.ISupportInitialize)gridTrips).EndInit();
        ((System.ComponentModel.ISupportInitialize)gridBins).EndInit();
        actionBar.ResumeLayout(false);
        actionBar.PerformLayout();
        historyBar.ResumeLayout(false);
        historyBar.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)gridHistory).EndInit();
        ResumeLayout(false);
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
    private System.Windows.Forms.NumericUpDown numQty;
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
