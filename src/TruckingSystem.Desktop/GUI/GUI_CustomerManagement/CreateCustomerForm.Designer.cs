namespace TruckingSystem.Desktop.GUI_CustomerManagement
{
    partial class CreateCustomerForm
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
            lblName = new Label();
            txtName = new TextBox();
            lblPhone = new Label();
            txtPhone = new TextBox();
            lblTaxCode = new Label();
            txtTaxCode = new TextBox();
            lblType = new Label();
            cboType = new ComboBox();
            lblAddress = new Label();
            txtAddress = new TextBox();
            lblCreditLimit = new Label();
            numCreditLimit = new NumericUpDown();
            lblGoogleCalendarId = new Label();
            txtGoogleCalendarId = new TextBox();
            btnSave = new Button();
            lblStatus = new Label();
            btnReadWithAi = new Button();
            ((System.ComponentModel.ISupportInitialize)numCreditLimit).BeginInit();
            SuspendLayout();
            // 
            // lblName
            // 
            lblName.Font = new Font("Segoe UI", 9F);
            lblName.ForeColor = Color.FromArgb(100, 116, 139);
            lblName.Location = new Point(24, 20);
            lblName.Name = "lblName";
            lblName.Size = new Size(400, 18);
            lblName.TabIndex = 0;
            lblName.Text = "Tên khách hàng *";
            // 
            // txtName
            // 
            txtName.Location = new Point(24, 40);
            txtName.Name = "txtName";
            txtName.Size = new Size(400, 25);
            txtName.TabIndex = 1;
            // 
            // lblPhone
            // 
            lblPhone.Font = new Font("Segoe UI", 9F);
            lblPhone.ForeColor = Color.FromArgb(100, 116, 139);
            lblPhone.Location = new Point(24, 90);
            lblPhone.Name = "lblPhone";
            lblPhone.Size = new Size(400, 18);
            lblPhone.TabIndex = 2;
            lblPhone.Text = "Số điện thoại";
            // 
            // txtPhone
            // 
            txtPhone.Location = new Point(24, 110);
            txtPhone.Name = "txtPhone";
            txtPhone.Size = new Size(400, 25);
            txtPhone.TabIndex = 3;
            // 
            // lblTaxCode
            // 
            lblTaxCode.Font = new Font("Segoe UI", 9F);
            lblTaxCode.ForeColor = Color.FromArgb(100, 116, 139);
            lblTaxCode.Location = new Point(24, 160);
            lblTaxCode.Name = "lblTaxCode";
            lblTaxCode.Size = new Size(400, 18);
            lblTaxCode.TabIndex = 4;
            lblTaxCode.Text = "Mã số thuế (nếu có)";
            // 
            // txtTaxCode
            // 
            txtTaxCode.Location = new Point(24, 180);
            txtTaxCode.Name = "txtTaxCode";
            txtTaxCode.Size = new Size(400, 25);
            txtTaxCode.TabIndex = 5;
            // 
            // lblType
            // 
            lblType.Font = new Font("Segoe UI", 9F);
            lblType.ForeColor = Color.FromArgb(100, 116, 139);
            lblType.Location = new Point(24, 230);
            lblType.Name = "lblType";
            lblType.Size = new Size(400, 18);
            lblType.TabIndex = 6;
            lblType.Text = "Loại khách hàng";
            // 
            // cboType
            // 
            cboType.DropDownStyle = ComboBoxStyle.DropDownList;
            cboType.Location = new Point(24, 250);
            cboType.Name = "cboType";
            cboType.Size = new Size(400, 25);
            cboType.TabIndex = 7;
            // 
            // lblAddress
            // 
            lblAddress.Font = new Font("Segoe UI", 9F);
            lblAddress.ForeColor = Color.FromArgb(100, 116, 139);
            lblAddress.Location = new Point(24, 290);
            lblAddress.Name = "lblAddress";
            lblAddress.Size = new Size(400, 18);
            lblAddress.TabIndex = 8;
            lblAddress.Text = "Địa chỉ";
            // 
            // txtAddress
            // 
            txtAddress.Location = new Point(24, 310);
            txtAddress.Name = "txtAddress";
            txtAddress.Size = new Size(400, 25);
            txtAddress.TabIndex = 9;
            // 
            // lblCreditLimit
            // 
            lblCreditLimit.Font = new Font("Segoe UI", 9F);
            lblCreditLimit.ForeColor = Color.FromArgb(100, 116, 139);
            lblCreditLimit.Location = new Point(24, 350);
            lblCreditLimit.Name = "lblCreditLimit";
            lblCreditLimit.Size = new Size(400, 18);
            lblCreditLimit.TabIndex = 10;
            lblCreditLimit.Text = "Hạn mức công nợ (đ, tuỳ chọn)";
            // 
            // numCreditLimit
            // 
            numCreditLimit.Location = new Point(24, 370);
            numCreditLimit.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            numCreditLimit.Name = "numCreditLimit";
            numCreditLimit.Size = new Size(400, 25);
            numCreditLimit.TabIndex = 11;
            // 
            // lblGoogleCalendarId
            // 
            lblGoogleCalendarId.Font = new Font("Segoe UI", 9F);
            lblGoogleCalendarId.ForeColor = Color.FromArgb(100, 116, 139);
            lblGoogleCalendarId.Location = new Point(24, 410);
            lblGoogleCalendarId.Name = "lblGoogleCalendarId";
            lblGoogleCalendarId.Size = new Size(400, 18);
            lblGoogleCalendarId.TabIndex = 12;
            lblGoogleCalendarId.Text = "Google Calendar ID (tuỳ chọn — để đồng bộ lịch giao hàng)";
            // 
            // txtGoogleCalendarId
            // 
            txtGoogleCalendarId.Location = new Point(24, 430);
            txtGoogleCalendarId.Name = "txtGoogleCalendarId";
            txtGoogleCalendarId.Size = new Size(400, 25);
            txtGoogleCalendarId.TabIndex = 13;
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
            btnSave.Location = new Point(248, 510);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(150, 38);
            btnSave.TabIndex = 14;
            btnSave.Text = "Lưu khách hàng";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += BtnSave_Click;
            // 
            // lblStatus
            // 
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(220, 38, 38);
            lblStatus.Location = new Point(24, 458);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(400, 17);
            lblStatus.TabIndex = 15;
            // 
            // btnReadWithAi
            // 
            btnReadWithAi.BackColor = Color.FromArgb(37, 99, 235);
            btnReadWithAi.Cursor = Cursors.Hand;
            btnReadWithAi.FlatAppearance.BorderSize = 0;
            btnReadWithAi.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
            btnReadWithAi.FlatStyle = FlatStyle.Flat;
            btnReadWithAi.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnReadWithAi.ForeColor = Color.White;
            btnReadWithAi.Location = new Point(45, 510);
            btnReadWithAi.Name = "btnReadWithAi";
            btnReadWithAi.Size = new Size(150, 38);
            btnReadWithAi.TabIndex = 16;
            btnReadWithAi.Text = "Quét danh thiếp";
            btnReadWithAi.UseVisualStyleBackColor = false;
            btnReadWithAi.Click += btnReadWithAi_Click;
            // 
            // CreateCustomerForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(460, 560);
            Controls.Add(btnReadWithAi);
            Controls.Add(lblName);
            Controls.Add(txtName);
            Controls.Add(lblPhone);
            Controls.Add(txtPhone);
            Controls.Add(lblTaxCode);
            Controls.Add(txtTaxCode);
            Controls.Add(lblType);
            Controls.Add(cboType);
            Controls.Add(lblAddress);
            Controls.Add(txtAddress);
            Controls.Add(lblCreditLimit);
            Controls.Add(numCreditLimit);
            Controls.Add(lblGoogleCalendarId);
            Controls.Add(txtGoogleCalendarId);
            Controls.Add(btnSave);
            Controls.Add(lblStatus);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "CreateCustomerForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Thêm khách hàng mới";
            ((System.ComponentModel.ISupportInitialize)numCreditLimit).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblPhone;
        private System.Windows.Forms.TextBox txtPhone;
        private System.Windows.Forms.Label lblTaxCode;
        private System.Windows.Forms.TextBox txtTaxCode;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.ComboBox cboType;
        private System.Windows.Forms.Label lblAddress;
        private System.Windows.Forms.TextBox txtAddress;
        private System.Windows.Forms.Label lblCreditLimit;
        private System.Windows.Forms.NumericUpDown numCreditLimit;
        private System.Windows.Forms.Label lblGoogleCalendarId;
        private System.Windows.Forms.TextBox txtGoogleCalendarId;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label lblStatus;
        private Button btnReadWithAi;
    }
}
