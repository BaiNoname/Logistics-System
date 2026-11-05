using System.Text.RegularExpressions;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_CustomerManagement;
namespace TruckingSystem.Desktop.GUI_CustomerManagement
{
    /// <summary>Logic xử lý — giao diện nằm ở CreateCustomerForm.Designer.cs.</summary>
    public partial class CreateCustomerForm : BaseForm
    {
        private ApiServices? _api;
        private CancellationTokenSource? _cardScanCancellation;
        private bool _saving;
        private bool _contactsOpen;
        private readonly List<(TableLayoutPanel Panel, TextBox Name, TextBox Phone, TextBox Position)> _contactInputs = new();

        public CreateCustomerForm()
        {
            InitializeComponent();
            DialogFit.Enable(this, root);
            DialogKeys.Enable(this);
            Anim.FadeIn(this);
            btnContacts.Click += (_, _) =>
            {
                _contactsOpen = !_contactsOpen;
                UpdateContactSection();
            };
            numContacts.ValueChanged += (_, _) => UpdateContactSection();
            FormClosing += (_, e) => e.Cancel = _saving;
            // Hiện tiếng Việt (khớp danh sách khách hàng), gửi đúng mã cho server.
            cboType.Items.AddRange(new object[]
            {
                new ComboOption("Individual", "Cá nhân"),
                new ComboOption("Company", "Doanh nghiệp"),
            });
            cboType.SelectedIndex = 0;
            ApplyDesign();
            _ = new AddressSuggestBinder(txtAddress, () => _api); // gợi ý địa chỉ VietMap khi gõ
        }

        /// <summary>Quy tắc design.md: nhãn bắt buộc có *, tiêu đề khối màu Primary, chân
        /// [Quét danh thiếp] [Người liên hệ] [Đóng] [💾 Lưu khách hàng] — chỉ 1 nút chính.</summary>
        private void ApplyDesign()
        {
            lblPhone.Text = "Số điện thoại *:";
            lblType.Text = "Loại khách hàng *:";
            FormStyle.Section(lblFieldsTitle, lblScanTitle, lblContactsTitle);
            lblFieldsTitle.Text = "① Thông tin khách hàng";
            lblScanTitle.Text = "② Đối chiếu danh thiếp";
            lblContactsTitle.Text = "③ Người liên hệ";
            txtAddress.AcceptsReturn = true; // Enter trong ô địa chỉ là xuống dòng, không bấm Lưu
            FormStyle.NoTab(txtCardDetails);

            var close = FormStyle.NewCloseButton(this);
            close.Margin = new Padding(8, 0, 0, 0);
            buttonBar.SuspendLayout();
            buttonBar.Controls.Clear(); // xếp phải -> trái
            buttonBar.Controls.AddRange(new Control[] { btnContacts, btnReadWithAi });
            FormStyle.EditorFooter(this, close, btnSave);
            buttonBar.ResumeLayout(true);
            FormStyle.SaveButton(btnSave);
            ButtonStyles.Secondary(btnContacts);
            ButtonStyles.Secondary(btnReadWithAi);
        }

