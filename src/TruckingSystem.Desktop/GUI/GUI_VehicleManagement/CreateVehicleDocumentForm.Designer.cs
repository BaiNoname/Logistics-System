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

        private void InitializeComponent()
        {
            this.lblDocType = new System.Windows.Forms.Label();
            this.cboDocType = new System.Windows.Forms.ComboBox();
            this.lblIssueDate = new System.Windows.Forms.Label();
            this.dtIssueDate = new System.Windows.Forms.DateTimePicker();
            this.lblExpiryDate = new System.Windows.Forms.Label();
            this.dtExpiryDate = new System.Windows.Forms.DateTimePicker();
            this.lblFileUrl = new System.Windows.Forms.Label();
            this.txtFileUrl = new System.Windows.Forms.TextBox();
            this.btnBrowseFile = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // lblDocType
            //
            this.lblDocType.Location = new System.Drawing.Point(24, 20);
            this.lblDocType.Size = new System.Drawing.Size(360, 18);
            this.lblDocType.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblDocType.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblDocType.Text = "Loại giấy tờ * (Registration=Đăng kiểm, Insurance=Bảo hiểm)";
            //
            // cboDocType
            //
            this.cboDocType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDocType.Location = new System.Drawing.Point(24, 40);
            this.cboDocType.Size = new System.Drawing.Size(360, 23);
            //
            // lblIssueDate
            //
            this.lblIssueDate.Location = new System.Drawing.Point(24, 90);
            this.lblIssueDate.Size = new System.Drawing.Size(170, 18);
            this.lblIssueDate.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblIssueDate.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblIssueDate.Text = "Ngày cấp *";
            //
            // dtIssueDate
            //
            this.dtIssueDate.Location = new System.Drawing.Point(24, 110);
            this.dtIssueDate.Size = new System.Drawing.Size(170, 23);
            this.dtIssueDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            //
            // lblExpiryDate
            //
            this.lblExpiryDate.Location = new System.Drawing.Point(214, 90);
            this.lblExpiryDate.Size = new System.Drawing.Size(170, 18);
            this.lblExpiryDate.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblExpiryDate.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblExpiryDate.Text = "Ngày hết hạn *";
            //
            // dtExpiryDate
            //
            this.dtExpiryDate.Location = new System.Drawing.Point(214, 110);
            this.dtExpiryDate.Size = new System.Drawing.Size(170, 23);
            this.dtExpiryDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            //
            // lblFileUrl
            //
            this.lblFileUrl.Location = new System.Drawing.Point(24, 160);
            this.lblFileUrl.Size = new System.Drawing.Size(360, 18);
            this.lblFileUrl.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblFileUrl.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblFileUrl.Text = "File scan giấy tờ (tuỳ chọn — PDF/JPG/PNG, tối đa 10 MB)";
            //
            // txtFileUrl
            //
            this.txtFileUrl.Location = new System.Drawing.Point(24, 180);
            this.txtFileUrl.Size = new System.Drawing.Size(250, 23);
            this.txtFileUrl.ReadOnly = true;
            this.txtFileUrl.PlaceholderText = "Chưa chọn file";
            //
            // btnBrowseFile
            //
            this.btnBrowseFile.Location = new System.Drawing.Point(280, 179);
            this.btnBrowseFile.Size = new System.Drawing.Size(104, 25);
            this.btnBrowseFile.Text = "📎 Chọn file…";
            this.btnBrowseFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowseFile.BackColor = System.Drawing.Color.White;
            this.btnBrowseFile.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBrowseFile.Click += new System.EventHandler(this.BtnBrowseFile_Click);
            //
            // btnSave
            //
            this.btnSave.Location = new System.Drawing.Point(100, 250);
            this.btnSave.Size = new System.Drawing.Size(200, 40);
            this.btnSave.Text = "Lưu giấy tờ";
            this.btnSave.BackColor = TruckingSystem.Desktop.Common.Theme.Primary;
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatAppearance.MouseOverBackColor = TruckingSystem.Desktop.Common.Theme.PrimaryDark;
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            //
            // lblStatus
            //
            this.lblStatus.Location = new System.Drawing.Point(24, 215);
            this.lblStatus.Size = new System.Drawing.Size(360, 25);
            this.lblStatus.ForeColor = TruckingSystem.Desktop.Common.Theme.Danger;
            this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            //
            // CreateVehicleDocumentForm
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(410, 320);
            this.Controls.Add(this.lblDocType);
            this.Controls.Add(this.cboDocType);
            this.Controls.Add(this.lblIssueDate);
            this.Controls.Add(this.dtIssueDate);
            this.Controls.Add(this.lblExpiryDate);
            this.Controls.Add(this.dtExpiryDate);
            this.Controls.Add(this.lblFileUrl);
            this.Controls.Add(this.txtFileUrl);
            this.Controls.Add(this.btnBrowseFile);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnSave);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "CreateVehicleDocumentForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Nhập giấy tờ xe mới";
            this.ResumeLayout(false);
            this.PerformLayout();
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
    }
}