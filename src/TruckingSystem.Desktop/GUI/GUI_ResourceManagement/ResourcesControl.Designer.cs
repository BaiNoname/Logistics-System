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
            root = new TableLayoutPanel();
            lblTitle = new Label();
            topBar = new FlowLayoutPanel();
            lblWarehouse = new Label();
            cboWarehouse = new ComboBox();
            btnRefresh = new Button();
            summaryBar = new FlowLayoutPanel();
            lblSumVehicles = new Label();
            lblSumDrivers = new Label();
            lblSumDocs = new Label();
            lblSumTrips = new Label();
            lblHint = new Label();
            split = new TableLayoutPanel();
            grpVehicles = new GroupBox();
            vehicleLayout = new TableLayoutPanel();
            vehicleBar = new FlowLayoutPanel();
            btnVehicleDocs = new Button();
            btnVehicleFuel = new Button();
            chkVehicleProblems = new CheckBox();
            gridVehicles = new DataGridView();
            grpDrivers = new GroupBox();
            driverLayout = new TableLayoutPanel();
            driverBar = new FlowLayoutPanel();
            btnDriverProfile = new Button();
            btnDriverWorkDays = new Button();
            chkDriverProblems = new CheckBox();
            gridDrivers = new DataGridView();
            lblStatus = new Label();
            root.SuspendLayout();
            topBar.SuspendLayout();
            summaryBar.SuspendLayout();
            split.SuspendLayout();
            grpVehicles.SuspendLayout();
            vehicleLayout.SuspendLayout();
            vehicleBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridVehicles).BeginInit();
            grpDrivers.SuspendLayout();
            driverLayout.SuspendLayout();
            driverBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridDrivers).BeginInit();
            SuspendLayout();
            // 
            // root
            // 
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            root.Controls.Add(lblTitle, 0, 0);
            root.Controls.Add(topBar, 0, 1);
            root.Controls.Add(summaryBar, 0, 2);
            root.Controls.Add(lblHint, 0, 3);
            root.Controls.Add(split, 0, 4);
            root.Controls.Add(lblStatus, 0, 5);
            root.Dock = DockStyle.Fill;
            root.Location = new Point(0, 0);
            root.Margin = new Padding(0);
            root.Name = "root";
            root.RowCount = 6;
            root.RowStyles.Add(new RowStyle());
            root.RowStyles.Add(new RowStyle());
            root.RowStyles.Add(new RowStyle());
            root.RowStyles.Add(new RowStyle());
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle());
            root.Size = new Size(1200, 760);
            root.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(0, 0);
            lblTitle.Margin = new Padding(0, 0, 0, 6);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(283, 32);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "\U0001f9ed Nguồn lực theo kho";
            // 
            // topBar
            // 
            topBar.AutoSize = true;
            topBar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            topBar.Controls.Add(lblWarehouse);
            topBar.Controls.Add(cboWarehouse);
            topBar.Controls.Add(btnRefresh);
            topBar.Dock = DockStyle.Fill;
            topBar.Location = new Point(0, 38);
            topBar.Margin = new Padding(0);
            topBar.Name = "topBar";
            topBar.Size = new Size(1200, 40);
            topBar.TabIndex = 1;
            // 
            // lblWarehouse
            // 
            lblWarehouse.AutoSize = true;
            lblWarehouse.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblWarehouse.Location = new Point(0, 8);
            lblWarehouse.Margin = new Padding(0, 8, 6, 0);
            lblWarehouse.Name = "lblWarehouse";
            lblWarehouse.Size = new Size(39, 19);
            lblWarehouse.TabIndex = 0;
            lblWarehouse.Text = "Kho:";
            // 
            // cboWarehouse
            // 
            cboWarehouse.DropDownStyle = ComboBoxStyle.DropDownList;
            cboWarehouse.Font = new Font("Segoe UI", 10F);
            cboWarehouse.Location = new Point(45, 4);
            cboWarehouse.Margin = new Padding(0, 4, 10, 6);
            cboWarehouse.MinimumSize = new Size(240, 0);
            cboWarehouse.Name = "cboWarehouse";
            cboWarehouse.Size = new Size(240, 25);
            cboWarehouse.TabIndex = 1;
            cboWarehouse.SelectedIndexChanged += CboWarehouse_SelectedIndexChanged;
            // 
            // btnRefresh
            // 
            btnRefresh.AutoSize = true;
            btnRefresh.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnRefresh.BackColor = Color.White;
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnRefresh.ForeColor = Color.FromArgb(15, 23, 42);
            btnRefresh.Location = new Point(295, 0);
            btnRefresh.Margin = new Padding(0, 0, 8, 6);
            btnRefresh.MinimumSize = new Size(0, 34);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Padding = new Padding(8, 2, 8, 2);
            btnRefresh.Size = new Size(113, 34);
            btnRefresh.TabIndex = 2;
            btnRefresh.Text = "🔄 Làm mới";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Click += BtnRefresh_Click;
            // 
            // summaryBar
            // 
            summaryBar.AutoSize = true;
            summaryBar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            summaryBar.Controls.Add(lblSumVehicles);
            summaryBar.Controls.Add(lblSumDrivers);
            summaryBar.Controls.Add(lblSumDocs);
            summaryBar.Controls.Add(lblSumTrips);
            summaryBar.Dock = DockStyle.Fill;
            summaryBar.Location = new Point(0, 78);
            summaryBar.Margin = new Padding(0);
            summaryBar.Name = "summaryBar";
            summaryBar.Size = new Size(1200, 45);
            summaryBar.TabIndex = 2;
            // 
            // lblSumVehicles
            // 
            lblSumVehicles.AutoSize = true;
            lblSumVehicles.BackColor = Color.White;
            lblSumVehicles.BorderStyle = BorderStyle.FixedSingle;
            lblSumVehicles.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblSumVehicles.ForeColor = Color.FromArgb(15, 23, 42);
            lblSumVehicles.Location = new Point(0, 0);
            lblSumVehicles.Margin = new Padding(0, 0, 10, 8);
            lblSumVehicles.MinimumSize = new Size(170, 0);
            lblSumVehicles.Name = "lblSumVehicles";
            lblSumVehicles.Padding = new Padding(10, 8, 10, 8);
            lblSumVehicles.Size = new Size(170, 37);
            lblSumVehicles.TabIndex = 0;
            lblSumVehicles.Text = "—";
            // 
            // lblSumDrivers
            // 
            lblSumDrivers.AutoSize = true;
            lblSumDrivers.BackColor = Color.White;
            lblSumDrivers.BorderStyle = BorderStyle.FixedSingle;
            lblSumDrivers.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblSumDrivers.ForeColor = Color.FromArgb(15, 23, 42);
            lblSumDrivers.Location = new Point(180, 0);
            lblSumDrivers.Margin = new Padding(0, 0, 10, 8);
            lblSumDrivers.MinimumSize = new Size(170, 0);
            lblSumDrivers.Name = "lblSumDrivers";
            lblSumDrivers.Padding = new Padding(10, 8, 10, 8);
            lblSumDrivers.Size = new Size(170, 37);
            lblSumDrivers.TabIndex = 1;
            lblSumDrivers.Text = "—";
            // 
            // lblSumDocs
            // 
            lblSumDocs.AutoSize = true;
            lblSumDocs.BackColor = Color.White;
            lblSumDocs.BorderStyle = BorderStyle.FixedSingle;
            lblSumDocs.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblSumDocs.ForeColor = Color.FromArgb(15, 23, 42);
            lblSumDocs.Location = new Point(360, 0);
            lblSumDocs.Margin = new Padding(0, 0, 10, 8);
            lblSumDocs.MinimumSize = new Size(170, 0);
            lblSumDocs.Name = "lblSumDocs";
            lblSumDocs.Padding = new Padding(10, 8, 10, 8);
            lblSumDocs.Size = new Size(170, 37);
            lblSumDocs.TabIndex = 2;
            lblSumDocs.Text = "—";
            // 
            // lblSumTrips
            // 
            lblSumTrips.AutoSize = true;
            lblSumTrips.BackColor = Color.White;
            lblSumTrips.BorderStyle = BorderStyle.FixedSingle;
            lblSumTrips.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblSumTrips.ForeColor = Color.FromArgb(15, 23, 42);
            lblSumTrips.Location = new Point(540, 0);
            lblSumTrips.Margin = new Padding(0, 0, 10, 8);
            lblSumTrips.MinimumSize = new Size(170, 0);
            lblSumTrips.Name = "lblSumTrips";
            lblSumTrips.Padding = new Padding(10, 8, 10, 8);
            lblSumTrips.Size = new Size(170, 37);
            lblSumTrips.TabIndex = 3;
            lblSumTrips.Text = "—";
            // 
            // lblHint
            // 
            lblHint.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblHint.AutoSize = true;
            lblHint.Font = new Font("Segoe UI", 9F);
            lblHint.ForeColor = Color.FromArgb(100, 116, 139);
            lblHint.Location = new Point(0, 123);
            lblHint.Margin = new Padding(0, 0, 0, 6);
            lblHint.Name = "lblHint";
            lblHint.Size = new Size(1200, 15);
            lblHint.TabIndex = 3;
            lblHint.Text = "Bấm 1 xe -> bảng Tài xế cho biết ai đủ hạng bằng lái xe đó; bấm 1 tài xế -> bảng Xe cho biết xe nào người đó lái được.";
            // 
            // split
            // 
            split.ColumnCount = 1;
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            split.Controls.Add(grpVehicles, 0, 0);
            split.Controls.Add(grpDrivers, 0, 1);
            split.Dock = DockStyle.Fill;
            split.Location = new Point(0, 144);
            split.Margin = new Padding(0);
            split.Name = "split";
            split.RowCount = 2;
            split.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            split.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            split.Size = new Size(1200, 597);
            split.TabIndex = 4;
            // 
            // grpVehicles
            // 
            grpVehicles.Controls.Add(vehicleLayout);
            grpVehicles.Dock = DockStyle.Fill;
            grpVehicles.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            grpVehicles.ForeColor = Color.FromArgb(15, 23, 42);
            grpVehicles.Location = new Point(0, 0);
            grpVehicles.Margin = new Padding(0, 0, 0, 8);
            grpVehicles.Name = "grpVehicles";
            grpVehicles.Padding = new Padding(8, 4, 8, 8);
            grpVehicles.Size = new Size(1200, 290);
            grpVehicles.TabIndex = 0;
            grpVehicles.TabStop = false;
            grpVehicles.Text = "🚚 Xe";
            // 
            // vehicleLayout
            // 
            vehicleLayout.ColumnCount = 1;
            vehicleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            vehicleLayout.Controls.Add(vehicleBar, 0, 0);
            vehicleLayout.Controls.Add(gridVehicles, 0, 1);
            vehicleLayout.Dock = DockStyle.Fill;
            vehicleLayout.Font = new Font("Segoe UI", 9F);
            vehicleLayout.Location = new Point(8, 22);
            vehicleLayout.Margin = new Padding(0);
            vehicleLayout.Name = "vehicleLayout";
            vehicleLayout.RowCount = 2;
            vehicleLayout.RowStyles.Add(new RowStyle());
            vehicleLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            vehicleLayout.Size = new Size(1184, 260);
            vehicleLayout.TabIndex = 0;
            // 
            // vehicleBar
            // 
            vehicleBar.AutoSize = true;
            vehicleBar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            vehicleBar.Controls.Add(btnVehicleDocs);
            vehicleBar.Controls.Add(btnVehicleFuel);
            vehicleBar.Controls.Add(chkVehicleProblems);
            vehicleBar.Dock = DockStyle.Fill;
            vehicleBar.Location = new Point(0, 0);
            vehicleBar.Margin = new Padding(0);
            vehicleBar.Name = "vehicleBar";
            vehicleBar.Size = new Size(1184, 40);
            vehicleBar.TabIndex = 0;
            // 
            // btnVehicleDocs
            // 
            btnVehicleDocs.AutoSize = true;
            btnVehicleDocs.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnVehicleDocs.BackColor = Color.White;
            btnVehicleDocs.Cursor = Cursors.Hand;
            btnVehicleDocs.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnVehicleDocs.FlatStyle = FlatStyle.Flat;
            btnVehicleDocs.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnVehicleDocs.ForeColor = Color.FromArgb(15, 23, 42);
            btnVehicleDocs.Location = new Point(0, 0);
            btnVehicleDocs.Margin = new Padding(0, 0, 8, 6);
            btnVehicleDocs.MinimumSize = new Size(0, 34);
            btnVehicleDocs.Name = "btnVehicleDocs";
            btnVehicleDocs.Padding = new Padding(8, 2, 8, 2);
            btnVehicleDocs.Size = new Size(121, 34);
            btnVehicleDocs.TabIndex = 0;
            btnVehicleDocs.Text = "📄 Giấy tờ xe";
            btnVehicleDocs.UseVisualStyleBackColor = false;
            btnVehicleDocs.Click += BtnVehicleDocs_Click;
            // 
            // btnVehicleFuel
            // 
            btnVehicleFuel.AutoSize = true;
            btnVehicleFuel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnVehicleFuel.BackColor = Color.White;
            btnVehicleFuel.Cursor = Cursors.Hand;
            btnVehicleFuel.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnVehicleFuel.FlatStyle = FlatStyle.Flat;
            btnVehicleFuel.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnVehicleFuel.ForeColor = Color.FromArgb(15, 23, 42);
            btnVehicleFuel.Location = new Point(129, 0);
            btnVehicleFuel.Margin = new Padding(0, 0, 8, 6);
            btnVehicleFuel.MinimumSize = new Size(0, 34);
            btnVehicleFuel.Name = "btnVehicleFuel";
            btnVehicleFuel.Padding = new Padding(8, 2, 8, 2);
            btnVehicleFuel.Size = new Size(123, 34);
            btnVehicleFuel.TabIndex = 1;
            btnVehicleFuel.Text = "⛽ Nhiên liệu";
            btnVehicleFuel.UseVisualStyleBackColor = false;
            btnVehicleFuel.Click += BtnVehicleFuel_Click;
            // 
            // chkVehicleProblems
            // 
            chkVehicleProblems.AutoSize = true;
            chkVehicleProblems.Font = new Font("Segoe UI", 9F);
            chkVehicleProblems.Location = new Point(268, 8);
            chkVehicleProblems.Margin = new Padding(8, 8, 0, 0);
            chkVehicleProblems.Name = "chkVehicleProblems";
            chkVehicleProblems.Size = new Size(298, 19);
            chkVehicleProblems.TabIndex = 2;
            chkVehicleProblems.Text = "Chỉ hiện xe KHÔNG chạy được / giấy tờ sắp hết hạn";
            chkVehicleProblems.UseVisualStyleBackColor = true;
            chkVehicleProblems.CheckedChanged += ProblemsOnly_CheckedChanged;
            // 
            // gridVehicles
            // 
            gridVehicles.Dock = DockStyle.Fill;
            gridVehicles.Location = new Point(0, 40);
            gridVehicles.Margin = new Padding(0);
            gridVehicles.Name = "gridVehicles";
            gridVehicles.Size = new Size(1184, 220);
            gridVehicles.TabIndex = 1;
            gridVehicles.CellFormatting += GridVehicles_CellFormatting;
            gridVehicles.SelectionChanged += GridVehicles_SelectionChanged;
            // 
            // grpDrivers
            // 
            grpDrivers.Controls.Add(driverLayout);
            grpDrivers.Dock = DockStyle.Fill;
            grpDrivers.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            grpDrivers.ForeColor = Color.FromArgb(15, 23, 42);
            grpDrivers.Location = new Point(0, 298);
            grpDrivers.Margin = new Padding(0, 0, 0, 8);
            grpDrivers.Name = "grpDrivers";
            grpDrivers.Padding = new Padding(8, 4, 8, 8);
            grpDrivers.Size = new Size(1200, 291);
            grpDrivers.TabIndex = 1;
            grpDrivers.TabStop = false;
            grpDrivers.Text = "👷 Tài xế";
            // 
            // driverLayout
            // 
            driverLayout.ColumnCount = 1;
            driverLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            driverLayout.Controls.Add(driverBar, 0, 0);
            driverLayout.Controls.Add(gridDrivers, 0, 1);
            driverLayout.Dock = DockStyle.Fill;
            driverLayout.Font = new Font("Segoe UI", 9F);
            driverLayout.Location = new Point(8, 22);
            driverLayout.Margin = new Padding(0);
            driverLayout.Name = "driverLayout";
            driverLayout.RowCount = 2;
            driverLayout.RowStyles.Add(new RowStyle());
            driverLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            driverLayout.Size = new Size(1184, 261);
            driverLayout.TabIndex = 0;
            // 
            // driverBar
            // 
            driverBar.AutoSize = true;
            driverBar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            driverBar.Controls.Add(btnDriverProfile);
            driverBar.Controls.Add(btnDriverWorkDays);
            driverBar.Controls.Add(chkDriverProblems);
            driverBar.Dock = DockStyle.Fill;
            driverBar.Location = new Point(0, 0);
            driverBar.Margin = new Padding(0);
            driverBar.Name = "driverBar";
            driverBar.Size = new Size(1184, 40);
            driverBar.TabIndex = 0;
            // 
            // btnDriverProfile
            // 
            btnDriverProfile.AutoSize = true;
            btnDriverProfile.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnDriverProfile.BackColor = Color.White;
            btnDriverProfile.Cursor = Cursors.Hand;
            btnDriverProfile.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnDriverProfile.FlatStyle = FlatStyle.Flat;
            btnDriverProfile.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnDriverProfile.ForeColor = Color.FromArgb(15, 23, 42);
            btnDriverProfile.Location = new Point(0, 0);
            btnDriverProfile.Margin = new Padding(0, 0, 8, 6);
            btnDriverProfile.MinimumSize = new Size(0, 34);
            btnDriverProfile.Name = "btnDriverProfile";
            btnDriverProfile.Padding = new Padding(8, 2, 8, 2);
            btnDriverProfile.Size = new Size(133, 34);
            btnDriverProfile.TabIndex = 0;
            btnDriverProfile.Text = "📝 Hồ sơ tài xế";
            btnDriverProfile.UseVisualStyleBackColor = false;
            btnDriverProfile.Click += BtnDriverProfile_Click;
            // 
            // btnDriverWorkDays
            // 
            btnDriverWorkDays.AutoSize = true;
            btnDriverWorkDays.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnDriverWorkDays.BackColor = Color.White;
            btnDriverWorkDays.Cursor = Cursors.Hand;
            btnDriverWorkDays.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnDriverWorkDays.FlatStyle = FlatStyle.Flat;
            btnDriverWorkDays.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnDriverWorkDays.ForeColor = Color.FromArgb(15, 23, 42);
            btnDriverWorkDays.Location = new Point(141, 0);
            btnDriverWorkDays.Margin = new Padding(0, 0, 8, 6);
            btnDriverWorkDays.MinimumSize = new Size(0, 34);
            btnDriverWorkDays.Name = "btnDriverWorkDays";
            btnDriverWorkDays.Padding = new Padding(8, 2, 8, 2);
            btnDriverWorkDays.Size = new Size(118, 34);
            btnDriverWorkDays.TabIndex = 1;
            btnDriverWorkDays.Text = "📅 Ngày làm";
            btnDriverWorkDays.UseVisualStyleBackColor = false;
            btnDriverWorkDays.Click += BtnDriverWorkDays_Click;
            // 
            // chkDriverProblems
            // 
            chkDriverProblems.AutoSize = true;
            chkDriverProblems.Font = new Font("Segoe UI", 9F);
            chkDriverProblems.Location = new Point(275, 8);
            chkDriverProblems.Margin = new Padding(8, 8, 0, 0);
            chkDriverProblems.Name = "chkDriverProblems";
            chkDriverProblems.Size = new Size(314, 19);
            chkDriverProblems.TabIndex = 2;
            chkDriverProblems.Text = "Chỉ hiện tài xế KHÔNG chạy được / giấy tờ sắp hết hạn";
            chkDriverProblems.UseVisualStyleBackColor = true;
            chkDriverProblems.CheckedChanged += ProblemsOnly_CheckedChanged;
            // 
            // gridDrivers
            // 
            gridDrivers.Dock = DockStyle.Fill;
            gridDrivers.Location = new Point(0, 40);
            gridDrivers.Margin = new Padding(0);
            gridDrivers.Name = "gridDrivers";
            gridDrivers.Size = new Size(1184, 221);
            gridDrivers.TabIndex = 1;
            gridDrivers.CellFormatting += GridDrivers_CellFormatting;
            gridDrivers.SelectionChanged += GridDrivers_SelectionChanged;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
            lblStatus.Location = new Point(0, 745);
            lblStatus.Margin = new Padding(0, 4, 0, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(1200, 15);
            lblStatus.TabIndex = 5;
            // 
            // ResourcesControl
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(241, 245, 249);
            Controls.Add(root);
            Name = "ResourcesControl";
            Size = new Size(1200, 760);
            root.ResumeLayout(false);
            root.PerformLayout();
            topBar.ResumeLayout(false);
            topBar.PerformLayout();
            summaryBar.ResumeLayout(false);
            summaryBar.PerformLayout();
            split.ResumeLayout(false);
            grpVehicles.ResumeLayout(false);
            vehicleLayout.ResumeLayout(false);
            vehicleLayout.PerformLayout();
            vehicleBar.ResumeLayout(false);
            vehicleBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)gridVehicles).EndInit();
            grpDrivers.ResumeLayout(false);
            driverLayout.ResumeLayout(false);
            driverLayout.PerformLayout();
            driverBar.ResumeLayout(false);
            driverBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)gridDrivers).EndInit();
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
        private System.Windows.Forms.CheckBox chkVehicleProblems;
        private System.Windows.Forms.DataGridView gridVehicles;
        private System.Windows.Forms.GroupBox grpDrivers;
        private System.Windows.Forms.TableLayoutPanel driverLayout;
        private System.Windows.Forms.FlowLayoutPanel driverBar;
        private System.Windows.Forms.Button btnDriverProfile;
        private System.Windows.Forms.Button btnDriverWorkDays;
        private System.Windows.Forms.CheckBox chkDriverProblems;
        private System.Windows.Forms.DataGridView gridDrivers;
        private System.Windows.Forms.Label lblStatus;
    }
}
