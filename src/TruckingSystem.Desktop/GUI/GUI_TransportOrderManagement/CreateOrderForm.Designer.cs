namespace TruckingSystem.Desktop.GUI_TransportOrderManagement;

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
        this.lblCargoType = new System.Windows.Forms.Label();
        this.txtCargoType = new System.Windows.Forms.TextBox();
        this.lblPackageCount = new System.Windows.Forms.Label();
        this.numPackageCount = new System.Windows.Forms.NumericUpDown();
        this.chkFragile = new System.Windows.Forms.CheckBox();
        this.lblWeightKg = new System.Windows.Forms.Label();
        this.numWeightKg = new System.Windows.Forms.NumericUpDown();
        this.chkHazmat = new System.Windows.Forms.CheckBox();
        this.lblVolumeM3 = new System.Windows.Forms.Label();
        this.numVolumeM3 = new System.Windows.Forms.NumericUpDown();
        this.lblQtyTotal = new System.Windows.Forms.Label();
        this.numQtyTotal = new System.Windows.Forms.NumericUpDown();
        this.btnSave = new System.Windows.Forms.Button();
        this.lblStatus = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)(this.numPackageCount)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numWeightKg)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numVolumeM3)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numQtyTotal)).BeginInit();
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
        // lblCargoType
        //
        this.lblCargoType.Location = new System.Drawing.Point(15, 243);
        this.lblCargoType.Size = new System.Drawing.Size(130, 23);
        this.lblCargoType.Text = "Loại hàng:";
        //
        // txtCargoType
        //
        this.txtCargoType.Location = new System.Drawing.Point(150, 240);
        this.txtCargoType.Size = new System.Drawing.Size(300, 23);
        this.txtCargoType.Text = "Hàng khô đóng kiện";
        //
        // lblPackageCount
        //
        this.lblPackageCount.Location = new System.Drawing.Point(15, 278);
        this.lblPackageCount.Size = new System.Drawing.Size(130, 23);
        this.lblPackageCount.Text = "Số kiện:";
        //
        // numPackageCount
        //
        this.numPackageCount.Location = new System.Drawing.Point(150, 275);
        this.numPackageCount.Size = new System.Drawing.Size(120, 23);
        this.numPackageCount.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
        //
        // chkFragile
        //
        this.chkFragile.Location = new System.Drawing.Point(300, 277);
        this.chkFragile.Size = new System.Drawing.Size(90, 24);
        this.chkFragile.Text = "Dễ vỡ";
        //
        // lblWeightKg
        //
        this.lblWeightKg.Location = new System.Drawing.Point(15, 313);
        this.lblWeightKg.Size = new System.Drawing.Size(130, 23);
        this.lblWeightKg.Text = "Trọng lượng (kg):";
        //
        // numWeightKg
        //
        this.numWeightKg.Location = new System.Drawing.Point(150, 310);
        this.numWeightKg.Size = new System.Drawing.Size(120, 23);
        this.numWeightKg.DecimalPlaces = 2;
        this.numWeightKg.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
        //
        // chkHazmat
        //
        this.chkHazmat.Location = new System.Drawing.Point(300, 310);
        this.chkHazmat.Size = new System.Drawing.Size(130, 24);
        this.chkHazmat.Text = "Hàng nguy hiểm";
        //
        // lblVolumeM3
        //
        this.lblVolumeM3.Location = new System.Drawing.Point(15, 348);
        this.lblVolumeM3.Size = new System.Drawing.Size(130, 23);
        this.lblVolumeM3.Text = "Thể tích (m³):";
        //
        // numVolumeM3
        //
        this.numVolumeM3.Location = new System.Drawing.Point(150, 345);
        this.numVolumeM3.Size = new System.Drawing.Size(120, 23);
        this.numVolumeM3.DecimalPlaces = 2;
        this.numVolumeM3.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
        //
        // lblQtyTotal
        //
        this.lblQtyTotal.Location = new System.Drawing.Point(15, 383);
        this.lblQtyTotal.Size = new System.Drawing.Size(150, 23);
        this.lblQtyTotal.Text = "Số lượng (để chia xe):";
        //
        // numQtyTotal
        //
        this.numQtyTotal.Location = new System.Drawing.Point(150, 380);
        this.numQtyTotal.Size = new System.Drawing.Size(120, 23);
        this.numQtyTotal.DecimalPlaces = 2;
        this.numQtyTotal.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
        //
        // btnSave
        //
        this.btnSave.Location = new System.Drawing.Point(150, 420);
        this.btnSave.Size = new System.Drawing.Size(150, 38);
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
        this.lblStatus.Location = new System.Drawing.Point(150, 460);
        this.lblStatus.Size = new System.Drawing.Size(320, 40);
        this.lblStatus.ForeColor = System.Drawing.Color.Red;
        //
        // CreateOrderForm
        //
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.BackColor = System.Drawing.Color.White;
        this.ClientSize = new System.Drawing.Size(500, 560);
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
        this.Controls.Add(this.lblCargoType);
        this.Controls.Add(this.txtCargoType);
        this.Controls.Add(this.lblPackageCount);
        this.Controls.Add(this.numPackageCount);
        this.Controls.Add(this.chkFragile);
        this.Controls.Add(this.lblWeightKg);
        this.Controls.Add(this.numWeightKg);
        this.Controls.Add(this.chkHazmat);
        this.Controls.Add(this.lblVolumeM3);
        this.Controls.Add(this.numVolumeM3);
        this.Controls.Add(this.lblQtyTotal);
        this.Controls.Add(this.numQtyTotal);
        this.Controls.Add(this.btnSave);
        this.Controls.Add(this.lblStatus);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.Name = "CreateOrderForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Tạo đơn vận chuyển mới";
        ((System.ComponentModel.ISupportInitialize)(this.numPackageCount)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numWeightKg)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numVolumeM3)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numQtyTotal)).EndInit();
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
    private System.Windows.Forms.Label lblCargoType;
    private System.Windows.Forms.TextBox txtCargoType;
    private System.Windows.Forms.Label lblPackageCount;
    private System.Windows.Forms.NumericUpDown numPackageCount;
    private System.Windows.Forms.CheckBox chkFragile;
    private System.Windows.Forms.Label lblWeightKg;
    private System.Windows.Forms.NumericUpDown numWeightKg;
    private System.Windows.Forms.CheckBox chkHazmat;
    private System.Windows.Forms.Label lblVolumeM3;
    private System.Windows.Forms.NumericUpDown numVolumeM3;
    private System.Windows.Forms.Label lblQtyTotal;
    private System.Windows.Forms.NumericUpDown numQtyTotal;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.Label lblStatus;
}
