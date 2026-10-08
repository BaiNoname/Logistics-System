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

    private void InitializeComponent()
    {
        this.lblTitle = new System.Windows.Forms.Label();
        this.lblOrder = new System.Windows.Forms.Label();
        this.cboOrder = new System.Windows.Forms.ComboBox();
        this.lblCargoLine = new System.Windows.Forms.Label();
        this.cboCargoLine = new System.Windows.Forms.ComboBox();
        this.lblQty = new System.Windows.Forms.Label();
        this.numQty = new System.Windows.Forms.NumericUpDown();
        this.lblFlowTitle = new System.Windows.Forms.Label();
        this.lblFlowSummary = new System.Windows.Forms.Label();
        this.gridTrips = new System.Windows.Forms.DataGridView();
        this.lblBinsTitle = new System.Windows.Forms.Label();
        this.gridBins = new System.Windows.Forms.DataGridView();
        this.btnReceive = new System.Windows.Forms.Button();
        this.btnIssue = new System.Windows.Forms.Button();
        this.lblStatus = new System.Windows.Forms.Label();
        this.lblHistoryTitle = new System.Windows.Forms.Label();
        this.chkAllOrders = new System.Windows.Forms.CheckBox();
        this.btnReverse = new System.Windows.Forms.Button();
        this.gridHistory = new System.Windows.Forms.DataGridView();
        ((System.ComponentModel.ISupportInitialize)(this.numQty)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.gridTrips)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.gridBins)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.gridHistory)).BeginInit();
        this.SuspendLayout();
        //
        // lblTitle
        //
        this.lblTitle.Location = new System.Drawing.Point(24, 16);
        this.lblTitle.AutoSize = true;
        this.lblTitle.Text = "📥📤 Nhập / Xuất kho thủ công";
        this.lblTitle.Font = TruckingSystem.Desktop.Common.Theme.FontSectionTitle;
        this.lblTitle.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
        //
        // lblOrder / cboOrder
        //
        this.lblOrder.Location = new System.Drawing.Point(24, 56);
        this.lblOrder.Size = new System.Drawing.Size(300, 18);
        this.lblOrder.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblOrder.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblOrder.Text = "Đơn vận chuyển";
        this.cboOrder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboOrder.Location = new System.Drawing.Point(24, 76);
        this.cboOrder.Size = new System.Drawing.Size(300, 23);
        //
        // lblCargoLine / cboCargoLine
        //
        this.lblCargoLine.Location = new System.Drawing.Point(344, 56);
        this.lblCargoLine.Size = new System.Drawing.Size(380, 18);
        this.lblCargoLine.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblCargoLine.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblCargoLine.Text = "Dòng hàng";
        this.cboCargoLine.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboCargoLine.Location = new System.Drawing.Point(344, 76);
        this.cboCargoLine.Size = new System.Drawing.Size(380, 23);
        //
        // lblQty / numQty
        //
        this.lblQty.Location = new System.Drawing.Point(744, 56);
        this.lblQty.Size = new System.Drawing.Size(140, 18);
        this.lblQty.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblQty.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblQty.Text = "Số lượng";
        this.numQty.Location = new System.Drawing.Point(744, 76);
        this.numQty.Size = new System.Drawing.Size(120, 23);
        this.numQty.DecimalPlaces = 2;
        this.numQty.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
        //
        // lblFlowTitle / lblFlowSummary / gridTrips — TÌNH TRẠNG HÀNG CỦA ĐƠN
        //
        this.lblFlowTitle.Location = new System.Drawing.Point(24, 114);
        this.lblFlowTitle.AutoSize = true;
        this.lblFlowTitle.Text = "📊 Tình trạng hàng của đơn (dòng hàng đang chọn) và từng chuyến";
        this.lblFlowTitle.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
        this.lblFlowSummary.Location = new System.Drawing.Point(24, 138);
        this.lblFlowSummary.Size = new System.Drawing.Size(1130, 46);
        this.lblFlowSummary.Font = TruckingSystem.Desktop.Common.Theme.FontNormal;
        this.lblFlowSummary.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
        this.lblFlowSummary.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
        this.lblFlowSummary.Text = "Chọn 1 đơn để xem.";
        this.gridTrips.Location = new System.Drawing.Point(24, 190);
        this.gridTrips.Size = new System.Drawing.Size(1130, 128);
        this.gridTrips.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        //
        // lblBinsTitle / gridBins
        //
        this.lblBinsTitle.Location = new System.Drawing.Point(24, 330);
        this.lblBinsTitle.AutoSize = true;
        this.lblBinsTitle.Text = "Chọn vị trí nhập (tùy chọn — để trống = tự động Best-Fit)";
        this.lblBinsTitle.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
        this.gridBins.Location = new System.Drawing.Point(24, 352);
        this.gridBins.Size = new System.Drawing.Size(1130, 118);
        this.gridBins.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        this.gridBins.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.gridBins.MultiSelect = false;
        //
        // btnReceive / btnIssue / lblStatus
        //
        this.btnReceive.Location = new System.Drawing.Point(24, 482);
        this.btnReceive.Size = new System.Drawing.Size(160, 38);
        this.btnReceive.Text = "📥 Nhập kho";
        this.btnReceive.BackColor = TruckingSystem.Desktop.Common.Theme.Success;
        this.btnReceive.ForeColor = System.Drawing.Color.White;
        this.btnReceive.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnReceive.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
        this.btnReceive.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnReceive.FlatAppearance.BorderSize = 0;
        this.btnReceive.Click += new System.EventHandler(this.BtnReceive_Click);
        this.btnIssue.Location = new System.Drawing.Point(194, 482);
        this.btnIssue.Size = new System.Drawing.Size(160, 38);
        this.btnIssue.Text = "📤 Xuất kho";
        this.btnIssue.BackColor = TruckingSystem.Desktop.Common.Theme.Warning;
        this.btnIssue.ForeColor = System.Drawing.Color.White;
        this.btnIssue.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnIssue.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
        this.btnIssue.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnIssue.FlatAppearance.BorderSize = 0;
        this.btnIssue.Click += new System.EventHandler(this.BtnIssue_Click);
        this.lblStatus.Location = new System.Drawing.Point(370, 478);
        this.lblStatus.Size = new System.Drawing.Size(784, 46);
        this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
        //
        // lblHistoryTitle / chkAllOrders / btnReverse / gridHistory
        //
        this.lblHistoryTitle.Location = new System.Drawing.Point(24, 540);
        this.lblHistoryTitle.AutoSize = true;
        this.lblHistoryTitle.Text = "Lịch sử nhập/xuất kho";
        this.lblHistoryTitle.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
        this.chkAllOrders.Location = new System.Drawing.Point(640, 538);
        this.chkAllOrders.Size = new System.Drawing.Size(250, 24);
        this.chkAllOrders.Text = "Hiện tất cả đơn (bỏ lọc theo đơn)";
        this.chkAllOrders.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.btnReverse.Location = new System.Drawing.Point(904, 532);
        this.btnReverse.Size = new System.Drawing.Size(250, 32);
        this.btnReverse.Text = "↩️ Hoàn tác phiếu xuất tay";
        this.btnReverse.BackColor = System.Drawing.Color.White;
        this.btnReverse.ForeColor = TruckingSystem.Desktop.Common.Theme.Danger;
        this.btnReverse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnReverse.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
        this.btnReverse.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnReverse.FlatAppearance.BorderColor = TruckingSystem.Desktop.Common.Theme.Danger;
        this.btnReverse.Click += new System.EventHandler(this.BtnReverse_Click);
        this.gridHistory.Location = new System.Drawing.Point(24, 570);
        this.gridHistory.Size = new System.Drawing.Size(1130, 270);
        this.gridHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        //
        // ManualStockForm
        //
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.BackColor = System.Drawing.Color.White;
        this.ClientSize = new System.Drawing.Size(1180, 860);
        this.Controls.Add(this.lblTitle);
        this.Controls.Add(this.lblOrder);
        this.Controls.Add(this.cboOrder);
        this.Controls.Add(this.lblCargoLine);
        this.Controls.Add(this.cboCargoLine);
        this.Controls.Add(this.lblQty);
        this.Controls.Add(this.numQty);
        this.Controls.Add(this.lblFlowTitle);
        this.Controls.Add(this.lblFlowSummary);
        this.Controls.Add(this.gridTrips);
        this.Controls.Add(this.lblBinsTitle);
        this.Controls.Add(this.gridBins);
        this.Controls.Add(this.btnReceive);
        this.Controls.Add(this.btnIssue);
        this.Controls.Add(this.lblStatus);
        this.Controls.Add(this.lblHistoryTitle);
        this.Controls.Add(this.chkAllOrders);
        this.Controls.Add(this.btnReverse);
        this.Controls.Add(this.gridHistory);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.Name = "ManualStockForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Nhập / Xuất kho thủ công";
        ((System.ComponentModel.ISupportInitialize)(this.numQty)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.gridTrips)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.gridBins)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.gridHistory)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Label lblOrder;
    private System.Windows.Forms.ComboBox cboOrder;
    private System.Windows.Forms.Label lblCargoLine;
    private System.Windows.Forms.ComboBox cboCargoLine;
    private System.Windows.Forms.Label lblQty;
    private System.Windows.Forms.NumericUpDown numQty;
    private System.Windows.Forms.Label lblFlowTitle;
    private System.Windows.Forms.Label lblFlowSummary;
    private System.Windows.Forms.DataGridView gridTrips;
    private System.Windows.Forms.Label lblBinsTitle;
    private System.Windows.Forms.DataGridView gridBins;
    private System.Windows.Forms.Button btnReceive;
    private System.Windows.Forms.Button btnIssue;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.Label lblHistoryTitle;
    private System.Windows.Forms.CheckBox chkAllOrders;
    private System.Windows.Forms.Button btnReverse;
    private System.Windows.Forms.DataGridView gridHistory;
}
