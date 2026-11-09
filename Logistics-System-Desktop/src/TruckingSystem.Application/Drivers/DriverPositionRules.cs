using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;

namespace TruckingSystem.Application.Drivers;

/// <summary>Ai là TÀI XẾ được xác định theo CHỨC DANH của nhân viên (danh mục Chức danh do
/// màn Nhân viên / Hệ thống quản lý). Danh mục chỉ có cột Tên (chữ tự do, sửa được), nên so
/// khớp mềm: bỏ dấu, không phân biệt hoa thường, bỏ khoảng trắng thừa; chấp nhận các tên
/// chứa "tài xế", "lái xe" hoặc "driver" (VD "Tài xế", "tai xe", "Lái xe", "Tài xế container").
/// Hồ sơ tài xế (DriverProfile: bằng lái, kho đóng quân) vẫn là điều kiện để được Chia xe.
/// DÙNG CHUNG cho: màn Tài xế, Chia xe, Đổi xe/tài xế, Bắt đầu chuyến, Đăng ký ngày làm,
/// Thông báo — để mọi nơi hiểu "tài xế" giống nhau.</summary>
public static class DriverPositionRules
{
    private static readonly string[] Keywords = { "tai xe", "lai xe", "driver" };

    public static bool IsDriverPosition(string? positionName)
    {
        if (string.IsNullOrWhiteSpace(positionName)) return false;
        var n = Normalize(positionName);
        return Keywords.Any(k => n.Contains(k));
    }

    /// <summary>Id các chức danh được coi là Tài xế (danh mục nhỏ, lọc trong bộ nhớ) —
    /// dùng trong truy vấn: <c>e.PositionId != null &amp;&amp; ids.Contains(e.PositionId.Value)</c>.</summary>
    public static async Task<List<int>> DriverPositionIdsAsync(IApplicationDbContext db)
    {
        var positions = await db.Positions.Select(p => new { p.Id, p.Name }).ToListAsync();
        return positions.Where(p => IsDriverPosition(p.Name)).Select(p => p.Id).ToList();
    }

    /// <summary>"  Tài   Xế " -> "tai xe".</summary>
    private static string Normalize(string s)
    {
        var decomposed = s.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);
        foreach (var ch in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;
            sb.Append(ch == 'đ' ? 'd' : char.IsWhiteSpace(ch) ? ' ' : ch);
        }
        return string.Join(' ', sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>Bỏ dấu + chữ thường (dùng chung cho so khớp loại xe).</summary>
    internal static string NormalizeText(string? s) => string.IsNullOrWhiteSpace(s) ? "" : Normalize(s);
}
