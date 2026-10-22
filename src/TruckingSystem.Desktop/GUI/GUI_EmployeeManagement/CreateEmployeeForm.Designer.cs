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
            dtpHireDate = new DateTimePicker();
            dtpTerminationDate = new DateTimePicker();
            lblIdentityCardNo = new Label();
            txtIdentityCardNo = new TextBox();
            lblHireDate = new Label();
            lblTerminationDate = new Label();
            lblDateOfBirth = new Label();
            dtpDateOfBirth = new DateTimePicker();
            lblGender = new Label();
            pnlGender = new FlowLayoutPanel();
            rdoMale = new RadioButton();
            rdoFemale = new RadioButton();
            pnlGender.SuspendLayout();
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
            txtCode.Size = new Size(417, 25);
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
            txtFullName.Size = new Size(417, 25);
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
            txtPhone.Size = new Size(417, 25);
            txtPhone.TabIndex = 5;
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
            cboDept.Size = new Size(417, 25);
            cboDept.TabIndex = 7;
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
            cboPos.Size = new Size(417, 25);
            cboPos.TabIndex = 8;
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
            cboEmployeeStatus.Size = new Size(417, 25);
            cboEmployeeStatus.TabIndex = 9;
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
            btnSave.Location = new Point(131, 736);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(180, 38);
            btnSave.TabIndex = 12;
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
            lblStatus.Location = new Point(13, 695);
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
            tableLayoutPanel1.AutoScroll = true;
            tableLayoutPanel1.AutoScrollMinSize = new Size(0, 770);
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(lblCode, 0, 0);
            tableLayoutPanel1.Controls.Add(btnSave, 0, 34);
            tableLayoutPanel1.Controls.Add(cboEmployeeStatus, 0, 28);
            tableLayoutPanel1.Controls.Add(lblEmployeeStatus, 0, 27);
            tableLayoutPanel1.Controls.Add(cboPos, 0, 25);
            tableLayoutPanel1.Controls.Add(lblPos, 0, 24);
            tableLayoutPanel1.Controls.Add(cboDept, 0, 22);
            tableLayoutPanel1.Controls.Add(lblDept, 0, 21);
            tableLayoutPanel1.Controls.Add(txtCode, 0, 1);
            tableLayoutPanel1.Controls.Add(lblFullName, 0, 3);
            tableLayoutPanel1.Controls.Add(txtPhone, 0, 16);
            tableLayoutPanel1.Controls.Add(lblPhone, 0, 15);
            tableLayoutPanel1.Controls.Add(txtFullName, 0, 4);
            tableLayoutPanel1.Controls.Add(txtAddress, 0, 19);
            tableLayoutPanel1.Controls.Add(lblAddress, 0, 18);
            tableLayoutPanel1.Controls.Add(lblHireDate, 0, 29);
            tableLayoutPanel1.Controls.Add(dtpHireDate, 0, 30);
            tableLayoutPanel1.Controls.Add(lblTerminationDate, 0, 31);
            tableLayoutPanel1.Controls.Add(dtpTerminationDate, 0, 32);
            tableLayoutPanel1.Controls.Add(lblStatus, 0, 33);
            tableLayoutPanel1.Controls.Add(lblIdentityCardNo, 0, 12);
            tableLayoutPanel1.Controls.Add(txtIdentityCardNo, 0, 13);
            tableLayoutPanel1.Controls.Add(lblDateOfBirth, 0, 6);
            tableLayoutPanel1.Controls.Add(dtpDateOfBirth, 0, 7);
            tableLayoutPanel1.Controls.Add(lblGender, 0, 9);
            tableLayoutPanel1.Controls.Add(pnlGender, 0, 10);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.Padding = new Padding(10, 0, 10, 0);
            tableLayoutPanel1.RowCount = 35;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 10F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 10F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 10F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 10F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 10F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 10F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 10F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 10F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 10F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayoutPanel1.Size = new Size(460, 666);
            tableLayoutPanel1.TabIndex = 18;
            // 
            // txtAddress
            // 
            txtAddress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtAddress.Location = new Point(13, 328);
            txtAddress.Name = "txtAddress";
            txtAddress.Size = new Size(417, 25);
            txtAddress.TabIndex = 6;
            // 
            // dtpHireDate
            //
            lblHireDate.Text = "Ngày vào làm thực tế *";
            lblHireDate.Name = "lblHireDate";
            lblHireDate.AutoSize = true;
            lblHireDate.ForeColor = Color.FromArgb(100, 116, 139);
            lblTerminationDate.Text = "Ngày nghỉ việc (bỏ chọn nếu chưa nghỉ)";
            lblTerminationDate.Name = "lblTerminationDate";
            lblTerminationDate.AutoSize = true;
            lblTerminationDate.ForeColor = Color.FromArgb(100, 116, 139);
            dtpHireDate.Format = DateTimePickerFormat.Short;
            dtpHireDate.Dock = DockStyle.Top;
            dtpHireDate.AccessibleName = "Ngày vào làm thực tế";
            dtpHireDate.Location = new Point(13, 603);
            dtpHireDate.Name = "dtpHireDate";
            dtpHireDate.Size = new Size(200, 25);
            dtpHireDate.TabIndex = 10;
            //
            // dtpTerminationDate
            //
            dtpTerminationDate.Format = DateTimePickerFormat.Short;
            dtpTerminationDate.Dock = DockStyle.Top;
            dtpTerminationDate.ShowCheckBox = true;
            dtpTerminationDate.AccessibleName = "Ngày nghỉ việc thực tế (bỏ chọn nếu chưa nghỉ)";
            dtpTerminationDate.Location = new Point(13, 663);
            dtpTerminationDate.Name = "dtpTerminationDate";
            dtpTerminationDate.Size = new Size(200, 25);
            dtpTerminationDate.TabIndex = 11;
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
            txtIdentityCardNo.Size = new Size(417, 25);
            txtIdentityCardNo.TabIndex = 4;
            // 
            // lblDateOfBirth
            //
            lblDateOfBirth.Text = "Ngày sinh (bỏ chọn nếu chưa có)";
            lblDateOfBirth.Name = "lblDateOfBirth";
            lblDateOfBirth.AutoSize = true;
            lblDateOfBirth.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblDateOfBirth.ForeColor = Color.FromArgb(100, 116, 139);
            //
            // dtpDateOfBirth
            //
            dtpDateOfBirth.Name = "dtpDateOfBirth";
            dtpDateOfBirth.Dock = DockStyle.Top;
            dtpDateOfBirth.Format = DateTimePickerFormat.Custom;
            dtpDateOfBirth.CustomFormat = "dd/MM/yyyy";
            dtpDateOfBirth.MinDate = new DateTime(1900, 1, 1);
            dtpDateOfBirth.ShowCheckBox = true;
            dtpDateOfBirth.Checked = false;
            dtpDateOfBirth.TabIndex = 2;
            dtpDateOfBirth.AccessibleName = "Ngày sinh (bỏ chọn nếu chưa có)";
            //
            // lblGender
            //
            lblGender.Text = "Giới tính";
            lblGender.Name = "lblGender";
            lblGender.AutoSize = true;
            lblGender.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblGender.ForeColor = Color.FromArgb(100, 116, 139);
            //
            // pnlGender
            //
            pnlGender.Name = "pnlGender";
            pnlGender.Dock = DockStyle.Fill;
            pnlGender.Margin = new Padding(3, 0, 3, 0);
            pnlGender.WrapContents = false;
            pnlGender.TabIndex = 3;
            pnlGender.AccessibleName = "Giới tính";
            pnlGender.AccessibleRole = AccessibleRole.Grouping;
            pnlGender.Controls.Add(rdoMale);
            pnlGender.Controls.Add(rdoFemale);
            //
            // rdoMale
            //
            rdoMale.Name = "rdoMale";
            rdoMale.Text = "Nam";
            rdoMale.AutoSize = true;
            rdoMale.Margin = new Padding(0, 0, 16, 0);
            rdoMale.Checked = true;
            rdoMale.TabIndex = 0;
            rdoMale.TabStop = true;
            rdoMale.AccessibleName = "Nam";
            //
            // rdoFemale
            //
            rdoFemale.Name = "rdoFemale";
            rdoFemale.Text = "Nữ";
            rdoFemale.AutoSize = true;
            rdoFemale.Margin = Padding.Empty;
            rdoFemale.TabIndex = 1;
            rdoFemale.AccessibleName = "Nữ";
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
            pnlGender.ResumeLayout(false);
            pnlGender.PerformLayout();
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
        private DateTimePicker dtpHireDate;
        private DateTimePicker dtpTerminationDate;
        private Label lblHireDate;
        private Label lblTerminationDate;
        private Label lblDateOfBirth;
        private DateTimePicker dtpDateOfBirth;
        private Label lblGender;
        private FlowLayoutPanel pnlGender;
        private RadioButton rdoMale;
        private RadioButton rdoFemale;
    }
}
