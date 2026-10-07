namespace TruckingSystem.Desktop.GUI.GUI_ChatBot
{
    partial class ChatBotForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.RichTextBox txtConversation;
        private System.Windows.Forms.TextBox txtQuestion;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.ListBox lstSuggestions;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblTitle = new Label();
            lblSuggestions = new Label();
            header = new Panel();
            conversationPanel = new Panel();
            txtConversation = new RichTextBox();
            suggestionsPanel = new Panel();
            lstSuggestions = new ListBox();
            content = new TableLayoutPanel();
            composer = new TableLayoutPanel();
            btnClear = new Button();
            txtQuestion = new TextBox();
            btnSend = new Button();
            layout = new TableLayoutPanel();
            header.SuspendLayout();
            conversationPanel.SuspendLayout();
            suggestionsPanel.SuspendLayout();
            content.SuspendLayout();
            composer.SuspendLayout();
            layout.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.Location = new Point(20, 12);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(165, 32);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Trợ lý dữ liệu";
            // 
            // lblSuggestions
            // 
            lblSuggestions.Dock = DockStyle.Top;
            lblSuggestions.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblSuggestions.ForeColor = Color.FromArgb(122, 122, 122);
            lblSuggestions.Location = new Point(20, 20);
            lblSuggestions.Name = "lblSuggestions";
            lblSuggestions.Size = new Size(206, 34);
            lblSuggestions.TabIndex = 1;
            lblSuggestions.Text = "CÂU HỎI GỢI Ý";
            // 
            // header
            // 
            header.BackColor = Color.White;
            header.Controls.Add(lblTitle);
            header.Dock = DockStyle.Fill;
            header.Location = new Point(3, 3);
            header.Name = "header";
            header.Size = new Size(898, 66);
            header.TabIndex = 0;
            // 
            // conversationPanel
            // 
            conversationPanel.BackColor = Color.White;
            conversationPanel.Controls.Add(txtConversation);
            conversationPanel.Dock = DockStyle.Fill;
            conversationPanel.Location = new Point(3, 3);
            conversationPanel.Name = "conversationPanel";
            conversationPanel.Padding = new Padding(24);
            conversationPanel.Size = new Size(640, 285);
            conversationPanel.TabIndex = 0;
            // 
            // txtConversation
            // 
            txtConversation.BackColor = Color.White;
            txtConversation.BorderStyle = BorderStyle.None;
            txtConversation.Dock = DockStyle.Fill;
            txtConversation.Font = new Font("Segoe UI", 11F);
            txtConversation.HideSelection = false;
            txtConversation.Location = new Point(24, 24);
            txtConversation.Name = "txtConversation";
            txtConversation.ReadOnly = true;
            txtConversation.Size = new Size(592, 237);
            txtConversation.TabIndex = 0;
            txtConversation.Text = "";
            // 
            // suggestionsPanel
            // 
            suggestionsPanel.BackColor = Color.FromArgb(245, 245, 247);
            suggestionsPanel.Controls.Add(lstSuggestions);
            suggestionsPanel.Controls.Add(lblSuggestions);
            suggestionsPanel.Dock = DockStyle.Fill;
            suggestionsPanel.Location = new Point(649, 3);
            suggestionsPanel.Name = "suggestionsPanel";
            suggestionsPanel.Padding = new Padding(20);
            suggestionsPanel.Size = new Size(246, 285);
            suggestionsPanel.TabIndex = 1;
            // 
            // lstSuggestions
            // 
            lstSuggestions.BackColor = Color.FromArgb(245, 245, 247);
            lstSuggestions.BorderStyle = BorderStyle.None;
            lstSuggestions.Dock = DockStyle.Fill;
            lstSuggestions.Font = new Font("Segoe UI", 10F);
            lstSuggestions.IntegralHeight = false;
            lstSuggestions.ItemHeight = 17;
            lstSuggestions.Items.AddRange(new object[] { "Tra cứu thông tin gói cước.", "Tra cưu chi phí vận chuyển.", "Tra cứu đơn hàng." });
            lstSuggestions.Location = new Point(20, 54);
            lstSuggestions.Name = "lstSuggestions";
            lstSuggestions.Size = new Size(206, 211);
            lstSuggestions.TabIndex = 0;
            // 
            // content
            // 
            content.BackColor = Color.White;
            content.ColumnCount = 2;
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72F));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28F));
            content.Controls.Add(conversationPanel, 0, 0);
            content.Controls.Add(suggestionsPanel, 1, 0);
            content.Dock = DockStyle.Fill;
            content.Location = new Point(3, 75);
            content.Name = "content";
            content.RowCount = 1;
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            content.Size = new Size(898, 291);
            content.TabIndex = 1;
            // 
            // composer
            // 
            composer.BackColor = Color.White;
            composer.ColumnCount = 2;
            composer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            composer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92F));
            composer.Controls.Add(btnClear, 0, 0);
            composer.Controls.Add(txtQuestion, 0, 1);
            composer.Controls.Add(btnSend, 1, 1);
            composer.Dock = DockStyle.Fill;
            composer.Location = new Point(3, 372);
            composer.Name = "composer";
            composer.Padding = new Padding(24, 12, 24, 8);
            composer.RowCount = 2;
            composer.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            composer.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            composer.Size = new Size(898, 106);
            composer.TabIndex = 2;
            // 
            // btnClear
            // 
            btnClear.Anchor = AnchorStyles.Right;
            btnClear.AutoSize = true;
            composer.SetColumnSpan(btnClear, 2);
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.FlatStyle = FlatStyle.Flat;
            btnClear.ForeColor = Color.FromArgb(0, 102, 204);
            btnClear.Location = new Point(792, 15);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(79, 29);
            btnClear.TabIndex = 0;
            btnClear.Text = "Làm mới";
            // 
            // txtQuestion
            // 
            txtQuestion.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtQuestion.BorderStyle = BorderStyle.FixedSingle;
            txtQuestion.Font = new Font("Segoe UI", 11F);
            txtQuestion.Location = new Point(24, 51);
            txtQuestion.Margin = new Padding(0, 4, 12, 4);
            txtQuestion.Name = "txtQuestion";
            txtQuestion.Size = new Size(746, 27);
            txtQuestion.TabIndex = 1;
            // 
            // btnSend
            // 
            btnSend.BackColor = Color.FromArgb(0, 102, 204);
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.FlatStyle = FlatStyle.Flat;
            btnSend.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSend.ForeColor = Color.White;
            btnSend.Location = new Point(782, 51);
            btnSend.Margin = new Padding(0, 4, 0, 4);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(92, 27);
            btnSend.TabIndex = 2;
            btnSend.Text = "Gửi";
            btnSend.UseVisualStyleBackColor = false;
            // 
            // layout
            // 
            layout.BackColor = Color.White;
            layout.ColumnCount = 1;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            layout.Controls.Add(header, 0, 0);
            layout.Controls.Add(content, 0, 1);
            layout.Controls.Add(composer, 0, 2);
            layout.Dock = DockStyle.Fill;
            layout.Location = new Point(20, 20);
            layout.Name = "layout";
            layout.RowCount = 3;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 112F));
            layout.Size = new Size(904, 481);
            layout.TabIndex = 0;
            // 
            // ChatBotForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 245, 247);
            ClientSize = new Size(944, 521);
            Controls.Add(layout);
            Font = new Font("Segoe UI", 10F);
            ForeColor = Color.FromArgb(29, 29, 31);
            MinimumSize = new Size(760, 560);
            Name = "ChatBotForm";
            Padding = new Padding(20);
            StartPosition = FormStartPosition.CenterParent;
            Text = "Chatbot tra cứu dữ liệu";
            header.ResumeLayout(false);
            header.PerformLayout();
            conversationPanel.ResumeLayout(false);
            suggestionsPanel.ResumeLayout(false);
            content.ResumeLayout(false);
            composer.ResumeLayout(false);
            composer.PerformLayout();
            layout.ResumeLayout(false);
            ResumeLayout(false);

        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSuggestions;
        private System.Windows.Forms.Panel header;
        private System.Windows.Forms.Panel conversationPanel;
        private System.Windows.Forms.Panel suggestionsPanel;
        private System.Windows.Forms.TableLayoutPanel content;
        private System.Windows.Forms.TableLayoutPanel composer;
        private System.Windows.Forms.TableLayoutPanel layout;
    }
}