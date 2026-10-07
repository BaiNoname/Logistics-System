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

        var header = new TableLayoutPanel { Dock = DockStyle.Top, Height = 68, ColumnCount = 2 };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        header.Controls.Add(new Label { Text = "Quản lý tài khoản", Font = Theme.FontTitle, ForeColor = Theme.TextPrimary, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        var actions = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        var create = Theme.MakePrimaryButton("Thêm tài khoản", 145);
        var refresh = Theme.MakeSecondaryButton("Làm mới", 100);
        actions.Controls.Add(create);
        actions.Controls.Add(_edit);
        actions.Controls.Add(_password);
        actions.Controls.Add(refresh);
        header.Controls.Add(actions, 1, 0);

        Controls.Add(_grid);
        Controls.Add(_status);
        Controls.Add(header);
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

        using var dialog = new Form
        {
            Text = "Đặt lại mật khẩu: " + account.Username,
            Size = new Size(430, 210),
            MinimumSize = new Size(430, 210),
            MaximumSize = new Size(430, 210),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.White,
            Font = Theme.FontNormal,
        };
        var label = new Label { Text = "Mật khẩu mới", Left = 24, Top = 20, AutoSize = true };
        var input = new TextBox { Left = 24, Top = 49, Width = 365, UseSystemPasswordChar = true };
        var save = Theme.MakePrimaryButton("Lưu mật khẩu", 140);
        save.Left = 249;
        save.Top = 99;
        save.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(input.Text))
                MessageBox.Show(dialog, "Vui lòng nhập mật khẩu mới.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else dialog.DialogResult = DialogResult.OK;
        };
        dialog.Controls.AddRange(new Control[] { label, input, save });
        dialog.AcceptButton = save;
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;

        try
        {
            await _api.Accounts.ResetPasswordAsync(account.Id, input.Text);
            MessageBox.Show(FindForm(), "Đã đặt lại mật khẩu.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(FindForm(), ex.Message, "Không đặt lại được mật khẩu", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
