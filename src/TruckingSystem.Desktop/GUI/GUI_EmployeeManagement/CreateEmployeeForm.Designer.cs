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
            SuspendLayout();
            // 
            // lblCode
            // 
            lblCode.Font = new Font("Segoe UI", 9F);
            lblCode.ForeColor = Color.FromArgb(100, 116, 139);
            lblCode.Location = new Point(24, 20);
            lblCode.Name = "lblCode";
            lblCode.Size = new Size(400, 18);
            lblCode.TabIndex = 0;
            lblCode.Text = "Mã nhân viên *";
            // 
            // txtCode
            // 
            txtCode.Location = new Point(24, 40);
            txtCode.Name = "txtCode";
            txtCode.Size = new Size(400, 25);
            txtCode.TabIndex = 1;
            // 
            // lblFullName
            // 
            lblFullName.Font = new Font("Segoe UI", 9F);
            lblFullName.ForeColor = Color.FromArgb(100, 116, 139);
            lblFullName.Location = new Point(24, 90);
            lblFullName.Name = "lblFullName";
            lblFullName.Size = new Size(400, 18);
            lblFullName.TabIndex = 2;
            lblFullName.Text = "Họ tên *";
            // 
            // txtFullName
            // 
            txtFullName.Location = new Point(24, 110);
            txtFullName.Name = "txtFullName";
            txtFullName.Size = new Size(400, 25);
            txtFullName.TabIndex = 3;
            // 
            // lblPhone
            // 
            lblPhone.Font = new Font("Segoe UI", 9F);
            lblPhone.ForeColor = Color.FromArgb(100, 116, 139);
            lblPhone.Location = new Point(24, 160);
            lblPhone.Name = "lblPhone";
            lblPhone.Size = new Size(400, 18);
            lblPhone.TabIndex = 4;
            lblPhone.Text = "Số điện thoại";
            // 
            // txtPhone
            // 
            txtPhone.Location = new Point(24, 180);
            txtPhone.Name = "txtPhone";
            txtPhone.Size = new Size(400, 25);
            txtPhone.TabIndex = 5;
            // 
            // lblDept
            // 
            lblDept.Font = new Font("Segoe UI", 9F);
            lblDept.ForeColor = Color.FromArgb(100, 116, 139);
            lblDept.Location = new Point(24, 230);
            lblDept.Name = "lblDept";
            lblDept.Size = new Size(400, 18);
            lblDept.TabIndex = 6;
            lblDept.Text = "Phòng ban";
            // 
            // cboDept
            // 
            cboDept.DropDownStyle = ComboBoxStyle.DropDownList;
            cboDept.Location = new Point(24, 250);
            cboDept.Name = "cboDept";
            cboDept.Size = new Size(400, 25);
            cboDept.TabIndex = 7;
            // 
            // lblPos
            // 
            lblPos.Font = new Font("Segoe UI", 9F);
            lblPos.ForeColor = Color.FromArgb(100, 116, 139);
            lblPos.Location = new Point(24, 300);
            lblPos.Name = "lblPos";
            lblPos.Size = new Size(400, 18);
            lblPos.TabIndex = 8;
            lblPos.Text = "Chức danh";
            // 
            // cboPos
            // 
            cboPos.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPos.Location = new Point(24, 320);
            cboPos.Name = "cboPos";
            cboPos.Size = new Size(400, 25);
            cboPos.TabIndex = 9;
            // 
            // lblEmployeeStatus
            // 
            lblEmployeeStatus.Font = new Font("Segoe UI", 9F);
            lblEmployeeStatus.ForeColor = Color.FromArgb(100, 116, 139);
            lblEmployeeStatus.Location = new Point(24, 370);
            lblEmployeeStatus.Name = "lblEmployeeStatus";
            lblEmployeeStatus.Size = new Size(400, 18);
            lblEmployeeStatus.TabIndex = 10;
            lblEmployeeStatus.Text = "Trạng thái";
            // 
            // cboEmployeeStatus
            // 
            cboEmployeeStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cboEmployeeStatus.Location = new Point(24, 390);
            cboEmployeeStatus.Name = "cboEmployeeStatus";
            cboEmployeeStatus.Size = new Size(400, 25);
            cboEmployeeStatus.TabIndex = 11;
            // 
            // btnSave
            // 
            btnSave.BackColor = Color.FromArgb(37, 99, 235);
            btnSave.Cursor = Cursors.Hand;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSave.ForeColor = Color.White;
            btnSave.Location = new Point(244, 450);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(180, 38);
            btnSave.TabIndex = 12;
            btnSave.Text = "Lưu nhân viên";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += BtnSave_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(220, 38, 38);
            lblStatus.Location = new Point(24, 418);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(0, 15);
            lblStatus.TabIndex = 13;
            // 
            // CreateEmployeeForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(460, 530);
            Controls.Add(lblCode);
            Controls.Add(txtCode);
            Controls.Add(lblFullName);
            Controls.Add(txtFullName);
            Controls.Add(lblPhone);
            Controls.Add(txtPhone);
            Controls.Add(lblDept);
            Controls.Add(cboDept);
            Controls.Add(lblPos);
            Controls.Add(cboPos);
            Controls.Add(lblEmployeeStatus);
            Controls.Add(cboEmployeeStatus);
            Controls.Add(btnSave);
            Controls.Add(lblStatus);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "CreateEmployeeForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Thêm nhân viên mới";
            ResumeLayout(false);
            PerformLayout();
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
    }
}
