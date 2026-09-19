namespace TruckingSystem.Desktop.GUI_WarehouseManagement;

partial class WarehousesControl
{
    private System.ComponentModel.IContainer components = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    #region Component Designer generated code

    private void InitializeComponent()
    {
        this.lblTitle = new System.Windows.Forms.Label();
        this.lblChoose = new System.Windows.Forms.Label();
        this.cboWarehouse = new System.Windows.Forms.ComboBox();
        this.lblStatus = new System.Windows.Forms.Label();
        this.zonesPanel = new System.Windows.Forms.FlowLayoutPanel();
        this.testPanel = new System.Windows.Forms.Panel();
        this.lblTestTitle = new System.Windows.Forms.Label();
        this.lblCargoLineId = new System.Windows.Forms.Label();
        this.numCargoLineId = new System.Windows.Forms.NumericUpDown();
        this.lblQty = new System.Windows.Forms.Label();
        this.numQty = new System.Windows.Forms.NumericUpDown();
        this.btnSuggest = new System.Windows.Forms.Button();
        this.lblResult = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)(this.numCargoLineId)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numQty)).BeginInit();
        this.testPanel.SuspendLayout();
        this.SuspendLayout();
        //
        // lblTitle
        //
        this.lblTitle.Location = new System.Drawing.Point(0, 0);
        this.lblTitle.Size = new System.Drawing.Size(400, 32);
        this.lblTitle.Text = "Kho trung chuyển";
        this.lblTitle.Font = TruckingSystem.Desktop.Common.Theme.FontTitle;
        this.lblTitle.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
        //
        // lblChoose
        //
        this.lblChoose.Location = new System.Drawing.Point(0, 46);
        this.lblChoose.AutoSize = true;
        this.lblChoose.Text = "Chọn kho:";
        this.lblChoose.Font = TruckingSystem.Desktop.Common.Theme.FontNormal;
        //
        // cboWarehouse
        //
        this.cboWarehouse.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboWarehouse.Location = new System.Drawing.Point(80, 44);
        this.cboWarehouse.Size = new System.Drawing.Size(260, 23);
        //
        // lblStatus
        //
        this.lblStatus.Location = new System.Drawing.Point(0, 90);
        this.lblStatus.AutoSize = true;
        this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblStatus.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        //
        // zonesPanel
        //
        this.zonesPanel.Location = new System.Drawing.Point(0, 120);
        this.zonesPanel.Size = new System.Drawing.Size(1000, 430);
        this.zonesPanel.AutoScroll = true;
        this.zonesPanel.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this.zonesPanel.WrapContents = true;
        //
        // testPanel
        //
        this.testPanel.Location = new System.Drawing.Point(0, 570);
        this.testPanel.Size = new System.Drawing.Size(1000, 150);
        this.testPanel.BackColor = TruckingSystem.Desktop.Common.Theme.CardBg;
        this.testPanel.Controls.Add(this.lblTestTitle);
        this.testPanel.Controls.Add(this.lblCargoLineId);
        this.testPanel.Controls.Add(this.numCargoLineId);
        this.testPanel.Controls.Add(this.lblQty);
        this.testPanel.Controls.Add(this.numQty);
        this.testPanel.Controls.Add(this.btnSuggest);
        this.testPanel.Controls.Add(this.lblResult);
        //
        // lblTestTitle
        //
        this.lblTestTitle.Location = new System.Drawing.Point(20, 14);
        this.lblTestTitle.AutoSize = true;
        this.lblTestTitle.Text = "🧪  Kiểm tra thuật toán Best-Fit — đề xuất vị trí lưu trữ";
        this.lblTestTitle.Font = TruckingSystem.Desktop.Common.Theme.FontSectionTitle;
        this.lblTestTitle.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
        //
        // lblCargoLineId
        //
        this.lblCargoLineId.Location = new System.Drawing.Point(20, 56);
        this.lblCargoLineId.AutoSize = true;
        this.lblCargoLineId.Text = "Mã dòng hàng (CargoLineId):";
        this.lblCargoLineId.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblCargoLineId.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        //
        // numCargoLineId
        //
        this.numCargoLineId.Location = new System.Drawing.Point(20, 76);
        this.numCargoLineId.Size = new System.Drawing.Size(100, 23);
        this.numCargoLineId.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        this.numCargoLineId.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
        this.numCargoLineId.Value = new decimal(new int[] { 1, 0, 0, 0 });
        //
        // lblQty
        //
        this.lblQty.Location = new System.Drawing.Point(140, 56);
        this.lblQty.AutoSize = true;
        this.lblQty.Text = "Số lượng cần xếp (m³):";
        this.lblQty.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblQty.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        //
        // numQty
        //
        this.numQty.Location = new System.Drawing.Point(140, 76);
        this.numQty.Size = new System.Drawing.Size(100, 23);
        this.numQty.DecimalPlaces = 2;
        this.numQty.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
        this.numQty.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
        this.numQty.Value = new decimal(new int[] { 2, 0, 0, 0 });
        //
        // btnSuggest
        //
        this.btnSuggest.Location = new System.Drawing.Point(260, 75);
        this.btnSuggest.Size = new System.Drawing.Size(160, 36);
        this.btnSuggest.Text = "🔍  Gợi ý vị trí";
        this.btnSuggest.BackColor = TruckingSystem.Desktop.Common.Theme.Primary;
        this.btnSuggest.ForeColor = System.Drawing.Color.White;
        this.btnSuggest.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnSuggest.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
        this.btnSuggest.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnSuggest.FlatAppearance.BorderSize = 0;
        this.btnSuggest.FlatAppearance.MouseOverBackColor = TruckingSystem.Desktop.Common.Theme.PrimaryDark;
        this.btnSuggest.Click += new System.EventHandler(this.BtnSuggest_Click);
        //
        // lblResult
        //
        this.lblResult.Location = new System.Drawing.Point(460, 60);
        this.lblResult.MaximumSize = new System.Drawing.Size(600, 80);
        this.lblResult.AutoSize = true;
        this.lblResult.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
        //
        // WarehousesControl
        //
        this.BackColor = TruckingSystem.Desktop.Common.Theme.ContentBg;
        this.Dock = System.Windows.Forms.DockStyle.Fill;
        this.Controls.Add(this.lblTitle);
        this.Controls.Add(this.lblChoose);
        this.Controls.Add(this.cboWarehouse);
        this.Controls.Add(this.lblStatus);
        this.Controls.Add(this.zonesPanel);
        this.Controls.Add(this.testPanel);
        this.Name = "WarehousesControl";
        ((System.ComponentModel.ISupportInitialize)(this.numCargoLineId)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numQty)).EndInit();
        this.testPanel.ResumeLayout(false);
        this.testPanel.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Label lblChoose;
    private System.Windows.Forms.ComboBox cboWarehouse;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.FlowLayoutPanel zonesPanel;
    private System.Windows.Forms.Panel testPanel;
    private System.Windows.Forms.Label lblTestTitle;
    private System.Windows.Forms.Label lblCargoLineId;
    private System.Windows.Forms.NumericUpDown numCargoLineId;
    private System.Windows.Forms.Label lblQty;
    private System.Windows.Forms.NumericUpDown numQty;
    private System.Windows.Forms.Button btnSuggest;
    private System.Windows.Forms.Label lblResult;
}
