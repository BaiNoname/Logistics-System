namespace TruckingSystem.Desktop.GUI_TripManagement
{
    partial class ManageIncidentsForm
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
            btnAddIncident = new Button();
            btnDeleteIncident = new Button();
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
            lblTitle.Size = new Size(275, 36);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "⚠️ Sự cố giữa đường";
            // 
            // btnAddIncident
            // 
            btnAddIncident.BackColor = Color.FromArgb(37, 99, 235);
            btnAddIncident.Cursor = Cursors.Hand;
            btnAddIncident.FlatAppearance.BorderSize = 0;
            btnAddIncident.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
            btnAddIncident.FlatStyle = FlatStyle.Flat;
            btnAddIncident.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnAddIncident.ForeColor = Color.White;
            btnAddIncident.Location = new Point(798, 24);
            btnAddIncident.Margin = new Padding(4, 4, 4, 4);
            btnAddIncident.Name = "btnAddIncident";
            btnAddIncident.Size = new Size(240, 57);
            btnAddIncident.TabIndex = 1;
            btnAddIncident.Text = "➕ Ghi nhận sự cố";
            btnAddIncident.UseVisualStyleBackColor = false;
            btnAddIncident.Click += BtnAddIncident_Click;
            // 
            // btnDeleteIncident
            // 
            btnDeleteIncident.BackColor = Color.White;
            btnDeleteIncident.Cursor = Cursors.Hand;
            btnDeleteIncident.FlatAppearance.BorderColor = Color.FromArgb(220, 38, 38);
            btnDeleteIncident.FlatStyle = FlatStyle.Flat;
            btnDeleteIncident.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnDeleteIncident.ForeColor = Color.FromArgb(220, 38, 38);
            btnDeleteIncident.Location = new Point(1064, 24);
            btnDeleteIncident.Margin = new Padding(4, 4, 4, 4);
            btnDeleteIncident.Name = "btnDeleteIncident";
            btnDeleteIncident.Size = new Size(210, 57);
            btnDeleteIncident.TabIndex = 2;
            btnDeleteIncident.Text = "🗑️ Xóa sự cố";
            btnDeleteIncident.UseVisualStyleBackColor = false;
            btnDeleteIncident.Click += BtnDeleteIncident_Click;
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
            // ManageIncidentsForm
            // 
            AutoScaleDimensions = new SizeF(144F, 144F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(1380, 750);
            Controls.Add(lblTitle);
            Controls.Add(btnAddIncident);
            Controls.Add(btnDeleteIncident);
            Controls.Add(lblStatus);
            Controls.Add(grid);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(6, 6, 6, 6);
            MaximizeBox = false;
            Name = "ManageIncidentsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Sự cố";
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnAddIncident;
        private System.Windows.Forms.Button btnDeleteIncident;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView grid;
    }
}
