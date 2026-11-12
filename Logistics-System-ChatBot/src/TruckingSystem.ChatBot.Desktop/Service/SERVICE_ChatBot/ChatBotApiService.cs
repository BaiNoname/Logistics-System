using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_ChatBot;
namespace TruckingSystem.Desktop.SERVICE_ChatBot;
public sealed class ChatBotApiService(ApiConnection connection)
{
    public Task<ChatResponse?> SendAsync(string message, string? context, string? selection)
        => connection.PostAsync<ChatResponse>("api/public/freight-chat", new { message, context, selection });
}
