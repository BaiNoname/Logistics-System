namespace TruckingSystem.Desktop.GUI_Shared
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            bannerPanel = new Panel();
            lblAppName = new Label();
            lblTagline = new Label();
            roadLine = new Panel();
            rightPanel = new Panel();
            lblTitle = new Label();
            lblSub = new Label();
            lblServerUrl = new Label();
            txtServerUrl = new TextBox();
            lblUsername = new Label();
            txtUsername = new TextBox();
            lblPassword = new Label();
            txtPassword = new TextBox();
            btnLogin = new Button();
            lblStatus = new Label();
            btnClose = new Button();
            bannerPanel.SuspendLayout();
            rightPanel.SuspendLayout();
            SuspendLayout();
            // 
            // bannerPanel
            // 
            bannerPanel.BackColor = Color.FromArgb(30, 41, 59);
            bannerPanel.Controls.Add(lblAppName);
            bannerPanel.Controls.Add(lblTagline);
            bannerPanel.Controls.Add(roadLine);
            bannerPanel.Dock = DockStyle.Left;
            bannerPanel.Location = new Point(0, 0);
            bannerPanel.Margin = new Padding(4, 4, 4, 4);
            bannerPanel.Name = "bannerPanel";
            bannerPanel.Size = new Size(690, 960);
            bannerPanel.TabIndex = 1;
            // 
            // lblAppName
            // 
            lblAppName.AutoSize = true;
            lblAppName.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            lblAppName.ForeColor = Color.White;
            lblAppName.Location = new Point(108, 188);
            lblAppName.Margin = new Padding(4, 0, 4, 0);
            lblAppName.Name = "lblAppName";
            lblAppName.Size = new Size(429, 70);
            lblAppName.TabIndex = 0;
            lblAppName.Text = "Trucking System";
            // 
            // lblTagline
            // 
            lblTagline.AutoSize = true;
            lblTagline.Font = new Font("Segoe UI", 11F);
            lblTagline.ForeColor = Color.FromArgb(203, 213, 225);
            lblTagline.Location = new Point(146, 295);
            lblTagline.Margin = new Padding(4, 0, 4, 0);
            lblTagline.Name = "lblTagline";
            lblTagline.Size = new Size(349, 60);
            lblTagline.TabIndex = 1;
            lblTagline.Text = "Hệ thống quản lý vận tải và\nkho trung chuyển hàng hóa nội địa";
            // 
            // roadLine
            // 
            roadLine.BackColor = Color.FromArgb(51, 65, 85);
            roadLine.Location = new Point(0, 759);
            roadLine.Margin = new Padding(4, 4, 4, 4);
            roadLine.Name = "roadLine";
            roadLine.Size = new Size(690, 3);
            roadLine.TabIndex = 2;
            // 
            // rightPanel
            // 
            rightPanel.BackColor = Color.White;
            rightPanel.Controls.Add(lblTitle);
            rightPanel.Controls.Add(lblSub);
            rightPanel.Controls.Add(lblServerUrl);
            rightPanel.Controls.Add(txtServerUrl);
            rightPanel.Controls.Add(lblUsername);
            rightPanel.Controls.Add(txtUsername);
            rightPanel.Controls.Add(lblPassword);
            rightPanel.Controls.Add(txtPassword);
            rightPanel.Controls.Add(btnLogin);
            rightPanel.Controls.Add(lblStatus);
            rightPanel.Controls.Add(btnClose);
            rightPanel.Dock = DockStyle.Fill;
            rightPanel.Location = new Point(690, 0);
            rightPanel.Margin = new Padding(4, 4, 4, 4);
            rightPanel.Name = "rightPanel";
            rightPanel.Size = new Size(810, 960);
            rightPanel.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(135, 210);
            lblTitle.Margin = new Padding(4, 0, 4, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(205, 48);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Đăng nhập";
            // 
            // lblSub
            // 
            lblSub.AutoSize = true;
            lblSub.Font = new Font("Segoe UI", 10F);
            lblSub.ForeColor = Color.FromArgb(100, 116, 139);
            lblSub.Location = new Point(135, 270);
            lblSub.Margin = new Padding(4, 0, 4, 0);
            lblSub.Name = "lblSub";
            lblSub.Size = new Size(330, 28);
            lblSub.TabIndex = 1;
            lblSub.Text = "Nhập thông tin tài khoản để tiếp tục";
            // 
            // lblServerUrl
            // 
            lblServerUrl.AutoSize = true;
            lblServerUrl.Font = new Font("Segoe UI", 9F);
            lblServerUrl.ForeColor = Color.FromArgb(100, 116, 139);
            lblServerUrl.Location = new Point(135, 330);
            lblServerUrl.Margin = new Padding(4, 0, 4, 0);
            lblServerUrl.Name = "lblServerUrl";
            lblServerUrl.Size = new Size(109, 25);
            lblServerUrl.TabIndex = 2;
            lblServerUrl.Text = "SERVER URL";
            // 
            // txtServerUrl
            // 
            txtServerUrl.BorderStyle = BorderStyle.FixedSingle;
            txtServerUrl.Font = new Font("Segoe UI", 10F);
            txtServerUrl.Location = new Point(135, 360);
            txtServerUrl.Margin = new Padding(4, 4, 4, 4);
            txtServerUrl.Name = "txtServerUrl";
            txtServerUrl.Size = new Size(479, 34);
            txtServerUrl.TabIndex = 3;
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Font = new Font("Segoe UI", 9F);
            lblUsername.ForeColor = Color.FromArgb(100, 116, 139);
            lblUsername.Location = new Point(135, 435);
            lblUsername.Margin = new Padding(4, 0, 4, 0);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(104, 25);
            lblUsername.TabIndex = 4;
            lblUsername.Text = "TÀI KHOẢN";
            // 
            // txtUsername
            // 
            txtUsername.BorderStyle = BorderStyle.FixedSingle;
            txtUsername.Font = new Font("Segoe UI", 10F);
            txtUsername.Location = new Point(135, 465);
            txtUsername.Margin = new Padding(4, 4, 4, 4);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(479, 34);
            txtUsername.TabIndex = 5;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Font = new Font("Segoe UI", 9F);
            lblPassword.ForeColor = Color.FromArgb(100, 116, 139);
            lblPassword.Location = new Point(135, 540);
            lblPassword.Margin = new Padding(4, 0, 4, 0);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(101, 25);
            lblPassword.TabIndex = 6;
            lblPassword.Text = "MẬT KHẨU";
            // 
            // txtPassword
            // 
            txtPassword.BorderStyle = BorderStyle.FixedSingle;
            txtPassword.Font = new Font("Segoe UI", 10F);
            txtPassword.Location = new Point(135, 570);
            txtPassword.Margin = new Padding(4, 4, 4, 4);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '●';
            txtPassword.Size = new Size(479, 34);
            txtPassword.TabIndex = 7;
            // 
            // btnLogin
            // 
            btnLogin.BackColor = Color.FromArgb(37, 99, 235);
            btnLogin.Cursor = Cursors.Hand;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnLogin.ForeColor = Color.White;
            btnLogin.Location = new Point(135, 660);
            btnLogin.Margin = new Padding(4, 4, 4, 4);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(480, 66);
            btnLogin.TabIndex = 8;
            btnLogin.Text = "Đăng nhập";
            btnLogin.UseVisualStyleBackColor = false;
            btnLogin.Click += BtnLogin_Click;
            // 
            // lblStatus
            // 
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(220, 38, 38);
            lblStatus.Location = new Point(135, 742);
            lblStatus.Margin = new Padding(4, 0, 4, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(480, 60);
            lblStatus.TabIndex = 9;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.BackColor = Color.White;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.ForeColor = Color.FromArgb(100, 116, 139);
            btnClose.Location = new Point(1941, 15);
            btnClose.Margin = new Padding(4, 4, 4, 4);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(54, 54);
            btnClose.TabIndex = 10;
            btnClose.Text = "✕";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += BtnClose_Click;
            // 
            // LoginForm
            // 
            AcceptButton = btnLogin;
            AutoScaleDimensions = new SizeF(144F, 144F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(37, 99, 235);
            ClientSize = new Size(1500, 960);
            Controls.Add(rightPanel);
            Controls.Add(bannerPanel);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(6, 6, 6, 6);
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Trucking System — Đăng nhập";
            bannerPanel.ResumeLayout(false);
            bannerPanel.PerformLayout();
            rightPanel.ResumeLayout(false);
            rightPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel bannerPanel;
        private System.Windows.Forms.Label lblAppName;
        private System.Windows.Forms.Label lblTagline;
        private System.Windows.Forms.Panel roadLine;
        private System.Windows.Forms.Panel rightPanel;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSub;
        private System.Windows.Forms.Label lblServerUrl;
        private System.Windows.Forms.TextBox txtServerUrl;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnClose;
    }
}
