namespace TruckingSystem.Desktop.GUI_TripManagement
{
    partial class CreateIncidentForm
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>Bố cục: root (TableLayoutPanel 1 cột) = bảng nhập liệu (nhãn | ô nhập, ô mô tả
        /// nhiều dòng giãn theo chiều cao) / dòng trạng thái / thanh nút (phải -> trái).
        /// Không có tọa độ cứng.</summary>
        private void InitializeComponent()
        {
            root = new System.Windows.Forms.TableLayoutPanel();
            fields = new System.Windows.Forms.TableLayoutPanel();
            lblType = new System.Windows.Forms.Label();
            cboType = new System.Windows.Forms.ComboBox();
            lblDescription = new System.Windows.Forms.Label();
            txtDescription = new System.Windows.Forms.TextBox();
            lblCost = new System.Windows.Forms.Label();
            numCost = new TruckingSystem.Desktop.Common.NumberBox();
            lblStatus = new System.Windows.Forms.Label();
            buttonBar = new System.Windows.Forms.FlowLayoutPanel();
            btnSave = new System.Windows.Forms.Button();
            root.SuspendLayout();
            fields.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCost).BeginInit();
            buttonBar.SuspendLayout();
            SuspendLayout();
            //
            // root
            //
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.Controls.Add(fields, 0, 0);
            root.Controls.Add(lblStatus, 0, 1);
            root.Controls.Add(buttonBar, 0, 2);
            root.Dock = System.Windows.Forms.DockStyle.Fill;
            root.Name = "root";
            root.Padding = new System.Windows.Forms.Padding(20, 16, 20, 16);
            root.RowCount = 3;
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.Size = new System.Drawing.Size(460, 340);
            root.TabIndex = 0;
            //
            // fields
            //
            fields.ColumnCount = 2;
            fields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            fields.Controls.Add(lblType, 0, 0);
            fields.Controls.Add(cboType, 1, 0);
            fields.Controls.Add(lblDescription, 0, 1);
            fields.Controls.Add(txtDescription, 1, 1);
            fields.Controls.Add(lblCost, 0, 2);
            fields.Controls.Add(numCost, 1, 2);
            fields.Dock = System.Windows.Forms.DockStyle.Fill;
            fields.Margin = new System.Windows.Forms.Padding(0);
            fields.Name = "fields";
            fields.RowCount = 3;
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            fields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            fields.TabIndex = 0;
            //
            // lblType
            //
            lblType.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblType.AutoSize = true;
            lblType.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblType.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblType.Margin = new System.Windows.Forms.Padding(0, 0, 12, 8);
            lblType.Name = "lblType";
            lblType.TabIndex = 0;
            lblType.Text = "Loại sự cố *";
            //
            // cboType
            //
            cboType.Dock = System.Windows.Forms.DockStyle.Fill;
            cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboType.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            cboType.Name = "cboType";
            cboType.TabIndex = 1;
            //
            // lblDescription
            //
            lblDescription.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            lblDescription.AutoSize = true;
            lblDescription.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblDescription.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblDescription.Margin = new System.Windows.Forms.Padding(0, 4, 12, 8);
            lblDescription.Name = "lblDescription";
            lblDescription.TabIndex = 2;
            lblDescription.Text = "Mô tả chi tiết *";
            //
            // txtDescription
            //
            txtDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            txtDescription.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            txtDescription.MinimumSize = new System.Drawing.Size(0, 60);
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtDescription.TabIndex = 3;
            //
            // lblCost
            //
            lblCost.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblCost.AutoSize = true;
            lblCost.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblCost.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblCost.Margin = new System.Windows.Forms.Padding(0, 0, 12, 8);
            lblCost.Name = "lblCost";
            lblCost.TabIndex = 4;
            lblCost.Text = "Chi phí phát sinh (đ, tuỳ chọn)";
            //
            // numCost
            //
            numCost.Dock = System.Windows.Forms.DockStyle.Fill;
            numCost.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            numCost.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            numCost.Name = "numCost";
            numCost.TabIndex = 5;
            numCost.IsMoney = true;
            //
            // lblStatus
            //
            lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblStatus.AutoSize = true;
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblStatus.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
            lblStatus.Margin = new System.Windows.Forms.Padding(0, 4, 0, 8);
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
            btnSave.Text = "Lưu sự cố";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            //
            // CreateIncidentForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(460, 340);
            Controls.Add(root);
            MaximizeBox = false;
            MinimumSize = new System.Drawing.Size(420, 320);
            Name = "CreateIncidentForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Ghi nhận sự cố";
            root.ResumeLayout(false);
            root.PerformLayout();
            fields.ResumeLayout(false);
            fields.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCost).EndInit();
            buttonBar.ResumeLayout(false);
            buttonBar.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.ComboBox cboType;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Label lblCost;
        private TruckingSystem.Desktop.Common.NumberBox numCost;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.TableLayoutPanel fields;
        private System.Windows.Forms.FlowLayoutPanel buttonBar;
    }
}
