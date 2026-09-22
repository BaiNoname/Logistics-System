namespace TruckingSystem.Desktop.GUI_VehicleManagement
{
    partial class ManageVehicleDocumentsForm
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
            this.btnAddDocument = new System.Windows.Forms.Button();
            this.btnDeleteDocument = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.grid = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            //
            // lblTitle
            //
            this.lblTitle.Location = new System.Drawing.Point(20, 16);
            this.lblTitle.AutoSize = true;
            this.lblTitle.Text = "📄 Giấy tờ xe";
            this.lblTitle.Font = TruckingSystem.Desktop.Common.Theme.FontSectionTitle;
            this.lblTitle.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
            //
            // btnAddDocument
            //
            this.btnAddDocument.Location = new System.Drawing.Point(590, 16);
            this.btnAddDocument.Size = new System.Drawing.Size(160, 38);
            this.btnAddDocument.Text = "➕ Nhập giấy tờ mới";
            this.btnAddDocument.BackColor = TruckingSystem.Desktop.Common.Theme.Primary;
            this.btnAddDocument.ForeColor = System.Drawing.Color.White;
            this.btnAddDocument.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddDocument.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.btnAddDocument.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddDocument.FlatAppearance.BorderSize = 0;
            this.btnAddDocument.FlatAppearance.MouseOverBackColor = TruckingSystem.Desktop.Common.Theme.PrimaryDark;
            this.btnAddDocument.Click += new System.EventHandler(this.BtnAddDocument_Click);
            //
            // btnDeleteDocument
            //
            this.btnDeleteDocument.Location = new System.Drawing.Point(760, 16);
            this.btnDeleteDocument.Size = new System.Drawing.Size(140, 38);
            this.btnDeleteDocument.Text = "🗑️ Xóa giấy tờ";
            this.btnDeleteDocument.BackColor = System.Drawing.Color.White;
            this.btnDeleteDocument.ForeColor = TruckingSystem.Desktop.Common.Theme.Danger;
            this.btnDeleteDocument.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteDocument.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.btnDeleteDocument.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDeleteDocument.FlatAppearance.BorderSize = 1;
            this.btnDeleteDocument.FlatAppearance.BorderColor = TruckingSystem.Desktop.Common.Theme.Danger;
            this.btnDeleteDocument.Click += new System.EventHandler(this.BtnDeleteDocument_Click);
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
            // ManageVehicleDocumentsForm
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(920, 500);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.btnAddDocument);
            this.Controls.Add(this.btnDeleteDocument);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.grid);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "ManageVehicleDocumentsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Giấy tờ xe";
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnAddDocument;
        private System.Windows.Forms.Button btnDeleteDocument;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView grid;
    }
}