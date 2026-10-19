namespace TruckingSystem.Desktop.GUI_VehicleManagement
{
    partial class VehicleFuelLevelForm
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>Bố cục: TableLayoutPanel 2 cột — tiêu đề, mức hiện có + diễn giải, dung tích bình,
        /// ô "nhập mức đo thực tế" (+ nút Đầy bình), dòng báo, thanh nút Lưu / Đóng.</summary>
        private void InitializeComponent()
        {
            root = new System.Windows.Forms.TableLayoutPanel();
            lblTitle = new System.Windows.Forms.Label();
            lblCurrent = new System.Windows.Forms.Label();
            lblBreakdown = new System.Windows.Forms.Label();
            lblCapacity = new System.Windows.Forms.Label();
            numCapacity = new System.Windows.Forms.NumericUpDown();
            chkSetMeasured = new System.Windows.Forms.CheckBox();
            measuredBar = new System.Windows.Forms.FlowLayoutPanel();
            numMeasured = new System.Windows.Forms.NumericUpDown();
            btnFull = new System.Windows.Forms.Button();
            lblStatus = new System.Windows.Forms.Label();
            buttonBar = new System.Windows.Forms.FlowLayoutPanel();
            btnClose = new System.Windows.Forms.Button();
            btnSave = new System.Windows.Forms.Button();
            root.SuspendLayout();
            measuredBar.SuspendLayout();
            buttonBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCapacity).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numMeasured).BeginInit();
            SuspendLayout();
            //
            // root
            //
            root.ColumnCount = 2;
            root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.Controls.Add(lblTitle, 0, 0);
            root.Controls.Add(lblCurrent, 0, 1);
            root.Controls.Add(lblBreakdown, 0, 2);
            root.Controls.Add(lblCapacity, 0, 3);
            root.Controls.Add(numCapacity, 1, 3);
            root.Controls.Add(chkSetMeasured, 0, 4);
            root.Controls.Add(measuredBar, 1, 4);
            root.Controls.Add(lblStatus, 0, 5);
            root.Controls.Add(buttonBar, 0, 7);
            root.SetColumnSpan(lblTitle, 2);
            root.SetColumnSpan(lblCurrent, 2);
            root.SetColumnSpan(lblBreakdown, 2);
            root.SetColumnSpan(lblStatus, 2);
            root.SetColumnSpan(buttonBar, 2);
            root.Dock = System.Windows.Forms.DockStyle.Fill;
            root.Name = "root";
            root.Padding = new System.Windows.Forms.Padding(20, 16, 20, 16);
            root.RowCount = 8;
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.Size = new System.Drawing.Size(560, 420);
            root.TabIndex = 0;
            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.TabIndex = 0;
            lblTitle.Text = "⛽ Nhiên liệu trong bình";
            //
            // lblCurrent
            //
            lblCurrent.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblCurrent.AutoSize = true;
            lblCurrent.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            lblCurrent.ForeColor = System.Drawing.Color.FromArgb(22, 163, 74);
            lblCurrent.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            lblCurrent.Name = "lblCurrent";
            lblCurrent.TabIndex = 1;
            lblCurrent.Text = "Đang tải...";
            //
            // lblBreakdown
            //
            lblBreakdown.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblBreakdown.AutoSize = true;
            lblBreakdown.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblBreakdown.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblBreakdown.Margin = new System.Windows.Forms.Padding(0, 0, 0, 14);
            lblBreakdown.Name = "lblBreakdown";
            lblBreakdown.TabIndex = 2;
            //
            // lblCapacity
            //
            lblCapacity.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblCapacity.AutoSize = true;
            lblCapacity.Font = new System.Drawing.Font("Segoe UI", 10F);
            lblCapacity.Margin = new System.Windows.Forms.Padding(0, 4, 12, 4);
            lblCapacity.Name = "lblCapacity";
            lblCapacity.TabIndex = 3;
            lblCapacity.Text = "Dung tích bình (lít):";
            //
            // numCapacity
            //
            numCapacity.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numCapacity.DecimalPlaces = 0;
            numCapacity.Font = new System.Drawing.Font("Segoe UI", 10F);
            numCapacity.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            numCapacity.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
            numCapacity.Minimum = new decimal(new int[] { 20, 0, 0, 0 });
            numCapacity.MinimumSize = new System.Drawing.Size(120, 0);
            numCapacity.Name = "numCapacity";
            numCapacity.TabIndex = 4;
            numCapacity.ThousandsSeparator = true;
            numCapacity.Value = new decimal(new int[] { 200, 0, 0, 0 });
            //
            // chkSetMeasured
            //
            chkSetMeasured.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chkSetMeasured.AutoSize = true;
            chkSetMeasured.Font = new System.Drawing.Font("Segoe UI", 10F);
            chkSetMeasured.Margin = new System.Windows.Forms.Padding(0, 4, 12, 4);
            chkSetMeasured.Name = "chkSetMeasured";
            chkSetMeasured.TabIndex = 5;
            chkSetMeasured.Text = "Nhập mức đo thực tế (lít):";
            chkSetMeasured.UseVisualStyleBackColor = true;
            //
            // measuredBar
            //
            measuredBar.AutoSize = true;
            measuredBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            measuredBar.Controls.Add(numMeasured);
            measuredBar.Controls.Add(btnFull);
            measuredBar.Dock = System.Windows.Forms.DockStyle.Fill;
            measuredBar.Margin = new System.Windows.Forms.Padding(0);
            measuredBar.Name = "measuredBar";
            measuredBar.TabIndex = 6;
            measuredBar.WrapContents = false;
            //
            // numMeasured
            //
            numMeasured.DecimalPlaces = 1;
            numMeasured.Enabled = false;
            numMeasured.Font = new System.Drawing.Font("Segoe UI", 10F);
            numMeasured.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            numMeasured.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
            numMeasured.MinimumSize = new System.Drawing.Size(120, 0);
            numMeasured.Name = "numMeasured";
            numMeasured.TabIndex = 0;
            numMeasured.ThousandsSeparator = true;
            //
            // btnFull
            //
            btnFull.AutoSize = true;
            btnFull.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnFull.BackColor = System.Drawing.Color.White;
            btnFull.Cursor = System.Windows.Forms.Cursors.Hand;
            btnFull.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            btnFull.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnFull.Font = new System.Drawing.Font("Segoe UI", 9F);
            btnFull.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            btnFull.MinimumSize = new System.Drawing.Size(0, 30);
            btnFull.Name = "btnFull";
            btnFull.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
            btnFull.TabIndex = 1;
            btnFull.Text = "Đầy bình";
            btnFull.UseVisualStyleBackColor = false;
            //
            // lblStatus
            //
            lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblStatus.AutoSize = true;
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblStatus.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblStatus.Margin = new System.Windows.Forms.Padding(0, 10, 0, 4);
            lblStatus.Name = "lblStatus";
            lblStatus.TabIndex = 7;
            lblStatus.Text = "Nhập mức đo = đặt lại mốc tính (các lần đổ / tiêu hao trước mốc không còn tính).";
            //
            // buttonBar
            //
            buttonBar.AutoSize = true;
            buttonBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            buttonBar.Controls.Add(btnClose);
            buttonBar.Controls.Add(btnSave);
            buttonBar.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonBar.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            buttonBar.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            buttonBar.Name = "buttonBar";
            buttonBar.TabIndex = 8;
            buttonBar.WrapContents = false;
            //
            // btnClose
            //
            btnClose.AutoSize = true;
            btnClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnClose.BackColor = System.Drawing.Color.White;
            btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnClose.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnClose.Font = new System.Drawing.Font("Segoe UI", 10F);
            btnClose.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            btnClose.Margin = new System.Windows.Forms.Padding(0);
            btnClose.MinimumSize = new System.Drawing.Size(0, 36);
            btnClose.Name = "btnClose";
            btnClose.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            btnClose.TabIndex = 1;
            btnClose.Text = "Đóng";
            btnClose.UseVisualStyleBackColor = false;
            //
            // btnSave
            //
            btnSave.AutoSize = true;
            btnSave.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnSave.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnSave.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnSave.ForeColor = System.Drawing.Color.White;
            btnSave.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            btnSave.MinimumSize = new System.Drawing.Size(0, 36);
            btnSave.Name = "btnSave";
            btnSave.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            btnSave.TabIndex = 0;
            btnSave.Text = "💾 Lưu";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            //
            // VehicleFuelLevelForm
            //
            AcceptButton = btnSave;
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.White;
            CancelButton = btnClose;
            ClientSize = new System.Drawing.Size(560, 420);
            Controls.Add(root);
            MinimumSize = new System.Drawing.Size(500, 380);
            Name = "VehicleFuelLevelForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Nhiên liệu";
            ((System.ComponentModel.ISupportInitialize)numCapacity).EndInit();
            ((System.ComponentModel.ISupportInitialize)numMeasured).EndInit();
            root.ResumeLayout(false);
            root.PerformLayout();
            measuredBar.ResumeLayout(false);
            measuredBar.PerformLayout();
            buttonBar.ResumeLayout(false);
            buttonBar.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblCurrent;
        private System.Windows.Forms.Label lblBreakdown;
        private System.Windows.Forms.Label lblCapacity;
        private System.Windows.Forms.NumericUpDown numCapacity;
        private System.Windows.Forms.CheckBox chkSetMeasured;
        private System.Windows.Forms.FlowLayoutPanel measuredBar;
        private System.Windows.Forms.NumericUpDown numMeasured;
        private System.Windows.Forms.Button btnFull;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.FlowLayoutPanel buttonBar;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnSave;
    }
}
