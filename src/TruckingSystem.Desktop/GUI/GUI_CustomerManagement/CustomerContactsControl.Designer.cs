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
        _edit = new Button();
        _delete = new Button();
        _refresh = new Button();
        filter = new FlowLayoutPanel();
        lblCustomer = new Label();
        _customer = new ComboBox();
        _grid = new DataGridView();
        _status = new Label();
        layout.SuspendLayout();
        toolbar.SuspendLayout();
        filter.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_grid).BeginInit();
        SuspendLayout();
        // 
        // layout
        // 
        layout.ColumnCount = 1;
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.Controls.Add(toolbar, 0, 0);
        layout.Controls.Add(filter, 0, 1);
        layout.Controls.Add(_grid, 0, 2);
        layout.Controls.Add(_status, 0, 3);
        layout.Dock = DockStyle.Fill;
        layout.Location = new Point(0, 0);
        layout.Name = "layout";
        layout.RowCount = 4;
        layout.RowStyles.Add(new RowStyle());
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
        toolbar.Controls.Add(_edit);
        toolbar.Controls.Add(_delete);
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
        // _edit
        // 
        _edit.BackColor = Color.White;
        _edit.Cursor = Cursors.Hand;
        _edit.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
        _edit.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
        _edit.FlatStyle = FlatStyle.Flat;
        _edit.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _edit.ForeColor = Color.FromArgb(15, 23, 42);
        _edit.Location = new Point(199, 7);
        _edit.Name = "_edit";
        _edit.Size = new Size(90, 38);
        _edit.TabIndex = 1;
        _edit.Text = "Sửa";
        _edit.UseVisualStyleBackColor = false;
        _edit.Click += BtnEdit_Click;
        // 
        // _delete
        // 
        _delete.BackColor = Color.White;
        _delete.Cursor = Cursors.Hand;
        _delete.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
        _delete.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
        _delete.FlatStyle = FlatStyle.Flat;
        _delete.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _delete.ForeColor = Color.FromArgb(220, 38, 38);
        _delete.Location = new Point(295, 7);
        _delete.Name = "_delete";
        _delete.Size = new Size(90, 38);
        _delete.TabIndex = 2;
        _delete.Text = "Xóa";
        _delete.UseVisualStyleBackColor = false;
        _delete.Click += BtnDelete_Click;
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
        _refresh.TabIndex = 3;
        _refresh.Text = "Làm mới";
        _refresh.UseVisualStyleBackColor = false;
        _refresh.Click += BtnRefresh_Click;
        // 
        // filter
        // 
        filter.AutoSize = true;
        filter.Controls.Add(lblCustomer);
        filter.Controls.Add(_customer);
        filter.Dock = DockStyle.Fill;
        filter.Location = new Point(3, 65);
        filter.Name = "filter";
        filter.Padding = new Padding(0, 0, 0, 8);
        filter.Size = new Size(894, 37);
        filter.TabIndex = 1;
        // 
        // lblCustomer
        // 
        lblCustomer.AutoSize = true;
        lblCustomer.Location = new Point(0, 7);
        lblCustomer.Margin = new Padding(0, 7, 12, 0);
        lblCustomer.Name = "lblCustomer";
        lblCustomer.Size = new Size(81, 19);
        lblCustomer.TabIndex = 0;
        lblCustomer.Text = "Khách hàng";
        // 
        // _customer
        // 
        _customer.AccessibleName = "Lọc người liên hệ theo khách hàng";
        _customer.DisplayMember = "Name";
        _customer.DropDownStyle = ComboBoxStyle.DropDownList;
        _customer.FormattingEnabled = true;
        _customer.Location = new Point(96, 3);
        _customer.Name = "_customer";
        _customer.Size = new Size(320, 25);
        _customer.TabIndex = 1;
        _customer.ValueMember = "Id";
        _customer.SelectedIndexChanged += Customer_SelectedIndexChanged;
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
        _grid.TabIndex = 2;
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
        _status.TabIndex = 3;
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
        filter.ResumeLayout(false);
        filter.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_grid).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private TableLayoutPanel layout = null!;
    private FlowLayoutPanel toolbar = null!;
    private Button _create = null!;
    private Button _edit = null!;
    private Button _delete = null!;
    private Button _refresh = null!;
    private FlowLayoutPanel filter = null!;
    private Label lblCustomer = null!;
    private ComboBox _customer = null!;
    private DataGridView _grid = null!;
    private DataGridViewTextBoxColumn colCustomer = null!;
    private DataGridViewTextBoxColumn colName = null!;
    private DataGridViewTextBoxColumn colPhone = null!;
    private DataGridViewTextBoxColumn colPosition = null!;
    private Label _status = null!;
}
