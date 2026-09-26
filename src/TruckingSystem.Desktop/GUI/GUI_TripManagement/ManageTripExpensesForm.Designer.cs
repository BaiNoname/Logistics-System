namespace TruckingSystem.Desktop.GUI_TripManagement
{
    partial class ManageTripExpensesForm
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
            this.cboType = new System.Windows.Forms.ComboBox();
            this.numAmount = new System.Windows.Forms.NumericUpDown();
            this.txtNote = new System.Windows.Forms.TextBox();
            this.btnAddExpense = new System.Windows.Forms.Button();
            this.btnDeleteExpense = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.grid = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.numAmount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            //
            // lblTitle
            //
            this.lblTitle.Location = new System.Drawing.Point(20, 16);
            this.lblTitle.AutoSize = true;
            this.lblTitle.Text = "💰 Chi phí khác (ngoài xăng dầu)";
            this.lblTitle.Font = TruckingSystem.Desktop.Common.Theme.FontSectionTitle;
            this.lblTitle.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
            //
            // cboType
            //
            this.cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboType.Location = new System.Drawing.Point(20, 56);
            this.cboType.Size = new System.Drawing.Size(150, 23);
            //
            // numAmount
            //
            this.numAmount.Location = new System.Drawing.Point(180, 56);
            this.numAmount.Size = new System.Drawing.Size(150, 23);
            this.numAmount.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            //
            // txtNote
            //
            this.txtNote.Location = new System.Drawing.Point(340, 56);
            this.txtNote.Size = new System.Drawing.Size(250, 23);
            this.txtNote.PlaceholderText = "Ghi chú (tuỳ chọn)";
            //
            // btnAddExpense
            //
            this.btnAddExpense.Location = new System.Drawing.Point(600, 55);
            this.btnAddExpense.Size = new System.Drawing.Size(140, 30);
            this.btnAddExpense.Text = "➕ Thêm";
            this.btnAddExpense.BackColor = TruckingSystem.Desktop.Common.Theme.Primary;
            this.btnAddExpense.ForeColor = System.Drawing.Color.White;
            this.btnAddExpense.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddExpense.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.btnAddExpense.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddExpense.FlatAppearance.BorderSize = 0;
            this.btnAddExpense.FlatAppearance.MouseOverBackColor = TruckingSystem.Desktop.Common.Theme.PrimaryDark;
            this.btnAddExpense.Click += new System.EventHandler(this.BtnAddExpense_Click);
            //
            // btnDeleteExpense
            //
            this.btnDeleteExpense.Location = new System.Drawing.Point(750, 55);
            this.btnDeleteExpense.Size = new System.Drawing.Size(140, 30);
            this.btnDeleteExpense.Text = "🗑️ Xóa đã chọn";
            this.btnDeleteExpense.BackColor = System.Drawing.Color.White;
            this.btnDeleteExpense.ForeColor = TruckingSystem.Desktop.Common.Theme.Danger;
            this.btnDeleteExpense.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteExpense.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.btnDeleteExpense.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDeleteExpense.FlatAppearance.BorderSize = 1;
            this.btnDeleteExpense.FlatAppearance.BorderColor = TruckingSystem.Desktop.Common.Theme.Danger;
            this.btnDeleteExpense.Click += new System.EventHandler(this.BtnDeleteExpense_Click);
            //
            // lblStatus
            //
            this.lblStatus.Location = new System.Drawing.Point(20, 96);
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblStatus.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            //
            // grid
            //
            this.grid.Location = new System.Drawing.Point(20, 120);
            this.grid.Size = new System.Drawing.Size(880, 340);
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            //
            // ManageTripExpensesForm
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(920, 500);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.cboType);
            this.Controls.Add(this.numAmount);
            this.Controls.Add(this.txtNote);
            this.Controls.Add(this.btnAddExpense);
            this.Controls.Add(this.btnDeleteExpense);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.grid);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "ManageTripExpensesForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Chi phí khác";
            ((System.ComponentModel.ISupportInitialize)(this.numAmount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.ComboBox cboType;
        private System.Windows.Forms.NumericUpDown numAmount;
        private System.Windows.Forms.TextBox txtNote;
        private System.Windows.Forms.Button btnAddExpense;
        private System.Windows.Forms.Button btnDeleteExpense;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView grid;
    }
}
