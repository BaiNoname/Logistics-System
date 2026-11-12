using LogisticsSystem.Employee.Models;
using LogisticsSystem.Employee.Services;

namespace LogisticsSystem.Employee.Pages;

public partial class HomePage : ContentPage
{
    private MobileTodayDto? _me;

    public HomePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async void OnRefresh(object? sender, EventArgs e) => await LoadAsync();

    private async Task LoadAsync()
    {
        ErrorLabel.IsVisible = false;
        DateLabel.Text = $"Hôm nay, {DateTime.Today:dddd dd/MM/yyyy}";

        try
        {
            _me = await AppServices.Attendance.GetMeAsync();
            if (_me is not null)
                AppServices.Session.UpdateDisplayName(_me.Employee.FullName);

            GreetingLabel.Text = string.IsNullOrWhiteSpace(AppServices.Session.DisplayName)
                ? "Xin chào"
                : $"Xin chào, {AppServices.Session.DisplayName}";

            Render();
        }
        catch (ApiException ex)
        {
            ErrorLabel.Text = ex.Message;
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            Refresher.IsRefreshing = false;
        }
    }

    private void Render()
    {
        var today = _me?.Today;
        if (today is null)
        {
            StateLabel.Text = "Chưa có dữ liệu chấm công";
            CheckInLabel.Text = string.Empty;
            CheckOutLabel.Text = string.Empty;
            PendingLabel.IsVisible = false;
            ScanButton.Text = "Quét QR chấm công";
            return;
        }

        StateLabel.Text = string.IsNullOrWhiteSpace(today.StatusText) ? today.Status : today.StatusText;
        CheckInLabel.Text = $"Check-in: {Display.Time(today.CheckInAt)}" +
                            (string.IsNullOrWhiteSpace(today.CheckInSite) ? "" : $"  ({today.CheckInSite})");
        CheckOutLabel.Text = $"Check-out: {Display.Time(today.CheckOutAt)}" +
                             (string.IsNullOrWhiteSpace(today.CheckOutSite) ? "" : $"  ({today.CheckOutSite})");

        PendingLabel.Text = string.IsNullOrWhiteSpace(today.PendingAdjustmentText)
            ? "Có yêu cầu điều chỉnh đang chờ duyệt."
            : today.PendingAdjustmentText;
        PendingLabel.IsVisible = !string.IsNullOrWhiteSpace(today.PendingAdjustmentText);

        ScanButton.Text = today.Status switch
        {
            "NotCheckedIn" => "Quét QR check-in",
            "CheckedIn" or "MissingCheckOut" => "Quét QR check-out",
            _ => "Quét QR chấm công"
        };
    }

    private async void OnScanClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new ScanPage());
    }
}
