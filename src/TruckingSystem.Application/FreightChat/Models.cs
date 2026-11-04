using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Application.FreightChat;

public record FreightChatRequest(string Message, string? Context = null, string? Selection = null);
public record ChatOption(string Id, string Label);
public record FreightChatResponse(string Message, string Context, List<ChatOption> Options, FreightQuote? Quote = null);
public record FreightQuote(string PriceList, DateTime EffectiveDate, DateTime QuotedAtUtc,
    string VehicleType, List<QuotedLeg> Legs, decimal Total, string Notice);
public record QuotedLeg(string From, string To, decimal DistanceKm, decimal WeightKg, FreightQuoteLegPrice Price);
public record ChatMessage(string Role, string Text);
public class FreightDraft
{
    public string? Vehicle { get; set; }
    public List<DraftLeg> Legs { get; set; } = [];
}
public class DraftLeg
{
    public string? From { get; set; }
    public string? To { get; set; }
    public decimal? Weight { get; set; }
    public string? Unit { get; set; }
    public bool? IsEmpty { get; set; }
    public bool? IsFragile { get; set; }
    public bool? IsHazmat { get; set; }
}
public record ConfirmedPlace(string Address, double Lat, double Lng);
public record TariffDocument(int Id, int PriceListId, string Text, string Hash);
public record TariffMatch(int Id, string Hash);
public class ChatState
{
    public DateTime ExpiresUtc { get; set; }
    public List<ChatMessage> History { get; set; } = [];
    public FreightDraft Draft { get; set; } = new();
    public int? TariffId { get; set; }
    public string? TariffHash { get; set; }
    public List<int> TariffChoices { get; set; } = [];
    public List<ConfirmedPlace> Places { get; set; } = [];
    public List<ConfirmedPlace> PlaceChoices { get; set; } = [];
    public DateOnly? SurchargeDate { get; set; }
    public bool AwaitingConfirmation { get; set; }
}
/// <summary>Dịch vụ AI/tra cứu tạm thời không dùng được; Message là câu tiếng Việt an toàn để hiển thị cho khách.</summary>
public sealed class FreightServiceUnavailableException(string message) : Exception(message);
/// <summary>
/// Gói dữ kiện server giao cho AI diễn đạt: Message là nội dung bắt buộc truyền đạt (cũng là câu dự phòng),
/// Appended là khối server hiển thị nguyên văn sau câu AI (tóm tắt hành trình, báo giá).
/// </summary>
public record FreightReplyBrief(string Message, string? Appended, List<string> Options, List<string> Facts);
public interface IFreightChatAi
{
    Task<FreightDraft> ExtractAsync(FreightDraft draft, IReadOnlyList<ChatMessage> history, CancellationToken ct);
    /// <summary>Viết câu trả lời tự nhiên; null khi tính năng bị tắt trong cấu hình.</summary>
    Task<string?> ReplyAsync(IReadOnlyList<ChatMessage> history, FreightReplyBrief brief, CancellationToken ct);
}
public interface IFreightEmbedding
{
    Task<float[]> EmbedAsync(string text, bool query, CancellationToken ct);
}
public interface IFreightVectorStore
{
    Task SynchronizeAsync(IReadOnlyList<TariffDocument> documents, CancellationToken ct);
    Task<List<TariffMatch>> SearchAsync(int priceListId, string query, CancellationToken ct);
}
public interface IFreightChatContext
{
    string Protect(ChatState state);
    ChatState Unprotect(string context);
}
