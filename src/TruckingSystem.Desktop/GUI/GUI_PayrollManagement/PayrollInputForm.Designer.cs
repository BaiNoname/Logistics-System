namespace TruckingSystem.Desktop.GUI_PayrollManagement;

partial class PayrollInputForm
{
    private System.ComponentModel.IContainer components = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        _sections = new TabControl();
        _salaryPage = new TabPage();
        _salaryPageLayout = new TableLayoutPanel();
        SalaryHistory = new DataGridView();
        SalaryEditor = new TableLayoutPanel();
        SalaryDateLabel = new Label();
        SalaryDate = new DateTimePicker();
        SalaryAmountLabel = new Label();
        SalaryAmount = new NumericUpDown();
        SalaryReasonLabel = new Label();
        SalaryReason = new TextBox();
        SalaryHint = new Label();
        _itemPage = new TabPage();
        _itemPageLayout = new TableLayoutPanel();
        ItemKindLabel = new Label();
        ItemKind = new ComboBox();
        ItemAmountLabel = new Label();
        ItemAmount = new NumericUpDown();
        ItemProrateLabel = new Label();
        ItemProrate = new CheckBox();
        ItemReasonLabel = new Label();
        ItemReason = new TextBox();
        ItemHint = new Label();
        _paymentPage = new TabPage();
        _paymentPageLayout = new TableLayoutPanel();
        PaymentBalance = new Label();
        PaymentAmountLabel = new Label();
        PaymentAmount = new NumericUpDown();
        PaymentDateLabel = new Label();
        PaymentDate = new DateTimePicker();
        PaymentMethodLabel = new Label();
        PaymentMethod = new ComboBox();
        PaymentReferenceLabel = new Label();
        PaymentReference = new TextBox();
        PaymentHint = new Label();
        _detailsPage = new TabPage();
        _detailsPageLayout = new TableLayoutPanel();
        DetailPeriod = new Label();
        DetailSummary = new TextBox();
        DetailItems = new DataGridView();
        DetailPayments = new DataGridView();
        DetailDriverDates = new TextBox();
        _footer = new TableLayoutPanel();
        _error = new Label();
        _buttons = new FlowLayoutPanel();
        _cancel = new Button();
        _save = new Button();
        _sections.SuspendLayout();
        _salaryPage.SuspendLayout();
        _salaryPageLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)SalaryHistory).BeginInit();
        SalaryEditor.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)SalaryAmount).BeginInit();
        _itemPage.SuspendLayout();
        _itemPageLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)ItemAmount).BeginInit();
        _paymentPage.SuspendLayout();
        _paymentPageLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)PaymentAmount).BeginInit();
        _detailsPage.SuspendLayout();
        _detailsPageLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)DetailItems).BeginInit();
        ((System.ComponentModel.ISupportInitialize)DetailPayments).BeginInit();
        _footer.SuspendLayout();
        _buttons.SuspendLayout();
        SuspendLayout();
        // 
        // _sections
        // 
        _sections.Controls.Add(_salaryPage);
        _sections.Controls.Add(_itemPage);
        _sections.Controls.Add(_paymentPage);
        _sections.Controls.Add(_detailsPage);
        _sections.Dock = DockStyle.Fill;
        _sections.Location = new Point(0, 0);
        _sections.Multiline = true;
        _sections.Name = "_sections";
        _sections.SelectedIndex = 0;
        _sections.Size = new Size(720, 527);
        _sections.TabIndex = 0;
        // 
        // _salaryPage
        // 
        _salaryPage.AutoScroll = true;
        _salaryPage.Controls.Add(_salaryPageLayout);
        _salaryPage.Location = new Point(4, 26);
        _salaryPage.Name = "_salaryPage";
        _salaryPage.Size = new Size(712, 497);
        _salaryPage.TabIndex = 1;
        _salaryPage.Text = "Mức lương";
        _salaryPage.UseVisualStyleBackColor = true;
        // 
        // _salaryPageLayout
        // 
        _salaryPageLayout.AutoSize = true;
        _salaryPageLayout.ColumnCount = 2;
        _salaryPageLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 43F));
        _salaryPageLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 57F));
        _salaryPageLayout.Controls.Add(SalaryHistory, 0, 0);
        _salaryPageLayout.Controls.Add(SalaryEditor, 0, 1);
        _salaryPageLayout.Dock = DockStyle.Top;
        _salaryPageLayout.Location = new Point(0, 0);
        _salaryPageLayout.Name = "_salaryPageLayout";
        _salaryPageLayout.Padding = new Padding(12);
        _salaryPageLayout.RowCount = 2;
        _salaryPageLayout.RowStyles.Add(new RowStyle());
        _salaryPageLayout.RowStyles.Add(new RowStyle());
        _salaryPageLayout.Size = new Size(712, 463);
        _salaryPageLayout.TabIndex = 0;
        // 
        // SalaryHistory
        // 
        SalaryHistory.AccessibleName = "SalaryHistory";
        SalaryHistory.AllowUserToAddRows = false;
        SalaryHistory.AllowUserToDeleteRows = false;
        SalaryHistory.BackgroundColor = Color.White;
        _salaryPageLayout.SetColumnSpan(SalaryHistory, 2);
        SalaryHistory.Dock = DockStyle.Top;
        SalaryHistory.Location = new Point(16, 20);
        SalaryHistory.Margin = new Padding(4, 8, 4, 8);
        SalaryHistory.MultiSelect = false;
        SalaryHistory.Name = "SalaryHistory";
        SalaryHistory.ReadOnly = true;
        SalaryHistory.RowHeadersVisible = false;
        SalaryHistory.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        SalaryHistory.Size = new Size(680, 180);
        SalaryHistory.TabIndex = 1;
        // 
        // SalaryEditor
        // 
        SalaryEditor.AutoSize = true;
        SalaryEditor.ColumnCount = 2;
        _salaryPageLayout.SetColumnSpan(SalaryEditor, 2);
        SalaryEditor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 43F));
        SalaryEditor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 57F));
        SalaryEditor.Controls.Add(SalaryDateLabel, 0, 0);
        SalaryEditor.Controls.Add(SalaryDate, 1, 0);
        SalaryEditor.Controls.Add(SalaryAmountLabel, 0, 1);
        SalaryEditor.Controls.Add(SalaryAmount, 1, 1);
        SalaryEditor.Controls.Add(SalaryReasonLabel, 0, 2);
        SalaryEditor.Controls.Add(SalaryReason, 1, 2);
        SalaryEditor.Controls.Add(SalaryHint, 0, 3);
        SalaryEditor.Dock = DockStyle.Top;
        SalaryEditor.Location = new Point(16, 216);
        SalaryEditor.Margin = new Padding(4, 8, 4, 8);
        SalaryEditor.Name = "SalaryEditor";
        SalaryEditor.Padding = new Padding(12);
        SalaryEditor.RowCount = 4;
        SalaryEditor.RowStyles.Add(new RowStyle());
        SalaryEditor.RowStyles.Add(new RowStyle());
        SalaryEditor.RowStyles.Add(new RowStyle());
        SalaryEditor.RowStyles.Add(new RowStyle());
        SalaryEditor.Size = new Size(680, 227);
        SalaryEditor.TabIndex = 3;
        // 
        // SalaryDateLabel
        // 
        SalaryDateLabel.AutoSize = true;
        SalaryDateLabel.Dock = DockStyle.Fill;
        SalaryDateLabel.Location = new Point(16, 20);
        SalaryDateLabel.Margin = new Padding(4, 8, 4, 8);
        SalaryDateLabel.MaximumSize = new Size(660, 0);
        SalaryDateLabel.Name = "SalaryDateLabel";
        SalaryDateLabel.Size = new Size(274, 25);
        SalaryDateLabel.TabIndex = 0;
        SalaryDateLabel.Text = "Áp dụng từ ngày";
        SalaryDateLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // SalaryDate
        // 
        SalaryDate.AccessibleName = "Áp dụng từ ngày";
        SalaryDate.CustomFormat = "dd/MM/yyyy";
        SalaryDate.Dock = DockStyle.Top;
        SalaryDate.Format = DateTimePickerFormat.Custom;
        SalaryDate.Location = new Point(298, 20);
        SalaryDate.Margin = new Padding(4, 8, 4, 8);
        SalaryDate.Name = "SalaryDate";
        SalaryDate.Size = new Size(366, 25);
        SalaryDate.TabIndex = 1;
        // 
        // SalaryAmountLabel
        // 
        SalaryAmountLabel.AutoSize = true;
        SalaryAmountLabel.Dock = DockStyle.Fill;
        SalaryAmountLabel.Location = new Point(16, 61);
        SalaryAmountLabel.Margin = new Padding(4, 8, 4, 8);
        SalaryAmountLabel.MaximumSize = new Size(660, 0);
        SalaryAmountLabel.Name = "SalaryAmountLabel";
        SalaryAmountLabel.Size = new Size(274, 25);
        SalaryAmountLabel.TabIndex = 2;
        SalaryAmountLabel.Text = "Lương tháng (đồng)";
        SalaryAmountLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // SalaryAmount
        // 
        SalaryAmount.AccessibleName = "Lương tháng (đồng)";
        SalaryAmount.Dock = DockStyle.Top;
        SalaryAmount.Location = new Point(298, 61);
        SalaryAmount.Margin = new Padding(4, 8, 4, 8);
        SalaryAmount.Maximum = new decimal(new int[] { -727379968, 232, 0, 0 });
        SalaryAmount.Name = "SalaryAmount";
        SalaryAmount.Size = new Size(366, 25);
        SalaryAmount.TabIndex = 3;
        SalaryAmount.ThousandsSeparator = true;
        // 
        // SalaryReasonLabel
        // 
        SalaryReasonLabel.AutoSize = true;
        SalaryReasonLabel.Dock = DockStyle.Fill;
        SalaryReasonLabel.Location = new Point(16, 102);
        SalaryReasonLabel.Margin = new Padding(4, 8, 4, 8);
        SalaryReasonLabel.MaximumSize = new Size(660, 0);
        SalaryReasonLabel.Name = "SalaryReasonLabel";
        SalaryReasonLabel.Size = new Size(274, 70);
        SalaryReasonLabel.TabIndex = 4;
        SalaryReasonLabel.Text = "Căn cứ thay đổi *";
        SalaryReasonLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // SalaryReason
        // 
        SalaryReason.AccessibleName = "Căn cứ thay đổi *";
        SalaryReason.Dock = DockStyle.Top;
        SalaryReason.Location = new Point(298, 102);
        SalaryReason.Margin = new Padding(4, 8, 4, 8);
        SalaryReason.MaxLength = 500;
        SalaryReason.Multiline = true;
        SalaryReason.Name = "SalaryReason";
        SalaryReason.ScrollBars = ScrollBars.Vertical;
        SalaryReason.Size = new Size(366, 70);
        SalaryReason.TabIndex = 5;
        // 
        // SalaryHint
        // 
        SalaryHint.AutoSize = true;
        SalaryEditor.SetColumnSpan(SalaryHint, 2);
        SalaryHint.Dock = DockStyle.Top;
        SalaryHint.Location = new Point(16, 188);
        SalaryHint.Margin = new Padding(4, 8, 4, 8);
        SalaryHint.MaximumSize = new Size(660, 0);
        SalaryHint.Name = "SalaryHint";
        SalaryHint.Size = new Size(648, 19);
        SalaryHint.TabIndex = 7;
        SalaryHint.Text = "Mức lương từ ngày 1 đầu kỳ; mức đầu tiên có thể từ ngày vào làm.";
        // 
        // _itemPage
        // 
        _itemPage.AutoScroll = true;
        _itemPage.Controls.Add(_itemPageLayout);
        _itemPage.Location = new Point(4, 26);
        _itemPage.Name = "_itemPage";
        _itemPage.Size = new Size(712, 497);
        _itemPage.TabIndex = 3;
        _itemPage.Text = "Cộng / trừ";
        _itemPage.UseVisualStyleBackColor = true;
        // 
        // _itemPageLayout
        // 
        _itemPageLayout.AutoSize = true;
        _itemPageLayout.ColumnCount = 2;
        _itemPageLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 43F));
        _itemPageLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 57F));
        _itemPageLayout.Controls.Add(ItemKindLabel, 0, 0);
        _itemPageLayout.Controls.Add(ItemKind, 1, 0);
        _itemPageLayout.Controls.Add(ItemAmountLabel, 0, 1);
        _itemPageLayout.Controls.Add(ItemAmount, 1, 1);
        _itemPageLayout.Controls.Add(ItemProrateLabel, 0, 2);
        _itemPageLayout.Controls.Add(ItemProrate, 1, 2);
        _itemPageLayout.Controls.Add(ItemReasonLabel, 0, 3);
        _itemPageLayout.Controls.Add(ItemReason, 1, 3);
        _itemPageLayout.Controls.Add(ItemHint, 0, 4);
        _itemPageLayout.Dock = DockStyle.Top;
        _itemPageLayout.Location = new Point(0, 0);
        _itemPageLayout.Name = "_itemPageLayout";
        _itemPageLayout.Padding = new Padding(12);
        _itemPageLayout.RowCount = 5;
        _itemPageLayout.RowStyles.Add(new RowStyle());
        _itemPageLayout.RowStyles.Add(new RowStyle());
        _itemPageLayout.RowStyles.Add(new RowStyle());
        _itemPageLayout.RowStyles.Add(new RowStyle());
        _itemPageLayout.RowStyles.Add(new RowStyle());
        _itemPageLayout.Size = new Size(712, 266);
        _itemPageLayout.TabIndex = 0;
        // 
        // ItemKindLabel
        // 
        ItemKindLabel.AutoSize = true;
        ItemKindLabel.Dock = DockStyle.Fill;
        ItemKindLabel.Location = new Point(16, 20);
        ItemKindLabel.Margin = new Padding(4, 8, 4, 8);
        ItemKindLabel.MaximumSize = new Size(660, 0);
        ItemKindLabel.Name = "ItemKindLabel";
        ItemKindLabel.Size = new Size(287, 25);
        ItemKindLabel.TabIndex = 0;
        ItemKindLabel.Text = "Loại khoản";
        ItemKindLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // ItemKind
        // 
        ItemKind.AccessibleName = "Loại khoản";
        ItemKind.Dock = DockStyle.Top;
        ItemKind.DropDownStyle = ComboBoxStyle.DropDownList;
        ItemKind.Location = new Point(311, 20);
        ItemKind.Margin = new Padding(4, 8, 4, 8);
        ItemKind.Name = "ItemKind";
        ItemKind.Size = new Size(385, 25);
        ItemKind.TabIndex = 1;
        // 
        // ItemAmountLabel
        // 
        ItemAmountLabel.AutoSize = true;
        ItemAmountLabel.Dock = DockStyle.Fill;
        ItemAmountLabel.Location = new Point(16, 61);
        ItemAmountLabel.Margin = new Padding(4, 8, 4, 8);
        ItemAmountLabel.MaximumSize = new Size(660, 0);
        ItemAmountLabel.Name = "ItemAmountLabel";
        ItemAmountLabel.Size = new Size(287, 25);
        ItemAmountLabel.TabIndex = 2;
        ItemAmountLabel.Text = "Số tiền (đồng) *";
        ItemAmountLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // ItemAmount
        // 
        ItemAmount.AccessibleName = "Số tiền (đồng) *";
        ItemAmount.Dock = DockStyle.Top;
        ItemAmount.Location = new Point(311, 61);
        ItemAmount.Margin = new Padding(4, 8, 4, 8);
        ItemAmount.Maximum = new decimal(new int[] { -727379968, 232, 0, 0 });
        ItemAmount.Name = "ItemAmount";
        ItemAmount.Size = new Size(385, 25);
        ItemAmount.TabIndex = 3;
        ItemAmount.ThousandsSeparator = true;
        // 
        // ItemProrateLabel
        // 
        ItemProrateLabel.AutoSize = true;
        ItemProrateLabel.Dock = DockStyle.Fill;
        ItemProrateLabel.Location = new Point(16, 102);
        ItemProrateLabel.Margin = new Padding(4, 8, 4, 8);
        ItemProrateLabel.MaximumSize = new Size(660, 0);
        ItemProrateLabel.Name = "ItemProrateLabel";
        ItemProrateLabel.Size = new Size(287, 23);
        ItemProrateLabel.TabIndex = 4;
        ItemProrateLabel.Text = "Phụ cấp theo công";
        ItemProrateLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // ItemProrate
        // 
        ItemProrate.AccessibleName = "Phụ cấp theo công";
        ItemProrate.AutoSize = true;
        ItemProrate.Dock = DockStyle.Top;
        ItemProrate.Location = new Point(311, 102);
        ItemProrate.Margin = new Padding(4, 8, 4, 8);
        ItemProrate.Name = "ItemProrate";
        ItemProrate.Size = new Size(385, 23);
        ItemProrate.TabIndex = 5;
        ItemProrate.Text = "Nhân công hưởng / công chuẩn";
        // 
        // ItemReasonLabel
        // 
        ItemReasonLabel.AutoSize = true;
        ItemReasonLabel.Dock = DockStyle.Fill;
        ItemReasonLabel.Location = new Point(16, 141);
        ItemReasonLabel.Margin = new Padding(4, 8, 4, 8);
        ItemReasonLabel.MaximumSize = new Size(660, 0);
        ItemReasonLabel.Name = "ItemReasonLabel";
        ItemReasonLabel.Size = new Size(287, 70);
        ItemReasonLabel.TabIndex = 6;
        ItemReasonLabel.Text = "Căn cứ / lý do *";
        ItemReasonLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // ItemReason
        // 
        ItemReason.AccessibleName = "Căn cứ / lý do *";
        ItemReason.Dock = DockStyle.Top;
        ItemReason.Location = new Point(311, 141);
        ItemReason.Margin = new Padding(4, 8, 4, 8);
        ItemReason.MaxLength = 500;
        ItemReason.Multiline = true;
        ItemReason.Name = "ItemReason";
        ItemReason.ScrollBars = ScrollBars.Vertical;
        ItemReason.Size = new Size(385, 70);
        ItemReason.TabIndex = 7;
        // 
        // ItemHint
        // 
        ItemHint.AutoSize = true;
        _itemPageLayout.SetColumnSpan(ItemHint, 2);
        ItemHint.Dock = DockStyle.Top;
        ItemHint.Location = new Point(16, 227);
        ItemHint.Margin = new Padding(4, 8, 4, 8);
        ItemHint.MaximumSize = new Size(660, 0);
        ItemHint.Name = "ItemHint";
        ItemHint.Size = new Size(660, 19);
        ItemHint.TabIndex = 9;
        ItemHint.Text = "Tiền tăng ca, thuế/bảo hiểm do kế toán xác định. Không nhập khoản ứng/trả vào khấu trừ.";
        // 
        // _paymentPage
        // 
        _paymentPage.AutoScroll = true;
        _paymentPage.Controls.Add(_paymentPageLayout);
        _paymentPage.Location = new Point(4, 26);
        _paymentPage.Name = "_paymentPage";
        _paymentPage.Size = new Size(712, 497);
        _paymentPage.TabIndex = 5;
        _paymentPage.Text = "Chi trả";
        _paymentPage.UseVisualStyleBackColor = true;
        // 
        // _paymentPageLayout
        // 
        _paymentPageLayout.AutoSize = true;
        _paymentPageLayout.ColumnCount = 2;
        _paymentPageLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 43F));
        _paymentPageLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 57F));
        _paymentPageLayout.Controls.Add(PaymentBalance, 0, 0);
        _paymentPageLayout.Controls.Add(PaymentAmountLabel, 0, 1);
        _paymentPageLayout.Controls.Add(PaymentAmount, 1, 1);
        _paymentPageLayout.Controls.Add(PaymentDateLabel, 0, 2);
        _paymentPageLayout.Controls.Add(PaymentDate, 1, 2);
        _paymentPageLayout.Controls.Add(PaymentMethodLabel, 0, 3);
        _paymentPageLayout.Controls.Add(PaymentMethod, 1, 3);
        _paymentPageLayout.Controls.Add(PaymentReferenceLabel, 0, 4);
        _paymentPageLayout.Controls.Add(PaymentReference, 1, 4);
        _paymentPageLayout.Controls.Add(PaymentHint, 0, 5);
        _paymentPageLayout.Dock = DockStyle.Top;
        _paymentPageLayout.Location = new Point(0, 0);
        _paymentPageLayout.Name = "_paymentPageLayout";
        _paymentPageLayout.Padding = new Padding(12);
        _paymentPageLayout.RowCount = 6;
        _paymentPageLayout.RowStyles.Add(new RowStyle());
        _paymentPageLayout.RowStyles.Add(new RowStyle());
        _paymentPageLayout.RowStyles.Add(new RowStyle());
        _paymentPageLayout.RowStyles.Add(new RowStyle());
        _paymentPageLayout.RowStyles.Add(new RowStyle());
        _paymentPageLayout.RowStyles.Add(new RowStyle());
        _paymentPageLayout.Size = new Size(712, 277);
        _paymentPageLayout.TabIndex = 0;
        // 
        // PaymentBalance
        // 
        PaymentBalance.AutoSize = true;
        _paymentPageLayout.SetColumnSpan(PaymentBalance, 2);
        PaymentBalance.Dock = DockStyle.Top;
        PaymentBalance.Location = new Point(16, 20);
        PaymentBalance.Margin = new Padding(4, 8, 4, 8);
        PaymentBalance.MaximumSize = new Size(660, 0);
        PaymentBalance.Name = "PaymentBalance";
        PaymentBalance.Size = new Size(660, 19);
        PaymentBalance.TabIndex = 1;
        PaymentBalance.Text = "Còn phải trả: … đ";
        // 
        // PaymentAmountLabel
        // 
        PaymentAmountLabel.AutoSize = true;
        PaymentAmountLabel.Dock = DockStyle.Fill;
        PaymentAmountLabel.Location = new Point(16, 55);
        PaymentAmountLabel.Margin = new Padding(4, 8, 4, 8);
        PaymentAmountLabel.MaximumSize = new Size(660, 0);
        PaymentAmountLabel.Name = "PaymentAmountLabel";
        PaymentAmountLabel.Size = new Size(287, 25);
        PaymentAmountLabel.TabIndex = 2;
        PaymentAmountLabel.Text = "Số tiền đã chi *";
        PaymentAmountLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // PaymentAmount
        // 
        PaymentAmount.AccessibleName = "Số tiền đã chi *";
        PaymentAmount.Dock = DockStyle.Top;
        PaymentAmount.Location = new Point(311, 55);
        PaymentAmount.Margin = new Padding(4, 8, 4, 8);
        PaymentAmount.Maximum = new decimal(new int[] { -727379968, 232, 0, 0 });
        PaymentAmount.Name = "PaymentAmount";
        PaymentAmount.Size = new Size(385, 25);
        PaymentAmount.TabIndex = 3;
        PaymentAmount.ThousandsSeparator = true;
        // 
        // PaymentDateLabel
        // 
        PaymentDateLabel.AutoSize = true;
        PaymentDateLabel.Dock = DockStyle.Fill;
        PaymentDateLabel.Location = new Point(16, 96);
        PaymentDateLabel.Margin = new Padding(4, 8, 4, 8);
        PaymentDateLabel.MaximumSize = new Size(660, 0);
        PaymentDateLabel.Name = "PaymentDateLabel";
        PaymentDateLabel.Size = new Size(287, 25);
        PaymentDateLabel.TabIndex = 4;
        PaymentDateLabel.Text = "Ngày chi thực tế";
        PaymentDateLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // PaymentDate
        // 
        PaymentDate.AccessibleName = "Ngày chi thực tế";
        PaymentDate.CustomFormat = "dd/MM/yyyy";
        PaymentDate.Dock = DockStyle.Top;
        PaymentDate.Format = DateTimePickerFormat.Custom;
        PaymentDate.Location = new Point(311, 96);
        PaymentDate.Margin = new Padding(4, 8, 4, 8);
        PaymentDate.Name = "PaymentDate";
        PaymentDate.Size = new Size(385, 25);
        PaymentDate.TabIndex = 5;
        // 
        // PaymentMethodLabel
        // 
        PaymentMethodLabel.AutoSize = true;
        PaymentMethodLabel.Dock = DockStyle.Fill;
        PaymentMethodLabel.Location = new Point(16, 137);
        PaymentMethodLabel.Margin = new Padding(4, 8, 4, 8);
        PaymentMethodLabel.MaximumSize = new Size(660, 0);
        PaymentMethodLabel.Name = "PaymentMethodLabel";
        PaymentMethodLabel.Size = new Size(287, 25);
        PaymentMethodLabel.TabIndex = 6;
        PaymentMethodLabel.Text = "Hình thức";
        PaymentMethodLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // PaymentMethod
        // 
        PaymentMethod.AccessibleName = "Hình thức";
        PaymentMethod.Dock = DockStyle.Top;
        PaymentMethod.DropDownStyle = ComboBoxStyle.DropDownList;
        PaymentMethod.Location = new Point(311, 137);
        PaymentMethod.Margin = new Padding(4, 8, 4, 8);
        PaymentMethod.Name = "PaymentMethod";
        PaymentMethod.Size = new Size(385, 25);
        PaymentMethod.TabIndex = 7;
        // 
        // PaymentReferenceLabel
        // 
        PaymentReferenceLabel.AutoSize = true;
        PaymentReferenceLabel.Dock = DockStyle.Fill;
        PaymentReferenceLabel.Location = new Point(16, 178);
        PaymentReferenceLabel.Margin = new Padding(4, 8, 4, 8);
        PaymentReferenceLabel.MaximumSize = new Size(660, 0);
        PaymentReferenceLabel.Name = "PaymentReferenceLabel";
        PaymentReferenceLabel.Size = new Size(287, 25);
        PaymentReferenceLabel.TabIndex = 8;
        PaymentReferenceLabel.Text = "Mã chứng từ / giao dịch *";
        PaymentReferenceLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // PaymentReference
        // 
        PaymentReference.AccessibleName = "Mã chứng từ / giao dịch *";
        PaymentReference.Dock = DockStyle.Top;
        PaymentReference.Location = new Point(311, 178);
        PaymentReference.Margin = new Padding(4, 8, 4, 8);
        PaymentReference.MaxLength = 100;
        PaymentReference.Name = "PaymentReference";
        PaymentReference.Size = new Size(385, 25);
        PaymentReference.TabIndex = 9;
        // 
        // PaymentHint
        // 
        PaymentHint.AutoSize = true;
        _paymentPageLayout.SetColumnSpan(PaymentHint, 2);
        PaymentHint.Dock = DockStyle.Top;
        PaymentHint.Location = new Point(16, 219);
        PaymentHint.Margin = new Padding(4, 8, 4, 8);
        PaymentHint.MaximumSize = new Size(660, 0);
        PaymentHint.Name = "PaymentHint";
        PaymentHint.Size = new Size(660, 38);
        PaymentHint.TabIndex = 11;
        PaymentHint.Text = "Mỗi mã chứng từ chỉ được ghi một lần. Chỉ ghi khoản đã chi thực tế, kể cả tạm ứng phân bổ vào phiếu này.";
        // 
        // _detailsPage
        // 
        _detailsPage.AutoScroll = true;
        _detailsPage.Controls.Add(_detailsPageLayout);
        _detailsPage.Location = new Point(4, 26);
        _detailsPage.Name = "_detailsPage";
        _detailsPage.Size = new Size(712, 497);
        _detailsPage.TabIndex = 6;
        _detailsPage.Text = "Chi tiết phiếu";
        _detailsPage.UseVisualStyleBackColor = true;
        // 
        // _detailsPageLayout
        // 
        _detailsPageLayout.AutoSize = true;
        _detailsPageLayout.ColumnCount = 2;
        _detailsPageLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 43F));
        _detailsPageLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 57F));
        _detailsPageLayout.Controls.Add(DetailPeriod, 0, 0);
        _detailsPageLayout.Controls.Add(DetailSummary, 0, 1);
        _detailsPageLayout.Controls.Add(DetailItems, 0, 2);
        _detailsPageLayout.Controls.Add(DetailPayments, 0, 3);
        _detailsPageLayout.Controls.Add(DetailDriverDates, 0, 4);
        _detailsPageLayout.Dock = DockStyle.Top;
        _detailsPageLayout.Location = new Point(0, 0);
        _detailsPageLayout.Name = "_detailsPageLayout";
        _detailsPageLayout.Padding = new Padding(12);
        _detailsPageLayout.RowCount = 5;
        _detailsPageLayout.RowStyles.Add(new RowStyle());
        _detailsPageLayout.RowStyles.Add(new RowStyle());
        _detailsPageLayout.RowStyles.Add(new RowStyle());
        _detailsPageLayout.RowStyles.Add(new RowStyle());
        _detailsPageLayout.RowStyles.Add(new RowStyle());
        _detailsPageLayout.Size = new Size(695, 718);
        _detailsPageLayout.TabIndex = 0;
        // 
        // DetailPeriod
        // 
        DetailPeriod.AutoSize = true;
        _detailsPageLayout.SetColumnSpan(DetailPeriod, 2);
        DetailPeriod.Dock = DockStyle.Top;
        DetailPeriod.Location = new Point(16, 20);
        DetailPeriod.Margin = new Padding(4, 8, 4, 8);
        DetailPeriod.MaximumSize = new Size(660, 0);
        DetailPeriod.Name = "DetailPeriod";
        DetailPeriod.Size = new Size(660, 19);
        DetailPeriod.TabIndex = 1;
        DetailPeriod.Text = "Khoảng công đã dùng tính lương";
        // 
        // DetailSummary
        // 
        _detailsPageLayout.SetColumnSpan(DetailSummary, 2);
        DetailSummary.Dock = DockStyle.Top;
        DetailSummary.Location = new Point(16, 55);
        DetailSummary.Margin = new Padding(4, 8, 4, 8);
        DetailSummary.Multiline = true;
        DetailSummary.Name = "DetailSummary";
        DetailSummary.ReadOnly = true;
        DetailSummary.ScrollBars = ScrollBars.Vertical;
        DetailSummary.Size = new Size(663, 150);
        DetailSummary.TabIndex = 3;
        // 
        // DetailItems
        // 
        DetailItems.AccessibleName = "DetailItems";
        DetailItems.AllowUserToAddRows = false;
        DetailItems.AllowUserToDeleteRows = false;
        DetailItems.BackgroundColor = Color.White;
        _detailsPageLayout.SetColumnSpan(DetailItems, 2);
        DetailItems.Dock = DockStyle.Top;
        DetailItems.Location = new Point(16, 221);
        DetailItems.Margin = new Padding(4, 8, 4, 8);
        DetailItems.MultiSelect = false;
        DetailItems.Name = "DetailItems";
        DetailItems.ReadOnly = true;
        DetailItems.RowHeadersVisible = false;
        DetailItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        DetailItems.Size = new Size(663, 180);
        DetailItems.TabIndex = 5;
        // 
        // DetailPayments
        // 
        DetailPayments.AccessibleName = "DetailPayments";
        DetailPayments.AllowUserToAddRows = false;
        DetailPayments.AllowUserToDeleteRows = false;
        DetailPayments.BackgroundColor = Color.White;
        _detailsPageLayout.SetColumnSpan(DetailPayments, 2);
        DetailPayments.Dock = DockStyle.Top;
        DetailPayments.Location = new Point(16, 417);
        DetailPayments.Margin = new Padding(4, 8, 4, 8);
        DetailPayments.MultiSelect = false;
        DetailPayments.Name = "DetailPayments";
        DetailPayments.ReadOnly = true;
        DetailPayments.RowHeadersVisible = false;
        DetailPayments.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        DetailPayments.Size = new Size(663, 180);
        DetailPayments.TabIndex = 7;
        // 
        // DetailDriverDates
        // 
        DetailDriverDates.AccessibleName = "Ngày công tài xế đã lưu";
        _detailsPageLayout.SetColumnSpan(DetailDriverDates, 2);
        DetailDriverDates.Dock = DockStyle.Top;
        DetailDriverDates.Location = new Point(16, 613);
        DetailDriverDates.Margin = new Padding(4, 8, 4, 8);
        DetailDriverDates.Multiline = true;
        DetailDriverDates.Name = "DetailDriverDates";
        DetailDriverDates.ReadOnly = true;
        DetailDriverDates.ScrollBars = ScrollBars.Vertical;
        DetailDriverDates.Size = new Size(663, 85);
        DetailDriverDates.TabIndex = 9;
        // 
        // _footer
        // 
        _footer.AutoSize = true;
        _footer.ColumnCount = 1;
        _footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _footer.Controls.Add(_error, 0, 0);
        _footer.Controls.Add(_buttons, 0, 1);
        _footer.Dock = DockStyle.Bottom;
        _footer.Location = new Point(0, 527);
        _footer.Name = "_footer";
        _footer.RowCount = 2;
        _footer.RowStyles.Add(new RowStyle());
        _footer.RowStyles.Add(new RowStyle());
        _footer.Size = new Size(720, 93);
        _footer.TabIndex = 1;
        // 
        // _error
        // 
        _error.AccessibleRole = AccessibleRole.Alert;
        _error.AutoSize = true;
        _error.Dock = DockStyle.Top;
        _error.ForeColor = Color.FromArgb(220, 38, 38);
        _error.Location = new Point(3, 0);
        _error.MaximumSize = new Size(680, 0);
        _error.Name = "_error";
        _error.Size = new Size(680, 19);
        _error.TabIndex = 0;
        _error.Visible = false;
        // 
        // _buttons
        // 
        _buttons.AutoSize = true;
        _buttons.Controls.Add(_cancel);
        _buttons.Controls.Add(_save);
        _buttons.Dock = DockStyle.Fill;
        _buttons.FlowDirection = FlowDirection.RightToLeft;
        _buttons.Location = new Point(3, 22);
        _buttons.Name = "_buttons";
        _buttons.Padding = new Padding(12);
        _buttons.Size = new Size(714, 68);
        _buttons.TabIndex = 1;
        // 
        // _cancel
        // 
        _cancel.BackColor = Color.White;
        _cancel.Cursor = Cursors.Hand;
        _cancel.DialogResult = DialogResult.Cancel;
        _cancel.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
        _cancel.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252);
        _cancel.FlatStyle = FlatStyle.Flat;
        _cancel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _cancel.ForeColor = Color.FromArgb(15, 23, 42);
        _cancel.Location = new Point(567, 15);
        _cancel.Name = "_cancel";
        _cancel.Size = new Size(120, 38);
        _cancel.TabIndex = 1;
        _cancel.Text = "Đóng";
        _cancel.UseVisualStyleBackColor = false;
        // 
        // _save
        // 
        _save.BackColor = Color.FromArgb(37, 99, 235);
        _save.Cursor = Cursors.Hand;
        _save.FlatAppearance.BorderSize = 0;
        _save.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
        _save.FlatStyle = FlatStyle.Flat;
        _save.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _save.ForeColor = Color.White;
        _save.Location = new Point(441, 15);
        _save.Name = "_save";
        _save.Size = new Size(120, 38);
        _save.TabIndex = 0;
        _save.Text = "Lưu";
        _save.UseVisualStyleBackColor = false;
        // 
        // PayrollInputForm
        // 
        AcceptButton = _save;
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        CancelButton = _cancel;
        ClientSize = new Size(720, 620);
        Controls.Add(_sections);
        Controls.Add(_footer);
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "PayrollInputForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Nhập dữ liệu lương";
        _sections.ResumeLayout(false);
        _salaryPage.ResumeLayout(false);
        _salaryPage.PerformLayout();
        _salaryPageLayout.ResumeLayout(false);
        _salaryPageLayout.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)SalaryHistory).EndInit();
        SalaryEditor.ResumeLayout(false);
        SalaryEditor.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)SalaryAmount).EndInit();
        _itemPage.ResumeLayout(false);
        _itemPage.PerformLayout();
        _itemPageLayout.ResumeLayout(false);
        _itemPageLayout.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)ItemAmount).EndInit();
        _paymentPage.ResumeLayout(false);
        _paymentPage.PerformLayout();
        _paymentPageLayout.ResumeLayout(false);
        _paymentPageLayout.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)PaymentAmount).EndInit();
        _detailsPage.ResumeLayout(false);
        _detailsPage.PerformLayout();
        _detailsPageLayout.ResumeLayout(false);
        _detailsPageLayout.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)DetailItems).EndInit();
        ((System.ComponentModel.ISupportInitialize)DetailPayments).EndInit();
        _footer.ResumeLayout(false);
        _footer.PerformLayout();
        _buttons.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private TabControl _sections = null!;
    internal DateTimePicker SalaryDate = null!;
    internal NumericUpDown SalaryAmount = null!;
    internal TextBox SalaryReason = null!;
    internal Label SalaryHint = null!;
    internal TableLayoutPanel SalaryEditor = null!;
    internal Label SalaryDateLabel = null!;
    internal Label SalaryAmountLabel = null!;
    internal Label SalaryReasonLabel = null!;
    internal DataGridView SalaryHistory = null!;
    private TabPage _salaryPage = null!;
    private TableLayoutPanel _salaryPageLayout = null!;
    internal CheckBox ItemProrate = null!;
    internal ComboBox ItemKind = null!;
    internal NumericUpDown ItemAmount = null!;
    internal TextBox ItemReason = null!;
    internal Label ItemHint = null!;
    private TabPage _itemPage = null!;
    private TableLayoutPanel _itemPageLayout = null!;
    internal Label ItemKindLabel = null!;
    internal Label ItemAmountLabel = null!;
    internal Label ItemProrateLabel = null!;
    internal Label ItemReasonLabel = null!;
    internal TextBox PaymentReference = null!;
    internal Label PaymentBalance = null!;
    internal NumericUpDown PaymentAmount = null!;
    internal DateTimePicker PaymentDate = null!;
    internal ComboBox PaymentMethod = null!;
    internal Label PaymentHint = null!;
    private TabPage _paymentPage = null!;
    private TableLayoutPanel _paymentPageLayout = null!;
    internal Label PaymentAmountLabel = null!;
    internal Label PaymentDateLabel = null!;
    internal Label PaymentMethodLabel = null!;
    internal Label PaymentReferenceLabel = null!;
    internal Label DetailPeriod = null!;
    internal TextBox DetailSummary = null!;
    internal DataGridView DetailItems = null!;
    internal DataGridView DetailPayments = null!;
    internal TextBox DetailDriverDates = null!;
    private TabPage _detailsPage = null!;
    private TableLayoutPanel _detailsPageLayout = null!;
    private TableLayoutPanel _footer = null!;
    private FlowLayoutPanel _buttons = null!;
    private Button _save = null!;
    private Button _cancel = null!;
    private Label _error = null!;
}
