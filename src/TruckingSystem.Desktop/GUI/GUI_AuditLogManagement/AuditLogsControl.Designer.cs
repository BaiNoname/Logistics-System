namespace TruckingSystem.Desktop.GUI_AuditLogManagement
{
    partial class AuditLogsControl
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblAction = new System.Windows.Forms.Label();
            this.cboAction = new System.Windows.Forms.ComboBox();
            this.chkFilterByDate = new System.Windows.Forms.CheckBox();
            this.dtFrom = new System.Windows.Forms.DateTimePicker();
            this.lblTo = new System.Windows.Forms.Label();
            this.dtTo = new System.Windows.Forms.DateTimePicker();
            this.btnFilter = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.grid = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            //
            // lblTitle
            //
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.AutoSize = true;
            this.lblTitle.Text = "Nhật ký hệ thống (Audit Log)";
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
            //
            // lblAction
            //
            this.lblAction.Location = new System.Drawing.Point(0, 46);
            this.lblAction.AutoSize = true;
            this.lblAction.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblAction.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblAction.Text = "Lọc theo hành động:";
            //
            // cboAction
            //
            this.cboAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAction.Location = new System.Drawing.Point(0, 66);
            this.cboAction.Size = new System.Drawing.Size(220, 25);
            //
            // chkFilterByDate
            //
            this.chkFilterByDate.Location = new System.Drawing.Point(240, 46);
            this.chkFilterByDate.Size = new System.Drawing.Size(160, 24);
            this.chkFilterByDate.Text = "Lọc theo khoảng ngày";
            //
            // dtFrom
            //
            this.dtFrom.Location = new System.Drawing.Point(240, 66);
            this.dtFrom.Size = new System.Drawing.Size(140, 25);
            this.dtFrom.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            //
            // lblTo
            //
            this.lblTo.Location = new System.Drawing.Point(388, 70);
            this.lblTo.AutoSize = true;
            this.lblTo.Text = "→";
            //
            // dtTo
            //
            this.dtTo.Location = new System.Drawing.Point(410, 66);
            this.dtTo.Size = new System.Drawing.Size(140, 25);
            this.dtTo.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            //
            // btnFilter
            //
            this.btnFilter.Location = new System.Drawing.Point(570, 64);
            this.btnFilter.Size = new System.Drawing.Size(120, 30);
            this.btnFilter.Text = "🔍 Lọc";
            this.btnFilter.BackColor = TruckingSystem.Desktop.Common.Theme.Primary;
            this.btnFilter.ForeColor = System.Drawing.Color.White;
            this.btnFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFilter.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.btnFilter.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFilter.FlatAppearance.BorderSize = 0;
            this.btnFilter.FlatAppearance.MouseOverBackColor = TruckingSystem.Desktop.Common.Theme.PrimaryDark;
            this.btnFilter.Click += new System.EventHandler(this.BtnFilter_Click);
            //
            // lblStatus
            //
            this.lblStatus.Location = new System.Drawing.Point(0, 100);
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblStatus.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            //
            // grid
            //
            this.grid.Location = new System.Drawing.Point(0, 125);
            this.grid.Size = new System.Drawing.Size(1200, 440);
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            //
            // AuditLogsControl
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColor = System.Drawing.Color.White;
            this.Padding = new System.Windows.Forms.Padding(24);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblAction);
            this.Controls.Add(this.cboAction);
            this.Controls.Add(this.chkFilterByDate);
            this.Controls.Add(this.dtFrom);
            this.Controls.Add(this.lblTo);
            this.Controls.Add(this.dtTo);
            this.Controls.Add(this.btnFilter);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.grid);
            this.Name = "AuditLogsControl";
            this.Size = new System.Drawing.Size(1200, 600);
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblAction;
        private System.Windows.Forms.ComboBox cboAction;
        private System.Windows.Forms.CheckBox chkFilterByDate;
        private System.Windows.Forms.DateTimePicker dtFrom;
        private System.Windows.Forms.Label lblTo;
        private System.Windows.Forms.DateTimePicker dtTo;
        private System.Windows.Forms.Button btnFilter;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView grid;
    }
}
