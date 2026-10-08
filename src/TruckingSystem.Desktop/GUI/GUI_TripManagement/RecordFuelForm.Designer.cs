namespace TruckingSystem.Desktop.GUI_TripManagement;

partial class RecordFuelForm
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
        this.gridLogs = new System.Windows.Forms.DataGridView();
        this.lblTotal = new System.Windows.Forms.Label();
        this.btnDelete = new System.Windows.Forms.Button();
        this.grpAdd = new System.Windows.Forms.GroupBox();
        this.lblProvider = new System.Windows.Forms.Label();
        this.cboProvider = new System.Windows.Forms.ComboBox();
        this.lblLiters = new System.Windows.Forms.Label();
        this.numLiters = new System.Windows.Forms.NumericUpDown();
        this.lblStationName = new System.Windows.Forms.Label();
        this.txtStationName = new System.Windows.Forms.TextBox();
        this.lblPrice = new System.Windows.Forms.Label();
        this.lblAmount = new System.Windows.Forms.Label();
        this.btnSave = new System.Windows.Forms.Button();
        this.lblStatus = new System.Windows.Forms.Label();
        this.btnClose = new System.Windows.Forms.Button();
        ((System.ComponentModel.ISupportInitialize)(this.gridLogs)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numLiters)).BeginInit();
        this.grpAdd.SuspendLayout();
        this.SuspendLayout();
        //
        // lblTitle
        //
        this.lblTitle.AutoSize = true;
        this.lblTitle.Location = new System.Drawing.Point(16, 12);
        this.lblTitle.Text = "⛽ Nhiên liệu của chuyến";
        this.lblTitle.Font = TruckingSystem.Desktop.Common.Theme.FontSectionTitle;
        this.lblTitle.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
        //
        // lblInfo
        //
        this.lblInfo.Location = new System.Drawing.Point(18, 44);
        this.lblInfo.Size = new System.Drawing.Size(726, 20);
        this.lblInfo.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblInfo.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        //
        // gridLogs
        //
        this.gridLogs.Location = new System.Drawing.Point(16, 70);
        this.gridLogs.Size = new System.Drawing.Size(728, 210);
        this.gridLogs.Name = "gridLogs";
        //
        // lblTotal
        //
        this.lblTotal.Location = new System.Drawing.Point(16, 290);
        this.lblTotal.Size = new System.Drawing.Size(560, 24);
        this.lblTotal.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
        this.lblTotal.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
        //
        // btnDelete
        //
        this.btnDelete.Location = new System.Drawing.Point(594, 286);
        this.btnDelete.Size = new System.Drawing.Size(150, 30);
        this.btnDelete.Text = "🗑 Xóa lần đổ";
        this.btnDelete.BackColor = System.Drawing.Color.White;
        this.btnDelete.ForeColor = TruckingSystem.Desktop.Common.Theme.Danger;
        this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnDelete.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.btnDelete.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnDelete.Click += new System.EventHandler(this.BtnDelete_Click);
        //
        // grpAdd
        //
        this.grpAdd.Location = new System.Drawing.Point(16, 326);
        this.grpAdd.Size = new System.Drawing.Size(728, 172);
        this.grpAdd.Text = "Thêm lần đổ xăng";
        this.grpAdd.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
        this.grpAdd.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
        this.grpAdd.Controls.Add(this.lblProvider);
        this.grpAdd.Controls.Add(this.cboProvider);
        this.grpAdd.Controls.Add(this.lblLiters);
        this.grpAdd.Controls.Add(this.numLiters);
        this.grpAdd.Controls.Add(this.lblStationName);
        this.grpAdd.Controls.Add(this.txtStationName);
        this.grpAdd.Controls.Add(this.lblPrice);
        this.grpAdd.Controls.Add(this.lblAmount);
        this.grpAdd.Controls.Add(this.btnSave);
        //
        // lblProvider
        //
        this.lblProvider.AutoSize = true;
        this.lblProvider.Location = new System.Drawing.Point(14, 28);
        this.lblProvider.Text = "Hãng nhiên liệu *";
        this.lblProvider.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblProvider.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        //
        // cboProvider
        //
        this.cboProvider.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboProvider.Location = new System.Drawing.Point(14, 48);
        this.cboProvider.Size = new System.Drawing.Size(260, 23);
        this.cboProvider.Font = TruckingSystem.Desktop.Common.Theme.FontNormal;
        //
        // lblLiters
        //
        this.lblLiters.AutoSize = true;
        this.lblLiters.Location = new System.Drawing.Point(290, 28);
        this.lblLiters.Text = "Số lít *";
        this.lblLiters.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblLiters.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        //
        // numLiters
        //
        this.numLiters.Location = new System.Drawing.Point(290, 48);
        this.numLiters.Size = new System.Drawing.Size(120, 23);
        this.numLiters.DecimalPlaces = 2;
        this.numLiters.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
        this.numLiters.ThousandsSeparator = true;
        this.numLiters.Font = TruckingSystem.Desktop.Common.Theme.FontNormal;
        //
        // lblStationName
        //
        this.lblStationName.AutoSize = true;
        this.lblStationName.Location = new System.Drawing.Point(426, 28);
        this.lblStationName.Text = "Cửa hàng / ghi chú (tuỳ chọn)";
        this.lblStationName.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblStationName.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        //
        // txtStationName
        //
        this.txtStationName.Location = new System.Drawing.Point(426, 48);
        this.txtStationName.Size = new System.Drawing.Size(288, 23);
        this.txtStationName.Font = TruckingSystem.Desktop.Common.Theme.FontNormal;
        this.txtStationName.PlaceholderText = "VD: CHXD số 12 - QL1A";
        //
        // lblPrice
        //
        this.lblPrice.Location = new System.Drawing.Point(14, 84);
        this.lblPrice.Size = new System.Drawing.Size(700, 22);
        this.lblPrice.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblPrice.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        //
        // lblAmount
        //
        this.lblAmount.Location = new System.Drawing.Point(14, 112);
        this.lblAmount.Size = new System.Drawing.Size(540, 28);
        this.lblAmount.Font = TruckingSystem.Desktop.Common.Theme.FontSectionTitle;
        this.lblAmount.ForeColor = TruckingSystem.Desktop.Common.Theme.Primary;
        //
        // btnSave
        //
        this.btnSave.Location = new System.Drawing.Point(574, 120);
        this.btnSave.Size = new System.Drawing.Size(140, 36);
        this.btnSave.Text = "➕ Thêm";
        this.btnSave.BackColor = TruckingSystem.Desktop.Common.Theme.Primary;
        this.btnSave.ForeColor = System.Drawing.Color.White;
        this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnSave.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
        this.btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnSave.FlatAppearance.BorderSize = 0;
        this.btnSave.FlatAppearance.MouseOverBackColor = TruckingSystem.Desktop.Common.Theme.PrimaryDark;
        this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
        //
        // lblStatus
        //
        this.lblStatus.Location = new System.Drawing.Point(16, 508);
        this.lblStatus.Size = new System.Drawing.Size(610, 36);
        this.lblStatus.ForeColor = TruckingSystem.Desktop.Common.Theme.Danger;
        this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        //
        // btnClose
        //
        this.btnClose.Location = new System.Drawing.Point(644, 510);
        this.btnClose.Size = new System.Drawing.Size(100, 32);
        this.btnClose.Text = "Đóng";
        this.btnClose.BackColor = System.Drawing.Color.White;
        this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnClose.Font = TruckingSystem.Desktop.Common.Theme.FontNormal;
        this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
        //
        // RecordFuelForm
        //
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.BackColor = System.Drawing.Color.White;
        this.ClientSize = new System.Drawing.Size(760, 556);
        this.Controls.Add(this.lblTitle);
        this.Controls.Add(this.lblInfo);
        this.Controls.Add(this.gridLogs);
        this.Controls.Add(this.lblTotal);
        this.Controls.Add(this.btnDelete);
        this.Controls.Add(this.grpAdd);
        this.Controls.Add(this.lblStatus);
        this.Controls.Add(this.btnClose);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "RecordFuelForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Nhiên liệu của chuyến";
        ((System.ComponentModel.ISupportInitialize)(this.gridLogs)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numLiters)).EndInit();
        this.grpAdd.ResumeLayout(false);
        this.grpAdd.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Label lblInfo;
    private System.Windows.Forms.DataGridView gridLogs;
    private System.Windows.Forms.Label lblTotal;
    private System.Windows.Forms.Button btnDelete;
    private System.Windows.Forms.GroupBox grpAdd;
    private System.Windows.Forms.Label lblProvider;
    private System.Windows.Forms.ComboBox cboProvider;
    private System.Windows.Forms.Label lblLiters;
    private System.Windows.Forms.NumericUpDown numLiters;
    private System.Windows.Forms.Label lblStationName;
    private System.Windows.Forms.TextBox txtStationName;
    private System.Windows.Forms.Label lblPrice;
    private System.Windows.Forms.Label lblAmount;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.Button btnClose;
}
