namespace TruckingSystem.Desktop.GUI_TransportOrderManagement
{
    partial class RecordStorageFeeForm
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>Bố cục: root (TableLayoutPanel 1 cột) = tiêu đề / thông tin đơn (hạn giao, đơn giá kho) /
        /// bảng nhập (nhãn | ô: "Hàng nằm kho tới ngày", "Lý do") / ghi chú khoảng tính phí / trạng thái /
        /// thanh nút (phải sang trái). Không tọa độ cứng.</summary>
        private void InitializeComponent()
        {
            root = new System.Windows.Forms.TableLayoutPanel();
            lblTitle = new System.Windows.Forms.Label();
            lblInfo = new System.Windows.Forms.Label();
            fields = new System.Windows.Forms.TableLayoutPanel();
            lblEndDate = new System.Windows.Forms.Label();
            dtEndDate = new System.Windows.Forms.DateTimePicker();
            lblReason = new System.Windows.Forms.Label();
            txtReason = new System.Windows.Forms.TextBox();
            lblHint = new System.Windows.Forms.Label();
            lblStatus = new System.Windows.Forms.Label();
            buttonBar = new System.Windows.Forms.FlowLayoutPanel();
            btnCancel = new System.Windows.Forms.Button();
            btnSave = new System.Windows.Forms.Button();
            root.SuspendLayout();
            fields.SuspendLayout();
            buttonBar.SuspendLayout();
            SuspendLayout();
            //
            // root
            //
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.Controls.Add(lblTitle, 0, 0);
            root.Controls.Add(lblInfo, 0, 1);
            root.Controls.Add(fields, 0, 2);
            root.Controls.Add(lblHint, 0, 3);
            root.Controls.Add(lblStatus, 0, 4);
            root.Controls.Add(buttonBar, 0, 5);
            root.Dock = System.Windows.Forms.DockStyle.Fill;
            root.Name = "root";
            root.Padding = new System.Windows.Forms.Padding(20, 16, 20, 16);
            root.RowCount = 6;
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.Size = new System.Drawing.Size(600, 440);
            root.TabIndex = 0;
            //
            // lblTitle
            //
            lblTitle.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            lblTitle.Name = "lblTitle";
            lblTitle.TabIndex = 0;
            lblTitle.Text = "➕ Ghi nhận phí lưu kho bổ sung";
            //
            // lblInfo
            //
            lblInfo.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblInfo.AutoSize = true;
            lblInfo.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblInfo.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblInfo.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            lblInfo.Name = "lblInfo";
            lblInfo.TabIndex = 1;
            //
            // fields
            //
            fields.ColumnCount = 2;
            fields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            fields.Controls.Add(lblEndDate, 0, 0);
            fields.Controls.Add(dtEndDate, 1, 0);
            fields.Controls.Add(lblReason, 0, 1);
            fields.Controls.Add(txtReason, 1, 1);
            fields.Dock = System.Windows.Forms.DockStyle.Fill;
            fields.Margin = new System.Windows.Forms.Padding(0);
            fields.Name = "fields";
            fields.RowCount = 2;
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            fields.TabIndex = 2;
            //
            // lblEndDate
            //
            lblEndDate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblEndDate.AutoSize = true;
            lblEndDate.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblEndDate.Margin = new System.Windows.Forms.Padding(0, 0, 12, 8);
            lblEndDate.Name = "lblEndDate";
            lblEndDate.TabIndex = 0;
            lblEndDate.Text = "Hàng nằm kho (do khách) tới ngày *";
            //
            // dtEndDate
            //
            dtEndDate.CustomFormat = "dd/MM/yyyy";
            dtEndDate.Dock = System.Windows.Forms.DockStyle.Fill;
            dtEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            dtEndDate.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            dtEndDate.Name = "dtEndDate";
            dtEndDate.TabIndex = 1;
            //
            // lblReason
            //
            lblReason.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            lblReason.AutoSize = true;
            lblReason.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblReason.Margin = new System.Windows.Forms.Padding(0, 4, 12, 8);
            lblReason.Name = "lblReason";
            lblReason.TabIndex = 2;
            lblReason.Text = "Lý do *";
            //
            // txtReason
            //
            txtReason.Dock = System.Windows.Forms.DockStyle.Fill;
            txtReason.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            txtReason.MinimumSize = new System.Drawing.Size(0, 60);
            txtReason.Multiline = true;
            txtReason.Name = "txtReason";
            txtReason.PlaceholderText = "VD: Khách hẹn lại ngày nhận hàng, nhân viên quên đổi hạn giao";
            txtReason.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtReason.TabIndex = 3;
            //
            // lblHint
            //
            lblHint.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblHint.AutoSize = true;
            lblHint.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblHint.ForeColor = System.Drawing.Color.FromArgb(180, 83, 9);
            lblHint.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            lblHint.Name = "lblHint";
            lblHint.TabIndex = 3;
            //
            // lblStatus
            //
            lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblStatus.AutoSize = true;
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblStatus.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
            lblStatus.Margin = new System.Windows.Forms.Padding(0, 4, 0, 8);
            lblStatus.Name = "lblStatus";
            lblStatus.TabIndex = 4;
            //
            // buttonBar
            //
            buttonBar.AutoSize = true;
            buttonBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            buttonBar.Controls.Add(btnCancel);
            buttonBar.Controls.Add(btnSave);
            buttonBar.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonBar.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            buttonBar.Margin = new System.Windows.Forms.Padding(0);
            buttonBar.Name = "buttonBar";
            buttonBar.TabIndex = 5;
            buttonBar.WrapContents = false;
            //
            // btnCancel
            //
            btnCancel.AutoSize = true;
            btnCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnCancel.BackColor = System.Drawing.Color.White;
            btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnCancel.Font = new System.Drawing.Font("Segoe UI", 10F);
            btnCancel.Margin = new System.Windows.Forms.Padding(0);
            btnCancel.MinimumSize = new System.Drawing.Size(0, 38);
            btnCancel.Name = "btnCancel";
            btnCancel.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Hủy";
            btnCancel.UseVisualStyleBackColor = false;
            //
            // btnSave
            //
            btnSave.AutoSize = true;
            btnSave.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnSave.BackColor = System.Drawing.Color.FromArgb(217, 119, 6);
            btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnSave.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnSave.ForeColor = System.Drawing.Color.White;
            btnSave.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            btnSave.MinimumSize = new System.Drawing.Size(0, 38);
            btnSave.Name = "btnSave";
            btnSave.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnSave.TabIndex = 0;
            btnSave.Text = "💾 Ghi nhận";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            //
            // RecordStorageFeeForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.White;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(600, 440);
            Controls.Add(root);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(520, 400);
            Name = "RecordStorageFeeForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Ghi nhận phí lưu kho bổ sung";
            root.ResumeLayout(false);
            root.PerformLayout();
            fields.ResumeLayout(false);
            fields.PerformLayout();
            buttonBar.ResumeLayout(false);
            buttonBar.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.TableLayoutPanel fields;
        private System.Windows.Forms.Label lblEndDate;
        private System.Windows.Forms.DateTimePicker dtEndDate;
        private System.Windows.Forms.Label lblReason;
        private System.Windows.Forms.TextBox txtReason;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.FlowLayoutPanel buttonBar;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSave;
    }
}
