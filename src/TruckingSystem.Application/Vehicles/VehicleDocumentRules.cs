using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.Vehicles;

/// <summary>Quy tắc giấy tờ xe DÙNG CHUNG cho Chia xe, Bắt đầu chuyến và Thông báo.
/// Gia hạn giấy tờ = THÊM 1 dòng mới (dòng cũ vẫn giữ làm lịch sử), nên chỉ xét
/// giấy tờ MỚI NHẤT (hạn xa nhất) của từng loại — trước đây dòng cũ đã hết hạn
/// vẫn làm xe bị loại khỏi Chia xe và báo "ĐÃ HẾT HẠN" mãi dù đã gia hạn.
/// Ngày hết hạn tính theo NGÀY Việt Nam, NGÀY HẾT HẠN VẪN CÒN HIỆU LỰC
/// (khớp màn Giấy tờ xe trên Desktop: "Đã hết hạn" khi ngày hết hạn &lt; hôm nay).</summary>
public static class VehicleDocumentRules
{
    public sealed record LatestDoc(int VehicleId, VehicleDocumentType DocType, DateTime ExpiryDate);

    /// <summary>Hạn xa nhất của từng (xe, loại giấy tờ) — bỏ qua loại "Khác".</summary>
    public static async Task<List<LatestDoc>> GetLatestAsync(IApplicationDbContext db, int? vehicleId = null)
    {
        var query = db.VehicleDocuments.Where(d => d.DocType != VehicleDocumentType.Other);
        if (vehicleId.HasValue) query = query.Where(d => d.VehicleId == vehicleId.Value);

        var rows = await query
            .GroupBy(d => new { d.VehicleId, d.DocType })
            .Select(g => new { g.Key.VehicleId, g.Key.DocType, Latest = g.Max(d => d.ExpiryDate) })
            .ToListAsync();
        return rows.Select(r => new LatestDoc(r.VehicleId, r.DocType, r.Latest)).ToList();
    }

    /// <summary>Còn hiệu lực vào ngày <paramref name="date"/> (ngày VN) — ngày hết hạn vẫn tính là còn hạn.</summary>
    public static bool IsValidOn(DateTime expiryDate, DateTime date) => expiryDate.Date >= date.Date;

    public static string DocTypeText(VehicleDocumentType type) => type switch
    {
        VehicleDocumentType.Registration => "Đăng kiểm",
        VehicleDocumentType.Insurance => "Bảo hiểm",
        VehicleDocumentType.TransportPermit => "Phù hiệu xe (GP vận tải)",
        _ => "Giấy tờ",
    };

    /// <summary>3 giấy tờ BẮT BUỘC để xe tải kinh doanh vận tải được chạy: Đăng kiểm (kiểm định),
    /// Bảo hiểm TNDS bắt buộc, Phù hiệu xe (GP vận tải). Thiếu hoặc hết hạn loại nào cũng chặn.</summary>
    public static readonly VehicleDocumentType[] Required =
        { VehicleDocumentType.Registration, VehicleDocumentType.Insurance, VehicleDocumentType.TransportPermit };

    /// <summary>Lý do xe KHÔNG được chạy tới hết ngày VN <paramref name="date"/> vì giấy tờ (null = hợp lệ).
    /// Truyền NGÀY CUỐI của chuyến dự kiến -> giấy tờ phải còn hạn PHỦ HẾT thời gian chuyến (hạn
    /// xa nhất của mỗi loại; gia hạn = thêm dòng mới). Trước đây Bảo hiểm / Giấy phép vận tải
    /// chưa nhập vẫn cho chạy — nay cả 3 loại đều bắt buộc.</summary>
    public static string? BlockReason(IEnumerable<LatestDoc> docsOfVehicle, string plate, DateTime date)
    {
        var docs = docsOfVehicle.ToList();
        var missing = Required.Where(t => docs.All(d => d.DocType != t)).ToList();
        if (missing.Count > 0)
            return $"Xe {plate} chưa nhập {string.Join(", ", missing.Select(DocTypeText))} — nhập trong màn Đội xe → Giấy tờ xe trước khi chạy.";
        var expired = docs.Where(d => Required.Contains(d.DocType) && !IsValidOn(d.ExpiryDate, date))
            .OrderBy(d => d.ExpiryDate).FirstOrDefault();
        if (expired is not null)
            return $"{DocTypeText(expired.DocType)} xe {plate} hết hạn ngày {expired.ExpiryDate:dd/MM/yyyy}, trước khi chuyến xong (cần còn hạn tới hết {date:dd/MM/yyyy}) — gia hạn trong màn Đội xe → Giấy tờ xe.";
        return null;
    }
}
