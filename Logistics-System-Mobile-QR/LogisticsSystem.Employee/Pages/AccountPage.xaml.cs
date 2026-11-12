using LogisticsSystem.Employee.Config;
using LogisticsSystem.Employee.Services;

namespace LogisticsSystem.Employee.Pages;

public partial class AccountPage : ContentPage
{
    public AccountPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        NameLabel.Text = AppServices.Session.DisplayName;
        ServerLabel.Text = AppSettings.ApiBaseUrl;
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        var confirmed = await DisplayAlertAsync("Đăng xuất", "Bạn có chắc muốn đăng xuất?", "Đăng xuất", "Hủy");
        if (!confirmed) return;

        AppServices.Session.Clear();
        AppRoot.ShowLogin();
    }
}
