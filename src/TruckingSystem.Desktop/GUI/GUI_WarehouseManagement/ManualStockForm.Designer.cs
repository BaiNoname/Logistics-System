namespace TruckingSystem.Desktop.GUI_WarehouseManagement
{
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
            this.lblBinsTitle = new System.Windows.Forms.Label();
            this.gridBins = new System.Windows.Forms.DataGridView();
            this.btnReceive = new System.Windows.Forms.Button();
            this.btnIssue = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblHistoryTitle = new System.Windows.Forms.Label();
            this.gridHistory = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.numQty)).BeginInit();
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
            // lblOrder
            //
            this.lblOrder.Location = new System.Drawing.Point(24, 56);
            this.lblOrder.Size = new System.Drawing.Size(200, 18);
            this.lblOrder.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblOrder.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblOrder.Text = "Đơn vận chuyển";
            //
            // cboOrder
            //
            this.cboOrder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboOrder.Location = new System.Drawing.Point(24, 76);
            this.cboOrder.Size = new System.Drawing.Size(260, 23);
            //
            // lblCargoLine
            //
            this.lblCargoLine.Location = new System.Drawing.Point(304, 56);
            this.lblCargoLine.Size = new System.Drawing.Size(260, 18);
            this.lblCargoLine.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblCargoLine.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblCargoLine.Text = "Dòng hàng";
            //
            // cboCargoLine
            //
            this.cboCargoLine.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCargoLine.Location = new System.Drawing.Point(304, 76);
            this.cboCargoLine.Size = new System.Drawing.Size(320, 23);
            //
            // lblQty
            //
            this.lblQty.Location = new System.Drawing.Point(650, 56);
            this.lblQty.Size = new System.Drawing.Size(120, 18);
            this.lblQty.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblQty.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblQty.Text = "Số lượng";
            //
            // numQty
            //
            this.numQty.Location = new System.Drawing.Point(650, 76);
            this.numQty.Size = new System.Drawing.Size(100, 23);
            this.numQty.DecimalPlaces = 2;
            this.numQty.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
            //
            // lblBinsTitle
            //
            this.lblBinsTitle.Location = new System.Drawing.Point(24, 112);
            this.lblBinsTitle.AutoSize = true;
            this.lblBinsTitle.Text = "Chọn vị trí nhập (tùy chọn — để trống = tự động Best-Fit)";
            this.lblBinsTitle.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            //
            // gridBins
            //
            this.gridBins.Location = new System.Drawing.Point(24, 135);
            this.gridBins.Size = new System.Drawing.Size(900, 160);
            this.gridBins.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.gridBins.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridBins.MultiSelect = false;
            //
            // btnReceive
            //
            this.btnReceive.Location = new System.Drawing.Point(24, 310);
            this.btnReceive.Size = new System.Drawing.Size(160, 38);
            this.btnReceive.Text = "📥 Nhập kho";
            this.btnReceive.BackColor = TruckingSystem.Desktop.Common.Theme.Success;
            this.btnReceive.ForeColor = System.Drawing.Color.White;
            this.btnReceive.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReceive.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.btnReceive.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnReceive.FlatAppearance.BorderSize = 0;
            this.btnReceive.Click += new System.EventHandler(this.BtnReceive_Click);
            //
            // btnIssue
            //
            this.btnIssue.Location = new System.Drawing.Point(194, 310);
            this.btnIssue.Size = new System.Drawing.Size(160, 38);
            this.btnIssue.Text = "📤 Xuất kho";
            this.btnIssue.BackColor = TruckingSystem.Desktop.Common.Theme.Warning;
            this.btnIssue.ForeColor = System.Drawing.Color.White;
            this.btnIssue.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIssue.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.btnIssue.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnIssue.FlatAppearance.BorderSize = 0;
            this.btnIssue.Click += new System.EventHandler(this.BtnIssue_Click);
            //
            // lblStatus
            //
            this.lblStatus.Location = new System.Drawing.Point(370, 315);
            this.lblStatus.Size = new System.Drawing.Size(550, 30);
            this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            //
            // lblHistoryTitle
            //
            this.lblHistoryTitle.Location = new System.Drawing.Point(24, 365);
            this.lblHistoryTitle.AutoSize = true;
            this.lblHistoryTitle.Text = "Lịch sử nhập/xuất kho";
            this.lblHistoryTitle.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            //
            // gridHistory
            //
            this.gridHistory.Location = new System.Drawing.Point(24, 390);
            this.gridHistory.Size = new System.Drawing.Size(900, 280);
            this.gridHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            //
            // ManualStockForm
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(950, 700);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblOrder);
            this.Controls.Add(this.cboOrder);
            this.Controls.Add(this.lblCargoLine);
            this.Controls.Add(this.cboCargoLine);
            this.Controls.Add(this.lblQty);
            this.Controls.Add(this.numQty);
            this.Controls.Add(this.lblBinsTitle);
            this.Controls.Add(this.gridBins);
            this.Controls.Add(this.btnReceive);
            this.Controls.Add(this.btnIssue);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblHistoryTitle);
            this.Controls.Add(this.gridHistory);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "ManualStockForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Nhập / Xuất kho thủ công";
            ((System.ComponentModel.ISupportInitialize)(this.numQty)).EndInit();
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
        private System.Windows.Forms.Label lblBinsTitle;
        private System.Windows.Forms.DataGridView gridBins;
        private System.Windows.Forms.Button btnReceive;
        private System.Windows.Forms.Button btnIssue;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblHistoryTitle;
        private System.Windows.Forms.DataGridView gridHistory;
    }
}
