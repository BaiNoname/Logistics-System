namespace TruckingSystem.Desktop.GUI_TripManagement;

partial class RecordFuelForm
{
    private System.ComponentModel.IContainer components = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>Bố cục: root (TableLayoutPanel 1 cột) = tiêu đề / loại nhiên liệu / lưới lịch sử
    /// (chiếm phần còn lại) / hàng tổng + nút xóa / nhóm "Thêm lần đổ xăng" (ô nhập giãn theo %
    /// bề ngang) / trạng thái / thanh nút Đóng. Không có tọa độ cứng.</summary>
    private void InitializeComponent()
    {
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
        System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
        root = new System.Windows.Forms.TableLayoutPanel();
        lblTitle = new System.Windows.Forms.Label();
        lblInfo = new System.Windows.Forms.Label();
        gridLogs = new System.Windows.Forms.DataGridView();
        colId = new System.Windows.Forms.DataGridViewTextBoxColumn();
        colTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
        colStation = new System.Windows.Forms.DataGridViewTextBoxColumn();
        colLiters = new System.Windows.Forms.DataGridViewTextBoxColumn();
        colPrice = new System.Windows.Forms.DataGridViewTextBoxColumn();
        colCost = new System.Windows.Forms.DataGridViewTextBoxColumn();
        totalRow = new System.Windows.Forms.TableLayoutPanel();
        lblTotal = new System.Windows.Forms.Label();
        btnDelete = new System.Windows.Forms.Button();
        grpAdd = new System.Windows.Forms.GroupBox();
        addLayout = new System.Windows.Forms.TableLayoutPanel();
        addFields = new System.Windows.Forms.TableLayoutPanel();
        lblProvider = new System.Windows.Forms.Label();
        cboProvider = new System.Windows.Forms.ComboBox();
        lblLiters = new System.Windows.Forms.Label();
        numLiters = new TruckingSystem.Desktop.Common.NumberBox();
        lblStationName = new System.Windows.Forms.Label();
        txtStationName = new System.Windows.Forms.TextBox();
        lblPrice = new System.Windows.Forms.Label();
        amountRow = new System.Windows.Forms.TableLayoutPanel();
        lblAmount = new System.Windows.Forms.Label();
        btnSave = new System.Windows.Forms.Button();
        lblStatus = new System.Windows.Forms.Label();
        buttonBar = new System.Windows.Forms.FlowLayoutPanel();
        btnClose = new System.Windows.Forms.Button();
        root.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridLogs).BeginInit();
        totalRow.SuspendLayout();
        grpAdd.SuspendLayout();
        addLayout.SuspendLayout();
        addFields.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numLiters).BeginInit();
        amountRow.SuspendLayout();
        buttonBar.SuspendLayout();
        SuspendLayout();
        //
        // root
        //
        root.ColumnCount = 1;
        root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        root.Controls.Add(lblTitle, 0, 0);
        root.Controls.Add(lblInfo, 0, 1);
        root.Controls.Add(gridLogs, 0, 2);
        root.Controls.Add(totalRow, 0, 3);
        root.Controls.Add(grpAdd, 0, 4);
        root.Controls.Add(lblStatus, 0, 5);
        root.Controls.Add(buttonBar, 0, 6);
        root.Dock = System.Windows.Forms.DockStyle.Fill;
        root.Name = "root";
        root.Padding = new System.Windows.Forms.Padding(20, 16, 20, 16);
        root.RowCount = 7;
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        root.Size = new System.Drawing.Size(780, 620);
        root.TabIndex = 0;
        //
        // lblTitle
        //
        lblTitle.AutoSize = true;
        lblTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
        lblTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
        lblTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
        lblTitle.Name = "lblTitle";
        lblTitle.TabIndex = 0;
        lblTitle.Text = "⛽ Nhiên liệu của chuyến";
        //
        // lblInfo
        //
        lblInfo.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblInfo.AutoSize = true;
        lblInfo.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblInfo.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblInfo.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
        lblInfo.Name = "lblInfo";
        lblInfo.TabIndex = 1;
        //
        // gridLogs
        //
        gridLogs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        gridLogs.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colId, colTime, colStation, colLiters, colPrice, colCost });
        gridLogs.Dock = System.Windows.Forms.DockStyle.Fill;
        gridLogs.Margin = new System.Windows.Forms.Padding(0);
        gridLogs.MinimumSize = new System.Drawing.Size(0, 100);
        gridLogs.Name = "gridLogs";
        gridLogs.TabIndex = 2;
        //
        // colId
        //
        colId.Name = "colId";
        colId.Visible = false;
        //
        // colTime
        //
        colTime.FillWeight = 18F;
        colTime.HeaderText = "Thời gian";
        colTime.Name = "colTime";
        //
        // colStation
        //
        colStation.FillWeight = 36F;
        colStation.HeaderText = "Hãng / cửa hàng";
        colStation.Name = "colStation";
        //
        // colLiters
        //
        dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
        colLiters.DefaultCellStyle = dataGridViewCellStyle1;
        colLiters.FillWeight = 12F;
        colLiters.HeaderText = "Số lít";
        colLiters.Name = "colLiters";
        //
        // colPrice
        //
        dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
        colPrice.DefaultCellStyle = dataGridViewCellStyle2;
        colPrice.FillWeight = 15F;
        colPrice.HeaderText = "Đơn giá";
        colPrice.Name = "colPrice";
        //
        // colCost
        //
        dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
        colCost.DefaultCellStyle = dataGridViewCellStyle3;
        colCost.FillWeight = 17F;
        colCost.HeaderText = "Thành tiền";
        colCost.Name = "colCost";
        //
        // totalRow
        //
        totalRow.AutoSize = true;
        totalRow.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        totalRow.ColumnCount = 2;
        totalRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        totalRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        totalRow.Controls.Add(lblTotal, 0, 0);
        totalRow.Controls.Add(btnDelete, 1, 0);
        totalRow.Dock = System.Windows.Forms.DockStyle.Fill;
        totalRow.Margin = new System.Windows.Forms.Padding(0, 8, 0, 8);
        totalRow.Name = "totalRow";
        totalRow.RowCount = 1;
        totalRow.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        totalRow.TabIndex = 3;
        //
        // lblTotal
        //
        lblTotal.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblTotal.AutoSize = true;
        lblTotal.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        lblTotal.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
        lblTotal.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
        lblTotal.Name = "lblTotal";
        lblTotal.TabIndex = 0;
        //
        // btnDelete
        //
        btnDelete.Anchor = System.Windows.Forms.AnchorStyles.Right;
        btnDelete.AutoSize = true;
        btnDelete.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        btnDelete.BackColor = System.Drawing.Color.White;
        btnDelete.Cursor = System.Windows.Forms.Cursors.Hand;
        btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnDelete.Font = new System.Drawing.Font("Segoe UI", 9F);
        btnDelete.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
        btnDelete.Margin = new System.Windows.Forms.Padding(0);
        btnDelete.MinimumSize = new System.Drawing.Size(0, 34);
        btnDelete.Name = "btnDelete";
        btnDelete.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
        btnDelete.TabIndex = 1;
        btnDelete.Text = "🗑 Xóa lần đổ";
        btnDelete.UseVisualStyleBackColor = false;
        btnDelete.Click += new System.EventHandler(this.BtnDelete_Click);
        //
        // grpAdd
        //
        grpAdd.AutoSize = true;
        grpAdd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        grpAdd.Controls.Add(addLayout);
        grpAdd.Dock = System.Windows.Forms.DockStyle.Fill;
        grpAdd.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        grpAdd.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
        grpAdd.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
        grpAdd.Name = "grpAdd";
        grpAdd.Padding = new System.Windows.Forms.Padding(12, 8, 12, 12);
        grpAdd.TabIndex = 4;
        grpAdd.TabStop = false;
        grpAdd.Text = "Thêm lần đổ xăng";
        //
        // addLayout
        //
        addLayout.AutoSize = true;
        addLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        addLayout.ColumnCount = 1;
        addLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        addLayout.Controls.Add(addFields, 0, 0);
        addLayout.Controls.Add(lblPrice, 0, 1);
        addLayout.Controls.Add(amountRow, 0, 2);
        addLayout.Dock = System.Windows.Forms.DockStyle.Top;
        addLayout.Margin = new System.Windows.Forms.Padding(0);
        addLayout.Name = "addLayout";
        addLayout.RowCount = 3;
        addLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        addLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        addLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        addLayout.TabIndex = 0;
        //
        // addFields
        //
        addFields.AutoSize = true;
        addFields.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        addFields.ColumnCount = 3;
        addFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38F));
        addFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
        addFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 42F));
        addFields.Controls.Add(lblProvider, 0, 0);
        addFields.Controls.Add(lblLiters, 1, 0);
        addFields.Controls.Add(lblStationName, 2, 0);
        addFields.Controls.Add(cboProvider, 0, 1);
        addFields.Controls.Add(numLiters, 1, 1);
        addFields.Controls.Add(txtStationName, 2, 1);
        addFields.Dock = System.Windows.Forms.DockStyle.Fill;
        addFields.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
        addFields.Name = "addFields";
        addFields.RowCount = 2;
        addFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        addFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        addFields.TabIndex = 0;
        //
        // lblProvider
        //
        lblProvider.AutoSize = true;
        lblProvider.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblProvider.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblProvider.Margin = new System.Windows.Forms.Padding(0, 0, 12, 4);
        lblProvider.Name = "lblProvider";
        lblProvider.TabIndex = 0;
        lblProvider.Text = "Hãng nhiên liệu *";
        //
        // lblLiters
        //
        lblLiters.AutoSize = true;
        lblLiters.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblLiters.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblLiters.Margin = new System.Windows.Forms.Padding(0, 0, 12, 4);
        lblLiters.Name = "lblLiters";
        lblLiters.TabIndex = 1;
        lblLiters.Text = "Số lít *";
        //
        // lblStationName
        //
        lblStationName.AutoSize = true;
        lblStationName.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblStationName.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblStationName.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
        lblStationName.Name = "lblStationName";
        lblStationName.TabIndex = 2;
        lblStationName.Text = "Cửa hàng / ghi chú (tuỳ chọn)";
        //
        // cboProvider
        //
        cboProvider.Dock = System.Windows.Forms.DockStyle.Fill;
        cboProvider.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        cboProvider.Font = new System.Drawing.Font("Segoe UI", 10F);
        cboProvider.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
        cboProvider.Name = "cboProvider";
        cboProvider.TabIndex = 3;
        //
        // numLiters
        //
        numLiters.DecimalPlaces = 2;
        numLiters.Dock = System.Windows.Forms.DockStyle.Fill;
        numLiters.Font = new System.Drawing.Font("Segoe UI", 10F);
        numLiters.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
        numLiters.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
        numLiters.Name = "numLiters";
        numLiters.TabIndex = 4;
        numLiters.ThousandsSeparator = true;
        //
        // txtStationName
        //
        txtStationName.Dock = System.Windows.Forms.DockStyle.Fill;
        txtStationName.Font = new System.Drawing.Font("Segoe UI", 10F);
        txtStationName.Margin = new System.Windows.Forms.Padding(0);
        txtStationName.Name = "txtStationName";
        txtStationName.PlaceholderText = "VD: CHXD số 12 - QL1A";
        txtStationName.TabIndex = 5;
        //
        // lblPrice
        //
        lblPrice.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblPrice.AutoSize = true;
        lblPrice.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblPrice.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblPrice.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
        lblPrice.Name = "lblPrice";
        lblPrice.TabIndex = 1;
        //
        // amountRow
        //
        amountRow.AutoSize = true;
        amountRow.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        amountRow.ColumnCount = 2;
        amountRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        amountRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
        amountRow.Controls.Add(lblAmount, 0, 0);
        amountRow.Controls.Add(btnSave, 1, 0);
        amountRow.Dock = System.Windows.Forms.DockStyle.Fill;
        amountRow.Margin = new System.Windows.Forms.Padding(0);
        amountRow.Name = "amountRow";
        amountRow.RowCount = 1;
        amountRow.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        amountRow.TabIndex = 2;
        //
        // lblAmount
        //
        lblAmount.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblAmount.AutoSize = true;
        lblAmount.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
        lblAmount.ForeColor = System.Drawing.Color.FromArgb(37, 99, 235);
        lblAmount.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
        lblAmount.Name = "lblAmount";
        lblAmount.TabIndex = 0;
        //
        // btnSave
        //
        btnSave.Anchor = System.Windows.Forms.AnchorStyles.Right;
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
        btnSave.MinimumSize = new System.Drawing.Size(0, 36);
        btnSave.Name = "btnSave";
        btnSave.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
        btnSave.TabIndex = 1;
        btnSave.Text = "➕ Thêm";
        btnSave.UseVisualStyleBackColor = false;
        btnSave.Click += new System.EventHandler(this.BtnSave_Click);
        //
        // lblStatus
        //
        lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblStatus.AutoSize = true;
        lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblStatus.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
        lblStatus.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
        lblStatus.Name = "lblStatus";
        lblStatus.TabIndex = 5;
        //
        // buttonBar
        //
        buttonBar.AutoSize = true;
        buttonBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        buttonBar.Controls.Add(btnClose);
        buttonBar.Dock = System.Windows.Forms.DockStyle.Fill;
        buttonBar.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        buttonBar.Margin = new System.Windows.Forms.Padding(0);
        buttonBar.Name = "buttonBar";
        buttonBar.TabIndex = 6;
        buttonBar.WrapContents = true;
        //
        // btnClose
        //
        btnClose.AutoSize = true;
        btnClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        btnClose.BackColor = System.Drawing.Color.White;
        btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
        btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnClose.Font = new System.Drawing.Font("Segoe UI", 10F);
        btnClose.Margin = new System.Windows.Forms.Padding(0);
        btnClose.MinimumSize = new System.Drawing.Size(0, 34);
        btnClose.Name = "btnClose";
        btnClose.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
        btnClose.TabIndex = 0;
        btnClose.Text = "Đóng";
        btnClose.UseVisualStyleBackColor = false;
        btnClose.Click += new System.EventHandler(this.BtnClose_Click);
        //
        // RecordFuelForm
        //
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.White;
        ClientSize = new System.Drawing.Size(780, 620);
        Controls.Add(root);
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new System.Drawing.Size(640, 500);
        Name = "RecordFuelForm";
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        Text = "Nhiên liệu của chuyến";
        root.ResumeLayout(false);
        root.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)gridLogs).EndInit();
        totalRow.ResumeLayout(false);
        totalRow.PerformLayout();
        grpAdd.ResumeLayout(false);
        grpAdd.PerformLayout();
        addLayout.ResumeLayout(false);
        addLayout.PerformLayout();
        addFields.ResumeLayout(false);
        addFields.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)numLiters).EndInit();
        amountRow.ResumeLayout(false);
        amountRow.PerformLayout();
        buttonBar.ResumeLayout(false);
        buttonBar.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Label lblInfo;
    private System.Windows.Forms.DataGridView gridLogs;
    private System.Windows.Forms.Label lblTotal;
    private System.Windows.Forms.Button btnDelete;
    private System.Windows.Forms.GroupBox grpAdd;
    private System.Windows.Forms.Label lblProvider;
    private System.Windows.Forms.ComboBox cboProvider;
    private System.Windows.Forms.Label lblLiters;
    private TruckingSystem.Desktop.Common.NumberBox numLiters;
    private System.Windows.Forms.Label lblStationName;
    private System.Windows.Forms.TextBox txtStationName;
    private System.Windows.Forms.Label lblPrice;
    private System.Windows.Forms.Label lblAmount;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.Button btnClose;
    private System.Windows.Forms.TableLayoutPanel root;
    private System.Windows.Forms.DataGridViewTextBoxColumn colId;
    private System.Windows.Forms.DataGridViewTextBoxColumn colTime;
    private System.Windows.Forms.DataGridViewTextBoxColumn colStation;
    private System.Windows.Forms.DataGridViewTextBoxColumn colLiters;
    private System.Windows.Forms.DataGridViewTextBoxColumn colPrice;
    private System.Windows.Forms.DataGridViewTextBoxColumn colCost;
    private System.Windows.Forms.TableLayoutPanel totalRow;
    private System.Windows.Forms.TableLayoutPanel addLayout;
    private System.Windows.Forms.TableLayoutPanel addFields;
    private System.Windows.Forms.TableLayoutPanel amountRow;
    private System.Windows.Forms.FlowLayoutPanel buttonBar;
}
