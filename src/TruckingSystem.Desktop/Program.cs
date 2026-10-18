using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // LƯỚI AN TOÀN: lỗi không bắt ở 1 nút bấm (VD mất mạng / server trả lỗi giữa chừng trong 1
        // handler async void) trước đây làm TẮT HẲN cả ứng dụng. Giờ hiện thông báo lỗi và chạy tiếp.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => ShowUnexpectedError(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex) ShowUnexpectedError(ex);
        };

        Application.Run(new TruckingAppContext());
    }

    private static void ShowUnexpectedError(Exception ex)
    {
        try
        {
            var inner = ex is AggregateException agg && agg.InnerException is not null ? agg.InnerException : ex;
            var message = inner is HttpRequestException or TaskCanceledException
                ? "Không kết nối được máy chủ (hoặc máy chủ phản hồi quá lâu). Kiểm tra mạng / API rồi thử lại.\n\nChi tiết: " + inner.Message
                : "Đã xảy ra lỗi ngoài dự kiến — thao tác vừa rồi chưa hoàn tất, ứng dụng vẫn chạy tiếp.\n\nChi tiết: " + inner.Message;
            MessageBox.Show(message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch
        {
            // Không để chính hộp thông báo lỗi gây lỗi tiếp.
        }
    }
}
