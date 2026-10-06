namespace TruckingSystem.Desktop.GUI_WarehouseManagement;

partial class EditWarehouseForm
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
        this.lblAddress = new System.Windows.Forms.Label();
        this.txtSearch = new System.Windows.Forms.TextBox();
        this.lstSuggestions = new System.Windows.Forms.ListBox();
        this.lblSelectedAddress = new System.Windows.Forms.Label();
        this.lblCapacity = new System.Windows.Forms.Label();
        this.numCapacity = new System.Windows.Forms.NumericUpDown();
        this.lblLength = new System.Windows.Forms.Label();
        this.numLength = new System.Windows.Forms.NumericUpDown();
        this.lblWidth = new System.Windows.Forms.Label();
        this.numWidth = new System.Windows.Forms.NumericUpDown();
        this.lblAreaNote = new System.Windows.Forms.Label();
        this.lblDoorSide = new System.Windows.Forms.Label();
        this.cboDoorSide = new System.Windows.Forms.ComboBox();
        this.lblDoorOffset = new System.Windows.Forms.Label();
        this.numDoorOffset = new System.Windows.Forms.NumericUpDown();
        this.lblDoorNote = new System.Windows.Forms.Label();
        this.btnSave = new System.Windows.Forms.Button();
        this.lblStatus = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)(this.numCapacity)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numLength)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numWidth)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numDoorOffset)).BeginInit();
        this.SuspendLayout();
        //
        // lblName
        //
        this.lblName.Location = new System.Drawing.Point(24, 20);
        this.lblName.Size = new System.Drawing.Size(400, 18);
        this.lblName.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblName.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblName.Text = "Tên kho * (VD: Kho Hà Nội)";
        //
        // txtName
        //
        this.txtName.Location = new System.Drawing.Point(24, 40);
        this.txtName.Size = new System.Drawing.Size(400, 23);
        //
        // lblAddress
        //
        this.lblAddress.Location = new System.Drawing.Point(24, 90);
        this.lblAddress.Size = new System.Drawing.Size(400, 18);
        this.lblAddress.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblAddress.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblAddress.Text = "🔍 Tìm địa chỉ (tuỳ chọn, gõ từ khóa rồi chọn từ gợi ý)";
        //
        // txtSearch
        //
        this.txtSearch.Location = new System.Drawing.Point(24, 110);
        this.txtSearch.Size = new System.Drawing.Size(400, 23);
        //
        // lstSuggestions
        //
        this.lstSuggestions.Location = new System.Drawing.Point(24, 138);
        this.lstSuggestions.Size = new System.Drawing.Size(400, 90);
        this.lstSuggestions.Visible = false;
        this.lstSuggestions.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        //
        // lblSelectedAddress
        //
        this.lblSelectedAddress.Location = new System.Drawing.Point(24, 138);
        this.lblSelectedAddress.Size = new System.Drawing.Size(400, 20);
        this.lblSelectedAddress.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblSelectedAddress.ForeColor = TruckingSystem.Desktop.Common.Theme.Success;
        //
        // lblCapacity
        //
        this.lblCapacity.Location = new System.Drawing.Point(24, 160);
        this.lblCapacity.Size = new System.Drawing.Size(400, 18);
        this.lblCapacity.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblCapacity.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblCapacity.Text = "Sức chứa (m³) — dùng để tính Best-Fit, tuỳ chọn";
        //
        // numCapacity
        //
        this.numCapacity.Location = new System.Drawing.Point(24, 180);
        this.numCapacity.Size = new System.Drawing.Size(400, 23);
        this.numCapacity.DecimalPlaces = 0;
        this.numCapacity.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
        this.numCapacity.Increment = new decimal(new int[] { 10, 0, 0, 0 });
        //
        // lblLength
        //
        this.lblLength.Location = new System.Drawing.Point(24, 220);
        this.lblLength.Size = new System.Drawing.Size(190, 18);
        this.lblLength.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblLength.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblLength.Text = "Chiều dài (mét)";
        //
        // numLength
        //
        this.numLength.Location = new System.Drawing.Point(24, 240);
        this.numLength.Size = new System.Drawing.Size(190, 23);
        this.numLength.DecimalPlaces = 1;
        this.numLength.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        this.numLength.Value = new decimal(new int[] { 30, 0, 0, 0 });
        //
        // lblWidth
        //
        this.lblWidth.Location = new System.Drawing.Point(234, 220);
        this.lblWidth.Size = new System.Drawing.Size(190, 18);
        this.lblWidth.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblWidth.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblWidth.Text = "Chiều rộng (mét)";
        //
        // numWidth
        //
        this.numWidth.Location = new System.Drawing.Point(234, 240);
        this.numWidth.Size = new System.Drawing.Size(190, 23);
        this.numWidth.DecimalPlaces = 1;
        this.numWidth.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        this.numWidth.Value = new decimal(new int[] { 20, 0, 0, 0 });
        //
        // lblAreaNote
        //
        this.lblAreaNote.Location = new System.Drawing.Point(24, 270);
        this.lblAreaNote.Size = new System.Drawing.Size(400, 40);
        this.lblAreaNote.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblAreaNote.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblAreaNote.Text = "💡 Diện tích sàn = Dài × Rộng — dùng để vẽ sơ đồ tổng quan kho đúng tỉ lệ thật ở màn Kho trung chuyển.";
        //
        // lblDoorSide
        //
        this.lblDoorSide.Location = new System.Drawing.Point(24, 318);
        this.lblDoorSide.Size = new System.Drawing.Size(190, 18);
        this.lblDoorSide.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblDoorSide.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblDoorSide.Text = "🚪 Cửa kho nằm ở";
        //
        // cboDoorSide
        //
        this.cboDoorSide.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboDoorSide.Location = new System.Drawing.Point(24, 338);
        this.cboDoorSide.Size = new System.Drawing.Size(190, 23);
        //
        // lblDoorOffset
        //
        this.lblDoorOffset.Location = new System.Drawing.Point(234, 318);
        this.lblDoorOffset.Size = new System.Drawing.Size(190, 18);
        this.lblDoorOffset.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblDoorOffset.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblDoorOffset.Text = "Cách góc (mét)";
        //
        // numDoorOffset
        //
        this.numDoorOffset.Location = new System.Drawing.Point(234, 338);
        this.numDoorOffset.Size = new System.Drawing.Size(190, 23);
        this.numDoorOffset.DecimalPlaces = 1;
        this.numDoorOffset.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        this.numDoorOffset.Enabled = false;
        //
        // lblDoorNote
        //
        this.lblDoorNote.Location = new System.Drawing.Point(24, 366);
        this.lblDoorNote.Size = new System.Drawing.Size(400, 36);
        this.lblDoorNote.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblDoorNote.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblDoorNote.Text = "Tường trên/dưới: đo từ góc TRÁI · Tường trái/phải: đo từ góc TRÊN. Hệ thống tự tính khoảng cách tới cửa của từng ô.";
        //
        // btnSave
        //
        this.btnSave.Location = new System.Drawing.Point(120, 440);
        this.btnSave.Size = new System.Drawing.Size(200, 40);
        this.btnSave.Text = "Lưu kho mới";
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
        this.lblStatus.Location = new System.Drawing.Point(24, 408);
        this.lblStatus.Size = new System.Drawing.Size(400, 25);
        this.lblStatus.ForeColor = TruckingSystem.Desktop.Common.Theme.Danger;
        this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        //
        // EditWarehouseForm
        //
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.BackColor = System.Drawing.Color.White;
        this.ClientSize = new System.Drawing.Size(460, 500);
        this.Controls.Add(this.lblName);
        this.Controls.Add(this.txtName);
        this.Controls.Add(this.lblAddress);
        this.Controls.Add(this.txtSearch);
        this.Controls.Add(this.lstSuggestions);
        this.Controls.Add(this.lblSelectedAddress);
        this.Controls.Add(this.lblCapacity);
        this.Controls.Add(this.numCapacity);
        this.Controls.Add(this.lblLength);
        this.Controls.Add(this.numLength);
        this.Controls.Add(this.lblWidth);
        this.Controls.Add(this.numWidth);
        this.Controls.Add(this.lblAreaNote);
        this.Controls.Add(this.lblDoorSide);
        this.Controls.Add(this.cboDoorSide);
        this.Controls.Add(this.lblDoorOffset);
        this.Controls.Add(this.numDoorOffset);
        this.Controls.Add(this.lblDoorNote);
        this.Controls.Add(this.lblStatus);
        this.Controls.Add(this.btnSave);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.Name = "EditWarehouseForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Sửa thông tin kho";
        ((System.ComponentModel.ISupportInitialize)(this.numCapacity)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numLength)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numWidth)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numDoorOffset)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label lblName;
    private System.Windows.Forms.TextBox txtName;
    private System.Windows.Forms.Label lblAddress;
    private System.Windows.Forms.TextBox txtSearch;
    private System.Windows.Forms.ListBox lstSuggestions;
    private System.Windows.Forms.Label lblSelectedAddress;
    private System.Windows.Forms.Label lblCapacity;
    private System.Windows.Forms.NumericUpDown numCapacity;
    private System.Windows.Forms.Label lblLength;
    private System.Windows.Forms.NumericUpDown numLength;
    private System.Windows.Forms.Label lblWidth;
    private System.Windows.Forms.NumericUpDown numWidth;
    private System.Windows.Forms.Label lblDoorSide;
    private System.Windows.Forms.ComboBox cboDoorSide;
    private System.Windows.Forms.Label lblDoorOffset;
    private System.Windows.Forms.NumericUpDown numDoorOffset;
    private System.Windows.Forms.Label lblDoorNote;
    private System.Windows.Forms.Label lblAreaNote;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.Label lblStatus;
}