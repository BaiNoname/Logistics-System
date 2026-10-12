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
            _root = new System.Windows.Forms.Panel();
            dashLayout = new System.Windows.Forms.TableLayoutPanel();
            _lblTitle = new System.Windows.Forms.Label();
            _filterBar = new System.Windows.Forms.FlowLayoutPanel();
            lblPeriod = new System.Windows.Forms.Label();
            _cboPeriod = new System.Windows.Forms.ComboBox();
            lblFrom = new System.Windows.Forms.Label();
            _dtpFrom = new System.Windows.Forms.DateTimePicker();
            lblTo = new System.Windows.Forms.Label();
            _dtpTo = new System.Windows.Forms.DateTimePicker();
            _btnRefresh = new System.Windows.Forms.Button();
            _lblStatus = new System.Windows.Forms.Label();
            _kpiFinance = new System.Windows.Forms.FlowLayoutPanel();
            _kpiOps = new System.Windows.Forms.FlowLayoutPanel();
            chartsRow = new System.Windows.Forms.TableLayoutPanel();
            _chartMonthly = new TruckingSystem.Desktop.GUI_Shared.DashboardChartPanel();
            _chartCosts = new TruckingSystem.Desktop.GUI_Shared.DashboardChartPanel();
            _lblGridTitle = new System.Windows.Forms.Label();
            _grid = new System.Windows.Forms.DataGridView();
            _lblFuelTitle = new System.Windows.Forms.Label();
            _kpiFuel = new System.Windows.Forms.FlowLayoutPanel();
            _lblFuelNote = new System.Windows.Forms.Label();
            _gridFuel = new System.Windows.Forms.DataGridView();
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
            _root.Dock = System.Windows.Forms.DockStyle.Fill;
            _root.Name = "_root";
            _root.Size = new System.Drawing.Size(1164, 700);
            _root.TabIndex = 0;
            //
            // dashLayout
            //
            dashLayout.AutoSize = true;
            dashLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            dashLayout.ColumnCount = 1;
            dashLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            dashLayout.Controls.Add(_lblTitle, 0, 0);
            dashLayout.Controls.Add(_filterBar, 0, 1);
            dashLayout.Controls.Add(_kpiFinance, 0, 2);
            dashLayout.Controls.Add(_kpiOps, 0, 3);
            dashLayout.Controls.Add(chartsRow, 0, 4);
            dashLayout.Controls.Add(_lblGridTitle, 0, 5);
            dashLayout.Controls.Add(_grid, 0, 6);
            dashLayout.Controls.Add(_lblFuelTitle, 0, 7);
            dashLayout.Controls.Add(_kpiFuel, 0, 8);
            dashLayout.Controls.Add(_lblFuelNote, 0, 9);
            dashLayout.Controls.Add(_gridFuel, 0, 10);
            dashLayout.Dock = System.Windows.Forms.DockStyle.Top;
            dashLayout.Margin = new System.Windows.Forms.Padding(0);
            dashLayout.Name = "dashLayout";
            dashLayout.Padding = new System.Windows.Forms.Padding(0, 0, 12, 12);
            dashLayout.RowCount = 11;
            dashLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            dashLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            dashLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            dashLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            dashLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 306F));
            dashLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            dashLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 440F));
            dashLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            dashLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            dashLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            dashLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 360F));
            dashLayout.TabIndex = 0;
            //
            // _lblTitle
            //
            _lblTitle.AutoSize = true;
            _lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            _lblTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            _lblTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            _lblTitle.Name = "_lblTitle";
            _lblTitle.TabIndex = 0;
            _lblTitle.Text = "📊 Tổng quan & Lãi / Lỗ";
            //
            // _filterBar
            //
            _filterBar.AutoSize = true;
            _filterBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            _filterBar.Controls.Add(lblPeriod);
            _filterBar.Controls.Add(_cboPeriod);
            _filterBar.Controls.Add(lblFrom);
            _filterBar.Controls.Add(_dtpFrom);
            _filterBar.Controls.Add(lblTo);
            _filterBar.Controls.Add(_dtpTo);
            _filterBar.Controls.Add(_btnRefresh);
            _filterBar.Controls.Add(_lblStatus);
            _filterBar.Dock = System.Windows.Forms.DockStyle.Fill;
            _filterBar.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            _filterBar.Name = "_filterBar";
            _filterBar.TabIndex = 1;
            _filterBar.WrapContents = true;
            //
            // lblPeriod
            //
            lblPeriod.AutoSize = true;
            lblPeriod.Font = new System.Drawing.Font("Segoe UI", 10F);
            lblPeriod.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblPeriod.Margin = new System.Windows.Forms.Padding(0, 8, 6, 0);
            lblPeriod.Name = "lblPeriod";
            lblPeriod.TabIndex = 0;
            lblPeriod.Text = "Kỳ báo cáo:";
            //
            // _cboPeriod
            //
            _cboPeriod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _cboPeriod.Items.AddRange(new object[] { "Tháng này", "3 tháng gần nhất", "Năm nay", "Tất cả", "Tùy chọn" });
            _cboPeriod.Margin = new System.Windows.Forms.Padding(0, 4, 12, 4);
            _cboPeriod.Name = "_cboPeriod";
            _cboPeriod.Size = new System.Drawing.Size(160, 25);
            _cboPeriod.TabIndex = 1;
            _cboPeriod.SelectedIndexChanged += new System.EventHandler(this.CboPeriod_SelectedIndexChanged);
            //
            // lblFrom
            //
            lblFrom.AutoSize = true;
            lblFrom.Font = new System.Drawing.Font("Segoe UI", 10F);
            lblFrom.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblFrom.Margin = new System.Windows.Forms.Padding(0, 8, 6, 0);
            lblFrom.Name = "lblFrom";
            lblFrom.TabIndex = 2;
            lblFrom.Text = "Từ";
            //
            // _dtpFrom
            //
            _dtpFrom.CustomFormat = "dd/MM/yyyy";
            _dtpFrom.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            _dtpFrom.Margin = new System.Windows.Forms.Padding(0, 4, 6, 4);
            _dtpFrom.Name = "_dtpFrom";
            _dtpFrom.Size = new System.Drawing.Size(130, 25);
            _dtpFrom.TabIndex = 3;
            //
            // lblTo
            //
            lblTo.AutoSize = true;
            lblTo.Font = new System.Drawing.Font("Segoe UI", 10F);
            lblTo.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblTo.Margin = new System.Windows.Forms.Padding(0, 8, 6, 0);
            lblTo.Name = "lblTo";
            lblTo.TabIndex = 4;
            lblTo.Text = "đến";
            //
            // _dtpTo
            //
            _dtpTo.CustomFormat = "dd/MM/yyyy";
            _dtpTo.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            _dtpTo.Margin = new System.Windows.Forms.Padding(0, 4, 12, 4);
            _dtpTo.Name = "_dtpTo";
            _dtpTo.Size = new System.Drawing.Size(130, 25);
            _dtpTo.TabIndex = 5;
            //
            // _btnRefresh
            //
            _btnRefresh.AutoSize = true;
            _btnRefresh.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            _btnRefresh.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            _btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            _btnRefresh.FlatAppearance.BorderSize = 0;
            _btnRefresh.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            _btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            _btnRefresh.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            _btnRefresh.ForeColor = System.Drawing.Color.White;
            _btnRefresh.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            _btnRefresh.MinimumSize = new System.Drawing.Size(0, 34);
            _btnRefresh.Name = "_btnRefresh";
            _btnRefresh.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            _btnRefresh.TabIndex = 6;
            _btnRefresh.Text = "🔄 Xem báo cáo";
            _btnRefresh.UseVisualStyleBackColor = false;
            _btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
            //
            // _lblStatus
            //
            _lblStatus.AutoSize = true;
            _lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            _lblStatus.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            _lblStatus.Margin = new System.Windows.Forms.Padding(12, 10, 0, 0);
            _lblStatus.Name = "_lblStatus";
            _lblStatus.TabIndex = 7;
            //
            // _kpiFinance
            //
            _kpiFinance.AutoSize = true;
            _kpiFinance.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            _kpiFinance.Dock = System.Windows.Forms.DockStyle.Fill;
            _kpiFinance.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            _kpiFinance.Name = "_kpiFinance";
            _kpiFinance.TabIndex = 2;
            _kpiFinance.WrapContents = true;
            //
            // _kpiOps
            //
            _kpiOps.AutoSize = true;
            _kpiOps.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            _kpiOps.Dock = System.Windows.Forms.DockStyle.Fill;
            _kpiOps.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            _kpiOps.Name = "_kpiOps";
            _kpiOps.TabIndex = 3;
            _kpiOps.WrapContents = true;
            //
            // chartsRow
            //
            chartsRow.ColumnCount = 2;
            chartsRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            chartsRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            chartsRow.Controls.Add(_chartMonthly, 0, 0);
            chartsRow.Controls.Add(_chartCosts, 1, 0);
            chartsRow.Dock = System.Windows.Forms.DockStyle.Fill;
            chartsRow.Margin = new System.Windows.Forms.Padding(0, 0, 0, 16);
            chartsRow.Name = "chartsRow";
            chartsRow.RowCount = 1;
            chartsRow.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            chartsRow.TabIndex = 4;
            //
            // _chartMonthly
            //
            _chartMonthly.Dock = System.Windows.Forms.DockStyle.Fill;
            _chartMonthly.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            _chartMonthly.Name = "_chartMonthly";
            _chartMonthly.TabIndex = 0;
            _chartMonthly.Paint += new System.Windows.Forms.PaintEventHandler(this.ChartMonthly_Paint);
            //
            // _chartCosts
            //
            _chartCosts.Dock = System.Windows.Forms.DockStyle.Fill;
            _chartCosts.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            _chartCosts.Name = "_chartCosts";
            _chartCosts.TabIndex = 1;
            _chartCosts.Paint += new System.Windows.Forms.PaintEventHandler(this.ChartCosts_Paint);
            //
            // _lblGridTitle
            //
            _lblGridTitle.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _lblGridTitle.AutoSize = true;
            _lblGridTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            _lblGridTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            _lblGridTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            _lblGridTitle.Name = "_lblGridTitle";
            _lblGridTitle.TabIndex = 5;
            _lblGridTitle.Text = "📋 Lãi / Lỗ từng đơn (lỗ nặng nhất lên đầu) — double-click 1 đơn để xem chi tiết từng khoản chi";
            //
            // _grid
            //
            _grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _grid.Dock = System.Windows.Forms.DockStyle.Fill;
            _grid.Margin = new System.Windows.Forms.Padding(0, 0, 0, 20);
            _grid.Name = "_grid";
            _grid.TabIndex = 6;
            _grid.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.Grid_CellDoubleClick);
            _grid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.Grid_CellFormatting);
            _grid.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.Grid_DataBindingComplete);
            //
            // _lblFuelTitle
            //
            _lblFuelTitle.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _lblFuelTitle.AutoSize = true;
            _lblFuelTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            _lblFuelTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            _lblFuelTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            _lblFuelTitle.Name = "_lblFuelTitle";
            _lblFuelTitle.TabIndex = 7;
            _lblFuelTitle.Text = "⛽ Kiểm soát nhiên liệu — so sánh Dự toán (VietMap) · Định mức theo GPS thực tế · Thực đổ (hóa đơn)";
            //
            // _kpiFuel
            //
            _kpiFuel.AutoSize = true;
            _kpiFuel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            _kpiFuel.Dock = System.Windows.Forms.DockStyle.Fill;
            _kpiFuel.Margin = new System.Windows.Forms.Padding(0);
            _kpiFuel.Name = "_kpiFuel";
            _kpiFuel.TabIndex = 8;
            _kpiFuel.WrapContents = true;
            //
            // _lblFuelNote
            //
            _lblFuelNote.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _lblFuelNote.AutoSize = true;
            _lblFuelNote.Font = new System.Drawing.Font("Segoe UI", 9F);
            _lblFuelNote.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            _lblFuelNote.Margin = new System.Windows.Forms.Padding(0, 6, 0, 8);
            _lblFuelNote.Name = "_lblFuelNote";
            _lblFuelNote.TabIndex = 9;
            _lblFuelNote.Text = "Định mức = km GPS × L/100km của xe × giá nhiên liệu ngày chạy (bảng giá ở màn Nhiên liệu). Dòng tô đỏ: lệch quá 15% (km GPS so với km VietMap — có thể chạy vòng/lệch tuyến; hoặc tiền thực đổ so với định mức — có thể hao hụt). Chuyến chưa nhập hóa đơn xăng thì lãi/lỗ tạm tính theo định mức.";
            //
            // _gridFuel
            //
            _gridFuel.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _gridFuel.Dock = System.Windows.Forms.DockStyle.Fill;
            _gridFuel.Margin = new System.Windows.Forms.Padding(0);
            _gridFuel.Name = "_gridFuel";
            _gridFuel.TabIndex = 10;
            _gridFuel.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.GridFuel_CellFormatting);
            _gridFuel.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.GridFuel_DataBindingComplete);
            //
            // DashboardControl
            //
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            Controls.Add(_root);
            Name = "DashboardControl";
            Size = new System.Drawing.Size(1164, 700);
            ((System.ComponentModel.ISupportInitialize)_grid).EndInit();
            ((System.ComponentModel.ISupportInitialize)_gridFuel).EndInit();
            chartsRow.ResumeLayout(false);
            _filterBar.ResumeLayout(false);
            _filterBar.PerformLayout();
            dashLayout.ResumeLayout(false);
            dashLayout.PerformLayout();
            _root.ResumeLayout(false);
            _root.PerformLayout();
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
        private System.Windows.Forms.DataGridView _grid;
        private System.Windows.Forms.Label _lblFuelTitle;
        private System.Windows.Forms.FlowLayoutPanel _kpiFuel;
        private System.Windows.Forms.Label _lblFuelNote;
        private System.Windows.Forms.DataGridView _gridFuel;
    }
}
