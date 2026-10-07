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
    private readonly CheckedListBox _roles = new() { CheckOnClick = true, BorderStyle = BorderStyle.FixedSingle };
    private readonly CheckBox _active = new() { Text = "Tài khoản hoạt động", AutoSize = true, Checked = true };

    public string Username => _username.Text.Trim();
    public string? Email => string.IsNullOrWhiteSpace(_email.Text) ? null : _email.Text.Trim();
    public string Password => _password.Text;
    public bool IsActive => _active.Checked;
    public string[] Roles => _roles.CheckedItems.Cast<string>().ToArray();

    public AccountEditorForm(List<string> roles, AccountItem? account)
    {
        Text = account is null ? "Thêm tài khoản" : "Sửa tài khoản và phân quyền";
        ClientSize = new Size(470, account is null ? 550 : 480);
        MinimumSize = Size;
        MaximumSize = Size;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.White;

        AddField("Tên đăng nhập *", _username, 22);
        AddField("Email", _email, 92);
        if (account is null) AddField("Mật khẩu *", _password, 162);
        else
        {
            _username.Text = account.Username;
            _username.ReadOnly = true;
            _email.Text = account.Email ?? "";
            _active.Checked = account.IsActive;
        }

        var roleTop = account is null ? 238 : 168;
        Controls.Add(new Label { Text = "Vai trò *", Left = 24, Top = roleTop, Width = 400, ForeColor = Theme.TextSecondary });
        _roles.SetBounds(24, roleTop + 24, 420, 150);
        foreach (var role in roles)
            _roles.Items.Add(role, account?.Roles.Contains(role, StringComparer.OrdinalIgnoreCase) == true);
        Controls.Add(_roles);

        _active.Left = 24;
        _active.Top = roleTop + 190;
        Controls.Add(_active);

        var save = Theme.MakePrimaryButton("Lưu tài khoản", 150);
        save.SetBounds(294, roleTop + 228, 150, 38);
        save.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(Username) || (account is null && string.IsNullOrWhiteSpace(Password)) || Roles.Length == 0)
            {
                MessageBox.Show(this, "Nhập tên đăng nhập, mật khẩu và chọn ít nhất một vai trò.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            DialogResult = DialogResult.OK;
        };
        Controls.Add(save);
        AcceptButton = save;
    }

    private void AddField(string label, TextBox input, int top)
    {
        Controls.Add(new Label { Text = label, Left = 24, Top = top, Width = 420, ForeColor = Theme.TextSecondary });
        input.SetBounds(24, top + 24, 420, 27);
        Controls.Add(input);
    }
}
