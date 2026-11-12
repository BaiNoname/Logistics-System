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

        /// <summary>Bố cục: bannerPanel (TableLayoutPanel, Dock=Left, rộng 460 ở 96 DPI) = khoảng
        /// trống / tên ứng dụng / khẩu hiệu / vùng xe tải chạy (truckCanvas — hoạt ảnh vẽ bằng code)
        /// / vạch đường / khoảng trống. rightPanel (TableLayoutPanel 3 cột 15%-70%-15%, Dock=Fill) =
        /// nút ✕ (trên, phải) / khối đăng nhập loginBox căn giữa theo chiều dọc / nút chat bot
        /// (dưới, phải). loginBox: mỗi nhãn + ô nhập 1 dòng AutoSize, ô nhập giãn kín bề ngang.
        /// Không có tọa độ cứng -> không đè chữ ở mọi mức phóng to màn hình.</summary>
        private void InitializeComponent()
        {
            bannerPanel = new System.Windows.Forms.TableLayoutPanel();
            lblAppName = new System.Windows.Forms.Label();
            lblTagline = new System.Windows.Forms.Label();
            truckCanvas = new System.Windows.Forms.Panel();
            roadLine = new System.Windows.Forms.Panel();
            rightPanel = new System.Windows.Forms.TableLayoutPanel();
            btnClose = new System.Windows.Forms.Button();
            loginBox = new System.Windows.Forms.TableLayoutPanel();
            lblTitle = new System.Windows.Forms.Label();
            lblSub = new System.Windows.Forms.Label();
            lblServerUrl = new System.Windows.Forms.Label();
            txtServerUrl = new System.Windows.Forms.TextBox();
            lblUsername = new System.Windows.Forms.Label();
            txtUsername = new System.Windows.Forms.TextBox();
            lblPassword = new System.Windows.Forms.Label();
            txtPassword = new System.Windows.Forms.TextBox();
            btnLogin = new System.Windows.Forms.Button();
            lblStatus = new System.Windows.Forms.Label();
            bannerPanel.SuspendLayout();
            rightPanel.SuspendLayout();
            loginBox.SuspendLayout();
            SuspendLayout();
            //
            // bannerPanel
            //
            bannerPanel.BackColor = System.Drawing.Color.FromArgb(30, 41, 59);
            bannerPanel.ColumnCount = 1;
            bannerPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            bannerPanel.Controls.Add(lblAppName, 0, 1);
            bannerPanel.Controls.Add(lblTagline, 0, 2);
            bannerPanel.Controls.Add(truckCanvas, 0, 3);
            bannerPanel.Controls.Add(roadLine, 0, 4);
            bannerPanel.Dock = System.Windows.Forms.DockStyle.Left;
            bannerPanel.Margin = new System.Windows.Forms.Padding(0);
            bannerPanel.Name = "bannerPanel";
            bannerPanel.RowCount = 6;
            bannerPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            bannerPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            bannerPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            bannerPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            bannerPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            bannerPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            bannerPanel.Size = new System.Drawing.Size(460, 640);
            bannerPanel.TabIndex = 1;
            //
            // lblAppName
            //
            lblAppName.Anchor = System.Windows.Forms.AnchorStyles.None;
            lblAppName.AutoSize = true;
            lblAppName.Font = new System.Drawing.Font("Segoe UI", 26F, System.Drawing.FontStyle.Bold);
            lblAppName.ForeColor = System.Drawing.Color.White;
            lblAppName.Margin = new System.Windows.Forms.Padding(12, 0, 12, 16);
            lblAppName.Name = "lblAppName";
            lblAppName.TabIndex = 0;
            lblAppName.Text = "Trucking System";
            //
            // lblTagline
            //
            lblTagline.Anchor = System.Windows.Forms.AnchorStyles.None;
            lblTagline.AutoSize = true;
            lblTagline.Font = new System.Drawing.Font("Segoe UI", 11F);
            lblTagline.ForeColor = System.Drawing.Color.FromArgb(203, 213, 225);
            lblTagline.Margin = new System.Windows.Forms.Padding(12, 0, 12, 0);
            lblTagline.Name = "lblTagline";
            lblTagline.TabIndex = 1;
            lblTagline.Text = "Hệ thống quản lý vận tải và\nkho trung chuyển hàng hóa nội địa";
            lblTagline.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            //
            // truckCanvas
            //
            truckCanvas.BackColor = System.Drawing.Color.FromArgb(30, 41, 59);
            truckCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
            truckCanvas.Margin = new System.Windows.Forms.Padding(0);
            truckCanvas.Name = "truckCanvas";
            truckCanvas.TabIndex = 2;
            //
            // roadLine
            //
            roadLine.BackColor = System.Drawing.Color.FromArgb(51, 65, 85);
            roadLine.Dock = System.Windows.Forms.DockStyle.Fill;
            roadLine.Margin = new System.Windows.Forms.Padding(0);
            roadLine.Name = "roadLine";
            roadLine.Size = new System.Drawing.Size(460, 2);
            roadLine.TabIndex = 3;
            //
            // rightPanel
            //
            rightPanel.BackColor = System.Drawing.Color.White;
            rightPanel.ColumnCount = 3;
            rightPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            rightPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            rightPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            rightPanel.Controls.Add(btnClose, 0, 0);
            rightPanel.Controls.Add(loginBox, 1, 2);
            rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            rightPanel.Margin = new System.Windows.Forms.Padding(0);
            rightPanel.Name = "rightPanel";
            rightPanel.RowCount = 5;
            rightPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            rightPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            rightPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            rightPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            rightPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            rightPanel.SetColumnSpan(btnClose, 3);
            rightPanel.Size = new System.Drawing.Size(540, 640);
            rightPanel.TabIndex = 0;
            //
            // btnClose
            //
            btnClose.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnClose.AutoSize = true;
            btnClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnClose.BackColor = System.Drawing.Color.White;
            btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnClose.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            btnClose.Margin = new System.Windows.Forms.Padding(0, 10, 10, 0);
            btnClose.MinimumSize = new System.Drawing.Size(36, 36);
            btnClose.Name = "btnClose";
            btnClose.TabIndex = 10;
            btnClose.Text = "✕";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            //
            // loginBox
            //
            loginBox.AutoSize = true;
            loginBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            loginBox.ColumnCount = 1;
            loginBox.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            loginBox.Controls.Add(lblTitle, 0, 0);
            loginBox.Controls.Add(lblSub, 0, 1);
            loginBox.Controls.Add(lblServerUrl, 0, 2);
            loginBox.Controls.Add(txtServerUrl, 0, 3);
            loginBox.Controls.Add(lblUsername, 0, 4);
            loginBox.Controls.Add(txtUsername, 0, 5);
            loginBox.Controls.Add(lblPassword, 0, 6);
            loginBox.Controls.Add(txtPassword, 0, 7);
            loginBox.Controls.Add(btnLogin, 0, 8);
            loginBox.Controls.Add(lblStatus, 0, 9);
            loginBox.Dock = System.Windows.Forms.DockStyle.Fill;
            loginBox.Margin = new System.Windows.Forms.Padding(0);
            loginBox.Name = "loginBox";
            loginBox.RowCount = 10;
            loginBox.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            loginBox.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            loginBox.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            loginBox.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            loginBox.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            loginBox.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            loginBox.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            loginBox.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            loginBox.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            loginBox.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            loginBox.TabIndex = 0;
            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            lblTitle.Name = "lblTitle";
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Đăng nhập";
            //
            // lblSub
            //
            lblSub.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblSub.AutoSize = true;
            lblSub.Font = new System.Drawing.Font("Segoe UI", 10F);
            lblSub.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblSub.Margin = new System.Windows.Forms.Padding(0, 0, 0, 20);
            lblSub.Name = "lblSub";
            lblSub.TabIndex = 1;
            lblSub.Text = "Nhập thông tin tài khoản để tiếp tục";
            //
            // lblServerUrl
            //
            lblServerUrl.AutoSize = true;
            lblServerUrl.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblServerUrl.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblServerUrl.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            lblServerUrl.Name = "lblServerUrl";
            lblServerUrl.TabIndex = 2;
            lblServerUrl.Text = "SERVER URL";
            //
            // txtServerUrl
            //
            txtServerUrl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txtServerUrl.Dock = System.Windows.Forms.DockStyle.Fill;
            txtServerUrl.Font = new System.Drawing.Font("Segoe UI", 10F);
            txtServerUrl.Margin = new System.Windows.Forms.Padding(0, 0, 0, 20);
            txtServerUrl.Name = "txtServerUrl";
            txtServerUrl.TabIndex = 3;
            //
            // lblUsername
            //
            lblUsername.AutoSize = true;
            lblUsername.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblUsername.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblUsername.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            lblUsername.Name = "lblUsername";
            lblUsername.TabIndex = 4;
            lblUsername.Text = "TÀI KHOẢN";
            //
            // txtUsername
            //
            txtUsername.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txtUsername.Dock = System.Windows.Forms.DockStyle.Fill;
            txtUsername.Font = new System.Drawing.Font("Segoe UI", 10F);
            txtUsername.Margin = new System.Windows.Forms.Padding(0, 0, 0, 20);
            txtUsername.Name = "txtUsername";
            txtUsername.TabIndex = 5;
            //
            // lblPassword
            //
            lblPassword.AutoSize = true;
            lblPassword.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblPassword.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblPassword.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            lblPassword.Name = "lblPassword";
            lblPassword.TabIndex = 6;
            lblPassword.Text = "MẬT KHẨU";
            //
            // txtPassword
            //
            txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txtPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            txtPassword.Font = new System.Drawing.Font("Segoe UI", 10F);
            txtPassword.Margin = new System.Windows.Forms.Padding(0, 0, 0, 30);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '●';
            txtPassword.TabIndex = 7;
            //
            // btnLogin
            //
            btnLogin.AutoSize = true;
            btnLogin.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnLogin.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            btnLogin.Cursor = System.Windows.Forms.Cursors.Hand;
            btnLogin.Dock = System.Windows.Forms.DockStyle.Fill;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnLogin.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnLogin.ForeColor = System.Drawing.Color.White;
            btnLogin.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            btnLogin.MinimumSize = new System.Drawing.Size(0, 44);
            btnLogin.Name = "btnLogin";
            btnLogin.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnLogin.TabIndex = 8;
            btnLogin.Text = "Đăng nhập";
            btnLogin.UseVisualStyleBackColor = false;
            btnLogin.Click += new System.EventHandler(this.BtnLogin_Click);
            //
            // lblStatus
            //
            lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblStatus.AutoSize = true;
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblStatus.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
            lblStatus.Margin = new System.Windows.Forms.Padding(0);
            lblStatus.MinimumSize = new System.Drawing.Size(0, 40);
            lblStatus.Name = "lblStatus";
            lblStatus.TabIndex = 9;
            //
            // LoginForm
            //
            AcceptButton = btnLogin;
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            ClientSize = new System.Drawing.Size(1000, 640);
            Controls.Add(rightPanel);
            Controls.Add(bannerPanel);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Margin = new System.Windows.Forms.Padding(4);
            Name = "LoginForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Trucking System — Đăng nhập";
            loginBox.ResumeLayout(false);
            loginBox.PerformLayout();
            rightPanel.ResumeLayout(false);
            rightPanel.PerformLayout();
            bannerPanel.ResumeLayout(false);
            bannerPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel bannerPanel;
        private System.Windows.Forms.Label lblAppName;
        private System.Windows.Forms.Label lblTagline;
        private System.Windows.Forms.Panel truckCanvas;
        private System.Windows.Forms.Panel roadLine;
        private System.Windows.Forms.TableLayoutPanel rightPanel;
        private System.Windows.Forms.TableLayoutPanel loginBox;
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
