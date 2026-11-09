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
        /// cuộn được) / nút Đăng xuất (dưới); topBar (Dock=Top) gồm chuông thông báo + avatar +
        /// tên người dùng / vai trò (căn giữa dọc, thẳng hàng) + đường viền dưới; content (Dock=Fill) chứa màn hình đang mở.
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
            this.topBarLayout = new System.Windows.Forms.TableLayoutPanel();
            this.lblAvatar = new System.Windows.Forms.Label();
            this.userInfo = new System.Windows.Forms.TableLayoutPanel();
            this.lblUser = new System.Windows.Forms.Label();
            this.lblRole = new System.Windows.Forms.Label();
            this.btnNotifications = new System.Windows.Forms.Button();
            this.topBarBorder = new System.Windows.Forms.Panel();
            this.sidebar.SuspendLayout();
            this.sidebarBottom.SuspendLayout();
            this.logoPanel.SuspendLayout();
            this.topBar.SuspendLayout();
            this.topBarLayout.SuspendLayout();
            this.userInfo.SuspendLayout();
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
            // topBar — 1 TableLayoutPanel: [khoảng trống co giãn] [chuông] [avatar] [tên + vai trò].
            // Mọi phần tử Anchor=None -> luôn CĂN GIỮA theo chiều dọc, thẳng hàng với nhau ở mọi
            // mức phóng to màn hình (trước đây Label Dock=Right + AutoSize bị lệch so với nút chuông).
            //
            this.topBar.BackColor = System.Drawing.Color.White;
            this.topBar.Controls.Add(this.topBarLayout);
            this.topBar.Controls.Add(this.topBarBorder);
            this.topBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.topBar.Name = "topBar";
            this.topBar.Padding = new System.Windows.Forms.Padding(24, 0, 24, 0);
            this.topBar.Size = new System.Drawing.Size(1020, 64);
            this.topBar.TabIndex = 1;
            //
            // topBarLayout
            //
            this.topBarLayout.ColumnCount = 4;
            this.topBarLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.topBarLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.topBarLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.topBarLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.topBarLayout.Controls.Add(this.btnNotifications, 1, 0);
            this.topBarLayout.Controls.Add(this.lblAvatar, 2, 0);
            this.topBarLayout.Controls.Add(this.userInfo, 3, 0);
            this.topBarLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.topBarLayout.Margin = new System.Windows.Forms.Padding(0);
            this.topBarLayout.Name = "topBarLayout";
            this.topBarLayout.RowCount = 1;
            this.topBarLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.topBarLayout.TabIndex = 0;
            //
            // btnNotifications
            //
            this.btnNotifications.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnNotifications.BackColor = System.Drawing.Color.White;
            this.btnNotifications.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNotifications.FlatAppearance.BorderSize = 0;
            this.btnNotifications.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.btnNotifications.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNotifications.Font = new System.Drawing.Font("Segoe UI Emoji", 14F);
            this.btnNotifications.Margin = new System.Windows.Forms.Padding(0, 0, 16, 0);
            this.btnNotifications.Name = "btnNotifications";
            this.btnNotifications.Padding = new System.Windows.Forms.Padding(0);
            this.btnNotifications.Size = new System.Drawing.Size(44, 44);
            this.btnNotifications.TabIndex = 0;
            this.btnNotifications.Text = "🔔";
            this.btnNotifications.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnNotifications.UseVisualStyleBackColor = false;
            this.btnNotifications.Click += new System.EventHandler(this.BtnNotifications_Click);
            //
            // lblAvatar
            //
            this.lblAvatar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblAvatar.BackColor = System.Drawing.Color.FromArgb(219, 234, 254);
            this.lblAvatar.Font = new System.Drawing.Font("Segoe UI Emoji", 14F);
            this.lblAvatar.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.lblAvatar.Name = "lblAvatar";
            this.lblAvatar.Size = new System.Drawing.Size(40, 40);
            this.lblAvatar.TabIndex = 1;
            this.lblAvatar.Text = "👤";
            this.lblAvatar.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // userInfo — tên đăng nhập (đậm) + vai trò (chữ nhỏ) xếp 2 dòng
            //
            this.userInfo.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.userInfo.AutoSize = true;
            this.userInfo.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.userInfo.ColumnCount = 1;
            this.userInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.userInfo.Controls.Add(this.lblUser, 0, 0);
            this.userInfo.Controls.Add(this.lblRole, 0, 1);
            this.userInfo.Margin = new System.Windows.Forms.Padding(0);
            this.userInfo.Name = "userInfo";
            this.userInfo.RowCount = 2;
            this.userInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.userInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.userInfo.TabIndex = 2;
            //
            // lblUser
            //
            this.lblUser.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblUser.AutoSize = true;
            this.lblUser.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblUser.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            this.lblUser.Margin = new System.Windows.Forms.Padding(0);
            this.lblUser.Name = "lblUser";
            this.lblUser.TabIndex = 0;
            this.lblUser.Text = "Người dùng";
            //
            // lblRole
            //
            this.lblRole.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblRole.AutoSize = true;
            this.lblRole.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblRole.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblRole.Margin = new System.Windows.Forms.Padding(0);
            this.lblRole.Name = "lblRole";
            this.lblRole.TabIndex = 1;
            this.lblRole.Text = "Vai trò";
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
            this.userInfo.ResumeLayout(false);
            this.userInfo.PerformLayout();
            this.topBarLayout.ResumeLayout(false);
            this.topBarLayout.PerformLayout();
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
        private System.Windows.Forms.TableLayoutPanel topBarLayout;
        private System.Windows.Forms.Label lblAvatar;
        private System.Windows.Forms.TableLayoutPanel userInfo;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.Label lblRole;
        private System.Windows.Forms.Button btnNotifications;
        private System.Windows.Forms.Panel topBarBorder;
    }
}
