namespace TruckingSystem.Desktop.GUI_WarehouseManagement;

partial class ManageZonesForm
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
        this.btnAddZone = new System.Windows.Forms.Button();
        this.btnDeleteZone = new System.Windows.Forms.Button();
        this.lblStatus = new System.Windows.Forms.Label();
        this.grid = new System.Windows.Forms.DataGridView();
        this.SuspendLayout();
        //
        // lblTitle
        //
        this.lblTitle.Location = new System.Drawing.Point(20, 16);
        this.lblTitle.Size = new System.Drawing.Size(400, 32);
        this.lblTitle.Text = "Quản lý khu vực trong kho";
        this.lblTitle.Font = TruckingSystem.Desktop.Common.Theme.FontTitle;
        this.lblTitle.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
        //
        // btnAddZone
        //
        this.btnAddZone.Location = new System.Drawing.Point(590, 16);
        this.btnAddZone.Size = new System.Drawing.Size(160, 38);
        this.btnAddZone.Text = "➕ Thêm khu vực mới";
        this.btnAddZone.BackColor = TruckingSystem.Desktop.Common.Theme.Primary;
        this.btnAddZone.ForeColor = System.Drawing.Color.White;
        this.btnAddZone.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnAddZone.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
        this.btnAddZone.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnAddZone.FlatAppearance.BorderSize = 0;
        this.btnAddZone.FlatAppearance.MouseOverBackColor = TruckingSystem.Desktop.Common.Theme.PrimaryDark;
        this.btnAddZone.Click += new System.EventHandler(this.BtnAddZone_Click);
        //
        // btnDeleteZone
        //
        this.btnDeleteZone.Location = new System.Drawing.Point(760, 16);
        this.btnDeleteZone.Size = new System.Drawing.Size(140, 38);
        this.btnDeleteZone.Text = "🗑️ Xóa khu vực";
        this.btnDeleteZone.BackColor = System.Drawing.Color.White;
        this.btnDeleteZone.ForeColor = TruckingSystem.Desktop.Common.Theme.Danger;
        this.btnDeleteZone.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnDeleteZone.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
        this.btnDeleteZone.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnDeleteZone.FlatAppearance.BorderSize = 1;
        this.btnDeleteZone.FlatAppearance.BorderColor = TruckingSystem.Desktop.Common.Theme.Danger;
        this.btnDeleteZone.Click += new System.EventHandler(this.BtnDeleteZone_Click);
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
        this.grid.Size = new System.Drawing.Size(880, 380);
        this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        //
        // ManageZonesForm
        //
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.BackColor = System.Drawing.Color.White;
        this.ClientSize = new System.Drawing.Size(920, 500);
        this.Controls.Add(this.lblTitle);
        this.Controls.Add(this.btnAddZone);
        this.Controls.Add(this.btnDeleteZone);
        this.Controls.Add(this.lblStatus);
        this.Controls.Add(this.grid);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.Name = "ManageZonesForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Quản lý khu vực trong kho";
        ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Button btnAddZone;
    private System.Windows.Forms.Button btnDeleteZone;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.DataGridView grid;
}