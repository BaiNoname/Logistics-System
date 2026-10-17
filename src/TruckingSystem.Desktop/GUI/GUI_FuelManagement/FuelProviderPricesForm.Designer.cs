namespace TruckingSystem.Desktop.GUI_FuelManagement
{
    partial class FuelProviderPricesForm
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>Bố cục: root (TableLayoutPanel 1 cột) = tiêu đề / hàng nhập liệu / trạng thái /
        /// lưới (chiếm phần còn lại). Hàng nhập liệu là TableLayoutPanel: nhãn ở hàng trên, ô nhập
        /// ở hàng dưới, các ô giãn theo % bề ngang; nút tự co theo chữ. Không có tọa độ cứng.</summary>
        private void InitializeComponent()
        {
            root = new TableLayoutPanel();
            lblTitle = new Label();
            inputs = new TableLayoutPanel();
            lblFuelType = new Label();
            lblPrice = new Label();
            lblEffectiveDate = new Label();
            cboFuelType = new ComboBox();
            numPrice = new NumericUpDown();
            dtEffectiveDate = new DateTimePicker();
            btnAddPrice = new Button();
            btnDeletePrice = new Button();
            filterBar = new TruckingSystem.Desktop.Common.FilterBar();
            lblStatus = new Label();
            grid = new DataGridView();
            root.SuspendLayout();
            inputs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numPrice).BeginInit();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            // 
            // root
            // 
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            root.Controls.Add(lblTitle, 0, 0);
            root.Controls.Add(inputs, 0, 1);
            root.Controls.Add(filterBar, 0, 2);
            root.Controls.Add(lblStatus, 0, 3);
            root.Controls.Add(grid, 0, 4);
            root.Dock = DockStyle.Fill;
            root.Location = new Point(0, 0);
            root.Margin = new Padding(4);
            root.Name = "root";
            root.Padding = new Padding(30, 24, 30, 24);
            root.RowCount = 5;
            root.RowStyles.Add(new RowStyle());
            root.RowStyles.Add(new RowStyle());
            root.RowStyles.Add(new RowStyle());
            root.RowStyles.Add(new RowStyle());
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.Size = new Size(1500, 930);
            root.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(30, 24);
            lblTitle.Margin = new Padding(0, 0, 0, 18);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(306, 36);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "⛽ Lịch sử giá xăng dầu";
            // 
            // inputs
            // 
            inputs.AutoSize = true;
            inputs.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            inputs.ColumnCount = 5;
            inputs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            inputs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            inputs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            inputs.ColumnStyles.Add(new ColumnStyle());
            inputs.ColumnStyles.Add(new ColumnStyle());
            inputs.Controls.Add(lblFuelType, 0, 0);
            inputs.Controls.Add(lblPrice, 1, 0);
            inputs.Controls.Add(lblEffectiveDate, 2, 0);
            inputs.Controls.Add(cboFuelType, 0, 1);
            inputs.Controls.Add(numPrice, 1, 1);
            inputs.Controls.Add(dtEffectiveDate, 2, 1);
            inputs.Controls.Add(btnAddPrice, 3, 1);
            inputs.Controls.Add(btnDeletePrice, 4, 1);
            inputs.Dock = DockStyle.Fill;
            inputs.Location = new Point(30, 78);
            inputs.Margin = new Padding(0, 0, 0, 12);
            inputs.Name = "inputs";
            inputs.RowCount = 2;
            inputs.RowStyles.Add(new RowStyle());
            inputs.RowStyles.Add(new RowStyle());
            inputs.Size = new Size(1440, 82);
            inputs.TabIndex = 1;
            // 
            // lblFuelType
            // 
            lblFuelType.AutoSize = true;
            lblFuelType.Font = new Font("Segoe UI", 9F);
            lblFuelType.ForeColor = Color.FromArgb(100, 116, 139);
            lblFuelType.Location = new Point(0, 0);
            lblFuelType.Margin = new Padding(0, 0, 18, 6);
            lblFuelType.Name = "lblFuelType";
            lblFuelType.Size = new Size(124, 25);
            lblFuelType.TabIndex = 0;
            lblFuelType.Text = "Loại nhiên liệu";
            // 
            // lblPrice
            // 
            lblPrice.AutoSize = true;
            lblPrice.Font = new Font("Segoe UI", 9F);
            lblPrice.ForeColor = Color.FromArgb(100, 116, 139);
            lblPrice.Location = new Point(405, 0);
            lblPrice.Margin = new Padding(0, 0, 18, 6);
            lblPrice.Name = "lblPrice";
            lblPrice.Size = new Size(63, 25);
            lblPrice.TabIndex = 1;
            lblPrice.Text = "Giá (đ)";
            // 
            // lblEffectiveDate
            // 
            lblEffectiveDate.AutoSize = true;
            lblEffectiveDate.Font = new Font("Segoe UI", 9F);
            lblEffectiveDate.ForeColor = Color.FromArgb(100, 116, 139);
            lblEffectiveDate.Location = new Point(708, 0);
            lblEffectiveDate.Margin = new Padding(0, 0, 18, 6);
            lblEffectiveDate.Name = "lblEffectiveDate";
            lblEffectiveDate.Size = new Size(120, 25);
            lblEffectiveDate.TabIndex = 2;
            lblEffectiveDate.Text = "Ngày hiệu lực";
            // 
            // cboFuelType
            // 
            cboFuelType.Dock = DockStyle.Fill;
            cboFuelType.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFuelType.Location = new Point(0, 31);
            cboFuelType.Margin = new Padding(0, 0, 18, 0);
            cboFuelType.Name = "cboFuelType";
            cboFuelType.Size = new Size(387, 36);
            cboFuelType.TabIndex = 3;
            // 
            // numPrice
            // 
            numPrice.Dock = DockStyle.Fill;
            numPrice.Location = new Point(405, 31);
            numPrice.Margin = new Padding(0, 0, 18, 0);
            numPrice.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            numPrice.Name = "numPrice";
            numPrice.Size = new Size(285, 34);
            numPrice.TabIndex = 4;
            numPrice.ThousandsSeparator = true;
            // 
            // dtEffectiveDate
            // 
            dtEffectiveDate.CustomFormat = "dd/MM/yyyy";
            dtEffectiveDate.Dock = DockStyle.Fill;
            dtEffectiveDate.Format = DateTimePickerFormat.Custom;
            dtEffectiveDate.Location = new Point(708, 31);
            dtEffectiveDate.Margin = new Padding(0, 0, 18, 0);
            dtEffectiveDate.Name = "dtEffectiveDate";
            dtEffectiveDate.Size = new Size(285, 34);
            dtEffectiveDate.TabIndex = 5;
            // 
            // btnAddPrice
            // 
            btnAddPrice.Anchor = AnchorStyles.Left;
            btnAddPrice.AutoSize = true;
            btnAddPrice.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnAddPrice.BackColor = Color.FromArgb(37, 99, 235);
            btnAddPrice.Cursor = Cursors.Hand;
            btnAddPrice.FlatAppearance.BorderSize = 0;
            btnAddPrice.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
            btnAddPrice.FlatStyle = FlatStyle.Flat;
            btnAddPrice.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnAddPrice.ForeColor = Color.White;
            btnAddPrice.Location = new Point(1011, 31);
            btnAddPrice.Margin = new Padding(0, 0, 12, 0);
            btnAddPrice.MinimumSize = new Size(0, 51);
            btnAddPrice.Name = "btnAddPrice";
            btnAddPrice.Padding = new Padding(15, 3, 15, 3);
            btnAddPrice.Size = new Size(174, 51);
            btnAddPrice.TabIndex = 6;
            btnAddPrice.Text = "➕ Thêm giá";
            btnAddPrice.UseVisualStyleBackColor = false;
            btnAddPrice.Click += BtnAddPrice_Click;
            // 
            // btnDeletePrice
            // 
            btnDeletePrice.Anchor = AnchorStyles.Left;
            btnDeletePrice.AutoSize = true;
            btnDeletePrice.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnDeletePrice.BackColor = Color.White;
            btnDeletePrice.Cursor = Cursors.Hand;
            btnDeletePrice.FlatAppearance.BorderColor = Color.FromArgb(220, 38, 38);
            btnDeletePrice.FlatStyle = FlatStyle.Flat;
            btnDeletePrice.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnDeletePrice.ForeColor = Color.FromArgb(220, 38, 38);
            btnDeletePrice.Location = new Point(1197, 31);
            btnDeletePrice.Margin = new Padding(0);
            btnDeletePrice.MinimumSize = new Size(0, 51);
            btnDeletePrice.Name = "btnDeletePrice";
            btnDeletePrice.Padding = new Padding(15, 3, 15, 3);
            btnDeletePrice.Size = new Size(241, 51);
            btnDeletePrice.TabIndex = 7;
            btnDeletePrice.Text = "🗑️ Xóa giá đã chọn";
            btnDeletePrice.UseVisualStyleBackColor = false;
            btnDeletePrice.Click += BtnDeletePrice_Click;
            // 
            // filterBar
            // 
            filterBar.AutoSize = true;
            filterBar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            filterBar.Dock = DockStyle.Fill;
            filterBar.Location = new Point(30, 172);
            filterBar.Margin = new Padding(0, 0, 0, 9);
            filterBar.Name = "filterBar";
            filterBar.SearchPlaceholder = "Loại nhiên liệu, giá, ngày...";
            filterBar.Size = new Size(1440, 46);
            filterBar.TabIndex = 2;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
            lblStatus.Location = new Point(30, 233);
            lblStatus.Margin = new Padding(0, 6, 0, 12);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(1440, 25);
            lblStatus.TabIndex = 2;
            // 
            // grid
            // 
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid.Dock = DockStyle.Fill;
            grid.Location = new Point(30, 270);
            grid.Margin = new Padding(0);
            grid.Name = "grid";
            grid.RowHeadersWidth = 62;
            grid.Size = new Size(1440, 636);
            grid.TabIndex = 3;
            // 
            // FuelProviderPricesForm
            // 
            AutoScaleDimensions = new SizeF(144F, 144F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(1500, 930);
            Controls.Add(root);
            Margin = new Padding(6);
            MinimumSize = new Size(1129, 662);
            Name = "FuelProviderPricesForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Giá xăng dầu";
            root.ResumeLayout(false);
            root.PerformLayout();
            inputs.ResumeLayout(false);
            inputs.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numPrice).EndInit();
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.TableLayoutPanel inputs;
        private System.Windows.Forms.Label lblFuelType;
        private System.Windows.Forms.ComboBox cboFuelType;
        private System.Windows.Forms.Label lblPrice;
        private System.Windows.Forms.NumericUpDown numPrice;
        private System.Windows.Forms.Label lblEffectiveDate;
        private System.Windows.Forms.DateTimePicker dtEffectiveDate;
        private System.Windows.Forms.Button btnAddPrice;
        private System.Windows.Forms.Button btnDeletePrice;
        private System.Windows.Forms.Label lblStatus;
        private TruckingSystem.Desktop.Common.FilterBar filterBar;
        private System.Windows.Forms.DataGridView grid;
    }
}
