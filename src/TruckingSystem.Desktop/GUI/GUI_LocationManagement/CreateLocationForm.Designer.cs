namespace TruckingSystem.Desktop.GUI_LocationManagement;

partial class CreateLocationForm
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
        this.lblSearch = new System.Windows.Forms.Label();
        this.txtSearch = new System.Windows.Forms.TextBox();
        this.lstSuggestions = new System.Windows.Forms.ListBox();
        this.lblSelectedAddress = new System.Windows.Forms.Label();
        this.btnSave = new System.Windows.Forms.Button();
        this.lblStatus = new System.Windows.Forms.Label();
        this.SuspendLayout();
        //
        // lblName
        //
        this.lblName.Location = new System.Drawing.Point(24, 20);
        this.lblName.Size = new System.Drawing.Size(400, 18);
        this.lblName.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblName.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblName.Text = "Tên địa điểm *";
        //
        // txtName
        //
        this.txtName.Location = new System.Drawing.Point(24, 40);
        this.txtName.Size = new System.Drawing.Size(400, 23);
        //
        // lblSearch
        //
        this.lblSearch.Location = new System.Drawing.Point(24, 90);
        this.lblSearch.Size = new System.Drawing.Size(400, 18);
        this.lblSearch.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.lblSearch.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblSearch.Text = "🔍 Tìm địa chỉ * (gõ từ khóa, VD: \"193 Trần Phú\")";
        //
        // txtSearch
        //
        this.txtSearch.Location = new System.Drawing.Point(24, 110);
        this.txtSearch.Size = new System.Drawing.Size(400, 23);
        //
        // lstSuggestions
        //
        this.lstSuggestions.Location = new System.Drawing.Point(24, 138);
        this.lstSuggestions.Size = new System.Drawing.Size(400, 108);
        this.lstSuggestions.Visible = false;
        this.lstSuggestions.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        //
        // lblSelectedAddress
        //
        this.lblSelectedAddress.Location = new System.Drawing.Point(24, 138);
        this.lblSelectedAddress.Size = new System.Drawing.Size(400, 50);
        this.lblSelectedAddress.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblSelectedAddress.ForeColor = TruckingSystem.Desktop.Common.Theme.Success;
        //
        // btnSave
        //
        this.btnSave.Location = new System.Drawing.Point(244, 260);
        this.btnSave.Size = new System.Drawing.Size(180, 38);
        this.btnSave.Text = "Lưu địa điểm";
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
        this.lblStatus.Location = new System.Drawing.Point(24, 220);
        this.lblStatus.Size = new System.Drawing.Size(400, 36);
        this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        //
        // CreateLocationForm
        //
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.BackColor = System.Drawing.Color.White;
        this.ClientSize = new System.Drawing.Size(460, 320);
        this.Controls.Add(this.lblName);
        this.Controls.Add(this.txtName);
        this.Controls.Add(this.lblSearch);
        this.Controls.Add(this.txtSearch);
        this.Controls.Add(this.lstSuggestions);
        this.Controls.Add(this.lblSelectedAddress);
        this.Controls.Add(this.lblStatus);
        this.Controls.Add(this.btnSave);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.Name = "CreateLocationForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Thêm địa điểm mới";
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label lblName;
    private System.Windows.Forms.TextBox txtName;
    private System.Windows.Forms.Label lblSearch;
    private System.Windows.Forms.TextBox txtSearch;
    private System.Windows.Forms.ListBox lstSuggestions;
    private System.Windows.Forms.Label lblSelectedAddress;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.Label lblStatus;
}