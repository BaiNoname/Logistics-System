namespace TruckingSystem.Desktop.GUI_TransportOrderManagement
{
    partial class ChangeDeadlineForm
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>Bố cục: TableLayoutPanel 2 cột (nhãn | ô nhập) — hạn hiện tại, hạn mới, gợi ý,
        /// lý do (nhiều dòng, giãn theo form), dòng báo lỗi, thanh nút Lưu / Đóng bên phải.</summary>
        private void InitializeComponent()
        {
            root = new System.Windows.Forms.TableLayoutPanel();
            lblTitle = new System.Windows.Forms.Label();
            lblCurrent = new System.Windows.Forms.Label();
            lblNewDate = new System.Windows.Forms.Label();
            dtNewDate = new System.Windows.Forms.DateTimePicker();
            lblRequester = new System.Windows.Forms.Label();
            cboRequester = new System.Windows.Forms.ComboBox();
            lblFee = new System.Windows.Forms.Label();
            lblHint = new System.Windows.Forms.Label();
            lblReason = new System.Windows.Forms.Label();
            txtReason = new System.Windows.Forms.TextBox();
            lblStatus = new System.Windows.Forms.Label();
            buttonBar = new System.Windows.Forms.FlowLayoutPanel();
            btnClose = new System.Windows.Forms.Button();
            btnSave = new System.Windows.Forms.Button();
            root.SuspendLayout();
            buttonBar.SuspendLayout();
            SuspendLayout();
            //
            // root
            //
            root.ColumnCount = 2;
            root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.Controls.Add(lblTitle, 0, 0);
            root.Controls.Add(lblCurrent, 0, 1);
            root.Controls.Add(lblNewDate, 0, 2);
            root.Controls.Add(dtNewDate, 1, 2);
            root.Controls.Add(lblRequester, 0, 3);
            root.Controls.Add(cboRequester, 1, 3);
            root.Controls.Add(lblFee, 1, 4);
            root.Controls.Add(lblHint, 1, 5);
            root.Controls.Add(lblReason, 0, 6);
            root.Controls.Add(txtReason, 1, 6);
            root.Controls.Add(lblStatus, 0, 7);
            root.Controls.Add(buttonBar, 0, 8);
            root.SetColumnSpan(lblTitle, 2);
            root.SetColumnSpan(lblCurrent, 2);
            root.SetColumnSpan(lblStatus, 2);
            root.SetColumnSpan(buttonBar, 2);
            root.Dock = System.Windows.Forms.DockStyle.Fill;
            root.Name = "root";
            root.Padding = new System.Windows.Forms.Padding(20, 16, 20, 16);
            root.RowCount = 9;
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.Size = new System.Drawing.Size(560, 340);
            root.TabIndex = 0;
            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            lblTitle.Name = "lblTitle";
            lblTitle.TabIndex = 0;
            lblTitle.Text = "📅 Đổi hạn giao";
            //
            // lblCurrent
            //
            lblCurrent.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblCurrent.AutoSize = true;
            lblCurrent.Font = new System.Drawing.Font("Segoe UI", 10F);
            lblCurrent.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblCurrent.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            lblCurrent.Name = "lblCurrent";
            lblCurrent.TabIndex = 1;
            lblCurrent.Text = "Hạn giao hiện tại:";
            //
            // lblNewDate
            //
            lblNewDate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblNewDate.AutoSize = true;
            lblNewDate.Font = new System.Drawing.Font("Segoe UI", 10F);
            lblNewDate.Margin = new System.Windows.Forms.Padding(0, 4, 12, 4);
            lblNewDate.Name = "lblNewDate";
            lblNewDate.TabIndex = 2;
            lblNewDate.Text = "Hạn giao mới: *";
            //
            // dtNewDate
            //
            dtNewDate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            dtNewDate.CustomFormat = "dd/MM/yyyy";
            dtNewDate.Font = new System.Drawing.Font("Segoe UI", 10F);
            dtNewDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            dtNewDate.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            dtNewDate.MinimumSize = new System.Drawing.Size(160, 0);
            dtNewDate.Name = "dtNewDate";
            dtNewDate.TabIndex = 3;
            //
            // lblRequester
            //
            lblRequester.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblRequester.AutoSize = true;
            lblRequester.Font = new System.Drawing.Font("Segoe UI", 10F);
            lblRequester.Margin = new System.Windows.Forms.Padding(0, 4, 12, 4);
            lblRequester.Name = "lblRequester";
            lblRequester.Text = "Bên yêu cầu: *";
            //
            // cboRequester
            //
            cboRequester.Anchor = System.Windows.Forms.AnchorStyles.Left;
            cboRequester.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboRequester.Font = new System.Drawing.Font("Segoe UI", 10F);
            cboRequester.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            cboRequester.MinimumSize = new System.Drawing.Size(260, 0);
            cboRequester.Name = "cboRequester";
            //
            // lblFee
            //
            lblFee.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblFee.AutoSize = true;
            lblFee.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            lblFee.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblFee.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            lblFee.Name = "lblFee";
            lblFee.Text = "";
            //
            // lblHint
            //
            lblHint.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblHint.AutoSize = true;
            lblHint.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblHint.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblHint.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            lblHint.Name = "lblHint";
            lblHint.TabIndex = 4;
            lblHint.Text = "🕒 Đang ước tính số ngày chạy của tuyến...";
            //
            // lblReason
            //
            lblReason.AutoSize = true;
            lblReason.Font = new System.Drawing.Font("Segoe UI", 10F);
            lblReason.Margin = new System.Windows.Forms.Padding(0, 6, 12, 4);
            lblReason.Name = "lblReason";
            lblReason.TabIndex = 5;
            lblReason.Text = "Lý do: *";
            //
            // txtReason
            //
            txtReason.Dock = System.Windows.Forms.DockStyle.Fill;
            txtReason.Font = new System.Drawing.Font("Segoe UI", 10F);
            txtReason.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            txtReason.MaxLength = 500;
            txtReason.MinimumSize = new System.Drawing.Size(0, 70);
            txtReason.Multiline = true;
            txtReason.Name = "txtReason";
            txtReason.PlaceholderText = "VD: Khách đề nghị dời ngày nhận / Kẹt đường do mưa bão, đã báo khách";
            txtReason.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtReason.TabIndex = 6;
            //
            // lblStatus
            //
            lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblStatus.AutoSize = true;
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblStatus.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblStatus.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            lblStatus.Name = "lblStatus";
            lblStatus.TabIndex = 7;
            lblStatus.Text = "Hệ thống ghi nhật ký người đổi, hạn cũ → hạn mới và lý do.";
            //
            // buttonBar
            //
            buttonBar.AutoSize = true;
            buttonBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            buttonBar.Controls.Add(btnClose);
            buttonBar.Controls.Add(btnSave);
            buttonBar.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonBar.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            buttonBar.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            buttonBar.Name = "buttonBar";
            buttonBar.TabIndex = 8;
            buttonBar.WrapContents = false;
            //
            // btnClose
            //
            btnClose.AutoSize = true;
            btnClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnClose.BackColor = System.Drawing.Color.White;
            btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnClose.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnClose.Font = new System.Drawing.Font("Segoe UI", 10F);
            btnClose.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            btnClose.Margin = new System.Windows.Forms.Padding(0);
            btnClose.MinimumSize = new System.Drawing.Size(0, 36);
            btnClose.Name = "btnClose";
            btnClose.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            btnClose.TabIndex = 1;
            btnClose.Text = "Đóng";
            btnClose.UseVisualStyleBackColor = false;
            //
            // btnSave
            //
            btnSave.AutoSize = true;
            btnSave.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnSave.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnSave.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnSave.ForeColor = System.Drawing.Color.White;
            btnSave.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            btnSave.MinimumSize = new System.Drawing.Size(0, 36);
            btnSave.Name = "btnSave";
            btnSave.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            btnSave.TabIndex = 0;
            btnSave.Text = "💾 Lưu hạn giao";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            //
            // ChangeDeadlineForm
            //
            AcceptButton = btnSave;
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.White;
            CancelButton = btnClose;
            ClientSize = new System.Drawing.Size(600, 420);
            Controls.Add(root);
            MinimumSize = new System.Drawing.Size(480, 320);
            Name = "ChangeDeadlineForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Đổi hạn giao";
            root.ResumeLayout(false);
            root.PerformLayout();
            buttonBar.ResumeLayout(false);
            buttonBar.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblCurrent;
        private System.Windows.Forms.Label lblNewDate;
        private System.Windows.Forms.DateTimePicker dtNewDate;
        private System.Windows.Forms.Label lblRequester;
        private System.Windows.Forms.ComboBox cboRequester;
        private System.Windows.Forms.Label lblFee;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.Label lblReason;
        private System.Windows.Forms.TextBox txtReason;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.FlowLayoutPanel buttonBar;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnSave;
    }
}
