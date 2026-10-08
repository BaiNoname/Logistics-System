namespace TruckingSystem.Desktop.GUI_DriverManagement
{
    // Ghi rõ lớp cha ở cả file Designer: Designer luôn biết đây là Form.
    partial class DriverWorkDaysForm : TruckingSystem.Desktop.Common.BaseForm
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.grpRegister = new System.Windows.Forms.GroupBox();
            this.lblFrom = new System.Windows.Forms.Label();
            this.dtFrom = new System.Windows.Forms.DateTimePicker();
            this.lblTo = new System.Windows.Forms.Label();
            this.dtTo = new System.Windows.Forms.DateTimePicker();
            this.btnRegister = new System.Windows.Forms.Button();
            this.lblHint = new System.Windows.Forms.Label();
            this.chkShowPast = new System.Windows.Forms.CheckBox();
            this.grid = new System.Windows.Forms.DataGridView();
            this.lblSummary = new System.Windows.Forms.Label();
            this.btnDelete = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.grpRegister.SuspendLayout();
            this.SuspendLayout();
            //
            // lblTitle
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(16, 12);
            this.lblTitle.Text = "📅 Ngày làm việc";
            this.lblTitle.Font = TruckingSystem.Desktop.Common.Theme.FontSectionTitle;
            this.lblTitle.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
            //
            // grpRegister
            //
            this.grpRegister.Location = new System.Drawing.Point(16, 50);
            this.grpRegister.Size = new System.Drawing.Size(588, 128);
            this.grpRegister.Text = "Đăng ký ngày làm (không cần chọn ca, tính cả T7 – CN)";
            this.grpRegister.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.grpRegister.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
            this.grpRegister.Controls.Add(this.lblFrom);
            this.grpRegister.Controls.Add(this.dtFrom);
            this.grpRegister.Controls.Add(this.lblTo);
            this.grpRegister.Controls.Add(this.dtTo);
            this.grpRegister.Controls.Add(this.btnRegister);
            this.grpRegister.Controls.Add(this.lblHint);
            //
            // lblFrom
            //
            this.lblFrom.AutoSize = true;
            this.lblFrom.Location = new System.Drawing.Point(14, 28);
            this.lblFrom.Text = "Từ ngày";
            this.lblFrom.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblFrom.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            //
            // dtFrom
            //
            this.dtFrom.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtFrom.CustomFormat = "dd/MM/yyyy";
            this.dtFrom.Location = new System.Drawing.Point(14, 48);
            this.dtFrom.Size = new System.Drawing.Size(140, 23);
            this.dtFrom.Font = TruckingSystem.Desktop.Common.Theme.FontNormal;
            //
            // lblTo
            //
            this.lblTo.AutoSize = true;
            this.lblTo.Location = new System.Drawing.Point(170, 28);
            this.lblTo.Text = "Đến ngày";
            this.lblTo.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblTo.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            //
            // dtTo
            //
            this.dtTo.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtTo.CustomFormat = "dd/MM/yyyy";
            this.dtTo.Location = new System.Drawing.Point(170, 48);
            this.dtTo.Size = new System.Drawing.Size(140, 23);
            this.dtTo.Font = TruckingSystem.Desktop.Common.Theme.FontNormal;
            //
            // btnRegister
            //
            this.btnRegister.Location = new System.Drawing.Point(330, 42);
            this.btnRegister.Size = new System.Drawing.Size(140, 34);
            this.btnRegister.Text = "➕ Đăng ký";
            this.btnRegister.BackColor = TruckingSystem.Desktop.Common.Theme.Primary;
            this.btnRegister.ForeColor = System.Drawing.Color.White;
            this.btnRegister.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRegister.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.btnRegister.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRegister.FlatAppearance.BorderSize = 0;
            this.btnRegister.FlatAppearance.MouseOverBackColor = TruckingSystem.Desktop.Common.Theme.PrimaryDark;
            this.btnRegister.Click += new System.EventHandler(this.BtnRegister_Click);
            //
            // lblHint
            //
            this.lblHint.Location = new System.Drawing.Point(14, 90);
            this.lblHint.Size = new System.Drawing.Size(560, 24);
            this.lblHint.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblHint.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            //
            // chkShowPast
            //
            this.chkShowPast.AutoSize = true;
            this.chkShowPast.Location = new System.Drawing.Point(18, 190);
            this.chkShowPast.Text = "Hiện cả ngày đã qua";
            this.chkShowPast.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.chkShowPast.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            //
            // grid
            //
            this.grid.Location = new System.Drawing.Point(16, 216);
            this.grid.Size = new System.Drawing.Size(588, 260);
            this.grid.Name = "grid";
            //
            // lblSummary
            //
            this.lblSummary.Location = new System.Drawing.Point(16, 486);
            this.lblSummary.Size = new System.Drawing.Size(420, 40);
            this.lblSummary.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblSummary.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
            //
            // btnDelete
            //
            this.btnDelete.Location = new System.Drawing.Point(454, 484);
            this.btnDelete.Size = new System.Drawing.Size(150, 30);
            this.btnDelete.Text = "🗑 Xóa ngày đã chọn";
            this.btnDelete.BackColor = System.Drawing.Color.White;
            this.btnDelete.ForeColor = TruckingSystem.Desktop.Common.Theme.Danger;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.btnDelete.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDelete.Click += new System.EventHandler(this.BtnDelete_Click);
            //
            // lblStatus
            //
            this.lblStatus.Location = new System.Drawing.Point(16, 534);
            this.lblStatus.Size = new System.Drawing.Size(470, 36);
            this.lblStatus.ForeColor = TruckingSystem.Desktop.Common.Theme.Danger;
            this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            //
            // btnClose
            //
            this.btnClose.Location = new System.Drawing.Point(504, 536);
            this.btnClose.Size = new System.Drawing.Size(100, 32);
            this.btnClose.Text = "Đóng";
            this.btnClose.BackColor = System.Drawing.Color.White;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = TruckingSystem.Desktop.Common.Theme.FontNormal;
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            //
            // DriverWorkDaysForm
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(620, 582);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.grpRegister);
            this.Controls.Add(this.chkShowPast);
            this.Controls.Add(this.grid);
            this.Controls.Add(this.lblSummary);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DriverWorkDaysForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Ngày làm việc";
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.grpRegister.ResumeLayout(false);
            this.grpRegister.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.GroupBox grpRegister;
        private System.Windows.Forms.Label lblFrom;
        private System.Windows.Forms.DateTimePicker dtFrom;
        private System.Windows.Forms.Label lblTo;
        private System.Windows.Forms.DateTimePicker dtTo;
        private System.Windows.Forms.Button btnRegister;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.CheckBox chkShowPast;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.Label lblSummary;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnClose;
    }
}
