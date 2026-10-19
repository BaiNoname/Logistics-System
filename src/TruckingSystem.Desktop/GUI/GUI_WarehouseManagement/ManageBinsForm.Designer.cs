namespace TruckingSystem.Desktop.GUI_WarehouseManagement;

partial class ManageBinsForm
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
        btnAddBin = new System.Windows.Forms.Button();
        btnEditBin = new System.Windows.Forms.Button();
        btnDeleteBin = new System.Windows.Forms.Button();
        lblStatus = new System.Windows.Forms.Label();
        filterBar = new TruckingSystem.Desktop.Common.FilterBar();
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
        root.Controls.Add(filterBar, 0, 2);
        root.Controls.Add(lblStatus, 0, 3);
        root.Controls.Add(grid, 0, 4);
        root.Dock = System.Windows.Forms.DockStyle.Fill;
        root.Margin = new System.Windows.Forms.Padding(0);
        root.Name = "root";
        root.RowCount = 5;
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
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
        lblTitle.Text = "Vị trí lưu trữ";
        // 
        // toolbar
        // 
        toolbar.AutoSize = true;
        toolbar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        toolbar.Controls.Add(btnAddBin);
        toolbar.Controls.Add(btnEditBin);
        toolbar.Controls.Add(btnDeleteBin);
        toolbar.Dock = System.Windows.Forms.DockStyle.Fill;
        toolbar.Margin = new System.Windows.Forms.Padding(0);
        toolbar.Name = "toolbar";
        toolbar.TabIndex = 1;
        toolbar.WrapContents = true;
        // 
        // btnAddBin
        // 
        btnAddBin.AutoSize = true;
        btnAddBin.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        btnAddBin.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
        btnAddBin.Cursor = System.Windows.Forms.Cursors.Hand;
        btnAddBin.FlatAppearance.BorderSize = 0;
        btnAddBin.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
        btnAddBin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnAddBin.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        btnAddBin.ForeColor = System.Drawing.Color.White;
        btnAddBin.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
        btnAddBin.MinimumSize = new System.Drawing.Size(0, 38);
        btnAddBin.Name = "btnAddBin";
        btnAddBin.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        btnAddBin.TabIndex = 0;
        btnAddBin.Text = "➕ Thêm vị trí mới";
        btnAddBin.UseVisualStyleBackColor = false;
        btnAddBin.Click += new System.EventHandler(this.BtnAddBin_Click);
        // 
        // btnEditBin
        // 
        btnEditBin.AutoSize = true;
        btnEditBin.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        btnEditBin.BackColor = System.Drawing.Color.White;
        btnEditBin.Cursor = System.Windows.Forms.Cursors.Hand;
        btnEditBin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnEditBin.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        btnEditBin.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
        btnEditBin.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
        btnEditBin.MinimumSize = new System.Drawing.Size(0, 38);
        btnEditBin.Name = "btnEditBin";
        btnEditBin.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        btnEditBin.TabIndex = 1;
        btnEditBin.Text = "✏️ Sửa vị trí";
        btnEditBin.UseVisualStyleBackColor = false;
        btnEditBin.Click += new System.EventHandler(this.BtnEditBin_Click);
        // 
        // btnDeleteBin
        // 
        btnDeleteBin.AutoSize = true;
        btnDeleteBin.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        btnDeleteBin.BackColor = System.Drawing.Color.White;
        btnDeleteBin.Cursor = System.Windows.Forms.Cursors.Hand;
        btnDeleteBin.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(220, 38, 38);
        btnDeleteBin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnDeleteBin.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        btnDeleteBin.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
        btnDeleteBin.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
        btnDeleteBin.MinimumSize = new System.Drawing.Size(0, 38);
        btnDeleteBin.Name = "btnDeleteBin";
        btnDeleteBin.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        btnDeleteBin.TabIndex = 2;
        btnDeleteBin.Text = "🗑️ Xóa vị trí";
        btnDeleteBin.UseVisualStyleBackColor = false;
        btnDeleteBin.Click += new System.EventHandler(this.BtnDeleteBin_Click);
        // 
        // filterBar — tìm nhanh + lọc (Common/FilterBar); các ô chọn thêm ở .cs
        // 
        filterBar.Dock = System.Windows.Forms.DockStyle.Fill;
        filterBar.Name = "filterBar";
        filterBar.SearchPlaceholder = "Mã ô, khu, trạng thái...";
        filterBar.TabIndex = 2;
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
        // ManageBinsForm
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.White;
        ClientSize = new System.Drawing.Size(1000, 620);
        Controls.Add(root);
        MinimumSize = new System.Drawing.Size(720, 420);
        Name = "ManageBinsForm";
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        Text = "Vị trí lưu trữ";
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
    private System.Windows.Forms.Button btnAddBin;
    private System.Windows.Forms.Button btnEditBin;
    private System.Windows.Forms.Button btnDeleteBin;
    private System.Windows.Forms.Label lblStatus;
    private TruckingSystem.Desktop.Common.FilterBar filterBar;
    private System.Windows.Forms.DataGridView grid;
}
