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

    private void InitializeComponent()
    {
        lblTitle = new Label();
        btnRefresh = new Button();
        btnFuel = new Button();
        btnStart = new Button();
        btnComplete = new Button();
        btnViewRoute = new Button();
        btnDriverInfo = new Button();
        btnFuelEstimate = new Button();
        btnIncidents = new Button();
        btnSyncCalendar = new Button();
        btnExpenses = new Button();
        btnQuickEstimate = new Button();
        btnApprove = new Button();
        btnReject = new Button();
        lblStatus = new Label();
        grid = new DataGridView();
        ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
        SuspendLayout();
        // 
        // lblTitle
        // 
        lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
        lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
        lblTitle.Location = new Point(-1, 19);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(382, 50);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "Quản lý chuyến (Trip)";
        // 
        // btnRefresh
        // 
        btnRefresh.BackColor = Color.White;
        btnRefresh.Cursor = Cursors.Hand;
        btnRefresh.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
        btnRefresh.FlatStyle = FlatStyle.Flat;
        btnRefresh.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnRefresh.ForeColor = Color.FromArgb(15, 23, 42);
        btnRefresh.Location = new Point(387, 3);
        btnRefresh.Name = "btnRefresh";
        btnRefresh.Size = new Size(123, 38);
        btnRefresh.TabIndex = 1;
        btnRefresh.Text = "🔄 Làm mới";
        btnRefresh.UseVisualStyleBackColor = false;
        btnRefresh.Click += BtnRefresh_Click;
        // 
        // btnFuel
        // 
        btnFuel.BackColor = Color.White;
        btnFuel.Cursor = Cursors.Hand;
        btnFuel.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
        btnFuel.FlatStyle = FlatStyle.Flat;
        btnFuel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnFuel.ForeColor = Color.FromArgb(15, 23, 42);
        btnFuel.Location = new Point(497, 3);
        btnFuel.Name = "btnFuel";
        btnFuel.Size = new Size(153, 38);
        btnFuel.TabIndex = 2;
        btnFuel.Text = "⛽ Ghi xăng";
        btnFuel.UseVisualStyleBackColor = false;
        btnFuel.Click += BtnFuel_Click;
        // 
        // btnStart
        // 
        btnStart.BackColor = Color.White;
        btnStart.Cursor = Cursors.Hand;
        btnStart.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
        btnStart.FlatStyle = FlatStyle.Flat;
        btnStart.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnStart.ForeColor = Color.FromArgb(15, 23, 42);
        btnStart.Location = new Point(637, 3);
        btnStart.Name = "btnStart";
        btnStart.Size = new Size(173, 38);
        btnStart.TabIndex = 3;
        btnStart.Text = "▶️ Bắt đầu chuyến";
        btnStart.UseVisualStyleBackColor = false;
        btnStart.Click += BtnStart_Click;
        // 
        // btnComplete
        // 
        btnComplete.BackColor = Color.FromArgb(37, 99, 235);
        btnComplete.Cursor = Cursors.Hand;
        btnComplete.FlatAppearance.BorderSize = 0;
        btnComplete.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
        btnComplete.FlatStyle = FlatStyle.Flat;
        btnComplete.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnComplete.ForeColor = Color.White;
        btnComplete.Location = new Point(797, 3);
        btnComplete.Name = "btnComplete";
        btnComplete.Size = new Size(173, 38);
        btnComplete.TabIndex = 4;
        btnComplete.Text = "✅ Hoàn tất chuyến";
        btnComplete.UseVisualStyleBackColor = false;
        btnComplete.Click += BtnComplete_Click;
        // 
        // btnViewRoute
        // 
        btnViewRoute.BackColor = Color.White;
        btnViewRoute.Cursor = Cursors.Hand;
        btnViewRoute.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
        btnViewRoute.FlatStyle = FlatStyle.Flat;
        btnViewRoute.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnViewRoute.ForeColor = Color.FromArgb(15, 23, 42);
        btnViewRoute.Location = new Point(957, 3);
        btnViewRoute.Name = "btnViewRoute";
        btnViewRoute.Size = new Size(203, 38);
        btnViewRoute.TabIndex = 4;
        btnViewRoute.Text = "🗺️ Xem lộ trình";
        btnViewRoute.UseVisualStyleBackColor = false;
        btnViewRoute.Click += BtnViewRoute_Click;
        // 
        // btnDriverInfo
        // 
        btnDriverInfo.BackColor = Color.White;
        btnDriverInfo.Cursor = Cursors.Hand;
        btnDriverInfo.FlatStyle = FlatStyle.Flat;
        btnDriverInfo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnDriverInfo.ForeColor = Color.FromArgb(15, 23, 42);
        btnDriverInfo.Location = new Point(1147, 3);
        btnDriverInfo.Name = "btnDriverInfo";
        btnDriverInfo.Size = new Size(203, 38);
        btnDriverInfo.TabIndex = 5;
        btnDriverInfo.Text = "📱 Lấy link tài xế";
        btnDriverInfo.UseVisualStyleBackColor = false;
        btnDriverInfo.Click += BtnDriverInfo_Click;
        // 
        // btnFuelEstimate
        // 
        btnFuelEstimate.BackColor = Color.White;
        btnFuelEstimate.Cursor = Cursors.Hand;
        btnFuelEstimate.FlatStyle = FlatStyle.Flat;
        btnFuelEstimate.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnFuelEstimate.ForeColor = Color.FromArgb(15, 23, 42);
        btnFuelEstimate.Location = new Point(593, 47);
        btnFuelEstimate.Name = "btnFuelEstimate";
        btnFuelEstimate.Size = new Size(242, 38);
        btnFuelEstimate.TabIndex = 6;
        btnFuelEstimate.Text = "⛽ Ước tính nhiên liệu";
        btnFuelEstimate.UseVisualStyleBackColor = false;
        btnFuelEstimate.Click += BtnFuelEstimate_Click;
        // 
        // btnIncidents
        // 
        btnIncidents.BackColor = Color.White;
        btnIncidents.Cursor = Cursors.Hand;
        btnIncidents.FlatAppearance.BorderColor = Color.FromArgb(220, 38, 38);
        btnIncidents.FlatStyle = FlatStyle.Flat;
        btnIncidents.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnIncidents.ForeColor = Color.FromArgb(220, 38, 38);
        btnIncidents.Location = new Point(1183, 47);
        btnIncidents.Name = "btnIncidents";
        btnIncidents.Size = new Size(180, 38);
        btnIncidents.TabIndex = 7;
        btnIncidents.Text = "⚠️ Sự cố";
        btnIncidents.UseVisualStyleBackColor = false;
        btnIncidents.Click += BtnIncidents_Click;
        // 
        // btnSyncCalendar
        // 
        btnSyncCalendar.BackColor = Color.White;
        btnSyncCalendar.Cursor = Cursors.Hand;
        btnSyncCalendar.FlatStyle = FlatStyle.Flat;
        btnSyncCalendar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnSyncCalendar.ForeColor = Color.FromArgb(15, 23, 42);
        btnSyncCalendar.Location = new Point(387, 47);
        btnSyncCalendar.Name = "btnSyncCalendar";
        btnSyncCalendar.Size = new Size(200, 38);
        btnSyncCalendar.TabIndex = 8;
        btnSyncCalendar.Text = "🗓️ Đồng bộ Calendar";
        btnSyncCalendar.UseVisualStyleBackColor = false;
        btnSyncCalendar.Click += BtnSyncCalendar_Click;
        // 
        // btnExpenses
        // 
        btnExpenses.BackColor = Color.White;
        btnExpenses.Cursor = Cursors.Hand;
        btnExpenses.FlatStyle = FlatStyle.Flat;
        btnExpenses.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnExpenses.ForeColor = Color.FromArgb(15, 23, 42);
        btnExpenses.Location = new Point(841, 47);
        btnExpenses.Name = "btnExpenses";
        btnExpenses.Size = new Size(180, 38);
        btnExpenses.TabIndex = 9;
        btnExpenses.Text = "💰 Chi phí khác";
        btnExpenses.UseVisualStyleBackColor = false;
        btnExpenses.Click += BtnExpenses_Click;
        // 
        // btnQuickEstimate
        // 
        btnQuickEstimate.BackColor = Color.White;
        btnQuickEstimate.Cursor = Cursors.Hand;
        btnQuickEstimate.FlatStyle = FlatStyle.Flat;
        btnQuickEstimate.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnQuickEstimate.ForeColor = Color.FromArgb(15, 23, 42);
        btnQuickEstimate.Location = new Point(1027, 47);
        btnQuickEstimate.Name = "btnQuickEstimate";
        btnQuickEstimate.Size = new Size(150, 38);
        btnQuickEstimate.TabIndex = 10;
        btnQuickEstimate.Text = "📏 Ước tính";
        btnQuickEstimate.UseVisualStyleBackColor = false;
        btnQuickEstimate.Click += BtnQuickEstimate_Click;
        // 
        // btnApprove
        // 
        btnApprove.BackColor = Color.FromArgb(22, 163, 74);
        btnApprove.Cursor = Cursors.Hand;
        btnApprove.FlatAppearance.BorderSize = 0;
        btnApprove.FlatAppearance.MouseOverBackColor = Color.FromArgb(21, 128, 61);
        btnApprove.FlatStyle = FlatStyle.Flat;
        btnApprove.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnApprove.ForeColor = Color.White;
        btnApprove.Location = new Point(0, 91);
        btnApprove.Name = "btnApprove";
        btnApprove.Size = new Size(220, 38);
        btnApprove.TabIndex = 11;
        btnApprove.Text = "✅ Duyệt xác nhận tài xế";
        btnApprove.UseVisualStyleBackColor = false;
        btnApprove.Click += BtnApprove_Click;
        // 
        // btnReject
        // 
        btnReject.BackColor = Color.White;
        btnReject.Cursor = Cursors.Hand;
        btnReject.FlatAppearance.BorderColor = Color.FromArgb(220, 38, 38);
        btnReject.FlatStyle = FlatStyle.Flat;
        btnReject.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnReject.ForeColor = Color.FromArgb(220, 38, 38);
        btnReject.Location = new Point(230, 91);
        btnReject.Name = "btnReject";
        btnReject.Size = new Size(160, 38);
        btnReject.TabIndex = 12;
        btnReject.Text = "❌ Từ chối";
        btnReject.UseVisualStyleBackColor = false;
        btnReject.Click += BtnReject_Click;
        // 
        // lblStatus
        // 
        lblStatus.AutoSize = true;
        lblStatus.Font = new Font("Segoe UI", 9F);
        lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
        lblStatus.Location = new Point(400, 100);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(0, 25);
        lblStatus.TabIndex = 5;
        // 
        // grid
        // 
        grid.ColumnHeadersHeight = 34;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.Location = new Point(0, 134);
        grid.Name = "grid";
        grid.RowHeadersWidth = 62;
        grid.Size = new Size(1000, 560);
        grid.TabIndex = 6;
        // 
        // TripsControl
        // 
        AutoScaleDimensions = new SizeF(144F, 144F);
        BackColor = Color.FromArgb(241, 245, 249);
        Controls.Add(lblTitle);
        Controls.Add(btnRefresh);
        Controls.Add(btnFuel);
        Controls.Add(btnStart);
        Controls.Add(btnComplete);
        Controls.Add(btnViewRoute);
        Controls.Add(btnDriverInfo);
        Controls.Add(btnFuelEstimate);
        Controls.Add(btnIncidents);
        Controls.Add(btnSyncCalendar);
        Controls.Add(btnExpenses);
        Controls.Add(btnQuickEstimate);
        Controls.Add(btnApprove);
        Controls.Add(btnReject);
        Controls.Add(lblStatus);
        Controls.Add(grid);
        Name = "TripsControl";
        Size = new Size(1810, 654);
        ((System.ComponentModel.ISupportInitialize)grid).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Button btnRefresh;
    private System.Windows.Forms.Button btnFuel;
    private System.Windows.Forms.Button btnStart;
    private System.Windows.Forms.Button btnComplete;
    private System.Windows.Forms.Button btnViewRoute;
    private System.Windows.Forms.Button btnDriverInfo;
    private System.Windows.Forms.Button btnFuelEstimate;
    private System.Windows.Forms.Button btnIncidents;
    private System.Windows.Forms.Button btnSyncCalendar;
    private System.Windows.Forms.Button btnExpenses;
    private System.Windows.Forms.Button btnQuickEstimate;
    private System.Windows.Forms.Button btnApprove;
    private System.Windows.Forms.Button btnReject;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.DataGridView grid;
}