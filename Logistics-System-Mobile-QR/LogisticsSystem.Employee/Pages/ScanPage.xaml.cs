using LogisticsSystem.Employee.Models;
using LogisticsSystem.Employee.Services;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace LogisticsSystem.Employee.Pages;

public partial class ScanPage : ContentPage
{
    private CameraBarcodeReaderView? _reader;
    private bool _processing;
    private bool _completed;

    public ScanPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var camera = await Permissions.RequestAsync<Permissions.Camera>();
        if (camera != PermissionStatus.Granted)
        {
            StatusLabel.Text = "Chưa cấp quyền camera. Vào Cài đặt để cấp quyền.";
            return;
        }

        if (_reader is null)
        {
            _reader = new CameraBarcodeReaderView
            {
                Options = new BarcodeReaderOptions
                {
                    Formats = BarcodeFormats.TwoDimensional,
                    AutoRotate = true,
                    Multiple = false
                }
            };
            _reader.BarcodesDetected += OnBarcodeDetected;
            CameraHost.Children.Insert(0, _reader);
        }

        _completed = false;
        _reader.IsDetecting = true;
        StatusLabel.Text = "Đang chờ quét QR...";
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (_reader is not null)
            _reader.IsDetecting = false;
    }

    private void OnBarcodeDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        if (_processing || _completed) return;

        var value = e.Results?.FirstOrDefault()?.Value;
        if (string.IsNullOrWhiteSpace(value)) return;

        _processing = true;
        var qrContent = value.Trim();

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (_reader is not null) _reader.IsDetecting = false;
            await ProcessQrAsync(qrContent);
        });
    }

    /// <summary>
    /// Luồng theo API: quét lần đầu với ConfirmCheckOut=false. Nếu server trả Code "ConfirmCheckOut"
    /// thì hỏi xác nhận rồi gửi lại với ConfirmCheckOut=true.
    /// </summary>
    private async Task ProcessQrAsync(string qrContent)
    {
        try
        {
            SetBusy(true, "Đang lấy vị trí...");
            var location = await LocationService.GetCurrentAsync();
            if (location.Accuracy is null)
                throw new InvalidOperationException("Không đọc được độ chính xác GPS. Hãy thử lại ở nơi thoáng hơn.");

            var request = new AttendanceScanRequest
            {
                QrContent = qrContent,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                AccuracyMeters = location.Accuracy.Value,
                IsMockLocation = location.IsFromMockProvider,
                ConfirmCheckOut = false
            };

            SetBusy(true, "Đang gửi chấm công...");
            var result = await AppServices.Attendance.ScanAsync(request);

            if (result is not null && result.Success && result.Code == "ConfirmCheckOut")
            {
                var confirmed = await DisplayAlertAsync(
                    "Xác nhận check-out",
                    string.IsNullOrWhiteSpace(result.Message) ? "Bạn muốn check-out bây giờ?" : result.Message,
                    "Check-out",
                    "Hủy");
                if (!confirmed) return;

                SetBusy(true, "Đang gửi check-out...");
                request.ConfirmCheckOut = true;
                result = await AppServices.Attendance.ScanAsync(request);
            }

            if (result is not null && result.Success)
            {
                _completed = true;
                await DisplayAlertAsync("Thành công", result.Message, "OK");
                await Navigation.PopAsync();
                return;
            }

            await DisplayAlertAsync("Chưa ghi nhận được", result?.Message ?? "Chấm công thất bại.", "OK");
        }
        catch (ApiException ex)
        {
            await DisplayAlertAsync("Lỗi", ex.Message, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Lỗi", ex.Message, "OK");
        }
        finally
        {
            SetBusy(false, "Đang chờ quét QR...");
            _processing = false;
            if (!_completed && _reader is not null)
                _reader.IsDetecting = true;
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void SetBusy(bool busy, string status)
    {
        Busy.IsVisible = busy;
        Busy.IsRunning = busy;
        StatusLabel.Text = status;
    }
}
