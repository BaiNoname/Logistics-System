namespace TruckingSystem.Desktop.GUI_CustomerManagement;

partial class CustomerContactsControl
{
    private System.ComponentModel.IContainer components = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    #region Component Designer generated code

    private void InitializeComponent()
    {
        layout = new TableLayoutPanel();
        toolbar = new FlowLayoutPanel();
        _create = new Button();
        _refresh = new Button();
        _grid = new DataGridView();
        _status = new Label();
        layout.SuspendLayout();
        toolbar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_grid).BeginInit();
        SuspendLayout();
        // 
        // layout
        // 
        layout.ColumnCount = 1;
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.Controls.Add(toolbar, 0, 0);
        layout.Controls.Add(_grid, 0, 1);
        layout.Controls.Add(_status, 0, 2);
        layout.Dock = DockStyle.Fill;
        layout.Location = new Point(0, 0);
        layout.Name = "layout";
        layout.RowCount = 3;
        layout.RowStyles.Add(new RowStyle());
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        layout.Size = new Size(900, 420);
        layout.TabIndex = 0;
        // 
        // toolbar
        // 
        toolbar.AutoSize = true;
        toolbar.Controls.Add(_create);
        toolbar.Controls.Add(_refresh);
        toolbar.Dock = DockStyle.Fill;
        toolbar.Location = new Point(3, 3);
        toolbar.Name = "toolbar";
        toolbar.Padding = new Padding(0, 4, 0, 8);
        toolbar.Size = new Size(894, 56);
        toolbar.TabIndex = 0;
        // 
        // _create
        // 
        _create.BackColor = Color.FromArgb(37, 99, 235);
        _create.Cursor = Cursors.Hand;
        _create.FlatAppearance.BorderSize = 0;
        _create.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
        _create.FlatStyle = FlatStyle.Flat;
        _create.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _create.ForeColor = Color.White;
        _create.Location = new Point(3, 7);
        _create.Name = "_create";
        _create.Size = new Size(190, 38);
        _create.TabIndex = 0;
        _create.Text = "Thêm người liên hệ";
        _create.UseVisualStyleBackColor = false;
        _create.Click += BtnCreate_Click;
        // 
        // _refresh
        // 
        _refresh.BackColor = Color.White;
        _refresh.Cursor = Cursors.Hand;
        _refresh.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
        _refresh.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
        _refresh.FlatStyle = FlatStyle.Flat;
        _refresh.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _refresh.ForeColor = Color.FromArgb(15, 23, 42);
        _refresh.Location = new Point(391, 7);
        _refresh.Name = "_refresh";
        _refresh.Size = new Size(120, 38);
        _refresh.TabIndex = 1;
        _refresh.Text = "Làm mới";
        _refresh.UseVisualStyleBackColor = false;
        _refresh.Click += BtnRefresh_Click;
        // 
        // _grid
        // 
        _grid.AccessibleName = "Danh sách người liên hệ khách hàng";
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AllowUserToResizeRows = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.BackgroundColor = Color.White;
        _grid.BorderStyle = BorderStyle.None;
        _grid.ColumnHeadersHeight = 42;
        _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        _grid.Dock = DockStyle.Fill;
        _grid.Location = new Point(3, 108);
        _grid.MultiSelect = false;
        _grid.Name = "_grid";
        _grid.ReadOnly = true;
        _grid.RowHeadersVisible = false;
        _grid.RowTemplate.Height = 36;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.Size = new Size(894, 277);
        _grid.TabIndex = 1;
        _grid.CellDoubleClick += Grid_CellDoubleClick;
        _grid.SelectionChanged += Grid_SelectionChanged;
        // 
        // _status
        // 
        _status.AutoEllipsis = true;
        _status.Dock = DockStyle.Fill;
        _status.ForeColor = Color.FromArgb(100, 116, 139);
        _status.Location = new Point(3, 388);
        _status.Name = "_status";
        _status.Size = new Size(894, 32);
        _status.TabIndex = 2;
        _status.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // CustomerContactsControl
        // 
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.White;
        Controls.Add(layout);
        Name = "CustomerContactsControl";
        Size = new Size(900, 420);
        layout.ResumeLayout(false);
        layout.PerformLayout();
        toolbar.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_grid).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private TableLayoutPanel layout = null!;
    private FlowLayoutPanel toolbar = null!;
    private Button _create = null!;
    private Button _refresh = null!;
    private DataGridView _grid = null!;
    private DataGridViewTextBoxColumn colCustomer = null!;
    private DataGridViewTextBoxColumn colName = null!;
    private DataGridViewTextBoxColumn colPhone = null!;
    private DataGridViewTextBoxColumn colPosition = null!;
    private Label _status = null!;
}
