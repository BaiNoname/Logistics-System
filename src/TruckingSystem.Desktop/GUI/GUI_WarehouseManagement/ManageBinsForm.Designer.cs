namespace TruckingSystem.Desktop.GUI_WarehouseManagement;

partial class ManageBinsForm
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
        this.btnAddBin = new System.Windows.Forms.Button();
        this.btnEditBin = new System.Windows.Forms.Button();
        this.btnDeleteBin = new System.Windows.Forms.Button();
        this.lblStatus = new System.Windows.Forms.Label();
        this.grid = new System.Windows.Forms.DataGridView();
        this.SuspendLayout();
        //
        // lblTitle
        //
        this.lblTitle.Location = new System.Drawing.Point(20, 16);
        this.lblTitle.Size = new System.Drawing.Size(400, 32);
        this.lblTitle.Text = "Vị trí lưu trữ";
        this.lblTitle.Font = TruckingSystem.Desktop.Common.Theme.FontTitle;
        this.lblTitle.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
        //
        //
        // btnEditBin
        //
        this.btnEditBin.Location = new System.Drawing.Point(590, 16);
        this.btnEditBin.Size = new System.Drawing.Size(130, 38);
        this.btnEditBin.Text = "✏️ Sửa vị trí";
        this.btnEditBin.BackColor = System.Drawing.Color.White;
        this.btnEditBin.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
        this.btnEditBin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnEditBin.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
        this.btnEditBin.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnEditBin.FlatAppearance.BorderSize = 1;
        this.btnEditBin.Click += new System.EventHandler(this.BtnEditBin_Click);
        // btnAddBin
        //
        this.btnAddBin.Location = new System.Drawing.Point(730, 16);
        this.btnAddBin.Size = new System.Drawing.Size(160, 38);
        this.btnAddBin.Text = "➕ Thêm vị trí mới";
        this.btnAddBin.BackColor = TruckingSystem.Desktop.Common.Theme.Primary;
        this.btnAddBin.ForeColor = System.Drawing.Color.White;
        this.btnAddBin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnAddBin.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
        this.btnAddBin.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnAddBin.FlatAppearance.BorderSize = 0;
        this.btnAddBin.FlatAppearance.MouseOverBackColor = TruckingSystem.Desktop.Common.Theme.PrimaryDark;
        this.btnAddBin.Click += new System.EventHandler(this.BtnAddBin_Click);
        //
        // btnDeleteBin
        //
        this.btnDeleteBin.Location = new System.Drawing.Point(900, 16);
        this.btnDeleteBin.Size = new System.Drawing.Size(140, 38);
        this.btnDeleteBin.Text = "🗑️ Xóa vị trí";
        this.btnDeleteBin.BackColor = System.Drawing.Color.White;
        this.btnDeleteBin.ForeColor = TruckingSystem.Desktop.Common.Theme.Danger;
        this.btnDeleteBin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnDeleteBin.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
        this.btnDeleteBin.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnDeleteBin.FlatAppearance.BorderSize = 1;
        this.btnDeleteBin.FlatAppearance.BorderColor = TruckingSystem.Desktop.Common.Theme.Danger;
        this.btnDeleteBin.Click += new System.EventHandler(this.BtnDeleteBin_Click);
        //
        // lblStatus
        //
        this.lblStatus.Location = new System.Drawing.Point(20, 56);
        this.lblStatus.AutoSize = true;
        this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblStatus.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        //
        // grid
        //
        ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
        this.grid.Location = new System.Drawing.Point(20, 76);
        this.grid.Size = new System.Drawing.Size(1020, 380);
        this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        //
        // ManageBinsForm
        //
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.BackColor = System.Drawing.Color.White;
        this.ClientSize = new System.Drawing.Size(1060, 500);
        this.Controls.Add(this.lblTitle);
        this.Controls.Add(this.btnAddBin);
        this.Controls.Add(this.btnEditBin);
        this.Controls.Add(this.btnDeleteBin);
        this.Controls.Add(this.lblStatus);
        this.Controls.Add(this.grid);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.Name = "ManageBinsForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Vị trí lưu trữ";
        ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Button btnAddBin;
    private System.Windows.Forms.Button btnEditBin;
    private System.Windows.Forms.Button btnDeleteBin;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.DataGridView grid;
}