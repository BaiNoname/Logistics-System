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

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblInfo = new System.Windows.Forms.Label();
            this.gridLines = new System.Windows.Forms.DataGridView();
            this.colType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colQty = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPkg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colKg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.picPhoto = new System.Windows.Forms.PictureBox();
            this.btnPrev = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.lblPhoto = new System.Windows.Forms.Label();
            this.btnOpen = new System.Windows.Forms.Button();
            this.btnApprove = new System.Windows.Forms.Button();
            this.btnReject = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.gridLines)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPhoto)).BeginInit();
            this.SuspendLayout();
            //
            // lblTitle
            //
            this.lblTitle.Location = new System.Drawing.Point(16, 12);
            this.lblTitle.Size = new System.Drawing.Size(940, 30);
            this.lblTitle.Text = "🔎 Kiểm tra hàng";
            this.lblTitle.Font = TruckingSystem.Desktop.Common.Theme.FontSectionTitle;
            this.lblTitle.ForeColor = TruckingSystem.Desktop.Common.Theme.TextPrimary;
            //
            // lblInfo
            //
            this.lblInfo.Location = new System.Drawing.Point(18, 46);
            this.lblInfo.Size = new System.Drawing.Size(940, 40);
            this.lblInfo.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblInfo.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            //
            // gridLines
            //
            this.gridLines.Location = new System.Drawing.Point(16, 92);
            this.gridLines.Size = new System.Drawing.Size(420, 460);
            this.gridLines.Name = "gridLines";
            this.gridLines.AutoGenerateColumns = false;
            this.gridLines.AllowUserToAddRows = false;
            this.gridLines.ReadOnly = true;
            this.gridLines.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { this.colType, this.colQty, this.colPkg, this.colKg });
            //
            // colType
            //
            this.colType.Name = "colType";
            this.colType.HeaderText = "Hàng hóa";
            this.colType.FillWeight = 46F;
            //
            // colQty
            //
            this.colQty.Name = "colQty";
            this.colQty.HeaderText = "Số lượng";
            this.colQty.FillWeight = 18F;
            //
            // colPkg
            //
            this.colPkg.Name = "colPkg";
            this.colPkg.HeaderText = "Số kiện";
            this.colPkg.FillWeight = 16F;
            //
            // colKg
            //
            this.colKg.Name = "colKg";
            this.colKg.HeaderText = "Kg";
            this.colKg.FillWeight = 20F;
            //
            // picPhoto
            //
            this.picPhoto.Location = new System.Drawing.Point(452, 92);
            this.picPhoto.Size = new System.Drawing.Size(512, 410);
            this.picPhoto.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picPhoto.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.picPhoto.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picPhoto.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picPhoto.TabStop = false;
            this.picPhoto.DoubleClick += new System.EventHandler(this.PicPhoto_DoubleClick);
            //
            // btnPrev
            //
            this.btnPrev.Location = new System.Drawing.Point(452, 512);
            this.btnPrev.Size = new System.Drawing.Size(90, 34);
            this.btnPrev.Text = "◀ Trước";
            this.btnPrev.BackColor = System.Drawing.Color.White;
            this.btnPrev.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrev.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.btnPrev.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPrev.Click += new System.EventHandler(this.BtnPrev_Click);
            //
            // btnNext
            //
            this.btnNext.Location = new System.Drawing.Point(548, 512);
            this.btnNext.Size = new System.Drawing.Size(90, 34);
            this.btnNext.Text = "Sau ▶";
            this.btnNext.BackColor = System.Drawing.Color.White;
            this.btnNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNext.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.btnNext.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNext.Click += new System.EventHandler(this.BtnNext_Click);
            //
            // lblPhoto
            //
            this.lblPhoto.Location = new System.Drawing.Point(646, 520);
            this.lblPhoto.Size = new System.Drawing.Size(190, 24);
            this.lblPhoto.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.lblPhoto.ForeColor = TruckingSystem.Desktop.Common.Theme.TextSecondary;
            //
            // btnOpen
            //
            this.btnOpen.Location = new System.Drawing.Point(844, 512);
            this.btnOpen.Size = new System.Drawing.Size(120, 34);
            this.btnOpen.Text = "🔍 Mở ảnh lớn";
            this.btnOpen.BackColor = System.Drawing.Color.White;
            this.btnOpen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpen.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.btnOpen.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOpen.Click += new System.EventHandler(this.BtnOpen_Click);
            //
            // btnApprove
            //
            this.btnApprove.Location = new System.Drawing.Point(560, 568);
            this.btnApprove.Size = new System.Drawing.Size(160, 38);
            this.btnApprove.Text = "✅ Duyệt";
            this.btnApprove.BackColor = TruckingSystem.Desktop.Common.Theme.Primary;
            this.btnApprove.ForeColor = System.Drawing.Color.White;
            this.btnApprove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApprove.FlatAppearance.BorderSize = 0;
            this.btnApprove.Font = TruckingSystem.Desktop.Common.Theme.FontBold;
            this.btnApprove.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnApprove.Click += new System.EventHandler(this.BtnApprove_Click);
            //
            // btnReject
            //
            this.btnReject.Location = new System.Drawing.Point(730, 568);
            this.btnReject.Size = new System.Drawing.Size(120, 38);
            this.btnReject.Text = "❌ Từ chối";
            this.btnReject.BackColor = System.Drawing.Color.White;
            this.btnReject.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReject.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.btnReject.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnReject.ForeColor = TruckingSystem.Desktop.Common.Theme.Danger;
            this.btnReject.Click += new System.EventHandler(this.BtnReject_Click);
            //
            // btnClose
            //
            this.btnClose.Location = new System.Drawing.Point(864, 568);
            this.btnClose.Size = new System.Drawing.Size(100, 38);
            this.btnClose.Text = "Đóng";
            this.btnClose.BackColor = System.Drawing.Color.White;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = TruckingSystem.Desktop.Common.Theme.FontSmall;
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            //
            // TripCargoReviewForm
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(980, 620);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.gridLines);
            this.Controls.Add(this.picPhoto);
            this.Controls.Add(this.btnPrev);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.lblPhoto);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.btnApprove);
            this.Controls.Add(this.btnReject);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TripCargoReviewForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Kiểm tra hàng";
            ((System.ComponentModel.ISupportInitialize)(this.gridLines)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPhoto)).EndInit();
            this.ResumeLayout(false);
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
    }
}
