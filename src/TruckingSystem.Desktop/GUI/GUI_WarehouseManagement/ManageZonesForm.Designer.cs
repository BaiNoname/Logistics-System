namespace TruckingSystem.Desktop.GUI_WarehouseManagement;

partial class ManageZonesForm
{
    private System.ComponentModel.IContainer components = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>Bố cục: root (TableLayoutPanel 1 cột) = tiêu đề / thanh nút (FlowLayoutPanel tự xuống
    /// dòng) / trạng thái / lưới (chiếm phần còn lại). Không tọa độ cứng — co giãn theo DPI và cỡ cửa sổ.</summary>
    private void InitializeComponent()
    {
        root = new System.Windows.Forms.TableLayoutPanel();
        lblTitle = new System.Windows.Forms.Label();
        toolbar = new System.Windows.Forms.FlowLayoutPanel();
        btnAddZone = new System.Windows.Forms.Button();
        btnEditZone = new System.Windows.Forms.Button();
        btnDeleteZone = new System.Windows.Forms.Button();
        lblStatus = new System.Windows.Forms.Label();
        grid = new System.Windows.Forms.DataGridView();
        root.SuspendLayout();
        toolbar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
        SuspendLayout();
        // 
        // root
        // 
        root.ColumnCount = 1;
        root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        root.Controls.Add(lblTitle, 0, 0);
        root.Controls.Add(toolbar, 0, 1);
        root.Controls.Add(lblStatus, 0, 2);
        root.Controls.Add(grid, 0, 3);
        root.Dock = System.Windows.Forms.DockStyle.Fill;
        root.Margin = new System.Windows.Forms.Padding(0);
        root.Name = "root";
        root.RowCount = 4;
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        root.TabIndex = 0;
        root.Padding = new System.Windows.Forms.Padding(20, 16, 20, 16);
        root.Size = new System.Drawing.Size(1000, 620);
        // 
        // lblTitle
        // 
        lblTitle.AutoSize = true;
        lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
        lblTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
        lblTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
        lblTitle.Name = "lblTitle";
        lblTitle.TabIndex = 0;
        lblTitle.Text = "Quản lý khu vực trong kho";
        // 
        // toolbar
        // 
        toolbar.AutoSize = true;
        toolbar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        toolbar.Controls.Add(btnAddZone);
        toolbar.Controls.Add(btnEditZone);
        toolbar.Controls.Add(btnDeleteZone);
        toolbar.Dock = System.Windows.Forms.DockStyle.Fill;
        toolbar.Margin = new System.Windows.Forms.Padding(0);
        toolbar.Name = "toolbar";
        toolbar.TabIndex = 1;
        toolbar.WrapContents = true;
        // 
        // btnAddZone
        // 
        btnAddZone.AutoSize = true;
        btnAddZone.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        btnAddZone.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
        btnAddZone.Cursor = System.Windows.Forms.Cursors.Hand;
        btnAddZone.FlatAppearance.BorderSize = 0;
        btnAddZone.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
        btnAddZone.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnAddZone.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        btnAddZone.ForeColor = System.Drawing.Color.White;
        btnAddZone.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
        btnAddZone.MinimumSize = new System.Drawing.Size(0, 38);
        btnAddZone.Name = "btnAddZone";
        btnAddZone.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        btnAddZone.TabIndex = 0;
        btnAddZone.Text = "➕ Thêm khu vực mới";
        btnAddZone.UseVisualStyleBackColor = false;
        btnAddZone.Click += new System.EventHandler(this.BtnAddZone_Click);
        // 
        // btnEditZone
        // 
        btnEditZone.AutoSize = true;
        btnEditZone.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        btnEditZone.BackColor = System.Drawing.Color.White;
        btnEditZone.Cursor = System.Windows.Forms.Cursors.Hand;
        btnEditZone.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnEditZone.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        btnEditZone.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
        btnEditZone.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
        btnEditZone.MinimumSize = new System.Drawing.Size(0, 38);
        btnEditZone.Name = "btnEditZone";
        btnEditZone.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        btnEditZone.TabIndex = 1;
        btnEditZone.Text = "✏️ Sửa khu vực";
        btnEditZone.UseVisualStyleBackColor = false;
        btnEditZone.Click += new System.EventHandler(this.BtnEditZone_Click);
        // 
        // btnDeleteZone
        // 
        btnDeleteZone.AutoSize = true;
        btnDeleteZone.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        btnDeleteZone.BackColor = System.Drawing.Color.White;
        btnDeleteZone.Cursor = System.Windows.Forms.Cursors.Hand;
        btnDeleteZone.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(220, 38, 38);
        btnDeleteZone.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnDeleteZone.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        btnDeleteZone.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
        btnDeleteZone.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
        btnDeleteZone.MinimumSize = new System.Drawing.Size(0, 38);
        btnDeleteZone.Name = "btnDeleteZone";
        btnDeleteZone.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        btnDeleteZone.TabIndex = 2;
        btnDeleteZone.Text = "🗑️ Xóa khu vực";
        btnDeleteZone.UseVisualStyleBackColor = false;
        btnDeleteZone.Click += new System.EventHandler(this.BtnDeleteZone_Click);
        // 
        // lblStatus
        // 
        lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblStatus.AutoSize = true;
        lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblStatus.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblStatus.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
        lblStatus.Name = "lblStatus";
        lblStatus.TabIndex = 2;
        // 
        // grid
        // 
        grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        grid.Dock = System.Windows.Forms.DockStyle.Fill;
        grid.Margin = new System.Windows.Forms.Padding(0);
        grid.Name = "grid";
        grid.TabIndex = 3;
        // 
        // ManageZonesForm
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.White;
        ClientSize = new System.Drawing.Size(1000, 620);
        Controls.Add(root);
        MinimumSize = new System.Drawing.Size(720, 420);
        Name = "ManageZonesForm";
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        Text = "Quản lý khu vực trong kho";
        ((System.ComponentModel.ISupportInitialize)grid).EndInit();
        toolbar.ResumeLayout(false);
        toolbar.PerformLayout();
        root.ResumeLayout(false);
        root.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel root;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.FlowLayoutPanel toolbar;
    private System.Windows.Forms.Button btnAddZone;
    private System.Windows.Forms.Button btnEditZone;
    private System.Windows.Forms.Button btnDeleteZone;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.DataGridView grid;
}
