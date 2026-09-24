namespace TruckingSystem.Desktop.GUI_VehicleManagement
{
    partial class EditVehicleForm
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
            this.lblPlateNumber = new System.Windows.Forms.Label();
            this.lblVehicleType = new System.Windows.Forms.Label();
            this.lblCapacityQty = new System.Windows.Forms.Label();
            this.lblCapacityKg = new System.Windows.Forms.Label();
            this.lblCapacityM3 = new System.Windows.Forms.Label();
            this.lblStatusField = new System.Windows.Forms.Label();
            this.txtPlateNumber = new System.Windows.Forms.TextBox();
            this.txtVehicleType = new System.Windows.Forms.TextBox();
            this.numCapacityQty = new System.Windows.Forms.NumericUpDown();
            this.numCapacityKg = new System.Windows.Forms.NumericUpDown();
            this.numCapacityM3 = new System.Windows.Forms.NumericUpDown();
            this.cboStatus = new System.Windows.Forms.ComboBox();
            this.lblFuelConsumption = new System.Windows.Forms.Label();
            this.numFuelConsumption = new System.Windows.Forms.NumericUpDown();
            this.lblFuelType = new System.Windows.Forms.Label();
            this.cboFuelType = new System.Windows.Forms.ComboBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numCapacityQty)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCapacityKg)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCapacityM3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFuelConsumption)).BeginInit();
            this.SuspendLayout();
            //
            // lblPlateNumber
            //
            this.lblPlateNumber.Location = new System.Drawing.Point(15, 23);
            this.lblPlateNumber.Size = new System.Drawing.Size(130, 23);
            this.lblPlateNumber.Text = "Biển số xe:";
            //
            // lblVehicleType
            //
            this.lblVehicleType.Location = new System.Drawing.Point(15, 58);
            this.lblVehicleType.Size = new System.Drawing.Size(130, 23);
            this.lblVehicleType.Text = "Loại xe:";
            //
            // lblCapacityQty
            //
            this.lblCapacityQty.Location = new System.Drawing.Point(15, 93);
            this.lblCapacityQty.Size = new System.Drawing.Size(130, 23);
            this.lblCapacityQty.Text = "Sức chứa (đơn vị):";
            //
            // lblCapacityKg
            //
            this.lblCapacityKg.Location = new System.Drawing.Point(15, 128);
            this.lblCapacityKg.Size = new System.Drawing.Size(130, 23);
            this.lblCapacityKg.Text = "Sức chứa (kg):";
            //
            // lblCapacityM3
            //
            this.lblCapacityM3.Location = new System.Drawing.Point(15, 163);
            this.lblCapacityM3.Size = new System.Drawing.Size(130, 23);
            this.lblCapacityM3.Text = "Sức chứa (m³):";
            //
            // lblStatusField
            //
            this.lblStatusField.Location = new System.Drawing.Point(15, 198);
            this.lblStatusField.Size = new System.Drawing.Size(130, 23);
            this.lblStatusField.Text = "Trạng thái:";
            //
            // txtPlateNumber
            //
            this.txtPlateNumber.Location = new System.Drawing.Point(150, 20);
            this.txtPlateNumber.Size = new System.Drawing.Size(220, 23);
            //
            // txtVehicleType
            //
            this.txtVehicleType.Location = new System.Drawing.Point(150, 55);
            this.txtVehicleType.Size = new System.Drawing.Size(220, 23);
            //
            // numCapacityQty
            //
            this.numCapacityQty.Location = new System.Drawing.Point(150, 90);
            this.numCapacityQty.Size = new System.Drawing.Size(120, 23);
            this.numCapacityQty.DecimalPlaces = 2;
            this.numCapacityQty.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            //
            // numCapacityKg
            //
            this.numCapacityKg.Location = new System.Drawing.Point(150, 125);
            this.numCapacityKg.Size = new System.Drawing.Size(120, 23);
            this.numCapacityKg.DecimalPlaces = 2;
            this.numCapacityKg.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            //
            // numCapacityM3
            //
            this.numCapacityM3.Location = new System.Drawing.Point(150, 160);
            this.numCapacityM3.Size = new System.Drawing.Size(120, 23);
            this.numCapacityM3.DecimalPlaces = 2;
            this.numCapacityM3.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            //
            // cboStatus
            //
            this.cboStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboStatus.Location = new System.Drawing.Point(150, 195);
            this.cboStatus.Size = new System.Drawing.Size(220, 23);
            //
            // lblFuelConsumption
            //
            this.lblFuelConsumption.Location = new System.Drawing.Point(15, 233);
            this.lblFuelConsumption.Size = new System.Drawing.Size(130, 23);
            this.lblFuelConsumption.Text = "Định mức (L/100km):";
            //
            // numFuelConsumption
            //
            this.numFuelConsumption.Location = new System.Drawing.Point(150, 230);
            this.numFuelConsumption.Size = new System.Drawing.Size(120, 23);
            this.numFuelConsumption.DecimalPlaces = 1;
            this.numFuelConsumption.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
            //
            // lblFuelType
            //
            this.lblFuelType.Location = new System.Drawing.Point(15, 268);
            this.lblFuelType.Size = new System.Drawing.Size(130, 23);
            this.lblFuelType.Text = "Loại nhiên liệu:";
            //
            // cboFuelType
            //
            this.cboFuelType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFuelType.Location = new System.Drawing.Point(150, 265);
            this.cboFuelType.Size = new System.Drawing.Size(150, 23);
            //
            // btnSave
            //
            this.btnSave.Location = new System.Drawing.Point(150, 310);
            this.btnSave.Size = new System.Drawing.Size(150, 38);
            this.btnSave.Text = "Lưu thay đổi";
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
            this.lblStatus.Location = new System.Drawing.Point(15, 355);
            this.lblStatus.Size = new System.Drawing.Size(360, 40);
            this.lblStatus.ForeColor = System.Drawing.Color.Red;
            //
            // EditVehicleForm
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(400, 420);
            this.Controls.Add(this.lblPlateNumber);
            this.Controls.Add(this.lblVehicleType);
            this.Controls.Add(this.lblCapacityQty);
            this.Controls.Add(this.lblCapacityKg);
            this.Controls.Add(this.lblCapacityM3);
            this.Controls.Add(this.lblStatusField);
            this.Controls.Add(this.txtPlateNumber);
            this.Controls.Add(this.txtVehicleType);
            this.Controls.Add(this.numCapacityQty);
            this.Controls.Add(this.numCapacityKg);
            this.Controls.Add(this.numCapacityM3);
            this.Controls.Add(this.cboStatus);
            this.Controls.Add(this.lblFuelConsumption);
            this.Controls.Add(this.numFuelConsumption);
            this.Controls.Add(this.lblFuelType);
            this.Controls.Add(this.cboFuelType);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.lblStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "EditVehicleForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Sửa thông tin xe";
            ((System.ComponentModel.ISupportInitialize)(this.numCapacityQty)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCapacityKg)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCapacityM3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFuelConsumption)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblPlateNumber;
        private System.Windows.Forms.Label lblVehicleType;
        private System.Windows.Forms.Label lblCapacityQty;
        private System.Windows.Forms.Label lblCapacityKg;
        private System.Windows.Forms.Label lblCapacityM3;
        private System.Windows.Forms.Label lblStatusField;
        private System.Windows.Forms.TextBox txtPlateNumber;
        private System.Windows.Forms.TextBox txtVehicleType;
        private System.Windows.Forms.NumericUpDown numCapacityQty;
        private System.Windows.Forms.NumericUpDown numCapacityKg;
        private System.Windows.Forms.NumericUpDown numCapacityM3;
        private System.Windows.Forms.ComboBox cboStatus;
        private System.Windows.Forms.Label lblFuelConsumption;
        private System.Windows.Forms.NumericUpDown numFuelConsumption;
        private System.Windows.Forms.Label lblFuelType;
        private System.Windows.Forms.ComboBox cboFuelType;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label lblStatus;
    }
}
