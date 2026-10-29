namespace TruckingSystem.Application.AuditLogs;

/// <summary>1 dòng nhật ký. UserName = người thao tác (null = hệ thống tự chạy); UserRole = vai trò lúc thao tác.</summary>
public record AuditLogDto(int Id, string Action, string EntityName, int? EntityId, string? Detail, DateTime Timestamp,
    string? UserName = null, string? UserRole = null);

/// <summary>Bộ lọc tùy chọn — bỏ trống field nào thì KHÔNG lọc theo field đó. Take tối đa 1000 dòng / lần.</summary>
public record AuditLogFilterRequest(string? Action, DateTime? FromDate, DateTime? ToDate,
    string? UserName = null, string? EntityName = null, string? Search = null, int Skip = 0, int Take = 300);

/// <summary>Giá trị cho các ô lọc: hành động, người thao tác, loại đối tượng ĐÃ TỪNG có trong nhật ký.</summary>
public record AuditLogFilterOptionsDto(List<string> Actions, List<string> Users, List<string> Entities);
