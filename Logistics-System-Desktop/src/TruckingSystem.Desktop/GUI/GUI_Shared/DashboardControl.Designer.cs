namespace TruckingSystem.Desktop.GUI_Shared
{
    partial class DashboardControl
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>Bố cục: _root (Panel cuộn dọc, Dock=Fill) chứa dashLayout (TableLayoutPanel 1 cột,
        /// Dock=Top, tự cao theo nội dung) = tiêu đề / thanh lọc (FlowLayoutPanel tự xuống dòng) /
        /// hàng thẻ tài chính / hàng thẻ vận hành / 2 biểu đồ chia đôi (chartsRow) / tiêu đề + bảng
        /// lãi-lỗ từng đơn / tiêu đề + thẻ + ghi chú + bảng kiểm soát nhiên liệu. Thẻ số liệu sinh
        /// bằng code (theo dữ liệu) vào các FlowLayoutPanel _kpiFinance / _kpiOps / _kpiFuel và tự
        /// xuống dòng khi hẹp. Không có tọa độ cứng. Sửa được bằng Designer.</summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DashboardControl));
            _root = new Panel();
            dashLayout = new TableLayoutPanel();
            _lblTitle = new Label();
            _filterBar = new FlowLayoutPanel();
            lblPeriod = new Label();
            _cboPeriod = new ComboBox();
            lblFrom = new Label();
            _dtpFrom = new DateTimePicker();
            lblTo = new Label();
            _dtpTo = new DateTimePicker();
            _btnRefresh = new Button();
            _lblStatus = new Label();
            _kpiFinance = new FlowLayoutPanel();
            _kpiOps = new FlowLayoutPanel();
            chartsRow = new TableLayoutPanel();
            _chartMonthly = new DashboardChartPanel();
            _chartCosts = new DashboardChartPanel();
            _lblGridTitle = new Label();
            _ordersFilter = new TruckingSystem.Desktop.Common.FilterBar();
            _grid = new DataGridView();
            _lblFuelTitle = new Label();
            _kpiFuel = new FlowLayoutPanel();
            _lblFuelNote = new Label();
            _fuelFilter = new TruckingSystem.Desktop.Common.FilterBar();
            _gridFuel = new DataGridView();
            _root.SuspendLayout();
            dashLayout.SuspendLayout();
            _filterBar.SuspendLayout();
            chartsRow.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_grid).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_gridFuel).BeginInit();
            SuspendLayout();
            // 
            // _root
            // 
            _root.AutoScroll = true;
            _root.Controls.Add(dashLayout);
            _root.Dock = DockStyle.Fill;
            _root.Location = new Point(0, 0);
            _root.Name = "_root";
            _root.Size = new Size(1164, 700);
            _root.TabIndex = 0;
            // 
            // dashLayout
            // 
            dashLayout.AutoSize = true;
            dashLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            dashLayout.ColumnCount = 1;
            dashLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            dashLayout.Controls.Add(_lblTitle, 0, 0);
            dashLayout.Controls.Add(_filterBar, 0, 1);
            dashLayout.Controls.Add(_kpiFinance, 0, 2);
            dashLayout.Controls.Add(_kpiOps, 0, 3);
            dashLayout.Controls.Add(chartsRow, 0, 4);
            dashLayout.Controls.Add(_lblGridTitle, 0, 5);
            dashLayout.Controls.Add(_ordersFilter, 0, 6);
            dashLayout.Controls.Add(_grid, 0, 7);
            dashLayout.Controls.Add(_lblFuelTitle, 0, 8);
            dashLayout.Controls.Add(_kpiFuel, 0, 9);
            dashLayout.Controls.Add(_lblFuelNote, 0, 10);
            dashLayout.Controls.Add(_fuelFilter, 0, 11);
            dashLayout.Controls.Add(_gridFuel, 0, 12);
            dashLayout.Dock = DockStyle.Top;
            dashLayout.Location = new Point(0, 0);
            dashLayout.Margin = new Padding(0);
            dashLayout.Name = "dashLayout";
            dashLayout.Padding = new Padding(0, 0, 12, 12);
            dashLayout.RowCount = 13;
            dashLayout.RowStyles.Add(new RowStyle());
            dashLayout.RowStyles.Add(new RowStyle());
            dashLayout.RowStyles.Add(new RowStyle());
            dashLayout.RowStyles.Add(new RowStyle());
            dashLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 306F));
            dashLayout.RowStyles.Add(new RowStyle());
            dashLayout.RowStyles.Add(new RowStyle());
            dashLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 440F));
            dashLayout.RowStyles.Add(new RowStyle());
            dashLayout.RowStyles.Add(new RowStyle());
            dashLayout.RowStyles.Add(new RowStyle());
            dashLayout.RowStyles.Add(new RowStyle());
            dashLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 360F));
            dashLayout.Size = new Size(1147, 1422);
            dashLayout.TabIndex = 0;
            // 
            // _lblTitle
            // 
            _lblTitle.AutoSize = true;
            _lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            _lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            _lblTitle.Location = new Point(0, 0);
            _lblTitle.Margin = new Padding(0, 0, 0, 10);
            _lblTitle.Name = "_lblTitle";
            _lblTitle.Size = new Size(277, 32);
            _lblTitle.TabIndex = 0;
            _lblTitle.Text = "📊 Tổng quan & Lãi / Lỗ";
            // 
            // _filterBar
            // 
            _filterBar.AutoSize = true;
            _filterBar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _filterBar.Controls.Add(lblPeriod);
            _filterBar.Controls.Add(_cboPeriod);
            _filterBar.Controls.Add(lblFrom);
            _filterBar.Controls.Add(_dtpFrom);
            _filterBar.Controls.Add(lblTo);
            _filterBar.Controls.Add(_dtpTo);
            _filterBar.Controls.Add(_btnRefresh);
            _filterBar.Controls.Add(_lblStatus);
            _filterBar.Dock = DockStyle.Fill;
            _filterBar.Location = new Point(0, 42);
            _filterBar.Margin = new Padding(0, 0, 0, 8);
            _filterBar.Name = "_filterBar";
            _filterBar.Size = new Size(1135, 42);
            _filterBar.TabIndex = 1;
            // 
            // lblPeriod
            // 
            lblPeriod.AutoSize = true;
            lblPeriod.Font = new Font("Segoe UI", 10F);
            lblPeriod.ForeColor = Color.FromArgb(100, 116, 139);
            lblPeriod.Location = new Point(0, 8);
            lblPeriod.Margin = new Padding(0, 8, 6, 0);
            lblPeriod.Name = "lblPeriod";
            lblPeriod.Size = new Size(78, 19);
            lblPeriod.TabIndex = 0;
            lblPeriod.Text = "Kỳ báo cáo:";
            // 
            // _cboPeriod
            // 
            _cboPeriod.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboPeriod.Items.AddRange(new object[] { "Tháng này", "3 tháng gần nhất", "Năm nay", "Tất cả", "Tùy chọn" });
            _cboPeriod.Location = new Point(84, 4);
            _cboPeriod.Margin = new Padding(0, 4, 12, 4);
            _cboPeriod.Name = "_cboPeriod";
            _cboPeriod.Size = new Size(160, 25);
            _cboPeriod.TabIndex = 1;
            _cboPeriod.SelectedIndexChanged += CboPeriod_SelectedIndexChanged;
            // 
            // lblFrom
            // 
            lblFrom.AutoSize = true;
            lblFrom.Font = new Font("Segoe UI", 10F);
            lblFrom.ForeColor = Color.FromArgb(100, 116, 139);
            lblFrom.Location = new Point(256, 8);
            lblFrom.Margin = new Padding(0, 8, 6, 0);
            lblFrom.Name = "lblFrom";
            lblFrom.Size = new Size(24, 19);
            lblFrom.TabIndex = 2;
            lblFrom.Text = "Từ";
            // 
            // _dtpFrom
            // 
            _dtpFrom.CustomFormat = "dd/MM/yyyy";
            _dtpFrom.Format = DateTimePickerFormat.Custom;
            _dtpFrom.Location = new Point(286, 4);
            _dtpFrom.Margin = new Padding(0, 4, 6, 4);
            _dtpFrom.Name = "_dtpFrom";
            _dtpFrom.Size = new Size(130, 25);
            _dtpFrom.TabIndex = 3;
            // 
            // lblTo
            // 
            lblTo.AutoSize = true;
            lblTo.Font = new Font("Segoe UI", 10F);
            lblTo.ForeColor = Color.FromArgb(100, 116, 139);
            lblTo.Location = new Point(422, 8);
            lblTo.Margin = new Padding(0, 8, 6, 0);
            lblTo.Name = "lblTo";
            lblTo.Size = new Size(32, 19);
            lblTo.TabIndex = 4;
            lblTo.Text = "đến";
            // 
            // _dtpTo
            // 
            _dtpTo.CustomFormat = "dd/MM/yyyy";
            _dtpTo.Format = DateTimePickerFormat.Custom;
            _dtpTo.Location = new Point(460, 4);
            _dtpTo.Margin = new Padding(0, 4, 12, 4);
            _dtpTo.Name = "_dtpTo";
            _dtpTo.Size = new Size(130, 25);
            _dtpTo.TabIndex = 5;
            // 
            // _btnRefresh
            // 
            _btnRefresh.AutoSize = true;
            _btnRefresh.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _btnRefresh.BackColor = Color.FromArgb(37, 99, 235);
            _btnRefresh.Cursor = Cursors.Hand;
            _btnRefresh.FlatAppearance.BorderSize = 0;
            _btnRefresh.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
            _btnRefresh.FlatStyle = FlatStyle.Flat;
            _btnRefresh.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _btnRefresh.ForeColor = Color.White;
            _btnRefresh.Location = new Point(602, 2);
            _btnRefresh.Margin = new Padding(0, 2, 0, 2);
            _btnRefresh.MinimumSize = new Size(0, 38);
            _btnRefresh.Name = "_btnRefresh";
            _btnRefresh.Padding = new Padding(10, 2, 10, 2);
            _btnRefresh.Size = new Size(151, 38);
            _btnRefresh.TabIndex = 6;
            _btnRefresh.Text = "🔄 Xem báo cáo";
            _btnRefresh.UseVisualStyleBackColor = false;
            _btnRefresh.Click += BtnRefresh_Click;
            // 
            // _lblStatus
            // 
            _lblStatus.AutoSize = true;
            _lblStatus.Font = new Font("Segoe UI", 9F);
            _lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
            _lblStatus.Location = new Point(765, 10);
            _lblStatus.Margin = new Padding(12, 10, 0, 0);
            _lblStatus.Name = "_lblStatus";
            _lblStatus.Size = new Size(0, 15);
            _lblStatus.TabIndex = 7;
            // 
            // _kpiFinance
            // 
            _kpiFinance.AutoSize = true;
            _kpiFinance.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _kpiFinance.Dock = DockStyle.Fill;
            _kpiFinance.Location = new Point(0, 92);
            _kpiFinance.Margin = new Padding(0, 0, 0, 6);
            _kpiFinance.Name = "_kpiFinance";
            _kpiFinance.Size = new Size(1135, 1);
            _kpiFinance.TabIndex = 2;
            // 
            // _kpiOps
            // 
            _kpiOps.AutoSize = true;
            _kpiOps.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _kpiOps.Dock = DockStyle.Fill;
            _kpiOps.Location = new Point(0, 98);
            _kpiOps.Margin = new Padding(0, 0, 0, 10);
            _kpiOps.Name = "_kpiOps";
            _kpiOps.Size = new Size(1135, 1);
            _kpiOps.TabIndex = 3;
            // 
            // chartsRow
            // 
            chartsRow.ColumnCount = 2;
            chartsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            chartsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            chartsRow.Controls.Add(_chartMonthly, 0, 0);
            chartsRow.Controls.Add(_chartCosts, 1, 0);
            chartsRow.Dock = DockStyle.Fill;
            chartsRow.Location = new Point(0, 108);
            chartsRow.Margin = new Padding(0, 0, 0, 16);
            chartsRow.Name = "chartsRow";
            chartsRow.RowCount = 1;
            chartsRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            chartsRow.Size = new Size(1135, 290);
            chartsRow.TabIndex = 4;
            // 
            // _chartMonthly
            // 
            _chartMonthly.BackColor = Color.White;
            _chartMonthly.Dock = DockStyle.Fill;
            _chartMonthly.Location = new Point(0, 0);
            _chartMonthly.Margin = new Padding(0, 0, 8, 0);
            _chartMonthly.Name = "_chartMonthly";
            _chartMonthly.Size = new Size(559, 290);
            _chartMonthly.TabIndex = 0;
            _chartMonthly.Paint += ChartMonthly_Paint;
            // 
            // _chartCosts
            // 
            _chartCosts.BackColor = Color.White;
            _chartCosts.Dock = DockStyle.Fill;
            _chartCosts.Location = new Point(575, 0);
            _chartCosts.Margin = new Padding(8, 0, 0, 0);
            _chartCosts.Name = "_chartCosts";
            _chartCosts.Size = new Size(560, 290);
            _chartCosts.TabIndex = 1;
            _chartCosts.Paint += ChartCosts_Paint;
            // 
            // _lblGridTitle
            // 
            _lblGridTitle.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _lblGridTitle.AutoSize = true;
            _lblGridTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            _lblGridTitle.ForeColor = Color.FromArgb(15, 23, 42);
            _lblGridTitle.Location = new Point(0, 414);
            _lblGridTitle.Margin = new Padding(0, 0, 0, 8);
            _lblGridTitle.Name = "_lblGridTitle";
            _lblGridTitle.Size = new Size(1135, 25);
            _lblGridTitle.TabIndex = 5;
            _lblGridTitle.Text = "📋 Lãi / Lỗ từng đơn (lỗ nặng nhất lên đầu) — double-click 1 đơn để xem chi tiết từng khoản chi";
            // 
            // _ordersFilter
            // 
            _ordersFilter.AutoSize = true;
            _ordersFilter.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _ordersFilter.Dock = DockStyle.Fill;
            _ordersFilter.Location = new Point(0, 447);
            _ordersFilter.Margin = new Padding(0, 0, 0, 6);
            _ordersFilter.Name = "_ordersFilter";
            _ordersFilter.SearchPlaceholder = "Mã đơn, khách hàng...";
            _ordersFilter.Size = new Size(1135, 37);
            _ordersFilter.TabIndex = 6;
            // 
            // _grid
            // 
            _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _grid.Dock = DockStyle.Fill;
            _grid.Location = new Point(0, 490);
            _grid.Margin = new Padding(0, 0, 0, 20);
            _grid.Name = "_grid";
            _grid.Size = new Size(1135, 420);
            _grid.TabIndex = 6;
            _grid.CellDoubleClick += Grid_CellDoubleClick;
            _grid.CellFormatting += Grid_CellFormatting;
            _grid.DataBindingComplete += Grid_DataBindingComplete;
            // 
            // _lblFuelTitle
            // 
            _lblFuelTitle.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _lblFuelTitle.AutoSize = true;
            _lblFuelTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            _lblFuelTitle.ForeColor = Color.FromArgb(15, 23, 42);
            _lblFuelTitle.Location = new Point(0, 930);
            _lblFuelTitle.Margin = new Padding(0, 0, 0, 8);
            _lblFuelTitle.Name = "_lblFuelTitle";
            _lblFuelTitle.Size = new Size(1135, 25);
            _lblFuelTitle.TabIndex = 7;
            _lblFuelTitle.Text = "⛽ Kiểm soát nhiên liệu — so sánh Dự toán (VietMap) · Định mức theo GPS thực tế · Thực đổ (hóa đơn)";
            // 
            // _kpiFuel
            // 
            _kpiFuel.AutoSize = true;
            _kpiFuel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _kpiFuel.Dock = DockStyle.Fill;
            _kpiFuel.Location = new Point(0, 963);
            _kpiFuel.Margin = new Padding(0);
            _kpiFuel.Name = "_kpiFuel";
            _kpiFuel.Size = new Size(1135, 1);
            _kpiFuel.TabIndex = 8;
            // 
            // _lblFuelNote
            // 
            _lblFuelNote.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _lblFuelNote.AutoSize = true;
            _lblFuelNote.Font = new Font("Segoe UI", 9F);
            _lblFuelNote.ForeColor = Color.FromArgb(100, 116, 139);
            _lblFuelNote.Location = new Point(0, 969);
            _lblFuelNote.Margin = new Padding(0, 6, 0, 8);
            _lblFuelNote.Name = "_lblFuelNote";
            _lblFuelNote.Size = new Size(1135, 30);
            _lblFuelNote.TabIndex = 9;
            _lblFuelNote.Text = resources.GetString("_lblFuelNote.Text");
            // 
            // _fuelFilter
            // 
            _fuelFilter.AutoSize = true;
            _fuelFilter.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _fuelFilter.Dock = DockStyle.Fill;
            _fuelFilter.Location = new Point(0, 1007);
            _fuelFilter.Margin = new Padding(0, 0, 0, 6);
            _fuelFilter.Name = "_fuelFilter";
            _fuelFilter.SearchPlaceholder = "Chuyến, mã đơn, biển số...";
            _fuelFilter.Size = new Size(1135, 37);
            _fuelFilter.TabIndex = 10;
            // 
            // _gridFuel
            // 
            _gridFuel.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _gridFuel.Dock = DockStyle.Fill;
            _gridFuel.Location = new Point(0, 1050);
            _gridFuel.Margin = new Padding(0);
            _gridFuel.Name = "_gridFuel";
            _gridFuel.Size = new Size(1135, 360);
            _gridFuel.TabIndex = 10;
            _gridFuel.CellFormatting += GridFuel_CellFormatting;
            _gridFuel.DataBindingComplete += GridFuel_DataBindingComplete;
            // 
            // DashboardControl
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(241, 245, 249);
            Controls.Add(_root);
            Name = "DashboardControl";
            Size = new Size(1164, 700);
            _root.ResumeLayout(false);
            _root.PerformLayout();
            dashLayout.ResumeLayout(false);
            dashLayout.PerformLayout();
            _filterBar.ResumeLayout(false);
            _filterBar.PerformLayout();
            chartsRow.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_grid).EndInit();
            ((System.ComponentModel.ISupportInitialize)_gridFuel).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel _root;
        private System.Windows.Forms.TableLayoutPanel dashLayout;
        private System.Windows.Forms.Label _lblTitle;
        private System.Windows.Forms.FlowLayoutPanel _filterBar;
        private System.Windows.Forms.Label lblPeriod;
        private System.Windows.Forms.ComboBox _cboPeriod;
        private System.Windows.Forms.Label lblFrom;
        private System.Windows.Forms.DateTimePicker _dtpFrom;
        private System.Windows.Forms.Label lblTo;
        private System.Windows.Forms.DateTimePicker _dtpTo;
        private System.Windows.Forms.Button _btnRefresh;
        private System.Windows.Forms.Label _lblStatus;
        private System.Windows.Forms.FlowLayoutPanel _kpiFinance;
        private System.Windows.Forms.FlowLayoutPanel _kpiOps;
        private System.Windows.Forms.TableLayoutPanel chartsRow;
        private TruckingSystem.Desktop.GUI_Shared.DashboardChartPanel _chartMonthly;
        private TruckingSystem.Desktop.GUI_Shared.DashboardChartPanel _chartCosts;
        private System.Windows.Forms.Label _lblGridTitle;
        private TruckingSystem.Desktop.Common.FilterBar _ordersFilter;
        private System.Windows.Forms.DataGridView _grid;
        private System.Windows.Forms.Label _lblFuelTitle;
        private System.Windows.Forms.FlowLayoutPanel _kpiFuel;
        private System.Windows.Forms.Label _lblFuelNote;
        private TruckingSystem.Desktop.Common.FilterBar _fuelFilter;
        private System.Windows.Forms.DataGridView _gridFuel;
    }
}
