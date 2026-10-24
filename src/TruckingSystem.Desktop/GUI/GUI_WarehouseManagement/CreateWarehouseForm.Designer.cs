namespace TruckingSystem.Desktop.GUI_WarehouseManagement;

partial class CreateWarehouseForm
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
        root = new TableLayoutPanel();
        scrollPanel = new Panel();
        inputs = new TableLayoutPanel();
        lblName = new Label();
        txtName = new TextBox();
        lblAddress = new Label();
        txtSearch = new TextBox();
        lstSuggestions = new ListBox();
        lblSelectedAddress = new Label();
        lblCapacity = new Label();
        numCapacity = new NumericUpDown();
        lblLength = new Label();
        lblWidth = new Label();
        numLength = new NumericUpDown();
        numWidth = new NumericUpDown();
        lblAreaNote = new Label();
        lblDoorSide = new Label();
        lblDoorOffset = new Label();
        cboDoorSide = new ComboBox();
        numDoorOffset = new NumericUpDown();
        lblDoorNote = new Label();
        lblStorageFee = new Label();
        numStorageFee = new NumericUpDown();
        lblStatus = new Label();
        buttonBar = new FlowLayoutPanel();
        btnSave = new Button();
        root.SuspendLayout();
        scrollPanel.SuspendLayout();
        inputs.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numCapacity).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numLength).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numWidth).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numDoorOffset).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numStorageFee).BeginInit();
        buttonBar.SuspendLayout();
        SuspendLayout();
        // 
        // root
        // 
        root.ColumnCount = 1;
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.Controls.Add(scrollPanel, 0, 0);
        root.Controls.Add(lblStatus, 0, 1);
        root.Controls.Add(buttonBar, 0, 2);
        root.Dock = DockStyle.Fill;
        root.Location = new Point(0, 0);
        root.Margin = new Padding(0);
        root.Name = "root";
        root.Padding = new Padding(30, 24, 30, 24);
        root.RowCount = 3;
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle());
        root.RowStyles.Add(new RowStyle());
        root.Size = new Size(750, 900);
        root.TabIndex = 0;
        // 
        // scrollPanel
        // 
        scrollPanel.AutoScroll = true;
        scrollPanel.Controls.Add(inputs);
        scrollPanel.Dock = DockStyle.Fill;
        scrollPanel.Location = new Point(30, 24);
        scrollPanel.Margin = new Padding(0);
        scrollPanel.Name = "scrollPanel";
        scrollPanel.Size = new Size(690, 746);
        scrollPanel.TabIndex = 0;
        // 
        // inputs
        // 
        inputs.AutoSize = true;
        inputs.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        inputs.ColumnCount = 2;
        inputs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        inputs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        inputs.Controls.Add(lblName, 0, 0);
        inputs.Controls.Add(txtName, 0, 1);
        inputs.Controls.Add(lblAddress, 0, 2);
        inputs.Controls.Add(txtSearch, 0, 3);
        inputs.Controls.Add(lstSuggestions, 0, 4);
        inputs.Controls.Add(lblSelectedAddress, 0, 5);
        inputs.Controls.Add(lblCapacity, 0, 6);
        inputs.Controls.Add(numCapacity, 0, 7);
        inputs.Controls.Add(lblLength, 0, 8);
        inputs.Controls.Add(lblWidth, 1, 8);
        inputs.Controls.Add(numLength, 0, 9);
        inputs.Controls.Add(numWidth, 1, 9);
        inputs.Controls.Add(lblAreaNote, 0, 10);
        inputs.Controls.Add(lblDoorSide, 0, 11);
        inputs.Controls.Add(lblDoorOffset, 1, 11);
        inputs.Controls.Add(cboDoorSide, 0, 12);
        inputs.Controls.Add(numDoorOffset, 1, 12);
        inputs.Controls.Add(lblDoorNote, 0, 13);
        inputs.Controls.Add(lblStorageFee, 0, 14);
        inputs.Controls.Add(numStorageFee, 0, 15);
        inputs.Dock = DockStyle.Top;
        inputs.Location = new Point(0, 0);
        inputs.Margin = new Padding(0);
        inputs.Name = "inputs";
        inputs.RowCount = 16;
        inputs.RowStyles.Add(new RowStyle());
        inputs.RowStyles.Add(new RowStyle());
        inputs.RowStyles.Add(new RowStyle());
        inputs.RowStyles.Add(new RowStyle());
        inputs.RowStyles.Add(new RowStyle());
        inputs.RowStyles.Add(new RowStyle());
        inputs.RowStyles.Add(new RowStyle());
        inputs.RowStyles.Add(new RowStyle());
        inputs.RowStyles.Add(new RowStyle());
        inputs.RowStyles.Add(new RowStyle());
        inputs.RowStyles.Add(new RowStyle());
        inputs.RowStyles.Add(new RowStyle());
        inputs.RowStyles.Add(new RowStyle());
        inputs.RowStyles.Add(new RowStyle());
        inputs.RowStyles.Add(new RowStyle());
        inputs.RowStyles.Add(new RowStyle());
        inputs.Size = new Size(664, 832);
        inputs.TabIndex = 0;
        // 
        // lblName
        // 
        lblName.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblName.AutoSize = true;
        inputs.SetColumnSpan(lblName, 2);
        lblName.Font = new Font("Segoe UI", 9F);
        lblName.ForeColor = Color.FromArgb(100, 116, 139);
        lblName.Location = new Point(0, 0);
        lblName.Margin = new Padding(0, 0, 0, 6);
        lblName.Name = "lblName";
        lblName.Size = new Size(664, 25);
        lblName.TabIndex = 0;
        lblName.Text = "Tên kho * (VD: Kho Hà Nội)";
        // 
        // txtName
        // 
        inputs.SetColumnSpan(txtName, 2);
        txtName.Dock = DockStyle.Fill;
        txtName.Location = new Point(0, 31);
        txtName.Margin = new Padding(0, 0, 0, 18);
        txtName.Name = "txtName";
        txtName.Size = new Size(664, 34);
        txtName.TabIndex = 1;
        // 
        // lblAddress
        // 
        lblAddress.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblAddress.AutoSize = true;
        inputs.SetColumnSpan(lblAddress, 2);
        lblAddress.Font = new Font("Segoe UI", 9F);
        lblAddress.ForeColor = Color.FromArgb(100, 116, 139);
        lblAddress.Location = new Point(0, 83);
        lblAddress.Margin = new Padding(0, 0, 0, 6);
        lblAddress.Name = "lblAddress";
        lblAddress.Size = new Size(664, 25);
        lblAddress.TabIndex = 2;
        lblAddress.Text = "🔍 Địa chỉ kho * (gõ từ khóa rồi CHỌN từ gợi ý)";
        // 
        // txtSearch
        // 
        inputs.SetColumnSpan(txtSearch, 2);
        txtSearch.Dock = DockStyle.Fill;
        txtSearch.Location = new Point(0, 114);
        txtSearch.Margin = new Padding(0, 0, 0, 6);
        txtSearch.Name = "txtSearch";
        txtSearch.Size = new Size(664, 34);
        txtSearch.TabIndex = 3;
        // 
        // lstSuggestions
        // 
        inputs.SetColumnSpan(lstSuggestions, 2);
        lstSuggestions.Dock = DockStyle.Fill;
        lstSuggestions.Font = new Font("Segoe UI", 9F);
        lstSuggestions.IntegralHeight = false;
        lstSuggestions.ItemHeight = 25;
        lstSuggestions.Location = new Point(0, 154);
        lstSuggestions.Margin = new Padding(0, 0, 0, 6);
        lstSuggestions.MaximumSize = new Size(4, 223);
        lstSuggestions.MinimumSize = new Size(4, 133);
        lstSuggestions.Name = "lstSuggestions";
        lstSuggestions.Size = new Size(4, 142);
        lstSuggestions.TabIndex = 4;
        lstSuggestions.Visible = false;
        // 
        // lblSelectedAddress
        // 
        lblSelectedAddress.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblSelectedAddress.AutoSize = true;
        inputs.SetColumnSpan(lblSelectedAddress, 2);
        lblSelectedAddress.Font = new Font("Segoe UI", 9F);
        lblSelectedAddress.ForeColor = Color.FromArgb(22, 163, 74);
        lblSelectedAddress.Location = new Point(0, 302);
        lblSelectedAddress.Margin = new Padding(0, 0, 0, 18);
        lblSelectedAddress.Name = "lblSelectedAddress";
        lblSelectedAddress.Size = new Size(664, 25);
        lblSelectedAddress.TabIndex = 5;
        // 
        // lblCapacity
        // 
        lblCapacity.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblCapacity.AutoSize = true;
        inputs.SetColumnSpan(lblCapacity, 2);
        lblCapacity.Font = new Font("Segoe UI", 9F);
        lblCapacity.ForeColor = Color.FromArgb(100, 116, 139);
        lblCapacity.Location = new Point(0, 345);
        lblCapacity.Margin = new Padding(0, 0, 0, 6);
        lblCapacity.Name = "lblCapacity";
        lblCapacity.Size = new Size(664, 25);
        lblCapacity.TabIndex = 6;
        lblCapacity.Text = "Sức chứa (m³) — dùng để tính Best-Fit, tuỳ chọn";
        // 
        // numCapacity
        // 
        inputs.SetColumnSpan(numCapacity, 2);
        numCapacity.Dock = DockStyle.Fill;
        numCapacity.Increment = new decimal(new int[] { 10, 0, 0, 0 });
        numCapacity.Location = new Point(0, 376);
        numCapacity.Margin = new Padding(0, 0, 0, 18);
        numCapacity.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
        numCapacity.Name = "numCapacity";
        numCapacity.Size = new Size(664, 34);
        numCapacity.TabIndex = 7;
        // 
        // lblLength
        // 
        lblLength.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblLength.AutoSize = true;
        lblLength.Font = new Font("Segoe UI", 9F);
        lblLength.ForeColor = Color.FromArgb(100, 116, 139);
        lblLength.Location = new Point(0, 428);
        lblLength.Margin = new Padding(0, 0, 12, 6);
        lblLength.Name = "lblLength";
        lblLength.Size = new Size(320, 25);
        lblLength.TabIndex = 8;
        lblLength.Text = "Chiều dài (mét) *";
        // 
        // lblWidth
        // 
        lblWidth.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblWidth.AutoSize = true;
        lblWidth.Font = new Font("Segoe UI", 9F);
        lblWidth.ForeColor = Color.FromArgb(100, 116, 139);
        lblWidth.Location = new Point(344, 428);
        lblWidth.Margin = new Padding(12, 0, 0, 6);
        lblWidth.Name = "lblWidth";
        lblWidth.Size = new Size(320, 25);
        lblWidth.TabIndex = 9;
        lblWidth.Text = "Chiều rộng (mét) *";
        // 
        // numLength
        // 
        numLength.DecimalPlaces = 1;
        numLength.Dock = DockStyle.Fill;
        numLength.Location = new Point(0, 459);
        numLength.Margin = new Padding(0, 0, 12, 18);
        numLength.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        numLength.Name = "numLength";
        numLength.Size = new Size(320, 34);
        numLength.TabIndex = 10;
        numLength.Value = new decimal(new int[] { 30, 0, 0, 0 });
        // 
        // numWidth
        // 
        numWidth.DecimalPlaces = 1;
        numWidth.Dock = DockStyle.Fill;
        numWidth.Location = new Point(344, 459);
        numWidth.Margin = new Padding(12, 0, 0, 18);
        numWidth.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        numWidth.Name = "numWidth";
        numWidth.Size = new Size(320, 34);
        numWidth.TabIndex = 11;
        numWidth.Value = new decimal(new int[] { 20, 0, 0, 0 });
        // 
        // lblAreaNote
        // 
        lblAreaNote.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblAreaNote.AutoSize = true;
        inputs.SetColumnSpan(lblAreaNote, 2);
        lblAreaNote.Font = new Font("Segoe UI", 9F);
        lblAreaNote.ForeColor = Color.FromArgb(100, 116, 139);
        lblAreaNote.Location = new Point(0, 511);
        lblAreaNote.Margin = new Padding(0, 0, 0, 18);
        lblAreaNote.Name = "lblAreaNote";
        lblAreaNote.Size = new Size(664, 50);
        lblAreaNote.TabIndex = 12;
        lblAreaNote.Text = "💡 Diện tích sàn = Dài × Rộng — dùng để vẽ sơ đồ tổng quan kho đúng tỉ lệ thật ở màn Kho trung chuyển.";
        // 
        // lblDoorSide
        // 
        lblDoorSide.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblDoorSide.AutoSize = true;
        lblDoorSide.Font = new Font("Segoe UI", 9F);
        lblDoorSide.ForeColor = Color.FromArgb(100, 116, 139);
        lblDoorSide.Location = new Point(0, 579);
        lblDoorSide.Margin = new Padding(0, 0, 12, 6);
        lblDoorSide.Name = "lblDoorSide";
        lblDoorSide.Size = new Size(320, 25);
        lblDoorSide.TabIndex = 13;
        lblDoorSide.Text = "🚪 Cửa kho nằm ở";
        // 
        // lblDoorOffset
        // 
        lblDoorOffset.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblDoorOffset.AutoSize = true;
        lblDoorOffset.Font = new Font("Segoe UI", 9F);
        lblDoorOffset.ForeColor = Color.FromArgb(100, 116, 139);
        lblDoorOffset.Location = new Point(344, 579);
        lblDoorOffset.Margin = new Padding(12, 0, 0, 6);
        lblDoorOffset.Name = "lblDoorOffset";
        lblDoorOffset.Size = new Size(320, 25);
        lblDoorOffset.TabIndex = 14;
        lblDoorOffset.Text = "Cách góc (mét)";
        // 
        // cboDoorSide
        // 
        cboDoorSide.Dock = DockStyle.Fill;
        cboDoorSide.DropDownStyle = ComboBoxStyle.DropDownList;
        cboDoorSide.Location = new Point(0, 610);
        cboDoorSide.Margin = new Padding(0, 0, 12, 18);
        cboDoorSide.Name = "cboDoorSide";
        cboDoorSide.Size = new Size(320, 36);
        cboDoorSide.TabIndex = 15;
        // 
        // numDoorOffset
        // 
        numDoorOffset.DecimalPlaces = 1;
        numDoorOffset.Dock = DockStyle.Fill;
        numDoorOffset.Enabled = false;
        numDoorOffset.Location = new Point(344, 610);
        numDoorOffset.Margin = new Padding(12, 0, 0, 18);
        numDoorOffset.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        numDoorOffset.Name = "numDoorOffset";
        numDoorOffset.Size = new Size(320, 34);
        numDoorOffset.TabIndex = 16;
        // 
        // lblDoorNote
        // 
        lblDoorNote.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblDoorNote.AutoSize = true;
        inputs.SetColumnSpan(lblDoorNote, 2);
        lblDoorNote.Font = new Font("Segoe UI", 9F);
        lblDoorNote.ForeColor = Color.FromArgb(100, 116, 139);
        lblDoorNote.Location = new Point(0, 662);
        lblDoorNote.Margin = new Padding(0);
        lblDoorNote.Name = "lblDoorNote";
        lblDoorNote.Size = new Size(664, 50);
        lblDoorNote.TabIndex = 17;
        lblDoorNote.Text = "Tường trên/dưới: đo từ góc TRÁI · Tường trái/phải: đo từ góc TRÊN. Hệ thống tự tính khoảng cách tới cửa của từng ô.";
        // 
        // lblStorageFee
        // 
        lblStorageFee.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblStorageFee.AutoSize = true;
        inputs.SetColumnSpan(lblStorageFee, 2);
        lblStorageFee.Font = new Font("Segoe UI", 9F);
        lblStorageFee.ForeColor = Color.FromArgb(100, 116, 139);
        lblStorageFee.Location = new Point(0, 724);
        lblStorageFee.Margin = new Padding(0, 12, 0, 6);
        lblStorageFee.Name = "lblStorageFee";
        lblStorageFee.Size = new Size(664, 50);
        lblStorageFee.TabIndex = 18;
        lblStorageFee.Text = "💰 Phí lưu kho (đồng / m³ / ngày) — tính cho khách khi KHÁCH xin dời hạn giao trễ hơn; 0 = không tính";
        // 
        // numStorageFee
        // 
        inputs.SetColumnSpan(numStorageFee, 2);
        numStorageFee.Dock = DockStyle.Fill;
        numStorageFee.Increment = new decimal(new int[] { 1000, 0, 0, 0 });
        numStorageFee.Location = new Point(0, 780);
        numStorageFee.Margin = new Padding(0, 0, 0, 18);
        numStorageFee.Maximum = new decimal(new int[] { 100000000, 0, 0, 0 });
        numStorageFee.Name = "numStorageFee";
        numStorageFee.Size = new Size(664, 34);
        numStorageFee.TabIndex = 19;
        numStorageFee.ThousandsSeparator = true;
        // 
        // lblStatus
        // 
        lblStatus.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblStatus.AutoSize = true;
        lblStatus.Font = new Font("Segoe UI", 9F);
        lblStatus.ForeColor = Color.FromArgb(220, 38, 38);
        lblStatus.Location = new Point(30, 782);
        lblStatus.Margin = new Padding(0, 12, 0, 12);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(690, 25);
        lblStatus.TabIndex = 1;
        // 
        // buttonBar
        // 
        buttonBar.AutoSize = true;
        buttonBar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        buttonBar.Controls.Add(btnSave);
        buttonBar.Dock = DockStyle.Fill;
        buttonBar.FlowDirection = FlowDirection.RightToLeft;
        buttonBar.Location = new Point(30, 819);
        buttonBar.Margin = new Padding(0);
        buttonBar.Name = "buttonBar";
        buttonBar.Size = new Size(690, 57);
        buttonBar.TabIndex = 2;
        // 
        // btnSave
        // 
        btnSave.AutoSize = true;
        btnSave.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        btnSave.BackColor = Color.FromArgb(37, 99, 235);
        btnSave.Cursor = Cursors.Hand;
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
        btnSave.FlatStyle = FlatStyle.Flat;
        btnSave.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnSave.ForeColor = Color.White;
        btnSave.Location = new Point(519, 0);
        btnSave.Margin = new Padding(0);
        btnSave.MinimumSize = new Size(0, 57);
        btnSave.Name = "btnSave";
        btnSave.Padding = new Padding(15, 6, 15, 6);
        btnSave.Size = new Size(171, 57);
        btnSave.TabIndex = 0;
        btnSave.Text = "Lưu kho mới";
        btnSave.UseVisualStyleBackColor = false;
        btnSave.Click += BtnSave_Click;
        // 
        // CreateWarehouseForm
        // 
        AutoScaleDimensions = new SizeF(144F, 144F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.White;
        ClientSize = new Size(750, 900);
        Controls.Add(root);
        Margin = new Padding(6);
        MinimumSize = new Size(649, 632);
        Name = "CreateWarehouseForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Thêm kho mới";
        root.ResumeLayout(false);
        root.PerformLayout();
        scrollPanel.ResumeLayout(false);
        scrollPanel.PerformLayout();
        inputs.ResumeLayout(false);
        inputs.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)numCapacity).EndInit();
        ((System.ComponentModel.ISupportInitialize)numLength).EndInit();
        ((System.ComponentModel.ISupportInitialize)numWidth).EndInit();
        ((System.ComponentModel.ISupportInitialize)numDoorOffset).EndInit();
        ((System.ComponentModel.ISupportInitialize)numStorageFee).EndInit();
        buttonBar.ResumeLayout(false);
        buttonBar.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel root;
    private System.Windows.Forms.Panel scrollPanel;
    private System.Windows.Forms.TableLayoutPanel inputs;
    private System.Windows.Forms.Label lblName;
    private System.Windows.Forms.TextBox txtName;
    private System.Windows.Forms.Label lblAddress;
    private System.Windows.Forms.TextBox txtSearch;
    private System.Windows.Forms.ListBox lstSuggestions;
    private System.Windows.Forms.Label lblSelectedAddress;
    private System.Windows.Forms.Label lblCapacity;
    private System.Windows.Forms.NumericUpDown numCapacity;
    private System.Windows.Forms.Label lblStorageFee;
    private System.Windows.Forms.NumericUpDown numStorageFee;
    private System.Windows.Forms.Label lblLength;
    private System.Windows.Forms.Label lblWidth;
    private System.Windows.Forms.NumericUpDown numLength;
    private System.Windows.Forms.NumericUpDown numWidth;
    private System.Windows.Forms.Label lblAreaNote;
    private System.Windows.Forms.Label lblDoorSide;
    private System.Windows.Forms.Label lblDoorOffset;
    private System.Windows.Forms.ComboBox cboDoorSide;
    private System.Windows.Forms.NumericUpDown numDoorOffset;
    private System.Windows.Forms.Label lblDoorNote;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.FlowLayoutPanel buttonBar;
    private System.Windows.Forms.Button btnSave;
}
