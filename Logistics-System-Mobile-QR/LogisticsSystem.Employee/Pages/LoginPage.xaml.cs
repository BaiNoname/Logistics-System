using LogisticsSystem.Employee.Config;
using LogisticsSystem.Employee.Services;

namespace LogisticsSystem.Employee.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
        ServerEntry.Text = AppSettings.ApiBaseUrl;
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;

        var userName = UserNameEntry.Text?.Trim() ?? string.Empty;
        var password = PasswordEntry.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrEmpty(password))
        {
            ShowError("Nhập tên đăng nhập và mật khẩu.");
            return;
        }

        if (!AppSettings.TryNormalize(ServerEntry.Text, out var serverUrl))
        {
            ShowError("Địa chỉ máy chủ không hợp lệ.");
            return;
        }

        AppSettings.ApiBaseUrl = serverUrl;
        SetBusy(true);

        try
        {
            var result = await AppServices.Auth.LoginAsync(userName, password);
            await AppServices.Session.SaveAsync(result.Token!, userName);

            AppRoot.ShowMain();
        }
        catch (ApiException ex)
        {
            ShowError(ex.Message);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    private void SetBusy(bool busy)
    {
        Busy.IsVisible = busy;
        Busy.IsRunning = busy;
        LoginButton.IsEnabled = !busy;
    }
}
