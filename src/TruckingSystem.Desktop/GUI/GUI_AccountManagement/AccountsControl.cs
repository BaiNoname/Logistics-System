using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.SERVICE_Auth;

namespace TruckingSystem.Desktop.GUI_AccountManagement;

public class AccountsControl : BaseUserControl
{
    private readonly ApiServices _api;
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill };
    private readonly Label _status = new() { AutoSize = true, ForeColor = Theme.TextSecondary, Dock = DockStyle.Bottom, Height = 28 };
    private readonly Button _edit = Theme.MakeSecondaryButton("Sửa / phân quyền", 150);
    private readonly Button _password = Theme.MakeSecondaryButton("Đặt lại mật khẩu", 155);
    private List<AccountItem> _accounts = new();
    private List<string> _roles = new();

    public AccountsControl(ApiServices api)
    {
        _api = api;
        Theme.StyleGrid(_grid);
        _grid.SelectionChanged += (_, _) => UpdateButtons();

        Padding = new Padding(16);
        var title = new Label { Text = "Quản lý tài khoản", Font = Theme.FontTitle, ForeColor = Theme.TextPrimary, AutoSize = true };
        var actions = new FlowLayoutPanel { AutoSize = true, WrapContents = true, Dock = DockStyle.Fill };
        var create = Theme.MakePrimaryButton("Thêm tài khoản", 145);
        var refresh = Theme.MakeSecondaryButton("Làm mới", 100);
        actions.Controls.AddRange(new Control[] { create, _edit, _password, refresh });
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(title, 0, 0);
        layout.Controls.Add(actions, 0, 1);
        layout.Controls.Add(_grid, 0, 2);
        layout.Controls.Add(_status, 0, 3);
        Controls.Add(layout);
        GridDecor.Pills(_grid, "Trạng_thái", (text, _) => text == "Hoạt động" ? GridDecor.Green : GridDecor.Slate);
        Load += (_, _) => Theme.UniformButtons(actions);
        create.Click += async (_, _) => await OpenEditorAsync(null);
        _edit.Click += async (_, _) => await OpenEditorAsync(SelectedAccount());
        _password.Click += async (_, _) => await ResetPasswordAsync();
        refresh.Click += async (_, _) => await LoadAsync();
        UpdateButtons();
        Load += async (_, _) => await LoadAsync();
    }

    private AccountItem? SelectedAccount()
    {
        if (_grid.CurrentRow?.Cells["Id"].Value is not string id) return null;
        return _accounts.FirstOrDefault(a => a.Id == id);
    }

    private void UpdateButtons() => _edit.Enabled = _password.Enabled = SelectedAccount() is not null;

    private async Task LoadAsync()
    {
        _status.ForeColor = Theme.TextSecondary;
        _status.Text = "Đang tải tài khoản...";
        try
        {
            _roles = await _api.Accounts.GetRolesAsync();
            _accounts = await _api.Accounts.GetAllAsync();
            _grid.DataSource = _accounts.Select(a => new
            {
                a.Id,
                Tên_đăng_nhập = a.Username,
                Email = a.Email ?? "",
                Vai_trò = string.Join(", ", a.Roles),
                Trạng_thái = a.IsActive ? "Hoạt động" : "Đã khóa",
            }).ToList();
            if (_grid.Columns["Id"] is not null) _grid.Columns["Id"]!.Visible = false;
            _status.Text = $"{_accounts.Count} tài khoản";
            UpdateButtons();
        }
        catch (Exception ex)
        {
            _status.ForeColor = Theme.Danger;
            _status.Text = "Không tải được tài khoản: " + ex.Message;
        }
    }

    private async Task OpenEditorAsync(AccountItem? account)
    {
        if (_roles.Count == 0)
        {
            MessageBox.Show(FindForm(), "Không có vai trò để phân quyền.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        using var dialog = new AccountEditorForm(_roles, account);
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;

        try
        {
            if (account is null)
                await _api.Accounts.CreateAsync(dialog.Username, dialog.Email, dialog.Password, dialog.IsActive, dialog.Roles);
            else
                await _api.Accounts.UpdateAsync(account.Id, dialog.Email, dialog.IsActive, dialog.Roles);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(FindForm(), ex.Message, "Không lưu được tài khoản", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task ResetPasswordAsync()
    {
        var account = SelectedAccount();
        if (account is null) return;

        using var dialog = new BaseForm
        {
            Text = "Đặt lại mật khẩu: " + account.Username,
            ClientSize = new Size(470, 240), MinimumSize = new Size(430, 240),
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false, MinimizeBox = false, BackColor = Theme.CardBg,
        };
        var fields = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(20, 16, 20, 16) };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var label = new Label { Text = "Mật khẩu mới *:", AutoSize = true, ForeColor = Theme.TextPrimary };
        var input = new TextBox { Dock = DockStyle.Top, UseSystemPasswordChar = true, AccessibleName = "Mật khẩu mới" };
        var status = new Label { AutoSize = true, Dock = DockStyle.Fill, ForeColor = Theme.Danger };
        fields.Controls.Add(label); fields.Controls.Add(input); fields.Controls.Add(status);
        dialog.Controls.Add(fields);
        var save = Theme.MakePrimaryButton("Lưu mật khẩu", 150);
        FormStyle.EditorFooter(dialog, FormStyle.NewCloseButton(dialog), save);
        ModernLook.Apply(dialog);
        DialogKeys.Enable(dialog, save);
        Anim.FadeIn(dialog);
        save.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(input.Text))
                FieldHint.Show(input, status, "Vui lòng nhập mật khẩu mới.");
            else dialog.DialogResult = DialogResult.OK;
        };

        dialog.AcceptButton = save;
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;

        try
        {
            await _api.Accounts.ResetPasswordAsync(account.Id, input.Text);
            Toast.Show(this, "✅ Đã đặt lại mật khẩu.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(FindForm(), ex.Message, "Không đặt lại được mật khẩu", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
