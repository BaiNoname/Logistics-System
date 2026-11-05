using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.SERVICE_Auth;

namespace TruckingSystem.Desktop.GUI_AccountManagement;

public class AccountEditorForm : BaseForm
{
    private readonly TextBox _username = new();
    private readonly TextBox _email = new();
    private readonly TextBox _password = new() { UseSystemPasswordChar = true };
    private readonly ComboBox _employee = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckedListBox _roles = new() { CheckOnClick = true, BorderStyle = BorderStyle.FixedSingle };
    private readonly CheckBox _active = new() { Text = "Tài khoản hoạt động", AutoSize = true, Checked = true };
    private bool _saving;

    public string Username => _username.Text.Trim();
    public string? Email => string.IsNullOrWhiteSpace(_email.Text) ? null : _email.Text.Trim();
    public string Password => _password.Text;
    public bool IsActive => _active.Checked;
    public string[] Roles => _roles.CheckedItems.Cast<string>().ToArray();
    public int? EmployeeId => _employee.SelectedItem is AccountEmployeeOption { Id: > 0 } employee ? employee.Id : null;

    public AccountEditorForm(List<string> roles, AccountItem? account, List<AccountEmployeeOption>? employees = null,
        Func<AccountEditorForm, Task>? saveAccount = null)
    {
        Text = account is null ? "Thêm tài khoản" : "Sửa tài khoản và phân quyền";
        ClientSize = new Size(520, account is null ? 700 : 630);
        MinimumSize = new Size(440, 400);
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Theme.CardBg;
        var fields = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, Padding = new Padding(20, 16, 20, 16) };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        void Section(string text)
        {
            var label = new Label { Text = text, AutoSize = true, Margin = new Padding(3, 10, 3, 6) };
            FormStyle.Section(label);
            fields.Controls.Add(label);
        }
        void Field(string text, Control input)
        {
            fields.Controls.Add(new Label { Text = text, AutoSize = true, ForeColor = Theme.TextPrimary, Margin = new Padding(3, 6, 3, 4) });
            input.Dock = DockStyle.Top;
            input.Margin = new Padding(3, 0, 12, 6);
            input.AccessibleName = text.TrimEnd(':');
            input.TabIndex = fields.Controls.Count;
            fields.Controls.Add(input);
        }
        Section("① Thông tin đăng nhập");
        Field("Tên đăng nhập *:", _username);
        Field("Email:", _email);
        var options = new List<AccountEmployeeOption> { new(0, "", "") };
        options.AddRange(employees ?? new());
        if (account?.EmployeeId is int currentId && !options.Any(e => e.Id == currentId))
            options.Add(new(currentId, "", account.EmployeeName ?? $"Nhân viên #{currentId}"));
        _employee.DisplayMember = nameof(AccountEmployeeOption.DisplayName);
        _employee.ValueMember = nameof(AccountEmployeeOption.Id);
        _employee.DataSource = options;
        Load += (_, _) => _employee.SelectedValue = account?.EmployeeId ?? 0;
        Field("Gán nhân viên:", _employee);
        if (account is null) Field("Mật khẩu *:", _password);
        else
        {
            _username.Text = account.Username;
            _username.ReadOnly = true;
            _username.TabStop = false;
            _email.Text = account.Email ?? "";
            _active.Checked = account.IsActive;
        }
        Section("② Quyền truy cập");
        _roles.Height = 150;
        Field("Vai trò *:", _roles);
        foreach (var role in roles)
            _roles.Items.Add(role, account?.Roles.Contains(role, StringComparer.OrdinalIgnoreCase) == true);
        _active.TabIndex = fields.Controls.Count;
        if (account is not null) fields.Controls.Add(_active);
        var status = new Label { AutoSize = true, Dock = DockStyle.Fill, ForeColor = Theme.Danger };
        _roles.ItemCheck += (_, _) => { FieldHint.Clear(_roles); status.Text = ""; };
        fields.Controls.Add(status);
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        scroll.Controls.Add(fields);
        Controls.Add(scroll);
        var save = Theme.MakePrimaryButton("Lưu tài khoản", 150);
        FormStyle.EditorFooter(this, FormStyle.NewCloseButton(this), save);
        DialogKeys.Enable(this, save);
        Anim.FadeIn(this);
        FormClosing += (_, e) => e.Cancel = _saving;
        save.Click += async (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                FieldHint.Show(_username, status, "Vui lòng nhập tên đăng nhập.");
                return;
            }
            if (account is null && string.IsNullOrWhiteSpace(Password))
            {
                FieldHint.Show(_password, status, "Vui lòng nhập mật khẩu.");
                return;
            }
            if (Roles.Length == 0)
            {
                FieldHint.Show(_roles, status, "Vui lòng chọn ít nhất một vai trò.");
                return;
            }
            _saving = true;
            fields.Enabled = save.Enabled = false;
            try
            {
                if (saveAccount is not null) await saveAccount(this);
                _saving = false;
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                status.Text = ex.Message;
            }
            finally { _saving = false; fields.Enabled = save.Enabled = true; }
        };

        AcceptButton = save;
    }

}
