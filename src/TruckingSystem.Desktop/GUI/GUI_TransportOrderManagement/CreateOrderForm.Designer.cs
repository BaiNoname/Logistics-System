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

        /// <summary>Bố cục: root (TableLayoutPanel 1 cột) = vùng nhập liệu (Panel tự cuộn, chứa bảng
        /// nhãn | ô nhập | nút phụ) / tiêu đề lưới hàng hóa + nút xóa dòng / lưới hàng hóa (chiếm phần
        /// còn lại) / trạng thái / thanh nút Lưu ở dưới. Không có tọa độ cứng.</summary>
        private void InitializeComponent()
        {
            root = new System.Windows.Forms.TableLayoutPanel();
            fieldsPanel = new System.Windows.Forms.Panel();
            fields = new System.Windows.Forms.TableLayoutPanel();
            lblCustomer = new System.Windows.Forms.Label();
            cboCustomer = new System.Windows.Forms.ComboBox();
            lblPickup = new System.Windows.Forms.Label();
            txtPickup = new System.Windows.Forms.TextBox();
            lblPickupQuick = new System.Windows.Forms.Label();
            cboPickupLocation = new System.Windows.Forms.ComboBox();
            btnAddPickupLocation = new System.Windows.Forms.Button();
            lblDropoff = new System.Windows.Forms.Label();
            txtDropoff = new System.Windows.Forms.TextBox();
            lblDropoffQuick = new System.Windows.Forms.Label();
            cboDropoffLocation = new System.Windows.Forms.ComboBox();
            btnAddDropoffLocation = new System.Windows.Forms.Button();
            lblCommitted = new System.Windows.Forms.Label();
            dtCommitted = new System.Windows.Forms.DateTimePicker();
            lblDeadlineHint = new System.Windows.Forms.Label();
            lblPickupMethod = new System.Windows.Forms.Label();
            pickupMethodBar = new System.Windows.Forms.FlowLayoutPanel();
            rbCompanyPickup = new System.Windows.Forms.RadioButton();
            rbSelfDelivery = new System.Windows.Forms.RadioButton();
            lblWarehouse = new System.Windows.Forms.Label();
            cboWarehouse = new System.Windows.Forms.ComboBox();
            cargoHeader = new System.Windows.Forms.TableLayoutPanel();
            lblCargoTitle = new System.Windows.Forms.Label();
            btnRemoveCargoLine = new System.Windows.Forms.Button();
            gridCargoLines = new System.Windows.Forms.DataGridView();
            colCargoType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colPackageCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colWeightKg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colVolumeM3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colIsFragile = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            colIsHazmat = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            colQtyTotal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            lblStatus = new System.Windows.Forms.Label();
            buttonBar = new System.Windows.Forms.FlowLayoutPanel();
            btnSave = new System.Windows.Forms.Button();
            root.SuspendLayout();
            fieldsPanel.SuspendLayout();
            fields.SuspendLayout();
            pickupMethodBar.SuspendLayout();
            cargoHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridCargoLines).BeginInit();
            buttonBar.SuspendLayout();
            SuspendLayout();
            //
            // root
            //
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.Controls.Add(fieldsPanel, 0, 0);
            root.Controls.Add(cargoHeader, 0, 1);
            root.Controls.Add(gridCargoLines, 0, 2);
            root.Controls.Add(lblStatus, 0, 3);
            root.Controls.Add(buttonBar, 0, 4);
            root.Dock = System.Windows.Forms.DockStyle.Fill;
            root.Name = "root";
            root.Padding = new System.Windows.Forms.Padding(20, 16, 20, 16);
            root.RowCount = 5;
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.Size = new System.Drawing.Size(980, 760);
            root.TabIndex = 0;
            //
            // fieldsPanel
            //
            fieldsPanel.AutoScroll = true;
            fieldsPanel.Controls.Add(fields);
            fieldsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            fieldsPanel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            fieldsPanel.Name = "fieldsPanel";
            fieldsPanel.TabIndex = 0;
            //
            // fields
            //
            fields.AutoSize = true;
            fields.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            fields.ColumnCount = 3;
            fields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            fields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.Controls.Add(lblCustomer, 0, 0);
            fields.Controls.Add(cboCustomer, 1, 0);
            fields.Controls.Add(lblPickup, 0, 1);
            fields.Controls.Add(txtPickup, 1, 1);
            fields.Controls.Add(lblPickupQuick, 0, 2);
            fields.Controls.Add(cboPickupLocation, 1, 2);
            fields.Controls.Add(btnAddPickupLocation, 2, 2);
            fields.Controls.Add(lblDropoff, 0, 3);
            fields.Controls.Add(txtDropoff, 1, 3);
            fields.Controls.Add(lblDropoffQuick, 0, 4);
            fields.Controls.Add(cboDropoffLocation, 1, 4);
            fields.Controls.Add(btnAddDropoffLocation, 2, 4);
            fields.Controls.Add(lblCommitted, 0, 5);
            fields.Controls.Add(dtCommitted, 1, 5);
            fields.Controls.Add(lblDeadlineHint, 1, 6);
            fields.Controls.Add(lblPickupMethod, 0, 7);
            fields.Controls.Add(pickupMethodBar, 1, 7);
            fields.Controls.Add(lblWarehouse, 0, 8);
            fields.Controls.Add(cboWarehouse, 1, 8);
            fields.Dock = System.Windows.Forms.DockStyle.Top;
            fields.Margin = new System.Windows.Forms.Padding(0);
            fields.Name = "fields";
            fields.RowCount = 9;
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.TabIndex = 0;
            //
            // lblCustomer
            //
            lblCustomer.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblCustomer.AutoSize = true;
            lblCustomer.Margin = new System.Windows.Forms.Padding(0, 4, 12, 4);
            lblCustomer.Name = "lblCustomer";
            lblCustomer.TabIndex = 0;
            lblCustomer.Text = "Khách hàng:";
            //
            // cboCustomer
            //
            cboCustomer.Dock = System.Windows.Forms.DockStyle.Fill;
            cboCustomer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboCustomer.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            cboCustomer.Name = "cboCustomer";
            cboCustomer.TabIndex = 1;
            //
            // lblPickup
            //
            lblPickup.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblPickup.AutoSize = true;
            lblPickup.Margin = new System.Windows.Forms.Padding(0, 4, 12, 4);
            lblPickup.Name = "lblPickup";
            lblPickup.TabIndex = 2;
            lblPickup.Text = "Điểm lấy hàng:";
            //
            // txtPickup
            //
            txtPickup.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            txtPickup.Dock = System.Windows.Forms.DockStyle.Fill;
            txtPickup.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            txtPickup.Name = "txtPickup";
            txtPickup.ReadOnly = true;
            txtPickup.TabIndex = 3;
            //
            // lblPickupQuick
            //
            lblPickupQuick.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblPickupQuick.AutoSize = true;
            lblPickupQuick.Margin = new System.Windows.Forms.Padding(0, 4, 12, 4);
            lblPickupQuick.Name = "lblPickupQuick";
            lblPickupQuick.TabIndex = 4;
            lblPickupQuick.Text = "📍 Địa điểm lấy *:";
            //
            // cboPickupLocation
            //
            cboPickupLocation.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cboPickupLocation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboPickupLocation.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            cboPickupLocation.Name = "cboPickupLocation";
            cboPickupLocation.TabIndex = 5;
            //
            // btnAddPickupLocation
            //
            btnAddPickupLocation.Anchor = System.Windows.Forms.AnchorStyles.Left;
            btnAddPickupLocation.AutoSize = true;
            btnAddPickupLocation.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnAddPickupLocation.BackColor = System.Drawing.Color.White;
            btnAddPickupLocation.Cursor = System.Windows.Forms.Cursors.Hand;
            btnAddPickupLocation.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnAddPickupLocation.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            btnAddPickupLocation.MinimumSize = new System.Drawing.Size(0, 34);
            btnAddPickupLocation.Name = "btnAddPickupLocation";
            btnAddPickupLocation.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            btnAddPickupLocation.TabIndex = 6;
            btnAddPickupLocation.Text = "＋ Địa điểm mới";
            btnAddPickupLocation.UseVisualStyleBackColor = false;
            //
            // lblDropoff
            //
            lblDropoff.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblDropoff.AutoSize = true;
            lblDropoff.Margin = new System.Windows.Forms.Padding(0, 4, 12, 4);
            lblDropoff.Name = "lblDropoff";
            lblDropoff.TabIndex = 7;
            lblDropoff.Text = "Điểm giao hàng:";
            //
            // txtDropoff
            //
            txtDropoff.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            txtDropoff.Dock = System.Windows.Forms.DockStyle.Fill;
            txtDropoff.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            txtDropoff.Name = "txtDropoff";
            txtDropoff.ReadOnly = true;
            txtDropoff.TabIndex = 8;
            //
            // lblDropoffQuick
            //
            lblDropoffQuick.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblDropoffQuick.AutoSize = true;
            lblDropoffQuick.Margin = new System.Windows.Forms.Padding(0, 4, 12, 4);
            lblDropoffQuick.Name = "lblDropoffQuick";
            lblDropoffQuick.TabIndex = 9;
            lblDropoffQuick.Text = "📍 Địa điểm giao *:";
            //
            // cboDropoffLocation
            //
            cboDropoffLocation.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cboDropoffLocation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboDropoffLocation.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            cboDropoffLocation.Name = "cboDropoffLocation";
            cboDropoffLocation.TabIndex = 10;
            //
            // btnAddDropoffLocation
            //
            btnAddDropoffLocation.Anchor = System.Windows.Forms.AnchorStyles.Left;
            btnAddDropoffLocation.AutoSize = true;
            btnAddDropoffLocation.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnAddDropoffLocation.BackColor = System.Drawing.Color.White;
            btnAddDropoffLocation.Cursor = System.Windows.Forms.Cursors.Hand;
            btnAddDropoffLocation.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnAddDropoffLocation.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            btnAddDropoffLocation.MinimumSize = new System.Drawing.Size(0, 34);
            btnAddDropoffLocation.Name = "btnAddDropoffLocation";
            btnAddDropoffLocation.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            btnAddDropoffLocation.TabIndex = 11;
            btnAddDropoffLocation.Text = "＋ Địa điểm mới";
            btnAddDropoffLocation.UseVisualStyleBackColor = false;
            //
            // lblCommitted
            //
            lblCommitted.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblCommitted.AutoSize = true;
            lblCommitted.Margin = new System.Windows.Forms.Padding(0, 4, 12, 4);
            lblCommitted.Name = "lblCommitted";
            lblCommitted.TabIndex = 12;
            lblCommitted.Text = "Hạn giao cam kết: *";
            //
            // dtCommitted
            //
            dtCommitted.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dtCommitted.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            dtCommitted.Name = "dtCommitted";
            dtCommitted.TabIndex = 13;
            //
            // lblDeadlineHint — ước tính số ngày chạy của tuyến + gợi ý hạn giao (điền ở CreateOrderForm.cs)
            //
            lblDeadlineHint.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblDeadlineHint.AutoSize = true;
            fields.SetColumnSpan(lblDeadlineHint, 2);
            lblDeadlineHint.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblDeadlineHint.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblDeadlineHint.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            lblDeadlineHint.Name = "lblDeadlineHint";
            lblDeadlineHint.TabIndex = 14;
            lblDeadlineHint.Text = "Chọn địa điểm lấy / giao và kho để hệ thống ước tính số ngày chạy và gợi ý hạn giao.";
            //
            // lblPickupMethod
            //
            lblPickupMethod.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblPickupMethod.AutoSize = true;
            lblPickupMethod.Margin = new System.Windows.Forms.Padding(0, 4, 12, 4);
            lblPickupMethod.Name = "lblPickupMethod";
            lblPickupMethod.TabIndex = 15;
            lblPickupMethod.Text = "Phương thức lấy:";
            //
            // pickupMethodBar
            //
            pickupMethodBar.AutoSize = true;
            pickupMethodBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            fields.SetColumnSpan(pickupMethodBar, 2);
            pickupMethodBar.Controls.Add(rbCompanyPickup);
            pickupMethodBar.Controls.Add(rbSelfDelivery);
            pickupMethodBar.Dock = System.Windows.Forms.DockStyle.Fill;
            pickupMethodBar.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            pickupMethodBar.Name = "pickupMethodBar";
            pickupMethodBar.TabIndex = 16;
            pickupMethodBar.WrapContents = true;
            //
            // rbCompanyPickup
            //
            rbCompanyPickup.AutoSize = true;
            rbCompanyPickup.Checked = true;
            rbCompanyPickup.Margin = new System.Windows.Forms.Padding(0, 4, 24, 4);
            rbCompanyPickup.Name = "rbCompanyPickup";
            rbCompanyPickup.TabIndex = 0;
            rbCompanyPickup.TabStop = true;
            rbCompanyPickup.Text = "Công ty cử xe đến lấy";
            rbCompanyPickup.UseVisualStyleBackColor = true;
            //
            // rbSelfDelivery
            //
            rbSelfDelivery.AutoSize = true;
            rbSelfDelivery.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            rbSelfDelivery.Name = "rbSelfDelivery";
            rbSelfDelivery.TabIndex = 1;
            rbSelfDelivery.Text = "Khách tự mang tới kho";
            rbSelfDelivery.UseVisualStyleBackColor = true;
            //
            // lblWarehouse
            //
            lblWarehouse.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblWarehouse.AutoSize = true;
            lblWarehouse.Margin = new System.Windows.Forms.Padding(0, 4, 12, 4);
            lblWarehouse.Name = "lblWarehouse";
            lblWarehouse.TabIndex = 17;
            lblWarehouse.Text = "Kho trung chuyển *:";
            //
            // cboWarehouse
            //
            cboWarehouse.Dock = System.Windows.Forms.DockStyle.Fill;
            cboWarehouse.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboWarehouse.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            cboWarehouse.Name = "cboWarehouse";
            cboWarehouse.TabIndex = 18;
            //
            // cargoHeader
            //
            cargoHeader.AutoSize = true;
            cargoHeader.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            cargoHeader.ColumnCount = 2;
            cargoHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            cargoHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            cargoHeader.Controls.Add(lblCargoTitle, 0, 0);
            cargoHeader.Controls.Add(btnRemoveCargoLine, 1, 0);
            cargoHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            cargoHeader.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            cargoHeader.Name = "cargoHeader";
            cargoHeader.RowCount = 1;
            cargoHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            cargoHeader.TabIndex = 1;
            //
            // lblCargoTitle
            //
            lblCargoTitle.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblCargoTitle.AutoSize = true;
            lblCargoTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblCargoTitle.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            lblCargoTitle.Name = "lblCargoTitle";
            lblCargoTitle.TabIndex = 0;
            lblCargoTitle.Text = "Danh sách hàng hóa (bấm dòng trống cuối bảng để thêm dòng mới):";
            //
            // btnRemoveCargoLine
            //
            btnRemoveCargoLine.Anchor = System.Windows.Forms.AnchorStyles.Right;
            btnRemoveCargoLine.AutoSize = true;
            btnRemoveCargoLine.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnRemoveCargoLine.BackColor = System.Drawing.Color.White;
            btnRemoveCargoLine.Cursor = System.Windows.Forms.Cursors.Hand;
            btnRemoveCargoLine.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnRemoveCargoLine.Margin = new System.Windows.Forms.Padding(0);
            btnRemoveCargoLine.MinimumSize = new System.Drawing.Size(0, 34);
            btnRemoveCargoLine.Name = "btnRemoveCargoLine";
            btnRemoveCargoLine.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            btnRemoveCargoLine.TabIndex = 1;
            btnRemoveCargoLine.Text = "🗑 Xóa dòng đã chọn";
            btnRemoveCargoLine.UseVisualStyleBackColor = false;
            //
            // gridCargoLines
            //
            gridCargoLines.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            gridCargoLines.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridCargoLines.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colCargoType, colPackageCount, colWeightKg, colVolumeM3, colIsFragile, colIsHazmat, colQtyTotal });
            gridCargoLines.Dock = System.Windows.Forms.DockStyle.Fill;
            gridCargoLines.Margin = new System.Windows.Forms.Padding(0);
            gridCargoLines.Name = "gridCargoLines";
            gridCargoLines.RowHeadersVisible = false;
            gridCargoLines.TabIndex = 2;
            //
            // colCargoType
            //
            colCargoType.DataPropertyName = "CargoType";
            colCargoType.FillWeight = 160F;
            colCargoType.HeaderText = "Loại hàng";
            colCargoType.MinimumWidth = 8;
            colCargoType.Name = "colCargoType";
            //
            // colPackageCount
            //
            colPackageCount.DataPropertyName = "PackageCount";
            colPackageCount.FillWeight = 70F;
            colPackageCount.HeaderText = "Số kiện";
            colPackageCount.MinimumWidth = 8;
            colPackageCount.Name = "colPackageCount";
            //
            // colWeightKg
            //
            colWeightKg.DataPropertyName = "WeightKg";
            colWeightKg.FillWeight = 80F;
            colWeightKg.HeaderText = "KL (kg)";
            colWeightKg.MinimumWidth = 8;
            colWeightKg.Name = "colWeightKg";
            //
            // colVolumeM3
            //
            colVolumeM3.DataPropertyName = "VolumeM3";
            colVolumeM3.FillWeight = 80F;
            colVolumeM3.HeaderText = "TT (m³)";
            colVolumeM3.MinimumWidth = 8;
            colVolumeM3.Name = "colVolumeM3";
            //
            // colIsFragile
            //
            colIsFragile.DataPropertyName = "IsFragile";
            colIsFragile.FillWeight = 55F;
            colIsFragile.HeaderText = "Dễ vỡ";
            colIsFragile.MinimumWidth = 8;
            colIsFragile.Name = "colIsFragile";
            //
            // colIsHazmat
            //
            colIsHazmat.DataPropertyName = "IsHazmat";
            colIsHazmat.FillWeight = 75F;
            colIsHazmat.HeaderText = "Nguy hiểm";
            colIsHazmat.MinimumWidth = 8;
            colIsHazmat.Name = "colIsHazmat";
            //
            // colQtyTotal
            //
            colQtyTotal.DataPropertyName = "QtyTotal";
            colQtyTotal.FillWeight = 105F;
            colQtyTotal.HeaderText = "SL (để chia xe)";
            colQtyTotal.MinimumWidth = 8;
            colQtyTotal.Name = "colQtyTotal";
            //
            // lblStatus
            //
            lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblStatus.AutoSize = true;
            lblStatus.ForeColor = System.Drawing.Color.Red;
            lblStatus.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.TabIndex = 3;
            //
            // buttonBar
            //
            buttonBar.AutoSize = true;
            buttonBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            buttonBar.Controls.Add(btnSave);
            buttonBar.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonBar.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            buttonBar.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            buttonBar.Name = "buttonBar";
            buttonBar.TabIndex = 4;
            buttonBar.WrapContents = true;
            //
            // btnSave
            //
            btnSave.AutoSize = true;
            btnSave.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnSave.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnSave.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnSave.ForeColor = System.Drawing.Color.White;
            btnSave.Margin = new System.Windows.Forms.Padding(0);
            btnSave.MinimumSize = new System.Drawing.Size(140, 38);
            btnSave.Name = "btnSave";
            btnSave.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnSave.TabIndex = 0;
            btnSave.Text = "Lưu đơn";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            //
            // CreateOrderForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(980, 760);
            Controls.Add(root);
            MinimumSize = new System.Drawing.Size(760, 500);
            Name = "CreateOrderForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Tạo đơn vận chuyển mới";
            root.ResumeLayout(false);
            root.PerformLayout();
            fieldsPanel.ResumeLayout(false);
            fieldsPanel.PerformLayout();
            fields.ResumeLayout(false);
            fields.PerformLayout();
            pickupMethodBar.ResumeLayout(false);
            pickupMethodBar.PerformLayout();
            cargoHeader.ResumeLayout(false);
            cargoHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)gridCargoLines).EndInit();
            buttonBar.ResumeLayout(false);
            buttonBar.PerformLayout();
            ResumeLayout(false);
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
        private System.Windows.Forms.Label lblDeadlineHint;
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
        private System.Windows.Forms.Button btnAddPickupLocation;
        private System.Windows.Forms.Button btnAddDropoffLocation;
        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.Panel fieldsPanel;
        private System.Windows.Forms.TableLayoutPanel fields;
        private System.Windows.Forms.FlowLayoutPanel pickupMethodBar;
        private System.Windows.Forms.TableLayoutPanel cargoHeader;
        private System.Windows.Forms.FlowLayoutPanel buttonBar;
    }
}
