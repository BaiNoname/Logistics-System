namespace TruckingSystem.Desktop.GUI_WarehouseManagement;

partial class EditZoneForm
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
        lblName = new System.Windows.Forms.Label();
        txtName = new System.Windows.Forms.TextBox();
        lblZoneType = new System.Windows.Forms.Label();
        cboZoneType = new System.Windows.Forms.ComboBox();
        lblPosX = new System.Windows.Forms.Label();
        lblPosY = new System.Windows.Forms.Label();
        numPosX = new System.Windows.Forms.NumericUpDown();
        numPosY = new System.Windows.Forms.NumericUpDown();
        lblWidth = new System.Windows.Forms.Label();
        lblHeight = new System.Windows.Forms.Label();
        numWidth = new System.Windows.Forms.NumericUpDown();
        numHeight = new System.Windows.Forms.NumericUpDown();
        lblNote = new System.Windows.Forms.Label();
        lblStatus = new System.Windows.Forms.Label();
        buttonBar = new System.Windows.Forms.FlowLayoutPanel();
        btnSave = new System.Windows.Forms.Button();
        root.SuspendLayout();
        scrollPanel.SuspendLayout();
        inputs.SuspendLayout();
        buttonBar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numPosX).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numPosY).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numWidth).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numHeight).BeginInit();
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
        root.Size = new System.Drawing.Size(480, 440);
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
        inputs.Controls.Add(lblName, 0, 0);
        inputs.Controls.Add(txtName, 0, 1);
        inputs.Controls.Add(lblZoneType, 0, 2);
        inputs.Controls.Add(cboZoneType, 0, 3);
        inputs.Controls.Add(lblPosX, 0, 4);
        inputs.Controls.Add(lblPosY, 1, 4);
        inputs.Controls.Add(numPosX, 0, 5);
        inputs.Controls.Add(numPosY, 1, 5);
        inputs.Controls.Add(lblWidth, 0, 6);
        inputs.Controls.Add(lblHeight, 1, 6);
        inputs.Controls.Add(numWidth, 0, 7);
        inputs.Controls.Add(numHeight, 1, 7);
        inputs.Controls.Add(lblNote, 0, 8);
        inputs.Dock = System.Windows.Forms.DockStyle.Top;
        inputs.Margin = new System.Windows.Forms.Padding(0);
        inputs.Name = "inputs";
        inputs.RowCount = 9;
        inputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        inputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        inputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        inputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        inputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        inputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        inputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        inputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        inputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        inputs.SetColumnSpan(lblName, 2);
        inputs.SetColumnSpan(txtName, 2);
        inputs.SetColumnSpan(lblZoneType, 2);
        inputs.SetColumnSpan(cboZoneType, 2);
        inputs.SetColumnSpan(lblNote, 2);
        inputs.TabIndex = 0;
        // 
        // lblName
        // 
        lblName.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblName.AutoSize = true;
        lblName.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblName.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblName.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
        lblName.Name = "lblName";
        lblName.TabIndex = 0;
        lblName.Text = "Tên khu vực * (VD: Khu C - Hàng đông lạnh)";
        // 
        // txtName
        // 
        txtName.Dock = System.Windows.Forms.DockStyle.Fill;
        txtName.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
        txtName.Name = "txtName";
        txtName.TabIndex = 1;
        // 
        // lblZoneType
        // 
        lblZoneType.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblZoneType.AutoSize = true;
        lblZoneType.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblZoneType.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblZoneType.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
        lblZoneType.Name = "lblZoneType";
        lblZoneType.TabIndex = 2;
        lblZoneType.Text = "Loại hàng của khu vực";
        // 
        // cboZoneType
        // 
        cboZoneType.Dock = System.Windows.Forms.DockStyle.Fill;
        cboZoneType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        cboZoneType.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
        cboZoneType.Name = "cboZoneType";
        cboZoneType.TabIndex = 3;
        // 
        // lblPosX
        // 
        lblPosX.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblPosX.AutoSize = true;
        lblPosX.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblPosX.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblPosX.Margin = new System.Windows.Forms.Padding(0, 0, 8, 4);
        lblPosX.Name = "lblPosX";
        lblPosX.TabIndex = 4;
        lblPosX.Text = "Vị trí X (mét, tính từ góc trái kho)";
        // 
        // lblPosY
        // 
        lblPosY.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblPosY.AutoSize = true;
        lblPosY.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblPosY.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblPosY.Margin = new System.Windows.Forms.Padding(8, 0, 0, 4);
        lblPosY.Name = "lblPosY";
        lblPosY.TabIndex = 5;
        lblPosY.Text = "Vị trí Y (mét, tính từ góc trên kho)";
        // 
        // numPosX
        // 
        numPosX.Dock = System.Windows.Forms.DockStyle.Fill;
        numPosX.DecimalPlaces = 1;
        numPosX.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        numPosX.Margin = new System.Windows.Forms.Padding(0, 0, 8, 12);
        numPosX.Name = "numPosX";
        numPosX.TabIndex = 6;
        // 
        // numPosY
        // 
        numPosY.Dock = System.Windows.Forms.DockStyle.Fill;
        numPosY.DecimalPlaces = 1;
        numPosY.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        numPosY.Margin = new System.Windows.Forms.Padding(8, 0, 0, 12);
        numPosY.Name = "numPosY";
        numPosY.TabIndex = 7;
        // 
        // lblWidth
        // 
        lblWidth.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblWidth.AutoSize = true;
        lblWidth.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblWidth.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblWidth.Margin = new System.Windows.Forms.Padding(0, 0, 8, 4);
        lblWidth.Name = "lblWidth";
        lblWidth.TabIndex = 8;
        lblWidth.Text = "Chiều rộng khu vực (mét) *";
        // 
        // lblHeight
        // 
        lblHeight.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblHeight.AutoSize = true;
        lblHeight.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblHeight.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblHeight.Margin = new System.Windows.Forms.Padding(8, 0, 0, 4);
        lblHeight.Name = "lblHeight";
        lblHeight.TabIndex = 9;
        lblHeight.Text = "Chiều cao khu vực (mét) *";
        // 
        // numWidth
        // 
        numWidth.Dock = System.Windows.Forms.DockStyle.Fill;
        numWidth.DecimalPlaces = 1;
        numWidth.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        numWidth.Value = new decimal(new int[] { 10, 0, 0, 0 });
        numWidth.Margin = new System.Windows.Forms.Padding(0, 0, 8, 12);
        numWidth.Name = "numWidth";
        numWidth.TabIndex = 10;
        // 
        // numHeight
        // 
        numHeight.Dock = System.Windows.Forms.DockStyle.Fill;
        numHeight.DecimalPlaces = 1;
        numHeight.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        numHeight.Value = new decimal(new int[] { 10, 0, 0, 0 });
        numHeight.Margin = new System.Windows.Forms.Padding(8, 0, 0, 12);
        numHeight.Name = "numHeight";
        numHeight.TabIndex = 11;
        // 
        // lblNote
        // 
        lblNote.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblNote.AutoSize = true;
        lblNote.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblNote.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblNote.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
        lblNote.Name = "lblNote";
        lblNote.TabIndex = 12;
        lblNote.Text = "💡 Đảm bảo (Vị trí X + Rộng) và (Vị trí Y + Cao) không vượt quá kích thước tổng của kho, tránh đè lên khu vực khác.";
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
        btnSave.Text = "Lưu khu vực";
        btnSave.UseVisualStyleBackColor = false;
        btnSave.Click += new System.EventHandler(this.BtnSave_Click);
        // 
        // EditZoneForm
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.White;
        ClientSize = new System.Drawing.Size(480, 440);
        Controls.Add(root);
        MinimumSize = new System.Drawing.Size(420, 360);
        Name = "EditZoneForm";
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        Text = "Sửa khu vực";
        ((System.ComponentModel.ISupportInitialize)numPosX).EndInit();
        ((System.ComponentModel.ISupportInitialize)numPosY).EndInit();
        ((System.ComponentModel.ISupportInitialize)numWidth).EndInit();
        ((System.ComponentModel.ISupportInitialize)numHeight).EndInit();
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
    private System.Windows.Forms.Label lblName;
    private System.Windows.Forms.TextBox txtName;
    private System.Windows.Forms.Label lblZoneType;
    private System.Windows.Forms.ComboBox cboZoneType;
    private System.Windows.Forms.Label lblPosX;
    private System.Windows.Forms.Label lblPosY;
    private System.Windows.Forms.NumericUpDown numPosX;
    private System.Windows.Forms.NumericUpDown numPosY;
    private System.Windows.Forms.Label lblWidth;
    private System.Windows.Forms.Label lblHeight;
    private System.Windows.Forms.NumericUpDown numWidth;
    private System.Windows.Forms.NumericUpDown numHeight;
    private System.Windows.Forms.Label lblNote;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.FlowLayoutPanel buttonBar;
    private System.Windows.Forms.Button btnSave;
}
