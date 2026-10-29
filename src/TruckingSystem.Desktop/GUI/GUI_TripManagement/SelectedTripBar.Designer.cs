namespace TruckingSystem.Desktop.GUI_TripManagement
{
    partial class SelectedTripBar
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>Bố cục: layout (TableLayoutPanel 3 cột) = vạch màu trạng thái | 3 dòng thông tin chuyến
        /// (tiêu đề đậm / xe-tài xế-tuyến / việc cần làm tiếp) | gợi ý thao tác chuột. Không tọa độ cứng.</summary>
        private void InitializeComponent()
        {
            layout = new System.Windows.Forms.TableLayoutPanel();
            accent = new System.Windows.Forms.Panel();
            lblHeadline = new System.Windows.Forms.Label();
            lblDetail = new System.Windows.Forms.Label();
            lblNext = new System.Windows.Forms.Label();
            lblHint = new System.Windows.Forms.Label();
            layout.SuspendLayout();
            SuspendLayout();
            //
            // layout
            //
            layout.AutoSize = true;
            layout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            layout.BackColor = System.Drawing.Color.White;
            layout.ColumnCount = 3;
            layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 6F));
            layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            layout.Controls.Add(accent, 0, 0);
            layout.Controls.Add(lblHeadline, 1, 0);
            layout.Controls.Add(lblDetail, 1, 1);
            layout.Controls.Add(lblNext, 1, 2);
            layout.Controls.Add(lblHint, 2, 0);
            layout.Dock = System.Windows.Forms.DockStyle.Top;
            layout.Margin = new System.Windows.Forms.Padding(0);
            layout.Name = "layout";
            layout.Padding = new System.Windows.Forms.Padding(0, 8, 12, 8);
            layout.RowCount = 3;
            layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            layout.SetRowSpan(accent, 3);
            layout.SetRowSpan(lblHint, 3);
            layout.TabIndex = 0;
            //
            // accent — vạch màu theo trạng thái chuyến
            //
            accent.BackColor = System.Drawing.Color.FromArgb(203, 213, 225);
            accent.Dock = System.Windows.Forms.DockStyle.Fill;
            accent.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            accent.Name = "accent";
            accent.TabIndex = 0;
            //
            // lblHeadline
            //
            lblHeadline.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblHeadline.AutoSize = true;
            lblHeadline.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            lblHeadline.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblHeadline.Margin = new System.Windows.Forms.Padding(12, 0, 12, 2);
            lblHeadline.Name = "lblHeadline";
            lblHeadline.TabIndex = 1;
            lblHeadline.Text = "Chưa chọn chuyến";
            //
            // lblDetail
            //
            lblDetail.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblDetail.AutoSize = true;
            lblDetail.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            lblDetail.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            lblDetail.Margin = new System.Windows.Forms.Padding(12, 0, 12, 2);
            lblDetail.Name = "lblDetail";
            lblDetail.TabIndex = 2;
            lblDetail.Text = "Bấm 1 dòng trong bảng bên dưới để xem thông tin và việc cần làm của chuyến.";
            //
            // lblNext — việc cần làm tiếp theo của chuyến đang chọn
            //
            lblNext.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblNext.AutoSize = true;
            lblNext.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            lblNext.ForeColor = System.Drawing.Color.FromArgb(37, 99, 235);
            lblNext.Margin = new System.Windows.Forms.Padding(12, 0, 12, 0);
            lblNext.Name = "lblNext";
            lblNext.TabIndex = 3;
            //
            // lblHint — cách dùng chuột
            //
            lblHint.Anchor = System.Windows.Forms.AnchorStyles.Right;
            lblHint.AutoSize = true;
            lblHint.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblHint.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblHint.Margin = new System.Windows.Forms.Padding(0);
            lblHint.Name = "lblHint";
            lblHint.TabIndex = 4;
            lblHint.Text = "🖱 Chuột phải vào dòng: mọi thao tác\n🖱 Nhấp đúp: xem lộ trình";
            lblHint.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // SelectedTripBar
            //
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            AutoSize = true;
            AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            BackColor = System.Drawing.Color.FromArgb(226, 232, 240);
            Controls.Add(layout);
            Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            Name = "SelectedTripBar";
            Padding = new System.Windows.Forms.Padding(1);
            Size = new System.Drawing.Size(900, 80);
            layout.ResumeLayout(false);
            layout.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel layout;
        private System.Windows.Forms.Panel accent;
        private System.Windows.Forms.Label lblHeadline;
        private System.Windows.Forms.Label lblDetail;
        private System.Windows.Forms.Label lblNext;
        private System.Windows.Forms.Label lblHint;
    }
}
