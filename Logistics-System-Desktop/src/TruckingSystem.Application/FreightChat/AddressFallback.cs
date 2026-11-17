namespace TruckingSystem.Application.FreightChat;

/// <summary>
/// Địa chỉ ước tính khi bản đồ không tìm thấy địa chỉ khách gõ: bỏ dần phần cụ thể nhất (số nhà, phường, quận…)
/// để tra theo khu vực rộng hơn, VD "Quận 1 thành phố Hồ Chí Minh" → "thành phố Hồ Chí Minh". Khách vẫn bấm xác nhận.
/// </summary>
public static class AddressFallback
{
    public const int MaxTries = 3;
    // Từ mở đầu một cấp hành chính (đã bỏ dấu); cụm hai từ đặt trước để không tách "thị xã" thành "thị" | "xã".
    private static readonly string[] TwoWords = ["thanh pho", "thi xa", "thi tran"];
    private static readonly string[] OneWord = ["phuong", "xa", "quan", "huyen", "tp", "tinh"];

    /// <summary>Các câu tra rộng dần (không gồm câu gốc), cụ thể nhất trước, tối đa <see cref="MaxTries"/>.</summary>
    public static List<string> Broader(string? address)
    {
        if (string.IsNullOrWhiteSpace(address)) return [];
        var parts = address.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        if (parts.Count <= 1) parts = SplitAdministrative(address.Trim());
        var original = string.Join(", ", parts);
        return Enumerable.Range(1, Math.Max(parts.Count - 1, 0))
            .Select(i => string.Join(", ", parts.Skip(i)))
            .Where(q => q.Length > 0 && !string.Equals(q, original, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase).Take(MaxTries).ToList();
    }

    // Từ chung chung trong địa chỉ, không dùng để đối chiếu kết quả bản đồ.
    private static readonly HashSet<string> Generic =
        ["hem", "duong", "so", "ngo", "ngach", "kiet", "to", "ap", "khu", "pho", "thon", "lo", "nha", "dia", "chi", "o", "tai", .. OneWord];

    /// <summary>
    /// Bản đồ trả gợi ý gần đúng ngay cả khi tên đường/hẻm không có thật. Kết quả chỉ coi là đúng địa chỉ khi
    /// có gợi ý chứa đủ các từ riêng (tên đường, tên khu…) ở phần cụ thể nhất khách gõ; phần đó chỉ có số/từ chung thì coi là đúng.
    /// </summary>
    public static bool MatchesTyped(string? address, IEnumerable<string> suggestions)
    {
        if (string.IsNullOrWhiteSpace(address)) return true;
        var parts = address.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        var first = parts.Count > 1 ? parts[0] : SplitAdministrative(address.Trim())[0];
        var words = FreightText.Fold(first).Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => !Generic.Contains(w) && !w.All(char.IsDigit)).ToList();
        if (words.Count == 0) return true;
        return suggestions.Select(FreightText.Fold).Any(s => words.All(w => FreightText.Has(s, w)));
    }

    /// <summary>Tách câu không có dấu phẩy tại chỗ bắt đầu cấp hành chính: "Quận 1 TP HCM" → ["Quận 1", "TP HCM"].</summary>
    private static List<string> SplitAdministrative(string address)
    {
        var tokens = address.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var folded = tokens.Select(FreightText.Fold).ToArray();
        var starts = new List<int> { 0 };
        for (var i = 1; i < tokens.Length; i++)
        {
            var pair = i + 1 < tokens.Length ? $"{folded[i]} {folded[i + 1]}" : null;
            if (pair is not null && TwoWords.Contains(pair)) { starts.Add(i); i++; continue; }
            // "TP.HCM" bỏ dấu thành "tp hcm" nên so từ đầu tiên của token.
            if (OneWord.Contains(folded[i].Split(' ')[0])) starts.Add(i);
        }
        starts.Add(tokens.Length);
        return starts.Zip(starts.Skip(1), (from, to) => string.Join(' ', tokens[from..to])).ToList();
    }
}
