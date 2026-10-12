namespace TruckingSystem.Desktop.GUI_DriverManagement
{
    partial class PickEmployeeForDriverForm
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
            lblInfo = new System.Windows.Forms.Label();
            cboEmployee = new System.Windows.Forms.ComboBox();
            lblStatus = new System.Windows.Forms.Label();
            buttonBar = new System.Windows.Forms.FlowLayoutPanel();
            btnCancel = new System.Windows.Forms.Button();
            btnOk = new System.Windows.Forms.Button();
            root.SuspendLayout();
            scrollFields.SuspendLayout();
            fields.SuspendLayout();
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
            root.Size = new System.Drawing.Size(440, 190);
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
            fields.ColumnCount = 1;
            fields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            fields.Controls.Add(lblInfo, 0, 0);
            fields.Controls.Add(cboEmployee, 0, 1);
            fields.AutoSize = true;
            fields.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            fields.Dock = System.Windows.Forms.DockStyle.Top;
            fields.Margin = new System.Windows.Forms.Padding(0);
            fields.Name = "fields";
            fields.TabIndex = 0;
            fields.RowCount = 2;
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            // 
            // lblInfo
            // 
            lblInfo.AutoSize = true;
            lblInfo.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblInfo.Margin = new System.Windows.Forms.Padding(0, 8, 0, 2);
            lblInfo.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblInfo.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblInfo.Text = "Chọn nhân viên sẽ trở thành tài xế:";
            lblInfo.Name = "lblInfo";
            lblInfo.TabIndex = 0;
            // 
            // cboEmployee
            // 
            cboEmployee.Dock = System.Windows.Forms.DockStyle.Fill;
            cboEmployee.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            cboEmployee.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboEmployee.Name = "cboEmployee";
            cboEmployee.TabIndex = 1;
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
            buttonBar.Controls.Add(btnCancel);
            buttonBar.Controls.Add(btnOk);
            buttonBar.AutoSize = true;
            buttonBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            buttonBar.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonBar.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            buttonBar.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            buttonBar.WrapContents = true;
            buttonBar.Name = "buttonBar";
            buttonBar.TabIndex = 2;
            // 
            // btnCancel
            // 
            btnCancel.AutoSize = true;
            btnCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnCancel.BackColor = System.Drawing.Color.White;
            btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnCancel.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            btnCancel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnCancel.MinimumSize = new System.Drawing.Size(0, 36);
            btnCancel.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnCancel.Text = "Hủy";
            btnCancel.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            btnCancel.Name = "btnCancel";
            btnCancel.TabIndex = 1;
            btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // btnOk
            // 
            btnOk.AutoSize = true;
            btnOk.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnOk.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            btnOk.Cursor = System.Windows.Forms.Cursors.Hand;
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnOk.ForeColor = System.Drawing.Color.White;
            btnOk.UseVisualStyleBackColor = false;
            btnOk.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnOk.MinimumSize = new System.Drawing.Size(0, 36);
            btnOk.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnOk.Text = "Tiếp tục";
            btnOk.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            btnOk.Name = "btnOk";
            btnOk.TabIndex = 0;
            btnOk.Click += new System.EventHandler(this.BtnOk_Click);
            // 
            // PickEmployeeForDriverForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(440, 190);
            MaximizeBox = false;
            MinimumSize = new System.Drawing.Size(400, 220);
            Name = "PickEmployeeForDriverForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Thêm tài xế mới";
            Controls.Add(root);
            root.ResumeLayout(false);
            root.PerformLayout();
            scrollFields.ResumeLayout(false);
            scrollFields.PerformLayout();
            fields.ResumeLayout(false);
            fields.PerformLayout();
            buttonBar.ResumeLayout(false);
            buttonBar.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.ComboBox cboEmployee;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.Panel scrollFields;
        private System.Windows.Forms.TableLayoutPanel fields;
        private System.Windows.Forms.FlowLayoutPanel buttonBar;
    }
}
