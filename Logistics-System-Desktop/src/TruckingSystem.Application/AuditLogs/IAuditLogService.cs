namespace TruckingSystem.Application.AuditLogs;

public interface IAuditLogService
{
    Task<List<AuditLogDto>> GetAsync(AuditLogFilterRequest filter);

    /// <summary>Danh sách các giá trị Action ĐÃ TỪNG xuất hiện trong log —
    /// đổ vào dropdown lọc, tránh gõ tay sai chính tả.</summary>
    Task<List<string>> GetDistinctActionsAsync();

    /// <summary>Hành động / người thao tác / loại đối tượng đã có trong nhật ký — cho các ô lọc.</summary>
    Task<AuditLogFilterOptionsDto> GetFilterOptionsAsync();

    /// <summary>LỊCH SỬ THAY ĐỔI của 1 đối tượng (chuột phải đơn / chuyến / xe -> "Lịch sử thay đổi"): nhật ký của
    /// chính nó + các bản ghi con (đơn: chuyến, dòng hàng, đổi hạn giao, phiếu nhập/xuất; chuyến: nhiên liệu, chi phí,
    /// sự cố, phiếu nhập/xuất; xe: giấy tờ). Null nếu loại đối tượng không hỗ trợ.</summary>
    Task<List<AuditLogDto>?> GetHistoryAsync(string entityName, int entityId);
}
