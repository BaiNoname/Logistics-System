namespace TruckingSystem.Desktop.GUI_TripManagement
{
    partial class ReassignTripForm : TruckingSystem.Desktop.Common.BaseForm
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>Bố cục: root (TableLayoutPanel 1 cột) = tiêu đề / thông tin / bảng nhập liệu
        /// (nhãn | ô chọn giãn ngang) / gợi ý / trạng thái / thanh nút (phải -> trái).
        /// Không có tọa độ cứng.</summary>
        private void InitializeComponent()
        {
            root = new System.Windows.Forms.TableLayoutPanel();
            lblTitle = new System.Windows.Forms.Label();
            lblInfo = new System.Windows.Forms.Label();
            fields = new System.Windows.Forms.TableLayoutPanel();
            lblVehicle = new System.Windows.Forms.Label();
            cboVehicle = new System.Windows.Forms.ComboBox();
            lblDriver = new System.Windows.Forms.Label();
            cboDriver = new System.Windows.Forms.ComboBox();
            lblHint = new System.Windows.Forms.Label();
            lblStatus = new System.Windows.Forms.Label();
            buttonBar = new System.Windows.Forms.FlowLayoutPanel();
            btnClose = new System.Windows.Forms.Button();
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
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.Size = new System.Drawing.Size(720, 330);
            root.TabIndex = 0;
            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            lblTitle.Name = "lblTitle";
            lblTitle.TabIndex = 0;
            lblTitle.Text = "🔁 Đổi xe / tài xế";
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
            fields.AutoSize = true;
            fields.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            fields.ColumnCount = 2;
            fields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            fields.Controls.Add(lblVehicle, 0, 0);
            fields.Controls.Add(cboVehicle, 1, 0);
            fields.Controls.Add(lblDriver, 0, 1);
            fields.Controls.Add(cboDriver, 1, 1);
            fields.Dock = System.Windows.Forms.DockStyle.Fill;
            fields.Margin = new System.Windows.Forms.Padding(0);
            fields.Name = "fields";
            fields.RowCount = 2;
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.TabIndex = 2;
            //
            // lblVehicle
            //
            lblVehicle.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblVehicle.AutoSize = true;
            lblVehicle.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblVehicle.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblVehicle.Margin = new System.Windows.Forms.Padding(0, 0, 12, 10);
            lblVehicle.Name = "lblVehicle";
            lblVehicle.TabIndex = 0;
            lblVehicle.Text = "Xe *";
            //
            // cboVehicle
            //
            cboVehicle.Dock = System.Windows.Forms.DockStyle.Fill;
            cboVehicle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboVehicle.Font = new System.Drawing.Font("Segoe UI", 10F);
            cboVehicle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            cboVehicle.Name = "cboVehicle";
            cboVehicle.TabIndex = 1;
            //
            // lblDriver
            //
            lblDriver.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblDriver.AutoSize = true;
            lblDriver.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblDriver.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblDriver.Margin = new System.Windows.Forms.Padding(0, 0, 12, 10);
            lblDriver.Name = "lblDriver";
            lblDriver.TabIndex = 2;
            lblDriver.Text = "Tài xế *";
            //
            // cboDriver
            //
            cboDriver.Dock = System.Windows.Forms.DockStyle.Fill;
            cboDriver.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboDriver.Font = new System.Drawing.Font("Segoe UI", 10F);
            cboDriver.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            cboDriver.Name = "cboDriver";
            cboDriver.TabIndex = 3;
            //
            // lblHint
            //
            lblHint.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblHint.AutoSize = true;
            lblHint.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblHint.ForeColor = System.Drawing.Color.FromArgb(217, 119, 6);
            lblHint.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            lblHint.Name = "lblHint";
            lblHint.TabIndex = 3;
            //
            // lblStatus
            //
            lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblStatus.AutoSize = true;
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblStatus.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblStatus.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            lblStatus.Name = "lblStatus";
            lblStatus.TabIndex = 4;
            //
            // buttonBar
            //
            buttonBar.AutoSize = true;
            buttonBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            buttonBar.Controls.Add(btnClose);
            buttonBar.Controls.Add(btnSave);
            buttonBar.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonBar.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            buttonBar.Margin = new System.Windows.Forms.Padding(0);
            buttonBar.Name = "buttonBar";
            buttonBar.TabIndex = 5;
            buttonBar.WrapContents = true;
            //
            // btnClose
            //
            btnClose.AutoSize = true;
            btnClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnClose.BackColor = System.Drawing.Color.White;
            btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnClose.Font = new System.Drawing.Font("Segoe UI", 10F);
            btnClose.Margin = new System.Windows.Forms.Padding(0);
            btnClose.MinimumSize = new System.Drawing.Size(0, 36);
            btnClose.Name = "btnClose";
            btnClose.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            btnClose.TabIndex = 1;
            btnClose.Text = "Đóng";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            //
            // btnSave
            //
            btnSave.AutoSize = true;
            btnSave.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnSave.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            btnSave.Enabled = false;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnSave.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnSave.ForeColor = System.Drawing.Color.White;
            btnSave.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            btnSave.MinimumSize = new System.Drawing.Size(0, 36);
            btnSave.Name = "btnSave";
            btnSave.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            btnSave.TabIndex = 0;
            btnSave.Text = "💾 Đổi";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            //
            // ReassignTripForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(720, 330);
            Controls.Add(root);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(540, 340);
            Name = "ReassignTripForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Đổi xe / tài xế";
            root.ResumeLayout(false);
            root.PerformLayout();
            fields.ResumeLayout(false);
            fields.PerformLayout();
            buttonBar.ResumeLayout(false);
            buttonBar.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Label lblVehicle;
        private System.Windows.Forms.ComboBox cboVehicle;
        private System.Windows.Forms.Label lblDriver;
        private System.Windows.Forms.ComboBox cboDriver;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.TableLayoutPanel fields;
        private System.Windows.Forms.FlowLayoutPanel buttonBar;
    }
}
