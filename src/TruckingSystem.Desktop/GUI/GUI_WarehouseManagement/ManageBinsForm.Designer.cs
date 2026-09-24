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
        lblTitle = new Label();
        btnAddBin = new Button();
        btnEditBin = new Button();
        btnDeleteBin = new Button();
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
        lblTitle.Size = new Size(277, 48);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "Vị trí lưu trữ";
        // 
        // btnAddBin
        // 
        btnAddBin.BackColor = Color.FromArgb(37, 99, 235);
        btnAddBin.Cursor = Cursors.Hand;
        btnAddBin.FlatAppearance.BorderSize = 0;
        btnAddBin.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
        btnAddBin.FlatStyle = FlatStyle.Flat;
        btnAddBin.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnAddBin.ForeColor = Color.White;
        btnAddBin.Location = new Point(503, 24);
        btnAddBin.Margin = new Padding(4, 4, 4, 4);
        btnAddBin.Name = "btnAddBin";
        btnAddBin.Size = new Size(240, 57);
        btnAddBin.TabIndex = 1;
        btnAddBin.Text = "➕ Thêm vị trí mới";
        btnAddBin.UseVisualStyleBackColor = false;
        btnAddBin.Click += BtnAddBin_Click;
        // 
        // btnEditBin
        // 
        btnEditBin.BackColor = Color.White;
        btnEditBin.Cursor = Cursors.Hand;
        btnEditBin.FlatStyle = FlatStyle.Flat;
        btnEditBin.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnEditBin.ForeColor = Color.FromArgb(15, 23, 42);
        btnEditBin.Location = new Point(751, 24);
        btnEditBin.Margin = new Padding(4, 4, 4, 4);
        btnEditBin.Name = "btnEditBin";
        btnEditBin.Size = new Size(195, 57);
        btnEditBin.TabIndex = 2;
        btnEditBin.Text = "✏️ Sửa vị trí";
        btnEditBin.UseVisualStyleBackColor = false;
        btnEditBin.Click += BtnEditBin_Click;
        // 
        // btnDeleteBin
        // 
        btnDeleteBin.BackColor = Color.White;
        btnDeleteBin.Cursor = Cursors.Hand;
        btnDeleteBin.FlatAppearance.BorderColor = Color.FromArgb(220, 38, 38);
        btnDeleteBin.FlatStyle = FlatStyle.Flat;
        btnDeleteBin.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnDeleteBin.ForeColor = Color.FromArgb(220, 38, 38);
        btnDeleteBin.Location = new Point(954, 24);
        btnDeleteBin.Margin = new Padding(4, 4, 4, 4);
        btnDeleteBin.Name = "btnDeleteBin";
        btnDeleteBin.Size = new Size(210, 57);
        btnDeleteBin.TabIndex = 3;
        btnDeleteBin.Text = "🗑️ Xóa vị trí";
        btnDeleteBin.UseVisualStyleBackColor = false;
        btnDeleteBin.Click += BtnDeleteBin_Click;
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
        // ManageBinsForm
        // 
        AutoScaleDimensions = new SizeF(144F, 144F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.White;
        ClientSize = new Size(1590, 750);
        Controls.Add(lblTitle);
        Controls.Add(btnAddBin);
        Controls.Add(btnEditBin);
        Controls.Add(btnDeleteBin);
        Controls.Add(lblStatus);
        Controls.Add(grid);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        Margin = new Padding(6, 6, 6, 6);
        MaximizeBox = false;
        Name = "ManageBinsForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Vị trí lưu trữ";
        ((System.ComponentModel.ISupportInitialize)grid).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Button btnAddBin;
    private System.Windows.Forms.Button btnEditBin;
    private System.Windows.Forms.Button btnDeleteBin;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.DataGridView grid;
}