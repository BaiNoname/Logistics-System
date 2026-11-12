namespace TruckingSystem.Desktop.GUI.GUI_ChatBot
{
    partial class ChatBotForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            layout = new TableLayoutPanel();
            header = new Panel();
            btnClear = new Button();
            lblSubtitle = new Label();
            lblTitle = new Label();
            headerLine = new Panel();
            flowMessages = new FlowLayoutPanel();
            composerHost = new Panel();
            composerBox = new RoundedPanel();
            txtQuestion = new TextBox();
            btnSend = new SendButton();
            lblNotice = new Label();
            typingTimer = new System.Windows.Forms.Timer(components);
            messageMenu = new ContextMenuStrip(components);
            mnuCopy = new ToolStripMenuItem();
            layout.SuspendLayout();
            header.SuspendLayout();
            composerHost.SuspendLayout();
            composerBox.SuspendLayout();
            messageMenu.SuspendLayout();
            SuspendLayout();
            //
            // layout
            //
            layout.BackColor = Color.FromArgb(250, 249, 245);
            layout.ColumnCount = 1;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.Controls.Add(header, 0, 0);
            layout.Controls.Add(flowMessages, 0, 1);
            layout.Controls.Add(composerHost, 0, 2);
            layout.Dock = DockStyle.Fill;
            layout.Location = new Point(0, 0);
            layout.Margin = new Padding(0);
            layout.Name = "layout";
            layout.RowCount = 3;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 61F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));
            layout.Size = new Size(944, 681);
            layout.TabIndex = 0;
            //
            // header
            //
            header.BackColor = Color.FromArgb(250, 249, 245);
            header.Controls.Add(btnClear);
            header.Controls.Add(lblSubtitle);
            header.Controls.Add(lblTitle);
            header.Controls.Add(headerLine);
            header.Dock = DockStyle.Fill;
            header.Location = new Point(0, 0);
            header.Margin = new Padding(0);
            header.Name = "header";
            header.Size = new Size(944, 61);
            header.TabIndex = 0;
            //
            // btnClear
            //
            btnClear.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClear.AutoSize = true;
            btnClear.BackColor = Color.FromArgb(250, 249, 245);
            btnClear.Cursor = Cursors.Hand;
            btnClear.FlatAppearance.BorderColor = Color.FromArgb(227, 224, 214);
            btnClear.FlatAppearance.MouseOverBackColor = Color.FromArgb(242, 240, 232);
            btnClear.FlatStyle = FlatStyle.Flat;
            btnClear.Font = new Font("Segoe UI", 9.75F);
            btnClear.ForeColor = Color.FromArgb(31, 30, 29);
            btnClear.Location = new Point(752, 14);
            btnClear.Name = "btnClear";
            btnClear.Padding = new Padding(6, 0, 6, 0);
            btnClear.Size = new Size(172, 32);
            btnClear.TabIndex = 2;
            btnClear.Text = "＋  Cuộc trò chuyện mới";
            btnClear.UseVisualStyleBackColor = false;
            //
            // lblSubtitle
            //
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 9F);
            lblSubtitle.ForeColor = Color.FromArgb(115, 114, 108);
            lblSubtitle.Location = new Point(22, 34);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(257, 15);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Báo cước tham khảo theo bảng giá đang áp dụng";
            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(31, 30, 29);
            lblTitle.Location = new Point(20, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(124, 21);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Trợ lý tra cước";
            //
            // headerLine
            //
            headerLine.BackColor = Color.FromArgb(227, 224, 214);
            headerLine.Dock = DockStyle.Bottom;
            headerLine.Location = new Point(0, 60);
            headerLine.Name = "headerLine";
            headerLine.Size = new Size(944, 1);
            headerLine.TabIndex = 3;
            //
            // flowMessages
            //
            flowMessages.AutoScroll = true;
            flowMessages.BackColor = Color.FromArgb(250, 249, 245);
            flowMessages.Dock = DockStyle.Fill;
            flowMessages.FlowDirection = FlowDirection.TopDown;
            flowMessages.Location = new Point(0, 61);
            flowMessages.Margin = new Padding(0);
            flowMessages.Name = "flowMessages";
            flowMessages.Padding = new Padding(0, 20, 0, 12);
            flowMessages.Size = new Size(944, 500);
            flowMessages.TabIndex = 1;
            flowMessages.WrapContents = false;
            //
            // composerHost
            //
            composerHost.BackColor = Color.FromArgb(250, 249, 245);
            composerHost.Controls.Add(composerBox);
            composerHost.Controls.Add(lblNotice);
            composerHost.Dock = DockStyle.Fill;
            composerHost.Location = new Point(0, 561);
            composerHost.Margin = new Padding(0);
            composerHost.Name = "composerHost";
            composerHost.Size = new Size(944, 120);
            composerHost.TabIndex = 2;
            //
            // composerBox
            //
            composerBox.BorderColor = Color.FromArgb(227, 224, 214);
            composerBox.Controls.Add(txtQuestion);
            composerBox.Controls.Add(btnSend);
            composerBox.FillColor = Color.White;
            composerBox.Location = new Point(92, 8);
            composerBox.Name = "composerBox";
            composerBox.Radius = 18;
            composerBox.Size = new Size(760, 80);
            composerBox.TabIndex = 0;
            //
            // txtQuestion
            //
            txtQuestion.BackColor = Color.White;
            txtQuestion.BorderStyle = BorderStyle.None;
            txtQuestion.Font = new Font("Segoe UI", 11F);
            txtQuestion.ForeColor = Color.FromArgb(31, 30, 29);
            txtQuestion.Location = new Point(18, 14);
            txtQuestion.MaxLength = 2000;
            txtQuestion.Multiline = true;
            txtQuestion.Name = "txtQuestion";
            txtQuestion.PlaceholderText = "Mô tả chuyến hàng, ví dụ: chở 3 tấn gạch từ Thủ Đức đi Biên Hòa…";
            txtQuestion.Size = new Size(676, 52);
            txtQuestion.TabIndex = 0;
            //
            // btnSend
            //
            btnSend.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSend.Location = new Point(714, 40);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(32, 32);
            btnSend.TabIndex = 1;
            btnSend.UseVisualStyleBackColor = false;
            //
            // lblNotice
            //
            lblNotice.Font = new Font("Segoe UI", 8.25F);
            lblNotice.ForeColor = Color.FromArgb(115, 114, 108);
            lblNotice.Location = new Point(92, 94);
            lblNotice.Name = "lblNotice";
            lblNotice.Size = new Size(760, 18);
            lblNotice.TabIndex = 1;
            lblNotice.Text = "Enter để gửi, Shift+Enter để xuống dòng. Cước tham khảo, chưa gồm VAT và các khoản ngoài bảng giá.";
            lblNotice.TextAlign = ContentAlignment.MiddleCenter;
            //
            // typingTimer
            //
            typingTimer.Interval = 400;
            //
            // messageMenu
            //
            messageMenu.Items.AddRange(new ToolStripItem[] { mnuCopy });
            messageMenu.Name = "messageMenu";
            messageMenu.Size = new Size(181, 26);
            //
            // mnuCopy
            //
            mnuCopy.Name = "mnuCopy";
            mnuCopy.Size = new Size(180, 22);
            mnuCopy.Text = "Sao chép nội dung";
            //
            // ChatBotForm
            //
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(250, 249, 245);
            ClientSize = new Size(944, 681);
            Controls.Add(layout);
            Font = new Font("Segoe UI", 10F);
            ForeColor = Color.FromArgb(31, 30, 29);
            MinimumSize = new Size(640, 560);
            Name = "ChatBotForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Chatbot tra cứu cước vận chuyển";
            layout.ResumeLayout(false);
            header.ResumeLayout(false);
            header.PerformLayout();
            composerHost.ResumeLayout(false);
            composerBox.ResumeLayout(false);
            composerBox.PerformLayout();
            messageMenu.ResumeLayout(false);
            ResumeLayout(false);
        }

        private System.Windows.Forms.TableLayoutPanel layout;
        private System.Windows.Forms.Panel header;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel headerLine;
        private System.Windows.Forms.FlowLayoutPanel flowMessages;
        private System.Windows.Forms.Panel composerHost;
        private RoundedPanel composerBox;
        private System.Windows.Forms.TextBox txtQuestion;
        private SendButton btnSend;
        private System.Windows.Forms.Label lblNotice;
        private System.Windows.Forms.Timer typingTimer;
        private System.Windows.Forms.ContextMenuStrip messageMenu;
        private System.Windows.Forms.ToolStripMenuItem mnuCopy;
    }
}
