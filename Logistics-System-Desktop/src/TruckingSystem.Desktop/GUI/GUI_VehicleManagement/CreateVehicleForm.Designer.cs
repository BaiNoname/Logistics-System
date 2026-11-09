namespace TruckingSystem.Desktop.GUI_VehicleManagement
{
    partial class CreateVehicleForm
    {
        /// <summary>Bắt buộc phải có — Designer dùng để quản lý các thành phần
        /// không hiển thị (components) như ImageList, Timer nếu có sau này.</summary>
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>Bố cục: root (TableLayoutPanel 1 cột) = bảng nhập liệu 2 cột (nhãn tự co | ô nhập giãn hết bề ngang, nằm trong Panel tự cuộn khi màn hình thấp) / trạng thái / thanh nút. Không có tọa độ cứng.</summary>
        private void InitializeComponent()
        {
            root = new System.Windows.Forms.TableLayoutPanel();
            scrollFields = new System.Windows.Forms.Panel();
            fields = new System.Windows.Forms.TableLayoutPanel();
            lblPlateNumber = new System.Windows.Forms.Label();
            txtPlateNumber = new System.Windows.Forms.TextBox();
            lblVehicleType = new System.Windows.Forms.Label();
            cboVehicleType = new System.Windows.Forms.ComboBox();
            lblCapacityQty = new System.Windows.Forms.Label();
            numCapacityQty = new TruckingSystem.Desktop.Common.NumberBox();
            lblCapacityKg = new System.Windows.Forms.Label();
            numCapacityKg = new TruckingSystem.Desktop.Common.NumberBox();
            lblCapacityM3 = new System.Windows.Forms.Label();
            numCapacityM3 = new TruckingSystem.Desktop.Common.NumberBox();
            lblFuelConsumption = new System.Windows.Forms.Label();
            numFuelConsumption = new TruckingSystem.Desktop.Common.NumberBox();
            lblFuelType = new System.Windows.Forms.Label();
            cboFuelType = new System.Windows.Forms.ComboBox();
            lblWarehouse = new System.Windows.Forms.Label();
            cboWarehouse = new System.Windows.Forms.ComboBox();
            lblStatus = new System.Windows.Forms.Label();
            buttonBar = new System.Windows.Forms.FlowLayoutPanel();
            btnSave = new System.Windows.Forms.Button();
            root.SuspendLayout();
            scrollFields.SuspendLayout();
            fields.SuspendLayout();
            buttonBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCapacityQty).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numCapacityKg).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numCapacityM3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numFuelConsumption).BeginInit();
            SuspendLayout();
            // 
            // root
            // 
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.Controls.Add(scrollFields, 0, 0);
            root.Controls.Add(lblStatus, 0, 1);
            root.Controls.Add(buttonBar, 0, 2);
            root.Dock = System.Windows.Forms.DockStyle.Fill;
            root.Padding = new System.Windows.Forms.Padding(20, 16, 20, 16);
            root.Size = new System.Drawing.Size(460, 400);
            root.Name = "root";
            root.TabIndex = 0;
            root.RowCount = 3;
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            // 
            // scrollFields
            // 
            scrollFields.Controls.Add(fields);
            scrollFields.AutoScroll = true;
            scrollFields.Dock = System.Windows.Forms.DockStyle.Fill;
            scrollFields.Margin = new System.Windows.Forms.Padding(0);
            scrollFields.Name = "scrollFields";
            scrollFields.TabIndex = 0;
            // 
            // fields
            // 
            fields.ColumnCount = 2;
            fields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            fields.Controls.Add(lblPlateNumber, 0, 0);
            fields.Controls.Add(txtPlateNumber, 1, 0);
            fields.Controls.Add(lblVehicleType, 0, 1);
            fields.Controls.Add(cboVehicleType, 1, 1);
            fields.Controls.Add(lblCapacityQty, 0, 2);
            fields.Controls.Add(numCapacityQty, 1, 2);
            fields.Controls.Add(lblCapacityKg, 0, 3);
            fields.Controls.Add(numCapacityKg, 1, 3);
            fields.Controls.Add(lblCapacityM3, 0, 4);
            fields.Controls.Add(numCapacityM3, 1, 4);
            fields.Controls.Add(lblFuelConsumption, 0, 5);
            fields.Controls.Add(numFuelConsumption, 1, 5);
            fields.Controls.Add(lblFuelType, 0, 6);
            fields.Controls.Add(cboFuelType, 1, 6);
            fields.Controls.Add(lblWarehouse, 0, 7);
            fields.Controls.Add(cboWarehouse, 1, 7);
            fields.AutoSize = true;
            fields.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            fields.Dock = System.Windows.Forms.DockStyle.Top;
            fields.Margin = new System.Windows.Forms.Padding(0);
            fields.Name = "fields";
            fields.TabIndex = 0;
            fields.RowCount = 8;
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            // 
            // lblPlateNumber
            // 
            lblPlateNumber.AutoSize = true;
            lblPlateNumber.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblPlateNumber.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            lblPlateNumber.Text = "Biển số xe *";
            lblPlateNumber.Name = "lblPlateNumber";
            lblPlateNumber.TabIndex = 0;
            // 
            // txtPlateNumber
            // 
            txtPlateNumber.Dock = System.Windows.Forms.DockStyle.Fill;
            txtPlateNumber.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            txtPlateNumber.Name = "txtPlateNumber";
            txtPlateNumber.TabIndex = 1;
            // 
            // lblVehicleType
            // 
            lblVehicleType.AutoSize = true;
            lblVehicleType.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblVehicleType.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            lblVehicleType.Text = "Loại xe *";
            lblVehicleType.Name = "lblVehicleType";
            lblVehicleType.TabIndex = 2;
            // 
            // cboVehicleType
            // 
            cboVehicleType.Dock = System.Windows.Forms.DockStyle.Fill;
            cboVehicleType.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            cboVehicleType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboVehicleType.Name = "cboVehicleType";
            cboVehicleType.TabIndex = 3;
            // 
            // lblCapacityQty
            // 
            lblCapacityQty.AutoSize = true;
            lblCapacityQty.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblCapacityQty.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            lblCapacityQty.Text = "Sức chứa (đơn vị) *";
            lblCapacityQty.Name = "lblCapacityQty";
            lblCapacityQty.TabIndex = 4;
            // 
            // numCapacityQty
            // 
            numCapacityQty.Dock = System.Windows.Forms.DockStyle.Fill;
            numCapacityQty.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            numCapacityQty.DecimalPlaces = 2;
            numCapacityQty.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            numCapacityQty.Value = new decimal(new int[] { 5000, 0, 0, 0 });
            numCapacityQty.Name = "numCapacityQty";
            numCapacityQty.TabIndex = 5;
            // 
            // lblCapacityKg
            // 
            lblCapacityKg.AutoSize = true;
            lblCapacityKg.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblCapacityKg.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            lblCapacityKg.Text = "Sức chứa (kg) *";
            lblCapacityKg.Name = "lblCapacityKg";
            lblCapacityKg.TabIndex = 6;
            // 
            // numCapacityKg
            // 
            numCapacityKg.Dock = System.Windows.Forms.DockStyle.Fill;
            numCapacityKg.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            numCapacityKg.DecimalPlaces = 2;
            numCapacityKg.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            numCapacityKg.Value = new decimal(new int[] { 5000, 0, 0, 0 });
            numCapacityKg.Name = "numCapacityKg";
            numCapacityKg.TabIndex = 7;
            // 
            // lblCapacityM3
            // 
            lblCapacityM3.AutoSize = true;
            lblCapacityM3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblCapacityM3.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            lblCapacityM3.Text = "Sức chứa (m³) *";
            lblCapacityM3.Name = "lblCapacityM3";
            lblCapacityM3.TabIndex = 8;
            // 
            // numCapacityM3
            // 
            numCapacityM3.Dock = System.Windows.Forms.DockStyle.Fill;
            numCapacityM3.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            numCapacityM3.DecimalPlaces = 2;
            numCapacityM3.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            numCapacityM3.Value = new decimal(new int[] { 25, 0, 0, 0 });
            numCapacityM3.Name = "numCapacityM3";
            numCapacityM3.TabIndex = 9;
            // 
            // lblFuelConsumption
            // 
            lblFuelConsumption.AutoSize = true;
            lblFuelConsumption.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblFuelConsumption.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            lblFuelConsumption.Text = "Định mức (L/100km) *";
            lblFuelConsumption.Name = "lblFuelConsumption";
            lblFuelConsumption.TabIndex = 10;
            // 
            // numFuelConsumption
            // 
            numFuelConsumption.Dock = System.Windows.Forms.DockStyle.Fill;
            numFuelConsumption.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            numFuelConsumption.DecimalPlaces = 1;
            numFuelConsumption.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
            numFuelConsumption.Name = "numFuelConsumption";
            numFuelConsumption.TabIndex = 11;
            // 
            // lblFuelType
            // 
            lblFuelType.AutoSize = true;
            lblFuelType.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblFuelType.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            lblFuelType.Text = "Loại nhiên liệu *";
            lblFuelType.Name = "lblFuelType";
            lblFuelType.TabIndex = 12;
            // 
            // cboFuelType
            // 
            cboFuelType.Dock = System.Windows.Forms.DockStyle.Fill;
            cboFuelType.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            cboFuelType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboFuelType.Name = "cboFuelType";
            cboFuelType.TabIndex = 13;
            // 
            // lblWarehouse
            // 
            lblWarehouse.AutoSize = true;
            lblWarehouse.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblWarehouse.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            lblWarehouse.Text = "Kho đóng quân (tuỳ chọn)";
            lblWarehouse.Name = "lblWarehouse";
            lblWarehouse.TabIndex = 14;
            // 
            // cboWarehouse
            // 
            cboWarehouse.Dock = System.Windows.Forms.DockStyle.Fill;
            cboWarehouse.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            cboWarehouse.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboWarehouse.Name = "cboWarehouse";
            cboWarehouse.TabIndex = 15;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblStatus.Margin = new System.Windows.Forms.Padding(0, 8, 0, 4);
            lblStatus.ForeColor = System.Drawing.Color.Red;
            lblStatus.Name = "lblStatus";
            lblStatus.TabIndex = 1;
            // 
            // buttonBar
            // 
            buttonBar.Controls.Add(btnSave);
            buttonBar.AutoSize = true;
            buttonBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            buttonBar.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonBar.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            buttonBar.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            buttonBar.WrapContents = true;
            buttonBar.Name = "buttonBar";
            buttonBar.TabIndex = 2;
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
            btnSave.ForeColor = System.Drawing.Color.White;
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnSave.MinimumSize = new System.Drawing.Size(0, 36);
            btnSave.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnSave.Text = "Lưu";
            btnSave.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            btnSave.Name = "btnSave";
            btnSave.TabIndex = 0;
            btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // CreateVehicleForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(460, 400);
            MaximizeBox = false;
            MinimumSize = new System.Drawing.Size(420, 360);
            Name = "CreateVehicleForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Thêm xe mới";
            Controls.Add(root);
            root.ResumeLayout(false);
            root.PerformLayout();
            scrollFields.ResumeLayout(false);
            scrollFields.PerformLayout();
            fields.ResumeLayout(false);
            fields.PerformLayout();
            buttonBar.ResumeLayout(false);
            buttonBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCapacityQty).EndInit();
            ((System.ComponentModel.ISupportInitialize)numCapacityKg).EndInit();
            ((System.ComponentModel.ISupportInitialize)numCapacityM3).EndInit();
            ((System.ComponentModel.ISupportInitialize)numFuelConsumption).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblPlateNumber;
        private System.Windows.Forms.Label lblVehicleType;
        private System.Windows.Forms.Label lblCapacityQty;
        private System.Windows.Forms.Label lblCapacityKg;
        private System.Windows.Forms.Label lblCapacityM3;
        private System.Windows.Forms.TextBox txtPlateNumber;
        private System.Windows.Forms.ComboBox cboVehicleType;
        private TruckingSystem.Desktop.Common.NumberBox numCapacityQty;
        private TruckingSystem.Desktop.Common.NumberBox numCapacityKg;
        private TruckingSystem.Desktop.Common.NumberBox numCapacityM3;
        private System.Windows.Forms.Label lblFuelConsumption;
        private TruckingSystem.Desktop.Common.NumberBox numFuelConsumption;
        private System.Windows.Forms.Label lblFuelType;
        private System.Windows.Forms.ComboBox cboFuelType;
        private System.Windows.Forms.Label lblWarehouse;
        private System.Windows.Forms.ComboBox cboWarehouse;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.Panel scrollFields;
        private System.Windows.Forms.TableLayoutPanel fields;
        private System.Windows.Forms.FlowLayoutPanel buttonBar;
    }
}
