using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_AuditLogManagement;
using TruckingSystem.Desktop.SERVICE_Auth;

namespace TruckingSystem.Desktop.GUI_AccountManagement;

public class AccountsControl : BaseUserControl
{
    private readonly ApiServices _api;
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill };
    private readonly Label _status = new() { AutoSize = true, ForeColor = Theme.TextSecondary, Dock = DockStyle.Bottom, Height = 28 };
    private readonly ToolStripMenuItem _edit = new("✏️ Sửa / phân quyền");
    private readonly ToolStripMenuItem _password = new("🔑 Đặt lại mật khẩu");
    private List<AccountItem> _accounts = new();
    private List<string> _roles = new();
    private bool _loading, _busy;
    private bool CanManage => !_loading && !_busy && Enabled && _api.Connection.IsInRole("Admin");

    public AccountsControl(ApiServices api)
    {
        _api = api;
        Theme.StyleGrid(_grid);
        _grid.SelectionChanged += (_, _) => UpdateMenu();
        var menu = new ContextMenuStrip();
        menu.Items.AddRange(new ToolStripItem[] { _edit, _password });
        menu.Opening += (_, e) => { UpdateMenu(); e.Cancel = !CanManage || SelectedAccount() is null; };
        ModernMenu.Apply(menu);
        _grid.ContextMenuStrip = menu;
        _grid.CellMouseDown += (_, e) =>
        {
            if (e.Button != MouseButtons.Right) return;
            _grid.ClearSelection();
            if (e.RowIndex < 0) { _grid.CurrentCell = null; return; }
            _grid.CurrentCell = _grid.Rows[e.RowIndex].Cells.Cast<DataGridViewCell>().First(c => c.Visible);
            _grid.Rows[e.RowIndex].Selected = true;
        };
        _grid.MouseDown += (_, e) =>
        {
            if (e.Button != MouseButtons.Right || _grid.HitTest(e.X, e.Y).RowIndex >= 0) return;
            _grid.ClearSelection();
            _grid.CurrentCell = null;
        };
        Disposed += (_, _) => menu.Dispose();

        Padding = new Padding(16);
        var title = new Label { Text = "Quản lý tài khoản", Font = Theme.FontTitle, ForeColor = Theme.TextPrimary, AutoSize = true };
        var actions = new FlowLayoutPanel { AutoSize = true, WrapContents = true, Dock = DockStyle.Fill };
        var create = Theme.MakePrimaryButton("Thêm tài khoản", 145);
        var refresh = Theme.MakeSecondaryButton("Làm mới", 100);
        actions.Controls.AddRange(new Control[] { create, refresh });
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
        _edit.Click += async (_, _) => { if (SelectedAccount() is { } account) await OpenEditorAsync(account); };
        _password.Click += async (_, _) => await ResetPasswordAsync();
        refresh.Click += async (_, _) => { if (!_busy) await LoadAsync(); };
        UpdateMenu();
        Load += async (_, _) => await LoadAsync();
    }

    private AccountItem? SelectedAccount()
    {
        if (_grid.CurrentRow?.Cells["Id"].Value is not string id) return null;
        return _accounts.FirstOrDefault(a => a.Id == id);
    }

    private void UpdateMenu() => _edit.Enabled = _password.Enabled = CanManage && SelectedAccount() is not null;

    private async Task LoadAsync()
    {
        if (_loading) return;
        _loading = true;
        UpdateMenu();
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
                Nhân_viên = a.EmployeeName ?? "Chưa gán",
                Vai_trò = string.Join(", ", a.Roles.Select(AuditLabels.Role)),
                Trạng_thái = a.IsActive ? "Hoạt động" : "Đã khóa",
            }).ToList();
            if (_grid.Columns["Id"] is not null) _grid.Columns["Id"]!.Visible = false;
            _status.Text = $"{_accounts.Count} tài khoản — chuột phải để sửa / phân quyền hoặc đặt lại mật khẩu";
        }
        catch (Exception ex)
        {
            _status.ForeColor = Theme.Danger;
            _status.Text = "Không tải được tài khoản: " + ex.Message;
        }
        finally { _loading = false; UpdateMenu(); }
    }

    private async Task OpenEditorAsync(AccountItem? account)
    {
        if (!CanManage) return;
        if (_roles.Count == 0)
        {
            MessageBox.Show(FindForm(), "Không có vai trò để phân quyền.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        _busy = true;
        UpdateMenu();
        try
        {
            var employees = await _api.Accounts.GetEmployeeOptionsAsync(account?.Id);
            using var dialog = new AccountEditorForm(_roles, account, employees, async form =>
            {
                if (account is null)
                    await _api.Accounts.CreateAsync(form.Username, form.Email, form.Password, form.IsActive, form.Roles, form.EmployeeId);
                else
                    await _api.Accounts.UpdateAsync(account.Id, form.Email, form.IsActive, form.Roles, form.EmployeeId);
            });
            if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(FindForm(), ex.Message, "Không lưu được tài khoản", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { _busy = false; UpdateMenu(); }
    }

    private async Task ResetPasswordAsync()
    {
        if (!CanManage) return;
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

        _busy = true;
        UpdateMenu();
        try
        {
            await _api.Accounts.ResetPasswordAsync(account.Id, input.Text);
            Toast.Show(this, "✅ Đã đặt lại mật khẩu.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(FindForm(), ex.Message, "Không đặt lại được mật khẩu", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { _busy = false; UpdateMenu(); }
    }
}
