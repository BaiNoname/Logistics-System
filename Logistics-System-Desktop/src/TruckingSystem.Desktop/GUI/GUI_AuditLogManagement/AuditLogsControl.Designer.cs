namespace TruckingSystem.Desktop.GUI_AuditLogManagement
{
    partial class AuditLogsControl
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>Bố cục: root (TableLayoutPanel 1 cột) = tiêu đề / thanh khoảng ngày + Tải + Tải thêm / bộ lọc
        /// (hành động, người, đối tượng, ô tìm) / dòng trạng thái / bảng nhật ký (60%) / tiêu đề chi tiết / ô chi tiết
        /// thay đổi của dòng đang chọn (40%). Không tọa độ cứng — co giãn theo cửa sổ.</summary>
        private void InitializeComponent()
        {
            root = new System.Windows.Forms.TableLayoutPanel();
            lblTitle = new System.Windows.Forms.Label();
            dateBar = new System.Windows.Forms.FlowLayoutPanel();
            lblFrom = new System.Windows.Forms.Label();
            dtFrom = new System.Windows.Forms.DateTimePicker();
            lblTo = new System.Windows.Forms.Label();
            dtTo = new System.Windows.Forms.DateTimePicker();
            btnLoad = new System.Windows.Forms.Button();
            btnMore = new System.Windows.Forms.Button();
            filterBar = new TruckingSystem.Desktop.Common.FilterBar();
            lblStatus = new System.Windows.Forms.Label();
            grid = new System.Windows.Forms.DataGridView();
            lblDetailTitle = new System.Windows.Forms.Label();
            txtDetail = new System.Windows.Forms.TextBox();
            root.SuspendLayout();
            dateBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            //
            // root
            //
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.Controls.Add(lblTitle, 0, 0);
            root.Controls.Add(dateBar, 0, 1);
            root.Controls.Add(filterBar, 0, 2);
            root.Controls.Add(lblStatus, 0, 3);
            root.Controls.Add(grid, 0, 4);
            root.Controls.Add(lblDetailTitle, 0, 5);
            root.Controls.Add(txtDetail, 0, 6);
            root.Dock = System.Windows.Forms.DockStyle.Fill;
            root.Margin = new System.Windows.Forms.Padding(0);
            root.Name = "root";
            root.RowCount = 7;
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 62F));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 38F));
            root.Size = new System.Drawing.Size(1200, 700);
            root.TabIndex = 0;
            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
            lblTitle.Name = "lblTitle";
            lblTitle.TabIndex = 0;
            lblTitle.Text = "📜 Nhật ký hệ thống";
            //
            // dateBar — khoảng ngày cần xem + Tải / Tải thêm
            //
            dateBar.AutoSize = true;
            dateBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            dateBar.Controls.Add(lblFrom);
            dateBar.Controls.Add(dtFrom);
            dateBar.Controls.Add(lblTo);
            dateBar.Controls.Add(dtTo);
            dateBar.Controls.Add(btnLoad);
            dateBar.Controls.Add(btnMore);
            dateBar.Dock = System.Windows.Forms.DockStyle.Fill;
            dateBar.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            dateBar.Name = "dateBar";
            dateBar.TabIndex = 1;
            dateBar.WrapContents = true;
            //
            // lblFrom
            //
            lblFrom.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblFrom.AutoSize = true;
            lblFrom.Font = new System.Drawing.Font("Segoe UI", 10F);
            lblFrom.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            lblFrom.Name = "lblFrom";
            lblFrom.TabIndex = 0;
            lblFrom.Text = "Từ ngày:";
            //
            // dtFrom
            //
            dtFrom.Anchor = System.Windows.Forms.AnchorStyles.Left;
            dtFrom.CustomFormat = "dd/MM/yyyy";
            dtFrom.Font = new System.Drawing.Font("Segoe UI", 10F);
            dtFrom.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            dtFrom.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            dtFrom.MinimumSize = new System.Drawing.Size(130, 0);
            dtFrom.Name = "dtFrom";
            dtFrom.TabIndex = 1;
            //
            // lblTo
            //
            lblTo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblTo.AutoSize = true;
            lblTo.Font = new System.Drawing.Font("Segoe UI", 10F);
            lblTo.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            lblTo.Name = "lblTo";
            lblTo.TabIndex = 2;
            lblTo.Text = "đến:";
            //
            // dtTo
            //
            dtTo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            dtTo.CustomFormat = "dd/MM/yyyy";
            dtTo.Font = new System.Drawing.Font("Segoe UI", 10F);
            dtTo.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            dtTo.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            dtTo.MinimumSize = new System.Drawing.Size(130, 0);
            dtTo.Name = "dtTo";
            dtTo.TabIndex = 3;
            //
            // btnLoad
            //
            btnLoad.AutoSize = true;
            btnLoad.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnLoad.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            btnLoad.Cursor = System.Windows.Forms.Cursors.Hand;
            btnLoad.FlatAppearance.BorderSize = 0;
            btnLoad.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            btnLoad.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnLoad.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnLoad.ForeColor = System.Drawing.Color.White;
            btnLoad.Margin = new System.Windows.Forms.Padding(0, 0, 8, 6);
            btnLoad.MinimumSize = new System.Drawing.Size(0, 36);
            btnLoad.Name = "btnLoad";
            btnLoad.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            btnLoad.TabIndex = 4;
            btnLoad.Text = "🔄 Tải nhật ký";
            btnLoad.UseVisualStyleBackColor = false;
            btnLoad.Click += new System.EventHandler(this.BtnLoad_Click);
            //
            // btnMore
            //
            btnMore.AutoSize = true;
            btnMore.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnMore.BackColor = System.Drawing.Color.White;
            btnMore.Cursor = System.Windows.Forms.Cursors.Hand;
            btnMore.Enabled = false;
            btnMore.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
            btnMore.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnMore.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnMore.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            btnMore.Margin = new System.Windows.Forms.Padding(0, 0, 8, 6);
            btnMore.MinimumSize = new System.Drawing.Size(0, 36);
            btnMore.Name = "btnMore";
            btnMore.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            btnMore.TabIndex = 5;
            btnMore.Text = "⬇️ Tải thêm dòng cũ hơn";
            btnMore.UseVisualStyleBackColor = false;
            btnMore.Click += new System.EventHandler(this.BtnMore_Click);
            //
            // filterBar — lọc hành động / người / đối tượng + ô tìm (lọc ngay trên máy)
            //
            filterBar.Dock = System.Windows.Forms.DockStyle.Fill;
            filterBar.Name = "filterBar";
            filterBar.SearchPlaceholder = "Tìm mã đơn, biển số, tên, nội dung thay đổi...";
            filterBar.TabIndex = 2;
            //
            // lblStatus
            //
            lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblStatus.AutoSize = true;
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblStatus.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblStatus.Margin = new System.Windows.Forms.Padding(0, 2, 0, 6);
            lblStatus.Name = "lblStatus";
            lblStatus.TabIndex = 3;
            //
            // grid — danh sách nhật ký (mới nhất trên cùng)
            //
            grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid.Dock = System.Windows.Forms.DockStyle.Fill;
            grid.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            grid.Name = "grid";
            grid.TabIndex = 4;
            //
            // lblDetailTitle
            //
            lblDetailTitle.AutoSize = true;
            lblDetailTitle.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            lblDetailTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblDetailTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            lblDetailTitle.Name = "lblDetailTitle";
            lblDetailTitle.TabIndex = 5;
            lblDetailTitle.Text = "🔍 Chi tiết dòng đang chọn";
            //
            // txtDetail — đổi những gì (field: cũ → mới), mỗi thay đổi 1 dòng
            //
            txtDetail.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            txtDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            txtDetail.Font = new System.Drawing.Font("Segoe UI", 10F);
            txtDetail.Margin = new System.Windows.Forms.Padding(0);
            txtDetail.MinimumSize = new System.Drawing.Size(0, 100);
            txtDetail.Multiline = true;
            txtDetail.Name = "txtDetail";
            txtDetail.ReadOnly = true;
            txtDetail.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtDetail.TabIndex = 6;
            //
            // AuditLogsControl
            //
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            Controls.Add(root);
            Name = "AuditLogsControl";
            Size = new System.Drawing.Size(1200, 700);
            root.ResumeLayout(false);
            root.PerformLayout();
            dateBar.ResumeLayout(false);
            dateBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.FlowLayoutPanel dateBar;
        private System.Windows.Forms.Label lblFrom;
        private System.Windows.Forms.DateTimePicker dtFrom;
        private System.Windows.Forms.Label lblTo;
        private System.Windows.Forms.DateTimePicker dtTo;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnMore;
        private TruckingSystem.Desktop.Common.FilterBar filterBar;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.Label lblDetailTitle;
        private System.Windows.Forms.TextBox txtDetail;
    }
}
