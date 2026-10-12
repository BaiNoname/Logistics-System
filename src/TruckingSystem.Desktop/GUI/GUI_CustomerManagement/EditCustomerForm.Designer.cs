namespace TruckingSystem.Desktop.GUI_CustomerManagement
{
    partial class EditCustomerForm
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
            root = new TableLayoutPanel();
            scrollFields = new Panel();
            fields = new TableLayoutPanel();
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
            lblStatus = new Label();
            buttonBar = new FlowLayoutPanel();
            btnSave = new Button();
            root.SuspendLayout();
            scrollFields.SuspendLayout();
            fields.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCreditLimit).BeginInit();
            buttonBar.SuspendLayout();
            SuspendLayout();
            // 
            // root
            // 
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            root.Controls.Add(scrollFields, 0, 0);
            root.Controls.Add(lblStatus, 0, 1);
            root.Controls.Add(buttonBar, 0, 2);
            root.Dock = DockStyle.Fill;
            root.Location = new Point(0, 0);
            root.Name = "root";
            root.Padding = new Padding(20, 16, 20, 16);
            root.RowCount = 3;
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle());
            root.RowStyles.Add(new RowStyle());
            root.Size = new Size(480, 540);
            root.TabIndex = 0;
            // 
            // scrollFields
            // 
            scrollFields.AutoScroll = true;
            scrollFields.Controls.Add(fields);
            scrollFields.Dock = DockStyle.Fill;
            scrollFields.Location = new Point(20, 16);
            scrollFields.Margin = new Padding(0);
            scrollFields.Name = "scrollFields";
            scrollFields.Size = new Size(440, 436);
            scrollFields.TabIndex = 0;
            // 
            // fields
            // 
            fields.AutoSize = true;
            fields.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            fields.ColumnCount = 1;
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            fields.Controls.Add(lblName, 0, 0);
            fields.Controls.Add(txtName, 0, 1);
            fields.Controls.Add(lblPhone, 0, 2);
            fields.Controls.Add(txtPhone, 0, 3);
            fields.Controls.Add(lblTaxCode, 0, 4);
            fields.Controls.Add(txtTaxCode, 0, 5);
            fields.Controls.Add(lblType, 0, 6);
            fields.Controls.Add(cboType, 0, 7);
            fields.Controls.Add(lblAddress, 0, 8);
            fields.Controls.Add(txtAddress, 0, 9);
            fields.Controls.Add(lblCreditLimit, 0, 10);
            fields.Controls.Add(numCreditLimit, 0, 11);
            fields.Controls.Add(lblGoogleCalendarId, 0, 12);
            fields.Controls.Add(txtGoogleCalendarId, 0, 13);
            fields.Dock = DockStyle.Top;
            fields.Location = new Point(0, 0);
            fields.Margin = new Padding(0);
            fields.Name = "fields";
            fields.RowCount = 14;
            fields.RowStyles.Add(new RowStyle());
            fields.RowStyles.Add(new RowStyle());
            fields.RowStyles.Add(new RowStyle());
            fields.RowStyles.Add(new RowStyle());
            fields.RowStyles.Add(new RowStyle());
            fields.RowStyles.Add(new RowStyle());
            fields.RowStyles.Add(new RowStyle());
            fields.RowStyles.Add(new RowStyle());
            fields.RowStyles.Add(new RowStyle());
            fields.RowStyles.Add(new RowStyle());
            fields.RowStyles.Add(new RowStyle());
            fields.RowStyles.Add(new RowStyle());
            fields.RowStyles.Add(new RowStyle());
            fields.RowStyles.Add(new RowStyle());
            fields.Size = new Size(440, 390);
            fields.TabIndex = 0;
            // 
            // lblName
            // 
            lblName.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblName.AutoSize = true;
            lblName.Font = new Font("Segoe UI", 9F);
            lblName.ForeColor = Color.FromArgb(100, 116, 139);
            lblName.Location = new Point(0, 8);
            lblName.Margin = new Padding(0, 8, 0, 2);
            lblName.Name = "lblName";
            lblName.Size = new Size(440, 15);
            lblName.TabIndex = 0;
            lblName.Text = "Tên khách hàng *";
            // 
            // txtName
            // 
            txtName.Dock = DockStyle.Fill;
            txtName.Location = new Point(0, 28);
            txtName.Margin = new Padding(0, 3, 0, 3);
            txtName.Name = "txtName";
            txtName.Size = new Size(440, 25);
            txtName.TabIndex = 1;
            // 
            // lblPhone
            // 
            lblPhone.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblPhone.AutoSize = true;
            lblPhone.Font = new Font("Segoe UI", 9F);
            lblPhone.ForeColor = Color.FromArgb(100, 116, 139);
            lblPhone.Location = new Point(0, 64);
            lblPhone.Margin = new Padding(0, 8, 0, 2);
            lblPhone.Name = "lblPhone";
            lblPhone.Size = new Size(440, 15);
            lblPhone.TabIndex = 2;
            lblPhone.Text = "Số điện thoại";
            // 
            // txtPhone
            // 
            txtPhone.Dock = DockStyle.Fill;
            txtPhone.Location = new Point(0, 84);
            txtPhone.Margin = new Padding(0, 3, 0, 3);
            txtPhone.Name = "txtPhone";
            txtPhone.Size = new Size(440, 25);
            txtPhone.TabIndex = 3;
            // 
            // lblTaxCode
            // 
            lblTaxCode.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblTaxCode.AutoSize = true;
            lblTaxCode.Font = new Font("Segoe UI", 9F);
            lblTaxCode.ForeColor = Color.FromArgb(100, 116, 139);
            lblTaxCode.Location = new Point(0, 120);
            lblTaxCode.Margin = new Padding(0, 8, 0, 2);
            lblTaxCode.Name = "lblTaxCode";
            lblTaxCode.Size = new Size(440, 15);
            lblTaxCode.TabIndex = 4;
            lblTaxCode.Text = "Mã số thuế (nếu có)";
            // 
            // txtTaxCode
            // 
            txtTaxCode.Dock = DockStyle.Fill;
            txtTaxCode.Location = new Point(0, 140);
            txtTaxCode.Margin = new Padding(0, 3, 0, 3);
            txtTaxCode.Name = "txtTaxCode";
            txtTaxCode.Size = new Size(440, 25);
            txtTaxCode.TabIndex = 5;
            // 
            // lblType
            // 
            lblType.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblType.AutoSize = true;
            lblType.Font = new Font("Segoe UI", 9F);
            lblType.ForeColor = Color.FromArgb(100, 116, 139);
            lblType.Location = new Point(0, 176);
            lblType.Margin = new Padding(0, 8, 0, 2);
            lblType.Name = "lblType";
            lblType.Size = new Size(440, 15);
            lblType.TabIndex = 6;
            lblType.Text = "Loại khách hàng";
            // 
            // cboType
            // 
            cboType.Dock = DockStyle.Fill;
            cboType.DropDownStyle = ComboBoxStyle.DropDownList;
            cboType.Location = new Point(0, 196);
            cboType.Margin = new Padding(0, 3, 0, 3);
            cboType.Name = "cboType";
            cboType.Size = new Size(440, 25);
            cboType.TabIndex = 7;
            // 
            // lblAddress
            // 
            lblAddress.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblAddress.AutoSize = true;
            lblAddress.Font = new Font("Segoe UI", 9F);
            lblAddress.ForeColor = Color.FromArgb(100, 116, 139);
            lblAddress.Location = new Point(0, 230);
            lblAddress.Margin = new Padding(0, 8, 0, 2);
            lblAddress.Name = "lblAddress";
            lblAddress.Size = new Size(440, 15);
            lblAddress.TabIndex = 8;
            lblAddress.Text = "Địa chỉ";
            // 
            // txtAddress
            // 
            txtAddress.Dock = DockStyle.Fill;
            txtAddress.Location = new Point(0, 250);
            txtAddress.Margin = new Padding(0, 3, 0, 3);
            txtAddress.Name = "txtAddress";
            txtAddress.Size = new Size(440, 25);
            txtAddress.TabIndex = 9;
            // 
            // lblCreditLimit
            // 
            lblCreditLimit.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblCreditLimit.AutoSize = true;
            lblCreditLimit.Font = new Font("Segoe UI", 9F);
            lblCreditLimit.ForeColor = Color.FromArgb(100, 116, 139);
            lblCreditLimit.Location = new Point(0, 286);
            lblCreditLimit.Margin = new Padding(0, 8, 0, 2);
            lblCreditLimit.Name = "lblCreditLimit";
            lblCreditLimit.Size = new Size(440, 15);
            lblCreditLimit.TabIndex = 10;
            lblCreditLimit.Text = "Hạn mức công nợ (đ, tuỳ chọn)";
            // 
            // numCreditLimit
            // 
            numCreditLimit.Dock = DockStyle.Fill;
            numCreditLimit.Location = new Point(0, 306);
            numCreditLimit.Margin = new Padding(0, 3, 0, 3);
            numCreditLimit.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            numCreditLimit.Name = "numCreditLimit";
            numCreditLimit.Size = new Size(440, 25);
            numCreditLimit.TabIndex = 11;
            // 
            // lblGoogleCalendarId
            // 
            lblGoogleCalendarId.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblGoogleCalendarId.AutoSize = true;
            lblGoogleCalendarId.Font = new Font("Segoe UI", 9F);
            lblGoogleCalendarId.ForeColor = Color.FromArgb(100, 116, 139);
            lblGoogleCalendarId.Location = new Point(0, 342);
            lblGoogleCalendarId.Margin = new Padding(0, 8, 0, 2);
            lblGoogleCalendarId.Name = "lblGoogleCalendarId";
            lblGoogleCalendarId.Size = new Size(440, 15);
            lblGoogleCalendarId.TabIndex = 12;
            lblGoogleCalendarId.Text = "Google Calendar ID (tuỳ chọn — để đồng bộ lịch giao hàng)";
            // 
            // txtGoogleCalendarId
            // 
            txtGoogleCalendarId.Dock = DockStyle.Fill;
            txtGoogleCalendarId.Location = new Point(0, 362);
            txtGoogleCalendarId.Margin = new Padding(0, 3, 0, 3);
            txtGoogleCalendarId.Name = "txtGoogleCalendarId";
            txtGoogleCalendarId.Size = new Size(440, 25);
            txtGoogleCalendarId.TabIndex = 13;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(220, 38, 38);
            lblStatus.Location = new Point(20, 460);
            lblStatus.Margin = new Padding(0, 8, 0, 4);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(440, 15);
            lblStatus.TabIndex = 1;
            // 
            // buttonBar
            // 
            buttonBar.AutoSize = true;
            buttonBar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonBar.Controls.Add(btnSave);
            buttonBar.Dock = DockStyle.Fill;
            buttonBar.FlowDirection = FlowDirection.RightToLeft;
            buttonBar.Location = new Point(20, 487);
            buttonBar.Margin = new Padding(0, 8, 0, 0);
            buttonBar.Name = "buttonBar";
            buttonBar.Size = new Size(440, 37);
            buttonBar.TabIndex = 2;
            // 
            // btnSave
            // 
            btnSave.AutoSize = true;
            btnSave.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnSave.BackColor = Color.FromArgb(37, 99, 235);
            btnSave.Cursor = Cursors.Hand;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSave.ForeColor = Color.White;
            btnSave.Location = new Point(318, 0);
            btnSave.Margin = new Padding(8, 0, 0, 0);
            btnSave.MinimumSize = new Size(0, 36);
            btnSave.Name = "btnSave";
            btnSave.Padding = new Padding(10, 4, 10, 4);
            btnSave.Size = new Size(122, 37);
            btnSave.TabIndex = 0;
            btnSave.Text = "Lưu thay đổi";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += BtnSave_Click;
            // 
            // EditCustomerForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(480, 540);
            Controls.Add(root);
            MaximizeBox = false;
            MinimumSize = new Size(420, 360);
            Name = "EditCustomerForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Sửa thông tin khách hàng";
            root.ResumeLayout(false);
            root.PerformLayout();
            scrollFields.ResumeLayout(false);
            scrollFields.PerformLayout();
            fields.ResumeLayout(false);
            fields.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCreditLimit).EndInit();
            buttonBar.ResumeLayout(false);
            buttonBar.PerformLayout();
            ResumeLayout(false);
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
        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.Panel scrollFields;
        private System.Windows.Forms.TableLayoutPanel fields;
        private System.Windows.Forms.FlowLayoutPanel buttonBar;
    }
}
