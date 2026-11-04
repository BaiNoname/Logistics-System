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
            lblCode = new Label { Text = "Mã nhân viên *:" };
            txtCode = new TextBox { Name = "txtCode" };
            lblFullName = new Label { Text = "Họ tên *:" };
            txtFullName = new TextBox { Name = "txtFullName" };
            lblPhone = new Label { Text = "Số điện thoại *:" };
            txtPhone = new TextBox { Name = "txtPhone" };
            lblDept = new Label { Text = "Phòng ban *:" };
            cboDept = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            lblPos = new Label { Text = "Chức danh *:" };
            cboPos = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            lblEmployeeStatus = new Label { Text = "Trạng thái *:" };
            cboEmployeeStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            lblAddress = new Label { Text = "Địa chỉ:" };
            txtAddress = new TextBox { Name = "txtAddress" };
            lblIdentityCardNo = new Label { Text = "Căn cước:" };
            txtIdentityCardNo = new TextBox();
            lblHireDate = new Label { Text = "Ngày vào làm thực tế *:" };
            dtpHireDate = new DateTimePicker { Format = DateTimePickerFormat.Short };
            lblTerminationDate = new Label { Text = "Ngày nghỉ việc (bỏ chọn nếu chưa nghỉ):" };
            dtpTerminationDate = new DateTimePicker { Format = DateTimePickerFormat.Short, ShowCheckBox = true };
            lblDateOfBirth = new Label { Text = "Ngày sinh (bỏ chọn nếu chưa có):" };
            dtpDateOfBirth = new DateTimePicker { Format = DateTimePickerFormat.Short, MinDate = new DateTime(1900, 1, 1), ShowCheckBox = true };
            lblGender = new Label { Text = "Giới tính *:" };
            pnlGender = new FlowLayoutPanel { AutoSize = true, WrapContents = false, AccessibleName = "Giới tính", AccessibleRole = AccessibleRole.Grouping };
            rdoMale = new RadioButton { Text = "Nam", AutoSize = true, Checked = true };
            rdoFemale = new RadioButton { Text = "Nữ", AutoSize = true };
            pnlGender.Controls.AddRange(new Control[] { rdoMale, rdoFemale });
            btnSave = new Button { Text = "Lưu nhân viên", AutoSize = true };
            btnSave.Click += BtnSave_Click;
            lblStatus = new Label { AutoSize = true, Dock = DockStyle.Fill, ForeColor = Common.Theme.Danger };
            tableLayoutPanel1 = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Padding = new Padding(20, 16, 20, 16) };
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            void Section(string text, int row)
            {
                var label = new Label { Text = text, AutoSize = true, Margin = new Padding(3, 10, 3, 6) };
                Common.FormStyle.Section(label);
                tableLayoutPanel1.Controls.Add(label, 0, row);
                tableLayoutPanel1.SetColumnSpan(label, 2);
            }
            void Field(Label label, Control input, int col, int row, int tab)
            {
                label.AutoSize = true;
                label.ForeColor = Common.Theme.TextPrimary;
                label.Margin = new Padding(3, 6, 12, 4);
                input.Dock = DockStyle.Top;
                input.TabIndex = tab;
                input.AccessibleName = label.Text.TrimEnd(':');
                input.Margin = new Padding(3, 0, 12, 6);
                tableLayoutPanel1.Controls.Add(label, col, row);
                tableLayoutPanel1.Controls.Add(input, col, row + 1);
            }
            Section("① Thông tin cá nhân", 0);
            Field(lblCode, txtCode, 0, 1, 0);
            Field(lblFullName, txtFullName, 1, 1, 1);
            Field(lblDateOfBirth, dtpDateOfBirth, 0, 3, 2);
            Field(lblGender, pnlGender, 1, 3, 3);
            Field(lblIdentityCardNo, txtIdentityCardNo, 0, 5, 4);
            Field(lblPhone, txtPhone, 1, 5, 5);
            Field(lblAddress, txtAddress, 0, 7, 6);
            tableLayoutPanel1.SetColumnSpan(lblAddress, 2);
            tableLayoutPanel1.SetColumnSpan(txtAddress, 2);
            Section("② Thông tin công việc", 9);
            Field(lblDept, cboDept, 0, 10, 7);
            Field(lblPos, cboPos, 1, 10, 8);
            Field(lblEmployeeStatus, cboEmployeeStatus, 0, 12, 9);
            Field(lblHireDate, dtpHireDate, 1, 12, 10);
            Field(lblTerminationDate, dtpTerminationDate, 0, 14, 11);
            tableLayoutPanel1.SetColumnSpan(lblTerminationDate, 2);
            tableLayoutPanel1.Controls.Add(lblStatus, 0, 16);
            tableLayoutPanel1.SetColumnSpan(lblStatus, 2);
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            scroll.Controls.Add(tableLayoutPanel1);
            Controls.Add(scroll);
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Common.Theme.CardBg;
            ClientSize = new Size(760, 670);
            MinimumSize = new Size(620, 450);
            MaximizeBox = false;
            Name = "CreateEmployeeForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Thêm nhân viên mới";
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
