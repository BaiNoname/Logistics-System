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
            lblTitle = new Label();
            lblAction = new Label();
            cboAction = new ComboBox();
            chkFilterByDate = new CheckBox();
            dtFrom = new DateTimePicker();
            lblTo = new Label();
            dtTo = new DateTimePicker();
            btnFilter = new Button();
            lblStatus = new Label();
            grid = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(0, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(316, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Nhật ký hệ thống (Audit Log)";
            // 
            // lblAction
            // 
            lblAction.AutoSize = true;
            lblAction.Font = new Font("Segoe UI", 9F);
            lblAction.ForeColor = Color.FromArgb(100, 116, 139);
            lblAction.Location = new Point(0, 46);
            lblAction.Name = "lblAction";
            lblAction.Size = new Size(117, 15);
            lblAction.TabIndex = 1;
            lblAction.Text = "Lọc theo hành động:";
            // 
            // cboAction
            // 
            cboAction.DropDownStyle = ComboBoxStyle.DropDownList;
            cboAction.Location = new Point(0, 66);
            cboAction.Name = "cboAction";
            cboAction.Size = new Size(220, 25);
            cboAction.TabIndex = 2;
            // 
            // chkFilterByDate
            // 
            chkFilterByDate.Location = new Point(240, 41);
            chkFilterByDate.Name = "chkFilterByDate";
            chkFilterByDate.Size = new Size(160, 24);
            chkFilterByDate.TabIndex = 3;
            chkFilterByDate.Text = "Lọc theo khoảng ngày";
            // 
            // dtFrom
            // 
            dtFrom.Format = DateTimePickerFormat.Short;
            dtFrom.Location = new Point(240, 66);
            dtFrom.Name = "dtFrom";
            dtFrom.Size = new Size(140, 25);
            dtFrom.TabIndex = 4;
            // 
            // lblTo
            // 
            lblTo.AutoSize = true;
            lblTo.Location = new Point(388, 70);
            lblTo.Name = "lblTo";
            lblTo.Size = new Size(21, 19);
            lblTo.TabIndex = 5;
            lblTo.Text = "→";
            // 
            // dtTo
            // 
            dtTo.Format = DateTimePickerFormat.Short;
            dtTo.Location = new Point(410, 66);
            dtTo.Name = "dtTo";
            dtTo.Size = new Size(140, 25);
            dtTo.TabIndex = 6;
            // 
            // btnFilter
            // 
            btnFilter.BackColor = Color.FromArgb(37, 99, 235);
            btnFilter.Cursor = Cursors.Hand;
            btnFilter.FlatAppearance.BorderSize = 0;
            btnFilter.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
            btnFilter.FlatStyle = FlatStyle.Flat;
            btnFilter.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnFilter.ForeColor = Color.White;
            btnFilter.Location = new Point(570, 64);
            btnFilter.Name = "btnFilter";
            btnFilter.Size = new Size(120, 30);
            btnFilter.TabIndex = 7;
            btnFilter.Text = "🔍 Lọc";
            btnFilter.UseVisualStyleBackColor = false;
            btnFilter.Click += BtnFilter_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
            lblStatus.Location = new Point(0, 100);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(0, 15);
            lblStatus.TabIndex = 8;
            // 
            // grid
            // 
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.Location = new Point(0, 125);
            grid.Name = "grid";
            grid.Size = new Size(1200, 440);
            grid.TabIndex = 9;
            // 
            // AuditLogsControl
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            Controls.Add(lblTitle);
            Controls.Add(lblAction);
            Controls.Add(cboAction);
            Controls.Add(chkFilterByDate);
            Controls.Add(dtFrom);
            Controls.Add(lblTo);
            Controls.Add(dtTo);
            Controls.Add(btnFilter);
            Controls.Add(lblStatus);
            Controls.Add(grid);
            Name = "AuditLogsControl";
            Padding = new Padding(24);
            Size = new Size(1200, 600);
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
            PerformLayout();
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
