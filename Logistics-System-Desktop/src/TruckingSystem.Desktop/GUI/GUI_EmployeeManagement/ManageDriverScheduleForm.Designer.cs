namespace TruckingSystem.Desktop.GUI_EmployeeManagement
{
    partial class ManageDriverScheduleForm
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
            this.lblShiftDate = new System.Windows.Forms.Label();
            this.dtShiftDate = new System.Windows.Forms.DateTimePicker();
            this.lblShiftType = new System.Windows.Forms.Label();
            this.cboShiftType = new System.Windows.Forms.ComboBox();
            this.btnAddShift = new System.Windows.Forms.Button();
            this.btnDeleteShift = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.grid = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            //
            // lblTitle
            //
            this.lblTitle.Location = new System.Drawing.Point(20, 16);
            this.lblTitle.AutoSize = true;
            this.lblTitle.Text = "📅 Lịch làm việc";
            this.lblTitle.Font = TruckingSystem.Desktop.Common.Theme.FontSectionTitle;
            this.lblTitle.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
            //
            // lblShiftDate
            //
            this.lblShiftDate.Location = new System.Drawing.Point(20, 56);
            this.lblShiftDate.AutoSize = true;
            this.lblShiftDate.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblShiftDate.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblShiftDate.Text = "Ngày";
            //
            // dtShiftDate
            //
            this.dtShiftDate.Location = new System.Drawing.Point(20, 76);
            this.dtShiftDate.Size = new System.Drawing.Size(150, 23);
            this.dtShiftDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            //
            // lblShiftType
            //
            this.lblShiftType.Location = new System.Drawing.Point(190, 56);
            this.lblShiftType.AutoSize = true;
            this.lblShiftType.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblShiftType.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            this.lblShiftType.Text = "Ca làm việc";
            //
            // cboShiftType
            //
            this.cboShiftType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboShiftType.Location = new System.Drawing.Point(190, 76);
            this.cboShiftType.Size = new System.Drawing.Size(150, 23);
            //
            // btnAddShift
            //
            this.btnAddShift.Location = new System.Drawing.Point(360, 75);
            this.btnAddShift.Size = new System.Drawing.Size(150, 32);
            this.btnAddShift.Text = "➕ Thêm ca";
            this.btnAddShift.BackColor = TruckingSystem.Desktop.Common.Theme.Primary;
            this.btnAddShift.ForeColor = System.Drawing.Color.White;
            this.btnAddShift.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddShift.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.btnAddShift.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddShift.FlatAppearance.BorderSize = 0;
            this.btnAddShift.FlatAppearance.MouseOverBackColor = TruckingSystem.Desktop.Common.Theme.PrimaryDark;
            this.btnAddShift.Click += new System.EventHandler(this.BtnAddShift_Click);
            //
            // btnDeleteShift
            //
            this.btnDeleteShift.Location = new System.Drawing.Point(530, 75);
            this.btnDeleteShift.Size = new System.Drawing.Size(150, 32);
            this.btnDeleteShift.Text = "🗑️ Xóa ca đã chọn";
            this.btnDeleteShift.BackColor = System.Drawing.Color.White;
            this.btnDeleteShift.ForeColor = TruckingSystem.Desktop.Common.Theme.Danger;
            this.btnDeleteShift.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteShift.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.btnDeleteShift.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDeleteShift.FlatAppearance.BorderSize = 1;
            this.btnDeleteShift.FlatAppearance.BorderColor = TruckingSystem.Desktop.Common.Theme.Danger;
            this.btnDeleteShift.Click += new System.EventHandler(this.BtnDeleteShift_Click);
            //
            // lblStatus
            //
            this.lblStatus.Location = new System.Drawing.Point(20, 115);
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblStatus.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            //
            // grid
            //
            this.grid.Location = new System.Drawing.Point(20, 140);
            this.grid.Size = new System.Drawing.Size(880, 330);
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            //
            // ManageDriverScheduleForm
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(920, 500);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblShiftDate);
            this.Controls.Add(this.dtShiftDate);
            this.Controls.Add(this.lblShiftType);
            this.Controls.Add(this.cboShiftType);
            this.Controls.Add(this.btnAddShift);
            this.Controls.Add(this.btnDeleteShift);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.grid);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "ManageDriverScheduleForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Lịch làm việc";
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblShiftDate;
        private System.Windows.Forms.DateTimePicker dtShiftDate;
        private System.Windows.Forms.Label lblShiftType;
        private System.Windows.Forms.ComboBox cboShiftType;
        private System.Windows.Forms.Button btnAddShift;
        private System.Windows.Forms.Button btnDeleteShift;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView grid;
    }
}
