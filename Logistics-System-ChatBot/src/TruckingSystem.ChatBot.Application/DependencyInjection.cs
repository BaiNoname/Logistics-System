using Microsoft.Extensions.DependencyInjection;

namespace TruckingSystem.ChatBot.Application;

/// <summary>Đăng ký use case chatbot tra cước công khai (chỉ host TruckingSystem.ChatBot.API gọi).</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddFreightChat(this IServiceCollection services)
    {
        services.AddScoped<TruckingSystem.Application.FreightChat.FreightChatService>();
        return services;
    }
}
