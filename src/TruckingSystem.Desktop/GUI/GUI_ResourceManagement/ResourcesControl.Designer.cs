namespace TruckingSystem.Desktop.GUI_ResourceManagement
{
    partial class ResourcesControl
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>Bố cục: tiêu đề / chọn kho + Làm mới / 4 ô tổng hợp / dòng gợi ý / 2 khung
        /// (Xe trên, Tài xế dưới — mỗi khung: thanh nút + bảng) chia đôi chiều cao / dòng trạng thái.</summary>
        private void InitializeComponent()
        {
            root = new System.Windows.Forms.TableLayoutPanel();
            lblTitle = new System.Windows.Forms.Label();
            topBar = new System.Windows.Forms.FlowLayoutPanel();
            lblWarehouse = new System.Windows.Forms.Label();
            cboWarehouse = new System.Windows.Forms.ComboBox();
            btnRefresh = new System.Windows.Forms.Button();
            summaryBar = new System.Windows.Forms.FlowLayoutPanel();
            lblSumVehicles = new System.Windows.Forms.Label();
            lblSumDrivers = new System.Windows.Forms.Label();
            lblSumDocs = new System.Windows.Forms.Label();
            lblSumTrips = new System.Windows.Forms.Label();
            lblHint = new System.Windows.Forms.Label();
            split = new System.Windows.Forms.TableLayoutPanel();
            grpVehicles = new System.Windows.Forms.GroupBox();
            vehicleLayout = new System.Windows.Forms.TableLayoutPanel();
            vehicleBar = new System.Windows.Forms.FlowLayoutPanel();
            btnVehicleDocs = new System.Windows.Forms.Button();
            btnVehicleFuel = new System.Windows.Forms.Button();
            vehicleFilter = new TruckingSystem.Desktop.Common.FilterBar();
            gridVehicles = new System.Windows.Forms.DataGridView();
            grpDrivers = new System.Windows.Forms.GroupBox();
            driverLayout = new System.Windows.Forms.TableLayoutPanel();
            driverBar = new System.Windows.Forms.FlowLayoutPanel();
            btnDriverProfile = new System.Windows.Forms.Button();
            btnDriverWorkDays = new System.Windows.Forms.Button();
            driverFilter = new TruckingSystem.Desktop.Common.FilterBar();
            gridDrivers = new System.Windows.Forms.DataGridView();
            lblStatus = new System.Windows.Forms.Label();
            root.SuspendLayout();
            topBar.SuspendLayout();
            summaryBar.SuspendLayout();
            split.SuspendLayout();
            grpVehicles.SuspendLayout();
            vehicleLayout.SuspendLayout();
            vehicleBar.SuspendLayout();
            grpDrivers.SuspendLayout();
            driverLayout.SuspendLayout();
            driverBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridVehicles).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridDrivers).BeginInit();
            SuspendLayout();
            //
            // root
            //
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.Controls.Add(lblTitle, 0, 0);
            root.Controls.Add(topBar, 0, 1);
            root.Controls.Add(summaryBar, 0, 2);
            root.Controls.Add(lblHint, 0, 3);
            root.Controls.Add(split, 0, 4);
            root.Controls.Add(lblStatus, 0, 5);
            root.AutoSize = true;
            root.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            root.Dock = System.Windows.Forms.DockStyle.Top;
            root.Margin = new System.Windows.Forms.Padding(0);
            root.Name = "root";
            root.RowCount = 6;
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.Size = new System.Drawing.Size(1200, 760);
            root.TabIndex = 0;
            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            lblTitle.Name = "lblTitle";
            lblTitle.TabIndex = 0;
            lblTitle.Text = "🧭 Nguồn lực theo kho";
            //
            // topBar
            //
            topBar.AutoSize = true;
            topBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            topBar.Controls.Add(lblWarehouse);
            topBar.Controls.Add(cboWarehouse);
            topBar.Controls.Add(btnRefresh);
            topBar.Dock = System.Windows.Forms.DockStyle.Fill;
            topBar.Margin = new System.Windows.Forms.Padding(0);
            topBar.Name = "topBar";
            topBar.TabIndex = 1;
            topBar.WrapContents = true;
            //
            // lblWarehouse
            //
            lblWarehouse.AutoSize = true;
            lblWarehouse.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblWarehouse.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblWarehouse.Margin = new System.Windows.Forms.Padding(0, 0, 6, 8);
            lblWarehouse.Name = "lblWarehouse";
            lblWarehouse.TabIndex = 0;
            lblWarehouse.Text = "Kho:";
            //
            // cboWarehouse
            //
            cboWarehouse.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboWarehouse.Font = new System.Drawing.Font("Segoe UI", 10F);
            cboWarehouse.Anchor = System.Windows.Forms.AnchorStyles.Left;
            cboWarehouse.Margin = new System.Windows.Forms.Padding(0, 0, 10, 8);
            cboWarehouse.MinimumSize = new System.Drawing.Size(240, 0);
            cboWarehouse.Name = "cboWarehouse";
            cboWarehouse.TabIndex = 1;
            cboWarehouse.SelectedIndexChanged += new System.EventHandler(this.CboWarehouse_SelectedIndexChanged);
            //
            // btnRefresh
            //
            btnRefresh.AutoSize = true;
            btnRefresh.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnRefresh.BackColor = System.Drawing.Color.White;
            btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            btnRefresh.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnRefresh.Anchor = System.Windows.Forms.AnchorStyles.Left;
            btnRefresh.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnRefresh.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            btnRefresh.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            btnRefresh.MinimumSize = new System.Drawing.Size(130, 38);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnRefresh.TabIndex = 2;
            btnRefresh.Text = "🔄 Làm mới";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
            //
            // summaryBar
            //
            summaryBar.AutoSize = true;
            summaryBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            summaryBar.Controls.Add(lblSumVehicles);
            summaryBar.Controls.Add(lblSumDrivers);
            summaryBar.Controls.Add(lblSumDocs);
            summaryBar.Controls.Add(lblSumTrips);
            summaryBar.Dock = System.Windows.Forms.DockStyle.Fill;
            summaryBar.Margin = new System.Windows.Forms.Padding(0);
            summaryBar.Name = "summaryBar";
            summaryBar.TabIndex = 2;
            summaryBar.WrapContents = true;
            //
            // lblSumVehicles — ô tổng hợp
            //
            lblSumVehicles.AutoSize = true;
            lblSumVehicles.BackColor = System.Drawing.Color.White;
            lblSumVehicles.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            lblSumVehicles.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblSumVehicles.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblSumVehicles.Margin = new System.Windows.Forms.Padding(0, 0, 12, 10);
            lblSumVehicles.MinimumSize = new System.Drawing.Size(250, 64);
            lblSumVehicles.Name = "lblSumVehicles";
            lblSumVehicles.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            lblSumVehicles.TabIndex = 0;
            lblSumVehicles.Text = "—";
            //
            // lblSumDrivers — ô tổng hợp
            //
            lblSumDrivers.AutoSize = true;
            lblSumDrivers.BackColor = System.Drawing.Color.White;
            lblSumDrivers.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            lblSumDrivers.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblSumDrivers.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblSumDrivers.Margin = new System.Windows.Forms.Padding(0, 0, 12, 10);
            lblSumDrivers.MinimumSize = new System.Drawing.Size(250, 64);
            lblSumDrivers.Name = "lblSumDrivers";
            lblSumDrivers.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            lblSumDrivers.TabIndex = 1;
            lblSumDrivers.Text = "—";
            //
            // lblSumDocs — ô tổng hợp
            //
            lblSumDocs.AutoSize = true;
            lblSumDocs.BackColor = System.Drawing.Color.White;
            lblSumDocs.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            lblSumDocs.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblSumDocs.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblSumDocs.Margin = new System.Windows.Forms.Padding(0, 0, 12, 10);
            lblSumDocs.MinimumSize = new System.Drawing.Size(250, 64);
            lblSumDocs.Name = "lblSumDocs";
            lblSumDocs.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            lblSumDocs.TabIndex = 2;
            lblSumDocs.Text = "—";
            //
            // lblSumTrips — ô tổng hợp
            //
            lblSumTrips.AutoSize = true;
            lblSumTrips.BackColor = System.Drawing.Color.White;
            lblSumTrips.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            lblSumTrips.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblSumTrips.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblSumTrips.Margin = new System.Windows.Forms.Padding(0, 0, 12, 10);
            lblSumTrips.MinimumSize = new System.Drawing.Size(250, 64);
            lblSumTrips.Name = "lblSumTrips";
            lblSumTrips.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            lblSumTrips.TabIndex = 3;
            lblSumTrips.Text = "—";
            //
            // lblHint
            //
            lblHint.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblHint.AutoSize = true;
            lblHint.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblHint.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblHint.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            lblHint.Name = "lblHint";
            lblHint.TabIndex = 3;
            lblHint.Text = "Bấm 1 xe -> bảng Tài xế cho biết ai đủ hạng bằng lái xe đó; bấm 1 tài xế -> bảng Xe cho biết xe nào người đó lái được.";
            //
            // split — Xe (trên) / Tài xế (dưới); mỗi bảng cao theo số dòng, cả màn cuộn như 1 trang
            //
            split.ColumnCount = 1;
            split.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            split.Controls.Add(grpVehicles, 0, 0);
            split.Controls.Add(grpDrivers, 0, 1);
            split.AutoSize = true;
            split.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            split.Dock = System.Windows.Forms.DockStyle.Fill;
            split.Margin = new System.Windows.Forms.Padding(0);
            split.Name = "split";
            split.RowCount = 2;
            split.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            split.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            split.TabIndex = 4;
            //
            // grpVehicles
            //
            grpVehicles.Controls.Add(vehicleLayout);
            grpVehicles.AutoSize = true;
            grpVehicles.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            grpVehicles.Dock = System.Windows.Forms.DockStyle.Fill;
            grpVehicles.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            grpVehicles.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            grpVehicles.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            grpVehicles.Name = "grpVehicles";
            grpVehicles.Padding = new System.Windows.Forms.Padding(10, 6, 10, 10);
            grpVehicles.TabIndex = 0;
            grpVehicles.Text = "🚚 Xe";
            //
            // vehicleLayout
            //
            vehicleLayout.ColumnCount = 1;
            vehicleLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            vehicleLayout.Controls.Add(vehicleBar, 0, 0);
            vehicleLayout.Controls.Add(vehicleFilter, 0, 1);
            vehicleLayout.Controls.Add(gridVehicles, 0, 2);
            vehicleLayout.AutoSize = true;
            vehicleLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            vehicleLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            vehicleLayout.Font = new System.Drawing.Font("Segoe UI", 9F);
            vehicleLayout.Margin = new System.Windows.Forms.Padding(0);
            vehicleLayout.Name = "vehicleLayout";
            vehicleLayout.RowCount = 3;
            vehicleLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            vehicleLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            vehicleLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            vehicleLayout.TabIndex = 0;
            //
            // vehicleBar
            //
            vehicleBar.AutoSize = true;
            vehicleBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            vehicleBar.Controls.Add(btnVehicleDocs);
            vehicleBar.Controls.Add(btnVehicleFuel);
            vehicleBar.Dock = System.Windows.Forms.DockStyle.Fill;
            vehicleBar.Margin = new System.Windows.Forms.Padding(0);
            vehicleBar.Name = "vehicleBar";
            vehicleBar.TabIndex = 0;
            vehicleBar.WrapContents = true;
            //
            // btnVehicleDocs
            //
            btnVehicleDocs.AutoSize = true;
            btnVehicleDocs.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnVehicleDocs.BackColor = System.Drawing.Color.White;
            btnVehicleDocs.Cursor = System.Windows.Forms.Cursors.Hand;
            btnVehicleDocs.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            btnVehicleDocs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnVehicleDocs.Anchor = System.Windows.Forms.AnchorStyles.Left;
            btnVehicleDocs.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnVehicleDocs.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            btnVehicleDocs.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            btnVehicleDocs.MinimumSize = new System.Drawing.Size(130, 38);
            btnVehicleDocs.Name = "btnVehicleDocs";
            btnVehicleDocs.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnVehicleDocs.TabIndex = 0;
            btnVehicleDocs.Text = "📄 Giấy tờ xe";
            btnVehicleDocs.UseVisualStyleBackColor = false;
            btnVehicleDocs.Click += new System.EventHandler(this.BtnVehicleDocs_Click);
            //
            // btnVehicleFuel
            //
            btnVehicleFuel.AutoSize = true;
            btnVehicleFuel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnVehicleFuel.BackColor = System.Drawing.Color.White;
            btnVehicleFuel.Cursor = System.Windows.Forms.Cursors.Hand;
            btnVehicleFuel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            btnVehicleFuel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnVehicleFuel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            btnVehicleFuel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnVehicleFuel.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            btnVehicleFuel.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            btnVehicleFuel.MinimumSize = new System.Drawing.Size(130, 38);
            btnVehicleFuel.Name = "btnVehicleFuel";
            btnVehicleFuel.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnVehicleFuel.TabIndex = 1;
            btnVehicleFuel.Text = "⛽ Nhiên liệu";
            btnVehicleFuel.UseVisualStyleBackColor = false;
            btnVehicleFuel.Click += new System.EventHandler(this.BtnVehicleFuel_Click);
            //
            // vehicleFilter — tìm nhanh + lọc theo tình trạng (xe chạy được / không / giấy tờ sắp hết hạn...)
            //
            vehicleFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            vehicleFilter.Name = "vehicleFilter";
            vehicleFilter.SearchPlaceholder = "Biển số, loại xe, lý do...";
            vehicleFilter.TabIndex = 1;
            //
            // gridVehicles
            //
            gridVehicles.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gridVehicles.Size = new System.Drawing.Size(1100, 120);
            gridVehicles.Margin = new System.Windows.Forms.Padding(0);
            gridVehicles.Name = "gridVehicles";
            gridVehicles.TabIndex = 1;
            gridVehicles.SelectionChanged += new System.EventHandler(this.GridVehicles_SelectionChanged);
            gridVehicles.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.GridVehicles_CellFormatting);
            //
            // grpDrivers
            //
            grpDrivers.Controls.Add(driverLayout);
            grpDrivers.AutoSize = true;
            grpDrivers.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            grpDrivers.Dock = System.Windows.Forms.DockStyle.Fill;
            grpDrivers.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            grpDrivers.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            grpDrivers.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            grpDrivers.Name = "grpDrivers";
            grpDrivers.Padding = new System.Windows.Forms.Padding(10, 6, 10, 10);
            grpDrivers.TabIndex = 1;
            grpDrivers.Text = "👷 Tài xế";
            //
            // driverLayout
            //
            driverLayout.ColumnCount = 1;
            driverLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            driverLayout.Controls.Add(driverBar, 0, 0);
            driverLayout.Controls.Add(driverFilter, 0, 1);
            driverLayout.Controls.Add(gridDrivers, 0, 2);
            driverLayout.AutoSize = true;
            driverLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            driverLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            driverLayout.Font = new System.Drawing.Font("Segoe UI", 9F);
            driverLayout.Margin = new System.Windows.Forms.Padding(0);
            driverLayout.Name = "driverLayout";
            driverLayout.RowCount = 3;
            driverLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            driverLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            driverLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            driverLayout.TabIndex = 0;
            //
            // driverBar
            //
            driverBar.AutoSize = true;
            driverBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            driverBar.Controls.Add(btnDriverProfile);
            driverBar.Controls.Add(btnDriverWorkDays);
            driverBar.Dock = System.Windows.Forms.DockStyle.Fill;
            driverBar.Margin = new System.Windows.Forms.Padding(0);
            driverBar.Name = "driverBar";
            driverBar.TabIndex = 0;
            driverBar.WrapContents = true;
            //
            // btnDriverProfile
            //
            btnDriverProfile.AutoSize = true;
            btnDriverProfile.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnDriverProfile.BackColor = System.Drawing.Color.White;
            btnDriverProfile.Cursor = System.Windows.Forms.Cursors.Hand;
            btnDriverProfile.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            btnDriverProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnDriverProfile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            btnDriverProfile.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnDriverProfile.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            btnDriverProfile.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            btnDriverProfile.MinimumSize = new System.Drawing.Size(130, 38);
            btnDriverProfile.Name = "btnDriverProfile";
            btnDriverProfile.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnDriverProfile.TabIndex = 0;
            btnDriverProfile.Text = "📝 Hồ sơ tài xế";
            btnDriverProfile.UseVisualStyleBackColor = false;
            btnDriverProfile.Click += new System.EventHandler(this.BtnDriverProfile_Click);
            //
            // btnDriverWorkDays
            //
            btnDriverWorkDays.AutoSize = true;
            btnDriverWorkDays.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnDriverWorkDays.BackColor = System.Drawing.Color.White;
            btnDriverWorkDays.Cursor = System.Windows.Forms.Cursors.Hand;
            btnDriverWorkDays.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            btnDriverWorkDays.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnDriverWorkDays.Anchor = System.Windows.Forms.AnchorStyles.Left;
            btnDriverWorkDays.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnDriverWorkDays.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            btnDriverWorkDays.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
            btnDriverWorkDays.MinimumSize = new System.Drawing.Size(130, 38);
            btnDriverWorkDays.Name = "btnDriverWorkDays";
            btnDriverWorkDays.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnDriverWorkDays.TabIndex = 1;
            btnDriverWorkDays.Text = "📅 Ngày làm";
            btnDriverWorkDays.UseVisualStyleBackColor = false;
            btnDriverWorkDays.Click += new System.EventHandler(this.BtnDriverWorkDays_Click);
            //
            // driverFilter — tìm nhanh + lọc theo tình trạng (tài xế chạy được / không / giấy tờ sắp hết hạn...)
            //
            driverFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            driverFilter.Name = "driverFilter";
            driverFilter.SearchPlaceholder = "Mã NV, họ tên, SĐT, lý do...";
            driverFilter.TabIndex = 1;
            //
            // gridDrivers
            //
            gridDrivers.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gridDrivers.Size = new System.Drawing.Size(1100, 120);
            gridDrivers.Margin = new System.Windows.Forms.Padding(0);
            gridDrivers.Name = "gridDrivers";
            gridDrivers.TabIndex = 1;
            gridDrivers.SelectionChanged += new System.EventHandler(this.GridDrivers_SelectionChanged);
            gridDrivers.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.GridDrivers_CellFormatting);
            //
            // lblStatus
            //
            lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblStatus.AutoSize = true;
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblStatus.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblStatus.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.TabIndex = 5;
            //
            // ResourcesControl
            //
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            AutoScroll = true;
            BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            Controls.Add(root);
            Name = "ResourcesControl";
            Size = new System.Drawing.Size(1200, 760);
            ((System.ComponentModel.ISupportInitialize)gridVehicles).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridDrivers).EndInit();
            driverBar.ResumeLayout(false);
            driverBar.PerformLayout();
            driverLayout.ResumeLayout(false);
            driverLayout.PerformLayout();
            grpDrivers.ResumeLayout(false);
            vehicleBar.ResumeLayout(false);
            vehicleBar.PerformLayout();
            vehicleLayout.ResumeLayout(false);
            vehicleLayout.PerformLayout();
            grpVehicles.ResumeLayout(false);
            split.ResumeLayout(false);
            summaryBar.ResumeLayout(false);
            summaryBar.PerformLayout();
            topBar.ResumeLayout(false);
            topBar.PerformLayout();
            root.ResumeLayout(false);
            root.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.FlowLayoutPanel topBar;
        private System.Windows.Forms.Label lblWarehouse;
        private System.Windows.Forms.ComboBox cboWarehouse;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.FlowLayoutPanel summaryBar;
        private System.Windows.Forms.Label lblSumVehicles;
        private System.Windows.Forms.Label lblSumDrivers;
        private System.Windows.Forms.Label lblSumDocs;
        private System.Windows.Forms.Label lblSumTrips;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.TableLayoutPanel split;
        private System.Windows.Forms.GroupBox grpVehicles;
        private System.Windows.Forms.TableLayoutPanel vehicleLayout;
        private System.Windows.Forms.FlowLayoutPanel vehicleBar;
        private System.Windows.Forms.Button btnVehicleDocs;
        private System.Windows.Forms.Button btnVehicleFuel;
        private TruckingSystem.Desktop.Common.FilterBar vehicleFilter;
        private System.Windows.Forms.DataGridView gridVehicles;
        private System.Windows.Forms.GroupBox grpDrivers;
        private System.Windows.Forms.TableLayoutPanel driverLayout;
        private System.Windows.Forms.FlowLayoutPanel driverBar;
        private System.Windows.Forms.Button btnDriverProfile;
        private System.Windows.Forms.Button btnDriverWorkDays;
        private TruckingSystem.Desktop.Common.FilterBar driverFilter;
        private System.Windows.Forms.DataGridView gridDrivers;
        private System.Windows.Forms.Label lblStatus;
    }
}
