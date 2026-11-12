using LogisticsSystem.Employee.Models;
using LogisticsSystem.Employee.Services;

namespace LogisticsSystem.Employee.Pages;

public partial class AdjustmentPage : ContentPage
{
    private List<AdjustmentReasonDto> _reasons = [];

    public AdjustmentPage()
    {
        InitializeComponent();
        DatePicker.Date = DateTime.Today;
        InTimePicker.Time = new TimeSpan(8, 0, 0);
        OutTimePicker.Time = new TimeSpan(17, 0, 0);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadReasonsAsync();
        await LoadRequestsAsync();
    }

    private async Task LoadReasonsAsync()
    {
        if (_reasons.Count > 0) return;

        try
        {
            _reasons = await AppServices.Attendance.GetAdjustmentReasonsAsync();
            ReasonPicker.ItemsSource = _reasons.Select(r => r.Text).ToList();
        }
        catch (ApiException ex)
        {
            ShowError(ex.Message);
        }
    }

    private async void OnSubmitClicked(object? sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;

        if (ReasonPicker.SelectedIndex < 0 || ReasonPicker.SelectedIndex >= _reasons.Count)
        {
            ShowError("Chọn lý do điều chỉnh.");
            return;
        }

        var note = NoteEditor.Text?.Trim();
        var workDate = (DatePicker.Date ?? DateTime.Today).Date;

        var request = new CreateAdjustmentRequest
        {
            WorkDate = workDate,
            Reason = _reasons[ReasonPicker.SelectedIndex].Value,
            RequestedCheckInAt = InCheck.IsChecked ? workDate + (InTimePicker.Time ?? TimeSpan.Zero) : null,
            RequestedCheckOutAt = OutCheck.IsChecked ? workDate + (OutTimePicker.Time ?? TimeSpan.Zero) : null,
            Note = string.IsNullOrEmpty(note) ? null : note
        };

        SubmitButton.IsEnabled = false;
        try
        {
            var result = await AppServices.Attendance.CreateAdjustmentAsync(request);
            if (result is not null && !result.Success)
            {
                ShowError(result.Message ?? "Không gửi được yêu cầu.");
                return;
            }

            NoteEditor.Text = string.Empty;
            InCheck.IsChecked = false;
            OutCheck.IsChecked = false;
            ReasonPicker.SelectedIndex = -1;
            await DisplayAlertAsync("Đã gửi", "Yêu cầu đã được gửi. Bạn sẽ thấy kết quả xử lý tại đây.", "OK");
            await LoadRequestsAsync();
        }
        catch (ApiException ex)
        {
            ShowError(ex.Message);
        }
        finally
        {
            SubmitButton.IsEnabled = true;
        }
    }

    private async Task LoadRequestsAsync()
    {
        try
        {
            var items = await AppServices.Attendance.GetAdjustmentsAsync();
            RequestsHost.Children.Clear();

            foreach (var item in items.OrderByDescending(i => i.CreatedAt))
                RequestsHost.Children.Add(BuildCard(item));

            StatusLabel.Text = items.Count == 0 ? "Chưa có yêu cầu nào." : string.Empty;
        }
        catch (ApiException ex)
        {
            StatusLabel.Text = ex.Message;
        }
    }

    private static View BuildCard(AdjustmentRequestDto item)
    {
        var lines = new VerticalStackLayout { Spacing = 8 };
        lines.Children.Add(new Label
        {
            Text = $"{Display.Date(item.WorkDate)} · {item.ReasonText}",
            FontAttributes = FontAttributes.Bold
        });
        lines.Children.Add(new Label { Text = $"Trạng thái: {item.StatusText}" });
        if (!string.IsNullOrWhiteSpace(item.ReviewNote))
            lines.Children.Add(new Label { Text = $"Ghi chú xử lý: {item.ReviewNote}", Style = (Style)Application.Current!.Resources["Caption"] });

        return new Border
        {
            Content = lines
        };
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }
}
