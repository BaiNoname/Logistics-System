namespace TruckingSystem.Desktop.GUI_TripManagement
{
    partial class ManageIncidentsForm
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
            this.btnAddIncident = new System.Windows.Forms.Button();
            this.btnDeleteIncident = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.grid = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            //
            // lblTitle
            //
            this.lblTitle.Location = new System.Drawing.Point(20, 16);
            this.lblTitle.AutoSize = true;
            this.lblTitle.Text = "⚠️ Sự cố giữa đường";
            this.lblTitle.Font = TruckingSystem.Desktop.Common.Theme.FontSectionTitle;
            this.lblTitle.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
            //
            // btnAddIncident
            //
            this.btnAddIncident.Location = new System.Drawing.Point(590, 16);
            this.btnAddIncident.Size = new System.Drawing.Size(160, 38);
            this.btnAddIncident.Text = "➕ Ghi nhận sự cố";
            this.btnAddIncident.BackColor = TruckingSystem.Desktop.Common.Theme.Primary;
            this.btnAddIncident.ForeColor = System.Drawing.Color.White;
            this.btnAddIncident.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddIncident.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.btnAddIncident.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddIncident.FlatAppearance.BorderSize = 0;
            this.btnAddIncident.FlatAppearance.MouseOverBackColor = TruckingSystem.Desktop.Common.Theme.PrimaryDark;
            this.btnAddIncident.Click += new System.EventHandler(this.BtnAddIncident_Click);
            //
            // btnDeleteIncident
            //
            this.btnDeleteIncident.Location = new System.Drawing.Point(760, 16);
            this.btnDeleteIncident.Size = new System.Drawing.Size(140, 38);
            this.btnDeleteIncident.Text = "🗑️ Xóa sự cố";
            this.btnDeleteIncident.BackColor = System.Drawing.Color.White;
            this.btnDeleteIncident.ForeColor = TruckingSystem.Desktop.Common.Theme.Danger;
            this.btnDeleteIncident.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteIncident.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.btnDeleteIncident.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDeleteIncident.FlatAppearance.BorderSize = 1;
            this.btnDeleteIncident.FlatAppearance.BorderColor = TruckingSystem.Desktop.Common.Theme.Danger;
            this.btnDeleteIncident.Click += new System.EventHandler(this.BtnDeleteIncident_Click);
            //
            // lblStatus
            //
            this.lblStatus.Location = new System.Drawing.Point(20, 56);
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblStatus.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            //
            // grid
            //
            this.grid.Location = new System.Drawing.Point(20, 80);
            this.grid.Size = new System.Drawing.Size(880, 380);
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            //
            // ManageIncidentsForm
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(920, 500);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.btnAddIncident);
            this.Controls.Add(this.btnDeleteIncident);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.grid);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "ManageIncidentsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Sự cố";
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnAddIncident;
        private System.Windows.Forms.Button btnDeleteIncident;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView grid;
    }
}
