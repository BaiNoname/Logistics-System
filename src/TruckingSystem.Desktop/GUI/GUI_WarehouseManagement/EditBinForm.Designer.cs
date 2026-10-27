namespace TruckingSystem.Desktop.GUI_WarehouseManagement;

partial class EditBinForm
{
    private System.ComponentModel.IContainer components = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>Bố cục: root (TableLayoutPanel 1 cột) = vùng nhập liệu cuộn được (Panel AutoScroll chứa
    /// TableLayoutPanel 2 cột bằng nhau: nhãn ở trên, ô nhập ở dưới, giãn theo bề ngang) / dòng trạng thái /
    /// thanh nút (phải sang trái). Không tọa độ cứng — co giãn theo DPI; màn nhỏ thì cuộn được.</summary>
    private void InitializeComponent()
    {
        root = new System.Windows.Forms.TableLayoutPanel();
        scrollPanel = new System.Windows.Forms.Panel();
        inputs = new System.Windows.Forms.TableLayoutPanel();
        lblZoneInfo = new System.Windows.Forms.Label();
        lblCode = new System.Windows.Forms.Label();
        txtCode = new System.Windows.Forms.TextBox();
        lblCapacity = new System.Windows.Forms.Label();
        lblMaxWeight = new System.Windows.Forms.Label();
        numCapacity = new TruckingSystem.Desktop.Common.NumberBox();
        numMaxWeight = new TruckingSystem.Desktop.Common.NumberBox();
        lblPosX = new System.Windows.Forms.Label();
        lblPosY = new System.Windows.Forms.Label();
        numPosX = new TruckingSystem.Desktop.Common.NumberBox();
        numPosY = new TruckingSystem.Desktop.Common.NumberBox();
        lblWidth = new System.Windows.Forms.Label();
        lblHeight = new System.Windows.Forms.Label();
        numWidth = new TruckingSystem.Desktop.Common.NumberBox();
        numHeight = new TruckingSystem.Desktop.Common.NumberBox();
        lblDistanceFromDoor = new System.Windows.Forms.Label();
        numDistanceFromDoor = new TruckingSystem.Desktop.Common.NumberBox();
        lblNote = new System.Windows.Forms.Label();
        lblStatus = new System.Windows.Forms.Label();
        buttonBar = new System.Windows.Forms.FlowLayoutPanel();
        btnSave = new System.Windows.Forms.Button();
        root.SuspendLayout();
        scrollPanel.SuspendLayout();
        inputs.SuspendLayout();
        buttonBar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numCapacity).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numMaxWeight).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numPosX).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numPosY).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numWidth).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numHeight).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numDistanceFromDoor).BeginInit();
        SuspendLayout();
        // 
        // root
        // 
        root.ColumnCount = 1;
        root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        root.Controls.Add(scrollPanel, 0, 0);
        root.Controls.Add(lblStatus, 0, 1);
        root.Controls.Add(buttonBar, 0, 2);
        root.Dock = System.Windows.Forms.DockStyle.Fill;
        root.Margin = new System.Windows.Forms.Padding(0);
        root.Name = "root";
        root.RowCount = 3;
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        root.TabIndex = 0;
        root.Padding = new System.Windows.Forms.Padding(20, 16, 20, 16);
        root.Size = new System.Drawing.Size(480, 540);
        // 
        // scrollPanel
        // 
        scrollPanel.AutoScroll = true;
        scrollPanel.Controls.Add(inputs);
        scrollPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        scrollPanel.Margin = new System.Windows.Forms.Padding(0);
        scrollPanel.Name = "scrollPanel";
        scrollPanel.TabIndex = 0;
        // 
        // inputs
        // 
        inputs.AutoSize = true;
        inputs.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        inputs.ColumnCount = 2;
        inputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        inputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        inputs.Controls.Add(lblZoneInfo, 0, 0);
        inputs.Controls.Add(lblCode, 0, 1);
        inputs.Controls.Add(txtCode, 0, 2);
        inputs.Controls.Add(lblCapacity, 0, 3);
        inputs.Controls.Add(lblMaxWeight, 1, 3);
        inputs.Controls.Add(numCapacity, 0, 4);
        inputs.Controls.Add(numMaxWeight, 1, 4);
        inputs.Controls.Add(lblPosX, 0, 5);
        inputs.Controls.Add(lblPosY, 1, 5);
        inputs.Controls.Add(numPosX, 0, 6);
        inputs.Controls.Add(numPosY, 1, 6);
        inputs.Controls.Add(lblWidth, 0, 7);
        inputs.Controls.Add(lblHeight, 1, 7);
        inputs.Controls.Add(numWidth, 0, 8);
        inputs.Controls.Add(numHeight, 1, 8);
        inputs.Controls.Add(lblDistanceFromDoor, 0, 9);
        inputs.Controls.Add(numDistanceFromDoor, 0, 10);
        inputs.Controls.Add(lblNote, 0, 11);
        inputs.Dock = System.Windows.Forms.DockStyle.Top;
        inputs.Margin = new System.Windows.Forms.Padding(0);
        inputs.Name = "inputs";
        inputs.RowCount = 12;
        inputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        inputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        inputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        inputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        inputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        inputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        inputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        inputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        inputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        inputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        inputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        inputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        inputs.SetColumnSpan(lblZoneInfo, 2);
        inputs.SetColumnSpan(lblCode, 2);
        inputs.SetColumnSpan(txtCode, 2);
        inputs.SetColumnSpan(lblDistanceFromDoor, 2);
        inputs.SetColumnSpan(numDistanceFromDoor, 2);
        inputs.SetColumnSpan(lblNote, 2);
        inputs.TabIndex = 0;
        // 
        // lblZoneInfo
        // 
        lblZoneInfo.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblZoneInfo.AutoSize = true;
        lblZoneInfo.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblZoneInfo.ForeColor = System.Drawing.Color.FromArgb(37, 99, 235);
        lblZoneInfo.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
        lblZoneInfo.Name = "lblZoneInfo";
        lblZoneInfo.TabIndex = 0;
        // 
        // lblCode
        // 
        lblCode.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblCode.AutoSize = true;
        lblCode.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblCode.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblCode.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
        lblCode.Name = "lblCode";
        lblCode.TabIndex = 1;
        lblCode.Text = "Mã vị trí * (VD: C-01)";
        // 
        // txtCode
        // 
        txtCode.Dock = System.Windows.Forms.DockStyle.Fill;
        txtCode.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
        txtCode.Name = "txtCode";
        txtCode.TabIndex = 2;
        // 
        // lblCapacity
        // 
        lblCapacity.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblCapacity.AutoSize = true;
        lblCapacity.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblCapacity.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblCapacity.Margin = new System.Windows.Forms.Padding(0, 0, 8, 4);
        lblCapacity.Name = "lblCapacity";
        lblCapacity.TabIndex = 3;
        lblCapacity.Text = "Sức chứa (m³) *";
        // 
        // lblMaxWeight
        // 
        lblMaxWeight.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblMaxWeight.AutoSize = true;
        lblMaxWeight.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblMaxWeight.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblMaxWeight.Margin = new System.Windows.Forms.Padding(8, 0, 0, 4);
        lblMaxWeight.Name = "lblMaxWeight";
        lblMaxWeight.TabIndex = 4;
        lblMaxWeight.Text = "Tải trọng tối đa (kg) *";
        // 
        // numCapacity
        // 
        numCapacity.Dock = System.Windows.Forms.DockStyle.Fill;
        numCapacity.DecimalPlaces = 2;
        numCapacity.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
        numCapacity.Value = new decimal(new int[] { 5, 0, 0, 0 });
        numCapacity.Margin = new System.Windows.Forms.Padding(0, 0, 8, 12);
        numCapacity.Name = "numCapacity";
        numCapacity.TabIndex = 5;
        // 
        // numMaxWeight
        // 
        numMaxWeight.Dock = System.Windows.Forms.DockStyle.Fill;
        numMaxWeight.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
        numMaxWeight.Value = new decimal(new int[] { 2000, 0, 0, 0 });
        numMaxWeight.Margin = new System.Windows.Forms.Padding(8, 0, 0, 12);
        numMaxWeight.Name = "numMaxWeight";
        numMaxWeight.TabIndex = 6;
        // 
        // lblPosX
        // 
        lblPosX.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblPosX.AutoSize = true;
        lblPosX.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblPosX.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblPosX.Margin = new System.Windows.Forms.Padding(0, 0, 8, 4);
        lblPosX.Name = "lblPosX";
        lblPosX.TabIndex = 7;
        lblPosX.Text = "Vị trí X (mét, so với góc khu vực) *";
        // 
        // lblPosY
        // 
        lblPosY.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblPosY.AutoSize = true;
        lblPosY.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblPosY.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblPosY.Margin = new System.Windows.Forms.Padding(8, 0, 0, 4);
        lblPosY.Name = "lblPosY";
        lblPosY.TabIndex = 8;
        lblPosY.Text = "Vị trí Y (mét, so với góc khu vực) *";
        // 
        // numPosX
        // 
        numPosX.Dock = System.Windows.Forms.DockStyle.Fill;
        numPosX.DecimalPlaces = 2;
        numPosX.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        numPosX.Margin = new System.Windows.Forms.Padding(0, 0, 8, 12);
        numPosX.Name = "numPosX";
        numPosX.TabIndex = 9;
        // 
        // numPosY
        // 
        numPosY.Dock = System.Windows.Forms.DockStyle.Fill;
        numPosY.DecimalPlaces = 2;
        numPosY.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        numPosY.Margin = new System.Windows.Forms.Padding(8, 0, 0, 12);
        numPosY.Name = "numPosY";
        numPosY.TabIndex = 10;
        // 
        // lblWidth
        // 
        lblWidth.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblWidth.AutoSize = true;
        lblWidth.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblWidth.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblWidth.Margin = new System.Windows.Forms.Padding(0, 0, 8, 4);
        lblWidth.Name = "lblWidth";
        lblWidth.TabIndex = 11;
        lblWidth.Text = "Chiều rộng vị trí (mét) *";
        // 
        // lblHeight
        // 
        lblHeight.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblHeight.AutoSize = true;
        lblHeight.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblHeight.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblHeight.Margin = new System.Windows.Forms.Padding(8, 0, 0, 4);
        lblHeight.Name = "lblHeight";
        lblHeight.TabIndex = 12;
        lblHeight.Text = "Chiều cao vị trí (mét) *";
        // 
        // numWidth
        // 
        numWidth.Dock = System.Windows.Forms.DockStyle.Fill;
        numWidth.DecimalPlaces = 2;
        numWidth.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        numWidth.Value = new decimal(new int[] { 1, 0, 0, 0 });
        numWidth.Margin = new System.Windows.Forms.Padding(0, 0, 8, 12);
        numWidth.Name = "numWidth";
        numWidth.TabIndex = 13;
        // 
        // numHeight
        // 
        numHeight.Dock = System.Windows.Forms.DockStyle.Fill;
        numHeight.DecimalPlaces = 2;
        numHeight.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        numHeight.Value = new decimal(new int[] { 1, 0, 0, 0 });
        numHeight.Margin = new System.Windows.Forms.Padding(8, 0, 0, 12);
        numHeight.Name = "numHeight";
        numHeight.TabIndex = 14;
        // 
        // lblDistanceFromDoor
        // 
        lblDistanceFromDoor.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblDistanceFromDoor.AutoSize = true;
        lblDistanceFromDoor.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblDistanceFromDoor.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblDistanceFromDoor.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
        lblDistanceFromDoor.Name = "lblDistanceFromDoor";
        lblDistanceFromDoor.TabIndex = 15;
        lblDistanceFromDoor.Text = "🚪 Khoảng cách tới cửa (mét) — hệ thống TỰ TÍNH từ vị trí cửa kho";
        // 
        // numDistanceFromDoor
        // 
        numDistanceFromDoor.Dock = System.Windows.Forms.DockStyle.Fill;
        numDistanceFromDoor.DecimalPlaces = 1;
        numDistanceFromDoor.Enabled = false;
        numDistanceFromDoor.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
        numDistanceFromDoor.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
        numDistanceFromDoor.Name = "numDistanceFromDoor";
        numDistanceFromDoor.TabIndex = 16;
        // 
        // lblNote
        // 
        lblNote.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblNote.AutoSize = true;
        lblNote.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblNote.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblNote.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
        lblNote.Name = "lblNote";
        lblNote.TabIndex = 17;
        lblNote.Text = "💡 Hệ thống tự kiểm tra: vị trí không được đè lên vị trí khác, không được vượt ra ngoài ranh giới khu vực.";
        // 
        // lblStatus
        // 
        lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblStatus.AutoSize = true;
        lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblStatus.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
        lblStatus.Margin = new System.Windows.Forms.Padding(0, 8, 0, 8);
        lblStatus.Name = "lblStatus";
        lblStatus.TabIndex = 1;
        // 
        // buttonBar
        // 
        buttonBar.AutoSize = true;
        buttonBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        buttonBar.Controls.Add(btnSave);
        buttonBar.Dock = System.Windows.Forms.DockStyle.Fill;
        buttonBar.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
        buttonBar.Margin = new System.Windows.Forms.Padding(0);
        buttonBar.Name = "buttonBar";
        buttonBar.TabIndex = 2;
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
        btnSave.MinimumSize = new System.Drawing.Size(0, 38);
        btnSave.Name = "btnSave";
        btnSave.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
        btnSave.TabIndex = 0;
        btnSave.Text = "Lưu thay đổi";
        btnSave.UseVisualStyleBackColor = false;
        btnSave.Click += new System.EventHandler(this.BtnSave_Click);
        // 
        // EditBinForm
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.White;
        ClientSize = new System.Drawing.Size(480, 540);
        Controls.Add(root);
        MinimumSize = new System.Drawing.Size(420, 400);
        Name = "EditBinForm";
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        Text = "Sửa vị trí lưu trữ";
        ((System.ComponentModel.ISupportInitialize)numCapacity).EndInit();
        ((System.ComponentModel.ISupportInitialize)numMaxWeight).EndInit();
        ((System.ComponentModel.ISupportInitialize)numPosX).EndInit();
        ((System.ComponentModel.ISupportInitialize)numPosY).EndInit();
        ((System.ComponentModel.ISupportInitialize)numWidth).EndInit();
        ((System.ComponentModel.ISupportInitialize)numHeight).EndInit();
        ((System.ComponentModel.ISupportInitialize)numDistanceFromDoor).EndInit();
        buttonBar.ResumeLayout(false);
        buttonBar.PerformLayout();
        inputs.ResumeLayout(false);
        inputs.PerformLayout();
        scrollPanel.ResumeLayout(false);
        scrollPanel.PerformLayout();
        root.ResumeLayout(false);
        root.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel root;
    private System.Windows.Forms.Panel scrollPanel;
    private System.Windows.Forms.TableLayoutPanel inputs;
    private System.Windows.Forms.Label lblZoneInfo;
    private System.Windows.Forms.Label lblCode;
    private System.Windows.Forms.TextBox txtCode;
    private System.Windows.Forms.Label lblCapacity;
    private System.Windows.Forms.Label lblMaxWeight;
    private TruckingSystem.Desktop.Common.NumberBox numCapacity;
    private TruckingSystem.Desktop.Common.NumberBox numMaxWeight;
    private System.Windows.Forms.Label lblPosX;
    private System.Windows.Forms.Label lblPosY;
    private TruckingSystem.Desktop.Common.NumberBox numPosX;
    private TruckingSystem.Desktop.Common.NumberBox numPosY;
    private System.Windows.Forms.Label lblWidth;
    private System.Windows.Forms.Label lblHeight;
    private TruckingSystem.Desktop.Common.NumberBox numWidth;
    private TruckingSystem.Desktop.Common.NumberBox numHeight;
    private System.Windows.Forms.Label lblDistanceFromDoor;
    private TruckingSystem.Desktop.Common.NumberBox numDistanceFromDoor;
    private System.Windows.Forms.Label lblNote;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.FlowLayoutPanel buttonBar;
    private System.Windows.Forms.Button btnSave;
}
