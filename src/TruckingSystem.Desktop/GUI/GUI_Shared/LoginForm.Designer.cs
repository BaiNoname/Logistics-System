namespace TruckingSystem.Desktop.GUI_Shared;

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
        this.bannerPanel = new System.Windows.Forms.Panel();
        this.lblAppName = new System.Windows.Forms.Label();
        this.lblTagline = new System.Windows.Forms.Label();
        this.roadLine = new System.Windows.Forms.Panel();
        this.rightPanel = new System.Windows.Forms.Panel();
        this.lblTitle = new System.Windows.Forms.Label();
        this.lblSub = new System.Windows.Forms.Label();
        this.lblServerUrl = new System.Windows.Forms.Label();
        this.txtServerUrl = new System.Windows.Forms.TextBox();
        this.lblUsername = new System.Windows.Forms.Label();
        this.txtUsername = new System.Windows.Forms.TextBox();
        this.lblPassword = new System.Windows.Forms.Label();
        this.txtPassword = new System.Windows.Forms.TextBox();
        this.btnLogin = new System.Windows.Forms.Button();
        this.lblStatus = new System.Windows.Forms.Label();
        this.btnClose = new System.Windows.Forms.Button();
        this.bannerPanel.SuspendLayout();
        this.rightPanel.SuspendLayout();
        this.SuspendLayout();
        //
        // bannerPanel
        //
        this.bannerPanel.Dock = System.Windows.Forms.DockStyle.Left;
        this.bannerPanel.Width = 460;
        this.bannerPanel.BackColor = TruckingSystem.Desktop.Common.Theme.SidebarBg;
        this.bannerPanel.Controls.Add(this.lblAppName);
        this.bannerPanel.Controls.Add(this.lblTagline);
        this.bannerPanel.Controls.Add(this.roadLine);
        //
        // lblAppName
        //
        this.lblAppName.Location = new System.Drawing.Point(40, 200);
        this.lblAppName.AutoSize = true;
        this.lblAppName.Text = "Trucking System";
        this.lblAppName.Font = new System.Drawing.Font("Segoe UI", 26F, System.Drawing.FontStyle.Bold);
        this.lblAppName.ForeColor = System.Drawing.Color.White;
        //
        // lblTagline
        //
        this.lblTagline.Location = new System.Drawing.Point(40, 255);
        this.lblTagline.AutoSize = true;
        this.lblTagline.Text = "Hệ thống quản lý vận tải và\nkho trung chuyển hàng hóa nội địa";
        this.lblTagline.Font = new System.Drawing.Font("Segoe UI", 11F);
        this.lblTagline.ForeColor = TruckingSystem.Desktop.Common.Theme.SidebarText;
        //
        // roadLine
        //
        this.roadLine.Location = new System.Drawing.Point(0, 506);
        this.roadLine.Size = new System.Drawing.Size(460, 2);
        this.roadLine.BackColor = System.Drawing.ColorTranslator.FromHtml("#334155");
        //
        // rightPanel
        //
        this.rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.rightPanel.BackColor = System.Drawing.Color.White;
        this.rightPanel.Controls.Add(this.lblTitle);
        this.rightPanel.Controls.Add(this.lblSub);
        this.rightPanel.Controls.Add(this.lblServerUrl);
        this.rightPanel.Controls.Add(this.txtServerUrl);
        this.rightPanel.Controls.Add(this.lblUsername);
        this.rightPanel.Controls.Add(this.txtUsername);
        this.rightPanel.Controls.Add(this.lblPassword);
        this.rightPanel.Controls.Add(this.txtPassword);
        this.rightPanel.Controls.Add(this.btnLogin);
        this.rightPanel.Controls.Add(this.lblStatus);
        this.rightPanel.Controls.Add(this.btnClose);
        //
        // lblTitle
        //
        this.lblTitle.Location = new System.Drawing.Point(90, 140);
        this.lblTitle.AutoSize = true;
        this.lblTitle.Text = "Đăng nhập";
        this.lblTitle.Font = TruckingSystem.Desktop.Common.Theme.FontTitle;
        this.lblTitle.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
        //
        // lblSub
        //
        this.lblSub.Location = new System.Drawing.Point(90, 180);
        this.lblSub.AutoSize = true;
        this.lblSub.Text = "Nhập thông tin tài khoản để tiếp tục";
        this.lblSub.Font = TruckingSystem.Desktop.Common.Theme.FontNormal;
        this.lblSub.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        //
        // lblServerUrl
        //
        this.lblServerUrl.Location = new System.Drawing.Point(90, 220);
        this.lblServerUrl.AutoSize = true;
        this.lblServerUrl.Text = "SERVER URL";
        this.lblServerUrl.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblServerUrl.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        //
        // txtServerUrl
        //
        this.txtServerUrl.Location = new System.Drawing.Point(90, 240);
        this.txtServerUrl.Size = new System.Drawing.Size(320, 32);
        this.txtServerUrl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.txtServerUrl.Font = TruckingSystem.Desktop.Common.Theme.FontNormal;
        //
        // lblUsername
        //
        this.lblUsername.Location = new System.Drawing.Point(90, 290);
        this.lblUsername.AutoSize = true;
        this.lblUsername.Text = "TÀI KHOẢN";
        this.lblUsername.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblUsername.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        //
        // txtUsername
        //
        this.txtUsername.Location = new System.Drawing.Point(90, 310);
        this.txtUsername.Size = new System.Drawing.Size(320, 32);
        this.txtUsername.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.txtUsername.Font = TruckingSystem.Desktop.Common.Theme.FontNormal;
        //
        // lblPassword
        //
        this.lblPassword.Location = new System.Drawing.Point(90, 360);
        this.lblPassword.AutoSize = true;
        this.lblPassword.Text = "MẬT KHẨU";
        this.lblPassword.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        this.lblPassword.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        //
        // txtPassword
        //
        this.txtPassword.Location = new System.Drawing.Point(90, 380);
        this.txtPassword.Size = new System.Drawing.Size(320, 32);
        this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.txtPassword.Font = TruckingSystem.Desktop.Common.Theme.FontNormal;
        this.txtPassword.PasswordChar = '●';
        //
        // btnLogin
        //
        this.btnLogin.Location = new System.Drawing.Point(90, 440);
        this.btnLogin.Size = new System.Drawing.Size(320, 44);
        this.btnLogin.Text = "Đăng nhập";
        this.btnLogin.BackColor = TruckingSystem.Desktop.Common.Theme.Primary;
        this.btnLogin.ForeColor = System.Drawing.Color.White;
        this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnLogin.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
        this.btnLogin.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnLogin.FlatAppearance.BorderSize = 0;
        this.btnLogin.FlatAppearance.MouseOverBackColor = TruckingSystem.Desktop.Common.Theme.PrimaryDark;
        this.btnLogin.Click += new System.EventHandler(this.BtnLogin_Click);
        //
        // lblStatus
        //
        this.lblStatus.Location = new System.Drawing.Point(90, 495);
        this.lblStatus.Size = new System.Drawing.Size(320, 40);
        this.lblStatus.ForeColor = TruckingSystem.Desktop.Common.Theme.Danger;
        this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
        //
        // btnClose
        //
        this.btnClose.Location = new System.Drawing.Point(954, 10);
        this.btnClose.Size = new System.Drawing.Size(36, 36);
        this.btnClose.Text = "✕";
        this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnClose.BackColor = System.Drawing.Color.White;
        this.btnClose.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
        this.btnClose.FlatAppearance.BorderSize = 0;
        this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
        //
        // LoginForm
        //
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.BackColor = TruckingSystem.Desktop.Common.Theme.Primary;
        this.ClientSize = new System.Drawing.Size(1000, 640);
        this.Controls.Add(this.rightPanel);
        this.Controls.Add(this.bannerPanel);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        this.Name = "LoginForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Trucking System — Đăng nhập";
        this.AcceptButton = this.btnLogin;
        this.bannerPanel.ResumeLayout(false);
        this.bannerPanel.PerformLayout();
        this.rightPanel.ResumeLayout(false);
        this.rightPanel.PerformLayout();
        this.ResumeLayout(false);
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
