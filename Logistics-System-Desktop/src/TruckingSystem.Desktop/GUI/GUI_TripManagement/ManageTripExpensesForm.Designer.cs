namespace TruckingSystem.Desktop.GUI_TripManagement
{
    partial class ManageTripExpensesForm
    {
        private System.ComponentModel.IContainer components = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>Bố cục: root (TableLayoutPanel 1 cột) = tiêu đề / hàng nhập liệu / trạng thái /
        /// lưới (chiếm phần còn lại). Hàng nhập liệu là TableLayoutPanel: nhãn ở hàng trên, ô nhập
        /// ở hàng dưới, các ô giãn theo % bề ngang; nút tự co theo chữ. Không có tọa độ cứng.</summary>
        private void InitializeComponent()
        {
            root = new System.Windows.Forms.TableLayoutPanel();
            lblTitle = new System.Windows.Forms.Label();
            inputs = new System.Windows.Forms.TableLayoutPanel();
            lblType = new System.Windows.Forms.Label();
            lblAmount = new System.Windows.Forms.Label();
            lblNote = new System.Windows.Forms.Label();
            cboType = new System.Windows.Forms.ComboBox();
            numAmount = new TruckingSystem.Desktop.Common.NumberBox();
            txtNote = new System.Windows.Forms.TextBox();
            btnAddExpense = new System.Windows.Forms.Button();
            btnDeleteExpense = new System.Windows.Forms.Button();
            lblStatus = new System.Windows.Forms.Label();
            grid = new System.Windows.Forms.DataGridView();
            root.SuspendLayout();
            inputs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numAmount).BeginInit();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            //
            // root
            //
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.Controls.Add(lblTitle, 0, 0);
            root.Controls.Add(inputs, 0, 1);
            root.Controls.Add(lblStatus, 0, 2);
            root.Controls.Add(grid, 0, 3);
            root.Dock = System.Windows.Forms.DockStyle.Fill;
            root.Name = "root";
            root.Padding = new System.Windows.Forms.Padding(20, 16, 20, 16);
            root.RowCount = 4;
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            root.Size = new System.Drawing.Size(940, 540);
            root.TabIndex = 0;
            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            lblTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            lblTitle.Name = "lblTitle";
            lblTitle.TabIndex = 0;
            lblTitle.Text = "💰 Chi phí khác (ngoài xăng dầu)";
            //
            // inputs
            //
            inputs.AutoSize = true;
            inputs.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            inputs.ColumnCount = 5;
            inputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28F));
            inputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 24F));
            inputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 48F));
            inputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            inputs.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            inputs.Controls.Add(lblType, 0, 0);
            inputs.Controls.Add(lblAmount, 1, 0);
            inputs.Controls.Add(lblNote, 2, 0);
            inputs.Controls.Add(cboType, 0, 1);
            inputs.Controls.Add(numAmount, 1, 1);
            inputs.Controls.Add(txtNote, 2, 1);
            inputs.Controls.Add(btnAddExpense, 3, 1);
            inputs.Controls.Add(btnDeleteExpense, 4, 1);
            inputs.Dock = System.Windows.Forms.DockStyle.Fill;
            inputs.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            inputs.Name = "inputs";
            inputs.RowCount = 2;
            inputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            inputs.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            inputs.TabIndex = 1;
            //
            // lblType
            //
            lblType.AutoSize = true;
            lblType.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblType.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblType.Margin = new System.Windows.Forms.Padding(0, 0, 12, 4);
            lblType.Name = "lblType";
            lblType.TabIndex = 0;
            lblType.Text = "Loại chi phí";
            //
            // lblAmount
            //
            lblAmount.AutoSize = true;
            lblAmount.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblAmount.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblAmount.Margin = new System.Windows.Forms.Padding(0, 0, 12, 4);
            lblAmount.Name = "lblAmount";
            lblAmount.TabIndex = 1;
            lblAmount.Text = "Số tiền (đ)";
            //
            // lblNote
            //
            lblNote.AutoSize = true;
            lblNote.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblNote.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblNote.Margin = new System.Windows.Forms.Padding(0, 0, 12, 4);
            lblNote.Name = "lblNote";
            lblNote.TabIndex = 2;
            lblNote.Text = "Ghi chú (bắt buộc với \"Chi phí khác\")";
            //
            // cboType
            //
            cboType.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboType.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            cboType.Name = "cboType";
            cboType.TabIndex = 3;
            //
            // numAmount
            //
            numAmount.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            numAmount.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            numAmount.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            numAmount.Name = "numAmount";
            numAmount.TabIndex = 4;
            numAmount.IsMoney = true;
            //
            // txtNote
            //
            txtNote.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtNote.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            txtNote.Name = "txtNote";
            txtNote.PlaceholderText = "Ghi chú (tuỳ chọn)";
            txtNote.TabIndex = 5;
            //
            // btnAddExpense
            //
            btnAddExpense.Anchor = System.Windows.Forms.AnchorStyles.Left;
            btnAddExpense.AutoSize = true;
            btnAddExpense.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnAddExpense.BackColor = System.Drawing.Color.FromArgb(37, 99, 235);
            btnAddExpense.Cursor = System.Windows.Forms.Cursors.Hand;
            btnAddExpense.FlatAppearance.BorderSize = 0;
            btnAddExpense.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(29, 78, 216);
            btnAddExpense.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnAddExpense.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnAddExpense.ForeColor = System.Drawing.Color.White;
            btnAddExpense.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            btnAddExpense.MinimumSize = new System.Drawing.Size(0, 34);
            btnAddExpense.Name = "btnAddExpense";
            btnAddExpense.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            btnAddExpense.TabIndex = 6;
            btnAddExpense.Text = "➕ Thêm";
            btnAddExpense.UseVisualStyleBackColor = false;
            btnAddExpense.Click += new System.EventHandler(this.BtnAddExpense_Click);
            //
            // btnDeleteExpense
            //
            btnDeleteExpense.Anchor = System.Windows.Forms.AnchorStyles.Left;
            btnDeleteExpense.AutoSize = true;
            btnDeleteExpense.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnDeleteExpense.BackColor = System.Drawing.Color.White;
            btnDeleteExpense.Cursor = System.Windows.Forms.Cursors.Hand;
            btnDeleteExpense.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(220, 38, 38);
            btnDeleteExpense.FlatAppearance.BorderSize = 1;
            btnDeleteExpense.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnDeleteExpense.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            btnDeleteExpense.ForeColor = System.Drawing.Color.FromArgb(220, 38, 38);
            btnDeleteExpense.Margin = new System.Windows.Forms.Padding(0);
            btnDeleteExpense.MinimumSize = new System.Drawing.Size(0, 34);
            btnDeleteExpense.Name = "btnDeleteExpense";
            btnDeleteExpense.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            btnDeleteExpense.TabIndex = 7;
            btnDeleteExpense.Text = "🗑️ Xóa đã chọn";
            btnDeleteExpense.UseVisualStyleBackColor = false;
            btnDeleteExpense.Click += new System.EventHandler(this.BtnDeleteExpense_Click);
            //
            // lblStatus
            //
            lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblStatus.AutoSize = true;
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            lblStatus.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            lblStatus.Margin = new System.Windows.Forms.Padding(0, 4, 0, 8);
            lblStatus.Name = "lblStatus";
            lblStatus.TabIndex = 2;
            //
            // grid
            //
            grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid.Dock = System.Windows.Forms.DockStyle.Fill;
            grid.Margin = new System.Windows.Forms.Padding(0);
            grid.Name = "grid";
            grid.TabIndex = 3;
            //
            // ManageTripExpensesForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.White;
            ClientSize = new System.Drawing.Size(940, 540);
            Controls.Add(root);
            MinimumSize = new System.Drawing.Size(720, 420);
            Name = "ManageTripExpensesForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Chi phí khác";
            root.ResumeLayout(false);
            root.PerformLayout();
            inputs.ResumeLayout(false);
            inputs.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numAmount).EndInit();
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.TableLayoutPanel inputs;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.Label lblAmount;
        private System.Windows.Forms.Label lblNote;
        private System.Windows.Forms.ComboBox cboType;
        private TruckingSystem.Desktop.Common.NumberBox numAmount;
        private System.Windows.Forms.TextBox txtNote;
        private System.Windows.Forms.Button btnAddExpense;
        private System.Windows.Forms.Button btnDeleteExpense;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView grid;
    }
}
