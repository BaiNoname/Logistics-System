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
        this.lblLiters = new System.Windows.Forms.Label();
        this.numLiters = new System.Windows.Forms.NumericUpDown();
        this.lblCost = new System.Windows.Forms.Label();
        this.numCost = new System.Windows.Forms.NumericUpDown();
        this.lblStationName = new System.Windows.Forms.Label();
        this.txtStationName = new System.Windows.Forms.TextBox();
        this.btnSave = new System.Windows.Forms.Button();
        this.lblStatus = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)(this.numLiters)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numCost)).BeginInit();
        this.SuspendLayout();
        //
        // lblTitle
        //
        this.lblTitle.Location = new System.Drawing.Point(20, 16);
        this.lblTitle.AutoSize = true;
        this.lblTitle.Text = "⛽ Ghi nhận đổ xăng";
        this.lblTitle.Font = TruckingSystem.Desktop.Common.Theme.FontSectionTitle;
        this.lblTitle.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
        //
        // lblLiters
        //
        this.lblLiters.Location = new System.Drawing.Point(20, 60);
        this.lblLiters.AutoSize = true;
        this.lblLiters.Text = "Số lít *";
        this.lblLiters.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblLiters.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        //
        // numLiters
        //
        this.numLiters.Location = new System.Drawing.Point(20, 80);
        this.numLiters.Size = new System.Drawing.Size(380, 23);
        this.numLiters.DecimalPlaces = 2;
        this.numLiters.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
        //
        // lblCost
        //
        this.lblCost.Location = new System.Drawing.Point(20, 115);
        this.lblCost.AutoSize = true;
        this.lblCost.Text = "Chi phí (VNĐ) *";
        this.lblCost.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblCost.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        //
        // numCost
        //
        this.numCost.Location = new System.Drawing.Point(20, 135);
        this.numCost.Size = new System.Drawing.Size(380, 23);
        this.numCost.DecimalPlaces = 0;
        this.numCost.Maximum = new decimal(new int[] { 100000000, 0, 0, 0 });
        this.numCost.Increment = new decimal(new int[] { 1000, 0, 0, 0 });
        //
        // lblStationName
        //
        this.lblStationName.Location = new System.Drawing.Point(20, 170);
        this.lblStationName.AutoSize = true;
        this.lblStationName.Text = "Tên trạm xăng (tuỳ chọn)";
        this.lblStationName.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblStationName.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        //
        // txtStationName
        //
        this.txtStationName.Location = new System.Drawing.Point(20, 190);
        this.txtStationName.Size = new System.Drawing.Size(380, 23);
        //
        // btnSave
        //
        this.btnSave.Location = new System.Drawing.Point(140, 240);
        this.btnSave.Size = new System.Drawing.Size(150, 38);
        this.btnSave.Text = "Lưu";
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
        this.lblStatus.Location = new System.Drawing.Point(20, 220);
        this.lblStatus.Size = new System.Drawing.Size(380, 20);
        this.lblStatus.ForeColor = TruckingSystem.Desktop.Common.Theme.Danger;
        this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        //
        // RecordFuelForm
        //
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.BackColor = System.Drawing.Color.White;
        this.ClientSize = new System.Drawing.Size(420, 300);
        this.Controls.Add(this.lblTitle);
        this.Controls.Add(this.lblLiters);
        this.Controls.Add(this.numLiters);
        this.Controls.Add(this.lblCost);
        this.Controls.Add(this.numCost);
        this.Controls.Add(this.lblStationName);
        this.Controls.Add(this.txtStationName);
        this.Controls.Add(this.lblStatus);
        this.Controls.Add(this.btnSave);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.Name = "RecordFuelForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Ghi nhận đổ xăng";
        ((System.ComponentModel.ISupportInitialize)(this.numLiters)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numCost)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Label lblLiters;
    private System.Windows.Forms.NumericUpDown numLiters;
    private System.Windows.Forms.Label lblCost;
    private System.Windows.Forms.NumericUpDown numCost;
    private System.Windows.Forms.Label lblStationName;
    private System.Windows.Forms.TextBox txtStationName;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.Label lblStatus;
}