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
        this.lblCode = new System.Windows.Forms.Label();
        this.txtCode = new System.Windows.Forms.TextBox();
        this.lblCapacity = new System.Windows.Forms.Label();
        this.numCapacity = new System.Windows.Forms.NumericUpDown();
        this.lblMaxWeight = new System.Windows.Forms.Label();
        this.numMaxWeight = new System.Windows.Forms.NumericUpDown();
        this.lblRow = new System.Windows.Forms.Label();
        this.numRow = new System.Windows.Forms.NumericUpDown();
        this.lblColumn = new System.Windows.Forms.Label();
        this.numColumn = new System.Windows.Forms.NumericUpDown();
        this.lblDistanceFromDoor = new System.Windows.Forms.Label();
        this.numDistanceFromDoor = new System.Windows.Forms.NumericUpDown();
        this.lblNote = new System.Windows.Forms.Label();
        this.btnSave = new System.Windows.Forms.Button();
        this.lblStatus = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)(this.numCapacity)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numMaxWeight)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numRow)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numColumn)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numDistanceFromDoor)).BeginInit();
        this.SuspendLayout();
        //
        // lblCode
        //
        this.lblCode.Location = new System.Drawing.Point(24, 20);
        this.lblCode.Size = new System.Drawing.Size(400, 18);
        this.lblCode.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblCode.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblCode.Text = "Mã vị trí * (VD: C-01)";
        //
        // txtCode
        //
        this.txtCode.Location = new System.Drawing.Point(24, 40);
        this.txtCode.Size = new System.Drawing.Size(400, 23);
        //
        // lblCapacity
        //
        this.lblCapacity.Location = new System.Drawing.Point(24, 90);
        this.lblCapacity.Size = new System.Drawing.Size(190, 18);
        this.lblCapacity.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblCapacity.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblCapacity.Text = "Sức chứa (m³) *";
        //
        // numCapacity
        //
        this.numCapacity.Location = new System.Drawing.Point(24, 110);
        this.numCapacity.Size = new System.Drawing.Size(190, 23);
        this.numCapacity.DecimalPlaces = 2;
        this.numCapacity.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
        this.numCapacity.Value = new decimal(new int[] { 5, 0, 0, 0 });
        //
        // lblMaxWeight
        //
        this.lblMaxWeight.Location = new System.Drawing.Point(234, 90);
        this.lblMaxWeight.Size = new System.Drawing.Size(190, 18);
        this.lblMaxWeight.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblMaxWeight.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblMaxWeight.Text = "Tải trọng tối đa (kg) *";
        //
        // numMaxWeight
        //
        this.numMaxWeight.Location = new System.Drawing.Point(234, 110);
        this.numMaxWeight.Size = new System.Drawing.Size(190, 23);
        this.numMaxWeight.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
        this.numMaxWeight.Value = new decimal(new int[] { 2000, 0, 0, 0 });
        //
        // lblRow
        //
        this.lblRow.Location = new System.Drawing.Point(24, 160);
        this.lblRow.Size = new System.Drawing.Size(190, 18);
        this.lblRow.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblRow.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblRow.Text = "Hàng (Row, bắt đầu từ 0)";
        //
        // numRow
        //
        this.numRow.Location = new System.Drawing.Point(24, 180);
        this.numRow.Size = new System.Drawing.Size(190, 23);
        this.numRow.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
        //
        // lblColumn
        //
        this.lblColumn.Location = new System.Drawing.Point(234, 160);
        this.lblColumn.Size = new System.Drawing.Size(190, 18);
        this.lblColumn.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblColumn.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblColumn.Text = "Cột (Column, bắt đầu từ 0)";
        //
        // numColumn
        //
        this.numColumn.Location = new System.Drawing.Point(234, 180);
        this.numColumn.Size = new System.Drawing.Size(190, 23);
        this.numColumn.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
        //
        // lblDistanceFromDoor
        //
        this.lblDistanceFromDoor.Location = new System.Drawing.Point(24, 215);
        this.lblDistanceFromDoor.Size = new System.Drawing.Size(400, 18);
        this.lblDistanceFromDoor.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblDistanceFromDoor.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblDistanceFromDoor.Text = "Khoảng cách tới cửa kho (mét) — dùng để ưu tiên xếp hàng gấp";
        //
        // numDistanceFromDoor
        //
        this.numDistanceFromDoor.Location = new System.Drawing.Point(24, 235);
        this.numDistanceFromDoor.Size = new System.Drawing.Size(400, 23);
        this.numDistanceFromDoor.DecimalPlaces = 1;
        this.numDistanceFromDoor.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        //
        // lblNote
        //
        this.lblNote.Location = new System.Drawing.Point(24, 270);
        this.lblNote.Size = new System.Drawing.Size(400, 40);
        this.lblNote.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblNote.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblNote.Text = "💡 Hàng/Cột quyết định vị trí ô này trong lưới sơ đồ — 2 ô trùng Hàng+Cột sẽ đè lên nhau, tránh nhập trùng.";
        //
        // btnSave
        //
        this.btnSave.Location = new System.Drawing.Point(120, 355);
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
        this.lblStatus.Location = new System.Drawing.Point(24, 320);
        this.lblStatus.Size = new System.Drawing.Size(400, 25);
        this.lblStatus.ForeColor = TruckingSystem.Desktop.Common.Theme.Danger;
        this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        //
        // CreateBinForm
        //
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.BackColor = System.Drawing.Color.White;
        this.ClientSize = new System.Drawing.Size(450, 420);
        this.Controls.Add(this.lblCode);
        this.Controls.Add(this.txtCode);
        this.Controls.Add(this.lblCapacity);
        this.Controls.Add(this.numCapacity);
        this.Controls.Add(this.lblMaxWeight);
        this.Controls.Add(this.numMaxWeight);
        this.Controls.Add(this.lblRow);
        this.Controls.Add(this.numRow);
        this.Controls.Add(this.lblColumn);
        this.Controls.Add(this.numColumn);
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
        ((System.ComponentModel.ISupportInitialize)(this.numRow)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numColumn)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numDistanceFromDoor)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label lblCode;
    private System.Windows.Forms.TextBox txtCode;
    private System.Windows.Forms.Label lblCapacity;
    private System.Windows.Forms.NumericUpDown numCapacity;
    private System.Windows.Forms.Label lblMaxWeight;
    private System.Windows.Forms.NumericUpDown numMaxWeight;
    private System.Windows.Forms.Label lblRow;
    private System.Windows.Forms.NumericUpDown numRow;
    private System.Windows.Forms.Label lblColumn;
    private System.Windows.Forms.NumericUpDown numColumn;
    private System.Windows.Forms.Label lblDistanceFromDoor;
    private System.Windows.Forms.NumericUpDown numDistanceFromDoor;
    private System.Windows.Forms.Label lblNote;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.Label lblStatus;
}