        public CreateCustomerForm(ApiServices api) : this()
        {
            _api = api;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _cardScanCancellation?.Cancel();
            base.OnFormClosed(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cardScanCancellation?.Cancel();
                _cardScanCancellation = null;
                pictureCard.Image?.Dispose();
                pictureCard.Image = null;
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        private static (TableLayoutPanel Panel, TextBox Name, TextBox Phone, TextBox Position) CreateContactFields(int number)
        {
            var panel = new TableLayoutPanel
            {
                Name = $"contactGroup{number}",
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(8),
                Margin = new Padding(0, 0, 0, 8),
                BackColor = Theme.ContentBg
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (var row = 0; row < 4; row++) panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var heading = new Label { Text = $"Người liên hệ {number}", AutoSize = true, Font = Theme.FontBold, Margin = new Padding(3, 0, 3, 8) };
            panel.Controls.Add(heading, 0, 0);
            panel.SetColumnSpan(heading, 2);
            var name = new TextBox { Name = $"contactName{number}", AccessibleName = $"Họ tên người liên hệ {number}", Dock = DockStyle.Top, TabIndex = 0 };
            var phone = new TextBox { Name = $"contactPhone{number}", AccessibleName = $"Số điện thoại người liên hệ {number}", Dock = DockStyle.Top, TabIndex = 1 };
            var position = new TextBox { Name = $"contactPosition{number}", AccessibleName = $"Chức vụ người liên hệ {number}", Dock = DockStyle.Top, TabIndex = 2 };
            panel.Controls.Add(new Label { Text = "Họ tên *:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
            panel.Controls.Add(name, 1, 1);
            panel.Controls.Add(new Label { Text = "Điện thoại *:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 2);
            panel.Controls.Add(phone, 1, 2);
            panel.Controls.Add(new Label { Text = "Chức vụ", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 3);
            panel.Controls.Add(position, 1, 3);
            return (panel, name, phone, position);
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                FieldHint.Show(txtName, lblStatus, "Vui lòng nhập tên khách hàng.");
                return;
            }
            if (!IsValidPhoneNumber(txtPhone.Text))
            {
                FieldHint.Show(txtPhone, lblStatus, "Vui lòng nhập số điện thoại hợp lệ (ví dụ: 0901234567 hoặc +84 901234567).");
                return;
            }
            if (cboType.SelectedItem is not ComboOption customerType)
            {
                FieldHint.Show(cboType, lblStatus, "Vui lòng chọn loại khách hàng Cá nhân hoặc Doanh nghiệp.");
                return;
            }
            if (_api is null || !btnSave.Enabled) return;
            var contacts = ReadContacts();
            if (contacts is null) return;

            // Khóa nút Lưu trong lúc chờ server — bấm 2 lần trước đây tạo 2 khách hàng trùng.
            _saving = true;
            btnSave.Enabled = btnReadWithAi.Enabled = scrollFields.Enabled = btnContacts.Enabled = contactsSidebar.Enabled = false;
            try
            {
                var result = await _api.Customers.CreateCustomerAsync(
                    txtName.Text.Trim(), txtPhone.Text.Trim(), txtTaxCode.Text.Trim(), customerType.Value,
                    string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text.Trim(),
                    numCreditLimit.Value == 0 ? null : numCreditLimit.Value,
                    string.IsNullOrWhiteSpace(txtGoogleCalendarId.Text) ? null : txtGoogleCalendarId.Text.Trim(), contacts);
                if (IsDisposed || Disposing) return;

                if (!result.Success)
                {
                    FormStyle.Status(lblStatus, "⚠️ " + result.Message);
                    MessageBox.Show(this, result.Message, "Không thể lưu khách hàng", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _saving = false;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                if (IsDisposed || Disposing) return;
                FormStyle.Status(lblStatus, "Lỗi: " + ex.Message);
                MessageBox.Show(this, ex.Message, "Lỗi lưu khách hàng", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _saving = false;
                if (!IsDisposed) btnSave.Enabled = btnReadWithAi.Enabled = scrollFields.Enabled = btnContacts.Enabled = contactsSidebar.Enabled = true;
            }
        }

        private void UpdateContactSection()
        {
            var count = (int)numContacts.Value;
            contactsList.SuspendLayout();
            while (_contactInputs.Count < count)
            {
                var input = CreateContactFields(_contactInputs.Count + 1);
                _contactInputs.Add(input);
                contactsList.RowCount = _contactInputs.Count;
                contactsList.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                contactsList.Controls.Add(input.Panel, 0, _contactInputs.Count - 1);
            }
            for (var i = 0; i < _contactInputs.Count; i++)
                _contactInputs[i].Panel.Visible = i < count;
            contactsList.ResumeLayout(true);
            contactsSidebar.Visible = _contactsOpen;
            scanPanel.Visible = !_contactsOpen;
            btnContacts.Text = $"{(_contactsOpen ? "Đóng người liên hệ" : "Người liên hệ")} ({count})";
        }

        private List<CustomerContactInput>? ReadContacts()
        {
            var contacts = new List<CustomerContactInput>();
            for (var i = 0; i < (int)numContacts.Value; i++)
            {
                var input = _contactInputs[i];
                var name = input.Name.Text.Trim();
                var phone = input.Phone.Text.Trim();
                if (name.Length == 0 || !CustomerContactForm.IsValidPhoneNumber(phone))
                {
                    _contactsOpen = true;
                    UpdateContactSection();
                    var target = name.Length == 0 ? input.Name : input.Phone;
                    contactsSidebar.ScrollControlIntoView(target);
                    FieldHint.Show(target, lblStatus, name.Length == 0 ? $"Vui lòng nhập họ tên người liên hệ {i + 1}."
                        : $"Số điện thoại người liên hệ {i + 1} không hợp lệ (ví dụ: 0901234567 hoặc +84 901234567).");
                    return null;
                }
                contacts.Add(new CustomerContactInput(name, phone,
                    string.IsNullOrWhiteSpace(input.Position.Text) ? null : input.Position.Text.Trim()));
            }
            return contacts;
        }

        private async void btnReadWithAi_Click(object? sender, EventArgs e)
        {
            if (_cardScanCancellation is not null) return;
            using var dialog = new OpenFileDialog
            {
                Title = "Chọn ảnh danh thiếp",
                Filter = "Ảnh danh thiếp|*.jpg;*.jpeg;*.png"
            };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            if (_api is null)
            {
                MessageBox.Show(this, "Không kết nối được dịch vụ khách hàng.", "Lỗi đọc danh thiếp", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using var scanCancellation = new CancellationTokenSource();
            _cardScanCancellation = scanCancellation;
            SetCardScanning(true);
            FormStyle.Status(lblStatus, "Đang đọc danh thiếp...", StatusKind.Info);
            txtCardDetails.Clear();
            try
            {
                LoadCardPreview(dialog.FileName);
                var info = await _api.Customers.ReadBusinessCardAsync(dialog.FileName, scanCancellation.Token);
                if (IsDisposed || Disposing) return;
                ApplyBusinessCardInfo(info);
            }
            catch (OperationCanceledException) when (scanCancellation.IsCancellationRequested) { }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
            {
                if (IsDisposed || Disposing) return;
                FormStyle.Status(lblStatus, "Ảnh không phải danh thiếp.");
                MessageBox.Show(this, ex.Message, "Không phải danh thiếp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                if (IsDisposed || Disposing) return;
                FormStyle.Status(lblStatus, "Không đọc được danh thiếp.");
                var message = ex is OperationCanceledException ? "Yêu cầu quét danh thiếp quá thời gian chờ. Vui lòng thử lại." : ex.Message;
                MessageBox.Show(this, message, "Lỗi đọc danh thiếp", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _cardScanCancellation = null;
                if (!IsDisposed && !Disposing) SetCardScanning(false);
            }
        }

        private void LoadCardPreview(string path)
        {
            using var file = File.OpenRead(path);
            if (file.Length == 0 || file.Length > 10 * 1024 * 1024)
                throw new ArgumentException("Vui lòng chọn ảnh danh thiếp không rỗng, tối đa 10 MB.");
            using var source = Image.FromStream(file, false, true);
            var preview = new Bitmap(source);
            pictureCard.Image?.Dispose();
            pictureCard.Image = preview;
        }

        private void ApplyBusinessCardInfo(CustomerBusinessCardInfoItem info)
        {
            txtCardDetails.Text = string.Join(Environment.NewLine, new[]
            {
                $"Họ và tên: {info.FullName}", $"Công ty: {info.Company}", $"Chức danh: {info.Title}",
                $"Email: {info.Email}", $"Website: {info.Website}", $"Loại khách AI đọc được: {info.CustomerType}"
            });
            if (!string.IsNullOrWhiteSpace(info.Name)) txtName.Text = info.Name;
            if (!string.IsNullOrWhiteSpace(info.Phone)) txtPhone.Text = info.Phone;
            if (!string.IsNullOrWhiteSpace(info.TaxCode)) txtTaxCode.Text = info.TaxCode;
            if (!string.IsNullOrWhiteSpace(info.Address)) txtAddress.Text = info.Address.ReplaceLineEndings(Environment.NewLine);
            var warning = info.ErrorMessage;
            if (!string.IsNullOrWhiteSpace(info.CustomerType))
            {
                cboType.SelectedItem = cboType.Items.Cast<ComboOption>()
                    .FirstOrDefault(option => option.Value.Equals(info.CustomerType, StringComparison.OrdinalIgnoreCase));
                if (cboType.SelectedItem is null)
                    warning = string.Join(" ", new[] { warning, "Vui lòng chọn loại khách hàng Cá nhân hoặc Doanh nghiệp." }.Where(value => !string.IsNullOrWhiteSpace(value)));
            }
            FormStyle.Status(lblStatus, warning ?? "Đã đọc xong. Vui lòng kiểm tra thông tin.",
                warning is null ? StatusKind.Success : StatusKind.Warning);
        }

        private void SetCardScanning(bool scanning)
        {
            foreach (var control in new Control[] { txtName, txtPhone, txtTaxCode, txtAddress, cboType, btnSave, btnReadWithAi })
                control.Enabled = !scanning;
            UseWaitCursor = scanning;
        }


        public static bool IsValidPhoneNumber(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            return Regex.IsMatch(
                phone,
                @"^(?:0[35789][0-9]{8}|\+[1-9][0-9]{0,2} [0-9]{4,14})$"
            );
        }
    }
}
