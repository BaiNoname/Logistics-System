namespace TruckingSystem.Desktop.GUI_Shared
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>Bố cục khung chính: sidebar (Dock=Left, rộng 260 ở 96 DPI -> tự nhân theo
        /// mức phóng to màn hình) gồm logo (trên) / danh sách nút điều hướng navPanel (giữa,
        /// cuộn được) / nút Đăng xuất (dưới); topBar (Dock=Top) gồm tên người dùng + chuông
        /// thông báo + đường viền dưới; content (Dock=Fill) chứa màn hình đang mở.
        /// Các nút điều hướng phụ thuộc quyền nên vẫn sinh bằng code (MainForm.cs) vào navPanel.</summary>
        private void InitializeComponent()
        {
            this.sidebar = new System.Windows.Forms.Panel();
            this.navPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.sidebarBottom = new System.Windows.Forms.Panel();
            this.btnLogout = new System.Windows.Forms.Button();
            this.logoPanel = new System.Windows.Forms.Panel();
            this.lblLogo = new System.Windows.Forms.Label();
            this.content = new System.Windows.Forms.Panel();
            this.topBar = new System.Windows.Forms.Panel();
            this.lblUser = new System.Windows.Forms.Label();
            this.btnNotifications = new System.Windows.Forms.Button();
            this.topBarBorder = new System.Windows.Forms.Panel();
            this.sidebar.SuspendLayout();
            this.sidebarBottom.SuspendLayout();
            this.logoPanel.SuspendLayout();
            this.topBar.SuspendLayout();
            this.SuspendLayout();
            //
            // sidebar
            //
            this.sidebar.BackColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.sidebar.Controls.Add(this.navPanel);
            this.sidebar.Controls.Add(this.sidebarBottom);
            this.sidebar.Controls.Add(this.logoPanel);
            this.sidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.sidebar.Name = "sidebar";
            this.sidebar.Size = new System.Drawing.Size(260, 800);
            this.sidebar.TabIndex = 2;
            //
            // navPanel
            //
            this.navPanel.AutoScroll = true;
            this.navPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.navPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.navPanel.Name = "navPanel";
            this.navPanel.Padding = new System.Windows.Forms.Padding(10, 10, 10, 0);
            this.navPanel.TabIndex = 1;
            this.navPanel.WrapContents = false;
            this.navPanel.ClientSizeChanged += new System.EventHandler(this.NavPanel_ClientSizeChanged);
            //
            // sidebarBottom
            //
            this.sidebarBottom.Controls.Add(this.btnLogout);
            this.sidebarBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.sidebarBottom.Name = "sidebarBottom";
            this.sidebarBottom.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.sidebarBottom.Size = new System.Drawing.Size(260, 60);
            this.sidebarBottom.TabIndex = 2;
            //
            // btnLogout
            //
            this.btnLogout.BackColor = System.Drawing.Color.FromArgb(30, 41, 59);
            this.btnLogout.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLogout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLogout.FlatAppearance.BorderSize = 0;
            this.btnLogout.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(51, 65, 85);
            this.btnLogout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogout.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.btnLogout.ForeColor = System.Drawing.Color.FromArgb(203, 213, 225);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.TabIndex = 0;
            this.btnLogout.Text = "  🚪   Đăng xuất";
            this.btnLogout.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnLogout.UseVisualStyleBackColor = false;
            this.btnLogout.Click += new System.EventHandler(this.BtnLogout_Click);
            //
            // logoPanel
            //
            this.logoPanel.Controls.Add(this.lblLogo);
            this.logoPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.logoPanel.Name = "logoPanel";
            this.logoPanel.Size = new System.Drawing.Size(260, 70);
            this.logoPanel.TabIndex = 0;
            //
            // lblLogo
            //
            this.lblLogo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLogo.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblLogo.ForeColor = System.Drawing.Color.White;
            this.lblLogo.Name = "lblLogo";
            this.lblLogo.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.lblLogo.TabIndex = 0;
            this.lblLogo.Text = "🚚 TruckingSystem";
            this.lblLogo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // content
            //
            this.content.Dock = System.Windows.Forms.DockStyle.Fill;
            this.content.Name = "content";
            this.content.Padding = new System.Windows.Forms.Padding(24);
            this.content.TabIndex = 0;
            //
            // topBar
            //
            this.topBar.BackColor = System.Drawing.Color.White;
            this.topBar.Controls.Add(this.lblUser);
            this.topBar.Controls.Add(this.btnNotifications);
            this.topBar.Controls.Add(this.topBarBorder);
            this.topBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.topBar.Name = "topBar";
            this.topBar.Padding = new System.Windows.Forms.Padding(24, 0, 24, 0);
            this.topBar.Size = new System.Drawing.Size(1020, 60);
            this.topBar.TabIndex = 1;
            //
            // lblUser
            //
            this.lblUser.AutoSize = true;
            this.lblUser.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblUser.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblUser.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblUser.Name = "lblUser";
            this.lblUser.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.lblUser.TabIndex = 1;
            this.lblUser.Text = "👤";
            this.lblUser.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // btnNotifications
            //
            this.btnNotifications.AutoSize = true;
            this.btnNotifications.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnNotifications.BackColor = System.Drawing.Color.White;
            this.btnNotifications.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNotifications.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnNotifications.FlatAppearance.BorderSize = 0;
            this.btnNotifications.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNotifications.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.btnNotifications.MinimumSize = new System.Drawing.Size(48, 0);
            this.btnNotifications.Name = "btnNotifications";
            this.btnNotifications.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.btnNotifications.TabIndex = 0;
            this.btnNotifications.Text = "🔔";
            this.btnNotifications.UseVisualStyleBackColor = false;
            this.btnNotifications.Click += new System.EventHandler(this.BtnNotifications_Click);
            //
            // topBarBorder
            //
            this.topBarBorder.BackColor = System.Drawing.Color.FromArgb(226, 232, 240);
            this.topBarBorder.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.topBarBorder.Name = "topBarBorder";
            this.topBarBorder.Size = new System.Drawing.Size(1020, 1);
            this.topBarBorder.TabIndex = 2;
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.ClientSize = new System.Drawing.Size(1280, 800);
            this.Controls.Add(this.content);
            this.Controls.Add(this.topBar);
            this.Controls.Add(this.sidebar);
            this.MinimumSize = new System.Drawing.Size(900, 500);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Trucking System";
            this.sidebar.ResumeLayout(false);
            this.sidebarBottom.ResumeLayout(false);
            this.logoPanel.ResumeLayout(false);
            this.topBar.ResumeLayout(false);
            this.topBar.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel sidebar;
        private System.Windows.Forms.Panel content;
        private System.Windows.Forms.Panel topBar;
        private System.Windows.Forms.Panel logoPanel;
        private System.Windows.Forms.Label lblLogo;
        private System.Windows.Forms.FlowLayoutPanel navPanel;
        private System.Windows.Forms.Panel sidebarBottom;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.Button btnNotifications;
        private System.Windows.Forms.Panel topBarBorder;
    }
}
