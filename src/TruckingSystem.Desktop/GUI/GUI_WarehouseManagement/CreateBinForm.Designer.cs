namespace TruckingSystem.Desktop.GUI_WarehouseManagement;

partial class CreateBinForm
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
        this.lblZoneInfo = new System.Windows.Forms.Label();
        this.lblCode = new System.Windows.Forms.Label();
        this.txtCode = new System.Windows.Forms.TextBox();
        this.lblCapacity = new System.Windows.Forms.Label();
        this.numCapacity = new System.Windows.Forms.NumericUpDown();
        this.lblMaxWeight = new System.Windows.Forms.Label();
        this.numMaxWeight = new System.Windows.Forms.NumericUpDown();
        this.lblPosX = new System.Windows.Forms.Label();
        this.numPosX = new System.Windows.Forms.NumericUpDown();
        this.lblPosY = new System.Windows.Forms.Label();
        this.numPosY = new System.Windows.Forms.NumericUpDown();
        this.lblWidth = new System.Windows.Forms.Label();
        this.numWidth = new System.Windows.Forms.NumericUpDown();
        this.lblHeight = new System.Windows.Forms.Label();
        this.numHeight = new System.Windows.Forms.NumericUpDown();
        this.lblDistanceFromDoor = new System.Windows.Forms.Label();
        this.numDistanceFromDoor = new System.Windows.Forms.NumericUpDown();
        this.lblNote = new System.Windows.Forms.Label();
        this.btnSave = new System.Windows.Forms.Button();
        this.lblStatus = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)(this.numCapacity)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numMaxWeight)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numPosX)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numPosY)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numWidth)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numHeight)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numDistanceFromDoor)).BeginInit();
        this.SuspendLayout();
        //
        // lblZoneInfo
        //
        this.lblZoneInfo.Location = new System.Drawing.Point(24, 16);
        this.lblZoneInfo.Size = new System.Drawing.Size(400, 34);
        this.lblZoneInfo.ForeColor = TruckingSystem.Desktop.Common.Theme.Primary;
        this.lblZoneInfo.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        //
        // lblCode
        //
        this.lblCode.Location = new System.Drawing.Point(24, 56);
        this.lblCode.Size = new System.Drawing.Size(400, 18);
        this.lblCode.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblCode.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblCode.Text = "Mã vị trí * (VD: C-01)";
        //
        // txtCode
        //
        this.txtCode.Location = new System.Drawing.Point(24, 76);
        this.txtCode.Size = new System.Drawing.Size(400, 23);
        //
        // lblCapacity
        //
        this.lblCapacity.Location = new System.Drawing.Point(24, 122);
        this.lblCapacity.Size = new System.Drawing.Size(190, 18);
        this.lblCapacity.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblCapacity.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblCapacity.Text = "Sức chứa (m³) *";
        //
        // numCapacity
        //
        this.numCapacity.Location = new System.Drawing.Point(24, 142);
        this.numCapacity.Size = new System.Drawing.Size(190, 23);
        this.numCapacity.DecimalPlaces = 2;
        this.numCapacity.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
        this.numCapacity.Value = new decimal(new int[] { 5, 0, 0, 0 });
        //
        // lblMaxWeight
        //
        this.lblMaxWeight.Location = new System.Drawing.Point(234, 122);
        this.lblMaxWeight.Size = new System.Drawing.Size(190, 18);
        this.lblMaxWeight.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblMaxWeight.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblMaxWeight.Text = "Tải trọng tối đa (kg) *";
        //
        // numMaxWeight
        //
        this.numMaxWeight.Location = new System.Drawing.Point(234, 142);
        this.numMaxWeight.Size = new System.Drawing.Size(190, 23);
        this.numMaxWeight.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
        this.numMaxWeight.Value = new decimal(new int[] { 2000, 0, 0, 0 });
        //
        // lblPosX
        //
        this.lblPosX.Location = new System.Drawing.Point(24, 192);
        this.lblPosX.Size = new System.Drawing.Size(190, 18);
        this.lblPosX.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblPosX.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblPosX.Text = "Vị trí X (mét, so với góc khu vực) *";
        //
        // numPosX
        //
        this.numPosX.Location = new System.Drawing.Point(24, 212);
        this.numPosX.Size = new System.Drawing.Size(190, 23);
        this.numPosX.DecimalPlaces = 2;
        this.numPosX.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        //
        // lblPosY
        //
        this.lblPosY.Location = new System.Drawing.Point(234, 192);
        this.lblPosY.Size = new System.Drawing.Size(190, 18);
        this.lblPosY.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblPosY.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblPosY.Text = "Vị trí Y (mét, so với góc khu vực) *";
        //
        // numPosY
        //
        this.numPosY.Location = new System.Drawing.Point(234, 212);
        this.numPosY.Size = new System.Drawing.Size(190, 23);
        this.numPosY.DecimalPlaces = 2;
        this.numPosY.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        //
        // lblWidth
        //
        this.lblWidth.Location = new System.Drawing.Point(24, 257);
        this.lblWidth.Size = new System.Drawing.Size(190, 18);
        this.lblWidth.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblWidth.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblWidth.Text = "Chiều rộng vị trí (mét) *";
        //
        // numWidth
        //
        this.numWidth.Location = new System.Drawing.Point(24, 277);
        this.numWidth.Size = new System.Drawing.Size(190, 23);
        this.numWidth.DecimalPlaces = 2;
        this.numWidth.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        this.numWidth.Value = new decimal(new int[] { 1, 0, 0, 0 });
        //
        // lblHeight
        //
        this.lblHeight.Location = new System.Drawing.Point(234, 257);
        this.lblHeight.Size = new System.Drawing.Size(190, 18);
        this.lblHeight.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblHeight.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblHeight.Text = "Chiều cao vị trí (mét) *";
        //
        // numHeight
        //
        this.numHeight.Location = new System.Drawing.Point(234, 277);
        this.numHeight.Size = new System.Drawing.Size(190, 23);
        this.numHeight.DecimalPlaces = 2;
        this.numHeight.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        this.numHeight.Value = new decimal(new int[] { 1, 0, 0, 0 });
        //
        // lblDistanceFromDoor
        //
        this.lblDistanceFromDoor.Location = new System.Drawing.Point(24, 312);
        this.lblDistanceFromDoor.Size = new System.Drawing.Size(400, 18);
        this.lblDistanceFromDoor.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblDistanceFromDoor.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblDistanceFromDoor.Text = "🚪 Khoảng cách tới cửa (mét) — hệ thống TỰ TÍNH từ vị trí cửa kho";
        //
        // numDistanceFromDoor
        //
        this.numDistanceFromDoor.Location = new System.Drawing.Point(24, 332);
        this.numDistanceFromDoor.Size = new System.Drawing.Size(400, 23);
        this.numDistanceFromDoor.DecimalPlaces = 1;
        this.numDistanceFromDoor.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
        this.numDistanceFromDoor.Enabled = false; // chỉ hiển thị — không cho nhập tay nữa
        //
        // lblNote
        //
        this.lblNote.Location = new System.Drawing.Point(24, 365);
        this.lblNote.Size = new System.Drawing.Size(400, 40);
        this.lblNote.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblNote.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblNote.Text = "💡 Hệ thống tự kiểm tra: vị trí không được đè lên vị trí khác, không được vượt ra ngoài ranh giới khu vực.";
        //
        // btnSave
        //
        this.btnSave.Location = new System.Drawing.Point(135, 450);
        this.btnSave.Size = new System.Drawing.Size(180, 38);
        this.btnSave.Text = "Lưu vị trí";
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
        this.lblStatus.Location = new System.Drawing.Point(24, 415);
        this.lblStatus.Size = new System.Drawing.Size(400, 30);
        this.lblStatus.ForeColor = TruckingSystem.Desktop.Common.Theme.Danger;
        this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        //
        // CreateBinForm
        //
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.BackColor = System.Drawing.Color.White;
        this.ClientSize = new System.Drawing.Size(450, 510);
        this.Controls.Add(this.lblZoneInfo);
        this.Controls.Add(this.lblCode);
        this.Controls.Add(this.txtCode);
        this.Controls.Add(this.lblCapacity);
        this.Controls.Add(this.numCapacity);
        this.Controls.Add(this.lblMaxWeight);
        this.Controls.Add(this.numMaxWeight);
        this.Controls.Add(this.lblPosX);
        this.Controls.Add(this.numPosX);
        this.Controls.Add(this.lblPosY);
        this.Controls.Add(this.numPosY);
        this.Controls.Add(this.lblWidth);
        this.Controls.Add(this.numWidth);
        this.Controls.Add(this.lblHeight);
        this.Controls.Add(this.numHeight);
        this.Controls.Add(this.lblDistanceFromDoor);
        this.Controls.Add(this.numDistanceFromDoor);
        this.Controls.Add(this.lblNote);
        this.Controls.Add(this.lblStatus);
        this.Controls.Add(this.btnSave);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.Name = "CreateBinForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Thêm vị trí lưu trữ mới";
        ((System.ComponentModel.ISupportInitialize)(this.numCapacity)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numMaxWeight)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numPosX)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numPosY)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numWidth)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numHeight)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numDistanceFromDoor)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label lblZoneInfo;
    private System.Windows.Forms.Label lblCode;
    private System.Windows.Forms.TextBox txtCode;
    private System.Windows.Forms.Label lblCapacity;
    private System.Windows.Forms.NumericUpDown numCapacity;
    private System.Windows.Forms.Label lblMaxWeight;
    private System.Windows.Forms.NumericUpDown numMaxWeight;
    private System.Windows.Forms.Label lblPosX;
    private System.Windows.Forms.NumericUpDown numPosX;
    private System.Windows.Forms.Label lblPosY;
    private System.Windows.Forms.NumericUpDown numPosY;
    private System.Windows.Forms.Label lblWidth;
    private System.Windows.Forms.NumericUpDown numWidth;
    private System.Windows.Forms.Label lblHeight;
    private System.Windows.Forms.NumericUpDown numHeight;
    private System.Windows.Forms.Label lblDistanceFromDoor;
    private System.Windows.Forms.NumericUpDown numDistanceFromDoor;
    private System.Windows.Forms.Label lblNote;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.Label lblStatus;
}