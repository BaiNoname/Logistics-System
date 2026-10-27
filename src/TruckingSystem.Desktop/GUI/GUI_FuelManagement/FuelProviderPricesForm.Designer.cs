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
            root = new System.Windows.Forms.TableLayoutPanel();
            lblTitle = new System.Windows.Forms.Label();
            inputs = new System.Windows.Forms.TableLayoutPanel();
            lblFuelType = new System.Windows.Forms.Label();
            lblPrice = new System.Windows.Forms.Label();
            lblEffectiveDate = new System.Windows.Forms.Label();
            cboFuelType = new System.Windows.Forms.ComboBox();
            numPrice = new TruckingSystem.Desktop.Common.NumberBox();
            dtEffectiveDate = new System.Windows.Forms.DateTimePicker();
            btnAddPrice = new System.Windows.Forms.Button();
            btnDeletePrice = new System.Windows.Forms.Button();
            lblStatus = new System.Windows.Forms.Label();
            filterBar = new TruckingSystem.Desktop.Common.FilterBar();
            grid = new System.Windows.Forms.DataGridView();
            root.SuspendLayout();
            inputs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numPrice).BeginInit();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            // 
            // root
            // 
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.Controls.Add(lblTitle, 0, 0);
            root.Controls.Add(inputs, 0, 1);
            root.Controls.Add(filterBar, 0, 2);
            root.Controls.Add(lblStatus, 0, 3);
            root.Controls.Add(grid, 0, 4);
            root.Dock = System.Windows.Forms.DockStyle.Fill;
            root.Name = "root";
            root.Padding = new System.Windows.Forms.Padding(20, 16, 20, 16);
            root.RowCount = 5;
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.Size = new System.Drawing.Size(1000, 620);
            root.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            lblTitle.Name = "lblTitle";
            lblTitle.TabIndex = 0;
            lblTitle.Text = "⛽ Lịch sử giá xăng dầu";
            // 
            // inputs
            // 
            inputs.AutoSize = true;
            inputs.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            inputs.ColumnCount = 5;
            inputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            inputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            inputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            inputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            inputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            inputs.Controls.Add(lblFuelType, 0, 0);
            inputs.Controls.Add(lblPrice, 1, 0);
            inputs.Controls.Add(lblEffectiveDate, 2, 0);
            inputs.Controls.Add(cboFuelType, 0, 1);
            inputs.Controls.Add(numPrice, 1, 1);
            inputs.Controls.Add(dtEffectiveDate, 2, 1);
            inputs.Controls.Add(btnAddPrice, 3, 1);
            inputs.Controls.Add(btnDeletePrice, 4, 1);
            inputs.Dock = System.Windows.Forms.DockStyle.Fill;
            inputs.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            inputs.Name = "inputs";
            inputs.RowCount = 2;
            inputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            inputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            inputs.TabIndex = 1;
            // 
            // lblFuelType
            // 
            lblFuelType.AutoSize = true;
            lblFuelType.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblFuelType.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblFuelType.Margin = new System.Windows.Forms.Padding(0, 0, 12, 4);
            lblFuelType.Name = "lblFuelType";
            lblFuelType.TabIndex = 0;
            lblFuelType.Text = "Loại nhiên liệu";
            // 
            // lblPrice
            // 
            lblPrice.AutoSize = true;
            lblPrice.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblPrice.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblPrice.Margin = new System.Windows.Forms.Padding(0, 0, 12, 4);
            lblPrice.Name = "lblPrice";
            lblPrice.TabIndex = 1;
            lblPrice.Text = "Giá (đ)";
            // 
            // lblEffectiveDate
            // 
            lblEffectiveDate.AutoSize = true;
            lblEffectiveDate.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblEffectiveDate.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblEffectiveDate.Margin = new System.Windows.Forms.Padding(0, 0, 12, 4);
            lblEffectiveDate.Name = "lblEffectiveDate";
            lblEffectiveDate.TabIndex = 2;
            lblEffectiveDate.Text = "Ngày hiệu lực";
            // 
            // cboFuelType
            // 
            cboFuelType.Dock = System.Windows.Forms.DockStyle.Fill;
            cboFuelType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboFuelType.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            cboFuelType.Name = "cboFuelType";
            cboFuelType.TabIndex = 3;
            // 
            // numPrice
            // 
            numPrice.Dock = System.Windows.Forms.DockStyle.Fill;
            numPrice.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            numPrice.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            numPrice.Name = "numPrice";
            numPrice.TabIndex = 4;
            numPrice.ThousandsSeparator = true;
            numPrice.IsMoney = true;
            // 
            // dtEffectiveDate
            // 
            dtEffectiveDate.CustomFormat = "dd/MM/yyyy";
            dtEffectiveDate.Dock = System.Windows.Forms.DockStyle.Fill;
            dtEffectiveDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            dtEffectiveDate.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            dtEffectiveDate.Name = "dtEffectiveDate";
            dtEffectiveDate.TabIndex = 5;
            // 
            // btnAddPrice
            // 
            btnAddPrice.Anchor = System.Windows.Forms.AnchorStyles.Left;
            btnAddPrice.AutoSize = true;
            btnAddPrice.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnAddPrice.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            btnAddPrice.Cursor = System.Windows.Forms.Cursors.Hand;
            btnAddPrice.FlatAppearance.BorderSize = 0;
            btnAddPrice.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            btnAddPrice.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnAddPrice.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnAddPrice.ForeColor = System.Drawing.Color.White;
            btnAddPrice.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            btnAddPrice.MinimumSize = new System.Drawing.Size(0, 34);
            btnAddPrice.Name = "btnAddPrice";
            btnAddPrice.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            btnAddPrice.TabIndex = 6;
            btnAddPrice.Text = "➕ Thêm giá";
            btnAddPrice.UseVisualStyleBackColor = false;
            btnAddPrice.Click += new System.EventHandler(this.BtnAddPrice_Click);
            // 
            // btnDeletePrice
            // 
            btnDeletePrice.Anchor = System.Windows.Forms.AnchorStyles.Left;
            btnDeletePrice.AutoSize = true;
            btnDeletePrice.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnDeletePrice.BackColor = System.Drawing.Color.White;
            btnDeletePrice.Cursor = System.Windows.Forms.Cursors.Hand;
            btnDeletePrice.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(220, 38, 38);
            btnDeletePrice.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnDeletePrice.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnDeletePrice.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
            btnDeletePrice.Margin = new System.Windows.Forms.Padding(0);
            btnDeletePrice.MinimumSize = new System.Drawing.Size(0, 34);
            btnDeletePrice.Name = "btnDeletePrice";
            btnDeletePrice.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            btnDeletePrice.TabIndex = 7;
            btnDeletePrice.Text = "🗑️ Xóa giá đã chọn";
            btnDeletePrice.UseVisualStyleBackColor = false;
            btnDeletePrice.Click += new System.EventHandler(this.BtnDeletePrice_Click);
            // 
            // filterBar — tìm nhanh + lọc (Common/FilterBar); các ô chọn thêm ở .cs
            // 
            filterBar.Dock = System.Windows.Forms.DockStyle.Fill;
            filterBar.Name = "filterBar";
            filterBar.SearchPlaceholder = "Loại nhiên liệu, giá, ngày...";
            filterBar.TabIndex = 2;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblStatus.AutoSize = true;
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblStatus.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblStatus.Margin = new System.Windows.Forms.Padding(0, 4, 0, 8);
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
            // FuelProviderPricesForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(1000, 620);
            Controls.Add(root);
            MinimumSize = new System.Drawing.Size(760, 460);
            Name = "FuelProviderPricesForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
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
        private TruckingSystem.Desktop.Common.NumberBox numPrice;
        private System.Windows.Forms.Label lblEffectiveDate;
        private System.Windows.Forms.DateTimePicker dtEffectiveDate;
        private System.Windows.Forms.Button btnAddPrice;
        private System.Windows.Forms.Button btnDeletePrice;
        private System.Windows.Forms.Label lblStatus;
        private TruckingSystem.Desktop.Common.FilterBar filterBar;
        private System.Windows.Forms.DataGridView grid;
    }
}
