namespace TruckingSystem.Desktop.GUI_WarehouseManagement;

partial class CreateWarehouseForm
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
        this.btnSave = new System.Windows.Forms.Button();
        this.lblStatus = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)(this.numCapacity)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numLength)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numWidth)).BeginInit();
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
        // btnSave
        //
        this.btnSave.Location = new System.Drawing.Point(120, 360);
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
        this.lblStatus.Location = new System.Drawing.Point(24, 330);
        this.lblStatus.Size = new System.Drawing.Size(400, 25);
        this.lblStatus.ForeColor = TruckingSystem.Desktop.Common.Theme.Danger;
        this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        //
        // CreateWarehouseForm
        //
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.BackColor = System.Drawing.Color.White;
        this.ClientSize = new System.Drawing.Size(460, 420);
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
        this.Controls.Add(this.lblStatus);
        this.Controls.Add(this.btnSave);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.Name = "CreateWarehouseForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Thêm kho mới";
        ((System.ComponentModel.ISupportInitialize)(this.numCapacity)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numLength)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numWidth)).EndInit();
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
    private System.Windows.Forms.Label lblAreaNote;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.Label lblStatus;
}