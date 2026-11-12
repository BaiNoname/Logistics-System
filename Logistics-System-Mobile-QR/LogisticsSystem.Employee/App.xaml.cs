using LogisticsSystem.Employee.Pages;
using LogisticsSystem.Employee.Services;

namespace LogisticsSystem.Employee;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        Page root = AppServices.Session.IsLoggedIn
            ? new AppShell()
            : new NavigationPage(new LoginPage());

        return new Window(root);
    }
}

/// <summary>Chuyển đổi giữa màn hình đăng nhập và ứng dụng chính.</summary>
public static class AppRoot
{
    public static void ShowMain() => SetRoot(new AppShell());

    public static void ShowLogin() => SetRoot(new NavigationPage(new LoginPage()));

    private static void SetRoot(Page page)
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window is not null)
            window.Page = page;
    }
}
