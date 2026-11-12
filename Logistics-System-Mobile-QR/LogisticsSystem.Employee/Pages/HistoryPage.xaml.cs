using System.Collections.ObjectModel;
using LogisticsSystem.Employee.Models;
using LogisticsSystem.Employee.Services;

namespace LogisticsSystem.Employee.Pages;

public partial class HistoryPage : ContentPage
{
    private readonly ObservableCollection<AttendanceRow> _rows = [];

    public HistoryPage()
    {
        InitializeComponent();
        RecordsView.ItemsSource = _rows;

        var today = DateTime.Today;
        FromPicker.Date = new DateTime(today.Year, today.Month, 1);
        ToPicker.Date = today;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_rows.Count == 0)
            await LoadAsync();
    }

    private async void OnLoadClicked(object? sender, EventArgs e) => await LoadAsync();

    private async Task LoadAsync()
    {
        var from = (FromPicker.Date ?? DateTime.Today).Date;
        var to = (ToPicker.Date ?? DateTime.Today).Date;

        if (from > to)
        {
            StatusLabel.Text = "Ngày bắt đầu phải trước ngày kết thúc.";
            return;
        }

        StatusLabel.Text = "Đang tải...";
        try
        {
            var days = await AppServices.Attendance.GetHistoryAsync(from, to);

            _rows.Clear();
            foreach (var day in days.OrderByDescending(d => d.WorkDate))
                _rows.Add(AttendanceRow.From(day));

            StatusLabel.Text = _rows.Count == 0
                ? "Không có bản ghi trong khoảng thời gian này."
                : $"{_rows.Count} ngày có dữ liệu.";
        }
        catch (ApiException ex)
        {
            StatusLabel.Text = ex.Message;
        }
    }
}

public sealed class AttendanceRow
{
    public string DateText { get; init; } = string.Empty;
    public string TimesText { get; init; } = string.Empty;
    public string StatusText { get; init; } = string.Empty;
    public string NoteText { get; init; } = string.Empty;

    public static AttendanceRow From(AttendanceDayDto d) => new()
    {
        DateText = Display.Date(d.WorkDate),
        TimesText = $"Vào {Display.Time(d.CheckInAt)}  ·  Ra {Display.Time(d.CheckOutAt)}",
        StatusText = d.StatusText,
        NoteText = d.IsAdjusted ? "Đã điều chỉnh" : (d.PendingAdjustmentText ?? string.Empty)
    };
}
