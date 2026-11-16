using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using TruckingSystem.Application.FreightChat;

namespace TruckingSystem.Infrastructure.ExternalServices;

public sealed class FreightChatContext(IDataProtectionProvider provider) : IFreightChatContext
{
    private readonly IDataProtector protector = provider.CreateProtector("FreightChat.v2");
    public string Protect(ChatState state) => protector.Protect(JsonSerializer.Serialize(state));
    public ChatState Unprotect(string context)
    {
        try
        {
            var state = JsonSerializer.Deserialize<ChatState>(protector.Unprotect(context));
            if (state == null || state.ExpiresUtc <= DateTime.UtcNow) throw new InvalidOperationException();
            return state;
        }
        catch { throw new ArgumentException("Cuộc trò chuyện đã hết hạn hoặc không hợp lệ. Vui lòng chọn Cuộc trò chuyện mới."); }
    }
}
