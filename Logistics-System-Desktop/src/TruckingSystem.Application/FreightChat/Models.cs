using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Application.FreightChat;

public record FreightChatRequest(string Message, string? Context = null, string? Selection = null);
public record ChatOption(string Id, string Label);
public record FreightChatResponse(string Message, string Context, List<ChatOption> Options, FreightQuote? Quote, List<ChatTable> Tables);
/// <summary>
/// Bảng server dựng sẵn để client vẽ: Kind = priceList | formula | guide | summary. Ô đã định dạng vi-VN;
/// NumericColumns là chỉ số cột canh phải.
/// </summary>
public record ChatTable(string Kind, string Title, string? Subtitle, List<string> Columns, List<int> NumericColumns,
    List<List<string>> Rows, string? Note);
/// <summary>Dữ liệu bảng tóm tắt đơn ở bước xác nhận (nhãn tiếng Việt đã dịch sẵn).</summary>
public record FreightSummary(string Cargo, string Weight, bool IsFragile, bool IsHazmat, string Vehicle, string PickupMethod,
    string From, string WarehouseName, string? WarehouseAddress, bool WarehouseSuitable, string To);
/// <summary>Báo giá một lô: PickupMethod = "company" | "self"; Legs = chặng lấy hàng (nếu có) và giao hàng.</summary>
public record FreightQuote(string PriceList, DateTime EffectiveDate, DateTime QuotedAtUtc, string VehicleType,
    string PickupMethod, string Warehouse, List<QuotedLeg> Legs, List<FreightQuoteLine> Lines, decimal Total, string Notice);
public record QuotedLeg(string Name, string From, string To, decimal DistanceKm);
public record ChatMessage(string Role, string Text);

/// <summary>Dữ liệu AI trích từ hội thoại; null = khách chưa nói (server sẽ hỏi).</summary>
public record FreightDraft
{
    public string? Cargo { get; set; }
    public decimal? Weight { get; set; }
    public string? Unit { get; set; }
    /// <summary>AI suy từ Cargo; null = không chắc.</summary>
    public bool? IsFragile { get; set; }
    public bool? IsHazmat { get; set; }
    /// <summary>Nơi lấy hàng (hoặc khu vực khách ở khi tự mang đến kho).</summary>
    public string? From { get; set; }
    public string? To { get; set; }
    /// <summary>"company" (kho cử xe tới lấy) | "self" (khách tự mang đến kho) | null.</summary>
    public string? PickupMethod { get; set; }
    /// <summary>Loại xe khách tự nêu; server không để AI tự chọn.</summary>
    public string? Vehicle { get; set; }
    /// <summary>Bảng khách muốn xem trong tin nhắn mới nhất (priceList | formula | guide); server đọc rồi xóa.</summary>
    public List<string>? Asks { get; set; }
}

/// <summary>Câu hỏi bắt buộc server vừa hỏi: Slot = cargo | weight | cargoType | from | to | pickupMethod | vehicle | place | confirm.</summary>
public record PendingQuestion(string Slot, string Text);
public record ConfirmedPlace(string Address, double Lat, double Lng);
public record TariffDocument(int Id, int PriceListId, string Text, string Hash);
public record TariffMatch(int Id, string Hash);
public class ChatState
{
    public DateTime ExpiresUtc { get; set; }
    public List<ChatMessage> History { get; set; } = [];
    public FreightDraft Draft { get; set; } = new();
    public PendingQuestion? Pending { get; set; }
    public int? TariffId { get; set; }
    public string? TariffHash { get; set; }
    /// <summary>Khối lượng (kg) lúc chọn xe; đổi khối lượng thì gợi ý xe lại.</summary>
    public decimal? TariffWeightKg { get; set; }
    public List<int> TariffChoices { get; set; } = [];
    public ConfirmedPlace? PickupPlace { get; set; }
    public ConfirmedPlace? DropoffPlace { get; set; }
    public List<ConfirmedPlace> PlaceChoices { get; set; } = [];
    public WarehouseCandidate? Warehouse { get; set; }
    public DateOnly? SurchargeDate { get; set; }
    public bool AwaitingConfirmation { get; set; }
}
/// <summary>Dịch vụ AI/tra cứu tạm thời không dùng được; Message là câu tiếng Việt an toàn để hiển thị cho khách.</summary>
public sealed class FreightServiceUnavailableException(string message) : Exception(message);
/// <summary>
/// Gói dữ kiện server giao cho AI diễn đạt: Message là nội dung bắt buộc truyền đạt (cũng là câu dự phòng),
/// Appended là khối server hiển thị nguyên văn sau câu AI; TableShown = lượt này có bảng kèm theo.
/// </summary>
public record FreightReplyBrief(string Message, string? Appended, List<string> Options, List<string> Facts, bool TableShown = false);
public interface IFreightChatAi
{
    /// <summary>Trích dữ liệu lô hàng; <paramref name="pending"/> là câu server vừa hỏi để hiểu câu trả lời cụt.</summary>
    Task<FreightDraft> ExtractAsync(FreightDraft draft, IReadOnlyList<ChatMessage> history, PendingQuestion? pending, CancellationToken ct);
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
