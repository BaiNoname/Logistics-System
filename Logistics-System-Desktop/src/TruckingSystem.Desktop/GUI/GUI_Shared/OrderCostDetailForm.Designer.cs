namespace TruckingSystem.Desktop.GUI_Shared
{
    partial class OrderCostDetailForm
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>Bố cục: root (TableLayoutPanel 1 cột) = tiêu đề / dòng phụ / vùng thẻ tổng hợp
        /// (FlowLayoutPanel, thẻ được thêm bằng code theo dữ liệu đơn) / tổng hợp theo loại /
        /// bảng khoản chi (chiếm phần còn lại) / thanh nút Đóng. Không có tọa độ cứng.</summary>
        private void InitializeComponent()
        {
            root = new System.Windows.Forms.TableLayoutPanel();
            lblTitle = new System.Windows.Forms.Label();
            lblSub = new System.Windows.Forms.Label();
            summary = new System.Windows.Forms.FlowLayoutPanel();
            lblByCategory = new System.Windows.Forms.Label();
            grid = new System.Windows.Forms.DataGridView();
            buttonBar = new System.Windows.Forms.FlowLayoutPanel();
            btnClose = new System.Windows.Forms.Button();
            root.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            buttonBar.SuspendLayout();
            SuspendLayout();
            //
            // root
            //
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.Controls.Add(lblTitle, 0, 0);
            root.Controls.Add(lblSub, 0, 1);
            root.Controls.Add(summary, 0, 2);
            root.Controls.Add(lblByCategory, 0, 3);
            root.Controls.Add(grid, 0, 4);
            root.Controls.Add(buttonBar, 0, 5);
            root.Dock = System.Windows.Forms.DockStyle.Fill;
            root.Name = "root";
            root.Padding = new System.Windows.Forms.Padding(20, 16, 20, 16);
            root.RowCount = 6;
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.Size = new System.Drawing.Size(1000, 620);
            root.TabIndex = 0;
            //
            // lblTitle
            //
            lblTitle.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            lblTitle.Name = "lblTitle";
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Đang tải...";
            //
            // lblSub
            //
            lblSub.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblSub.AutoSize = true;
            lblSub.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblSub.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblSub.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            lblSub.Name = "lblSub";
            lblSub.TabIndex = 1;
            //
            // summary
            //
            summary.AutoSize = true;
            summary.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            summary.Dock = System.Windows.Forms.DockStyle.Fill;
            summary.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            summary.Name = "summary";
            summary.TabIndex = 2;
            summary.WrapContents = true;
            //
            // lblByCategory
            //
            lblByCategory.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblByCategory.AutoSize = true;
            lblByCategory.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblByCategory.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblByCategory.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            lblByCategory.Name = "lblByCategory";
            lblByCategory.TabIndex = 3;
            //
            // grid
            //
            grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid.Dock = System.Windows.Forms.DockStyle.Fill;
            grid.Margin = new System.Windows.Forms.Padding(0);
            grid.Name = "grid";
            grid.TabIndex = 4;
            grid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.Grid_CellFormatting);
            //
            // buttonBar
            //
            buttonBar.AutoSize = true;
            buttonBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            buttonBar.Controls.Add(btnClose);
            buttonBar.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonBar.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            buttonBar.Margin = new System.Windows.Forms.Padding(0, 12, 0, 0);
            buttonBar.Name = "buttonBar";
            buttonBar.TabIndex = 5;
            buttonBar.WrapContents = true;
            //
            // btnClose
            //
            btnClose.AutoSize = true;
            btnClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnClose.BackColor = System.Drawing.Color.White;
            btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            btnClose.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(226, 232, 240);
            btnClose.FlatAppearance.BorderSize = 1;
            btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnClose.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnClose.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            btnClose.Margin = new System.Windows.Forms.Padding(0);
            btnClose.MinimumSize = new System.Drawing.Size(110, 34);
            btnClose.Name = "btnClose";
            btnClose.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            btnClose.TabIndex = 0;
            btnClose.Text = "Đóng";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            //
            // OrderCostDetailForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            CancelButton = btnClose;
            ClientSize = new System.Drawing.Size(1000, 620);
            Controls.Add(root);
            MinimumSize = new System.Drawing.Size(820, 480);
            Name = "OrderCostDetailForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Chi phí đơn hàng";
            root.ResumeLayout(false);
            root.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            buttonBar.ResumeLayout(false);
            buttonBar.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSub;
        private System.Windows.Forms.FlowLayoutPanel summary;
        private System.Windows.Forms.Label lblByCategory;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.FlowLayoutPanel buttonBar;
        private System.Windows.Forms.Button btnClose;
    }
}
