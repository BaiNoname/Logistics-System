namespace TruckingSystem.Desktop.GUI_VehicleManagement
{
    partial class CreateVehicleDocumentForm
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>Bố cục: root (TableLayoutPanel 1 cột) = vùng nhập liệu (Panel tự cuộn khi màn hình thấp) / trạng thái / thanh nút (FlowLayoutPanel phải-sang-trái). Không có tọa độ cứng — chữ to/nhỏ theo DPI vẫn không đè nhau.</summary>
        private void InitializeComponent()
        {
            root = new System.Windows.Forms.TableLayoutPanel();
            scrollFields = new System.Windows.Forms.Panel();
            fields = new System.Windows.Forms.TableLayoutPanel();
            lblDocType = new System.Windows.Forms.Label();
            cboDocType = new System.Windows.Forms.ComboBox();
            lblIssueDate = new System.Windows.Forms.Label();
            lblExpiryDate = new System.Windows.Forms.Label();
            dtIssueDate = new System.Windows.Forms.DateTimePicker();
            dtExpiryDate = new System.Windows.Forms.DateTimePicker();
            lblFileUrl = new System.Windows.Forms.Label();
            fileRow = new System.Windows.Forms.TableLayoutPanel();
            txtFileUrl = new System.Windows.Forms.TextBox();
            btnBrowseFile = new System.Windows.Forms.Button();
            lblStatus = new System.Windows.Forms.Label();
            buttonBar = new System.Windows.Forms.FlowLayoutPanel();
            btnSave = new System.Windows.Forms.Button();
            root.SuspendLayout();
            scrollFields.SuspendLayout();
            fields.SuspendLayout();
            fileRow.SuspendLayout();
            buttonBar.SuspendLayout();
            SuspendLayout();
            // 
            // root
            // 
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.Controls.Add(scrollFields, 0, 0);
            root.Controls.Add(lblStatus, 0, 1);
            root.Controls.Add(buttonBar, 0, 2);
            root.Dock = System.Windows.Forms.DockStyle.Fill;
            root.Padding = new System.Windows.Forms.Padding(20, 16, 20, 16);
            root.Size = new System.Drawing.Size(460, 300);
            root.Name = "root";
            root.TabIndex = 0;
            root.RowCount = 3;
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            // 
            // scrollFields
            // 
            scrollFields.Controls.Add(fields);
            scrollFields.AutoScroll = true;
            scrollFields.Dock = System.Windows.Forms.DockStyle.Fill;
            scrollFields.Margin = new System.Windows.Forms.Padding(0);
            scrollFields.Name = "scrollFields";
            scrollFields.TabIndex = 0;
            // 
            // fields
            // 
            fields.ColumnCount = 2;
            fields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            fields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            fields.Controls.Add(lblDocType, 0, 0);
            fields.Controls.Add(cboDocType, 0, 1);
            fields.Controls.Add(lblIssueDate, 0, 2);
            fields.Controls.Add(lblExpiryDate, 1, 2);
            fields.Controls.Add(dtIssueDate, 0, 3);
            fields.Controls.Add(dtExpiryDate, 1, 3);
            fields.Controls.Add(lblFileUrl, 0, 4);
            fields.Controls.Add(fileRow, 0, 5);
            fields.AutoSize = true;
            fields.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            fields.Dock = System.Windows.Forms.DockStyle.Top;
            fields.Margin = new System.Windows.Forms.Padding(0);
            fields.Name = "fields";
            fields.TabIndex = 0;
            fields.RowCount = 6;
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.SetColumnSpan(lblDocType, 2);
            fields.SetColumnSpan(cboDocType, 2);
            fields.SetColumnSpan(lblFileUrl, 2);
            fields.SetColumnSpan(fileRow, 2);
            // 
            // lblDocType
            // 
            lblDocType.AutoSize = true;
            lblDocType.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblDocType.Margin = new System.Windows.Forms.Padding(0, 8, 0, 2);
            lblDocType.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblDocType.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblDocType.Text = "Loại giấy tờ * (Registration=Đăng kiểm, Insurance=Bảo hiểm)";
            lblDocType.Name = "lblDocType";
            lblDocType.TabIndex = 0;
            // 
            // cboDocType
            // 
            cboDocType.Dock = System.Windows.Forms.DockStyle.Fill;
            cboDocType.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            cboDocType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboDocType.Name = "cboDocType";
            cboDocType.TabIndex = 1;
            // 
            // lblIssueDate
            // 
            lblIssueDate.AutoSize = true;
            lblIssueDate.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblIssueDate.Margin = new System.Windows.Forms.Padding(0, 8, 0, 2);
            lblIssueDate.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblIssueDate.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblIssueDate.Text = "Ngày cấp *";
            lblIssueDate.Name = "lblIssueDate";
            lblIssueDate.TabIndex = 2;
            // 
            // lblExpiryDate
            // 
            lblExpiryDate.AutoSize = true;
            lblExpiryDate.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblExpiryDate.Margin = new System.Windows.Forms.Padding(0, 8, 0, 2);
            lblExpiryDate.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblExpiryDate.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblExpiryDate.Text = "Ngày hết hạn *";
            lblExpiryDate.Name = "lblExpiryDate";
            lblExpiryDate.TabIndex = 3;
            // 
            // dtIssueDate
            // 
            dtIssueDate.Dock = System.Windows.Forms.DockStyle.Fill;
            dtIssueDate.Margin = new System.Windows.Forms.Padding(0, 3, 12, 3);
            dtIssueDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            dtIssueDate.Name = "dtIssueDate";
            dtIssueDate.TabIndex = 4;
            // 
            // dtExpiryDate
            // 
            dtExpiryDate.Dock = System.Windows.Forms.DockStyle.Fill;
            dtExpiryDate.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            dtExpiryDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            dtExpiryDate.Name = "dtExpiryDate";
            dtExpiryDate.TabIndex = 5;
            // 
            // lblFileUrl
            // 
            lblFileUrl.AutoSize = true;
            lblFileUrl.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblFileUrl.Margin = new System.Windows.Forms.Padding(0, 8, 0, 2);
            lblFileUrl.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblFileUrl.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblFileUrl.Text = "File scan giấy tờ (tuỳ chọn — PDF/JPG/PNG, tối đa 10 MB)";
            lblFileUrl.Name = "lblFileUrl";
            lblFileUrl.TabIndex = 6;
            // 
            // fileRow
            // 
            fileRow.ColumnCount = 2;
            fileRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            fileRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            fileRow.Controls.Add(txtFileUrl, 0, 0);
            fileRow.Controls.Add(btnBrowseFile, 1, 0);
            fileRow.AutoSize = true;
            fileRow.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            fileRow.Dock = System.Windows.Forms.DockStyle.Fill;
            fileRow.Margin = new System.Windows.Forms.Padding(0);
            fileRow.Name = "fileRow";
            fileRow.TabIndex = 7;
            fileRow.RowCount = 1;
            fileRow.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            // 
            // txtFileUrl
            // 
            txtFileUrl.Margin = new System.Windows.Forms.Padding(0, 3, 8, 3);
            txtFileUrl.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtFileUrl.PlaceholderText = "Chưa chọn file";
            txtFileUrl.ReadOnly = true;
            txtFileUrl.Name = "txtFileUrl";
            txtFileUrl.TabIndex = 0;
            // 
            // btnBrowseFile
            // 
            btnBrowseFile.AutoSize = true;
            btnBrowseFile.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnBrowseFile.BackColor = System.Drawing.Color.White;
            btnBrowseFile.Cursor = System.Windows.Forms.Cursors.Hand;
            btnBrowseFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnBrowseFile.MinimumSize = new System.Drawing.Size(0, 30);
            btnBrowseFile.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            btnBrowseFile.Text = "📎 Chọn file…";
            btnBrowseFile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            btnBrowseFile.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            btnBrowseFile.Name = "btnBrowseFile";
            btnBrowseFile.TabIndex = 1;
            btnBrowseFile.Click += new System.EventHandler(this.BtnBrowseFile_Click);
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblStatus.Margin = new System.Windows.Forms.Padding(0, 8, 0, 4);
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblStatus.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
            lblStatus.Name = "lblStatus";
            lblStatus.TabIndex = 1;
            // 
            // buttonBar
            // 
            buttonBar.Controls.Add(btnSave);
            buttonBar.AutoSize = true;
            buttonBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            buttonBar.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonBar.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            buttonBar.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            buttonBar.WrapContents = true;
            buttonBar.Name = "buttonBar";
            buttonBar.TabIndex = 2;
            // 
            // btnSave
            // 
            btnSave.AutoSize = true;
            btnSave.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnSave.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnSave.ForeColor = System.Drawing.Color.White;
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnSave.MinimumSize = new System.Drawing.Size(0, 36);
            btnSave.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnSave.Text = "Lưu giấy tờ";
            btnSave.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            btnSave.Name = "btnSave";
            btnSave.TabIndex = 0;
            btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // CreateVehicleDocumentForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(460, 300);
            MaximizeBox = false;
            MinimumSize = new System.Drawing.Size(420, 300);
            Name = "CreateVehicleDocumentForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Nhập giấy tờ xe mới";
            Controls.Add(root);
            root.ResumeLayout(false);
            root.PerformLayout();
            scrollFields.ResumeLayout(false);
            scrollFields.PerformLayout();
            fields.ResumeLayout(false);
            fields.PerformLayout();
            fileRow.ResumeLayout(false);
            fileRow.PerformLayout();
            buttonBar.ResumeLayout(false);
            buttonBar.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblDocType;
        private System.Windows.Forms.ComboBox cboDocType;
        private System.Windows.Forms.Label lblIssueDate;
        private System.Windows.Forms.DateTimePicker dtIssueDate;
        private System.Windows.Forms.Label lblExpiryDate;
        private System.Windows.Forms.DateTimePicker dtExpiryDate;
        private System.Windows.Forms.Label lblFileUrl;
        private System.Windows.Forms.TextBox txtFileUrl;
        private System.Windows.Forms.Button btnBrowseFile;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.Panel scrollFields;
        private System.Windows.Forms.TableLayoutPanel fields;
        private System.Windows.Forms.TableLayoutPanel fileRow;
        private System.Windows.Forms.FlowLayoutPanel buttonBar;
    }
}
