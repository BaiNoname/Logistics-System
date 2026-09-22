namespace TruckingSystem.Desktop.GUI_WarehouseManagement;

partial class EditZoneForm
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
        this.lblName = new System.Windows.Forms.Label();
        this.txtName = new System.Windows.Forms.TextBox();
        this.lblZoneType = new System.Windows.Forms.Label();
        this.cboZoneType = new System.Windows.Forms.ComboBox();
        this.lblPosX = new System.Windows.Forms.Label();
        this.numPosX = new System.Windows.Forms.NumericUpDown();
        this.lblPosY = new System.Windows.Forms.Label();
        this.numPosY = new System.Windows.Forms.NumericUpDown();
        this.lblWidth = new System.Windows.Forms.Label();
        this.numWidth = new System.Windows.Forms.NumericUpDown();
        this.lblHeight = new System.Windows.Forms.Label();
        this.numHeight = new System.Windows.Forms.NumericUpDown();
        this.lblNote = new System.Windows.Forms.Label();
        this.btnSave = new System.Windows.Forms.Button();
        this.lblStatus = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)(this.numPosX)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numPosY)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numWidth)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numHeight)).BeginInit();
        this.SuspendLayout();
        //
        // lblName
        //
        this.lblName.Location = new System.Drawing.Point(24, 20);
        this.lblName.Size = new System.Drawing.Size(420, 18);
        this.lblName.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblName.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblName.Text = "Tên khu vực * (VD: Khu C - Hàng đông lạnh)";
        //
        // txtName
        //
        this.txtName.Location = new System.Drawing.Point(24, 40);
        this.txtName.Size = new System.Drawing.Size(420, 23);
        //
        // lblZoneType
        //
        this.lblZoneType.Location = new System.Drawing.Point(24, 90);
        this.lblZoneType.Size = new System.Drawing.Size(420, 18);
        this.lblZoneType.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblZoneType.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblZoneType.Text = "Loại hàng của khu vực";
        //
        // cboZoneType
        //
        this.cboZoneType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboZoneType.Location = new System.Drawing.Point(24, 110);
        this.cboZoneType.Size = new System.Drawing.Size(420, 23);
        //
        // lblPosX
        //
        this.lblPosX.Location = new System.Drawing.Point(24, 160);
        this.lblPosX.Size = new System.Drawing.Size(200, 18);
        this.lblPosX.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblPosX.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblPosX.Text = "Vị trí X (mét, tính từ góc trái kho)";
        //
        // numPosX
        //
        this.numPosX.Location = new System.Drawing.Point(24, 180);
        this.numPosX.Size = new System.Drawing.Size(200, 23);
        this.numPosX.DecimalPlaces = 1;
        this.numPosX.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        //
        // lblPosY
        //
        this.lblPosY.Location = new System.Drawing.Point(244, 160);
        this.lblPosY.Size = new System.Drawing.Size(200, 18);
        this.lblPosY.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblPosY.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblPosY.Text = "Vị trí Y (mét, tính từ góc trên kho)";
        //
        // numPosY
        //
        this.numPosY.Location = new System.Drawing.Point(244, 180);
        this.numPosY.Size = new System.Drawing.Size(200, 23);
        this.numPosY.DecimalPlaces = 1;
        this.numPosY.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        //
        // lblWidth
        //
        this.lblWidth.Location = new System.Drawing.Point(24, 220);
        this.lblWidth.Size = new System.Drawing.Size(200, 18);
        this.lblWidth.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblWidth.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblWidth.Text = "Chiều rộng khu vực (mét) *";
        //
        // numWidth
        //
        this.numWidth.Location = new System.Drawing.Point(24, 240);
        this.numWidth.Size = new System.Drawing.Size(200, 23);
        this.numWidth.DecimalPlaces = 1;
        this.numWidth.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        this.numWidth.Value = new decimal(new int[] { 10, 0, 0, 0 });
        //
        // lblHeight
        //
        this.lblHeight.Location = new System.Drawing.Point(244, 220);
        this.lblHeight.Size = new System.Drawing.Size(200, 18);
        this.lblHeight.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblHeight.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblHeight.Text = "Chiều cao khu vực (mét) *";
        //
        // numHeight
        //
        this.numHeight.Location = new System.Drawing.Point(244, 240);
        this.numHeight.Size = new System.Drawing.Size(200, 23);
        this.numHeight.DecimalPlaces = 1;
        this.numHeight.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        this.numHeight.Value = new decimal(new int[] { 10, 0, 0, 0 });
        //
        // lblNote
        //
        this.lblNote.Location = new System.Drawing.Point(24, 275);
        this.lblNote.Size = new System.Drawing.Size(420, 40);
        this.lblNote.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblNote.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblNote.Text = "💡 Đảm bảo (Vị trí X + Rộng) và (Vị trí Y + Cao) không vượt quá kích thước tổng của kho, tránh đè lên khu vực khác.";
        //
        // btnSave
        //
        this.btnSave.Location = new System.Drawing.Point(140, 380);
        this.btnSave.Size = new System.Drawing.Size(180, 38);
        this.btnSave.Text = "Lưu khu vực";
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
        this.lblStatus.Location = new System.Drawing.Point(24, 345);
        this.lblStatus.Size = new System.Drawing.Size(420, 25);
        this.lblStatus.ForeColor = TruckingSystem.Desktop.Common.Theme.Danger;
        this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        //
        // EditZoneForm
        //
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.BackColor = System.Drawing.Color.White;
        this.ClientSize = new System.Drawing.Size(470, 440);
        this.Controls.Add(this.lblName);
        this.Controls.Add(this.txtName);
        this.Controls.Add(this.lblZoneType);
        this.Controls.Add(this.cboZoneType);
        this.Controls.Add(this.lblPosX);
        this.Controls.Add(this.numPosX);
        this.Controls.Add(this.lblPosY);
        this.Controls.Add(this.numPosY);
        this.Controls.Add(this.lblWidth);
        this.Controls.Add(this.numWidth);
        this.Controls.Add(this.lblHeight);
        this.Controls.Add(this.numHeight);
        this.Controls.Add(this.lblNote);
        this.Controls.Add(this.lblStatus);
        this.Controls.Add(this.btnSave);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.Name = "EditZoneForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Sửa khu vực";
        ((System.ComponentModel.ISupportInitialize)(this.numPosX)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numPosY)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numWidth)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numHeight)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label lblName;
    private System.Windows.Forms.TextBox txtName;
    private System.Windows.Forms.Label lblZoneType;
    private System.Windows.Forms.ComboBox cboZoneType;
    private System.Windows.Forms.Label lblPosX;
    private System.Windows.Forms.NumericUpDown numPosX;
    private System.Windows.Forms.Label lblPosY;
    private System.Windows.Forms.NumericUpDown numPosY;
    private System.Windows.Forms.Label lblWidth;
    private System.Windows.Forms.NumericUpDown numWidth;
    private System.Windows.Forms.Label lblHeight;
    private System.Windows.Forms.NumericUpDown numHeight;
    private System.Windows.Forms.Label lblNote;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.Label lblStatus;
}