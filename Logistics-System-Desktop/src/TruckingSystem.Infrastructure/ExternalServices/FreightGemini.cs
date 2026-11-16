using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TruckingSystem.Application.FreightChat;

namespace TruckingSystem.Infrastructure.ExternalServices;

public sealed class FreightGemini(HttpClient http, IConfiguration config) : IFreightChatAi, IFreightEmbedding
{
    public async Task<FreightDraft> ExtractAsync(FreightDraft draft, IReadOnlyList<ChatMessage> history, PendingQuestion? pending, CancellationToken ct)
        => FreightAi.ParseDraft(await GenerateAsync(FreightAi.Instruction, FreightAi.UserContent(draft, history, pending), json: true, 45, ct));

    public async Task<string?> ReplyAsync(IReadOnlyList<ChatMessage> history, FreightReplyBrief brief, CancellationToken ct)
        => FreightAi.NaturalReplies(config)
            ? await GenerateAsync(FreightAi.ReplyInstruction, FreightAi.ReplyContent(history, brief), json: false, 20, ct)
            : null;

    private async Task<string?> GenerateAsync(string instruction, string content, bool json, int timeoutSeconds, CancellationToken ct)
    {
        var model = config["FreightChat:Model"] ?? "gemini-3.8-flash";
        using var response = await CallAsync(model + ":generateContent", new
        {
            systemInstruction = new { parts = new[] { new { text = instruction } } },
            contents = new[] { new { role = "user", parts = new[] { new { text = content } } } },
            generationConfig = json
                ? (object)new { responseMimeType = "application/json", temperature = 0, maxOutputTokens = 4096 }
                : new { temperature = 0.4, maxOutputTokens = 1024 }
        }, timeoutSeconds, ct);
        try { return response.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString(); }
        catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException or IndexOutOfRangeException)
        { throw new FreightServiceUnavailableException(FreightAi.Malformed); }
    }

    public async Task<float[]> EmbedAsync(string text, bool query, CancellationToken ct)
    {
        using var response = await CallAsync("gemini-embedding-001:embedContent", new
        {
            model = "models/gemini-embedding-001",
            content = new { parts = new[] { new { text } } },
            taskType = query ? "RETRIEVAL_QUERY" : "RETRIEVAL_DOCUMENT", outputDimensionality = 768
        }, 45, ct);
        float[] values;
        try { values = response.RootElement.GetProperty("embedding").GetProperty("values").EnumerateArray().Select(v => v.GetSingle()).ToArray(); }
        catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException or FormatException) { values = []; }
        var norm = Math.Sqrt(values.Sum(v => (double)v * v));
        if (values.Length != 768 || !double.IsFinite(norm) || norm <= 0)
            throw new FreightServiceUnavailableException("Dịch vụ tìm kiếm loại xe trả dữ liệu không hợp lệ. Vui lòng thử lại sau.");
        return values.Select(v => (float)(v / norm)).ToArray();
    }

    private Task<JsonDocument> CallAsync(string operation, object payload, int timeoutSeconds, CancellationToken ct)
    {
        // Khi hội thoại dùng nhà cung cấp khác, FreightChat:ApiKey là khóa của nhà cung cấp đó; Gemini chỉ còn lo embedding.
        var key = FreightAi.UsesOpenAiCompatible(config)
            ? config["GEMINI_API_KEY"] ?? config["Gemini:ApiKey"]
            : config["FreightChat:ApiKey"] ?? config["GEMINI_API_KEY"] ?? config["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(key)) throw new FreightServiceUnavailableException("Chatbot chưa được cấu hình dịch vụ AI. Vui lòng liên hệ quản trị viên.");
        if (operation.Any(c => !(char.IsAsciiLetterOrDigit(c) || c is '-' or '.' or ':')))
            throw new FreightServiceUnavailableException("Cấu hình model chatbot không hợp lệ. Vui lòng liên hệ quản trị viên.");
        return FreightAi.SendAsync(http, () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://generativelanguage.googleapis.com/v1beta/models/" + operation);
            request.Headers.Add("x-goog-api-key", key);
            request.Content = JsonContent.Create(payload);
            return request;
        }, "Gemini", ct, timeoutSeconds);
    }
}
