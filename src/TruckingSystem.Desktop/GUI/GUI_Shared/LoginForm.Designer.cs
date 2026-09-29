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
            btnOpenChatBot = new Button();
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
            bannerPanel.Name = "bannerPanel";
            bannerPanel.Size = new Size(460, 640);
            bannerPanel.TabIndex = 1;
            // 
            // lblAppName
            // 
            lblAppName.AutoSize = true;
            lblAppName.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            lblAppName.ForeColor = Color.White;
            lblAppName.Location = new Point(72, 125);
            lblAppName.Name = "lblAppName";
            lblAppName.Size = new Size(291, 47);
            lblAppName.TabIndex = 0;
            lblAppName.Text = "Trucking System";
            // 
            // lblTagline
            // 
            lblTagline.AutoSize = true;
            lblTagline.Font = new Font("Segoe UI", 11F);
            lblTagline.ForeColor = Color.FromArgb(203, 213, 225);
            lblTagline.Location = new Point(97, 197);
            lblTagline.Name = "lblTagline";
            lblTagline.Size = new Size(238, 40);
            lblTagline.TabIndex = 1;
            lblTagline.Text = "Hệ thống quản lý vận tải và\nkho trung chuyển hàng hóa nội địa";
            // 
            // roadLine
            // 
            roadLine.BackColor = Color.FromArgb(51, 65, 85);
            roadLine.Location = new Point(0, 506);
            roadLine.Name = "roadLine";
            roadLine.Size = new Size(460, 2);
            roadLine.TabIndex = 2;
            // 
            // rightPanel
            // 
            rightPanel.BackColor = Color.White;
            rightPanel.Controls.Add(btnOpenChatBot);
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
            rightPanel.Location = new Point(460, 0);
            rightPanel.Name = "rightPanel";
            rightPanel.Size = new Size(540, 640);
            rightPanel.TabIndex = 0;
            // 
            // btnOpenChatBot
            // 
            btnOpenChatBot.BackColor = Color.FromArgb(37, 99, 235);
            btnOpenChatBot.Cursor = Cursors.Hand;
            btnOpenChatBot.FlatAppearance.BorderSize = 0;
            btnOpenChatBot.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
            btnOpenChatBot.FlatStyle = FlatStyle.Flat;
            btnOpenChatBot.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnOpenChatBot.ForeColor = Color.White;
            btnOpenChatBot.Location = new Point(425, 592);
            btnOpenChatBot.Name = "btnOpenChatBot";
            btnOpenChatBot.Size = new Size(103, 36);
            btnOpenChatBot.TabIndex = 11;
            btnOpenChatBot.Text = "Mở chat bot";
            btnOpenChatBot.UseVisualStyleBackColor = false;
            btnOpenChatBot.Click += btnOpenChatBot_Click;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(90, 140);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(139, 32);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Đăng nhập";
            // 
            // lblSub
            // 
            lblSub.AutoSize = true;
            lblSub.Font = new Font("Segoe UI", 10F);
            lblSub.ForeColor = Color.FromArgb(100, 116, 139);
            lblSub.Location = new Point(90, 180);
            lblSub.Name = "lblSub";
            lblSub.Size = new Size(233, 19);
            lblSub.TabIndex = 1;
            lblSub.Text = "Nhập thông tin tài khoản để tiếp tục";
            // 
            // lblServerUrl
            // 
            lblServerUrl.AutoSize = true;
            lblServerUrl.Font = new Font("Segoe UI", 9F);
            lblServerUrl.ForeColor = Color.FromArgb(100, 116, 139);
            lblServerUrl.Location = new Point(90, 220);
            lblServerUrl.Name = "lblServerUrl";
            lblServerUrl.Size = new Size(70, 15);
            lblServerUrl.TabIndex = 2;
            lblServerUrl.Text = "SERVER URL";
            // 
            // txtServerUrl
            // 
            txtServerUrl.BorderStyle = BorderStyle.FixedSingle;
            txtServerUrl.Font = new Font("Segoe UI", 10F);
            txtServerUrl.Location = new Point(90, 240);
            txtServerUrl.Name = "txtServerUrl";
            txtServerUrl.Size = new Size(320, 25);
            txtServerUrl.TabIndex = 3;
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Font = new Font("Segoe UI", 9F);
            lblUsername.ForeColor = Color.FromArgb(100, 116, 139);
            lblUsername.Location = new Point(90, 290);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(69, 15);
            lblUsername.TabIndex = 4;
            lblUsername.Text = "TÀI KHOẢN";
            // 
            // txtUsername
            // 
            txtUsername.BorderStyle = BorderStyle.FixedSingle;
            txtUsername.Font = new Font("Segoe UI", 10F);
            txtUsername.Location = new Point(90, 310);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(320, 25);
            txtUsername.TabIndex = 5;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Font = new Font("Segoe UI", 9F);
            lblPassword.ForeColor = Color.FromArgb(100, 116, 139);
            lblPassword.Location = new Point(90, 360);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(68, 15);
            lblPassword.TabIndex = 6;
            lblPassword.Text = "MẬT KHẨU";
            // 
            // txtPassword
            // 
            txtPassword.BorderStyle = BorderStyle.FixedSingle;
            txtPassword.Font = new Font("Segoe UI", 10F);
            txtPassword.Location = new Point(90, 380);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '●';
            txtPassword.Size = new Size(320, 25);
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
            btnLogin.Location = new Point(90, 440);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(320, 44);
            btnLogin.TabIndex = 8;
            btnLogin.Text = "Đăng nhập";
            btnLogin.UseVisualStyleBackColor = false;
            btnLogin.Click += BtnLogin_Click;
            // 
            // lblStatus
            // 
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(220, 38, 38);
            lblStatus.Location = new Point(90, 495);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(320, 40);
            lblStatus.TabIndex = 9;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.BackColor = Color.White;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.ForeColor = Color.FromArgb(100, 116, 139);
            btnClose.Location = new Point(1294, 10);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(36, 36);
            btnClose.TabIndex = 10;
            btnClose.Text = "✕";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += BtnClose_Click;
            // 
            // LoginForm
            // 
            AcceptButton = btnLogin;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(37, 99, 235);
            ClientSize = new Size(1000, 640);
            Controls.Add(rightPanel);
            Controls.Add(bannerPanel);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(4);
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
        private Button btnOpenChatBot;
    }
}
