using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TruckingSystem.Application.FreightChat;
using TruckingSystem.Infrastructure.ExternalServices;

namespace TruckingSystem.ChatBot.Infrastructure;

/// <summary>
/// AI + vector RAG cho chatbot tra cước — host TruckingSystem.ChatBot.API gọi sau AddInfrastructureCore của hệ thống,
/// nên API chính không giữ key AI hội thoại và không chạy worker index bảng giá.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddFreightChatInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IFreightChatContext, FreightChatContext>();
        services.AddHttpClient<FreightGemini>();
        services.AddHttpClient<FreightOpenAiChat>();
        // FreightChat:Provider chọn AI hội thoại (Gemini mặc định / OpenAICompatible); embedding luôn dùng Gemini.
        if (FreightAi.UsesOpenAiCompatible(configuration))
            services.AddScoped<IFreightChatAi>(sp => sp.GetRequiredService<FreightOpenAiChat>());
        else
            services.AddScoped<IFreightChatAi>(sp => sp.GetRequiredService<FreightGemini>());
        services.AddScoped<IFreightEmbedding>(sp => sp.GetRequiredService<FreightGemini>());
        services.AddScoped<IFreightVectorStore, FreightVectorStore>();
        services.AddHostedService<FreightIndexWorker>();
        return services;
    }
}
