namespace TruckingSystem.Desktop.GUI_CustomerManagement;

partial class CustomersControl
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
        this.btnRefresh = new System.Windows.Forms.Button();
        this.btnCreate = new System.Windows.Forms.Button();
        this.lblStatus = new System.Windows.Forms.Label();
        this.grid = new System.Windows.Forms.DataGridView();
        this.SuspendLayout();
        //
        // lblTitle
        //
        this.lblTitle.Location = new System.Drawing.Point(0, 0);
        this.lblTitle.Size = new System.Drawing.Size(400, 32);
        this.lblTitle.Text = "Khách hàng";
        this.lblTitle.Font = TruckingSystem.Desktop.Common.Theme.FontTitle;
        this.lblTitle.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
        //
        // btnRefresh
        //
        this.btnRefresh.Location = new System.Drawing.Point(760, 0);
        this.btnRefresh.Size = new System.Drawing.Size(110, 38);
        this.btnRefresh.Text = "🔄 Làm mới";
        this.btnRefresh.BackColor = System.Drawing.Color.White;
        this.btnRefresh.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
        this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnRefresh.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
        this.btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnRefresh.FlatAppearance.BorderSize = 1;
        this.btnRefresh.FlatAppearance.MouseOverBackColor = TruckingSystem.Desktop.Common.Theme.GridAltRow;
        this.btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
        //
        // btnCreate
        //
        this.btnCreate.Location = new System.Drawing.Point(880, 0);
        this.btnCreate.Size = new System.Drawing.Size(170, 38);
        this.btnCreate.Text = "➕ Thêm khách hàng";
        this.btnCreate.BackColor = TruckingSystem.Desktop.Common.Theme.Primary;
        this.btnCreate.ForeColor = System.Drawing.Color.White;
        this.btnCreate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnCreate.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
        this.btnCreate.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnCreate.FlatAppearance.BorderSize = 0;
        this.btnCreate.FlatAppearance.MouseOverBackColor = TruckingSystem.Desktop.Common.Theme.PrimaryDark;
        this.btnCreate.Click += new System.EventHandler(this.BtnCreate_Click);
        //
        // lblStatus
        //
        this.lblStatus.Location = new System.Drawing.Point(0, 44);
        this.lblStatus.AutoSize = true;
        this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblStatus.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        //
        // grid
        //
        ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
        this.grid.Location = new System.Drawing.Point(0, 76);
        this.grid.Size = new System.Drawing.Size(1000, 560);
        this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        //
        // CustomersControl
        //
        this.BackColor = TruckingSystem.Desktop.Common.Theme.ContentBg;
        this.Dock = System.Windows.Forms.DockStyle.Fill;
        this.Controls.Add(this.lblTitle);
        this.Controls.Add(this.btnRefresh);
        this.Controls.Add(this.btnCreate);
        this.Controls.Add(this.lblStatus);
        this.Controls.Add(this.grid);
        this.Name = "CustomersControl";
        ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Button btnRefresh;
    private System.Windows.Forms.Button btnCreate;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.DataGridView grid;
}
