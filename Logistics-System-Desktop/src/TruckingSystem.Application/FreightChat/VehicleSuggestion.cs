using System.Globalization;
using System.Text.RegularExpressions;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Application.FreightChat;

/// <summary>Gợi ý loại xe theo khối lượng: đọc tải trọng ghi trong tên xe của bảng giá ("Xe tải 1,5 tấn").</summary>
public static partial class VehicleSuggestion
{
    public const int MaxChoices = 5;

    /// <summary>Tải trọng (tấn) đọc từ tên xe; null khi tên không ghi số tấn (VD đầu kéo container).</summary>
    public static decimal? CapacityTons(string vehicleType)
    {
        var match = Capacity().Match(vehicleType ?? "");
        return match.Success && decimal.TryParse(match.Groups[1].Value.Replace(',', '.'), NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture, out var tons) && tons > 0 ? tons : null;
    }

    /// <summary>Xe chở đủ khối lượng không; null khi tên xe không ghi tải trọng.</summary>
    public static bool? Fits(TariffRule rule, decimal weightKg)
        => CapacityTons(rule.VehicleType) is decimal tons ? tons * 1000m >= weightKg : null;

    /// <summary>
    /// Xếp hạng xe cho khối lượng: xe đủ tải nhỏ nhất trước, rồi xe không rõ tải (container), cuối cùng xe thiếu tải
    /// (lớn trước). Phần tử đầu là xe gợi ý; tối đa <see cref="MaxChoices"/> xe.
    /// </summary>
    public static List<TariffRule> Rank(IReadOnlyList<TariffRule> rules, decimal weightKg)
    {
        var tons = weightKg / 1000m;
        return rules.Select(r => (Rule: r, Tons: CapacityTons(r.VehicleType)))
            .OrderBy(x => x.Tons is null ? 1 : x.Tons >= tons ? 0 : 2)
            .ThenBy(x => x.Tons is decimal t ? (t >= tons ? t : -t) : 0)
            .ThenBy(x => x.Rule.Id)
            .Select(x => x.Rule).Take(MaxChoices).ToList();
    }

    private static readonly string[] AskPhrases =
        ["xe nao", "loai xe nao", "xe gi", "loai xe gi", "xe phu hop", "nen di xe", "nen dung xe", "dung xe nao", "can xe nao", "can xe gi", "di xe nao"];

    /// <summary>Khách hỏi nên dùng xe nào (từ khóa không dấu).</summary>
    public static bool IsVehicleQuestion(string? message)
    {
        var text = FreightText.Fold(message);
        return AskPhrases.Any(p => FreightText.Has(text, p));
    }

    [GeneratedRegex(@"(\d+(?:[.,]\d+)?)\s*tấn", RegexOptions.IgnoreCase)]
    private static partial Regex Capacity();
}
