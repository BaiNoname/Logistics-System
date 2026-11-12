namespace TruckingSystem.Application.FreightChat;

/// <summary>
/// Dự phòng phía server cho câu trả lời cụt khi AI không điền ô đang hỏi
/// (cách lấy hàng, loại hàng); không khớp thì trả null để hỏi lại.
/// </summary>
public static class SlotAnswers
{
    private static readonly string[] Self =
        ["tu mang", "mang den", "mang toi", "mang qua", "tu dem", "dem den", "dem toi", "tu cho", "tu gui", "tu van chuyen"];
    private static readonly string[] Company =
        ["kho lay", "toi lay", "den lay", "qua lay", "xe lay", "lay giup", "lay tan noi", "lay tai nha", "cu xe", "kho cu xe"];
    private static readonly string[] Hazmat = ["nguy hiem", "de chay", "de no", "hoa chat", "doc hai", "chay no"];
    private static readonly string[] Fragile = ["de vo"];

    /// <summary>"company" (kho cử xe tới lấy) | "self" (khách tự mang đến kho) | null.</summary>
    public static string? PickupMethod(string? message)
    {
        var text = FreightText.Fold(message);
        if (Self.Any(p => FreightText.Has(text, p))) return "self";
        if (Company.Any(p => FreightText.Has(text, p))) return "company";
        return null;
    }

    /// <summary>(dễ vỡ, nguy hiểm) đọc từ câu trả lời; cụm có "không" ngay trước bị coi là phủ định.</summary>
    public static (bool Fragile, bool Hazmat)? CargoType(string? message)
    {
        var text = FreightText.Fold(message);
        if (text.Length == 0) return null;
        var fragile = Find(text, Fragile);
        var hazmat = Find(text, Hazmat);
        if (fragile is not null || hazmat is not null) return (fragile == true, hazmat == true);
        if (FreightText.Has(text, "khong") || FreightText.Has(text, "ko") || FreightText.Has(text, "binh thuong")
            || FreightText.Has(text, "hang thuong") || text == "thuong") return (false, false);
        return null;
    }

    private static readonly HashSet<string> BareReplies =
        ["khong", "ko", "k", "co", "ok", "oke", "u", "uh", "um", "vang", "da", "duoc", "dc", "roi", "khong biet", "chua biet"];
    private static readonly HashSet<string> GenericCargo =
        ["hang", "hang hoa", "do", "do dac", "kien hang", "thung hang", "hang cua minh", "mot lo hang", "lo hang", "hang le"];

    /// <summary>Câu trả lời trống nghĩa (có/không/ok…) — không dùng để điền ô có nhiều lựa chọn.</summary>
    public static bool IsBareReply(string? message) => BareReplies.Contains(FreightText.Fold(message));

    /// <summary>Mô tả hàng chung chung, chưa đủ để nhận định dễ vỡ/nguy hiểm.</summary>
    public static bool IsGenericCargo(string? cargo) => cargo is not null && GenericCargo.Contains(FreightText.Fold(cargo));

    /// <summary>true = có cụm khẳng định; false = chỉ có cụm bị phủ định; null = không nhắc tới.</summary>
    private static bool? Find(string text, string[] phrases)
    {
        bool? result = null;
        foreach (var phrase in phrases)
        {
            if (!FreightText.Has(text, phrase)) continue;
            var negated = FreightText.Has(text, "khong " + phrase) || FreightText.Has(text, "ko " + phrase);
            if (!negated) return true;
            result = false;
        }
        return result;
    }
}
