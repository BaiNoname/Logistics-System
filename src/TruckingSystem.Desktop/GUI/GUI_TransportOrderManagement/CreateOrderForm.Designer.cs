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
            lblCustomer = new Label();
            cboCustomer = new ComboBox();
            lblPickup = new Label();
            txtPickup = new TextBox();
            lblPickupQuick = new Label();
            cboPickupLocation = new ComboBox();
            lblDropoff = new Label();
            txtDropoff = new TextBox();
            lblDropoffQuick = new Label();
            cboDropoffLocation = new ComboBox();
            lblCommitted = new Label();
            dtCommitted = new DateTimePicker();
            chkHasCommittedDate = new CheckBox();
            lblPickupMethod = new Label();
            rbCompanyPickup = new RadioButton();
            rbSelfDelivery = new RadioButton();
            lblWarehouse = new Label();
            cboWarehouse = new ComboBox();
            lblCargoTitle = new Label();
            btnRemoveCargoLine = new Button();
            gridCargoLines = new DataGridView();
            colCargoType = new DataGridViewTextBoxColumn();
            colPackageCount = new DataGridViewTextBoxColumn();
            colWeightKg = new DataGridViewTextBoxColumn();
            colVolumeM3 = new DataGridViewTextBoxColumn();
            colIsFragile = new DataGridViewCheckBoxColumn();
            colIsHazmat = new DataGridViewCheckBoxColumn();
            colQtyTotal = new DataGridViewTextBoxColumn();
            btnSave = new Button();
            lblStatus = new Label();
            ((System.ComponentModel.ISupportInitialize)gridCargoLines).BeginInit();
            SuspendLayout();
            // 
            // lblCustomer
            // 
            lblCustomer.Location = new Point(22, 34);
            lblCustomer.Margin = new Padding(4, 0, 4, 0);
            lblCustomer.Name = "lblCustomer";
            lblCustomer.Size = new Size(195, 34);
            lblCustomer.TabIndex = 0;
            lblCustomer.Text = "Khách hàng:";
            // 
            // cboCustomer
            // 
            cboCustomer.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCustomer.Location = new Point(225, 30);
            cboCustomer.Margin = new Padding(4, 4, 4, 4);
            cboCustomer.Name = "cboCustomer";
            cboCustomer.Size = new Size(448, 36);
            cboCustomer.TabIndex = 1;
            // 
            // lblPickup
            // 
            lblPickup.Location = new Point(22, 87);
            lblPickup.Margin = new Padding(4, 0, 4, 0);
            lblPickup.Name = "lblPickup";
            lblPickup.Size = new Size(195, 34);
            lblPickup.TabIndex = 2;
            lblPickup.Text = "Điểm lấy hàng:";
            // 
            // txtPickup
            // 
            txtPickup.Location = new Point(225, 82);
            txtPickup.Margin = new Padding(4, 4, 4, 4);
            txtPickup.Name = "txtPickup";
            txtPickup.Size = new Size(448, 34);
            txtPickup.TabIndex = 3;
            // 
            // lblPickupQuick
            // 
            lblPickupQuick.Location = new Point(22, 136);
            lblPickupQuick.Margin = new Padding(4, 0, 4, 0);
            lblPickupQuick.Name = "lblPickupQuick";
            lblPickupQuick.Size = new Size(195, 34);
            lblPickupQuick.TabIndex = 4;
            lblPickupQuick.Text = "🔽 chọn nhanh:";
            // 
            // cboPickupLocation
            // 
            cboPickupLocation.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPickupLocation.Location = new Point(225, 132);
            cboPickupLocation.Margin = new Padding(4, 4, 4, 4);
            cboPickupLocation.Name = "cboPickupLocation";
            cboPickupLocation.Size = new Size(448, 36);
            cboPickupLocation.TabIndex = 5;
            // 
            // lblDropoff
            // 
            lblDropoff.Location = new Point(22, 189);
            lblDropoff.Margin = new Padding(4, 0, 4, 0);
            lblDropoff.Name = "lblDropoff";
            lblDropoff.Size = new Size(195, 34);
            lblDropoff.TabIndex = 6;
            lblDropoff.Text = "Điểm giao hàng:";
            // 
            // txtDropoff
            // 
            txtDropoff.Location = new Point(225, 184);
            txtDropoff.Margin = new Padding(4, 4, 4, 4);
            txtDropoff.Name = "txtDropoff";
            txtDropoff.Size = new Size(448, 34);
            txtDropoff.TabIndex = 7;
            // 
            // lblDropoffQuick
            // 
            lblDropoffQuick.Location = new Point(22, 238);
            lblDropoffQuick.Margin = new Padding(4, 0, 4, 0);
            lblDropoffQuick.Name = "lblDropoffQuick";
            lblDropoffQuick.Size = new Size(195, 34);
            lblDropoffQuick.TabIndex = 8;
            lblDropoffQuick.Text = "🔽 chọn nhanh:";
            // 
            // cboDropoffLocation
            // 
            cboDropoffLocation.DropDownStyle = ComboBoxStyle.DropDownList;
            cboDropoffLocation.Location = new Point(225, 234);
            cboDropoffLocation.Margin = new Padding(4, 4, 4, 4);
            cboDropoffLocation.Name = "cboDropoffLocation";
            cboDropoffLocation.Size = new Size(448, 36);
            cboDropoffLocation.TabIndex = 9;
            // 
            // lblCommitted
            // 
            lblCommitted.Location = new Point(22, 297);
            lblCommitted.Margin = new Padding(4, 0, 4, 0);
            lblCommitted.Name = "lblCommitted";
            lblCommitted.Size = new Size(195, 34);
            lblCommitted.TabIndex = 10;
            lblCommitted.Text = "Ngày hẹn giao:";
            // 
            // dtCommitted
            // 
            dtCommitted.Location = new Point(225, 292);
            dtCommitted.Margin = new Padding(4, 4, 4, 4);
            dtCommitted.Name = "dtCommitted";
            dtCommitted.Size = new Size(326, 34);
            dtCommitted.TabIndex = 11;
            // 
            // chkHasCommittedDate
            // 
            chkHasCommittedDate.Location = new Point(574, 290);
            chkHasCommittedDate.Margin = new Padding(4, 4, 4, 4);
            chkHasCommittedDate.Name = "chkHasCommittedDate";
            chkHasCommittedDate.Size = new Size(165, 36);
            chkHasCommittedDate.TabIndex = 12;
            chkHasCommittedDate.Text = "Có hẹn giao";
            // 
            // lblPickupMethod
            // 
            lblPickupMethod.Location = new Point(22, 352);
            lblPickupMethod.Margin = new Padding(4, 0, 4, 0);
            lblPickupMethod.Name = "lblPickupMethod";
            lblPickupMethod.Size = new Size(195, 34);
            lblPickupMethod.TabIndex = 13;
            lblPickupMethod.Text = "Phương thức lấy:";
            // 
            // rbCompanyPickup
            // 
            rbCompanyPickup.Checked = true;
            rbCompanyPickup.Location = new Point(225, 350);
            rbCompanyPickup.Margin = new Padding(4, 4, 4, 4);
            rbCompanyPickup.Name = "rbCompanyPickup";
            rbCompanyPickup.Size = new Size(270, 36);
            rbCompanyPickup.TabIndex = 14;
            rbCompanyPickup.TabStop = true;
            rbCompanyPickup.Text = "Công ty cử xe đến lấy";
            // 
            // rbSelfDelivery
            // 
            rbSelfDelivery.Location = new Point(510, 350);
            rbSelfDelivery.Margin = new Padding(4, 4, 4, 4);
            rbSelfDelivery.Name = "rbSelfDelivery";
            rbSelfDelivery.Size = new Size(270, 36);
            rbSelfDelivery.TabIndex = 15;
            rbSelfDelivery.Text = "Khách tự mang tới kho";
            // 
            // lblWarehouse
            // 
            lblWarehouse.Location = new Point(22, 398);
            lblWarehouse.Margin = new Padding(4, 0, 4, 0);
            lblWarehouse.Name = "lblWarehouse";
            lblWarehouse.Size = new Size(195, 34);
            lblWarehouse.TabIndex = 16;
            lblWarehouse.Text = "Kho trung chuyển *:";
            // 
            // cboWarehouse
            // 
            cboWarehouse.DropDownStyle = ComboBoxStyle.DropDownList;
            cboWarehouse.Location = new Point(225, 393);
            cboWarehouse.Margin = new Padding(4, 4, 4, 4);
            cboWarehouse.Name = "cboWarehouse";
            cboWarehouse.Size = new Size(448, 36);
            cboWarehouse.TabIndex = 17;
            // 
            // lblCargoTitle
            // 
            lblCargoTitle.AutoSize = true;
            lblCargoTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblCargoTitle.Location = new Point(22, 472);
            lblCargoTitle.Margin = new Padding(4, 0, 4, 0);
            lblCargoTitle.Name = "lblCargoTitle";
            lblCargoTitle.Size = new Size(663, 28);
            lblCargoTitle.TabIndex = 18;
            lblCargoTitle.Text = "Danh sách hàng hóa (bấm dòng trống cuối bảng để thêm dòng mới):";
            // 
            // btnRemoveCargoLine
            // 
            btnRemoveCargoLine.BackColor = Color.White;
            btnRemoveCargoLine.FlatStyle = FlatStyle.Flat;
            btnRemoveCargoLine.Location = new Point(841, 464);
            btnRemoveCargoLine.Margin = new Padding(4, 4, 4, 4);
            btnRemoveCargoLine.Name = "btnRemoveCargoLine";
            btnRemoveCargoLine.Size = new Size(226, 45);
            btnRemoveCargoLine.TabIndex = 19;
            btnRemoveCargoLine.Text = "🗑 Xóa dòng đã chọn";
            btnRemoveCargoLine.UseVisualStyleBackColor = false;
            // 
            // gridCargoLines
            // 
            gridCargoLines.ColumnHeadersHeight = 34;
            gridCargoLines.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            gridCargoLines.Columns.AddRange(new DataGridViewColumn[] { colCargoType, colPackageCount, colWeightKg, colVolumeM3, colIsFragile, colIsHazmat, colQtyTotal });
            gridCargoLines.Location = new Point(22, 518);
            gridCargoLines.Margin = new Padding(4, 4, 4, 4);
            gridCargoLines.Name = "gridCargoLines";
            gridCargoLines.RowHeadersVisible = false;
            gridCargoLines.RowHeadersWidth = 62;
            gridCargoLines.Size = new Size(1057, 270);
            gridCargoLines.TabIndex = 20;
            // 
            // colCargoType
            // 
            colCargoType.DataPropertyName = "CargoType";
            colCargoType.HeaderText = "Loại hàng";
            colCargoType.MinimumWidth = 8;
            colCargoType.Name = "colCargoType";
            colCargoType.Width = 160;
            // 
            // colPackageCount
            // 
            colPackageCount.DataPropertyName = "PackageCount";
            colPackageCount.HeaderText = "Số kiện";
            colPackageCount.MinimumWidth = 8;
            colPackageCount.Name = "colPackageCount";
            colPackageCount.Width = 70;
            // 
            // colWeightKg
            // 
            colWeightKg.DataPropertyName = "WeightKg";
            colWeightKg.HeaderText = "KL (kg)";
            colWeightKg.MinimumWidth = 8;
            colWeightKg.Name = "colWeightKg";
            colWeightKg.Width = 80;
            // 
            // colVolumeM3
            // 
            colVolumeM3.DataPropertyName = "VolumeM3";
            colVolumeM3.HeaderText = "TT (m³)";
            colVolumeM3.MinimumWidth = 8;
            colVolumeM3.Name = "colVolumeM3";
            colVolumeM3.Width = 80;
            // 
            // colIsFragile
            // 
            colIsFragile.DataPropertyName = "IsFragile";
            colIsFragile.HeaderText = "Dễ vỡ";
            colIsFragile.MinimumWidth = 8;
            colIsFragile.Name = "colIsFragile";
            colIsFragile.Width = 55;
            // 
            // colIsHazmat
            // 
            colIsHazmat.DataPropertyName = "IsHazmat";
            colIsHazmat.HeaderText = "Nguy hiểm";
            colIsHazmat.MinimumWidth = 8;
            colIsHazmat.Name = "colIsHazmat";
            colIsHazmat.Width = 75;
            // 
            // colQtyTotal
            // 
            colQtyTotal.DataPropertyName = "QtyTotal";
            colQtyTotal.HeaderText = "SL (để chia xe)";
            colQtyTotal.MinimumWidth = 8;
            colQtyTotal.Name = "colQtyTotal";
            colQtyTotal.Width = 105;
            // 
            // btnSave
            // 
            btnSave.BackColor = Color.FromArgb(37, 99, 235);
            btnSave.Cursor = Cursors.Hand;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSave.ForeColor = Color.White;
            btnSave.Location = new Point(22, 810);
            btnSave.Margin = new Padding(4, 4, 4, 4);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(270, 60);
            btnSave.TabIndex = 21;
            btnSave.Text = "Lưu đơn";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += BtnSave_Click;
            // 
            // lblStatus
            // 
            lblStatus.ForeColor = Color.Red;
            lblStatus.Location = new Point(315, 810);
            lblStatus.Margin = new Padding(4, 0, 4, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(735, 60);
            lblStatus.TabIndex = 22;
            // 
            // CreateOrderForm
            // 
            AutoScaleDimensions = new SizeF(144F, 144F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(1080, 915);
            Controls.Add(lblCustomer);
            Controls.Add(cboCustomer);
            Controls.Add(lblPickup);
            Controls.Add(txtPickup);
            Controls.Add(lblPickupQuick);
            Controls.Add(cboPickupLocation);
            Controls.Add(lblDropoff);
            Controls.Add(txtDropoff);
            Controls.Add(lblDropoffQuick);
            Controls.Add(cboDropoffLocation);
            Controls.Add(lblCommitted);
            Controls.Add(dtCommitted);
            Controls.Add(chkHasCommittedDate);
            Controls.Add(lblPickupMethod);
            Controls.Add(rbCompanyPickup);
            Controls.Add(rbSelfDelivery);
            Controls.Add(lblWarehouse);
            Controls.Add(cboWarehouse);
            Controls.Add(lblCargoTitle);
            Controls.Add(btnRemoveCargoLine);
            Controls.Add(gridCargoLines);
            Controls.Add(btnSave);
            Controls.Add(lblStatus);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(6, 6, 6, 6);
            MaximizeBox = false;
            Name = "CreateOrderForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Tạo đơn vận chuyển mới";
            ((System.ComponentModel.ISupportInitialize)gridCargoLines).EndInit();
            ResumeLayout(false);
            PerformLayout();
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
        private System.Windows.Forms.Label lblWarehouse;
        private System.Windows.Forms.ComboBox cboWarehouse;
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
