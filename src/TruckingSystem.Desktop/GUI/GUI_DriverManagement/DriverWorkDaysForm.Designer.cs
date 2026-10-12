namespace TruckingSystem.Desktop.GUI_DriverManagement
{
    // Ghi rõ lớp cha ở cả file Designer: Designer luôn biết đây là Form.
    partial class DriverWorkDaysForm : TruckingSystem.Desktop.Common.BaseForm
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>Bố cục: root (TableLayoutPanel 1 cột) = tiêu đề / khung đăng ký (GroupBox tự co, bên trong là bảng Từ ngày | Đến ngày | nút) / ô "hiện cả ngày đã qua" / lưới (chiếm phần còn lại) / chân form (tóm tắt + nút Xóa, trạng thái + nút Đóng). Không có tọa độ cứng.</summary>
        private void InitializeComponent()
        {
            root = new System.Windows.Forms.TableLayoutPanel();
            lblTitle = new System.Windows.Forms.Label();
            grpRegister = new System.Windows.Forms.GroupBox();
            registerLayout = new System.Windows.Forms.TableLayoutPanel();
            lblFrom = new System.Windows.Forms.Label();
            lblTo = new System.Windows.Forms.Label();
            dtFrom = new System.Windows.Forms.DateTimePicker();
            dtTo = new System.Windows.Forms.DateTimePicker();
            btnRegister = new System.Windows.Forms.Button();
            lblHint = new System.Windows.Forms.Label();
            chkShowPast = new System.Windows.Forms.CheckBox();
            grid = new System.Windows.Forms.DataGridView();
            footer = new System.Windows.Forms.TableLayoutPanel();
            lblSummary = new System.Windows.Forms.Label();
            btnDelete = new System.Windows.Forms.Button();
            lblStatus = new System.Windows.Forms.Label();
            btnClose = new System.Windows.Forms.Button();
            root.SuspendLayout();
            grpRegister.SuspendLayout();
            registerLayout.SuspendLayout();
            footer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            // 
            // root
            // 
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.Controls.Add(lblTitle, 0, 0);
            root.Controls.Add(grpRegister, 0, 1);
            root.Controls.Add(chkShowPast, 0, 2);
            root.Controls.Add(grid, 0, 3);
            root.Controls.Add(footer, 0, 4);
            root.Dock = System.Windows.Forms.DockStyle.Fill;
            root.Padding = new System.Windows.Forms.Padding(20, 16, 20, 16);
            root.Size = new System.Drawing.Size(640, 600);
            root.Name = "root";
            root.TabIndex = 0;
            root.RowCount = 5;
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            // 
            // lblTitle
            // 
            lblTitle.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            lblTitle.Text = "📅 Ngày làm việc";
            lblTitle.Name = "lblTitle";
            lblTitle.TabIndex = 0;
            // 
            // grpRegister
            // 
            grpRegister.Controls.Add(registerLayout);
            grpRegister.AutoSize = true;
            grpRegister.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            grpRegister.Dock = System.Windows.Forms.DockStyle.Fill;
            grpRegister.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            grpRegister.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            grpRegister.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            grpRegister.Padding = new System.Windows.Forms.Padding(10, 6, 10, 8);
            grpRegister.Text = "Đăng ký ngày làm (không cần chọn ca, tính cả T7 – CN)";
            grpRegister.Name = "grpRegister";
            grpRegister.TabIndex = 1;
            // 
            // registerLayout
            // 
            registerLayout.ColumnCount = 3;
            registerLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            registerLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            registerLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            registerLayout.Controls.Add(lblFrom, 0, 0);
            registerLayout.Controls.Add(lblTo, 1, 0);
            registerLayout.Controls.Add(dtFrom, 0, 1);
            registerLayout.Controls.Add(dtTo, 1, 1);
            registerLayout.Controls.Add(btnRegister, 2, 1);
            registerLayout.Controls.Add(lblHint, 0, 2);
            registerLayout.AutoSize = true;
            registerLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            registerLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            registerLayout.Margin = new System.Windows.Forms.Padding(0);
            registerLayout.Name = "registerLayout";
            registerLayout.TabIndex = 0;
            registerLayout.RowCount = 3;
            registerLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            registerLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            registerLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            registerLayout.SetColumnSpan(lblHint, 3);
            // 
            // lblFrom
            // 
            lblFrom.AutoSize = true;
            lblFrom.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblFrom.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblFrom.Margin = new System.Windows.Forms.Padding(0, 4, 12, 2);
            lblFrom.Text = "Từ ngày";
            lblFrom.Name = "lblFrom";
            lblFrom.TabIndex = 0;
            // 
            // lblTo
            // 
            lblTo.AutoSize = true;
            lblTo.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblTo.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblTo.Margin = new System.Windows.Forms.Padding(0, 4, 12, 2);
            lblTo.Text = "Đến ngày";
            lblTo.Name = "lblTo";
            lblTo.TabIndex = 1;
            // 
            // dtFrom
            // 
            dtFrom.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dtFrom.CustomFormat = "dd/MM/yyyy";
            dtFrom.Font = new System.Drawing.Font("Segoe UI", 10F);
            dtFrom.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            dtFrom.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            dtFrom.Name = "dtFrom";
            dtFrom.TabIndex = 2;
            // 
            // dtTo
            // 
            dtTo.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dtTo.CustomFormat = "dd/MM/yyyy";
            dtTo.Font = new System.Drawing.Font("Segoe UI", 10F);
            dtTo.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            dtTo.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            dtTo.Name = "dtTo";
            dtTo.TabIndex = 3;
            // 
            // btnRegister
            // 
            btnRegister.AutoSize = true;
            btnRegister.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnRegister.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            btnRegister.Cursor = System.Windows.Forms.Cursors.Hand;
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            btnRegister.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnRegister.ForeColor = System.Drawing.Color.White;
            btnRegister.UseVisualStyleBackColor = false;
            btnRegister.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnRegister.MinimumSize = new System.Drawing.Size(0, 34);
            btnRegister.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            btnRegister.Text = "➕ Đăng ký";
            btnRegister.Anchor = System.Windows.Forms.AnchorStyles.Left;
            btnRegister.Margin = new System.Windows.Forms.Padding(0);
            btnRegister.Name = "btnRegister";
            btnRegister.TabIndex = 4;
            btnRegister.Click += new System.EventHandler(this.BtnRegister_Click);
            // 
            // lblHint
            // 
            lblHint.AutoSize = true;
            lblHint.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblHint.Margin = new System.Windows.Forms.Padding(0, 8, 0, 4);
            lblHint.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblHint.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblHint.Name = "lblHint";
            lblHint.TabIndex = 5;
            // 
            // chkShowPast
            // 
            chkShowPast.AutoSize = true;
            chkShowPast.Font = new System.Drawing.Font("Segoe UI", 9F);
            chkShowPast.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            chkShowPast.Margin = new System.Windows.Forms.Padding(2, 0, 0, 6);
            chkShowPast.Text = "Hiện cả ngày đã qua";
            chkShowPast.Name = "chkShowPast";
            chkShowPast.TabIndex = 2;
            // 
            // grid
            // 
            grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid.Dock = System.Windows.Forms.DockStyle.Fill;
            grid.Margin = new System.Windows.Forms.Padding(0);
            grid.Name = "grid";
            grid.TabIndex = 3;
            // 
            // footer
            // 
            footer.ColumnCount = 2;
            footer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            footer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            footer.Controls.Add(lblSummary, 0, 0);
            footer.Controls.Add(btnDelete, 1, 0);
            footer.Controls.Add(lblStatus, 0, 1);
            footer.Controls.Add(btnClose, 1, 1);
            footer.AutoSize = true;
            footer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            footer.Dock = System.Windows.Forms.DockStyle.Fill;
            footer.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            footer.Name = "footer";
            footer.TabIndex = 4;
            footer.RowCount = 2;
            footer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            footer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            // 
            // lblSummary
            // 
            lblSummary.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblSummary.AutoSize = true;
            lblSummary.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblSummary.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblSummary.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            lblSummary.Name = "lblSummary";
            lblSummary.TabIndex = 0;
            // 
            // btnDelete
            // 
            btnDelete.AutoSize = true;
            btnDelete.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnDelete.BackColor = System.Drawing.Color.White;
            btnDelete.Cursor = System.Windows.Forms.Cursors.Hand;
            btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnDelete.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
            btnDelete.Font = new System.Drawing.Font("Segoe UI", 9F);
            btnDelete.MinimumSize = new System.Drawing.Size(0, 34);
            btnDelete.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            btnDelete.Text = "🗑 Xóa ngày đã chọn";
            btnDelete.Anchor = System.Windows.Forms.AnchorStyles.Right;
            btnDelete.Margin = new System.Windows.Forms.Padding(0);
            btnDelete.Name = "btnDelete";
            btnDelete.TabIndex = 1;
            btnDelete.Click += new System.EventHandler(this.BtnDelete_Click);
            // 
            // lblStatus
            // 
            lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblStatus.AutoSize = true;
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblStatus.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
            lblStatus.Margin = new System.Windows.Forms.Padding(0, 8, 12, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.TabIndex = 2;
            // 
            // btnClose
            // 
            btnClose.AutoSize = true;
            btnClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnClose.BackColor = System.Drawing.Color.White;
            btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnClose.Font = new System.Drawing.Font("Segoe UI", 10F);
            btnClose.MinimumSize = new System.Drawing.Size(0, 34);
            btnClose.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            btnClose.Text = "Đóng";
            btnClose.Anchor = System.Windows.Forms.AnchorStyles.Right;
            btnClose.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            btnClose.Name = "btnClose";
            btnClose.TabIndex = 3;
            btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // DriverWorkDaysForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(640, 600);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(560, 480);
            Name = "DriverWorkDaysForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Ngày làm việc";
            Controls.Add(root);
            root.ResumeLayout(false);
            root.PerformLayout();
            grpRegister.ResumeLayout(false);
            grpRegister.PerformLayout();
            registerLayout.ResumeLayout(false);
            registerLayout.PerformLayout();
            footer.ResumeLayout(false);
            footer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.GroupBox grpRegister;
        private System.Windows.Forms.Label lblFrom;
        private System.Windows.Forms.DateTimePicker dtFrom;
        private System.Windows.Forms.Label lblTo;
        private System.Windows.Forms.DateTimePicker dtTo;
        private System.Windows.Forms.Button btnRegister;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.CheckBox chkShowPast;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.Label lblSummary;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.TableLayoutPanel registerLayout;
        private System.Windows.Forms.TableLayoutPanel footer;
    }
}
