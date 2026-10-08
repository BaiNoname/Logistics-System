namespace TruckingSystem.Desktop.GUI_NotificationManagement
{
    partial class NotificationsForm
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
            lblTitle = new Label();
            btnRefresh = new Button();
            btnMarkAllRead = new Button();
            lblCategory = new Label();
            cboCategory = new ComboBox();
            lblReadState = new Label();
            cboReadState = new ComboBox();
            txtSearch = new TextBox();
            lblStatus = new Label();
            grid = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(30, 24);
            lblTitle.Margin = new Padding(4, 0, 4, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(191, 36);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "🔔 Thông báo";
            //
            // btnRefresh
            //
            btnRefresh.BackColor = Color.White;
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Font = new Font("Segoe UI", 9F);
            btnRefresh.ForeColor = Color.FromArgb(15, 23, 42);
            btnRefresh.Location = new Point(740, 24);
            btnRefresh.Margin = new Padding(4);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(210, 51);
            btnRefresh.TabIndex = 1;
            btnRefresh.Text = "🔄 Làm mới";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Click += BtnRefresh_Click;
            //
            // btnMarkAllRead
            //
            btnMarkAllRead.BackColor = Color.White;
            btnMarkAllRead.Cursor = Cursors.Hand;
            btnMarkAllRead.FlatStyle = FlatStyle.Flat;
            btnMarkAllRead.Font = new Font("Segoe UI", 9F);
            btnMarkAllRead.ForeColor = Color.FromArgb(15, 23, 42);
            btnMarkAllRead.Location = new Point(960, 24);
            btnMarkAllRead.Margin = new Padding(4);
            btnMarkAllRead.Name = "btnMarkAllRead";
            btnMarkAllRead.Size = new Size(270, 51);
            btnMarkAllRead.TabIndex = 2;
            btnMarkAllRead.Text = "✅ Đánh dấu tất cả đã đọc";
            btnMarkAllRead.UseVisualStyleBackColor = false;
            btnMarkAllRead.Click += BtnMarkAllRead_Click;
            //
            // lblCategory
            //
            lblCategory.AutoSize = true;
            lblCategory.Font = new Font("Segoe UI", 9F);
            lblCategory.ForeColor = Color.FromArgb(100, 116, 139);
            lblCategory.Location = new Point(30, 96);
            lblCategory.Margin = new Padding(4, 0, 4, 0);
            lblCategory.Name = "lblCategory";
            lblCategory.Size = new Size(53, 25);
            lblCategory.TabIndex = 3;
            lblCategory.Text = "Loại:";
            //
            // cboCategory
            //
            cboCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCategory.Font = new Font("Segoe UI", 9F);
            cboCategory.Location = new Point(95, 92);
            cboCategory.Margin = new Padding(4);
            cboCategory.Name = "cboCategory";
            cboCategory.Size = new Size(260, 33);
            cboCategory.TabIndex = 4;
            //
            // lblReadState
            //
            lblReadState.AutoSize = true;
            lblReadState.Font = new Font("Segoe UI", 9F);
            lblReadState.ForeColor = Color.FromArgb(100, 116, 139);
            lblReadState.Location = new Point(380, 96);
            lblReadState.Margin = new Padding(4, 0, 4, 0);
            lblReadState.Name = "lblReadState";
            lblReadState.Size = new Size(100, 25);
            lblReadState.TabIndex = 5;
            lblReadState.Text = "Trạng thái:";
            //
            // cboReadState
            //
            cboReadState.DropDownStyle = ComboBoxStyle.DropDownList;
            cboReadState.Font = new Font("Segoe UI", 9F);
            cboReadState.Location = new Point(490, 92);
            cboReadState.Margin = new Padding(4);
            cboReadState.Name = "cboReadState";
            cboReadState.Size = new Size(190, 33);
            cboReadState.TabIndex = 6;
            //
            // txtSearch
            //
            txtSearch.Font = new Font("Segoe UI", 9F);
            txtSearch.Location = new Point(705, 92);
            txtSearch.Margin = new Padding(4);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "🔍 Tìm theo biển số, tên tài xế, mã chuyến...";
            txtSearch.Size = new Size(525, 31);
            txtSearch.TabIndex = 7;
            //
            // lblStatus
            //
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
            lblStatus.Location = new Point(30, 140);
            lblStatus.Margin = new Padding(4, 0, 4, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(0, 25);
            lblStatus.TabIndex = 8;
            //
            // grid
            //
            grid.ColumnHeadersHeight = 34;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.Location = new Point(-2, 178);
            grid.Margin = new Padding(4);
            grid.Name = "grid";
            grid.RowHeadersWidth = 62;
            grid.Size = new Size(1262, 602);
            grid.TabIndex = 9;
            grid.CellClick += Grid_CellClick;
            //
            // NotificationsForm
            //
            AutoScaleDimensions = new SizeF(144F, 144F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(1260, 780);
            Controls.Add(lblTitle);
            Controls.Add(btnRefresh);
            Controls.Add(btnMarkAllRead);
            Controls.Add(lblCategory);
            Controls.Add(cboCategory);
            Controls.Add(lblReadState);
            Controls.Add(cboReadState);
            Controls.Add(txtSearch);
            Controls.Add(lblStatus);
            Controls.Add(grid);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(6);
            MaximizeBox = false;
            Name = "NotificationsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Thông báo";
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnMarkAllRead;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.ComboBox cboCategory;
        private System.Windows.Forms.Label lblReadState;
        private System.Windows.Forms.ComboBox cboReadState;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView grid;
    }
}
