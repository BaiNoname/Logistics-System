using System.Text.RegularExpressions;

namespace TruckingSystem.Application.Common;

/// <summary>Kiểm tra SỐ ĐIỆN THOẠI / MÃ SỐ THUẾ dùng chung (khách hàng, hãng xăng...).
/// Chấp nhận cách gõ thường gặp: có khoảng trắng / dấu chấm / gạch, đầu +84 hoặc 84.</summary>
public static class ContactRules
{
    /// <summary>"+84 909.123.456" -> "0909123456"; null nếu trống.</summary>
    public static string? NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return null;
        var digits = Regex.Replace(phone.Trim(), @"[\s.\-()]", "");
        if (digits.StartsWith("+84")) digits = "0" + digits[3..];
        else if (digits.StartsWith("84") && digits.Length >= 11) digits = "0" + digits[2..];
        return digits;
    }

    /// <summary>Số Việt Nam: 10 số (di động, cố định mới) hoặc 11 số (cố định cũ), bắt đầu bằng 0.</summary>
    public static bool IsValidPhone(string? normalized)
        => normalized is not null && Regex.IsMatch(normalized, @"^0\d{9,10}$");

    /// <summary>Mã số thuế: 10 số, hoặc 10 số + "-" + 3 số (chi nhánh).</summary>
    public static bool IsValidTaxCode(string? taxCode)
        => taxCode is not null && Regex.IsMatch(taxCode.Trim(), @"^\d{10}(-\d{3})?$");
}
