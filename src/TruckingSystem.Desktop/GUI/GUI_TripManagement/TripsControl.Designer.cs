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
        lblStatus = new Label();
        grid = new DataGridView();
        ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
        SuspendLayout();
        // 
        // lblTitle
        // 
        lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
        lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
        lblTitle.Location = new Point(0, 0);
        lblTitle.Margin = new Padding(2, 0, 2, 0);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(255, 49);
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
        btnRefresh.Location = new Point(258, 3);
        btnRefresh.Margin = new Padding(2, 2, 2, 2);
        btnRefresh.Name = "btnRefresh";
        btnRefresh.Size = new Size(120, 35);
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
        btnFuel.Location = new Point(382, 3);
        btnFuel.Margin = new Padding(2, 2, 2, 2);
        btnFuel.Name = "btnFuel";
        btnFuel.Size = new Size(120, 35);
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
        btnStart.Location = new Point(506, 3);
        btnStart.Margin = new Padding(2, 2, 2, 2);
        btnStart.Name = "btnStart";
        btnStart.Size = new Size(120, 35);
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
        btnComplete.Location = new Point(630, 3);
        btnComplete.Margin = new Padding(2, 2, 2, 2);
        btnComplete.Name = "btnComplete";
        btnComplete.Size = new Size(120, 35);
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
        btnViewRoute.Location = new Point(754, 3);
        btnViewRoute.Margin = new Padding(2, 2, 2, 2);
        btnViewRoute.Name = "btnViewRoute";
        btnViewRoute.Size = new Size(120, 35);
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
        btnDriverInfo.Location = new Point(878, 2);
        btnDriverInfo.Margin = new Padding(2, 2, 2, 2);
        btnDriverInfo.Name = "btnDriverInfo";
        btnDriverInfo.Size = new Size(120, 35);
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
        btnFuelEstimate.Location = new Point(382, 42);
        btnFuelEstimate.Margin = new Padding(2, 2, 2, 2);
        btnFuelEstimate.Name = "btnFuelEstimate";
        btnFuelEstimate.Size = new Size(120, 35);
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
        btnIncidents.Location = new Point(754, 42);
        btnIncidents.Margin = new Padding(2, 2, 2, 2);
        btnIncidents.Name = "btnIncidents";
        btnIncidents.Size = new Size(120, 35);
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
        btnSyncCalendar.Location = new Point(258, 42);
        btnSyncCalendar.Margin = new Padding(2, 2, 2, 2);
        btnSyncCalendar.Name = "btnSyncCalendar";
        btnSyncCalendar.Size = new Size(120, 35);
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
        btnExpenses.Location = new Point(506, 42);
        btnExpenses.Margin = new Padding(2, 2, 2, 2);
        btnExpenses.Name = "btnExpenses";
        btnExpenses.Size = new Size(120, 35);
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
        btnQuickEstimate.Location = new Point(630, 42);
        btnQuickEstimate.Margin = new Padding(2, 2, 2, 2);
        btnQuickEstimate.Name = "btnQuickEstimate";
        btnQuickEstimate.Size = new Size(120, 35);
        btnQuickEstimate.TabIndex = 10;
        btnQuickEstimate.Text = "📏 Ước tính";
        btnQuickEstimate.UseVisualStyleBackColor = false;
        btnQuickEstimate.Click += BtnQuickEstimate_Click;
        // 
        // lblStatus
        // 
        lblStatus.AutoSize = true;
        lblStatus.Font = new Font("Segoe UI", 9F);
        lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
        lblStatus.Location = new Point(0, 29);
        lblStatus.Margin = new Padding(2, 0, 2, 0);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(0, 15);
        lblStatus.TabIndex = 5;
        // 
        // grid
        // 
        grid.ColumnHeadersHeight = 34;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.Location = new Point(0, 81);
        grid.Margin = new Padding(2, 2, 2, 2);
        grid.Name = "grid";
        grid.RowHeadersWidth = 62;
        grid.Size = new Size(667, 373);
        grid.TabIndex = 6;
        // 
        // TripsControl
        // 
        AutoScaleDimensions = new SizeF(96F, 96F);
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
        Controls.Add(lblStatus);
        Controls.Add(grid);
        Margin = new Padding(2, 2, 2, 2);
        Name = "TripsControl";
        Size = new Size(1207, 569);
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
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.DataGridView grid;
}