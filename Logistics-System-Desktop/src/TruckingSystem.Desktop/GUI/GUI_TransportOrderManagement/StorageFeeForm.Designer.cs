namespace TruckingSystem.Desktop.GUI_TransportOrderManagement
{
    partial class StorageFeeForm : TruckingSystem.Desktop.Common.BaseForm
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>Bố cục: root (TableLayoutPanel 1 cột) = tiêu đề / hàng thẻ số liệu (KpiStrip — các thẻ
        /// thêm bằng code) / lưới từng lần đổi hạn giao (chiếm phần cao còn lại) / ghi chú cách tính /
        /// hàng nút (Sao chép diễn giải · Đóng). Không tọa độ cứng.</summary>
        private void InitializeComponent()
        {
            root = new System.Windows.Forms.TableLayoutPanel();
            lblTitle = new System.Windows.Forms.Label();
            kpi = new TruckingSystem.Desktop.Common.KpiStrip();
            grid = new System.Windows.Forms.DataGridView();
            txtDetail = new System.Windows.Forms.TextBox();
            lblNote = new System.Windows.Forms.Label();
            bottomBar = new System.Windows.Forms.TableLayoutPanel();
            btnCopy = new System.Windows.Forms.Button();
            btnClose = new System.Windows.Forms.Button();
            root.SuspendLayout();
            bottomBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            //
            // root
            //
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.Controls.Add(lblTitle, 0, 0);
            root.Controls.Add(kpi, 0, 1);
            root.Controls.Add(grid, 0, 2);
            root.Controls.Add(txtDetail, 0, 3);
            root.Controls.Add(lblNote, 0, 4);
            root.Controls.Add(bottomBar, 0, 5);
            root.Dock = System.Windows.Forms.DockStyle.Fill;
            root.Margin = new System.Windows.Forms.Padding(0);
            root.Name = "root";
            root.Padding = new System.Windows.Forms.Padding(16, 12, 16, 12);
            root.RowCount = 6;
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.Size = new System.Drawing.Size(1100, 760);
            root.TabIndex = 0;
            //
            // lblTitle
            //
            lblTitle.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            lblTitle.Name = "lblTitle";
            lblTitle.TabIndex = 0;
            lblTitle.Text = "💰 Phí lưu kho của đơn";
            //
            // kpi — hàng thẻ số liệu (thẻ thêm trong StorageFeeForm.cs)
            //
            kpi.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Top;
            kpi.Name = "kpi";
            kpi.TabIndex = 1;
            //
            // grid — từng lần đổi hạn giao
            //
            grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid.Dock = System.Windows.Forms.DockStyle.Fill;
            grid.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            grid.Name = "grid";
            grid.TabIndex = 2;
            //
            // txtDetail — CHI TIẾT TỪNG NGÀY của mọi lần: m³ còn trong kho × đơn giá = tiền, ngày nào không tính và vì sao
            //
            txtDetail.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            txtDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            txtDetail.Font = new System.Drawing.Font("Consolas", 9.5F);
            txtDetail.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            txtDetail.MinimumSize = new System.Drawing.Size(0, 120);
            txtDetail.Multiline = true;
            txtDetail.Name = "txtDetail";
            txtDetail.ReadOnly = true;
            txtDetail.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            txtDetail.TabIndex = 3;
            txtDetail.WordWrap = false;
            //
            // lblNote — ghi chú cách tính / đã chốt hay tạm tính
            //
            lblNote.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblNote.AutoSize = true;
            lblNote.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblNote.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblNote.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            lblNote.Name = "lblNote";
            lblNote.TabIndex = 4;
            lblNote.Text = "Đang tải...";
            //
            // bottomBar
            //
            bottomBar.AutoSize = true;
            bottomBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            bottomBar.ColumnCount = 3;
            bottomBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            bottomBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            bottomBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            bottomBar.Controls.Add(btnCopy, 0, 0);
            bottomBar.Controls.Add(btnClose, 2, 0);
            bottomBar.Dock = System.Windows.Forms.DockStyle.Fill;
            bottomBar.Margin = new System.Windows.Forms.Padding(0);
            bottomBar.Name = "bottomBar";
            bottomBar.RowCount = 1;
            bottomBar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            bottomBar.TabIndex = 5;
            //
            // btnCopy — sao chép đoạn diễn giải gửi khách / kế toán
            //
            btnCopy.Anchor = System.Windows.Forms.AnchorStyles.Left;
            btnCopy.AutoSize = true;
            btnCopy.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnCopy.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            btnCopy.Cursor = System.Windows.Forms.Cursors.Hand;
            btnCopy.FlatAppearance.BorderSize = 0;
            btnCopy.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            btnCopy.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnCopy.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnCopy.ForeColor = System.Drawing.Color.White;
            btnCopy.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            btnCopy.MinimumSize = new System.Drawing.Size(0, 38);
            btnCopy.Name = "btnCopy";
            btnCopy.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnCopy.TabIndex = 0;
            btnCopy.Text = "📋 Sao chép diễn giải";
            btnCopy.UseVisualStyleBackColor = false;
            btnCopy.Enabled = false;
            btnCopy.Click += new System.EventHandler(this.BtnCopy_Click);
            //
            // btnClose
            //
            btnClose.Anchor = System.Windows.Forms.AnchorStyles.Right;
            btnClose.AutoSize = true;
            btnClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnClose.BackColor = System.Drawing.Color.White;
            btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnClose.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnClose.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnClose.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            btnClose.Margin = new System.Windows.Forms.Padding(0);
            btnClose.MinimumSize = new System.Drawing.Size(0, 38);
            btnClose.Name = "btnClose";
            btnClose.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnClose.TabIndex = 2;
            btnClose.Text = "Đóng";
            btnClose.UseVisualStyleBackColor = false;
            //
            // StorageFeeForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.White;
            CancelButton = btnClose;
            ClientSize = new System.Drawing.Size(1100, 760);
            Controls.Add(root);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(760, 560);
            Name = "StorageFeeForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Phí lưu kho";
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            bottomBar.ResumeLayout(false);
            bottomBar.PerformLayout();
            root.ResumeLayout(false);
            root.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.Label lblTitle;
        private TruckingSystem.Desktop.Common.KpiStrip kpi;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.TextBox txtDetail;
        private System.Windows.Forms.Label lblNote;
        private System.Windows.Forms.TableLayoutPanel bottomBar;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.Button btnClose;
    }
}
