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
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(400, 73);
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
        btnRefresh.Location = new Point(460, 0);
        btnRefresh.Name = "btnRefresh";
        btnRefresh.Size = new Size(100, 38);
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
        btnFuel.Location = new Point(570, 0);
        btnFuel.Name = "btnFuel";
        btnFuel.Size = new Size(130, 38);
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
        btnStart.Location = new Point(710, 0);
        btnStart.Name = "btnStart";
        btnStart.Size = new Size(150, 38);
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
        btnComplete.Location = new Point(870, 0);
        btnComplete.Name = "btnComplete";
        btnComplete.Size = new Size(150, 38);
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
        btnViewRoute.Location = new Point(1030, 0);
        btnViewRoute.Name = "btnViewRoute";
        btnViewRoute.Size = new Size(180, 38);
        btnViewRoute.TabIndex = 4;
        btnViewRoute.Text = "🗺️ Xem lộ trình";
        btnViewRoute.UseVisualStyleBackColor = false;
        btnViewRoute.Click += BtnViewRoute_Click;
        // 
        // lblStatus
        // 
        lblStatus.AutoSize = true;
        lblStatus.Font = new Font("Segoe UI", 9F);
        lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
        lblStatus.Location = new Point(0, 44);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(0, 25);
        lblStatus.TabIndex = 5;
        // 
        // grid
        // 
        grid.ColumnHeadersHeight = 34;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.Location = new Point(0, 76);
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
        Controls.Add(lblStatus);
        Controls.Add(grid);
        Name = "TripsControl";
        Size = new Size(1315, 568);
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
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.DataGridView grid;
}