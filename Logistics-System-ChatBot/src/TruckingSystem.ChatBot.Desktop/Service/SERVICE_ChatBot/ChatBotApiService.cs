using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_ChatBot;
namespace TruckingSystem.Desktop.SERVICE_ChatBot;
/// <summary>
/// Gửi câu hỏi tới ServerUrl trong chatbot.settings.json: chỉ ghi domain (vd. https://trucking...) thì nối thêm
/// api/public/freight-chat; ghi URL có đường dẫn thì gọi đúng nguyên URL đó.
/// </summary>
public sealed class ChatBotApiService(ApiConnection connection)
{
    public const string DefaultPath = "api/public/freight-chat";
    private readonly string path = EndpointPath(connection.BaseUrl);

    public static string EndpointPath(string serverUrl)
        => Uri.TryCreate(serverUrl, UriKind.Absolute, out var uri) && uri.AbsolutePath.Trim('/').Length > 0 ? "" : DefaultPath;

    public Task<ChatResponse?> SendAsync(string message, string? context, string? selection)
        => connection.PostAsync<ChatResponse>(path, new { message, context, selection });
}
