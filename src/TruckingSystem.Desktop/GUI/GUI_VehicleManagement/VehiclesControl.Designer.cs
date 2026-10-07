namespace TruckingSystem.Desktop.GUI_VehicleManagement
{
    partial class VehiclesControl
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            lblTitle = new Label();
            btnRefresh = new Button();
            btnCreate = new Button();
            btnDocuments = new Button();
            btnEdit = new Button();
            lblStatus = new Label();
            grid = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(0, 0);
            lblTitle.Margin = new Padding(4, 0, 4, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(208, 46);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Đội xe";
            // 
            // btnRefresh
            // 
            btnRefresh.BackColor = Color.White;
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnRefresh.ForeColor = Color.FromArgb(15, 23, 42);
            btnRefresh.Location = new Point(655, 10);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(148, 52);
            btnRefresh.TabIndex = 1;
            btnRefresh.Text = "🔄 Làm mới";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Click += BtnRefresh_Click;
            // 
            // btnCreate
            // 
            btnCreate.BackColor = Color.FromArgb(37, 99, 235);
            btnCreate.Cursor = Cursors.Hand;
            btnCreate.FlatAppearance.BorderSize = 0;
            btnCreate.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
            btnCreate.FlatStyle = FlatStyle.Flat;
            btnCreate.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCreate.ForeColor = Color.White;
            btnCreate.Location = new Point(809, 10);
            btnCreate.Name = "btnCreate";
            btnCreate.Size = new Size(150, 52);
            btnCreate.TabIndex = 2;
            btnCreate.Text = "➕ Thêm xe mới";
            btnCreate.UseVisualStyleBackColor = false;
            btnCreate.Click += BtnCreate_Click;
            // 
            // btnDocuments
            // 
            btnDocuments.BackColor = Color.White;
            btnDocuments.Cursor = Cursors.Hand;
            btnDocuments.FlatStyle = FlatStyle.Flat;
            btnDocuments.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnDocuments.ForeColor = Color.FromArgb(15, 23, 42);
            btnDocuments.Location = new Point(1121, 10);
            btnDocuments.Name = "btnDocuments";
            btnDocuments.Size = new Size(160, 52);
            btnDocuments.TabIndex = 3;
            btnDocuments.Text = "📄 Giấy tờ xe";
            btnDocuments.UseVisualStyleBackColor = false;
            btnDocuments.Click += BtnDocuments_Click;
            // 
            // btnEdit
            // 
            btnEdit.BackColor = Color.White;
            btnEdit.Cursor = Cursors.Hand;
            btnEdit.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnEdit.ForeColor = Color.FromArgb(15, 23, 42);
            btnEdit.Location = new Point(965, 10);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(150, 52);
            btnEdit.TabIndex = 3;
            btnEdit.Text = "✏️ Sửa xe";
            btnEdit.UseVisualStyleBackColor = false;
            btnEdit.Click += BtnEdit_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
            lblStatus.Location = new Point(0, 44);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(0, 25);
            lblStatus.TabIndex = 3;
            // 
            // grid
            // 
            grid.ColumnHeadersHeight = 34;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.Location = new Point(0, 76);
            grid.Name = "grid";
            grid.RowHeadersWidth = 62;
            grid.Size = new Size(1306, 560);
            grid.TabIndex = 4;
            // 
            // VehiclesControl
            // 
            AutoScaleDimensions = new SizeF(144F, 144F);
            BackColor = Color.FromArgb(241, 245, 249);
            Controls.Add(lblTitle);
            Controls.Add(btnRefresh);
            Controls.Add(btnCreate);
            Controls.Add(btnDocuments);
            Controls.Add(btnEdit);
            Controls.Add(lblStatus);
            Controls.Add(grid);
            Margin = new Padding(3, 3, 3, 3);
            Name = "VehiclesControl";
            Size = new Size(1450, 614);
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnDocuments;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView grid;
    }
}