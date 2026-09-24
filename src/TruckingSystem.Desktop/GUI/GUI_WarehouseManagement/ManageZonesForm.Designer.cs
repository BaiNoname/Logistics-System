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
        lblTitle = new Label();
        btnAddZone = new Button();
        btnEditZone = new Button();
        btnDeleteZone = new Button();
        lblStatus = new Label();
        grid = new DataGridView();
        ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
        SuspendLayout();
        // 
        // lblTitle
        // 
        lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
        lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
        lblTitle.Location = new Point(30, 24);
        lblTitle.Margin = new Padding(4, 0, 4, 0);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(600, 48);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "Quản lý khu vực trong kho";
        // 
        // btnAddZone
        // 
        btnAddZone.BackColor = Color.FromArgb(37, 99, 235);
        btnAddZone.Cursor = Cursors.Hand;
        btnAddZone.FlatAppearance.BorderSize = 0;
        btnAddZone.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
        btnAddZone.FlatStyle = FlatStyle.Flat;
        btnAddZone.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnAddZone.ForeColor = Color.White;
        btnAddZone.Location = new Point(886, 24);
        btnAddZone.Margin = new Padding(4, 4, 4, 4);
        btnAddZone.Name = "btnAddZone";
        btnAddZone.Size = new Size(240, 57);
        btnAddZone.TabIndex = 1;
        btnAddZone.Text = "➕ Thêm khu vực mới";
        btnAddZone.UseVisualStyleBackColor = false;
        btnAddZone.Click += BtnAddZone_Click;
        // 
        // btnEditZone
        // 
        btnEditZone.BackColor = Color.White;
        btnEditZone.Cursor = Cursors.Hand;
        btnEditZone.FlatStyle = FlatStyle.Flat;
        btnEditZone.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnEditZone.ForeColor = Color.FromArgb(15, 23, 42);
        btnEditZone.Location = new Point(676, 24);
        btnEditZone.Margin = new Padding(4, 4, 4, 4);
        btnEditZone.Name = "btnEditZone";
        btnEditZone.Size = new Size(195, 57);
        btnEditZone.TabIndex = 2;
        btnEditZone.Text = "✏️ Sửa khu vực";
        btnEditZone.UseVisualStyleBackColor = false;
        btnEditZone.Click += BtnEditZone_Click;
        // 
        // btnDeleteZone
        // 
        btnDeleteZone.BackColor = Color.White;
        btnDeleteZone.Cursor = Cursors.Hand;
        btnDeleteZone.FlatAppearance.BorderColor = Color.FromArgb(220, 38, 38);
        btnDeleteZone.FlatStyle = FlatStyle.Flat;
        btnDeleteZone.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnDeleteZone.ForeColor = Color.FromArgb(220, 38, 38);
        btnDeleteZone.Location = new Point(1141, 24);
        btnDeleteZone.Margin = new Padding(4, 4, 4, 4);
        btnDeleteZone.Name = "btnDeleteZone";
        btnDeleteZone.Size = new Size(210, 57);
        btnDeleteZone.TabIndex = 3;
        btnDeleteZone.Text = "🗑️ Xóa khu vực";
        btnDeleteZone.UseVisualStyleBackColor = false;
        btnDeleteZone.Click += BtnDeleteZone_Click;
        // 
        // lblStatus
        // 
        lblStatus.AutoSize = true;
        lblStatus.Font = new Font("Segoe UI", 9F);
        lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
        lblStatus.Location = new Point(30, 84);
        lblStatus.Margin = new Padding(4, 0, 4, 0);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(0, 25);
        lblStatus.TabIndex = 4;
        // 
        // grid
        // 
        grid.ColumnHeadersHeight = 34;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.Location = new Point(30, 114);
        grid.Margin = new Padding(4, 4, 4, 4);
        grid.Name = "grid";
        grid.RowHeadersWidth = 62;
        grid.Size = new Size(1530, 570);
        grid.TabIndex = 5;
        // 
        // ManageZonesForm
        // 
        AutoScaleDimensions = new SizeF(144F, 144F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.White;
        ClientSize = new Size(1590, 750);
        Controls.Add(lblTitle);
        Controls.Add(btnAddZone);
        Controls.Add(btnEditZone);
        Controls.Add(btnDeleteZone);
        Controls.Add(lblStatus);
        Controls.Add(grid);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        Margin = new Padding(6, 6, 6, 6);
        MaximizeBox = false;
        Name = "ManageZonesForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Quản lý khu vực trong kho";
        ((System.ComponentModel.ISupportInitialize)grid).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Button btnAddZone;
    private System.Windows.Forms.Button btnEditZone;
    private System.Windows.Forms.Button btnDeleteZone;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.DataGridView grid;
}