namespace TruckingSystem.Application.AuditLogs;

public record AuditLogDto(int Id, string Action, string EntityName, int? EntityId, string? Detail, DateTime Timestamp);

/// <summary>Bộ lọc tùy chọn — bỏ trống field nào thì KHÔNG lọc theo field đó.</summary>
public record AuditLogFilterRequest(string? Action, DateTime? FromDate, DateTime? ToDate);
