namespace TruckingSystem.Desktop.GUI_TransportOrderManagement
{
    partial class OrderDetailForm
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
            this.lblHeader = new System.Windows.Forms.Label();
            this.lblInfo = new System.Windows.Forms.Label();
            this.lblTrackingLink = new System.Windows.Forms.Label();
            this.lblCargoTitle = new System.Windows.Forms.Label();
            this.gridCargo = new System.Windows.Forms.DataGridView();
            this.lblTripsTitle = new System.Windows.Forms.Label();
            this.gridTrips = new System.Windows.Forms.DataGridView();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnAllocate = new System.Windows.Forms.Button();
            this.btnViewRoute = new System.Windows.Forms.Button();
            this.lblResult = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.gridCargo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridTrips)).BeginInit();
            this.SuspendLayout();
            //
            // lblHeader
            //
            this.lblHeader.Location = new System.Drawing.Point(24, 20);
            this.lblHeader.AutoSize = true;
            this.lblHeader.Font = TruckingSystem.Desktop.Common.Theme.FontSectionTitle;
            this.lblHeader.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
            //
            // lblInfo
            //
            this.lblInfo.Location = new System.Drawing.Point(24, 50);
            this.lblInfo.AutoSize = true;
            this.lblInfo.Font = TruckingSystem.Desktop.Common.Theme.FontNormal;
            this.lblInfo.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            //
            // lblTrackingLink
            //
            this.lblTrackingLink.Location = new System.Drawing.Point(24, 75);
            this.lblTrackingLink.AutoSize = true;
            this.lblTrackingLink.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblTrackingLink.ForeColor = TruckingSystem.Desktop.Common.Theme.Primary;
            //
            // lblCargoTitle
            //
            this.lblCargoTitle.Location = new System.Drawing.Point(24, 100);
            this.lblCargoTitle.AutoSize = true;
            this.lblCargoTitle.Text = "Dòng hàng hóa";
            this.lblCargoTitle.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            //
            // gridCargo
            //
            this.gridCargo.Location = new System.Drawing.Point(24, 125);
            this.gridCargo.Size = new System.Drawing.Size(700, 130);
            this.gridCargo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            //
            // lblTripsTitle
            //
            this.lblTripsTitle.Location = new System.Drawing.Point(24, 270);
            this.lblTripsTitle.AutoSize = true;
            this.lblTripsTitle.Text = "Chuyến đã gán";
            this.lblTripsTitle.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            //
            // gridTrips
            //
            this.gridTrips.Location = new System.Drawing.Point(24, 295);
            this.gridTrips.Size = new System.Drawing.Size(700, 150);
            this.gridTrips.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            //
            // btnEdit
            //
            this.btnEdit.Location = new System.Drawing.Point(24, 460);
            this.btnEdit.Size = new System.Drawing.Size(110, 36);
            this.btnEdit.Text = "✏️ Sửa đơn";
            this.btnEdit.BackColor = System.Drawing.Color.White;
            this.btnEdit.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
            this.btnEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEdit.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.btnEdit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEdit.FlatAppearance.BorderSize = 1;
            this.btnEdit.FlatAppearance.MouseOverBackColor = TruckingSystem.Desktop.Common.Theme.GridAltRow;
            this.btnEdit.Click += new System.EventHandler(this.BtnEdit_Click);
            //
            // btnConfirm
            //
            this.btnConfirm.Location = new System.Drawing.Point(144, 460);
            this.btnConfirm.Size = new System.Drawing.Size(150, 36);
            this.btnConfirm.Text = "✅ Xác nhận đơn";
            this.btnConfirm.BackColor = TruckingSystem.Desktop.Common.Theme.Success;
            this.btnConfirm.ForeColor = System.Drawing.Color.White;
            this.btnConfirm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConfirm.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.btnConfirm.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnConfirm.FlatAppearance.BorderSize = 0;
            this.btnConfirm.Click += new System.EventHandler(this.BtnConfirm_Click);
            //
            // btnCancel
            //
            this.btnCancel.Location = new System.Drawing.Point(304, 460);
            this.btnCancel.Size = new System.Drawing.Size(110, 36);
            this.btnCancel.Text = "❌ Hủy đơn";
            this.btnCancel.BackColor = System.Drawing.Color.White;
            this.btnCancel.ForeColor = TruckingSystem.Desktop.Common.Theme.Danger;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.FlatAppearance.BorderSize = 1;
            this.btnCancel.FlatAppearance.BorderColor = TruckingSystem.Desktop.Common.Theme.Danger;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            //
            // btnAllocate
            //
            this.btnAllocate.Location = new System.Drawing.Point(24, 505);
            this.btnAllocate.Size = new System.Drawing.Size(200, 40);
            this.btnAllocate.Text = "🚚  Chia xe (Greedy)";
            this.btnAllocate.BackColor = TruckingSystem.Desktop.Common.Theme.Primary;
            this.btnAllocate.ForeColor = System.Drawing.Color.White;
            this.btnAllocate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAllocate.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.btnAllocate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAllocate.FlatAppearance.BorderSize = 0;
            this.btnAllocate.FlatAppearance.MouseOverBackColor = TruckingSystem.Desktop.Common.Theme.PrimaryDark;
            this.btnAllocate.Click += new System.EventHandler(this.BtnAllocate_Click);
            //
            // btnViewRoute
            //
            this.btnViewRoute.Location = new System.Drawing.Point(234, 505);
            this.btnViewRoute.Size = new System.Drawing.Size(180, 40);
            this.btnViewRoute.Text = "🗺️  Xem lộ trình";
            this.btnViewRoute.BackColor = System.Drawing.Color.White;
            this.btnViewRoute.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
            this.btnViewRoute.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnViewRoute.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.btnViewRoute.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnViewRoute.FlatAppearance.BorderSize = 1;
            this.btnViewRoute.FlatAppearance.MouseOverBackColor = TruckingSystem.Desktop.Common.Theme.GridAltRow;
            this.btnViewRoute.Enabled = false;
            this.btnViewRoute.Click += new System.EventHandler(this.BtnViewRoute_Click);
            //
            // lblResult
            //
            this.lblResult.Location = new System.Drawing.Point(24, 550);
            this.lblResult.Size = new System.Drawing.Size(700, 60);
            this.lblResult.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            //
            // OrderDetailForm
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(780, 660);
            this.Controls.Add(this.lblHeader);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.lblTrackingLink);
            this.Controls.Add(this.lblCargoTitle);
            this.Controls.Add(this.gridCargo);
            this.Controls.Add(this.lblTripsTitle);
            this.Controls.Add(this.gridTrips);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnAllocate);
            this.Controls.Add(this.btnViewRoute);
            this.Controls.Add(this.lblResult);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "OrderDetailForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Chi tiết đơn vận chuyển";
            ((System.ComponentModel.ISupportInitialize)(this.gridCargo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridTrips)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Label lblTrackingLink;
        private System.Windows.Forms.Label lblCargoTitle;
        private System.Windows.Forms.DataGridView gridCargo;
        private System.Windows.Forms.Label lblTripsTitle;
        private System.Windows.Forms.DataGridView gridTrips;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnAllocate;
        private System.Windows.Forms.Button btnViewRoute;
        private System.Windows.Forms.Label lblResult;
    }
}
