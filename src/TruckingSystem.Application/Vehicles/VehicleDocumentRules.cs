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
        VehicleDocumentType.TransportPermit => "Giấy phép vận tải",
        _ => "Giấy tờ",
    };

    /// <summary>Lý do xe KHÔNG được chạy vào ngày VN <paramref name="date"/> vì giấy tờ
    /// (null = hợp lệ): Đăng kiểm BẮT BUỘC có và còn hạn; Bảo hiểm / Giấy phép vận tải
    /// chưa nhập thì cho qua, đã nhập mà bản mới nhất hết hạn thì chặn.</summary>
    public static string? BlockReason(IEnumerable<LatestDoc> docsOfVehicle, string plate, DateTime date)
    {
        var docs = docsOfVehicle.ToList();
        var registration = docs.FirstOrDefault(d => d.DocType == VehicleDocumentType.Registration);
        if (registration is null)
            return $"Xe {plate} chưa nhập Đăng kiểm — nhập trong màn Xe trước khi chạy.";
        if (!IsValidOn(registration.ExpiryDate, date))
            return $"Đăng kiểm xe {plate} đã hết hạn ({registration.ExpiryDate:dd/MM/yyyy}) — gia hạn trong màn Xe trước khi chạy.";

        var expiredOther = docs.FirstOrDefault(d => d.DocType != VehicleDocumentType.Registration && !IsValidOn(d.ExpiryDate, date));
        if (expiredOther is not null)
            return $"{DocTypeText(expiredOther.DocType)} xe {plate} đã hết hạn ({expiredOther.ExpiryDate:dd/MM/yyyy}) — gia hạn trong màn Xe trước khi chạy.";
        return null;
    }
}
