namespace TruckingSystem.Desktop.GUI_EmployeeManagement
{
    partial class CreateEmployeeForm
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
            lblCode = new Label();
            txtCode = new TextBox();
            lblFullName = new Label();
            txtFullName = new TextBox();
            lblPhone = new Label();
            txtPhone = new TextBox();
            lblDept = new Label();
            cboDept = new ComboBox();
            lblPos = new Label();
            cboPos = new ComboBox();
            lblEmployeeStatus = new Label();
            cboEmployeeStatus = new ComboBox();
            btnSave = new Button();
            lblStatus = new Label();
            lblAddress = new Label();
            tableLayoutPanel1 = new TableLayoutPanel();
            txtAddress = new TextBox();
            lblIdentityCardNo = new Label();
            txtIdentityCardNo = new TextBox();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // lblCode
            // 
            lblCode.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblCode.Font = new Font("Segoe UI", 9F);
            lblCode.ForeColor = Color.FromArgb(100, 116, 139);
            lblCode.Location = new Point(13, 7);
            lblCode.Name = "lblCode";
            lblCode.Size = new Size(400, 18);
            lblCode.TabIndex = 0;
            lblCode.Text = "Mã nhân viên *";
            // 
            // txtCode
            // 
            txtCode.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtCode.Location = new Point(13, 28);
            txtCode.Name = "txtCode";
            txtCode.Size = new Size(434, 25);
            txtCode.TabIndex = 0;
            // 
            // lblFullName
            // 
            lblFullName.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblFullName.Font = new Font("Segoe UI", 9F);
            lblFullName.ForeColor = Color.FromArgb(100, 116, 139);
            lblFullName.Location = new Point(13, 82);
            lblFullName.Name = "lblFullName";
            lblFullName.Size = new Size(400, 18);
            lblFullName.TabIndex = 2;
            lblFullName.Text = "Họ tên *";
            // 
            // txtFullName
            // 
            txtFullName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtFullName.Location = new Point(13, 103);
            txtFullName.Name = "txtFullName";
            txtFullName.Size = new Size(434, 25);
            txtFullName.TabIndex = 1;
            // 
            // lblPhone
            // 
            lblPhone.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblPhone.Font = new Font("Segoe UI", 9F);
            lblPhone.ForeColor = Color.FromArgb(100, 116, 139);
            lblPhone.Location = new Point(13, 232);
            lblPhone.Name = "lblPhone";
            lblPhone.Size = new Size(400, 18);
            lblPhone.TabIndex = 4;
            lblPhone.Text = "Số điện thoại";
            // 
            // txtPhone
            // 
            txtPhone.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtPhone.Location = new Point(13, 253);
            txtPhone.Name = "txtPhone";
            txtPhone.Size = new Size(434, 25);
            txtPhone.TabIndex = 3;
            // 
            // lblDept
            // 
            lblDept.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblDept.Font = new Font("Segoe UI", 9F);
            lblDept.ForeColor = Color.FromArgb(100, 116, 139);
            lblDept.Location = new Point(13, 382);
            lblDept.Name = "lblDept";
            lblDept.Size = new Size(400, 18);
            lblDept.TabIndex = 6;
            lblDept.Text = "Phòng ban";
            // 
            // cboDept
            // 
            cboDept.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboDept.DropDownStyle = ComboBoxStyle.DropDownList;
            cboDept.Location = new Point(13, 403);
            cboDept.Name = "cboDept";
            cboDept.Size = new Size(434, 25);
            cboDept.TabIndex = 5;
            // 
            // lblPos
            // 
            lblPos.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblPos.Font = new Font("Segoe UI", 9F);
            lblPos.ForeColor = Color.FromArgb(100, 116, 139);
            lblPos.Location = new Point(13, 457);
            lblPos.Name = "lblPos";
            lblPos.Size = new Size(400, 18);
            lblPos.TabIndex = 8;
            lblPos.Text = "Chức danh";
            // 
            // cboPos
            // 
            cboPos.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboPos.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPos.Location = new Point(13, 478);
            cboPos.Name = "cboPos";
            cboPos.Size = new Size(434, 25);
            cboPos.TabIndex = 6;
            // 
            // lblEmployeeStatus
            // 
            lblEmployeeStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblEmployeeStatus.Font = new Font("Segoe UI", 9F);
            lblEmployeeStatus.ForeColor = Color.FromArgb(100, 116, 139);
            lblEmployeeStatus.Location = new Point(13, 532);
            lblEmployeeStatus.Name = "lblEmployeeStatus";
            lblEmployeeStatus.Size = new Size(400, 18);
            lblEmployeeStatus.TabIndex = 10;
            lblEmployeeStatus.Text = "Trạng thái";
            // 
            // cboEmployeeStatus
            // 
            cboEmployeeStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboEmployeeStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cboEmployeeStatus.Location = new Point(13, 553);
            cboEmployeeStatus.Name = "cboEmployeeStatus";
            cboEmployeeStatus.Size = new Size(434, 25);
            cboEmployeeStatus.TabIndex = 7;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.None;
            btnSave.BackColor = Color.FromArgb(37, 99, 235);
            btnSave.Cursor = Cursors.Hand;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSave.ForeColor = Color.White;
            btnSave.Location = new Point(140, 614);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(180, 38);
            btnSave.TabIndex = 8;
            btnSave.Text = "Lưu nhân viên";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += BtnSave_Click;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(220, 38, 38);
            lblStatus.Location = new Point(13, 585);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(0, 15);
            lblStatus.TabIndex = 13;
            // 
            // lblAddress
            // 
            lblAddress.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblAddress.Font = new Font("Segoe UI", 9F);
            lblAddress.ForeColor = Color.FromArgb(100, 116, 139);
            lblAddress.Location = new Point(13, 307);
            lblAddress.Name = "lblAddress";
            lblAddress.Size = new Size(400, 18);
            lblAddress.TabIndex = 16;
            lblAddress.Text = "Địa chỉ";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(lblCode, 0, 0);
            tableLayoutPanel1.Controls.Add(btnSave, 0, 24);
            tableLayoutPanel1.Controls.Add(cboEmployeeStatus, 0, 22);
            tableLayoutPanel1.Controls.Add(lblEmployeeStatus, 0, 21);
            tableLayoutPanel1.Controls.Add(cboPos, 0, 19);
            tableLayoutPanel1.Controls.Add(lblPos, 0, 18);
            tableLayoutPanel1.Controls.Add(cboDept, 0, 16);
            tableLayoutPanel1.Controls.Add(lblDept, 0, 15);
            tableLayoutPanel1.Controls.Add(txtCode, 0, 1);
            tableLayoutPanel1.Controls.Add(lblFullName, 0, 3);
            tableLayoutPanel1.Controls.Add(txtPhone, 0, 10);
            tableLayoutPanel1.Controls.Add(lblPhone, 0, 9);
            tableLayoutPanel1.Controls.Add(txtFullName, 0, 4);
            tableLayoutPanel1.Controls.Add(txtAddress, 0, 13);
            tableLayoutPanel1.Controls.Add(lblAddress, 0, 12);
            tableLayoutPanel1.Controls.Add(lblStatus, 0, 23);
            tableLayoutPanel1.Controls.Add(lblIdentityCardNo, 0, 6);
            tableLayoutPanel1.Controls.Add(txtIdentityCardNo, 0, 7);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.Padding = new Padding(10, 0, 10, 0);
            tableLayoutPanel1.RowCount = 25;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(460, 666);
            tableLayoutPanel1.TabIndex = 18;
            // 
            // txtAddress
            // 
            txtAddress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtAddress.Location = new Point(13, 328);
            txtAddress.Name = "txtAddress";
            txtAddress.Size = new Size(434, 25);
            txtAddress.TabIndex = 4;
            // 
            // lblIdentityCardNo
            // 
            lblIdentityCardNo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblIdentityCardNo.Font = new Font("Segoe UI", 9F);
            lblIdentityCardNo.ForeColor = Color.FromArgb(100, 116, 139);
            lblIdentityCardNo.Location = new Point(13, 157);
            lblIdentityCardNo.Name = "lblIdentityCardNo";
            lblIdentityCardNo.Size = new Size(400, 18);
            lblIdentityCardNo.TabIndex = 18;
            lblIdentityCardNo.Text = "Căn cước";
            // 
            // txtIdentityCardNo
            // 
            txtIdentityCardNo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtIdentityCardNo.Location = new Point(13, 178);
            txtIdentityCardNo.Name = "txtIdentityCardNo";
            txtIdentityCardNo.Size = new Size(434, 25);
            txtIdentityCardNo.TabIndex = 2;
            // 
            // CreateEmployeeForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(460, 666);
            Controls.Add(tableLayoutPanel1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "CreateEmployeeForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Thêm nhân viên mới";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblCode;
        private System.Windows.Forms.TextBox txtCode;
        private System.Windows.Forms.Label lblFullName;
        private System.Windows.Forms.TextBox txtFullName;
        private System.Windows.Forms.Label lblPhone;
        private System.Windows.Forms.TextBox txtPhone;
        private System.Windows.Forms.Label lblDept;
        private System.Windows.Forms.ComboBox cboDept;
        private System.Windows.Forms.Label lblPos;
        private System.Windows.Forms.ComboBox cboPos;
        private System.Windows.Forms.Label lblEmployeeStatus;
        private System.Windows.Forms.ComboBox cboEmployeeStatus;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label lblStatus;
        private Label lblAddress;
        private TableLayoutPanel tableLayoutPanel1;
        private TextBox txtAddress;
        private Label lblIdentityCardNo;
        private TextBox txtIdentityCardNo;
    }
}
