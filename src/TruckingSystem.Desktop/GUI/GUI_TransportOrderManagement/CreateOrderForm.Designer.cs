namespace TruckingSystem.Desktop.GUI_TransportOrderManagement
{
    partial class CreateOrderForm
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
            this.lblCustomer = new System.Windows.Forms.Label();
            this.cboCustomer = new System.Windows.Forms.ComboBox();
            this.lblPickup = new System.Windows.Forms.Label();
            this.txtPickup = new System.Windows.Forms.TextBox();
            this.lblPickupQuick = new System.Windows.Forms.Label();
            this.cboPickupLocation = new System.Windows.Forms.ComboBox();
            this.lblDropoff = new System.Windows.Forms.Label();
            this.txtDropoff = new System.Windows.Forms.TextBox();
            this.lblDropoffQuick = new System.Windows.Forms.Label();
            this.cboDropoffLocation = new System.Windows.Forms.ComboBox();
            this.lblCommitted = new System.Windows.Forms.Label();
            this.dtCommitted = new System.Windows.Forms.DateTimePicker();
            this.chkHasCommittedDate = new System.Windows.Forms.CheckBox();
            this.lblPickupMethod = new System.Windows.Forms.Label();
            this.rbCompanyPickup = new System.Windows.Forms.RadioButton();
            this.rbSelfDelivery = new System.Windows.Forms.RadioButton();
            this.lblCargoTitle = new System.Windows.Forms.Label();
            this.btnRemoveCargoLine = new System.Windows.Forms.Button();
            this.gridCargoLines = new System.Windows.Forms.DataGridView();
            this.colCargoType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPackageCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colWeightKg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colVolumeM3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIsFragile = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colIsHazmat = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colQtyTotal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnSave = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.gridCargoLines)).BeginInit();
            this.SuspendLayout();
            //
            // lblCustomer
            //
            this.lblCustomer.Location = new System.Drawing.Point(15, 23);
            this.lblCustomer.Size = new System.Drawing.Size(130, 23);
            this.lblCustomer.Text = "Khách hàng:";
            //
            // cboCustomer
            //
            this.cboCustomer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCustomer.Location = new System.Drawing.Point(150, 20);
            this.cboCustomer.Size = new System.Drawing.Size(300, 23);
            //
            // lblPickup
            //
            this.lblPickup.Location = new System.Drawing.Point(15, 58);
            this.lblPickup.Size = new System.Drawing.Size(130, 23);
            this.lblPickup.Text = "Điểm lấy hàng:";
            //
            // txtPickup
            //
            this.txtPickup.Location = new System.Drawing.Point(150, 55);
            this.txtPickup.Size = new System.Drawing.Size(300, 23);
            //
            // lblPickupQuick
            //
            this.lblPickupQuick.Location = new System.Drawing.Point(15, 91);
            this.lblPickupQuick.Size = new System.Drawing.Size(130, 23);
            this.lblPickupQuick.Text = "🔽 chọn nhanh:";
            //
            // cboPickupLocation
            //
            this.cboPickupLocation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPickupLocation.Location = new System.Drawing.Point(150, 88);
            this.cboPickupLocation.Size = new System.Drawing.Size(300, 23);
            //
            // lblDropoff
            //
            this.lblDropoff.Location = new System.Drawing.Point(15, 126);
            this.lblDropoff.Size = new System.Drawing.Size(130, 23);
            this.lblDropoff.Text = "Điểm giao hàng:";
            //
            // txtDropoff
            //
            this.txtDropoff.Location = new System.Drawing.Point(150, 123);
            this.txtDropoff.Size = new System.Drawing.Size(300, 23);
            //
            // lblDropoffQuick
            //
            this.lblDropoffQuick.Location = new System.Drawing.Point(15, 159);
            this.lblDropoffQuick.Size = new System.Drawing.Size(130, 23);
            this.lblDropoffQuick.Text = "🔽 chọn nhanh:";
            //
            // cboDropoffLocation
            //
            this.cboDropoffLocation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDropoffLocation.Location = new System.Drawing.Point(150, 156);
            this.cboDropoffLocation.Size = new System.Drawing.Size(300, 23);
            //
            // lblCommitted
            //
            this.lblCommitted.Location = new System.Drawing.Point(15, 198);
            this.lblCommitted.Size = new System.Drawing.Size(130, 23);
            this.lblCommitted.Text = "Ngày hẹn giao:";
            //
            // dtCommitted
            //
            this.dtCommitted.Location = new System.Drawing.Point(150, 195);
            this.dtCommitted.Size = new System.Drawing.Size(200, 23);
            //
            // chkHasCommittedDate
            //
            this.chkHasCommittedDate.Location = new System.Drawing.Point(360, 198);
            this.chkHasCommittedDate.Size = new System.Drawing.Size(110, 24);
            this.chkHasCommittedDate.Text = "Có hẹn giao";
            //
            // lblPickupMethod
            //
            this.lblPickupMethod.Location = new System.Drawing.Point(15, 235);
            this.lblPickupMethod.Size = new System.Drawing.Size(130, 23);
            this.lblPickupMethod.Text = "Phương thức lấy:";
            //
            // rbCompanyPickup
            //
            this.rbCompanyPickup.Location = new System.Drawing.Point(150, 233);
            this.rbCompanyPickup.Size = new System.Drawing.Size(180, 24);
            this.rbCompanyPickup.Text = "Công ty cử xe đến lấy";
            this.rbCompanyPickup.Checked = true;
            //
            // rbSelfDelivery
            //
            this.rbSelfDelivery.Location = new System.Drawing.Point(340, 233);
            this.rbSelfDelivery.Size = new System.Drawing.Size(180, 24);
            this.rbSelfDelivery.Text = "Khách tự mang tới kho";
            //
            // lblCargoTitle
            //
            this.lblCargoTitle.Location = new System.Drawing.Point(15, 275);
            this.lblCargoTitle.AutoSize = true;
            this.lblCargoTitle.Text = "Danh sách hàng hóa (bấm dòng trống cuối bảng để thêm dòng mới):";
            this.lblCargoTitle.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            //
            // btnRemoveCargoLine
            //
            this.btnRemoveCargoLine.Location = new System.Drawing.Point(560, 270);
            this.btnRemoveCargoLine.Size = new System.Drawing.Size(140, 30);
            this.btnRemoveCargoLine.Text = "🗑 Xóa dòng đã chọn";
            this.btnRemoveCargoLine.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemoveCargoLine.BackColor = System.Drawing.Color.White;
            this.btnRemoveCargoLine.FlatAppearance.BorderSize = 1;
            //
            // gridCargoLines
            //
            this.gridCargoLines.Location = new System.Drawing.Point(15, 305);
            this.gridCargoLines.Size = new System.Drawing.Size(685, 180);
            this.gridCargoLines.AllowUserToAddRows = true;
            this.gridCargoLines.AllowUserToDeleteRows = true;
            this.gridCargoLines.RowHeadersVisible = false;
            this.gridCargoLines.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.gridCargoLines.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colCargoType, this.colPackageCount, this.colWeightKg, this.colVolumeM3,
                this.colIsFragile, this.colIsHazmat, this.colQtyTotal });
            //
            // colCargoType
            //
            this.colCargoType.DataPropertyName = "CargoType";
            this.colCargoType.HeaderText = "Loại hàng";
            this.colCargoType.Width = 160;
            //
            // colPackageCount
            //
            this.colPackageCount.DataPropertyName = "PackageCount";
            this.colPackageCount.HeaderText = "Số kiện";
            this.colPackageCount.Width = 70;
            //
            // colWeightKg
            //
            this.colWeightKg.DataPropertyName = "WeightKg";
            this.colWeightKg.HeaderText = "KL (kg)";
            this.colWeightKg.Width = 80;
            //
            // colVolumeM3
            //
            this.colVolumeM3.DataPropertyName = "VolumeM3";
            this.colVolumeM3.HeaderText = "TT (m³)";
            this.colVolumeM3.Width = 80;
            //
            // colIsFragile
            //
            this.colIsFragile.DataPropertyName = "IsFragile";
            this.colIsFragile.HeaderText = "Dễ vỡ";
            this.colIsFragile.Width = 55;
            //
            // colIsHazmat
            //
            this.colIsHazmat.DataPropertyName = "IsHazmat";
            this.colIsHazmat.HeaderText = "Nguy hiểm";
            this.colIsHazmat.Width = 75;
            //
            // colQtyTotal
            //
            this.colQtyTotal.DataPropertyName = "QtyTotal";
            this.colQtyTotal.HeaderText = "SL (để chia xe)";
            this.colQtyTotal.Width = 105;
            //
            // btnSave
            //
            this.btnSave.Location = new System.Drawing.Point(15, 500);
            this.btnSave.Size = new System.Drawing.Size(180, 40);
            this.btnSave.Text = "Lưu đơn";
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
            this.lblStatus.Location = new System.Drawing.Point(210, 500);
            this.lblStatus.Size = new System.Drawing.Size(490, 40);
            this.lblStatus.ForeColor = System.Drawing.Color.Red;
            //
            // CreateOrderForm
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(720, 560);
            this.Controls.Add(this.lblCustomer);
            this.Controls.Add(this.cboCustomer);
            this.Controls.Add(this.lblPickup);
            this.Controls.Add(this.txtPickup);
            this.Controls.Add(this.lblPickupQuick);
            this.Controls.Add(this.cboPickupLocation);
            this.Controls.Add(this.lblDropoff);
            this.Controls.Add(this.txtDropoff);
            this.Controls.Add(this.lblDropoffQuick);
            this.Controls.Add(this.cboDropoffLocation);
            this.Controls.Add(this.lblCommitted);
            this.Controls.Add(this.dtCommitted);
            this.Controls.Add(this.chkHasCommittedDate);
            this.Controls.Add(this.lblPickupMethod);
            this.Controls.Add(this.rbCompanyPickup);
            this.Controls.Add(this.rbSelfDelivery);
            this.Controls.Add(this.lblCargoTitle);
            this.Controls.Add(this.btnRemoveCargoLine);
            this.Controls.Add(this.gridCargoLines);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.lblStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "CreateOrderForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Tạo đơn vận chuyển mới";
            ((System.ComponentModel.ISupportInitialize)(this.gridCargoLines)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblCustomer;
        private System.Windows.Forms.ComboBox cboCustomer;
        private System.Windows.Forms.Label lblPickup;
        private System.Windows.Forms.TextBox txtPickup;
        private System.Windows.Forms.Label lblPickupQuick;
        private System.Windows.Forms.ComboBox cboPickupLocation;
        private System.Windows.Forms.Label lblDropoff;
        private System.Windows.Forms.TextBox txtDropoff;
        private System.Windows.Forms.Label lblDropoffQuick;
        private System.Windows.Forms.ComboBox cboDropoffLocation;
        private System.Windows.Forms.Label lblCommitted;
        private System.Windows.Forms.DateTimePicker dtCommitted;
        private System.Windows.Forms.CheckBox chkHasCommittedDate;
        private System.Windows.Forms.Label lblPickupMethod;
        private System.Windows.Forms.RadioButton rbCompanyPickup;
        private System.Windows.Forms.RadioButton rbSelfDelivery;
        private System.Windows.Forms.Label lblCargoTitle;
        private System.Windows.Forms.Button btnRemoveCargoLine;
        private System.Windows.Forms.DataGridView gridCargoLines;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCargoType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPackageCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn colWeightKg;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVolumeM3;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colIsFragile;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colIsHazmat;
        private System.Windows.Forms.DataGridViewTextBoxColumn colQtyTotal;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label lblStatus;
    }
}
