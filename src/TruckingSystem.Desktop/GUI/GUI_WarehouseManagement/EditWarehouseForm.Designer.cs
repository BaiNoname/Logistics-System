namespace TruckingSystem.Desktop.GUI_WarehouseManagement;

partial class EditWarehouseForm
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
        lblAddress = new System.Windows.Forms.Label();
        txtSearch = new System.Windows.Forms.TextBox();
        lstSuggestions = new System.Windows.Forms.ListBox();
        lblSelectedAddress = new System.Windows.Forms.Label();
        lblCapacity = new System.Windows.Forms.Label();
        numCapacity = new System.Windows.Forms.NumericUpDown();
        lblLength = new System.Windows.Forms.Label();
        lblWidth = new System.Windows.Forms.Label();
        numLength = new System.Windows.Forms.NumericUpDown();
        numWidth = new System.Windows.Forms.NumericUpDown();
        lblAreaNote = new System.Windows.Forms.Label();
        lblDoorSide = new System.Windows.Forms.Label();
        lblDoorOffset = new System.Windows.Forms.Label();
        cboDoorSide = new System.Windows.Forms.ComboBox();
        numDoorOffset = new System.Windows.Forms.NumericUpDown();
        lblDoorNote = new System.Windows.Forms.Label();
        lblStatus = new System.Windows.Forms.Label();
        buttonBar = new System.Windows.Forms.FlowLayoutPanel();
        btnSave = new System.Windows.Forms.Button();
        root.SuspendLayout();
        scrollPanel.SuspendLayout();
        inputs.SuspendLayout();
        buttonBar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numCapacity).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numLength).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numWidth).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numDoorOffset).BeginInit();
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
        root.Size = new System.Drawing.Size(500, 600);
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
        inputs.Dock = System.Windows.Forms.DockStyle.Top;
        inputs.Margin = new System.Windows.Forms.Padding(0);
        inputs.Name = "inputs";
        inputs.RowCount = 14;
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
        inputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        inputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        inputs.SetColumnSpan(lblName, 2);
        inputs.SetColumnSpan(txtName, 2);
        inputs.SetColumnSpan(lblAddress, 2);
        inputs.SetColumnSpan(txtSearch, 2);
        inputs.SetColumnSpan(lstSuggestions, 2);
        inputs.SetColumnSpan(lblSelectedAddress, 2);
        inputs.SetColumnSpan(lblCapacity, 2);
        inputs.SetColumnSpan(numCapacity, 2);
        inputs.SetColumnSpan(lblAreaNote, 2);
        inputs.SetColumnSpan(lblDoorNote, 2);
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
        lblName.Text = "Tên kho * (VD: Kho Hà Nội)";
        // 
        // txtName
        // 
        txtName.Dock = System.Windows.Forms.DockStyle.Fill;
        txtName.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
        txtName.Name = "txtName";
        txtName.TabIndex = 1;
        // 
        // lblAddress
        // 
        lblAddress.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblAddress.AutoSize = true;
        lblAddress.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblAddress.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblAddress.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
        lblAddress.Name = "lblAddress";
        lblAddress.TabIndex = 2;
        lblAddress.Text = "🔍 Địa chỉ kho * (gõ từ khóa rồi CHỌN từ gợi ý)";
        // 
        // txtSearch
        // 
        txtSearch.Dock = System.Windows.Forms.DockStyle.Fill;
        txtSearch.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
        txtSearch.Name = "txtSearch";
        txtSearch.TabIndex = 3;
        // 
        // lstSuggestions
        // 
        lstSuggestions.Dock = System.Windows.Forms.DockStyle.Fill;
        lstSuggestions.Font = new System.Drawing.Font("Segoe UI", 9F);
        lstSuggestions.IntegralHeight = false;
        lstSuggestions.MaximumSize = new System.Drawing.Size(0, 150);
        lstSuggestions.MinimumSize = new System.Drawing.Size(0, 90);
        lstSuggestions.Visible = false;
        lstSuggestions.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
        lstSuggestions.Name = "lstSuggestions";
        lstSuggestions.TabIndex = 4;
        // 
        // lblSelectedAddress
        // 
        lblSelectedAddress.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblSelectedAddress.AutoSize = true;
        lblSelectedAddress.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblSelectedAddress.ForeColor = System.Drawing.Color.FromArgb(22, 163, 74);
        lblSelectedAddress.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
        lblSelectedAddress.Name = "lblSelectedAddress";
        lblSelectedAddress.TabIndex = 5;
        // 
        // lblCapacity
        // 
        lblCapacity.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblCapacity.AutoSize = true;
        lblCapacity.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblCapacity.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblCapacity.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
        lblCapacity.Name = "lblCapacity";
        lblCapacity.TabIndex = 6;
        lblCapacity.Text = "Sức chứa (m³) — dùng để tính Best-Fit, tuỳ chọn";
        // 
        // numCapacity
        // 
        numCapacity.Dock = System.Windows.Forms.DockStyle.Fill;
        numCapacity.DecimalPlaces = 0;
        numCapacity.Increment = new decimal(new int[] { 10, 0, 0, 0 });
        numCapacity.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
        numCapacity.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
        numCapacity.Name = "numCapacity";
        numCapacity.TabIndex = 7;
        // 
        // lblLength
        // 
        lblLength.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblLength.AutoSize = true;
        lblLength.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblLength.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblLength.Margin = new System.Windows.Forms.Padding(0, 0, 8, 4);
        lblLength.Name = "lblLength";
        lblLength.TabIndex = 8;
        lblLength.Text = "Chiều dài (mét) *";
        // 
        // lblWidth
        // 
        lblWidth.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblWidth.AutoSize = true;
        lblWidth.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblWidth.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblWidth.Margin = new System.Windows.Forms.Padding(8, 0, 0, 4);
        lblWidth.Name = "lblWidth";
        lblWidth.TabIndex = 9;
        lblWidth.Text = "Chiều rộng (mét) *";
        // 
        // numLength
        // 
        numLength.Dock = System.Windows.Forms.DockStyle.Fill;
        numLength.DecimalPlaces = 1;
        numLength.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        numLength.Value = new decimal(new int[] { 30, 0, 0, 0 });
        numLength.Margin = new System.Windows.Forms.Padding(0, 0, 8, 12);
        numLength.Name = "numLength";
        numLength.TabIndex = 10;
        // 
        // numWidth
        // 
        numWidth.Dock = System.Windows.Forms.DockStyle.Fill;
        numWidth.DecimalPlaces = 1;
        numWidth.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        numWidth.Value = new decimal(new int[] { 20, 0, 0, 0 });
        numWidth.Margin = new System.Windows.Forms.Padding(8, 0, 0, 12);
        numWidth.Name = "numWidth";
        numWidth.TabIndex = 11;
        // 
        // lblAreaNote
        // 
        lblAreaNote.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblAreaNote.AutoSize = true;
        lblAreaNote.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblAreaNote.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblAreaNote.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
        lblAreaNote.Name = "lblAreaNote";
        lblAreaNote.TabIndex = 12;
        lblAreaNote.Text = "💡 Diện tích sàn = Dài × Rộng — dùng để vẽ sơ đồ tổng quan kho đúng tỉ lệ thật ở màn Kho trung chuyển.";
        // 
        // lblDoorSide
        // 
        lblDoorSide.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblDoorSide.AutoSize = true;
        lblDoorSide.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblDoorSide.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblDoorSide.Margin = new System.Windows.Forms.Padding(0, 0, 8, 4);
        lblDoorSide.Name = "lblDoorSide";
        lblDoorSide.TabIndex = 13;
        lblDoorSide.Text = "🚪 Cửa kho nằm ở";
        // 
        // lblDoorOffset
        // 
        lblDoorOffset.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblDoorOffset.AutoSize = true;
        lblDoorOffset.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblDoorOffset.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblDoorOffset.Margin = new System.Windows.Forms.Padding(8, 0, 0, 4);
        lblDoorOffset.Name = "lblDoorOffset";
        lblDoorOffset.TabIndex = 14;
        lblDoorOffset.Text = "Cách góc (mét)";
        // 
        // cboDoorSide
        // 
        cboDoorSide.Dock = System.Windows.Forms.DockStyle.Fill;
        cboDoorSide.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        cboDoorSide.Margin = new System.Windows.Forms.Padding(0, 0, 8, 12);
        cboDoorSide.Name = "cboDoorSide";
        cboDoorSide.TabIndex = 15;
        // 
        // numDoorOffset
        // 
        numDoorOffset.Dock = System.Windows.Forms.DockStyle.Fill;
        numDoorOffset.DecimalPlaces = 1;
        numDoorOffset.Enabled = false;
        numDoorOffset.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
        numDoorOffset.Margin = new System.Windows.Forms.Padding(8, 0, 0, 12);
        numDoorOffset.Name = "numDoorOffset";
        numDoorOffset.TabIndex = 16;
        // 
        // lblDoorNote
        // 
        lblDoorNote.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
        lblDoorNote.AutoSize = true;
        lblDoorNote.Font = new System.Drawing.Font("Segoe UI", 9F);
        lblDoorNote.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        lblDoorNote.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
        lblDoorNote.Name = "lblDoorNote";
        lblDoorNote.TabIndex = 17;
        lblDoorNote.Text = "Tường trên/dưới: đo từ góc TRÁI · Tường trái/phải: đo từ góc TRÊN. Hệ thống tự tính khoảng cách tới cửa của từng ô.";
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
        btnSave.Text = "💾 Lưu thay đổi";
        btnSave.UseVisualStyleBackColor = false;
        btnSave.Click += new System.EventHandler(this.BtnSave_Click);
        // 
        // EditWarehouseForm
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        BackColor = System.Drawing.Color.White;
        ClientSize = new System.Drawing.Size(500, 600);
        Controls.Add(root);
        MinimumSize = new System.Drawing.Size(440, 440);
        Name = "EditWarehouseForm";
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        Text = "Sửa thông tin kho";
        ((System.ComponentModel.ISupportInitialize)numCapacity).EndInit();
        ((System.ComponentModel.ISupportInitialize)numLength).EndInit();
        ((System.ComponentModel.ISupportInitialize)numWidth).EndInit();
        ((System.ComponentModel.ISupportInitialize)numDoorOffset).EndInit();
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
    private System.Windows.Forms.Label lblAddress;
    private System.Windows.Forms.TextBox txtSearch;
    private System.Windows.Forms.ListBox lstSuggestions;
    private System.Windows.Forms.Label lblSelectedAddress;
    private System.Windows.Forms.Label lblCapacity;
    private System.Windows.Forms.NumericUpDown numCapacity;
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
