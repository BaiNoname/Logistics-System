namespace TruckingSystem.Desktop.GUI_TripManagement;

partial class TripsControl
{
    private System.ComponentModel.IContainer components = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary>Bố cục: TableLayoutPanel 1 cột — tiêu đề, thanh nút (FlowLayoutPanel tự xuống
    /// dòng khi màn hình hẹp), dòng trạng thái, lưới chiếm phần còn lại. Không dùng tọa độ cứng
    /// -> không đè chữ ở mọi kích thước / mức phóng to màn hình. Sửa được bằng Designer.</summary>
    private void InitializeComponent()
    {
        root = new System.Windows.Forms.TableLayoutPanel();
        lblTitle = new System.Windows.Forms.Label();
        toolbar = new System.Windows.Forms.FlowLayoutPanel();
        btnRefresh = new System.Windows.Forms.Button();
        btnStart = new System.Windows.Forms.Button();
        btnComplete = new System.Windows.Forms.Button();
        btnApprove = new System.Windows.Forms.Button();
        btnReject = new System.Windows.Forms.Button();
        btnCargoPhotos = new System.Windows.Forms.Button();
        btnReassign = new System.Windows.Forms.Button();
        btnReturnAtWarehouse = new System.Windows.Forms.Button();
        btnFuel = new System.Windows.Forms.Button();
        btnFuelEstimate = new System.Windows.Forms.Button();
        btnExpenses = new System.Windows.Forms.Button();
        btnIncidents = new System.Windows.Forms.Button();
        btnQuickEstimate = new System.Windows.Forms.Button();
        btnViewRoute = new System.Windows.Forms.Button();
        btnSyncCalendar = new System.Windows.Forms.Button();
        btnDriverInfo = new System.Windows.Forms.Button();
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
        root.Location = new System.Drawing.Point(0, 0);
        root.Name = "root";
        root.Padding = new System.Windows.Forms.Padding(0);
        root.RowCount = 4;
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        root.Size = new System.Drawing.Size(1200, 700);
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
        lblTitle.Text = "Quản lý chuyến (Trip)";
        // 
        // toolbar
        // 
        toolbar.AutoSize = true;
        toolbar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        toolbar.Controls.Add(btnRefresh);
        toolbar.Controls.Add(btnStart);
        toolbar.Controls.Add(btnComplete);
        toolbar.Controls.Add(btnApprove);
        toolbar.Controls.Add(btnReject);
        toolbar.Controls.Add(btnCargoPhotos);
        toolbar.Controls.Add(btnReassign);
        toolbar.Controls.Add(btnReturnAtWarehouse);
        toolbar.Controls.Add(btnFuel);
        toolbar.Controls.Add(btnFuelEstimate);
        toolbar.Controls.Add(btnExpenses);
        toolbar.Controls.Add(btnIncidents);
        toolbar.Controls.Add(btnQuickEstimate);
        toolbar.Controls.Add(btnViewRoute);
        toolbar.Controls.Add(btnSyncCalendar);
        toolbar.Controls.Add(btnDriverInfo);
        toolbar.Dock = System.Windows.Forms.DockStyle.Fill;
        toolbar.Margin = new System.Windows.Forms.Padding(0);
        toolbar.Name = "toolbar";
        toolbar.TabIndex = 1;
        toolbar.WrapContents = true;
        // 
        // btnRefresh
        // 
        btnRefresh.AutoSize = true;
        btnRefresh.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        btnRefresh.BackColor = System.Drawing.Color.White;
        btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
        btnRefresh.FlatAppearance.BorderSize = 1;
        btnRefresh.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
        btnRefresh.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(248, 250, 252);
        btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnRefresh.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        btnRefresh.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
        btnRefresh.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
        btnRefresh.MinimumSize = new System.Drawing.Size(0, 38);
        btnRefresh.Name = "btnRefresh";
        btnRefresh.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        btnRefresh.TabIndex = 0;
        btnRefresh.Text = "🔄 Làm mới";
        btnRefresh.UseVisualStyleBackColor = false;
        btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
        // 
        // btnStart
        // 
        btnStart.AutoSize = true;
        btnStart.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        btnStart.BackColor = System.Drawing.Color.White;
        btnStart.Cursor = System.Windows.Forms.Cursors.Hand;
        btnStart.FlatAppearance.BorderSize = 1;
        btnStart.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
        btnStart.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(248, 250, 252);
        btnStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnStart.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        btnStart.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
        btnStart.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
        btnStart.MinimumSize = new System.Drawing.Size(0, 38);
        btnStart.Name = "btnStart";
        btnStart.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        btnStart.TabIndex = 1;
        btnStart.Text = "▶️ Bắt đầu chuyến";
        btnStart.UseVisualStyleBackColor = false;
        btnStart.Click += new System.EventHandler(this.BtnStart_Click);
        // 
        // btnComplete
        // 
        btnComplete.AutoSize = true;
        btnComplete.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        btnComplete.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
        btnComplete.Cursor = System.Windows.Forms.Cursors.Hand;
        btnComplete.FlatAppearance.BorderSize = 0;
        btnComplete.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
        btnComplete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnComplete.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        btnComplete.ForeColor = System.Drawing.Color.White;
        btnComplete.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
        btnComplete.MinimumSize = new System.Drawing.Size(0, 38);
        btnComplete.Name = "btnComplete";
        btnComplete.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        btnComplete.TabIndex = 2;
        btnComplete.Text = "✅ Hoàn tất chuyến";
        btnComplete.UseVisualStyleBackColor = false;
        btnComplete.Click += new System.EventHandler(this.BtnComplete_Click);
        // 
        // btnApprove
        // 
        btnApprove.AutoSize = true;
        btnApprove.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        btnApprove.BackColor = System.Drawing.Color.FromArgb(22, 163, 74);
        btnApprove.Cursor = System.Windows.Forms.Cursors.Hand;
        btnApprove.FlatAppearance.BorderSize = 0;
        btnApprove.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(21, 128, 61);
        btnApprove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnApprove.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        btnApprove.ForeColor = System.Drawing.Color.White;
        btnApprove.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
        btnApprove.MinimumSize = new System.Drawing.Size(0, 38);
        btnApprove.Name = "btnApprove";
        btnApprove.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        btnApprove.TabIndex = 3;
        btnApprove.Text = "✅ Duyệt xác nhận tài xế";
        btnApprove.UseVisualStyleBackColor = false;
        btnApprove.Click += new System.EventHandler(this.BtnApprove_Click);
        // 
        // btnReject
        // 
        btnReject.AutoSize = true;
        btnReject.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        btnReject.BackColor = System.Drawing.Color.White;
        btnReject.Cursor = System.Windows.Forms.Cursors.Hand;
        btnReject.FlatAppearance.BorderSize = 1;
        btnReject.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(220, 38, 38);
        btnReject.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(254, 242, 242);
        btnReject.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnReject.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        btnReject.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
        btnReject.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
        btnReject.MinimumSize = new System.Drawing.Size(0, 38);
        btnReject.Name = "btnReject";
        btnReject.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        btnReject.TabIndex = 4;
        btnReject.Text = "❌ Từ chối";
        btnReject.UseVisualStyleBackColor = false;
        btnReject.Click += new System.EventHandler(this.BtnReject_Click);
        // 
        // btnCargoPhotos
        // 
        btnCargoPhotos.AutoSize = true;
        btnCargoPhotos.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        btnCargoPhotos.BackColor = System.Drawing.Color.White;
        btnCargoPhotos.Cursor = System.Windows.Forms.Cursors.Hand;
        btnCargoPhotos.FlatAppearance.BorderSize = 1;
        btnCargoPhotos.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
        btnCargoPhotos.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(248, 250, 252);
        btnCargoPhotos.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnCargoPhotos.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        btnCargoPhotos.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
        btnCargoPhotos.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
        btnCargoPhotos.MinimumSize = new System.Drawing.Size(0, 38);
        btnCargoPhotos.Name = "btnCargoPhotos";
        btnCargoPhotos.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        btnCargoPhotos.TabIndex = 5;
        btnCargoPhotos.Text = "📷 Ảnh hàng";
        btnCargoPhotos.UseVisualStyleBackColor = false;
        btnCargoPhotos.Click += new System.EventHandler(this.BtnCargoPhotos_Click);
        // 
        // btnReassign
        // 
        btnReassign.AutoSize = true;
        btnReassign.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        btnReassign.BackColor = System.Drawing.Color.White;
        btnReassign.Cursor = System.Windows.Forms.Cursors.Hand;
        btnReassign.FlatAppearance.BorderSize = 1;
        btnReassign.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
        btnReassign.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(248, 250, 252);
        btnReassign.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnReassign.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        btnReassign.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
        btnReassign.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
        btnReassign.MinimumSize = new System.Drawing.Size(0, 38);
        btnReassign.Name = "btnReassign";
        btnReassign.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        btnReassign.TabIndex = 6;
        btnReassign.Text = "🔁 Đổi xe / tài xế";
        btnReassign.UseVisualStyleBackColor = false;
        btnReassign.Click += new System.EventHandler(this.BtnReassign_Click);
        // 
        // btnReturnAtWarehouse
        // 
        btnReturnAtWarehouse.AutoSize = true;
        btnReturnAtWarehouse.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        btnReturnAtWarehouse.BackColor = System.Drawing.Color.White;
        btnReturnAtWarehouse.Cursor = System.Windows.Forms.Cursors.Hand;
        btnReturnAtWarehouse.FlatAppearance.BorderSize = 1;
        btnReturnAtWarehouse.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
        btnReturnAtWarehouse.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(248, 250, 252);
        btnReturnAtWarehouse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnReturnAtWarehouse.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        btnReturnAtWarehouse.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
        btnReturnAtWarehouse.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
        btnReturnAtWarehouse.MinimumSize = new System.Drawing.Size(0, 38);
        btnReturnAtWarehouse.Name = "btnReturnAtWarehouse";
        btnReturnAtWarehouse.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        btnReturnAtWarehouse.TabIndex = 7;
        btnReturnAtWarehouse.Text = "🏬 Trả khách tại kho";
        btnReturnAtWarehouse.UseVisualStyleBackColor = false;
        btnReturnAtWarehouse.Click += new System.EventHandler(this.BtnReturnAtWarehouse_Click);
        // 
        // btnFuel
        // 
        btnFuel.AutoSize = true;
        btnFuel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        btnFuel.BackColor = System.Drawing.Color.White;
        btnFuel.Cursor = System.Windows.Forms.Cursors.Hand;
        btnFuel.FlatAppearance.BorderSize = 1;
        btnFuel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
        btnFuel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(248, 250, 252);
        btnFuel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnFuel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        btnFuel.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
        btnFuel.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
        btnFuel.MinimumSize = new System.Drawing.Size(0, 38);
        btnFuel.Name = "btnFuel";
        btnFuel.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        btnFuel.TabIndex = 8;
        btnFuel.Text = "⛽ Ghi xăng";
        btnFuel.UseVisualStyleBackColor = false;
        btnFuel.Click += new System.EventHandler(this.BtnFuel_Click);
        // 
        // btnFuelEstimate
        // 
        btnFuelEstimate.AutoSize = true;
        btnFuelEstimate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        btnFuelEstimate.BackColor = System.Drawing.Color.White;
        btnFuelEstimate.Cursor = System.Windows.Forms.Cursors.Hand;
        btnFuelEstimate.FlatAppearance.BorderSize = 1;
        btnFuelEstimate.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
        btnFuelEstimate.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(248, 250, 252);
        btnFuelEstimate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnFuelEstimate.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        btnFuelEstimate.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
        btnFuelEstimate.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
        btnFuelEstimate.MinimumSize = new System.Drawing.Size(0, 38);
        btnFuelEstimate.Name = "btnFuelEstimate";
        btnFuelEstimate.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        btnFuelEstimate.TabIndex = 9;
        btnFuelEstimate.Text = "⛽ Ước tính nhiên liệu";
        btnFuelEstimate.UseVisualStyleBackColor = false;
        btnFuelEstimate.Click += new System.EventHandler(this.BtnFuelEstimate_Click);
        // 
        // btnExpenses
        // 
        btnExpenses.AutoSize = true;
        btnExpenses.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        btnExpenses.BackColor = System.Drawing.Color.White;
        btnExpenses.Cursor = System.Windows.Forms.Cursors.Hand;
        btnExpenses.FlatAppearance.BorderSize = 1;
        btnExpenses.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
        btnExpenses.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(248, 250, 252);
        btnExpenses.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnExpenses.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        btnExpenses.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
        btnExpenses.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
        btnExpenses.MinimumSize = new System.Drawing.Size(0, 38);
        btnExpenses.Name = "btnExpenses";
        btnExpenses.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        btnExpenses.TabIndex = 10;
        btnExpenses.Text = "💰 Chi phí khác";
        btnExpenses.UseVisualStyleBackColor = false;
        btnExpenses.Click += new System.EventHandler(this.BtnExpenses_Click);
        // 
        // btnIncidents
        // 
        btnIncidents.AutoSize = true;
        btnIncidents.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        btnIncidents.BackColor = System.Drawing.Color.White;
        btnIncidents.Cursor = System.Windows.Forms.Cursors.Hand;
        btnIncidents.FlatAppearance.BorderSize = 1;
        btnIncidents.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(220, 38, 38);
        btnIncidents.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(254, 242, 242);
        btnIncidents.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnIncidents.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        btnIncidents.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
        btnIncidents.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
        btnIncidents.MinimumSize = new System.Drawing.Size(0, 38);
        btnIncidents.Name = "btnIncidents";
        btnIncidents.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        btnIncidents.TabIndex = 11;
        btnIncidents.Text = "⚠️ Sự cố";
        btnIncidents.UseVisualStyleBackColor = false;
        btnIncidents.Click += new System.EventHandler(this.BtnIncidents_Click);
        // 
        // btnQuickEstimate
        // 
        btnQuickEstimate.AutoSize = true;
        btnQuickEstimate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        btnQuickEstimate.BackColor = System.Drawing.Color.White;
        btnQuickEstimate.Cursor = System.Windows.Forms.Cursors.Hand;
        btnQuickEstimate.FlatAppearance.BorderSize = 1;
        btnQuickEstimate.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
        btnQuickEstimate.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(248, 250, 252);
        btnQuickEstimate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnQuickEstimate.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        btnQuickEstimate.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
        btnQuickEstimate.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
        btnQuickEstimate.MinimumSize = new System.Drawing.Size(0, 38);
        btnQuickEstimate.Name = "btnQuickEstimate";
        btnQuickEstimate.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        btnQuickEstimate.TabIndex = 12;
        btnQuickEstimate.Text = "📏 Ước tính lộ trình";
        btnQuickEstimate.UseVisualStyleBackColor = false;
        btnQuickEstimate.Click += new System.EventHandler(this.BtnQuickEstimate_Click);
        // 
        // btnViewRoute
        // 
        btnViewRoute.AutoSize = true;
        btnViewRoute.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        btnViewRoute.BackColor = System.Drawing.Color.White;
        btnViewRoute.Cursor = System.Windows.Forms.Cursors.Hand;
        btnViewRoute.FlatAppearance.BorderSize = 1;
        btnViewRoute.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
        btnViewRoute.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(248, 250, 252);
        btnViewRoute.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnViewRoute.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        btnViewRoute.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
        btnViewRoute.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
        btnViewRoute.MinimumSize = new System.Drawing.Size(0, 38);
        btnViewRoute.Name = "btnViewRoute";
        btnViewRoute.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        btnViewRoute.TabIndex = 13;
        btnViewRoute.Text = "🗺️ Xem lộ trình";
        btnViewRoute.UseVisualStyleBackColor = false;
        btnViewRoute.Click += new System.EventHandler(this.BtnViewRoute_Click);
        // 
        // btnSyncCalendar
        // 
        btnSyncCalendar.AutoSize = true;
        btnSyncCalendar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        btnSyncCalendar.BackColor = System.Drawing.Color.White;
        btnSyncCalendar.Cursor = System.Windows.Forms.Cursors.Hand;
        btnSyncCalendar.FlatAppearance.BorderSize = 1;
        btnSyncCalendar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
        btnSyncCalendar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(248, 250, 252);
        btnSyncCalendar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnSyncCalendar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        btnSyncCalendar.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
        btnSyncCalendar.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
        btnSyncCalendar.MinimumSize = new System.Drawing.Size(0, 38);
        btnSyncCalendar.Name = "btnSyncCalendar";
        btnSyncCalendar.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        btnSyncCalendar.TabIndex = 14;
        btnSyncCalendar.Text = "🗓️ Đồng bộ Calendar";
        btnSyncCalendar.UseVisualStyleBackColor = false;
        btnSyncCalendar.Click += new System.EventHandler(this.BtnSyncCalendar_Click);
        // 
        // btnDriverInfo
        // 
        btnDriverInfo.AutoSize = true;
        btnDriverInfo.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        btnDriverInfo.BackColor = System.Drawing.Color.White;
        btnDriverInfo.Cursor = System.Windows.Forms.Cursors.Hand;
        btnDriverInfo.FlatAppearance.BorderSize = 1;
        btnDriverInfo.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
        btnDriverInfo.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(248, 250, 252);
        btnDriverInfo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnDriverInfo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        btnDriverInfo.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
        btnDriverInfo.Margin = new System.Windows.Forms.Padding(0, 0, 8, 8);
        btnDriverInfo.MinimumSize = new System.Drawing.Size(0, 38);
        btnDriverInfo.Name = "btnDriverInfo";
        btnDriverInfo.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        btnDriverInfo.TabIndex = 15;
        btnDriverInfo.Text = "📱 Lấy link tài xế";
        btnDriverInfo.UseVisualStyleBackColor = false;
        btnDriverInfo.Click += new System.EventHandler(this.BtnDriverInfo_Click);
        // 
        // lblStatus
        // 
        lblStatus.AutoSize = true;
        lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblStatus.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblStatus.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
        lblStatus.Name = "lblStatus";
        lblStatus.TabIndex = 2;
        // 
        // grid
        // 
        grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.Dock = System.Windows.Forms.DockStyle.Fill;
        grid.Margin = new System.Windows.Forms.Padding(0);
        grid.Name = "grid";
        grid.TabIndex = 3;
        // 
        // TripsControl
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
        Controls.Add(root);
        Name = "TripsControl";
        Size = new System.Drawing.Size(1200, 700);
        ((System.ComponentModel.ISupportInitialize)grid).EndInit();
        toolbar.ResumeLayout(false);
        toolbar.PerformLayout();
        root.ResumeLayout(false);
        root.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel root;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.FlowLayoutPanel toolbar;
    private System.Windows.Forms.Button btnRefresh;
    private System.Windows.Forms.Button btnStart;
    private System.Windows.Forms.Button btnComplete;
    private System.Windows.Forms.Button btnApprove;
    private System.Windows.Forms.Button btnReject;
    private System.Windows.Forms.Button btnCargoPhotos;
    private System.Windows.Forms.Button btnReassign;
    private System.Windows.Forms.Button btnReturnAtWarehouse;
    private System.Windows.Forms.Button btnFuel;
    private System.Windows.Forms.Button btnFuelEstimate;
    private System.Windows.Forms.Button btnExpenses;
    private System.Windows.Forms.Button btnIncidents;
    private System.Windows.Forms.Button btnQuickEstimate;
    private System.Windows.Forms.Button btnViewRoute;
    private System.Windows.Forms.Button btnSyncCalendar;
    private System.Windows.Forms.Button btnDriverInfo;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.DataGridView grid;
}
