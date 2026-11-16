using Newtonsoft.Json.Linq;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.GUI.GUI_ChatBot;

namespace TruckingSystem.ChatBot.Desktop;

/// <summary>App chatbot tra cước đóng gói riêng: mở thẳng khung chat, gọi TruckingSystem.ChatBot.API.</summary>
internal static class Program
{
    private const string DefaultServerUrl = "https://localhost:8090";

    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Lỗi không bắt trong handler async void: báo lỗi rồi chạy tiếp, không tắt app.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => ShowUnexpectedError(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex) ShowUnexpectedError(ex);
        };

        Application.Run(new ChatBotForm(new ApiConnection(ReadServerUrl())));
    }

    /// <summary>ServerUrl trong chatbot.settings.json cạnh file exe (domain của ChatBot API, hoặc URL endpoint đầy đủ); thiếu/sai thì dùng mặc định.</summary>
    private static string ReadServerUrl()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "chatbot.settings.json");
            if (File.Exists(path))
            {
                var url = JObject.Parse(File.ReadAllText(path))["ServerUrl"]?.ToString()?.Trim();
                if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                    return url!;
            }
        }
        catch
        {
            // File cấu hình hỏng: dùng địa chỉ mặc định.
        }
        return DefaultServerUrl;
    }

    private static void ShowUnexpectedError(Exception ex)
    {
        try
        {
            var inner = ex is AggregateException agg && agg.InnerException is not null ? agg.InnerException : ex;
            var message = inner is HttpRequestException or TaskCanceledException
                ? "Không kết nối được máy chủ chatbot (hoặc máy chủ phản hồi quá lâu). Kiểm tra mạng / ChatBot API rồi thử lại.\n\nChi tiết: " + inner.Message
                : "Đã xảy ra lỗi ngoài dự kiến — thao tác vừa rồi chưa hoàn tất, ứng dụng vẫn chạy tiếp.\n\nChi tiết: " + inner.Message;
            MessageBox.Show(message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch
        {
            // Không để chính hộp thông báo lỗi gây lỗi tiếp.
        }
    }
}
