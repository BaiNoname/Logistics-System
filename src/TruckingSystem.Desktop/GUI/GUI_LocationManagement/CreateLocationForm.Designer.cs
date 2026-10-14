namespace TruckingSystem.Desktop.GUI_LocationManagement
{
    partial class CreateLocationForm
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>Bố cục: root (TableLayoutPanel 1 cột) = vùng nhập liệu (Panel tự cuộn khi màn hình thấp) / trạng thái / thanh nút (FlowLayoutPanel phải-sang-trái). Không có tọa độ cứng — chữ to/nhỏ theo DPI vẫn không đè nhau.</summary>
        private void InitializeComponent()
        {
            root = new System.Windows.Forms.TableLayoutPanel();
            scrollFields = new System.Windows.Forms.Panel();
            fields = new System.Windows.Forms.TableLayoutPanel();
            lblName = new System.Windows.Forms.Label();
            txtName = new System.Windows.Forms.TextBox();
            lblSearch = new System.Windows.Forms.Label();
            txtSearch = new System.Windows.Forms.TextBox();
            lstSuggestions = new System.Windows.Forms.ListBox();
            lblSelectedAddress = new System.Windows.Forms.Label();
            lblStatus = new System.Windows.Forms.Label();
            buttonBar = new System.Windows.Forms.FlowLayoutPanel();
            btnSave = new System.Windows.Forms.Button();
            root.SuspendLayout();
            scrollFields.SuspendLayout();
            fields.SuspendLayout();
            buttonBar.SuspendLayout();
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
            root.Size = new System.Drawing.Size(480, 360);
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
            fields.ColumnCount = 1;
            fields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            fields.Controls.Add(lblName, 0, 0);
            fields.Controls.Add(txtName, 0, 1);
            fields.Controls.Add(lblSearch, 0, 2);
            fields.Controls.Add(txtSearch, 0, 3);
            fields.Controls.Add(lstSuggestions, 0, 4);
            fields.Controls.Add(lblSelectedAddress, 0, 5);
            fields.AutoSize = true;
            fields.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            fields.Dock = System.Windows.Forms.DockStyle.Top;
            fields.Margin = new System.Windows.Forms.Padding(0);
            fields.Name = "fields";
            fields.TabIndex = 0;
            fields.RowCount = 6;
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblName.Margin = new System.Windows.Forms.Padding(0, 8, 0, 2);
            lblName.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblName.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblName.Text = "Tên địa điểm *";
            lblName.Name = "lblName";
            lblName.TabIndex = 0;
            // 
            // txtName
            // 
            txtName.Dock = System.Windows.Forms.DockStyle.Fill;
            txtName.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            txtName.Name = "txtName";
            txtName.TabIndex = 1;
            // 
            // lblSearch
            // 
            lblSearch.AutoSize = true;
            lblSearch.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblSearch.Margin = new System.Windows.Forms.Padding(0, 8, 0, 2);
            lblSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblSearch.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblSearch.Text = "🔍 Tìm địa chỉ * (gõ từ khóa, VD: \"193 Trần Phú\")";
            lblSearch.Name = "lblSearch";
            lblSearch.TabIndex = 2;
            // 
            // txtSearch
            // 
            txtSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            txtSearch.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            txtSearch.Name = "txtSearch";
            txtSearch.TabIndex = 3;
            // 
            // lstSuggestions
            // 
            lstSuggestions.Dock = System.Windows.Forms.DockStyle.Fill;
            lstSuggestions.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            lstSuggestions.Font = new System.Drawing.Font("Segoe UI", 9F);
            lstSuggestions.IntegralHeight = false;
            lstSuggestions.MinimumSize = new System.Drawing.Size(0, 110);
            lstSuggestions.Visible = false;
            lstSuggestions.Name = "lstSuggestions";
            lstSuggestions.TabIndex = 4;
            // 
            // lblSelectedAddress
            // 
            lblSelectedAddress.AutoSize = true;
            lblSelectedAddress.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblSelectedAddress.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblSelectedAddress.ForeColor = System.Drawing.Color.FromArgb(22, 163, 74);
            lblSelectedAddress.Margin = new System.Windows.Forms.Padding(0, 6, 0, 2);
            lblSelectedAddress.Name = "lblSelectedAddress";
            lblSelectedAddress.TabIndex = 5;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblStatus.Margin = new System.Windows.Forms.Padding(0, 8, 0, 4);
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
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
            btnSave.Text = "Lưu địa điểm";
            btnSave.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            btnSave.Name = "btnSave";
            btnSave.TabIndex = 0;
            btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // CreateLocationForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(480, 360);
            MaximizeBox = false;
            MinimumSize = new System.Drawing.Size(420, 340);
            Name = "CreateLocationForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Thêm địa điểm mới";
            Controls.Add(root);
            root.ResumeLayout(false);
            root.PerformLayout();
            scrollFields.ResumeLayout(false);
            scrollFields.PerformLayout();
            fields.ResumeLayout(false);
            fields.PerformLayout();
            buttonBar.ResumeLayout(false);
            buttonBar.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.ListBox lstSuggestions;
        private System.Windows.Forms.Label lblSelectedAddress;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.Panel scrollFields;
        private System.Windows.Forms.TableLayoutPanel fields;
        private System.Windows.Forms.FlowLayoutPanel buttonBar;
    }
}
