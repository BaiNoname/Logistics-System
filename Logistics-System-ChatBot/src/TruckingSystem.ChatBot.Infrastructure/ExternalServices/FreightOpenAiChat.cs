using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using TruckingSystem.Application.FreightChat;

namespace TruckingSystem.Infrastructure.ExternalServices;

/// <summary>
/// Hội thoại chatbot cước qua API chuẩn OpenAI (/chat/completions): NVIDIA NIM, OpenAI, Groq...
/// Đổi nhà cung cấp bằng FreightChat:BaseUrl/Model/ApiKey; nếu để trống thì dùng mục Nvidia có sẵn.
/// </summary>
public sealed class FreightOpenAiChat(HttpClient http, IConfiguration config) : IFreightChatAi
{
    public async Task<FreightDraft> ExtractAsync(FreightDraft draft, IReadOnlyList<ChatMessage> history, PendingQuestion? pending, CancellationToken ct)
        => FreightAi.ParseDraft(await CompleteAsync(FreightAi.Instruction, FreightAi.UserContent(draft, history, pending), json: true, 45, ct));

    public async Task<string?> ReplyAsync(IReadOnlyList<ChatMessage> history, FreightReplyBrief brief, CancellationToken ct)
        => FreightAi.NaturalReplies(config)
            ? await CompleteAsync(FreightAi.ReplyInstruction, FreightAi.ReplyContent(history, brief), json: false, 20, ct)
            : null;

    private async Task<string?> CompleteAsync(string instruction, string content, bool json, int timeoutSeconds, CancellationToken ct)
    {
        var baseUrl = config["FreightChat:BaseUrl"] ?? config["Nvidia:BaseUrl"];
        var model = config["FreightChat:Model"] ?? config["Nvidia:Model"];
        var key = config["FreightChat:ApiKey"] ?? config["Nvidia:ApiKey"];
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(model))
            throw new FreightServiceUnavailableException("Chatbot chưa được cấu hình dịch vụ AI. Vui lòng liên hệ quản trị viên.");
        if (!Uri.TryCreate((baseUrl ?? "").TrimEnd('/') + "/chat/completions", UriKind.Absolute, out var endpoint) || endpoint.Scheme != Uri.UriSchemeHttps)
            throw new FreightServiceUnavailableException("Cấu hình địa chỉ dịch vụ AI không hợp lệ. Vui lòng liên hệ quản trị viên.");
        // Model có chế độ suy luận (VD Nemotron) chậm 10 lần và có thể hết token trước khi trả JSON.
        var disableThinking = bool.TryParse(config["FreightChat:DisableThinking"], out var configured)
            ? configured : endpoint.Host.EndsWith("nvidia.com", StringComparison.OrdinalIgnoreCase);
        var payload = new Dictionary<string, object>
        {
            ["model"] = model,
            ["temperature"] = json ? 0 : 0.4,
            ["max_tokens"] = json ? 4096 : 1024,
            ["messages"] = new object[]
            {
                new { role = "system", content = instruction },
                new { role = "user", content }
            }
        };
        if (json) payload["response_format"] = new { type = "json_object" };
        if (disableThinking) payload["chat_template_kwargs"] = new { enable_thinking = false };

        using var response = await FreightAi.SendAsync(http, () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
            request.Content = JsonContent.Create(payload);
            return request;
        }, endpoint.Host, ct, timeoutSeconds);
        // Chỉ đọc content; reasoning_content (nếu có) là suy luận nội bộ của model, bỏ qua.
        try { return response.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString(); }
        catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException or IndexOutOfRangeException)
        { throw new FreightServiceUnavailableException(FreightAi.Malformed); }
    }
}
