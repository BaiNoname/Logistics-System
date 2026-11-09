namespace TruckingSystem.Desktop.GUI_TripManagement
{
    partial class TripCargoReviewForm : TruckingSystem.Desktop.Common.BaseForm
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>Bố cục: root (TableLayoutPanel 1 cột) = tiêu đề / thông tin / nội dung (chiếm
        /// phần còn lại: lưới hàng bên trái 44% | ảnh + thanh điều hướng ảnh bên phải 56%) / thanh
        /// nút Duyệt-Từ chối-Đóng (phải -> trái). Không có tọa độ cứng.</summary>
        private void InitializeComponent()
        {
            root = new System.Windows.Forms.TableLayoutPanel();
            lblTitle = new System.Windows.Forms.Label();
            lblInfo = new System.Windows.Forms.Label();
            content = new System.Windows.Forms.TableLayoutPanel();
            gridLines = new System.Windows.Forms.DataGridView();
            colType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colQty = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colPkg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colKg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            photoPanel = new System.Windows.Forms.TableLayoutPanel();
            picPhoto = new System.Windows.Forms.PictureBox();
            photoNav = new System.Windows.Forms.TableLayoutPanel();
            btnPrev = new System.Windows.Forms.Button();
            btnNext = new System.Windows.Forms.Button();
            lblPhoto = new System.Windows.Forms.Label();
            btnOpen = new System.Windows.Forms.Button();
            buttonBar = new System.Windows.Forms.FlowLayoutPanel();
            btnClose = new System.Windows.Forms.Button();
            btnReject = new System.Windows.Forms.Button();
            btnApprove = new System.Windows.Forms.Button();
            root.SuspendLayout();
            content.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridLines).BeginInit();
            photoPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picPhoto).BeginInit();
            photoNav.SuspendLayout();
            buttonBar.SuspendLayout();
            SuspendLayout();
            //
            // root
            //
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.Controls.Add(lblTitle, 0, 0);
            root.Controls.Add(lblInfo, 0, 1);
            root.Controls.Add(content, 0, 2);
            root.Controls.Add(buttonBar, 0, 3);
            root.Dock = System.Windows.Forms.DockStyle.Fill;
            root.Name = "root";
            root.Padding = new System.Windows.Forms.Padding(20, 16, 20, 16);
            root.RowCount = 4;
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.Size = new System.Drawing.Size(1000, 640);
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
            lblTitle.Text = "🔎 Kiểm tra hàng";
            //
            // lblInfo
            //
            lblInfo.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblInfo.AutoSize = true;
            lblInfo.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblInfo.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblInfo.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            lblInfo.Name = "lblInfo";
            lblInfo.TabIndex = 1;
            //
            // content
            //
            content.ColumnCount = 2;
            content.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 44F));
            content.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 56F));
            content.Controls.Add(gridLines, 0, 0);
            content.Controls.Add(photoPanel, 1, 0);
            content.Dock = System.Windows.Forms.DockStyle.Fill;
            content.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            content.Name = "content";
            content.RowCount = 1;
            content.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            content.TabIndex = 2;
            //
            // gridLines
            //
            gridLines.AllowUserToAddRows = false;
            gridLines.AutoGenerateColumns = false;
            gridLines.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridLines.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colType, colQty, colPkg, colKg });
            gridLines.Dock = System.Windows.Forms.DockStyle.Fill;
            gridLines.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            gridLines.Name = "gridLines";
            gridLines.ReadOnly = true;
            gridLines.TabIndex = 0;
            //
            // colType
            //
            colType.FillWeight = 46F;
            colType.HeaderText = "Hàng hóa";
            colType.Name = "colType";
            //
            // colQty
            //
            colQty.FillWeight = 18F;
            colQty.HeaderText = "Số lượng";
            colQty.Name = "colQty";
            //
            // colPkg
            //
            colPkg.FillWeight = 16F;
            colPkg.HeaderText = "Số kiện";
            colPkg.Name = "colPkg";
            //
            // colKg
            //
            colKg.FillWeight = 20F;
            colKg.HeaderText = "Kg";
            colKg.Name = "colKg";
            //
            // photoPanel
            //
            photoPanel.ColumnCount = 1;
            photoPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            photoPanel.Controls.Add(picPhoto, 0, 0);
            photoPanel.Controls.Add(photoNav, 0, 1);
            photoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            photoPanel.Margin = new System.Windows.Forms.Padding(0);
            photoPanel.Name = "photoPanel";
            photoPanel.RowCount = 2;
            photoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            photoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            photoPanel.TabIndex = 1;
            //
            // picPhoto
            //
            picPhoto.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            picPhoto.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            picPhoto.Cursor = System.Windows.Forms.Cursors.Hand;
            picPhoto.Dock = System.Windows.Forms.DockStyle.Fill;
            picPhoto.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            picPhoto.Name = "picPhoto";
            picPhoto.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picPhoto.TabIndex = 0;
            picPhoto.TabStop = false;
            picPhoto.DoubleClick += new System.EventHandler(this.PicPhoto_DoubleClick);
            //
            // photoNav
            //
            photoNav.AutoSize = true;
            photoNav.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            photoNav.ColumnCount = 4;
            photoNav.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            photoNav.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            photoNav.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            photoNav.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            photoNav.Controls.Add(btnPrev, 0, 0);
            photoNav.Controls.Add(btnNext, 1, 0);
            photoNav.Controls.Add(lblPhoto, 2, 0);
            photoNav.Controls.Add(btnOpen, 3, 0);
            photoNav.Dock = System.Windows.Forms.DockStyle.Fill;
            photoNav.Margin = new System.Windows.Forms.Padding(0);
            photoNav.Name = "photoNav";
            photoNav.RowCount = 1;
            photoNav.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            photoNav.TabIndex = 1;
            //
            // btnPrev
            //
            btnPrev.AutoSize = true;
            btnPrev.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnPrev.BackColor = System.Drawing.Color.White;
            btnPrev.Cursor = System.Windows.Forms.Cursors.Hand;
            btnPrev.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnPrev.Font = new System.Drawing.Font("Segoe UI", 9F);
            btnPrev.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            btnPrev.MinimumSize = new System.Drawing.Size(0, 34);
            btnPrev.Name = "btnPrev";
            btnPrev.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            btnPrev.TabIndex = 0;
            btnPrev.Text = "◀ Trước";
            btnPrev.UseVisualStyleBackColor = false;
            btnPrev.Click += new System.EventHandler(this.BtnPrev_Click);
            //
            // btnNext
            //
            btnNext.AutoSize = true;
            btnNext.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnNext.BackColor = System.Drawing.Color.White;
            btnNext.Cursor = System.Windows.Forms.Cursors.Hand;
            btnNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnNext.Font = new System.Drawing.Font("Segoe UI", 9F);
            btnNext.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            btnNext.MinimumSize = new System.Drawing.Size(0, 34);
            btnNext.Name = "btnNext";
            btnNext.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            btnNext.TabIndex = 1;
            btnNext.Text = "Sau ▶";
            btnNext.UseVisualStyleBackColor = false;
            btnNext.Click += new System.EventHandler(this.BtnNext_Click);
            //
            // lblPhoto
            //
            lblPhoto.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblPhoto.AutoSize = true;
            lblPhoto.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblPhoto.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblPhoto.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            lblPhoto.Name = "lblPhoto";
            lblPhoto.TabIndex = 2;
            //
            // btnOpen
            //
            btnOpen.Anchor = System.Windows.Forms.AnchorStyles.Right;
            btnOpen.AutoSize = true;
            btnOpen.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnOpen.BackColor = System.Drawing.Color.White;
            btnOpen.Cursor = System.Windows.Forms.Cursors.Hand;
            btnOpen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnOpen.Font = new System.Drawing.Font("Segoe UI", 9F);
            btnOpen.Margin = new System.Windows.Forms.Padding(0);
            btnOpen.MinimumSize = new System.Drawing.Size(0, 34);
            btnOpen.Name = "btnOpen";
            btnOpen.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            btnOpen.TabIndex = 3;
            btnOpen.Text = "🔍 Mở ảnh lớn";
            btnOpen.UseVisualStyleBackColor = false;
            btnOpen.Click += new System.EventHandler(this.BtnOpen_Click);
            //
            // buttonBar
            //
            buttonBar.AutoSize = true;
            buttonBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            buttonBar.Controls.Add(btnClose);
            buttonBar.Controls.Add(btnReject);
            buttonBar.Controls.Add(btnApprove);
            buttonBar.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonBar.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            buttonBar.Margin = new System.Windows.Forms.Padding(0);
            buttonBar.Name = "buttonBar";
            buttonBar.TabIndex = 3;
            buttonBar.WrapContents = true;
            //
            // btnClose
            //
            btnClose.AutoSize = true;
            btnClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnClose.BackColor = System.Drawing.Color.White;
            btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnClose.Font = new System.Drawing.Font("Segoe UI", 9F);
            btnClose.Margin = new System.Windows.Forms.Padding(0);
            btnClose.MinimumSize = new System.Drawing.Size(0, 38);
            btnClose.Name = "btnClose";
            btnClose.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnClose.TabIndex = 2;
            btnClose.Text = "Đóng";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            //
            // btnReject
            //
            btnReject.AutoSize = true;
            btnReject.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnReject.BackColor = System.Drawing.Color.White;
            btnReject.Cursor = System.Windows.Forms.Cursors.Hand;
            btnReject.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnReject.Font = new System.Drawing.Font("Segoe UI", 9F);
            btnReject.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
            btnReject.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            btnReject.MinimumSize = new System.Drawing.Size(0, 38);
            btnReject.Name = "btnReject";
            btnReject.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnReject.TabIndex = 1;
            btnReject.Text = "❌ Từ chối";
            btnReject.UseVisualStyleBackColor = false;
            btnReject.Click += new System.EventHandler(this.BtnReject_Click);
            //
            // btnApprove
            //
            btnApprove.AutoSize = true;
            btnApprove.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnApprove.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            btnApprove.Cursor = System.Windows.Forms.Cursors.Hand;
            btnApprove.FlatAppearance.BorderSize = 0;
            btnApprove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnApprove.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnApprove.ForeColor = System.Drawing.Color.White;
            btnApprove.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            btnApprove.MinimumSize = new System.Drawing.Size(0, 38);
            btnApprove.Name = "btnApprove";
            btnApprove.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            btnApprove.TabIndex = 0;
            btnApprove.Text = "✅ Duyệt";
            btnApprove.UseVisualStyleBackColor = false;
            btnApprove.Click += new System.EventHandler(this.BtnApprove_Click);
            //
            // TripCargoReviewForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(1000, 640);
            Controls.Add(root);
            MaximizeBox = true;
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(760, 500);
            Name = "TripCargoReviewForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Kiểm tra hàng";
            root.ResumeLayout(false);
            root.PerformLayout();
            content.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridLines).EndInit();
            photoPanel.ResumeLayout(false);
            photoPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picPhoto).EndInit();
            photoNav.ResumeLayout(false);
            photoNav.PerformLayout();
            buttonBar.ResumeLayout(false);
            buttonBar.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.DataGridView gridLines;
        private System.Windows.Forms.DataGridViewTextBoxColumn colType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colQty;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPkg;
        private System.Windows.Forms.DataGridViewTextBoxColumn colKg;
        private System.Windows.Forms.PictureBox picPhoto;
        private System.Windows.Forms.Button btnPrev;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Label lblPhoto;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Button btnApprove;
        private System.Windows.Forms.Button btnReject;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.TableLayoutPanel content;
        private System.Windows.Forms.TableLayoutPanel photoPanel;
        private System.Windows.Forms.TableLayoutPanel photoNav;
        private System.Windows.Forms.FlowLayoutPanel buttonBar;
    }
}
