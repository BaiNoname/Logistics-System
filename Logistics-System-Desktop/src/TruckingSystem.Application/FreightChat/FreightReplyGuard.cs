using System.Globalization;
using System.Text.RegularExpressions;

namespace TruckingSystem.Application.FreightChat;

/// <summary>Chỉ nhận câu AI viết khi mọi con số đều có trong dữ kiện server hoặc lời khách; còn lại dùng câu dự phòng.</summary>
public static partial class FreightReplyGuard
{
    public const int MaxLength = 1000;

    public static bool Accept(string? reply, FreightReplyBrief brief, IEnumerable<ChatMessage> history)
    {
        if (string.IsNullOrWhiteSpace(reply) || reply.Length > MaxLength) return false;
        if (reply.Contains("```") || reply.Contains('{') || reply.Contains('<')
            || reply.Contains("http", StringComparison.OrdinalIgnoreCase)) return false;
        var sources = new List<string> { brief.Message, brief.Appended ?? "" };
        sources.AddRange(brief.Options);
        sources.AddRange(brief.Facts);
        sources.AddRange(history.Where(m => m.Role == "user").Select(m => m.Text));
        var allowed = sources.SelectMany(s => Numbers().Matches(s)).SelectMany(m => Values(m.Value)).ToHashSet();
        return Numbers().Matches(reply).All(m => Values(m.Value).Any(allowed.Contains));
    }

    public const int MaxSentences = 3;

    /// <summary>
    /// Giữ phần trả lời/đáp lời của AI: bỏ câu AI tự hỏi khách (câu hỏi bắt buộc do server nối sau), câu chép lại lời server
    /// hoặc nút lựa chọn, lời chào lặp lại sau lượt đầu; tối đa <see cref="MaxSentences"/> câu.
    /// </summary>
    public static string? Clean(string? reply, FreightReplyBrief brief, IEnumerable<ChatMessage> history)
    {
        if (string.IsNullOrWhiteSpace(reply)) return null;
        var earlier = history.Where(m => m.Role == "assistant").Select(m => m.Text).ToList();
        // Dòng lời server (đủ dài để không trùng nhầm); nút lựa chọn chỉ loại khi AI chép nguyên nhãn thành một câu.
        var server = earlier.Append(brief.Message).Append(brief.Appended ?? "")
            .SelectMany(t => t.Split('\n')).Select(Normalize).Where(s => s.Length >= 12).ToList();
        var options = brief.Options.Select(Normalize).ToHashSet();
        var kept = new List<string>();
        foreach (var raw in Sentences().Split(reply.Trim()))
        {
            var sentence = raw.Trim();
            if (earlier.Count > 0 && kept.Count == 0) sentence = Greeting().Replace(sentence, "").Trim();
            var normalized = Normalize(sentence);
            if (normalized.Length == 0 || sentence.Contains('?') || sentence.Contains('[')) continue;
            // Câu lộ lời dặn nội bộ (AI chép lại hướng dẫn thay vì nói với khách).
            if (normalized.Contains("lời mở đầu") || normalized.Contains("chép lại") || normalized.Contains("khung chat")) continue;
            // AI hay chép "bảng … đang hiển thị dưới đây" từ lượt trước dù lượt này không kèm bảng.
            // Cũng bỏ câu mời "xem bảng giá", "đã liệt kê bảng giá", "bảng giá đã cập nhật" khi không có bảng.
            if (!brief.TableShown && normalized.Contains("bảng")
                && TableClaims.Any(normalized.Contains)) continue;
            if (options.Contains(normalized) || server.Any(s => normalized.Contains(s) || (normalized.Length >= 12 && s.Contains(normalized)))) continue;
            kept.Add(sentence.Length > 0 ? char.ToUpper(sentence[0]) + sentence[1..] : sentence);
            if (kept.Count == MaxSentences) break;
        }
        var text = string.Join(" ", kept).Trim();
        return text.Length == 0 ? null : text;
    }

    private static readonly string[] TableClaims =
        ["hiển thị", "dưới đây", "bên dưới", "giới thiệu", "liệt kê", "xem", "cập nhật", "đã gửi", "tham khảo bảng"];

    private static string Normalize(string text) => text.Trim().TrimEnd('.', '!', ':', '…', ' ').ToLowerInvariant();

    /// <summary>Hiểu số theo cả kiểu Việt (1.500,5) và kiểu Anh (1,500.5).</summary>
    private static IEnumerable<decimal> Values(string token)
    {
        foreach (var culture in new[] { "vi-VN", "en-US" })
            if (decimal.TryParse(token, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint,
                CultureInfo.GetCultureInfo(culture), out var value))
                yield return value; // decimal so sánh theo giá trị: 8000.00 == 8000
    }

    [GeneratedRegex(@"\d+(?:[.,]\d+)*")]
    private static partial Regex Numbers();

    [GeneratedRegex(@"(?<=[.!?…])\s+")]
    private static partial Regex Sentences();

    [GeneratedRegex(@"^(xin chào|chào bạn|chào anh chị|chào anh|chào chị|chào quý khách|chào shop|chào)\b[^.!,]{0,20}[.!,]\s*", RegexOptions.IgnoreCase)]
    private static partial Regex Greeting();
}
