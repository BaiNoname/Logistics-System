using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_InvoiceManagement
{
    partial class InvoicesControl
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
            lblNote = new Label();
            lblStatus = new Label();
            grid = new DataGridView();
            root = new TableLayoutPanel();
            toolbar = new FlowLayoutPanel();
            filterBar = new FilterBar
            {
                Name = "filterBar", Dock = DockStyle.Fill, TabIndex = 4,
                SearchPlaceholder = "Mã hóa đơn, mã đơn, tên khách hàng"
            };
            lblSummaryPeriod = new Label
            {
                AutoSize = true, Dock = DockStyle.Fill, ForeColor = Theme.TextSecondary,
                Text = "Thống kê tháng hiện tại", Margin = new Padding(2, 6, 2, 8)
            };
            var summary = new FlowLayoutPanel
            {
                Name = "summary", Dock = DockStyle.Fill, AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true, Margin = Padding.Empty
            };
            lblMonthCount = new Label { Name = "lblMonthCount", Text = "—" };
            lblRevenue = new Label { Name = "lblRevenue", Text = "—" };
            lblUnpaid = new Label { Name = "lblUnpaid", Text = "—" };
            lblPaid = new Label { Name = "lblPaid", Text = "—" };
            lblOverdue = new Label { Name = "lblOverdue", Text = "Trong đó quá hạn: —" };
            summary.Controls.Add(MakeSummaryCard("Hóa đơn tháng này", lblMonthCount,
                new Label { Text = "Tất cả trạng thái" }, Theme.Primary));
            summary.Controls.Add(MakeSummaryCard("Tổng doanh thu", lblRevenue,
                new Label { Text = "Gồm nháp, không gồm hóa đơn hủy" }, Theme.Primary));
            summary.Controls.Add(MakeSummaryCard("Chưa thanh toán / Quá hạn", lblUnpaid,
                lblOverdue, Theme.Danger));
            summary.Controls.Add(MakeSummaryCard("Đã thanh toán", lblPaid,
                new Label { Text = "Đã thanh toán đầy đủ" }, Theme.Success));
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            root.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(2, 0);
            lblTitle.Margin = new Padding(2, 0, 2, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(112, 32);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Hóa đơn";
            // 
            // btnRefresh
            // 
            btnRefresh.BackColor = Color.White;
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnRefresh.ForeColor = Color.FromArgb(15, 23, 42);
            btnRefresh.Location = new Point(2, 33);
            btnRefresh.Margin = new Padding(2);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(103, 27);
            btnRefresh.TabIndex = 1;
            btnRefresh.Text = "🔄 Làm mới";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Click += BtnRefresh_Click;
            btnCreate.Name = "btnCreate";
            btnCreate.Text = "Lập hóa đơn";
            btnCreate.Size = new Size(140, 38);
            btnCreate.BackColor = Common.Theme.Primary;
            btnCreate.ForeColor = Color.White;
            btnCreate.Font = Common.Theme.FontBold;
            btnCreate.FlatStyle = FlatStyle.Flat;
            btnCreate.FlatAppearance.BorderSize = 0;
            btnCreate.Cursor = Cursors.Hand;
            btnCreate.Click += BtnCreate_Click;
            btnRefresh.Size = new Size(140, 38);
            // 
            // lblNote
            // 
            lblNote.AutoSize = true;
            lblNote.Dock = DockStyle.Fill;
            lblNote.Font = new Font("Segoe UI", 9F);
            lblNote.ForeColor = Color.FromArgb(100, 116, 139);
            lblNote.Location = new Point(0, 0);
            lblNote.Margin = new Padding(2, 0, 2, 0);
            lblNote.Name = "lblNote";
            lblNote.Size = new Size(340, 15);
            lblNote.TabIndex = 2;
            lblNote.Text = "Lập hóa đơn nháp cho đơn đã hoàn thành và chưa có hóa đơn.";
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Dock = DockStyle.Fill;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
            lblStatus.Location = new Point(0, 0);
            lblStatus.Margin = new Padding(2, 0, 2, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(0, 15);
            lblStatus.TabIndex = 3;
            // 
            // grid
            // 
            grid.ColumnHeadersHeight = 34;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.Dock = DockStyle.Fill;
            grid.Location = new Point(0, 0);
            grid.Margin = new Padding(2);
            grid.Name = "grid";
            grid.RowHeadersWidth = 62;
            grid.Size = new Size(945, 379);
            grid.TabIndex = 6;
            // 
            // root
            // 
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            root.Dock = DockStyle.Fill;
            root.ColumnCount = 1;
            root.RowCount = 8;
            root.Padding = new Padding(8);
            root.Controls.Add(lblTitle, 0, 0);
            root.Controls.Add(lblSummaryPeriod, 0, 1);
            root.Controls.Add(summary, 0, 2);
            root.Controls.Add(lblNote, 0, 3);
            root.Controls.Add(toolbar, 0, 4);
            root.Controls.Add(filterBar, 0, 5);
            root.Controls.Add(lblStatus, 0, 6);
            root.Controls.Add(grid, 0, 7);
            root.Location = new Point(0, 0);
            root.Margin = new Padding(2);
            root.Name = "root";
            root.RowStyles.Add(new RowStyle());
            root.RowStyles.Add(new RowStyle());
            root.RowStyles.Add(new RowStyle());
            root.RowStyles.Add(new RowStyle());
            root.RowStyles.Add(new RowStyle());
            root.RowStyles.Add(new RowStyle());
            root.RowStyles.Add(new RowStyle());
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.Size = new Size(133, 67);
            root.TabIndex = 5;
            // 
            // toolbar
            // 
            toolbar.Location = new Point(2, 2);
            toolbar.Margin = new Padding(2);
            toolbar.Name = "toolbar";
            toolbar.AutoSize = true;
            toolbar.Dock = DockStyle.Fill;
            toolbar.WrapContents = true;
            toolbar.Controls.Add(btnCreate);
            toolbar.Controls.Add(btnRefresh);
            toolbar.Size = new Size(129, 67);
            toolbar.TabIndex = 3;
            // 
            // InvoicesControl
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            BackColor = Color.FromArgb(241, 245, 249);
            Controls.Add(root);
            Margin = new Padding(2);
            Name = "InvoicesControl";
            Size = new Size(945, 379);
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            root.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private static TableLayoutPanel MakeSummaryCard(string title, Label value, Label caption, Color accent)
        {
            var card = new TableLayoutPanel
            {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(215, 100), BackColor = Theme.CardBg,
                ColumnCount = 1, RowCount = 3, Padding = new Padding(12), Margin = new Padding(0, 0, 10, 10)
            };
            card.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            for (var i = 0; i < 3; i++) card.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var heading = new Label { Text = title, AutoSize = true, ForeColor = Theme.TextSecondary, Font = Theme.FontSmall };
            value.AutoSize = true;
            value.Font = Theme.FontSectionTitle;
            value.ForeColor = accent;
            value.Margin = new Padding(3, 6, 3, 6);
            caption.AutoSize = true;
            caption.Font = Theme.FontSmall;
            caption.ForeColor = Theme.TextSecondary;
            card.Controls.Add(heading, 0, 0);
            card.Controls.Add(value, 0, 1);
            card.Controls.Add(caption, 0, 2);
            return card;
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Label lblNote;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.DataGridView grid;
        private TableLayoutPanel root;
        private FlowLayoutPanel toolbar;
        private FilterBar filterBar;
        private Label lblSummaryPeriod;
        private Label lblMonthCount;
        private Label lblRevenue;
        private Label lblUnpaid;
        private Label lblOverdue;
        private Label lblPaid;
    }
}
