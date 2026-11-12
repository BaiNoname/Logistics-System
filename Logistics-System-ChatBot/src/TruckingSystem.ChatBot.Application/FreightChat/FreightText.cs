using System.Globalization;
using System.Text;

namespace TruckingSystem.Application.FreightChat;

/// <summary>Chuẩn hóa chữ tiếng Việt để so khớp từ khóa: bỏ dấu, đ→d, chữ thường, gộp khoảng trắng.</summary>
public static class FreightText
{
    public static string Fold(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "";
        var builder = new StringBuilder(text.Length);
        foreach (var c in text.Normalize(NormalizationForm.FormD))
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                builder.Append(c switch { 'đ' => 'd', 'Đ' => 'D', _ => char.IsLetterOrDigit(c) ? c : ' ' });
        return string.Join(' ', builder.ToString().ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>Chữ đã Fold có chứa cụm (khớp theo ranh giới từ) không.</summary>
    public static bool Has(string folded, string phrase) => $" {folded} ".Contains($" {phrase} ");
}
