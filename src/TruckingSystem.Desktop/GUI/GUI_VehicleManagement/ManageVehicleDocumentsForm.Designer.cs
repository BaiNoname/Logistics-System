namespace TruckingSystem.Desktop.GUI_VehicleManagement
{
    partial class ManageVehicleDocumentsForm
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
            btnAddDocument = new Button();
            btnDeleteDocument = new Button();
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
            lblTitle.Location = new Point(30, 9);
            lblTitle.Margin = new Padding(4, 0, 4, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(180, 36);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "📄 Giấy tờ xe";
            // 
            // btnAddDocument
            // 
            btnAddDocument.BackColor = Color.FromArgb(37, 99, 235);
            btnAddDocument.Cursor = Cursors.Hand;
            btnAddDocument.FlatAppearance.BorderSize = 0;
            btnAddDocument.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
            btnAddDocument.FlatStyle = FlatStyle.Flat;
            btnAddDocument.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnAddDocument.ForeColor = Color.White;
            btnAddDocument.Location = new Point(817, 24);
            btnAddDocument.Margin = new Padding(4, 4, 4, 4);
            btnAddDocument.Name = "btnAddDocument";
            btnAddDocument.Size = new Size(240, 57);
            btnAddDocument.TabIndex = 1;
            btnAddDocument.Text = "➕ Nhập giấy tờ mới";
            btnAddDocument.UseVisualStyleBackColor = false;
            btnAddDocument.Click += BtnAddDocument_Click;
            // 
            // btnDeleteDocument
            // 
            btnDeleteDocument.BackColor = Color.White;
            btnDeleteDocument.Cursor = Cursors.Hand;
            btnDeleteDocument.FlatAppearance.BorderColor = Color.FromArgb(220, 38, 38);
            btnDeleteDocument.FlatStyle = FlatStyle.Flat;
            btnDeleteDocument.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnDeleteDocument.ForeColor = Color.FromArgb(220, 38, 38);
            btnDeleteDocument.Location = new Point(1065, 24);
            btnDeleteDocument.Margin = new Padding(4, 4, 4, 4);
            btnDeleteDocument.Name = "btnDeleteDocument";
            btnDeleteDocument.Size = new Size(210, 57);
            btnDeleteDocument.TabIndex = 2;
            btnDeleteDocument.Text = "🗑️ Xóa giấy tờ";
            btnDeleteDocument.UseVisualStyleBackColor = false;
            btnDeleteDocument.Click += BtnDeleteDocument_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
            lblStatus.Location = new Point(30, 84);
            lblStatus.Margin = new Padding(4, 0, 4, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(0, 25);
            lblStatus.TabIndex = 3;
            // 
            // grid
            // 
            grid.ColumnHeadersHeight = 34;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.Location = new Point(30, 120);
            grid.Margin = new Padding(4, 4, 4, 4);
            grid.Name = "grid";
            grid.RowHeadersWidth = 62;
            grid.Size = new Size(1320, 570);
            grid.TabIndex = 4;
            // 
            // ManageVehicleDocumentsForm
            // 
            AutoScaleDimensions = new SizeF(144F, 144F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(1380, 750);
            Controls.Add(lblTitle);
            Controls.Add(btnAddDocument);
            Controls.Add(btnDeleteDocument);
            Controls.Add(lblStatus);
            Controls.Add(grid);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(6, 6, 6, 6);
            MaximizeBox = false;
            Name = "ManageVehicleDocumentsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Giấy tờ xe";
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnAddDocument;
        private System.Windows.Forms.Button btnDeleteDocument;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView grid;
    }
}