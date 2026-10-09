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

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblInfo = new System.Windows.Forms.Label();
            this.gridPending = new System.Windows.Forms.DataGridView();
            this.btnAutoTrip = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.lblManualTitle = new System.Windows.Forms.Label();
            this.lblQty = new System.Windows.Forms.Label();
            this.numQty = new System.Windows.Forms.NumericUpDown();
            this.lblManualHint = new System.Windows.Forms.Label();
            this.gridBins = new System.Windows.Forms.DataGridView();
            this.btnManual = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.gridPending)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numQty)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridBins)).BeginInit();
            this.SuspendLayout();
            //
            // lblTitle
            //
            this.lblTitle.Location = new System.Drawing.Point(16, 12);
            this.lblTitle.Size = new System.Drawing.Size(1040, 30);
            this.lblTitle.Text = "📦 Hàng chờ nhập kho";
            this.lblTitle.Font = TruckingSystem.Desktop.Common.Theme.FontSectionTitle;
            this.lblTitle.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
            //
            // lblInfo
            //
            this.lblInfo.Location = new System.Drawing.Point(18, 44);
            this.lblInfo.Size = new System.Drawing.Size(1040, 40);
            this.lblInfo.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblInfo.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblInfo.Text = "Xe Lấy hàng đã về kho (đã duyệt) nhưng hàng CHƯA lên kệ. Chọn 1 dòng rồi nhập: TỰ ĐỘNG cả chuyến theo Best-Fit, " +
                "hoặc THỦ CÔNG — tự chọn vị trí + số lượng (nhập nhiều lần để chia nhiều vị trí). Chặng Giao của đơn chỉ bắt đầu được khi hàng đã nhập kho đủ.";
            //
            // gridPending
            //
            this.gridPending.Location = new System.Drawing.Point(16, 88);
            this.gridPending.Size = new System.Drawing.Size(1040, 250);
            this.gridPending.Name = "gridPending";
            //
            // btnAutoTrip
            //
            this.btnAutoTrip.Location = new System.Drawing.Point(16, 348);
            this.btnAutoTrip.Size = new System.Drawing.Size(330, 38);
            this.btnAutoTrip.Text = "🤖 Nhập TỰ ĐỘNG cả chuyến (Best-Fit)";
            this.btnAutoTrip.BackColor = TruckingSystem.Desktop.Common.Theme.Primary;
            this.btnAutoTrip.ForeColor = System.Drawing.Color.White;
            this.btnAutoTrip.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAutoTrip.FlatAppearance.BorderSize = 0;
            this.btnAutoTrip.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.btnAutoTrip.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAutoTrip.Enabled = false;
            this.btnAutoTrip.Click += new System.EventHandler(this.BtnAutoTrip_Click);
            //
            // btnRefresh
            //
            this.btnRefresh.Location = new System.Drawing.Point(936, 348);
            this.btnRefresh.Size = new System.Drawing.Size(120, 38);
            this.btnRefresh.Text = "🔄 Tải lại";
            this.btnRefresh.BackColor = System.Drawing.Color.White;
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
            //
            // lblManualTitle
            //
            this.lblManualTitle.Location = new System.Drawing.Point(16, 400);
            this.lblManualTitle.Size = new System.Drawing.Size(640, 26);
            this.lblManualTitle.Text = "✋ Nhập THỦ CÔNG dòng hàng đang chọn";
            this.lblManualTitle.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.lblManualTitle.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
            //
            // lblQty
            //
            this.lblQty.Location = new System.Drawing.Point(680, 402);
            this.lblQty.Size = new System.Drawing.Size(90, 24);
            this.lblQty.Text = "Số lượng:";
            this.lblQty.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblQty.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // numQty
            //
            this.numQty.Location = new System.Drawing.Point(776, 400);
            this.numQty.Size = new System.Drawing.Size(120, 27);
            this.numQty.Maximum = new decimal(new int[] { 0, 0, 0, 0 });
            this.numQty.Enabled = false;
            //
            // lblManualHint
            //
            this.lblManualHint.Location = new System.Drawing.Point(18, 428);
            this.lblManualHint.Size = new System.Drawing.Size(1040, 22);
            this.lblManualHint.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblManualHint.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblManualHint.Text = "Chọn vị trí bên dưới (chỉ hiện vị trí ĐÚNG khu theo tính chất hàng). Dòng đầu = để Best-Fit tự chọn cho riêng số lượng này.";
            //
            // gridBins
            //
            this.gridBins.Location = new System.Drawing.Point(16, 454);
            this.gridBins.Size = new System.Drawing.Size(1040, 170);
            this.gridBins.Name = "gridBins";
            //
            // btnManual
            //
            this.btnManual.Location = new System.Drawing.Point(16, 634);
            this.btnManual.Size = new System.Drawing.Size(300, 38);
            this.btnManual.Text = "📥 Nhập vào vị trí đã chọn";
            this.btnManual.BackColor = System.Drawing.Color.White;
            this.btnManual.ForeColor = TruckingSystem.Desktop.Common.Theme.Primary;
            this.btnManual.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnManual.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.btnManual.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnManual.Enabled = false;
            this.btnManual.Click += new System.EventHandler(this.BtnManual_Click);
            //
            // lblStatus
            //
            this.lblStatus.Location = new System.Drawing.Point(330, 632);
            this.lblStatus.Size = new System.Drawing.Size(590, 64);
            this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblStatus.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            //
            // btnClose
            //
            this.btnClose.Location = new System.Drawing.Point(956, 634);
            this.btnClose.Size = new System.Drawing.Size(100, 38);
            this.btnClose.Text = "Đóng";
            this.btnClose.BackColor = System.Drawing.Color.White;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            //
            // PendingReceiptsForm
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1072, 704);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.gridPending);
            this.Controls.Add(this.btnAutoTrip);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.lblManualTitle);
            this.Controls.Add(this.lblQty);
            this.Controls.Add(this.numQty);
            this.Controls.Add(this.lblManualHint);
            this.Controls.Add(this.gridBins);
            this.Controls.Add(this.btnManual);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PendingReceiptsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Hàng chờ nhập kho";
            ((System.ComponentModel.ISupportInitialize)(this.gridPending)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numQty)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridBins)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.DataGridView gridPending;
        private System.Windows.Forms.Button btnAutoTrip;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label lblManualTitle;
        private System.Windows.Forms.Label lblQty;
        private System.Windows.Forms.NumericUpDown numQty;
        private System.Windows.Forms.Label lblManualHint;
        private System.Windows.Forms.DataGridView gridBins;
        private System.Windows.Forms.Button btnManual;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnClose;
    }
}
