namespace TruckingSystem.Desktop.GUI_WarehouseManagement
{
    partial class WarehousesControl
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            lblTitle = new Label();
            lblChoose = new Label();
            cboWarehouse = new ComboBox();
            btnCreateWarehouse = new Button();
            btnEditWarehouse = new Button();
            btnManageZones = new Button();
            btnDeleteWarehouse = new Button();
            btnManualStock = new Button();
            lblStatus = new Label();
            zonesPanel = new Panel();
            testPanel = new Panel();
            lblTestTitle = new Label();
            lblCargoLineId = new Label();
            cboOrderSuggest = new ComboBox();
            cboCargoLineSuggest = new ComboBox();
            lblQty = new Label();
            numQty = new NumericUpDown();
            btnSuggest = new Button();
            lblResult = new Label();
            testPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numQty).BeginInit();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(0, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(400, 54);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Kho trung chuyển";
            // 
            // lblChoose
            // 
            lblChoose.AutoSize = true;
            lblChoose.Font = new Font("Segoe UI", 10F);
            lblChoose.Location = new Point(-2, 72);
            lblChoose.Name = "lblChoose";
            lblChoose.Size = new Size(100, 28);
            lblChoose.TabIndex = 1;
            lblChoose.Text = "Chọn kho:";
            // 
            // cboWarehouse
            // 
            cboWarehouse.DropDownStyle = ComboBoxStyle.DropDownList;
            cboWarehouse.Location = new Point(118, 69);
            cboWarehouse.Name = "cboWarehouse";
            cboWarehouse.Size = new Size(280, 36);
            cboWarehouse.TabIndex = 2;
            // 
            // btnCreateWarehouse
            // 
            btnCreateWarehouse.BackColor = Color.FromArgb(37, 99, 235);
            btnCreateWarehouse.Cursor = Cursors.Hand;
            btnCreateWarehouse.FlatAppearance.BorderSize = 0;
            btnCreateWarehouse.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
            btnCreateWarehouse.FlatStyle = FlatStyle.Flat;
            btnCreateWarehouse.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCreateWarehouse.ForeColor = Color.White;
            btnCreateWarehouse.Location = new Point(424, 60);
            btnCreateWarehouse.Name = "btnCreateWarehouse";
            btnCreateWarehouse.Size = new Size(170, 52);
            btnCreateWarehouse.TabIndex = 3;
            btnCreateWarehouse.Text = "➕ Thêm kho mới";
            btnCreateWarehouse.UseVisualStyleBackColor = false;
            btnCreateWarehouse.Click += BtnCreateWarehouse_Click;
            // 
            // btnEditWarehouse
            // 
            btnEditWarehouse.BackColor = Color.White;
            btnEditWarehouse.Cursor = Cursors.Hand;
            btnEditWarehouse.FlatStyle = FlatStyle.Flat;
            btnEditWarehouse.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnEditWarehouse.ForeColor = Color.FromArgb(15, 23, 42);
            btnEditWarehouse.Location = new Point(598, 60);
            btnEditWarehouse.Name = "btnEditWarehouse";
            btnEditWarehouse.Size = new Size(130, 52);
            btnEditWarehouse.TabIndex = 4;
            btnEditWarehouse.Text = "✏️ Sửa kho";
            btnEditWarehouse.UseVisualStyleBackColor = false;
            btnEditWarehouse.Click += BtnEditWarehouse_Click;
            // 
            // btnManageZones
            // 
            btnManageZones.BackColor = Color.White;
            btnManageZones.Cursor = Cursors.Hand;
            btnManageZones.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
            btnManageZones.FlatStyle = FlatStyle.Flat;
            btnManageZones.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnManageZones.ForeColor = Color.FromArgb(15, 23, 42);
            btnManageZones.Location = new Point(735, 60);
            btnManageZones.Name = "btnManageZones";
            btnManageZones.Size = new Size(218, 52);
            btnManageZones.TabIndex = 4;
            btnManageZones.Text = "🏗️ Quản lý khu vực";
            btnManageZones.UseVisualStyleBackColor = false;
            btnManageZones.Click += BtnManageZones_Click;
            // 
            // btnDeleteWarehouse
            // 
            btnDeleteWarehouse.BackColor = Color.White;
            btnDeleteWarehouse.Cursor = Cursors.Hand;
            btnDeleteWarehouse.FlatAppearance.BorderColor = Color.FromArgb(220, 38, 38);
            btnDeleteWarehouse.FlatStyle = FlatStyle.Flat;
            btnDeleteWarehouse.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnDeleteWarehouse.ForeColor = Color.FromArgb(220, 38, 38);
            btnDeleteWarehouse.Location = new Point(982, 60);
            btnDeleteWarehouse.Name = "btnDeleteWarehouse";
            btnDeleteWarehouse.Size = new Size(130, 52);
            btnDeleteWarehouse.TabIndex = 5;
            btnDeleteWarehouse.Text = "🗑️ Xóa kho";
            btnDeleteWarehouse.UseVisualStyleBackColor = false;
            btnDeleteWarehouse.Click += BtnDeleteWarehouse_Click;
            // 
            // btnManualStock
            // 
            btnManualStock.BackColor = Color.FromArgb(217, 119, 6);
            btnManualStock.Cursor = Cursors.Hand;
            btnManualStock.FlatAppearance.BorderSize = 0;
            btnManualStock.FlatStyle = FlatStyle.Flat;
            btnManualStock.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnManualStock.ForeColor = Color.White;
            btnManualStock.Location = new Point(1124, 60);
            btnManualStock.Name = "btnManualStock";
            btnManualStock.Size = new Size(200, 52);
            btnManualStock.TabIndex = 5;
            btnManualStock.Text = "📤 Nhập/Xuất kho";
            btnManualStock.UseVisualStyleBackColor = false;
            btnManualStock.Click += BtnManualStock_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
            lblStatus.Location = new Point(0, 100);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(0, 25);
            lblStatus.TabIndex = 6;
            // 
            // zonesPanel
            // 
            zonesPanel.AutoScroll = true;
            zonesPanel.BackColor = Color.White;
            zonesPanel.BorderStyle = BorderStyle.FixedSingle;
            zonesPanel.Location = new Point(0, 130);
            zonesPanel.Name = "zonesPanel";
            zonesPanel.Size = new Size(1305, 430);
            zonesPanel.TabIndex = 7;
            // 
            // testPanel
            // 
            testPanel.AutoScroll = true;
            testPanel.BackColor = Color.White;
            testPanel.Controls.Add(lblTestTitle);
            testPanel.Controls.Add(lblCargoLineId);
            testPanel.Controls.Add(cboOrderSuggest);
            testPanel.Controls.Add(cboCargoLineSuggest);
            testPanel.Controls.Add(lblQty);
            testPanel.Controls.Add(numQty);
            testPanel.Controls.Add(btnSuggest);
            testPanel.Controls.Add(lblResult);
            testPanel.Location = new Point(0, 570);
            testPanel.Name = "testPanel";
            testPanel.Size = new Size(1000, 180);
            testPanel.TabIndex = 8;
            // 
            // lblTestTitle
            // 
            lblTestTitle.AutoSize = true;
            lblTestTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblTestTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTestTitle.Location = new Point(20, 14);
            lblTestTitle.Name = "lblTestTitle";
            lblTestTitle.Size = new Size(689, 36);
            lblTestTitle.TabIndex = 0;
            lblTestTitle.Text = "\U0001f9ea  Kiểm tra thuật toán Best-Fit — đề xuất vị trí lưu trữ";
            // 
            // lblCargoLineId
            // 
            lblCargoLineId.AutoSize = true;
            lblCargoLineId.Font = new Font("Segoe UI", 9F);
            lblCargoLineId.ForeColor = Color.FromArgb(100, 116, 139);
            lblCargoLineId.Location = new Point(20, 56);
            lblCargoLineId.Name = "lblCargoLineId";
            lblCargoLineId.Size = new Size(390, 25);
            lblCargoLineId.TabIndex = 1;
            lblCargoLineId.Text = "Chọn đơn hàng → dòng hàng cần thử xếp vị trí:";
            // 
            // cboOrderSuggest
            // 
            cboOrderSuggest.DropDownStyle = ComboBoxStyle.DropDownList;
            cboOrderSuggest.Location = new Point(20, 76);
            cboOrderSuggest.Name = "cboOrderSuggest";
            cboOrderSuggest.Size = new Size(270, 36);
            cboOrderSuggest.TabIndex = 2;
            // 
            // cboCargoLineSuggest
            // 
            cboCargoLineSuggest.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCargoLineSuggest.Location = new Point(300, 76);
            cboCargoLineSuggest.Name = "cboCargoLineSuggest";
            cboCargoLineSuggest.Size = new Size(220, 36);
            cboCargoLineSuggest.TabIndex = 3;
            // 
            // lblQty
            // 
            lblQty.AutoSize = true;
            lblQty.Font = new Font("Segoe UI", 9F);
            lblQty.ForeColor = Color.FromArgb(100, 116, 139);
            lblQty.Location = new Point(540, 56);
            lblQty.Name = "lblQty";
            lblQty.Size = new Size(154, 25);
            lblQty.TabIndex = 3;
            lblQty.Text = "Số lượng cần xếp:";
            // 
            // numQty
            // 
            numQty.DecimalPlaces = 2;
            numQty.Location = new Point(540, 76);
            numQty.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
            numQty.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
            numQty.Name = "numQty";
            numQty.Size = new Size(100, 34);
            numQty.TabIndex = 4;
            numQty.Value = new decimal(new int[] { 2, 0, 0, 0 });
            // 
            // btnSuggest
            // 
            btnSuggest.BackColor = Color.FromArgb(37, 99, 235);
            btnSuggest.Cursor = Cursors.Hand;
            btnSuggest.FlatAppearance.BorderSize = 0;
            btnSuggest.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
            btnSuggest.FlatStyle = FlatStyle.Flat;
            btnSuggest.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSuggest.ForeColor = Color.White;
            btnSuggest.Location = new Point(660, 75);
            btnSuggest.Name = "btnSuggest";
            btnSuggest.Size = new Size(160, 36);
            btnSuggest.TabIndex = 5;
            btnSuggest.Text = "🔍  Gợi ý vị trí";
            btnSuggest.UseVisualStyleBackColor = false;
            btnSuggest.Click += BtnSuggest_Click;
            // 
            // lblResult
            // 
            lblResult.AutoSize = true;
            lblResult.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblResult.Location = new Point(20, 116);
            lblResult.MaximumSize = new Size(900, 0);
            lblResult.Name = "lblResult";
            lblResult.Size = new Size(0, 28);
            lblResult.TabIndex = 6;
            // 
            // WarehousesControl
            // 
            AutoScaleDimensions = new SizeF(144F, 144F);
            BackColor = Color.FromArgb(241, 245, 249);
            Controls.Add(lblTitle);
            Controls.Add(lblChoose);
            Controls.Add(cboWarehouse);
            Controls.Add(btnCreateWarehouse);
            Controls.Add(btnEditWarehouse);
            Controls.Add(btnManageZones);
            Controls.Add(btnDeleteWarehouse);
            Controls.Add(btnManualStock);
            Controls.Add(lblStatus);
            Controls.Add(zonesPanel);
            Controls.Add(testPanel);
            Margin = new Padding(3, 3, 3, 3);
            Name = "WarehousesControl";
            Size = new Size(1420, 568);
            testPanel.ResumeLayout(false);
            testPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numQty).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblChoose;
        private System.Windows.Forms.ComboBox cboWarehouse;
        private System.Windows.Forms.Button btnCreateWarehouse;
        private System.Windows.Forms.Button btnEditWarehouse;
        private System.Windows.Forms.Button btnManageZones;
        private System.Windows.Forms.Button btnDeleteWarehouse;
        private System.Windows.Forms.Button btnManualStock;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Panel zonesPanel;
        private System.Windows.Forms.Panel testPanel;
        private System.Windows.Forms.Label lblTestTitle;
        private System.Windows.Forms.Label lblCargoLineId;
        private System.Windows.Forms.ComboBox cboOrderSuggest;
        private System.Windows.Forms.ComboBox cboCargoLineSuggest;
        private System.Windows.Forms.Label lblQty;
        private System.Windows.Forms.NumericUpDown numQty;
        private System.Windows.Forms.Button btnSuggest;
        private System.Windows.Forms.Label lblResult;
    }
}